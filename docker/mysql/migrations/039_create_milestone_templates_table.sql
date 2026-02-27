-- Migration 039: Create milestone_templates table
-- Date: 2026-02-27
-- Description: Reusable milestone templates per company for performance-based vesting acceleration

-- ============================================
-- TABLE: milestone_templates
-- Purpose: Define reusable metric targets that can be attached to individual vesting grants
-- ============================================
CREATE TABLE IF NOT EXISTS milestone_templates (
    id CHAR(36) NOT NULL,
    client_id CHAR(36) NOT NULL                  COMMENT 'Multi-tenant isolation',
    company_id CHAR(36) NOT NULL                 COMMENT 'FK: companies.id',

    -- Template definition
    name VARCHAR(200) NOT NULL                   COMMENT 'Template name e.g. ARR $1M Target',
    description TEXT NULL                        COMMENT 'Detailed description of what must be achieved',
    category VARCHAR(50) NOT NULL                COMMENT 'Financial | Operational | Product | Market | Strategic',
    metric_type VARCHAR(20) NOT NULL             COMMENT 'Currency | Percentage | Count | Boolean',
    target_operator VARCHAR(20) NOT NULL         COMMENT 'GreaterThan | LessThan | Equals | GreaterOrEqual | LessOrEqual',
    measurement_frequency VARCHAR(20) NOT NULL   COMMENT 'Daily | Weekly | Monthly | Quarterly | Annually | OneTime',
    is_active TINYINT(1) NOT NULL DEFAULT 1     COMMENT 'Whether template is available for new milestones',

    -- Acceleration configuration
    acceleration_type VARCHAR(20) NOT NULL       COMMENT 'Percentage | Months | Shares',
    acceleration_amount DECIMAL(8,4) NOT NULL    COMMENT 'Amount of acceleration (% of period / number of months / number of shares)',
    max_acceleration_cap DECIMAL(8,4) NULL       COMMENT 'Maximum cumulative acceleration allowed (NULL = 75% default)',

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
    CONSTRAINT fk_mt_client FOREIGN KEY (client_id)
        REFERENCES clients(id) ON DELETE RESTRICT,
    CONSTRAINT fk_mt_company FOREIGN KEY (company_id)
        REFERENCES companies(id) ON DELETE RESTRICT,
    CONSTRAINT fk_mt_created_by FOREIGN KEY (created_by)
        REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT fk_mt_updated_by FOREIGN KEY (updated_by)
        REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT fk_mt_deleted_by FOREIGN KEY (deleted_by)
        REFERENCES users(id) ON DELETE RESTRICT,

    -- Indexes
    INDEX idx_mt_client (client_id),
    INDEX idx_mt_company (company_id),
    INDEX idx_mt_category (category),
    INDEX idx_mt_active (is_active),
    INDEX idx_mt_deleted (is_deleted),
    INDEX idx_mt_company_active (company_id, is_active, is_deleted),

    -- Check Constraints
    CONSTRAINT chk_mt_category CHECK (
        category IN ('Financial', 'Operational', 'Product', 'Market', 'Strategic')
    ),
    CONSTRAINT chk_mt_metric_type CHECK (
        metric_type IN ('Currency', 'Percentage', 'Count', 'Boolean')
    ),
    CONSTRAINT chk_mt_target_operator CHECK (
        target_operator IN ('GreaterThan', 'LessThan', 'Equals', 'GreaterOrEqual', 'LessOrEqual')
    ),
    CONSTRAINT chk_mt_measurement_frequency CHECK (
        measurement_frequency IN ('Daily', 'Weekly', 'Monthly', 'Quarterly', 'Annually', 'OneTime')
    ),
    CONSTRAINT chk_mt_acceleration_type CHECK (
        acceleration_type IN ('Percentage', 'Months', 'Shares')
    ),
    CONSTRAINT chk_mt_acceleration_amount CHECK (
        acceleration_amount > 0 AND acceleration_amount <= 100
    ),
    CONSTRAINT chk_mt_max_cap CHECK (
        max_acceleration_cap IS NULL
        OR (max_acceleration_cap >= acceleration_amount AND max_acceleration_cap <= 100)
    )
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Reusable milestone templates used to drive performance-based vesting acceleration';

-- ─── Seed: Example templates ──────────────────────────────────────────────────
-- These will be inserted by seeding scripts; no defaults here to avoid company FK dependency.
