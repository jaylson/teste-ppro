-- =====================================================
-- Partnership Manager - Database Initialization
-- Version: 1.0.0
-- =====================================================

SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;

-- =====================================================
-- CREATE DATABASES
-- =====================================================
CREATE DATABASE IF NOT EXISTS partnership_manager
    CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE DATABASE IF NOT EXISTS hangfire
    CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

GRANT ALL PRIVILEGES ON partnership_manager.* TO 'pm_user'@'%';
GRANT ALL PRIVILEGES ON hangfire.* TO 'pm_user'@'%';
FLUSH PRIVILEGES;

USE partnership_manager;

-- =====================================================
-- CORE TABLES
-- =====================================================

-- Companies Table
CREATE TABLE IF NOT EXISTS companies (
    id CHAR(36) PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    trading_name VARCHAR(200) NULL,
    cnpj VARCHAR(14) NOT NULL,
    legal_form VARCHAR(20) NOT NULL,
    foundation_date DATE NOT NULL,
    total_shares DECIMAL(15,0) NOT NULL,
    share_price DECIMAL(15,4) NOT NULL,
    currency VARCHAR(3) NOT NULL DEFAULT 'BRL',
    logo_url VARCHAR(500) NULL,
    settings JSON NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'Active',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by CHAR(36) NULL,
    updated_by CHAR(36) NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    deleted_at DATETIME(6) NULL,
    
    UNIQUE INDEX idx_company_cnpj (cnpj),
    INDEX idx_company_status (status),
    INDEX idx_company_deleted (is_deleted)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Users Table
CREATE TABLE IF NOT EXISTS users (
    id CHAR(36) PRIMARY KEY,
    company_id CHAR(36) NOT NULL,
    email VARCHAR(255) NOT NULL,
    name VARCHAR(200) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    avatar_url VARCHAR(500) NULL,
    phone VARCHAR(20) NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    language VARCHAR(20) NOT NULL DEFAULT 'Portuguese',
    timezone VARCHAR(50) NOT NULL DEFAULT 'America/Sao_Paulo',
    preferences JSON NULL,
    two_factor_enabled TINYINT(1) NOT NULL DEFAULT 0,
    two_factor_secret VARCHAR(100) NULL,
    last_login_at DATETIME(6) NULL,
    failed_login_attempts INT NOT NULL DEFAULT 0,
    lockout_end DATETIME(6) NULL,
    refresh_token VARCHAR(500) NULL,
    refresh_token_expiry DATETIME(6) NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by CHAR(36) NULL,
    updated_by CHAR(36) NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    deleted_at DATETIME(6) NULL,
    
    INDEX idx_user_company (company_id),
    UNIQUE INDEX idx_user_email_company (company_id, email),
    INDEX idx_user_status (status),
    INDEX idx_user_deleted (is_deleted),
    
    CONSTRAINT fk_user_company FOREIGN KEY (company_id) 
        REFERENCES companies(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- User Roles Table
CREATE TABLE IF NOT EXISTS user_roles (
    id CHAR(36) PRIMARY KEY,
    user_id CHAR(36) NOT NULL,
    role VARCHAR(50) NOT NULL,
    permissions JSON NULL,
    granted_by CHAR(36) NULL,
    granted_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    expires_at DATETIME(6) NULL,
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    INDEX idx_role_user (user_id),
    INDEX idx_role_name (role),
    UNIQUE INDEX idx_role_user_active (user_id, role, is_active),
    
    CONSTRAINT fk_role_user FOREIGN KEY (user_id) 
        REFERENCES users(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Audit Logs Table (append-only)
CREATE TABLE IF NOT EXISTS audit_logs (
    id CHAR(36) PRIMARY KEY,
    company_id CHAR(36) NULL,
    user_id CHAR(36) NULL,
    action VARCHAR(50) NOT NULL,
    entity_type VARCHAR(100) NOT NULL,
    entity_id CHAR(36) NOT NULL,
    old_values JSON NULL,
    new_values JSON NULL,
    ip_address VARCHAR(45) NULL,
    user_agent TEXT NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    
    INDEX idx_audit_company (company_id),
    INDEX idx_audit_user (user_id),
    INDEX idx_audit_entity (entity_type, entity_id),
    INDEX idx_audit_created (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- SEED DATA
-- =====================================================

-- Default Company
INSERT INTO companies (id, name, trading_name, cnpj, legal_form, foundation_date, total_shares, share_price, currency, status)
VALUES (
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    'Empresa Demonstração LTDA',
    'Demo Corp',
    '12345678000190',
    'LTDA',
    '2020-01-01',
    1000000,
    10.0000,
    'BRL',
    'Active'
);

-- Admin User (password: Admin@123)
INSERT INTO users (id, company_id, email, name, password_hash, status, language)
VALUES (
    'f1e2d3c4-b5a6-7890-abcd-ef1234567890',
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    'admin@demo.com',
    'Administrador',
    '$2a$11$rKN.XpH.kQOjZlqD8y4.7.BQVD3lNJD7vCr8qFMBqR5J5qEOyQlqC',
    'Active',
    'Portuguese'
);

-- Admin Role
INSERT INTO user_roles (id, user_id, role, granted_at, is_active)
VALUES (
    'b1c2d3e4-f5a6-7890-abcd-ef1234567890',
    'f1e2d3c4-b5a6-7890-abcd-ef1234567890',
    'Admin',
    NOW(),
    1
);

SELECT 'Database initialized successfully!' AS status;
