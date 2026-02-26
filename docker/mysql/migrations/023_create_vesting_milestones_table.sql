-- Migration 023: Create vesting_milestones table
-- Date: 2026-02-25
-- Description: Performance milestones that can trigger or accelerate vesting

-- ============================================
-- TABLE: vesting_milestones
-- Purpose: Milestone-based vesting triggers (e.g. "reach $1M ARR" → accelerate 10%)
-- ============================================
CREATE TABLE IF NOT EXISTS vesting_milestones (
    id CHAR(36) NOT NULL,
    client_id CHAR(36) NOT NULL              COMMENT 'Multi-tenant isolation',
    vesting_plan_id CHAR(36) NOT NULL        COMMENT 'FK: vesting_plans.id',
    company_id CHAR(36) NOT NULL             COMMENT 'FK: companies.id (denormalized)',

    -- Milestone definition
    name VARCHAR(200) NOT NULL               COMMENT 'Milestone name e.g. Series A Closing',
    description TEXT NULL                    COMMENT 'Detailed criteria description',
    milestone_type VARCHAR(50) NOT NULL      COMMENT 'Financial | Product | Operational | Custom',
    target_value DECIMAL(18,4) NULL          COMMENT 'Numeric target (revenue, users, etc.) - NULL for non-numeric',
    target_unit VARCHAR(50) NULL             COMMENT 'BRL | USD | Users | Percentage | Custom',

    -- Vesting acceleration
    acceleration_percentage DECIMAL(8,4) NOT NULL DEFAULT 0
                                             COMMENT '% of remaining unvested shares to accelerate upon completion',
    is_required_for_full_vesting TINYINT(1) NOT NULL DEFAULT 0
                                             COMMENT 'If 1, vesting halts until milestone is met',

    -- Status
    status VARCHAR(20) NOT NULL DEFAULT 'Pending' COMMENT 'Pending | Achieved | Failed | Cancelled',
    target_date DATE NULL                    COMMENT 'Expected date to achieve milestone',
    achieved_date DATE NULL                  COMMENT 'Actual date milestone was achieved',
    achieved_by CHAR(36) NULL               COMMENT 'FK: users.id who marked as achieved',
    achieved_value DECIMAL(18,4) NULL        COMMENT 'Actual value achieved',

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
    CONSTRAINT fk_vm_client FOREIGN KEY (client_id)
        REFERENCES clients(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vm_plan FOREIGN KEY (vesting_plan_id)
        REFERENCES vesting_plans(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vm_company FOREIGN KEY (company_id)
        REFERENCES companies(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vm_achieved_by FOREIGN KEY (achieved_by)
        REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vm_created_by FOREIGN KEY (created_by)
        REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vm_updated_by FOREIGN KEY (updated_by)
        REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT fk_vm_deleted_by FOREIGN KEY (deleted_by)
        REFERENCES users(id) ON DELETE RESTRICT,

    -- Indexes
    INDEX idx_vm_plan (vesting_plan_id),
    INDEX idx_vm_company (company_id),
    INDEX idx_vm_status (status),
    INDEX idx_vm_plan_status (vesting_plan_id, status),
    INDEX idx_vm_target_date (target_date),
    INDEX idx_vm_deleted (is_deleted),

    -- Check Constraints
    CONSTRAINT chk_vm_acceleration CHECK (acceleration_percentage >= 0 AND acceleration_percentage <= 100),
    CONSTRAINT chk_vm_milestone_type CHECK (milestone_type IN ('Financial', 'Product', 'Operational', 'Custom')),
    CONSTRAINT chk_vm_status CHECK (status IN ('Pending', 'Achieved', 'Failed', 'Cancelled'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Performance milestones that trigger or accelerate vesting for a plan';
