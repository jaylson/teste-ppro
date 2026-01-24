-- Migration 013: Create shares and share_transactions tables
-- Date: 2026-01-24
-- Description: Core tables for Cap Table management - shares holdings and immutable transaction ledger

-- ============================================
-- TABLE: shares
-- Purpose: Represents shareholdings owned by shareholders
-- ============================================
CREATE TABLE IF NOT EXISTS shares (
    id CHAR(36) NOT NULL,
    client_id CHAR(36) NOT NULL,
    company_id CHAR(36) NOT NULL,
    shareholder_id CHAR(36) NOT NULL,
    share_class_id CHAR(36) NOT NULL,
    certificate_number VARCHAR(50) NULL COMMENT 'Optional certificate number',
    quantity DECIMAL(18,4) NOT NULL COMMENT 'Number of shares held',
    acquisition_price DECIMAL(18,4) NOT NULL COMMENT 'Price paid per share',
    total_cost DECIMAL(18,4) GENERATED ALWAYS AS (quantity * acquisition_price) STORED,
    acquisition_date DATE NOT NULL,
    origin VARCHAR(20) NOT NULL COMMENT 'How shares were acquired: Issue, Transfer, Conversion, Grant',
    origin_transaction_id CHAR(36) NULL COMMENT 'FK to share_transactions for traceability',
    status VARCHAR(20) NOT NULL DEFAULT 'Active' COMMENT 'Active, Cancelled, Converted, Transferred',
    notes TEXT NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by CHAR(36) NULL,
    updated_by CHAR(36) NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    deleted_at DATETIME(6) NULL,
    
    PRIMARY KEY (id),
    
    -- Foreign Keys
    CONSTRAINT fk_share_client FOREIGN KEY (client_id) 
        REFERENCES clients(id) ON DELETE RESTRICT,
    CONSTRAINT fk_share_company FOREIGN KEY (company_id) 
        REFERENCES companies(id) ON DELETE RESTRICT,
    CONSTRAINT fk_share_shareholder FOREIGN KEY (shareholder_id) 
        REFERENCES shareholders(id) ON DELETE RESTRICT,
    CONSTRAINT fk_share_class FOREIGN KEY (share_class_id) 
        REFERENCES share_classes(id) ON DELETE RESTRICT,
    
    -- Indexes for common queries
    INDEX idx_share_client (client_id),
    INDEX idx_share_company (company_id),
    INDEX idx_share_shareholder (shareholder_id),
    INDEX idx_share_class (share_class_id),
    INDEX idx_share_status (status),
    INDEX idx_share_origin (origin),
    INDEX idx_share_acquisition_date (acquisition_date),
    INDEX idx_share_deleted (is_deleted),
    
    -- Check constraints
    CONSTRAINT chk_share_quantity CHECK (quantity > 0),
    CONSTRAINT chk_share_price CHECK (acquisition_price >= 0),
    CONSTRAINT chk_share_origin CHECK (origin IN ('Issue', 'Transfer', 'Conversion', 'Grant')),
    CONSTRAINT chk_share_status CHECK (status IN ('Active', 'Cancelled', 'Converted', 'Transferred'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- TABLE: share_transactions
-- Purpose: Immutable ledger of all share transactions (append-only)
-- ============================================
CREATE TABLE IF NOT EXISTS share_transactions (
    id CHAR(36) NOT NULL,
    client_id CHAR(36) NOT NULL,
    company_id CHAR(36) NOT NULL,
    transaction_type VARCHAR(20) NOT NULL COMMENT 'Issue, Transfer, Cancel, Convert, Split, Reverse_Split',
    transaction_number VARCHAR(50) NULL COMMENT 'Sequential number per company',
    reference_date DATE NOT NULL COMMENT 'Legal date of the transaction',
    
    -- Share and Class info
    share_id CHAR(36) NULL COMMENT 'Related share record (may be NULL for batch operations)',
    share_class_id CHAR(36) NOT NULL,
    
    -- Quantity and Value
    quantity DECIMAL(18,4) NOT NULL,
    price_per_share DECIMAL(18,4) NOT NULL,
    total_value DECIMAL(18,4) GENERATED ALWAYS AS (quantity * price_per_share) STORED,
    
    -- Parties involved
    from_shareholder_id CHAR(36) NULL COMMENT 'Source shareholder (for transfers, cancellations)',
    to_shareholder_id CHAR(36) NULL COMMENT 'Destination shareholder (for issues, transfers)',
    
    -- Additional info
    reason VARCHAR(200) NULL COMMENT 'Reason/description of the transaction',
    document_reference VARCHAR(200) NULL COMMENT 'Contract or document number',
    notes TEXT NULL,
    
    -- Approval
    approved_by CHAR(36) NULL,
    approved_at DATETIME(6) NULL,
    
    -- Audit (immutable)
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    created_by CHAR(36) NULL,
    
    PRIMARY KEY (id),
    
    -- Foreign Keys
    CONSTRAINT fk_transaction_client FOREIGN KEY (client_id) 
        REFERENCES clients(id) ON DELETE RESTRICT,
    CONSTRAINT fk_transaction_company FOREIGN KEY (company_id) 
        REFERENCES companies(id) ON DELETE RESTRICT,
    CONSTRAINT fk_transaction_share_class FOREIGN KEY (share_class_id) 
        REFERENCES share_classes(id) ON DELETE RESTRICT,
    CONSTRAINT fk_transaction_from_shareholder FOREIGN KEY (from_shareholder_id) 
        REFERENCES shareholders(id) ON DELETE RESTRICT,
    CONSTRAINT fk_transaction_to_shareholder FOREIGN KEY (to_shareholder_id) 
        REFERENCES shareholders(id) ON DELETE RESTRICT,
    
    -- Indexes
    INDEX idx_transaction_client (client_id),
    INDEX idx_transaction_company (company_id),
    INDEX idx_transaction_type (transaction_type),
    INDEX idx_transaction_date (reference_date),
    INDEX idx_transaction_share (share_id),
    INDEX idx_transaction_class (share_class_id),
    INDEX idx_transaction_from (from_shareholder_id),
    INDEX idx_transaction_to (to_shareholder_id),
    INDEX idx_transaction_created (created_at),
    
    -- Check constraints
    CONSTRAINT chk_transaction_type CHECK (
        transaction_type IN ('Issue', 'Transfer', 'Cancel', 'Convert', 'Split', 'Reverse_Split')
    ),
    CONSTRAINT chk_transaction_quantity CHECK (quantity > 0),
    CONSTRAINT chk_transaction_price CHECK (price_per_share >= 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- TRIGGER: Prevent UPDATE on share_transactions (immutability)
-- ============================================
DELIMITER //

CREATE TRIGGER trg_share_transactions_no_update
BEFORE UPDATE ON share_transactions
FOR EACH ROW
BEGIN
    SIGNAL SQLSTATE '45000' 
    SET MESSAGE_TEXT = 'Cannot update share_transactions: ledger is immutable';
END//

-- ============================================
-- TRIGGER: Prevent DELETE on share_transactions (immutability)
-- ============================================
CREATE TRIGGER trg_share_transactions_no_delete
BEFORE DELETE ON share_transactions
FOR EACH ROW
BEGIN
    SIGNAL SQLSTATE '45000' 
    SET MESSAGE_TEXT = 'Cannot delete share_transactions: ledger is immutable';
END//

DELIMITER ;

-- ============================================
-- Add FK from shares to share_transactions (deferred)
-- ============================================
ALTER TABLE shares
    ADD CONSTRAINT fk_share_origin_transaction 
    FOREIGN KEY (origin_transaction_id) REFERENCES share_transactions(id) ON DELETE SET NULL;

-- ============================================
-- SEED: Sample data for demo company
-- ============================================
SET @client_id = '00000000-0000-0000-0000-000000000001';
SET @company_id = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890';
SET @admin_id = 'a1b2c3d4-e5f6-7890-abcd-ef1234567891';

-- Get first shareholder and share class for demo
SET @shareholder_id = (SELECT id FROM shareholders WHERE company_id = @company_id AND is_deleted = 0 LIMIT 1);
SET @share_class_id = (SELECT id FROM share_classes WHERE company_id = @company_id AND is_deleted = 0 LIMIT 1);

-- Only insert if we have the required records
INSERT INTO share_transactions (
    id, client_id, company_id, transaction_type, transaction_number, reference_date,
    share_class_id, quantity, price_per_share, to_shareholder_id,
    reason, approved_by, approved_at, created_at, created_by
)
SELECT 
    UUID(), @client_id, @company_id, 'Issue', 'TXN-2024-001', '2024-01-01',
    @share_class_id, 100000, 1.00, @shareholder_id,
    'Initial share issuance for founder', @admin_id, NOW(), NOW(), @admin_id
FROM dual
WHERE @shareholder_id IS NOT NULL AND @share_class_id IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM share_transactions WHERE company_id = @company_id);

-- Create corresponding share record
INSERT INTO shares (
    id, client_id, company_id, shareholder_id, share_class_id,
    certificate_number, quantity, acquisition_price, acquisition_date, origin,
    status, created_at, created_by
)
SELECT 
    UUID(), @client_id, @company_id, @shareholder_id, @share_class_id,
    'CERT-001', 100000, 1.00, '2024-01-01', 'Issue',
    'Active', NOW(), @admin_id
FROM dual
WHERE @shareholder_id IS NOT NULL AND @share_class_id IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM shares WHERE company_id = @company_id);

-- Update share with origin_transaction_id
UPDATE shares s
INNER JOIN share_transactions t ON t.company_id = s.company_id 
    AND t.to_shareholder_id = s.shareholder_id 
    AND t.share_class_id = s.share_class_id
SET s.origin_transaction_id = t.id
WHERE s.company_id = @company_id AND s.origin_transaction_id IS NULL;
