-- Migration: 014_create_contracts_module.sql
-- Author: GitHub Copilot
-- Date: 13/02/2026
-- Description: Create contracts module tables - part 1
--              Enum types, contract templates, and clauses library

USE partnership_manager;

-- ============================================
-- ENUM TYPES (MySQL 5.7+)
-- ============================================

-- Template Types
CREATE TYPE contract_template_type AS ENUM (
  'stock_option',
  'shareholders_agreement',
  'nda',
  'investment',
  'employment',
  'service_agreement',
  'partnership',
  'confidentiality',
  'other'
) COMMENT 'Types of contract templates';

-- Clause Categories/Types
CREATE TYPE clause_type AS ENUM (
  'governance',
  'rights_obligations',
  'compliance',
  'financial',
  'termination',
  'confidentiality',
  'dispute_resolution',
  'amendments',
  'general'
) COMMENT 'Categories of clauses';

-- Contract Status
CREATE TYPE contract_status AS ENUM (
  'draft',
  'pending_review',
  'approved',
  'sent_for_signature',
  'partially_signed',
  'signed',
  'executed',
  'expired',
  'cancelled'
) COMMENT 'Contract lifecycle status';

-- Signature Status
CREATE TYPE signature_status AS ENUM (
  'pending',
  'waiting_signature',
  'signed',
  'rejected',
  'expired'
) COMMENT 'Individual signer status in contract';

-- ============================================
-- TABLE: contract_templates
-- ============================================
-- Reusable contract templates with dynamic fields
CREATE TABLE contract_templates (
    id CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    client_id CHAR(36) NOT NULL COMMENT 'FK: clients.id',
    company_id CHAR(36) COMMENT 'FK: companies.id (optional - company-specific template)',
    
    -- Template identification
    name VARCHAR(255) NOT NULL COMMENT 'Template name',
    description TEXT COMMENT 'Template description',
    code VARCHAR(50) NOT NULL COMMENT 'Unique code (e.g., SA-001)',
    template_type VARCHAR(50) NOT NULL COMMENT 'Type from contract_template_type enum',
    
    -- Template content
    content LONGTEXT NOT NULL COMMENT 'Template HTML/Text with {{variables}}',
    default_status VARCHAR(50) DEFAULT 'draft' COMMENT 'Default contract status when created from this template',
    
    -- Metadata
    tags JSON COMMENT 'Tags for categorization: ["governance", "legal"]',
    version INT DEFAULT 1 COMMENT 'Template version',
    is_active BOOLEAN DEFAULT TRUE COMMENT 'Template can be used for new contracts',
    
    -- Audit
    created_at DATETIME(6) NOT NULL COMMENT 'Created timestamp',
    updated_at DATETIME(6) NOT NULL COMMENT 'Last update timestamp',
    created_by CHAR(36) COMMENT 'FK: users.id',
    updated_by CHAR(36) COMMENT 'FK: users.id',
    is_deleted BOOLEAN DEFAULT FALSE COMMENT 'Soft delete flag',
    deleted_at DATETIME(6) COMMENT 'Soft delete timestamp',
    
    -- Constraints
    CONSTRAINT fk_contract_templates_client FOREIGN KEY (client_id) 
        REFERENCES clients(id) ON DELETE CASCADE,
    CONSTRAINT fk_contract_templates_company FOREIGN KEY (company_id) 
        REFERENCES companies(id) ON DELETE SET NULL,
    CONSTRAINT fk_contract_templates_created_by FOREIGN KEY (created_by) 
        REFERENCES users(id) ON DELETE SET NULL,
    CONSTRAINT fk_contract_templates_updated_by FOREIGN KEY (updated_by) 
        REFERENCES users(id) ON DELETE SET NULL,
    
    -- Indices
    UNIQUE INDEX idx_contract_template_code (client_id, code, is_deleted),
    INDEX idx_contract_template_client (client_id, is_deleted),
    INDEX idx_contract_template_company (company_id, is_deleted),
    INDEX idx_contract_template_type (template_type, is_deleted),
    INDEX idx_contract_template_active (is_active, is_deleted)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Reusable contract templates with dynamic fields';

-- ============================================
-- TABLE: clauses
-- ============================================
-- Library of standardized clause definitions
CREATE TABLE clauses (
    id CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    client_id CHAR(36) NOT NULL COMMENT 'FK: clients.id',
    
    -- Clause identification
    name VARCHAR(255) NOT NULL COMMENT 'Clause name (e.g., "Confidentiality Obligations")',
    description TEXT COMMENT 'Detailed clause description',
    code VARCHAR(50) NOT NULL COMMENT 'Unique code (e.g., CONF-001)',
    
    -- Content and categorization
    content LONGTEXT NOT NULL COMMENT 'Clause HTML/Text with {{variables}} support',
    clause_type VARCHAR(50) NOT NULL COMMENT 'Type from clause_type enum',
    is_mandatory BOOLEAN DEFAULT FALSE COMMENT 'Must be included in contracts',
    
    -- Metadata
    tags JSON COMMENT 'Tags: ["confidential", "risk-high"]',
    display_order INT DEFAULT 999 COMMENT 'Order in clause list',
    version INT DEFAULT 1 COMMENT 'Clause version',
    is_active BOOLEAN DEFAULT TRUE COMMENT 'Can be used in new contracts',
    
    -- Audit
    created_at DATETIME(6) NOT NULL COMMENT 'Created timestamp',
    updated_at DATETIME(6) NOT NULL COMMENT 'Last update timestamp',
    created_by CHAR(36) COMMENT 'FK: users.id',
    updated_by CHAR(36) COMMENT 'FK: users.id',
    is_deleted BOOLEAN DEFAULT FALSE COMMENT 'Soft delete flag',
    deleted_at DATETIME(6) COMMENT 'Soft delete timestamp',
    
    -- Constraints
    CONSTRAINT fk_clauses_client FOREIGN KEY (client_id) 
        REFERENCES clients(id) ON DELETE CASCADE,
    CONSTRAINT fk_clauses_created_by FOREIGN KEY (created_by) 
        REFERENCES users(id) ON DELETE SET NULL,
    CONSTRAINT fk_clauses_updated_by FOREIGN KEY (updated_by) 
        REFERENCES users(id) ON DELETE SET NULL,
    
    -- Indices
    UNIQUE INDEX idx_clause_code (client_id, code, is_deleted),
    INDEX idx_clause_client (client_id, is_deleted),
    INDEX idx_clause_type (clause_type, is_deleted),
    INDEX idx_clause_mandatory (is_mandatory, is_deleted),
    INDEX idx_clause_active (is_active, is_deleted)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Library of standardized clauses for contract building';

-- ============================================
-- VERIFY CREATION
-- ============================================
SELECT 
    TABLE_NAME,
    TABLE_TYPE,
    TABLE_ROWS,
    DATA_LENGTH
FROM information_schema.TABLES 
WHERE TABLE_SCHEMA = 'partnership_manager'
AND TABLE_NAME IN ('contract_templates', 'clauses')
ORDER BY TABLE_NAME;

SELECT 'Contract tables part 1 created successfully' AS status;
