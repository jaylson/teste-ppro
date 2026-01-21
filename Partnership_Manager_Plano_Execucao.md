# Partnership Manager
## Plano de Execução de Desenvolvimento

**Versão:** 1.0  
**Data:** 19 de Janeiro de 2025  
**Duração Estimada:** 32 semanas (8 meses)  
**Regime:** 40 horas/semana (8h/dia × 5 dias)

---

## Sumário

1. [Visão Geral do Projeto](#1-visão-geral-do-projeto)
2. [Premissas e Convenções](#2-premissas-e-convenções)
3. [Equipe e Papéis](#3-equipe-e-papéis)
4. [Stack Tecnológico](#4-stack-tecnológico)
5. [Fases de Desenvolvimento](#5-fases-de-desenvolvimento)
6. [Cronograma Detalhado](#6-cronograma-detalhado)
7. [Dependências entre Tarefas](#7-dependências-entre-tarefas)
8. [Critérios de Aceite](#8-critérios-de-aceite)
9. [Riscos e Mitigações](#9-riscos-e-mitigações)

---

## 1. Visão Geral do Projeto

### 1.1 Objetivo
Desenvolver o **Partnership Manager**, uma plataforma SaaS completa para gestão societária, incluindo cap table, vesting, contratos, valuation e portal do investidor.

### 1.2 Escopo de Entrega

| Fase | Módulos | Semanas |
|------|---------|---------|
| 0 | Setup + Billing | 4 |
| 1 | Core (Auth, Company, Users) | 3 |
| 2 | Societário (Cap Table, Shareholders) | 5 |
| 3 | Contratos | 5 |
| 4 | Vesting & Metas | 4 |
| 5 | Valuation + Financeiro | 4 |
| 6 | Portal Investidor + Comunicações | 4 |
| 7 | Integrações + Polish | 3 |
| **Total** | | **32 semanas** |

### 1.3 Entregáveis por Fase

```
Fase 0: Setup + Billing
├── Infraestrutura (CI/CD, ambientes)
├── Design System implementado
├── Módulo de Billing completo
└── Onboarding de clientes

Fase 1: Core
├── Autenticação (login, 2FA, recuperação)
├── Gestão de Clients e Companies
├── Gestão de Users e permissões
└── Audit Log

Fase 2: Societário
├── Cap Table (visualização + edição)
├── Shareholders (CRUD completo)
├── Share Classes
├── Share Transactions (Ledger)
└── Simulador de Rodadas

Fase 3: Contratos
├── Templates de Contratos
├── Biblioteca de Cláusulas
├── Contract Builder
├── Gestão de Contratos
└── Assinatura Digital (integração)

Fase 4: Vesting
├── Vesting Plans
├── Grants
├── Schedules (tempo)
├── Milestones (metas)
└── Dashboard do beneficiário

Fase 5: Valuation + Financeiro
├── Valuations (CRUD + histórico)
├── Metodologias de cálculo
├── Períodos Financeiros
├── Métricas
└── Gestão de Documentos

Fase 6: Portal + Comunicações
├── Portal do Investidor
├── Comunicações/Updates
├── Data Room
└── Notificações (email + in-app)

Fase 7: Integrações + Polish
├── API Pública
├── Webhooks
├── Relatórios/Exports
├── Performance + Segurança
└── Documentação
```

---

## 2. Premissas e Convenções

### 2.1 Unidades de Tempo

| Unidade | Horas | Descrição |
|---------|-------|-----------|
| **P** (Pequena) | 8h | 1 dia de trabalho |
| **M** (Média) | 16h | 2 dias de trabalho |
| **G** (Grande) | 24h | 3 dias de trabalho |
| **XG** (Extra Grande) | 40h | 1 semana de trabalho |

### 2.2 Convenções de Nomenclatura

```
[FASE]-[MÓDULO]-[TIPO]-[NÚMERO]: Descrição

Tipos:
- BE: Backend
- FE: Frontend
- DB: Database
- INT: Integração
- TST: Teste
- DOC: Documentação
- CFG: Configuração
```

**Exemplo:** `F2-CAP-BE-001: Criar API de Cap Table`

### 2.3 Definition of Done (DoD)

Uma tarefa só é considerada "Done" quando:
- [ ] Código implementado e revisado (PR aprovado)
- [ ] Testes unitários escritos (cobertura mínima 80%)
- [ ] Testes de integração passando
- [ ] Documentação da API atualizada (se aplicável)
- [ ] Code review aprovado por pelo menos 1 desenvolvedor
- [ ] Deploy em ambiente de staging sem erros
- [ ] Critérios de aceite validados

---

## 3. Equipe e Papéis

### 3.1 Composição Mínima Recomendada

| Papel | Quantidade | Responsabilidades |
|-------|------------|-------------------|
| Tech Lead | 1 | Arquitetura, code review, decisões técnicas |
| Backend Developer | 2 | APIs, regras de negócio, integrações |
| Frontend Developer | 2 | UI/UX, componentes, páginas |
| QA Engineer | 1 | Testes, qualidade, automação |
| DevOps | 0.5 | Infraestrutura, CI/CD, monitoramento |
| Product Owner | 0.5 | Priorização, validação, stakeholders |

### 3.2 Alocação por Fase

```
Fase 0 (Setup):      TL(100%) + BE(50%) + FE(50%) + DevOps(100%)
Fase 1-6 (Core):     TL(50%) + BE(100%) + FE(100%) + QA(100%)
Fase 7 (Polish):     TL(100%) + BE(50%) + FE(50%) + QA(100%)
```

---

## 4. Stack Tecnológico

### 4.1 Backend (.NET 8)
```yaml
Runtime: .NET 8 LTS
Framework: ASP.NET Core 8 (Minimal APIs + Controllers)
ORM: Entity Framework Core 8
Database: MySQL 8.0
Cache: Redis 7 (StackExchange.Redis)
Queue: Hangfire
Auth: Firebase Authentication + JWT
Docs: Swagger/Swashbuckle (nativo)
Validação: FluentValidation
Mapping: Mapster
Arquitetura: Clean Architecture / Vertical Slices
```

### 4.2 Frontend (Vue 3 + Nuxt 3)
```yaml
Framework: Nuxt 3.9+
Language: TypeScript 5
UI Components: PrimeVue 4
Styling: Tailwind CSS 3
State: Pinia
Forms: VeeValidate + Zod
Charts: Chart.js / Apache ECharts
HTTP: Nuxt useFetch
Composables: VueUse
```

### 4.3 Serviços Externos
```yaml
Auth: Firebase Authentication
Assinatura Digital: Clicksign
Email: Resend (ou Firebase Extensions)
Storage: Firebase Storage / AWS S3
```

### 4.4 Infraestrutura
```yaml
Cloud: AWS / Azure / GCP
Container: Docker + Docker Compose
CI/CD: GitHub Actions
Monitoring: Application Insights / Serilog + Seq
Logs: Serilog → MySQL / Seq
Storage: Firebase Storage / S3
```

### 4.5 Observações sobre Billing
```yaml
# Nesta versão, o módulo de Billing será apenas cadastral:
# - Cadastro de Clients, Plans, Subscriptions (sem integração com gateway)
# - Geração manual de faturas
# - Registro manual de pagamentos
# - Preparado para futura integração com gateway de pagamento
```

---

## 5. Fases de Desenvolvimento

---

## FASE 0: SETUP + BILLING
**Duração:** 4 semanas (160 horas)  
**Objetivo:** Preparar infraestrutura e implementar módulo de cobrança

### Semana 1: Setup Inicial (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F0-CFG-001 | Criar repositórios (mono-repo ou multi-repo) | CFG | 4h | DevOps | - |
| F0-CFG-002 | Configurar ambiente de desenvolvimento (Docker Compose) | CFG | 8h | DevOps | F0-CFG-001 |
| F0-CFG-003 | Setup do projeto backend (.NET 8 + Clean Architecture) | BE | 8h | BE1 | F0-CFG-001 |
| F0-CFG-004 | Setup do projeto frontend (Nuxt 3 + PrimeVue + Tailwind) | FE | 8h | FE1 | F0-CFG-001 |
| F0-CFG-005 | Configurar CI/CD básico (lint, build, test) | CFG | 8h | DevOps | F0-CFG-003, F0-CFG-004 |
| F0-CFG-006 | Criar ambientes (dev, staging, prod) | CFG | 4h | DevOps | F0-CFG-005 |

**Entregável:** Ambiente de desenvolvimento funcional com CI/CD

### Semana 2: Database + Design System (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F0-DB-001 | Criar entidades EF Core - Billing (Client, Plan, Subscription) | DB | 8h | BE1 | F0-CFG-003 |
| F0-DB-002 | Criar entidades EF Core - Billing (Invoice, Payment) | DB | 8h | BE1 | F0-DB-001 |
| F0-DB-003 | Configurar migrations e seeds | DB | 4h | BE1 | F0-DB-002 |
| F0-FE-001 | Configurar PrimeVue + Tailwind + Design Tokens | FE | 8h | FE1 | F0-CFG-004 |
| F0-FE-002 | Implementar Layout principal (AppSidebar, AppHeader) | FE | 8h | FE1 | F0-FE-001 |
| F0-FE-003 | Criar componentes base (Cards, Stats, Badges) | FE | 4h | FE2 | F0-FE-002 |

**Entregável:** Schema de billing criado + Design System base

### Semana 3: APIs de Billing - Cadastral (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F0-BE-001 | API Client (CRUD) | BE | 8h | BE1 | F0-DB-003 |
| F0-BE-002 | API Plan (CRUD + listagem) | BE | 8h | BE1 | F0-DB-003 |
| F0-BE-003 | API Subscription (criar, suspender, cancelar) - manual | BE | 8h | BE2 | F0-BE-001, F0-BE-002 |
| F0-BE-004 | API Invoice (gerar, listar, detalhar) - manual | BE | 8h | BE2 | F0-BE-003 |
| F0-BE-005 | API Payment (registrar pagamento manual) | BE | 8h | BE2 | F0-BE-004 |

**Nota:** Nesta fase, o billing é apenas cadastral. Não há integração com gateway de pagamento.

**Entregável:** APIs de billing funcionais (modo cadastral)

### Semana 4: Frontend Billing + Testes (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F0-FE-004 | Página de Planos (listagem para admin) | FE | 8h | FE1 | F0-BE-002 |
| F0-FE-005 | Página de Clientes e Assinaturas (gestão) | FE | 8h | FE1 | F0-BE-003 |
| F0-FE-006 | Dashboard de Billing (faturas, pagamentos manuais) | FE | 8h | FE2 | F0-BE-004 |
| F0-TST-001 | Testes unitários - Billing Backend | TST | 8h | QA | F0-BE-005 |
| F0-TST-002 | Testes E2E - Fluxo de gestão de assinaturas | TST | 8h | QA | F0-FE-006 |

**Entregável:** Módulo de Billing completo (modo cadastral/manual)

---

## FASE 1: CORE (AUTH + COMPANY + USERS)
**Duração:** 3 semanas (120 horas)  
**Objetivo:** Implementar autenticação e gestão de empresas/usuários

### Semana 5: Autenticação (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F1-DB-001 | Entidades EF Core - User, UserRole, UserCompanyAccess | DB | 8h | BE1 | F0-DB-003 |
| F1-BE-001 | Integração Firebase Auth - Middleware de validação JWT | BE | 8h | BE1 | F1-DB-001 |
| F1-BE-002 | API Auth - Sincronização de usuários Firebase → DB | BE | 8h | BE1 | F1-BE-001 |
| F1-BE-003 | API Auth - Endpoint de registro (cria no Firebase + DB) | BE | 4h | BE1 | F1-BE-001 |
| F1-BE-004 | API Auth - Claims customizadas e contexto do usuário | BE | 8h | BE2 | F1-BE-001 |
| F1-FE-001 | Integração Firebase Auth no Nuxt (plugin + composable) | FE | 4h | FE1 | F1-BE-003 |

**Entregável:** Sistema de autenticação completo com Firebase

### Semana 6: Company + Users (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F1-DB-002 | Entidades EF Core - Company + Configurations | DB | 4h | BE1 | F1-DB-001 |
| F1-BE-005 | API Company (CRUD com FluentValidation) | BE | 8h | BE1 | F1-DB-002 |
| F1-BE-006 | API User (CRUD + sincronização Firebase) | BE | 8h | BE2 | F1-DB-001 |
| F1-BE-007 | API UserCompanyAccess (gerenciar acessos) | BE | 8h | BE2 | F1-BE-006 |
| F1-FE-002 | Página de Configurações da Empresa (Nuxt + PrimeVue) | FE | 8h | FE1 | F1-BE-005 |
| F1-FE-003 | Página de Gestão de Usuários (DataTable + Dialog) | FE | 4h | FE2 | F1-BE-006 |

**Entregável:** Gestão de empresas e usuários

### Semana 7: Permissões + Audit (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F1-BE-008 | Sistema de permissões (RBAC) | BE | 16h | BE1 | F1-BE-007 |
| F1-BE-009 | Audit Log (interceptor global) | BE | 8h | BE2 | F1-BE-008 |
| F1-FE-004 | Componente de seleção de empresa (switcher) | FE | 4h | FE1 | F1-BE-007 |
| F1-FE-005 | Layout principal (sidebar, header, navegação) | FE | 8h | FE1 | F1-FE-004 |
| F1-TST-003 | Testes unitários - Auth + Permissões | TST | 4h | QA | F1-BE-008 |

**Entregável:** Sistema de permissões + Layout principal

---

## FASE 2: SOCIETÁRIO (CAP TABLE)
**Duração:** 5 semanas (200 horas)  
**Objetivo:** Implementar gestão completa de cap table

### Semana 8: Schema + Shareholders (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F2-DB-001 | Entidades EF Core - Shareholder, ShareClass | DB | 8h | BE1 | F1-DB-002 |
| F2-DB-002 | Entidades EF Core - Share, ShareTransaction | DB | 8h | BE1 | F2-DB-001 |
| F2-BE-001 | API ShareClass (CRUD com FluentValidation) | BE | 8h | BE1 | F2-DB-001 |
| F2-BE-002 | API Shareholder (CRUD + busca + paginação) | BE | 8h | BE2 | F2-DB-001 |
| F2-BE-003 | API Shareholder - Vincular a User | BE | 4h | BE2 | F2-BE-002 |
| F2-BE-004 | Validações de negócio - Shareholder (Domain) | BE | 4h | BE2 | F2-BE-002 |

**Entregável:** APIs de Shareholder e ShareClass

### Semana 9: Shares + Ledger (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F2-BE-005 | API Share (CRUD + validações) | BE | 8h | BE1 | F2-DB-002 |
| F2-BE-006 | API ShareTransaction - Emissão | BE | 8h | BE1 | F2-BE-005 |
| F2-BE-007 | API ShareTransaction - Transferência | BE | 8h | BE2 | F2-BE-006 |
| F2-BE-008 | API ShareTransaction - Cancelamento | BE | 4h | BE2 | F2-BE-006 |
| F2-BE-009 | View materializada - Cap Table atual | BE | 8h | BE1 | F2-BE-007 |
| F2-BE-010 | Regras de imutabilidade do Ledger | BE | 4h | BE1 | F2-BE-008 |

**Entregável:** Ledger de ações imutável

### Semana 10: Frontend Cap Table (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F2-FE-001 | Página Cap Table - Header + Stats | FE | 8h | FE1 | F2-BE-009 |
| F2-FE-002 | Página Cap Table - Distribuição visual | FE | 8h | FE1 | F2-FE-001 |
| F2-FE-003 | Página Cap Table - Tabela de sócios | FE | 8h | FE2 | F2-FE-001 |
| F2-FE-004 | Página Cap Table - Filtros e busca | FE | 4h | FE2 | F2-FE-003 |
| F2-FE-005 | Modal - Novo Sócio | FE | 8h | FE1 | F2-BE-002 |
| F2-FE-006 | Modal - Editar Sócio | FE | 4h | FE1 | F2-FE-005 |

**Entregável:** Página de Cap Table funcional

### Semana 11: Frontend Sócios + Detalhes (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F2-FE-007 | Página Sócios - Grid de cards | FE | 8h | FE1 | F2-BE-002 |
| F2-FE-008 | Página Sócios - Filtros por tipo/status | FE | 4h | FE1 | F2-FE-007 |
| F2-FE-009 | Página Detalhe do Sócio - Informações | FE | 8h | FE2 | F2-FE-007 |
| F2-FE-010 | Página Detalhe do Sócio - Histórico | FE | 8h | FE2 | F2-BE-007 |
| F2-FE-011 | Página Detalhe do Sócio - Documentos | FE | 4h | FE2 | F2-FE-009 |
| F2-FE-012 | Componente Timeline de transações | FE | 8h | FE1 | F2-BE-007 |

**Entregável:** Páginas de gestão de sócios

### Semana 12: Simulador de Rodadas (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F2-BE-011 | API Simulador - Cálculos de diluição | BE | 16h | BE1 | F2-BE-009 |
| F2-BE-012 | API Simulador - Salvar cenários | BE | 8h | BE1 | F2-BE-011 |
| F2-FE-013 | Modal Simulador - Inputs | FE | 8h | FE1 | F2-BE-011 |
| F2-FE-014 | Modal Simulador - Resultados + Tabela | FE | 8h | FE2 | F2-FE-013 |

**Entregável:** Simulador de rodadas funcional

---

## FASE 3: CONTRATOS
**Duração:** 5 semanas (200 horas)  
**Objetivo:** Implementar gestão de contratos dinâmicos

### Semana 13: Schema + Templates (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F3-DB-001 | Entidades EF Core - ContractTemplate, Clause | DB | 8h | BE1 | F1-DB-002 |
| F3-DB-002 | Entidades EF Core - Contract, ContractParty, ContractClause | DB | 8h | BE1 | F3-DB-001 |
| F3-BE-001 | API ContractTemplate (CRUD) | BE | 8h | BE1 | F3-DB-001 |
| F3-BE-002 | API Clause (CRUD + categorização) | BE | 8h | BE2 | F3-DB-001 |
| F3-BE-003 | Seeds - Templates padrão (6 tipos) | BE | 4h | BE2 | F3-BE-001 |
| F3-BE-004 | Seeds - Cláusulas padrão | BE | 4h | BE2 | F3-BE-002 |

**Entregável:** APIs de templates e cláusulas

### Semana 14: API de Contratos (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F3-BE-005 | API Contract (criar, listar, detalhar) | BE | 8h | BE1 | F3-DB-002 |
| F3-BE-006 | API ContractParty (adicionar, remover partes) | BE | 8h | BE1 | F3-BE-005 |
| F3-BE-007 | API ContractClause (selecionar cláusulas) | BE | 8h | BE2 | F3-BE-005 |
| F3-BE-008 | Motor de geração de contrato (merge template + dados) | BE | 16h | BE1 | F3-BE-007 |

**Entregável:** Motor de geração de contratos

### Semana 15: Frontend Contratos (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F3-FE-001 | Página Contratos - Header + Templates | FE | 8h | FE1 | F3-BE-001 |
| F3-FE-002 | Página Contratos - Lista de contratos | FE | 8h | FE1 | F3-BE-005 |
| F3-FE-003 | Página Contratos - Filtros e busca | FE | 4h | FE1 | F3-FE-002 |
| F3-FE-004 | Modal Contract Builder - Step 1: Tipo | FE | 4h | FE2 | F3-BE-001 |
| F3-FE-005 | Modal Contract Builder - Step 2: Partes | FE | 8h | FE2 | F3-FE-004 |
| F3-FE-006 | Modal Contract Builder - Step 3: Cláusulas | FE | 8h | FE2 | F3-FE-005 |

**Entregável:** Página de contratos + Contract Builder (parcial)

### Semana 16: Contract Builder + Detalhe (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F3-FE-007 | Modal Contract Builder - Step 4: Dados | FE | 8h | FE1 | F3-FE-006 |
| F3-FE-008 | Modal Contract Builder - Step 5: Preview | FE | 8h | FE1 | F3-BE-008 |
| F3-FE-009 | Modal Detalhe do Contrato - Info + Timeline | FE | 8h | FE2 | F3-BE-005 |
| F3-FE-010 | Modal Detalhe do Contrato - Partes + Status | FE | 8h | FE2 | F3-FE-009 |
| F3-FE-011 | Visualizador de contrato (PDF preview) | FE | 8h | FE1 | F3-BE-008 |

**Entregável:** Contract Builder completo

### Semana 17: Assinatura + Workflow (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F3-BE-009 | Integração com provedor de assinatura digital | INT | 16h | BE1 | F3-BE-008 |
| F3-BE-010 | Webhook de atualização de status | BE | 8h | BE1 | F3-BE-009 |
| F3-BE-011 | Trigger de workflow ao criar contrato | BE | 8h | BE2 | F3-BE-005 |
| F3-FE-012 | Ações de assinatura (enviar, lembrar) | FE | 4h | FE2 | F3-BE-009 |
| F3-TST-004 | Testes E2E - Fluxo de contrato | TST | 4h | QA | F3-FE-012 |

**Entregável:** Fluxo de assinatura digital

---

## FASE 4: VESTING & METAS
**Duração:** 4 semanas (160 horas)  
**Objetivo:** Implementar programas de vesting

### Semana 18: Schema + APIs Vesting (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F4-DB-001 | Entidades EF Core - VestingPlan, VestingGrant | DB | 8h | BE1 | F2-DB-002 |
| F4-DB-002 | Entidades EF Core - VestingSchedule, VestingMilestone | DB | 8h | BE1 | F4-DB-001 |
| F4-BE-001 | API VestingPlan (CRUD) | BE | 8h | BE1 | F4-DB-001 |
| F4-BE-002 | API VestingGrant (criar, listar) | BE | 8h | BE2 | F4-DB-001 |
| F4-BE-003 | API VestingGrant (calcular vested) | BE | 8h | BE2 | F4-BE-002 |

**Entregável:** APIs de Vesting Plans e Grants

### Semana 19: Schedule + Milestones (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F4-BE-004 | Geração automática de VestingSchedule | BE | 16h | BE1 | F4-BE-002 |
| F4-BE-005 | API VestingMilestone (CRUD) | BE | 8h | BE2 | F4-DB-002 |
| F4-BE-006 | API VestingMilestone (atualizar progresso) | BE | 8h | BE2 | F4-BE-005 |
| F4-BE-007 | Job de processamento de vesting (mensal) | BE | 8h | BE1 | F4-BE-004 |

**Entregável:** Motor de vesting por tempo e metas

### Semana 20: Frontend Vesting (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F4-FE-001 | Página Vesting - Pool summary | FE | 8h | FE1 | F4-BE-001 |
| F4-FE-002 | Página Vesting - Dashboard do beneficiário | FE | 8h | FE1 | F4-BE-003 |
| F4-FE-003 | Página Vesting - Progress bar + timeline | FE | 8h | FE2 | F4-FE-002 |
| F4-FE-004 | Página Vesting - Cards de metas | FE | 8h | FE2 | F4-BE-006 |
| F4-FE-005 | Página Vesting - Tabela de grants | FE | 8h | FE1 | F4-BE-002 |

**Entregável:** Página de Vesting completa

### Semana 21: Grants Admin + Testes (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F4-FE-006 | Modal Novo Grant | FE | 8h | FE1 | F4-BE-002 |
| F4-FE-007 | Modal Editar Grant | FE | 4h | FE1 | F4-FE-006 |
| F4-FE-008 | Modal Gerenciar Metas | FE | 8h | FE2 | F4-BE-005 |
| F4-BE-008 | Notificações de vesting (próximo evento) | BE | 8h | BE2 | F4-BE-007 |
| F4-TST-005 | Testes unitários - Motor de vesting | TST | 8h | QA | F4-BE-007 |
| F4-TST-006 | Testes E2E - Fluxo de vesting | TST | 4h | QA | F4-FE-008 |

**Entregável:** Gestão completa de vesting

---

## FASE 5: VALUATION + FINANCEIRO
**Duração:** 4 semanas (160 horas)  
**Objetivo:** Implementar avaliação e dados financeiros

### Semana 22: Schema + APIs Valuation (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F5-DB-001 | Entidades EF Core - Valuation, ValuationMethod | DB | 8h | BE1 | F1-DB-002 |
| F5-DB-002 | Entidades EF Core - FinancialPeriod, FinancialMetric | DB | 8h | BE1 | F5-DB-001 |
| F5-BE-001 | API Valuation (CRUD) | BE | 8h | BE1 | F5-DB-001 |
| F5-BE-002 | API ValuationMethod (cálculos) | BE | 16h | BE2 | F5-BE-001 |

**Entregável:** APIs de Valuation

### Semana 23: Cálculos + Financeiro (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F5-BE-003 | Cálculo - Múltiplo de ARR | BE | 8h | BE1 | F5-BE-002 |
| F5-BE-004 | Cálculo - DCF simplificado | BE | 8h | BE1 | F5-BE-002 |
| F5-BE-005 | Cálculo - Múltiplo de EBITDA | BE | 4h | BE1 | F5-BE-002 |
| F5-BE-006 | API FinancialPeriod (CRUD) | BE | 8h | BE2 | F5-DB-002 |
| F5-BE-007 | API FinancialMetric (CRUD + cálculos) | BE | 8h | BE2 | F5-BE-006 |
| F5-BE-008 | Variação automática (vs período anterior) | BE | 4h | BE2 | F5-BE-007 |

**Entregável:** Motor de cálculo de valuation

### Semana 24: Frontend Valuation (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F5-FE-001 | Página Valuation - Hero banner | FE | 8h | FE1 | F5-BE-001 |
| F5-FE-002 | Página Valuation - Cards de metodologias | FE | 8h | FE1 | F5-BE-002 |
| F5-FE-003 | Página Valuation - Inputs utilizados | FE | 4h | FE2 | F5-FE-002 |
| F5-FE-004 | Página Valuation - Gráfico histórico | FE | 8h | FE2 | F5-BE-001 |
| F5-FE-005 | Página Valuation - Tabela de histórico | FE | 4h | FE2 | F5-FE-004 |
| F5-FE-006 | Modal Novo Valuation | FE | 8h | FE1 | F5-BE-002 |

**Entregável:** Página de Valuation completa

### Semana 25: Frontend Financeiro (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F5-FE-007 | Página Financeiro - Seletor de período | FE | 4h | FE1 | F5-BE-006 |
| F5-FE-008 | Página Financeiro - Cards de métricas | FE | 8h | FE1 | F5-BE-007 |
| F5-FE-009 | Página Financeiro - Gráfico MRR | FE | 8h | FE2 | F5-FE-008 |
| F5-FE-010 | Página Financeiro - Composição de receita | FE | 4h | FE2 | F5-FE-008 |
| F5-FE-011 | Página Financeiro - Documentos | FE | 8h | FE1 | F5-BE-006 |
| F5-TST-007 | Testes E2E - Valuation + Financeiro | TST | 8h | QA | F5-FE-011 |

**Entregável:** Módulo financeiro completo

---

## FASE 6: PORTAL + COMUNICAÇÕES
**Duração:** 4 semanas (160 horas)  
**Objetivo:** Implementar portal do investidor e comunicações

### Semana 26: Schema + APIs Portal (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F6-DB-001 | Entidades EF Core - Communication, CommunicationView | DB | 8h | BE1 | F1-DB-002 |
| F6-DB-002 | Entidades EF Core - Document (completo) | DB | 4h | BE1 | F6-DB-001 |
| F6-BE-001 | API Communication (CRUD) | BE | 8h | BE1 | F6-DB-001 |
| F6-BE-002 | API CommunicationView (rastreamento) | BE | 4h | BE1 | F6-BE-001 |
| F6-BE-003 | API Document (upload, download, listagem) | BE | 8h | BE2 | F6-DB-002 |
| F6-BE-004 | API Portal - Dados do investidor | BE | 8h | BE2 | F2-BE-009 |

**Entregável:** APIs de comunicação e portal

### Semana 27: Frontend Portal (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F6-FE-001 | Portal - Layout específico (header, navegação) | FE | 8h | FE1 | F6-BE-004 |
| F6-FE-002 | Portal - Hero banner (investimento) | FE | 8h | FE1 | F6-FE-001 |
| F6-FE-003 | Portal - Gráfico evolução do investimento | FE | 8h | FE2 | F6-FE-002 |
| F6-FE-004 | Portal - Métricas da empresa | FE | 4h | FE2 | F6-FE-001 |
| F6-FE-005 | Portal - Cap Table resumido | FE | 4h | FE2 | F6-FE-001 |
| F6-FE-006 | Portal - Lista de comunicações | FE | 8h | FE1 | F6-BE-001 |

**Entregável:** Portal do Investidor (visualização)

### Semana 28: Comunicações + Docs (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F6-FE-007 | Portal - Data Room (documentos) | FE | 8h | FE1 | F6-BE-003 |
| F6-FE-008 | Admin - Página de Comunicações | FE | 8h | FE2 | F6-BE-001 |
| F6-FE-009 | Admin - Editor de comunicação (rich text) | FE | 8h | FE2 | F6-FE-008 |
| F6-FE-010 | Admin - Seletor de visibilidade | FE | 4h | FE2 | F6-FE-009 |
| F6-BE-005 | Envio de email para comunicações | BE | 8h | BE1 | F6-BE-001 |
| F6-BE-006 | Sistema de notificações in-app | BE | 4h | BE2 | F6-BE-001 |

**Entregável:** Sistema de comunicações completo

### Semana 29: Notificações + Testes (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F6-FE-011 | Componente de notificações (sino) | FE | 8h | FE1 | F6-BE-006 |
| F6-FE-012 | Página de notificações | FE | 4h | FE1 | F6-FE-011 |
| F6-BE-007 | Templates de email (transacionais) | BE | 8h | BE1 | F6-BE-005 |
| F6-BE-008 | Preferências de notificação por usuário | BE | 4h | BE2 | F6-BE-006 |
| F6-TST-008 | Testes E2E - Portal do Investidor | TST | 8h | QA | F6-FE-007 |
| F6-TST-009 | Testes E2E - Comunicações | TST | 8h | QA | F6-FE-010 |

**Entregável:** Sistema de notificações completo

---

## FASE 7: INTEGRAÇÕES + POLISH
**Duração:** 3 semanas (120 horas)  
**Objetivo:** API pública, integrações e refinamentos

### Semana 30: Workflows + Aprovações (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F7-DB-001 | Entidades EF Core - Workflow, WorkflowStep, WorkflowApproval | DB | 8h | BE1 | F1-DB-002 |
| F7-BE-001 | API Workflow (CRUD) | BE | 8h | BE1 | F7-DB-001 |
| F7-BE-002 | API WorkflowStep (avançar, retroceder) | BE | 8h | BE2 | F7-BE-001 |
| F7-BE-003 | API WorkflowApproval (aprovar, rejeitar) | BE | 8h | BE2 | F7-BE-002 |
| F7-FE-001 | Página Aprovações - Lista de workflows | FE | 8h | FE1 | F7-BE-001 |

**Entregável:** Sistema de workflows

### Semana 31: Dashboard + Exports (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F7-FE-002 | Página Aprovações - Cards de workflow | FE | 8h | FE1 | F7-BE-003 |
| F7-FE-003 | Página Aprovações - Timeline de etapas | FE | 8h | FE2 | F7-FE-002 |
| F7-FE-004 | Dashboard - Consolidação final | FE | 8h | FE1 | Todas |
| F7-BE-004 | Export PDF - Cap Table | BE | 8h | BE1 | F2-BE-009 |
| F7-BE-005 | Export Excel - Cap Table | BE | 4h | BE1 | F7-BE-004 |
| F7-BE-006 | Export PDF - Relatórios | BE | 4h | BE2 | F5-BE-007 |

**Entregável:** Dashboard final + exports

### Semana 32: API Pública + Documentação (40h)

| ID | Tarefa | Tipo | Horas | Responsável | Dependência |
|----|--------|------|-------|-------------|-------------|
| F7-BE-007 | API Pública - Endpoints de leitura | BE | 8h | BE1 | Todas |
| F7-BE-008 | API Pública - Autenticação (API Keys) | BE | 8h | BE1 | F7-BE-007 |
| F7-BE-009 | Webhooks - Eventos principais | BE | 8h | BE2 | F7-BE-008 |
| F7-DOC-001 | Documentação da API (Swagger) | DOC | 8h | BE1 | F7-BE-007 |
| F7-DOC-002 | Guia do usuário | DOC | 4h | QA | Todas |
| F7-TST-010 | Testes de carga e performance | TST | 4h | QA | Todas |

**Entregável:** API Pública + Documentação

---

## 6. Cronograma Detalhado

### 6.1 Visão Geral (Gantt Simplificado)

```
Semana:  1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 32
         |-----F0-----|--F1--|--------F2--------|--------F3--------|-----F4-----|-----F5-----|-----F6-----|--F7--|
Fase 0   ████████████
Fase 1                  ██████████
Fase 2                            ████████████████████
Fase 3                                              ████████████████████
Fase 4                                                                ████████████████
Fase 5                                                                              ████████████████
Fase 6                                                                                            ████████████████
Fase 7                                                                                                          ████████████
```

### 6.2 Marcos (Milestones)

| Marco | Semana | Data (início Jan/25) | Entregável |
|-------|--------|----------------------|------------|
| M1 | 4 | Fev/2025 | Setup + Billing completo |
| M2 | 7 | Mar/2025 | Core (Auth + Company) completo |
| M3 | 12 | Abr/2025 | Cap Table + Simulador completo |
| M4 | 17 | Mai/2025 | Contratos completo |
| M5 | 21 | Jun/2025 | Vesting completo |
| M6 | 25 | Jul/2025 | Valuation + Financeiro completo |
| M7 | 29 | Ago/2025 | Portal + Comunicações completo |
| M8 | 32 | Set/2025 | **MVP Completo** |

### 6.3 Checkpoints de Qualidade

| Checkpoint | Semana | Critérios |
|------------|--------|-----------|
| CP1 | 4 | CI/CD funcionando, cobertura > 70% |
| CP2 | 12 | Performance: < 200ms p95 APIs Cap Table |
| CP3 | 17 | Segurança: Pen test em contratos |
| CP4 | 25 | UX: Validação com 5 usuários reais |
| CP5 | 32 | Load test: 100 usuários simultâneos |

---

## 7. Dependências entre Tarefas

### 7.1 Dependências Críticas (Caminho Crítico)

```
F0-CFG-003 (Setup Backend)
    └── F0-DB-001 (Schema Billing)
        └── F1-DB-001 (Schema User)
            └── F1-BE-001 (API Auth)
                └── F1-BE-008 (RBAC)
                    └── F2-DB-001 (Schema Shareholder)
                        └── F2-BE-009 (View Cap Table)
                            └── F7-FE-004 (Dashboard Final)
```

### 7.2 Tarefas Paralelas (Podem executar em paralelo)

| Período | Backend | Frontend |
|---------|---------|----------|
| Sem 3-4 | APIs Billing | Design System |
| Sem 8-10 | APIs Shareholder/Share | Cap Table UI |
| Sem 13-15 | APIs Contract | Contract Builder UI |
| Sem 18-20 | APIs Vesting | Vesting UI |
| Sem 22-24 | APIs Valuation | Valuation UI |

### 7.3 Matriz de Dependências

```
         F0  F1  F2  F3  F4  F5  F6  F7
    F0   -   →   →   →   →   →   →   →
    F1   -   -   →   →   →   →   →   →
    F2   -   -   -   ↗   →   →   →   →
    F3   -   -   -   -   ↗   -   -   →
    F4   -   -   ←   ↙   -   -   -   →
    F5   -   -   -   -   -   -   -   →
    F6   -   -   -   -   -   -   -   →
    F7   -   -   -   -   -   -   -   -

Legenda:
→ Dependência direta (A precisa terminar antes de B)
↗ Dependência parcial (Algumas tarefas de B dependem de A)
↙ Integração (B se integra com A mas pode iniciar antes)
```

---

## 8. Critérios de Aceite

### 8.1 Por Módulo

#### Billing (Modo Cadastral)
- [ ] Clientes podem ser cadastrados manualmente
- [ ] Planos podem ser criados e configurados
- [ ] Assinaturas podem ser criadas e gerenciadas manualmente
- [ ] Faturas podem ser geradas (manual ou automática por período)
- [ ] Faturamento pode ser direcionado para múltiplas empresas
- [ ] Pagamentos podem ser registrados manualmente
- [ ] Relatório de inadimplência disponível

**Nota:** Integração com gateway de pagamento será implementada em versão futura.

#### Core
- [ ] Login com email/senha funciona (via Firebase)
- [ ] Login social (Google, etc.) funciona (via Firebase)
- [ ] Recuperação de senha por email funciona (via Firebase)
- [ ] Usuário pode alternar entre empresas
- [ ] Permissões são respeitadas em todas as APIs

#### Cap Table
- [ ] Cap Table mostra distribuição correta
- [ ] Sócios podem ser adicionados/editados/removidos
- [ ] Transações de ações são registradas no ledger
- [ ] Simulador calcula diluição corretamente
- [ ] Exports PDF/Excel funcionam

#### Contratos
- [ ] Templates podem ser criados/editados
- [ ] Cláusulas podem ser organizadas por categoria
- [ ] Contract Builder gera contrato válido
- [ ] Integração com assinatura digital funciona
- [ ] Status do contrato é atualizado em tempo real

#### Vesting
- [ ] Planos podem ser criados com configurações
- [ ] Grants são calculados corretamente
- [ ] Vesting por tempo é processado automaticamente
- [ ] Metas podem ser atualizadas com progresso
- [ ] Beneficiário vê dashboard correto

#### Valuation
- [ ] Valuations podem ser registrados
- [ ] 3 metodologias são calculadas automaticamente
- [ ] Histórico é mantido com gráfico
- [ ] Preço por ação é atualizado na empresa

#### Portal
- [ ] Investidor vê apenas dados autorizados
- [ ] Evolução do investimento é calculada
- [ ] Comunicações são recebidas conforme visibilidade
- [ ] Data Room mostra documentos corretos

### 8.2 Não-Funcionais

| Requisito | Critério | Métrica |
|-----------|----------|---------|
| Performance | Tempo de resposta | p95 < 200ms |
| Disponibilidade | Uptime | > 99.5% |
| Segurança | Vulnerabilidades | 0 críticas, 0 altas |
| Cobertura | Testes unitários | > 80% |
| Cobertura | Testes E2E | Fluxos críticos 100% |
| Acessibilidade | WCAG | Nível AA |
| i18n | Idiomas | PT, EN, ES |

---

## 9. Riscos e Mitigações

### 9.1 Riscos Técnicos

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Integração com assinatura digital | Média | Alto | Definir provedor na Fase 0, POC na Semana 2 |
| Performance do Cap Table | Média | Médio | View materializada, cache em Redis |
| Complexidade do motor de contratos | Alta | Médio | Começar simples, iterar |
| Cálculos de vesting incorretos | Média | Alto | Testes extensivos, revisão de regras |

### 9.2 Riscos de Projeto

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Escopo creep | Alta | Alto | PRD fechado, change request formal |
| Atraso em dependências | Média | Médio | Buffer de 10% por fase |
| Rotatividade de equipe | Baixa | Alto | Documentação, pair programming |
| Requisitos mal definidos | Média | Alto | Protótipo validado, demos semanais |

### 9.3 Contingências

| Cenário | Ação |
|---------|------|
| Atraso > 1 semana em fase | Revisão de escopo, priorização |
| Bug crítico em produção | Hotfix imediato, postmortem |
| Integração falha | Fallback para solução manual |
| Performance abaixo | Sprint dedicado para otimização |

---

## Anexo A: Template de Tarefa

```markdown
## [ID]: Título da Tarefa

**Estimativa:** Xh
**Responsável:** Nome
**Dependências:** [IDs]

### Descrição
Descrição detalhada do que deve ser feito.

### Critérios de Aceite
- [ ] Critério 1
- [ ] Critério 2
- [ ] Critério 3

### Notas Técnicas
- Observações relevantes
- Links para documentação

### Checklist
- [ ] Código implementado
- [ ] Testes escritos
- [ ] PR criado
- [ ] Code review aprovado
- [ ] Merge na develop
```

---

## Anexo B: Comandos Úteis para IA

### Para iniciar uma tarefa:
```
Implemente a tarefa [ID] seguindo:
1. Leia a especificação em Partnership_Manager_MER_Entidades.md
2. Siga os critérios de aceite
3. Crie testes unitários
4. Documente a API (se aplicável)
```

### Para revisar código:
```
Revise o PR para a tarefa [ID]:
1. Verifique aderência à especificação
2. Verifique cobertura de testes
3. Verifique performance
4. Verifique segurança
```

### Para gerar testes:
```
Gere testes para a tarefa [ID]:
1. Testes unitários para regras de negócio
2. Testes de integração para APIs
3. Considere casos de borda
4. Considere cenários de erro
```

---

## Anexo C: Definições de Pronto (DoD) por Tipo

### Backend (API)
- [ ] Endpoint implementado conforme spec
- [ ] Validação de entrada (DTO + class-validator)
- [ ] Tratamento de erros padronizado
- [ ] Testes unitários (services)
- [ ] Testes de integração (controllers)
- [ ] Swagger documentado
- [ ] Permissões implementadas

### Frontend (Página)
- [ ] Layout conforme protótipo
- [ ] Responsivo (mobile, tablet, desktop)
- [ ] Estados de loading, erro, vazio
- [ ] Integração com API
- [ ] Testes de componente
- [ ] i18n implementado
- [ ] Acessibilidade verificada

### Database (Schema)
- [ ] Entidades EF Core criado
- [ ] Migration gerada e testada
- [ ] Seeds de dados de teste
- [ ] Índices criados
- [ ] Constraints validadas
- [ ] Documentação atualizada

---

*Documento gerado em 19/01/2025 - Partnership Manager v1.0*
