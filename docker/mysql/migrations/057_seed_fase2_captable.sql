-- =====================================================
-- Migration 057 — Fase 2: Cap Table
-- Share Classes + Shareholders + Shares + Transactions
-- Data: 2026-03-13
-- =====================================================

-- UUIDs de referência (criados na Fase 1)
SET @cli  = 'DEMO0001-0000-4000-A000-000000000001';
SET @co1  = 'DEMO0001-0000-4000-A000-000000000010'; -- InovaTech
SET @co2  = 'DEMO0001-0000-4000-A000-000000000020'; -- VidaSaúde
SET @co3  = 'DEMO0001-0000-4000-A000-000000000030'; -- FinanGrow
SET @u01  = 'DEMO0001-0000-4000-A000-000000000101'; -- Carlos Silva
SET @u02  = 'DEMO0001-0000-4000-A000-000000000102'; -- Ana Santos
SET @u07  = 'DEMO0001-0000-4000-A000-000000000107'; -- João Ferreira
SET @u08  = 'DEMO0001-0000-4000-A000-000000000108'; -- Mariana Oliveira
SET @u09  = 'DEMO0001-0000-4000-A000-000000000109'; -- Pedro Alves
SET @u10  = 'DEMO0001-0000-4000-A000-000000000110'; -- Lúcia Mendes
SET @u14  = 'DEMO0001-0000-4000-A000-000000000114'; -- Fernanda Lima (Finance)

-- =====================================================
-- UUIDs fixos — Share Classes
-- =====================================================
-- InovaTech
SET @sc1_on   = 'DEMO0002-0000-4000-A000-000000000011'; -- IT Ordinárias
SET @sc1_pna  = 'DEMO0002-0000-4000-A000-000000000012'; -- IT Preferenciais A
SET @sc1_esop = 'DEMO0002-0000-4000-A000-000000000013'; -- IT ESOP Pool
-- VidaSaúde
SET @sc2_on   = 'DEMO0002-0000-4000-A000-000000000021'; -- VS Ordinárias
SET @sc2_pna  = 'DEMO0002-0000-4000-A000-000000000022'; -- VS Preferenciais A
SET @sc2_esop = 'DEMO0002-0000-4000-A000-000000000023'; -- VS ESOP Pool
-- FinanGrow
SET @sc3_on   = 'DEMO0002-0000-4000-A000-000000000031'; -- FG Ordinárias
SET @sc3_pnc  = 'DEMO0002-0000-4000-A000-000000000032'; -- FG Preferenciais Conversíveis

-- =====================================================
-- UUIDs fixos — Shareholders
-- =====================================================
-- InovaTech (6 shareholders)
SET @sh1_carlos  = 'DEMO0003-0000-4000-A000-000000000101';
SET @sh1_ana     = 'DEMO0003-0000-4000-A000-000000000102';
SET @sh1_fundo   = 'DEMO0003-0000-4000-A000-000000000103'; -- Fundo Aceleração Brasil
SET @sh1_jp      = 'DEMO0003-0000-4000-A000-000000000104'; -- JP Angel Investimentos
SET @sh1_esop    = 'DEMO0003-0000-4000-A000-000000000105'; -- ESOP Pool InovaTech
-- VidaSaúde (4 shareholders)
SET @sh2_joao    = 'DEMO0003-0000-4000-A000-000000000201';
SET @sh2_mariana = 'DEMO0003-0000-4000-A000-000000000202';
SET @sh2_marina  = 'DEMO0003-0000-4000-A000-000000000203'; -- Marina Torres Fund
SET @sh2_esop    = 'DEMO0003-0000-4000-A000-000000000204'; -- ESOP Pool VidaSaúde
-- FinanGrow (4 shareholders)
SET @sh3_pedro   = 'DEMO0003-0000-4000-A000-000000000301';
SET @sh3_lucia   = 'DEMO0003-0000-4000-A000-000000000302';
SET @sh3_fundo   = 'DEMO0003-0000-4000-A000-000000000303'; -- Fundo Aceleração Brasil (mesmo)
SET @sh3_esop    = 'DEMO0003-0000-4000-A000-000000000304'; -- ESOP Pool FinanGrow

-- =====================================================
-- 1. SHARE CLASSES
-- =====================================================
INSERT IGNORE INTO share_classes
    (id, client_id, company_id, name, code, description,
     has_voting_rights, votes_per_share, liquidation_preference, participating,
     is_convertible, conversion_ratio, anti_dilution_type, status, display_order,
     created_by, updated_by)
VALUES
-- === InovaTech ===
(@sc1_on, @cli, @co1, 'Cotas Ordinárias', 'ON',
 'Cotas ordinárias com direito a voto — classe principal dos fundadores',
 1, 1.0000, 1.00, 0, 0, NULL, 'None', 'Active', 1, @u01, @u01),
(@sc1_pna, @cli, @co1, 'Preferenciais Série A', 'PNA',
 'Cotas preferenciais emitidas no Seed Round e Series A — sem voto, com preferência de liquidação',
 0, 0.0000, 1.50, 1, 1, 1.0000, 'WeightedAverage', 'Active', 2, @u01, @u01),
(@sc1_esop, @cli, @co1, 'ESOP Pool', 'ESOP',
 'Pool reservado para opções de compra de funcionários (Employee Stock Option Plan)',
 0, 0.0000, 1.00, 0, 1, 1.0000, 'None', 'Active', 3, @u01, @u01),
-- === VidaSaúde ===
(@sc2_on, @cli, @co2, 'Ações Ordinárias', 'ON',
 'Ações ordinárias — fundadores e conselho',
 1, 1.0000, 1.00, 0, 0, NULL, 'None', 'Active', 1, @u07, @u07),
(@sc2_pna, @cli, @co2, 'Preferenciais Série A', 'PNA',
 'Ações preferenciais emitidas no Seed Round',
 0, 0.0000, 2.00, 1, 1, 1.0000, 'FullRatchet', 'Active', 2, @u07, @u07),
(@sc2_esop, @cli, @co2, 'ESOP Pool', 'ESOP',
 'Pool de opções para equipe técnica',
 0, 0.0000, 1.00, 0, 1, 1.0000, 'None', 'Active', 3, @u07, @u07),
-- === FinanGrow ===
(@sc3_on, @cli, @co3, 'Cotas Ordinárias', 'ON',
 'Cotas ordinárias dos fundadores',
 1, 1.0000, 1.00, 0, 0, NULL, 'None', 'Active', 1, @u09, @u09),
(@sc3_pnc, @cli, @co3, 'Preferenciais Conversíveis', 'PNC',
 'Cotas preferenciais convertíveis (SAFE convertido no Pre-seed)',
 0, 0.0000, 1.50, 1, 1, 1.0000, 'WeightedAverage', 'Active', 2, @u09, @u09);

-- =====================================================
-- 2. SHAREHOLDERS
-- =====================================================
INSERT IGNORE INTO shareholders
    (id, client_id, company_id, name, document, document_type, email, phone,
     type, status,
     address_street, address_number, address_city, address_state, address_zip_code,
     tag_along, drag_along, shareholders_agreement, right_of_first_refusal,
     created_by, updated_by)
VALUES
-- === InovaTech shareholders ===
(@sh1_carlos, @cli, @co1, 'Carlos Silva', '12345678901', 'cpf',
 'carlos.silva@inovatech.com.br', '+55 11 99001-0001',
 'Founder', 'Active', 'Avenida Paulista', '1374', 'São Paulo', 'SP', '01310100',
 1, 1, 1, 1, @u01, @u01),
(@sh1_ana, @cli, @co1, 'Ana Santos', '23456789012', 'cpf',
 'ana.santos@inovatech.com.br', '+55 11 99001-0002',
 'Founder', 'Active', 'Avenida Paulista', '1374', 'São Paulo', 'SP', '01310100',
 1, 1, 1, 1, @u01, @u01),
(@sh1_fundo, @cli, @co1, 'Fundo Aceleração Brasil FIP', '11222333000144', 'cnpj',
 'investimentos@fundoaceleracao.com.br', '+55 11 3000-1000',
 'Investor', 'Active', 'Rua Funchal', '418', 'São Paulo', 'SP', '04551060',
 1, 0, 1, 1, @u01, @u01),
(@sh1_jp, @cli, @co1, 'JP Angel Investimentos Ltda', '44555666000177', 'cnpj',
 'jp.angel@venture.com.br', '+55 11 97000-0015',
 'Investor', 'Active', 'Alameda Santos', '1800', 'São Paulo', 'SP', '01418200',
 1, 0, 1, 1, @u01, @u01),
(@sh1_esop, @cli, @co1, 'ESOP Pool InovaTech', '00000000000000', 'cnpj',
 'rh@inovatech.com.br', NULL,
 'ESOP', 'Active', 'Avenida Paulista', '1374', 'São Paulo', 'SP', '01310100',
 0, 0, 0, 0, @u01, @u01),
-- === VidaSaúde shareholders ===
(@sh2_joao, @cli, @co2, 'João Ferreira', '34567890123', 'cpf',
 'joao.ferreira@vidasaude.com.br', '+55 31 99002-0001',
 'Founder', 'Active', 'Rua dos Inconfidentes', '921', 'Belo Horizonte', 'MG', '30130110',
 1, 1, 1, 1, @u07, @u07),
(@sh2_mariana, @cli, @co2, 'Mariana Oliveira', '45678901234', 'cpf',
 'mariana.oliveira@vidasaude.com.br', '+55 31 99002-0002',
 'Founder', 'Active', 'Rua dos Inconfidentes', '921', 'Belo Horizonte', 'MG', '30130110',
 1, 1, 1, 1, @u07, @u07),
(@sh2_marina, @cli, @co2, 'Marina Torres Health Fund', '55666777000188', 'cnpj',
 'marina@healthfund.com.br', '+55 11 98000-0016',
 'Investor', 'Active', 'Av. Brigadeiro Faria Lima', '3144', 'São Paulo', 'SP', '01451000',
 1, 0, 1, 1, @u07, @u07),
(@sh2_esop, @cli, @co2, 'ESOP Pool VidaSaúde', '00000000000001', 'cnpj',
 'rh@vidasaude.com.br', NULL,
 'ESOP', 'Active', 'Rua dos Inconfidentes', '921', 'Belo Horizonte', 'MG', '30130110',
 0, 0, 0, 0, @u07, @u07),
-- === FinanGrow shareholders ===
(@sh3_pedro, @cli, @co3, 'Pedro Alves', '56789012345', 'cpf',
 'pedro.alves@finangrow.com.br', '+55 61 99003-0001',
 'Founder', 'Active', 'Setor Comercial Sul', '12', 'Brasília', 'DF', '70040020',
 1, 1, 1, 1, @u09, @u09),
(@sh3_lucia, @cli, @co3, 'Lúcia Mendes', '67890123456', 'cpf',
 'lucia.mendes@finangrow.com.br', '+55 61 99003-0002',
 'Founder', 'Active', 'Setor Comercial Sul', '12', 'Brasília', 'DF', '70040020',
 1, 1, 1, 1, @u09, @u09),
(@sh3_fundo, @cli, @co3, 'Fundo Aceleração Brasil FIP', '11222333000144', 'cnpj',
 'investimentos@fundoaceleracao.com.br', '+55 11 3000-1000',
 'Investor', 'Active', 'Rua Funchal', '418', 'São Paulo', 'SP', '04551060',
 1, 0, 1, 1, @u09, @u09),
(@sh3_esop, @cli, @co3, 'ESOP Pool FinanGrow', '00000000000002', 'cnpj',
 'rh@finangrow.com.br', NULL,
 'ESOP', 'Active', 'Setor Comercial Sul', '12', 'Brasília', 'DF', '70040020',
 0, 0, 0, 0, @u09, @u09);

-- =====================================================
-- 3. SHARE TRANSACTIONS (imutável — emissões)
-- co1 = InovaTech: capitalização 2020 + Seed 2022 + Series A 2024
-- co2 = VidaSaúde: capitalização 2021 + Seed 2023
-- co3 = FinanGrow: capitalização 2022 + Pre-seed SAFE 2024
-- =====================================================

-- Variáveis para TXN IDs
SET @tx1a = 'DEMO0004-0000-4000-A000-000000000001'; -- IT capital inicial Carlos
SET @tx1b = 'DEMO0004-0000-4000-A000-000000000002'; -- IT capital inicial Ana
SET @tx1c = 'DEMO0004-0000-4000-A000-000000000003'; -- IT Seed Fundo
SET @tx1d = 'DEMO0004-0000-4000-A000-000000000004'; -- IT Seed JP
SET @tx1e = 'DEMO0004-0000-4000-A000-000000000005'; -- IT SeriesA Fundo (follow-on)
SET @tx1f = 'DEMO0004-0000-4000-A000-000000000006'; -- IT ESOP reserva
SET @tx2a = 'DEMO0004-0000-4000-A000-000000000011'; -- VS capital inicial João
SET @tx2b = 'DEMO0004-0000-4000-A000-000000000012'; -- VS capital inicial Mariana
SET @tx2c = 'DEMO0004-0000-4000-A000-000000000013'; -- VS Seed Marina
SET @tx2d = 'DEMO0004-0000-4000-A000-000000000014'; -- VS ESOP reserva
SET @tx3a = 'DEMO0004-0000-4000-A000-000000000021'; -- FG capital inicial Pedro
SET @tx3b = 'DEMO0004-0000-4000-A000-000000000022'; -- FG capital inicial Lúcia
SET @tx3c = 'DEMO0004-0000-4000-A000-000000000023'; -- FG Pre-seed Fundo
SET @tx3d = 'DEMO0004-0000-4000-A000-000000000024'; -- FG ESOP reserva

INSERT IGNORE INTO share_transactions
    (id, client_id, company_id, transaction_type, transaction_number, reference_date,
     share_class_id, quantity, price_per_share,
     from_shareholder_id, to_shareholder_id,
     reason, document_reference, approved_by, approved_at, created_by)
VALUES
-- === InovaTech ===
-- Capitalização inicial 2020-03-15
(@tx1a, @cli, @co1, 'Issue', 'IT-TXN-001', '2020-03-15', @sc1_on, 4000000, 0.1000,
 NULL, @sh1_carlos, 'Capitalização inicial — fundador Carlos Silva', 'CONTRATO-2020-001', @u01, '2020-03-15 10:00:00', @u01),
(@tx1b, @cli, @co1, 'Issue', 'IT-TXN-002', '2020-03-15', @sc1_on, 4000000, 0.1000,
 NULL, @sh1_ana, 'Capitalização inicial — fundadora Ana Santos', 'CONTRATO-2020-001', @u01, '2020-03-15 10:00:00', @u01),
-- Seed Round 2022-06-01 — emissão PNA para investidores
(@tx1c, @cli, @co1, 'Issue', 'IT-TXN-003', '2022-06-01', @sc1_pna, 800000, 1.0000,
 NULL, @sh1_fundo, 'Seed Round — Fundo Aceleração Brasil, R$ 800k por 8%', 'SAFE-2022-001', @u01, '2022-06-01 14:00:00', @u01),
(@tx1d, @cli, @co1, 'Issue', 'IT-TXN-004', '2022-06-01', @sc1_pna, 200000, 1.0000,
 NULL, @sh1_jp, 'Seed Round — JP Angel, R$ 200k por 2%', 'SAFE-2022-002', @u01, '2022-06-01 14:00:00', @u01),
-- Series A 2024-09-01 — follow-on Fundo
(@tx1e, @cli, @co1, 'Issue', 'IT-TXN-005', '2024-09-01', @sc1_pna, 500000, 4.5000,
 NULL, @sh1_fundo, 'Series A — Fundo Aceleração Brasil follow-on, R$ 2.25M', 'SHA-2024-001', @u01, '2024-09-01 09:00:00', @u01),
-- ESOP reserva
(@tx1f, @cli, @co1, 'Issue', 'IT-TXN-006', '2020-03-15', @sc1_esop, 500000, 0.0000,
 NULL, @sh1_esop, 'Reserva ESOP Pool — 5% da empresa', 'ESOP-PLAN-2020', @u01, '2020-03-15 10:00:00', @u01),
-- === VidaSaúde ===
-- Capitalização inicial 2021-06-01
(@tx2a, @cli, @co2, 'Issue', 'VS-TXN-001', '2021-06-01', @sc2_on, 2000000, 0.1000,
 NULL, @sh2_joao, 'Capitalização inicial — fundador João Ferreira', 'CONTRATO-VS-2021-001', @u07, '2021-06-01 10:00:00', @u07),
(@tx2b, @cli, @co2, 'Issue', 'VS-TXN-002', '2021-06-01', @sc2_on, 1500000, 0.1000,
 NULL, @sh2_mariana, 'Capitalização inicial — fundadora Mariana Oliveira', 'CONTRATO-VS-2021-001', @u07, '2021-06-01 10:00:00', @u07),
-- Seed Round 2023-03-15 — emissão PNA Marina
(@tx2c, @cli, @co2, 'Issue', 'VS-TXN-003', '2023-03-15', @sc2_pna, 1000000, 2.0000,
 NULL, @sh2_marina, 'Seed Round — Marina Torres Health Fund, R$ 2M por 20%', 'SHA-VS-2023-001', @u07, '2023-03-15 11:00:00', @u07),
-- VidaSaúde ESOP
(@tx2d, @cli, @co2, 'Issue', 'VS-TXN-004', '2021-06-01', @sc2_esop, 500000, 0.0000,
 NULL, @sh2_esop, 'Reserva ESOP Pool VidaSaúde — 10% da empresa', 'ESOP-VS-2021', @u07, '2021-06-01 10:00:00', @u07),
-- === FinanGrow ===
-- Capitalização inicial 2022-11-10
(@tx3a, @cli, @co3, 'Issue', 'FG-TXN-001', '2022-11-10', @sc3_on, 800000, 0.1000,
 NULL, @sh3_pedro, 'Capitalização inicial — fundador Pedro Alves', 'CONTRATO-FG-2022-001', @u09, '2022-11-10 10:00:00', @u09),
(@tx3b, @cli, @co3, 'Issue', 'FG-TXN-002', '2022-11-10', @sc3_on, 600000, 0.1000,
 NULL, @sh3_lucia, 'Capitalização inicial — fundadora Lúcia Mendes', 'CONTRATO-FG-2022-001', @u09, '2022-11-10 10:00:00', @u09),
-- Pre-seed 2024-05-01 — conversão SAFE Fundo
(@tx3c, @cli, @co3, 'Issue', 'FG-TXN-003', '2024-05-01', @sc3_pnc, 300000, 1.5000,
 NULL, @sh3_fundo, 'Pre-seed — Fundo Aceleração Brasil, R$ 450k por 15%', 'SAFE-FG-2024-001', @u09, '2024-05-01 09:00:00', @u09),
-- FinanGrow ESOP
(@tx3d, @cli, @co3, 'Issue', 'FG-TXN-004', '2022-11-10', @sc3_on, 300000, 0.0000,
 NULL, @sh3_esop, 'Reserva ESOP Pool FinanGrow — 15% da empresa', 'ESOP-FG-2022', @u09, '2022-11-10 10:00:00', @u09);

-- =====================================================
-- 4. SHARES (posições ativas)
-- =====================================================
INSERT IGNORE INTO shares
    (id, client_id, company_id, shareholder_id, share_class_id,
     certificate_number, quantity, acquisition_price, acquisition_date,
     origin, origin_transaction_id, status, created_by, updated_by)
VALUES
-- === InovaTech ===
('DEMO0005-0000-4000-A000-000000000001', @cli, @co1, @sh1_carlos, @sc1_on,
 'IT-CERT-001', 4000000, 0.1000, '2020-03-15', 'Issue', @tx1a, 'Active', @u01, @u01),
('DEMO0005-0000-4000-A000-000000000002', @cli, @co1, @sh1_ana, @sc1_on,
 'IT-CERT-002', 4000000, 0.1000, '2020-03-15', 'Issue', @tx1b, 'Active', @u01, @u01),
('DEMO0005-0000-4000-A000-000000000003', @cli, @co1, @sh1_fundo, @sc1_pna,
 'IT-CERT-003', 800000, 1.0000, '2022-06-01', 'Issue', @tx1c, 'Active', @u01, @u01),
('DEMO0005-0000-4000-A000-000000000004', @cli, @co1, @sh1_jp, @sc1_pna,
 'IT-CERT-004', 200000, 1.0000, '2022-06-01', 'Issue', @tx1d, 'Active', @u01, @u01),
('DEMO0005-0000-4000-A000-000000000005', @cli, @co1, @sh1_fundo, @sc1_pna,
 'IT-CERT-005', 500000, 4.5000, '2024-09-01', 'Issue', @tx1e, 'Active', @u01, @u01),
('DEMO0005-0000-4000-A000-000000000006', @cli, @co1, @sh1_esop, @sc1_esop,
 'IT-CERT-006', 500000, 0.0000, '2020-03-15', 'Issue', @tx1f, 'Active', @u01, @u01),
-- === VidaSaúde ===
('DEMO0005-0000-4000-A000-000000000011', @cli, @co2, @sh2_joao, @sc2_on,
 'VS-CERT-001', 2000000, 0.1000, '2021-06-01', 'Issue', @tx2a, 'Active', @u07, @u07),
('DEMO0005-0000-4000-A000-000000000012', @cli, @co2, @sh2_mariana, @sc2_on,
 'VS-CERT-002', 1500000, 0.1000, '2021-06-01', 'Issue', @tx2b, 'Active', @u07, @u07),
('DEMO0005-0000-4000-A000-000000000013', @cli, @co2, @sh2_marina, @sc2_pna,
 'VS-CERT-003', 1000000, 2.0000, '2023-03-15', 'Issue', @tx2c, 'Active', @u07, @u07),
('DEMO0005-0000-4000-A000-000000000014', @cli, @co2, @sh2_esop, @sc2_esop,
 'VS-CERT-004', 500000, 0.0000, '2021-06-01', 'Issue', @tx2d, 'Active', @u07, @u07),
-- === FinanGrow ===
('DEMO0005-0000-4000-A000-000000000021', @cli, @co3, @sh3_pedro, @sc3_on,
 'FG-CERT-001', 800000, 0.1000, '2022-11-10', 'Issue', @tx3a, 'Active', @u09, @u09),
('DEMO0005-0000-4000-A000-000000000022', @cli, @co3, @sh3_lucia, @sc3_on,
 'FG-CERT-002', 600000, 0.1000, '2022-11-10', 'Issue', @tx3b, 'Active', @u09, @u09),
('DEMO0005-0000-4000-A000-000000000023', @cli, @co3, @sh3_fundo, @sc3_pnc,
 'FG-CERT-003', 300000, 1.5000, '2024-05-01', 'Issue', @tx3c, 'Active', @u09, @u09),
('DEMO0005-0000-4000-A000-000000000024', @cli, @co3, @sh3_esop, @sc3_on,
 'FG-CERT-004', 300000, 0.0000, '2022-11-10', 'Issue', @tx3d, 'Active', @u09, @u09);

-- Migration 057 Fase 2 — aplicada com sucesso
