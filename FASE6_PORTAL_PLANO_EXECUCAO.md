# Partnership Manager — Fase 6
## Plano de Execução: Portal do Investidor, Comunicações, Notificações & Workflow

**Versão:** 1.0  
**Data:** 06 de Março de 2026  
**Duração Estimada:** 5 semanas (~200h de desenvolvimento)  
**Objetivo:** Implementar o Portal do Investidor com visualização consolidada dos dados, sistema de Comunicações com rich-text e segmentação por papel, Data Room com gestão de documentos, Notificações in-app + email (Resend) e engine de Workflow de aprovações.  
**Fases Anteriores Concluídas:** Fases 1–5 (Core, Cap Table, Contratos, Vesting, Valuation + Financeiro)

---

## 📋 Sumário

1. [Avaliação do Estado Atual](#1-avaliação-do-estado-atual)
2. [Escopo da Fase 6](#2-escopo-da-fase-6)
3. [Arquitetura e Decisões Técnicas](#3-arquitetura-e-decisões-técnicas)
4. [Entidades e Modelo de Dados](#4-entidades-e-modelo-de-dados)
5. [Tarefas Atômicas — Banco de Dados](#5-tarefas-atômicas--banco-de-dados)
6. [Tarefas Atômicas — Backend](#6-tarefas-atômicas--backend)
7. [Tarefas Atômicas — Frontend](#7-tarefas-atômicas--frontend)
8. [Dependências entre Tarefas](#8-dependências-entre-tarefas)
9. [Definition of Done Global](#9-definition-of-done-global)
10. [Checklist de Entrega da Fase](#10-checklist-de-entrega-da-fase)

---

## 1. Avaliação do Estado Atual

### 1.1 O que foi construído (Fases 1–5)

| Fase | Módulos Entregues | Status |
|------|-------------------|--------|
| **Fase 1** | Auth (Firebase), Companies, Users, RBAC, Audit Log | ✅ Completo |
| **Fase 2** | Shareholders, ShareClasses, ShareTransactions (Ledger), Cap Table View, Simulador de Rodadas | ✅ Completo |
| **Fase 3** | ContractTemplates, Clause Library, Contract Builder (5 steps), ClickSign API v3.0 (Envelopes) | ✅ Completo |
| **Fase 4** | VestingPlans, VestingGrants, VestingSchedules (tempo), VestingMilestones (metas), Dashboard do beneficiário | ✅ Completo |
| **Fase 5** | Valuations (7 metodologias), FinancialPeriods, FinancialMetrics, DocumentManagement (parcial), Custom Formula Builder (NCalc2) | ✅ Completo |

### 1.2 Infraestrutura disponível para uso na Fase 6

- **Backend:** .NET 9, Dapper ORM, FluentValidation, Hangfire (jobs), Redis (cache), Firebase Auth (JWT)
- **Frontend:** React 18 + TypeScript + Vite + Tailwind CSS, React Query, Zustand
- **Banco:** MySQL 8.0 com todas as migrations anteriores aplicadas
- **Email:** Resend configurado (pode já existir serviço parcial — verificar antes de criar)
- **Storage:** Firebase Storage ou volume Docker (verificar implementação da Fase 5)
- **Padrão de migrations:** `/database/migrations/{NNN}_create_{table}.sql` com rollback

### 1.3 O que a Fase 6 precisa construir

- **Portal do Investidor:** Layout dedicado, dashboard consolidado (participação + valuation + vesting + documentos)
- **Comunicações:** CRUD admin, publicação, segmentação por role, rastreamento de leitura, editor rich-text
- **Data Room:** Organização em pastas, controle de acesso por documento, download seguro
- **Notificações in-app:** Sino no header com badge, painel de notificações, "marcar como lida"
- **Email transacional:** Templates Resend, jobs Hangfire para dispatch assíncrono
- **Workflow de Aprovações:** Engine genérica (contratos, shareholder, comunicação), steps, aprovação/rejeição com comentários

---

## 2. Escopo da Fase 6

### 2.1 Módulos

| # | Módulo | Prioridade | Complexidade |
|---|--------|------------|--------------|
| M1 | Comunicações (admin + publicação + tracking) | Alta | Média |
| M2 | Data Room (documentos + pastas + acesso) | Alta | Alta |
| M3 | Notificações in-app | Alta | Baixa |
| M4 | Email transacional (Resend + templates) | Alta | Média |
| M5 | Portal do Investidor (layout + dashboard + views) | Alta | Alta |
| M6 | Workflow de Aprovações (engine genérica) | Média | Alta |

### 2.2 Fora do Escopo (Fase 7)

- API Pública (API Keys, endpoints read-only)
- Webhooks externos
- Export PDF/Excel de relatórios
- Performance tuning / Load tests
- Documentação de usuário

---

## 3. Arquitetura e Decisões Técnicas

### 3.1 Portal do Investidor — Acesso

O Portal do Investidor é acessado por shareholders vinculados a um `user_id` no sistema. A verificação de acesso usa o role `investor`/`shareholder` do RBAC existente. O portal exibe **somente dados de leitura** da empresa da qual o usuário é sócio.

```
Rota: /portal → Layout PortalLayout.tsx (separado do AdminLayout)
Auth: Firebase JWT + middleware ValidarRolePortal
```

### 3.2 Data Room — Armazenamento

Documentos são armazenados via URL (Firebase Storage ou path local). A entidade `documents` já existe parcialmente da Fase 5 — verificar se a migration já foi aplicada antes de recriar.

### 3.3 Notificações In-App — Estratégia

- **Criação:** Síncrona, ao salvar Communication/Workflow
- **Polling:** React Query com intervalo de 30s para `/api/notifications/unread-count`
- **Leitura:** PUT individual ou bulk

### 3.4 Email — Resend + Hangfire

- `EmailService` encapsula Resend SDK
- Templates HTML armazenados em `/src/backend/PartnershipManager.Infrastructure/Email/Templates/`
- Job Hangfire `NotificationEmailJob` executa dispatch em fila

### 3.5 Workflow — Engine Genérica

O workflow é referenciado por `reference_type` (string discriminator) + `reference_id`. Isso permite que qualquer entidade dispare um workflow sem acoplamento forte.

---

## 4. Entidades e Modelo de Dados

### 4.1 Novas Entidades — Fase 6

```
communications          → Comunicados publicados pela empresa
communication_views     → Rastreamento de leitura por usuário
documents               → Documentos (já pode existir da F5 — verificar)
data_rooms              → Espaço de documentos por empresa
data_room_folders       → Pastas dentro do Data Room
data_room_documents     → Associação documento ↔ pasta (com visibilidade)
notifications           → Notificações in-app por usuário
notification_preferences → Preferências de canal por usuário
email_logs              → Log de emails enviados
workflows               → Instância de fluxo de aprovação
workflow_steps          → Etapas de cada workflow
workflow_approvals      → Registros de decisão (aprovado/rejeitado)
```

### 4.2 Enums Novos

```sql
-- Tipo de comunicação
communication_type: ENUM('announcement', 'update', 'report', 'alert', 'invitation')

-- Visibilidade de comunicação
communication_visibility: ENUM('all', 'investors', 'founders', 'employees', 'specific')

-- Tipo de notificação
notification_type: ENUM('communication_published', 'workflow_assigned', 
  'workflow_approved', 'workflow_rejected', 'contract_signed', 
  'vesting_event', 'document_uploaded', 'system')

-- Canal de notificação
notification_channel: ENUM('in_app', 'email', 'both', 'none')

-- Status de workflow
workflow_status: ENUM('pending', 'in_progress', 'approved', 'rejected', 'cancelled')

-- Tipo de workflow
workflow_type: ENUM('contract_approval', 'shareholder_change', 
  'communication_approval', 'document_verification', 'vesting_approval')

-- Decisão de workflow
workflow_decision: ENUM('approved', 'rejected', 'requested_changes')

-- Status de step
workflow_step_status: ENUM('pending', 'in_progress', 'completed', 'skipped')
```

---

## 5. Tarefas Atômicas — Banco de Dados

> **Ordem obrigatória:** Migrations devem ser executadas sequencialmente. Cada migration inclui script de rollback e query de validação.

---

### F6-DB-001 — Migration: Tabela `communications`

**Tipo:** Database Migration  
**Estimativa:** 3h  
**Dependências:** Fases 1–5 concluídas (tabela `companies` e `users` existentes)

**Arquivo a criar:** `/database/migrations/040_create_communications_table.sql`

```sql
CREATE TABLE communications (
  id            CHAR(36)      NOT NULL DEFAULT (UUID()),
  company_id    CHAR(36)      NOT NULL,
  title         VARCHAR(200)  NOT NULL,
  content       LONGTEXT      NOT NULL,
  content_html  LONGTEXT      NULL,
  summary       VARCHAR(500)  NULL,
  comm_type     ENUM('announcement','update','report','alert','invitation') NOT NULL,
  visibility    ENUM('all','investors','founders','employees','specific') NOT NULL DEFAULT 'all',
  target_roles  JSON          NULL,
  attachments   JSON          NULL,
  is_pinned     TINYINT(1)    NOT NULL DEFAULT 0,
  published_at  TIMESTAMP     NULL,
  expires_at    TIMESTAMP     NULL,
  created_by    CHAR(36)      NOT NULL,
  created_at    TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at    TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  deleted_at    TIMESTAMP     NULL,
  PRIMARY KEY (id),
  CONSTRAINT fk_comm_company FOREIGN KEY (company_id) REFERENCES companies(id),
  CONSTRAINT fk_comm_user    FOREIGN KEY (created_by)  REFERENCES users(id),
  INDEX idx_comm_company   (company_id),
  INDEX idx_comm_type      (comm_type),
  INDEX idx_comm_published (published_at),
  INDEX idx_comm_pinned    (company_id, is_pinned),
  INDEX idx_comm_deleted   (deleted_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Rollback:** `DROP TABLE IF EXISTS communications;`

**Validação:**
```sql
SELECT COUNT(*) FROM information_schema.tables 
WHERE table_schema = DATABASE() AND table_name = 'communications';
-- Esperado: 1
```

**Critérios de Done:**
- [ ] Migration aplicada sem erros
- [ ] Rollback testado e funcional
- [ ] Todos os índices criados
- [ ] Foreign keys validadas

---

### F6-DB-002 — Migration: Tabela `communication_views`

**Tipo:** Database Migration  
**Estimativa:** 2h  
**Dependências:** F6-DB-001

**Arquivo a criar:** `/database/migrations/041_create_communication_views_table.sql`

```sql
CREATE TABLE communication_views (
  id                 CHAR(36)  NOT NULL DEFAULT (UUID()),
  communication_id   CHAR(36)  NOT NULL,
  user_id            CHAR(36)  NOT NULL,
  viewed_at          TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  view_duration_secs INT       NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uk_comm_view_user (communication_id, user_id),
  CONSTRAINT fk_commview_comm FOREIGN KEY (communication_id) REFERENCES communications(id) ON DELETE CASCADE,
  CONSTRAINT fk_commview_user FOREIGN KEY (user_id)          REFERENCES users(id),
  INDEX idx_commview_comm (communication_id),
  INDEX idx_commview_user (user_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Critérios de Done:**
- [ ] Migration aplicada
- [ ] UNIQUE KEY impedindo duplicatas de visualização
- [ ] CASCADE em delete de communication

---

### F6-DB-003 — Migration: Tabela `documents` (verificar/criar/complementar)

**Tipo:** Database Migration  
**Estimativa:** 3h  
**Dependências:** Fase 5 concluída  
**⚠️ VERIFICAR PRIMEIRO:** Se a Fase 5 já criou a tabela `documents`, verificar os campos existentes e criar migration de `ALTER TABLE` apenas para campos faltantes. Não recriar se já existir.

**Arquivo a criar:** `/database/migrations/042_create_or_alter_documents_table.sql`

```sql
-- Se não existir, criar completa:
CREATE TABLE IF NOT EXISTS documents (
  id                    CHAR(36)     NOT NULL DEFAULT (UUID()),
  company_id            CHAR(36)     NOT NULL,
  name                  VARCHAR(200) NOT NULL,
  description           TEXT         NULL,
  document_type         ENUM('contract','financial_report','legal','shareholder',
                             'vesting','identity','certificate','other') NOT NULL,
  file_url              VARCHAR(500) NOT NULL,
  file_name             VARCHAR(255) NOT NULL,
  file_size_bytes       BIGINT       NOT NULL,
  mime_type             VARCHAR(100) NOT NULL,
  checksum              VARCHAR(64)  NOT NULL,
  reference_type        VARCHAR(50)  NULL,
  reference_id          CHAR(36)     NULL,
  verification_status   ENUM('pending','verified','rejected') NOT NULL DEFAULT 'pending',
  verified_at           TIMESTAMP    NULL,
  verified_by           CHAR(36)     NULL,
  visibility            ENUM('internal','investors','public') NOT NULL DEFAULT 'internal',
  uploaded_by           CHAR(36)     NOT NULL,
  created_at            TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
  deleted_at            TIMESTAMP    NULL,
  PRIMARY KEY (id),
  CONSTRAINT fk_doc_company    FOREIGN KEY (company_id)  REFERENCES companies(id),
  CONSTRAINT fk_doc_uploader   FOREIGN KEY (uploaded_by) REFERENCES users(id),
  INDEX idx_doc_company    (company_id),
  INDEX idx_doc_type       (document_type),
  INDEX idx_doc_ref        (reference_type, reference_id),
  INDEX idx_doc_visibility (visibility),
  INDEX idx_doc_deleted    (deleted_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Critérios de Done:**
- [ ] Tabela existe com todos os campos listados
- [ ] Sem duplicação de colunas
- [ ] Índices otimizados

---

### F6-DB-004 — Migration: Data Room (tabelas `data_rooms`, `data_room_folders`, `data_room_documents`)

**Tipo:** Database Migration  
**Estimativa:** 4h  
**Dependências:** F6-DB-003

**Arquivo a criar:** `/database/migrations/043_create_data_room_tables.sql`

```sql
CREATE TABLE data_rooms (
  id          CHAR(36)     NOT NULL DEFAULT (UUID()),
  company_id  CHAR(36)     NOT NULL,
  name        VARCHAR(200) NOT NULL,
  description TEXT         NULL,
  is_active   TINYINT(1)   NOT NULL DEFAULT 1,
  created_at  TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at  TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uk_dataroom_company (company_id),
  CONSTRAINT fk_dr_company FOREIGN KEY (company_id) REFERENCES companies(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE data_room_folders (
  id              CHAR(36)     NOT NULL DEFAULT (UUID()),
  data_room_id    CHAR(36)     NOT NULL,
  parent_id       CHAR(36)     NULL,
  name            VARCHAR(200) NOT NULL,
  description     TEXT         NULL,
  display_order   INT          NOT NULL DEFAULT 0,
  visibility      ENUM('internal','investors','public') NOT NULL DEFAULT 'internal',
  created_by      CHAR(36)     NOT NULL,
  created_at      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
  deleted_at      TIMESTAMP    NULL,
  PRIMARY KEY (id),
  CONSTRAINT fk_drfolder_room   FOREIGN KEY (data_room_id) REFERENCES data_rooms(id) ON DELETE CASCADE,
  CONSTRAINT fk_drfolder_parent FOREIGN KEY (parent_id)    REFERENCES data_room_folders(id),
  INDEX idx_drfolder_room   (data_room_id),
  INDEX idx_drfolder_parent (parent_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE data_room_documents (
  id            CHAR(36)   NOT NULL DEFAULT (UUID()),
  folder_id     CHAR(36)   NOT NULL,
  document_id   CHAR(36)   NOT NULL,
  display_order INT        NOT NULL DEFAULT 0,
  added_by      CHAR(36)   NOT NULL,
  added_at      TIMESTAMP  NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uk_drdoc_folder_doc (folder_id, document_id),
  CONSTRAINT fk_drdoc_folder FOREIGN KEY (folder_id)   REFERENCES data_room_folders(id) ON DELETE CASCADE,
  CONSTRAINT fk_drdoc_doc    FOREIGN KEY (document_id) REFERENCES documents(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Critérios de Done:**
- [ ] 3 tabelas criadas
- [ ] Hierarquia de pastas (self-referencing) funcional
- [ ] Constraints de visibilidade definidas

---

### F6-DB-005 — Migration: Tabelas `notifications` e `notification_preferences`

**Tipo:** Database Migration  
**Estimativa:** 3h  
**Dependências:** F6-DB-001

**Arquivo a criar:** `/database/migrations/044_create_notifications_tables.sql`

```sql
CREATE TABLE notifications (
  id                CHAR(36)     NOT NULL DEFAULT (UUID()),
  user_id           CHAR(36)     NOT NULL,
  company_id        CHAR(36)     NOT NULL,
  notification_type ENUM('communication_published','workflow_assigned',
                         'workflow_approved','workflow_rejected','contract_signed',
                         'vesting_event','document_uploaded','system') NOT NULL,
  title             VARCHAR(200) NOT NULL,
  body              TEXT         NOT NULL,
  action_url        VARCHAR(500) NULL,
  reference_type    VARCHAR(50)  NULL,
  reference_id      CHAR(36)     NULL,
  is_read           TINYINT(1)   NOT NULL DEFAULT 0,
  read_at           TIMESTAMP    NULL,
  created_at        TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  CONSTRAINT fk_notif_user    FOREIGN KEY (user_id)    REFERENCES users(id),
  CONSTRAINT fk_notif_company FOREIGN KEY (company_id) REFERENCES companies(id),
  INDEX idx_notif_user_read   (user_id, is_read),
  INDEX idx_notif_user_date   (user_id, created_at DESC),
  INDEX idx_notif_company     (company_id),
  INDEX idx_notif_ref         (reference_type, reference_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE notification_preferences (
  id                CHAR(36)  NOT NULL DEFAULT (UUID()),
  user_id           CHAR(36)  NOT NULL,
  notification_type ENUM('communication_published','workflow_assigned',
                         'workflow_approved','workflow_rejected','contract_signed',
                         'vesting_event','document_uploaded','system') NOT NULL,
  channel           ENUM('in_app','email','both','none') NOT NULL DEFAULT 'both',
  updated_at        TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uk_notifpref_user_type (user_id, notification_type),
  CONSTRAINT fk_notifpref_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Critérios de Done:**
- [ ] Tabelas criadas
- [ ] Índice composto `(user_id, is_read)` para queries de badge count
- [ ] Preferências com UNIQUE KEY por user+type

---

### F6-DB-006 — Migration: Tabela `email_logs`

**Tipo:** Database Migration  
**Estimativa:** 2h  
**Dependências:** F6-DB-005

**Arquivo a criar:** `/database/migrations/045_create_email_logs_table.sql`

```sql
CREATE TABLE email_logs (
  id                CHAR(36)     NOT NULL DEFAULT (UUID()),
  company_id        CHAR(36)     NULL,
  recipient_email   VARCHAR(255) NOT NULL,
  recipient_name    VARCHAR(200) NULL,
  subject           VARCHAR(300) NOT NULL,
  template_name     VARCHAR(100) NOT NULL,
  reference_type    VARCHAR(50)  NULL,
  reference_id      CHAR(36)     NULL,
  resend_message_id VARCHAR(100) NULL,
  status            ENUM('queued','sent','failed','bounced') NOT NULL DEFAULT 'queued',
  error_message     TEXT         NULL,
  sent_at           TIMESTAMP    NULL,
  created_at        TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  INDEX idx_emaillog_company  (company_id),
  INDEX idx_emaillog_status   (status),
  INDEX idx_emaillog_ref      (reference_type, reference_id),
  INDEX idx_emaillog_date     (created_at DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Critérios de Done:**
- [ ] Tabela criada com campos de rastreamento de status
- [ ] Índice por status para re-tentativas de falhas

---

### F6-DB-007 — Migration: Tabelas `workflows`, `workflow_steps`, `workflow_approvals`

**Tipo:** Database Migration  
**Estimativa:** 5h  
**Dependências:** F6-DB-001

**Arquivo a criar:** `/database/migrations/046_create_workflow_tables.sql`

```sql
CREATE TABLE workflows (
  id                    CHAR(36)     NOT NULL DEFAULT (UUID()),
  company_id            CHAR(36)     NOT NULL,
  workflow_type         ENUM('contract_approval','shareholder_change',
                             'communication_approval','document_verification',
                             'vesting_approval') NOT NULL,
  reference_type        VARCHAR(50)  NOT NULL,
  reference_id          CHAR(36)     NOT NULL,
  title                 VARCHAR(200) NOT NULL,
  description           TEXT         NULL,
  status                ENUM('pending','in_progress','approved','rejected','cancelled') 
                                     NOT NULL DEFAULT 'pending',
  priority              ENUM('low','medium','high','urgent') NOT NULL DEFAULT 'medium',
  current_step          INT          NOT NULL DEFAULT 1,
  total_steps           INT          NOT NULL DEFAULT 1,
  requested_by          CHAR(36)     NOT NULL,
  requested_at          TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
  due_date              TIMESTAMP    NULL,
  completed_at          TIMESTAMP    NULL,
  cancelled_at          TIMESTAMP    NULL,
  cancelled_by          CHAR(36)     NULL,
  cancellation_reason   TEXT         NULL,
  metadata              JSON         NULL,
  created_at            TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  CONSTRAINT fk_wf_company   FOREIGN KEY (company_id)   REFERENCES companies(id),
  CONSTRAINT fk_wf_requester FOREIGN KEY (requested_by) REFERENCES users(id),
  INDEX idx_wf_company    (company_id),
  INDEX idx_wf_status     (status),
  INDEX idx_wf_type       (workflow_type),
  INDEX idx_wf_ref        (reference_type, reference_id),
  INDEX idx_wf_due        (due_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE workflow_steps (
  id               CHAR(36)     NOT NULL DEFAULT (UUID()),
  workflow_id      CHAR(36)     NOT NULL,
  step_order       INT          NOT NULL,
  name             VARCHAR(100) NOT NULL,
  description      TEXT         NULL,
  step_type        ENUM('approval','review','notification','automated') NOT NULL,
  assigned_role    ENUM('admin','founder','employee','investor') NULL,
  assigned_user_id CHAR(36)     NULL,
  status           ENUM('pending','in_progress','completed','skipped') 
                               NOT NULL DEFAULT 'pending',
  is_current       TINYINT(1)   NOT NULL DEFAULT 0,
  started_at       TIMESTAMP    NULL,
  due_date         TIMESTAMP    NULL,
  completed_at     TIMESTAMP    NULL,
  completed_by     CHAR(36)     NULL,
  notes            TEXT         NULL,
  PRIMARY KEY (id),
  CONSTRAINT fk_wfstep_workflow FOREIGN KEY (workflow_id)      REFERENCES workflows(id) ON DELETE CASCADE,
  CONSTRAINT fk_wfstep_user     FOREIGN KEY (assigned_user_id) REFERENCES users(id),
  INDEX idx_wfstep_workflow (workflow_id),
  INDEX idx_wfstep_current  (workflow_id, is_current)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE workflow_approvals (
  id               CHAR(36)   NOT NULL DEFAULT (UUID()),
  workflow_step_id CHAR(36)   NOT NULL,
  user_id          CHAR(36)   NOT NULL,
  decision         ENUM('approved','rejected','requested_changes') NOT NULL,
  comments         TEXT       NULL,
  decided_at       TIMESTAMP  NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ip_address       VARCHAR(45) NULL,
  PRIMARY KEY (id),
  CONSTRAINT fk_wfapprv_step FOREIGN KEY (workflow_step_id) REFERENCES workflow_steps(id) ON DELETE CASCADE,
  CONSTRAINT fk_wfapprv_user FOREIGN KEY (user_id)          REFERENCES users(id),
  INDEX idx_wfapprv_step (workflow_step_id),
  INDEX idx_wfapprv_user (user_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Critérios de Done:**
- [ ] 3 tabelas criadas com relacionamentos corretos
- [ ] CASCADE em delete de workflow → steps → approvals
- [ ] Índice `(workflow_id, is_current)` para queries de step ativo

---

## 6. Tarefas Atômicas — Backend

> **Ordem obrigatória:** Domain Entities → Repositories → Services → Controllers  
> **Premissa:** Verificar entidades existentes antes de criar. Seguir padrão `BaseEntity`.

---

### F6-BE-DOM-001 — Domain: Entidades Communication + CommunicationView

**Tipo:** Backend Domain  
**Estimativa:** 3h  
**Dependências:** F6-DB-001, F6-DB-002

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Domain/Entities/Communication.cs
/src/backend/PartnershipManager.Domain/Entities/CommunicationView.cs
/src/backend/PartnershipManager.Domain/Enums/CommunicationType.cs
/src/backend/PartnershipManager.Domain/Enums/CommunicationVisibility.cs
/src/backend/PartnershipManager.Domain/Interfaces/ICommunicationRepository.cs
```

**Padrão de entidade:**
```csharp
public class Communication : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ContentHtml { get; set; }
    public string? Summary { get; set; }
    public CommunicationType CommType { get; set; }
    public CommunicationVisibility Visibility { get; set; }
    public string? TargetRolesJson { get; set; }
    public bool IsPinned { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Guid CreatedBy { get; set; }
    
    // Navegações
    public Company? Company { get; set; }
    public User? CreatedByUser { get; set; }
    public ICollection<CommunicationView> Views { get; set; } = new List<CommunicationView>();
}
```

**Critérios de Done:**
- [ ] `dotnet build` sem erros
- [ ] Entidades herdam de `BaseEntity`
- [ ] Enums com todos os valores da migration
- [ ] Interface do repositório com métodos CRUD + tracking de views

---

### F6-BE-DOM-002 — Domain: Entidades Document + DataRoom

**Tipo:** Backend Domain  
**Estimativa:** 3h  
**Dependências:** F6-DB-003, F6-DB-004  
**⚠️ VERIFICAR:** Se entidade `Document` já existe da Fase 5, apenas complementar campos faltantes.

**Arquivos a criar/complementar:**
```
/src/backend/PartnershipManager.Domain/Entities/Document.cs
/src/backend/PartnershipManager.Domain/Entities/DataRoom.cs
/src/backend/PartnershipManager.Domain/Entities/DataRoomFolder.cs
/src/backend/PartnershipManager.Domain/Enums/DocumentType.cs
/src/backend/PartnershipManager.Domain/Enums/DocumentVisibility.cs
/src/backend/PartnershipManager.Domain/Interfaces/IDocumentRepository.cs
/src/backend/PartnershipManager.Domain/Interfaces/IDataRoomRepository.cs
```

**Critérios de Done:**
- [ ] `dotnet build` sem erros
- [ ] Sem duplicação com entidades da Fase 5
- [ ] `DataRoomFolder` com auto-referência para `parent_id`
- [ ] Interfaces com métodos de upload, listagem por pasta, download URL

---

### F6-BE-DOM-003 — Domain: Entidades Notification + NotificationPreference

**Tipo:** Backend Domain  
**Estimativa:** 2h  
**Dependências:** F6-DB-005

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Domain/Entities/Notification.cs
/src/backend/PartnershipManager.Domain/Entities/NotificationPreference.cs
/src/backend/PartnershipManager.Domain/Enums/NotificationType.cs
/src/backend/PartnershipManager.Domain/Enums/NotificationChannel.cs
/src/backend/PartnershipManager.Domain/Interfaces/INotificationRepository.cs
```

**Critérios de Done:**
- [ ] `dotnet build` sem erros
- [ ] `INotificationRepository` com métodos: `CreateAsync`, `GetByUserAsync`, `GetUnreadCountAsync`, `MarkAsReadAsync`, `MarkAllAsReadAsync`

---

### F6-BE-DOM-004 — Domain: Entidades Workflow + WorkflowStep + WorkflowApproval

**Tipo:** Backend Domain  
**Estimativa:** 4h  
**Dependências:** F6-DB-007

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Domain/Entities/Workflow.cs
/src/backend/PartnershipManager.Domain/Entities/WorkflowStep.cs
/src/backend/PartnershipManager.Domain/Entities/WorkflowApproval.cs
/src/backend/PartnershipManager.Domain/Enums/WorkflowType.cs
/src/backend/PartnershipManager.Domain/Enums/WorkflowStatus.cs
/src/backend/PartnershipManager.Domain/Enums/WorkflowStepStatus.cs
/src/backend/PartnershipManager.Domain/Enums/WorkflowDecision.cs
/src/backend/PartnershipManager.Domain/Interfaces/IWorkflowRepository.cs
```

**Critérios de Done:**
- [ ] `dotnet build` sem erros
- [ ] `Workflow` com propriedades `CurrentStep` e `TotalSteps`
- [ ] `IWorkflowRepository` com métodos: `CreateAsync`, `GetByIdAsync`, `GetByCompanyAsync`, `AdvanceStepAsync`, `GetPendingByUserAsync`

---

### F6-BE-REP-001 — Repository: CommunicationRepository

**Tipo:** Backend Repository  
**Estimativa:** 4h  
**Dependências:** F6-BE-DOM-001

**Arquivo a criar:** `/src/backend/PartnershipManager.Infrastructure/Repositories/CommunicationRepository.cs`

**Métodos obrigatórios:**
```csharp
Task<PagedResult<Communication>> GetByCompanyAsync(Guid companyId, CommunicationFilter filter, PaginationRequest pagination);
Task<Communication?> GetByIdAsync(Guid id, Guid companyId);
Task<Guid> CreateAsync(Communication communication);
Task UpdateAsync(Communication communication);
Task SoftDeleteAsync(Guid id, Guid companyId);
Task PublishAsync(Guid id, Guid companyId);
Task TrackViewAsync(Guid communicationId, Guid userId, int? durationSecs);
Task<bool> HasViewedAsync(Guid communicationId, Guid userId);
Task<IEnumerable<Communication>> GetForRoleAsync(Guid companyId, string role, int limit);
```

**Critérios de Done:**
- [ ] Todas as queries com `company_id` (multi-tenancy)
- [ ] Todas as queries com `deleted_at IS NULL` (soft delete)
- [ ] `GetForRoleAsync` filtra por `visibility` e `target_roles` JSON
- [ ] DI registrada em `Program.cs`
- [ ] `dotnet build` sem erros

---

### F6-BE-REP-002 — Repository: DocumentRepository + DataRoomRepository

**Tipo:** Backend Repository  
**Estimativa:** 5h  
**Dependências:** F6-BE-DOM-002

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Infrastructure/Repositories/DocumentRepository.cs
/src/backend/PartnershipManager.Infrastructure/Repositories/DataRoomRepository.cs
```

**Métodos DocumentRepository:**
```csharp
Task<PagedResult<Document>> GetByCompanyAsync(Guid companyId, DocumentFilter filter, PaginationRequest pagination);
Task<Document?> GetByIdAsync(Guid id, Guid companyId);
Task<Guid> CreateAsync(Document document);
Task SoftDeleteAsync(Guid id, Guid companyId);
Task<IEnumerable<Document>> GetByReferenceAsync(string referenceType, Guid referenceId);
Task UpdateVerificationAsync(Guid id, string status, Guid verifiedBy);
```

**Métodos DataRoomRepository:**
```csharp
Task<DataRoom?> GetByCompanyAsync(Guid companyId);
Task<Guid> CreateAsync(DataRoom dataRoom);
Task<IEnumerable<DataRoomFolder>> GetFoldersAsync(Guid dataRoomId, Guid? parentId);
Task<Guid> CreateFolderAsync(DataRoomFolder folder);
Task<IEnumerable<Document>> GetDocumentsInFolderAsync(Guid folderId);
Task AddDocumentToFolderAsync(Guid folderId, Guid documentId, Guid addedBy);
Task RemoveDocumentFromFolderAsync(Guid folderId, Guid documentId);
```

**Critérios de Done:**
- [ ] Queries com `company_id` e `deleted_at IS NULL`
- [ ] DI registrada
- [ ] `dotnet build` sem erros

---

### F6-BE-REP-003 — Repository: NotificationRepository

**Tipo:** Backend Repository  
**Estimativa:** 3h  
**Dependências:** F6-BE-DOM-003

**Arquivo a criar:** `/src/backend/PartnershipManager.Infrastructure/Repositories/NotificationRepository.cs`

**Métodos:**
```csharp
Task<Guid> CreateAsync(Notification notification);
Task<PagedResult<Notification>> GetByUserAsync(Guid userId, Guid companyId, PaginationRequest pagination);
Task<int> GetUnreadCountAsync(Guid userId, Guid companyId);
Task MarkAsReadAsync(Guid id, Guid userId);
Task MarkAllAsReadAsync(Guid userId, Guid companyId);
Task<NotificationPreference?> GetPreferenceAsync(Guid userId, string notificationType);
Task UpsertPreferenceAsync(NotificationPreference preference);
Task<IEnumerable<NotificationPreference>> GetAllPreferencesAsync(Guid userId);
```

**Critérios de Done:**
- [ ] `GetUnreadCountAsync` otimizado com índice `(user_id, is_read)`
- [ ] DI registrada
- [ ] `dotnet build` sem erros

---

### F6-BE-REP-004 — Repository: WorkflowRepository

**Tipo:** Backend Repository  
**Estimativa:** 5h  
**Dependências:** F6-BE-DOM-004

**Arquivo a criar:** `/src/backend/PartnershipManager.Infrastructure/Repositories/WorkflowRepository.cs`

**Métodos:**
```csharp
Task<Guid> CreateAsync(Workflow workflow, IEnumerable<WorkflowStep> steps);
Task<Workflow?> GetByIdAsync(Guid id, Guid companyId);
Task<PagedResult<Workflow>> GetByCompanyAsync(Guid companyId, WorkflowFilter filter, PaginationRequest pagination);
Task<IEnumerable<Workflow>> GetPendingByUserAsync(Guid userId, Guid companyId);
Task<WorkflowStep?> GetCurrentStepAsync(Guid workflowId);
Task RecordApprovalAsync(WorkflowApproval approval);
Task AdvanceStepAsync(Guid workflowId, int nextStep);
Task CompleteWorkflowAsync(Guid workflowId, WorkflowStatus finalStatus);
Task CancelWorkflowAsync(Guid workflowId, Guid cancelledBy, string reason);
```

**Critérios de Done:**
- [ ] `CreateAsync` cria workflow + steps em transação
- [ ] `AdvanceStepAsync` atualiza `current_step` e `is_current` do step
- [ ] DI registrada
- [ ] `dotnet build` sem erros

---

### F6-BE-SVC-001 — Service: CommunicationService

**Tipo:** Backend Application Service  
**Estimativa:** 5h  
**Dependências:** F6-BE-REP-001, F6-BE-REP-003

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Application/DTOs/Communication/CreateCommunicationRequest.cs
/src/backend/PartnershipManager.Application/DTOs/Communication/UpdateCommunicationRequest.cs
/src/backend/PartnershipManager.Application/DTOs/Communication/CommunicationResponse.cs
/src/backend/PartnershipManager.Application/Validators/Communication/CreateCommunicationValidator.cs
/src/backend/PartnershipManager.Application/Services/CommunicationService.cs
/src/backend/PartnershipManager.Domain/Interfaces/ICommunicationService.cs
```

**Regras de negócio:**
- `Publish`: Define `published_at = NOW()`, dispara notificação para usuários elegíveis
- `Create`: Salva como draft (`published_at = NULL`)
- `GetForUser`: Filtra por `visibility` e role do usuário logado
- `TrackView`: Upsert (não duplica por usuário/comunicação)

**Critérios de Done:**
- [ ] `dotnet build` sem erros
- [ ] Validações com FluentValidation
- [ ] Mensagens de erro em `Messages.cs` (constantes)
- [ ] Testes unitários: criar, publicar, filtrar por role (mín. 5 casos)
- [ ] DI registrada

---

### F6-BE-SVC-002 — Service: DocumentService + DataRoomService

**Tipo:** Backend Application Service  
**Estimativa:** 6h  
**Dependências:** F6-BE-REP-002

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Application/DTOs/Document/UploadDocumentRequest.cs
/src/backend/PartnershipManager.Application/DTOs/Document/DocumentResponse.cs
/src/backend/PartnershipManager.Application/DTOs/DataRoom/DataRoomFolderResponse.cs
/src/backend/PartnershipManager.Application/Services/DocumentService.cs
/src/backend/PartnershipManager.Application/Services/DataRoomService.cs
```

**Regras de negócio:**
- Upload: valida MIME type, calcula checksum SHA-256, salva URL gerada pelo storage
- DataRoom: cria automaticamente um DataRoom por empresa se não existir
- Visibilidade: documentos `internal` não aparecem para investidores no portal
- Download: retorna URL temporária assinada (ou URL direta, conforme storage implementado)

**Critérios de Done:**
- [ ] `dotnet build` sem erros
- [ ] Validação de tipos de arquivo permitidos (PDF, DOCX, XLSX, PNG, JPG)
- [ ] Limite de tamanho: 50MB por arquivo
- [ ] Testes unitários: upload, listagem por pasta, visibilidade (mín. 4 casos)

---

### F6-BE-SVC-003 — Service: NotificationService

**Tipo:** Backend Application Service  
**Estimativa:** 4h  
**Dependências:** F6-BE-REP-003

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Application/DTOs/Notification/NotificationResponse.cs
/src/backend/PartnershipManager.Application/DTOs/Notification/NotificationPreferenceRequest.cs
/src/backend/PartnershipManager.Application/Services/NotificationService.cs
/src/backend/PartnershipManager.Domain/Interfaces/INotificationService.cs
```

**Método principal:**
```csharp
Task NotifyUsersAsync(
    Guid companyId,
    IEnumerable<Guid> userIds,
    NotificationType type,
    string title,
    string body,
    string? actionUrl = null,
    string? referenceType = null,
    Guid? referenceId = null
);
```

**Critérios de Done:**
- [ ] `NotifyUsersAsync` respeita preferências do usuário (`channel = 'none'` pula)
- [ ] `dotnet build` sem erros
- [ ] DI registrada

---

### F6-BE-SVC-004 — Service: EmailService (Resend)

**Tipo:** Backend Infrastructure Service  
**Estimativa:** 5h  
**Dependências:** F6-DB-006  
**⚠️ VERIFICAR:** Se `EmailService` já existe (Fase 3 pode ter implementado para contratos). Reutilizar e estender se existir.

**Arquivos a criar/complementar:**
```
/src/backend/PartnershipManager.Infrastructure/Email/EmailService.cs
/src/backend/PartnershipManager.Infrastructure/Email/Templates/communication_published.html
/src/backend/PartnershipManager.Infrastructure/Email/Templates/workflow_assigned.html
/src/backend/PartnershipManager.Infrastructure/Email/Templates/workflow_decision.html
/src/backend/PartnershipManager.Infrastructure/Email/Templates/document_uploaded.html
/src/backend/PartnershipManager.Infrastructure/Email/Templates/welcome_portal.html
```

**Interface:**
```csharp
public interface IEmailService
{
    Task<bool> SendAsync(string toEmail, string toName, string subject, 
                         string templateName, Dictionary<string, string> variables);
    Task LogAsync(EmailLog log);
}
```

**Critérios de Done:**
- [ ] Templates HTML com variáveis `{{variable_name}}`
- [ ] `EmailLog` gravado após cada envio (status sent/failed)
- [ ] `dotnet build` sem erros
- [ ] Configuração via `appsettings.json`: `Resend:ApiKey`

---

### F6-BE-SVC-005 — Service: WorkflowService

**Tipo:** Backend Application Service  
**Estimativa:** 7h  
**Dependências:** F6-BE-REP-004, F6-BE-SVC-003

**Arquivos a criar:**
```
/src/backend/PartnershipManager.Application/DTOs/Workflow/CreateWorkflowRequest.cs
/src/backend/PartnershipManager.Application/DTOs/Workflow/WorkflowResponse.cs
/src/backend/PartnershipManager.Application/DTOs/Workflow/WorkflowDecisionRequest.cs
/src/backend/PartnershipManager.Application/Services/WorkflowService.cs
/src/backend/PartnershipManager.Domain/Interfaces/IWorkflowService.cs
```

**Regras de negócio:**
- `CreateAsync`: Cria workflow + steps + notifica responsável do step 1
- `ApproveStepAsync`: Registra decisão, avança step, notifica próximo responsável; se último step → status `approved`
- `RejectStepAsync`: Registra rejeição, status `rejected`, notifica solicitante
- `CancelAsync`: Só permitido se status `pending` ou `in_progress`; notifica envolvidos

**Critérios de Done:**
- [ ] Engine de steps funcional com transição correta
- [ ] Notificações disparadas a cada transição de step
- [ ] Testes unitários: criar, aprovar, rejeitar, cancelar (mín. 8 casos)
- [ ] `dotnet build` sem erros

---

### F6-BE-SVC-006 — Service: InvestorPortalService

**Tipo:** Backend Application Service  
**Estimativa:** 6h  
**Dependências:** F6-BE-SVC-001, F6-BE-SVC-002  
**Dependências de dados:** Repositórios de Shareholders, Shares, Valuations, VestingGrants, FinancialMetrics

**Arquivo a criar:**
```
/src/backend/PartnershipManager.Application/Services/InvestorPortalService.cs
/src/backend/PartnershipManager.Application/DTOs/Portal/InvestorSummaryResponse.cs
/src/backend/PartnershipManager.Application/DTOs/Portal/InvestorCapTableResponse.cs
/src/backend/PartnershipManager.Application/DTOs/Portal/InvestorVestingResponse.cs
```

**Dados agregados por endpoint:**
- `GetSummaryAsync`: shares do investidor, % ownership, valuation atual, valor estimado da participação
- `GetCapTableAsync`: distribuição simplificada (não revela outros investidores nominalmente se `visibility = restricted`)
- `GetVestingStatusAsync`: grants do investidor, shares vestidas vs. total, próximo evento
- `GetMetricsAsync`: métricas financeiras marcadas como `is_highlight = true`

**Critérios de Done:**
- [ ] Todos os dados filtrados por `shareholder.user_id = currentUserId`
- [ ] `dotnet build` sem erros
- [ ] DI registrada

---

### F6-BE-JOB-001 — Hangfire Job: NotificationEmailDispatchJob

**Tipo:** Backend Job  
**Estimativa:** 4h  
**Dependências:** F6-BE-SVC-003, F6-BE-SVC-004

**Arquivo a criar:**
```
/src/backend/PartnershipManager.Infrastructure/Jobs/NotificationEmailDispatchJob.cs
```

**Comportamento:**
- Job recorrente a cada 2 minutos
- Busca notificações não enviadas por email (`email_logs` com status `queued`)
- Para cada notificação com preferência `email` ou `both`, envia email via `EmailService`
- Atualiza `email_logs.status = 'sent'` ou `'failed'`
- Registrado no Hangfire em `Program.cs`

**Critérios de Done:**
- [ ] Job registrado com `RecurringJob.AddOrUpdate`
- [ ] Retry policy: máximo 3 tentativas
- [ ] `dotnet build` sem erros

---

### F6-BE-API-001 — Controller: CommunicationsController

**Tipo:** Backend API Controller  
**Estimativa:** 4h  
**Dependências:** F6-BE-SVC-001

**Arquivo a criar:** `/src/backend/PartnershipManager.API/Controllers/CommunicationsController.cs`

**Endpoints:**
```
GET    /api/communications              → Lista com filtros (type, visibility, search, pagination)
POST   /api/communications              → Criar comunicação (draft)
GET    /api/communications/{id}         → Detalhe com views count
PUT    /api/communications/{id}         → Atualizar (apenas drafts)
POST   /api/communications/{id}/publish → Publicar comunicação
POST   /api/communications/{id}/view    → Registrar visualização
DELETE /api/communications/{id}         → Soft delete
```

**Critérios de Done:**
- [ ] `[Authorize]` em todos os endpoints
- [ ] `[Authorize(Roles = "admin,founder")]` em POST/PUT/DELETE/publish
- [ ] Swagger documentado com ProducesResponseType
- [ ] Paginação em GET lista

---

### F6-BE-API-002 — Controller: DocumentsController

**Tipo:** Backend API Controller  
**Estimativa:** 4h  
**Dependências:** F6-BE-SVC-002

**Arquivo a criar:** `/src/backend/PartnershipManager.API/Controllers/DocumentsController.cs`

**Endpoints:**
```
GET    /api/documents                   → Lista com filtros (type, reference, pagination)
POST   /api/documents/upload            → Upload (multipart/form-data)
GET    /api/documents/{id}              → Detalhe
GET    /api/documents/{id}/download-url → URL de download temporária
PUT    /api/documents/{id}/verify       → Verificar documento (admin)
DELETE /api/documents/{id}              → Soft delete
```

**Critérios de Done:**
- [ ] Upload aceita `multipart/form-data` com validação de tamanho (50MB)
- [ ] Download URL retorna URL segura/temporária
- [ ] `dotnet build` sem erros

---

### F6-BE-API-003 — Controller: DataRoomController

**Tipo:** Backend API Controller  
**Estimativa:** 3h  
**Dependências:** F6-BE-SVC-002

**Arquivo a criar:** `/src/backend/PartnershipManager.API/Controllers/DataRoomController.cs`

**Endpoints:**
```
GET    /api/dataroom                           → Estrutura completa (pastas + docs)
POST   /api/dataroom/folders                   → Criar pasta
PUT    /api/dataroom/folders/{id}              → Renomear/mover pasta
DELETE /api/dataroom/folders/{id}              → Deletar pasta
POST   /api/dataroom/folders/{id}/documents    → Adicionar documento à pasta
DELETE /api/dataroom/folders/{id}/documents/{docId} → Remover doc da pasta
```

**Critérios de Done:**
- [ ] `dotnet build` sem erros
- [ ] Swagger documentado

---

### F6-BE-API-004 — Controller: NotificationsController

**Tipo:** Backend API Controller  
**Estimativa:** 3h  
**Dependências:** F6-BE-SVC-003

**Arquivo a criar:** `/src/backend/PartnershipManager.API/Controllers/NotificationsController.cs`

**Endpoints:**
```
GET  /api/notifications                 → Lista paginada do usuário atual
GET  /api/notifications/unread-count    → Contagem de não lidas
PUT  /api/notifications/{id}/read       → Marcar como lida
PUT  /api/notifications/read-all        → Marcar todas como lidas
GET  /api/notification-preferences      → Preferências do usuário
PUT  /api/notification-preferences      → Atualizar preferências
```

**Critérios de Done:**
- [ ] `unread-count` deve responder em < 50ms (usa índice)
- [ ] `dotnet build` sem erros

---

### F6-BE-API-005 — Controller: WorkflowsController

**Tipo:** Backend API Controller  
**Estimativa:** 4h  
**Dependências:** F6-BE-SVC-005

**Arquivo a criar:** `/src/backend/PartnershipManager.API/Controllers/WorkflowsController.cs`

**Endpoints:**
```
GET  /api/workflows                              → Lista com filtros (status, type, assigned_to_me)
GET  /api/workflows/{id}                         → Detalhe completo (steps + approvals)
POST /api/workflows                              → Criar workflow
POST /api/workflows/{id}/steps/{stepId}/approve  → Aprovar step atual
POST /api/workflows/{id}/steps/{stepId}/reject   → Rejeitar step
POST /api/workflows/{id}/cancel                  → Cancelar workflow
```

**Critérios de Done:**
- [ ] Filtro `?assigned_to_me=true` retorna workflows com step atual atribuído ao usuário logado
- [ ] `dotnet build` sem erros

---

### F6-BE-API-006 — Controller: InvestorPortalController

**Tipo:** Backend API Controller  
**Estimativa:** 4h  
**Dependências:** F6-BE-SVC-006

**Arquivo a criar:** `/src/backend/PartnershipManager.API/Controllers/InvestorPortalController.cs`

**Endpoints:**
```
GET /api/portal/summary        → Resumo do investimento
GET /api/portal/cap-table      → Cap table simplificada
GET /api/portal/vesting        → Status de vesting do investidor
GET /api/portal/communications → Comunicações visíveis ao investidor
GET /api/portal/documents      → Documentos do Data Room (visibility = investors ou public)
GET /api/portal/metrics        → Métricas financeiras (highlights only)
```

**Middleware a criar:** `ValidateInvestorAccessMiddleware` — verifica se o usuário tem role `investor` ou é um shareholder da empresa.

**Critérios de Done:**
- [ ] Middleware de acesso aplicado em todos os endpoints
- [ ] Nenhum dado sensível de outros investidores exposto
- [ ] `dotnet build` sem erros

---

## 7. Tarefas Atômicas — Frontend

> **Ordem:** Types → Services → Hooks → Components → Pages  
> **Premissa:** Verificar hooks/services existentes antes de criar. Usar React Query + Zustand existentes.

---

### F6-FE-TYPES-001 — TypeScript Types: Módulo Fase 6

**Tipo:** Frontend Types  
**Estimativa:** 2h  
**Dependências:** F6-BE-API-001 a F6-BE-API-006 (contratos de API definidos)

**Arquivos a criar:**
```
/src/frontend/src/types/communication.ts
/src/frontend/src/types/document.ts
/src/frontend/src/types/dataRoom.ts
/src/frontend/src/types/notification.ts
/src/frontend/src/types/workflow.ts
/src/frontend/src/types/investorPortal.ts
```

**Exemplo de `communication.ts`:**
```typescript
export type CommunicationType = 'announcement' | 'update' | 'report' | 'alert' | 'invitation';
export type CommunicationVisibility = 'all' | 'investors' | 'founders' | 'employees' | 'specific';

export interface Communication {
  id: string;
  companyId: string;
  title: string;
  content: string;
  contentHtml?: string;
  summary?: string;
  commType: CommunicationType;
  visibility: CommunicationVisibility;
  targetRoles?: string[];
  isPinned: boolean;
  publishedAt?: string;
  expiresAt?: string;
  createdBy: string;
  createdAt: string;
  viewsCount?: number;
  hasViewed?: boolean;
}

export interface CreateCommunicationRequest {
  title: string;
  content: string;
  contentHtml?: string;
  summary?: string;
  commType: CommunicationType;
  visibility: CommunicationVisibility;
  targetRoles?: string[];
  isPinned: boolean;
  expiresAt?: string;
}
```

**Critérios de Done:**
- [ ] `npx tsc --noEmit` sem erros
- [ ] Tipos espelham contratos da API
- [ ] Tipos de request e response separados

---

### F6-FE-SVC-001 — Services: Serviços Fase 6

**Tipo:** Frontend Services  
**Estimativa:** 4h  
**Dependências:** F6-FE-TYPES-001

**Arquivos a criar:**
```
/src/frontend/src/services/communicationService.ts
/src/frontend/src/services/documentService.ts
/src/frontend/src/services/notificationService.ts
/src/frontend/src/services/workflowService.ts
/src/frontend/src/services/investorPortalService.ts
```

**Padrão (usar `api.ts` existente):**
```typescript
export const communicationService = {
  getAll: (params: CommunicationFilters) => api.get<PagedResult<Communication>>('/communications', { params }),
  getById: (id: string) => api.get<Communication>(`/communications/${id}`),
  create: (data: CreateCommunicationRequest) => api.post<Communication>('/communications', data),
  update: (id: string, data: UpdateCommunicationRequest) => api.put<Communication>(`/communications/${id}`, data),
  publish: (id: string) => api.post<Communication>(`/communications/${id}/publish`),
  trackView: (id: string, duration?: number) => api.post(`/communications/${id}/view`, { viewDurationSecs: duration }),
  delete: (id: string) => api.delete(`/communications/${id}`),
};
```

**Critérios de Done:**
- [ ] `npx tsc --noEmit` sem erros
- [ ] Todos os endpoints da API cobertos
- [ ] `documentService.uploadFile` usa `FormData` para multipart

---

### F6-FE-HOOKS-001 — Custom Hooks: Fase 6

**Tipo:** Frontend Hooks  
**Estimativa:** 4h  
**Dependências:** F6-FE-SVC-001

**Arquivos a criar:**
```
/src/frontend/src/hooks/useCommunications.ts
/src/frontend/src/hooks/useDocuments.ts
/src/frontend/src/hooks/useNotifications.ts
/src/frontend/src/hooks/useWorkflows.ts
/src/frontend/src/hooks/useInvestorPortal.ts
```

**Padrão (React Query):**
```typescript
export function useCommunications(filters?: CommunicationFilters) {
  return useQuery({
    queryKey: ['communications', filters],
    queryFn: () => communicationService.getAll(filters ?? {}),
  });
}

export function useUnreadNotificationsCount() {
  return useQuery({
    queryKey: ['notifications', 'unread-count'],
    queryFn: notificationService.getUnreadCount,
    refetchInterval: 30_000, // polling a cada 30s
  });
}
```

**Critérios de Done:**
- [ ] `npx tsc --noEmit` sem erros
- [ ] `useUnreadNotificationsCount` com `refetchInterval: 30000`
- [ ] Mutations com `onSuccess` invalidando queries relevantes

---

### F6-FE-NOTIF-001 — Componente: NotificationBell (header)

**Tipo:** Frontend Component  
**Estimativa:** 3h  
**Dependências:** F6-FE-HOOKS-001

**Arquivo a criar:** `/src/frontend/src/components/notifications/NotificationBell.tsx`

**Comportamento:**
- Ícone de sino no header com badge numérico vermelho (unread count)
- Ao clicar, abre `NotificationDropdown` posicionado abaixo do ícone
- Badge desaparece quando count = 0
- Integrado ao `Header.tsx` existente

**Critérios de Done:**
- [ ] Badge visível com número de não lidas
- [ ] Badge oculto quando count = 0
- [ ] `npm run build` sem erros
- [ ] Mobile responsivo

---

### F6-FE-NOTIF-002 — Componente: NotificationDropdown

**Tipo:** Frontend Component  
**Estimativa:** 4h  
**Dependências:** F6-FE-NOTIF-001

**Arquivo a criar:** `/src/frontend/src/components/notifications/NotificationDropdown.tsx`

**UI:**
- Lista dos últimos 10 itens, cada um com:
  - Ícone por tipo, título, corpo (truncado), data relativa ("há 5 min")
  - Fundo destacado para não lidas
  - Click → marca como lida + navega para `action_url`
- Botão "Ver todas" → navega para `/notifications`
- Botão "Marcar todas como lidas"
- Estado vazio: ilustração + "Nenhuma notificação"

**Critérios de Done:**
- [ ] Fecha ao clicar fora (click outside)
- [ ] Marca como lida ao clicar no item
- [ ] `npm run build` sem erros

---

### F6-FE-NOTIF-003 — Page: /notifications

**Tipo:** Frontend Page  
**Estimativa:** 3h  
**Dependências:** F6-FE-NOTIF-002

**Arquivo a criar:** `/src/frontend/src/pages/notifications/index.tsx`

**UI:**
- Tabela paginada de todas as notificações
- Filtros: tipo, lidas/não lidas
- Ação de marcar todas como lidas
- Cada linha clicável (navega para action_url)

**Critérios de Done:**
- [ ] Rota `/notifications` registrada no router
- [ ] Item de menu adicionado na navegação (ou acessível via ícone de sino)
- [ ] `npm run build` sem erros

---

### F6-FE-COMM-001 — Page: /communications (admin)

**Tipo:** Frontend Page  
**Estimativa:** 5h  
**Dependências:** F6-FE-HOOKS-001

**Arquivo a criar:** `/src/frontend/src/pages/communications/index.tsx`

**UI:**
- Header com título "Comunicações" + botão "Nova Comunicação"
- Filtros: tipo, visibilidade, status (draft/publicada), busca
- Cards ou tabela com: título, tipo (badge colorido), visibilidade, data de publicação, views count
- Comunicados fixados (pinned) no topo com indicador visual
- Ações por item: editar, publicar, deletar

**Critérios de Done:**
- [ ] Rota registrada
- [ ] Link no menu lateral (sidebar existente)
- [ ] `npm run build` sem erros
- [ ] Mobile responsivo

---

### F6-FE-COMM-002 — Componente: CommunicationFormModal

**Tipo:** Frontend Component  
**Estimativa:** 6h  
**Dependências:** F6-FE-COMM-001

**Arquivo a criar:** `/src/frontend/src/components/communications/CommunicationFormModal.tsx`

**UI:**
- Modal de criação/edição com:
  - Campo Título
  - Seletor de Tipo (announcement, update, report, alert, invitation)
  - Editor rich-text (usar `@tiptap/react` ou `react-quill` — verificar se já está instalado)
  - Resumo (textarea)
  - Seletor de Visibilidade
  - Toggle "Fixar comunicado"
  - Date picker para expiração (opcional)
- Botões: "Salvar como draft" e "Publicar agora"

**Critérios de Done:**
- [ ] Rich-text editor funcional
- [ ] Validações client-side (título obrigatório, conteúdo obrigatório)
- [ ] `npm run build` sem erros

---

### F6-FE-COMM-003 — Page: /communications/:id (detalhe)

**Tipo:** Frontend Page  
**Estimativa:** 3h  
**Dependências:** F6-FE-COMM-002

**Arquivo a criar:** `/src/frontend/src/pages/communications/[id].tsx`

**UI:**
- Exibe HTML do comunicado
- Metadados: tipo, publicado em, expira em, views
- Rastreia visualização automaticamente ao montar (`trackView`)
- Navegação: voltar para lista

**Critérios de Done:**
- [ ] `trackView` chamado via `useEffect` ao montar
- [ ] Renderiza `contentHtml` com `dangerouslySetInnerHTML` (sanitizar)
- [ ] `npm run build` sem erros

---

### F6-FE-DOC-001 — Page: /documents

**Tipo:** Frontend Page  
**Estimativa:** 4h  
**Dependências:** F6-FE-HOOKS-001

**Arquivo a criar:** `/src/frontend/src/pages/documents/index.tsx`

**UI:**
- Tabela com: nome, tipo, tamanho, enviado por, data, status verificação, visibilidade
- Filtros: tipo, visibilidade
- Botão "Upload de Documento"
- Download via ícone na tabela
- Badge de status: pendente, verificado, rejeitado

**Critérios de Done:**
- [ ] Rota registrada
- [ ] Link no menu lateral
- [ ] `npm run build` sem erros

---

### F6-FE-DOC-002 — Componente: DocumentUploadModal

**Tipo:** Frontend Component  
**Estimativa:** 4h  
**Dependências:** F6-FE-DOC-001

**Arquivo a criar:** `/src/frontend/src/components/documents/DocumentUploadModal.tsx`

**UI:**
- Drag-and-drop zone + botão de seleção de arquivo
- Preview do arquivo selecionado (nome, tamanho)
- Campos: nome, descrição, tipo, visibilidade, referência (optional)
- Progress bar de upload
- Validação: tipos permitidos, tamanho máximo 50MB

**Critérios de Done:**
- [ ] Drag-and-drop funcional
- [ ] Progress bar durante upload
- [ ] Mensagem de erro clara para tipos/tamanho inválidos
- [ ] `npm run build` sem erros

---

### F6-FE-DATAROOM-001 — Page: /dataroom

**Tipo:** Frontend Page  
**Estimativa:** 6h  
**Dependências:** F6-FE-DOC-002

**Arquivo a criar:** `/src/frontend/src/pages/dataroom/index.tsx`

**UI:**
- Layout de dois painéis: árvore de pastas (esquerda) + lista de documentos (direita)
- Criar pasta, renomear, deletar (drag-and-drop optional)
- Adicionar documentos existentes a uma pasta
- Badge de visibilidade por pasta
- Estado vazio: "Nenhum documento nesta pasta"

**Critérios de Done:**
- [ ] Navegação entre pastas funcional
- [ ] Criar/remover pastas
- [ ] Adicionar documentos às pastas
- [ ] `npm run build` sem erros

---

### F6-FE-WF-001 — Page: /approvals

**Tipo:** Frontend Page  
**Estimativa:** 4h  
**Dependências:** F6-FE-HOOKS-001

**Arquivo a criar:** `/src/frontend/src/pages/approvals/index.tsx`

**UI:**
- Tabs: "Aguardando minha ação" | "Em andamento" | "Concluídos" | "Todos"
- Cards por workflow: título, tipo (badge), prioridade (cor), step atual/total, prazo
- Indicador de urgência para workflows com `due_date` próximo

**Critérios de Done:**
- [ ] Rota registrada
- [ ] Link no menu lateral (com badge de pendentes)
- [ ] `npm run build` sem erros

---

### F6-FE-WF-002 — Page: /approvals/:id (detalhe do workflow)

**Tipo:** Frontend Page  
**Estimativa:** 5h  
**Dependências:** F6-FE-WF-001

**Arquivo a criar:** `/src/frontend/src/pages/approvals/[id].tsx`

**UI:**
- Header: título, tipo, status, prioridade, solicitante
- Timeline vertical de steps com status visual (✓ concluído, ⌛ aguardando, ○ pendente)
- Para step atual atribuído ao usuário: botões "Aprovar" e "Rejeitar"
- `ApprovalModal`: textarea para comentários + confirmação
- Histórico de decisões com comentários

**Critérios de Done:**
- [ ] Timeline visual de steps
- [ ] Modal de aprovação/rejeição com comentário obrigatório para rejeição
- [ ] Atualiza lista após decisão (invalidate query)
- [ ] `npm run build` sem erros

---

### F6-FE-PORTAL-001 — Layout: PortalLayout

**Tipo:** Frontend Layout  
**Estimativa:** 4h  
**Dependências:** F6-FE-TYPES-001  
**⚠️ IMPORTANTE:** Layout separado do `AdminLayout`. Rotas `/portal/*` usam este layout.

**Arquivo a criar:** `/src/frontend/src/layouts/PortalLayout.tsx`

**UI:**
- Sidebar simplificada: Dashboard, Minha Participação, Vesting, Comunicações, Data Room
- Header com nome da empresa + avatar do usuário
- Design limpo, voltado ao investidor (sem menus administrativos)
- Breadcrumb de navegação

**Critérios de Done:**
- [ ] Rotas `/portal/*` protegidas por middleware de role `investor`/`shareholder`
- [ ] Roteamento configurado no `App.tsx`
- [ ] `npm run build` sem erros
- [ ] Mobile responsivo

---

### F6-FE-PORTAL-002 — Page: /portal (Dashboard do Investidor)

**Tipo:** Frontend Page  
**Estimativa:** 7h  
**Dependências:** F6-FE-PORTAL-001

**Arquivo a criar:** `/src/frontend/src/pages/portal/index.tsx`

**UI — Seções:**
1. **Hero Banner:** Nome do investidor, % participação atual, valor estimado da participação (com badge de variação vs. valuation anterior)
2. **Cards Rápidos:** Shares totais | Valor de valuation | Último vesting | Documentos disponíveis
3. **Gráfico de Evolução:** Line chart do valor estimado da participação ao longo das valuations históricas (Recharts)
4. **Métricas da Empresa:** Cards com as métricas `is_highlight = true` do último período financeiro (MRR, Runway, etc.)
5. **Últimos Comunicados:** Lista dos 3 mais recentes com badge "Novo" para não lidos
6. **Próximos Eventos de Vesting:** Se aplicável

**Critérios de Done:**
- [ ] Line chart funcional com dados reais
- [ ] Dados carregados via `useInvestorPortal`
- [ ] Loading states em todas as seções
- [ ] `npm run build` sem erros
- [ ] Mobile responsivo

---

### F6-FE-PORTAL-003 — Page: /portal/communications

**Tipo:** Frontend Page  
**Estimativa:** 3h  
**Dependências:** F6-FE-PORTAL-002

**Arquivo a criar:** `/src/frontend/src/pages/portal/communications/index.tsx`

**UI:**
- Lista de comunicados publicados e visíveis ao investidor
- Cards com título, tipo, resumo, data, badge "Não lido"
- Click → detalhe do comunicado (rastreia view)
- Comunicados fixados no topo

**Critérios de Done:**
- [ ] Usa endpoint `/api/portal/communications` (não o admin)
- [ ] `npm run build` sem erros

---

### F6-FE-PORTAL-004 — Page: /portal/dataroom

**Tipo:** Frontend Page  
**Estimativa:** 3h  
**Dependências:** F6-FE-PORTAL-003

**Arquivo a criar:** `/src/frontend/src/pages/portal/dataroom/index.tsx`

**UI:**
- Versão read-only do Data Room (apenas documentos com `visibility = investors` ou `public`)
- Navegação por pastas (sem criação/edição)
- Download disponível para todos os documentos listados
- Estado vazio: "Nenhum documento compartilhado"

**Critérios de Done:**
- [ ] Usa endpoint `/api/portal/documents`
- [ ] Sem opções de upload/edição
- [ ] `npm run build` sem erros

---

### F6-FE-PORTAL-005 — Page: /portal/vesting

**Tipo:** Frontend Page  
**Estimativa:** 4h  
**Dependências:** F6-FE-PORTAL-002

**Arquivo a criar:** `/src/frontend/src/pages/portal/vesting/index.tsx`

**UI:**
- Resumo de cada grant do investidor:
  - Progress bar (vestido vs. total)
  - Próximo evento de vesting (data + quantidade)
  - Status das milestones (se aplicável)
- Timeline visual do schedule de vesting
- Estado vazio: "Nenhum programa de vesting ativo"

**Critérios de Done:**
- [ ] Usa endpoint `/api/portal/vesting`
- [ ] Progress bar animada
- [ ] `npm run build` sem erros

---

### F6-FE-PREF-001 — Componente: NotificationPreferences (Settings)

**Tipo:** Frontend Component  
**Estimativa:** 3h  
**Dependências:** F6-FE-HOOKS-001

**Arquivo a criar:** `/src/frontend/src/components/settings/NotificationPreferences.tsx`

**UI:**
- Tabela de tipos de notificação × canais (In-App, Email)
- Toggle por célula
- Salvar automaticamente por row (debounced) ou botão global "Salvar"
- Integrado à página `/settings` existente (adicionar nova seção/tab)

**Critérios de Done:**
- [ ] Integrado na página de settings
- [ ] Salva preferências via PUT endpoint
- [ ] `npm run build` sem erros

---

## 8. Dependências entre Tarefas

```
BANCO DE DADOS (executar nesta ordem):
F6-DB-001 → F6-DB-002
F6-DB-001 → F6-DB-003 → F6-DB-004
F6-DB-004 → F6-DB-005 → F6-DB-006
F6-DB-001 → F6-DB-007

BACKEND DOMAIN:
F6-DB-001 → F6-BE-DOM-001
F6-DB-003 → F6-BE-DOM-002
F6-DB-005 → F6-BE-DOM-003
F6-DB-007 → F6-BE-DOM-004

BACKEND REPOSITORIES (após Domain):
F6-BE-DOM-001 → F6-BE-REP-001
F6-BE-DOM-002 → F6-BE-REP-002
F6-BE-DOM-003 → F6-BE-REP-003
F6-BE-DOM-004 → F6-BE-REP-004

BACKEND SERVICES (após Repositories):
F6-BE-REP-001 + F6-BE-REP-003 → F6-BE-SVC-001
F6-BE-REP-002                  → F6-BE-SVC-002
F6-BE-REP-003                  → F6-BE-SVC-003
F6-BE-SVC-003                  → F6-BE-SVC-004
F6-BE-REP-004 + F6-BE-SVC-003  → F6-BE-SVC-005
F6-BE-SVC-001 + F6-BE-SVC-002  → F6-BE-SVC-006
F6-BE-SVC-003 + F6-BE-SVC-004  → F6-BE-JOB-001

BACKEND CONTROLLERS (após Services):
F6-BE-SVC-001 → F6-BE-API-001
F6-BE-SVC-002 → F6-BE-API-002 + F6-BE-API-003
F6-BE-SVC-003 → F6-BE-API-004
F6-BE-SVC-005 → F6-BE-API-005
F6-BE-SVC-006 → F6-BE-API-006

FRONTEND (após APIs definidas):
F6-BE-API-001~006   → F6-FE-TYPES-001
F6-FE-TYPES-001     → F6-FE-SVC-001
F6-FE-SVC-001       → F6-FE-HOOKS-001
F6-FE-HOOKS-001     → F6-FE-NOTIF-001 → F6-FE-NOTIF-002 → F6-FE-NOTIF-003
F6-FE-HOOKS-001     → F6-FE-COMM-001  → F6-FE-COMM-002  → F6-FE-COMM-003
F6-FE-HOOKS-001     → F6-FE-DOC-001   → F6-FE-DOC-002   → F6-FE-DATAROOM-001
F6-FE-HOOKS-001     → F6-FE-WF-001    → F6-FE-WF-002
F6-FE-NOTIF-001     → Header (integração)
F6-FE-TYPES-001     → F6-FE-PORTAL-001 → F6-FE-PORTAL-002 → F6-FE-PORTAL-003
                                       → F6-FE-PORTAL-004
                                       → F6-FE-PORTAL-005
F6-FE-HOOKS-001     → F6-FE-PREF-001
```

### Paralelismo possível após DB estar pronto

| Paralelo A | Paralelo B |
|------------|------------|
| F6-BE-DOM-001 + F6-BE-DOM-002 | F6-BE-DOM-003 + F6-BE-DOM-004 |
| F6-BE-REP-001 | F6-BE-REP-002 + F6-BE-REP-003 + F6-BE-REP-004 |
| F6-BE-SVC-001 | F6-BE-SVC-002 + F6-BE-SVC-003 |
| F6-FE-COMM-001~003 | F6-FE-WF-001~002 |
| F6-FE-DOC-001~002 + F6-FE-DATAROOM-001 | F6-FE-PORTAL-001~005 |

---

## 9. Definition of Done Global

Uma tarefa é considerada **Done** quando:

### Backend
- [ ] `dotnet build` — zero erros, zero warnings
- [ ] `dotnet test` — todos os testes da fase passando
- [ ] Cobertura mínima de 80% nos Services
- [ ] FluentValidation implementado em todos os Requests
- [ ] Todas as queries com `company_id` (multi-tenancy)
- [ ] Todas as queries com `deleted_at IS NULL` (soft delete)
- [ ] DI registrada em `Program.cs` ou `ServiceCollectionExtensions.cs`
- [ ] Swagger documentado (`[ProducesResponseType]`)
- [ ] Controller com `[Authorize]`

### Frontend
- [ ] `npm run build` — zero erros
- [ ] `npm run lint` — zero warnings
- [ ] `npx tsc --noEmit` — zero erros
- [ ] Loading states em todas as chamadas assíncronas
- [ ] Error states com mensagem amigável
- [ ] Mobile responsivo (testar em 375px e 768px)
- [ ] React Query com invalidação de cache após mutations

### Banco de Dados
- [ ] Migration aplicada com sucesso
- [ ] Rollback testado e funcional
- [ ] Índices criados conforme especificação
- [ ] Foreign keys com comportamento de cascade definido

---

## 10. Checklist de Entrega da Fase

```
┌─────────────────────────────────────────────────────────────────┐
│                    ENTREGA FASE 6                               │
├─────────────────────────────────────────────────────────────────┤
│ BANCO DE DADOS                                                  │
│ □ 7 migrations aplicadas (040 a 046)                           │
│ □ Todos os rollbacks testados                                   │
│ □ Índices validados via EXPLAIN                                 │
├─────────────────────────────────────────────────────────────────┤
│ BACKEND                                                         │
│ □ 4 entidades de Domain criadas                                 │
│ □ 4 repositories implementados                                  │
│ □ 6 services com regras de negócio                              │
│ □ 1 Hangfire job configurado                                    │
│ □ 6 controllers com todos os endpoints                          │
│ □ dotnet build limpo                                            │
│ □ Cobertura de testes ≥ 80% nos services                       │
├─────────────────────────────────────────────────────────────────┤
│ FRONTEND                                                        │
│ □ Types definidos para todos os módulos                         │
│ □ Services e hooks implementados                                │
│ □ NotificationBell integrado no Header                          │
│ □ Módulo Comunicações completo (CRUD admin)                     │
│ □ Módulo Documentos + Data Room completo                        │
│ □ Módulo Workflows/Aprovações completo                          │
│ □ Portal do Investidor completo (5 páginas)                     │
│ □ npm run build limpo                                           │
│ □ Mobile responsivo validado                                    │
├─────────────────────────────────────────────────────────────────┤
│ INTEGRAÇÃO                                                      │
│ □ Email Resend funcional com 5 templates                        │
│ □ Notificações in-app funcionando em tempo real (polling)       │
│ □ Portal acessível por usuário com role investor                │
│ □ Workflow completo: criar → aprovar → concluir                 │
│ □ Data Room com hierarquia de pastas                            │
└─────────────────────────────────────────────────────────────────┘
```

---

## Resumo de Esforço

| Área | Tarefas | Estimativa Total |
|------|---------|-----------------|
| Banco de Dados | 7 | ~22h |
| Backend Domain | 4 | ~12h |
| Backend Repositories | 4 | ~17h |
| Backend Services | 6 + 1 job | ~37h |
| Backend Controllers | 6 | ~22h |
| Frontend Types + Services + Hooks | 3 | ~10h |
| Frontend Components + Pages | 16 | ~72h |
| **Total** | **47 tarefas** | **~192h** |

---

*Documento gerado em 06/03/2026 para execução por GitHub Agent (Claude Sonnet)*  
*Referência: `PREMISSAS_DESENVOLVIMENTO.md` — sempre verificar antes de criar*
