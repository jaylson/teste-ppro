-- =====================================================
-- Migration 004: Add client_id to companies
-- Date: 2026-01-23
-- Description: Adicionar referência para clients na tabela companies
-- Dependencies: 003_create_clients_table.sql
-- =====================================================

USE partnership_manager;

-- =====================================================
-- STEP 1: Criar Client padrão para migração de dados
-- =====================================================
-- Este client será associado às companies existentes

INSERT INTO clients (
    id,
    name,
    trading_name,
    document,
    document_type,
    email,
    phone,
    status,
    created_at,
    updated_at,
    is_deleted
) VALUES (
    '00000000-0000-0000-0000-000000000001',
    'Cliente Demo',
    'Demo',
    '00000000000000',
    'cnpj',
    'demo@partnershipmanager.com',
    NULL,
    'Active',
    CURRENT_TIMESTAMP(6),
    CURRENT_TIMESTAMP(6),
    0
) ON DUPLICATE KEY UPDATE id = id;

-- =====================================================
-- STEP 2: Adicionar coluna client_id em companies
-- =====================================================
-- Inicialmente NULL para permitir migração

ALTER TABLE companies
ADD COLUMN client_id CHAR(36) NULL COMMENT 'FK para clients' AFTER id;

-- =====================================================
-- STEP 3: Popular client_id nas companies existentes
-- =====================================================
-- Associar todas as companies ao client padrão

UPDATE companies
SET client_id = '00000000-0000-0000-0000-000000000001'
WHERE client_id IS NULL;

-- =====================================================
-- STEP 4: Tornar client_id NOT NULL e adicionar FK
-- =====================================================

ALTER TABLE companies
MODIFY COLUMN client_id CHAR(36) NOT NULL;

-- Criar índice
ALTER TABLE companies
ADD INDEX idx_company_client (client_id);

-- Criar Foreign Key
ALTER TABLE companies
ADD CONSTRAINT fk_company_client 
    FOREIGN KEY (client_id) 
    REFERENCES clients(id) 
    ON DELETE RESTRICT;

-- Migration successfully applied
