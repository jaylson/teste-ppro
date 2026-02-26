-- Migration 021: Create vesting_grants table
-- Date: 2026-02-25
-- Description: Individual vesting grants assigned to shareholders under a vesting plan

-- ============================================
-- TABLE: vesting_grants
-- Purpose: Grants equity to a specific shareholder under a vesting plan
-- ============================================
CREATE TABLE IF NOT EXISTS vesting_grants (
    id CHAR(36) NOT NULL,
    client_id CHAR(36) NOT NULL                  COMMENT 'Multi-tenant isolation',
    vesting_plan_id CHAR(36) NOT NULL            COMMENT 'FK: vesting_plans.id',
    shareholder_id CHAR(36) NOT NULL             COMMENT 'FK: shareholders.id',
    company_id CHAR(36) NOT NULL                 COMMENT 'FK: companies.id (denormalized for query perf)',

    -- Grant parameters
    grant_date DATE NOT NULL                     COMMENT 'Date the grant was awarded',
    total_shares DECIMAL(15,0) NOT NULL          COMMENT 'Total shares granted',
    share_price DECIMAL(15,4) NOT NULL           COMMENT 'Strike price at grant date',
    equity_percentage DECIMAL(8,4) NOT NULL      COMMENT 'Equity % this grant represents',

    -- Vesting schedule dates
    vesting_start_date DATE NOT NULL             COMMENT 'When vesting clock starts',
    vesting_end_date DATE NOT NULL               COMMENT 'When fully vested',
    cliff_date DATE NULL                         COMMENT 'NULL if no cliff; otherwise date cliff is met',

    -- Status
    status VARCHAR(20) NOT NULL DEFAULT 'Pending' COMMENT 'Pending | Approved | Active | Exercised | Expired | Cancelled',

    -- Progress tracking (updated by service on each calculation)
    vested_shares DECIMAL(15,0) NOT NULL DEFAULT 0     COMMENT 'Shares vested as of last recalculation',
    exercised_shares DECIMAL(15,0) NOT NULL DEFAULT 0  COMMENT 'Shares already exercised by shareholder',

    -- Approval workflow
    approved_at DATETIME(6) NULL,
    approved_by CHAR(36) NULL                    COMMENT 'FK: users.id',

    -- Notes
    notes TEXT NULL,

    -- Audit fields
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by CHAR(36) NOT NULL,
    updated_by CHAR(36) NOT NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    deleted_at DATETIME(6) NULL,
    deleted_by CHAR(36) NULL,

    PRIMARY KEY (id),

    -- Foreign Keys
    CONSTRAINT fk_vg_client FOREIGN KEY (client_id)
        REFERENCES clients(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vg_plan FOREIGN KEY (vesting_plan_id)
        REFERENCES vesting_plans(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vg_shareholder FOREIGN KEY (shareholder_id)
        REFERENCES shareholders(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vg_company FOREIGN KEY (company_id)
        REFERENCES companies(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vg_created_by FOREIGN KEY (created_by)
        REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vg_updated_by FOREIGN KEY (updated_by)
        REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vg_approved_by FOREIGN KEY (approved_by)
        REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vg_deleted_by FOREIGN KEY (deleted_by)
        REFERENCES users(id) ON DELETE RESTRICT,

    -- Indexes
    INDEX idx_vg_client (client_id),
    INDEX idx_vg_plan (vesting_plan_id),
    INDEX idx_vg_shareholder (shareholder_id),
    INDEX idx_vg_company (company_id),
    INDEX idx_vg_status (status),
    INDEX idx_vg_company_status (company_id, status),
    INDEX idx_vg_shareholder_status (shareholder_id, status),
    INDEX idx_vg_grant_date (grant_date),
    INDEX idx_vg_deleted (is_deleted),

    -- Check Constraints
    CONSTRAINT chk_vg_total_shares CHECK (total_shares > 0),
    CONSTRAINT chk_vg_share_price CHECK (share_price >= 0),
    CONSTRAINT chk_vg_equity_pct CHECK (equity_percentage > 0 AND equity_percentage <= 100),
    CONSTRAINT chk_vg_vested_shares CHECK (vested_shares >= 0 AND vested_shares <= total_shares),
    CONSTRAINT chk_vg_exercised_shares CHECK (exercised_shares >= 0 AND exercised_shares <= vested_shares),
    CONSTRAINT chk_vg_dates CHECK (vesting_start_date <= vesting_end_date),
    CONSTRAINT chk_vg_cliff_date CHECK (cliff_date IS NULL OR (cliff_date >= vesting_start_date AND cliff_date <= vesting_end_date)),
    CONSTRAINT chk_vg_status CHECK (status IN ('Pending', 'Approved', 'Active', 'Exercised', 'Expired', 'Cancelled'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Individual equity grants to shareholders - each grant tracks vesting progress over time';
