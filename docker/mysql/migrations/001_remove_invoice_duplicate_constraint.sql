-- Migration: Remove constraint que impede geração de faturas após cancelamento
-- Data: 2026-01-22
-- Descrição: Faturas canceladas não devem bloquear a geração de novas faturas

USE partnership_manager;

-- Passo 1: Remover a foreign key temporariamente
ALTER TABLE BillingInvoices DROP FOREIGN KEY BillingInvoices_ibfk_2;

-- Passo 2: Remove o índice único que impede duplicação
ALTER TABLE BillingInvoices DROP INDEX idx_subscription_issue_month;

-- Passo 3: Cria um índice normal (não único) para performance
ALTER TABLE BillingInvoices ADD INDEX idx_subscription_issue_month (SubscriptionId, IssueDate);

-- Passo 4: Adiciona índice para ajudar nas queries de faturas não canceladas
ALTER TABLE BillingInvoices ADD INDEX idx_subscription_issue_status (SubscriptionId, IssueDate, Status);

-- Passo 5: Recriar a foreign key
ALTER TABLE BillingInvoices ADD CONSTRAINT BillingInvoices_ibfk_2 
    FOREIGN KEY (SubscriptionId) REFERENCES BillingSubscriptions(Id);
