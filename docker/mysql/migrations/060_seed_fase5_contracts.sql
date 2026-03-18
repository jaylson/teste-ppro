-- =====================================================
-- Migration 060 — Fase 5: Contratos
-- Contracts + Parties + Versions
-- Data: 2026-03-16
-- =====================================================

SET @cli  = 'DEMO0001-0000-4000-A000-000000000001';
SET @co1  = 'DEMO0001-0000-4000-A000-000000000010'; -- InovaTech
SET @co2  = 'DEMO0001-0000-4000-A000-000000000020'; -- VidaSaúde
SET @co3  = 'DEMO0001-0000-4000-A000-000000000030'; -- FinanGrow
SET @u01  = 'DEMO0001-0000-4000-A000-000000000101'; -- Carlos Silva
SET @u02  = 'DEMO0001-0000-4000-A000-000000000102'; -- Ana Santos
SET @u03  = 'DEMO0001-0000-4000-A000-000000000103'; -- Roberto Lima
SET @u07  = 'DEMO0001-0000-4000-A000-000000000107'; -- João Ferreira
SET @u09  = 'DEMO0001-0000-4000-A000-000000000109'; -- Pedro Alves
SET @u06  = 'DEMO0001-0000-4000-A000-000000000106'; -- Gabriel Sousa (HR)
SET @u13  = 'DEMO0001-0000-4000-A000-000000000113'; -- Rafael Cunha (Legal)
SET @u14  = 'DEMO0001-0000-4000-A000-000000000114'; -- Fernanda Lima (Finance)

-- Shareholders
SET @sh1_carlos  = 'DEMO0003-0000-4000-A000-000000000101';
SET @sh1_fundo   = 'DEMO0003-0000-4000-A000-000000000103';
SET @sh1_jp      = 'DEMO0003-0000-4000-A000-000000000104';
SET @sh2_joao    = 'DEMO0003-0000-4000-A000-000000000201';
SET @sh2_marina  = 'DEMO0003-0000-4000-A000-000000000203';
SET @sh3_pedro   = 'DEMO0003-0000-4000-A000-000000000301';
SET @sh3_fundo   = 'DEMO0003-0000-4000-A000-000000000303';

-- =====================================================
-- CONTRACT UUIDs
-- =====================================================
-- InovaTech
SET @ct1a = 'DEMO0010-0001-4000-A000-000000000001'; -- SHA Series A
SET @ct1b = 'DEMO0010-0001-4000-A000-000000000002'; -- NDA Parceiro Tech
SET @ct1c = 'DEMO0010-0001-4000-A000-000000000003'; -- Stock Option Carlos
SET @ct1d = 'DEMO0010-0001-4000-A000-000000000004'; -- Employment Roberto
-- VidaSaúde
SET @ct2a = 'DEMO0010-0002-4000-A000-000000000001'; -- SHA Marina Torres
SET @ct2b = 'DEMO0010-0002-4000-A000-000000000002'; -- NDA Hospital
SET @ct2c = 'DEMO0010-0002-4000-A000-000000000003'; -- Serviço Desenvolvimento
SET @ct2d = 'DEMO0010-0002-4000-A000-000000000004'; -- Stock Option João
-- FinanGrow
SET @ct3a = 'DEMO0010-0003-4000-A000-000000000001'; -- SHA Fundo Aceleração
SET @ct3b = 'DEMO0010-0003-4000-A000-000000000002'; -- Contrato Investimento SAFE
SET @ct3c = 'DEMO0010-0003-4000-A000-000000000003'; -- NDA Banco Parceiro
SET @ct3d = 'DEMO0010-0003-4000-A000-000000000004'; -- Employment Pedro

-- =====================================================
-- 1. CONTRACTS
-- =====================================================
INSERT IGNORE INTO contracts
    (id, client_id, company_id, title, description, contract_type,
     current_version_number, status, contract_date, expiration_date,
     external_reference, notes, created_by, updated_by)
VALUES
-- === InovaTech ===
(@ct1a, @cli, @co1,
 'Acordo de Acionistas — InovaTech Series A',
 'SHA regulando direitos e obrigações entre fundadores e investidores após Series A.',
 'shareholders_agreement', 2, 'executed',
 '2024-09-01 00:00:00', NULL,
 'IT-SHA-2024-001', 'Revisado e aprovado pelo jurídico. 2a versão inclui anti-diluição.', @u13, @u13),

(@ct1b, @cli, @co1,
 'NDA — ClickSolutions Technology Ltda',
 'Acordo de confidencialidade para avaliação de parceria co-marketing B2B.',
 'nda', 1, 'signed',
 '2025-08-15 00:00:00', '2027-08-15 00:00:00',
 'IT-NDA-2025-001', 'Parceiro potencial para integração de pagamentos.', @u13, @u13),

(@ct1c, @cli, @co1,
 'Acordo de Opção de Compra — Carlos Silva',
 'Stock Option Agreement referente ao ESOP grant do fundador.',
 'stock_option', 1, 'executed',
 '2022-01-15 00:00:00', NULL,
 'IT-ESOP-2022-001', 'Vinculado ao vesting plan IT-PLAN-01.', @u13, @u13),

(@ct1d, @cli, @co1,
 'Contrato de Trabalho — Roberto Lima',
 'CLT sênior com cláusula de confidencialidade e ESOP.',
 'employment', 1, 'executed',
 '2024-01-02 00:00:00', NULL,
 'IT-CLT-2024-001', 'Cargo: Engenheiro de Software Sênior.', @u13, @u13),

-- === VidaSaúde ===
(@ct2a, @cli, @co2,
 'Acordo de Acionistas — VidaSaúde Seed Round',
 'SHA entre fundadores e Marina Torres Health Fund após Seed Round 2023.',
 'shareholders_agreement', 1, 'executed',
 '2023-03-20 00:00:00', NULL,
 'VS-SHA-2023-001', 'Inclui preferência de liquidação 2x para preferenciais.', @u07, @u13),

(@ct2b, @cli, @co2,
 'NDA — Hospital São Lucas',
 'Confidencialidade para piloto de telemedicina no Hospital São Lucas.',
 'nda', 1, 'signed',
 '2024-06-01 00:00:00', '2026-06-01 00:00:00',
 'VS-NDA-2024-001', 'Projeto piloto aprovado para Q3 2024.', @u13, @u07),

(@ct2c, @cli, @co2,
 'Contrato de Prestação de Serviços — DataHealth Tecnologia',
 'Desenvolvimento de módulo de inteligência artificial para triagem médica.',
 'service_agreement', 1, 'signed',
 '2024-09-01 00:00:00', '2025-08-31 00:00:00',
 'VS-SVC-2024-001', 'Contrato de 12 meses renováveis. Entrega em 4 milestones.', @u07, @u07),

(@ct2d, @cli, @co2,
 'Acordo de Opção de Compra — João Ferreira',
 'Stock Option Agreement do fundador João Ferreira.',
 'stock_option', 1, 'executed',
 '2021-06-15 00:00:00', NULL,
 'VS-ESOP-2021-001', 'Vinculado ao vesting plan VS-PLAN-01.', @u07, @u07),

-- === FinanGrow ===
(@ct3a, @cli, @co3,
 'Acordo de Acionistas — FinanGrow Pre-seed',
 'SHA entre fundadores e Fundo Aceleração Brasil após conversão do SAFE.',
 'shareholders_agreement', 1, 'executed',
 '2024-05-15 00:00:00', NULL,
 'FG-SHA-2024-001', 'Governa direitos dos preferenciais conversíveis.', @u09, @u13),

(@ct3b, @cli, @co3,
 'Contrato de Investimento — SAFE FinanGrow',
 'Simple Agreement for Future Equity convertido em maio de 2024.',
 'investment', 1, 'executed',
 '2023-10-01 00:00:00', NULL,
 'FG-SAFE-2023-001', 'R$450k convertido a uma avaliação pré-money de R$2.55M.', @u09, @u14),

(@ct3c, @cli, @co3,
 'NDA — Banco Nacional do Brasil',
 'Confidencialidade para discussão de parceria de infraestrutura de pagamentos.',
 'nda', 1, 'sent_for_signature',
 '2026-03-10 00:00:00', '2028-03-10 00:00:00',
 'FG-NDA-2026-001', 'Aguardando assinatura do banco. Enviado por email 10/03/2026.', @u13, @u13),

(@ct3d, @cli, @co3,
 'Contrato de Trabalho — Pedro Alves',
 'Contrato de fundador com PJ e ESOP.',
 'employment', 1, 'executed',
 '2022-11-01 00:00:00', NULL,
 'FG-CLT-2022-001', 'Cargo: CEO e Co-fundador.', @u09, @u09);

-- =====================================================
-- 2. CONTRACT PARTIES
-- =====================================================
INSERT IGNORE INTO contract_parties
    (id, contract_id, party_type, party_name, party_email,
     user_id, shareholder_id, signature_status, signature_date,
     sequence_order)
VALUES
-- SHA Series A InovaTech — 3 partes
('DEMO0011-0001-4000-A000-000000000001', @ct1a, 'Offeror', 'InovaTech Soluções Digitais Ltda', 'juridico@inovatech.com.br', @u01, NULL, 'signed', '2024-09-01 10:00:00', 1),
('DEMO0011-0001-4000-A000-000000000002', @ct1a, 'Investor', 'Fundo Aceleração Brasil FIP', 'investimentos@fundoaceleracao.com.br', NULL, @sh1_fundo, 'signed', '2024-09-01 14:00:00', 2),
('DEMO0011-0001-4000-A000-000000000003', @ct1a, 'Investor', 'JP Angel Investimentos Ltda', 'jp.angel@venture.com.br', NULL, @sh1_jp, 'signed', '2024-09-02 10:00:00', 3),
-- NDA Parceiro Tech — 2 partes
('DEMO0011-0001-4000-A000-000000000004', @ct1b, 'Offeror', 'InovaTech Soluções Digitais Ltda', 'juridico@inovatech.com.br', @u13, NULL, 'signed', '2025-08-15 10:00:00', 1),
('DEMO0011-0001-4000-A000-000000000005', @ct1b, 'Counterpart', 'ClickSolutions Technology Ltda', 'legal@clicksolutions.com.br', NULL, NULL, 'signed', '2025-08-16 10:00:00', 2),
-- Stock Option Carlos
('DEMO0011-0001-4000-A000-000000000006', @ct1c, 'Company', 'InovaTech Soluções Digitais Ltda', 'juridico@inovatech.com.br', @u01, NULL, 'signed', '2022-01-15 10:00:00', 1),
('DEMO0011-0001-4000-A000-000000000007', @ct1c, 'Beneficiary', 'Carlos Silva', 'carlos.silva@inovatech.com.br', @u01, @sh1_carlos, 'signed', '2022-01-15 14:00:00', 2),
-- Employment Roberto
('DEMO0011-0001-4000-A000-000000000008', @ct1d, 'Employer', 'InovaTech Soluções Digitais Ltda', 'rh@inovatech.com.br', @u06, NULL, 'signed', '2024-01-02 10:00:00', 1),
('DEMO0011-0001-4000-A000-000000000009', @ct1d, 'Employee', 'Roberto Lima', 'roberto.lima@inovatech.com.br', @u03, NULL, 'signed', '2024-01-02 14:00:00', 2),
-- SHA VidaSaúde
('DEMO0011-0002-4000-A000-000000000001', @ct2a, 'Founder', 'João Ferreira', 'joao.ferreira@vidasaude.com.br', @u07, @sh2_joao, 'signed', '2023-03-20 10:00:00', 1),
('DEMO0011-0002-4000-A000-000000000002', @ct2a, 'Investor', 'Marina Torres Health Fund', 'marina@healthfund.com.br', NULL, @sh2_marina, 'signed', '2023-03-20 14:00:00', 2),
-- NDA Hospital
('DEMO0011-0002-4000-A000-000000000003', @ct2b, 'Offeror', 'VidaSaúde Tecnologia em Saúde S.A.', 'juridico@vidasaude.com.br', @u07, NULL, 'signed', '2024-06-01 10:00:00', 1),
('DEMO0011-0002-4000-A000-000000000004', @ct2b, 'Counterpart', 'Hospital São Lucas', 'juridico@saolucas.com.br', NULL, NULL, 'signed', '2024-06-03 10:00:00', 2),
-- Stock Option João
('DEMO0011-0002-4000-A000-000000000005', @ct2d, 'Company', 'VidaSaúde Tecnologia em Saúde S.A.', 'juridico@vidasaude.com.br', @u07, NULL, 'signed', '2021-06-15 10:00:00', 1),
('DEMO0011-0002-4000-A000-000000000006', @ct2d, 'Beneficiary', 'João Ferreira', 'joao.ferreira@vidasaude.com.br', @u07, @sh2_joao, 'signed', '2021-06-15 14:00:00', 2),
-- SHA FinanGrow
('DEMO0011-0003-4000-A000-000000000001', @ct3a, 'Founder', 'Pedro Alves', 'pedro.alves@finangrow.com.br', @u09, @sh3_pedro, 'signed', '2024-05-15 10:00:00', 1),
('DEMO0011-0003-4000-A000-000000000002', @ct3a, 'Investor', 'Fundo Aceleração Brasil FIP', 'investimentos@fundoaceleracao.com.br', NULL, @sh3_fundo, 'signed', '2024-05-15 14:00:00', 2),
-- SAFE FinanGrow
('DEMO0011-0003-4000-A000-000000000003', @ct3b, 'Issuer', 'FinanGrow Participações Ltda', 'juridico@finangrow.com.br', @u09, NULL, 'signed', '2023-10-01 10:00:00', 1),
('DEMO0011-0003-4000-A000-000000000004', @ct3b, 'Investor', 'Fundo Aceleração Brasil FIP', 'investimentos@fundoaceleracao.com.br', NULL, @sh3_fundo, 'signed', '2023-10-01 14:00:00', 2),
-- NDA Banco (aguardando assinatura do banco)
('DEMO0011-0003-4000-A000-000000000005', @ct3c, 'Offeror', 'FinanGrow Participações Ltda', 'juridico@finangrow.com.br', @u13, NULL, 'signed', '2026-03-10 10:00:00', 1),
('DEMO0011-0003-4000-A000-000000000006', @ct3c, 'Counterpart', 'Banco Nacional do Brasil', 'legal@banconacional.com.br', NULL, NULL, 'waiting_signature', NULL, 2);

-- =====================================================
-- 3. CONTRACT VERSIONS
-- =====================================================
INSERT IGNORE INTO contract_versions
    (id, contract_id, version_number, file_path, file_size, file_hash,
     file_type, source, notes, created_by)
VALUES
-- SHA IT v1 (rascunho) e v2 (final)
('DEMO0012-0001-4000-A000-000000000001', @ct1a, 1, '/contracts/inovatech/sha_series_a_v1.pdf', 245760, 'abc123def456789012345678901234567890', 'pdf', 'upload', 'Versão inicial aprovada pelo jurídico.', @u13),
('DEMO0012-0001-4000-A000-000000000002', @ct1a, 2, '/contracts/inovatech/sha_series_a_v2.pdf', 267264, 'def456abc789012345678901234567890123', 'pdf', 'upload', 'Revisão: inclui cláusula anti-diluição série A.', @u13),
-- NDA IT v1
('DEMO0012-0001-4000-A000-000000000003', @ct1b, 1, '/contracts/inovatech/nda_clicksolutions_v1.pdf', 98304, '999aaa111bbb222ccc333ddd444eee555fff', 'pdf', 'builder', 'Gerado pelo construtor de contratos.', @u13),
-- Stock Option IT v1
('DEMO0012-0001-4000-A000-000000000004', @ct1c, 1, '/contracts/inovatech/esop_carlos_v1.pdf', 122880, '777bbb333ccc444ddd555eee666fff777aaa', 'pdf', 'upload', 'Original assinado e digitalizado.', @u13),
-- Employment IT v1
('DEMO0012-0001-4000-A000-000000000005', @ct1d, 1, '/contracts/inovatech/clt_roberto_v1.pdf', 110592, '555ccc666ddd777eee888fff999aaa111bbb', 'pdf', 'builder', 'Gerado pelo construtor de contratos CLT.', @u13),
-- SHA VS v1
('DEMO0012-0002-4000-A000-000000000001', @ct2a, 1, '/contracts/vidasaude/sha_seed_v1.pdf', 198656, '111ddd222eee333fff444aaa555bbb666ccc', 'pdf', 'upload', 'SHA Seed Round VidaSaúde.', @u13),
-- NDA VS v1
('DEMO0012-0002-4000-A000-000000000002', @ct2b, 1, '/contracts/vidasaude/nda_saolucas_v1.pdf', 92160, '222eee333fff444aaa555bbb666ccc777ddd', 'pdf', 'builder', 'NDA para parceria com Hospital.', @u13),
-- Service VS v1
('DEMO0012-0002-4000-A000-000000000003', @ct2c, 1, '/contracts/vidasaude/svc_datahealth_v1.pdf', 176128, '333fff444aaa555bbb666ccc777ddd888eee', 'pdf', 'upload', 'Contrato de desenvolvimento IA.', @u13),
-- SHA FG v1
('DEMO0012-0003-4000-A000-000000000001', @ct3a, 1, '/contracts/finangrow/sha_preseed_v1.pdf', 212992, '444aaa555bbb666ccc777ddd888eee999fff', 'pdf', 'upload', 'SHA pós-conversão SAFE.', @u13),
-- SAFE FG v1
('DEMO0012-0003-4000-A000-000000000002', @ct3b, 1, '/contracts/finangrow/safe_v1.pdf', 135168, '555bbb666ccc777ddd888eee999fff111aaa', 'pdf', 'upload', 'SAFE original Y Combinator adaptado.', @u13),
-- NDA Banco FG v1
('DEMO0012-0003-4000-A000-000000000003', @ct3c, 1, '/contracts/finangrow/nda_banco_v1.pdf', 88064, '666ccc777ddd888eee999fff111aaa222bbb', 'pdf', 'builder', 'NDA para parceria bancária.', @u13);

-- Migration 060 Fase 5 Contratos — fim
