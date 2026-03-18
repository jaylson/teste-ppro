-- =====================================================
-- Migration 059 — Fase 4: Valuation + Financial
-- Valuations + Financial Periods + Metrics
-- Data: 2026-03-16
-- =====================================================

SET @cli  = 'DEMO0001-0000-4000-A000-000000000001';
SET @co1  = 'DEMO0001-0000-4000-A000-000000000010'; -- InovaTech
SET @co2  = 'DEMO0001-0000-4000-A000-000000000020'; -- VidaSaúde
SET @co3  = 'DEMO0001-0000-4000-A000-000000000030'; -- FinanGrow
SET @u01  = 'DEMO0001-0000-4000-A000-000000000101'; -- Carlos Silva
SET @u07  = 'DEMO0001-0000-4000-A000-000000000107'; -- João Ferreira
SET @u09  = 'DEMO0001-0000-4000-A000-000000000109'; -- Pedro Alves
SET @u11  = 'DEMO0001-0000-4000-A000-000000000111'; -- Dr. Marcos (Board)
SET @u14  = 'DEMO0001-0000-4000-A000-000000000114'; -- Fernanda Lima (Finance)

-- =====================================================
-- 1. VALUATIONS
-- InovaTech: Seed 2022 + Series A 2024 + Review 2025 + Draft 2026
-- VidaSaúde: Seed 2023 + Review 2025
-- FinanGrow: Pre-seed 2024 + Draft 2026
-- =====================================================
INSERT IGNORE INTO valuations
    (id, client_id, company_id, valuation_date, event_type, event_name,
     valuation_amount, total_shares, price_per_share,
     status, notes, submitted_at, submitted_by, approved_at, approved_by,
     created_by, updated_by)
VALUES
-- === InovaTech ===
('DEMO0009-0001-4000-A000-000000000001', @cli, @co1,
 '2022-06-01', 'seed', 'Seed Round — InovaTech',
 9000000.00, 10000000, 0.9000,
 'approved', 'Avaliação pré-money R$8M; Pós-money R$9M. Fundo Aceleração entrou com R$800k.',
 '2022-05-15 09:00:00', @u01, '2022-05-20 14:00:00', @u11, @u01, @u01),

('DEMO0009-0001-4000-A000-000000000002', @cli, @co1,
 '2024-09-01', 'series_a', 'Series A — InovaTech',
 20250000.00, 10500000, 4.5000,
 'approved', 'Avaliação pré-money R$18M; Pós-money R$20.25M. Follow-on Fundo + novos investidores.',
 '2024-08-20 09:00:00', @u01, '2024-08-28 15:00:00', @u11, @u01, @u01),

('DEMO0009-0001-4000-A000-000000000003', @cli, @co1,
 '2025-12-01', 'internal', 'Avaliação Interna Q4 2025 — InovaTech',
 28000000.00, 10500000, 5.3333,
 'approved', 'Revisão interna anual. Crescimento de MRR 38% YoY. Runway 18 meses.',
 '2025-11-25 09:00:00', @u14, '2025-12-05 11:00:00', @u11, @u14, @u14),

('DEMO0009-0001-4000-A000-000000000004', @cli, @co1,
 '2026-03-01', 'other', 'Planejamento Pré-IPO 2027 — InovaTech',
 45000000.00, 10500000, 8.5714,
 'draft', 'Rascunho de valuation para análise de timing de IPO no Novo Mercado B3.',
 NULL, NULL, NULL, NULL, @u01, @u01),

-- === VidaSaúde ===
('DEMO0009-0002-4000-A000-000000000001', @cli, @co2,
 '2023-03-15', 'seed', 'Seed Round — VidaSaúde',
 10000000.00, 5500000, 2.0000,
 'approved', 'Pré-money R$8M. Marina Torres Health Fund. Tração de 8k usuários na plataforma.',
 '2023-03-01 09:00:00', @u07, '2023-03-10 14:00:00', @u11, @u07, @u07),

('DEMO0009-0002-4000-A000-000000000002', @cli, @co2,
 '2025-06-01', 'internal', 'Avaliação Interna H1 2025 — VidaSaúde',
 18000000.00, 5500000, 3.2727,
 'approved', 'Revisão semestral. 42k usuários ativos. Parceria com 3 hospitais fechada.',
 '2025-05-20 09:00:00', @u07, '2025-06-05 10:00:00', @u11, @u07, @u07),

-- === FinanGrow ===
('DEMO0009-0003-4000-A000-000000000001', @cli, @co3,
 '2024-05-01', 'seed', 'Pre-seed — FinanGrow',
 3000000.00, 2000000, 1.5000,
 'approved', 'Conversão do SAFE. Pré-money R$2.55M. Fundo Aceleração Brasil.',
 '2024-04-20 09:00:00', @u09, '2024-04-28 14:00:00', @u11, @u09, @u09),

('DEMO0009-0003-4000-A000-000000000002', @cli, @co3,
 '2026-01-15', 'internal', 'Avaliação Interna Q1 2026 — FinanGrow',
 6500000.00, 2000000, 3.2500,
 'draft', 'Análise preliminar para Seed Round planejado para Q3 2026.',
 NULL, NULL, NULL, NULL, @u09, @u09);

-- =====================================================
-- 2. FINANCIAL PERIODS (InovaTech: 24m | VS: 12m | FG: 6m)
-- =====================================================

-- InovaTech: Jan/2024 a Dez/2025
INSERT IGNORE INTO financial_periods
    (client_id, company_id, year, month, status,
     submitted_at, submitted_by, approved_at, approved_by,
     created_by, updated_by)
VALUES
(@cli,@co1,2024,1,'approved', '2024-02-05 09:00:00',@u14,'2024-02-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2024,2,'approved', '2024-03-05 09:00:00',@u14,'2024-03-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2024,3,'approved', '2024-04-05 09:00:00',@u14,'2024-04-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2024,4,'approved', '2024-05-05 09:00:00',@u14,'2024-05-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2024,5,'approved', '2024-06-05 09:00:00',@u14,'2024-06-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2024,6,'approved', '2024-07-05 09:00:00',@u14,'2024-07-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2024,7,'approved', '2024-08-05 09:00:00',@u14,'2024-08-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2024,8,'approved', '2024-09-05 09:00:00',@u14,'2024-09-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2024,9,'approved', '2024-10-05 09:00:00',@u14,'2024-10-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2024,10,'approved','2024-11-05 09:00:00',@u14,'2024-11-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2024,11,'approved','2024-12-05 09:00:00',@u14,'2024-12-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2024,12,'approved','2025-01-05 09:00:00',@u14,'2025-01-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2025,1,'approved', '2025-02-05 09:00:00',@u14,'2025-02-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2025,2,'approved', '2025-03-05 09:00:00',@u14,'2025-03-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2025,3,'approved', '2025-04-05 09:00:00',@u14,'2025-04-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2025,4,'approved', '2025-05-05 09:00:00',@u14,'2025-05-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2025,5,'approved', '2025-06-05 09:00:00',@u14,'2025-06-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2025,6,'approved', '2025-07-05 09:00:00',@u14,'2025-07-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2025,7,'approved', '2025-08-05 09:00:00',@u14,'2025-08-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2025,8,'approved', '2025-09-05 09:00:00',@u14,'2025-09-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2025,9,'approved', '2025-10-05 09:00:00',@u14,'2025-10-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2025,10,'approved','2025-11-05 09:00:00',@u14,'2025-11-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2025,11,'approved','2025-12-05 09:00:00',@u14,'2025-12-10 10:00:00',@u01,@u14,@u14),
(@cli,@co1,2025,12,'approved','2026-01-05 09:00:00',@u14,'2026-01-10 10:00:00',@u01,@u14,@u14);

-- VidaSaúde: Jan/2025 a Dez/2025
INSERT IGNORE INTO financial_periods
    (client_id, company_id, year, month, status,
     submitted_at, submitted_by, approved_at, approved_by,
     created_by, updated_by)
VALUES
(@cli,@co2,2025,1,'approved', '2025-02-05 09:00:00',@u07,'2025-02-12 10:00:00',@u07,@u07,@u07),
(@cli,@co2,2025,2,'approved', '2025-03-05 09:00:00',@u07,'2025-03-12 10:00:00',@u07,@u07,@u07),
(@cli,@co2,2025,3,'approved', '2025-04-05 09:00:00',@u07,'2025-04-12 10:00:00',@u07,@u07,@u07),
(@cli,@co2,2025,4,'approved', '2025-05-05 09:00:00',@u07,'2025-05-12 10:00:00',@u07,@u07,@u07),
(@cli,@co2,2025,5,'approved', '2025-06-05 09:00:00',@u07,'2025-06-12 10:00:00',@u07,@u07,@u07),
(@cli,@co2,2025,6,'approved', '2025-07-05 09:00:00',@u07,'2025-07-12 10:00:00',@u07,@u07,@u07),
(@cli,@co2,2025,7,'approved', '2025-08-05 09:00:00',@u07,'2025-08-12 10:00:00',@u07,@u07,@u07),
(@cli,@co2,2025,8,'approved', '2025-09-05 09:00:00',@u07,'2025-09-12 10:00:00',@u07,@u07,@u07),
(@cli,@co2,2025,9,'approved', '2025-10-05 09:00:00',@u07,'2025-10-12 10:00:00',@u07,@u07,@u07),
(@cli,@co2,2025,10,'approved','2025-11-05 09:00:00',@u07,'2025-11-12 10:00:00',@u07,@u07,@u07),
(@cli,@co2,2025,11,'approved','2025-12-05 09:00:00',@u07,'2025-12-12 10:00:00',@u07,@u07,@u07),
(@cli,@co2,2025,12,'approved','2026-01-05 09:00:00',@u07,'2026-01-12 10:00:00',@u07,@u07,@u07);

-- FinanGrow: Jul/2025 a Dez/2025
INSERT IGNORE INTO financial_periods
    (client_id, company_id, year, month, status,
     submitted_at, submitted_by, approved_at, approved_by,
     created_by, updated_by)
VALUES
(@cli,@co3,2025,7,'approved', '2025-08-05 09:00:00',@u09,'2025-08-10 10:00:00',@u09,@u09,@u09),
(@cli,@co3,2025,8,'approved', '2025-09-05 09:00:00',@u09,'2025-09-10 10:00:00',@u09,@u09,@u09),
(@cli,@co3,2025,9,'approved', '2025-10-05 09:00:00',@u09,'2025-10-10 10:00:00',@u09,@u09,@u09),
(@cli,@co3,2025,10,'approved','2025-11-05 09:00:00',@u09,'2025-11-10 10:00:00',@u09,@u09,@u09),
(@cli,@co3,2025,11,'approved','2025-12-05 09:00:00',@u09,'2025-12-10 10:00:00',@u09,@u09,@u09),
(@cli,@co3,2025,12,'approved','2026-01-05 09:00:00',@u09,'2026-01-10 10:00:00',@u09,@u09,@u09);

-- =====================================================
-- 3. FINANCIAL METRICS (por period)
-- Buscar period IDs dinâmicamente e inserir métricas
-- =====================================================

-- InovaTech — Série de MRR crescente 2024/2025 (SaaS em escala)
INSERT IGNORE INTO financial_metrics
    (client_id, period_id,
     gross_revenue, net_revenue, mrr, arr,
     cash_balance, burn_rate, runway_months,
     customer_count, churn_rate, cac, ltv, nps,
     ebitda, ebitda_margin, net_income,
     created_by, updated_by)
SELECT @cli, fp.id,
  gross_rev, net_rev, mrr_val, mrr_val*12,
  cash_bal, burn, ROUND(cash_bal/burn,1),
  cust, churn, cac_val, ltv_val, nps_val,
  ebitda_val, ebitda_pct, net_inc,
  @u14, @u14
FROM financial_periods fp
JOIN (SELECT 2024 yr, 1 mo, 255000 gross_rev, 242250 net_rev, 85000 mrr_val, 480000 cash_bal, 148000 burn, 180 cust, 0.0280 churn, 1200 cac_val, 36000 ltv_val, 32 nps_val, -85000 ebitda_val, -0.3510 ebitda_pct, -92000 net_inc
UNION ALL SELECT 2024,2, 263000,249850, 87700, 520000,145000,187,0.0265,1180,36500,34,-80500,-0.3061,-87000
UNION ALL SELECT 2024,3, 272000,258400, 90500, 558000,142000,193,0.0250,1160,37000,36,-76000,-0.2794,-82000
UNION ALL SELECT 2024,4, 282000,267900, 93800, 595000,139000,199,0.0240,1145,37500,38,-71000,-0.2518,-77000
UNION ALL SELECT 2024,5, 293000,278350, 97300, 631000,136000,204,0.0230,1130,38000,39,-66000,-0.2253,-72000
UNION ALL SELECT 2024,6, 305000,289750,101600, 668000,133000,210,0.0220,1120,38500,41,-61000,-0.2000,-67000
UNION ALL SELECT 2024,7, 318000,302100,106000, 702000,130000,216,0.0215,1110,39000,43,-56000,-0.1761,-62000
UNION ALL SELECT 2024,8, 332000,315400,110600, 735000,127000,222,0.0210,1100,39500,45,-51000,-0.1536,-57000
UNION ALL SELECT 2024,9, 347000,329650,115600, 768000,124000,228,0.0205,1090,40000,47,-46000,-0.1326,-52000
UNION ALL SELECT 2024,10,363000,344850,121000, 800000,121000,234,0.0200,1080,40500,49,-41000,-0.1130,-47000
UNION ALL SELECT 2024,11,380000,361000,126800, 831000,118000,240,0.0195,1070,41000,51,-36000,-0.0947,-42000
UNION ALL SELECT 2024,12,398000,378100,132700, 862000,115000,247,0.0190,1060,41500,53,-31000,-0.0779,-37000
UNION ALL SELECT 2025,1, 418000,397100,139300, 892000,115000,255,0.0185,1050,42000,55,-26000,-0.0622,-31500
UNION ALL SELECT 2025,2, 439000,417050,146300, 921000,116000,263,0.0180,1040,42500,57,-20000,-0.0456,-25000
UNION ALL SELECT 2025,3, 461000,437950,153700, 950000,118000,271,0.0175,1030,43000,59,-14000,-0.0304,-19000
UNION ALL SELECT 2025,4, 484000,459800,161400, 978000,120000,279,0.0170,1020,43500,61, -7500,-0.0155,-12500
UNION ALL SELECT 2025,5, 509000,483550,169700,1006000,123000,288,0.0165,1010,44000,64,  500, 0.0010,  -4000
UNION ALL SELECT 2025,6, 535000,508250,178300,1034000,127000,297,0.0160,1000,44500,66,  8500, 0.0159,   4000
UNION ALL SELECT 2025,7, 562000,533900,187400,1062000,132000,306,0.0155, 995,45000,68, 18000, 0.0320,  13000
UNION ALL SELECT 2025,8, 591000,561450,197000,1090000,138000,316,0.0150, 988,45500,70, 28000, 0.0474,  23000
UNION ALL SELECT 2025,9, 621000,589950,207000,1118000,145000,326,0.0145, 982,46000,72, 39000, 0.0628,  34000
UNION ALL SELECT 2025,10,653000,620350,217700,1145000,153000,336,0.0140, 975,46500,74, 51000, 0.0781,  46000
UNION ALL SELECT 2025,11,686000,651700,228700,1172000,163000,347,0.0135, 969,47000,76, 64000, 0.0934,  59000
UNION ALL SELECT 2025,12,721000,684950,240300,1199000,174000,358,0.0130, 963,47500,79, 78000, 0.1082,  73000
) m ON fp.company_id=CONVERT(@co1 USING utf8mb4) COLLATE utf8mb4_unicode_ci AND fp.year=m.yr AND fp.month=m.mo;

-- VidaSaúde — Healthtech crescendo (2025 12 meses)
INSERT IGNORE INTO financial_metrics
    (client_id, period_id,
     gross_revenue, net_revenue, mrr, arr,
     cash_balance, burn_rate, runway_months,
     customer_count, churn_rate, cac, ltv, nps,
     ebitda, ebitda_margin, net_income,
     created_by, updated_by)
SELECT @cli, fp.id,
  gross_rev, net_rev, mrr_val, mrr_val*12,
  cash_bal, burn, ROUND(cash_bal/burn,1),
  cust, churn, cac_val, ltv_val, nps_val,
  ebitda_val, ebitda_pct, net_inc,
  @u07, @u07
FROM financial_periods fp
JOIN (SELECT 2025 yr, 1 mo,  98000 gross_rev, 93100 net_rev, 32700 mrr_val, 380000 cash_bal, 72000 burn, 8200 cust, 0.0320 churn, 280 cac_val, 12000 ltv_val, 38 nps_val, -28000 ebitda_val, -0.2857 ebitda_pct, -31000 net_inc
UNION ALL SELECT 2025,2, 104000, 98800, 34700, 370000, 70000, 8500, 0.0310, 275, 12200, 40, -25000,-0.2404,-28000
UNION ALL SELECT 2025,3, 111000,105450, 37000, 360000, 68000, 8800, 0.0300, 270, 12400, 42, -22000,-0.1982,-25000
UNION ALL SELECT 2025,4, 119000,113050, 39700, 352000, 66000, 9100, 0.0290, 265, 12600, 44, -19000,-0.1597,-22000
UNION ALL SELECT 2025,5, 128000,121600, 42700, 345000, 64000, 9400, 0.0280, 260, 12800, 46, -15000,-0.1172,-18000
UNION ALL SELECT 2025,6, 138000,131100, 46000, 340000, 62000, 9700, 0.0270, 255, 13000, 48, -11000,-0.0797,-14000
UNION ALL SELECT 2025,7, 149000,141550, 49700, 336000, 60000,10000, 0.0260, 252, 13200, 50,  -6500,-0.0436,-10000
UNION ALL SELECT 2025,8, 161000,152950, 53700, 334000, 58500,10300, 0.0250, 249, 13400, 52,  -1500,-0.0093, -5000
UNION ALL SELECT 2025,9, 174000,165300, 58000, 334000, 57000,10600, 0.0245, 246, 13600, 54,   4000, 0.0230,    500
UNION ALL SELECT 2025,10,188000,178600, 62700, 335000, 56000,10900, 0.0240, 243, 13800, 56,  10000, 0.0532,  6000
UNION ALL SELECT 2025,11,203000,192850, 67700, 338000, 55000,11200, 0.0235, 240, 14000, 58,  17000, 0.0837, 13000
UNION ALL SELECT 2025,12,219000,208050, 73000, 343000, 54000,11500, 0.0230, 237, 14200, 60,  25000, 0.1142, 21000
) m ON fp.company_id=CONVERT(@co2 USING utf8mb4) COLLATE utf8mb4_unicode_ci AND fp.year=m.yr AND fp.month=m.mo;

-- FinanGrow — Fintech early-stage (Jul-Dez 2025)
INSERT IGNORE INTO financial_metrics
    (client_id, period_id,
     gross_revenue, net_revenue, mrr, arr,
     cash_balance, burn_rate, runway_months,
     customer_count, churn_rate, cac, ltv, nps,
     ebitda, ebitda_margin, net_income,
     created_by, updated_by)
SELECT @cli, fp.id,
  gross_rev, net_rev, mrr_val, mrr_val*12,
  cash_bal, burn, ROUND(cash_bal/burn,1),
  cust, churn, cac_val, ltv_val, nps_val,
  ebitda_val, ebitda_pct, net_inc,
  @u09, @u09
FROM financial_periods fp
JOIN (SELECT 2025 yr, 7 mo, 28000 gross_rev, 26600 net_rev, 9300 mrr_val, 180000 cash_bal, 42000 burn, 320 cust, 0.0450 churn, 850 cac_val, 8500 ltv_val, 28 nps_val, -18000 ebitda_val, -0.6429 ebitda_pct, -20000 net_inc
UNION ALL SELECT 2025,8,  31000, 29450, 10300, 172000, 41000, 340, 0.0430, 840, 8600, 30, -16500,-0.5323,-18500
UNION ALL SELECT 2025,9,  34000, 32300, 11300, 166000, 40000, 360, 0.0410, 830, 8700, 32, -15000,-0.4412,-17000
UNION ALL SELECT 2025,10, 37500, 35625, 12500, 162000, 39500, 380, 0.0390, 820, 8800, 34, -13000,-0.3467,-15000
UNION ALL SELECT 2025,11, 41500, 39425, 13800, 160000, 39000, 400, 0.0370, 810, 8900, 36, -11000,-0.2651,-13000
UNION ALL SELECT 2025,12, 45500, 43225, 15200, 160000, 38500, 420, 0.0350, 800, 9000, 38,  -9000,-0.1978,-11000
) m ON fp.company_id=CONVERT(@co3 USING utf8mb4) COLLATE utf8mb4_unicode_ci AND fp.year=m.yr AND fp.month=m.mo;

-- Migration 059 Fase 4 Valuation/Financial — fim
