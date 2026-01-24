-- =====================================================
-- Migration 006: Create user_companies table
-- Date: 2026-01-23
-- Description: Criar tabela para relacionamento N:N entre users e companies
-- Dependencies: 003_create_clients_table.sql, 004_add_client_id_to_companies.sql, 005_add_client_id_to_users.sql
-- =====================================================

USE partnership_manager;

-- =====================================================
-- CREATE TABLE: user_companies
-- =====================================================
-- Permite que um usuário tenha acesso a múltiplas companies
-- com diferentes níveis de permissão

CREATE TABLE IF NOT EXISTS user_companies (
    id CHAR(36) NOT NULL,
    user_id CHAR(36) NOT NULL COMMENT 'FK para users',
    company_id CHAR(36) NOT NULL COMMENT 'FK para companies',
    role VARCHAR(50) NOT NULL DEFAULT 'Viewer' COMMENT 'Admin, Manager, Editor, Viewer',
    is_default TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Company padrão do usuário',
    granted_at DATETIME(6) NOT NULL COMMENT 'Data de concessão do acesso',
    granted_by CHAR(36) NULL COMMENT 'ID do usuário que concedeu o acesso',
    revoked_at DATETIME(6) NULL COMMENT 'Data de revogação do acesso',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    PRIMARY KEY (id),
    UNIQUE INDEX idx_user_company_unique (user_id, company_id),
    INDEX idx_user_company_user (user_id),
    INDEX idx_user_company_company (company_id),
    INDEX idx_user_company_role (role),
    
    CONSTRAINT fk_uc_user 
        FOREIGN KEY (user_id) 
        REFERENCES users(id) 
        ON DELETE CASCADE,
    
    CONSTRAINT fk_uc_company 
        FOREIGN KEY (company_id) 
        REFERENCES companies(id) 
        ON DELETE CASCADE,
    
    CONSTRAINT fk_uc_granted_by 
        FOREIGN KEY (granted_by) 
        REFERENCES users(id) 
        ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Relacionamento N:N entre users e companies com permissões';

-- =====================================================
-- VALIDATION CONSTRAINTS
-- =====================================================

-- Validar role
ALTER TABLE user_companies
ADD CONSTRAINT chk_uc_role 
CHECK (role IN ('Admin', 'Manager', 'Editor', 'Viewer'));

-- =====================================================
-- STEP 2: Popular user_companies com dados existentes
-- =====================================================
-- Migrar relacionamentos existentes de users.company_id

INSERT INTO user_companies (
    id,
    user_id,
    company_id,
    role,
    is_default,
    granted_at,
    created_at
)
SELECT 
    UUID(),
    u.id,
    u.company_id,
    'Admin', -- Usuários existentes recebem role Admin
    1, -- Definir como company padrão
    u.created_at,
    CURRENT_TIMESTAMP(6)
FROM users u
WHERE u.company_id IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM user_companies uc 
      WHERE uc.user_id = u.id AND uc.company_id = u.company_id
  );

-- Migration successfully applied
