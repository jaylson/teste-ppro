-- Migration 065: Fix DEMO UUID format
-- IDs with prefix 'DEMO' contain 'M' and 'O' which are not valid hexadecimal
-- characters, causing Dapper to fail when parsing CHAR(36) as System.Guid.
-- Fix: replace 'DEMO' -> 'DE00' (all valid hex) in every affected column.
-- This preserves the 36-character length and the uniqueness of each ID.

SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================
-- 1. clients
-- ============================================================
UPDATE clients
SET id = REPLACE(id, 'DEMO', 'DE00')
WHERE id LIKE 'DEMO%';

-- ============================================================
-- 2. companies
-- ============================================================
UPDATE companies
SET
    id        = IF(id        LIKE 'DEMO%', REPLACE(id,        'DEMO', 'DE00'), id),
    client_id = IF(client_id LIKE 'DEMO%', REPLACE(client_id, 'DEMO', 'DE00'), client_id),
    created_by = IF(created_by LIKE 'DEMO%', REPLACE(created_by, 'DEMO', 'DE00'), created_by)
WHERE id LIKE 'DEMO%' OR client_id LIKE 'DEMO%';

-- ============================================================
-- 3. users
-- ============================================================
UPDATE users
SET
    id         = IF(id         LIKE 'DEMO%', REPLACE(id,         'DEMO', 'DE00'), id),
    client_id  = IF(client_id  LIKE 'DEMO%', REPLACE(client_id,  'DEMO', 'DE00'), client_id),
    company_id = IF(company_id LIKE 'DEMO%', REPLACE(company_id, 'DEMO', 'DE00'), company_id),
    created_by = IF(created_by LIKE 'DEMO%', REPLACE(created_by, 'DEMO', 'DE00'), created_by),
    updated_by = IF(updated_by LIKE 'DEMO%', REPLACE(updated_by, 'DEMO', 'DE00'), updated_by)
WHERE id LIKE 'DEMO%';

-- ============================================================
-- 4. user_roles
-- ============================================================
UPDATE user_roles
SET
    user_id    = IF(user_id    LIKE 'DEMO%', REPLACE(user_id,    'DEMO', 'DE00'), user_id),
    granted_by = IF(granted_by LIKE 'DEMO%', REPLACE(granted_by, 'DEMO', 'DE00'), granted_by)
WHERE user_id LIKE 'DEMO%';

-- ============================================================
-- 5. user_companies
-- ============================================================
UPDATE user_companies
SET
    user_id    = IF(user_id    LIKE 'DEMO%', REPLACE(user_id,    'DEMO', 'DE00'), user_id),
    company_id = IF(company_id LIKE 'DEMO%', REPLACE(company_id, 'DEMO', 'DE00'), company_id)
WHERE user_id LIKE 'DEMO%' OR company_id LIKE 'DEMO%';

-- ============================================================
-- 6. shareholders
-- ============================================================
UPDATE shareholders
SET
    id         = IF(id         LIKE 'DEMO%', REPLACE(id,         'DEMO', 'DE00'), id),
    client_id  = IF(client_id  LIKE 'DEMO%', REPLACE(client_id,  'DEMO', 'DE00'), client_id),
    company_id = IF(company_id LIKE 'DEMO%', REPLACE(company_id, 'DEMO', 'DE00'), company_id),
    created_by = IF(created_by LIKE 'DEMO%', REPLACE(created_by, 'DEMO', 'DE00'), created_by),
    updated_by = IF(updated_by LIKE 'DEMO%', REPLACE(updated_by, 'DEMO', 'DE00'), updated_by)
WHERE id LIKE 'DEMO%';

-- ============================================================
-- 7. share_classes
-- ============================================================
UPDATE share_classes
SET
    id         = IF(id         LIKE 'DEMO%', REPLACE(id,         'DEMO', 'DE00'), id),
    client_id  = IF(client_id  LIKE 'DEMO%', REPLACE(client_id,  'DEMO', 'DE00'), client_id),
    company_id = IF(company_id LIKE 'DEMO%', REPLACE(company_id, 'DEMO', 'DE00'), company_id),
    created_by = IF(created_by LIKE 'DEMO%', REPLACE(created_by, 'DEMO', 'DE00'), created_by),
    updated_by = IF(updated_by LIKE 'DEMO%', REPLACE(updated_by, 'DEMO', 'DE00'), updated_by)
WHERE id LIKE 'DEMO%';

-- ============================================================
-- 8. shares
-- ============================================================
UPDATE shares
SET
    id               = IF(id               LIKE 'DEMO%', REPLACE(id,               'DEMO', 'DE00'), id),
    client_id        = IF(client_id        LIKE 'DEMO%', REPLACE(client_id,        'DEMO', 'DE00'), client_id),
    company_id       = IF(company_id       LIKE 'DEMO%', REPLACE(company_id,       'DEMO', 'DE00'), company_id),
    shareholder_id   = IF(shareholder_id   LIKE 'DEMO%', REPLACE(shareholder_id,   'DEMO', 'DE00'), shareholder_id),
    share_class_id   = IF(share_class_id   LIKE 'DEMO%', REPLACE(share_class_id,   'DEMO', 'DE00'), share_class_id),
    origin_transaction_id = IF(origin_transaction_id LIKE 'DEMO%', REPLACE(origin_transaction_id, 'DEMO', 'DE00'), origin_transaction_id),
    created_by       = IF(created_by       LIKE 'DEMO%', REPLACE(created_by,       'DEMO', 'DE00'), created_by),
    updated_by       = IF(updated_by       LIKE 'DEMO%', REPLACE(updated_by,       'DEMO', 'DE00'), updated_by)
WHERE id LIKE 'DEMO%'
   OR client_id LIKE 'DEMO%'
   OR company_id LIKE 'DEMO%'
   OR shareholder_id LIKE 'DEMO%'
   OR share_class_id LIKE 'DEMO%';

-- ============================================================
-- 9. share_transactions (cap table)
-- ============================================================
UPDATE share_transactions
SET
    id                   = IF(id                   LIKE 'DEMO%', REPLACE(id,                   'DEMO', 'DE00'), id),
    client_id            = IF(client_id            LIKE 'DEMO%', REPLACE(client_id,            'DEMO', 'DE00'), client_id),
    company_id           = IF(company_id           LIKE 'DEMO%', REPLACE(company_id,           'DEMO', 'DE00'), company_id),
    share_id             = IF(share_id             LIKE 'DEMO%', REPLACE(share_id,             'DEMO', 'DE00'), share_id),
    share_class_id       = IF(share_class_id       LIKE 'DEMO%', REPLACE(share_class_id,       'DEMO', 'DE00'), share_class_id),
    from_shareholder_id  = IF(from_shareholder_id  LIKE 'DEMO%', REPLACE(from_shareholder_id,  'DEMO', 'DE00'), from_shareholder_id),
    to_shareholder_id    = IF(to_shareholder_id    LIKE 'DEMO%', REPLACE(to_shareholder_id,    'DEMO', 'DE00'), to_shareholder_id),
    approved_by          = IF(approved_by          LIKE 'DEMO%', REPLACE(approved_by,          'DEMO', 'DE00'), approved_by),
    created_by           = IF(created_by           LIKE 'DEMO%', REPLACE(created_by,           'DEMO', 'DE00'), created_by)
WHERE id LIKE 'DEMO%' OR client_id LIKE 'DEMO%';

-- ============================================================
-- 10. vesting_plans
-- ============================================================
UPDATE vesting_plans
SET
    id         = IF(id         LIKE 'DEMO%', REPLACE(id,         'DEMO', 'DE00'), id),
    client_id  = IF(client_id  LIKE 'DEMO%', REPLACE(client_id,  'DEMO', 'DE00'), client_id),
    company_id = IF(company_id LIKE 'DEMO%', REPLACE(company_id, 'DEMO', 'DE00'), company_id),
    created_by = IF(created_by LIKE 'DEMO%', REPLACE(created_by, 'DEMO', 'DE00'), created_by),
    updated_by = IF(updated_by LIKE 'DEMO%', REPLACE(updated_by, 'DEMO', 'DE00'), updated_by)
WHERE id LIKE 'DEMO%';

-- ============================================================
-- 11. vesting_grants
-- ============================================================
UPDATE vesting_grants
SET
    id             = IF(id             LIKE 'DEMO%', REPLACE(id,             'DEMO', 'DE00'), id),
    client_id      = IF(client_id      LIKE 'DEMO%', REPLACE(client_id,      'DEMO', 'DE00'), client_id),
    company_id     = IF(company_id     LIKE 'DEMO%', REPLACE(company_id,     'DEMO', 'DE00'), company_id),
    vesting_plan_id = IF(vesting_plan_id LIKE 'DEMO%', REPLACE(vesting_plan_id, 'DEMO', 'DE00'), vesting_plan_id),
    shareholder_id = IF(shareholder_id LIKE 'DEMO%', REPLACE(shareholder_id, 'DEMO', 'DE00'), shareholder_id),
    created_by     = IF(created_by     LIKE 'DEMO%', REPLACE(created_by,     'DEMO', 'DE00'), created_by),
    updated_by     = IF(updated_by     LIKE 'DEMO%', REPLACE(updated_by,     'DEMO', 'DE00'), updated_by)
WHERE id LIKE 'DEMO%';

-- ============================================================
-- 12. vesting_schedules
-- ============================================================
UPDATE vesting_schedules
SET
    id               = IF(id               LIKE 'DEMO%', REPLACE(id,               'DEMO', 'DE00'), id),
    client_id        = IF(client_id        LIKE 'DEMO%', REPLACE(client_id,        'DEMO', 'DE00'), client_id),
    company_id       = IF(company_id       LIKE 'DEMO%', REPLACE(company_id,       'DEMO', 'DE00'), company_id),
    vesting_grant_id = IF(vesting_grant_id LIKE 'DEMO%', REPLACE(vesting_grant_id, 'DEMO', 'DE00'), vesting_grant_id),
    created_by       = IF(created_by       LIKE 'DEMO%', REPLACE(created_by,       'DEMO', 'DE00'), created_by),
    updated_by       = IF(updated_by       LIKE 'DEMO%', REPLACE(updated_by,       'DEMO', 'DE00'), updated_by)
WHERE id LIKE 'DEMO%' OR vesting_grant_id LIKE 'DEMO%' OR client_id LIKE 'DEMO%';

-- ============================================================
-- 13. valuations
-- ============================================================
UPDATE valuations
SET
    id         = IF(id         LIKE 'DEMO%', REPLACE(id,         'DEMO', 'DE00'), id),
    client_id  = IF(client_id  LIKE 'DEMO%', REPLACE(client_id,  'DEMO', 'DE00'), client_id),
    company_id = IF(company_id LIKE 'DEMO%', REPLACE(company_id, 'DEMO', 'DE00'), company_id),
    created_by = IF(created_by LIKE 'DEMO%', REPLACE(created_by, 'DEMO', 'DE00'), created_by),
    updated_by = IF(updated_by LIKE 'DEMO%', REPLACE(updated_by, 'DEMO', 'DE00'), updated_by)
WHERE id LIKE 'DEMO%';

-- ============================================================
-- 14. financial_periods
-- ============================================================
UPDATE financial_periods
SET
    id           = IF(id           LIKE 'DEMO%', REPLACE(id,           'DEMO', 'DE00'), id),
    client_id    = IF(client_id    LIKE 'DEMO%', REPLACE(client_id,    'DEMO', 'DE00'), client_id),
    company_id   = IF(company_id   LIKE 'DEMO%', REPLACE(company_id,   'DEMO', 'DE00'), company_id),
    created_by   = IF(created_by   LIKE 'DEMO%', REPLACE(created_by,   'DEMO', 'DE00'), created_by),
    updated_by   = IF(updated_by   LIKE 'DEMO%', REPLACE(updated_by,   'DEMO', 'DE00'), updated_by)
WHERE id LIKE 'DEMO%' OR client_id LIKE 'DEMO%';

-- ============================================================
-- 15. financial_metrics
-- ============================================================
UPDATE financial_metrics
SET
    client_id  = IF(client_id  LIKE 'DEMO%', REPLACE(client_id,  'DEMO', 'DE00'), client_id),
    period_id  = IF(period_id  LIKE 'DEMO%', REPLACE(period_id,  'DEMO', 'DE00'), period_id),
    created_by = IF(created_by LIKE 'DEMO%', REPLACE(created_by, 'DEMO', 'DE00'), created_by),
    updated_by = IF(updated_by LIKE 'DEMO%', REPLACE(updated_by, 'DEMO', 'DE00'), updated_by)
WHERE client_id LIKE 'DEMO%' OR period_id LIKE 'DEMO%';

-- ============================================================
-- 16. contracts
-- ============================================================
UPDATE contracts
SET
    id         = IF(id         LIKE 'DEMO%', REPLACE(id,         'DEMO', 'DE00'), id),
    client_id  = IF(client_id  LIKE 'DEMO%', REPLACE(client_id,  'DEMO', 'DE00'), client_id),
    company_id = IF(company_id LIKE 'DEMO%', REPLACE(company_id, 'DEMO', 'DE00'), company_id),
    created_by = IF(created_by LIKE 'DEMO%', REPLACE(created_by, 'DEMO', 'DE00'), created_by),
    updated_by = IF(updated_by LIKE 'DEMO%', REPLACE(updated_by, 'DEMO', 'DE00'), updated_by)
WHERE id LIKE 'DEMO%';

-- ============================================================
-- 17. contract_parties
-- ============================================================
UPDATE contract_parties
SET
    id             = IF(id             LIKE 'DEMO%', REPLACE(id,             'DEMO', 'DE00'), id),
    contract_id    = IF(contract_id    LIKE 'DEMO%', REPLACE(contract_id,    'DEMO', 'DE00'), contract_id),
    user_id        = IF(user_id        LIKE 'DEMO%', REPLACE(user_id,        'DEMO', 'DE00'), user_id),
    shareholder_id = IF(shareholder_id LIKE 'DEMO%', REPLACE(shareholder_id, 'DEMO', 'DE00'), shareholder_id)
WHERE id LIKE 'DEMO%' OR contract_id LIKE 'DEMO%' OR shareholder_id LIKE 'DEMO%';

-- ============================================================
-- 18. contract_versions
-- ============================================================
UPDATE contract_versions
SET
    id          = IF(id          LIKE 'DEMO%', REPLACE(id,          'DEMO', 'DE00'), id),
    contract_id = IF(contract_id LIKE 'DEMO%', REPLACE(contract_id, 'DEMO', 'DE00'), contract_id),
    created_by  = IF(created_by  LIKE 'DEMO%', REPLACE(created_by,  'DEMO', 'DE00'), created_by)
WHERE id LIKE 'DEMO%' OR contract_id LIKE 'DEMO%';

-- ============================================================
-- 19. communications
-- ============================================================
UPDATE communications
SET
    id         = IF(id         LIKE 'DEMO%', REPLACE(id,         'DEMO', 'DE00'), id),
    company_id = IF(company_id LIKE 'DEMO%', REPLACE(company_id, 'DEMO', 'DE00'), company_id),
    created_by = IF(created_by LIKE 'DEMO%', REPLACE(created_by, 'DEMO', 'DE00'), created_by),
    updated_by = IF(updated_by LIKE 'DEMO%', REPLACE(updated_by, 'DEMO', 'DE00'), updated_by)
WHERE id LIKE 'DEMO%';

-- ============================================================
-- 20. data_rooms
-- ============================================================
UPDATE data_rooms
SET
    id         = IF(id         LIKE 'DEMO%', REPLACE(id,         'DEMO', 'DE00'), id),
    company_id = IF(company_id LIKE 'DEMO%', REPLACE(company_id, 'DEMO', 'DE00'), company_id),
    created_by = IF(created_by LIKE 'DEMO%', REPLACE(created_by, 'DEMO', 'DE00'), created_by),
    updated_by = IF(updated_by LIKE 'DEMO%', REPLACE(updated_by, 'DEMO', 'DE00'), updated_by)
WHERE id LIKE 'DEMO%';

-- ============================================================
-- 21. data_room_folders
-- ============================================================
UPDATE data_room_folders
SET
    id               = IF(id               LIKE 'DEMO%', REPLACE(id,               'DEMO', 'DE00'), id),
    data_room_id     = IF(data_room_id     LIKE 'DEMO%', REPLACE(data_room_id,     'DEMO', 'DE00'), data_room_id),
    parent_id        = IF(parent_id        LIKE 'DEMO%', REPLACE(parent_id,        'DEMO', 'DE00'), parent_id),
    created_by       = IF(created_by       LIKE 'DEMO%', REPLACE(created_by,       'DEMO', 'DE00'), created_by)
WHERE id LIKE 'DEMO%' OR data_room_id LIKE 'DEMO%';

-- ============================================================
-- 22. documents
-- ============================================================
UPDATE documents
SET
    client_id  = IF(client_id  LIKE 'DEMO%', REPLACE(client_id,  'DEMO', 'DE00'), client_id),
    company_id = IF(company_id LIKE 'DEMO%', REPLACE(company_id, 'DEMO', 'DE00'), company_id),
    entity_id  = IF(entity_id  LIKE 'DEMO%', REPLACE(entity_id,  'DEMO', 'DE00'), entity_id),
    created_by = IF(created_by LIKE 'DEMO%', REPLACE(created_by, 'DEMO', 'DE00'), created_by)
WHERE client_id LIKE 'DEMO%' OR company_id LIKE 'DEMO%';

-- ============================================================
-- 23. data_room_folder_documents
-- ============================================================
UPDATE data_room_folder_documents
SET
    folder_id   = IF(folder_id   LIKE 'DEMO%', REPLACE(folder_id,   'DEMO', 'DE00'), folder_id),
    document_id = IF(document_id LIKE 'DEMO%', REPLACE(document_id, 'DEMO', 'DE00'), document_id),
    added_by    = IF(added_by    LIKE 'DEMO%', REPLACE(added_by,    'DEMO', 'DE00'), added_by)
WHERE folder_id LIKE 'DEMO%' OR document_id LIKE 'DEMO%' OR added_by LIKE 'DEMO%';

-- ============================================================
-- 24. workflows
-- ============================================================
UPDATE workflows
SET
    id         = IF(id         LIKE 'DEMO%', REPLACE(id,         'DEMO', 'DE00'), id),
    company_id = IF(company_id LIKE 'DEMO%', REPLACE(company_id, 'DEMO', 'DE00'), company_id),
    created_by = IF(created_by LIKE 'DEMO%', REPLACE(created_by, 'DEMO', 'DE00'), created_by),
    updated_by = IF(updated_by LIKE 'DEMO%', REPLACE(updated_by, 'DEMO', 'DE00'), updated_by)
WHERE id LIKE 'DEMO%';

-- ============================================================
-- 25. workflow_steps
-- ============================================================
UPDATE workflow_steps
SET
    workflow_id      = IF(workflow_id      LIKE 'DEMO%', REPLACE(workflow_id,      'DEMO', 'DE00'), workflow_id),
    assigned_user_id = IF(assigned_user_id LIKE 'DEMO%', REPLACE(assigned_user_id, 'DEMO', 'DE00'), assigned_user_id),
    completed_by     = IF(completed_by     LIKE 'DEMO%', REPLACE(completed_by,     'DEMO', 'DE00'), completed_by)
WHERE workflow_id LIKE 'DEMO%' OR assigned_user_id LIKE 'DEMO%';

-- ============================================================
-- 26. workflow_approvals
-- ============================================================
UPDATE workflow_approvals
SET
    workflow_step_id = IF(workflow_step_id LIKE 'DEMO%', REPLACE(workflow_step_id, 'DEMO', 'DE00'), workflow_step_id),
    user_id          = IF(user_id          LIKE 'DEMO%', REPLACE(user_id,          'DEMO', 'DE00'), user_id)
WHERE workflow_step_id LIKE 'DEMO%' OR user_id LIKE 'DEMO%';

SET FOREIGN_KEY_CHECKS = 1;
