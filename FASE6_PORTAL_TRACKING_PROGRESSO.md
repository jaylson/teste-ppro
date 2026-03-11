# Partnership Manager — Fase 6
## 📋 Tracking de Progresso das Tarefas

**Versão:** 1.0  
**Data de Criação:** 06 de Março de 2026  
**Última Atualização:** 06 de Março de 2026  
**Responsável:** GitHub Agent + Claude Sonnet  
**Status Geral:** ⚪ Não Iniciado  
**Fase:** 6 — Portal do Investidor + Comunicações + Notificações + Workflow

---

## 🎯 INSTRUÇÕES PARA AGENTES DE IA

### ⚠️ **REGRAS CRÍTICAS**

1. **SEMPRE** atualizar este arquivo ao iniciar/pausar/concluir tarefas
2. **NUNCA** reanalisar tarefas marcadas como `[x]` Concluído
3. **SEMPRE** verificar `## 🕐 ÚLTIMA SESSÃO` antes de continuar
4. **MARCAR** claramente qual é a próxima tarefa pendente com `→ PRÓXIMA`
5. **VERIFICAR** entidades/serviços existentes antes de criar (premissa DRY)

### 📌 **ESTADOS DE TAREFA**

| Estado | Símbolo | Descrição |
|--------|---------|-----------|
| **Pendente** | `[ ]` | Tarefa não iniciada |
| **Em Andamento** | `[🔄]` | Tarefa sendo executada atualmente |
| **Pausado** | `[⏸️]` | Tarefa iniciada mas pausada |
| **Concluído** | `[x]` | Tarefa completamente finalizada |
| **Bloqueado** | `[❌]` | Tarefa impedida por dependência |
| **Pulado** | `[⏭️]` | Tarefa pulada (já existia ou desnecessária) |

### 🚀 **COMO USAR ESTE DOCUMENTO**

```bash
# AO INICIAR TRABALHO
1. Leia a seção "🕐 ÚLTIMA SESSÃO" por completo
2. Identifique a tarefa marcada como → PRÓXIMA
3. Marque como [🔄] em andamento
4. Execute seguindo o plano FASE6_PORTAL_PLANO_EXECUCAO.md
5. Marque como [x] ao concluir e atualize observações

# AO PAUSAR TRABALHO
1. Marque tarefa atual como [⏸️]
2. Adicione observações em "🕐 ÚLTIMA SESSÃO"
3. Identifique a próxima tarefa com → PRÓXIMA
4. Commit: "chore: pausando F6-XXX-YYY"

# AO RETOMAR TRABALHO
1. Leia "🕐 ÚLTIMA SESSÃO"
2. Continue da tarefa [⏸️] ou próxima [ ]
3. Marque como [🔄] e prossiga
```

---

## 🕐 ÚLTIMA SESSÃO

```
Data: 06/03/2026
Sessão: #0 — Fase ainda não iniciada
Última Tarefa: N/A
Próxima Tarefa: F6-DB-001 (primeira migration)
Bloqueios: Nenhum
Notas: 
  - Verificar se tabela `documents` já existe (Fase 5 pode ter criado)
  - Verificar se EmailService já existe (Fase 3 pode ter implementado)
  - Verificar se migration counter atual está em 039 ou anterior
  - Ajustar numeração das migrations conforme estado atual do banco
```

---

## 📊 PROGRESSO GERAL

| Área | Total | Concluído | Em Andamento | Pendente |
|------|-------|-----------|--------------|----------|
| 🗄️ Banco de Dados | 7 | 0 | 0 | 7 |
| 🔷 Backend Domain | 4 | 0 | 0 | 4 |
| 🔶 Backend Repository | 4 | 0 | 0 | 4 |
| ⚙️ Backend Service | 7 | 0 | 0 | 7 |
| 🌐 Backend API | 6 | 0 | 0 | 6 |
| 🖥️ Frontend | 20 | 0 | 0 | 20 |
| **TOTAL** | **48** | **0** | **0** | **48** |

**Progresso:** 0% ░░░░░░░░░░░░░░░░░░░░ 0/48

---

## 🗄️ SEMANA 1 — BANCO DE DADOS (estimativa: 22h)

> Executar todas as migrations em sequência. Verificar numeração atual antes de criar.

---

### F6-DB-001 — Migration: Tabela `communications`
- **Status:** [ ] Pendente → **PRÓXIMA**
- **Tipo:** Database Migration
- **Estimativa:** 3h
- **Dependências:** Fases 1–5 concluídas
- **Arquivo:** `/database/migrations/040_create_communications_table.sql`
- **Critérios de Aceite:**
  - [ ] Migration executada sem erros
  - [ ] Tabela criada com todos os campos (id, company_id, title, content, content_html, summary, comm_type, visibility, target_roles, attachments, is_pinned, published_at, expires_at, created_by, created_at, updated_at, deleted_at)
  - [ ] Foreign keys para `companies` e `users`
  - [ ] 5 índices criados (company, type, published, pinned, deleted)
  - [ ] Script de rollback testado: `DROP TABLE IF EXISTS communications`
  - [ ] Query de validação retorna `1`
- **Observações:**

---

### F6-DB-002 — Migration: Tabela `communication_views`
- **Status:** [❌] Bloqueado por F6-DB-001
- **Tipo:** Database Migration
- **Estimativa:** 2h
- **Dependências:** F6-DB-001
- **Arquivo:** `/database/migrations/041_create_communication_views_table.sql`
- **Critérios de Aceite:**
  - [ ] Migration executada sem erros
  - [ ] UNIQUE KEY `(communication_id, user_id)` — impede duplicatas
  - [ ] CASCADE ON DELETE da tabela `communications`
  - [ ] Rollback testado
- **Observações:**

---

### F6-DB-003 — Migration: Tabela `documents` (verificar/criar/complementar)
- **Status:** [ ] Pendente
- **Tipo:** Database Migration
- **Estimativa:** 3h
- **Dependências:** Fase 5 concluída
- **Arquivo:** `/database/migrations/042_create_or_alter_documents_table.sql`
- **⚠️ AÇÃO PRÉVIA:** Executar `SHOW TABLES LIKE 'documents'` — se existir, usar `ALTER TABLE`
- **Critérios de Aceite:**
  - [ ] Verificação prévia feita (existe ou não existe)
  - [ ] Tabela possui todos os campos: id, company_id, name, description, document_type, file_url, file_name, file_size_bytes, mime_type, checksum, reference_type, reference_id, verification_status, verified_at, verified_by, visibility, uploaded_by, created_at, deleted_at
  - [ ] Sem duplicação de colunas
  - [ ] Índices criados/verificados
  - [ ] Rollback testado
- **Observações:**

---

### F6-DB-004 — Migration: Data Room (3 tabelas)
- **Status:** [❌] Bloqueado por F6-DB-003
- **Tipo:** Database Migration
- **Estimativa:** 4h
- **Dependências:** F6-DB-003
- **Arquivo:** `/database/migrations/043_create_data_room_tables.sql`
- **Critérios de Aceite:**
  - [ ] Tabela `data_rooms` criada com UNIQUE KEY `(company_id)` — uma por empresa
  - [ ] Tabela `data_room_folders` com auto-referência `parent_id`
  - [ ] Tabela `data_room_documents` com UNIQUE KEY `(folder_id, document_id)`
  - [ ] CASCADE em delete de `data_rooms` → folders → documents
  - [ ] Rollback testado para as 3 tabelas
- **Observações:**

---

### F6-DB-005 — Migration: Tabelas `notifications` e `notification_preferences`
- **Status:** [❌] Bloqueado por F6-DB-001
- **Tipo:** Database Migration
- **Estimativa:** 3h
- **Dependências:** F6-DB-001
- **Arquivo:** `/database/migrations/044_create_notifications_tables.sql`
- **Critérios de Aceite:**
  - [ ] Tabela `notifications` com todos os campos (id, user_id, company_id, notification_type, title, body, action_url, reference_type, reference_id, is_read, read_at, created_at)
  - [ ] Índice composto `(user_id, is_read)` — crítico para badge count
  - [ ] Índice `(user_id, created_at DESC)` — para listagem ordenada
  - [ ] Tabela `notification_preferences` com UNIQUE KEY `(user_id, notification_type)`
  - [ ] CASCADE ON DELETE da tabela `users`
  - [ ] Rollback testado
- **Observações:**

---

### F6-DB-006 — Migration: Tabela `email_logs`
- **Status:** [❌] Bloqueado por F6-DB-005
- **Tipo:** Database Migration
- **Estimativa:** 2h
- **Dependências:** F6-DB-005
- **Arquivo:** `/database/migrations/045_create_email_logs_table.sql`
- **Critérios de Aceite:**
  - [ ] Tabela criada com campos de tracking: resend_message_id, status, error_message, sent_at
  - [ ] Índice por status para re-tentativas: `idx_emaillog_status`
  - [ ] Rollback testado
- **Observações:**

---

### F6-DB-007 — Migration: Tabelas Workflow (3 tabelas)
- **Status:** [ ] Pendente
- **Tipo:** Database Migration
- **Estimativa:** 5h
- **Dependências:** F6-DB-001
- **Arquivo:** `/database/migrations/046_create_workflow_tables.sql`
- **Critérios de Aceite:**
  - [ ] Tabela `workflows` com campos: id, company_id, workflow_type, reference_type, reference_id, title, description, status, priority, current_step, total_steps, requested_by, requested_at, due_date, completed_at, cancelled_at, cancelled_by, cancellation_reason, metadata, created_at
  - [ ] Tabela `workflow_steps` com campos: id, workflow_id, step_order, name, description, step_type, assigned_role, assigned_user_id, status, is_current, started_at, due_date, completed_at, completed_by, notes
  - [ ] Tabela `workflow_approvals` com campos: id, workflow_step_id, user_id, decision, comments, decided_at, ip_address
  - [ ] CASCADE: workflows → steps → approvals
  - [ ] Índice `(workflow_id, is_current)` na tabela steps
  - [ ] Rollback testado para as 3 tabelas em ordem inversa
- **Observações:**

---

## 🔷 SEMANA 2 — BACKEND: DOMAIN + REPOSITORIES (estimativa: 29h)

---

### F6-BE-DOM-001 — Domain: Communication + CommunicationView
- **Status:** [❌] Bloqueado por F6-DB-001, F6-DB-002
- **Tipo:** Backend Domain
- **Estimativa:** 3h
- **Arquivos:**
  - `Domain/Entities/Communication.cs`
  - `Domain/Entities/CommunicationView.cs`
  - `Domain/Enums/CommunicationType.cs`
  - `Domain/Enums/CommunicationVisibility.cs`
  - `Domain/Interfaces/ICommunicationRepository.cs`
- **Critérios de Aceite:**
  - [ ] Entidades herdam de `BaseEntity`
  - [ ] Todos os campos da migration mapeados
  - [ ] Enums com todos os valores definidos
  - [ ] Interface com 8 métodos (CRUD + tracking)
  - [ ] `dotnet build` sem erros
- **Observações:**

---

### F6-BE-DOM-002 — Domain: Document + DataRoom + DataRoomFolder
- **Status:** [❌] Bloqueado por F6-DB-003, F6-DB-004
- **Tipo:** Backend Domain
- **Estimativa:** 3h
- **Arquivos:**
  - `Domain/Entities/Document.cs` (verificar se existe)
  - `Domain/Entities/DataRoom.cs`
  - `Domain/Entities/DataRoomFolder.cs`
  - `Domain/Enums/DocumentType.cs` (verificar se existe)
  - `Domain/Enums/DocumentVisibility.cs`
  - `Domain/Interfaces/IDocumentRepository.cs`
  - `Domain/Interfaces/IDataRoomRepository.cs`
- **Critérios de Aceite:**
  - [ ] Verificação feita — sem duplicação com Fase 5
  - [ ] `DataRoomFolder.ParentId` nullable (auto-referência)
  - [ ] Interfaces definidas
  - [ ] `dotnet build` sem erros
- **Observações:**

---

### F6-BE-DOM-003 — Domain: Notification + NotificationPreference
- **Status:** [❌] Bloqueado por F6-DB-005
- **Tipo:** Backend Domain
- **Estimativa:** 2h
- **Arquivos:**
  - `Domain/Entities/Notification.cs`
  - `Domain/Entities/NotificationPreference.cs`
  - `Domain/Enums/NotificationType.cs`
  - `Domain/Enums/NotificationChannel.cs`
  - `Domain/Interfaces/INotificationRepository.cs`
- **Critérios de Aceite:**
  - [ ] 5 arquivos criados
  - [ ] Interface com métodos: CreateAsync, GetByUserAsync, GetUnreadCountAsync, MarkAsReadAsync, MarkAllAsReadAsync, GetPreferenceAsync, UpsertPreferenceAsync
  - [ ] `dotnet build` sem erros
- **Observações:**

---

### F6-BE-DOM-004 — Domain: Workflow + WorkflowStep + WorkflowApproval
- **Status:** [❌] Bloqueado por F6-DB-007
- **Tipo:** Backend Domain
- **Estimativa:** 4h
- **Arquivos:**
  - `Domain/Entities/Workflow.cs`
  - `Domain/Entities/WorkflowStep.cs`
  - `Domain/Entities/WorkflowApproval.cs`
  - `Domain/Enums/WorkflowType.cs`
  - `Domain/Enums/WorkflowStatus.cs`
  - `Domain/Enums/WorkflowStepStatus.cs`
  - `Domain/Enums/WorkflowDecision.cs`
  - `Domain/Interfaces/IWorkflowRepository.cs`
- **Critérios de Aceite:**
  - [ ] 8 arquivos criados
  - [ ] `Workflow.CurrentStep` e `TotalSteps` mapeados
  - [ ] Interface com 9 métodos
  - [ ] `dotnet build` sem erros
- **Observações:**

---

### F6-BE-REP-001 — Repository: CommunicationRepository
- **Status:** [❌] Bloqueado por F6-BE-DOM-001
- **Tipo:** Backend Repository
- **Estimativa:** 4h
- **Arquivo:** `Infrastructure/Repositories/CommunicationRepository.cs`
- **Critérios de Aceite:**
  - [ ] 8 métodos implementados
  - [ ] Todas as queries com `company_id` (multi-tenancy)
  - [ ] Todas as queries com `deleted_at IS NULL`
  - [ ] `GetForRoleAsync` filtra JSON `target_roles`
  - [ ] DI registrada em `Program.cs`
  - [ ] `dotnet build` sem erros
- **Observações:**

---

### F6-BE-REP-002 — Repository: DocumentRepository + DataRoomRepository
- **Status:** [❌] Bloqueado por F6-BE-DOM-002
- **Tipo:** Backend Repository
- **Estimativa:** 5h
- **Arquivos:**
  - `Infrastructure/Repositories/DocumentRepository.cs`
  - `Infrastructure/Repositories/DataRoomRepository.cs`
- **Critérios de Aceite:**
  - [ ] DocumentRepository: 6 métodos
  - [ ] DataRoomRepository: 7 métodos
  - [ ] `GetByCompanyAsync` cria DataRoom automaticamente se não existir
  - [ ] DI registrada
  - [ ] `dotnet build` sem erros
- **Observações:**

---

### F6-BE-REP-003 — Repository: NotificationRepository
- **Status:** [❌] Bloqueado por F6-BE-DOM-003
- **Tipo:** Backend Repository
- **Estimativa:** 3h
- **Arquivo:** `Infrastructure/Repositories/NotificationRepository.cs`
- **Critérios de Aceite:**
  - [ ] 8 métodos implementados
  - [ ] `GetUnreadCountAsync` usa índice composto
  - [ ] `UpsertPreferenceAsync` usa INSERT ... ON DUPLICATE KEY UPDATE
  - [ ] DI registrada
  - [ ] `dotnet build` sem erros
- **Observações:**

---

### F6-BE-REP-004 — Repository: WorkflowRepository
- **Status:** [❌] Bloqueado por F6-BE-DOM-004
- **Tipo:** Backend Repository
- **Estimativa:** 5h
- **Arquivo:** `Infrastructure/Repositories/WorkflowRepository.cs`
- **Critérios de Aceite:**
  - [ ] `CreateAsync` cria workflow + steps em transação única
  - [ ] `AdvanceStepAsync` atualiza `current_step` e flags `is_current`
  - [ ] 9 métodos implementados
  - [ ] DI registrada
  - [ ] `dotnet build` sem erros
- **Observações:**

---

## ⚙️ SEMANA 3 — BACKEND: SERVICES + JOBS (estimativa: 37h)

---

### F6-BE-SVC-001 — Service: CommunicationService
- **Status:** [❌] Bloqueado por F6-BE-REP-001, F6-BE-REP-003
- **Tipo:** Backend Service
- **Estimativa:** 5h
- **Arquivos:**
  - `Application/DTOs/Communication/CreateCommunicationRequest.cs`
  - `Application/DTOs/Communication/UpdateCommunicationRequest.cs`
  - `Application/DTOs/Communication/CommunicationResponse.cs`
  - `Application/Validators/Communication/CreateCommunicationValidator.cs`
  - `Application/Services/CommunicationService.cs`
  - `Domain/Interfaces/ICommunicationService.cs`
- **Critérios de Aceite:**
  - [ ] FluentValidation em todos os Requests
  - [ ] `Publish` define `published_at` e dispara notificações
  - [ ] `GetForUser` filtra por role do usuário logado
  - [ ] Mensagens de erro em `Messages.cs`
  - [ ] Testes unitários: mínimo 5 casos de teste
  - [ ] DI registrada
  - [ ] `dotnet build` sem erros
- **Observações:**

---

### F6-BE-SVC-002 — Service: DocumentService + DataRoomService
- **Status:** [❌] Bloqueado por F6-BE-REP-002
- **Tipo:** Backend Service
- **Estimativa:** 6h
- **Arquivos:**
  - `Application/DTOs/Document/UploadDocumentRequest.cs`
  - `Application/DTOs/Document/DocumentResponse.cs`
  - `Application/DTOs/DataRoom/DataRoomFolderResponse.cs`
  - `Application/Services/DocumentService.cs`
  - `Application/Services/DataRoomService.cs`
- **Critérios de Aceite:**
  - [ ] Validação: tipos MIME permitidos (PDF, DOCX, XLSX, PNG, JPG)
  - [ ] Validação: tamanho máximo 50MB
  - [ ] Checksum SHA-256 calculado no upload
  - [ ] `DataRoomService.GetOrCreateAsync` cria DataRoom se não existir
  - [ ] Testes unitários: mínimo 4 casos de teste
  - [ ] `dotnet build` sem erros
- **Observações:**

---

### F6-BE-SVC-003 — Service: NotificationService
- **Status:** [❌] Bloqueado por F6-BE-REP-003
- **Tipo:** Backend Service
- **Estimativa:** 4h
- **Arquivos:**
  - `Application/DTOs/Notification/NotificationResponse.cs`
  - `Application/DTOs/Notification/NotificationPreferenceRequest.cs`
  - `Application/Services/NotificationService.cs`
  - `Domain/Interfaces/INotificationService.cs`
- **Critérios de Aceite:**
  - [ ] `NotifyUsersAsync` respeita preferências (channel = 'none' pula)
  - [ ] Método retorna sem erro mesmo se usuário sem preferência definida (usa default 'both')
  - [ ] DI registrada
  - [ ] `dotnet build` sem erros
- **Observações:**

---

### F6-BE-SVC-004 — Service: EmailService (Resend)
- **Status:** [❌] Bloqueado por F6-BE-SVC-003
- **Tipo:** Backend Infrastructure Service
- **Estimativa:** 5h
- **⚠️ VERIFICAR:** `grep -r "EmailService" /src/backend/` — se existir, estender ao invés de recriar
- **Arquivos:**
  - `Infrastructure/Email/EmailService.cs`
  - `Infrastructure/Email/Templates/communication_published.html`
  - `Infrastructure/Email/Templates/workflow_assigned.html`
  - `Infrastructure/Email/Templates/workflow_decision.html`
  - `Infrastructure/Email/Templates/document_uploaded.html`
  - `Infrastructure/Email/Templates/welcome_portal.html`
- **Critérios de Aceite:**
  - [ ] Verificação de existência feita
  - [ ] Templates HTML com variáveis `{{variable_name}}`
  - [ ] `EmailLog` gravado após cada envio
  - [ ] Configuração via `appsettings.json`: `Resend:ApiKey`
  - [ ] `dotnet build` sem erros
- **Observações:**

---

### F6-BE-SVC-005 — Service: WorkflowService
- **Status:** [❌] Bloqueado por F6-BE-REP-004, F6-BE-SVC-003
- **Tipo:** Backend Service
- **Estimativa:** 7h
- **Arquivos:**
  - `Application/DTOs/Workflow/CreateWorkflowRequest.cs`
  - `Application/DTOs/Workflow/WorkflowResponse.cs`
  - `Application/DTOs/Workflow/WorkflowDecisionRequest.cs`
  - `Application/Services/WorkflowService.cs`
  - `Domain/Interfaces/IWorkflowService.cs`
- **Critérios de Aceite:**
  - [ ] `CreateAsync` cria workflow + steps + notifica responsável step 1
  - [ ] `ApproveStepAsync`: avança step corretamente; último step → status 'approved'
  - [ ] `RejectStepAsync`: status 'rejected' + notifica solicitante
  - [ ] `CancelAsync`: só permite se 'pending' ou 'in_progress'
  - [ ] Testes unitários: mínimo 8 casos de teste (criar, aprovar todos steps, rejeitar, cancelar)
  - [ ] `dotnet build` sem erros
- **Observações:**

---

### F6-BE-SVC-006 — Service: InvestorPortalService
- **Status:** [❌] Bloqueado por F6-BE-SVC-001, F6-BE-SVC-002
- **Tipo:** Backend Service
- **Estimativa:** 6h
- **Arquivos:**
  - `Application/Services/InvestorPortalService.cs`
  - `Application/DTOs/Portal/InvestorSummaryResponse.cs`
  - `Application/DTOs/Portal/InvestorCapTableResponse.cs`
  - `Application/DTOs/Portal/InvestorVestingResponse.cs`
- **Critérios de Aceite:**
  - [ ] `GetSummaryAsync`: shares, % ownership, valuation atual, valor estimado
  - [ ] `GetCapTableAsync`: distribuição sem revelar outros investidores nominalmente
  - [ ] `GetVestingStatusAsync`: grants do usuário, schedule, próximo evento
  - [ ] `GetMetricsAsync`: só métricas com `is_highlight = true`
  - [ ] Todos os dados filtrados por `shareholder.user_id = currentUserId`
  - [ ] `dotnet build` sem erros
- **Observações:**

---

### F6-BE-JOB-001 — Hangfire Job: NotificationEmailDispatchJob
- **Status:** [❌] Bloqueado por F6-BE-SVC-003, F6-BE-SVC-004
- **Tipo:** Backend Job
- **Estimativa:** 4h
- **Arquivo:** `Infrastructure/Jobs/NotificationEmailDispatchJob.cs`
- **Critérios de Aceite:**
  - [ ] Job recorrente a cada 2 minutos (`RecurringJob.AddOrUpdate`)
  - [ ] Busca email_logs com status 'queued'
  - [ ] Atualiza status para 'sent' ou 'failed'
  - [ ] Retry policy: máximo 3 tentativas
  - [ ] Registrado em `Program.cs`
  - [ ] `dotnet build` sem erros
- **Observações:**

---

## 🌐 SEMANA 4 — BACKEND: CONTROLLERS (estimativa: 22h)

---

### F6-BE-API-001 — Controller: CommunicationsController
- **Status:** [❌] Bloqueado por F6-BE-SVC-001
- **Tipo:** Backend Controller
- **Estimativa:** 4h
- **Arquivo:** `API/Controllers/CommunicationsController.cs`
- **Endpoints:**
  - [ ] `GET /api/communications` — lista com filtros e paginação
  - [ ] `POST /api/communications` — criar draft
  - [ ] `GET /api/communications/{id}` — detalhe com views count
  - [ ] `PUT /api/communications/{id}` — atualizar (apenas drafts)
  - [ ] `POST /api/communications/{id}/publish` — publicar
  - [ ] `POST /api/communications/{id}/view` — registrar visualização
  - [ ] `DELETE /api/communications/{id}` — soft delete
- **Critérios de Aceite:**
  - [ ] `[Authorize]` em todos os endpoints
  - [ ] `[Authorize(Roles = "admin,founder")]` em POST/PUT/DELETE/publish
  - [ ] Swagger com `[ProducesResponseType]`
  - [ ] `dotnet build` sem erros
- **Observações:**

---

### F6-BE-API-002 — Controller: DocumentsController
- **Status:** [❌] Bloqueado por F6-BE-SVC-002
- **Tipo:** Backend Controller
- **Estimativa:** 4h
- **Arquivo:** `API/Controllers/DocumentsController.cs`
- **Endpoints:**
  - [ ] `GET /api/documents` — lista com filtros
  - [ ] `POST /api/documents/upload` — multipart upload
  - [ ] `GET /api/documents/{id}` — detalhe
  - [ ] `GET /api/documents/{id}/download-url` — URL temporária
  - [ ] `PUT /api/documents/{id}/verify` — verificar
  - [ ] `DELETE /api/documents/{id}` — soft delete
- **Critérios de Aceite:**
  - [ ] Upload com `[RequestSizeLimit(52428800)]` (50MB)
  - [ ] `dotnet build` sem erros
- **Observações:**

---

### F6-BE-API-003 — Controller: DataRoomController
- **Status:** [❌] Bloqueado por F6-BE-SVC-002
- **Tipo:** Backend Controller
- **Estimativa:** 3h
- **Arquivo:** `API/Controllers/DataRoomController.cs`
- **Endpoints:**
  - [ ] `GET /api/dataroom` — estrutura completa
  - [ ] `POST /api/dataroom/folders` — criar pasta
  - [ ] `PUT /api/dataroom/folders/{id}` — atualizar pasta
  - [ ] `DELETE /api/dataroom/folders/{id}` — deletar pasta
  - [ ] `POST /api/dataroom/folders/{id}/documents` — adicionar doc
  - [ ] `DELETE /api/dataroom/folders/{id}/documents/{docId}` — remover doc
- **Critérios de Aceite:**
  - [ ] `dotnet build` sem erros
  - [ ] Swagger documentado
- **Observações:**

---

### F6-BE-API-004 — Controller: NotificationsController
- **Status:** [❌] Bloqueado por F6-BE-SVC-003
- **Tipo:** Backend Controller
- **Estimativa:** 3h
- **Arquivo:** `API/Controllers/NotificationsController.cs`
- **Endpoints:**
  - [ ] `GET /api/notifications` — lista paginada do usuário
  - [ ] `GET /api/notifications/unread-count` — contador (< 50ms)
  - [ ] `PUT /api/notifications/{id}/read` — marcar como lida
  - [ ] `PUT /api/notifications/read-all` — marcar todas
  - [ ] `GET /api/notification-preferences` — obter preferências
  - [ ] `PUT /api/notification-preferences` — atualizar preferências
- **Critérios de Aceite:**
  - [ ] `unread-count` sem paginação, resposta rápida
  - [ ] `dotnet build` sem erros
- **Observações:**

---

### F6-BE-API-005 — Controller: WorkflowsController
- **Status:** [❌] Bloqueado por F6-BE-SVC-005
- **Tipo:** Backend Controller
- **Estimativa:** 4h
- **Arquivo:** `API/Controllers/WorkflowsController.cs`
- **Endpoints:**
  - [ ] `GET /api/workflows` — lista com filtro `?assigned_to_me=true`
  - [ ] `GET /api/workflows/{id}` — detalhe com steps + approvals
  - [ ] `POST /api/workflows` — criar workflow
  - [ ] `POST /api/workflows/{id}/steps/{stepId}/approve` — aprovar step
  - [ ] `POST /api/workflows/{id}/steps/{stepId}/reject` — rejeitar step
  - [ ] `POST /api/workflows/{id}/cancel` — cancelar
- **Critérios de Aceite:**
  - [ ] Filtro `assigned_to_me` funcional
  - [ ] `dotnet build` sem erros
- **Observações:**

---

### F6-BE-API-006 — Controller: InvestorPortalController
- **Status:** [❌] Bloqueado por F6-BE-SVC-006
- **Tipo:** Backend Controller
- **Estimativa:** 4h
- **Arquivo:** `API/Controllers/InvestorPortalController.cs`
- **Endpoints:**
  - [ ] `GET /api/portal/summary` — resumo do investimento
  - [ ] `GET /api/portal/cap-table` — cap table simplificada
  - [ ] `GET /api/portal/vesting` — status de vesting
  - [ ] `GET /api/portal/communications` — comunicações visíveis
  - [ ] `GET /api/portal/documents` — documentos do Data Room
  - [ ] `GET /api/portal/metrics` — métricas highlights
- **Critérios de Aceite:**
  - [ ] Middleware `ValidateInvestorAccessMiddleware` criado e aplicado
  - [ ] Sem dados sensíveis de outros investidores
  - [ ] `dotnet build` sem erros
- **Observações:**

---

## 🖥️ SEMANA 5 — FRONTEND (estimativa: 82h)

---

### F6-FE-TYPES-001 — TypeScript Types: Fase 6
- **Status:** [❌] Bloqueado por F6-BE-API-001~006
- **Tipo:** Frontend Types
- **Estimativa:** 2h
- **Arquivos:**
  - `types/communication.ts`
  - `types/document.ts`
  - `types/dataRoom.ts`
  - `types/notification.ts`
  - `types/workflow.ts`
  - `types/investorPortal.ts`
- **Critérios de Aceite:**
  - [ ] `npx tsc --noEmit` sem erros
  - [ ] Tipos espelham contratos de API
  - [ ] Request e Response types separados
- **Observações:**

---

### F6-FE-SVC-001 — Services: Serviços Fase 6
- **Status:** [❌] Bloqueado por F6-FE-TYPES-001
- **Tipo:** Frontend Services
- **Estimativa:** 4h
- **Arquivos:**
  - `services/communicationService.ts`
  - `services/documentService.ts`
  - `services/notificationService.ts`
  - `services/workflowService.ts`
  - `services/investorPortalService.ts`
- **Critérios de Aceite:**
  - [ ] Todos usam `api.ts` existente (não criar novo axios instance)
  - [ ] `documentService.uploadFile` usa `FormData`
  - [ ] `npx tsc --noEmit` sem erros
- **Observações:**

---

### F6-FE-HOOKS-001 — Custom Hooks: Fase 6
- **Status:** [❌] Bloqueado por F6-FE-SVC-001
- **Tipo:** Frontend Hooks
- **Estimativa:** 4h
- **Arquivos:**
  - `hooks/useCommunications.ts`
  - `hooks/useDocuments.ts`
  - `hooks/useNotifications.ts`
  - `hooks/useWorkflows.ts`
  - `hooks/useInvestorPortal.ts`
- **Critérios de Aceite:**
  - [ ] `useUnreadNotificationsCount` com `refetchInterval: 30000`
  - [ ] Mutations com `onSuccess` invalidando queries
  - [ ] `npx tsc --noEmit` sem erros
- **Observações:**

---

### F6-FE-NOTIF-001 — Componente: NotificationBell
- **Status:** [❌] Bloqueado por F6-FE-HOOKS-001
- **Tipo:** Frontend Component
- **Estimativa:** 3h
- **Arquivo:** `components/notifications/NotificationBell.tsx`
- **Critérios de Aceite:**
  - [ ] Badge com número de não lidas
  - [ ] Badge oculto quando count = 0
  - [ ] Integrado ao `Header.tsx` existente
  - [ ] Mobile responsivo
  - [ ] `npm run build` sem erros
- **Observações:**

---

### F6-FE-NOTIF-002 — Componente: NotificationDropdown
- **Status:** [❌] Bloqueado por F6-FE-NOTIF-001
- **Tipo:** Frontend Component
- **Estimativa:** 4h
- **Arquivo:** `components/notifications/NotificationDropdown.tsx`
- **Critérios de Aceite:**
  - [ ] Lista últimos 10 itens
  - [ ] Fecha ao clicar fora (click outside handler)
  - [ ] Marca como lida ao clicar no item
  - [ ] Botão "Marcar todas como lidas"
  - [ ] Estado vazio com mensagem
  - [ ] `npm run build` sem erros
- **Observações:**

---

### F6-FE-NOTIF-003 — Page: /notifications
- **Status:** [❌] Bloqueado por F6-FE-NOTIF-002
- **Tipo:** Frontend Page
- **Estimativa:** 3h
- **Arquivo:** `pages/notifications/index.tsx`
- **Critérios de Aceite:**
  - [ ] Rota `/notifications` registrada
  - [ ] Tabela paginada com filtros
  - [ ] `npm run build` sem erros
- **Observações:**

---

### F6-FE-COMM-001 — Page: /communications (admin)
- **Status:** [❌] Bloqueado por F6-FE-HOOKS-001
- **Tipo:** Frontend Page
- **Estimativa:** 5h
- **Arquivo:** `pages/communications/index.tsx`
- **Critérios de Aceite:**
  - [ ] Rota `/communications` registrada
  - [ ] Link no menu lateral (sidebar)
  - [ ] Cards/tabela com filtros
  - [ ] Pinned no topo
  - [ ] Mobile responsivo
  - [ ] `npm run build` sem erros
- **Observações:**

---

### F6-FE-COMM-002 — Componente: CommunicationFormModal
- **Status:** [❌] Bloqueado por F6-FE-COMM-001
- **Tipo:** Frontend Component
- **Estimativa:** 6h
- **Arquivo:** `components/communications/CommunicationFormModal.tsx`
- **⚠️ VERIFICAR:** `npm list @tiptap/react react-quill` — usar biblioteca de rich-text já instalada
- **Critérios de Aceite:**
  - [ ] Rich-text editor funcional
  - [ ] Validações client-side
  - [ ] Botões "Salvar draft" e "Publicar agora"
  - [ ] `npm run build` sem erros
- **Observações:**

---

### F6-FE-COMM-003 — Page: /communications/:id
- **Status:** [❌] Bloqueado por F6-FE-COMM-002
- **Tipo:** Frontend Page
- **Estimativa:** 3h
- **Arquivo:** `pages/communications/[id].tsx`
- **Critérios de Aceite:**
  - [ ] Renderiza `contentHtml` sanitizado
  - [ ] `trackView` via `useEffect` ao montar
  - [ ] `npm run build` sem erros
- **Observações:**

---

### F6-FE-DOC-001 — Page: /documents
- **Status:** [❌] Bloqueado por F6-FE-HOOKS-001
- **Tipo:** Frontend Page
- **Estimativa:** 4h
- **Arquivo:** `pages/documents/index.tsx`
- **Critérios de Aceite:**
  - [ ] Rota `/documents` registrada
  - [ ] Link no menu lateral
  - [ ] Tabela com badge de status verificação
  - [ ] `npm run build` sem erros
- **Observações:**

---

### F6-FE-DOC-002 — Componente: DocumentUploadModal
- **Status:** [❌] Bloqueado por F6-FE-DOC-001
- **Tipo:** Frontend Component
- **Estimativa:** 4h
- **Arquivo:** `components/documents/DocumentUploadModal.tsx`
- **Critérios de Aceite:**
  - [ ] Drag-and-drop funcional
  - [ ] Progress bar durante upload
  - [ ] Validação de tipo e tamanho (50MB)
  - [ ] `npm run build` sem erros
- **Observações:**

---

### F6-FE-DATAROOM-001 — Page: /dataroom
- **Status:** [❌] Bloqueado por F6-FE-DOC-002
- **Tipo:** Frontend Page
- **Estimativa:** 6h
- **Arquivo:** `pages/dataroom/index.tsx`
- **Critérios de Aceite:**
  - [ ] Layout dois painéis (árvore de pastas + documentos)
  - [ ] Criar/renomear/deletar pastas
  - [ ] Adicionar documentos às pastas
  - [ ] `npm run build` sem erros
- **Observações:**

---

### F6-FE-WF-001 — Page: /approvals (lista)
- **Status:** [❌] Bloqueado por F6-FE-HOOKS-001
- **Tipo:** Frontend Page
- **Estimativa:** 4h
- **Arquivo:** `pages/approvals/index.tsx`
- **Critérios de Aceite:**
  - [ ] Rota `/approvals` registrada
  - [ ] Link no menu lateral com badge de pendentes
  - [ ] 4 tabs: "Aguardando minha ação" | "Em andamento" | "Concluídos" | "Todos"
  - [ ] `npm run build` sem erros
- **Observações:**

---

### F6-FE-WF-002 — Page: /approvals/:id (detalhe)
- **Status:** [❌] Bloqueado por F6-FE-WF-001
- **Tipo:** Frontend Page
- **Estimativa:** 5h
- **Arquivo:** `pages/approvals/[id].tsx`
- **Critérios de Aceite:**
  - [ ] Timeline visual de steps
  - [ ] Modal de aprovação/rejeição com comentário (obrigatório na rejeição)
  - [ ] Atualiza lista após decisão
  - [ ] `npm run build` sem erros
- **Observações:**

---

### F6-FE-PORTAL-001 — Layout: PortalLayout
- **Status:** [❌] Bloqueado por F6-FE-TYPES-001
- **Tipo:** Frontend Layout
- **Estimativa:** 4h
- **Arquivo:** `layouts/PortalLayout.tsx`
- **Critérios de Aceite:**
  - [ ] Rotas `/portal/*` protegidas por middleware de role
  - [ ] Sidebar simplificada (sem menus admin)
  - [ ] Mobile responsivo
  - [ ] `npm run build` sem erros
- **Observações:**

---

### F6-FE-PORTAL-002 — Page: /portal (Dashboard)
- **Status:** [❌] Bloqueado por F6-FE-PORTAL-001
- **Tipo:** Frontend Page
- **Estimativa:** 7h
- **Arquivo:** `pages/portal/index.tsx`
- **Critérios de Aceite:**
  - [ ] Hero Banner com % participação e valor estimado
  - [ ] 4 Cards rápidos
  - [ ] Line chart de evolução do investimento (Recharts)
  - [ ] Métricas da empresa (highlights)
  - [ ] Últimos 3 comunicados
  - [ ] Loading states em todas as seções
  - [ ] Mobile responsivo
  - [ ] `npm run build` sem erros
- **Observações:**

---

### F6-FE-PORTAL-003 — Page: /portal/communications
- **Status:** [❌] Bloqueado por F6-FE-PORTAL-002
- **Tipo:** Frontend Page
- **Estimativa:** 3h
- **Arquivo:** `pages/portal/communications/index.tsx`
- **Critérios de Aceite:**
  - [ ] Usa endpoint `/api/portal/communications`
  - [ ] Badge "Não lido" visível
  - [ ] `npm run build` sem erros
- **Observações:**

---

### F6-FE-PORTAL-004 — Page: /portal/dataroom
- **Status:** [❌] Bloqueado por F6-FE-PORTAL-003
- **Tipo:** Frontend Page
- **Estimativa:** 3h
- **Arquivo:** `pages/portal/dataroom/index.tsx`
- **Critérios de Aceite:**
  - [ ] Read-only (sem upload/edição)
  - [ ] Usa endpoint `/api/portal/documents`
  - [ ] Download disponível
  - [ ] `npm run build` sem erros
- **Observações:**

---

### F6-FE-PORTAL-005 — Page: /portal/vesting
- **Status:** [❌] Bloqueado por F6-FE-PORTAL-002
- **Tipo:** Frontend Page
- **Estimativa:** 4h
- **Arquivo:** `pages/portal/vesting/index.tsx`
- **Critérios de Aceite:**
  - [ ] Progress bar por grant
  - [ ] Timeline visual do schedule
  - [ ] Próximo evento de vesting destacado
  - [ ] `npm run build` sem erros
- **Observações:**

---

### F6-FE-PREF-001 — Componente: NotificationPreferences
- **Status:** [❌] Bloqueado por F6-FE-HOOKS-001
- **Tipo:** Frontend Component
- **Estimativa:** 3h
- **Arquivo:** `components/settings/NotificationPreferences.tsx`
- **Critérios de Aceite:**
  - [ ] Integrado à página `/settings` existente
  - [ ] Toggle por tipo × canal
  - [ ] Salva preferências via PUT
  - [ ] `npm run build` sem erros
- **Observações:**

---

## 🔗 MAPA DE DEPENDÊNCIAS RESUMIDO

```
DB: 001 → 002
    001 → 003 → 004
    001 → 005 → 006
    001 → 007 (paralelo com 003-006)

BE-DOM: DB-001 → DOM-001
        DB-003 → DOM-002
        DB-005 → DOM-003
        DB-007 → DOM-004

BE-REP: DOM-001 → REP-001
        DOM-002 → REP-002
        DOM-003 → REP-003
        DOM-004 → REP-004

BE-SVC: REP-001+REP-003 → SVC-001
        REP-002         → SVC-002
        REP-003         → SVC-003
        SVC-003         → SVC-004
        REP-004+SVC-003 → SVC-005
        SVC-001+SVC-002 → SVC-006
        SVC-003+SVC-004 → JOB-001

BE-API: SVC-001 → API-001
        SVC-002 → API-002+API-003
        SVC-003 → API-004
        SVC-005 → API-005
        SVC-006 → API-006

FE:     API-001~006 → TYPES-001 → SVC-001 → HOOKS-001
        HOOKS-001   → NOTIF-001 → NOTIF-002 → NOTIF-003
        HOOKS-001   → COMM-001  → COMM-002  → COMM-003
        HOOKS-001   → DOC-001   → DOC-002   → DATAROOM-001
        HOOKS-001   → WF-001    → WF-002
        TYPES-001   → PORTAL-001 → PORTAL-002 → PORTAL-003/004/005
        HOOKS-001   → PREF-001
```

---

## 📈 HISTÓRICO DE SESSÕES

| Sessão | Data | Tarefas Iniciadas | Tarefas Concluídas | Observações |
|--------|------|-------------------|-------------------|-------------|
| #0 | 06/03/2026 | — | — | Documento criado, fase não iniciada |

---

## 🚦 BLOQUEIOS E RISCOS

| # | Risco | Probabilidade | Impacto | Mitigação |
|---|-------|---------------|---------|-----------|
| R1 | Tabela `documents` já existe na Fase 5 | Alta | Baixo | Verificar antes de criar migration; usar ALTER TABLE |
| R2 | EmailService já existe na Fase 3 | Média | Baixo | Verificar e estender; não recriar |
| R3 | Biblioteca rich-text não instalada | Média | Médio | Verificar `npm list`; instalar `@tiptap/react` se necessário |
| R4 | Numeração de migrations conflitante | Média | Alto | Verificar última migration antes de criar novas |
| R5 | InvestorPortal requer dados de múltiplas fases | Baixa | Alto | Confirmar que repositórios de Shareholders, Valuations, VestingGrants estão disponíveis |

---

*Documento mantido pelo GitHub Agent durante execução da Fase 6*  
*Referências: `FASE6_PORTAL_PLANO_EXECUCAO.md` | `PREMISSAS_DESENVOLVIMENTO.md`*
