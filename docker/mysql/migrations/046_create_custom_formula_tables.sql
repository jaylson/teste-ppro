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
