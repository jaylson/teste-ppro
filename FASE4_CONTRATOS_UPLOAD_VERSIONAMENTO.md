# Fase 4 — Contratos: Upload DOCX, Versionamento e Controle de Formato

**Data de criação:** 23/02/2026  
**Status:** Implementação concluída — aguardando testes e validação

---

## Controle de Tarefas

> Legenda: ⬜ Não iniciado · 🔄 Em andamento · ✅ Concluído · ❌ Bloqueado

| # | Etapa | Status | Responsável | Data início | Data conclusão | Observações |
|---|-------|--------|-------------|-------------|----------------|-------------|
| 1 | Migration SQL — tabela `contract_versions` | ✅ Concluído | Copilot | 23/02/2026 | 23/02/2026 | `019_create_contract_versions.sql` criado |
| 2 | Domínio — entidade `ContractVersion` | ✅ Concluído | Copilot | 23/02/2026 | 23/02/2026 | `ContractVersion.cs` criado; `Contract.cs` atualizado |
| 3 | Infraestrutura — `IContractStorageService` + DbContext | ✅ Concluído | Copilot | 23/02/2026 | 23/02/2026 | Interface + impl + repo criados; DI registrado |
| 4 | Application — DTOs e Commands | ✅ Concluído | Copilot | 23/02/2026 | 23/02/2026 | DTOs + ContractVersionService criados; UpdateAsync atualizado |
| 5 | API — Novos Endpoints no ContractsController | ✅ Concluído | Copilot | 23/02/2026 | 23/02/2026 | 4 endpoints + builder integrado com RecordBuilderVersionAsync |
| 6 | Frontend — Modificar Step 1 do Wizard (upload DOCX) | ✅ Concluído | Copilot | 23/02/2026 | 23/02/2026 | Toggle "Usar Builder"/"Upload DOCX" + dropzone; `builder.tsx` actualizado |
| 7 | Frontend — Componente `ContractVersionHistory` | ✅ Concluído | Copilot | 23/02/2026 | 23/02/2026 | `ContractVersionHistory.tsx` criado; exibido em `ContractDetails` |
| 8 | Frontend — Componente `UploadNewVersion` (modal) | ✅ Concluído | Copilot | 23/02/2026 | 23/02/2026 | `UploadNewVersion.tsx` criado; integrado em `ContractActions` |
| 9 | Frontend — Bloquear edição e integrar componentes | ✅ Concluído | Copilot | 23/02/2026 | 23/02/2026 | "Editar" desabilitado pós-Draft; botão "Upload nova versão" adicionado |
| 10 | Testes e ajustes finais (end-to-end) | ⬜ Não iniciado | — | — | — | Depende de todas as etapas anteriores |

### Resumo de progresso
- **Total de etapas:** 10
- **Concluídas:** 9 / 10
- **Em andamento:** 0 / 10
- **Não iniciadas:** 1 / 10

### Checklist de arquivos entregues

#### Backend
- [x] `docker/mysql/migrations/019_create_contract_versions.sql`
- [x] `src/backend/PartnershipManager.Domain/Entities/Contract/ContractVersion.cs`
- [x] `src/backend/PartnershipManager.Domain/Entities/Contract/Contract.cs` *(modificado)*
- [x] `src/backend/PartnershipManager.Domain/Interfaces/Services/IContractStorageService.cs`
- [x] `src/backend/PartnershipManager.Infrastructure/Services/ContractStorageService.cs`
- [x] DbContext *(modificado — +DbSet\<ContractVersion\>)*
- [x] `src/backend/PartnershipManager.Application/DTOs/ContractVersionDto.cs`
- [x] `src/backend/PartnershipManager.Application/Features/Contracts/UploadContractVersion/UploadContractVersionCommand.cs`
- [x] `src/backend/PartnershipManager.Application/Features/Contracts/UploadContractVersion/UploadContractVersionHandler.cs`
- [x] `src/backend/PartnershipManager.Application/Features/Contracts/CreateContractFromUpload/CreateContractFromUploadCommand.cs`
- [x] `src/backend/PartnershipManager.Application/Features/Contracts/CreateContractFromUpload/CreateContractFromUploadHandler.cs`
- [x] Handler do builder *(modificado — cria ContractVersion v1 ao gerar)*
- [x] `src/backend/PartnershipManager.API/Controllers/ContractsController.cs` *(modificado — 4 novos endpoints)*

#### Frontend
- [ ] `src/frontend/src/components/contracts/builder/Step1SelectType.tsx` *(modificado — toggle Builder/Upload)*
- [ ] `src/frontend/src/components/contracts/ContractVersionHistory.tsx`
- [ ] `src/frontend/src/components/contracts/UploadNewVersion.tsx`
- [ ] `src/frontend/src/components/contracts/ContractActions.tsx` *(modificado — bloquear editar + botão nova versão)*
- [ ] `src/frontend/src/components/contracts/ContractDetails.tsx` *(modificado — incluir ContractVersionHistory)*

---

## Contexto e Decisões de Produto

### Funcionalidades a implementar
1. **Upload de DOCX** — o usuário pode criar um contrato fazendo upload direto de um `.docx`, sem usar o builder
2. **Versionamento de contratos** — toda criação (builder ou upload) e qualquer novo upload geram uma nova versão com histórico completo e download disponível
3. **Controle de formato pós-geração** — após o contrato sair do status `Draft`, o botão "Editar" fica desabilitado; a única forma de atualizar é via "Upload nova versão"

### Decisões definidas
| Pergunta | Decisão |
|----------|---------|
| Onde fica o upload DOCX? | Como alternativa no **Step 1 do wizard** (toggle "Usar Builder" / "Upload de DOCX") |
| Versões anteriores ficam acessíveis? | Sim — **histórico completo** com download de todas as versões |
| O builder deve gerar DOCX? | Não — o builder continua gerando **PDF**; DOCX é exclusivamente via upload externo |
| O que aparece no lugar de "Editar" após gerar? | Botão "Editar" **desabilitado com tooltip** + botão **"Upload nova versão"** |

---

## Arquitetura das Mudanças

### Novo fluxo — Upload via Step 1

```
Step 1 (toggle)
 ├── "Usar Builder" → fluxo atual (Steps 1→2→3→4→5)
 └── "Upload de DOCX"
      ├── Formulário: Título, Empresa, Arquivo .docx
      └── POST /api/contracts/upload
           → Cria Contract (status Draft) + ContractVersion v1 (source=Upload)
           → Redireciona para ContractDetails
```

### Novo fluxo — Versionamento

```
Qualquer contrato criado ganha versão automática:
  - Builder finaliza (generate) → ContractVersion v1 (source=Builder, fileType=Pdf)
  - Upload via Step 1         → ContractVersion v1 (source=Upload, fileType=Docx)
  - Upload nova versão        → ContractVersion v(n+1) (source=Upload, fileType=Docx)

GET /api/contracts/{id}/versions           → lista histórico
GET /api/contracts/{id}/versions/{vid}/download → download de versão específica
```

### Bloqueio pós-geração

```
Contract.Status == Draft    → botão "Editar" habilitado (comportamento atual)
Contract.Status != Draft    → botão "Editar" desabilitado + tooltip explicativo
                            → botão "Upload nova versão" sempre visível (exceto Cancelled/Expired)
```

---

## Etapas de Implementação

### ETAPA 1 — Migration SQL: tabela `contract_versions`
**Arquivo:** `docker/mysql/migrations/019_create_contract_versions.sql`

**O que fazer:**
- Criar tabela `contract_versions` com os campos:
  - `id` CHAR(36) PK
  - `contract_id` CHAR(36) FK → `contracts.id`
  - `version_number` INT NOT NULL
  - `file_path` VARCHAR(500) NOT NULL
  - `file_size` BIGINT
  - `file_hash` VARCHAR(64) — SHA256
  - `file_type` ENUM('pdf','docx') NOT NULL
  - `source` ENUM('builder','upload') NOT NULL
  - `notes` TEXT
  - `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP
  - `created_by` VARCHAR(255)
  - UNIQUE KEY `uk_contract_version` (`contract_id`, `version_number`)
- Alterar tabela `contracts`: adicionar coluna `current_version_number INT NOT NULL DEFAULT 0`

**Verificação:** rodar migration no banco, confirmar schema com `DESCRIBE contract_versions;`

---

### ETAPA 2 — Domínio: entidade `ContractVersion`
**Arquivos:**
- Criar: `src/backend/PartnershipManager.Domain/Entities/Contract/ContractVersion.cs`
- Atualizar: `src/backend/PartnershipManager.Domain/Entities/Contract/Contract.cs`

**O que fazer em `ContractVersion.cs`:**
```csharp
public class ContractVersion
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public int VersionNumber { get; set; }
    public string FilePath { get; set; }
    public long? FileSize { get; set; }
    public string? FileHash { get; set; }
    public DocumentFileType FileType { get; set; }    // Pdf | Docx
    public ContractVersionSource Source { get; set; } // Builder | Upload
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }

    // Navegação
    public Contract Contract { get; set; }
}

public enum DocumentFileType { Pdf, Docx }
public enum ContractVersionSource { Builder, Upload }
```

**O que fazer em `Contract.cs`:**
- Adicionar propriedade `public int CurrentVersionNumber { get; set; }`
- Adicionar coleção `public List<ContractVersion> Versions { get; set; } = new();`

**Verificação:** `dotnet build` sem erros no projeto Domain

---

### ETAPA 3 — Infraestrutura: `IContractStorageService` + DbContext
**Arquivos:**
- Criar: `src/backend/PartnershipManager.Domain/Interfaces/Services/IContractStorageService.cs`
- Criar: `src/backend/PartnershipManager.Infrastructure/Services/ContractStorageService.cs`
- Atualizar: DbContext para incluir `DbSet<ContractVersion>` e mapeamento

**Interface `IContractStorageService`:**
```csharp
public interface IContractStorageService
{
    Task<string> SaveDocxAsync(Guid contractId, int versionNumber, Stream stream, string originalFileName);
    Task<Stream> GetFileStreamAsync(string filePath);
    Task<string> ComputeHashAsync(Stream stream);
    string GetContentType(DocumentFileType fileType);
}
```

**Implementação:** salva em `/contracts/{contractId}/v{versionNumber}.docx` (mesmo padrão local do PDF atual)

**DbContext:** adicionar `DbSet<ContractVersion> ContractVersions { get; set; }` e configuração:
```csharp
entity.ToTable("contract_versions");
entity.HasKey(e => e.Id);
entity.HasOne(e => e.Contract)
      .WithMany(c => c.Versions)
      .HasForeignKey(e => e.ContractId);
entity.HasIndex(e => new { e.ContractId, e.VersionNumber }).IsUnique();
```

**Verificação:** `dotnet build` sem erros no projeto Infrastructure

---

### ETAPA 4 — Application: DTOs e Commands
**Arquivos a criar em `src/backend/PartnershipManager.Application/`:**
- `DTOs/ContractVersionDto.cs`
- `Features/Contracts/UploadContractVersion/UploadContractVersionCommand.cs`
- `Features/Contracts/UploadContractVersion/UploadContractVersionHandler.cs`
- `Features/Contracts/CreateContractFromUpload/CreateContractFromUploadCommand.cs`
- `Features/Contracts/CreateContractFromUpload/CreateContractFromUploadHandler.cs`

**`ContractVersionDto`:**
```csharp
public class ContractVersionDto
{
    public Guid Id { get; set; }
    public int VersionNumber { get; set; }
    public string FileType { get; set; }   // "pdf" | "docx"
    public string Source { get; set; }     // "builder" | "upload"
    public long? FileSize { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
```

**`CreateContractFromUploadCommand`:**
```csharp
public record CreateContractFromUploadCommand(
    Guid ClientId,
    Guid CompanyId,
    string Title,
    string? Description,
    Stream FileStream,
    string FileName,
    string? CreatedBy
) : IRequest<ContractDto>;
```

**`UploadContractVersionCommand`:**
```csharp
public record UploadContractVersionCommand(
    Guid ContractId,
    Stream FileStream,
    string FileName,
    string? Notes,
    string? CreatedBy
) : IRequest<ContractVersionDto>;
```

**Lógica dos Handlers:**
- `CreateContractFromUploadHandler`: cria `Contract` (status Draft), chama `IContractStorageService.SaveDocxAsync`, cria `ContractVersion` (v1, source=Upload), salva tudo
- `UploadContractVersionHandler`: busca contrato, incrementa `CurrentVersionNumber`, chama storage service, cria nova `ContractVersion`, salva

**Atualizar** o handler existente `GenerateContractHandler` (ou equivalente no `ContractBuilderController`): após gerar PDF, criar `ContractVersion` (v1, source=Builder, fileType=Pdf) e definir `CurrentVersionNumber = 1`

**Verificação:** `dotnet build` sem erros no projeto Application

---

### ETAPA 5 — API: Novos Endpoints no ContractsController
**Arquivo:** `src/backend/PartnershipManager.API/Controllers/ContractsController.cs`

**Adicionar:**
```
POST   /api/contracts/upload                              → CreateContractFromUpload
POST   /api/contracts/{id}/upload-version                 → UploadContractVersion
GET    /api/contracts/{id}/versions                       → lista ContractVersionDtos
GET    /api/contracts/{id}/versions/{versionId}/download  → stream do arquivo
```

**Endpoint de upload** (multipart/form-data):
```csharp
[HttpPost("upload")]
[Consumes("multipart/form-data")]
public async Task<IActionResult> UploadContract([FromForm] UploadContractRequest request)
```

**Endpoint de download** — verificar extensão do arquivo e retornar com content-type correto:
- `application/pdf` para `.pdf`
- `application/vnd.openxmlformats-officedocument.wordprocessingml.document` para `.docx`

**Registrar** `IContractStorageService` no DI (em `Extensions/ServiceCollectionExtensions.cs` ou equivalente):
```csharp
services.AddScoped<IContractStorageService, ContractStorageService>();
```

**Verificação:** `dotnet build` completo na solution; testar endpoints via curl ou Swagger

---

### ETAPA 6 — Frontend: Modificar Step 1 do Wizard
**Arquivo:** `src/frontend/src/components/contracts/builder/Step1SelectType.tsx`

**O que adicionar:**
- Toggle/tabs no topo da tela: **"Usar Builder"** | **"Upload de DOCX"**
- Quando "Upload de DOCX" selecionado, renderizar formulário:
  - Input: Título do contrato (obrigatório)
  - Select: Empresa (obrigatório, usa CompanyId)
  - Textarea: Descrição (opcional)
  - Dropzone: aceita apenas `.docx`, limite 10MB, com preview do nome do arquivo
  - Botão "Criar Contrato" (disabled enquanto campos obrigatórios não preenchidos)
- Ao submeter: `POST /api/contracts/upload` com `multipart/form-data`
- Em caso de sucesso: redirecionar para `/contracts/{id}` (pular Steps 2–5)
- Em caso de erro: exibir mensagem de erro inline

**Validações no frontend:**
- Tipo de arquivo: apenas `application/vnd.openxmlformats-officedocument.wordprocessingml.document` ou `.docx`
- Tamanho máximo: 10MB
- Título obrigatório, mínimo 3 caracteres

**Verificação:** testar upload de DOCX no wizard, confirmar redirecionamento para a tela de detalhes

---

### ETAPA 7 — Frontend: Componente `ContractVersionHistory`
**Arquivo a criar:** `src/frontend/src/components/contracts/ContractVersionHistory.tsx`

**Interface visual:**
```
┌─────────────────────────────────────────────────────────┐
│  Histórico de Versões                                    │
├──────┬────────────────┬──────────┬──────────┬───────────┤
│ Vers.│ Data           │ Origem   │ Formato  │           │
├──────┼────────────────┼──────────┼──────────┼───────────┤
│ v2   │ 23/02/2026     │ Upload   │ DOCX     │ [Download]│
│ v1   │ 20/02/2026     │ Builder  │ PDF      │ [Download]│
└──────┴────────────────┴──────────┴──────────┴───────────┘
```

**O que fazer:**
- Chamar `GET /api/contracts/{id}/versions`
- Exibir lista ordenada por `versionNumber` DESC
- Botão "Download" chama `GET /api/contracts/{id}/versions/{versionId}/download`
- Badge colorido: Builder = azul, Upload = verde; PDF = vermelho, DOCX = azul
- Exibir notas da versão se existirem

**Verificação:** abrir tela de detalhes de um contrato, confirmar que a seção de histórico aparece

---

### ETAPA 8 — Frontend: Componente `UploadNewVersion`
**Arquivo a criar:** `src/frontend/src/components/contracts/UploadNewVersion.tsx`

**Interface visual:** Modal/Drawer com:
- Título: "Upload de Nova Versão"
- Dropzone para `.docx` (mesmo comportamento do Step 1)
- Campo de notas (textarea, opcional)
- Botões: "Cancelar" e "Fazer Upload"
- Loading state durante upload

**O que fazer:**
- Submit: `POST /api/contracts/{id}/upload-version` com `multipart/form-data`
- Em caso de sucesso: fechar modal, invalidar query de versões (`refetch`), exibir toast de sucesso
- Em caso de erro: exibir mensagem de erro dentro do modal

**Verificação:** testar upload de nova versão, confirmar que aparece no histórico com número incrementado

---

### ETAPA 9 — Frontend: Bloquear Edição e Integrar Componentes
**Arquivos a atualizar:**
- `src/frontend/src/components/contracts/ContractActions.tsx`
- `src/frontend/src/components/contracts/ContractDetails.tsx`

**Em `ContractActions.tsx`:**
- Verificar `contract.status`
- Se status == `Draft`: botão "Editar" habilitado (comportamento atual mantido)
- Se status != `Draft`: botão "Editar" desabilitado com tooltip:
  > "Este contrato não pode ser editado diretamente. Para atualizar, realize um novo upload de documento."
- Adicionar botão "Upload nova versão" que abre o `UploadNewVersion` modal:
  - Visível em todos os status exceto `Cancelled` e `Expired`

**Em `ContractDetails.tsx`:**
- Importar e renderizar `<ContractVersionHistory contractId={id} />`
- Posicionar abaixo das informações principais do contrato (antes da seção de partes/cláusulas ou após, conforme layout atual)

**Verificação:** 
1. Contrato em status Draft → "Editar" habilitado, "Upload nova versão" visível
2. Contrato em status não-Draft → "Editar" desabilitado com tooltip, "Upload nova versão" visível
3. Contrato Cancelled/Expired → "Upload nova versão" oculto

---

### ETAPA 10 — Testes e Ajustes Finais
**Checklist de testes end-to-end:**

- [ ] Criar contrato via Upload de DOCX no Step 1 → status Draft, versão 1 registrada
- [ ] Criar contrato via Builder → ao gerar, versão 1 registrada (source=Builder, tipo=PDF)
- [ ] Upload de nova versão em contrato existente → versão 2 aparece no histórico
- [ ] Download da versão 1 (PDF) → arquivo PDF válido, content-type correto
- [ ] Download da versão 2 (DOCX) → arquivo DOCX válido, content-type correto
- [ ] Contrato gerado (sai de Draft) → botão "Editar" desabilitado com tooltip
- [ ] Botão "Upload nova versão" → abre modal corretamente
- [ ] Upload de arquivo que não é .docx → erro de validação no frontend e backend
- [ ] Upload de arquivo maior que 10MB → erro de validação no frontend
- [ ] Histórico de versões ordenado do mais novo para o mais antigo

---

## Dependências Técnicas

| Pacote | Onde | Para quê |
|--------|------|----------|
| `DocumentFormat.OpenXml` (já pode existir, verificar) | Backend | Validação estrutural de DOCX (opcional) |
| `react-dropzone` (verificar se já existe) | Frontend | Dropzone de upload |
| Sem novos pacotes de PDF | — | Builder continua com QuestPDF existente |

---

## Ordem de execução recomendada

```
ETAPA 1 (SQL)
    ↓
ETAPA 2 (Domain)
    ↓
ETAPA 3 (Infrastructure)
    ↓
ETAPA 4 (Application)
    ↓
ETAPA 5 (API)        ← build completo aqui antes de ir ao frontend
    ↓
ETAPA 6 (Step 1 Upload)
    ↓
ETAPA 7 (VersionHistory)
    ↓
ETAPA 8 (UploadNewVersion modal)
    ↓
ETAPA 9 (ContractActions + ContractDetails)
    ↓
ETAPA 10 (Testes)
```

---

## Arquivos que serão criados (novos)

| Arquivo | Etapa |
|---------|-------|
| `docker/mysql/migrations/019_create_contract_versions.sql` | 1 |
| `src/backend/PartnershipManager.Domain/Entities/Contract/ContractVersion.cs` | 2 |
| `src/backend/PartnershipManager.Domain/Interfaces/Services/IContractStorageService.cs` | 3 |
| `src/backend/PartnershipManager.Infrastructure/Services/ContractStorageService.cs` | 3 |
| `src/backend/PartnershipManager.Application/DTOs/ContractVersionDto.cs` | 4 |
| `src/backend/PartnershipManager.Application/Features/Contracts/UploadContractVersion/UploadContractVersionCommand.cs` | 4 |
| `src/backend/PartnershipManager.Application/Features/Contracts/UploadContractVersion/UploadContractVersionHandler.cs` | 4 |
| `src/backend/PartnershipManager.Application/Features/Contracts/CreateContractFromUpload/CreateContractFromUploadCommand.cs` | 4 |
| `src/backend/PartnershipManager.Application/Features/Contracts/CreateContractFromUpload/CreateContractFromUploadHandler.cs` | 4 |
| `src/frontend/src/components/contracts/ContractVersionHistory.tsx` | 7 |
| `src/frontend/src/components/contracts/UploadNewVersion.tsx` | 8 |

## Arquivos que serão modificados (existentes)

| Arquivo | Etapa | O que muda |
|---------|-------|-----------|
| `src/backend/PartnershipManager.Domain/Entities/Contract/Contract.cs` | 2 | `+CurrentVersionNumber`, `+Versions` |
| DbContext (Infrastructure) | 3 | `+DbSet<ContractVersion>` + mapeamento |
| `src/backend/PartnershipManager.API/Controllers/ContractsController.cs` | 5 | 4 novos endpoints + registro DI |
| Handler de geração do builder | 4/5 | Criar `ContractVersion` v1 ao gerar |
| `src/frontend/src/components/contracts/builder/Step1SelectType.tsx` | 6 | Toggle Builder/Upload + formulário DOCX |
| `src/frontend/src/components/contracts/ContractActions.tsx` | 9 | Bloquear editar + botão upload nova versão |
| `src/frontend/src/components/contracts/ContractDetails.tsx` | 9 | Incluir `ContractVersionHistory` |
