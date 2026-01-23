# 📊 Documentação da Estrutura do Banco de Dados
## Partnership Manager - Database Schema

**Versão:** 1.0.0  
**Data de Atualização:** 23 de Janeiro de 2026  
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

1. **Core Module** - Empresas e Usuários
2. **Billing Module** - Faturamento e Assinaturas
3. **Audit Module** - Logs de Auditoria

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

#### 1. companies

Tabela central que armazena informações das empresas/companhias gerenciadas no sistema.

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| id | CHAR(36) | NOT NULL | Identificador único (GUID) |
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
| created_at | DATETIME(6) | NOT NULL | Data de criação |
| updated_at | DATETIME(6) | NOT NULL | Data de última atualização |
| created_by | CHAR(36) | NULL | ID do usuário criador |
| updated_by | CHAR(36) | NULL | ID do último usuário que atualizou |
| is_deleted | TINYINT(1) | NOT NULL | Flag de soft delete (padrão: 0) |
| deleted_at | DATETIME(6) | NULL | Data de exclusão (soft delete) |

**Índices:**
- PRIMARY KEY: `id`
- UNIQUE INDEX: `idx_company_cnpj` (cnpj)
- INDEX: `idx_company_status` (status)
- INDEX: `idx_company_deleted` (is_deleted)

**Valores Padrões:**
- currency: 'BRL'
- status: 'Active'
- is_deleted: 0

---

#### 2. users

Armazena informações dos usuários do sistema, vinculados a uma empresa.

| Coluna | Tipo | Nulo | Descrição |
|--------|------|------|-----------|
| id | CHAR(36) | NOT NULL | Identificador único (GUID) |
| company_id | CHAR(36) | NOT NULL | FK para companies |
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
- INDEX: `idx_user_company` (company_id)
- UNIQUE INDEX: `idx_user_email_company` (company_id, email)
- INDEX: `idx_user_status` (status)
- INDEX: `idx_user_deleted` (is_deleted)

**Foreign Keys:**
- `fk_user_company`: company_id → companies(id) ON DELETE RESTRICT

**Valores Padrões:**
- status: 'Pending'
- language: 'Portuguese'
- timezone: 'America/Sao_Paulo'
- two_factor_enabled: 0
- failed_login_attempts: 0
- is_deleted: 0

---

#### 3. user_roles

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

### 💰 Billing Module

#### 5. BillingClients

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
companies (1) ──────────── (N) users
    │
    └── (1) ──────────── (N) audit_logs

users (1) ──────────── (N) user_roles
    │
    └── (1) ──────────── (N) audit_logs

BillingClients (1) ──────────── (N) BillingSubscriptions
                │
                └── (1) ──────────── (N) BillingInvoices

BillingPlans (1) ──────────── (N) BillingSubscriptions

BillingSubscriptions (1) ──────────── (N) BillingInvoices

BillingInvoices (1) ──────────── (N) BillingPayments
```

### Regras de Integridade Referencial

1. **companies → users**: ON DELETE RESTRICT
   - Não permite exclusão de empresa com usuários associados
   
2. **users → user_roles**: ON DELETE CASCADE
   - Remove todas as funções ao excluir um usuário
   
3. **BillingClients → BillingSubscriptions**: Padrão (RESTRICT)
   
4. **BillingPlans → BillingSubscriptions**: Padrão (RESTRICT)
   
5. **BillingSubscriptions → BillingInvoices**: Padrão (RESTRICT)
   
6. **BillingInvoices → BillingPayments**: Padrão (RESTRICT)

---

## ⚡ Índices e Performance

### Índices Únicos (Garantem Integridade)

1. **companies.idx_company_cnpj** - CNPJ único por empresa
2. **users.idx_user_email_company** - E-mail único por empresa
3. **user_roles.idx_role_user_active** - Uma função ativa por usuário
4. **BillingClients.idx_billing_client_email** - E-mail único (com DeletedAt)
5. **BillingClients.idx_billing_client_document** - CPF/CNPJ único (com DeletedAt)
6. **BillingInvoices.idx_billing_invoice_number** - Número único de fatura

### Índices Compostos (Otimização de Queries)

1. **BillingInvoices.idx_subscription_issue_month** (SubscriptionId, IssueDate)
   - Otimiza busca de faturas por período
   
2. **BillingInvoices.idx_subscription_issue_status** (SubscriptionId, IssueDate, Status)
   - Busca de faturas ativas/pendentes por assinatura e período
   
3. **audit_logs.idx_audit_entity** (entity_type, entity_id)
   - Rastreamento de auditoria por entidade

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
**Data:** (Data não especificada)  
**Arquivo:** `002_add_subscription_payment_fields.sql`

**Objetivo:** Adicionar controle de vencimento e método de pagamento preferencial

**Alterações:**
```sql
ALTER TABLE BillingSubscriptions 
ADD COLUMN DueDay INT NOT NULL DEFAULT 10
ADD COLUMN PaymentMethod INT NOT NULL DEFAULT 3
ADD CONSTRAINT CHK_Subscription_DueDay CHECK (DueDay >= 1 AND DueDay <= 31)
CREATE INDEX IX_BillingSubscriptions_PaymentMethod ON BillingSubscriptions(PaymentMethod)
```

**Impacto:**
- ✅ Permite definir dia de vencimento customizado por assinatura
- ✅ Registra método de pagamento preferencial do cliente
- ✅ Facilita automação de geração de faturas

---

## 🌱 Dados Iniciais

### Empresa Demonstração

```
ID: a1b2c3d4-e5f6-7890-abcd-ef1234567890
Nome: Empresa Demonstração LTDA
CNPJ: 12345678000190
Status: Active
```

### Usuário Administrador

```
ID: f1e2d3c4-b5a6-7890-abcd-ef1234567890
E-mail: admin@demo.com
Senha: Admin@123
Função: Admin
Status: Active
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

### Fase 1: Módulo de Sócios/Acionistas
```sql
-- shareholders (sócios/acionistas)
-- share_transactions (movimentação de ações)
-- dividends (distribuição de lucros)
```

### Fase 2: Módulo Financeiro
```sql
-- accounts (contas bancárias)
-- transactions (movimentações financeiras)
-- budgets (orçamentos)
-- cost_centers (centros de custo)
```

### Fase 3: Módulo de Documentos
```sql
-- documents (armazenamento de documentos)
-- document_categories
-- document_versions
```

### Fase 4: Módulo de Notificações
```sql
-- notifications (notificações do sistema)
-- notification_templates
-- notification_preferences
```

### Fase 5: Melhorias no Billing
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
-- Verificar FKs órfãs em users
SELECT u.id, u.email 
FROM users u 
LEFT JOIN companies c ON u.company_id = c.id 
WHERE c.id IS NULL AND u.is_deleted = 0;

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

**Última Atualização:** 23 de Janeiro de 2026  
**Versão do Documento:** 1.0.0
