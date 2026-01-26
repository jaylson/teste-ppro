# Fase 2 - CapTable: Controle de Progresso

**Início:** 23/01/2026  
**Última Atualização:** 26/01/2026 - Semana 5 iniciada (Frontend Cap Table)
**Status Geral:** 🟢 Semanas 1-4 concluídas | Semana 5 em andamento

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
- [x] **F2-ARC-DB-001** - Criar tabela `clients`
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Tabela criada com sucesso via migration 003_create_clients_table.sql
- [x] **F2-ARC-DB-002** - Alterar `companies`: add client_id
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Coluna client_id adicionada, FK criada, client padrão criado para migração
- [x] **F2-ARC-DB-003** - Alterar `users`: add client_id
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ client_id adicionado, company_id tornou-se nullable
- [x] **F2-ARC-DB-004** - Criar tabela `user_companies`
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Tabela criada, dados migrados de users.company_id
- [x] **F2-ARC-DB-005** - Seed data (Client demo)
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ BillingClients vinculado ao Core, client demo completo

### Backend
- [x] **F2-ARC-BE-001** - Entidade `Client.cs`
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Entidade criada com validações de CPF/CNPJ, enums DocumentType e ClientStatus adicionados
- [x] **F2-ARC-BE-006** - Atualizar Company.cs (ClientId)
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Propriedade ClientId adicionada, método Create atualizado
- [x] **F2-ARC-BE-007** - Atualizar User.cs (ClientId)
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ ClientId adicionado, CompanyId tornou-se nullable
- [x] **F2-ARC-BE-002** - DTOs: ClientRequest/Response
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ DTOs criados: ClientResponse, CreateClientRequest, UpdateClientRequest, etc
- [x] **F2-ARC-BE-003** - Validator: ClientValidator
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Validators criados com validação de CPF/CNPJ, email, campos
- [x] **F2-ARC-BE-004** - Repository: ClientRepository
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Repository completo com CRUD, paginação, queries especializadas
- [x] **F2-ARC-BE-005** - Controller: ClientsController
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Controller completo, endpoints REST, DI registrado
@@- [x] **F2-ARC-BE-008** - Middleware: ClientContextMiddleware
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Middleware criado, extrai ClientId do JWT, valida cliente ativo, AuthService atualizado
@@- [x] **F2-ARC-BE-009** - Atualizar CompanyContextMiddleware
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Middleware criado, valida Company pertence ao Client, verifica status Active

### Frontend
- [x] **F2-ARC-FE-001** - Type: Client
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Tipos criados (Client, enums DocumentType/ClientStatus, requests e resposta paginada)
- [x] **F2-ARC-FE-002** - Service: clientService.ts
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ CRUD de clientes, ações de status e listagem de empresas por cliente
- [x] **F2-ARC-FE-003** - Store: useClientStore
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Persistência de currentClient/selectedCompanyId, loading control
- [x] **F2-ARC-FE-004** - Hook: useClient
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Hooks React Query para clientes, empresas, mutate de status e documentação
- [x] **F2-ARC-FE-005** - Componente: CompanySwitcher
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Seletor de empresas com loading e fallback
- [x] **F2-ARC-FE-006** - Atualizar Header com CompanySwitcher
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Header criado no MainLayout exibindo usuário e CompanySwitcher

### Checkpoint Semana 1
- [x] Build backend sem erros
  - Corrigido: Conflito de nomes `Client` nos testes (alias BillingClient)
  - Corrigido: client_id no UserRepository SelectColumns
  - Corrigido: Claim clientId no ClientContextMiddleware
  - Corrigido: Mapeamento GUID no ShareholderRepository
- [x] Build frontend sem erros
  - Compila com warnings de chunk size (otimização futura)
- [x] Testes via Swagger OK
  - Login: ✅ Funcionando (admin@demo.com / Admin@123)
  - Shareholders GET: ✅ Funcionando
  - Health Check: ✅ Healthy (MySQL, Redis, Hangfire)
- [x] Company Switcher funcionando
  - Componente criado e integrado ao Header

---

## SEMANA 2: Shareholders (Backend)

### Database
- [x] **F2-SHR-DB-001** - Criar tabela `shareholders`
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Migration 008_create_shareholders_table.sql criada (client_id, company_id, índices, constraints de tipo/status/document_type)
- [x] **F2-SHR-DB-002** - Índices de performance
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Índices em company_id, type, status, document; unique (company_id, document)

### Backend
- [x] **F2-SHR-BE-001** - Entidade Shareholder.cs
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Agregado com validação CPF/CNPJ, ClientId/CompanyId, status e notas
- [x] **F2-SHR-BE-002/003** - Enums ShareholderType/Status
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Enums já existentes reutilizados
- [x] **F2-SHR-BE-004/005** - DTOs ShareholderRequest/Response + lista
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Resposta inclui CompanyName, paginado com PagedResult
- [x] **F2-SHR-BE-006** - Validator ShareholderValidator
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ FluentValidation para CPF/CNPJ, email, phone
- [x] **F2-SHR-BE-007/013** - Interface/Repository ShareholderRepository
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Dapper com filtros, paginação, busca por documento, soft delete
- [x] **F2-SHR-BE-014/015** - Service ShareholderService
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Regras de negócio (empresa pertence ao cliente, conflito de documento), mapeia para DTO
- [x] **F2-SHR-BE-016/020** - ShareholdersController (CRUD)
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ Endpoints GET list/id, POST, PUT, DELETE com ClientContext + Company header opcional
- [x] **F2-SHR-BE-021** - Registrar DI
  - Início: 23/01/2026
  - Fim: 23/01/2026
  - Observações: ✅ ServiceExtensions registra ShareholderRepository/Service

### Testes
- [x] **F2-SHR-TST-001** - Testes via Swagger
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ Endpoints testados via curl, GET shareholders retorna dados corretamente

### Checkpoint Semana 2
- [x] CRUD Shareholders completo via API (testado)
  - GET /api/shareholders: ✅ Retorna lista paginada
  - Autenticação JWT: ✅ ClientId extraído do token
  - Middleware ClientContext: ✅ Valida cliente ativo

---

## SEMANA 3: Shareholders Frontend + Share Classes Backend

### Frontend Shareholders
- [x] **F2-SHR-FE-001** - Type: Shareholder TypeScript interfaces
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ shareholder.types.ts com enums, interfaces e tipos de request/response
- [x] **F2-SHR-FE-002** - Service: shareholderService.ts
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ CRUD completo, paginação e filtros
- [x] **F2-SHR-FE-003/004** - Hooks: useShareholders (queries + mutations)
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ React Query hooks para listagem, detalhe, create, update, delete
- [x] **F2-SHR-FE-005** - Componente: ShareholderCard
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ Card com avatar, tipo, status, documento e ações
- [x] **F2-SHR-FE-006** - Componente: ShareholderBadge (Type + Status)
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ TypeBadge (Individual/Company/Investment Fund) e StatusBadge (Active/Inactive/Pending)
- [x] **F2-SHR-FE-007** - Componente: ShareholderFilters
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ Busca por nome, filtros de tipo e status, botão limpar
- [x] **F2-SHR-FE-008** - Página: ShareholdersListPage
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ Grid de cards, paginação, filtros, modal de criação
- [x] **F2-SHR-FE-009** - Modal: ShareholderFormModal
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ Form completo com validação, modo create/edit
- [x] **F2-SHR-FE-010** - Página: ShareholderDetailPage
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ Detalhes completos, tabs futuras preparadas, edição e exclusão

### Database Share Classes
- [x] **F2-SHC-DB-001** - Criar tabela `share_classes`
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ Migration 009_create_share_classes_table.sql com voting rights, liquidation, conversion, anti-dilution

### Backend Share Classes
- [x] **F2-SHC-BE-001** - Entidade ShareClass.cs
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ Agregado com preferências, conversão, anti-diluição, triggers de atualização
- [x] **F2-SHC-BE-002** - DTOs: ShareClassRequest/Response
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ ShareClassResponse, CreateShareClassRequest, UpdateShareClassRequest, ShareClassSummaryDto
- [x] **F2-SHC-BE-003** - Validator: ShareClassValidator
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ FluentValidation para nome, seniority, prices, authorized shares
- [x] **F2-SHC-BE-004** - Repository: ShareClassRepository
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ Dapper CRUD com filtros, paginação, busca por nome, company_id
- [x] **F2-SHC-BE-005** - Controller: ShareClassesController
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ Endpoints REST /api/share-classes, ShareClassService com regras de negócio, DI registrado

### Checkpoint Semana 3
- [x] Build backend sem erros
  - Build succeeded with 2 warning(s) - warnings em BackgroundJobs e ClientsController
- [x] Build frontend sem erros
  - Compilado com warning de chunk size (912 kB)
- [x] Rotas /shareholders configuradas
  - Rotas /shareholders e /shareholders/:id adicionadas ao App.tsx
- [x] Componentes visuais funcionais
  - Cards, Badges, Filtros, Modal e Páginas implementados

---

## SEMANA 4: Shares + Ledger + Cap Table View

### Database
- [x] **F2-SHA-DB-001** - Criar tabela `shares`
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ Migration 013_create_shares_table.sql com campos completos
- [x] **F2-SHA-DB-002** - Criar tabela `share_transactions`
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ Ledger imutável com triggers para impedir UPDATE/DELETE
- [x] **F2-SHA-DB-003** - Trigger: Impedir UPDATE/DELETE em transactions
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ Triggers criados: trg_share_transactions_no_update, trg_share_transactions_no_delete

### Backend
- [x] **F2-SHA-BE-001** - Entidade `Share.cs`
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ Entidade com status, origin, navigation properties
- [x] **F2-SHA-BE-002** - Entidade `ShareTransaction.cs`
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ Imutável, factory methods para Issue/Transfer/Cancel/Convert
- [x] **F2-SHA-BE-003** - Enums ShareOrigin, ShareStatus, TransactionType
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ Adicionados em Enums.cs
- [x] **F2-SHA-BE-004** - DTOs de Shares e Transactions
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ ShareDTOs.cs com Request/Response completos + CapTable DTOs
- [x] **F2-SHA-BE-005** - Validators ShareValidators
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ FluentValidation para Issue/Transfer/Cancel/Convert
- [x] **F2-SHA-BE-006** - Repository: ShareRepository
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ CRUD, paginação, balance queries
- [x] **F2-SHA-BE-007** - Repository: ShareTransactionRepository
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ Append-only, transaction number generation
- [x] **F2-SHA-BE-008** - Service: ShareService
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ IssueShares, TransferShares, CancelShares, GetCapTable
- [x] **F2-SHA-BE-009** - Controller: SharesController
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ Endpoints REST para shares, transactions, balance
- [x] **F2-CAP-BE-001** - Controller: CapTableController
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ GET cap-table/{companyId}, summary-by-type, summary-by-class

### Testes
- [x] **F2-SHA-TST-001** - Testes via API
  - Início: 24/01/2026
  - Fim: 24/01/2026
  - Observações: ✅ Endpoints testados: GET /shares, GET /transactions, GET /cap-table

### Checkpoint Semana 4
- [x] Build backend sem erros
  - Build succeeded with 2 warning(s)
- [x] Endpoints Shares funcionando
  - GET /api/shares ✅
  - GET /api/shares/transactions ✅
  - GET /api/shares/balance ✅
- [x] Cap Table funcionando
  - GET /api/cap-table/{companyId} ✅
  - Retorna entries, summaryByType, summaryByClass

---

## SEMANA 5: Frontend Cap Table + Simulador + Documentação

### Frontend Cap Table
- [x] **F2-CAP-FE-001** - Types: Share, Transaction, CapTable
  - Início: 26/01/2026
  - Fim: 26/01/2026
  - Observações: ✅ share.types.ts com enums (ShareOrigin, ShareStatus, TransactionType), interfaces e helpers
- [x] **F2-CAP-FE-002** - Service: shareService.ts
  - Início: 26/01/2026
  - Fim: 26/01/2026
  - Observações: ✅ CRUD shares, transactions, operações (issue, transfer, cancel)
- [x] **F2-CAP-FE-003** - Service: capTableService.ts
  - Início: 26/01/2026
  - Fim: 26/01/2026
  - Observações: ✅ getCapTable, getSummaryByType, getSummaryByClass
- [x] **F2-CAP-FE-004** - Hook: useCapTable
  - Início: 26/01/2026
  - Fim: 26/01/2026
  - Observações: ✅ useCapTable, useShares, useTransactions, useIssueShares, useTransferShares, useCancelShares
- [x] **F2-CAP-FE-005** - Componente: CapTableStats
  - Início: 26/01/2026
  - Fim: 26/01/2026
  - Observações: ✅ 4 StatCards (Total Ações, Valor Total, Acionistas, Classes), loading skeleton, utils/format.ts criado 
- [x] **F2-CAP-FE-006** - Componente: CapTableChart
  - Início: 26/01/2026
  - Fim: 26/01/2026
  - Observações: ✅ Gráfico donut Recharts com view type/class, tooltip customizado, paleta de 10 cores, loading skeleton
- [x] **F2-CAP-FE-007** - Componente: CapTableTable
  - Início: 26/01/2026
  - Fim: 26/01/2026
  - Observações: ✅ Tabela com sort em 5 colunas, busca por acionista/classe, barra de progresso %, avatar inicial, loading skeleton
- [x] **F2-CAP-FE-008** - Página: CapTablePage
  - Início: 26/01/2026
  - Fim: 26/01/2026
  - Observações: ✅ Página completa integrando Stats, Chart (toggle type/class), Table, resumo por classe, data de referência, botões refresh/export 

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

### Checkpoint Semana 5
- [ ] Todos os builds passando
- [ ] Documentação atualizada
- [ ] Demo completa funcionando

---

## Correções Aplicadas (24/01/2026)

### Banco de Dados
- [x] Migration 002 aplicada manualmente (DueDay, PaymentMethod em BillingSubscriptions)
- [x] CNPJ do cliente demo corrigido (de 00000000000000 para 11222333000181)
- [x] CPF do shareholder demo corrigido (de 12345678901 para 52998224725)
- [x] Hash da senha do usuário admin regenerado (BCrypt válido)

### Backend
- [x] ClientTests.cs: Alias `BillingClient` para resolver conflito de nomes
- [x] UserRepository.cs: Adicionado `client_id AS ClientId` no SelectColumns
- [x] ClientContextMiddleware.cs: Busca claim `clientId` (minúsculo) primeiro
- [x] ShareholderRepository.cs: ParseGuid para suportar Guid e string do Dapper
- [x] CompanyRepository.cs: Adicionado `client_id AS ClientId` em todas as queries SELECT

### Semana 3 - Correções Adicionais
- [x] Migration 009: Corrigido INSERT de share class convertível (is_convertible = 0 no INSERT, depois UPDATE)
- [x] BusinessException: Adicionada classe em DomainExceptions.cs

### Semana 4 - Correções Adicionais
- [x] Share.cs: Substituído MarkAsUpdated() por UpdatedAt = DateTime.UtcNow
- [x] ShareRepository.cs: Adicionado método ParseBool para converter TINYINT → bool
- [x] TypeScript: Corrigido DocumentType re-export em shareholder.types.ts
- [x] TypeScript: Corrigido confirmVariant vs variant em confirmações

---

## Credenciais de Teste

```
Email: admin@demo.com
Senha: Admin@123
CompanyId: a1b2c3d4-e5f6-7890-abcd-ef1234567890
ClientId: 00000000-0000-0000-0000-000000000001
```

## URLs de Acesso

| Serviço | URL |
|---------|-----|
| API | http://localhost:5000 |
| Swagger | http://localhost:5000/swagger |
| Frontend | http://localhost:3000 |
| Health | http://localhost:5000/health |
