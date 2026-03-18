-- =====================================================
-- Migration 064 — Fase 9: Billing
-- billingplans, billingclients, billingsubscriptions,
-- billinginvoices
-- Data: 2026-03-16
-- Enums: BillingCycle Monthly=1, Yearly=2
--        ClientType Individual=1, Company=2
--        ClientStatus Active=1, Suspended=2, Cancelled=3
--        SubscriptionStatus Pending=1, Active=2, Suspended=3, Cancelled=4
--        InvoiceStatus Pending=1, Paid=2, Overdue=3, Cancelled=4
-- =====================================================

-- =====================================================
-- BILLING PLANS (3 planos SaaS)
-- =====================================================
INSERT IGNORE INTO billingplans
    (Id, Name, Description, Price, BillingCycle, Features,
     MaxCompanies, MaxUsers, IsActive, CreatedBy, UpdatedBy)
VALUES
('DEMO0020-0000-4000-A000-000000000001',
 'Starter', 'Ideal para startups em fase inicial até 3 empresas no portfólio.',
 299.00, 1,
 '["Cap Table básico","Vesting simples","Até 3 empresas","Até 10 usuários","Suporte por e-mail"]',
 3, 10, 1, 'admin@sistema.com', 'admin@sistema.com'),

('DEMO0020-0000-4000-A000-000000000002',
 'Professional', 'Para fundos e gestores com múltiplas empresas e relatórios avançados.',
 899.00, 1,
 '["Cap Table completo","Vesting avançado","Contratos digitais","Valuation tracking","Até 10 empresas","Até 50 usuários","Data Room","Workflows de aprovação","Suporte prioritário"]',
 10, 50, 1, 'admin@sistema.com', 'admin@sistema.com'),

('DEMO0020-0000-4000-A000-000000000003',
 'Enterprise', 'Para gestoras e family offices com portfólio ilimitado e features premium.',
 2499.00, 1,
 '["Tudo do Professional","Portfólio ilimitado","Usuários ilimitados","API REST completa","Relatórios customizados","Portal do investidor white-label","Integração contábil","SLA 99,9%","Gerente de sucesso dedicado"]',
 -1, -1, 1, 'admin@sistema.com', 'admin@sistema.com');

-- =====================================================
-- BILLING CLIENTS (1 cliente = Ecossistema Ventures)
-- =====================================================
INSERT IGNORE INTO billingclients
    (Id, Name, Email, Document, Type, Status,
     Phone, Address, City, State, ZipCode, Country,
     CreatedBy, UpdatedBy)
VALUES
('DEMO0021-0000-4000-A000-000000000001',
 'Ecossistema Ventures Gestora Ltda',
 'financeiro@ecossistemaventures.com.br',
 '47.820.193/0001-55',
 2, 1,
 '+55 11 3456-7890',
 'Av. Faria Lima, 3.477 — Andar 12 — Itaim Bibi',
 'São Paulo', 'SP', '04538-133', 'Brasil',
 'admin@sistema.com', 'admin@sistema.com');

-- =====================================================
-- BILLING SUBSCRIPTIONS (1 Enterprise ativa)
-- =====================================================
INSERT IGNORE INTO billingsubscriptions
    (Id, ClientId, PlanId, Status, StartDate, EndDate,
     AutoRenew, CompaniesCount, UsersCount,
     CreatedBy, UpdatedBy)
VALUES
('DEMO0022-0000-4000-A000-000000000001',
 'DEMO0021-0000-4000-A000-000000000001',
 'DEMO0020-0000-4000-A000-000000000003',
 2, '2025-04-01', NULL,
 1, 4, 17,
 'admin@sistema.com', 'admin@sistema.com');

-- =====================================================
-- BILLING INVOICES (12 faturas mensais: abr/25–mar/26)
-- 11 Pagas + 1 Pendente (março/2026)
-- InvoiceStatus Paid=2, Pending=1
-- =====================================================
INSERT IGNORE INTO billinginvoices
    (Id, ClientId, SubscriptionId, InvoiceNumber, Amount,
     IssueDate, DueDate, Status, PaymentDate, Description,
     CreatedBy, UpdatedBy)
VALUES
('DEMO0023-0000-4000-A000-000000000001',
 'DEMO0021-0000-4000-A000-000000000001','DEMO0022-0000-4000-A000-000000000001',
 'INV-2025-0001', 2499.00, '2025-04-01','2025-04-10',
 2,'2025-04-08 10:30:00','Assinatura Enterprise — Abril 2025',
 'sistema@ecossistemaventures.com.br','sistema@ecossistemaventures.com.br'),

('DEMO0023-0000-4000-A000-000000000002',
 'DEMO0021-0000-4000-A000-000000000001','DEMO0022-0000-4000-A000-000000000001',
 'INV-2025-0002', 2499.00, '2025-05-01','2025-05-10',
 2,'2025-05-09 09:15:00','Assinatura Enterprise — Maio 2025',
 'sistema@ecossistemaventures.com.br','sistema@ecossistemaventures.com.br'),

('DEMO0023-0000-4000-A000-000000000003',
 'DEMO0021-0000-4000-A000-000000000001','DEMO0022-0000-4000-A000-000000000001',
 'INV-2025-0003', 2499.00, '2025-06-01','2025-06-10',
 2,'2025-06-07 11:00:00','Assinatura Enterprise — Junho 2025',
 'sistema@ecossistemaventures.com.br','sistema@ecossistemaventures.com.br'),

('DEMO0023-0000-4000-A000-000000000004',
 'DEMO0021-0000-4000-A000-000000000001','DEMO0022-0000-4000-A000-000000000001',
 'INV-2025-0004', 2499.00, '2025-07-01','2025-07-10',
 2,'2025-07-08 10:00:00','Assinatura Enterprise — Julho 2025',
 'sistema@ecossistemaventures.com.br','sistema@ecossistemaventures.com.br'),

('DEMO0023-0000-4000-A000-000000000005',
 'DEMO0021-0000-4000-A000-000000000001','DEMO0022-0000-4000-A000-000000000001',
 'INV-2025-0005', 2499.00, '2025-08-01','2025-08-10',
 2,'2025-08-06 14:20:00','Assinatura Enterprise — Agosto 2025',
 'sistema@ecossistemaventures.com.br','sistema@ecossistemaventures.com.br'),

('DEMO0023-0000-4000-A000-000000000006',
 'DEMO0021-0000-4000-A000-000000000001','DEMO0022-0000-4000-A000-000000000001',
 'INV-2025-0006', 2499.00, '2025-09-01','2025-09-10',
 2,'2025-09-09 09:45:00','Assinatura Enterprise — Setembro 2025',
 'sistema@ecossistemaventures.com.br','sistema@ecossistemaventures.com.br'),

('DEMO0023-0000-4000-A000-000000000007',
 'DEMO0021-0000-4000-A000-000000000001','DEMO0022-0000-4000-A000-000000000001',
 'INV-2025-0007', 2499.00, '2025-10-01','2025-10-10',
 2,'2025-10-08 11:30:00','Assinatura Enterprise — Outubro 2025',
 'sistema@ecossistemaventures.com.br','sistema@ecossistemaventures.com.br'),

('DEMO0023-0000-4000-A000-000000000008',
 'DEMO0021-0000-4000-A000-000000000001','DEMO0022-0000-4000-A000-000000000001',
 'INV-2025-0008', 2499.00, '2025-11-01','2025-11-10',
 2,'2025-11-07 10:00:00','Assinatura Enterprise — Novembro 2025',
 'sistema@ecossistemaventures.com.br','sistema@ecossistemaventures.com.br'),

('DEMO0023-0000-4000-A000-000000000009',
 'DEMO0021-0000-4000-A000-000000000001','DEMO0022-0000-4000-A000-000000000001',
 'INV-2025-0009', 2499.00, '2025-12-01','2025-12-10',
 2,'2025-12-05 09:00:00','Assinatura Enterprise — Dezembro 2025',
 'sistema@ecossistemaventures.com.br','sistema@ecossistemaventures.com.br'),

('DEMO0023-0000-4000-A000-000000000010',
 'DEMO0021-0000-4000-A000-000000000001','DEMO0022-0000-4000-A000-000000000001',
 'INV-2026-0001', 2499.00, '2026-01-01','2026-01-10',
 2,'2026-01-08 10:15:00','Assinatura Enterprise — Janeiro 2026',
 'sistema@ecossistemaventures.com.br','sistema@ecossistemaventures.com.br'),

('DEMO0023-0000-4000-A000-000000000011',
 'DEMO0021-0000-4000-A000-000000000001','DEMO0022-0000-4000-A000-000000000001',
 'INV-2026-0002', 2499.00, '2026-02-01','2026-02-10',
 2,'2026-02-07 14:00:00','Assinatura Enterprise — Fevereiro 2026',
 'sistema@ecossistemaventures.com.br','sistema@ecossistemaventures.com.br'),

-- Março 2026 — Pendente
('DEMO0023-0000-4000-A000-000000000012',
 'DEMO0021-0000-4000-A000-000000000001','DEMO0022-0000-4000-A000-000000000001',
 'INV-2026-0003', 2499.00, '2026-03-01','2026-03-10',
 1, NULL,'Assinatura Enterprise — Março 2026',
 'sistema@ecossistemaventures.com.br','sistema@ecossistemaventures.com.br');

-- Migration 064 Fase 9 Billing — fim
