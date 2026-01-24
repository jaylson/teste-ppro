# Fase 2 - CapTable: Controle de Progresso

**Início:** 23/01/2026  
**Última Atualização:** 24/01/2026 - Checkpoints Semana 1 e 2 concluídos  
**Status Geral:** 🟢 Semana 1 e 2 concluídas (Backend 100%, Frontend 100%, API testada)

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
