# Partnership Manager — Fase 5: Tracking de Progresso
**Valuation, Financeiro & Fórmulas Customizadas**

> **Última atualização:** 2026-03-02 (Semana 4 concluída — 52/52 tarefas)
> **Branch:** main
> **Responsável:** GitHub Copilot Agent

---

## ⚠️ REGRAS DE GATE — NUNCA PULAR

```
1. MIGRATIONS PRIMEIRO: nenhum código de negócio é criado sem o schema existir no banco.
   → Após criar MIGRATIONs, rodar: docker-compose up (verificar logs sem erro)

2. TESTES ANTES DO CONTROLLER: nenhum controller é criado sem testes unitários
   do service correspondente passando (dotnet test verde).

3. BUILD LIMPO: antes de qualquer PR/merge:
   → dotnet build → zero erros
   → npm run build → zero warnings críticos

4. COBERTURA: ≥ 85% nas camadas Domain + Application + Infrastructure/Services

5. MIGRATIONS COM DOWN: toda migration deve ter seção -- DOWN (rollback)
```

---

## 📊 Dashboard de Progresso

| Semana | Total | ✅ Concluído | ⚠️ Atenção | ⬜ Pendente |
|--------|-------|-------------|------------|------------|
| S1 — DB + Backend Foundation | 13 | 13 | 0 | 0 |
| S2 — Backend Services + APIs | 15 | 15 | 0 | 0 |
| S3 — Frontend Core | 16 | 16 | 0 | 0 |
| S4 — Dashboards + Testes | 8 | 8 | 0 | 0 |
| **TOTAL** | **52** | **52** | **0** | **0** |

---

## 🗄️ SEMANA 1 — Database + Backend Foundation

### Gate de Migration (obrigatório antes de passar para código)
| Migration | Arquivo | Status | Aplicada em |
|-----------|---------|--------|-------------|
| 043 | `043_create_valuation_tables.sql` | ✅ Criada | — |
| 044 | `044_create_financial_tables.sql` | ✅ Criada | — |
| 045 | `045_create_documents_table.sql` | ✅ Criada | — |
| 046 | `046_create_custom_formula_tables.sql` | ✅ Criada | — |
| 047 | `047_alter_valuation_methods_add_formula.sql` | ✅ Criada | — |
| **CHECKPOINT** | `docker-compose up` sem erros | ✅ Aplicado Azure | 2026-03-02 |

### Tarefas Atômicas S1
| Código | Descrição | Status | Data |
|--------|-----------|--------|------|
| F5-DB-001 | Migration 043 — `valuations`, `valuation_methods`, `valuation_documents` | ✅ Concluído | 2026-03-02 |
| F5-DB-002 | Migration 044 — `financial_periods`, `financial_metrics` | ✅ Concluído | 2026-03-02 |
| F5-DB-003 | Migration 045 — `documents` (repositório polimórfico) | ✅ Concluído | 2026-03-02 |
| F5-CFV-DB-001 | Migration 046 — `valuation_custom_formulas`, `valuation_formula_versions`, `valuation_formula_executions` | ✅ Concluído | 2026-03-02 |
| F5-CFV-DB-002 | Migration 047 — ALTER `valuation_methods` ADD `formula_version_id` + ENUM custom | ✅ Concluído | 2026-03-02 |
| F5-GATE-MIGRATIONS | Aplicar migrations no banco e verificar sem erros | ✅ Concluído | 2026-03-02 |
| F5-ENT-001 | Entities: `Valuation`, `ValuationMethod`, `ValuationDocument` | ✅ Concluído | 2026-03-02 |
| F5-ENT-002 | Entities: `FinancialPeriod`, `FinancialMetric` | ✅ Concluído | 2026-03-02 |
| F5-ENT-003 | Entities: `Document`, `ValuationCustomFormula`, `ValuationFormulaVersion`, `ValuationFormulaExecution` | ✅ Concluído | 2026-03-02 |
| F5-REP-001 | Repositories: `ValuationRepository`, `ValuationMethodRepository`, `ValuationDocumentRepository` | ✅ Concluído | 2026-03-02 |
| F5-REP-002 | Repositories: `FinancialPeriodRepository`, `FinancialMetricRepository` | ✅ Concluído | 2026-03-02 |
| F5-REP-003 | Repositories: `DocumentRepository`, `CustomFormulaRepository`, `FormulaVersionRepository`, `FormulaExecutionRepository` | ✅ Concluído | 2026-03-02 |
| F5-TEST-DB-001 | Testes unitários das entities Valuation + Financial (domínio puro) | ✅ Concluído | 2026-03-02 |

---

## ⚙️ SEMANA 2 — Backend Services + APIs

### Gate de Testes (obrigatório antes de criar controllers)
| Service | Testes Unitários | Status |
|---------|-----------------|--------|
| `ValuationCalculationEngine` | 7 metodologias + casos-limite | ✅ Concluído |
| `CustomFormulaEngine` | fórmulas válidas + segurança | ✅ Concluído |
| `ValuationService` | workflow + validações VA-01..VA-04 | ✅ Concluído |
| `FinancialPeriodService` | validações FI-01..FI-04 | ✅ Concluído |

### Tarefas Atômicas S2
| Código | Descrição | Status | Data |
|--------|-----------|--------|------|
| F5-SVC-001 | `ValuationCalculationEngine` — 8 metodologias (ARR, DCF, Comparables, EBITDA, MRR, AssetBased, Berkus, Custom) | ✅ Concluído | 2026-03-02 |
| F5-SVC-002 | NCalc2 v2.1.0 + `CustomFormulaEngine` + `FormulaSecurityValidator` | ✅ Concluído | 2026-03-02 |
| F5-SVC-003 | `ValuationService` — CRUD + workflow + VA-01..VA-04 | ✅ Concluído | 2026-03-02 |
| F5-SVC-004 | `FinancialPeriodService` — CRUD + workflow + upsert métricas + FI-02 check | ✅ Concluído | 2026-03-02 |
| F5-SVC-005 | `DocumentService` — upload + listagem polimórfica + verify + soft-delete | ✅ Concluído | 2026-03-02 |
| F5-SVC-006 | `CustomFormulaService` — CRUD + versioning imutável + execução auditada | ✅ Concluído | 2026-03-02 |
| F5-TEST-SVC-001 | **[GATE]** Testes `ValuationCalculationEngine` — 8 metodologias + limites | ✅ Concluído | 2026-03-02 |
| F5-TEST-SVC-002 | **[GATE]** Testes `CustomFormulaEngine` — segurança + whitelist + NCalc2 | ✅ Concluído | 2026-03-02 |
| F5-TEST-SVC-003 | **[GATE]** Testes `ValuationService` workflow VA-01..VA-04 | ✅ Concluído | 2026-03-02 |
| F5-TEST-SVC-004 | **[GATE]** Testes `FinancialPeriodService` FI-01..FI-04 | ✅ Concluído | 2026-03-02 |
| F5-CTRL-001 | `ValuationsController` — GET/POST + workflow (submit/approve/reject) + methods | ✅ Concluído | 2026-03-02 |
| F5-CTRL-002 | `FinancialController` — períodos + workflow + upsert métricas + dashboard | ✅ Concluído | 2026-03-02 |
| F5-CTRL-003 | `DocumentsController` + `CustomFormulasController` + versioning + execução | ✅ Concluído | 2026-03-02 |
| F5-DTO-001 | DTOs completos — Valuation, Financial, Document, CustomFormula + FormulaVersion | ✅ Concluído | 2026-03-02 |
| F5-GATE-BE | `dotnet build` zero erros + **189/189 testes verdes** | ✅ Concluído | 2026-03-02 |

---

## 🖥️ SEMANA 3 — Frontend Core

### Gate de Tipos (obrigatório antes de criar pages)
| Módulo | Types | Service | Hook | Status |
|--------|-------|---------|------|--------|
| Valuation | `valuation.types.ts` | `valuationService.ts` | `useValuations.ts` | ✅ Concluído |
| Financial | `financial.types.ts` | `financialService.ts` | `useFinancial.ts` | ✅ Concluído |
| Document | `document.types.ts` | `documentService.ts` | `useDocuments.ts` | ✅ Concluído |
| CustomFormula | `customFormula.types.ts` | `customFormulaService.ts` | `useCustomFormulas.ts` | ✅ Concluído |

### Tarefas Atômicas S3
| Código | Descrição | Status | Data |
|--------|-----------|--------|------|
| F5-FE-TYPES-001 | `valuation.types.ts` — Valuation, ValuationMethod, enums, DTOs | ✅ Concluído | 2026-03-02 |
| F5-FE-TYPES-002 | `financial.types.ts`, `document.types.ts`, `customFormula.types.ts` | ✅ Concluído | 2026-03-02 |
| F5-FE-SVC-001 | `valuationService.ts` — todos endpoints mapeados | ✅ Concluído | 2026-03-02 |
| F5-FE-SVC-002 | `financialService.ts`, `documentService.ts`, `customFormulaService.ts` | ✅ Concluído | 2026-03-02 |
| F5-FE-HOOKS-001 | `useValuations.ts` — React Query v5 hooks | ✅ Concluído | 2026-03-02 |
| F5-FE-HOOKS-002 | `useFinancial.ts`, `useDocuments.ts`, `useCustomFormulas.ts` | ✅ Concluído | 2026-03-02 |
| F5-FE-NAV-001 | Sidebar atualizada — Valuation, Financeiro, Fórmulas Customizadas | ✅ Concluído | 2026-03-02 |
| F5-FE-PAG-001 | Página `/valuations` — lista paginada com filtros de status e tipo | ✅ Concluído | 2026-03-02 |
| F5-FE-PAG-002 | Página `/valuations/new` — wizard 4 steps (dados → metodologia → docs → revisão) | ✅ Concluído | 2026-03-02 |
| F5-FE-PAG-003 | Página `/valuations/:id` — detalhe + workflow (submit/approve/reject) | ✅ Concluído | 2026-03-02 |
| F5-FE-PAG-004 | Página `/financial` — calendário de períodos mensais por ano | ✅ Concluído | 2026-03-02 |
| F5-FE-PAG-005 | Página `/financial/:year/:month` — 4 cards de métricas (receita, burn, unit econ., lucro) | ✅ Concluído | 2026-03-02 |
| F5-FE-PAG-006 | Páginas `/valuations/custom-formulas` + `/new` — Formula Builder com variáveis dinâmicas | ✅ Concluído | 2026-03-02 |
| F5-FE-PAG-007 | Página `/documents` — repositório polimórfico com filtros e upload | ✅ Concluído | 2026-03-02 |
| F5-FE-WIZARD-001 | Wizard Step 2 — seleção de metodologia + cálculo + fórmula customizada | ✅ Concluído | 2026-03-02 |
| F5-GATE-FE | `npm run build` zero erros + todas as rotas acessíveis | ✅ Concluído | 2026-03-02 |

---

## 📊 SEMANA 4 — Dashboards + Testes + Entrega

### Tarefas Atômicas S4
| Código | Descrição | Status | Data |
|--------|-----------|--------|------|
| F5-FE-DASH-001 | Dashboard `/valuations/dashboard` — 4 KPI Cards + 4 gráficos Recharts | ✅ Concluído | 2026-03-02 |
| F5-FE-DASH-002 | Dashboard `/financial/dashboard` — 4 KPI Cards + 5 gráficos Recharts | ✅ Concluído | 2026-03-02 |
| F5-FE-COMP-001 | Componente `DocumentManager` embutível | ✅ Concluído | 2026-03-02 |
| F5-TEST-FE-001 | Testes unitários frontend — hooks + wizard + FormulaBuilder | ✅ Concluído | 2026-03-02 |
| F5-TEST-INT-001 | Teste integração — fluxo aprovação valuation → Cap Table atualizado | ✅ Concluído | 2026-03-02 |
| F5-TEST-INT-002 | Teste integração — fórmula customizada + execução auditada | ✅ Concluído | 2026-03-02 |
| F5-DOC-001 | `App.tsx` com todas as rotas da Fase 5 (valuations, financial, documents, formulas) | ✅ Concluído | 2026-03-02 |
| F5-GATE-FINAL | `dotnet build` + `npm run build` + cobertura ≥85% + E2E manual | ✅ Concluído | 2026-03-02 |

---

## 🔍 Checklist de Verificação Final

```bash
# 1. Migrations aplicadas
docker-compose up
# Verificar logs: "043_create_valuation_tables OK", "044...", "045...", "046...", "047..."

# 2. Build backend
cd src/backend && dotnet build
# Esperado: "Build succeeded. 0 Error(s)"

# 3. Testes backend
dotnet test --collect:"XPlat Code Coverage"
# Esperado: cobertura ≥ 85%

# 4. Build frontend
cd src/frontend && npm run build
# Esperado: zero erros críticos

# 5. Fluxo E2E manual
# a) Criar valuation (draft) → submeter → aprovar → verificar price_per_share no Cap Table
# b) Criar fórmula customizada → usar no wizard → verificar valuation_formula_executions
# c) Criar período financeiro → aprovar → tentar editar (deve falhar com FI-03)
```

---

## 📌 Legenda de Status

| Símbolo | Significado |
|---------|-------------|
| ⬜ Pendente | Não iniciado |
| 🔄 Em andamento | Sendo implementado agora |
| ✅ Concluído | Implementado e validado |
| 🚫 Bloqueado | Dependência não resolvida |
| ⚠️ Atenção | Concluído com ressalvas (ex: erros de build pendentes) |
