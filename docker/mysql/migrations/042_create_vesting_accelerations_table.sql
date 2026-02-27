-- Migration 042: Create vesting_accelerations table
-- Date: 2026-02-27
-- Description: Immutable ledger of vesting accelerations applied when grant milestones are achieved.
--              Each row records the before/after state for audit and cap-table recalculation.

-- ============================================
-- TABLE: vesting_accelerations
-- Purpose: Append-only record of each acceleration applied to a vesting grant
-- ============================================
CREATE TABLE IF NOT EXISTS vesting_accelerations (
    id CHAR(36) NOT NULL,
    client_id CHAR(36) NOT NULL                  COMMENT 'Multi-tenant isolation',
    vesting_grant_id CHAR(36) NOT NULL           COMMENT 'FK: vesting_grants.id',
    grant_milestone_id CHAR(36) NOT NULL         COMMENT 'FK: grant_milestones.id — milestone that triggered this',

    -- Acceleration details
    acceleration_type VARCHAR(20) NOT NULL        COMMENT 'Percentage | Months | Shares',
    acceleration_amount DECIMAL(8,4) NOT NULL     COMMENT 'Configured value (% / months / shares)',

    -- Snapshot of before / after (for audit)
    original_vesting_end_date DATE NOT NULL       COMMENT 'VestingEndDate before this acceleration',
    new_vesting_end_date DATE NOT NULL            COMMENT 'VestingEndDate after this acceleration',
    shares_accelerated DECIMAL(15,4) NOT NULL     COMMENT 'Additional shares unlocked by this acceleration',

    -- Who applied
    applied_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    applied_by CHAR(36) NOT NULL                 COMMENT 'FK: users.id',

    -- Audit (immutable — no update / soft-delete)
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),

    PRIMARY KEY (id),

    -- Foreign Keys
    CONSTRAINT fk_va_client FOREIGN KEY (client_id)
        REFERENCES clients(id) ON DELETE RESTRICT,
    CONSTRAINT fk_va_grant FOREIGN KEY (vesting_grant_id)
        REFERENCES vesting_grants(id) ON DELETE RESTRICT,
    CONSTRAINT fk_va_milestone FOREIGN KEY (grant_milestone_id)
        REFERENCES grant_milestones(id) ON DELETE RESTRICT,
    CONSTRAINT fk_va_applied_by FOREIGN KEY (applied_by)
        REFERENCES users(id) ON DELETE RESTRICT,

    -- Indexes
    INDEX idx_va_client (client_id),
    INDEX idx_va_grant (vesting_grant_id),
    INDEX idx_va_milestone (grant_milestone_id),
    INDEX idx_va_applied_at (applied_at),

    -- Uniqueness: one acceleration per milestone (prevent double-apply)
    UNIQUE KEY uq_va_milestone (grant_milestone_id),

    -- Check Constraints
    CONSTRAINT chk_va_acceleration_type CHECK (
        acceleration_type IN ('Percentage', 'Months', 'Shares')
    ),
    CONSTRAINT chk_va_acceleration_amount CHECK (acceleration_amount > 0),
    CONSTRAINT chk_va_dates CHECK (
        new_vesting_end_date <= original_vesting_end_date
    ),
    CONSTRAINT chk_va_shares CHECK (shares_accelerated >= 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Immutable ledger of vesting accelerations — one row per milestone achievement applied to a grant';
