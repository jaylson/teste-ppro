-- =====================================================
-- Migration 007: Link BillingClients to Core + Seed Data
-- Date: 2026-01-23
-- Description: Vincular BillingClients ao Core e criar dados de demonstração
-- Dependencies: 003 a 006
-- =====================================================

USE partnership_manager;

-- =====================================================
-- STEP 1: Adicionar core_client_id em BillingClients
-- =====================================================

ALTER TABLE BillingClients
ADD COLUMN core_client_id CHAR(36) NULL COMMENT 'FK para clients (Core)' AFTER Id;

-- Criar índice
ALTER TABLE BillingClients
ADD INDEX idx_billing_core_client (core_client_id);

-- Criar Foreign Key
ALTER TABLE BillingClients
ADD CONSTRAINT fk_billing_core_client 
    FOREIGN KEY (core_client_id) 
    REFERENCES clients(id) 
    ON DELETE SET NULL;

-- =====================================================
-- STEP 2: Vincular BillingClients existentes ao Core
-- =====================================================
-- Associar billing clients ao client padrão

UPDATE BillingClients bc
SET bc.core_client_id = '00000000-0000-0000-0000-000000000001'
WHERE bc.core_client_id IS NULL
  AND bc.DeletedAt IS NULL;

-- =====================================================
-- STEP 3: Verificar e completar dados do Client Demo
-- =====================================================
-- Garantir que o client demo está completo

UPDATE clients
SET 
    name = 'Partnership Manager Demo',
    trading_name = 'PM Demo',
    email = 'demo@partnershipmanager.com',
    phone = '+55 11 99999-9999',
    settings = JSON_OBJECT(
        'language', 'pt-BR',
        'timezone', 'America/Sao_Paulo',
        'features', JSON_ARRAY('cap_table', 'billing', 'shareholders')
    )
WHERE id = '00000000-0000-0000-0000-000000000001';

-- Migration successfully applied
