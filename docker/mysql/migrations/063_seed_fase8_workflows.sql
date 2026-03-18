-- =====================================================
-- Migration 063 — Fase 8: Workflows
-- workflows, workflow_steps, workflow_approvals
-- Data: 2026-03-16
-- =====================================================

SET @co1  = 'DEMO0001-0000-4000-A000-000000000010'; -- InovaTech
SET @co2  = 'DEMO0001-0000-4000-A000-000000000020'; -- VidaSaúde
SET @co3  = 'DEMO0001-0000-4000-A000-000000000030'; -- FinanGrow
SET @u01  = 'DEMO0001-0000-4000-A000-000000000101'; -- Carlos Silva
SET @u02  = 'DEMO0001-0000-4000-A000-000000000102'; -- Ana Santos
SET @u07  = 'DEMO0001-0000-4000-A000-000000000107'; -- João Ferreira
SET @u08  = 'DEMO0001-0000-4000-A000-000000000108'; -- Mariana Oliveira
SET @u09  = 'DEMO0001-0000-4000-A000-000000000109'; -- Pedro Alves
SET @u10  = 'DEMO0001-0000-4000-A000-000000000110'; -- Lúcia Mendes
SET @u11  = 'DEMO0001-0000-4000-A000-000000000111'; -- Dr. Marcos (Board)
SET @u13  = 'DEMO0001-0000-4000-A000-000000000113'; -- Rafael Cunha (Legal)
SET @u14  = 'DEMO0001-0000-4000-A000-000000000114'; -- Fernanda Lima (Finance)
SET @u15  = 'DEMO0001-0000-4000-A000-000000000115'; -- JP Angel

-- =====================================================
-- WORKFLOWS (2 por empresa: 1 aprovado + 1 em andamento)
-- =====================================================
INSERT IGNORE INTO workflows
    (id, company_id, workflow_type, reference_type, reference_id,
     title, description, status, priority,
     current_step, total_steps,
     requested_by, requested_at, due_date, completed_at,
     created_by, updated_by)
VALUES
-- === InovaTech W1: Aprovação de Contrato SHA (aprovado) ===
('DEMO0017-0001-4000-A000-000000000001', @co1,
 'contract_approval', 'contract', 'DEMO0010-0001-4000-A000-000000000001',
 'Aprovação SHA — InovaTech Series A',
 'Workflow de aprovação do Acordo de Acionistas atualizado após Series A.',
 'approved', 'high', 3, 3,
 @u01, '2024-09-01 09:00:00', '2024-09-07 18:00:00', '2024-09-06 14:30:00',
 @u01, @u01),

-- === InovaTech W2: Mudança de Acionista (em andamento) ===
('DEMO0017-0001-4000-A000-000000000002', @co1,
 'shareholder_change', 'shareholder', 'DEMO0003-0000-4000-A000-000000000106',
 'Atualização Participação Roberto Lima — ESOP Exercise',
 'Workflow para registrar exercício de opções do ESOP por Roberto Lima.',
 'in_progress', 'medium', 2, 3,
 @u01, '2026-03-10 10:00:00', '2026-03-25 18:00:00', NULL,
 @u01, @u01),

-- === VidaSaúde W3: Aprovação Comunicação (aprovado) ===
('DEMO0017-0002-4000-A000-000000000001', @co2,
 'communication_approval', 'communication', 'DEMO0013-0002-4000-A000-000000000001',
 'Aprovação Comunicado Seed Round — VidaSaúde',
 'Aprovação do anúncio de fechamento do Seed Round para publicação.',
 'approved', 'high', 2, 2,
 @u07, '2023-03-24 08:00:00', '2023-03-25 12:00:00', '2023-03-25 09:15:00',
 @u07, @u07),

-- === VidaSaúde W4: Aprovação Vesting (em andamento) ===
('DEMO0017-0002-4000-A000-000000000002', @co2,
 'vesting_approval', 'vesting_grant', 'DEMO0007-0000-4000-A000-000000000004',
 'Aprovação Vesting Grant — Mariana Oliveira',
 'Aprovação do grant de vesting para Mariana Oliveira, co-fundadora da VidaSaúde.',
 'in_progress', 'medium', 1, 2,
 @u07, '2026-03-12 09:00:00', '2026-03-30 18:00:00', NULL,
 @u07, @u07),

-- === FinanGrow W5: Verificação Documento (aprovado) ===
('DEMO0017-0003-4000-A000-000000000001', @co3,
 'document_verification', 'document', 'DEMO0012-0003-4000-A000-000000000001',
 'Verificação SAFE — FinanGrow / Fundo Aceleração Brasil',
 'Verificação jurídica do SAFE antes da conversão oficial.',
 'approved', 'urgent', 2, 2,
 @u09, '2024-04-28 10:00:00', '2024-05-03 18:00:00', '2024-05-02 16:00:00',
 @u09, @u09),

-- === FinanGrow W6: Aprovação Contrato (em andamento) ===
('DEMO0017-0003-4000-A000-000000000002', @co3,
 'contract_approval', 'contract', 'DEMO0010-0003-4000-A000-000000000001',
 'Revisão SHA FinanGrow — Atualização Seed Round',
 'Workflow de revisão e aprovação das alterações no SHA para preparação do Seed Round.',
 'in_progress', 'high', 1, 3,
 @u09, '2026-03-05 09:00:00', '2026-03-31 18:00:00', NULL,
 @u09, @u09);

-- =====================================================
-- WORKFLOW STEPS
-- =====================================================
INSERT IGNORE INTO workflow_steps
    (id, workflow_id, step_order, name, description,
     step_type, assigned_role, assigned_user_id,
     status, is_current,
     started_at, due_date, completed_at, completed_by, notes)
VALUES
-- === W1 InovaTech SHA (aprovado — 3 steps) ===
('DEMO0018-0001-4000-A000-000000000001','DEMO0017-0001-4000-A000-000000000001',
 1,'Revisão Jurídica','Rafael Cunha revisa o SHA atualizado.',
 'review','Legal',@u13,'completed',0,
 '2024-09-01 10:00:00','2024-09-03 18:00:00','2024-09-03 15:00:00',@u13,
 'Documentação completa. Cláusulas de drag-along e tag-along revisadas e aprovadas.'),

('DEMO0018-0001-4000-A000-000000000002','DEMO0017-0001-4000-A000-000000000001',
 2,'Aprovação Board','Dr. Marcos aprova o SHA pelo Conselho.',
 'approval','BoardMember',@u11,'completed',0,
 '2024-09-03 16:00:00','2024-09-05 18:00:00','2024-09-05 11:00:00',@u11,
 'Board aprova por unanimidade.'),

('DEMO0018-0001-4000-A000-000000000003','DEMO0017-0001-4000-A000-000000000001',
 3,'Notificação Sócios','Notificar todos os sócios sobre o SHA aprovado.',
 'notification','Admin',@u01,'completed',0,
 '2024-09-05 11:30:00','2024-09-06 18:00:00','2024-09-06 14:30:00',@u01,
 'Todos os sócios notificados por e-mail.'),

-- === W2 InovaTech ESOP (em andamento — 3 steps) ===
('DEMO0018-0001-4000-A000-000000000004','DEMO0017-0001-4000-A000-000000000002',
 1,'Validação Jurídica ESOP','Validar exercício de opções conforme plano de stock option.',
 'review','Legal',@u13,'completed',0,
 '2026-03-10 11:00:00','2026-03-15 18:00:00','2026-03-14 15:00:00',@u13,
 'Exercício dentro dos termos do plano ESOP. Opções elegíveis: 50.000.'),

('DEMO0018-0001-4000-A000-000000000005','DEMO0017-0001-4000-A000-000000000002',
 2,'Aprovação Financeira','Fernanda Lima verifica e aprova o lançamento financeiro.',
 'approval','Finance',@u14,'in_progress',1,
 '2026-03-14 16:00:00','2026-03-20 18:00:00',NULL,NULL,NULL),

('DEMO0018-0001-4000-A000-000000000006','DEMO0017-0001-4000-A000-000000000002',
 3,'Atualização Cap Table','Atualizar a tabela de capitalização após exercício.',
 'automated',NULL,NULL,'pending',0,
 NULL,NULL,NULL,NULL,NULL),

-- === W3 VidaSaúde Comunicação (aprovado — 2 steps) ===
('DEMO0018-0002-4000-A000-000000000001','DEMO0017-0002-4000-A000-000000000001',
 1,'Revisão do Conteúdo','João revisa e aprova o texto do comunicado.',
 'review','Founder',@u07,'completed',0,
 '2023-03-24 09:00:00','2023-03-25 12:00:00','2023-03-24 17:00:00',@u07,
 'Conteúdo aprovado. Pequenos ajustes de tom realizados.'),

('DEMO0018-0002-4000-A000-000000000002','DEMO0017-0002-4000-A000-000000000001',
 2,'Publicação do Comunicado','Publicar o comunicado no portal de investidores.',
 'approval','Admin',@u07,'completed',0,
 '2023-03-24 17:30:00','2023-03-25 12:00:00','2023-03-25 09:15:00',@u07,
 'Comunicado publicado com sucesso.'),

-- === W4 VidaSaúde Vesting (em andamento — 2 steps) ===
('DEMO0018-0002-4000-A000-000000000003','DEMO0017-0002-4000-A000-000000000002',
 1,'Aprovação do Grant Vesting','João Ferreira aprova o grant de vesting de Mariana.',
 'approval','Founder',@u07,'in_progress',1,
 '2026-03-12 10:00:00','2026-03-22 18:00:00',NULL,NULL,NULL),

('DEMO0018-0002-4000-A000-000000000004','DEMO0017-0002-4000-A000-000000000002',
 2,'Notificação Beneficiária','Notificar Mariana Oliveira sobre aprovação do grant.',
 'notification','Admin',@u07,'pending',0,
 NULL,NULL,NULL,NULL,NULL),

-- === W5 FinanGrow Documento (aprovado — 2 steps) ===
('DEMO0018-0003-4000-A000-000000000001','DEMO0017-0003-4000-A000-000000000001',
 1,'Revisão Jurídica SAFE','Rafael Cunha verifica a estrutura legal do SAFE.',
 'review','Legal',@u13,'completed',0,
 '2024-04-28 11:00:00','2024-05-01 18:00:00','2024-04-30 16:00:00',@u13,
 'SAFE estruturado corretamente. Capital cap e desconto validados.'),

('DEMO0018-0003-4000-A000-000000000002','DEMO0017-0003-4000-A000-000000000001',
 2,'Aprovação Final Pedro','Pedro Alves aprova e autoriza conversão.',
 'approval','Founder',@u09,'completed',0,
 '2024-04-30 17:00:00','2024-05-03 18:00:00','2024-05-02 16:00:00',@u09,
 'Conversão aprovada. SAFE converte a R$2.55M pré-money.'),

-- === W6 FinanGrow SHA Seed (em andamento — 3 steps) ===
('DEMO0018-0003-4000-A000-000000000003','DEMO0017-0003-4000-A000-000000000002',
 1,'Revisão Alterações SHA','Rafael Cunha revisa todas as cláusulas alteradas.',
 'review','Legal',@u13,'in_progress',1,
 '2026-03-05 10:00:00','2026-03-20 18:00:00',NULL,NULL,NULL),

('DEMO0018-0003-4000-A000-000000000004','DEMO0017-0003-4000-A000-000000000002',
 2,'Aprovação Sócios','Pedro e Lúcia aprovam as alterações do SHA.',
 'approval','Founder',@u09,'pending',0,
 NULL,NULL,NULL,NULL,NULL),

('DEMO0018-0003-4000-A000-000000000005','DEMO0017-0003-4000-A000-000000000002',
 3,'Aprovação Investidor','Fundo Aceleração Brasil aprova as alterações.',
 'approval','Investor',@u15,'pending',0,
 NULL,NULL,NULL,NULL,NULL);

-- =====================================================
-- WORKFLOW APPROVALS (apenas steps aprovados)
-- =====================================================
INSERT IGNORE INTO workflow_approvals
    (id, workflow_step_id, user_id, decision, comments, decided_at, ip_address)
VALUES
-- W1 InovaTech SHA
('DEMO0019-0001-4000-A000-000000000001','DEMO0018-0001-4000-A000-000000000001',@u13,
 'approved','Documentação completa e cláusulas bem estruturadas.','2024-09-03 15:00:00','10.0.0.1'),
('DEMO0019-0001-4000-A000-000000000002','DEMO0018-0001-4000-A000-000000000002',@u11,
 'approved','SHA aprovado pelo Conselho por unanimidade em reunião.','2024-09-05 11:00:00','10.0.0.1'),
-- W1 step 4 (Roberto ESOP review)
('DEMO0019-0001-4000-A000-000000000003','DEMO0018-0001-4000-A000-000000000004',@u13,
 'approved','Exercício dentro dos termos. 50.000 opções elegíveis confirmadas.','2026-03-14 15:00:00','10.0.0.2'),
-- W3 VidaSaúde Comunicação
('DEMO0019-0002-4000-A000-000000000001','DEMO0018-0002-4000-A000-000000000001',@u07,
 'approved','Conteúdo aprovado após ajustes de tom.','2023-03-24 17:00:00','10.0.0.3'),
('DEMO0019-0002-4000-A000-000000000002','DEMO0018-0002-4000-A000-000000000002',@u07,
 'approved','Publicado com sucesso no portal.','2023-03-25 09:15:00','10.0.0.3'),
-- W5 FinanGrow SAFE
('DEMO0019-0003-4000-A000-000000000001','DEMO0018-0003-4000-A000-000000000001',@u13,
 'approved','SAFE estruturado corretamente. Conversão autorizada.','2024-04-30 16:00:00','10.0.0.4'),
('DEMO0019-0003-4000-A000-000000000002','DEMO0018-0003-4000-A000-000000000002',@u09,
 'approved','Conversão aprovada. R$450k a R$2.55M pré-money.','2024-05-02 16:00:00','10.0.0.4');

-- Migration 063 Fase 8 Workflows — fim
