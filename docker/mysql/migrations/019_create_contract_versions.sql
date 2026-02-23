-- Migration: 019_create_contract_versions.sql
-- Author: GitHub Copilot
-- Date: 23/02/2026
-- Description: Create contract versioning system
--              Adds contract_versions table and current_version_number to contracts
-- NOTE: Uses utf8mb4_0900_ai_ci (same as contracts table on Azure MySQL)

-- ============================================
-- ALTER TABLE: contracts
-- ============================================
ALTER TABLE contracts
    ADD COLUMN IF NOT EXISTS current_version_number INT NOT NULL DEFAULT 0
        COMMENT 'Current (latest) version number; 0 = sem documento gerado ainda'
        AFTER document_hash;

-- ============================================
-- TABLE: contract_versions
-- ============================================
CREATE TABLE IF NOT EXISTS contract_versions (
    id CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    contract_id CHAR(36) NOT NULL COMMENT 'FK: contracts.id',
    version_number INT NOT NULL COMMENT 'Sequential version number, starts at 1',
    file_path VARCHAR(500) NOT NULL COMMENT 'Path to stored file (local or S3)',
    file_size BIGINT COMMENT 'File size in bytes',
    file_hash VARCHAR(64) COMMENT 'SHA-256 hash for integrity verification',
    file_type ENUM('pdf', 'docx') NOT NULL COMMENT 'Format of the stored file',
    source ENUM('builder', 'upload') NOT NULL COMMENT 'How this version was created',
    notes TEXT COMMENT 'Optional notes about this version',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'When this version was created',
    created_by VARCHAR(255) COMMENT 'User identifier who created this version',
    CONSTRAINT fk_contract_versions_contract FOREIGN KEY (contract_id)
        REFERENCES contracts(id) ON DELETE CASCADE,
    UNIQUE KEY uk_contract_version (contract_id, version_number),
    INDEX idx_cv_contract (contract_id),
    INDEX idx_cv_contract_num (contract_id, version_number)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='Full version history for contract documents (PDF from builder or uploaded DOCX)';
