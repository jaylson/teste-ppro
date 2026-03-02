-- Migration 043: Create valuation core tables
-- Date: 2026-03-02
-- Description: Core tables for the Valuation module — valuations (lifecycle + approval workflow),
--              valuation_methods (calculation methodologies per valuation),
--              valuation_documents (supporting documents attached to a valuation).

-- ============================================
-- TABLE: valuations
-- Purpose: One valuation event per date per company (e.g. Seed, Series A, internal 409A)
-- Business Rules:
--   VA-01: Exactly one valuation_method must have is_selected = true
--   VA-02: Valuation requires approval workflow: draft → pending_approval → approved | rejected
--   VA-03: valuation_date must be after the last approved valuation date
--   VA-04: price_per_share = valuation_amount / total_shares (calculated automatically on approval)
-- ============================================
CREATE TABLE IF NOT EXISTS valuations (
    id                  CHAR(36)        NOT NULL DEFAULT (UUID()),
    client_id           CHAR(36)        NOT NULL                    COMMENT 'Multi-tenant isolation — FK: clients.id',
    company_id          CHAR(36)        NOT NULL                    COMMENT 'FK: companies.id',

    -- Event info
    valuation_date      DATE            NOT NULL                    COMMENT 'Date of this valuation event',
    event_type          VARCHAR(30)     NOT NULL                    COMMENT 'founding|seed|series_a|series_b|series_c|internal|external|409a|other',
    event_name          VARCHAR(200)    NULL                        COMMENT 'Human-readable name, e.g. "Série A Round"',

    -- Calculated result (set by engine, confirmed on approval)
    valuation_amount    DECIMAL(18,2)   NULL                        COMMENT 'Official valuation amount from selected methodology',
    total_shares        DECIMAL(18,4)   NOT NULL                    COMMENT 'Total shares considered (default = cap table total, editable for scenarios)',
    price_per_share     DECIMAL(18,6)   NULL                        COMMENT 'valuation_amount / total_shares — computed on approval (VA-04)',

    -- Workflow
    status              VARCHAR(30)     NOT NULL DEFAULT 'draft'    COMMENT 'draft|pending_approval|approved|rejected',
    notes               TEXT            NULL                        COMMENT 'Notes or justification',

    -- Approval fields
    submitted_at        DATETIME(6)     NULL,
    submitted_by        CHAR(36)        NULL                        COMMENT 'FK: users.id',
    approved_at         DATETIME(6)     NULL,
    approved_by         CHAR(36)        NULL                        COMMENT 'FK: users.id',
    rejected_at         DATETIME(6)     NULL,
    rejected_by         CHAR(36)        NULL                        COMMENT 'FK: users.id',
    rejection_reason    TEXT            NULL,

    -- Audit
    created_at          DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at          DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by          CHAR(36)        NOT NULL                    COMMENT 'FK: users.id',
    updated_by          CHAR(36)        NOT NULL                    COMMENT 'FK: users.id',
    is_deleted          TINYINT(1)      NOT NULL DEFAULT 0,
    deleted_at          DATETIME(6)     NULL,
    deleted_by          CHAR(36)        NULL                        COMMENT 'FK: users.id',

    PRIMARY KEY (id),

    CONSTRAINT fk_val_client        FOREIGN KEY (client_id)     REFERENCES clients(id)  ON DELETE RESTRICT,
    CONSTRAINT fk_val_company       FOREIGN KEY (company_id)    REFERENCES companies(id) ON DELETE RESTRICT,
    CONSTRAINT fk_val_created_by    FOREIGN KEY (created_by)    REFERENCES users(id)    ON DELETE RESTRICT,
    CONSTRAINT fk_val_updated_by    FOREIGN KEY (updated_by)    REFERENCES users(id)    ON DELETE RESTRICT,
    CONSTRAINT fk_val_submitted_by  FOREIGN KEY (submitted_by)  REFERENCES users(id)    ON DELETE RESTRICT,
    CONSTRAINT fk_val_approved_by   FOREIGN KEY (approved_by)   REFERENCES users(id)    ON DELETE RESTRICT,
    CONSTRAINT fk_val_rejected_by   FOREIGN KEY (rejected_by)   REFERENCES users(id)    ON DELETE RESTRICT,

    INDEX idx_val_client            (client_id),
    INDEX idx_val_company           (company_id),
    INDEX idx_val_company_date      (company_id, valuation_date DESC),
    INDEX idx_val_status            (status),
    INDEX idx_val_deleted           (is_deleted),
    INDEX idx_val_company_approved  (company_id, status, valuation_date),

    CONSTRAINT chk_val_event_type CHECK (
        event_type IN ('founding','seed','series_a','series_b','series_c','internal','external','409a','other')
    ),
    CONSTRAINT chk_val_status CHECK (
        status IN ('draft','pending_approval','approved','rejected')
    ),
    CONSTRAINT chk_val_total_shares CHECK (total_shares > 0),
    CONSTRAINT chk_val_amount CHECK (valuation_amount IS NULL OR valuation_amount > 0),
    CONSTRAINT chk_val_price_share CHECK (price_per_share IS NULL OR price_per_share > 0)

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Valuation events per company — each row is one valuation with approval lifecycle';


-- ============================================
-- TABLE: valuation_methods
-- Purpose: One or more calculation methodologies per valuation.
--          Exactly one must have is_selected = true (enforced at service layer — VA-01).
-- ============================================
CREATE TABLE IF NOT EXISTS valuation_methods (
    id                  CHAR(36)        NOT NULL DEFAULT (UUID()),
    client_id           CHAR(36)        NOT NULL                    COMMENT 'Multi-tenant isolation',
    valuation_id        CHAR(36)        NOT NULL                    COMMENT 'FK: valuations.id',

    method_type         VARCHAR(30)     NOT NULL                    COMMENT 'arr_multiple|dcf|comparables|ebitda_multiple|mrr_multiple|asset_based|berkus|custom',
    is_selected         TINYINT(1)      NOT NULL DEFAULT 0          COMMENT 'True for the primary/official methodology (VA-01: exactly one per valuation)',

    -- Calculated result
    calculated_value    DECIMAL(18,2)   NULL                        COMMENT 'Result produced by this methodology',

    -- Methodology-specific inputs (JSON — schema varies per method_type)
    inputs              JSON            NULL                        COMMENT 'Input parameters used in calculation (e.g. {arr: 1500000, multiple: 8})',
    data_source         VARCHAR(300)    NULL                        COMMENT 'Source of benchmark data (e.g. "PitchBook Q4 2025")',
    notes               TEXT            NULL,

    -- Custom formula reference (null for standard methodologies)
    formula_version_id  CHAR(36)        NULL                        COMMENT 'FK: valuation_formula_versions.id — only set when method_type=custom',

    -- Audit
    created_at          DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at          DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by          CHAR(36)        NOT NULL,
    updated_by          CHAR(36)        NOT NULL,

    PRIMARY KEY (id),

    CONSTRAINT fk_valmet_client         FOREIGN KEY (client_id)     REFERENCES clients(id)      ON DELETE RESTRICT,
    CONSTRAINT fk_valmet_valuation      FOREIGN KEY (valuation_id)  REFERENCES valuations(id)   ON DELETE CASCADE,
    CONSTRAINT fk_valmet_created_by     FOREIGN KEY (created_by)    REFERENCES users(id)        ON DELETE RESTRICT,
    CONSTRAINT fk_valmet_updated_by     FOREIGN KEY (updated_by)    REFERENCES users(id)        ON DELETE RESTRICT,
    -- fk_valmet_formula_version will be added in migration 047 after formula tables are created

    INDEX idx_vm_valuation          (valuation_id),
    INDEX idx_vm_client             (client_id),
    INDEX idx_vm_selected           (valuation_id, is_selected),

    CONSTRAINT chk_vm_method_type CHECK (
        method_type IN ('arr_multiple','dcf','comparables','ebitda_multiple','mrr_multiple','asset_based','berkus','custom')
    )

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Valuation calculation methodologies per valuation event — one row per method used';


-- ============================================
-- TABLE: valuation_documents
-- Purpose: Supporting documents attached to a specific valuation
-- ============================================
CREATE TABLE IF NOT EXISTS valuation_documents (
    id                  CHAR(36)        NOT NULL DEFAULT (UUID()),
    client_id           CHAR(36)        NOT NULL                    COMMENT 'Multi-tenant isolation',
    valuation_id        CHAR(36)        NOT NULL                    COMMENT 'FK: valuations.id',

    -- File metadata
    file_name           VARCHAR(500)    NOT NULL,
    file_size_bytes     BIGINT          NOT NULL,
    mime_type           VARCHAR(200)    NOT NULL,
    storage_path        VARCHAR(1000)   NOT NULL                    COMMENT 'Relative path in blob/S3 storage',
    download_url        VARCHAR(2000)   NULL                        COMMENT 'Signed URL or static URL for download',

    -- Verification
    is_verified         TINYINT(1)      NOT NULL DEFAULT 0,
    verified_at         DATETIME(6)     NULL,
    verified_by         CHAR(36)        NULL                        COMMENT 'FK: users.id',

    -- Audit
    created_at          DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at          DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by          CHAR(36)        NOT NULL,
    updated_by          CHAR(36)        NOT NULL,
    is_deleted          TINYINT(1)      NOT NULL DEFAULT 0,
    deleted_at          DATETIME(6)     NULL,
    deleted_by          CHAR(36)        NULL,

    PRIMARY KEY (id),

    CONSTRAINT fk_vdoc_client       FOREIGN KEY (client_id)     REFERENCES clients(id)      ON DELETE RESTRICT,
    CONSTRAINT fk_vdoc_valuation    FOREIGN KEY (valuation_id)  REFERENCES valuations(id)   ON DELETE CASCADE,
    CONSTRAINT fk_vdoc_created_by   FOREIGN KEY (created_by)    REFERENCES users(id)        ON DELETE RESTRICT,
    CONSTRAINT fk_vdoc_updated_by   FOREIGN KEY (updated_by)    REFERENCES users(id)        ON DELETE RESTRICT,
    CONSTRAINT fk_vdoc_verified_by  FOREIGN KEY (verified_by)   REFERENCES users(id)        ON DELETE RESTRICT,

    INDEX idx_vdoc_valuation        (valuation_id),
    INDEX idx_vdoc_client           (client_id),
    INDEX idx_vdoc_deleted          (is_deleted),

    CONSTRAINT chk_vdoc_size        CHECK (file_size_bytes > 0)

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Supporting documents attached to a valuation event';


-- ============================================
-- DOWN (rollback — execute in reverse order)
-- ============================================
-- DROP TABLE IF EXISTS valuation_documents;
-- DROP TABLE IF EXISTS valuation_methods;
-- DROP TABLE IF EXISTS valuations;
