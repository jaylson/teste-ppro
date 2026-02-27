-- Migration 040: Create grant_milestones table
-- Date: 2026-02-27
-- Description: Per-grant performance milestones with progress tracking and vesting acceleration.
--              These are distinct from the existing vesting_milestones (plan-level) — they are
--              attached directly to individual VestingGrants and support incremental progress recording.

-- ============================================
-- TABLE: grant_milestones
-- Purpose: Performance metas tied to a vesting grant; achievement triggers acceleration
-- ============================================
CREATE TABLE IF NOT EXISTS grant_milestones (
    id CHAR(36) NOT NULL,
    client_id CHAR(36) NOT NULL                  COMMENT 'Multi-tenant isolation',
    vesting_grant_id CHAR(36) NOT NULL           COMMENT 'FK: vesting_grants.id',
    milestone_template_id CHAR(36) NULL          COMMENT 'FK: milestone_templates.id — NULL if custom',
    company_id CHAR(36) NOT NULL                 COMMENT 'FK: companies.id (denormalized)',

    -- Milestone definition
    name VARCHAR(200) NOT NULL                   COMMENT 'Milestone name',
    description TEXT NULL                        COMMENT 'What must be achieved',
    category VARCHAR(50) NOT NULL                COMMENT 'Financial | Operational | Product | Market | Strategic',
    metric_type VARCHAR(20) NOT NULL             COMMENT 'Currency | Percentage | Count | Boolean',
    target_value DECIMAL(18,4) NOT NULL          COMMENT 'Numeric goal (for Boolean milestones use 1)',
    target_operator VARCHAR(20) NOT NULL         COMMENT 'GreaterThan | LessThan | Equals | GreaterOrEqual | LessOrEqual',
    target_date DATE NOT NULL                    COMMENT 'Deadline to achieve this milestone',
    measurement_frequency VARCHAR(20) NOT NULL   COMMENT 'Daily | Weekly | Monthly | Quarterly | Annually | OneTime',

    -- Progress tracking
    status VARCHAR(20) NOT NULL DEFAULT 'Pending' COMMENT 'Pending | InProgress | Achieved | Failed | Cancelled',
    current_value DECIMAL(18,4) NULL             COMMENT 'Latest recorded value',
    progress_percentage DECIMAL(5,2) NOT NULL DEFAULT 0 COMMENT '0-100 — calculated from current_value/target_value',

    -- Achievement data (set when status = Achieved)
    achieved_at DATETIME(6) NULL                 COMMENT 'When milestone was marked as achieved',
    achieved_value DECIMAL(18,4) NULL            COMMENT 'Value at time of achievement',
    verified_at DATETIME(6) NULL                 COMMENT 'When an approver confirmed the achievement',
    verified_by CHAR(36) NULL                   COMMENT 'FK: users.id — approver',

    -- Acceleration settings (can override template defaults)
    acceleration_type VARCHAR(20) NOT NULL       COMMENT 'Percentage | Months | Shares',
    acceleration_amount DECIMAL(8,4) NOT NULL    COMMENT 'Size of acceleration upon achievement',
    acceleration_applied TINYINT(1) NOT NULL DEFAULT 0 COMMENT '1 when acceleration has been executed',
    acceleration_applied_at DATETIME(6) NULL     COMMENT 'Timestamp when acceleration was applied to grant',

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
    CONSTRAINT fk_gm_client FOREIGN KEY (client_id)
        REFERENCES clients(id) ON DELETE RESTRICT,
    CONSTRAINT fk_gm_grant FOREIGN KEY (vesting_grant_id)
        REFERENCES vesting_grants(id) ON DELETE CASCADE,
    CONSTRAINT fk_gm_template FOREIGN KEY (milestone_template_id)
        REFERENCES milestone_templates(id) ON DELETE SET NULL,
    CONSTRAINT fk_gm_company FOREIGN KEY (company_id)
        REFERENCES companies(id) ON DELETE RESTRICT,
    CONSTRAINT fk_gm_verified_by FOREIGN KEY (verified_by)
        REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT fk_gm_created_by FOREIGN KEY (created_by)
        REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT fk_gm_updated_by FOREIGN KEY (updated_by)
        REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT fk_gm_deleted_by FOREIGN KEY (deleted_by)
        REFERENCES users(id) ON DELETE RESTRICT,

    -- Indexes
    INDEX idx_gm_client (client_id),
    INDEX idx_gm_grant (vesting_grant_id),
    INDEX idx_gm_company (company_id),
    INDEX idx_gm_status (status),
    INDEX idx_gm_target_date (target_date),
    INDEX idx_gm_grant_status (vesting_grant_id, status),
    INDEX idx_gm_deleted (is_deleted),

    -- Check Constraints
    CONSTRAINT chk_gm_category CHECK (
        category IN ('Financial', 'Operational', 'Product', 'Market', 'Strategic')
    ),
    CONSTRAINT chk_gm_metric_type CHECK (
        metric_type IN ('Currency', 'Percentage', 'Count', 'Boolean')
    ),
    CONSTRAINT chk_gm_target_operator CHECK (
        target_operator IN ('GreaterThan', 'LessThan', 'Equals', 'GreaterOrEqual', 'LessOrEqual')
    ),
    CONSTRAINT chk_gm_measurement_frequency CHECK (
        measurement_frequency IN ('Daily', 'Weekly', 'Monthly', 'Quarterly', 'Annually', 'OneTime')
    ),
    CONSTRAINT chk_gm_status CHECK (
        status IN ('Pending', 'InProgress', 'Achieved', 'Failed', 'Cancelled')
    ),
    CONSTRAINT chk_gm_acceleration_type CHECK (
        acceleration_type IN ('Percentage', 'Months', 'Shares')
    ),
    CONSTRAINT chk_gm_acceleration_amount CHECK (
        acceleration_amount > 0 AND acceleration_amount <= 100
    ),
    CONSTRAINT chk_gm_progress CHECK (
        progress_percentage >= 0 AND progress_percentage <= 100
    ),
    CONSTRAINT chk_gm_achieved_integrity CHECK (
        (status = 'Achieved' AND achieved_at IS NOT NULL AND achieved_value IS NOT NULL)
        OR status != 'Achieved'
    )
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Per-grant performance milestones with progress tracking and vesting acceleration support';
