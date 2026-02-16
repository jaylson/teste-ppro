-- Migration: 015_create_contracts_main_tables.sql
-- Author: GitHub Copilot
-- Date: 13/02/2026
-- Description: Create contracts module tables - part 2
--              Contracts, contract parties, and contract clauses

USE partnership_manager;

-- ============================================
-- TABLE: contracts
-- ============================================
-- Main contracts table with document management
CREATE TABLE contracts (
    id CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    client_id CHAR(36) NOT NULL COMMENT 'FK: clients.id',
    company_id CHAR(36) NOT NULL COMMENT 'FK: companies.id (company this contract belongs to)',
    
    -- Basic information
    title VARCHAR(255) NOT NULL COMMENT 'Contract title',
    description TEXT COMMENT 'Contract description/purpose',
    contract_type VARCHAR(50) NOT NULL COMMENT 'Type from contract_template_type enum',
    template_id CHAR(36) COMMENT 'FK: contract_templates.id (source template, nullable)',
    
    -- Document management
    document_path VARCHAR(500) COMMENT 'Path to stored document (S3/Local)',
    document_size BIGINT COMMENT 'Document file size in bytes',
    document_hash VARCHAR(64) COMMENT 'SHA-256 hash for integrity verification',
    
    -- Status and metadata
    status VARCHAR(50) NOT NULL DEFAULT 'draft' COMMENT 'Current status from contract_status enum',
    
    -- Key dates
    created_contract_date DATETIME COMMENT 'Date contract starts being effective',
    expiration_date DATETIME COMMENT 'Contract expiration date (if applicable)',
    
    -- Tracking
    external_reference VARCHAR(100) COMMENT 'Reference in external systems (e.g., external ID)',
    notes TEXT COMMENT 'Internal notes about the contract',
    
    -- Audit
    created_at DATETIME(6) NOT NULL COMMENT 'Created timestamp',
    updated_at DATETIME(6) NOT NULL COMMENT 'Last update timestamp',
    created_by CHAR(36) COMMENT 'FK: users.id (creator)',
    updated_by CHAR(36) COMMENT 'FK: users.id (last editor)',
    is_deleted BOOLEAN DEFAULT FALSE COMMENT 'Soft delete flag',
    deleted_at DATETIME(6) COMMENT 'Soft delete timestamp',
    
    -- Constraints
    CONSTRAINT fk_contracts_client FOREIGN KEY (client_id) 
        REFERENCES clients(id) ON DELETE CASCADE,
    CONSTRAINT fk_contracts_company FOREIGN KEY (company_id) 
        REFERENCES companies(id) ON DELETE CASCADE,
    CONSTRAINT fk_contracts_template FOREIGN KEY (template_id) 
        REFERENCES contract_templates(id) ON DELETE SET NULL,
    CONSTRAINT fk_contracts_created_by FOREIGN KEY (created_by) 
        REFERENCES users(id) ON DELETE SET NULL,
    CONSTRAINT fk_contracts_updated_by FOREIGN KEY (updated_by) 
        REFERENCES users(id) ON DELETE SET NULL,
    
    -- Indices
    UNIQUE INDEX idx_contract_external (client_id, external_reference, is_deleted),
    INDEX idx_contract_client (client_id, is_deleted),
    INDEX idx_contract_company (company_id, is_deleted),
    INDEX idx_contract_type (contract_type, is_deleted),
    INDEX idx_contract_status (status, is_deleted),
    INDEX idx_contract_expiration (expiration_date, is_deleted)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Main contracts table - actual contract documents';

-- ============================================
-- TABLE: contract_parties
-- ============================================
-- Parties involved in a contract (signers, recipients, etc)
CREATE TABLE contract_parties (
    id CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    contract_id CHAR(36) NOT NULL COMMENT 'FK: contracts.id',
    
    -- Party information
    party_type VARCHAR(50) NOT NULL COMMENT 'Role: "signer", "recipient", "witness", etc',
    party_name VARCHAR(255) NOT NULL COMMENT 'Full name of the party',
    party_email VARCHAR(255) NOT NULL COMMENT 'Email address for notifications',
    
    -- Optional associations
    user_id CHAR(36) COMMENT 'FK: users.id (if party is an internal user)',
    shareholder_id CHAR(36) COMMENT 'FK: shareholders.id (if party is a shareholder)',
    
    -- Signature tracking
    signature_status VARCHAR(50) DEFAULT 'pending' COMMENT 'From signature_status enum',
    signature_date DATETIME COMMENT 'When this party signed',
    signature_token VARCHAR(500) COMMENT 'Token for clicksign integration',
    
    -- Additional metadata
    external_id VARCHAR(100) COMMENT 'External party ID (e.g., from ClickSign)',
    sequence_order INT DEFAULT 1 COMMENT 'Order for multi-signature workflows',
    
    -- Audit
    created_at DATETIME(6) NOT NULL COMMENT 'Created timestamp',
    updated_at DATETIME(6) NOT NULL COMMENT 'Last update timestamp',
    is_deleted BOOLEAN DEFAULT FALSE COMMENT 'Soft delete flag',
    
    -- Constraints
    CONSTRAINT fk_contract_parties_contract FOREIGN KEY (contract_id) 
        REFERENCES contracts(id) ON DELETE CASCADE,
    CONSTRAINT fk_contract_parties_user FOREIGN KEY (user_id) 
        REFERENCES users(id) ON DELETE SET NULL,
    CONSTRAINT fk_contract_parties_shareholder FOREIGN KEY (shareholder_id) 
        REFERENCES shareholders(id) ON DELETE SET NULL,
    
    -- Indices
    UNIQUE INDEX idx_contract_party_order (contract_id, sequence_order, is_deleted),
    INDEX idx_contract_party_contract (contract_id, is_deleted),
    INDEX idx_contract_party_user (user_id, is_deleted),
    INDEX idx_contract_party_email (party_email, is_deleted),
    INDEX idx_contract_party_signature_status (signature_status, is_deleted)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Parties involved in contracts - signers and recipients';

-- ============================================
-- TABLE: contract_clauses
-- ============================================
-- Clauses included in a specific contract (join table with customization)
CREATE TABLE contract_clauses (
    id CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    contract_id CHAR(36) NOT NULL COMMENT 'FK: contracts.id',
    clause_id CHAR(36) NOT NULL COMMENT 'FK: clauses.id (base clause template)',
    
    -- Customization
    custom_content LONGTEXT COMMENT 'Custom clause content (overrides clause_id content if set)',
    display_order INT NOT NULL DEFAULT 999 COMMENT 'Order clauses appear in contract',
    is_mandatory BOOLEAN DEFAULT FALSE COMMENT 'Is this clause required in the contract',
    
    -- Metadata
    clause_variables JSON COMMENT 'Variables specific to this contract: {"interest_rate": "12%"}',
    notes TEXT COMMENT 'Notes about this clause in this contract',
    
    -- Audit
    created_at DATETIME(6) NOT NULL COMMENT 'Created timestamp',
    updated_at DATETIME(6) NOT NULL COMMENT 'Last update timestamp',
    created_by CHAR(36) COMMENT 'FK: users.id',
    updated_by CHAR(36) COMMENT 'FK: users.id',
    is_deleted BOOLEAN DEFAULT FALSE COMMENT 'Soft delete flag',
    
    -- Constraints
    CONSTRAINT fk_contract_clauses_contract FOREIGN KEY (contract_id) 
        REFERENCES contracts(id) ON DELETE CASCADE,
    CONSTRAINT fk_contract_clauses_clause FOREIGN KEY (clause_id) 
        REFERENCES clauses(id) ON DELETE CASCADE,
    CONSTRAINT fk_contract_clauses_created_by FOREIGN KEY (created_by) 
        REFERENCES users(id) ON DELETE SET NULL,
    CONSTRAINT fk_contract_clauses_updated_by FOREIGN KEY (updated_by) 
        REFERENCES users(id) ON DELETE SET NULL,
    
    -- Indices
    UNIQUE INDEX idx_contract_clause_order (contract_id, clause_id, is_deleted),
    INDEX idx_contract_clause_contract (contract_id, is_deleted),
    INDEX idx_contract_clause_clause (clause_id, is_deleted),
    INDEX idx_contract_clause_mandatory (is_mandatory, is_deleted)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Junction table: clauses included in specific contracts';

-- ============================================
-- INDEXES FOR PERFORMANCE
-- ============================================

-- Contract timeline queries
CREATE INDEX idx_contracts_created_at ON contracts(created_at DESC, is_deleted);
CREATE INDEX idx_contracts_updated_at ON contracts(updated_at DESC, is_deleted);

-- Contract party queries
CREATE INDEX idx_contract_parties_created_at ON contract_parties(created_at DESC, is_deleted);

-- ============================================
-- VERIFY CREATION
-- ============================================
SELECT 
    TABLE_NAME,
    TABLE_ROWS,
    DATA_LENGTH
FROM information_schema.TABLES 
WHERE TABLE_SCHEMA = 'partnership_manager'
AND TABLE_NAME IN ('contracts', 'contract_parties', 'contract_clauses')
ORDER BY TABLE_NAME;

SELECT 'Contract tables part 2 created successfully' AS status;
