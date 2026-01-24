-- =====================================================
-- Migration 008: Create Shareholders Table
-- Date: 2026-01-23
-- Description: Tabela de sócios vinculados às empresas
-- Dependencies: clients (003), companies (004), users (005)
-- =====================================================

USE partnership_manager;

-- =====================================================
-- CREATE TABLE: shareholders
-- =====================================================
-- Representa sócios/acionistas vinculados a uma empresa
-- Multi-tenant: client_id garante isolamento por cliente

CREATE TABLE IF NOT EXISTS shareholders (
    id CHAR(36) NOT NULL,
    client_id CHAR(36) NOT NULL,
    company_id CHAR(36) NOT NULL,
    name VARCHAR(200) NOT NULL,
    document VARCHAR(20) NOT NULL,
    document_type VARCHAR(10) NOT NULL,
    email VARCHAR(255) NULL,
    phone VARCHAR(20) NULL,
    type VARCHAR(20) NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'Active',
    notes TEXT NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by CHAR(36) NULL,
    updated_by CHAR(36) NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    deleted_at DATETIME(6) NULL,

    PRIMARY KEY (id),
    UNIQUE INDEX idx_shareholder_company_document (company_id, document),
    INDEX idx_shareholder_company (company_id),
    INDEX idx_shareholder_type (type),
    INDEX idx_shareholder_status (status),
    INDEX idx_shareholder_document (document),
    CONSTRAINT fk_shareholder_client FOREIGN KEY (client_id) REFERENCES clients(id),
    CONSTRAINT fk_shareholder_company FOREIGN KEY (company_id) REFERENCES companies(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Sócios/acionistas vinculados às empresas';

-- =====================================================
-- VALIDATION CONSTRAINTS
-- =====================================================

ALTER TABLE shareholders
ADD CONSTRAINT chk_shareholder_document_type
CHECK (document_type IN ('cpf', 'cnpj'));

ALTER TABLE shareholders
ADD CONSTRAINT chk_shareholder_type
CHECK (type IN ('Founder', 'Investor', 'Employee', 'Advisor', 'ESOP', 'Other'));

ALTER TABLE shareholders
ADD CONSTRAINT chk_shareholder_status
CHECK (status IN ('Active', 'Inactive', 'Pending', 'Exited'));

-- Migration successfully applied
