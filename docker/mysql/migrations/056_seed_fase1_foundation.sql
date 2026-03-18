-- =====================================================
-- Migration 056 — Fase 1: Dados de Demonstração
-- Client + Companies + Users + Roles + user_companies
-- Data: 2026-03-13
-- Senha de todos os usuários demo: Demo@2024!
-- BCrypt cost 11: $2b$11$Yj28vPdIBYS/i1uw7bqC3OFfrJHBGYUcASRcLY7QabnnehSFSWRyW
-- =====================================================

-- USE partnership_manager;  -- banco local; no Azure o schema chama-se 'ppro'

SET @demo_pw   = '$2b$11$Yj28vPdIBYS/i1uw7bqC3OFfrJHBGYUcASRcLY7QabnnehSFSWRyW';
SET @super_uid = '00000001-0000-4000-a000-000000000002'; -- SuperAdmin já existente

-- =====================================================
-- UUIDs fixos — Client + Companies
-- =====================================================
SET @cli  = 'DEMO0001-0000-4000-A000-000000000001'; -- Ecossistema Ventures
SET @co1  = 'DEMO0001-0000-4000-A000-000000000010'; -- InovaTech LTDA
SET @co2  = 'DEMO0001-0000-4000-A000-000000000020'; -- VidaSaúde S.A.
SET @co3  = 'DEMO0001-0000-4000-A000-000000000030'; -- FinanGrow Participações

-- =====================================================
-- UUIDs fixos — Usuários
-- =====================================================
SET @u01 = 'DEMO0001-0000-4000-A000-000000000101'; -- Carlos Silva   (Admin+Founder, InovaTech)
SET @u02 = 'DEMO0001-0000-4000-A000-000000000102'; -- Ana Santos     (Founder, InovaTech)
SET @u03 = 'DEMO0001-0000-4000-A000-000000000103'; -- Roberto Lima   (Employee, InovaTech)
SET @u04 = 'DEMO0001-0000-4000-A000-000000000104'; -- Thiago Barbosa (Employee, InovaTech)
SET @u05 = 'DEMO0001-0000-4000-A000-000000000105'; -- Beatriz Rocha  (Employee, InovaTech)
SET @u06 = 'DEMO0001-0000-4000-A000-000000000106'; -- Gabriel Sousa  (HR, InovaTech)
SET @u07 = 'DEMO0001-0000-4000-A000-000000000107'; -- João Ferreira  (Admin+Founder, VidaSaúde)
SET @u08 = 'DEMO0001-0000-4000-A000-000000000108'; -- Mariana Oliveira (Founder, VidaSaúde)
SET @u09 = 'DEMO0001-0000-4000-A000-000000000109'; -- Pedro Alves    (Admin+Founder, FinanGrow)
SET @u10 = 'DEMO0001-0000-4000-A000-000000000110'; -- Lúcia Mendes   (Founder, FinanGrow)
SET @u11 = 'DEMO0001-0000-4000-A000-000000000111'; -- Dr. Marcos Rodrigues (BoardMember, todas)
SET @u12 = 'DEMO0001-0000-4000-A000-000000000112'; -- Dra. Cláudia Pereira (BoardMember, IT+VS)
SET @u13 = 'DEMO0001-0000-4000-A000-000000000113'; -- Rafael Cunha   (Legal, todas)
SET @u14 = 'DEMO0001-0000-4000-A000-000000000114'; -- Fernanda Lima  (Finance, todas)
SET @u15 = 'DEMO0001-0000-4000-A000-000000000115'; -- JP Angel       (Investor, IT+FG)
SET @u16 = 'DEMO0001-0000-4000-A000-000000000116'; -- Marina Torres  (Investor, VidaSaúde)

-- =====================================================
-- 1. CLIENT (tenant raiz)
-- =====================================================
INSERT IGNORE INTO clients
    (id, name, trading_name, document, document_type, email, phone, status, created_at, updated_at)
VALUES (
    @cli,
    'Ecossistema Ventures Ltda',
    'Ecossistema Ventures',
    '98765432000188',
    'cnpj',
    'contato@ecossistema.ventures',
    '+55 11 99999-0000',
    'Active',
    NOW(), NOW()
);

-- =====================================================
-- 2. COMPANIES
-- =====================================================
INSERT IGNORE INTO companies
    (id, client_id, name, trading_name, cnpj, legal_form, foundation_date,
     total_shares, share_price, currency,
     cep, street, number, neighborhood, city, state,
     status, created_by, updated_by)
VALUES
-- InovaTech LTDA — SaaS, Series A
(
    @co1, @cli,
    'InovaTech LTDA', 'InovaTech',
    '12345678000190', 'LTDA', '2020-03-15',
    10000000, 4.5000, 'BRL',
    '01310100', 'Avenida Paulista', '1374', 'Bela Vista', 'São Paulo', 'SP',
    'Active', @super_uid, @super_uid
),
-- VidaSaúde S.A. — Healthtech, Seed
(
    @co2, @cli,
    'VidaSaúde S.A.', 'VidaSaúde',
    '23456789000111', 'SA', '2021-06-01',
    5000000, 2.8000, 'BRL',
    '30130110', 'Rua dos Inconfidentes', '921', 'Funcionários', 'Belo Horizonte', 'MG',
    'Active', @super_uid, @super_uid
),
-- FinanGrow Participações — Fintech, Pre-seed
(
    @co3, @cli,
    'FinanGrow Participações Ltda', 'FinanGrow',
    '34567890000122', 'LTDA', '2022-11-10',
    2000000, 1.5000, 'BRL',
    '70040020', 'Setor Comercial Sul', '12', 'Asa Sul', 'Brasília', 'DF',
    'Active', @super_uid, @super_uid
);

-- =====================================================
-- 3. USERS
-- =====================================================
INSERT IGNORE INTO users
    (id, client_id, company_id, email, name, password_hash, status, language, timezone,
     failed_login_attempts, created_at, updated_at)
VALUES
-- InovaTech team
(@u01, @cli, @co1, 'carlos.silva@inovatech.com.br',    'Carlos Silva',      @demo_pw, 'Active', 'Portuguese', 'America/Sao_Paulo', 0, NOW(), NOW()),
(@u02, @cli, @co1, 'ana.santos@inovatech.com.br',      'Ana Santos',        @demo_pw, 'Active', 'Portuguese', 'America/Sao_Paulo', 0, NOW(), NOW()),
(@u03, @cli, @co1, 'roberto.lima@inovatech.com.br',    'Roberto Lima',      @demo_pw, 'Active', 'Portuguese', 'America/Sao_Paulo', 0, NOW(), NOW()),
(@u04, @cli, @co1, 'thiago.dev@inovatech.com.br',      'Thiago Barbosa',    @demo_pw, 'Active', 'Portuguese', 'America/Sao_Paulo', 0, NOW(), NOW()),
(@u05, @cli, @co1, 'beatriz.design@inovatech.com.br',  'Beatriz Rocha',     @demo_pw, 'Active', 'Portuguese', 'America/Sao_Paulo', 0, NOW(), NOW()),
(@u06, @cli, @co1, 'gabriel.hr@inovatech.com.br',      'Gabriel Sousa',     @demo_pw, 'Active', 'Portuguese', 'America/Sao_Paulo', 0, NOW(), NOW()),
-- VidaSaúde team
(@u07, @cli, @co2, 'joao.ferreira@vidasaude.com.br',   'João Ferreira',     @demo_pw, 'Active', 'Portuguese', 'America/Sao_Paulo', 0, NOW(), NOW()),
(@u08, @cli, @co2, 'mariana.oliveira@vidasaude.com.br','Mariana Oliveira',  @demo_pw, 'Active', 'Portuguese', 'America/Sao_Paulo', 0, NOW(), NOW()),
-- FinanGrow team
(@u09, @cli, @co3, 'pedro.alves@finangrow.com.br',     'Pedro Alves',       @demo_pw, 'Active', 'Portuguese', 'America/Brasilia',  0, NOW(), NOW()),
(@u10, @cli, @co3, 'lucia.mendes@finangrow.com.br',    'Lúcia Mendes',      @demo_pw, 'Active', 'Portuguese', 'America/Brasilia',  0, NOW(), NOW()),
-- Cross-company
(@u11, @cli, @co1, 'marcos@boardmember.ecossistema.com.br', 'Dr. Marcos Rodrigues', @demo_pw, 'Active', 'Portuguese', 'America/Sao_Paulo', 0, NOW(), NOW()),
(@u12, @cli, @co1, 'claudia@boardmember.ecossistema.com.br','Dra. Cláudia Pereira', @demo_pw, 'Active', 'Portuguese', 'America/Sao_Paulo', 0, NOW(), NOW()),
(@u13, @cli, @co1, 'rafael@legal.ecossistema.com.br',  'Rafael Cunha',      @demo_pw, 'Active', 'Portuguese', 'America/Sao_Paulo', 0, NOW(), NOW()),
(@u14, @cli, @co1, 'fernanda@finance.ecossistema.com.br','Fernanda Lima',   @demo_pw, 'Active', 'Portuguese', 'America/Sao_Paulo', 0, NOW(), NOW()),
(@u15, @cli, @co1, 'jp.angel@venture.com.br',          'João Paulo Anjos',  @demo_pw, 'Active', 'Portuguese', 'America/Sao_Paulo', 0, NOW(), NOW()),
(@u16, @cli, @co2, 'marina@healthfund.com.br',         'Marina Torres',     @demo_pw, 'Active', 'Portuguese', 'America/Sao_Paulo', 0, NOW(), NOW());

-- =====================================================
-- 4. USER_ROLES
-- =====================================================
INSERT IGNORE INTO user_roles (id, user_id, role, is_active, granted_by, granted_at, created_at, updated_at)
VALUES
-- Carlos Silva: Admin + Founder
(UUID(), @u01, 'Admin',   1, @super_uid, NOW(), NOW(), NOW()),
(UUID(), @u01, 'Founder', 1, @super_uid, NOW(), NOW(), NOW()),
-- Ana Santos: Founder
(UUID(), @u02, 'Founder', 1, @super_uid, NOW(), NOW(), NOW()),
-- Roberto Lima: Employee
(UUID(), @u03, 'Employee',1, @super_uid, NOW(), NOW(), NOW()),
-- Thiago Barbosa: Employee
(UUID(), @u04, 'Employee',1, @super_uid, NOW(), NOW(), NOW()),
-- Beatriz Rocha: Employee
(UUID(), @u05, 'Employee',1, @super_uid, NOW(), NOW(), NOW()),
-- Gabriel Sousa: HR
(UUID(), @u06, 'HR',      1, @super_uid, NOW(), NOW(), NOW()),
-- João Ferreira: Admin + Founder
(UUID(), @u07, 'Admin',   1, @super_uid, NOW(), NOW(), NOW()),
(UUID(), @u07, 'Founder', 1, @super_uid, NOW(), NOW(), NOW()),
-- Mariana Oliveira: Founder
(UUID(), @u08, 'Founder', 1, @super_uid, NOW(), NOW(), NOW()),
-- Pedro Alves: Admin + Founder
(UUID(), @u09, 'Admin',   1, @super_uid, NOW(), NOW(), NOW()),
(UUID(), @u09, 'Founder', 1, @super_uid, NOW(), NOW(), NOW()),
-- Lúcia Mendes: Founder
(UUID(), @u10, 'Founder', 1, @super_uid, NOW(), NOW(), NOW()),
-- Dr. Marcos: BoardMember
(UUID(), @u11, 'BoardMember', 1, @super_uid, NOW(), NOW(), NOW()),
-- Dra. Cláudia: BoardMember
(UUID(), @u12, 'BoardMember', 1, @super_uid, NOW(), NOW(), NOW()),
-- Rafael Cunha: Legal
(UUID(), @u13, 'Legal',   1, @super_uid, NOW(), NOW(), NOW()),
-- Fernanda Lima: Finance
(UUID(), @u14, 'Finance', 1, @super_uid, NOW(), NOW(), NOW()),
-- JP Angel: Investor
(UUID(), @u15, 'Investor',1, @super_uid, NOW(), NOW(), NOW()),
-- Marina Torres: Investor
(UUID(), @u16, 'Investor',1, @super_uid, NOW(), NOW(), NOW());

-- =====================================================
-- 5. USER_COMPANIES (vincula usuários às empresas)
-- =====================================================
INSERT IGNORE INTO user_companies
    (id, user_id, company_id, role, is_default, granted_at, granted_by)
VALUES
-- === InovaTech (@co1) ===
(UUID(), @u01, @co1, 'Admin',   1, NOW(), @super_uid),  -- Carlos Silva (default)
(UUID(), @u02, @co1, 'Admin',   1, NOW(), @super_uid),  -- Ana Santos
(UUID(), @u03, @co1, 'Editor',  1, NOW(), @super_uid),  -- Roberto Lima
(UUID(), @u04, @co1, 'Editor',  1, NOW(), @super_uid),  -- Thiago Barbosa
(UUID(), @u05, @co1, 'Editor',  1, NOW(), @super_uid),  -- Beatriz Rocha
(UUID(), @u06, @co1, 'Manager', 1, NOW(), @super_uid),  -- Gabriel Sousa (HR)
(UUID(), @u11, @co1, 'Manager', 1, NOW(), @super_uid),  -- Dr. Marcos
(UUID(), @u12, @co1, 'Manager', 1, NOW(), @super_uid),  -- Dra. Cláudia
(UUID(), @u13, @co1, 'Manager', 1, NOW(), @super_uid),  -- Rafael (Legal)
(UUID(), @u14, @co1, 'Manager', 1, NOW(), @super_uid),  -- Fernanda (Finance)
(UUID(), @u15, @co1, 'Viewer',  1, NOW(), @super_uid),  -- JP Angel (Investor)
-- === VidaSaúde (@co2) ===
(UUID(), @u07, @co2, 'Admin',   1, NOW(), @super_uid),  -- João Ferreira (default)
(UUID(), @u08, @co2, 'Admin',   1, NOW(), @super_uid),  -- Mariana Oliveira
(UUID(), @u11, @co2, 'Manager', 0, NOW(), @super_uid),  -- Dr. Marcos
(UUID(), @u13, @co2, 'Manager', 0, NOW(), @super_uid),  -- Rafael (Legal)
(UUID(), @u14, @co2, 'Manager', 0, NOW(), @super_uid),  -- Fernanda (Finance)
(UUID(), @u16, @co2, 'Viewer',  1, NOW(), @super_uid),  -- Marina Torres (Investor)
-- === FinanGrow (@co3) ===
(UUID(), @u09, @co3, 'Admin',   1, NOW(), @super_uid),  -- Pedro Alves (default)
(UUID(), @u10, @co3, 'Admin',   1, NOW(), @super_uid),  -- Lúcia Mendes
(UUID(), @u11, @co3, 'Manager', 0, NOW(), @super_uid),  -- Dr. Marcos
(UUID(), @u13, @co3, 'Manager', 0, NOW(), @super_uid),  -- Rafael (Legal)
(UUID(), @u14, @co3, 'Manager', 0, NOW(), @super_uid),  -- Fernanda (Finance)
(UUID(), @u15, @co3, 'Viewer',  0, NOW(), @super_uid); -- JP Angel (Investor)

-- Migration 056 Fase 1 — aplicada com sucesso
