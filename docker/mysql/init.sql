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

-- =====================================================
-- BILLING MODULE TABLES
-- =====================================================

-- Billing Clients Table
CREATE TABLE IF NOT EXISTS BillingClients (
    Id CHAR(36) PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    Email VARCHAR(255) NOT NULL,
    Document VARCHAR(20) NOT NULL,
    Type TINYINT NOT NULL COMMENT '1=Individual, 2=Company',
    Status TINYINT NOT NULL DEFAULT 1 COMMENT '1=Active, 2=Suspended, 3=Cancelled',
    Phone VARCHAR(20) NULL,
    Address VARCHAR(500) NULL,
    City VARCHAR(100) NULL,
    State VARCHAR(2) NULL,
    ZipCode VARCHAR(10) NULL,
    Country VARCHAR(100) NULL DEFAULT 'Brasil',
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CreatedBy VARCHAR(100) NULL,
    UpdatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    UpdatedBy VARCHAR(100) NULL,
    DeletedAt DATETIME(6) NULL,
    
    UNIQUE INDEX idx_billing_client_email (Email, DeletedAt),
    UNIQUE INDEX idx_billing_client_document (Document, DeletedAt),
    INDEX idx_billing_client_status (Status),
    INDEX idx_billing_client_type (Type)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Billing Plans Table
CREATE TABLE IF NOT EXISTS BillingPlans (
    Id CHAR(36) PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Description TEXT NULL,
    Price DECIMAL(10,2) NOT NULL,
    BillingCycle TINYINT NOT NULL COMMENT '1=Monthly, 2=Yearly',
    Features JSON NULL,
    MaxCompanies INT NOT NULL DEFAULT 1,
    MaxUsers INT NOT NULL DEFAULT 5,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CreatedBy VARCHAR(100) NULL,
    UpdatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    UpdatedBy VARCHAR(100) NULL,
    DeletedAt DATETIME(6) NULL,
    
    INDEX idx_billing_plan_active (IsActive),
    INDEX idx_billing_plan_cycle (BillingCycle)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Billing Subscriptions Table
CREATE TABLE IF NOT EXISTS BillingSubscriptions (
    Id CHAR(36) PRIMARY KEY,
    ClientId CHAR(36) NOT NULL,
    PlanId CHAR(36) NOT NULL,
    Status TINYINT NOT NULL DEFAULT 1 COMMENT '1=Active, 2=Suspended, 3=Cancelled, 4=Pending',
    StartDate DATE NOT NULL,
    EndDate DATE NULL,
    AutoRenew TINYINT(1) NOT NULL DEFAULT 1,
    CompaniesCount INT NOT NULL DEFAULT 0,
    UsersCount INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CreatedBy VARCHAR(100) NULL,
    UpdatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    UpdatedBy VARCHAR(100) NULL,
    DeletedAt DATETIME(6) NULL,
    
    FOREIGN KEY (ClientId) REFERENCES BillingClients(Id),
    FOREIGN KEY (PlanId) REFERENCES BillingPlans(Id),
    INDEX idx_billing_subscription_client (ClientId),
    INDEX idx_billing_subscription_plan (PlanId),
    INDEX idx_billing_subscription_status (Status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Billing Invoices Table
CREATE TABLE IF NOT EXISTS BillingInvoices (
    Id CHAR(36) PRIMARY KEY,
    ClientId CHAR(36) NOT NULL,
    SubscriptionId CHAR(36) NULL,
    InvoiceNumber VARCHAR(50) NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    IssueDate DATE NOT NULL,
    DueDate DATE NOT NULL,
    Status TINYINT NOT NULL DEFAULT 2 COMMENT '1=Paid, 2=Pending, 3=Overdue, 4=Cancelled',
    PaymentDate DATETIME(6) NULL,
    Description TEXT NULL,
    Notes TEXT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CreatedBy VARCHAR(100) NULL,
    UpdatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    UpdatedBy VARCHAR(100) NULL,
    DeletedAt DATETIME(6) NULL,
    
    FOREIGN KEY (ClientId) REFERENCES BillingClients(Id),
    FOREIGN KEY (SubscriptionId) REFERENCES BillingSubscriptions(Id),
    UNIQUE INDEX idx_billing_invoice_number (InvoiceNumber),
    INDEX idx_subscription_issue_month (SubscriptionId, IssueDate),
    INDEX idx_subscription_issue_status (SubscriptionId, IssueDate, Status),
    INDEX idx_billing_invoice_client (ClientId),
    INDEX idx_billing_invoice_status (Status),
    INDEX idx_billing_invoice_duedate (DueDate)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Billing Payments Table
CREATE TABLE IF NOT EXISTS BillingPayments (
    Id CHAR(36) PRIMARY KEY,
    InvoiceId CHAR(36) NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    PaymentDate DATETIME(6) NOT NULL,
    PaymentMethod TINYINT NOT NULL COMMENT '1=BankTransfer, 2=CreditCard, 3=PIX, 4=Boleto',
    TransactionReference VARCHAR(200) NULL,
    Notes TEXT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CreatedBy VARCHAR(100) NULL,
    UpdatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    UpdatedBy VARCHAR(100) NULL,
    DeletedAt DATETIME(6) NULL,
    
    FOREIGN KEY (InvoiceId) REFERENCES BillingInvoices(Id),
    INDEX idx_billing_payment_invoice (InvoiceId),
    INDEX idx_billing_payment_date (PaymentDate)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- CORE TABLES (Continued)
-- =====================================================

CREATE TABLE IF NOT EXISTS users (
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
