-- Migration 048: Add target_value and target_unit to milestone_templates
-- Date: 2026-03-02
-- These optional fields provide default target values when a template
-- is applied to a grant milestone.

ALTER TABLE milestone_templates
    ADD COLUMN target_value DECIMAL(18,4) NULL AFTER target_operator,
    ADD COLUMN target_unit VARCHAR(50) NULL AFTER target_value;
