-- Migration 020: Create vesting_plans table
-- Date: 2026-02-25
-- Description: Core table for Vesting Plans - defines vesting schedules for equity compensation

-- ============================================
-- TABLE: vesting_plans
-- Purpose: Defines vesting plan templates per company
-- ============================================
CREATE TABLE IF NOT EXISTS vesting_plans (
    id CHAR(36) NOT NULL,
    client_id CHAR(36) NOT NULL                 COMMENT 'Multi-tenant isolation',
    company_id CHAR(36) NOT NULL                 COMMENT 'FK: companies.id',
    name VARCHAR(200) NOT NULL                   COMMENT 'Plan name e.g. Standard 4-Year Plan',
    description TEXT NULL                        COMMENT 'Detailed description',
    vesting_type VARCHAR(50) NOT NULL            COMMENT 'TimeBasedLinear | TimeBasedCliff | MilestoneBasedOnly | HybridTimeMilestone',
    cliff_months INT NOT NULL DEFAULT 0          COMMENT 'Months before first vesting (0 = no cliff)',
    vesting_months INT NOT NULL DEFAULT 48       COMMENT 'Total vesting period in months',
    total_equity_percentage DECIMAL(8,4) NOT NULL COMMENT 'Total equity % this plan may grant',
    status VARCHAR(20) NOT NULL DEFAULT 'Draft'  COMMENT 'Draft | Active | Inactive | Archived',
    activated_at DATETIME(6) NULL               COMMENT 'When plan was activated',
    activated_by CHAR(36) NULL                  COMMENT 'FK: users.id who activated',
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
    CONSTRAINT fk_vp_client FOREIGN KEY (client_id)
        REFERENCES clients(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vp_company FOREIGN KEY (company_id)
        REFERENCES companies(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vp_created_by FOREIGN KEY (created_by)
        REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vp_updated_by FOREIGN KEY (updated_by)
        REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vp_activated_by FOREIGN KEY (activated_by)
        REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vp_deleted_by FOREIGN KEY (deleted_by)
        REFERENCES users(id) ON DELETE RESTRICT,

    -- Indexes
    INDEX idx_vp_client (client_id),
    INDEX idx_vp_company (company_id),
    INDEX idx_vp_status (status),
    INDEX idx_vp_company_status (company_id, status),
    INDEX idx_vp_deleted (is_deleted),

    -- Check Constraints
    CONSTRAINT chk_vp_cliff_months CHECK (cliff_months >= 0 AND cliff_months <= 120),
    CONSTRAINT chk_vp_vesting_months CHECK (vesting_months >= 1 AND vesting_months <= 240),
    CONSTRAINT chk_vp_equity_pct CHECK (total_equity_percentage > 0 AND total_equity_percentage <= 100),
    CONSTRAINT chk_vp_vesting_type CHECK (vesting_type IN ('TimeBasedLinear', 'TimeBasedCliff', 'MilestoneBasedOnly', 'HybridTimeMilestone')),
    CONSTRAINT chk_vp_status CHECK (status IN ('Draft', 'Active', 'Inactive', 'Archived'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Vesting plan definitions per company - templates used to create grants for shareholders';
