# Partnership Manager
## Fase 2 - CapTable: Plano de Execução

**Versão:** 1.1  
**Data:** 23 de Janeiro de 2025  
**Duração Estimada:** 5 semanas (200 horas)  
**Regime:** 40 horas/semana (8h/dia × 5 dias)  
**Ambiente:** GitHub Agent (Copilot/Cursor AI)  
**Baseado em:** DATABASE_DOCUMENTATION.md v1.0.0

---

## 📋 Sumário

1. [Análise de Gap Arquitetural](#1-análise-de-gap-arquitetural)
2. [Escopo da Fase 2](#2-escopo-da-fase-2)
3. [Pré-Requisitos e Dependências](#3-pré-requisitos-e-dependências)
4. [Estrutura de Tarefas Atômicas](#4-estrutura-de-tarefas-atômicas)
5. [Cronograma Detalhado](#5-cronograma-detalhado)
6. [Critérios de Aceite](#6-critérios-de-aceite)
7. [Atualização de Documentação](#7-atualização-de-documentação)
8. [Comandos para GitHub Agent](#8-comandos-para-github-agent)
   - [8.0 Controle de Progresso (OBRIGATÓRIO)](#80-controle-de-progresso-obrigatório)
   - [8.1 Contexto Obrigatório](#81-contexto-obrigatório-para-cada-tarefa)
   - [8.2-8.6 Templates de Prompts](#82-prompt-template-criar-migration)

---

## 1. Análise de Gap Arquitetural

### 1.1 Estrutura Atual do Banco (Conforme DATABASE_DOCUMENTATION.md)

```
📦 partnership_manager (Database Atual)
├── 🏢 Core Module
│   ├── companies          ← Entidade raiz atual (PROBLEMA!)
│   ├── users              ← FK: company_id
│   ├── user_roles         ← FK: user_id
│   └── audit_logs         ← FK: company_id, user_id
│
└── 💰 Billing Module (SEPARADO do Core)
    ├── BillingClients     ← Clientes de faturamento (não integrado)
    ├── BillingPlans
    ├── BillingSubscriptions
    ├── BillingInvoices
    └── BillingPayments
```

### 1.2 Problema Identificado

| Situação Atual | Situação Necessária |
|----------------|---------------------|
| `companies` é a entidade raiz | `clients` deve ser a entidade raiz (nosso cliente SaaS) |
| `users` → `company_id` (1 empresa) | `clients` → N `companies` gerenciadas |
| `BillingClients` separado do Core | Billing deve estar vinculado a `clients` |
| Não há multi-tenancy por cliente | Clara separação: quem paga vs quem é gerenciado |

### 1.3 Nova Hierarquia Proposta

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         PLATAFORMA SaaS                                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │ CLIENT (Nosso Cliente SaaS) ─────────────────────────────────┐  │   │
│  │ - Escritório de advocacia                                    │  │   │
│  │ - Aceleradora                                                │  │   │
│  │ - Holding familiar                                           │  │   │
│  │ - Empresa individual                                    ┌────┴──┴┐  │
│  │                                                         │BILLING │  │
│  │  ┌──────────────────────────────────────────────────┐   │CLIENTE │  │
│  │  │ COMPANIES (Empresas Gerenciadas)                 │   │────────│  │
│  │  │                                                  │   │Plan    │  │
│  │  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ │   │Invoice │  │
│  │  │  │ COMPANY A   │ │ COMPANY B   │ │ COMPANY C   │ │   │Payment │  │
│  │  │  │ (Startup X) │ │ (Startup Y) │ │ (Holding Z) │ │   └────────┘  │
│  │  │  ├─────────────┤ ├─────────────┤ ├─────────────┤ │               │
│  │  │  │• Cap Table  │ │• Cap Table  │ │• Cap Table  │ │               │
│  │  │  │• Shareholders│ │• Shareholders│ │• Shareholders│ │               │
│  │  │  │• Contracts  │ │• Contracts  │ │• Contracts  │ │               │
│  │  │  │• Vesting    │ │• Vesting    │ │• Vesting    │ │               │
│  │  │  └─────────────┘ └─────────────┘ └─────────────┘ │               │
│  │  │                                                  │               │
│  │  └──────────────────────────────────────────────────┘               │
│  │                                                                     │
│  │  USERS (Podem acessar 1 ou mais Companies do Client)                │
│  │                                                                     │
│  └─────────────────────────────────────────────────────────────────────┘
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1.4 Impacto nas Entidades Existentes

| Entidade Atual | Mudança Necessária | Justificativa |
|----------------|-------------------|---------------|
| `companies` | Adicionar `client_id` (FK NOT NULL) | Empresa pertence a um Client |
| `users` | Adicionar `client_id` (FK NOT NULL) | Usuário pertence a um Client |
| `users.company_id` | Tornar NULLABLE | Usuário pode acessar múltiplas companies |
| `user_roles` | Sem mudança | Mantém estrutura atual |
| `audit_logs` | Adicionar `client_id` | Rastreabilidade por Client |
| `BillingClients` | Adicionar `client_id` (FK) | Vincular faturamento ao Client Core |
| **Nova:** `clients` | Criar tabela | Entidade raiz do sistema |
| **Nova:** `user_companies` | Criar tabela | N:N entre User e Companies |

### 1.5 Casos de Uso Suportados

| Cenário | Client | Companies | Observação |
|---------|--------|-----------|------------|
| **Startup individual** | Startup ABC Ltda | 1 (ela mesma) | Plano básico |
| **Escritório de advocacia** | Silva & Associados | N (clientes do escritório) | Plano corporativo |
| **Aceleradora** | ACE Startups | N (startups aceleradas) | Plano enterprise |
| **Holding familiar** | Família Santos Holdings | N (empresas da família) | Plano enterprise |
| **Fundo de investimento** | Monashees | N (portfolio companies) | Plano enterprise |

### 1.6 Novo Modelo de Dados (Core Module Atualizado)

```sql
-- NOVA TABELA: clients (entidade raiz)
CREATE TABLE clients (
    id CHAR(36) NOT NULL,
    name VARCHAR(200) NOT NULL,
    trading_name VARCHAR(200) NULL,
    document VARCHAR(20) NOT NULL,          -- CNPJ ou CPF
    document_type VARCHAR(10) NOT NULL,      -- 'cnpj' ou 'cpf'
    email VARCHAR(255) NOT NULL,
    phone VARCHAR(20) NULL,
    logo_url VARCHAR(500) NULL,
    settings JSON NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'Active',
    created_at DATETIME(6) NOT NULL,
    updated_at DATETIME(6) NOT NULL,
    created_by CHAR(36) NULL,
    updated_by CHAR(36) NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    deleted_at DATETIME(6) NULL,
    PRIMARY KEY (id),
    UNIQUE INDEX idx_client_document (document),
    INDEX idx_client_status (status),
    INDEX idx_client_deleted (is_deleted)
);

-- ALTERAÇÃO: companies (adicionar client_id)
ALTER TABLE companies 
    ADD COLUMN client_id CHAR(36) NOT NULL AFTER id,
    ADD INDEX idx_company_client (client_id),
    ADD CONSTRAINT fk_company_client 
        FOREIGN KEY (client_id) REFERENCES clients(id) ON DELETE RESTRICT;

-- ALTERAÇÃO: users (adicionar client_id, tornar company_id nullable)
ALTER TABLE users 
    ADD COLUMN client_id CHAR(36) NOT NULL AFTER id,
    MODIFY COLUMN company_id CHAR(36) NULL,
    ADD INDEX idx_user_client (client_id),
    ADD CONSTRAINT fk_user_client 
        FOREIGN KEY (client_id) REFERENCES clients(id) ON DELETE RESTRICT;

-- NOVA TABELA: user_companies (acesso N:N)
CREATE TABLE user_companies (
    id CHAR(36) NOT NULL,
    user_id CHAR(36) NOT NULL,
    company_id CHAR(36) NOT NULL,
    role VARCHAR(50) NOT NULL DEFAULT 'Viewer',
    is_default TINYINT(1) NOT NULL DEFAULT 0,
    granted_at DATETIME(6) NOT NULL,
    granted_by CHAR(36) NULL,
    created_at DATETIME(6) NOT NULL,
    PRIMARY KEY (id),
    UNIQUE INDEX idx_user_company_unique (user_id, company_id),
    INDEX idx_user_company_user (user_id),
    INDEX idx_user_company_company (company_id),
    CONSTRAINT fk_uc_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_uc_company FOREIGN KEY (company_id) REFERENCES companies(id) ON DELETE CASCADE
);

-- ALTERAÇÃO: BillingClients (vincular ao Core)
ALTER TABLE BillingClients 
    ADD COLUMN core_client_id CHAR(36) NULL,
    ADD INDEX idx_billing_core_client (core_client_id),
    ADD CONSTRAINT fk_billing_core_client 
        FOREIGN KEY (core_client_id) REFERENCES clients(id) ON DELETE SET NULL;
```

---

## 2. Escopo da Fase 2

### 2.1 Entregáveis

```
Fase 2: CapTable
├── 2.1 Correção Arquitetural (Client → Company)
│   ├── Entidade Client (Backend)
│   ├── Migration de banco de dados
│   ├── Atualização de Company (adicionar client_id)
│   ├── Atualização de User (adicionar client_id)
│   └── Frontend: Seletor de Company (Company Switcher)
│
├── 2.2 Shareholders
│   ├── CRUD completo (Backend)
│   ├── Tipos: Fundador, Investidor, Funcionário, Outro
│   ├── Vínculo com User (opcional)
│   └── Frontend: Lista + Detalhes + Formulários
│
├── 2.3 Share Classes
│   ├── CRUD completo (Backend)
│   ├── Direitos de voto, preferência de liquidação
│   └── Frontend: Configuração de classes
│
├── 2.4 Shares (Participações)
│   ├── CRUD com validações de negócio
│   ├── Emissão, Transferência, Cancelamento
│   └── Ledger imutável (ShareTransaction)
│
├── 2.5 Cap Table View
│   ├── View materializada para performance
│   ├── Cálculo de % de participação
│   └── Frontend: Visualização + Gráficos
│
└── 2.6 Simulador de Rodadas
    ├── API de cálculo de diluição
    ├── Salvar cenários
    └── Frontend: Modal interativo
```

### 2.2 Fora do Escopo (Fases Futuras)

- ❌ Contratos e assinatura digital (Fase 3)
- ❌ Vesting e milestones (Fase 4)
- ❌ Valuation e financeiro (Fase 5)
- ❌ Portal do investidor (Fase 6)

---

## 3. Pré-Requisitos e Dependências

### 3.1 Status Atual (Conforme DATABASE_DOCUMENTATION.md)

```
✅ IMPLEMENTADO                          ❌ PENDENTE
─────────────────────────────────────    ─────────────────────────────────────
✅ companies (Core)                      ❌ clients (nova tabela)
✅ users (FK: company_id)                ❌ user_companies (N:N)
✅ user_roles                            ❌ shareholders
✅ audit_logs                            ❌ share_classes
✅ BillingClients                        ❌ shares
✅ BillingPlans                          ❌ share_transactions
✅ BillingSubscriptions                  ❌ mv_cap_table (view)
✅ BillingInvoices
✅ BillingPayments
```

### 3.2 Checklist de Pré-Requisitos para Fase 2

```
┌─────────────────────────────────────────────────────────────────┐
│              CHECKLIST PRÉ-FASE 2 (VERIFICAR ANTES)             │
├─────────────────────────────────────────────────────────────────┤
│ INFRAESTRUTURA                                                  │
│ ☐ Docker Compose funcionando (MySQL 8.0 + Redis)                │
│ ☐ Database partnership_manager acessível                        │
│ ☐ Usuário pm_user com permissões adequadas                      │
│ ☐ CI/CD configurado (GitHub Actions)                            │
├─────────────────────────────────────────────────────────────────┤
│ BACKEND (.NET 9)                                                │
│ ☐ Autenticação JWT funcionando (refresh_token em users)         │
│ ☐ CompanyContextMiddleware implementado                         │
│ ☐ AuditLogInterceptor funcionando                               │
│ ☐ FluentValidation configurado                                  │
│ ☐ Dapper Repositories seguindo padrão existente                 │
├─────────────────────────────────────────────────────────────────┤
│ FRONTEND (React 18 + Vite)                                      │
│ ☐ Design System implementado (Tailwind)                         │
│ ☐ Layout principal (Sidebar + Header)                           │
│ ☐ Zustand stores configurados (useAuthStore, useCompanyStore)   │
│ ☐ React Query configurado                                       │
│ ☐ Rotas protegidas funcionando                                  │
├─────────────────────────────────────────────────────────────────┤
│ DADOS DE TESTE                                                  │
│ ☐ Empresa Demo: a1b2c3d4-e5f6-7890-abcd-ef1234567890           │
│ ☐ Admin: admin@demo.com / Admin@123                             │
│ ☐ Role Admin atribuída                                          │
└─────────────────────────────────────────────────────────────────┘
```

### 3.3 Dependências entre Tarefas (Caminho Crítico)

```
┌──────────────────────────────────────────────────────────────────────────┐
│                           CAMINHO CRÍTICO                                │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  F2-ARC-DB-001 (Criar tabela clients)                                    │
│       │                                                                  │
│       ├──► F2-ARC-DB-002 (Alterar companies: add client_id)              │
│       │         │                                                        │
│       │         └──► F2-ARC-DB-003 (Alterar users: add client_id)        │
│       │                   │                                              │
│       │                   └──► F2-ARC-DB-004 (Criar user_companies)      │
│       │                             │                                    │
│       │                             └──► F2-ARC-DB-005 (Seed data)       │
│       │                                       │                          │
│       └──► F2-ARC-BE-001 (Entity Client.cs)   │                          │
│                   │                           │                          │
│                   ▼                           ▼                          │
│            F2-ARC-BE-005 (ClientController) ◄─┘                          │
│                   │                                                      │
│                   ▼                                                      │
│            F2-ARC-FE-005 (CompanySwitcher)                               │
│                   │                                                      │
│                   ▼                                                      │
│  ┌────────────────┴────────────────────────────────────────────┐        │
│  │                 MÓDULO SHAREHOLDERS                          │        │
│  │  F2-SHR-DB-001 (Criar tabela shareholders)                   │        │
│  │       │                                                      │        │
│  │       └──► F2-SHR-BE-* ──► F2-SHR-FE-*                       │        │
│  └──────────────────────────────────────────────────────────────┘        │
│                   │                                                      │
│                   ▼                                                      │
│  ┌────────────────┴────────────────────────────────────────────┐        │
│  │                 MÓDULO SHARES + CAP TABLE                    │        │
│  │  F2-SHA-DB-001 ──► F2-SHA-BE-* ──► F2-CAP-*                  │        │
│  └──────────────────────────────────────────────────────────────┘        │
│                   │                                                      │
│                   ▼                                                      │
│            F2-DOC-001 (Atualizar DATABASE_DOCUMENTATION.md)              │
│                                                                          │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## 4. Estrutura de Tarefas Atômicas

### 4.1 Convenção de Nomenclatura

```
[FASE]-[MÓDULO]-[TIPO]-[NÚMERO]: Descrição

Módulos Fase 2:
- ARC: Arquitetura (Client/Company)
- SHR: Shareholders
- SHC: Share Classes
- SHA: Shares
- CAP: Cap Table View
- SIM: Simulador

Tipos:
- DB: Database/Migration
- BE: Backend
- FE: Frontend
- INT: Integração
- TST: Teste
- DOC: Documentação
```

### 4.2 Tamanhos de Tarefa (Otimizado para GitHub Agent)

| Tamanho | Horas | Linhas de Código | Ideal para AI |
|---------|-------|------------------|---------------|
| **XS** | 1-2h | < 50 linhas | ✅ Perfeito |
| **S** | 2-4h | 50-150 linhas | ✅ Muito bom |
| **M** | 4-8h | 150-300 linhas | ⚠️ Dividir se possível |
| **L** | 8-16h | 300-500 linhas | ❌ Deve ser dividida |

> **Regra de Ouro:** Tarefas > 4h devem ser divididas em subtarefas atômicas.

---

## 5. Cronograma Detalhado

### SEMANA 1: Correção Arquitetural (Client → Company)

#### Sprint Goal: Implementar hierarquia Client → Company

| ID | Tarefa | Tipo | Horas | Dependência | Critério de Aceite |
|----|--------|------|-------|-------------|-------------------|
| **F2-ARC-DB-001** | Criar tabela `clients` no MySQL | DB | 2h | - | Tabela criada com campos: id, name, trading_name, document, document_type, email, phone, settings, status, created_at, updated_at |
| **F2-ARC-DB-002** | Migration: Adicionar `client_id` em `companies` | DB | 2h | F2-ARC-DB-001 | FK criada, índice adicionado, constraint NOT NULL |
| **F2-ARC-DB-003** | Migration: Adicionar `client_id` em `users` | DB | 2h | F2-ARC-DB-001 | FK criada, índice adicionado |
| **F2-ARC-DB-004** | Seed: Criar Client demo e vincular Company existente | DB | 1h | F2-ARC-DB-002 | Dados de demo funcionando |
| **F2-ARC-BE-001** | Entidade `Client.cs` no Domain | BE | 2h | F2-ARC-DB-001 | Entidade com propriedades, validações básicas |
| **F2-ARC-BE-002** | DTOs: `ClientRequest`, `ClientResponse` | BE | 2h | F2-ARC-BE-001 | DTOs criados seguindo padrão existente |
| **F2-ARC-BE-003** | Validator: `ClientValidator` (FluentValidation) | BE | 2h | F2-ARC-BE-002 | Validações de CNPJ/CPF, email, campos obrigatórios |
| **F2-ARC-BE-004** | Repository: `IClientRepository` + `ClientRepository` | BE | 4h | F2-ARC-BE-001 | CRUD completo com Dapper |
| **F2-ARC-BE-005** | Controller: `ClientsController` | BE | 4h | F2-ARC-BE-004 | Endpoints REST, [Authorize], Swagger |
| **F2-ARC-BE-006** | Atualizar `Company.cs`: adicionar `ClientId` | BE | 1h | F2-ARC-BE-001 | Propriedade adicionada, navegação configurada |
| **F2-ARC-BE-007** | Atualizar `User.cs`: adicionar `ClientId` | BE | 1h | F2-ARC-BE-001 | Propriedade adicionada |
| **F2-ARC-BE-008** | Middleware: `ClientContextMiddleware` | BE | 4h | F2-ARC-BE-005 | Context de Client disponível em toda requisição |
| **F2-ARC-BE-009** | Atualizar `CompanyContextMiddleware` | BE | 2h | F2-ARC-BE-008 | Validar Company pertence ao Client do usuário |
| **F2-ARC-FE-001** | Type: `Client` no TypeScript | FE | 1h | F2-ARC-BE-002 | Interface TypeScript criada |
| **F2-ARC-FE-002** | Service: `clientService.ts` | FE | 2h | F2-ARC-FE-001 | Chamadas API implementadas |
| **F2-ARC-FE-003** | Store: `useClientStore` (Zustand) | FE | 2h | F2-ARC-FE-002 | Estado global de Client |
| **F2-ARC-FE-004** | Hook: `useClient` (React Query) | FE | 2h | F2-ARC-FE-002 | Query e mutations |
| **F2-ARC-FE-005** | Componente: `CompanySwitcher` | FE | 4h | F2-ARC-FE-003 | Dropdown para trocar de Company |
| **F2-ARC-FE-006** | Atualizar Header com `CompanySwitcher` | FE | 2h | F2-ARC-FE-005 | Integração no layout |

**Subtotal Semana 1:** 42h

---

### SEMANA 2: Shareholders (Backend)

#### Sprint Goal: CRUD completo de Shareholders no Backend

| ID | Tarefa | Tipo | Horas | Dependência | Critério de Aceite |
|----|--------|------|-------|-------------|-------------------|
| **F2-SHR-DB-001** | Criar/Validar tabela `shareholders` | DB | 2h | F2-ARC-DB-002 | Tabela com todos os campos do MER |
| **F2-SHR-DB-002** | Índices de performance em `shareholders` | DB | 1h | F2-SHR-DB-001 | Índices em company_id, type, status, document |
| **F2-SHR-BE-001** | Entidade `Shareholder.cs` | BE | 2h | F2-SHR-DB-001 | Entidade completa com enums |
| **F2-SHR-BE-002** | Enum `ShareholderType` | BE | 1h | - | Founder, Investor, Employee, Advisor, Other |
| **F2-SHR-BE-003** | Enum `ShareholderStatus` | BE | 1h | - | Active, Inactive, Pending |
| **F2-SHR-BE-004** | DTOs: `ShareholderRequest`, `ShareholderResponse` | BE | 2h | F2-SHR-BE-001 | DTOs com mapeamento |
| **F2-SHR-BE-005** | DTO: `ShareholderListResponse` (paginação) | BE | 1h | F2-SHR-BE-004 | Com filtros e ordenação |
| **F2-SHR-BE-006** | Validator: `ShareholderValidator` | BE | 2h | F2-SHR-BE-004 | CPF/CNPJ, email, campos obrigatórios |
| **F2-SHR-BE-007** | Interface: `IShareholderRepository` | BE | 1h | F2-SHR-BE-001 | Contrato do repository |
| **F2-SHR-BE-008** | Repository: `ShareholderRepository` - GetAll | BE | 3h | F2-SHR-BE-007 | Com filtros, paginação, ordenação |
| **F2-SHR-BE-009** | Repository: `ShareholderRepository` - GetById | BE | 1h | F2-SHR-BE-007 | Com includes necessários |
| **F2-SHR-BE-010** | Repository: `ShareholderRepository` - Create | BE | 2h | F2-SHR-BE-007 | Validação de duplicidade |
| **F2-SHR-BE-011** | Repository: `ShareholderRepository` - Update | BE | 2h | F2-SHR-BE-007 | Atualização parcial suportada |
| **F2-SHR-BE-012** | Repository: `ShareholderRepository` - Delete (soft) | BE | 1h | F2-SHR-BE-007 | Soft delete com is_deleted |
| **F2-SHR-BE-013** | Repository: `ShareholderRepository` - GetByDocument | BE | 1h | F2-SHR-BE-007 | Busca por CPF/CNPJ |
| **F2-SHR-BE-014** | Service: `IShareholderService` | BE | 1h | F2-SHR-BE-007 | Interface de serviço |
| **F2-SHR-BE-015** | Service: `ShareholderService` - Regras de negócio | BE | 4h | F2-SHR-BE-014 | Validações de domínio |
| **F2-SHR-BE-016** | Controller: `ShareholdersController` - GET list | BE | 2h | F2-SHR-BE-015 | Endpoint com filtros |
| **F2-SHR-BE-017** | Controller: `ShareholdersController` - GET by id | BE | 1h | F2-SHR-BE-015 | Retorno detalhado |
| **F2-SHR-BE-018** | Controller: `ShareholdersController` - POST | BE | 2h | F2-SHR-BE-015 | Criação com validação |
| **F2-SHR-BE-019** | Controller: `ShareholdersController` - PUT | BE | 2h | F2-SHR-BE-015 | Atualização completa |
| **F2-SHR-BE-020** | Controller: `ShareholdersController` - DELETE | BE | 1h | F2-SHR-BE-015 | Soft delete |
| **F2-SHR-BE-021** | Registrar DI em `Program.cs` | BE | 1h | F2-SHR-BE-015 | Injeção de dependência |
| **F2-SHR-TST-001** | Testes via Swagger | TST | 2h | F2-SHR-BE-021 | Todos endpoints testados |

**Subtotal Semana 2:** 38h

---

### SEMANA 3: Shareholders (Frontend) + Share Classes

#### Sprint Goal: UI de Shareholders + Backend de Share Classes

| ID | Tarefa | Tipo | Horas | Dependência | Critério de Aceite |
|----|--------|------|-------|-------------|-------------------|
| **F2-SHR-FE-001** | Type: `Shareholder` no TypeScript | FE | 1h | F2-SHR-BE-004 | Interface completa |
| **F2-SHR-FE-002** | Service: `shareholderService.ts` | FE | 2h | F2-SHR-FE-001 | Todas chamadas API |
| **F2-SHR-FE-003** | Hook: `useShareholders` (React Query) | FE | 2h | F2-SHR-FE-002 | Query com filtros |
| **F2-SHR-FE-004** | Hook: `useShareholderMutations` | FE | 2h | F2-SHR-FE-002 | Create, Update, Delete |
| **F2-SHR-FE-005** | Componente: `ShareholderCard` | FE | 3h | F2-SHR-FE-001 | Card com info resumida |
| **F2-SHR-FE-006** | Componente: `ShareholderBadge` (tipo) | FE | 1h | - | Badge colorido por tipo |
| **F2-SHR-FE-007** | Componente: `ShareholderFilters` | FE | 2h | - | Filtros por tipo/status |
| **F2-SHR-FE-008** | Página: `ShareholdersListPage` | FE | 4h | F2-SHR-FE-005 | Grid de cards + filtros |
| **F2-SHR-FE-009** | Modal: `ShareholderFormModal` | FE | 4h | F2-SHR-FE-004 | Form com React Hook Form + Zod |
| **F2-SHR-FE-010** | Página: `ShareholderDetailPage` | FE | 4h | F2-SHR-FE-003 | Detalhes + histórico |
| **F2-SHC-DB-001** | Criar/Validar tabela `share_classes` | DB | 2h | F2-ARC-DB-002 | Tabela conforme MER |
| **F2-SHC-BE-001** | Entidade `ShareClass.cs` | BE | 2h | F2-SHC-DB-001 | Com propriedades de direitos |
| **F2-SHC-BE-002** | DTOs: `ShareClassRequest/Response` | BE | 2h | F2-SHC-BE-001 | DTOs completos |
| **F2-SHC-BE-003** | Validator: `ShareClassValidator` | BE | 1h | F2-SHC-BE-002 | Validações de código único |
| **F2-SHC-BE-004** | Repository: `ShareClassRepository` | BE | 3h | F2-SHC-BE-001 | CRUD completo |
| **F2-SHC-BE-005** | Controller: `ShareClassesController` | BE | 3h | F2-SHC-BE-004 | Endpoints REST |
| **F2-SHC-TST-001** | Testes de integração BE | TST | 2h | F2-SHC-BE-005 | Cobertura mínima |

**Subtotal Semana 3:** 40h

---

### SEMANA 4: Shares + Ledger + Cap Table View

#### Sprint Goal: Sistema de ações com ledger imutável

| ID | Tarefa | Tipo | Horas | Dependência | Critério de Aceite |
|----|--------|------|-------|-------------|-------------------|
| **F2-SHA-DB-001** | Criar tabela `shares` | DB | 2h | F2-SHC-DB-001 | Tabela conforme MER |
| **F2-SHA-DB-002** | Criar tabela `share_transactions` | DB | 2h | F2-SHA-DB-001 | Ledger imutável |
| **F2-SHA-DB-003** | Trigger: Impedir UPDATE/DELETE em transactions | DB | 2h | F2-SHA-DB-002 | Imutabilidade garantida |
| **F2-SHA-BE-001** | Entidade `Share.cs` | BE | 2h | F2-SHA-DB-001 | Com navegações |
| **F2-SHA-BE-002** | Entidade `ShareTransaction.cs` | BE | 2h | F2-SHA-DB-002 | Imutável após criação |
| **F2-SHA-BE-003** | Enum `TransactionType` | BE | 1h | - | Issue, Transfer, Cancel, Convert |
| **F2-SHA-BE-004** | DTOs de Shares e Transactions | BE | 2h | F2-SHA-BE-001 | Request/Response |
| **F2-SHA-BE-005** | Repository: `ShareRepository` | BE | 4h | F2-SHA-BE-001 | CRUD + queries especiais |
| **F2-SHA-BE-006** | Repository: `ShareTransactionRepository` | BE | 3h | F2-SHA-BE-002 | Apenas Create + Get |
| **F2-SHA-BE-007** | Service: `ShareService` - Emissão | BE | 4h | F2-SHA-BE-005 | Regras de negócio CT-01 a CT-06 |
| **F2-SHA-BE-008** | Service: `ShareService` - Transferência | BE | 4h | F2-SHA-BE-007 | Validação de saldo |
| **F2-SHA-BE-009** | Service: `ShareService` - Cancelamento | BE | 2h | F2-SHA-BE-007 | Regras de cancelamento |
| **F2-SHA-BE-010** | Controller: `SharesController` | BE | 4h | F2-SHA-BE-007 | Endpoints completos |
| **F2-CAP-DB-001** | View materializada `mv_cap_table` | DB | 4h | F2-SHA-DB-001 | Cálculo de % |
| **F2-CAP-BE-001** | Service: `CapTableService` | BE | 4h | F2-CAP-DB-001 | Refresh + Query |
| **F2-CAP-BE-002** | Controller: `CapTableController` | BE | 2h | F2-CAP-BE-001 | GET cap table |

**Subtotal Semana 4:** 44h

---

### SEMANA 5: Frontend Cap Table + Simulador + Documentação

#### Sprint Goal: UI completa + Simulador + Docs atualizados

| ID | Tarefa | Tipo | Horas | Dependência | Critério de Aceite |
|----|--------|------|-------|-------------|-------------------|
| **F2-CAP-FE-001** | Types: `Share`, `Transaction`, `CapTable` | FE | 2h | F2-SHA-BE-004 | Interfaces TS |
| **F2-CAP-FE-002** | Service: `shareService.ts` | FE | 2h | F2-CAP-FE-001 | API calls |
| **F2-CAP-FE-003** | Service: `capTableService.ts` | FE | 1h | F2-CAP-FE-001 | API calls |
| **F2-CAP-FE-004** | Hook: `useCapTable` | FE | 2h | F2-CAP-FE-003 | React Query |
| **F2-CAP-FE-005** | Componente: `CapTableStats` | FE | 2h | - | Cards de KPIs |
| **F2-CAP-FE-006** | Componente: `CapTableChart` (Donut) | FE | 3h | - | Recharts |
| **F2-CAP-FE-007** | Componente: `CapTableTable` | FE | 4h | F2-CAP-FE-004 | Tabela com sort/filter |
| **F2-CAP-FE-008** | Página: `CapTablePage` | FE | 4h | F2-CAP-FE-005 | Página completa |
| **F2-SIM-BE-001** | Service: `RoundSimulatorService` | BE | 6h | F2-CAP-BE-001 | Cálculos de diluição |
| **F2-SIM-BE-002** | DTO: `SimulationRequest/Response` | BE | 2h | F2-SIM-BE-001 | Estrutura da simulação |
| **F2-SIM-BE-003** | Controller: `SimulatorController` | BE | 2h | F2-SIM-BE-001 | POST simulate |
| **F2-SIM-FE-001** | Modal: `RoundSimulatorModal` | FE | 6h | F2-SIM-BE-003 | Modal interativo |
| **F2-SIM-FE-002** | Componente: `SimulationResults` | FE | 3h | F2-SIM-FE-001 | Tabela de resultados |
| **F2-DOC-001** | Atualizar DATABASE_DOCUMENTATION.md | DOC | 4h | Todas | Nova estrutura documentada |
| **F2-DOC-002** | Atualizar MER com Client | DOC | 2h | F2-ARC-DB-001 | Diagrama atualizado |
| **F2-DOC-003** | Atualizar DOCUMENTACAO_FUNCIONAL.md | DOC | 4h | Todas | Funcionalidades da Fase 2 documentadas |
| **F2-TST-001** | Testes E2E críticos | TST | 4h | Todas | Fluxo principal testado |

**Subtotal Semana 5:** 53h

---

## 6. Critérios de Aceite

### 6.1 Por Módulo

#### Arquitetura (Client → Company)
- [ ] Client pode ser criado via API
- [ ] Company obrigatoriamente pertence a um Client
- [ ] User pode acessar apenas Companies do seu Client
- [ ] Company Switcher permite trocar entre empresas
- [ ] Dados são isolados por Client (multi-tenancy)

#### Shareholders
- [ ] CRUD completo funcionando
- [ ] Filtros por tipo e status
- [ ] Paginação com 10/25/50/100 itens
- [ ] Validação de CPF/CNPJ único por Company
- [ ] Soft delete implementado

#### Share Classes
- [ ] Criar classe com direitos configuráveis
- [ ] Código único por Company
- [ ] Listar classes com totais

#### Shares & Transactions
- [ ] Emitir ações para Shareholder
- [ ] Transferir entre Shareholders
- [ ] Cancelar ações com justificativa
- [ ] Ledger imutável (sem UPDATE/DELETE)
- [ ] Saldo consistente com transações

#### Cap Table
- [ ] Visualização atualizada em < 2s
- [ ] Gráfico de pizza por tipo de sócio
- [ ] Tabela com % de participação
- [ ] Total = 100% (ou menos se houver treasury)

#### Simulador
- [ ] Simular rodada com pre-money e investimento
- [ ] Calcular diluição de cada sócio
- [ ] Mostrar antes/depois

### 6.2 Critérios Técnicos

| Critério | Meta |
|----------|------|
| Cobertura de testes | ≥ 70% |
| Performance API Cap Table | < 200ms p95 |
| Lighthouse Score | ≥ 85 |
| Build sem erros | ✅ |
| Lint sem warnings | ✅ |

---

## 7. Atualização de Documentação

### 7.1 DATABASE_DOCUMENTATION.md - Alterações Necessárias

```markdown
## Alterações a serem feitas no DATABASE_DOCUMENTATION.md

### 1. ATUALIZAR Visão Geral (Seção 1)
- Adicionar módulo "Multi-tenancy" na lista de módulos implementados
- Atualizar diagrama de relacionamentos

### 2. NOVA SEÇÃO: Tabela `clients`
- Adicionar entre "Core Module" e tabela "companies"
- Documentar todos os campos
- Documentar índices e constraints

### 3. ATUALIZAR Tabela `companies`
- Adicionar campo `client_id`
- Adicionar FK para `clients`
- Atualizar índices

### 4. ATUALIZAR Tabela `users`  
- Adicionar campo `client_id`
- Tornar `company_id` nullable
- Adicionar FK para `clients`

### 5. NOVA SEÇÃO: Tabela `user_companies`
- Documentar relacionamento N:N
- Documentar campos e índices

### 6. ATUALIZAR Tabela `BillingClients`
- Adicionar campo `core_client_id`
- Documentar FK para `clients`

### 7. NOVAS SEÇÕES: Módulo Cap Table
- shareholders
- share_classes
- shares
- share_transactions
- mv_cap_table (view materializada)

### 8. ATUALIZAR Migrations Aplicadas
- Adicionar migrations da Fase 2
```

### 7.2 Template: Nova Seção `clients`

```markdown
#### X. clients

Entidade raiz do sistema que representa os clientes do SaaS (quem paga pela plataforma).
Um Client pode gerenciar múltiplas Companies.

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| id | CHAR(36) | NOT NULL | Identificador único (GUID) |
| name | VARCHAR(200) | NOT NULL | Razão social do cliente |
| trading_name | VARCHAR(200) | NULL | Nome fantasia |
| document | VARCHAR(20) | NOT NULL | CNPJ ou CPF |
| document_type | VARCHAR(10) | NOT NULL | 'cnpj' ou 'cpf' |
| email | VARCHAR(255) | NOT NULL | E-mail principal |
| phone | VARCHAR(20) | NULL | Telefone |
| logo_url | VARCHAR(500) | NULL | URL do logotipo |
| settings | JSON | NULL | Configurações personalizadas |
| status | VARCHAR(20) | NOT NULL | Status (Active, Inactive, Suspended) |
| created_at | DATETIME(6) | NOT NULL | Data de criação |
| updated_at | DATETIME(6) | NOT NULL | Data de última atualização |
| created_by | CHAR(36) | NULL | ID do usuário criador |
| updated_by | CHAR(36) | NULL | ID do último usuário que atualizou |
| is_deleted | TINYINT(1) | NOT NULL | Flag de soft delete (padrão: 0) |
| deleted_at | DATETIME(6) | NULL | Data de exclusão (soft delete) |

**Índices:**
- PRIMARY KEY: `id`
- UNIQUE INDEX: `idx_client_document` (document)
- INDEX: `idx_client_status` (status)
- INDEX: `idx_client_deleted` (is_deleted)

**Relacionamentos:**
- 1:N → companies (client_id)
- 1:N → users (client_id)
- 1:1 → BillingClients (core_client_id)

**Valores Padrões:**
- status: 'Active'
- is_deleted: 0
```

### 7.3 Template: Nova Seção `shareholders`

```markdown
#### Y. shareholders

Armazena informações dos sócios/acionistas vinculados a uma empresa.

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| id | CHAR(36) | NOT NULL | Identificador único (GUID) |
| company_id | CHAR(36) | NOT NULL | FK para companies |
| user_id | CHAR(36) | NULL | FK para users (se for usuário do sistema) |
| name | VARCHAR(200) | NOT NULL | Nome completo |
| document | VARCHAR(20) | NOT NULL | CPF ou CNPJ |
| document_type | VARCHAR(10) | NOT NULL | 'cpf' ou 'cnpj' |
| shareholder_type | VARCHAR(20) | NOT NULL | Founder, Investor, Employee, Advisor, Other |
| email | VARCHAR(255) | NULL | E-mail |
| phone | VARCHAR(20) | NULL | Telefone |
| address | JSON | NULL | Endereço completo |
| entry_date | DATE | NOT NULL | Data de entrada na sociedade |
| exit_date | DATE | NULL | Data de saída (se aplicável) |
| status | VARCHAR(20) | NOT NULL | Active, Inactive, Pending |
| notes | TEXT | NULL | Observações |
| created_at | DATETIME(6) | NOT NULL | Data de criação |
| updated_at | DATETIME(6) | NOT NULL | Data de atualização |
| created_by | CHAR(36) | NULL | ID do criador |
| updated_by | CHAR(36) | NULL | ID do atualizador |
| is_deleted | TINYINT(1) | NOT NULL | Soft delete |
| deleted_at | DATETIME(6) | NULL | Data de exclusão |

**Índices:**
- PRIMARY KEY: `id`
- INDEX: `idx_shareholder_company` (company_id)
- INDEX: `idx_shareholder_user` (user_id)
- UNIQUE INDEX: `idx_shareholder_document_company` (company_id, document)
- INDEX: `idx_shareholder_type` (shareholder_type)
- INDEX: `idx_shareholder_status` (status)
- INDEX: `idx_shareholder_deleted` (is_deleted)

**Foreign Keys:**
- `fk_shareholder_company`: company_id → companies(id) ON DELETE RESTRICT
- `fk_shareholder_user`: user_id → users(id) ON DELETE SET NULL

**Valores Padrões:**
- status: 'Active'
- is_deleted: 0
```

### 7.4 Template: Seção `share_classes`

```markdown
#### Z. share_classes

Classes de ações/cotas com direitos específicos.

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| id | CHAR(36) | NOT NULL | Identificador único |
| company_id | CHAR(36) | NOT NULL | FK para companies |
| name | VARCHAR(100) | NOT NULL | Nome da classe (ex: Ordinárias, Preferenciais A) |
| code | VARCHAR(20) | NOT NULL | Código (ex: ON, PNA, PNB) |
| has_voting_rights | TINYINT(1) | NOT NULL | Tem direito a voto |
| liquidation_preference | DECIMAL(5,2) | NOT NULL | Preferência de liquidação (ex: 1.0 = 1x) |
| dividend_preference | DECIMAL(5,2) | NULL | Preferência de dividendos |
| is_convertible | TINYINT(1) | NOT NULL | Pode ser convertida |
| conversion_ratio | DECIMAL(10,4) | NULL | Razão de conversão |
| rights | JSON | NULL | Direitos adicionais |
| status | VARCHAR(20) | NOT NULL | Active, Inactive |
| created_at | DATETIME(6) | NOT NULL | Data de criação |
| updated_at | DATETIME(6) | NOT NULL | Data de atualização |
| created_by | CHAR(36) | NULL | Criador |
| updated_by | CHAR(36) | NULL | Atualizador |

**Índices:**
- PRIMARY KEY: `id`
- INDEX: `idx_share_class_company` (company_id)
- UNIQUE INDEX: `idx_share_class_code` (company_id, code)

**Foreign Keys:**
- `fk_share_class_company`: company_id → companies(id) ON DELETE RESTRICT

**Valores Padrões:**
- has_voting_rights: 1
- liquidation_preference: 1.00
- is_convertible: 0
- status: 'Active'
```

### 7.5 Template: Seção `shares` e `share_transactions`

```markdown
#### W. shares

Participações acionárias dos sócios.

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| id | CHAR(36) | NOT NULL | Identificador único |
| company_id | CHAR(36) | NOT NULL | FK para companies |
| shareholder_id | CHAR(36) | NOT NULL | FK para shareholders |
| share_class_id | CHAR(36) | NOT NULL | FK para share_classes |
| quantity | DECIMAL(15,0) | NOT NULL | Quantidade de ações |
| acquisition_price | DECIMAL(15,4) | NOT NULL | Preço de aquisição por ação |
| acquisition_date | DATE | NOT NULL | Data de aquisição |
| origin | VARCHAR(20) | NOT NULL | Issue, Transfer, Conversion, Grant |
| origin_transaction_id | CHAR(36) | NULL | FK para share_transactions |
| contract_id | CHAR(36) | NULL | FK para contracts (futuro) |
| status | VARCHAR(20) | NOT NULL | Active, Cancelled, Converted |
| created_at | DATETIME(6) | NOT NULL | Data de criação |
| updated_at | DATETIME(6) | NOT NULL | Data de atualização |

**Índices:**
- PRIMARY KEY: `id`
- INDEX: `idx_share_company` (company_id)
- INDEX: `idx_share_shareholder` (shareholder_id)
- INDEX: `idx_share_class` (share_class_id)
- INDEX: `idx_share_status` (status)

---

#### V. share_transactions

Ledger imutável de transações de ações (append-only).

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| id | CHAR(36) | NOT NULL | Identificador único |
| company_id | CHAR(36) | NOT NULL | FK para companies |
| transaction_type | VARCHAR(20) | NOT NULL | Issue, Transfer, Cancel, Convert |
| share_id | CHAR(36) | NOT NULL | FK para shares |
| quantity | DECIMAL(15,0) | NOT NULL | Quantidade transacionada |
| price_per_share | DECIMAL(15,4) | NOT NULL | Preço por ação |
| total_value | DECIMAL(18,4) | NOT NULL | Valor total |
| from_shareholder_id | CHAR(36) | NULL | FK (origem - transferências) |
| to_shareholder_id | CHAR(36) | NULL | FK (destino - transferências) |
| reference_date | DATE | NOT NULL | Data de referência |
| notes | TEXT | NULL | Observações |
| approved_by | CHAR(36) | NULL | FK para users |
| approved_at | DATETIME(6) | NULL | Data de aprovação |
| created_at | DATETIME(6) | NOT NULL | Data de criação |
| created_by | CHAR(36) | NULL | Criador |

**Índices:**
- PRIMARY KEY: `id`
- INDEX: `idx_transaction_company` (company_id)
- INDEX: `idx_transaction_share` (share_id)
- INDEX: `idx_transaction_type` (transaction_type)
- INDEX: `idx_transaction_date` (reference_date)

**Características:**
- ⚠️ Tabela APPEND-ONLY (não permite UPDATE ou DELETE)
- Trigger para impedir modificações
```

### 7.6 Migrations a Documentar

```markdown
## 📝 Migrations Aplicadas

### Migration 003: Create Clients Table
**Data:** [DATA DA EXECUÇÃO]
**Arquivo:** `003_create_clients_table.sql`

**Objetivo:** Criar entidade raiz para multi-tenancy

**Alterações:**
- Criada tabela `clients`
- Adicionado `client_id` em `companies`
- Adicionado `client_id` em `users`
- Criada tabela `user_companies`
- Atualizada FK em `BillingClients`

---

### Migration 004: Create Shareholders Table
**Data:** [DATA DA EXECUÇÃO]
**Arquivo:** `004_create_shareholders_table.sql`

---

### Migration 005: Create Share Classes Table
**Data:** [DATA DA EXECUÇÃO]
**Arquivo:** `005_create_share_classes_table.sql`

---

### Migration 006: Create Shares and Transactions Tables
**Data:** [DATA DA EXECUÇÃO]
**Arquivo:** `006_create_shares_tables.sql`

---

### Migration 007: Create Cap Table View
**Data:** [DATA DA EXECUÇÃO]
**Arquivo:** `007_create_cap_table_view.sql`
```

### 7.7 DOCUMENTACAO_FUNCIONAL.md - Seções a Adicionar

```markdown
## Alterações a serem feitas no DOCUMENTACAO_FUNCIONAL.md

### 1. NOVA SEÇÃO: Módulo Multi-tenancy (Client)
- Conceito de Client vs Company
- Casos de uso suportados
- Fluxo de criação de Client
- Company Switcher (como usar)

### 2. NOVA SEÇÃO: Módulo Cap Table
- Visão geral do módulo
- Funcionalidades implementadas
- Fluxos de usuário

### 3. NOVA SEÇÃO: Gestão de Sócios (Shareholders)
- Tipos de sócios suportados
- CRUD de sócios
- Vinculação com usuário do sistema
- Filtros e buscas

### 4. NOVA SEÇÃO: Classes de Ações (Share Classes)
- O que são classes de ações
- Direitos configuráveis
- Como criar e gerenciar

### 5. NOVA SEÇÃO: Participações e Transações
- Emissão de ações
- Transferência entre sócios
- Cancelamento de ações
- Histórico (Ledger imutável)

### 6. NOVA SEÇÃO: Visualização do Cap Table
- Dashboard de Cap Table
- Gráficos e estatísticas
- Tabela de participações
- Exportação de dados

### 7. NOVA SEÇÃO: Simulador de Rodadas
- Como funciona o simulador
- Parâmetros de entrada
- Interpretação dos resultados
- Salvar cenários
```

### 7.8 Template: Documentação Funcional - Multi-tenancy

```markdown
## Módulo Multi-tenancy (Client)

### Visão Geral

O Partnership Manager utiliza uma arquitetura multi-tenant onde:

- **Client**: Representa o cliente do SaaS (quem contrata e paga pela plataforma)
- **Company**: Representa as empresas gerenciadas dentro do sistema

Um Client pode gerenciar múltiplas Companies, permitindo cenários como:
- Escritórios de advocacia gerenciando vários clientes
- Aceleradoras acompanhando seu portfólio de startups
- Holdings familiares com diversas empresas

### Hierarquia de Acesso

```
Client (Contratante)
├── Users (Usuários do Client)
│   └── Podem acessar 1 ou mais Companies
├── Company A
│   └── Shareholders, Cap Table, Contratos...
├── Company B
│   └── Shareholders, Cap Table, Contratos...
└── Company C
    └── Shareholders, Cap Table, Contratos...
```

### Funcionalidades

#### Gestão de Client
- Cadastro de dados do Client (razão social, CNPJ/CPF, contato)
- Configurações personalizadas
- Visualização de todas as Companies vinculadas

#### Company Switcher
- Localizado no header da aplicação
- Permite alternar rapidamente entre Companies
- Mostra apenas Companies que o usuário tem acesso
- Indica a Company atualmente selecionada

#### Permissões por Company
- Um usuário pode ter diferentes níveis de acesso em cada Company
- Roles disponíveis: Admin, Manager, Editor, Viewer
- Configurável pela tabela `user_companies`

### Fluxos de Usuário

#### Criar Novo Client
1. Administrador acessa "Configurações > Clients"
2. Clica em "Novo Client"
3. Preenche dados obrigatórios (nome, documento, email)
4. Sistema cria o Client e redireciona para criação da primeira Company

#### Alternar entre Companies
1. Usuário clica no Company Switcher (header)
2. Lista de Companies disponíveis é exibida
3. Usuário seleciona a Company desejada
4. Sistema atualiza o contexto e recarrega os dados
```

### 7.9 Template: Documentação Funcional - Cap Table

```markdown
## Módulo Cap Table

### Visão Geral

O módulo Cap Table permite a gestão completa da estrutura societária de uma empresa, incluindo:

- Cadastro e gestão de sócios (shareholders)
- Configuração de classes de ações
- Registro de participações acionárias
- Histórico completo de transações (ledger imutável)
- Visualização consolidada do cap table
- Simulador de rodadas de investimento

### Funcionalidades Principais

#### 1. Gestão de Sócios (Shareholders)

**Tipos de Sócios Suportados:**
| Tipo | Descrição | Cor no Sistema |
|------|-----------|----------------|
| Founder | Sócios fundadores | Azul |
| Investor | Investidores (Anjos, VCs, etc.) | Roxo |
| Employee | Funcionários com equity | Verde |
| Advisor | Conselheiros e advisors | Laranja |
| Other | Outros tipos | Cinza |

**Operações Disponíveis:**
- ✅ Criar novo sócio
- ✅ Editar informações do sócio
- ✅ Visualizar detalhes e histórico
- ✅ Vincular a usuário do sistema (opcional)
- ✅ Filtrar por tipo/status
- ✅ Buscar por nome/documento
- ✅ Desativar sócio (soft delete)

**Campos do Sócio:**
- Nome completo
- Documento (CPF/CNPJ)
- Tipo de sócio
- Email e telefone
- Endereço (opcional)
- Data de entrada
- Data de saída (se aplicável)
- Status (Ativo, Inativo, Pendente)

---

#### 2. Classes de Ações (Share Classes)

Permite configurar diferentes classes de ações/cotas com direitos específicos.

**Exemplos de Classes:**
| Classe | Código | Voto | Preferência |
|--------|--------|------|-------------|
| Ordinárias | ON | Sim | 1.0x |
| Preferenciais A | PNA | Não | 1.5x |
| Preferenciais B | PNB | Não | 2.0x |

**Direitos Configuráveis:**
- Direito a voto (sim/não)
- Preferência de liquidação (múltiplo)
- Preferência de dividendos
- Conversibilidade
- Direitos adicionais (JSON)

---

#### 3. Participações e Transações

**Tipos de Transação:**
| Tipo | Descrição |
|------|-----------|
| Issue | Emissão de novas ações |
| Transfer | Transferência entre sócios |
| Cancel | Cancelamento de ações |
| Convert | Conversão entre classes |

**Regras de Negócio:**
- ⚠️ Soma das participações ≤ 100%
- ⚠️ Total emitido ≤ Total autorizado
- ⚠️ Transferência requer saldo suficiente
- ⚠️ Ledger é imutável (append-only)

---

#### 4. Visualização do Cap Table

**Dashboard inclui:**
- **KPIs**: Total de ações, valor total, número de sócios
- **Gráfico de Pizza**: Distribuição por tipo de sócio
- **Tabela Detalhada**: 
  - Nome do sócio
  - Tipo
  - Quantidade de ações
  - % de participação
  - Valor estimado
  - Ações (editar, histórico)

**Filtros Disponíveis:**
- Por tipo de sócio
- Por classe de ação
- Por status
- Busca por nome

---

#### 5. Simulador de Rodadas

Permite simular o impacto de novas rodadas de investimento no cap table.

**Parâmetros de Entrada:**
- Pre-money valuation
- Valor do investimento
- Classe de ação para novos investidores

**Resultados:**
- Post-money valuation
- Novo preço por ação
- Tabela de diluição (antes/depois)
- % de cada sócio após rodada

**Funcionalidades:**
- ✅ Calcular diluição em tempo real
- ✅ Comparar cenários
- ✅ Salvar simulações (futuro)

### Fluxos de Usuário

#### Emitir Ações para Novo Sócio
1. Criar o sócio em "Sócios > Novo Sócio"
2. Acessar "Cap Table > Transações"
3. Clicar em "Nova Emissão"
4. Selecionar sócio, classe e quantidade
5. Definir preço e data
6. Confirmar transação
7. Cap table é atualizado automaticamente

#### Transferir Ações entre Sócios
1. Acessar "Cap Table > Transações"
2. Clicar em "Nova Transferência"
3. Selecionar sócio de origem
4. Selecionar sócio de destino
5. Definir quantidade e valor
6. Confirmar transação
7. Sistema valida saldo e registra no ledger

#### Simular Rodada de Investimento
1. Acessar "Cap Table > Simulador"
2. Informar pre-money valuation
3. Informar valor do investimento
4. Visualizar tabela de diluição
5. Ajustar parâmetros se necessário
6. (Opcional) Salvar cenário
```

### 7.10 Prompt Template: Atualizar DOCUMENTACAO_FUNCIONAL.md

```
Implemente a tarefa [F2-DOC-003] - Atualizar DOCUMENTACAO_FUNCIONAL.md

## CONTEXTO
- Arquivo: docs/DOCUMENTACAO_FUNCIONAL.md (ou localização existente)
- Objetivo: Documentar funcionalidades implementadas na Fase 2

## VERIFICAÇÃO
1. Localizar arquivo DOCUMENTACAO_FUNCIONAL.md no repositório
2. Verificar estrutura atual do documento
3. Identificar onde inserir novas seções

## ALTERAÇÕES NECESSÁRIAS

### 1. Adicionar no Índice:
- Módulo Multi-tenancy (Client)
- Módulo Cap Table
  - Gestão de Sócios
  - Classes de Ações
  - Participações e Transações
  - Visualização do Cap Table
  - Simulador de Rodadas

### 2. Criar Seções (usar templates 7.8 e 7.9):
- Seção Multi-tenancy com hierarquia Client → Company
- Seção Cap Table com todas as funcionalidades
- Fluxos de usuário passo a passo
- Tabelas de referência (tipos, status, etc.)

### 3. Atualizar Seções Existentes:
- Adicionar referências ao Company Switcher
- Atualizar fluxos que dependem de Company selecionada

## FORMATO
- Usar Markdown
- Incluir tabelas para referência rápida
- Incluir diagramas ASCII quando apropriado
- Manter consistência com estilo existente

## VALIDAÇÃO
- Markdown válido
- Links internos funcionando
- Screenshots/diagramas atualizados (se aplicável)
- Revisão por Product Owner
```

---

## 8. Comandos para GitHub Agent

### 8.0 ⚠️ CONTROLE DE PROGRESSO (OBRIGATÓRIO)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    REGRA DE OURO: MARCAR PROGRESSO                      │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ANTES de iniciar qualquer tarefa:                                      │
│  1. Verificar o arquivo FASE2_PROGRESSO.md                              │
│  2. Identificar a última tarefa concluída                               │
│  3. Continuar da próxima tarefa pendente                                │
│                                                                         │
│  APÓS concluir cada tarefa:                                             │
│  1. Atualizar FASE2_PROGRESSO.md marcando [x] na tarefa                 │
│  2. Adicionar data/hora de conclusão                                    │
│  3. Adicionar observações relevantes (se houver)                        │
│  4. Fazer commit do arquivo de progresso                                │
│                                                                         │
│  ISSO ELIMINA RETRABALHO E PERMITE PAUSAR/RETOMAR A QUALQUER MOMENTO   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 8.0.1 Arquivo de Controle: FASE2_PROGRESSO.md

**Criar este arquivo na raiz do projeto antes de iniciar a Fase 2:**

```markdown
# Fase 2 - CapTable: Controle de Progresso

**Início:** [DATA DE INÍCIO]  
**Última Atualização:** [DATA/HORA]  
**Status Geral:** 🟡 Em Andamento

---

## Legenda
- [ ] Pendente
- [🔄] Em andamento
- [x] Concluído
- [⏸️] Pausado
- [❌] Bloqueado (ver observações)

---

## SEMANA 1: Correção Arquitetural (Client → Company)

### Database
- [ ] **F2-ARC-DB-001** - Criar tabela `clients`
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-ARC-DB-002** - Alterar `companies`: add client_id
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-ARC-DB-003** - Alterar `users`: add client_id
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-ARC-DB-004** - Criar tabela `user_companies`
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-ARC-DB-005** - Seed data (Client demo)
  - Início: 
  - Fim: 
  - Observações: 

### Backend
- [ ] **F2-ARC-BE-001** - Entidade `Client.cs`
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-ARC-BE-002** - DTOs: ClientRequest/Response
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-ARC-BE-003** - Validator: ClientValidator
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-ARC-BE-004** - Repository: ClientRepository
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-ARC-BE-005** - Controller: ClientsController
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-ARC-BE-006** - Atualizar Company.cs (ClientId)
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-ARC-BE-007** - Atualizar User.cs (ClientId)
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-ARC-BE-008** - Middleware: ClientContextMiddleware
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-ARC-BE-009** - Atualizar CompanyContextMiddleware
  - Início: 
  - Fim: 
  - Observações: 

### Frontend
- [ ] **F2-ARC-FE-001** - Type: Client
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-ARC-FE-002** - Service: clientService.ts
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-ARC-FE-003** - Store: useClientStore
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-ARC-FE-004** - Hook: useClient
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-ARC-FE-005** - Componente: CompanySwitcher
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-ARC-FE-006** - Atualizar Header com CompanySwitcher
  - Início: 
  - Fim: 
  - Observações: 

### Checkpoint Semana 1
- [ ] Build backend sem erros
- [ ] Build frontend sem erros
- [ ] Testes via Swagger OK
- [ ] Company Switcher funcionando

---

## SEMANA 2: Shareholders (Backend)

### Database
- [ ] **F2-SHR-DB-001** - Criar tabela `shareholders`
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-DB-002** - Índices de performance
  - Início: 
  - Fim: 
  - Observações: 

### Backend
- [ ] **F2-SHR-BE-001** - Entidade Shareholder.cs
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-BE-002** - Enum ShareholderType
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-BE-003** - Enum ShareholderStatus
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-BE-004** - DTOs: ShareholderRequest/Response
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-BE-005** - DTO: ShareholderListResponse
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-BE-006** - Validator: ShareholderValidator
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-BE-007** - Interface: IShareholderRepository
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-BE-008** - Repository: GetAll
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-BE-009** - Repository: GetById
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-BE-010** - Repository: Create
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-BE-011** - Repository: Update
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-BE-012** - Repository: Delete (soft)
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-BE-013** - Repository: GetByDocument
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-BE-014** - Service: IShareholderService
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-BE-015** - Service: ShareholderService
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-BE-016** - Controller: GET list
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-BE-017** - Controller: GET by id
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-BE-018** - Controller: POST
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-BE-019** - Controller: PUT
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-BE-020** - Controller: DELETE
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-BE-021** - Registrar DI em Program.cs
  - Início: 
  - Fim: 
  - Observações: 

### Testes
- [ ] **F2-SHR-TST-001** - Testes via Swagger
  - Início: 
  - Fim: 
  - Observações: 

### Checkpoint Semana 2
- [ ] CRUD Shareholders completo via API
- [ ] Validações funcionando
- [ ] Soft delete OK

---

## SEMANA 3: Shareholders (Frontend) + Share Classes

### Frontend Shareholders
- [ ] **F2-SHR-FE-001** - Type: Shareholder
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-FE-002** - Service: shareholderService.ts
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-FE-003** - Hook: useShareholders
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-FE-004** - Hook: useShareholderMutations
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-FE-005** - Componente: ShareholderCard
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-FE-006** - Componente: ShareholderBadge
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-FE-007** - Componente: ShareholderFilters
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-FE-008** - Página: ShareholdersListPage
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-FE-009** - Modal: ShareholderFormModal
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHR-FE-010** - Página: ShareholderDetailPage
  - Início: 
  - Fim: 
  - Observações: 

### Share Classes (Backend)
- [ ] **F2-SHC-DB-001** - Criar tabela share_classes
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHC-BE-001** - Entidade ShareClass.cs
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHC-BE-002** - DTOs ShareClass
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHC-BE-003** - Validator ShareClassValidator
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHC-BE-004** - Repository ShareClassRepository
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHC-BE-005** - Controller ShareClassesController
  - Início: 
  - Fim: 
  - Observações: 

### Testes
- [ ] **F2-SHC-TST-001** - Testes de integração
  - Início: 
  - Fim: 
  - Observações: 

### Checkpoint Semana 3
- [ ] UI Shareholders completa
- [ ] CRUD Share Classes via API
- [ ] Navegação funcionando

---

## SEMANA 4: Shares + Ledger + Cap Table View

### Database
- [ ] **F2-SHA-DB-001** - Criar tabela shares
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHA-DB-002** - Criar tabela share_transactions
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHA-DB-003** - Trigger imutabilidade
  - Início: 
  - Fim: 
  - Observações: 

### Backend Shares
- [ ] **F2-SHA-BE-001** - Entidade Share.cs
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHA-BE-002** - Entidade ShareTransaction.cs
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHA-BE-003** - Enum TransactionType
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHA-BE-004** - DTOs Shares e Transactions
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHA-BE-005** - Repository ShareRepository
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHA-BE-006** - Repository ShareTransactionRepository
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHA-BE-007** - Service: Emissão
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHA-BE-008** - Service: Transferência
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHA-BE-009** - Service: Cancelamento
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SHA-BE-010** - Controller SharesController
  - Início: 
  - Fim: 
  - Observações: 

### Cap Table View
- [ ] **F2-CAP-DB-001** - View materializada mv_cap_table
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-CAP-BE-001** - Service CapTableService
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-CAP-BE-002** - Controller CapTableController
  - Início: 
  - Fim: 
  - Observações: 

### Checkpoint Semana 4
- [ ] Emissão de ações funcionando
- [ ] Transferência funcionando
- [ ] Ledger imutável (testar UPDATE/DELETE)
- [ ] View cap table retornando dados

---

## SEMANA 5: Frontend Cap Table + Simulador + Documentação

### Frontend Cap Table
- [ ] **F2-CAP-FE-001** - Types: Share, Transaction, CapTable
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-CAP-FE-002** - Service: shareService.ts
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-CAP-FE-003** - Service: capTableService.ts
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-CAP-FE-004** - Hook: useCapTable
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-CAP-FE-005** - Componente: CapTableStats
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-CAP-FE-006** - Componente: CapTableChart
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-CAP-FE-007** - Componente: CapTableTable
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-CAP-FE-008** - Página: CapTablePage
  - Início: 
  - Fim: 
  - Observações: 

### Simulador
- [ ] **F2-SIM-BE-001** - Service RoundSimulatorService
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SIM-BE-002** - DTOs Simulation
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SIM-BE-003** - Controller SimulatorController
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SIM-FE-001** - Modal: RoundSimulatorModal
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-SIM-FE-002** - Componente: SimulationResults
  - Início: 
  - Fim: 
  - Observações: 

### Documentação
- [ ] **F2-DOC-001** - Atualizar DATABASE_DOCUMENTATION.md
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-DOC-002** - Atualizar MER
  - Início: 
  - Fim: 
  - Observações: 
- [ ] **F2-DOC-003** - Atualizar DOCUMENTACAO_FUNCIONAL.md
  - Início: 
  - Fim: 
  - Observações: 

### Testes
- [ ] **F2-TST-001** - Testes E2E críticos
  - Início: 
  - Fim: 
  - Observações: 

### Checkpoint Final Fase 2
- [ ] Todos os builds passando
- [ ] Documentação atualizada
- [ ] Demo completa funcionando
- [ ] Code review aprovado

---

## Histórico de Sessões

| Data | Início | Fim | Tarefas Concluídas | Observações |
|------|--------|-----|-------------------|-------------|
| | | | | |
| | | | | |
| | | | | |

---

## Bloqueios e Impedimentos

| Data | Tarefa | Bloqueio | Status | Resolução |
|------|--------|----------|--------|-----------|
| | | | | |

---

## Observações Gerais

<!-- Adicione aqui notas importantes, decisões técnicas, etc. -->

```

### 8.0.2 Comandos de Controle de Progresso

**Ao INICIAR uma sessão de desenvolvimento:**
```
1. Abrir FASE2_PROGRESSO.md
2. Localizar última tarefa marcada com [x]
3. Identificar próxima tarefa pendente [ ]
4. Marcar tarefa atual como [🔄] Em andamento
5. Registrar data/hora de início
6. Commit: "chore: iniciando tarefa F2-XXX-XXX"
```

**Ao CONCLUIR uma tarefa:**
```
1. Marcar tarefa como [x] Concluído
2. Preencher data/hora de fim
3. Adicionar observações relevantes
4. Commit: "chore: concluída tarefa F2-XXX-XXX"
```

**Ao PAUSAR o trabalho:**
```
1. Marcar tarefa atual como [⏸️] Pausado
2. Registrar estado atual nas observações
3. Adicionar entrada no "Histórico de Sessões"
4. Commit: "chore: pausando desenvolvimento - última: F2-XXX-XXX"
```

**Ao RETOMAR o trabalho:**
```
1. Ler FASE2_PROGRESSO.md
2. Verificar "Histórico de Sessões" para contexto
3. Verificar "Bloqueios e Impedimentos"
4. Continuar da tarefa [⏸️] ou próxima [ ]
5. NÃO reanalisar tarefas já concluídas [x]
```

### 8.0.3 Prompt para Retomar Trabalho

```
Estou retomando o desenvolvimento da Fase 2 - CapTable.

## INSTRUÇÕES
1. Leia o arquivo FASE2_PROGRESSO.md
2. Identifique a última tarefa concluída [x]
3. Identifique a próxima tarefa pendente [ ] ou pausada [⏸️]
4. NÃO analise nem refaça tarefas já marcadas como [x]
5. Continue diretamente da próxima tarefa pendente
6. Ao concluir, atualize o arquivo de progresso

## CONTEXTO ADICIONAL
- Verificar "Histórico de Sessões" para entender onde paramos
- Verificar "Bloqueios e Impedimentos" para problemas conhecidos
- Ler "Observações" da tarefa pausada (se houver)

## OBJETIVO
Maximizar produtividade eliminando tempo de re-análise.
```

### 8.1 Contexto Obrigatório para Cada Tarefa

```markdown
Ao iniciar qualquer tarefa, o GitHub Agent DEVE:

1. VERIFICAR PREMISSAS (PREMISSAS_DESENVOLVIMENTO.md):
   - Ler o documento de premissas antes de iniciar
   - Seguir os padrões de código definidos
   - Usar a checklist pré-desenvolvimento

2. VERIFICAR EXISTÊNCIA:
   - Verificar se a entidade/componente já existe
   - Seguir padrões de arquivos similares
   - Não criar código duplicado

3. SEGUIR ESTRUTURA DE PASTAS:
   Backend:
   - Entities: src/backend/PartnershipManager.Domain/Entities/
   - DTOs: src/backend/PartnershipManager.Application/DTOs/
   - Validators: src/backend/PartnershipManager.Application/Validators/
   - Repositories: src/backend/PartnershipManager.Infrastructure/Repositories/
   - Controllers: src/backend/PartnershipManager.API/Controllers/
   
   Frontend:
   - Types: src/frontend/src/types/
   - Services: src/frontend/src/services/
   - Hooks: src/frontend/src/hooks/
   - Components: src/frontend/src/components/[module]/
   - Pages: src/frontend/src/pages/[module]/
```

### 8.2 Prompt Template: Criar Migration

```
Implemente a tarefa [F2-ARC-DB-001] - Criar tabela clients

## CONTEXTO
- Database: partnership_manager (MySQL 8.0)
- Charset: utf8mb4, Collation: utf8mb4_unicode_ci
- Padrão de IDs: CHAR(36) com GUID

## VERIFICAÇÃO
1. Verificar se tabela `clients` já existe
2. Consultar padrão das migrations em: docker/mysql/migrations/

## IMPLEMENTAÇÃO
Criar arquivo: docker/mysql/migrations/003_create_clients_table.sql

Conteúdo deve incluir:
- CREATE TABLE com todos os campos documentados
- Índices (PRIMARY, UNIQUE, INDEX)
- Valores DEFAULT
- Comentário com descrição da migration

## VALIDAÇÃO
- Sintaxe SQL válida para MySQL 8.0
- Nomes de índices únicos
- FKs com ON DELETE apropriado

## REFERÊNCIA
Consultar DATABASE_DOCUMENTATION.md seção 1.6
```

### 8.3 Prompt Template: Criar Entidade Backend

```
Implemente a tarefa [F2-ARC-BE-001] - Criar entidade Client.cs

## CONTEXTO
- Framework: .NET 9
- ORM: Dapper (não Entity Framework)
- Padrão: Domain/Entities/

## VERIFICAÇÃO
1. Confirmar que NÃO existe Client.cs em Domain/Entities
2. Verificar padrão de entidades existentes (Company.cs, User.cs)

## IMPLEMENTAÇÃO
Criar arquivo: src/backend/PartnershipManager.Domain/Entities/Client.cs

Requisitos:
- Herdar de BaseEntity (se existir) ou implementar campos de auditoria
- Propriedades conforme migration F2-ARC-DB-001
- Navegações: ICollection<Company>, ICollection<User>
- Sem lógica de negócio na entidade

## PADRÃO A SEGUIR
```csharp
namespace PartnershipManager.Domain.Entities;

public class Client
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // ... demais propriedades
    
    // Navegações
    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
```

## VALIDAÇÃO
- dotnet build sem erros
- Segue convenção de nomenclatura
```

### 8.4 Prompt Template: Criar Repository

```
Implemente a tarefa [F2-ARC-BE-004] - Criar ClientRepository

## CONTEXTO
- ORM: Dapper
- Padrão: Infrastructure/Repositories/
- Interface em: Domain/Interfaces/

## VERIFICAÇÃO
1. Verificar padrão de repositories existentes (CompanyRepository, UserRepository)
2. Confirmar que IClientRepository não existe

## IMPLEMENTAÇÃO

### Arquivo 1: Domain/Interfaces/IClientRepository.cs
```csharp
public interface IClientRepository
{
    Task<Client?> GetByIdAsync(Guid id);
    Task<Client?> GetByDocumentAsync(string document);
    Task<IEnumerable<Client>> GetAllAsync(ClientFilterRequest filter);
    Task<int> GetTotalCountAsync(ClientFilterRequest filter);
    Task<Guid> CreateAsync(Client client);
    Task<bool> UpdateAsync(Client client);
    Task<bool> SoftDeleteAsync(Guid id, Guid deletedBy);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> DocumentExistsAsync(string document, Guid? excludeId = null);
}
```

### Arquivo 2: Infrastructure/Repositories/ClientRepository.cs
- Injetar IDbConnection
- Usar Dapper para queries
- Implementar todos os métodos da interface
- Incluir paginação, filtros, ordenação

## VALIDAÇÃO
- Queries SQL válidas para MySQL
- Tratamento de soft delete (is_deleted = 0)
- Logs de erro apropriados
```

### 8.5 Prompt Template: Criar Componente Frontend

```
Implemente a tarefa [F2-SHR-FE-005] - Criar ShareholderCard

## CONTEXTO
- Framework: React 18 + TypeScript
- Estilização: Tailwind CSS
- Design System: Conforme paleta definida

## VERIFICAÇÃO
1. Verificar se componente existe em components/shareholders/
2. Consultar padrão de cards existentes (ex: CompanyCard)
3. Verificar Design System (cores, espaçamentos)

## IMPLEMENTAÇÃO
Criar arquivo: src/frontend/src/components/shareholders/ShareholderCard.tsx

Requisitos:
- TypeScript com interface Props
- Tailwind CSS (não styled-components)
- Responsivo (mobile-first)
- Acessibilidade (aria-labels)

## PADRÃO A SEGUIR
```tsx
interface ShareholderCardProps {
  shareholder: Shareholder;
  onEdit?: (id: string) => void;
  onDelete?: (id: string) => void;
  isLoading?: boolean;
}

export function ShareholderCard({ 
  shareholder, 
  onEdit, 
  onDelete,
  isLoading 
}: ShareholderCardProps) {
  // ...
}
```

## DESIGN
- Avatar com iniciais ou imagem
- Nome e tipo de sócio
- Badge colorido por tipo (Founder=blue, Investor=purple, Employee=green)
- % de participação (se disponível)
- Ações: Editar, Excluir (com confirmação)

## VALIDAÇÃO
- npm run lint sem warnings
- npm run build sem erros
- Responsivo em mobile/tablet/desktop
```

### 8.6 Prompt Template: Atualizar Documentação

```
Implemente a tarefa [F2-DOC-001] - Atualizar DATABASE_DOCUMENTATION.md

## CONTEXTO
- Arquivo: docs/DATABASE_DOCUMENTATION.md (ou raiz)
- Versão atual: 1.0.0
- Nova versão: 1.1.0

## ALTERAÇÕES NECESSÁRIAS

1. ATUALIZAR cabeçalho:
   - Versão: 1.1.0
   - Data: [DATA ATUAL]

2. ATUALIZAR Visão Geral:
   - Adicionar "Multi-tenancy Module" na lista
   - Adicionar "Cap Table Module" na lista

3. ADICIONAR novas seções (após Core Module):
   - Tabela clients (usar template 7.2)
   - Tabela user_companies (usar template)
   - Tabela shareholders (usar template 7.3)
   - Tabela share_classes (usar template 7.4)
   - Tabela shares (usar template 7.5)
   - Tabela share_transactions (usar template 7.5)

4. ATUALIZAR seções existentes:
   - companies: adicionar client_id
   - users: adicionar client_id, tornar company_id nullable
   - BillingClients: adicionar core_client_id

5. ADICIONAR migrations:
   - 003_create_clients_table.sql
   - 004_create_shareholders_table.sql
   - 005_create_share_classes_table.sql
   - 006_create_shares_tables.sql
   - 007_create_cap_table_view.sql

## VALIDAÇÃO
- Markdown válido
- Links internos funcionando
- Tabelas formatadas corretamente
```

---

## Anexo A: Scripts SQL Completos

### A.1 Migration 003: Create Clients Table

```sql
-- =============================================
-- Migration: 003_create_clients_table.sql
-- Description: Criar estrutura de multi-tenancy com entidade Client
-- Date: [DATA]
-- Author: Partnership Manager Team
-- =============================================

-- 1. Criar tabela clients
CREATE TABLE IF NOT EXISTS clients (
    id CHAR(36) NOT NULL,
    name VARCHAR(200) NOT NULL COMMENT 'Razão social',
    trading_name VARCHAR(200) NULL COMMENT 'Nome fantasia',
    document VARCHAR(20) NOT NULL COMMENT 'CNPJ ou CPF',
    document_type VARCHAR(10) NOT NULL COMMENT 'cnpj ou cpf',
    email VARCHAR(255) NOT NULL COMMENT 'Email principal',
    phone VARCHAR(20) NULL COMMENT 'Telefone',
    logo_url VARCHAR(500) NULL COMMENT 'URL do logotipo',
    settings JSON NULL COMMENT 'Configurações personalizadas',
    status VARCHAR(20) NOT NULL DEFAULT 'Active' COMMENT 'Active, Inactive, Suspended',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by CHAR(36) NULL,
    updated_by CHAR(36) NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    deleted_at DATETIME(6) NULL,
    PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Índices para clients
CREATE UNIQUE INDEX idx_client_document ON clients(document) WHERE is_deleted = 0;
CREATE INDEX idx_client_status ON clients(status);
CREATE INDEX idx_client_deleted ON clients(is_deleted);

-- 2. Criar Client de demonstração
INSERT INTO clients (id, name, trading_name, document, document_type, email, status, created_at, updated_at)
VALUES (
    'c1000000-0000-0000-0000-000000000001',
    'Cliente Demonstração LTDA',
    'Demo Client',
    '00000000000000',
    'cnpj',
    'contato@democlient.com',
    'Active',
    NOW(),
    NOW()
);

-- 3. Adicionar client_id em companies
ALTER TABLE companies 
    ADD COLUMN client_id CHAR(36) NULL AFTER id;

-- Vincular company existente ao client demo
UPDATE companies SET client_id = 'c1000000-0000-0000-0000-000000000001' WHERE client_id IS NULL;

-- Tornar client_id NOT NULL após popular
ALTER TABLE companies 
    MODIFY COLUMN client_id CHAR(36) NOT NULL;

-- Adicionar índice e FK
CREATE INDEX idx_company_client ON companies(client_id);
ALTER TABLE companies 
    ADD CONSTRAINT fk_company_client FOREIGN KEY (client_id) REFERENCES clients(id) ON DELETE RESTRICT;

-- 4. Adicionar client_id em users
ALTER TABLE users 
    ADD COLUMN client_id CHAR(36) NULL AFTER id;

-- Vincular users existentes via company
UPDATE users u 
    JOIN companies c ON u.company_id = c.id 
    SET u.client_id = c.client_id;

-- Tornar client_id NOT NULL
ALTER TABLE users 
    MODIFY COLUMN client_id CHAR(36) NOT NULL;

-- Tornar company_id NULLABLE (usuário pode acessar múltiplas companies)
ALTER TABLE users 
    MODIFY COLUMN company_id CHAR(36) NULL;

-- Adicionar índice e FK
CREATE INDEX idx_user_client ON users(client_id);
ALTER TABLE users 
    ADD CONSTRAINT fk_user_client FOREIGN KEY (client_id) REFERENCES clients(id) ON DELETE RESTRICT;

-- 5. Criar tabela user_companies (N:N)
CREATE TABLE IF NOT EXISTS user_companies (
    id CHAR(36) NOT NULL,
    user_id CHAR(36) NOT NULL,
    company_id CHAR(36) NOT NULL,
    role VARCHAR(50) NOT NULL DEFAULT 'Viewer' COMMENT 'Admin, Manager, Editor, Viewer',
    is_default TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Company padrão do usuário',
    granted_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    granted_by CHAR(36) NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    UNIQUE INDEX idx_user_company_unique (user_id, company_id),
    INDEX idx_user_company_user (user_id),
    INDEX idx_user_company_company (company_id),
    CONSTRAINT fk_uc_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_uc_company FOREIGN KEY (company_id) REFERENCES companies(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Migrar relacionamentos existentes para user_companies
INSERT INTO user_companies (id, user_id, company_id, role, is_default, granted_at, created_at)
SELECT 
    UUID(),
    u.id,
    u.company_id,
    COALESCE((SELECT ur.role FROM user_roles ur WHERE ur.user_id = u.id AND ur.is_active = 1 LIMIT 1), 'Viewer'),
    1,
    NOW(),
    NOW()
FROM users u 
WHERE u.company_id IS NOT NULL;

-- 6. Adicionar core_client_id em BillingClients
ALTER TABLE BillingClients 
    ADD COLUMN core_client_id CHAR(36) NULL;

CREATE INDEX idx_billing_core_client ON BillingClients(core_client_id);
ALTER TABLE BillingClients 
    ADD CONSTRAINT fk_billing_core_client FOREIGN KEY (core_client_id) REFERENCES clients(id) ON DELETE SET NULL;

-- =============================================
-- FIM DA MIGRATION 003
-- =============================================
```

### A.2 Migration 004: Create Shareholders Table

```sql
-- =============================================
-- Migration: 004_create_shareholders_table.sql
-- Description: Criar tabela de sócios/acionistas
-- Date: [DATA]
-- =============================================

CREATE TABLE IF NOT EXISTS shareholders (
    id CHAR(36) NOT NULL,
    company_id CHAR(36) NOT NULL,
    user_id CHAR(36) NULL COMMENT 'FK para users se for usuário do sistema',
    name VARCHAR(200) NOT NULL,
    document VARCHAR(20) NOT NULL COMMENT 'CPF ou CNPJ',
    document_type VARCHAR(10) NOT NULL COMMENT 'cpf ou cnpj',
    shareholder_type VARCHAR(20) NOT NULL COMMENT 'Founder, Investor, Employee, Advisor, Other',
    email VARCHAR(255) NULL,
    phone VARCHAR(20) NULL,
    address JSON NULL COMMENT 'Endereço completo em JSON',
    entry_date DATE NOT NULL COMMENT 'Data de entrada na sociedade',
    exit_date DATE NULL COMMENT 'Data de saída',
    status VARCHAR(20) NOT NULL DEFAULT 'Active' COMMENT 'Active, Inactive, Pending',
    notes TEXT NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by CHAR(36) NULL,
    updated_by CHAR(36) NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    deleted_at DATETIME(6) NULL,
    PRIMARY KEY (id),
    INDEX idx_shareholder_company (company_id),
    INDEX idx_shareholder_user (user_id),
    INDEX idx_shareholder_type (shareholder_type),
    INDEX idx_shareholder_status (status),
    INDEX idx_shareholder_deleted (is_deleted),
    CONSTRAINT fk_shareholder_company FOREIGN KEY (company_id) REFERENCES companies(id) ON DELETE RESTRICT,
    CONSTRAINT fk_shareholder_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Índice único para documento por empresa (considerando soft delete)
CREATE UNIQUE INDEX idx_shareholder_document_company 
    ON shareholders(company_id, document) 
    WHERE is_deleted = 0;
```

---

## Anexo B: Resumo Executivo

### Tarefas por Semana

| Semana | Foco | Tarefas | Horas |
|--------|------|---------|-------|
| 1 | Correção Arquitetural | 22 tarefas | 42h |
| 2 | Shareholders Backend | 24 tarefas | 38h |
| 3 | Shareholders Frontend + Share Classes | 17 tarefas | 40h |
| 4 | Shares + Ledger + Cap Table | 16 tarefas | 44h |
| 5 | Frontend Cap Table + Simulador + Docs | 17 tarefas | 53h |
| **TOTAL** | | **96 tarefas** | **217h** |

### Métricas de Sucesso

| Métrica | Meta | Como Medir |
|---------|------|-----------|
| Cobertura de Testes | ≥ 70% | dotnet test + coverage report |
| Performance Cap Table | < 200ms p95 | Swagger + métricas |
| Build Backend | 0 erros, 0 warnings | dotnet build |
| Build Frontend | 0 erros | npm run build |
| Lint Frontend | 0 warnings | npm run lint |
| DATABASE_DOCUMENTATION.md | 100% atualizado | Review manual |
| DOCUMENTACAO_FUNCIONAL.md | 100% atualizado | Review por PO |

### Riscos Identificados

| Risco | Probabilidade | Impacto | Mitigação |
|-------|--------------|---------|-----------|
| Migration falhar em dados existentes | Média | Alto | Backup antes, script de rollback |
| Performance do ledger imutável | Baixa | Médio | Índices apropriados, view materializada |
| Conflito com BillingClients existente | Média | Médio | FK opcional (SET NULL) |
| Complexidade do CompanySwitcher | Média | Médio | MVP simples primeiro |

---

**Documento gerado em:** 23 de Janeiro de 2025  
**Versão:** 1.1  
**Autor:** Tech Lead / Product Manager  
**Próxima revisão:** Após conclusão da Semana 1