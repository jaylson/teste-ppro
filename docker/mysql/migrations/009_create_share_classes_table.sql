-- Migration: 009_create_share_classes_table.sql
-- Description: Create share_classes table for Cap Table module
-- Date: 2026-01-24

-- =====================================================
-- Table: share_classes
-- Description: Classes of shares/quotas with specific rights
-- =====================================================

CREATE TABLE IF NOT EXISTS share_classes (
    id CHAR(36) NOT NULL,
    client_id CHAR(36) NOT NULL,
    company_id CHAR(36) NOT NULL,
    name VARCHAR(100) NOT NULL COMMENT 'Class name (e.g., Common, Preferred A)',
    code VARCHAR(20) NOT NULL COMMENT 'Code (e.g., ON, PNA, PNB)',
    description TEXT NULL COMMENT 'Detailed description',
    
    -- Rights configuration
    has_voting_rights TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Has voting rights',
    votes_per_share DECIMAL(10,4) NOT NULL DEFAULT 1.0000 COMMENT 'Votes per share',
    liquidation_preference DECIMAL(5,2) NOT NULL DEFAULT 1.00 COMMENT 'Liquidation preference multiplier (1.0 = 1x)',
    participating TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Participates in remaining assets after preference',
    dividend_preference DECIMAL(5,2) NULL COMMENT 'Dividend preference percentage',
    
    -- Conversion options
    is_convertible TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Can be converted to another class',
    converts_to_class_id CHAR(36) NULL COMMENT 'FK to target share_class for conversion',
    conversion_ratio DECIMAL(10,4) NULL COMMENT 'Conversion ratio (e.g., 1.5 = 1.5 target shares per 1 source share)',
    
    -- Additional rights (JSON for flexibility)
    rights JSON NULL COMMENT 'Additional rights configuration',
    
    -- Anti-dilution
    anti_dilution_type VARCHAR(30) NULL COMMENT 'None, FullRatchet, WeightedAverage',
    
    -- Status and metadata
    status VARCHAR(20) NOT NULL DEFAULT 'Active' COMMENT 'Active, Inactive',
    display_order INT NOT NULL DEFAULT 0 COMMENT 'Order for display purposes',
    
    -- Audit fields
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by CHAR(36) NULL,
    updated_by CHAR(36) NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    deleted_at DATETIME(6) NULL,
    
    PRIMARY KEY (id),
    
    -- Unique constraint: code must be unique per company
    CONSTRAINT uk_share_class_company_code UNIQUE (company_id, code),
    
    -- Foreign keys
    CONSTRAINT fk_share_class_client FOREIGN KEY (client_id) 
        REFERENCES clients(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_share_class_company FOREIGN KEY (company_id) 
        REFERENCES companies(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_share_class_converts_to FOREIGN KEY (converts_to_class_id) 
        REFERENCES share_classes(id) ON DELETE SET NULL ON UPDATE CASCADE,
    CONSTRAINT fk_share_class_created_by FOREIGN KEY (created_by) 
        REFERENCES users(id) ON DELETE SET NULL ON UPDATE CASCADE,
    CONSTRAINT fk_share_class_updated_by FOREIGN KEY (updated_by) 
        REFERENCES users(id) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Indexes for performance
-- =====================================================

CREATE INDEX idx_share_class_client ON share_classes(client_id);
CREATE INDEX idx_share_class_company ON share_classes(company_id);
CREATE INDEX idx_share_class_status ON share_classes(status);
CREATE INDEX idx_share_class_deleted ON share_classes(is_deleted);

-- =====================================================
-- Check constraints (simulated via trigger for MySQL 5.7 compatibility)
-- =====================================================

DELIMITER //

CREATE TRIGGER trg_share_class_validate_insert
BEFORE INSERT ON share_classes
FOR EACH ROW
BEGIN
    -- Validate status
    IF NEW.status NOT IN ('Active', 'Inactive') THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Invalid status. Must be Active or Inactive.';
    END IF;
    
    -- Validate anti-dilution type
    IF NEW.anti_dilution_type IS NOT NULL AND 
       NEW.anti_dilution_type NOT IN ('None', 'FullRatchet', 'WeightedAverage') THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Invalid anti_dilution_type. Must be None, FullRatchet, or WeightedAverage.';
    END IF;
    
    -- Validate conversion configuration
    IF NEW.is_convertible = 1 AND NEW.conversion_ratio IS NULL THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Convertible shares must have a conversion_ratio.';
    END IF;
    
    -- Validate liquidation preference
    IF NEW.liquidation_preference < 0 THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'liquidation_preference cannot be negative.';
    END IF;
END //

CREATE TRIGGER trg_share_class_validate_update
BEFORE UPDATE ON share_classes
FOR EACH ROW
BEGIN
    -- Validate status
    IF NEW.status NOT IN ('Active', 'Inactive') THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Invalid status. Must be Active or Inactive.';
    END IF;
    
    -- Validate anti-dilution type
    IF NEW.anti_dilution_type IS NOT NULL AND 
       NEW.anti_dilution_type NOT IN ('None', 'FullRatchet', 'WeightedAverage') THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Invalid anti_dilution_type. Must be None, FullRatchet, or WeightedAverage.';
    END IF;
    
    -- Validate conversion configuration
    IF NEW.is_convertible = 1 AND NEW.conversion_ratio IS NULL THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Convertible shares must have a conversion_ratio.';
    END IF;
    
    -- Validate liquidation preference
    IF NEW.liquidation_preference < 0 THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'liquidation_preference cannot be negative.';
    END IF;
END //

DELIMITER ;

-- =====================================================
-- Seed: Default share classes for demo company
-- =====================================================

INSERT INTO share_classes (
    id,
    client_id,
    company_id,
    name,
    code,
    description,
    has_voting_rights,
    votes_per_share,
    liquidation_preference,
    participating,
    is_convertible,
    status,
    display_order,
    created_at,
    updated_at
) VALUES 
-- Common/Ordinary shares
(
    '11111111-1111-1111-1111-111111111111',
    '00000000-0000-0000-0000-000000000001',
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    'Ações Ordinárias',
    'ON',
    'Ações ordinárias com direito a voto',
    1,
    1.0000,
    1.00,
    1,
    0,
    'Active',
    1,
    NOW(),
    NOW()
),
-- Preferred shares Series A (insert as non-convertible first, then update)
(
    '22222222-2222-2222-2222-222222222222',
    '00000000-0000-0000-0000-000000000001',
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    'Ações Preferenciais Série A',
    'PNA',
    'Ações preferenciais da Série A com preferência de liquidação de 1x',
    0,
    0.0000,
    1.00,
    0,
    0,
    'Active',
    2,
    NOW(),
    NOW()
);

-- Update to make PNA convertible to ON
UPDATE share_classes 
SET is_convertible = 1,
    converts_to_class_id = '11111111-1111-1111-1111-111111111111',
    conversion_ratio = 1.0000
WHERE id = '22222222-2222-2222-2222-222222222222';
