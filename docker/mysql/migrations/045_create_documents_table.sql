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
