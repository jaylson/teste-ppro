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
