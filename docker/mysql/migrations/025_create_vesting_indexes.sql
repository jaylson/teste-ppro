-- Migration 025: Additional composite indexes for vesting tables
-- Date: 2026-02-25
-- Description: Performance-optimized composite indexes for common query patterns

-- ============================================
-- Additional indexes beyond what was created in 020-024
-- These cover multi-column filter + sort patterns seen in API queries
-- ============================================

-- vesting_plans: filter by client + company + status (admin list page)
CREATE INDEX idx_vp_client_company_status
    ON vesting_plans (client_id, company_id, status, is_deleted);

-- vesting_plans: sort by created_at for pagination
CREATE INDEX idx_vp_company_created
    ON vesting_plans (company_id, created_at);

-- vesting_grants: the most common query - beneficiary dashboard
CREATE INDEX idx_vg_shareholder_company_status
    ON vesting_grants (shareholder_id, company_id, status, is_deleted);

-- vesting_grants: admin view - all grants for a plan
CREATE INDEX idx_vg_plan_status_created
    ON vesting_grants (vesting_plan_id, status, created_at);

-- vesting_grants: cap table integration - active grants pending exercise
CREATE INDEX idx_vg_company_active_vested
    ON vesting_grants (company_id, status, vested_shares, exercised_shares);

-- vesting_schedules: upcoming vesting events (cron job / notifications)
CREATE INDEX idx_vs_upcoming_events
    ON vesting_schedules (schedule_date, status, is_deleted);

-- vesting_milestones: plan milestones ordered by target date
CREATE INDEX idx_vm_plan_target_date
    ON vesting_milestones (vesting_plan_id, target_date, status, is_deleted);

-- vesting_transactions: financial reporting - transactions by period
CREATE INDEX idx_vt_company_date_range
    ON vesting_transactions (company_id, transaction_date);

-- vesting_transactions: shareholder exercise history
CREATE INDEX idx_vt_shareholder_grant
    ON vesting_transactions (shareholder_id, vesting_grant_id, transaction_date);
