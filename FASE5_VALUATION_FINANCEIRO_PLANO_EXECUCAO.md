# Partnership Manager
## Fase 5 — Valuation & Financeiro: Plano de Execução Completo

**Versão:** 1.0  
**Data:** 26 de Fevereiro de 2026  
**Duração Estimada:** 4 semanas (160 horas)  
**Regime:** 40h/semana | 8h/dia × 5 dias  
**Ambiente:** GitHub Agent (Claude / Cursor AI)  
**Baseado em:** MER v2.0, PREMISSAS_DESENVOLVIMENTO.md v1.0, padrões das Fases 1–4  

---

## 📋 Sumário

1. [Análise do Estado Atual](#1-análise-do-estado-atual)
2. [Escopo da Fase 5](#2-escopo-da-fase-5)
3. [Mapa de Interfaces e UX — O que o usuário verá](#3-mapa-de-interfaces-e-ux)
4. [Estrutura de Menus e Navegação](#4-estrutura-de-menus-e-navegação)
5. [Cadastros e Formulários](#5-cadastros-e-formulários)
6. [Pré-Requisitos e Dependências](#6-pré-requisitos-e-dependências)
7. [Nomenclatura das Tarefas Atômicas](#7-nomenclatura-das-tarefas-atômicas)
8. [Cronograma Detalhado — Semana 1: Database + Backend Foundation](#8-semana-1-database--backend-foundation)
9. [Cronograma Detalhado — Semana 2: Backend Services + APIs](#9-semana-2-backend-services--apis)
10. [Cronograma Detalhado — Semana 3: Frontend Core](#10-semana-3-frontend-core)
11. [Cronograma Detalhado — Semana 4: Dashboard + Documentos + Testes](#11-semana-4-dashboard--documentos--testes)
12. [Resumo de Todas as Tarefas](#12-resumo-de-todas-as-tarefas)
13. [Critérios de Aceite da Fase](#13-critérios-de-aceite-da-fase)
14. [Comandos de Verificação](#14-comandos-de-verificação)
15. [Checklist de Entrega Final](#15-checklist-de-entrega-final)

---

## 1. Análise do Estado Atual

### 1.1 Infraestrutura Existente (Herdada das Fases 1–4)

| Componente | Status | Relevância para Fase 5 |
|------------|--------|------------------------|
| Database Schema (MySQL 8.0) | ✅ Fases 1–4 migradas | Fases 1–4 migradas — próxima migration será 037+ |
| Backend .NET 9 + Dapper | ✅ Pronto | Padrões BaseEntity, Repository, Service, Controller estabelecidos |
| Frontend React 18 + TypeScript | ✅ Pronto | Design System, hooks (React Query), services, componentes |
| Autenticação Firebase + JWT | ✅ Implementado | User.GetCompanyId() disponível nos controllers |
| Multi-tenancy (Client→Company→User) | ✅ Implementado | TODOS os queries devem filtrar por company_id |
| Audit Trail | ✅ Implementado | Registrar ações de aprovação de valuation |
| Cap Table Module (Fase 2) | ✅ Implementado | **CRÍTICO** — shares, share_classes e price_per_share serão atualizados após valuation aprovado |
| Vesting Module (Fase 4) | ✅ Implementado | Grants impactam diluted shares no cálculo de valuation |
| Contracts + ClickSign (Fase 3) | ✅ Implementado | DocumentService será modelo para upload de arquivos de valuation |
| Redis Cache | ✅ Disponível | Métricas financeiras consolidadas devem usar cache (TTL 1h) |
| FluentValidation | ✅ Padrão estabelecido | Todos os validators devem seguir o padrão de Fases anteriores |
| Constants/Messages.cs | ✅ Estabelecido | NUNCA strings hardcoded — sempre usar ErrorMessages e SuccessMessages |

### 1.2 Entidades que serão Implementadas na Fase 5

```
📦 Módulo Valuation
├── 💰 valuations              ← Avaliações da empresa (CRUD + workflow de aprovação)
├── 📊 valuation_methods       ← Metodologias de cálculo (ARR, DCF, Comparable, etc.)
└── 📎 valuation_documents     ← Documentos anexados ao valuation

📦 Módulo Financeiro
├── 📅 financial_periods       ← Períodos mensais de dados financeiros
└── 📈 financial_metrics       ← Métricas por período (MRR, ARR, Burn, etc.)

📦 Módulo Documentos
└── 🗂️ documents              ← Repositório central de documentos (polimórfico)
```

### 1.3 Regras de Negócio Críticas (do MER)

| Código | Regra | Impacto na Implementação |
|--------|-------|--------------------------|
| VA-01 | Exatamente uma metodologia deve ser selecionada como principal | Validação: is_selected = true em exatamente 1 ValuationMethod |
| VA-02 | Valuation precisa de aprovação para ser oficial | Workflow: draft → pending_approval → approved/rejected |
| VA-03 | Data do valuation deve ser posterior ao anterior aprovado | Validação temporal no Service |
| VA-04 | price_per_share = valuation_amount / total_shares | Cálculo automático + atualização do Cap Table após aprovação |
| FI-01 | Um período (ano/mês) por empresa — chave única | UNIQUE INDEX (company_id, year, month) |
| FI-02 | Não pode aprovar período futuro sem aprovar anterior | Validação de sequência no FinancialService |
| FI-03 | Período aprovado não pode ser editado (locked) | Status 'locked' — imutável após aprovação |
| FI-04 | Documentos obrigatórios para aprovação de período | Verificar documentos antes de permitir submit |

---

## 2. Escopo da Fase 5

### 2.1 Objetivos Principais

1. **Motor de Valuation** — Engine de cálculo com 7 metodologias configuráveis
2. **Gestão de Valuations** — CRUD completo com workflow de aprovação e histórico
3. **Dashboard Executivo de Valuation** — Gráficos de evolução e comparação de metodologias
4. **Módulo Financeiro** — Períodos mensais com métricas-chave do negócio
5. **Dashboard Financeiro** — MRR, ARR, Burn Rate, Runway com tendências
6. **Gestão de Documentos** — Repositório central reutilizável em toda a plataforma
7. **Integração Cap Table** — Atualização automática do price_per_share pós-aprovação

### 2.2 Entregáveis por Semana

| Semana | Foco Principal | Entregáveis |
|--------|---------------|-------------|
| **S1** | Database + Backend Base | 3 migrations + 6 entities + 3 repositories |
| **S2** | Backend Logic + APIs | 3 services (incluindo motor de cálculo) + 3 controllers + DTOs/Validators |
| **S3** | Frontend Core | Types, services, hooks, páginas e formulários de Valuation e Financial |
| **S4** | Dashboards + Docs + Testes | 2 dashboards com charts + módulo documentos + testes unitários e integração |

### 2.3 Métricas de Sucesso da Fase

- [ ] **30 tarefas atômicas** concluídas
- [ ] **Motor de valuation** calculando 7 metodologias com precisão DECIMAL(18,2)
- [ ] **Cap Table atualizado** automaticamente após aprovação de valuation
- [ ] **Cobertura de testes** ≥ 85%
- [ ] **Zero erros** no `dotnet build` e `npm run build`
- [ ] **Tempo de resposta** < 200ms p95 nas APIs principais
- [ ] **Todas as migrations** com script de rollback (DOWN)

---

## 3. Mapa de Interfaces e UX

> Esta seção descreve **TUDO que o usuário verá e interagirá** na Fase 5. É o guia central para o desenvolvimento de Frontend.

### 3.1 Visão Geral das Telas

```
FASE 5 — INTERFACES DO USUÁRIO

📊 VALUATION
├── /valuations                          → Lista de Valuations (tabela paginada)
├── /valuations/new                      → Formulário de Criação (wizard 4 steps)
├── /valuations/:id                      → Detalhe do Valuation (view completo)
├── /valuations/:id/edit                 → Edição (apenas draft)
├── /valuations/dashboard                → Dashboard Executivo (gráficos + KPIs)
└── /valuations/history                  → Histórico comparativo

💰 FINANCEIRO
├── /financial                           → Visão geral (calendário de períodos)
├── /financial/:year/:month              → Edição de métricas de um período
├── /financial/dashboard                 → Dashboard com MRR, ARR, Burn, Runway
└── /financial/periods                   → Lista de todos os períodos

🗂️ DOCUMENTOS
├── /documents                           → Repositório geral da empresa
├── /documents/upload                    → Upload de documento
└── /documents/:id                       → Detalhe do documento
    (+ DocumentManager como componente embutido em Valuation e Financial)
```

### 3.2 Componentes de Dashboard — KPI Cards

Cada dashboard terá um conjunto de **KPI Cards** no topo:

**Dashboard de Valuation:**
```
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│ Valuation Atual │  │ Variação vs Ant.│  │ Price per Share │  │ Qtd. Valuations │
│ R$ 12.000.000   │  │ ▲ +18,5%        │  │ R$ 4,20/ação    │  │ 8 aprovados     │
│ [aprovado]      │  │ [vs dez/2024]   │  │ [atualizado]    │  │ [2 pendentes]   │
└─────────────────┘  └─────────────────┘  └─────────────────┘  └─────────────────┘
```

**Dashboard Financeiro:**
```
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│ MRR Atual       │  │ ARR Projetado   │  │ Burn Rate       │  │ Runway          │
│ R$ 120.000      │  │ R$ 1.440.000    │  │ R$ 80.000/mês   │  │ 🟢 18 meses    │
│ ▲ +12% MoM      │  │ [MRR × 12]      │  │ ▼ -5% MoM       │  │ [saldo/burn]   │
└─────────────────┘  └─────────────────┘  └─────────────────┘  └─────────────────┘
```

### 3.3 Componentes de Dashboard — Gráficos

**Gráficos de Valuation (`/valuations/dashboard`):**

| Gráfico | Tipo | Dados | Biblioteca |
|---------|------|-------|------------|
| Evolução Histórica de Valuation | Line Chart | valuations aprovados ao longo do tempo | Recharts |
| Comparação de Metodologias | Bar Chart | valor calculado por cada metodologia do valuation mais recente | Recharts |
| Timeline de Eventos | Timeline | seed, series_a, internal, 409a com valores | Custom Component |
| Distribuição por Tipo de Evento | Pie Chart | contagem por event_type | Recharts |

**Gráficos Financeiros (`/financial/dashboard`):**

| Gráfico | Tipo | Dados | Biblioteca |
|---------|------|-------|------------|
| Evolução MRR/ARR (12 meses) | Line Chart | mrr e arr por mês | Recharts |
| Burn Rate Mensal | Bar Chart | burn_rate últimos 6 meses | Recharts |
| Runway Indicador | Gauge/Progress | meses restantes com código de cor | Custom |
| Crescimento de Clientes | Area Chart | customer_count por mês | Recharts |
| CAC vs LTV | Bar Chart agrupado | cac e ltv lado a lado | Recharts |

---

## 4. Estrutura de Menus e Navegação

### 4.1 Sidebar Principal — Novos Itens da Fase 5

```
SIDEBAR NAVIGATION (após Fase 5)

🏠 Dashboard Geral
👥 Sócios (Fase 2)
📊 Cap Table (Fase 2)
📝 Contratos (Fase 3)
🎯 Vesting (Fase 4)
── ── ── ── ── ── ── ── [NOVOS — FASE 5]
💰 Valuation                              ← NOVO
   ├── 📋 Lista de Valuations
   ├── ➕ Novo Valuation
   ├── 📊 Dashboard de Valuation
   └── 📜 Histórico
💹 Financeiro                             ← NOVO
   ├── 📅 Períodos Financeiros
   ├── 📈 Dashboard Financeiro
   └── 🗂️ Documentos da Empresa           ← NOVO (módulo de docs integrado)
── ── ── ── ── ── ── ── [ADMIN]
⚙️ Configurações
```

### 4.2 Breadcrumb e Roteamento

```
/ → Dashboard Geral
/valuations → Valuation > Lista
/valuations/new → Valuation > Novo Valuation
/valuations/:id → Valuation > [Nome/Data do Valuation]
/valuations/:id/edit → Valuation > [Nome] > Editar
/valuations/dashboard → Valuation > Dashboard
/financial → Financeiro > Períodos
/financial/dashboard → Financeiro > Dashboard
/financial/:year/:month → Financeiro > [Mês/Ano]
/documents → Documentos > Repositório
/documents/:id → Documentos > [Nome do Documento]
```

### 4.3 Permissões por Rota

| Rota | viewer | finance | admin | founder |
|------|--------|---------|-------|---------|
| /valuations (lista) | ✅ | ✅ | ✅ | ✅ |
| /valuations/new | ❌ | ✅ | ✅ | ✅ |
| /valuations/:id/approve | ❌ | ❌ | ✅ | ✅ |
| /financial (leitura) | ❌ | ✅ | ✅ | ✅ |
| /financial/:year/:month (editar) | ❌ | ✅ | ✅ | ✅ |
| /financial/:year/:month/approve | ❌ | ❌ | ✅ | ✅ |
| /documents (leitura) | ✅ | ✅ | ✅ | ✅ |
| /documents/upload | ❌ | ✅ | ✅ | ✅ |

---

## 5. Cadastros e Formulários

### 5.1 Formulário — Novo Valuation (Wizard 4 Steps)

**Step 1: Dados Básicos**
```
┌─────────────────────────────────────────────────────┐
│ NOVO VALUATION — Passo 1 de 4                       │
│                                                     │
│ Data do Valuation *          [📅 date picker]       │
│ Tipo de Evento *             [dropdown]             │
│   founding | seed | series_a | series_b |           │
│   series_c | internal | external | 409a | other     │
│ Nome do Evento               [text] ex: "Série A"   │
│ Total de Ações Considerado * [number]               │
│   (preenchido automaticamente com total do cap table│
│    mas editável para cenários hipotéticos)          │
│ Notas / Justificativa        [textarea]             │
│                                                     │
│              [Cancelar]    [Próximo →]              │
└─────────────────────────────────────────────────────┘
```

**Step 2: Metodologias de Cálculo**
```
┌─────────────────────────────────────────────────────┐
│ NOVO VALUATION — Passo 2 de 4: Metodologias         │
│                                                     │
│ [+ Adicionar Metodologia]                           │
│                                                     │
│ ┌── ARR Multiple ──────────────────────────────┐   │
│ │ ARR Atual (R$) *         [120.000,00]         │   │
│ │ Múltiplo *               [8x]                 │   │
│ │ Fonte de dados           [PitchBook Q4 2025]  │   │
│ │ Valor calculado:  ▶ R$ 960.000,00             │   │
│ │ [⭐ Selecionar como principal] [🗑️ Remover]   │   │
│ └───────────────────────────────────────────────┘  │
│                                                     │
│ ┌── DCF (Fluxo de Caixa Descontado) ────────────┐  │
│ │ Fluxo Ano 1 (R$) *       [150.000,00]         │  │
│ │ Fluxo Ano 2 (R$) *       [280.000,00]         │  │
│ │ Fluxo Ano 3 (R$) *       [450.000,00]         │  │
│ │ Taxa de Desconto (%) *   [25]                  │  │
│ │ Taxa de Crescimento (%)  [15]                  │  │
│ │ Valor calculado:  ▶ R$ 640.000,00              │  │
│ │ [Selecionar como principal] [🗑️ Remover]       │  │
│ └───────────────────────────────────────────────┘  │
│                                                     │
│ Metodologias disponíveis para adicionar:            │
│ [ARR Multiple] [DCF] [Comparáveis] [EBITDA]        │
│ [MRR Multiple] [Valor Patrimonial] [Berkus]         │
│                                                     │
│         [← Anterior]    [Próximo →]                 │
└─────────────────────────────────────────────────────┘
```

**Step 3: Resumo e Metodologia Principal**
```
┌─────────────────────────────────────────────────────┐
│ NOVO VALUATION — Passo 3 de 4: Resumo               │
│                                                     │
│ Metodologia Principal: ARR Multiple ⭐              │
│                                                     │
│ Comparativo de Metodologias:                        │
│ ┌───────────────────────┬─────────────────────┐    │
│ │ ARR Multiple ⭐        │ R$ 960.000,00        │    │
│ │ DCF                   │ R$ 640.000,00        │    │
│ │ Comparáveis           │ R$ 1.100.000,00      │    │
│ └───────────────────────┴─────────────────────┘    │
│                                                     │
│ Valuation Selecionado:    R$ 960.000,00             │
│ Total de Ações:           228.571                   │
│ Price per Share:          ▶ R$ 4,20                 │
│                                                     │
│ ⚠️ Após aprovação, o Cap Table será                 │
│ atualizado com o novo Price per Share.              │
│                                                     │
│         [← Anterior]    [Próximo →]                 │
└─────────────────────────────────────────────────────┘
```

**Step 4: Documentos de Suporte**
```
┌─────────────────────────────────────────────────────┐
│ NOVO VALUATION — Passo 4 de 4: Documentos           │
│                                                     │
│ Documentos de Suporte (opcional)                    │
│                                                     │
│ ┌─────────────────────────────────────────────┐    │
│ │  📎 Arraste arquivos aqui ou clique para     │    │
│ │     selecionar                               │    │
│ │  Formatos: PDF, XLSX, DOCX | Máx: 50MB       │    │
│ └─────────────────────────────────────────────┘    │
│                                                     │
│ Documentos adicionados:                             │
│ 📄 relatorio-valuation-jan2026.pdf   2,4MB  [✕]   │
│ 📊 comparaveis-mercado.xlsx          1,1MB  [✕]   │
│                                                     │
│         [← Anterior]    [✅ Salvar Rascunho]        │
│                         [📤 Enviar para Aprovação] │
└─────────────────────────────────────────────────────┘
```

### 5.2 Formulário — Métricas Financeiras (Período Mensal)

```
┌─────────────────────────────────────────────────────────────────┐
│ MÉTRICAS FINANCEIRAS — Janeiro 2026                              │
│                                                                   │
│ [◀ Dez/2025]   [Jan/2026] ● Draft   [Fev/2026 ▶]               │
│                                                                   │
│ ┌── RECEITA ──────────────────────────────────────────────────┐ │
│ │ Receita Bruta      R$ [__________]   vs Dez/25: R$ 108.000  │ │
│ │ Receita Líquida    R$ [__________]   variação: --           │ │
│ │ MRR                R$ [__________]   ▲ +12% (se preenchido) │ │
│ │ ARR (auto)         R$ [calculado]    = MRR × 12             │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                   │
│ ┌── CAIXA & BURN ─────────────────────────────────────────────┐ │
│ │ Saldo de Caixa     R$ [__________]                           │ │
│ │ Burn Rate          R$ [__________]   (valor mensal gasto)    │ │
│ │ Runway (auto)         [calculado]    = Saldo / Burn = X meses│ │
│ │   🟢 > 12m  🟡 6–12m  🔴 < 6m                               │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                   │
│ ┌── UNIT ECONOMICS ───────────────────────────────────────────┐ │
│ │ Número de Clientes  [________]                               │ │
│ │ Taxa de Churn (%)   [________]                               │ │
│ │ CAC                 R$ [______]                              │ │
│ │ LTV                 R$ [______]                              │ │
│ │ NPS                 [___]                                    │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                   │
│ ┌── LUCRATIVIDADE ────────────────────────────────────────────┐ │
│ │ EBITDA             R$ [__________]                           │ │
│ │ Margem EBITDA (%)  [calculado]    = EBITDA / Receita Líq.   │ │
│ │ Lucro Líquido      R$ [__________]                           │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                   │
│ Observações do Período  [textarea]                               │
│                                                                   │
│  [💾 Salvar Rascunho]    [📤 Submeter para Aprovação]           │
└─────────────────────────────────────────────────────────────────┘
```

### 5.3 Formulário — Upload de Documento

```
┌──────────────────────────────────────────────────────┐
│ ADICIONAR DOCUMENTO                                   │
│                                                      │
│ Nome do Documento *     [___________________________]│
│ Tipo de Documento *     [dropdown]                   │
│   balance_sheet | income_statement | cash_flow |     │
│   audit_report | contract | certificate | bylaws |   │
│   minutes | presentation | other                     │
│ Descrição               [textarea]                   │
│ Visibilidade *          [dropdown]                   │
│   admin | board | shareholders | investors | public  │
│ Referência (opcional)   [dropdown] Vinculado a:      │
│   [Valuation | Contrato | Período Financeiro | Geral]│
│                                                      │
│ ┌──────────────────────────────────────────────┐    │
│ │  📎 Arraste o arquivo ou clique para escolher │    │
│ │  PDF, XLSX, DOCX, PNG, JPG — Máx. 50MB       │    │
│ │  [████████░░] 80% — fazendo upload...         │    │
│ └──────────────────────────────────────────────┘    │
│                                                      │
│         [Cancelar]        [✅ Salvar Documento]      │
└──────────────────────────────────────────────────────┘
```

### 5.4 Tela — Lista de Valuations

```
VALUATIONS
──────────────────────────────────────────────────────────────────
[🔍 Buscar...]  [Tipo ▼]  [Status ▼]  [Data ▼]    [+ Novo Valuation]

Data        Evento      Valor           Price/Share  Metodologia    Status        Variação
──────────────────────────────────────────────────────────────────────────────────────────
Jan/26     Series A    R$ 12.000.000   R$ 4,20      ARR Multiple   ✅ Aprovado   ▲ +18,5%
Out/25     Internal    R$ 10.120.000   R$ 3,54      Comparáveis    ✅ Aprovado   ▲ +5,2%
Mar/25     Seed        R$ 9.620.000    R$ 3,37      DCF            ✅ Aprovado   —
Jan/25     Seed        R$ 5.000.000    R$ 1,75      ARR Multiple   ⏳ Aguardando  ▲ +8,3%
Dez/24     founding    R$ 1.000.000    R$ 0,35      Patrimonial    ✅ Aprovado   —

[1-5 de 12]  [← 1 2 3 →]
```

### 5.5 Tela — Detalhe de Valuation

```
VALUATION — Series A | Janeiro 2026
Status: ✅ APROVADO   Aprovado por: João Silva em 15/01/2026
──────────────────────────────────────────────────────────────────
Valor Oficial:     R$ 12.000.000
Price per Share:   R$ 4,20
Total de Ações:    2.857.143
Event Type:        Series A

METODOLOGIAS CALCULADAS
────────────────────────────────────────────────────────
⭐ ARR Multiple       R$ 12.000.000   [inputs: ARR=1.5M, múltiplo=8x]
   DCF                R$  9.800.000   [inputs: 3 anos de fluxo, 25% desconto]
   Comparáveis        R$ 13.200.000   [inputs: 5 empresas comparáveis]
   EBITDA Multiple    R$ 11.500.000   [inputs: EBITDA=800k, múltiplo=14x]

DOCUMENTOS ANEXADOS
────────────────────────────────────────────────────────
📄 relatorio-valuation-jan2026.pdf        2,4MB  ✅ Verificado   [⬇ Download]
📊 comparaveis-mercado-q4-2025.xlsx       1,1MB  ✅ Verificado   [⬇ Download]

HISTÓRICO DE AÇÕES
────────────────────────────────────────────────────────
15/01 14:32  João Silva    → Aprovou o valuation
14/01 10:15  Maria Souza   → Enviou para aprovação
13/01 09:00  Maria Souza   → Criou valuation como rascunho

[✏️ Editar] (apenas se draft)   [📋 Exportar PDF]
```

### 5.6 Tela — Calendário de Períodos Financeiros

```
PERÍODOS FINANCEIROS — 2026
──────────────────────────────────────────────────────────────────
[◀ 2025]                          2026                    [2027 ▶]

Jan/26   Fev/26   Mar/26   Abr/26   Mai/26   Jun/26
✅ Aprov  ✅ Aprov  ⏳ Draft  ○ Novo   ○ Novo   ○ Novo

Jul/26   Ago/26   Set/26   Out/26   Nov/26   Dez/26
○ Novo   ○ Novo   ○ Novo   ○ Novo   ○ Novo   ○ Novo

Legenda: ✅ Aprovado  🔒 Locked  📤 Submetido  ✏️ Draft  ○ Não iniciado

[Clique em um mês para editar suas métricas]
```

---

## 6. Pré-Requisitos e Dependências

### 6.1 Dependências Técnicas

| Dependência | Status | Criticidade |
|-------------|--------|-------------|
| Fase 2 (Cap Table) — tabelas `shares`, `share_classes`, `shareholders` | ✅ Concluída | **CRÍTICA** |
| Fase 4 (Vesting) — tabelas `vesting_grants` para diluted shares | ✅ Concluída | Alta |
| Fase 3 (Contratos) — padrão de DocumentService para upload | ✅ Concluída | Alta |
| Sistema de permissões e roles | ✅ Implementado | Alta |
| Audit trail | ✅ Implementado | Alta |
| Firebase Storage configurado (para upload de arquivos) | ⚠️ Verificar | Alta |
| Redis Cache disponível | ✅ Disponível | Média |
| Recharts instalado no frontend | ⚠️ Verificar | Alta |

### 6.2 Pré-Configurações antes de Começar

```bash
# 1. Verificar que o projeto compila sem erros
cd src/backend && dotnet build --no-incremental 2>&1 | tail -5

# 2. Verificar migration mais recente (descobrir próximo número)
ls database/migrations/ | sort | tail -5

# 3. Verificar frontend
cd src/frontend && npm run build 2>&1 | tail -5

# 4. Verificar que Recharts está instalado
cd src/frontend && npm list recharts

# 5. Se não instalado:
cd src/frontend && npm install recharts @types/recharts

# 6. Verificar entidades existentes que SERÃO REUTILIZADAS
ls src/backend/PartnershipManager.Domain/Entities/ | grep -i "share\|company\|user"
```

---

## 7. Nomenclatura das Tarefas Atômicas

**Padrão:** `F5-[MÓDULO]-[TIPO]-[NÚMERO]`

| Código | Módulo |
|--------|--------|
| `F5` | Fase 5 |
| `VLT` | Valuation (entidade principal) |
| `VMT` | ValuationMethod (metodologias) |
| `FIN` | Financial Period + Metrics |
| `DOC` | Documents (repositório de arquivos) |
| `ENG` | Engine de Cálculo |
| `INT` | Integração (Cap Table) |
| `DSH` | Dashboard |
| `TST` | Testes |

| Tipo | Descrição |
|------|-----------|
| `DB` | Database Migration |
| `BE` | Backend (Entity, Repo, Service, Controller) |
| `FE` | Frontend (Types, Service, Hook, Component, Page) |
| `TST` | Testes (unitários, integração) |
| `CFG` | Configuração / DI / Registro |

**Estimativas padrão:**

| Tipo | Estimativa |
|------|-----------|
| DB Migration | 3–5h |
| Backend Entity + Enum | 3–5h |
| Backend Repository (Dapper) | 5–8h |
| Backend Service com regras de negócio | 8–14h |
| Backend Controller (REST) | 4–6h |
| Backend Validator (FluentValidation) | 2–4h |
| Frontend Types TypeScript | 2–3h |
| Frontend Service API Client | 3–5h |
| Frontend Hook (React Query) | 2–4h |
| Frontend Component | 4–8h |
| Frontend Page (layout completo) | 6–10h |
| Testes Unitários | 4–8h |
| Testes de Integração | 5–8h |

---

## 8. Semana 1: Database + Backend Foundation

**Meta:** Estrutura de dados + Entidades base  
**Prazo:** Semana 1 (40h)  
**Status:** ⚪ Não Iniciado  

---

### F5-DB-001: Migrations — Módulo Valuation

- **Status:** [ ] Pendente
- **Tipo:** Database Migration
- **Estimativa:** 6h
- **Responsável:** DBA / BE1
- **Dependências:** Fase 2 concluída (tabela `share_classes` deve existir)

**Arquivos a criar:**
```
/database/migrations/037_create_valuations_table.sql
/database/migrations/038_create_valuation_methods_table.sql
/database/migrations/039_create_valuation_documents_table.sql
```

**SQL de referência — 037_create_valuations_table.sql:**
```sql
-- UP
CREATE TABLE valuations (
    id          CHAR(36)        NOT NULL PRIMARY KEY DEFAULT (UUID()),
    company_id  CHAR(36)        NOT NULL,
    valuation_amount    DECIMAL(18,2)   NOT NULL CHECK (valuation_amount > 0),
    price_per_share     DECIMAL(15,4)   NOT NULL CHECK (price_per_share > 0),
    total_shares        DECIMAL(15,0)   NOT NULL CHECK (total_shares > 0),
    valuation_date      DATE            NOT NULL,
    event_type          ENUM('founding','seed','series_a','series_b','series_c','internal','external','409a','other') NOT NULL,
    event_name          VARCHAR(200)    NULL,
    selected_method_id  CHAR(36)        NULL,
    variation_percentage DECIMAL(8,2)   NULL,
    status              ENUM('draft','pending_approval','approved','superseded') NOT NULL DEFAULT 'draft',
    notes               TEXT            NULL,
    approved_by         CHAR(36)        NULL,
    approved_at         TIMESTAMP       NULL,
    created_by          CHAR(36)        NOT NULL,
    created_at          TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at          TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    deleted_at          TIMESTAMP       NULL,
    CONSTRAINT fk_valuation_company FOREIGN KEY (company_id) REFERENCES companies(id),
    CONSTRAINT fk_valuation_approved_by FOREIGN KEY (approved_by) REFERENCES users(id),
    INDEX idx_valuation_company (company_id),
    INDEX idx_valuation_date (valuation_date),
    INDEX idx_valuation_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- DOWN
DROP TABLE IF EXISTS valuations;
```

**SQL de referência — 038_create_valuation_methods_table.sql:**
```sql
-- UP
CREATE TABLE valuation_methods (
    id              CHAR(36)        NOT NULL PRIMARY KEY DEFAULT (UUID()),
    valuation_id    CHAR(36)        NOT NULL,
    method_type     ENUM('arr_multiple','mrr_multiple','ebitda_multiple','dcf','comparable','book_value','replacement','other') NOT NULL,
    calculated_value DECIMAL(18,2)  NOT NULL,
    inputs          JSON            NOT NULL,
    formula_details JSON            NULL,
    is_selected     TINYINT(1)      NOT NULL DEFAULT 0,
    notes           TEXT            NULL,
    created_at      TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_method_valuation FOREIGN KEY (valuation_id) REFERENCES valuations(id) ON DELETE CASCADE,
    INDEX idx_method_valuation (valuation_id),
    INDEX idx_method_selected (valuation_id, is_selected)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- DOWN
DROP TABLE IF EXISTS valuation_methods;
```

**Critérios de Aceite:**
- [ ] 3 tabelas criadas sem erros
- [ ] Foreign keys configuradas e testadas
- [ ] Índices criados conforme especificação do MER
- [ ] CHECK constraints validando dados negativos
- [ ] Scripts DOWN (rollback) funcionais para cada migration
- [ ] `mysql -e "DESCRIBE valuations;"` retorna todas as colunas

---

### F5-DB-002: Migrations — Módulo Financeiro

- **Status:** [ ] Pendente
- **Tipo:** Database Migration
- **Estimativa:** 5h
- **Responsável:** DBA / BE1
- **Dependências:** F5-DB-001

**Arquivos a criar:**
```
/database/migrations/040_create_financial_periods_table.sql
/database/migrations/041_create_financial_metrics_table.sql
```

**Notas importantes:**
- `financial_periods`: UNIQUE INDEX em `(company_id, year, month)` — regra FI-01
- `financial_metrics`: sem UNIQUE — múltiplas métricas por período
- Status: `draft` → `submitted` → `approved` → `locked`

**Critérios de Aceite:**
- [ ] UNIQUE INDEX `idx_period_company_date` em `(company_id, year, month)` funcional
- [ ] ENUM de `metric_type` contém todos os tipos do MER
- [ ] ENUM de `metric_category` contém todos os tipos do MER
- [ ] Foreign keys para `companies` e `users` corretas
- [ ] Scripts DOWN funcionais

---

### F5-DB-003: Migration — Repositório de Documentos

- **Status:** [ ] Pendente
- **Tipo:** Database Migration
- **Estimativa:** 4h
- **Responsável:** DBA / BE1
- **Dependências:** F5-DB-001

**Arquivos a criar:**
```
/database/migrations/042_create_documents_table.sql
```

**Notas importantes:**
- Tabela polimórfica: `reference_type` (ex: 'valuation', 'contract', 'financial_period') + `reference_id`
- Checksum SHA-256 obrigatório para integridade
- `visibility` controla quem pode ver o documento

**Critérios de Aceite:**
- [ ] Tabela criada com todos os campos do MER
- [ ] INDEX em `(company_id, reference_type, reference_id)` para busca por contexto
- [ ] Script DOWN funcional

---

### F5-VLT-BE-001: Entity Valuation + Enums

- **Status:** [ ] Pendente
- **Tipo:** Backend Entity
- **Estimativa:** 4h
- **Responsável:** BE1
- **Dependências:** F5-DB-001

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Domain/Entities/Valuation.cs
/src/backend/PartnershipManager.Domain/Enums/ValuationStatus.cs
/src/backend/PartnershipManager.Domain/Enums/ValuationEventType.cs
```

**Padrão obrigatório:**
```csharp
// Valuation.cs — DEVE herdar de BaseEntity
public class Valuation : BaseEntity
{
    public Guid CompanyId { get; set; }
    public decimal ValuationAmount { get; set; }
    public decimal PricePerShare { get; set; }
    public decimal TotalShares { get; set; }
    public DateTime ValuationDate { get; set; }
    public ValuationEventType EventType { get; set; }
    public string? EventName { get; set; }
    public Guid? SelectedMethodId { get; set; }
    public decimal? VariationPercentage { get; set; }
    public ValuationStatus Status { get; set; } = ValuationStatus.Draft;
    public string? Notes { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    
    // Navigation properties
    public Company? Company { get; set; }
    public ICollection<ValuationMethod> Methods { get; set; } = new List<ValuationMethod>();
}
```

**Critérios de Aceite:**
- [ ] Herança de `BaseEntity` (com `CreatedBy`, `CreatedAt`, `UpdatedAt`, `DeletedAt`)
- [ ] Enum `ValuationStatus`: `Draft`, `PendingApproval`, `Approved`, `Superseded`
- [ ] Enum `ValuationEventType`: `Founding`, `Seed`, `SeriesA`, `SeriesB`, `SeriesC`, `Internal`, `External`, `A409`, `Other`
- [ ] Navigation properties com `ICollection<ValuationMethod>`
- [ ] Propriedades com comentários XML doc

---

### F5-VLT-BE-002: Entity ValuationMethod + Entity ValuationDocument + Enum

- **Status:** [ ] Pendente
- **Tipo:** Backend Entity
- **Estimativa:** 4h
- **Responsável:** BE1
- **Dependências:** F5-VLT-BE-001

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Domain/Entities/ValuationMethod.cs
/src/backend/PartnershipManager.Domain/Entities/ValuationDocument.cs
/src/backend/PartnershipManager.Domain/Enums/ValuationMethodType.cs
```

**Nota:** `Inputs` e `FormulaDetails` devem ser `Dictionary<string, object?>` serializados como JSON pelo repositório.

**Critérios de Aceite:**
- [ ] Enum `ValuationMethodType`: `ArrMultiple`, `MrrMultiple`, `EbitdaMultiple`, `Dcf`, `Comparable`, `BookValue`, `Replacement`, `Other`
- [ ] `IsSelected` propriedade booleana com regra de negócio: só 1 por valuation
- [ ] `ValuationDocument` com campos `ValuationId`, `DocumentId`, `DocumentType`, `IsRequired`

---

### F5-FIN-BE-001: Entities FinancialPeriod + FinancialMetric + Enums

- **Status:** [ ] Pendente
- **Tipo:** Backend Entity
- **Estimativa:** 5h
- **Responsável:** BE2
- **Dependências:** F5-DB-002

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Domain/Entities/FinancialPeriod.cs
/src/backend/PartnershipManager.Domain/Entities/FinancialMetric.cs
/src/backend/PartnershipManager.Domain/Enums/FinancialPeriodStatus.cs
/src/backend/PartnershipManager.Domain/Enums/MetricCategory.cs
/src/backend/PartnershipManager.Domain/Enums/MetricType.cs
/src/backend/PartnershipManager.Domain/Enums/MetricUnit.cs
```

**Critérios de Aceite:**
- [ ] `FinancialPeriod` herda `BaseEntity`
- [ ] `FinancialPeriodStatus`: `Draft`, `Submitted`, `Approved`, `Locked`
- [ ] `MetricCategory`: `Revenue`, `Profitability`, `Cash`, `Operational`, `Growth`, `Other`
- [ ] `MetricType`: `GrossRevenue`, `NetRevenue`, `Mrr`, `Arr`, `Ebitda`, `EbitdaMargin`, `NetIncome`, `CashBalance`, `BurnRate`, `Runway`, `CustomerCount`, `ChurnRate`, `Nps`, `Cac`, `Ltv`, `Other`
- [ ] `MetricUnit`: `Currency`, `Percentage`, `Number`, `Months`

---

### F5-DOC-BE-001: Entity Document + Enums

- **Status:** [ ] Pendente
- **Tipo:** Backend Entity
- **Estimativa:** 4h
- **Responsável:** BE2
- **Dependências:** F5-DB-003

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Domain/Entities/Document.cs
/src/backend/PartnershipManager.Domain/Enums/DocumentCategory.cs
/src/backend/PartnershipManager.Domain/Enums/DocumentVisibility.cs
/src/backend/PartnershipManager.Domain/Enums/DocumentVerificationStatus.cs
```

**Critérios de Aceite:**
- [ ] `DocumentCategory`: `BalanceSheet`, `IncomeStatement`, `CashFlow`, `BankStatement`, `AuditReport`, `Contract`, `Certificate`, `Bylaws`, `Minutes`, `Presentation`, `Other`
- [ ] `DocumentVisibility`: `Public`, `Investors`, `Shareholders`, `Employees`, `Board`, `Admin`, `Private`
- [ ] `VerificationStatus`: `Pending`, `Verified`, `Rejected`
- [ ] Campos `ReferenceType` (string) e `ReferenceId` (Guid?) para relação polimórfica

---

### F5-VLT-BE-003: Repository IValuationRepository + ValuationRepository

- **Status:** [ ] Pendente
- **Tipo:** Backend Repository
- **Estimativa:** 8h
- **Responsável:** BE1
- **Dependências:** F5-VLT-BE-002

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Domain/Interfaces/IValuationRepository.cs
/src/backend/PartnershipManager.Infrastructure/Repositories/ValuationRepository.cs
```

**Métodos obrigatórios:**
```csharp
Task<Valuation?> GetByIdAsync(Guid id, Guid companyId);
Task<Valuation?> GetByIdWithMethodsAsync(Guid id, Guid companyId);
Task<PagedResult<Valuation>> GetByCompanyAsync(Guid companyId, PaginationRequest request, ValuationStatus? status = null);
Task<Valuation?> GetLatestApprovedAsync(Guid companyId);
Task<IEnumerable<Valuation>> GetHistoryAsync(Guid companyId, int limit = 24);
Task<Guid> CreateAsync(Valuation valuation);
Task<bool> UpdateStatusAsync(Guid id, ValuationStatus status, Guid? approvedBy = null);
Task<bool> SupersedeAllAsync(Guid companyId); // marca anteriores como 'superseded'
Task<IEnumerable<ValuationMethod>> GetMethodsByValuationAsync(Guid valuationId);
Task AddMethodAsync(ValuationMethod method);
Task<bool> SetSelectedMethodAsync(Guid valuationId, Guid methodId);
```

**Critérios de Aceite:**
- [ ] Todos os queries filtram por `company_id` (multi-tenancy)
- [ ] Todos os queries filtram por `deleted_at IS NULL` (soft delete)
- [ ] `GetByIdWithMethodsAsync` usa JOIN para carregar metodologias
- [ ] `GetHistoryAsync` ordena por `valuation_date DESC`
- [ ] Queries Dapper com parâmetros nomeados (sem concatenação de string)

---

### F5-FIN-BE-002: Repository IFinancialPeriodRepository + FinancialPeriodRepository

- **Status:** [ ] Pendente
- **Tipo:** Backend Repository
- **Estimativa:** 7h
- **Responsável:** BE2
- **Dependências:** F5-FIN-BE-001

**Métodos obrigatórios:**
```csharp
Task<FinancialPeriod?> GetByPeriodAsync(Guid companyId, int year, int month);
Task<FinancialPeriod?> GetWithMetricsAsync(Guid companyId, int year, int month);
Task<IEnumerable<FinancialPeriod>> GetRangeAsync(Guid companyId, int fromYear, int fromMonth, int toYear, int toMonth);
Task<Guid> CreateAsync(FinancialPeriod period);
Task<bool> UpdateStatusAsync(Guid id, FinancialPeriodStatus status, Guid? actionBy = null);
Task UpsertMetricAsync(FinancialMetric metric); // INSERT ON DUPLICATE KEY UPDATE
Task<IEnumerable<FinancialMetric>> GetMetricsByPeriodAsync(Guid periodId);
Task<FinancialMetric?> GetPreviousMetricAsync(Guid companyId, MetricType type, int year, int month);
```

**Critérios de Aceite:**
- [ ] `UpsertMetricAsync` é idempotente (pode chamar N vezes com o mesmo período)
- [ ] `GetPreviousMetricAsync` busca o mês anterior para calcular variação
- [ ] `GetRangeAsync` funciona cruzando anos (ex: Nov/25 a Mar/26)

---

### F5-DOC-BE-002: Repository IDocumentRepository + DocumentRepository

- **Status:** [ ] Pendente
- **Tipo:** Backend Repository
- **Estimativa:** 6h
- **Responsável:** BE2
- **Dependências:** F5-DOC-BE-001

**Métodos obrigatórios:**
```csharp
Task<Document?> GetByIdAsync(Guid id, Guid companyId);
Task<IEnumerable<Document>> GetByReferenceAsync(Guid companyId, string referenceType, Guid referenceId);
Task<PagedResult<Document>> GetByCompanyAsync(Guid companyId, PaginationRequest request, DocumentCategory? category = null);
Task<Guid> CreateAsync(Document document);
Task<bool> UpdateVerificationAsync(Guid id, DocumentVerificationStatus status, Guid verifiedBy);
Task<bool> SoftDeleteAsync(Guid id, Guid deletedBy);
```

**Critérios de Aceite:**
- [ ] `GetByReferenceAsync` busca documentos pelo contexto (ex: todos docs de um valuation)
- [ ] Soft delete usando `deleted_at`
- [ ] Busca sempre filtra `company_id`

---

## 9. Semana 2: Backend Services + APIs

**Meta:** Lógica de negócio + Endpoints REST completos  
**Prazo:** Semana 2 (40h)  
**Status:** ⚪ Não Iniciado  

---

### F5-ENG-BE-001: ValuationCalculationEngine (Motor de Cálculo — CRÍTICO)

- **Status:** [ ] Pendente
- **Tipo:** Backend Engine / Service
- **Estimativa:** 14h ⚠️ (tarefa mais complexa da fase)
- **Responsável:** BE1 (sênior)
- **Dependências:** F5-VLT-BE-002

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Application/Services/ValuationCalculationEngine.cs
/src/backend/PartnershipManager.Application/Models/ValuationCalculationInput.cs
/src/backend/PartnershipManager.Application/Models/ValuationCalculationResult.cs
```

**Metodologias a implementar:**

| Metodologia | Fórmula | Inputs necessários |
|-------------|---------|-------------------|
| **ARR Multiple** | `ARR × múltiplo` | `arr`, `multiple` |
| **MRR Multiple** | `MRR × múltiplo × 12` | `mrr`, `multiple` |
| **EBITDA Multiple** | `EBITDA × múltiplo` | `ebitda`, `multiple` |
| **DCF** | `Σ (FCt / (1+r)^t)` | `cashFlows[]`, `discountRate`, `growthRate` |
| **Comparable Companies** | `média ponderada dos comparáveis` | `comparables[]` com `value` e `weight` |
| **Book Value** | `patrimônio líquido` | `totalAssets`, `totalLiabilities` |
| **Replacement Cost** | `custo de reposição dos ativos` | `assetCosts[]` |

**Padrão de implementação:**
```csharp
public class ValuationCalculationEngine
{
    // Usar SEMPRE decimal, nunca double ou float
    public decimal CalculateArrMultiple(decimal arr, decimal multiple)
        => Math.Round(arr * multiple, 2, MidpointRounding.AwayFromZero);
        
    public decimal CalculateDcf(decimal[] cashFlows, decimal discountRate, int years)
    {
        decimal result = 0;
        for (int t = 1; t <= years; t++)
            result += cashFlows[t-1] / (decimal)Math.Pow((double)(1 + discountRate), t);
        return Math.Round(result, 2, MidpointRounding.AwayFromZero);
    }
    
    public decimal CalculatePricePerShare(decimal valuationAmount, decimal totalShares)
        => totalShares == 0 ? 0 : Math.Round(valuationAmount / totalShares, 4, MidpointRounding.AwayFromZero);
}
```

**Critérios de Aceite:**
- [ ] 7 metodologias implementadas
- [ ] NUNCA usar `double` ou `float` — somente `decimal`
- [ ] `MidpointRounding.AwayFromZero` em todos os `Math.Round`
- [ ] Classe testável (sem dependências de infraestrutura — pura lógica)
- [ ] Cobertura de testes unitários ≥ 90% para este engine
- [ ] Cada método documentado com XML doc e exemplo de cálculo

---

### F5-VLT-BE-004: ValuationService — Orquestração e Workflow

- **Status:** [ ] Pendente
- **Tipo:** Backend Service
- **Estimativa:** 10h
- **Responsável:** BE1
- **Dependências:** F5-ENG-BE-001, F5-VLT-BE-003

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Application/Services/ValuationService.cs
/src/backend/PartnershipManager.Application/Interfaces/IValuationService.cs
/src/backend/PartnershipManager.Application/DTOs/Valuation/CreateValuationRequest.cs
/src/backend/PartnershipManager.Application/DTOs/Valuation/ValuationResponse.cs
/src/backend/PartnershipManager.Application/DTOs/Valuation/ValuationListItemResponse.cs
/src/backend/PartnershipManager.Application/DTOs/Valuation/AddMethodRequest.cs
/src/backend/PartnershipManager.Application/DTOs/Valuation/ApproveValuationRequest.cs
/src/backend/PartnershipManager.Application/Validators/Valuation/CreateValuationValidator.cs
/src/backend/PartnershipManager.Application/Validators/Valuation/AddMethodValidator.cs
```

**Lógica crítica — ApproveAsync:**
```csharp
public async Task<ValuationResponse> ApproveAsync(Guid id, Guid companyId, Guid approvedById)
{
    // 1. Verificar que o aprovador ≠ o criador (segregação de funções — regra VA-02)
    // 2. Verificar que existe exatamente 1 metodologia selecionada (regra VA-01)
    // 3. Calcular variation_percentage vs valuation anterior aprovado
    // 4. Atualizar status para Approved
    // 5. Marcar valuations anteriores como Superseded (regra VA-03 — só 1 aprovado por vez)
    // 6. Chamar UpdateCapTablePriceAsync — INTEGRAÇÃO CRÍTICA
    // 7. Registrar no AuditLog
}
```

**Critérios de Aceite:**
- [ ] `CreateAsync`: calcula `PricePerShare` automaticamente = ValuationAmount / TotalShares
- [ ] `ApproveAsync`: segregação de funções (criador ≠ aprovador)
- [ ] `ApproveAsync`: valida exatamente 1 metodologia `IsSelected = true`
- [ ] `ApproveAsync`: marca anteriores como `Superseded`
- [ ] `ApproveAsync`: dispara atualização do Cap Table
- [ ] FluentValidation com mensagens usando `ErrorMessages` (sem hardcode)
- [ ] Audit log registrado em cada mudança de status

---

### F5-INT-BE-001: Integração Valuation ↔ Cap Table

- **Status:** [ ] Pendente
- **Tipo:** Backend Integration
- **Estimativa:** 5h
- **Responsável:** BE1
- **Dependências:** F5-VLT-BE-004
- **⚠️ ATENÇÃO:** Verificar ANTES como `share_classes` está estruturado na Fase 2

**Arquivos a modificar/criar:**
```
/src/backend/PartnershipManager.Application/Services/ValuationService.cs
  → adicionar método UpdateCapTablePriceAsync

/src/backend/PartnershipManager.Domain/Interfaces/IShareClassRepository.cs
  → verificar se UpdatePricePerShareAsync já existe; se não, adicionar

/src/backend/PartnershipManager.Infrastructure/Repositories/ShareClassRepository.cs
  → implementar UpdatePricePerShareAsync se necessário
```

**Lógica:**
```csharp
private async Task UpdateCapTablePriceAsync(Valuation approved, Guid updatedBy)
{
    // 1. Buscar todas as share_classes da empresa
    // 2. Atualizar price_per_share = approved.PricePerShare
    // 3. Criar ShareTransaction do tipo 'adjustment' com referência ao valuation
    // 4. Registrar no AuditLog: "Price per share atualizado de R$ X para R$ Y via Valuation #ID"
    // 5. Em caso de QUALQUER erro: reverter a aprovação do valuation (transação)
}
```

**Critérios de Aceite:**
- [ ] Operação em transação de banco de dados (rollback em caso de erro)
- [ ] AuditLog com valor anterior e novo
- [ ] Funciona mesmo que existam múltiplas share_classes
- [ ] Teste de regressão: verificar que o cap table ainda está consistente após atualização

---

### F5-VLT-BE-005: ValuationsController — REST API

- **Status:** [ ] Pendente
- **Tipo:** Backend Controller
- **Estimativa:** 5h
- **Responsável:** BE1
- **Dependências:** F5-VLT-BE-004

**Arquivo a criar:**
```
/src/backend/PartnershipManager.API/Controllers/ValuationsController.cs
```

**Endpoints obrigatórios:**
```
GET    /api/v1/companies/{companyId}/valuations              → lista paginada (filtros: status, event_type)
POST   /api/v1/companies/{companyId}/valuations              → criar valuation (draft)
GET    /api/v1/companies/{companyId}/valuations/{id}         → detalhe com metodologias
PUT    /api/v1/companies/{companyId}/valuations/{id}         → editar (somente draft)
POST   /api/v1/companies/{companyId}/valuations/{id}/methods → adicionar metodologia
PUT    /api/v1/companies/{companyId}/valuations/{id}/submit  → enviar para aprovação
PUT    /api/v1/companies/{companyId}/valuations/{id}/approve → aprovar
PUT    /api/v1/companies/{companyId}/valuations/{id}/reject  → rejeitar (com motivo)
GET    /api/v1/companies/{companyId}/valuations/latest       → valuation aprovado mais recente
GET    /api/v1/companies/{companyId}/valuations/history      → histórico para dashboard
DELETE /api/v1/companies/{companyId}/valuations/{id}         → soft delete (somente draft)
```

**Critérios de Aceite:**
- [ ] `[Authorize]` em todos os endpoints
- [ ] `[Authorize(Roles = "Admin,Founder")]` nos endpoints de aprovação/rejeição
- [ ] Swagger/OpenAPI documentado para cada endpoint
- [ ] `User.GetCompanyId()` usado (não aceitar `companyId` do body)
- [ ] Respostas HTTP corretas: 200, 201, 400, 403, 404, 409

---

### F5-FIN-BE-003: FinancialService + Controller

- **Status:** [ ] Pendente
- **Tipo:** Backend Service + Controller
- **Estimativa:** 12h
- **Responsável:** BE2
- **Dependências:** F5-FIN-BE-002

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Application/Services/FinancialService.cs
/src/backend/PartnershipManager.Application/Interfaces/IFinancialService.cs
/src/backend/PartnershipManager.Application/DTOs/Financial/FinancialPeriodResponse.cs
/src/backend/PartnershipManager.Application/DTOs/Financial/FinancialMetricRequest.cs
/src/backend/PartnershipManager.Application/DTOs/Financial/FinancialDashboardResponse.cs
/src/backend/PartnershipManager.Application/Validators/Financial/FinancialMetricValidator.cs
/src/backend/PartnershipManager.API/Controllers/FinancialPeriodsController.cs
```

**Endpoints obrigatórios:**
```
GET    /api/v1/companies/{companyId}/financial-periods                     → lista todos os períodos
GET    /api/v1/companies/{companyId}/financial-periods/{year}/{month}      → período com métricas
POST   /api/v1/companies/{companyId}/financial-periods                     → criar período
PUT    /api/v1/companies/{companyId}/financial-periods/{id}/metrics        → salvar métricas (batch upsert)
POST   /api/v1/companies/{companyId}/financial-periods/{id}/submit         → submeter para aprovação
POST   /api/v1/companies/{companyId}/financial-periods/{id}/approve        → aprovar
GET    /api/v1/companies/{companyId}/financial-periods/dashboard           → dados do dashboard (12 meses)
```

**Cálculos automáticos no Service:**
```
ARR = MRR × 12
Runway = CashBalance / BurnRate  (em meses, sem casas decimais)
EbitdaMargin = (EBITDA / NetRevenue) × 100  (em percentual)
Variation = (valorAtual - valorAnterior) / valorAnterior × 100
VariationAbsolute = valorAtual - valorAnterior
```

**Critérios de Aceite:**
- [ ] `UpsertMetricAsync` aceita lista de métricas (batch) — evitar N+1 requests
- [ ] Cálculos automáticos (ARR, Runway, Margins) feitos no Service, não no frontend
- [ ] Regra FI-02: validar que período anterior está aprovado antes de submeter atual
- [ ] Regra FI-03: período `Locked` — retornar 409 Conflict em tentativa de edição
- [ ] Cache Redis para `GetDashboardDataAsync` com TTL de 1 hora
- [ ] Cache invalidado ao salvar novas métricas

---

### F5-DOC-BE-003: DocumentService + Controller

- **Status:** [ ] Pendente
- **Tipo:** Backend Service + Controller
- **Estimativa:** 8h
- **Responsável:** BE2
- **Dependências:** F5-DOC-BE-002

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Application/Services/DocumentService.cs
/src/backend/PartnershipManager.Application/Interfaces/IDocumentService.cs
/src/backend/PartnershipManager.Application/DTOs/Document/UploadDocumentRequest.cs
/src/backend/PartnershipManager.Application/DTOs/Document/DocumentResponse.cs
/src/backend/PartnershipManager.API/Controllers/DocumentsController.cs
```

**Endpoints obrigatórios:**
```
GET    /api/v1/companies/{companyId}/documents                              → lista paginada (filtros: category, reference)
POST   /api/v1/companies/{companyId}/documents/upload                       → upload (multipart/form-data)
GET    /api/v1/companies/{companyId}/documents/{id}                         → detalhe
GET    /api/v1/companies/{companyId}/documents/{id}/download-url            → URL assinada (Firebase)
PUT    /api/v1/companies/{companyId}/documents/{id}/verify                  → verificar documento
DELETE /api/v1/companies/{companyId}/documents/{id}                         → soft delete
GET    /api/v1/companies/{companyId}/documents?ref_type=valuation&ref_id=X  → docs de um valuation
```

**Critérios de Aceite:**
- [ ] Upload calcula e armazena SHA-256 checksum
- [ ] Limite de 50MB por arquivo validado no backend
- [ ] Tipos MIME permitidos: `application/pdf`, `image/*`, `application/vnd.ms-excel`, `application/vnd.openxmlformats*`, `application/msword`
- [ ] URL de download é assinada com tempo de expiração (Firebase signed URL)
- [ ] Soft delete com `deleted_at`

---

## 10. Semana 3: Frontend Core

**Meta:** Interfaces funcionais de Valuation e Financeiro  
**Prazo:** Semana 3 (40h)  
**Status:** ⚪ Não Iniciado  

---

### F5-VLT-FE-001: Types TypeScript para Valuation

- **Status:** [ ] Pendente
- **Tipo:** Frontend Types
- **Estimativa:** 3h
- **Responsável:** FE1
- **Dependências:** F5-VLT-BE-005 (APIs definidas)

**Arquivo a criar:**
```
/src/frontend/src/types/valuation.ts
```

**Conteúdo obrigatório:**
```typescript
export type ValuationStatus = 'draft' | 'pending_approval' | 'approved' | 'superseded';
export type ValuationEventType = 'founding' | 'seed' | 'series_a' | 'series_b' | 'series_c' | 'internal' | 'external' | '409a' | 'other';
export type ValuationMethodType = 'arr_multiple' | 'mrr_multiple' | 'ebitda_multiple' | 'dcf' | 'comparable' | 'book_value' | 'replacement' | 'other';

export interface ValuationMethod {
  id: string;
  valuationId: string;
  methodType: ValuationMethodType;
  calculatedValue: number;
  inputs: Record<string, unknown>;
  formulaDetails?: Record<string, unknown>;
  isSelected: boolean;
  notes?: string;
}

export interface Valuation {
  id: string;
  companyId: string;
  valuationAmount: number;
  pricePerShare: number;
  totalShares: number;
  valuationDate: string; // ISO date
  eventType: ValuationEventType;
  eventName?: string;
  selectedMethodId?: string;
  variationPercentage?: number;
  status: ValuationStatus;
  notes?: string;
  approvedBy?: string;
  approvedAt?: string;
  methods: ValuationMethod[];
  createdAt: string;
  createdBy: string;
}

export interface ValuationListItem {
  id: string;
  valuationAmount: number;
  pricePerShare: number;
  valuationDate: string;
  eventType: ValuationEventType;
  eventName?: string;
  status: ValuationStatus;
  variationPercentage?: number;
  selectedMethodType?: ValuationMethodType;
}

export interface CreateValuationRequest {
  valuationDate: string;
  eventType: ValuationEventType;
  eventName?: string;
  totalShares: number;
  notes?: string;
}

export interface AddMethodRequest {
  methodType: ValuationMethodType;
  inputs: Record<string, unknown>;
  notes?: string;
}
```

**Critérios de Aceite:**
- [ ] Todos os types batem exatamente com a API do backend
- [ ] Exportados via `src/types/index.ts`
- [ ] Sem `any` — usar `unknown` quando necessário

---

### F5-VLT-FE-002: useValuation Hook + ValuationService

- **Status:** [ ] Pendente
- **Tipo:** Frontend Service + Hook
- **Estimativa:** 5h
- **Responsável:** FE1
- **Dependências:** F5-VLT-FE-001

**Arquivos a criar:**
```
/src/frontend/src/services/valuationService.ts
/src/frontend/src/hooks/useValuation.ts
/src/frontend/src/hooks/useValuationHistory.ts
```

**Service — funções obrigatórias:**
```typescript
// valuationService.ts
export const valuationService = {
  getAll: (companyId: string, params?: PaginationParams & ValuationFilters) => api.get(...),
  getById: (companyId: string, id: string) => api.get(...),
  getLatest: (companyId: string) => api.get(...),
  getHistory: (companyId: string) => api.get(...),
  create: (companyId: string, data: CreateValuationRequest) => api.post(...),
  addMethod: (companyId: string, valuationId: string, data: AddMethodRequest) => api.post(...),
  setSelectedMethod: (companyId: string, valuationId: string, methodId: string) => api.put(...),
  submit: (companyId: string, id: string) => api.put(...),
  approve: (companyId: string, id: string) => api.put(...),
  reject: (companyId: string, id: string, reason: string) => api.put(...),
};
```

**Critérios de Aceite:**
- [ ] React Query com `useQuery` para dados e `useMutation` para ações
- [ ] Invalidação de cache após mutations (`queryClient.invalidateQueries`)
- [ ] Toast notifications de sucesso/erro
- [ ] Loading states expostos (isLoading, isError)

---

### F5-VLT-FE-003: Página Lista de Valuations

- **Status:** [ ] Pendente
- **Tipo:** Frontend Page
- **Estimativa:** 7h
- **Responsável:** FE1
- **Dependências:** F5-VLT-FE-002

**Arquivos a criar:**
```
/src/frontend/src/pages/valuations/index.tsx        (ValuationsPage)
/src/frontend/src/components/valuation/ValuationStatusBadge.tsx
/src/frontend/src/components/valuation/ValuationVariationIndicator.tsx
```

**Funcionalidades obrigatórias:**
- Tabela com colunas: Data, Tipo de Evento, Valor, Price/Share, Metodologia, Status, Variação %
- `ValuationStatusBadge`: draft=cinza, pending_approval=amarelo, approved=verde, superseded=roxo
- `ValuationVariationIndicator`: ▲ +18,5% em verde / ▼ -5,2% em vermelho
- Filtros: status, event_type, range de datas (via query params na URL)
- Paginação server-side
- Botão de ação contextual por status: [Editar] draft | [Aprovar/Rejeitar] pending_approval
- Mobile responsive (tabela vira cards em telas < 768px)

**Critérios de Aceite:**
- [ ] `npm run build` sem erros TypeScript
- [ ] Filtros atualizam URL params (bookmarkable)
- [ ] Loading skeleton durante fetch
- [ ] Empty state quando não há valuations
- [ ] Confirmação modal antes de rejeitar

---

### F5-VLT-FE-004: Formulário Wizard — Criar/Editar Valuation

- **Status:** [ ] Pendente
- **Tipo:** Frontend Page (Wizard)
- **Estimativa:** 10h ⚠️
- **Responsável:** FE1
- **Dependências:** F5-VLT-FE-002

**Arquivos a criar:**
```
/src/frontend/src/pages/valuations/new.tsx           (ValuationWizardPage)
/src/frontend/src/components/valuation/WizardStep1BasicData.tsx
/src/frontend/src/components/valuation/WizardStep2Methods.tsx
/src/frontend/src/components/valuation/WizardStep3Summary.tsx
/src/frontend/src/components/valuation/WizardStep4Documents.tsx
/src/frontend/src/components/valuation/MethodologyInputs.tsx  (inputs dinâmicos por tipo)
/src/frontend/src/components/valuation/LiveCalculator.tsx     (cálculo em tempo real)
```

**LiveCalculator — cálculo no frontend:**
```typescript
// Espelhar a lógica do backend para feedback imediato
const calculateArrMultiple = (arr: number, multiple: number): number =>
  Math.round(arr * multiple * 100) / 100;

const calculatePricePerShare = (valuationAmount: number, totalShares: number): number =>
  totalShares > 0 ? Math.round((valuationAmount / totalShares) * 10000) / 10000 : 0;
```

**Critérios de Aceite:**
- [ ] 4 steps com progresso visual (step indicator)
- [ ] Validação por step antes de avançar
- [ ] Cálculo em tempo real ao digitar inputs da metodologia
- [ ] `isSelected` exatamente 1 por vez (radio, não checkbox)
- [ ] Step 4 integra o componente `DocumentManager` reutilizável
- [ ] Auto-save em rascunho ao sair do wizard (beforeunload)
- [ ] Redirecionamento para detalhe após submit

---

### F5-FIN-FE-001: Types + Service + Hook — Financeiro

- **Status:** [ ] Pendente
- **Tipo:** Frontend Types + Service + Hook
- **Estimativa:** 5h
- **Responsável:** FE2
- **Dependências:** F5-FIN-BE-003

**Arquivos a criar:**
```
/src/frontend/src/types/financial.ts
/src/frontend/src/services/financialService.ts
/src/frontend/src/hooks/useFinancialPeriod.ts
/src/frontend/src/hooks/useFinancialDashboard.ts
```

**Critérios de Aceite:**
- [ ] Types: `FinancialPeriod`, `FinancialMetric`, `FinancialDashboardData`
- [ ] Enums tipados: `MetricCategory`, `MetricType`, `FinancialPeriodStatus`
- [ ] `useFinancialPeriod(year, month)` carrega o período com métricas
- [ ] `useFinancialDashboard()` carrega dados agregados dos últimos 12 meses
- [ ] Sem `any` no TypeScript

---

### F5-FIN-FE-002: Página de Métricas Financeiras

- **Status:** [ ] Pendente
- **Tipo:** Frontend Page
- **Estimativa:** 9h
- **Responsável:** FE2
- **Dependências:** F5-FIN-FE-001

**Arquivos a criar:**
```
/src/frontend/src/pages/financial/[year]/[month].tsx   (FinancialMetricsPage)
/src/frontend/src/pages/financial/index.tsx             (FinancialPeriodsCalendarPage)
/src/frontend/src/components/financial/MetricInput.tsx  (campo de entrada de métrica)
/src/frontend/src/components/financial/PeriodNavigator.tsx
/src/frontend/src/components/financial/RunwayIndicator.tsx
```

**MetricInput — componente chave:**
```
┌────────────────────────────────────────────────┐
│ MRR           R$ [      120.000,00]   ▲ +12%   │
│                                vs Dez/25: 107k  │
└────────────────────────────────────────────────┘
```

**Critérios de Aceite:**
- [ ] Auto-save com debounce de 1.500ms (não salva a cada tecla)
- [ ] Exibição de variação vs mês anterior em tempo real
- [ ] `RunwayIndicator`: 🟢 > 12 meses | 🟡 6–12 meses | 🔴 < 6 meses
- [ ] Campos calculados (ARR, Runway, Margem EBITDA) em readonly + destaque
- [ ] Botões de ação por status: [Salvar Rascunho] draft | [Submeter] draft | [Aprovar] submitted
- [ ] Período Locked: todos os campos desabilitados com banner informativo

---

### F5-DOC-FE-001: Componente DocumentManager (Reutilizável)

- **Status:** [ ] Pendente
- **Tipo:** Frontend Component
- **Estimativa:** 8h
- **Responsável:** FE2
- **Dependências:** F5-DOC-BE-003

**Arquivos a criar:**
```
/src/frontend/src/components/documents/DocumentManager.tsx    (componente raiz)
/src/frontend/src/components/documents/DocumentUploader.tsx   (drag-and-drop)
/src/frontend/src/components/documents/DocumentCard.tsx       (card de documento)
/src/frontend/src/components/documents/DocumentList.tsx       (lista)
/src/frontend/src/services/documentService.ts
/src/frontend/src/hooks/useDocuments.ts
/src/frontend/src/types/document.ts
```

**Props do DocumentManager:**
```typescript
interface DocumentManagerProps {
  companyId: string;
  referenceType?: 'valuation' | 'financial_period' | 'contract' | 'general';
  referenceId?: string;
  readonly?: boolean;          // Modo somente leitura (ex: em período locked)
  allowedCategories?: DocumentCategory[];
  maxFileSizeMb?: number;      // default: 50
}
```

**Critérios de Aceite:**
- [ ] Reutilizável — funciona em Valuation Wizard, Financial Period e como página standalone
- [ ] Drag-and-drop com feedback visual durante arraste
- [ ] Barra de progresso de upload com cancelamento
- [ ] Ícone por tipo de arquivo (PDF, Excel, Word, imagem, genérico)
- [ ] Preview de imagem em miniatura
- [ ] Confirmação antes de deletar documento
- [ ] Validação de tamanho e tipo antes de iniciar upload

---

## 11. Semana 4: Dashboard + Documentos + Testes

**Meta:** Dashboards executivos + Módulo de Documentos standalone + Cobertura de testes ≥ 85%  
**Prazo:** Semana 4 (40h)  
**Status:** ⚪ Não Iniciado  

---

### F5-DSH-FE-001: Dashboard Executivo — Valuation Overview

- **Status:** [ ] Pendente
- **Tipo:** Frontend Dashboard
- **Estimativa:** 10h
- **Responsável:** FE1
- **Dependências:** F5-VLT-FE-002

**Arquivos a criar:**
```
/src/frontend/src/pages/valuations/dashboard.tsx
/src/frontend/src/components/valuation/ValuationKPICards.tsx
/src/frontend/src/components/charts/ValuationHistoryChart.tsx
/src/frontend/src/components/charts/MethodologyComparisonChart.tsx
/src/frontend/src/components/valuation/ValuationTimeline.tsx
```

**Componentes e dados:**

| Componente | Tipo de Chart | Dados |
|------------|--------------|-------|
| `ValuationHistoryChart` | Line Chart (Recharts) | valuations aprovados × tempo |
| `MethodologyComparisonChart` | Bar Chart (Recharts) | valor calculado por cada metodologia do último valuation |
| `ValuationTimeline` | Timeline custom | eventos com tipo, data e valor |
| `ValuationKPICards` | Cards | valuation atual, variação %, price/share, quantidade |

**Critérios de Aceite:**
- [ ] Recharts ResponsiveContainer para gráficos responsivos
- [ ] Tooltip customizado com formatação monetária BR
- [ ] KPI Cards com indicador de tendência (▲/▼ + cor)
- [ ] Loading skeleton para cada gráfico
- [ ] Filtro de período nos gráficos (6m, 12m, 24m, all)
- [ ] Mobile: stacked layout (cards → gráficos em coluna)
- [ ] Botão "Exportar PDF" (usando `window.print()` com print CSS)

---

### F5-DSH-FE-002: Dashboard Financeiro

- **Status:** [ ] Pendente
- **Tipo:** Frontend Dashboard
- **Estimativa:** 8h
- **Responsável:** FE2
- **Dependências:** F5-FIN-FE-001

**Arquivos a criar:**
```
/src/frontend/src/pages/financial/dashboard.tsx
/src/frontend/src/components/financial/FinancialKPICards.tsx
/src/frontend/src/components/charts/MrrArrChart.tsx
/src/frontend/src/components/charts/BurnRateChart.tsx
/src/frontend/src/components/charts/CustomerGrowthChart.tsx
/src/frontend/src/components/charts/CacLtvChart.tsx
```

**Critérios de Aceite:**
- [ ] `MrrArrChart`: linha dupla MRR (azul) e ARR (verde) últimos 12 meses
- [ ] `BurnRateChart`: barras com burn_rate e linha de threshold seguro
- [ ] `RunwayGauge`: indicador visual de saúde (verde/amarelo/vermelho)
- [ ] `CacLtvChart`: barras agrupadas CAC vs LTV (LTV/CAC ratio calculado)
- [ ] Dados vindos de `useFinancialDashboard` (cache Redis no backend)
- [ ] Tooltip com valores formatados em R$ ou %
- [ ] Filtros de range de datas (período de análise)

---

### F5-DOC-FE-002: Página Repositório de Documentos

- **Status:** [ ] Pendente
- **Tipo:** Frontend Page
- **Estimativa:** 6h
- **Responsável:** FE2
- **Dependências:** F5-DOC-FE-001

**Arquivos a criar:**
```
/src/frontend/src/pages/documents/index.tsx      (DocumentsPage)
/src/frontend/src/pages/documents/[id].tsx       (DocumentDetailPage)
```

**Funcionalidades:**
- Lista paginada de todos os documentos da empresa
- Filtros: categoria, visibilidade, status de verificação, período de upload
- Busca por nome (debounce 500ms)
- Agrupamento visual por categoria (acordeão ou abas)
- Badge de verificação: 🟡 Pendente | ✅ Verificado | ❌ Rejeitado
- Link para contexto de origem (ex: "Vinculado ao Valuation: Series A Jan/26")
- Ação de verificar/rejeitar (apenas admin)
- Download via URL assinada

**Critérios de Aceite:**
- [ ] Busca atualiza URL params (bookmarkable)
- [ ] Carregamento progressivo (infinite scroll ou paginação)
- [ ] Breadcrumb de referência funcional (link para o valuation/contrato relacionado)
- [ ] Empty state por categoria

---

### F5-TST-BE-001: Testes Unitários — ValuationCalculationEngine

- **Status:** [ ] Pendente
- **Tipo:** Tests — Backend Unit
- **Estimativa:** 8h
- **Responsável:** BE1
- **Dependências:** F5-ENG-BE-001

**Arquivo a criar:**
```
/src/backend/PartnershipManager.Tests/Services/ValuationCalculationEngineTests.cs
```

**Casos de teste obrigatórios:**

| Teste | Input | Expected Output |
|-------|-------|----------------|
| ARR Multiple básico | arr=1.200.000, múltiplo=8 | 9.600.000,00 |
| ARR Multiple com casas decimais | arr=1.233.456,78, múltiplo=7,5 | 9.250.925,85 |
| DCF 3 anos | cashFlows=[150k,280k,450k], rate=0,25 | ~610.000,00 (verificar) |
| Comparable médio simples | [10M, 12M, 11M] pesos iguais | 11.000.000,00 |
| PricePerShare | amount=12M, shares=2.857.143 | 4,2000 |
| PricePerShare zero shares | shares=0 | 0 (sem divisão por zero) |
| Variação positiva | anterior=10M, atual=12M | +20,00% |
| Variação negativa | anterior=12M, atual=10M | -16,67% |
| Arredondamento | resultado=9,50000001 | 9,50 |
| Não usar double | resultado deve ser exato em decimal | sem floating point error |

**Critérios de Aceite:**
- [ ] Cobertura ≥ 90% para `ValuationCalculationEngine`
- [ ] Todos os testes passam com `dotnet test`
- [ ] Nenhum teste com `Assert.IsTrue(Math.Abs(a-b) < 0.01)` — usar `Assert.AreEqual` com decimal exato

---

### F5-TST-BE-002: Testes Unitários — ValuationService e FinancialService

- **Status:** [ ] Pendente
- **Tipo:** Tests — Backend Unit
- **Estimativa:** 8h
- **Responsável:** BE2
- **Dependências:** F5-VLT-BE-004, F5-FIN-BE-003

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Tests/Services/ValuationServiceTests.cs
/src/backend/PartnershipManager.Tests/Services/FinancialServiceTests.cs
```

**Casos ValuationService:**
- Criar valuation com `PricePerShare` calculado corretamente
- Aprovar: rejeitar se criador = aprovador (segregação de funções)
- Aprovar: rejeitar se nenhuma metodologia selecionada
- Aprovar: rejeitar se mais de 1 metodologia `IsSelected=true`
- Aprovar: verificar que valuation anterior muda para `Superseded`
- Rejeitar: status volta para `Draft`, aprovador não registrado

**Casos FinancialService:**
- Criar período: segundo período duplicado retorna conflito
- ARR calculado = MRR × 12
- Runway = CashBalance / BurnRate (arredondado para baixo)
- Submeter: valida período anterior aprovado (regra FI-02)
- Aprovar período locked: tenta editar retorna 409
- Variação MoM: (120k - 107k) / 107k × 100 = 12,15%

**Critérios de Aceite:**
- [ ] Mocks via Moq para repositories
- [ ] Cobertura ≥ 85% para services
- [ ] Nomes de teste em português: `DeveCalcularPricePerShareCorretamente()`

---

### F5-TST-BE-003: Testes de Integração — APIs

- **Status:** [ ] Pendente
- **Tipo:** Tests — Backend Integration
- **Estimativa:** 7h
- **Responsável:** BE1
- **Dependências:** F5-VLT-BE-005, F5-FIN-BE-003

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Tests.Integration/Controllers/ValuationsControllerTests.cs
/src/backend/PartnershipManager.Tests.Integration/Controllers/FinancialPeriodsControllerTests.cs
```

**Fluxo de teste — Valuation (E2E):**
```
1. POST /valuations → criar rascunho (201)
2. POST /valuations/{id}/methods → adicionar ARR Multiple
3. POST /valuations/{id}/methods → adicionar DCF
4. PUT  /valuations/{id}/methods/{methodId}/select → selecionar ARR Multiple
5. PUT  /valuations/{id}/submit → enviar para aprovação (200)
6. PUT  /valuations/{id}/approve → aprovar como admin (200)
7. GET  /share-classes → verificar que price_per_share foi atualizado
8. GET  /valuations/latest → verificar que retorna o valuation aprovado
```

**Critérios de Aceite:**
- [ ] Banco de dados de teste isolado (usar In-Memory ou TestContainers)
- [ ] Teardown limpa dados após cada teste
- [ ] Testar também cenários de erro: 400, 403, 404, 409
- [ ] Testar autorização: viewer não pode criar/aprovar

---

### F5-TST-FE-001: Testes de Componentes Frontend

- **Status:** [ ] Pendente
- **Tipo:** Tests — Frontend
- **Estimativa:** 5h
- **Responsável:** FE1
- **Dependências:** F5-VLT-FE-004, F5-DOC-FE-001

**Arquivos a criar:**
```
/src/frontend/src/__tests__/valuation/LiveCalculator.test.tsx
/src/frontend/src/__tests__/valuation/ValuationStatusBadge.test.tsx
/src/frontend/src/__tests__/documents/DocumentManager.test.tsx
/src/frontend/src/__tests__/financial/MetricInput.test.tsx
```

**Casos de teste:**
- `LiveCalculator`: ARR=1.200.000 + múltiplo=8 → exibe "R$ 9.600.000,00"
- `LiveCalculator`: price/share = 9.600.000 / 2.857.143 → exibe "R$ 3,3600"
- `ValuationStatusBadge`: status='approved' → classe CSS verde + texto "Aprovado"
- `DocumentManager`: arquivo >50MB → exibe mensagem de erro antes do upload
- `DocumentManager`: tipo inválido → rejeita com mensagem clara
- `MetricInput`: variação vs anterior exibida corretamente (+12% em verde)

**Critérios de Aceite:**
- [ ] Vitest + React Testing Library
- [ ] Cobertura mínima 80% nos componentes testados
- [ ] Todos os testes passam com `npm test`
- [ ] Sem warnings de `act()`

---

### F5-CFG-001: Registro de DI, Rotas e Verificação Final

- **Status:** [ ] Pendente
- **Tipo:** Configuration
- **Estimativa:** 4h
- **Responsável:** BE1 + FE1
- **Dependências:** Todas as tarefas anteriores concluídas

**Arquivos a modificar:**
```
/src/backend/Extensions/ServiceCollectionExtensions.cs
  → Registrar: ValuationCalculationEngine, ValuationService, FinancialService, DocumentService
  → Registrar: ValuationRepository, FinancialPeriodRepository, DocumentRepository
  → Registrar: todos os Validators (IValidator<>)

/src/frontend/src/router/routes.tsx (ou equivalente)
  → /valuations
  → /valuations/new
  → /valuations/:id
  → /valuations/dashboard
  → /financial
  → /financial/dashboard
  → /financial/:year/:month
  → /documents
  → /documents/:id

/src/frontend/src/components/layout/Sidebar.tsx
  → Adicionar seção "Valuation" com sublinks
  → Adicionar seção "Financeiro" com sublinks
  → Adicionar link "Documentos"
```

**Comandos de verificação final:**
```bash
# Backend — zero erros, zero warnings
cd src/backend
dotnet build --no-incremental 2>&1
dotnet test --no-build

# Frontend — zero erros TypeScript e lint
cd src/frontend
npm run lint
npm run build

# Cobertura de testes
cd src/backend
dotnet test --collect:"XPlat Code Coverage"
# Verificar que coverage ≥ 85% nos módulos da Fase 5
```

**Critérios de Aceite:**
- [ ] `dotnet build` — sem erros e sem warnings
- [ ] `dotnet test` — todos os testes passando
- [ ] `npm run lint` — sem warnings
- [ ] `npm run build` — sem erros TypeScript
- [ ] Cobertura ≥ 85%
- [ ] Sidebar atualizada com novos menus
- [ ] Todas as rotas funcionando no frontend
- [ ] Swagger documentado para todos os novos endpoints

---

## 12. Resumo de Todas as Tarefas

### 12.1 Tabela Resumo

| ID | Título | Semana | Estimativa | Responsável | Dependências |
|----|--------|--------|-----------|-------------|--------------|
| F5-DB-001 | Migrations Valuation | S1 | 6h | BE1 | Fase 2 |
| F5-DB-002 | Migrations Financial | S1 | 5h | BE1 | F5-DB-001 |
| F5-DB-003 | Migration Documents | S1 | 4h | BE1 | F5-DB-001 |
| F5-VLT-BE-001 | Entity Valuation + Enums | S1 | 4h | BE1 | F5-DB-001 |
| F5-VLT-BE-002 | Entity ValuationMethod + ValuationDocument | S1 | 4h | BE1 | F5-VLT-BE-001 |
| F5-FIN-BE-001 | Entities Financial + Enums | S1 | 5h | BE2 | F5-DB-002 |
| F5-DOC-BE-001 | Entity Document + Enums | S1 | 4h | BE2 | F5-DB-003 |
| F5-VLT-BE-003 | Repository Valuation | S1 | 8h | BE1 | F5-VLT-BE-002 |
| F5-FIN-BE-002 | Repository Financial | S1 | 7h | BE2 | F5-FIN-BE-001 |
| F5-DOC-BE-002 | Repository Document | S1 | 6h | BE2 | F5-DOC-BE-001 |
| **Semana 1 Total** | | | **53h** | | |
| F5-ENG-BE-001 | ValuationCalculationEngine ⚠️ | S2 | 14h | BE1 | F5-VLT-BE-002 |
| F5-VLT-BE-004 | ValuationService + DTOs + Validators | S2 | 10h | BE1 | F5-ENG-BE-001 |
| F5-INT-BE-001 | Integração Cap Table | S2 | 5h | BE1 | F5-VLT-BE-004 |
| F5-VLT-BE-005 | ValuationsController | S2 | 5h | BE1 | F5-VLT-BE-004 |
| F5-FIN-BE-003 | FinancialService + Controller | S2 | 12h | BE2 | F5-FIN-BE-002 |
| F5-DOC-BE-003 | DocumentService + Controller | S2 | 8h | BE2 | F5-DOC-BE-002 |
| **Semana 2 Total** | | | **54h** | | |
| F5-VLT-FE-001 | Types TypeScript Valuation | S3 | 3h | FE1 | F5-VLT-BE-005 |
| F5-VLT-FE-002 | Service + Hook Valuation | S3 | 5h | FE1 | F5-VLT-FE-001 |
| F5-VLT-FE-003 | Página Lista Valuations | S3 | 7h | FE1 | F5-VLT-FE-002 |
| F5-VLT-FE-004 | Formulário Wizard Valuation | S3 | 10h | FE1 | F5-VLT-FE-002 |
| F5-FIN-FE-001 | Types + Service + Hook Financial | S3 | 5h | FE2 | F5-FIN-BE-003 |
| F5-FIN-FE-002 | Página Métricas Financeiras | S3 | 9h | FE2 | F5-FIN-FE-001 |
| F5-DOC-FE-001 | Componente DocumentManager | S3 | 8h | FE2 | F5-DOC-BE-003 |
| **Semana 3 Total** | | | **47h** | | |
| F5-DSH-FE-001 | Dashboard Valuation | S4 | 10h | FE1 | F5-VLT-FE-002 |
| F5-DSH-FE-002 | Dashboard Financeiro | S4 | 8h | FE2 | F5-FIN-FE-001 |
| F5-DOC-FE-002 | Página Repositório Documentos | S4 | 6h | FE2 | F5-DOC-FE-001 |
| F5-TST-BE-001 | Testes Unitários — Engine | S4 | 8h | BE1 | F5-ENG-BE-001 |
| F5-TST-BE-002 | Testes Unitários — Services | S4 | 8h | BE2 | F5-VLT-BE-004 |
| F5-TST-BE-003 | Testes de Integração — APIs | S4 | 7h | BE1 | F5-VLT-BE-005 |
| F5-TST-FE-001 | Testes de Componentes Frontend | S4 | 5h | FE1 | F5-VLT-FE-004 |
| F5-CFG-001 | DI + Rotas + Verificação Final | S4 | 4h | BE1+FE1 | Todas |
| **Semana 4 Total** | | | **56h** | | |
| **TOTAL FASE 5** | **30 tarefas** | | **~210h** | | |

> **Nota:** Total estimado inclui buffers de integração. A execução em paralelo (BE1+BE2+FE1+FE2) encaixa nas 4 semanas de 40h.

### 12.2 Grafo de Dependências

```
F5-DB-001 ──────┬──→ F5-VLT-BE-001 ──→ F5-VLT-BE-002 ──→ F5-VLT-BE-003 ──→ F5-ENG-BE-001
                │                                                              │
                └──→ F5-DB-003 ──→ F5-DOC-BE-001 ──→ F5-DOC-BE-002            │
                                                                               ▼
F5-DB-002 ──────────→ F5-FIN-BE-001 ──→ F5-FIN-BE-002                   F5-VLT-BE-004
                                                │                              │
                                                ▼                              ├──→ F5-INT-BE-001
                                         F5-FIN-BE-003                        │
                                              │                               └──→ F5-VLT-BE-005
                                              ▼                                          │
                                        F5-FIN-FE-001                                    ▼
                                              │                              F5-VLT-FE-001
                                              ├──→ F5-FIN-FE-002                   │
                                              └──→ F5-DSH-FE-002            F5-VLT-FE-002
                                                                                   │
                                                                        ┌──────────┤
                                                                        ▼          ▼
                                                               F5-VLT-FE-003  F5-VLT-FE-004
                                                                                   │
                                                                               F5-DSH-FE-001
```

---

## 13. Critérios de Aceite da Fase

### 13.1 Funcionais

- [ ] **Valuation completo:** criar rascunho → adicionar metodologias → selecionar principal → enviar → aprovar → cap table atualizado
- [ ] **Motor de cálculo:** 7 metodologias calculando corretamente com precisão decimal
- [ ] **Histórico:** gráfico de evolução de valuations mostrando últimos 24 meses
- [ ] **Financeiro:** inserir métricas do mês → calcular ARR/Runway automaticamente → submeter → aprovar
- [ ] **Documentos:** fazer upload → vincular ao valuation → download com URL assinada
- [ ] **Integração:** aprovar valuation → verificar que `price_per_share` nas share_classes foi atualizado
- [ ] **Permissões:** viewer não consegue criar; criador não consegue auto-aprovar

### 13.2 Técnicos

- [ ] `dotnet build --no-incremental` — **zero erros, zero warnings**
- [ ] `dotnet test` — **todos passando**
- [ ] `npm run build` — **zero erros TypeScript**
- [ ] `npm run lint` — **zero warnings ESLint**
- [ ] Cobertura de testes **≥ 85%** (mensurado com `dotnet-coverage`)
- [ ] Tempo de resposta das APIs: **< 200ms p95** (sem cache) e **< 50ms p95** (com cache)
- [ ] Migrations com scripts DOWN (rollback) **funcionais**
- [ ] **Nenhum** `any` no TypeScript
- [ ] **Nenhuma** string hardcoded no backend — usar `ErrorMessages` / `SuccessMessages`

### 13.3 UX

- [ ] Loading states em todos os fetches (skeleton ou spinner)
- [ ] Empty states descritivos em todas as listas
- [ ] Feedback de erro amigável (toast) em caso de falha de API
- [ ] Formulários com validação inline (não só no submit)
- [ ] Responsividade: funciona corretamente em 375px, 768px e 1440px

---

## 14. Comandos de Verificação

### 14.1 Antes de Iniciar Cada Tarefa

```bash
# 1. Verificar que o projeto ainda compila (nunca começar com build quebrado)
cd src/backend && dotnet build --no-incremental
cd src/frontend && npm run build

# 2. Verificar migration mais recente (para definir próximo número)
ls -la database/migrations/ | sort | tail -10

# 3. Verificar entidades existentes (evitar duplicatas)
ls src/backend/PartnershipManager.Domain/Entities/
ls src/backend/PartnershipManager.Domain/Enums/

# 4. Verificar se services/repos a modificar existem
grep -r "IShareClassRepository" src/backend/ --include="*.cs" -l
```

### 14.2 Após Cada Tarefa de Backend

```bash
# Build completo
cd src/backend
dotnet build --no-incremental 2>&1

# Testes
dotnet test --no-build 2>&1

# Verificar DI (se adicionou registro)
grep -n "AddScoped\|AddTransient\|AddSingleton" src/backend/Extensions/ServiceCollectionExtensions.cs
```

### 14.3 Após Cada Tarefa de Frontend

```bash
cd src/frontend

# TypeScript check
npx tsc --noEmit 2>&1

# Lint
npm run lint 2>&1

# Build
npm run build 2>&1
```

### 14.4 Testes das APIs via Swagger

Sequência manual para validar o fluxo completo de Valuation:
```
1. Autenticar como admin
2. POST /api/v1/companies/{id}/valuations → criar rascunho → guardar {valuationId}
3. POST /api/v1/companies/{id}/valuations/{valuationId}/methods → ARR Multiple
4. POST /api/v1/companies/{id}/valuations/{valuationId}/methods → DCF
5. PUT  /api/v1/companies/{id}/valuations/{valuationId}/methods/{methodId}/select
6. PUT  /api/v1/companies/{id}/valuations/{valuationId}/submit
7. PUT  /api/v1/companies/{id}/valuations/{valuationId}/approve (como outro usuário admin)
8. GET  /api/v1/companies/{id}/share-classes → verificar price_per_share atualizado
9. GET  /api/v1/companies/{id}/valuations/latest → verificar retorna o aprovado
10. GET /api/v1/companies/{id}/valuations/{valuationId} → verificar status=approved
```

---

## 15. Checklist de Entrega Final

```
┌─────────────────────────────────────────────────────────────────┐
│                    CHECKLIST FINAL — FASE 5                      │
├─────────────────────────────────────────────────────────────────┤
│ DATABASE                                                         │
│  ☐ 6 migrations criadas (037–042) com scripts UP e DOWN         │
│  ☐ Todas as FKs e índices verificados                            │
│  ☐ UNIQUE INDEX financial_periods (company_id, year, month)      │
│  ☐ CHECK constraints em colunas numéricas críticas              │
├─────────────────────────────────────────────────────────────────┤
│ BACKEND                                                          │
│  ☐ 7 entities criadas (herdando BaseEntity)                     │
│  ☐ Todos os Enums definidos (conforme MER)                      │
│  ☐ 3 repositories com CRUD + queries especializadas             │
│  ☐ ValuationCalculationEngine — 7 metodologias                  │
│  ☐ 3 services com regras de negócio                             │
│  ☐ 3 controllers com endpoints documentados no Swagger           │
│  ☐ Integração Cap Table funcionando (price_per_share atualizado) │
│  ☐ Cache Redis em financial dashboard                           │
│  ☐ DI registrada para todos os serviços e repositórios          │
│  ☐ Nenhuma string hardcoded (usar Messages.cs)                  │
│  ☐ dotnet build — ZERO erros                                    │
│  ☐ dotnet test — TODOS passando                                 │
├─────────────────────────────────────────────────────────────────┤
│ FRONTEND                                                         │
│  ☐ Types TypeScript para Valuation, Financial, Document         │
│  ☐ Services (API clients) para os 3 módulos                     │
│  ☐ Hooks (React Query) para todos os módulos                    │
│  ☐ Wizard de Valuation — 4 steps funcionando                    │
│  ☐ Página de Métricas Financeiras com auto-save                 │
│  ☐ Componente DocumentManager reutilizável                      │
│  ☐ Dashboard Valuation com 4 gráficos                           │
│  ☐ Dashboard Financeiro com 5 gráficos/indicadores              │
│  ☐ Sidebar atualizada com novos menus                           │
│  ☐ Todas as rotas configuradas                                  │
│  ☐ Sem `any` no TypeScript                                      │
│  ☐ npm run lint — ZERO warnings                                 │
│  ☐ npm run build — ZERO erros                                   │
├─────────────────────────────────────────────────────────────────┤
│ TESTES                                                           │
│  ☐ Testes unitários do Engine (cobertura ≥ 90%)                 │
│  ☐ Testes unitários dos Services (cobertura ≥ 85%)             │
│  ☐ Testes de integração dos Controllers (fluxo E2E)             │
│  ☐ Testes de componentes React (LiveCalculator, DocumentManager) │
│  ☐ Cobertura global ≥ 85% na Fase 5                            │
├─────────────────────────────────────────────────────────────────┤
│ UX & QUALIDADE                                                   │
│  ☐ Loading states em todos os fetches                           │
│  ☐ Empty states descritivos                                     │
│  ☐ Toasts de sucesso/erro                                       │
│  ☐ Responsividade testada (375px, 768px, 1440px)                │
│  ☐ Validação de formulários inline                              │
├─────────────────────────────────────────────────────────────────┤
│ DOCUMENTAÇÃO & TRACKING                                          │
│  ☐ FASE5_TRACKING_PROGRESSO.md criado e atualizado             │
│  ☐ Swagger/OpenAPI atualizado para novos endpoints              │
│  ☐ README do projeto atualizado com Fase 5                      │
└─────────────────────────────────────────────────────────────────┘
```

---

## Apêndice — Referências

| Documento | Localização |
|-----------|-------------|
| MER v2.0 | `Partnership_Manager_MER_Entidades.md` |
| Premissas de Desenvolvimento | `PREMISSAS_DESENVOLVIMENTO.md` |
| Plano Fase 4 (referência de padrão) | `FASE4_VESTING_PLANO_EXECUCAO.md` |
| Especificação Técnica | `Partnership_Manager_Especificacao_Tecnica.md` |
| Protótipo UI | https://claude.ai/public/artifacts/30eb2e6d-a59a-4d7b-bfcd-20d563e3682a |

---

*Partnership Manager — Fase 5 | Gerado em 26/02/2026*  
*Versão: 1.0 | Status: Pronto para Execução*
