-- =====================================================
-- Migration 061 — Fase 6: Comunicações
-- Communications (company_id, sem client_id)
-- Data: 2026-03-16
-- =====================================================

SET @co1  = 'DEMO0001-0000-4000-A000-000000000010'; -- InovaTech
SET @co2  = 'DEMO0001-0000-4000-A000-000000000020'; -- VidaSaúde
SET @co3  = 'DEMO0001-0000-4000-A000-000000000030'; -- FinanGrow
SET @u01  = 'DEMO0001-0000-4000-A000-000000000101'; -- Carlos Silva
SET @u07  = 'DEMO0001-0000-4000-A000-000000000107'; -- João Ferreira
SET @u09  = 'DEMO0001-0000-4000-A000-000000000109'; -- Pedro Alves
SET @u11  = 'DEMO0001-0000-4000-A000-000000000111'; -- Dr. Marcos (Board)
SET @u14  = 'DEMO0001-0000-4000-A000-000000000114'; -- Fernanda Lima

-- =====================================================
-- COMMUNICATIONS
-- =====================================================
INSERT IGNORE INTO communications
    (id, company_id, title, content, content_html, summary,
     comm_type, visibility, target_roles,
     is_pinned, published_at, expires_at,
     created_by, updated_by)
VALUES
-- === InovaTech (4 comunicações) ===
('DEMO0013-0001-4000-A000-000000000001', @co1,
 'InovaTech conclui Series A de R$2,25 milhões',
 'Temos o prazer de anunciar que a InovaTech concluiu com sucesso o Series A no valor de R$2.25 milhões, liderado pelo Fundo Aceleração Brasil com follow-on de JP Angel Investimentos. Os recursos serão utilizados para expansão da equipe de produto e go-to-market enterprise. Esta é uma etapa importantíssima na nossa jornada. Agradecemos a todos os sócios, investidores e colaboradores pela confiança.',
 '<h2>InovaTech conclui Series A de R$2,25 milhões</h2><p>Temos o prazer de anunciar que a InovaTech concluiu com sucesso o Series A no valor de R$2.25 milhões, liderado pelo <strong>Fundo Aceleração Brasil</strong> com follow-on de <strong>JP Angel Investimentos</strong>.</p><p>Os recursos serão utilizados para expansão da equipe de produto e go-to-market enterprise.</p>',
 'Series A de R$2.25M concluído. Expansão de time e go-to-market enterprise.',
 'announcement', 'all', '["Admin","Founder","Investor","BoardMember"]',
 1, '2024-09-05 09:00:00', NULL, @u01, @u01),

('DEMO0013-0001-4000-A000-000000000002', @co1,
 'Relatório Trimestral Q4 2025 — InovaTech',
 'Prezados investidores e parceiros,\n\nApresentamos os resultados do Q4 2025:\n\n• MRR: R$240.300 (+11% QoQ)\n• ARR: R$1.199.000 (marco de R$1M ARR atingido!)\n• Clientes ativos: 963\n• Churn: 1,3%\n• EBITDA: R$78.000 (positivo pelo 3o mês consecutivo)\n• Runway: 18+ meses\n\n2025 foi um ano transformador. Entramos no ano com MRR de R$132k e encerramos em R$240k — crescimento de 81% no MRR anual. Para 2026, nosso foco é Series B e mercado enterprise.',
 '<h2>Relatório Trimestral Q4 2025</h2><ul><li><strong>MRR:</strong> R$240.300 (+11% QoQ)</li><li><strong>ARR:</strong> R$1.199.000</li><li><strong>Clientes:</strong> 963</li><li><strong>Churn:</strong> 1,3%</li><li><strong>EBITDA:</strong> R$78.000 ✅</li></ul>',
 'Q4 2025: MRR R$240k, ARR R$1.19M, EBITDA positivo.',
 'report', 'investors', '["Admin","Founder","Investor","Finance","BoardMember"]',
 0, '2026-01-10 09:00:00', NULL, @u14, @u01),

('DEMO0013-0001-4000-A000-000000000003', @co1,
 'Roadmap do Produto InovaTech — Q1/Q2 2026',
 'Com o Series A fechado, definimos o roadmap para os próximos 6 meses:\n\nQ1 2026:\n- Launch módulo de Analytics Enterprise\n- Integração com Salesforce e HubSpot\n- Onboarding automatizado\n\nQ2 2026:\n- API pública v2.0\n- Marketplace de integrações\n- Preparação para expansão LatAm\n\nEstamos contratando: 3 Engenheiros Sênior, 1 Product Manager, 2 Account Executives.',
 '<h2>Roadmap 2026</h2><h3>Q1 2026</h3><ul><li>Analytics Enterprise</li><li>Integrações Salesforce/HubSpot</li></ul><h3>Q2 2026</h3><ul><li>API pública v2.0</li><li>Marketplace</li></ul>',
 'Roadmap Q1/Q2 2026: Analytics Enterprise, API v2.0, expansão LatAm.',
 'update', 'all', NULL,
 0, '2026-01-20 10:00:00', NULL, @u01, @u01),

('DEMO0013-0001-4000-A000-000000000004', @co1,
 'Reunião do Conselho — 20 de Março 2026',
 'Convocamos todos os membros do conselho para a reunião trimestral:\n\nData: 20/03/2026 às 14h00\nLocal: Av. Paulista, 1374 — Sala Conselho\nPauta: Aprovação balanço Q4 2025, Budget 2026, preparação Series B, decisão sobre expansão LatAm.\n\nPresença obrigatória. Confirmar até 17/03/2026.',
 '<h2>Reunião do Conselho</h2><p><strong>Data:</strong> 20/03/2026 14h00</p><p><strong>Pauta:</strong> Aprovação Q4, Budget 2026, Series B.</p>',
 'Reunião do conselho em 20/03/2026. Pauta: Q4 2025 e Series B.',
 'alert', 'specific', '["Admin","Founder","BoardMember"]',
 1, '2026-03-10 08:00:00', '2026-03-20 14:00:00', @u01, @u01),

-- === VidaSaúde (4 comunicações) ===
('DEMO0013-0002-4000-A000-000000000001', @co2,
 'VidaSaúde fecha Seed Round com Marina Torres Health Fund',
 'É com grande satisfação que anunciamos o fechamento do nosso Seed Round de R$2 milhões com Marina Torres Health Fund. Este investimento nos permitirá:\n\n• Expandir a equipe médica e técnica\n• Lançar o módulo de telemedicina premium\n• Iniciar parcerias com hospitais em MG e SP\n• Dobrar nossa base de usuários até 2024\n\nAgradecemos a todos os nossos usuários, parceiros e apoiadores desta jornada.',
 '<h2>Seed Round Fechado!</h2><p>R$2 milhões com Marina Torres Health Fund para expandir a plataforma de saúde.</p>',
 'Seed Round R$2M fechado com Marina Torres Health Fund.',
 'announcement', 'all', NULL,
 1, '2023-03-25 09:00:00', NULL, @u07, @u07),

('DEMO0013-0002-4000-A000-000000000002', @co2,
 'Atualização de Produto — Módulo IA para Triagem Médica',
 'Estamos avançando no desenvolvimento do módulo de triagem por Inteligência Artificial em parceria com DataHealth Tecnologia:\n\nStatus atual:\n✅ Milestone 1: Análise de dados e modelo base — Concluído\n✅ Milestone 2: Integração com prontuário eletrônico — Concluído\n🔄 Milestone 3: Testes clínicos no Hospital São Lucas — Em andamento (70%)\n⏳ Milestone 4: Validação ANVISA e lançamento — Q2 2026\n\nEstimativa: lançamento beta em maio 2026.',
 '<h2>IA para Triagem Médica</h2><p>Milestone 3 em andamento — 70% completo. Lançamento beta maio 2026.</p>',
 'Módulo de IA: Milestone 3 em andamento (70%). Lançamento beta mai/2026.',
 'update', 'all', NULL,
 0, '2026-01-15 10:00:00', NULL, @u07, @u07),

('DEMO0013-0002-4000-A000-000000000003', @co2,
 'Relatório KPIs Q3 2025 — VidaSaúde',
 'Resultados do terceiro trimestre de 2025:\n\nUsuários Ativos: 42.300 (+18% QoQ)\nMRR: R$58.000 (+19% QoQ)\nNPS: 54\nParcerias ativas: 4 hospitais, 12 clínicas\nChurn: 2,4%\n\nDestaque: Parceria com Hospital São Lucas gerou 3.200 novos usuários orgânicos no trimestre. Prevemos atingir 50k usuários até o final de 2025.',
 '<h2>KPIs Q3 2025</h2><ul><li>Usuários: 42.300 (+18%)</li><li>MRR: R$58.000</li><li>NPS: 54</li></ul>',
 'Q3 2025: 42k usuários, MRR R$58k, NPS 54.',
 'report', 'investors', '["Admin","Founder","Investor","Finance","BoardMember"]',
 0, '2025-10-10 09:00:00', NULL, @u07, @u07),

('DEMO0013-0002-4000-A000-000000000004', @co2,
 'Demo Day VidaSaúde — Convite Investidores',
 'Convidamos todos os nossos investidores e parceiros estratégicos para o VidaSaúde Demo Day:\n\nData: 15 de Abril de 2026\nHorário: 18h00 às 21h00\nLocal: BH Tech Hub — Rua dos Inconfidentes, 921, Belo Horizonte\n\nApresentaremos:\n- Demonstração ao vivo do módulo de IA\n- Resultados 2025 e projeções 2026\n- Roadmap para Series A\n\nVagas limitadas. Confirme presença até 10/04/2026.',
 '<h2>Demo Day VidaSaúde</h2><p>15/04/2026 | 18h | BH Tech Hub</p>',
 'Demo Day em 15/04/2026. Demonstração IA, resultados 2025, roadmap Series A.',
 'invitation', 'investors', '["Admin","Founder","Investor","BoardMember"]',
 1, '2026-03-01 09:00:00', '2026-04-15 21:00:00', @u07, @u07),

-- === FinanGrow (4 comunicações) ===
('DEMO0013-0003-4000-A000-000000000001', @co3,
 'FinanGrow converte SAFE — Pre-Seed Fechado!',
 'Com muita satisfação anunciamos a conversão do nosso SAFE e o fechamento da rodada Pre-Seed:\n\nValor convertido: R$450.000\nAvaliação pré-money: R$2.55 milhões\nInvestidor: Fundo Aceleração Brasil FIP\nData de conversão: 1o de maio de 2024\n\nCom esta captação, vamos acelerar o desenvolvimento do produto e conquistar nossos primeiros 500 clientes corporativos pagantes. Obrigado a todos que acreditaram desde o início!',
 '<h2>Pre-Seed Fechado!</h2><p>R$450k convertido. Fundo Aceleração Brasil. Avaliação R$3M pós-money.</p>',
 'SAFE convertido. Pre-Seed R$450k com Fundo Aceleração Brasil.',
 'announcement', 'all', NULL,
 1, '2024-05-05 09:00:00', NULL, @u09, @u09),

('DEMO0013-0003-4000-A000-000000000002', @co3,
 'Progresso do Produto — Plataforma de Gestão Financeira v2.0',
 'Atualização do desenvolvimento da plataforma FinanGrow v2.0:\n\nConcluído (Q3/Q4 2025):\n✅ Motor de conciliação bancária automática\n✅ Dashboard de fluxo de caixa em tempo real\n✅ Integração com 8 bancos via Open Finance\n✅ Módulo de gestão de cobranças\n\nEm desenvolvimento (Q1 2026):\n🔄 Módulo de folha de pagamento integrada\n🔄 Relatórios fiscais automatizados\n🔄 App mobile iOS/Android\n\nMeta: Lançamento v2.0 em junho/2026.',
 '<h2>Plataforma v2.0</h2><p>Conciliação bancária, Open Finance com 8 bancos. v2.0 em jun/2026.</p>',
 'v2.0 em andamento: Open Finance, conciliação automática, folha de pagamento.',
 'update', 'all', NULL,
 0, '2026-01-20 10:00:00', NULL, @u09, @u09),

('DEMO0013-0003-4000-A000-000000000003', @co3,
 'Métricas de Crescimento — Segundo Semestre 2025',
 'Resultados do H2 2025 (jul-dez/2025):\n\nClientes: de 320 para 420 (+31%)\nMRR: de R$9.300 para R$15.200 (+63%)\nARR Run Rate: R$182.400\nBurn Rate: R$38.500/mês\nRunway: 4,2 meses\nNPS: 38\nChurn: 3,5%\n\nAtividade comercial: 15 demos realizadas em dezembro, 4 contratos fechados. Pipeline atual: R$48k MRR potencial.\n\nPróximo objetivo: Seed Round em Q3 2026 com meta de R$1.5M.',
 '<h2>H2 2025</h2><p>MRR R$15.2k, cresceu 63% no semestre. Runway 4 meses. Meta: Seed Q3 2026.</p>',
 'H2 2025: MRR +63%, 420 clientes. Runway 4 meses. Seed Round planejado Q3 2026.',
 'report', 'investors', '["Admin","Founder","Investor","Finance","BoardMember"]',
 0, '2026-01-15 09:00:00', NULL, @u14, @u09),

('DEMO0013-0003-4000-A000-000000000004', @co3,
 'Urgente: Revisão do SHA — Prazo 31/03/2026',
 'Atenção sócios e investidores,\n\nO Acordo de Acionistas (SHA) vence em 30/06/2026 e precisamos iniciar a renovação. O escritório Cunha & Advogados preparou as alterações propostas:\n\n1. Atualização da cláusula de drag-along (prop. 75% para 80%)\n2. Inclusão de cláusula de preferência Seed Round\n3. Ajuste do modelo de vesting para novos sócios\n\nFavor revisarem o documento e enviarem comentários até 31/03/2026.\n\nDocumento disponível no Data Room.',
 '<h2>Revisão do SHA</h2><p>Prazo: 31/03/2026. Alterações: drag-along, preferência Seed, vesting.</p>',
 'SHA vence em jun/2026. Revisão obrigatória até 31/03/2026.',
 'alert', 'specific', '["Admin","Founder","Investor","Legal","BoardMember"]',
 1, '2026-03-05 08:00:00', '2026-03-31 23:59:00', @u13, @u09);

-- Migration 061 Fase 6 Comunicações — fim
