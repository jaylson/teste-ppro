-- Migration 024: Create vesting_transactions table
-- Date: 2026-02-25
-- Description: Immutable ledger of all share exercise events from vesting grants

-- ============================================
-- TABLE: vesting_transactions
-- Purpose: Append-only log of every exercise event (shares converted from vested to exercised)
-- ============================================
CREATE TABLE IF NOT EXISTS vesting_transactions (
    id CHAR(36) NOT NULL,
    client_id CHAR(36) NOT NULL              COMMENT 'Multi-tenant isolation',
    vesting_grant_id CHAR(36) NOT NULL       COMMENT 'FK: vesting_grants.id',
    shareholder_id CHAR(36) NOT NULL         COMMENT 'FK: shareholders.id (denormalized)',
    company_id CHAR(36) NOT NULL             COMMENT 'FK: companies.id (denormalized)',

    -- Transaction details
    transaction_date DATE NOT NULL           COMMENT 'Date shares were exercised',
    shares_exercised DECIMAL(15,0) NOT NULL  COMMENT 'Number of shares exercised in this transaction',
    share_price_at_exercise DECIMAL(15,4) NOT NULL COMMENT 'Fair market value per share at exercise date',
    strike_price DECIMAL(15,4) NOT NULL      COMMENT 'Grant strike price (copied from grant)',
    total_exercise_value DECIMAL(18,4) GENERATED ALWAYS AS (shares_exercised * share_price_at_exercise) STORED
                                             COMMENT 'Total value: shares × FMV at exercise',
    gain_amount DECIMAL(18,4) GENERATED ALWAYS AS (shares_exercised * (share_price_at_exercise - strike_price)) STORED
                                             COMMENT 'Economic gain: (FMV - strike) × shares',

    -- Reference to resulting share transaction in cap table
    share_transaction_id CHAR(36) NULL       COMMENT 'FK: share_transactions.id created from this exercise',

    -- Approval
    transaction_type VARCHAR(30) NOT NULL DEFAULT 'Exercise'
                                             COMMENT 'Exercise | EarlyExercise | AcceleratedExercise',
    notes TEXT NULL,

    -- Audit fields (no update/delete - append only ledger)
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    created_by CHAR(36) NOT NULL             COMMENT 'FK: users.id who processed exercise',

    PRIMARY KEY (id),

    -- Foreign Keys
    CONSTRAINT fk_vt_client FOREIGN KEY (client_id)
        REFERENCES clients(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vt_grant FOREIGN KEY (vesting_grant_id)
        REFERENCES vesting_grants(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vt_shareholder FOREIGN KEY (shareholder_id)
        REFERENCES shareholders(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vt_company FOREIGN KEY (company_id)
        REFERENCES companies(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vt_share_transaction FOREIGN KEY (share_transaction_id)
        REFERENCES share_transactions(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vt_created_by FOREIGN KEY (created_by)
        REFERENCES users(id) ON DELETE RESTRICT,

    -- Indexes
    INDEX idx_vt_grant (vesting_grant_id),
    INDEX idx_vt_shareholder (shareholder_id),
    INDEX idx_vt_company (company_id),
    INDEX idx_vt_date (transaction_date),
    INDEX idx_vt_grant_date (vesting_grant_id, transaction_date),
    INDEX idx_vt_shareholder_date (shareholder_id, transaction_date),

    -- Check Constraints
    CONSTRAINT chk_vt_shares CHECK (shares_exercised > 0),
    CONSTRAINT chk_vt_fmv CHECK (share_price_at_exercise > 0),
    CONSTRAINT chk_vt_strike CHECK (strike_price >= 0),
    CONSTRAINT chk_vt_type CHECK (transaction_type IN ('Exercise', 'EarlyExercise', 'AcceleratedExercise'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Immutable exercise ledger - one row per exercise event, never updated or soft-deleted';
