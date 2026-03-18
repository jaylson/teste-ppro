-- =====================================================
-- Migration 062 — Fase 7: Documentos e Data Rooms
-- documents, data_rooms, data_room_folders,
-- data_room_folder_documents
-- Data: 2026-03-16
-- =====================================================

SET @cli  = 'DEMO0001-0000-4000-A000-000000000001';
SET @co1  = 'DEMO0001-0000-4000-A000-000000000010'; -- InovaTech
SET @co2  = 'DEMO0001-0000-4000-A000-000000000020'; -- VidaSaúde
SET @co3  = 'DEMO0001-0000-4000-A000-000000000030'; -- FinanGrow
SET @u01  = 'DEMO0001-0000-4000-A000-000000000101'; -- Carlos
SET @u07  = 'DEMO0001-0000-4000-A000-000000000107'; -- João
SET @u09  = 'DEMO0001-0000-4000-A000-000000000109'; -- Pedro
SET @u13  = 'DEMO0001-0000-4000-A000-000000000113'; -- Rafael Cunha (Legal)
SET @u14  = 'DEMO0001-0000-4000-A000-000000000114'; -- Fernanda (Finance)

-- =====================================================
-- DATA ROOMS (1 por empresa)
-- =====================================================
INSERT IGNORE INTO data_rooms (id, company_id, name, description, is_active, created_by, updated_by)
VALUES
('DEMO0014-0001-4000-A000-000000000001', @co1,
 'Data Room InovaTech', 'Repositório seguro de documentos da InovaTech para due diligence e governança.', 1, @u01, @u01),
('DEMO0014-0002-4000-A000-000000000001', @co2,
 'Data Room VidaSaúde', 'Repositório seguro de documentos da VidaSaúde para investidores e parceiros.', 1, @u07, @u07),
('DEMO0014-0003-4000-A000-000000000001', @co3,
 'Data Room FinanGrow', 'Repositório seguro de documentos da FinanGrow para due diligence e parceiros.', 1, @u09, @u09);

-- =====================================================
-- DATA ROOM FOLDERS
-- =====================================================
INSERT IGNORE INTO data_room_folders
    (id, data_room_id, parent_id, name, description, display_order, visibility, created_by, updated_by)
VALUES
-- InovaTech (4 pastas)
('DEMO0015-0001-4000-A000-000000000001', 'DEMO0014-0001-4000-A000-000000000001', NULL,
 'Jurídico', 'Contratos, acordos societários e documentos legais.', 1, 'investors', @u01, @u01),
('DEMO0015-0001-4000-A000-000000000002', 'DEMO0014-0001-4000-A000-000000000001', NULL,
 'Financeiro', 'Relatórios financeiros, DREs e demonstrações.', 2, 'investors', @u14, @u01),
('DEMO0015-0001-4000-A000-000000000003', 'DEMO0014-0001-4000-A000-000000000001', NULL,
 'Produto & Tech', 'Roadmaps, arquitetura técnica e documentação do produto.', 3, 'internal', @u01, @u01),
('DEMO0015-0001-4000-A000-000000000004', 'DEMO0014-0001-4000-A000-000000000001', NULL,
 'Cap Table & Vesting', 'Tabela de capitalização, acordos de vesting e grants.', 4, 'investors', @u01, @u01),
-- VidaSaúde (3 pastas)
('DEMO0015-0002-4000-A000-000000000001', 'DEMO0014-0002-4000-A000-000000000001', NULL,
 'Jurídico', 'Contratos, NDA e documentos regulatórios ANVISA.', 1, 'investors', @u07, @u07),
('DEMO0015-0002-4000-A000-000000000002', 'DEMO0014-0002-4000-A000-000000000001', NULL,
 'Financeiro', 'Relatórios financeiros e métricas operacionais.', 2, 'investors', @u07, @u07),
('DEMO0015-0002-4000-A000-000000000003', 'DEMO0014-0002-4000-A000-000000000001', NULL,
 'Clínico & Regulatório', 'Documentos clínicos, aprovações e pareceres médicos.', 3, 'internal', @u07, @u07),
-- FinanGrow (2 pastas)
('DEMO0015-0003-4000-A000-000000000001', 'DEMO0014-0003-4000-A000-000000000001', NULL,
 'Jurídico', 'Contratos, SHA e documentos societários.', 1, 'investors', @u09, @u09),
('DEMO0015-0003-4000-A000-000000000002', 'DEMO0014-0003-4000-A000-000000000001', NULL,
 'Financeiro', 'Relatórios financeiros, projeções e métricas.', 2, 'investors', @u14, @u09);

-- =====================================================
-- DOCUMENTS
-- =====================================================
INSERT IGNORE INTO documents
    (id, client_id, company_id, name, document_type, description,
     file_name, file_size_bytes, mime_type, storage_path,
     entity_type, entity_id, visibility,
     is_verified, verified_at, verified_by,
     created_by, updated_by)
VALUES
-- === InovaTech — Jurídico ===
('DEMO0012-0001-4000-A000-000000000001', @cli, @co1,
 'Acordo de Acionistas — InovaTech 2024',
 'contract', 'Acordo de Acionistas atualizado após Series A.',
 'SHA_InovaTech_SeriesA_2024.pdf', 524288, 'application/pdf',
 '/dataroom/inovatech/juridico/SHA_InovaTech_SeriesA_2024.pdf',
 'contract', 'DEMO0010-0001-4000-A000-000000000001', 'investors',
 1, '2024-09-10 14:00:00', @u13, @u13, @u01),

('DEMO0012-0001-4000-A000-000000000002', @cli, @co1,
 'NDA — Fundo Aceleração Brasil',
 'contract', 'Acordo de confidencialidade com Fundo Aceleração Brasil para due diligence.',
 'NDA_FundoAceleracao_2023.pdf', 131072, 'application/pdf',
 '/dataroom/inovatech/juridico/NDA_FundoAceleracao_2023.pdf',
 'contract', 'DEMO0010-0001-4000-A000-000000000002', 'investors',
 1, '2023-09-02 10:00:00', @u13, @u13, @u01),

-- === InovaTech — Financeiro ===
('DEMO0012-0001-4000-A000-000000000003', @cli, @co1,
 'DRE Consolidado 2025 — InovaTech',
 'audit_report', 'DRE Consolidado 2025 — InovaTech auditada.',
 'DRE_InovaTech_2025_Auditada.pdf', 786432, 'application/pdf',
 '/dataroom/inovatech/financeiro/DRE_InovaTech_2025_Auditada.pdf',
 'company', @co1, 'investors',
 1, '2026-02-15 10:00:00', @u14, @u14, @u01),

('DEMO0012-0001-4000-A000-000000000004', @cli, @co1,
 'Projeções Financeiras 2026–2028 — InovaTech',
 'income_statement', 'Modelo financeiro com três cenários (base, otimista, pessimista) para Series B.',
 'Projecoes_InovaTech_2026_2028.xlsx', 204800, 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
 '/dataroom/inovatech/financeiro/Projecoes_InovaTech_2026_2028.xlsx',
 'company', @co1, 'investors',
 1, '2026-01-20 14:00:00', @u14, @u14, @u01),

-- === InovaTech — Produto & Tech ===
('DEMO0012-0001-4000-A000-000000000005', @cli, @co1,
 'Roadmap de Produto 2026 — InovaTech',
 'presentation', 'Roadmap detalhado Q1-Q4 2026 com épicos, milestones e métricas de sucesso.',
 'Roadmap_InovaTech_2026.pdf', 409600, 'application/pdf',
 '/dataroom/inovatech/produto/Roadmap_InovaTech_2026.pdf',
 'company', @co1, 'board',
 0, NULL, NULL, @u01, @u01),

-- === InovaTech — Cap Table ===
('DEMO0012-0001-4000-A000-000000000006', @cli, @co1,
 'Cap Table InovaTech — Março 2026',
 'other', 'Tabela de capitalização atualizada pós-Series A com todos os shareholders e percentuais.',
 'CapTable_InovaTech_Mar2026.xlsx', 163840, 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
 '/dataroom/inovatech/captable/CapTable_InovaTech_Mar2026.xlsx',
 'company', @co1, 'investors',
 1, '2026-03-01 09:00:00', @u14, @u01, @u01),

-- === VidaSaúde — Jurídico ===
('DEMO0012-0002-4000-A000-000000000001', @cli, @co2,
 'Acordo de Acionistas — VidaSaúde 2023',
 'contract', 'SHA vigente da VidaSaúde com Marina Torres Health Fund.',
 'SHA_VidaSaude_Seed_2023.pdf', 483328, 'application/pdf',
 '/dataroom/vidasaude/juridico/SHA_VidaSaude_Seed_2023.pdf',
 'contract', 'DEMO0010-0002-4000-A000-000000000001', 'investors',
 1, '2023-04-05 14:00:00', @u13, @u13, @u07),

('DEMO0012-0002-4000-A000-000000000002', @cli, @co2,
 'Licença ANVISA — Plataforma VidaSaúde',
 'certificate', 'Licença de operação de software para saúde emitida pela ANVISA em 2024.',
 'Licenca_ANVISA_VidaSaude_2024.pdf', 245760, 'application/pdf',
 '/dataroom/vidasaude/juridico/Licenca_ANVISA_VidaSaude_2024.pdf',
 'company', @co2, 'investors',
 1, '2024-03-20 10:00:00', @u07, @u07, @u07),

-- === VidaSaúde — Financeiro ===
('DEMO0012-0002-4000-A000-000000000003', @cli, @co2,
 'Relatório Financeiro Q4 2025 — VidaSaúde',
 'audit_report', 'Resultados do quarto trimestre de 2025 com análise de churn e LTV.',
 'Relatorio_Q4_2025_VidaSaude.pdf', 348160, 'application/pdf',
 '/dataroom/vidasaude/financeiro/Relatorio_Q4_2025_VidaSaude.pdf',
 'company', @co2, 'investors',
 1, '2026-01-12 09:00:00', @u14, @u07, @u07),

-- === VidaSaúde — Clínico ===
('DEMO0012-0002-4000-A000-000000000004', @cli, @co2,
 'Protocolo Clínico IA — Parceria Hospital São Lucas',
 'other', 'Protocolo aprovado para uso do módulo de triagem por IA no Hospital São Lucas — BH.',
 'Protocolo_IA_HospSaoLucas_2025.pdf', 655360, 'application/pdf',
 '/dataroom/vidasaude/clinico/Protocolo_IA_HospSaoLucas_2025.pdf',
 'company', @co2, 'board',
 1, '2025-09-15 11:00:00', @u07, @u07, @u07),

-- === FinanGrow — Jurídico ===
('DEMO0012-0003-4000-A000-000000000001', @cli, @co3,
 'SAFE — Fundo Aceleração Brasil FIP',
 'contract', 'Simple Agreement for Future Equity convertido em mai/2024 (R$450k).',
 'SAFE_FinanGrow_FundoAceleracao_2024.pdf', 393216, 'application/pdf',
 '/dataroom/finangrow/juridico/SAFE_FinanGrow_FundoAceleracao_2024.pdf',
 'contract', 'DEMO0010-0003-4000-A000-000000000002', 'investors',
 1, '2024-05-05 14:00:00', @u13, @u13, @u09),

('DEMO0012-0003-4000-A000-000000000002', @cli, @co3,
 'Acordo de Acionistas — FinanGrow 2024',
 'contract', 'SHA pós-conversão SAFE com cláusulas de drag-along e preferência.',
 'SHA_FinanGrow_2024.pdf', 458752, 'application/pdf',
 '/dataroom/finangrow/juridico/SHA_FinanGrow_2024.pdf',
 'contract', 'DEMO0010-0003-4000-A000-000000000001', 'investors',
 1, '2024-05-20 10:00:00', @u13, @u13, @u09),

-- === FinanGrow — Financeiro ===
('DEMO0012-0003-4000-A000-000000000003', @cli, @co3,
 'Projeções para Seed Round — FinanGrow 2026',
 'income_statement', 'Modelo financeiro para captação Seed de R$1.5M previsto para Q3 2026.',
 'Projecoes_Seed_FinanGrow_2026.xlsx', 286720, 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
 '/dataroom/finangrow/financeiro/Projecoes_Seed_FinanGrow_2026.xlsx',
 'company', @co3, 'investors',
 0, NULL, NULL, @u14, @u09),

('DEMO0012-0003-4000-A000-000000000004', @cli, @co3,
 'Métricas Unit Economics — FinanGrow H2/2025',
 'audit_report', 'Análise de LTV, CAC, payback e cohorts do segundo semestre de 2025.',
 'UnitEconomics_FinanGrow_H2_2025.pdf', 229376, 'application/pdf',
 '/dataroom/finangrow/financeiro/UnitEconomics_FinanGrow_H2_2025.pdf',
 'company', @co3, 'investors',
 1, '2026-01-20 10:00:00', @u14, @u14, @u09);

-- =====================================================
-- DATA ROOM FOLDER DOCUMENTS (liga docs às pastas)
-- =====================================================
INSERT IGNORE INTO data_room_folder_documents
    (id, folder_id, document_id, display_order, added_by)
VALUES
-- InovaTech: Jurídico
('DEMO0016-0001-4000-A000-000000000001','DEMO0015-0001-4000-A000-000000000001','DEMO0012-0001-4000-A000-000000000001',1,@u01),
('DEMO0016-0001-4000-A000-000000000002','DEMO0015-0001-4000-A000-000000000001','DEMO0012-0001-4000-A000-000000000002',2,@u01),
-- InovaTech: Financeiro
('DEMO0016-0001-4000-A000-000000000003','DEMO0015-0001-4000-A000-000000000002','DEMO0012-0001-4000-A000-000000000003',1,@u01),
('DEMO0016-0001-4000-A000-000000000004','DEMO0015-0001-4000-A000-000000000002','DEMO0012-0001-4000-A000-000000000004',2,@u01),
-- InovaTech: Produto
('DEMO0016-0001-4000-A000-000000000005','DEMO0015-0001-4000-A000-000000000003','DEMO0012-0001-4000-A000-000000000005',1,@u01),
-- InovaTech: Cap Table
('DEMO0016-0001-4000-A000-000000000006','DEMO0015-0001-4000-A000-000000000004','DEMO0012-0001-4000-A000-000000000006',1,@u01),
-- VidaSaúde: Jurídico
('DEMO0016-0002-4000-A000-000000000001','DEMO0015-0002-4000-A000-000000000001','DEMO0012-0002-4000-A000-000000000001',1,@u07),
('DEMO0016-0002-4000-A000-000000000002','DEMO0015-0002-4000-A000-000000000001','DEMO0012-0002-4000-A000-000000000002',2,@u07),
-- VidaSaúde: Financeiro
('DEMO0016-0002-4000-A000-000000000003','DEMO0015-0002-4000-A000-000000000002','DEMO0012-0002-4000-A000-000000000003',1,@u07),
-- VidaSaúde: Clínico
('DEMO0016-0002-4000-A000-000000000004','DEMO0015-0002-4000-A000-000000000003','DEMO0012-0002-4000-A000-000000000004',1,@u07),
-- FinanGrow: Jurídico
('DEMO0016-0003-4000-A000-000000000001','DEMO0015-0003-4000-A000-000000000001','DEMO0012-0003-4000-A000-000000000001',1,@u09),
('DEMO0016-0003-4000-A000-000000000002','DEMO0015-0003-4000-A000-000000000001','DEMO0012-0003-4000-A000-000000000002',2,@u09),
-- FinanGrow: Financeiro
('DEMO0016-0003-4000-A000-000000000003','DEMO0015-0003-4000-A000-000000000002','DEMO0012-0003-4000-A000-000000000003',1,@u09),
('DEMO0016-0003-4000-A000-000000000004','DEMO0015-0003-4000-A000-000000000002','DEMO0012-0003-4000-A000-000000000004',2,@u09);

-- Migration 062 Fase 7 Documentos + DataRoom — fim
