-- Migration 022: Create vesting_schedules table
-- Date: 2026-02-25
-- Description: Periodic vesting events generated from a grant's vesting plan

-- ============================================
-- TABLE: vesting_schedules
-- Purpose: Row-per-period vesting schedule records (monthly/quarterly/annual events)
-- ============================================
CREATE TABLE IF NOT EXISTS vesting_schedules (
    id CHAR(36) NOT NULL,
    client_id CHAR(36) NOT NULL          COMMENT 'Multi-tenant isolation',
    vesting_grant_id CHAR(36) NOT NULL   COMMENT 'FK: vesting_grants.id',
    company_id CHAR(36) NOT NULL         COMMENT 'FK: companies.id (denormalized)',

    -- Schedule period
    period_number INT NOT NULL           COMMENT 'Sequential period: 1 = first vesting event',
    schedule_date DATE NOT NULL          COMMENT 'Date on which shares vest for this period',
    shares_to_vest DECIMAL(15,0) NOT NULL COMMENT 'Shares vesting on this specific date',
    cumulative_shares DECIMAL(15,0) NOT NULL COMMENT 'Total vested shares after this event',
    percentage_to_vest DECIMAL(8,4) NOT NULL COMMENT '% of total grant vesting this period',

    -- Status
    status VARCHAR(20) NOT NULL DEFAULT 'Pending' COMMENT 'Pending | Vested | Skipped',
    vested_at DATETIME(6) NULL          COMMENT 'Actual timestamp when shares were vested',

    -- Audit fields
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by CHAR(36) NOT NULL,
    updated_by CHAR(36) NOT NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,

    PRIMARY KEY (id),

    -- Unique: one schedule row per period per grant
    UNIQUE KEY uk_vs_grant_period (vesting_grant_id, period_number),

    -- Foreign Keys
    CONSTRAINT fk_vs_client FOREIGN KEY (client_id)
        REFERENCES clients(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vs_grant FOREIGN KEY (vesting_grant_id)
        REFERENCES vesting_grants(id) ON DELETE CASCADE,
    CONSTRAINT fk_vs_company FOREIGN KEY (company_id)
        REFERENCES companies(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vs_created_by FOREIGN KEY (created_by)
        REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vs_updated_by FOREIGN KEY (updated_by)
        REFERENCES users(id) ON DELETE RESTRICT,

    -- Indexes
    INDEX idx_vs_grant (vesting_grant_id),
    INDEX idx_vs_company (company_id),
    INDEX idx_vs_schedule_date (schedule_date),
    INDEX idx_vs_status (status),
    INDEX idx_vs_grant_status (vesting_grant_id, status),
    INDEX idx_vs_deleted (is_deleted),

    -- Check Constraints
    CONSTRAINT chk_vs_period_number CHECK (period_number > 0),
    CONSTRAINT chk_vs_shares CHECK (shares_to_vest > 0),
    CONSTRAINT chk_vs_cumulative CHECK (cumulative_shares > 0),
    CONSTRAINT chk_vs_pct CHECK (percentage_to_vest > 0 AND percentage_to_vest <= 100),
    CONSTRAINT chk_vs_status CHECK (status IN ('Pending', 'Vested', 'Skipped'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Detailed vesting schedule - one row per vesting period event per grant';
