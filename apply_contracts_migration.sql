-- =====================================================
-- Partnership Manager - Apply Contracts Module Migrations
-- =====================================================
-- This script applies migrations 014 and 015 for the contracts module
-- It creates all necessary tables for contract management

SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;

-- Use the correct database
USE ppro;

-- ============================================
-- TABLE: contract_templates (if not exists)
-- ============================================
CREATE TABLE IF NOT EXISTS contract_templates (
    id CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    client_id CHAR(36) NOT NULL COMMENT 'FK: clients.id or BillingClients.id',
    company_id CHAR(36) COMMENT 'FK: companies.id (optional - company-specific template)',
    
    -- Template identification
    name VARCHAR(255) NOT NULL COMMENT 'Template name',
    description TEXT COMMENT 'Template description',
    code VARCHAR(50) NOT NULL COMMENT 'Unique code (e.g., SA-001)',
    template_type VARCHAR(50) NOT NULL COMMENT 'Type of contract template',
    
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
    
    -- Indices
    UNIQUE INDEX idx_contract_template_code (client_id, code, is_deleted),
    INDEX idx_contract_template_client (client_id, is_deleted),
    INDEX idx_contract_template_company (company_id, is_deleted),
    INDEX idx_contract_template_type (template_type, is_deleted),
    INDEX idx_contract_template_active (is_active, is_deleted)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Reusable contract templates with dynamic fields';

-- ============================================
-- TABLE: clauses (if not exists)
-- ============================================
CREATE TABLE IF NOT EXISTS clauses (
    id CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    client_id CHAR(36) NOT NULL COMMENT 'FK: clients.id or BillingClients.id',
    
    -- Clause identification
    name VARCHAR(255) NOT NULL COMMENT 'Clause name (e.g., "Confidentiality Obligations")',
    description TEXT COMMENT 'Detailed clause description',
    code VARCHAR(50) NOT NULL COMMENT 'Unique code (e.g., CONF-001)',
    
    -- Content and categorization
    content LONGTEXT NOT NULL COMMENT 'Clause HTML/Text with {{variables}} support',
    clause_type VARCHAR(50) NOT NULL COMMENT 'Type of clause',
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
    
    -- Indices
    UNIQUE INDEX idx_clause_code (client_id, code, is_deleted),
    INDEX idx_clause_client (client_id, is_deleted),
    INDEX idx_clause_type (clause_type, is_deleted),
    INDEX idx_clause_mandatory (is_mandatory, is_deleted),
    INDEX idx_clause_active (is_active, is_deleted)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Library of standardized clauses for contract building';

-- ============================================
-- TABLE: contracts
-- ============================================
CREATE TABLE IF NOT EXISTS contracts (
    id CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    client_id CHAR(36) NOT NULL COMMENT 'FK: BillingClients.id',
    company_id CHAR(36) NOT NULL COMMENT 'FK: companies.id (company this contract belongs to)',
    
    -- Basic information
    title VARCHAR(255) NOT NULL COMMENT 'Contract title',
    description TEXT COMMENT 'Contract description/purpose',
    contract_type VARCHAR(50) NOT NULL COMMENT 'Type of contract',
    template_id CHAR(36) COMMENT 'FK: contract_templates.id (source template, nullable)',
    
    -- Document management
    document_path VARCHAR(500) COMMENT 'Path to stored document (S3/Local)',
    document_size BIGINT COMMENT 'Document file size in bytes',
    document_hash VARCHAR(64) COMMENT 'SHA-256 hash for integrity verification',
    
    -- Status and metadata
    status VARCHAR(50) NOT NULL DEFAULT 'draft' COMMENT 'Current status',
    
    -- Key dates
    created_contract_date DATETIME COMMENT 'Date contract starts being effective',
    expiration_date DATETIME COMMENT 'Contract expiration date (if applicable)',
    
    -- Tracking
    external_reference VARCHAR(100) COMMENT 'Reference in external systems',
    notes TEXT COMMENT 'Internal notes about the contract',
    
    -- Audit
    created_at DATETIME(6) NOT NULL COMMENT 'Created timestamp',
    updated_at DATETIME(6) NOT NULL COMMENT 'Last update timestamp',
    created_by CHAR(36) COMMENT 'FK: users.id (creator)',
    updated_by CHAR(36) COMMENT 'FK: users.id (last editor)',
    is_deleted BOOLEAN DEFAULT FALSE COMMENT 'Soft delete flag',
    deleted_at DATETIME(6) COMMENT 'Soft delete timestamp',
    
    -- Indices
    UNIQUE INDEX idx_contract_external (client_id, external_reference, is_deleted),
    INDEX idx_contract_client (client_id, is_deleted),
    INDEX idx_contract_company (company_id, is_deleted),
    INDEX idx_contract_type (contract_type, is_deleted),
    INDEX idx_contract_status (status, is_deleted),
    INDEX idx_contract_expiration (expiration_date, is_deleted),
    INDEX idx_contracts_created_at (created_at DESC, is_deleted),
    INDEX idx_contracts_updated_at (updated_at DESC, is_deleted)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Main contracts table - actual contract documents';

-- ============================================
-- TABLE: contract_parties
-- ============================================
CREATE TABLE IF NOT EXISTS contract_parties (
    id CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    contract_id CHAR(36) NOT NULL COMMENT 'FK: contracts.id',
    
    -- Party information
    party_type VARCHAR(50) NOT NULL COMMENT 'Role: signer, recipient, witness, etc',
    party_name VARCHAR(255) NOT NULL COMMENT 'Full name of the party',
    party_email VARCHAR(255) NOT NULL COMMENT 'Email address for notifications',
    
    -- Optional associations
    user_id CHAR(36) COMMENT 'FK: users.id (if party is an internal user)',
    shareholder_id CHAR(36) COMMENT 'FK: shareholders.id (if party is a shareholder)',
    
    -- Signature tracking
    signature_status VARCHAR(50) DEFAULT 'pending' COMMENT 'Status of signature',
    signature_date DATETIME COMMENT 'When this party signed',
    signature_token VARCHAR(500) COMMENT 'Token for signature integration',
    
    -- Additional metadata
    external_id VARCHAR(100) COMMENT 'External party ID',
    sequence_order INT DEFAULT 1 COMMENT 'Order for multi-signature workflows',
    
    -- Audit
    created_at DATETIME(6) NOT NULL COMMENT 'Created timestamp',
    updated_at DATETIME(6) NOT NULL COMMENT 'Last update timestamp',
    is_deleted BOOLEAN DEFAULT FALSE COMMENT 'Soft delete flag',
    
    -- Constraints
    CONSTRAINT fk_contract_parties_contract FOREIGN KEY (contract_id) 
        REFERENCES contracts(id) ON DELETE CASCADE,
    
    -- Indices
    UNIQUE INDEX idx_contract_party_order (contract_id, sequence_order, is_deleted),
    INDEX idx_contract_party_contract (contract_id, is_deleted),
    INDEX idx_contract_party_email (party_email, is_deleted),
    INDEX idx_contract_party_signature_status (signature_status, is_deleted),
    INDEX idx_contract_parties_created_at (created_at DESC, is_deleted)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Parties involved in contracts - signers and recipients';

-- ============================================
-- TABLE: contract_clauses
-- ============================================
CREATE TABLE IF NOT EXISTS contract_clauses (
    id CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    contract_id CHAR(36) NOT NULL COMMENT 'FK: contracts.id',
    clause_id CHAR(36) NOT NULL COMMENT 'FK: clauses.id (base clause template)',
    
    -- Customization
    custom_content LONGTEXT COMMENT 'Custom clause content (overrides clause_id content if set)',
    display_order INT NOT NULL DEFAULT 999 COMMENT 'Order clauses appear in contract',
    is_mandatory BOOLEAN DEFAULT FALSE COMMENT 'Is this clause required in the contract',
    
    -- Metadata
    clause_variables JSON COMMENT 'Variables specific to this contract',
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
    
    -- Indices
    UNIQUE INDEX idx_contract_clause_order (contract_id, clause_id, is_deleted),
    INDEX idx_contract_clause_contract (contract_id, is_deleted),
    INDEX idx_contract_clause_clause (clause_id, is_deleted),
    INDEX idx_contract_clause_mandatory (is_mandatory, is_deleted)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Junction table: clauses included in specific contracts';

-- ============================================
-- VERIFY CREATION
-- ============================================
SELECT 
    TABLE_NAME,
    TABLE_ROWS,
    DATA_LENGTH
FROM information_schema.TABLES 
WHERE TABLE_SCHEMA = 'ppro'
AND TABLE_NAME IN ('contract_templates', 'clauses', 'contracts', 'contract_parties', 'contract_clauses')
ORDER BY TABLE_NAME;

SELECT 'Contracts module tables created successfully!' AS status;
