# 📊 Documentação da Estrutura do Banco de Dados
## Partnership Manager - Database Schema

**Versão:** 2.0.0  
**Data de Atualização:** 26 de Janeiro de 2026  
**SGBD:** MySQL 8.0+  
**Character Set:** UTF8MB4  
**Collation:** utf8mb4_unicode_ci

---

## 📑 Índice

1. [Visão Geral](#visão-geral)
2. [Databases](#databases)
3. [Estrutura de Tabelas](#estrutura-de-tabelas)
4. [Relacionamentos](#relacionamentos)
5. [Índices e Performance](#índices-e-performance)
6. [Migrations Aplicadas](#migrations-aplicadas)
7. [Dados Iniciais](#dados-iniciais)
8. [Considerações de Segurança](#considerações-de-segurança)

---

## 🎯 Visão Geral

O sistema Partnership Manager utiliza uma arquitetura de banco de dados relacional MySQL dividida em dois databases principais:

- **partnership_manager**: Banco de dados principal contendo todos os módulos do sistema
- **hangfire**: Banco de dados para gerenciamento de jobs em background

### Módulos Implementados

1. **Core Module** - Clients, Empresas e Usuários
2. **Cap Table Module** - Sócios, Classes de Ações, Participações e Transações
3. **Billing Module** - Faturamento e Assinaturas
4. **Audit Module** - Logs de Auditoria

---

## 🗄️ Databases

### partnership_manager
- **Charset:** utf8mb4
- **Collation:** utf8mb4_unicode_ci
- **Propósito:** Armazenamento de dados da aplicação
- **Usuário:** pm_user

### hangfire
- **Charset:** utf8mb4
- **Collation:** utf8mb4_unicode_ci
- **Propósito:** Jobs em background (geração de faturas, notificações, etc.)
- **Usuário:** pm_user

---

## 📋 Estrutura de Tabelas

### 🏢 Core Module

#### 1. clients

Tabela raiz que representa os clientes SaaS do sistema (escritórios, aceleradoras, holdings, etc).

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| id | CHAR(36) | NOT NULL | Identificador único (GUID) |
| name | VARCHAR(200) | NOT NULL | Nome/Razão social do cliente |
| trading_name | VARCHAR(200) | NULL | Nome fantasia |
| document | VARCHAR(20) | NOT NULL | CPF ou CNPJ |
| document_type | VARCHAR(10) | NOT NULL | Tipo: 'cpf' ou 'cnpj' |
| email | VARCHAR(255) | NOT NULL | E-mail principal |
| phone | VARCHAR(20) | NULL | Telefone |
| logo_url | VARCHAR(500) | NULL | URL do logotipo |
| settings | JSON | NULL | Configurações personalizadas |
| status | VARCHAR(20) | NOT NULL | Status (Active, Inactive, Suspended) |
| created_at | DATETIME(6) | NOT NULL | Data de criação |
| updated_at | DATETIME(6) | NOT NULL | Data de última atualização |
| created_by | CHAR(36) | NULL | ID do usuário criador |
| updated_by | CHAR(36) | NULL | ID do último atualizador |
| is_deleted | TINYINT(1) | NOT NULL | Flag de soft delete |
| deleted_at | DATETIME(6) | NULL | Data de exclusão |

**Índices:**
- PRIMARY KEY: `id`
- UNIQUE INDEX: `idx_client_document` (document)
- INDEX: `idx_client_status` (status)
- INDEX: `idx_client_deleted` (is_deleted)

**Valores Padrões:**
- status: 'Active'
- is_deleted: 0

---

#### 2. companies

Tabela que armazena empresas gerenciadas pelos clientes SaaS.

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| id | CHAR(36) | NOT NULL | Identificador único (GUID) |
| client_id | CHAR(36) | NOT NULL | FK para clients (cliente SaaS) |
| name | VARCHAR(200) | NOT NULL | Razão social da empresa |
| trading_name | VARCHAR(200) | NULL | Nome fantasia |
| cnpj | VARCHAR(14) | NOT NULL | CNPJ (único) |
| legal_form | VARCHAR(20) | NOT NULL | Forma jurídica (LTDA, SA, etc.) |
| foundation_date | DATE | NOT NULL | Data de fundação |
| total_shares | DECIMAL(15,0) | NOT NULL | Total de ações/cotas |
| share_price | DECIMAL(15,4) | NOT NULL | Valor unitário da ação/cota |
| currency | VARCHAR(3) | NOT NULL | Moeda (padrão: BRL) |
| logo_url | VARCHAR(500) | NULL | URL do logotipo |
| settings | JSON | NULL | Configurações personalizadas |
| status | VARCHAR(20) | NOT NULL | Status (Active, Inactive, etc.) |
| address_* | VARCHAR | NULL | Campos de endereço (street, number, etc.) |
| created_at | DATETIME(6) | NOT NULL | Data de criação |
| updated_at | DATETIME(6) | NOT NULL | Data de última atualização |
| created_by | CHAR(36) | NULL | ID do usuário criador |
| updated_by | CHAR(36) | NULL | ID do último usuário que atualizou |
| is_deleted | TINYINT(1) | NOT NULL | Flag de soft delete (padrão: 0) |
| deleted_at | DATETIME(6) | NULL | Data de exclusão (soft delete) |

**Índices:**
- PRIMARY KEY: `id`
- UNIQUE INDEX: `idx_company_cnpj` (cnpj)
- INDEX: `idx_company_client` (client_id)
- INDEX: `idx_company_status` (status)
- INDEX: `idx_company_deleted` (is_deleted)

**Foreign Keys:**
- `fk_company_client`: client_id → clients(id) ON DELETE RESTRICT

**Valores Padrões:**
- currency: 'BRL'
- status: 'Active'
- is_deleted: 0

---

#### 3. users

Armazena informações dos usuários do sistema, vinculados a um cliente SaaS.

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| id | CHAR(36) | NOT NULL | Identificador único (GUID) |
| client_id | CHAR(36) | NOT NULL | FK para clients (cliente SaaS) |
| company_id | CHAR(36) | NULL | FK para companies (empresa padrão) |
| email | VARCHAR(255) | NOT NULL | E-mail do usuário |
| name | VARCHAR(200) | NOT NULL | Nome completo |
| password_hash | VARCHAR(255) | NOT NULL | Hash da senha (bcrypt) |
| avatar_url | VARCHAR(500) | NULL | URL do avatar |
| phone | VARCHAR(20) | NULL | Telefone |
| status | VARCHAR(20) | NOT NULL | Status (Pending, Active, Inactive, Locked) |
| language | VARCHAR(20) | NOT NULL | Idioma preferencial |
| timezone | VARCHAR(50) | NOT NULL | Fuso horário |
| preferences | JSON | NULL | Preferências do usuário |
| two_factor_enabled | TINYINT(1) | NOT NULL | 2FA habilitado |
| two_factor_secret | VARCHAR(100) | NULL | Chave secreta 2FA |
| last_login_at | DATETIME(6) | NULL | Último login |
| failed_login_attempts | INT | NOT NULL | Tentativas de login falhadas |
| lockout_end | DATETIME(6) | NULL | Fim do bloqueio de conta |
| refresh_token | VARCHAR(500) | NULL | Token de atualização JWT |
| refresh_token_expiry | DATETIME(6) | NULL | Expiração do refresh token |
| created_at | DATETIME(6) | NOT NULL | Data de criação |
| updated_at | DATETIME(6) | NOT NULL | Data de atualização |
| created_by | CHAR(36) | NULL | ID do criador |
| updated_by | CHAR(36) | NULL | ID do atualizador |
| is_deleted | TINYINT(1) | NOT NULL | Soft delete |
| deleted_at | DATETIME(6) | NULL | Data de exclusão |

**Índices:**
- PRIMARY KEY: `id`
- INDEX: `idx_user_client` (client_id)
- INDEX: `idx_user_company` (company_id)
- UNIQUE INDEX: `idx_user_email_client` (client_id, email)
- INDEX: `idx_user_status` (status)
- INDEX: `idx_user_deleted` (is_deleted)

**Foreign Keys:**
- `fk_user_client`: client_id → clients(id) ON DELETE RESTRICT
- `fk_user_company`: company_id → companies(id) ON DELETE SET NULL

**Valores Padrões:**
- status: 'Pending'
- language: 'Portuguese'
- timezone: 'America/Sao_Paulo'
- two_factor_enabled: 0
- failed_login_attempts: 0
- is_deleted: 0

---

#### 4. user_companies

Tabela de relacionamento N:N entre usuários e empresas (acesso multi-empresa).

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| id | CHAR(36) | NOT NULL | Identificador único |
| user_id | CHAR(36) | NOT NULL | FK para users |
| company_id | CHAR(36) | NOT NULL | FK para companies |
| role | VARCHAR(50) | NOT NULL | Função do usuário na empresa |
| is_default | TINYINT(1) | NOT NULL | Empresa padrão do usuário |
| granted_at | DATETIME(6) | NOT NULL | Data da concessão de acesso |
| granted_by | CHAR(36) | NULL | Usuário que concedeu |
| created_at | DATETIME(6) | NOT NULL | Data de criação |

**Índices:**
- PRIMARY KEY: `id`
- UNIQUE INDEX: `idx_user_company_unique` (user_id, company_id)
- INDEX: `idx_user_company_user` (user_id)
- INDEX: `idx_user_company_company` (company_id)

**Foreign Keys:**
- `fk_uc_user`: user_id → users(id) ON DELETE CASCADE
- `fk_uc_company`: company_id → companies(id) ON DELETE CASCADE

**Valores Padrões:**
- role: 'Viewer'
- is_default: 0

---

#### 5. user_roles

Gerencia funções e permissões dos usuários.

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| id | CHAR(36) | NOT NULL | Identificador único |
| user_id | CHAR(36) | NOT NULL | FK para users |
| role | VARCHAR(50) | NOT NULL | Nome da função (Admin, Manager, Viewer, etc.) |
| permissions | JSON | NULL | Permissões específicas |
| granted_by | CHAR(36) | NULL | Usuário que concedeu a função |
| granted_at | DATETIME(6) | NOT NULL | Data da concessão |
| expires_at | DATETIME(6) | NULL | Data de expiração da função |
| is_active | TINYINT(1) | NOT NULL | Função ativa |
| created_at | DATETIME(6) | NOT NULL | Data de criação |
| updated_at | DATETIME(6) | NOT NULL | Data de atualização |

**Índices:**
- PRIMARY KEY: `id`
- INDEX: `idx_role_user` (user_id)
- INDEX: `idx_role_name` (role)
- UNIQUE INDEX: `idx_role_user_active` (user_id, role, is_active)

**Foreign Keys:**
- `fk_role_user`: user_id → users(id) ON DELETE CASCADE

**Valores Padrões:**
- is_active: 1

---

#### 4. audit_logs

Tabela de auditoria (append-only) para rastreamento de ações no sistema.

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| id | CHAR(36) | NOT NULL | Identificador único |
| company_id | CHAR(36) | NULL | FK para companies |
| user_id | CHAR(36) | NULL | FK para users |
| action | VARCHAR(50) | NOT NULL | Tipo de ação (Create, Update, Delete, etc.) |
| entity_type | VARCHAR(100) | NOT NULL | Tipo de entidade afetada |
| entity_id | CHAR(36) | NOT NULL | ID da entidade afetada |
| old_values | JSON | NULL | Valores anteriores |
| new_values | JSON | NULL | Novos valores |
| ip_address | VARCHAR(45) | NULL | Endereço IP do usuário |
| user_agent | TEXT | NULL | User agent do navegador |
| created_at | DATETIME(6) | NOT NULL | Data de criação do log |

**Índices:**
- PRIMARY KEY: `id`
- INDEX: `idx_audit_company` (company_id)
- INDEX: `idx_audit_user` (user_id)
- INDEX: `idx_audit_entity` (entity_type, entity_id)
- INDEX: `idx_audit_created` (created_at)

**Características:**
- Tabela append-only (não permite UPDATE ou DELETE)
- Registra todas as ações importantes do sistema

---

### � Cap Table Module

#### 8. shareholders

Sócios/acionistas das empresas gerenciadas.

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| id | CHAR(36) | NOT NULL | Identificador único |
| client_id | CHAR(36) | NOT NULL | FK para clients |
| company_id | CHAR(36) | NOT NULL | FK para companies |
| name | VARCHAR(200) | NOT NULL | Nome do sócio/acionista |
| document | VARCHAR(20) | NOT NULL | CPF ou CNPJ |
| document_type | VARCHAR(10) | NOT NULL | Tipo: 'cpf' ou 'cnpj' |
| type | VARCHAR(20) | NOT NULL | Tipo: Individual, Company, InvestmentFund |
| status | VARCHAR(20) | NOT NULL | Status: Active, Inactive, Pending |
| email | VARCHAR(255) | NULL | E-mail de contato |
| phone | VARCHAR(20) | NULL | Telefone |
| address_* | VARCHAR | NULL | Campos de endereço |
| gender | VARCHAR(20) | NULL | Gênero (para PF) |
| marital_status | VARCHAR(20) | NULL | Estado civil (para PF) |
| birth_date | DATE | NULL | Data de nascimento (para PF) |
| notes | TEXT | NULL | Observações |
| created_at | DATETIME(6) | NOT NULL | Data de criação |
| updated_at | DATETIME(6) | NOT NULL | Data de atualização |
| created_by | CHAR(36) | NULL | ID do criador |
| updated_by | CHAR(36) | NULL | ID do atualizador |
| is_deleted | TINYINT(1) | NOT NULL | Soft delete |
| deleted_at | DATETIME(6) | NULL | Data de exclusão |

**Índices:**
- PRIMARY KEY: `id`
- INDEX: `idx_shareholder_client` (client_id)
- INDEX: `idx_shareholder_company` (company_id)
- UNIQUE INDEX: `idx_shareholder_company_document` (company_id, document)
- INDEX: `idx_shareholder_type` (type)
- INDEX: `idx_shareholder_status` (status)
- INDEX: `idx_shareholder_deleted` (is_deleted)

**Foreign Keys:**
- `fk_shareholder_client`: client_id → clients(id) ON DELETE RESTRICT
- `fk_shareholder_company`: company_id → companies(id) ON DELETE RESTRICT

**Constraints:**
- CHK_shareholder_type: type IN ('Individual', 'Company', 'InvestmentFund')
- CHK_shareholder_status: status IN ('Active', 'Inactive', 'Pending')
- CHK_shareholder_document_type: document_type IN ('cpf', 'cnpj')

---

#### 9. share_classes

Classes de ações das empresas (Ordinárias, Preferenciais, etc).

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| id | CHAR(36) | NOT NULL | Identificador único |
| client_id | CHAR(36) | NOT NULL | FK para clients |
| company_id | CHAR(36) | NOT NULL | FK para companies |
| name | VARCHAR(100) | NOT NULL | Nome da classe (ex: Ordinárias) |
| code | VARCHAR(20) | NOT NULL | Código (ex: ON, PN) |
| description | TEXT | NULL | Descrição detalhada |
| seniority | INT | NOT NULL | Ordem de prioridade |
| authorized_shares | DECIMAL(18,4) | NOT NULL | Ações autorizadas |
| issued_shares | DECIMAL(18,4) | NOT NULL | Ações emitidas |
| par_value | DECIMAL(18,4) | NULL | Valor nominal |
| price_per_share | DECIMAL(18,4) | NULL | Preço por ação |
| has_voting_rights | TINYINT(1) | NOT NULL | Direito a voto |
| votes_per_share | DECIMAL(10,4) | NOT NULL | Votos por ação |
| has_dividend_preference | TINYINT(1) | NOT NULL | Preferência em dividendos |
| dividend_rate | DECIMAL(10,4) | NULL | Taxa de dividendo |
| liquidation_preference_multiple | DECIMAL(10,4) | NULL | Múltiplo de liquidação |
| is_participating | TINYINT(1) | NOT NULL | Participante (full ratchet) |
| is_convertible | TINYINT(1) | NOT NULL | Conversível |
| conversion_ratio | DECIMAL(10,4) | NULL | Taxa de conversão |
| convert_to_class_id | CHAR(36) | NULL | FK para classe destino |
| anti_dilution_protection | VARCHAR(50) | NULL | Proteção anti-diluição |
| status | VARCHAR(20) | NOT NULL | Status: Active, Inactive |
| created_at | DATETIME(6) | NOT NULL | Data de criação |
| updated_at | DATETIME(6) | NOT NULL | Data de atualização |
| created_by | CHAR(36) | NULL | ID do criador |
| updated_by | CHAR(36) | NULL | ID do atualizador |
| is_deleted | TINYINT(1) | NOT NULL | Soft delete |
| deleted_at | DATETIME(6) | NULL | Data de exclusão |

**Índices:**
- PRIMARY KEY: `id`
- INDEX: `idx_share_class_client` (client_id)
- INDEX: `idx_share_class_company` (company_id)
- UNIQUE INDEX: `idx_share_class_company_code` (company_id, code)
- INDEX: `idx_share_class_status` (status)
- INDEX: `idx_share_class_deleted` (is_deleted)

**Foreign Keys:**
- `fk_class_client`: client_id → clients(id) ON DELETE RESTRICT
- `fk_class_company`: company_id → companies(id) ON DELETE RESTRICT
- `fk_class_convert_to`: convert_to_class_id → share_classes(id) ON DELETE SET NULL

**Valores Padrões:**
- has_voting_rights: 1
- votes_per_share: 1.0
- is_participating: 0
- is_convertible: 0
- status: 'Active'

---

#### 10. shares

Participações acionárias (ações detidas por sócios).

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| id | CHAR(36) | NOT NULL | Identificador único |
| client_id | CHAR(36) | NOT NULL | FK para clients |
| company_id | CHAR(36) | NOT NULL | FK para companies |
| shareholder_id | CHAR(36) | NOT NULL | FK para shareholders |
| share_class_id | CHAR(36) | NOT NULL | FK para share_classes |
| certificate_number | VARCHAR(50) | NULL | Número do certificado |
| quantity | DECIMAL(18,4) | NOT NULL | Quantidade de ações |
| acquisition_price | DECIMAL(18,4) | NOT NULL | Preço de aquisição |
| total_cost | DECIMAL(18,4) | GENERATED | Custo total (quantity * price) |
| acquisition_date | DATE | NOT NULL | Data de aquisição |
| origin | VARCHAR(20) | NOT NULL | Origem: Issue, Transfer, Conversion, Grant |
| origin_transaction_id | CHAR(36) | NULL | FK para share_transactions |
| status | VARCHAR(20) | NOT NULL | Status: Active, Cancelled, Converted, Transferred |
| notes | TEXT | NULL | Observações |
| created_at | DATETIME(6) | NOT NULL | Data de criação |
| updated_at | DATETIME(6) | NOT NULL | Data de atualização |
| created_by | CHAR(36) | NULL | ID do criador |
| updated_by | CHAR(36) | NULL | ID do atualizador |
| is_deleted | TINYINT(1) | NOT NULL | Soft delete |
| deleted_at | DATETIME(6) | NULL | Data de exclusão |

**Índices:**
- PRIMARY KEY: `id`
- INDEX: `idx_share_client` (client_id)
- INDEX: `idx_share_company` (company_id)
- INDEX: `idx_share_shareholder` (shareholder_id)
- INDEX: `idx_share_class` (share_class_id)
- INDEX: `idx_share_status` (status)
- INDEX: `idx_share_origin` (origin)
- INDEX: `idx_share_acquisition_date` (acquisition_date)

**Foreign Keys:**
- `fk_share_client`: client_id → clients(id) ON DELETE RESTRICT
- `fk_share_company`: company_id → companies(id) ON DELETE RESTRICT
- `fk_share_shareholder`: shareholder_id → shareholders(id) ON DELETE RESTRICT
- `fk_share_class`: share_class_id → share_classes(id) ON DELETE RESTRICT

**Constraints:**
- CHK_share_quantity: quantity > 0
- CHK_share_price: acquisition_price >= 0
- CHK_share_origin: origin IN ('Issue', 'Transfer', 'Conversion', 'Grant')
- CHK_share_status: status IN ('Active', 'Cancelled', 'Converted', 'Transferred')

---

#### 11. share_transactions

Ledger imutável de todas as transações de ações.

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| id | CHAR(36) | NOT NULL | Identificador único |
| client_id | CHAR(36) | NOT NULL | FK para clients |
| company_id | CHAR(36) | NOT NULL | FK para companies |
| transaction_type | VARCHAR(20) | NOT NULL | Tipo: Issue, Transfer, Cancel, Convert, Split |
| transaction_number | VARCHAR(50) | NULL | Número sequencial |
| reference_date | DATE | NOT NULL | Data legal da transação |
| share_id | CHAR(36) | NULL | FK para shares |
| share_class_id | CHAR(36) | NOT NULL | FK para share_classes |
| quantity | DECIMAL(18,4) | NOT NULL | Quantidade transacionada |
| price_per_share | DECIMAL(18,4) | NOT NULL | Preço por ação |
| total_value | DECIMAL(18,4) | GENERATED | Valor total |
| from_shareholder_id | CHAR(36) | NULL | FK para shareholders (origem) |
| to_shareholder_id | CHAR(36) | NULL | FK para shareholders (destino) |
| reason | VARCHAR(200) | NULL | Motivo da transação |
| document_reference | VARCHAR(200) | NULL | Referência documental |
| notes | TEXT | NULL | Observações |
| approved_by | CHAR(36) | NULL | Aprovador |
| approved_at | DATETIME(6) | NULL | Data de aprovação |
| created_at | DATETIME(6) | NOT NULL | Data de criação |
| created_by | CHAR(36) | NULL | ID do criador |

**Índices:**
- PRIMARY KEY: `id`
- INDEX: `idx_transaction_client` (client_id)
- INDEX: `idx_transaction_company` (company_id)
- INDEX: `idx_transaction_type` (transaction_type)
- INDEX: `idx_transaction_date` (reference_date)
- INDEX: `idx_transaction_share` (share_id)
- INDEX: `idx_transaction_class` (share_class_id)
- INDEX: `idx_transaction_from` (from_shareholder_id)
- INDEX: `idx_transaction_to` (to_shareholder_id)
- INDEX: `idx_transaction_created` (created_at)

**Foreign Keys:**
- `fk_transaction_client`: client_id → clients(id) ON DELETE RESTRICT
- `fk_transaction_company`: company_id → companies(id) ON DELETE RESTRICT
- `fk_transaction_share_class`: share_class_id → share_classes(id) ON DELETE RESTRICT
- `fk_transaction_from_shareholder`: from_shareholder_id → shareholders(id) ON DELETE RESTRICT
- `fk_transaction_to_shareholder`: to_shareholder_id → shareholders(id) ON DELETE RESTRICT

**Constraints:**
- CHK_transaction_type: transaction_type IN ('Issue', 'Transfer', 'Cancel', 'Convert', 'Split', 'Reverse_Split')
- CHK_transaction_quantity: quantity > 0
- CHK_transaction_price: price_per_share >= 0

**Triggers:**
- `trg_share_transactions_no_update`: Impede UPDATE (imutabilidade)
- `trg_share_transactions_no_delete`: Impede DELETE (imutabilidade)

**Características:**
- Tabela append-only (não permite UPDATE ou DELETE)
- Ledger completo de todas as operações societárias

---

### 💰 Billing Module

#### 12. BillingClients

Clientes para faturamento (podem ser distintos dos usuários do sistema).

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| Id | CHAR(36) | NOT NULL | Identificador único |
| Name | VARCHAR(200) | NOT NULL | Nome do cliente |
| Email | VARCHAR(255) | NOT NULL | E-mail |
| Document | VARCHAR(20) | NOT NULL | CPF/CNPJ |
| Type | TINYINT | NOT NULL | Tipo: 1=Individual, 2=Company |
| Status | TINYINT | NOT NULL | 1=Active, 2=Suspended, 3=Cancelled |
| Phone | VARCHAR(20) | NULL | Telefone |
| Address | VARCHAR(500) | NULL | Endereço completo |
| City | VARCHAR(100) | NULL | Cidade |
| State | VARCHAR(2) | NULL | UF |
| ZipCode | VARCHAR(10) | NULL | CEP |
| Country | VARCHAR(100) | NULL | País (padrão: Brasil) |
| CreatedAt | DATETIME(6) | NOT NULL | Data de criação |
| CreatedBy | VARCHAR(100) | NULL | Criador |
| UpdatedAt | DATETIME(6) | NOT NULL | Data de atualização |
| UpdatedBy | VARCHAR(100) | NULL | Atualizador |
| DeletedAt | DATETIME(6) | NULL | Data de exclusão (soft delete) |

**Índices:**
- PRIMARY KEY: `Id`
- UNIQUE INDEX: `idx_billing_client_email` (Email, DeletedAt)
- UNIQUE INDEX: `idx_billing_client_document` (Document, DeletedAt)
- INDEX: `idx_billing_client_status` (Status)
- INDEX: `idx_billing_client_type` (Type)

**Valores Padrões:**
- Status: 1 (Active)
- Country: 'Brasil'

---

#### 6. BillingPlans

Planos de assinatura disponíveis.

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| Id | CHAR(36) | NOT NULL | Identificador único |
| Name | VARCHAR(100) | NOT NULL | Nome do plano |
| Description | TEXT | NULL | Descrição detalhada |
| Price | DECIMAL(10,2) | NOT NULL | Preço do plano |
| BillingCycle | TINYINT | NOT NULL | 1=Monthly, 2=Yearly |
| Features | JSON | NULL | Lista de funcionalidades |
| MaxCompanies | INT | NOT NULL | Limite de empresas |
| MaxUsers | INT | NOT NULL | Limite de usuários |
| IsActive | TINYINT(1) | NOT NULL | Plano ativo |
| CreatedAt | DATETIME(6) | NOT NULL | Data de criação |
| CreatedBy | VARCHAR(100) | NULL | Criador |
| UpdatedAt | DATETIME(6) | NOT NULL | Data de atualização |
| UpdatedBy | VARCHAR(100) | NULL | Atualizador |
| DeletedAt | DATETIME(6) | NULL | Data de exclusão |

**Índices:**
- PRIMARY KEY: `Id`
- INDEX: `idx_billing_plan_active` (IsActive)
- INDEX: `idx_billing_plan_cycle` (BillingCycle)

**Valores Padrões:**
- MaxCompanies: 1
- MaxUsers: 5
- IsActive: 1

---

#### 7. BillingSubscriptions

Assinaturas dos clientes aos planos.

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| Id | CHAR(36) | NOT NULL | Identificador único |
| ClientId | CHAR(36) | NOT NULL | FK para BillingClients |
| PlanId | CHAR(36) | NOT NULL | FK para BillingPlans |
| Status | TINYINT | NOT NULL | 1=Active, 2=Suspended, 3=Cancelled, 4=Pending |
| StartDate | DATE | NOT NULL | Data de início |
| EndDate | DATE | NULL | Data de término |
| AutoRenew | TINYINT(1) | NOT NULL | Renovação automática |
| CompaniesCount | INT | NOT NULL | Quantidade de empresas utilizadas |
| UsersCount | INT | NOT NULL | Quantidade de usuários utilizados |
| DueDay | INT | NOT NULL | Dia do vencimento (1-31) |
| PaymentMethod | INT | NOT NULL | Método preferencial: 1=BankTransfer, 2=CreditCard, 3=Pix, 4=Boleto, 5=Cash, 99=Other |
| CreatedAt | DATETIME(6) | NOT NULL | Data de criação |
| CreatedBy | VARCHAR(100) | NULL | Criador |
| UpdatedAt | DATETIME(6) | NOT NULL | Data de atualização |
| UpdatedBy | VARCHAR(100) | NULL | Atualizador |
| DeletedAt | DATETIME(6) | NULL | Data de exclusão |

**Índices:**
- PRIMARY KEY: `Id`
- INDEX: `idx_billing_subscription_client` (ClientId)
- INDEX: `idx_billing_subscription_plan` (PlanId)
- INDEX: `idx_billing_subscription_status` (Status)
- INDEX: `IX_BillingSubscriptions_PaymentMethod` (PaymentMethod)

**Foreign Keys:**
- ClientId → BillingClients(Id)
- PlanId → BillingPlans(Id)

**Constraints:**
- CHK_Subscription_DueDay: DueDay BETWEEN 1 AND 31

**Valores Padrões:**
- Status: 1 (Active)
- AutoRenew: 1
- CompaniesCount: 0
- UsersCount: 0
- DueDay: 10
- PaymentMethod: 3 (Pix)

---

#### 8. BillingInvoices

Faturas geradas para as assinaturas.

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| Id | CHAR(36) | NOT NULL | Identificador único |
| ClientId | CHAR(36) | NOT NULL | FK para BillingClients |
| SubscriptionId | CHAR(36) | NULL | FK para BillingSubscriptions |
| InvoiceNumber | VARCHAR(50) | NOT NULL | Número da fatura (único) |
| Amount | DECIMAL(10,2) | NOT NULL | Valor total |
| IssueDate | DATE | NOT NULL | Data de emissão |
| DueDate | DATE | NOT NULL | Data de vencimento |
| Status | TINYINT | NOT NULL | 1=Paid, 2=Pending, 3=Overdue, 4=Cancelled |
| PaymentDate | DATETIME(6) | NULL | Data do pagamento |
| Description | TEXT | NULL | Descrição |
| Notes | TEXT | NULL | Observações |
| CreatedAt | DATETIME(6) | NOT NULL | Data de criação |
| CreatedBy | VARCHAR(100) | NULL | Criador |
| UpdatedAt | DATETIME(6) | NOT NULL | Data de atualização |
| UpdatedBy | VARCHAR(100) | NULL | Atualizador |
| DeletedAt | DATETIME(6) | NULL | Data de exclusão |

**Índices:**
- PRIMARY KEY: `Id`
- UNIQUE INDEX: `idx_billing_invoice_number` (InvoiceNumber)
- INDEX: `idx_subscription_issue_month` (SubscriptionId, IssueDate)
- INDEX: `idx_subscription_issue_status` (SubscriptionId, IssueDate, Status)
- INDEX: `idx_billing_invoice_client` (ClientId)
- INDEX: `idx_billing_invoice_status` (Status)
- INDEX: `idx_billing_invoice_duedate` (DueDate)

**Foreign Keys:**
- ClientId → BillingClients(Id)
- SubscriptionId → BillingSubscriptions(Id)

**Valores Padrões:**
- Status: 2 (Pending)

**Observações:**
- Múltiplas faturas podem existir para a mesma subscription/mês (permite regeneração após cancelamento)
- O índice `idx_subscription_issue_month` é não-único para permitir esta flexibilidade

---

#### 9. BillingPayments

Registros de pagamentos realizados.

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| Id | CHAR(36) | NOT NULL | Identificador único |
| InvoiceId | CHAR(36) | NOT NULL | FK para BillingInvoices |
| Amount | DECIMAL(10,2) | NOT NULL | Valor pago |
| PaymentDate | DATETIME(6) | NOT NULL | Data do pagamento |
| PaymentMethod | TINYINT | NOT NULL | 1=BankTransfer, 2=CreditCard, 3=PIX, 4=Boleto |
| TransactionReference | VARCHAR(200) | NULL | Referência da transação |
| Notes | TEXT | NULL | Observações |
| CreatedAt | DATETIME(6) | NOT NULL | Data de criação |
| CreatedBy | VARCHAR(100) | NULL | Criador |
| UpdatedAt | DATETIME(6) | NOT NULL | Data de atualização |
| UpdatedBy | VARCHAR(100) | NULL | Atualizador |
| DeletedAt | DATETIME(6) | NULL | Data de exclusão |

**Índices:**
- PRIMARY KEY: `Id`
- INDEX: `idx_billing_payment_invoice` (InvoiceId)
- INDEX: `idx_billing_payment_date` (PaymentDate)

**Foreign Keys:**
- InvoiceId → BillingInvoices(Id)

---

## 🔗 Relacionamentos

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         HIERARQUIA PRINCIPAL                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  clients (1) ──────────── (N) companies                                 │
│      │                          │                                       │
│      ├── (1) ── (N) users       ├── (N) shareholders                   │
│      │            │             ├── (N) share_classes                   │
│      │            └── (N:N) ────┘ user_companies                        │
│      │                          │                                       │
│      └── (1) ── (N) BillingClients                                     │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                         CAP TABLE MODULE                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  shareholders (1) ──────────── (N) shares                              │
│        │                           │                                    │
│        └── (N) ── share_transactions ── (N)                            │
│                           │                                             │
│  share_classes (1) ───────┴─── (N) shares                              │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                         BILLING MODULE                                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  BillingClients (1) ──────────── (N) BillingSubscriptions              │
│                 │                        │                              │
│                 └── (1) ── (N) BillingInvoices ── (N) BillingPayments  │
│                                                                         │
│  BillingPlans (1) ──────────── (N) BillingSubscriptions                │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                         AUDIT & ROLES                                  │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  users (1) ──────────── (N) user_roles                                 │
│      │                                                                  │
│      └── (1) ──────────── (N) audit_logs                               │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### Regras de Integridade Referencial

#### Core Module
1. **clients → companies**: ON DELETE RESTRICT
   - Não permite exclusão de cliente com empresas associadas
   
2. **clients → users**: ON DELETE RESTRICT
   - Não permite exclusão de cliente com usuários associados

3. **companies → users**: ON DELETE SET NULL
   - Ao excluir empresa, company_id do usuário se torna NULL

4. **users → user_roles**: ON DELETE CASCADE
   - Remove todas as funções ao excluir um usuário

5. **user_companies**: ON DELETE CASCADE (ambas FKs)
   - Remove associações ao excluir usuário ou empresa

#### Cap Table Module
6. **companies → shareholders**: ON DELETE RESTRICT
   - Não permite exclusão de empresa com sócios

7. **companies → share_classes**: ON DELETE RESTRICT
   - Não permite exclusão de empresa com classes de ações

8. **shareholders → shares**: ON DELETE RESTRICT
   - Não permite exclusão de sócio com participações

9. **share_classes → shares**: ON DELETE RESTRICT
   - Não permite exclusão de classe com ações emitidas

10. **share_transactions**: ON DELETE RESTRICT (todas FKs)
    - Transações são imutáveis

#### Billing Module
11. **clients → BillingClients**: ON DELETE SET NULL (via core_client_id)
   
12. **BillingClients → BillingSubscriptions**: ON DELETE RESTRICT
   
13. **BillingPlans → BillingSubscriptions**: ON DELETE RESTRICT
   
14. **BillingSubscriptions → BillingInvoices**: ON DELETE RESTRICT
   
15. **BillingInvoices → BillingPayments**: ON DELETE RESTRICT

---

## ⚡ Índices e Performance

### Índices Únicos (Garantem Integridade)

1. **clients.idx_client_document** - Documento único por cliente SaaS
2. **companies.idx_company_cnpj** - CNPJ único por empresa
3. **users.idx_user_email_client** - E-mail único por cliente
4. **user_companies.idx_user_company_unique** - Acesso único user/company
5. **user_roles.idx_role_user_active** - Uma função ativa por usuário
6. **shareholders.idx_shareholder_company_document** - Documento único por empresa
7. **share_classes.idx_share_class_company_code** - Código único por empresa
8. **BillingClients.idx_billing_client_email** - E-mail único (com DeletedAt)
9. **BillingClients.idx_billing_client_document** - CPF/CNPJ único (com DeletedAt)
10. **BillingInvoices.idx_billing_invoice_number** - Número único de fatura

### Índices Compostos (Otimização de Queries)

1. **BillingInvoices.idx_subscription_issue_month** (SubscriptionId, IssueDate)
   - Otimiza busca de faturas por período
   
2. **BillingInvoices.idx_subscription_issue_status** (SubscriptionId, IssueDate, Status)
   - Busca de faturas ativas/pendentes por assinatura e período
   
3. **audit_logs.idx_audit_entity** (entity_type, entity_id)
   - Rastreamento de auditoria por entidade

4. **shares**: Índices para client_id, company_id, shareholder_id, share_class_id
   - Otimiza consultas de Cap Table

### Recomendações de Otimização

Para workloads de alto volume, considere:
- Particionamento de `audit_logs` por data
- Índices covering para queries frequentes
- Cache de planos ativos em Redis
- Arquivamento de faturas antigas

---

## 📝 Migrations Aplicadas

### Migration 001: Remove Invoice Duplicate Constraint
**Data:** 22/01/2026  
**Arquivo:** `001_remove_invoice_duplicate_constraint.sql`

**Objetivo:** Permitir regeneração de faturas após cancelamento

**Alterações:**
- Removido índice único `idx_subscription_issue_month`
- Criado índice não-único com mesmo nome para performance
- Adicionado índice `idx_subscription_issue_status` para queries filtradas por status

**Impacto:**
- ✅ Permite múltiplas faturas para mesma assinatura/mês
- ✅ Mantém performance de queries
- ⚠️ Validação de duplicação agora é feita em nível de aplicação

---

### Migration 002: Add Subscription Payment Fields
**Data:** 22/01/2026  
**Arquivo:** `002_add_subscription_payment_fields.sql`

**Objetivo:** Adicionar controle de vencimento e método de pagamento preferencial

**Alterações:**
- Adicionado DueDay INT NOT NULL DEFAULT 10
- Adicionado PaymentMethod INT NOT NULL DEFAULT 3
- Constraint CHK_Subscription_DueDay para validar dia (1-31)
- Índice IX_BillingSubscriptions_PaymentMethod

---

### Migration 003: Create Clients Table
**Data:** 23/01/2026  
**Arquivo:** `003_create_clients_table.sql`

**Objetivo:** Criar entidade raiz de clientes SaaS

**Alterações:**
- Criada tabela `clients` com campos de documento, contato e status
- Índices para document (único), status e soft delete
- Cliente demonstração inserido

---

### Migration 004: Add Client ID to Companies
**Data:** 23/01/2026  
**Arquivo:** `004_add_client_id_to_companies.sql`

**Objetivo:** Vincular empresas a clientes SaaS

**Alterações:**
- Adicionado client_id CHAR(36) NOT NULL em companies
- FK para clients com ON DELETE RESTRICT
- Índice idx_company_client

---

### Migration 005: Add Client ID to Users
**Data:** 23/01/2026  
**Arquivo:** `005_add_client_id_to_users.sql`

**Objetivo:** Vincular usuários a clientes SaaS

**Alterações:**
- Adicionado client_id CHAR(36) NOT NULL em users
- company_id alterado para NULLABLE
- FK para clients
- Índice idx_user_client

---

### Migration 006: Create User Companies Table
**Data:** 23/01/2026  
**Arquivo:** `006_create_user_companies_table.sql`

**Objetivo:** Permitir acesso multi-empresa para usuários

**Alterações:**
- Criada tabela user_companies (N:N)
- Campos role, is_default, granted_at
- Migração de dados existentes de users.company_id

---

### Migration 007: Link Billing to Core
**Data:** 23/01/2026  
**Arquivo:** `007_link_billing_to_core.sql`

**Objetivo:** Vincular módulo de billing aos clientes core

**Alterações:**
- Adicionado core_client_id em BillingClients
- FK para clients

---

### Migration 008: Create Shareholders Table
**Data:** 23/01/2026  
**Arquivo:** `008_create_shareholders_table.sql`

**Objetivo:** Gerenciar sócios/acionistas das empresas

**Alterações:**
- Criada tabela shareholders
- Suporte a PF (CPF) e PJ (CNPJ)
- Tipos: Individual, Company, InvestmentFund
- Índices para company_id, type, status, document

---

### Migration 009: Create Share Classes Table
**Data:** 24/01/2026  
**Arquivo:** `009_create_share_classes_table.sql`

**Objetivo:** Gerenciar classes de ações (ON, PN, etc.)

**Alterações:**
- Criada tabela share_classes
- Campos para voting rights, liquidation preference, conversion
- Proteção anti-diluição (None, Broad-Based, Full-Ratchet)
- Classes padrão inseridas (Ordinárias e Preferenciais)

---

### Migration 010: Fix UTF8 Mojibake
**Data:** 24/01/2026  
**Arquivo:** `010_fix_utf8_mojibake.sql`

**Objetivo:** Corrigir caracteres com encoding incorreto

---

### Migration 011: Add Shareholder Details
**Data:** 24/01/2026  
**Arquivo:** `011_add_shareholder_details.sql`

**Objetivo:** Adicionar campos pessoais para sócios PF

**Alterações:**
- Campos gender, marital_status, birth_date
- Campos de endereço completo

---

### Migration 012: Add Company Address Fields
**Data:** 24/01/2026  
**Arquivo:** `012_add_company_address_fields.sql`

**Objetivo:** Adicionar endereço às empresas

---

### Migration 013: Create Shares Table
**Data:** 24/01/2026  
**Arquivo:** `013_create_shares_table.sql`

**Objetivo:** Criar estrutura completa de Cap Table

**Alterações:**
- Criada tabela shares (participações acionárias)
- Criada tabela share_transactions (ledger imutável)
- Triggers para impedir UPDATE/DELETE em transactions
- Constraints de validação de tipo e status

**Impacto:**
- ✅ Cap Table completo com rastreabilidade
- ✅ Ledger imutável para compliance
- ✅ Suporte a Issue, Transfer, Cancel, Convert, Split

---

## 🌱 Dados Iniciais

### Cliente Demonstração

```
ID: 00000000-0000-0000-0000-000000000001
Nome: Cliente Demo
CNPJ: 11222333000181
Status: Active
```

### Empresa Demonstração

```
ID: a1b2c3d4-e5f6-7890-abcd-ef1234567890
ClientId: 00000000-0000-0000-0000-000000000001
Nome: Empresa Demonstração LTDA
CNPJ: 12345678000190
Status: Active
```

### Usuário Administrador

```
ID: f1e2d3c4-b5a6-7890-abcd-ef1234567890
ClientId: 00000000-0000-0000-0000-000000000001
E-mail: admin@demo.com
Senha: Admin@123
Função: Admin
Status: Active
```

### Sócio Demonstração

```
ID: 11111111-1111-1111-1111-111111111111
CompanyId: a1b2c3d4-e5f6-7890-abcd-ef1234567890
Nome: João Silva
CPF: 52998224725
Tipo: Individual
Status: Active
```

### Classes de Ações Padrão

```
1. Ordinárias (ON) - Voting: Sim, Seniority: 1
2. Preferenciais (PN) - Voting: Não, Dividend Preference: 6%
```

**⚠️ IMPORTANTE:** Altere a senha padrão em ambiente de produção!

---

## 🔒 Considerações de Segurança

### 1. Soft Delete

Todas as tabelas principais implementam soft delete através dos campos:
- `is_deleted` / `IsDeleted`
- `deleted_at` / `DeletedAt`

Isso permite:
- Recuperação de dados excluídos acidentalmente
- Auditoria completa
- Conformidade com LGPD/GDPR

### 2. Senhas

- Armazenadas com hash bcrypt (custo 11)
- Campo: `password_hash` (255 caracteres)
- Nunca armazene senhas em texto plano

### 3. Autenticação 2FA

Campos disponíveis:
- `two_factor_enabled`
- `two_factor_secret`

### 4. Controle de Acesso

- Sistema baseado em funções (`user_roles`)
- Permissões granulares em JSON
- Bloqueio de conta após tentativas falhadas (`failed_login_attempts`, `lockout_end`)

### 5. Auditoria

A tabela `audit_logs` registra:
- Quem (user_id)
- O quê (entity_type, entity_id, action)
- Quando (created_at)
- De onde (ip_address, user_agent)
- Valores antigos e novos (old_values, new_values)

### 6. Tokens JWT

- Refresh tokens armazenados em `refresh_token`
- Expiração controlada em `refresh_token_expiry`
- Implementar rotação de tokens

---

## 📊 Estatísticas e Volumetria

### Estimativa de Crescimento

| Tabela | Crescimento Estimado | Retenção |
|--------|---------------------|----------|
| companies | 50-100/ano | Permanente |
| users | 500-1000/ano | Permanente |
| user_roles | 500-1000/ano | Permanente |
| audit_logs | 10k-50k/mês | 2 anos |
| BillingClients | 100-500/ano | Permanente |
| BillingSubscriptions | 100-500/ano | Permanente |
| BillingInvoices | 1.2k-6k/ano | 7 anos (fiscal) |
| BillingPayments | 1k-5k/ano | 7 anos (fiscal) |

### Manutenção Recomendada

1. **Arquivamento de Audit Logs**
   - Frequência: Trimestral
   - Critério: Logs com mais de 2 anos
   
2. **Limpeza de Tokens Expirados**
   - Frequência: Semanal
   - Critério: refresh_token_expiry < NOW()

3. **Análise de Índices**
   - Frequência: Semestral
   - Verificar uso e fragmentação

---

## 🔄 Próximas Fases Sugeridas

### ✅ Fase 2: Cap Table (CONCLUÍDA - Janeiro/2026)
```
Implementado:
- clients (entidade raiz SaaS)
- user_companies (acesso multi-empresa)
- shareholders (sócios/acionistas)
- share_classes (classes de ações)
- shares (participações)
- share_transactions (ledger imutável)
- Simulador de rodadas de investimento
```

### Fase 3: Vesting & Stock Options
```sql
-- vesting_schedules (cronogramas de vesting)
-- stock_options (opções de ações)
-- option_grants (concessões)
-- option_exercises (exercícios)
```

### Fase 4: Dividendos & Distribuições
```sql
-- dividends (distribuição de lucros)
-- dividend_distributions (pagamentos)
-- retained_earnings (lucros retidos)
```

### Fase 5: Módulo Financeiro
```sql
-- accounts (contas bancárias)
-- transactions (movimentações financeiras)
-- budgets (orçamentos)
-- cost_centers (centros de custo)
```

### Fase 6: Módulo de Documentos
```sql
-- documents (armazenamento de documentos)
-- document_categories
-- document_versions
-- document_signatures (assinaturas eletrônicas)
```

### Fase 7: Módulo de Notificações
```sql
-- notifications (notificações do sistema)
-- notification_templates
-- notification_preferences
```

### Fase 8: Melhorias no Billing
```sql
-- billing_discounts (descontos e promoções)
-- billing_credits (créditos)
-- billing_refunds (reembolsos)
-- billing_usage_metrics (métricas de uso)
```

---

## 🛠️ Scripts Úteis

### Verificar Integridade Referencial

```sql
-- Verificar usuários sem client
SELECT u.id, u.email 
FROM users u 
LEFT JOIN clients c ON u.client_id = c.id 
WHERE c.id IS NULL AND u.is_deleted = 0;

-- Verificar empresas sem client
SELECT c.id, c.name 
FROM companies c 
LEFT JOIN clients cl ON c.client_id = cl.id 
WHERE cl.id IS NULL AND c.is_deleted = 0;

-- Verificar sócios com ações mas sem shares ativas
SELECT sh.id, sh.name, COUNT(s.id) as shares_count
FROM shareholders sh
LEFT JOIN shares s ON sh.id = s.shareholder_id AND s.status = 'Active' AND s.is_deleted = 0
WHERE sh.is_deleted = 0
GROUP BY sh.id, sh.name
HAVING shares_count = 0;

-- Verificar assinaturas sem cliente
SELECT s.Id 
FROM BillingSubscriptions s 
LEFT JOIN BillingClients c ON s.ClientId = c.Id 
WHERE c.Id IS NULL AND s.DeletedAt IS NULL;
```

### Estatísticas de Uso

```sql
-- Total de usuários ativos por empresa
SELECT c.name, COUNT(u.id) as total_users
FROM companies c
LEFT JOIN users u ON c.id = u.company_id AND u.is_deleted = 0 AND u.status = 'Active'
WHERE c.is_deleted = 0
GROUP BY c.id, c.name;

-- Receita mensal por plano
SELECT p.Name, 
       COUNT(s.Id) as subscriptions,
       SUM(p.Price) as monthly_revenue
FROM BillingPlans p
JOIN BillingSubscriptions s ON p.Id = s.PlanId
WHERE s.Status = 1 AND s.DeletedAt IS NULL
GROUP BY p.Id, p.Name;
```

### Limpeza de Dados

```sql
-- Remover tokens expirados (executar semanalmente)
UPDATE users 
SET refresh_token = NULL, refresh_token_expiry = NULL 
WHERE refresh_token_expiry < NOW();

-- Arquivar logs antigos (executar trimestralmente)
-- Primeiro, faça backup!
-- CREATE TABLE audit_logs_archive LIKE audit_logs;
-- INSERT INTO audit_logs_archive SELECT * FROM audit_logs WHERE created_at < DATE_SUB(NOW(), INTERVAL 2 YEAR);
-- DELETE FROM audit_logs WHERE created_at < DATE_SUB(NOW(), INTERVAL 2 YEAR);
```

---

## 📞 Suporte

Para dúvidas sobre a estrutura do banco de dados ou sugestões de melhorias, consulte:
- Arquiteto de Dados: [Nome/E-mail]
- Documentação Técnica: `/docs`
- Repository: [URL do repositório]

---

**Última Atualização:** 26 de Janeiro de 2026  
**Versão do Documento:** 2.0.0
