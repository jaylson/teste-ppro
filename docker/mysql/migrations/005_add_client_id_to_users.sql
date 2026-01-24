-- =====================================================
-- Migration 005: Add client_id to users and make company_id nullable
-- Date: 2026-01-23
-- Description: Adicionar client_id em users e permitir acesso a múltiplas companies
-- Dependencies: 003_create_clients_table.sql, 004_add_client_id_to_companies.sql
-- =====================================================

USE partnership_manager;

-- =====================================================
-- STEP 1: Adicionar coluna client_id em users
-- =====================================================
-- Inicialmente NULL para permitir migração

ALTER TABLE users
ADD COLUMN client_id CHAR(36) NULL COMMENT 'FK para clients' AFTER id;

-- =====================================================
-- STEP 2: Popular client_id nos users existentes
-- =====================================================
-- Associar users ao client das suas companies

UPDATE users u
INNER JOIN companies c ON u.company_id = c.id
SET u.client_id = c.client_id
WHERE u.client_id IS NULL;

-- =====================================================
-- STEP 3: Tornar client_id NOT NULL e adicionar FK
-- =====================================================

ALTER TABLE users
MODIFY COLUMN client_id CHAR(36) NOT NULL;

-- Criar índice
ALTER TABLE users
ADD INDEX idx_user_client (client_id);

-- Criar Foreign Key
ALTER TABLE users
ADD CONSTRAINT fk_user_client 
    FOREIGN KEY (client_id) 
    REFERENCES clients(id) 
    ON DELETE RESTRICT;

-- =====================================================
-- STEP 4: Tornar company_id NULLABLE
-- =====================================================
-- Usuário pode acessar múltiplas companies através de user_companies

ALTER TABLE users
MODIFY COLUMN company_id CHAR(36) NULL COMMENT 'FK para companies (deprecated - usar user_companies)';

-- Migration successfully applied
