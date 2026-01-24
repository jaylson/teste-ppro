-- =====================================================
-- Migration 011: Add Shareholder Personal Details
-- Date: 2026-01-24
-- Description: Adiciona campos de endereço e qualificação do sócio
-- =====================================================

USE partnership_manager;

ALTER TABLE shareholders
    ADD COLUMN address_street VARCHAR(200) NULL AFTER notes,
    ADD COLUMN address_number VARCHAR(50) NULL AFTER address_street,
    ADD COLUMN address_complement VARCHAR(100) NULL AFTER address_number,
    ADD COLUMN address_zip_code VARCHAR(10) NULL AFTER address_complement,
    ADD COLUMN address_city VARCHAR(100) NULL AFTER address_zip_code,
    ADD COLUMN address_state VARCHAR(2) NULL AFTER address_city,
    ADD COLUMN marital_status VARCHAR(20) NULL AFTER address_state,
    ADD COLUMN gender VARCHAR(20) NULL AFTER marital_status,
    ADD COLUMN birth_date DATE NULL AFTER gender;

-- Migration successfully applied
