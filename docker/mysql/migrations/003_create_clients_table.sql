-- =====================================================
-- Migration 003: Create Clients Table
-- Date: 2026-01-23
-- Description: Criar entidade raiz para multi-tenancy
-- Dependencies: None
-- =====================================================

USE partnership_manager;

-- =====================================================
-- CREATE TABLE: clients
-- =====================================================
-- Representa os clientes do SaaS (quem contrata a plataforma)
-- Um Client pode gerenciar múltiplas Companies

CREATE TABLE IF NOT EXISTS clients (
    id CHAR(36) NOT NULL,
    name VARCHAR(200) NOT NULL COMMENT 'Razão social do cliente',
    trading_name VARCHAR(200) NULL COMMENT 'Nome fantasia',
    document VARCHAR(20) NOT NULL COMMENT 'CNPJ ou CPF',
    document_type VARCHAR(10) NOT NULL COMMENT 'cnpj ou cpf',
    email VARCHAR(255) NOT NULL COMMENT 'E-mail principal',
    phone VARCHAR(20) NULL COMMENT 'Telefone de contato',
    logo_url VARCHAR(500) NULL COMMENT 'URL do logotipo',
    settings JSON NULL COMMENT 'Configurações personalizadas do cliente',
    status VARCHAR(20) NOT NULL DEFAULT 'Active' COMMENT 'Active, Inactive, Suspended',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by CHAR(36) NULL COMMENT 'ID do usuário que criou',
    updated_by CHAR(36) NULL COMMENT 'ID do último usuário que atualizou',
    is_deleted TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Soft delete flag',
    deleted_at DATETIME(6) NULL COMMENT 'Data de exclusão (soft delete)',
    
    PRIMARY KEY (id),
    UNIQUE INDEX idx_client_document (document),
    INDEX idx_client_status (status),
    INDEX idx_client_deleted (is_deleted),
    INDEX idx_client_email (email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Clientes do SaaS (entidade raiz do multi-tenancy)';

-- =====================================================
-- VALIDATION CONSTRAINTS
-- =====================================================

-- Validar document_type
ALTER TABLE clients
ADD CONSTRAINT chk_client_document_type 
CHECK (document_type IN ('cnpj', 'cpf'));

-- Validar status
ALTER TABLE clients
ADD CONSTRAINT chk_client_status 
CHECK (status IN ('Active', 'Inactive', 'Suspended'));

-- Migration successfully applied
