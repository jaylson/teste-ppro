-- Migration 026: Insert vesting seed data for testing
-- Date: 2026-02-25
-- Description: Sample vesting plans and grants for development/testing purposes
-- NOTE: Only inserts if the system test client and a company exist

-- ============================================
-- VARIABLES (MySQL stored variables for FK references)
-- ============================================
SET @system_client_id = '00000000-0000-0000-0000-000000000001';
SET @system_user_id   = '00000000-0000-0000-0000-000000000001';

-- Only run seed if there is at least one company for this client
SET @first_company_id = (
    SELECT id FROM companies
    WHERE is_deleted = 0
    LIMIT 1
);

-- Skip entire seed block if no company found
SET @skip_seed = IF(@first_company_id IS NULL, 1, 0);

-- ============================================
-- VESTING PLANS (3 common templates)
-- ============================================

-- Plan 1: Standard 4-year with 1-year cliff (most common for founders/employees)
SET @plan_1_id = UUID();
INSERT INTO vesting_plans (
    id, client_id, company_id, name, description,
    vesting_type, cliff_months, vesting_months, total_equity_percentage,
    status, created_by, updated_by
)
SELECT
    @plan_1_id,
    @system_client_id,
    @first_company_id,
    'Plano Padrão 4 Anos (Cliff 1 Ano)',
    'Plano padrão para fundadores e executivos: 25% vestem após 1 ano de cliff, restante mensalmente em 36 meses.',
    'TimeBasedLinear',
    12,   -- 12 months cliff
    48,   -- 48 months total
    5.0,  -- up to 5% equity per grant
    'Active',
    @system_user_id,
    @system_user_id
WHERE @skip_seed = 0
AND NOT EXISTS (
    SELECT 1 FROM vesting_plans WHERE name = 'Plano Padrão 4 Anos (Cliff 1 Ano)' AND is_deleted = 0
);

-- Plan 2: Accelerated 3-year for early employees (no cliff)
SET @plan_2_id = UUID();
INSERT INTO vesting_plans (
    id, client_id, company_id, name, description,
    vesting_type, cliff_months, vesting_months, total_equity_percentage,
    status, created_by, updated_by
)
SELECT
    @plan_2_id,
    @system_client_id,
    @first_company_id,
    'Early Employees 3 Anos (Sem Cliff)',
    'Plano para primeiros funcionários: vesting linear mensal por 36 meses sem período de cliff.',
    'TimeBasedLinear',
    0,    -- no cliff
    36,   -- 36 months total
    2.5,  -- up to 2.5% equity per grant
    'Active',
    @system_user_id,
    @system_user_id
WHERE @skip_seed = 0
AND NOT EXISTS (
    SELECT 1 FROM vesting_plans WHERE name = 'Early Employees 3 Anos (Sem Cliff)' AND is_deleted = 0
);

-- Plan 3: Hybrid time + milestone (for advisors)
SET @plan_3_id = UUID();
INSERT INTO vesting_plans (
    id, client_id, company_id, name, description,
    vesting_type, cliff_months, vesting_months, total_equity_percentage,
    status, created_by, updated_by
)
SELECT
    @plan_3_id,
    @system_client_id,
    @first_company_id,
    'Advisor 2 Anos Híbrido',
    'Plano para advisors: 50% por tempo (24 meses, cliff 6 meses) + 50% por milestones de desempenho.',
    'HybridTimeMilestone',
    6,    -- 6 months cliff
    24,   -- 24 months total
    1.0,  -- up to 1% equity per grant
    'Draft',
    @system_user_id,
    @system_user_id
WHERE @skip_seed = 0
AND NOT EXISTS (
    SELECT 1 FROM vesting_plans WHERE name = 'Advisor 2 Anos Híbrido' AND is_deleted = 0
);

-- ============================================
-- MILESTONES for Plan 3 (Hybrid Advisor)
-- Only insert if plan 3 was created and we can resolve its true id
-- ============================================
SET @plan_3_real_id = (
    SELECT id FROM vesting_plans
    WHERE name = 'Advisor 2 Anos Híbrido' AND is_deleted = 0
    LIMIT 1
);

INSERT INTO vesting_milestones (
    id, client_id, vesting_plan_id, company_id,
    name, description, milestone_type,
    target_value, target_unit,
    acceleration_percentage, is_required_for_full_vesting,
    status, target_date,
    created_by, updated_by
)
SELECT
    UUID(),
    @system_client_id,
    @plan_3_real_id,
    @first_company_id,
    'Fechamento Série A',
    'Empresa fecha rodada Série A de pelo menos R$ 5 milhões.',
    'Financial',
    5000000.00,
    'BRL',
    25.0,  -- 25% acceleration of remaining unvested
    0,
    'Pending',
    DATE_ADD(CURDATE(), INTERVAL 12 MONTH),
    @system_user_id,
    @system_user_id
WHERE @plan_3_real_id IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM vesting_milestones WHERE name = 'Fechamento Série A' AND is_deleted = 0
);

INSERT INTO vesting_milestones (
    id, client_id, vesting_plan_id, company_id,
    name, description, milestone_type,
    target_value, target_unit,
    acceleration_percentage, is_required_for_full_vesting,
    status, target_date,
    created_by, updated_by
)
SELECT
    UUID(),
    @system_client_id,
    @plan_3_real_id,
    @first_company_id,
    'Lançamento do Produto MVP',
    'Produto mínimo viável lançado com ao menos 100 usuários ativos.',
    'Product',
    100,
    'Users',
    25.0,
    0,
    'Pending',
    DATE_ADD(CURDATE(), INTERVAL 6 MONTH),
    @system_user_id,
    @system_user_id
WHERE @plan_3_real_id IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM vesting_milestones WHERE name = 'Lançamento do Produto MVP' AND is_deleted = 0
);
