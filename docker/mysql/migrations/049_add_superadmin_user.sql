-- =====================================================
-- Migration 049: Add SuperAdmin system user
-- Date: 2026-03-05
-- Description: Cria cliente e usuário SuperAdmin para
--              administração do sistema (acesso irrestrito).
-- Dependencies: 003_create_clients_table.sql,
--               005_add_client_id_to_users.sql
-- =====================================================
-- Credentials:
--   E-mail : admin@sistema.com
--   Senha  : SysAdmin@2024!
-- =====================================================

USE partnership_manager;

-- =====================================================
-- STEP 1: Cliente raiz do administrador do sistema
-- =====================================================
INSERT IGNORE INTO clients
    (id, name, trading_name, document, document_type, email, status, created_at, updated_at)
VALUES (
    '00000001-0000-4000-a000-000000000001',
    'Administração do Sistema',
    'Sistema Admin',
    '00000000000191',   -- documento fictício exclusivo para conta de sistema
    'cnpj',
    'admin@sistema.com',
    'Active',
    NOW(),
    NOW()
);

-- =====================================================
-- STEP 2: Usuário SuperAdmin
-- =====================================================
-- password_hash = BCrypt cost 11 de 'SysAdmin@2024!'
INSERT IGNORE INTO users
    (id, client_id, company_id, email, name, password_hash, status, language,
     failed_login_attempts, created_at, updated_at)
VALUES (
    '00000001-0000-4000-a000-000000000002',
    '00000001-0000-4000-a000-000000000001',
    NULL,
    'admin@sistema.com',
    'Administrador do Sistema',
    '$2b$11$pmw/gvuc1Oj0QImkIJBoK.ICPg3PDsa7azdPKNifsti7KXjY2uN1a',
    'Active',
    'Portuguese',
    0,
    NOW(),
    NOW()
);

-- =====================================================
-- STEP 3: Role SuperAdmin
-- =====================================================
INSERT IGNORE INTO user_roles
    (id, user_id, role, granted_at, is_active, created_at, updated_at)
VALUES (
    '00000001-0000-4000-a000-000000000003',
    '00000001-0000-4000-a000-000000000002',
    'SuperAdmin',
    NOW(),
    1,
    NOW(),
    NOW()
);

SELECT 'Migration 049 applied: SuperAdmin user created.' AS status;
