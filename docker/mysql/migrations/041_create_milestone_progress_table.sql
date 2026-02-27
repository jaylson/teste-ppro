-- Migration 041: Create milestone_progress table
-- Date: 2026-02-27
-- Description: Time-series log of incremental progress updates for grant milestones

-- ============================================
-- TABLE: milestone_progress
-- Purpose: Append-only log of progress measurements for grant_milestones
-- ============================================
CREATE TABLE IF NOT EXISTS milestone_progress (
    id CHAR(36) NOT NULL,
    client_id CHAR(36) NOT NULL                  COMMENT 'Multi-tenant isolation',
    grant_milestone_id CHAR(36) NOT NULL         COMMENT 'FK: grant_milestones.id',

    -- Progress data
    recorded_date DATE NOT NULL                  COMMENT 'Date the measurement applies to',
    recorded_value DECIMAL(18,4) NOT NULL        COMMENT 'Measured value on recorded_date',
    progress_percentage DECIMAL(5,2) NOT NULL    COMMENT '0-100 — captured at recording time',
    notes TEXT NULL                              COMMENT 'Optional observation or context',
    data_source VARCHAR(100) NULL                COMMENT 'Manual | APIIntegration | SystemCalculation',

    -- Who recorded
    recorded_by CHAR(36) NOT NULL               COMMENT 'FK: users.id',

    -- Audit (no soft-delete — immutable log)
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),

    PRIMARY KEY (id),

    -- Foreign Keys
    CONSTRAINT fk_mp_client FOREIGN KEY (client_id)
        REFERENCES clients(id) ON DELETE RESTRICT,
    CONSTRAINT fk_mp_milestone FOREIGN KEY (grant_milestone_id)
        REFERENCES grant_milestones(id) ON DELETE CASCADE,
    CONSTRAINT fk_mp_recorded_by FOREIGN KEY (recorded_by)
        REFERENCES users(id) ON DELETE RESTRICT,

    -- Indexes — optimized for time-series queries
    INDEX idx_mp_client (client_id),
    INDEX idx_mp_milestone_date (grant_milestone_id, recorded_date),
    INDEX idx_mp_milestone (grant_milestone_id),

    -- Check Constraints
    CONSTRAINT chk_mp_progress CHECK (progress_percentage >= 0 AND progress_percentage <= 100),
    CONSTRAINT chk_mp_data_source CHECK (
        data_source IS NULL
        OR data_source IN ('Manual', 'APIIntegration', 'SystemCalculation')
    )
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Immutable time-series log of progress measurements for grant milestones';
