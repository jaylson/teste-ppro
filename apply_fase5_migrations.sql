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
-- Migration 044: Create financial tables
-- Date: 2026-03-02
-- Description: Monthly financial data tables — financial_periods (one row per company/month)
--              and financial_metrics (detail metrics for each period).
-- Business Rules:
--   FI-01: One period per company per (year, month) — UNIQUE INDEX enforced here
--   FI-02: Cannot approve a period if a previous period has not been approved (enforced at service layer)
--   FI-03: Approved/locked period cannot be edited (enforced at service layer)
--   FI-04: Required documents must exist before period can be submitted (enforced at service layer)

-- ============================================
-- TABLE: financial_periods
-- Purpose: Container for one month of financial data per company
-- ============================================
CREATE TABLE IF NOT EXISTS financial_periods (
    id                  CHAR(36)        NOT NULL DEFAULT (UUID()),
    client_id           CHAR(36)        NOT NULL                    COMMENT 'Multi-tenant isolation',
    company_id          CHAR(36)        NOT NULL                    COMMENT 'FK: companies.id',

    -- Period identification
    year                SMALLINT        NOT NULL                    COMMENT 'YYYY, e.g. 2026',
    month               TINYINT         NOT NULL                    COMMENT '1–12',

    -- Status workflow: draft → submitted → approved → locked
    status              VARCHAR(20)     NOT NULL DEFAULT 'draft'    COMMENT 'draft|submitted|approved|locked',

    -- Notes
    notes               TEXT            NULL                        COMMENT 'Analyst notes for this period',

    -- Workflow timestamps
    submitted_at        DATETIME(6)     NULL,
    submitted_by        CHAR(36)        NULL                        COMMENT 'FK: users.id',
    approved_at         DATETIME(6)     NULL,
    approved_by         CHAR(36)        NULL                        COMMENT 'FK: users.id',
    locked_at           DATETIME(6)     NULL,
    locked_by           CHAR(36)        NULL                        COMMENT 'FK: users.id',

    -- Audit
    created_at          DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at          DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by          CHAR(36)        NOT NULL,
    updated_by          CHAR(36)        NOT NULL,
    is_deleted          TINYINT(1)      NOT NULL DEFAULT 0,
    deleted_at          DATETIME(6)     NULL,
    deleted_by          CHAR(36)        NULL,

    PRIMARY KEY (id),

    CONSTRAINT fk_fp_client         FOREIGN KEY (client_id)     REFERENCES clients(id)      ON DELETE RESTRICT,
    CONSTRAINT fk_fp_company        FOREIGN KEY (company_id)    REFERENCES companies(id)    ON DELETE RESTRICT,
    CONSTRAINT fk_fp_created_by     FOREIGN KEY (created_by)    REFERENCES users(id)        ON DELETE RESTRICT,
    CONSTRAINT fk_fp_updated_by     FOREIGN KEY (updated_by)    REFERENCES users(id)        ON DELETE RESTRICT,
    CONSTRAINT fk_fp_submitted_by   FOREIGN KEY (submitted_by)  REFERENCES users(id)        ON DELETE RESTRICT,
    CONSTRAINT fk_fp_approved_by    FOREIGN KEY (approved_by)   REFERENCES users(id)        ON DELETE RESTRICT,
    CONSTRAINT fk_fp_locked_by      FOREIGN KEY (locked_by)     REFERENCES users(id)        ON DELETE RESTRICT,

    -- FI-01: One period per company per year/month
    UNIQUE KEY uq_fp_company_period (company_id, year, month),

    INDEX idx_fp_client             (client_id),
    INDEX idx_fp_company            (company_id),
    INDEX idx_fp_status             (status),
    INDEX idx_fp_deleted            (is_deleted),
    INDEX idx_fp_company_year       (company_id, year DESC, month DESC),

    CONSTRAINT chk_fp_month         CHECK (month BETWEEN 1 AND 12),
    CONSTRAINT chk_fp_year          CHECK (year BETWEEN 2000 AND 2100),
    CONSTRAINT chk_fp_status        CHECK (status IN ('draft','submitted','approved','locked'))

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Monthly financial period container — one row per company/year/month';


-- ============================================
-- TABLE: financial_metrics
-- Purpose: Actual financial KPIs for a period. ARR and runway are calculated
--          server-side and stored for caching/dashboard queries.
-- ============================================
CREATE TABLE IF NOT EXISTS financial_metrics (
    id                      CHAR(36)        NOT NULL DEFAULT (UUID()),
    client_id               CHAR(36)        NOT NULL                    COMMENT 'Multi-tenant isolation',
    period_id               CHAR(36)        NOT NULL                    COMMENT 'FK: financial_periods.id — one metrics row per period',

    -- Revenue
    gross_revenue           DECIMAL(18,2)   NULL                        COMMENT 'Receita bruta do mês',
    net_revenue             DECIMAL(18,2)   NULL                        COMMENT 'Receita líquida do mês',
    mrr                     DECIMAL(18,2)   NULL                        COMMENT 'Monthly Recurring Revenue',
    arr                     DECIMAL(18,2)   NULL                        COMMENT 'ARR = MRR × 12 (calculated — stored for query performance)',

    -- Cash & Burn
    cash_balance            DECIMAL(18,2)   NULL                        COMMENT 'Saldo em caixa no fim do período',
    burn_rate               DECIMAL(18,2)   NULL                        COMMENT 'Cash consumed this month (positive = spending)',
    runway_months           DECIMAL(8,2)    NULL                        COMMENT 'cash_balance / burn_rate (calculated — stored for dashboards)',

    -- Unit Economics
    customer_count          INT             NULL                        COMMENT 'Total active customers at period end',
    churn_rate              DECIMAL(8,4)    NULL                        COMMENT 'Monthly churn % (0–100)',
    cac                     DECIMAL(18,2)   NULL                        COMMENT 'Customer Acquisition Cost',
    ltv                     DECIMAL(18,2)   NULL                        COMMENT 'Lifetime Value',
    nps                     SMALLINT        NULL                        COMMENT 'Net Promoter Score (-100 to 100)',

    -- Profitability
    ebitda                  DECIMAL(18,2)   NULL                        COMMENT 'EBITDA do período',
    ebitda_margin           DECIMAL(8,4)    NULL                        COMMENT 'EBITDA / Net Revenue % (calculated — stored)',
    net_income              DECIMAL(18,2)   NULL                        COMMENT 'Lucro líquido',

    -- Audit
    created_at              DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at              DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by              CHAR(36)        NOT NULL,
    updated_by              CHAR(36)        NOT NULL,

    PRIMARY KEY (id),

    CONSTRAINT fk_fm_client         FOREIGN KEY (client_id)     REFERENCES clients(id)          ON DELETE RESTRICT,
    CONSTRAINT fk_fm_period         FOREIGN KEY (period_id)     REFERENCES financial_periods(id) ON DELETE CASCADE,
    CONSTRAINT fk_fm_created_by     FOREIGN KEY (created_by)    REFERENCES users(id)            ON DELETE RESTRICT,
    CONSTRAINT fk_fm_updated_by     FOREIGN KEY (updated_by)    REFERENCES users(id)            ON DELETE RESTRICT,

    -- One metrics row per period
    UNIQUE KEY uq_fm_period         (period_id),

    INDEX idx_fm_client             (client_id),
    INDEX idx_fm_period             (period_id),

    CONSTRAINT chk_fm_churn         CHECK (churn_rate IS NULL OR (churn_rate >= 0 AND churn_rate <= 100)),
    CONSTRAINT chk_fm_nps           CHECK (nps IS NULL OR (nps BETWEEN -100 AND 100)),
    CONSTRAINT chk_fm_runway        CHECK (runway_months IS NULL OR runway_months >= 0)

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Financial KPI metrics for each monthly period — one row per period';


-- ============================================
-- DOWN (rollback — execute in reverse order)
-- ============================================
-- DROP TABLE IF EXISTS financial_metrics;
-- DROP TABLE IF EXISTS financial_periods;
-- Migration 045: Create central documents repository table
-- Date: 2026-03-02
-- Description: Polymorphic document repository — one table for all company documents,
--              linked optionally to any entity (valuation, financial_period, contract, etc.)
--              via entity_type + entity_id. Also usable as standalone company documents.

-- ============================================
-- TABLE: documents
-- Purpose: Central, polymorphic document repository per company.
--          entity_type + entity_id optionally link a document to any entity.
--          visibility controls who can see each document.
-- ============================================
CREATE TABLE IF NOT EXISTS documents (
    id                  CHAR(36)        NOT NULL DEFAULT (UUID()),
    client_id           CHAR(36)        NOT NULL                    COMMENT 'Multi-tenant isolation',
    company_id          CHAR(36)        NOT NULL                    COMMENT 'FK: companies.id',

    -- Document metadata
    name                VARCHAR(500)    NOT NULL                    COMMENT 'Human-readable document name',
    document_type       VARCHAR(50)     NOT NULL                    COMMENT 'balance_sheet|income_statement|cash_flow|audit_report|contract|certificate|bylaws|minutes|presentation|valuation_support|other',
    description         TEXT            NULL,

    -- File info
    file_name           VARCHAR(500)    NOT NULL                    COMMENT 'Original file name',
    file_size_bytes     BIGINT          NOT NULL,
    mime_type           VARCHAR(200)    NOT NULL,
    storage_path        VARCHAR(1000)   NOT NULL                    COMMENT 'Path in blob/S3 storage',
    download_url        VARCHAR(2000)   NULL                        COMMENT 'Signed or static download URL',

    -- Polymorphic association (optional)
    entity_type         VARCHAR(50)     NULL                        COMMENT 'valuation|financial_period|contract|null (standalone)',
    entity_id           CHAR(36)        NULL                        COMMENT 'UUID of the associated entity',

    -- Visibility
    visibility          VARCHAR(20)     NOT NULL DEFAULT 'admin'    COMMENT 'admin|board|shareholders|investors|public',

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

    CONSTRAINT fk_doc_client        FOREIGN KEY (client_id)     REFERENCES clients(id)      ON DELETE RESTRICT,
    CONSTRAINT fk_doc_company       FOREIGN KEY (company_id)    REFERENCES companies(id)    ON DELETE RESTRICT,
    CONSTRAINT fk_doc_created_by    FOREIGN KEY (created_by)    REFERENCES users(id)        ON DELETE RESTRICT,
    CONSTRAINT fk_doc_updated_by    FOREIGN KEY (updated_by)    REFERENCES users(id)        ON DELETE RESTRICT,
    CONSTRAINT fk_doc_verified_by   FOREIGN KEY (verified_by)   REFERENCES users(id)        ON DELETE RESTRICT,

    INDEX idx_doc_client            (client_id),
    INDEX idx_doc_company           (company_id),
    INDEX idx_doc_entity            (entity_type, entity_id),
    INDEX idx_doc_type              (document_type),
    INDEX idx_doc_visibility        (visibility),
    INDEX idx_doc_deleted           (is_deleted),
    INDEX idx_doc_company_type      (company_id, document_type, is_deleted),

    CONSTRAINT chk_doc_type CHECK (
        document_type IN (
            'balance_sheet','income_statement','cash_flow','audit_report',
            'contract','certificate','bylaws','minutes','presentation',
            'valuation_support','other'
        )
    ),
    CONSTRAINT chk_doc_visibility CHECK (
        visibility IN ('admin','board','shareholders','investors','public')
    ),
    CONSTRAINT chk_doc_size         CHECK (file_size_bytes > 0),
    CONSTRAINT chk_doc_entity_both  CHECK (
        (entity_type IS NULL AND entity_id IS NULL) OR
        (entity_type IS NOT NULL AND entity_id IS NOT NULL)
    )

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Central polymorphic document repository — documents linked to any entity or standalone';


-- ============================================
-- DOWN (rollback)
-- ============================================
-- DROP TABLE IF EXISTS documents;
-- Migration 046: Create custom formula valuation tables
-- Date: 2026-03-02
-- Description: Tables for the custom formula engine (NCalc2).
--   valuation_custom_formulas  — formula definition container per company
--   valuation_formula_versions — IMMUTABLE versioned snapshots of each formula
--   valuation_formula_executions — IMMUTABLE audit log of every formula execution

-- ============================================
-- TABLE: valuation_custom_formulas
-- Purpose: Container for a company's custom valuation formula definition.
--          The actual expression + variables live in valuation_formula_versions (immutable).
--          current_version_id always points to the latest version.
-- ============================================
CREATE TABLE IF NOT EXISTS valuation_custom_formulas (
    id                      CHAR(36)        NOT NULL DEFAULT (UUID()),
    client_id               CHAR(36)        NOT NULL                    COMMENT 'Multi-tenant isolation',
    company_id              CHAR(36)        NOT NULL                    COMMENT 'FK: companies.id',

    name                    VARCHAR(200)    NOT NULL                    COMMENT 'Formula name, e.g. "Fórmula Safra Agro"',
    description             TEXT            NULL                        COMMENT 'Business logic explanation',
    sector_tag              VARCHAR(100)    NULL                        COMMENT 'Free tag: agro, saude, impacto, marketplace...',

    -- Points to the latest version (null until first version is saved)
    current_version_id      CHAR(36)        NULL                        COMMENT 'FK: valuation_formula_versions.id',

    is_active               TINYINT(1)      NOT NULL DEFAULT 1          COMMENT 'Active/inactive (soft toggle)',

    -- Audit
    created_at              DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at              DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by              CHAR(36)        NOT NULL                    COMMENT 'FK: users.id',
    updated_by              CHAR(36)        NOT NULL                    COMMENT 'FK: users.id',
    is_deleted              TINYINT(1)      NOT NULL DEFAULT 0,
    deleted_at              DATETIME(6)     NULL,
    deleted_by              CHAR(36)        NULL,

    PRIMARY KEY (id),

    CONSTRAINT fk_vcf_client        FOREIGN KEY (client_id)     REFERENCES clients(id)      ON DELETE RESTRICT,
    CONSTRAINT fk_vcf_company       FOREIGN KEY (company_id)    REFERENCES companies(id)    ON DELETE RESTRICT,
    CONSTRAINT fk_vcf_created_by    FOREIGN KEY (created_by)    REFERENCES users(id)        ON DELETE RESTRICT,
    CONSTRAINT fk_vcf_updated_by    FOREIGN KEY (updated_by)    REFERENCES users(id)        ON DELETE RESTRICT,
    -- fk_vcf_current_version added after version table is created below

    INDEX idx_vcf_client            (client_id),
    INDEX idx_vcf_company           (company_id),
    INDEX idx_vcf_active            (company_id, is_active),
    INDEX idx_vcf_deleted           (is_deleted)

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Custom valuation formula definitions per company — versioned, each edit creates new version';


-- ============================================
-- TABLE: valuation_formula_versions
-- Purpose: IMMUTABLE snapshot of a formula version.
--          Once created, rows in this table are never updated.
--          variables column is a JSON array of VariableDefinition objects.
-- ============================================
CREATE TABLE IF NOT EXISTS valuation_formula_versions (
    id                      CHAR(36)        NOT NULL DEFAULT (UUID()),
    formula_id              CHAR(36)        NOT NULL                    COMMENT 'FK: valuation_custom_formulas.id',
    client_id               CHAR(36)        NOT NULL                    COMMENT 'Multi-tenant isolation (denormalized for query simplicity)',

    version_number          INT UNSIGNED    NOT NULL                    COMMENT 'Sequential version per formula: 1, 2, 3…',

    -- The formula expression  e.g. "[hectares] * [preco_saca] * [sacas_por_hectare]"
    expression              TEXT            NOT NULL                    COMMENT 'NCalc2 expression with [variable_name] references',

    -- Variable definitions as JSON array
    -- Schema: [{name, label, type (currency|percentage|number|integer|multiplier|boolean),
    --           unit, description, is_required, default_value, min_value, max_value, display_order}]
    variables               JSON            NOT NULL                    COMMENT 'Array of VariableDefinition objects',

    result_unit             VARCHAR(50)     NOT NULL DEFAULT 'BRL'      COMMENT 'Unit of result: BRL, USD, pontos...',
    result_label            VARCHAR(200)    NULL                        COMMENT 'Display label for the result',

    -- Test data (populated when formula is tested before saving)
    test_inputs             JSON            NULL                        COMMENT 'Example inputs used to test the formula',
    test_result             DECIMAL(18,2)   NULL                        COMMENT 'Result calculated with test_inputs',

    -- Validation state
    validation_status       VARCHAR(20)     NOT NULL DEFAULT 'draft'    COMMENT 'draft|validated|invalid',
    validation_errors       JSON            NULL                        COMMENT 'Array of validation error messages',

    -- Immutable — no updated_at
    created_at              DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    created_by              CHAR(36)        NOT NULL                    COMMENT 'FK: users.id',

    PRIMARY KEY (id),

    CONSTRAINT fk_vfv_formula       FOREIGN KEY (formula_id)    REFERENCES valuation_custom_formulas(id) ON DELETE CASCADE,
    CONSTRAINT fk_vfv_client        FOREIGN KEY (client_id)     REFERENCES clients(id)                   ON DELETE RESTRICT,
    CONSTRAINT fk_vfv_created_by    FOREIGN KEY (created_by)    REFERENCES users(id)                     ON DELETE RESTRICT,

    UNIQUE KEY uq_vfv_version       (formula_id, version_number),

    INDEX idx_vfv_formula           (formula_id),
    INDEX idx_vfv_client            (client_id),

    CONSTRAINT chk_vfv_status       CHECK (validation_status IN ('draft','validated','invalid')),
    CONSTRAINT chk_vfv_version      CHECK (version_number >= 1)

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Immutable versioned snapshots of custom formula expressions and variable definitions';


-- Now add the deferred self-reference FK on valuation_custom_formulas
ALTER TABLE valuation_custom_formulas
    ADD CONSTRAINT fk_vcf_current_version
        FOREIGN KEY (current_version_id) REFERENCES valuation_formula_versions(id) ON DELETE SET NULL;


-- ============================================
-- TABLE: valuation_formula_executions
-- Purpose: IMMUTABLE audit log of each formula execution within a valuation method.
--          Records exact inputs, expression snapshot, and result for full auditability.
-- ============================================
CREATE TABLE IF NOT EXISTS valuation_formula_executions (
    id                      CHAR(36)        NOT NULL DEFAULT (UUID()),
    client_id               CHAR(36)        NOT NULL                    COMMENT 'Multi-tenant isolation',
    valuation_method_id     CHAR(36)        NOT NULL                    COMMENT 'FK: valuation_methods.id',
    formula_version_id      CHAR(36)        NOT NULL                    COMMENT 'FK: valuation_formula_versions.id — exact version used',

    -- Execution record
    inputs_used             JSON            NOT NULL                    COMMENT 'Snapshot of {variable_name: value} pairs actually calculated',
    calculated_value        DECIMAL(18,2)   NOT NULL                    COMMENT 'Execution result',
    expression_snapshot     TEXT            NOT NULL                    COMMENT 'Copy of expression at execution time (extra safety)',

    -- Who ran it
    executed_by             CHAR(36)        NOT NULL                    COMMENT 'FK: users.id',
    executed_at             DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),

    PRIMARY KEY (id),

    CONSTRAINT fk_vfe_client        FOREIGN KEY (client_id)             REFERENCES clients(id)                  ON DELETE RESTRICT,
    CONSTRAINT fk_vfe_method        FOREIGN KEY (valuation_method_id)   REFERENCES valuation_methods(id)        ON DELETE CASCADE,
    CONSTRAINT fk_vfe_version       FOREIGN KEY (formula_version_id)    REFERENCES valuation_formula_versions(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vfe_executed_by   FOREIGN KEY (executed_by)           REFERENCES users(id)                    ON DELETE RESTRICT,

    INDEX idx_vfe_method            (valuation_method_id),
    INDEX idx_vfe_version           (formula_version_id),
    INDEX idx_vfe_client            (client_id),
    INDEX idx_vfe_executed_at       (executed_at)

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Immutable audit log of every custom formula execution — inputs + result snapshot';


-- ============================================
-- DOWN (rollback — execute in reverse order)
-- ============================================
-- DROP TABLE IF EXISTS valuation_formula_executions;
-- ALTER TABLE valuation_custom_formulas DROP FOREIGN KEY fk_vcf_current_version;
-- DROP TABLE IF EXISTS valuation_formula_versions;
-- DROP TABLE IF EXISTS valuation_custom_formulas;
-- Migration 047: Alter valuation_methods — add formula_version_id FK + extend method_type ENUM
-- Date: 2026-03-02
-- Description: Now that valuation_formula_versions table exists (migration 046), we can:
--   1. Add FK constraint fk_valmet_formula_version on valuation_methods.formula_version_id
--   The column formula_version_id was already created in migration 043 as a plain nullable column.
--   The method_type CHECK constraint already includes 'custom' from migration 043.

-- ============================================
-- Add the FK that was deferred until formula tables existed
-- ============================================
ALTER TABLE valuation_methods
    ADD CONSTRAINT fk_valmet_formula_version
        FOREIGN KEY (formula_version_id)
        REFERENCES valuation_formula_versions(id)
        ON DELETE RESTRICT;

-- ============================================
-- Add index for formula_version lookups
-- ============================================
ALTER TABLE valuation_methods
    ADD INDEX idx_vm_formula_version (formula_version_id);


-- ============================================
-- DOWN (rollback)
-- ============================================
-- ALTER TABLE valuation_methods DROP INDEX idx_vm_formula_version;
-- ALTER TABLE valuation_methods DROP FOREIGN KEY fk_valmet_formula_version;
