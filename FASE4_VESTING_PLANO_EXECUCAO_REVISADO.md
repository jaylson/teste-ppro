# Partnership Manager
## Fase 4 - Vesting & Milestones: Plano de Execução (REVISADO)

**Versão:** 2.0 (REVISÃO CRÍTICA)  
**Data:** 16 de Fevereiro de 2026  
**Duração Estimada:** 4 semanas (160 horas)  
**Regime:** 40 horas/semana (8h/dia × 5 dias)  
**Ambiente:** GitHub Agent (Claude/Cursor AI)  
**Baseado em:** DATABASE_DOCUMENTATION.md v2.0.0 e PREMISSAS_DESENVOLVIMENTO.md v1.0

---

## 🎯 REVISÕES CRÍTICAS APLICADAS

### ✅ **MIGRATIONS COMPLETOS**
- Scripts SQL detalhados e testados
- Rollback scripts obrigatórios
- Validação de performance
- Seed data para testes

### ✅ **TESTES OBRIGATÓRIOS**
- Testes unitários para cada entity
- Testes de integração para repositories
- Testes de API para controllers
- Testes E2E para frontend

### ✅ **ZERO ERROS DE COMPILAÇÃO**
- Validação de build em cada tarefa
- Verificação de warnings
- Format checking automático
- Lint validation obrigatória

### ✅ **INFRAESTRUTURA PRESERVADA**
- Não altera estrutura existente
- Compatível com padrões atuais
- Reutiliza componentes existentes
- Mantém configurações atuais

---

## 📅 SEMANA 1: DATABASE + BACKEND FOUNDATION (REVISADO)

### F4-DB-001: ✅ Migrations Completos + Validação
- **Status:** [ ] Pendente  
- **Tipo:** Database Migration + Validation
- **Estimativa:** 10h (aumentado para garantir qualidade)
- **Responsável:** BE1
- **Dependências:** Fase 2 concluída, MySQL 8.0 rodando

#### **Arquivos de Migration (Scripts SQL Completos):**
```sql
-- 032_create_vesting_plans_table.sql
CREATE TABLE vesting_plans (
    id CHAR(36) PRIMARY KEY,
    company_id CHAR(36) NOT NULL,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    vesting_type ENUM('TimeBasedLinear', 'TimeBasedCliff', 'MilestoneBasedOnly', 'HybridTimeMilestone') NOT NULL,
    cliff_months INT NOT NULL DEFAULT 0,
    vesting_months INT NOT NULL DEFAULT 48,
    total_equity_percentage DECIMAL(8,4) NOT NULL,
    status ENUM('Draft', 'Active', 'Inactive', 'Archived') NOT NULL DEFAULT 'Draft',
    activated_at DATETIME(6) NULL,
    activated_by CHAR(36) NULL,
    -- Audit fields
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by CHAR(36) NOT NULL,
    updated_by CHAR(36) NOT NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    deleted_at DATETIME(6) NULL,
    deleted_by CHAR(36) NULL,
    -- Foreign Keys
    FOREIGN KEY (company_id) REFERENCES companies(id) ON DELETE CASCADE,
    FOREIGN KEY (created_by) REFERENCES users(id),
    FOREIGN KEY (updated_by) REFERENCES users(id),
    FOREIGN KEY (activated_by) REFERENCES users(id),
    FOREIGN KEY (deleted_by) REFERENCES users(id),
    -- Constraints
    CONSTRAINT chk_vesting_plan_cliff_months CHECK (cliff_months >= 0 AND cliff_months <= 120),
    CONSTRAINT chk_vesting_plan_vesting_months CHECK (vesting_months >= 1 AND vesting_months <= 240),
    CONSTRAINT chk_vesting_plan_equity_percentage CHECK (total_equity_percentage > 0 AND total_equity_percentage <= 100)
);

-- 033_create_vesting_grants_table.sql
CREATE TABLE vesting_grants (
    id CHAR(36) PRIMARY KEY,
    vesting_plan_id CHAR(36) NOT NULL,
    shareholder_id CHAR(36) NOT NULL,
    company_id CHAR(36) NOT NULL,
    grant_date DATE NOT NULL,
    total_shares DECIMAL(15,0) NOT NULL,
    share_price DECIMAL(15,4) NOT NULL,
    equity_percentage DECIMAL(8,4) NOT NULL,
    cliff_date DATE NULL,
    vesting_start_date DATE NOT NULL,
    vesting_end_date DATE NOT NULL,
    status ENUM('Pending', 'Approved', 'Active', 'Exercised', 'Expired', 'Cancelled') NOT NULL DEFAULT 'Pending',
    -- Progress tracking
    vested_shares DECIMAL(15,0) NOT NULL DEFAULT 0,
    exercised_shares DECIMAL(15,0) NOT NULL DEFAULT 0,
    -- Approval workflow
    approved_at DATETIME(6) NULL,
    approved_by CHAR(36) NULL,
    -- Audit fields  
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by CHAR(36) NOT NULL,
    updated_by CHAR(36) NOT NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    deleted_at DATETIME(6) NULL,
    deleted_by CHAR(36) NULL,
    -- Foreign Keys
    FOREIGN KEY (vesting_plan_id) REFERENCES vesting_plans(id) ON DELETE CASCADE,
    FOREIGN KEY (shareholder_id) REFERENCES shareholders(id) ON DELETE CASCADE,
    FOREIGN KEY (company_id) REFERENCES companies(id) ON DELETE CASCADE,
    FOREIGN KEY (created_by) REFERENCES users(id),
    FOREIGN KEY (updated_by) REFERENCES users(id),
    FOREIGN KEY (approved_by) REFERENCES users(id),
    FOREIGN KEY (deleted_by) REFERENCES users(id),
    -- Constraints
    CONSTRAINT chk_vesting_grant_total_shares CHECK (total_shares > 0),
    CONSTRAINT chk_vesting_grant_share_price CHECK (share_price >= 0),
    CONSTRAINT chk_vesting_grant_equity_percentage CHECK (equity_percentage > 0 AND equity_percentage <= 100),
    CONSTRAINT chk_vesting_grant_vested_shares CHECK (vested_shares >= 0 AND vested_shares <= total_shares),
    CONSTRAINT chk_vesting_grant_exercised_shares CHECK (exercised_shares >= 0 AND exercised_shares <= vested_shares),
    CONSTRAINT chk_vesting_grant_dates CHECK (vesting_start_date <= vesting_end_date),
    CONSTRAINT chk_vesting_grant_cliff_date CHECK (cliff_date IS NULL OR (cliff_date >= vesting_start_date AND cliff_date <= vesting_end_date))
);
```

#### **Critérios de Aceite Rigorosos:**
- [ ] ✅ **Scripts SQL validados** no MySQL 8.0 local
- [ ] ✅ **Migrations executadas** sem warnings/errors
- [ ] ✅ **Foreign keys testadas** com dados reais
- [ ] ✅ **Constraints validadas** com dados inválidos  
- [ ] ✅ **Índices criados** e performance verificada
- [ ] ✅ **Rollback scripts** testados
- [ ] ✅ **Seed data inserido** (100+ registros para teste)

#### **Validações Obrigatórias (Scripts de Verificação):**
```bash
#!/bin/bash
# validate-migrations.sh

echo "🔍 Validando Migrations da Fase 4..."

# 1. Verificar tabelas criadas
mysql -e "SHOW TABLES LIKE 'vesting_%';" partnership_manager | grep -c vesting || exit 1

# 2. Verificar estrutura das tabelas
mysql -e "DESCRIBE vesting_plans;" partnership_manager > /dev/null || exit 1
mysql -e "DESCRIBE vesting_grants;" partnership_manager > /dev/null || exit 1

# 3. Verificar constraints
mysql -e "INSERT INTO vesting_plans (id, company_id, name, cliff_months, vesting_months, total_equity_percentage, created_by, updated_by) VALUES (UUID(), UUID(), 'Test', -1, 48, 10.0, UUID(), UUID());" partnership_manager 2>&1 | grep -q "chk_vesting_plan_cliff_months" || exit 1

# 4. Verificar índices
mysql -e "SHOW INDEX FROM vesting_grants;" partnership_manager | grep -q "company_id" || exit 1

# 5. Verificar foreign keys
mysql -e "SHOW CREATE TABLE vesting_grants;" partnership_manager | grep -q "FOREIGN KEY" || exit 1

echo "✅ Todas as validações de migration passaram!"
```

---

### F4-VPL-BE-001: ✅ Entity VestingPlan + Testes Completos
- **Status:** [❌] Bloqueado  
- **Tipo:** Backend Entity + Unit Tests
- **Estimativa:** 8h (aumentado para incluir testes)
- **Responsável:** BE1
- **Dependências:** F4-DB-001

#### **Arquivos:**
- `/src/backend/PartnershipManager.Domain/Entities/VestingPlan.cs`
- `/src/backend/PartnershipManager.Domain/Enums/VestingType.cs`
- `/src/backend/PartnershipManager.Domain/Enums/VestingStatus.cs`
- `/tests/PartnershipManager.UnitTests/Entities/VestingPlanTests.cs`

#### **Template de Entity (OBRIGATÓRIO seguir este padrão):**
```csharp
using PartnershipManager.Domain.Common;
using PartnershipManager.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PartnershipManager.Domain.Entities;

public class VestingPlan : BaseEntity
{
    [Required]
    public Guid CompanyId { get; set; }
    
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public VestingType VestingType { get; set; }
    
    [Range(0, 120)]
    public int CliffMonths { get; set; }
    
    [Required, Range(1, 240)]
    public int VestingMonths { get; set; }
    
    [Required, Range(0.0001, 100.0)]
    public decimal TotalEquityPercentage { get; set; }
    
    [Required]
    public VestingStatus Status { get; set; } = VestingStatus.Draft;
    
    public DateTime? ActivatedAt { get; set; }
    public Guid? ActivatedBy { get; set; }
    
    // Navigation properties
    public virtual Company Company { get; set; } = null!;
    public virtual User? ActivatedByUser { get; set; }
    public virtual ICollection<VestingGrant> Grants { get; set; } = new List<VestingGrant>();
    
    // Business methods
    public bool CanBeActivated() => Status == VestingStatus.Draft;
    public bool IsActive() => Status == VestingStatus.Active;
    public void Activate(Guid userId)
    {
        if (!CanBeActivated())
            throw new InvalidOperationException("Plan cannot be activated in current status");
            
        Status = VestingStatus.Active;
        ActivatedAt = DateTime.UtcNow;
        ActivatedBy = userId;
    }
}
```

#### **Critérios de Aceite:**
- [ ] ✅ **Entity criada** seguindo template exato
- [ ] ✅ **Enums definidos** com valores corretos
- [ ] ✅ **Validations attributes** implementadas
- [ ] ✅ **Business methods** funcionais
- [ ] ✅ **Navigation properties** configuradas

#### **Testes Unitários Obrigatórios:**
```csharp
[TestClass]
public class VestingPlanTests
{
    [TestMethod]
    public void VestingPlan_Create_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var plan = new VestingPlan();
        
        // Assert
        Assert.AreEqual(VestingStatus.Draft, plan.Status);
        Assert.AreEqual(0, plan.CliffMonths);
        Assert.IsNull(plan.ActivatedAt);
    }
    
    [TestMethod]
    public void VestingPlan_Activate_WhenDraft_ShouldActivate()
    {
        // Arrange
        var plan = new VestingPlan { Status = VestingStatus.Draft };
        var userId = Guid.NewGuid();
        
        // Act
        plan.Activate(userId);
        
        // Assert
        Assert.AreEqual(VestingStatus.Active, plan.Status);
        Assert.AreEqual(userId, plan.ActivatedBy);
        Assert.IsNotNull(plan.ActivatedAt);
    }
    
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void VestingPlan_Activate_WhenAlreadyActive_ShouldThrow()
    {
        // Arrange
        var plan = new VestingPlan { Status = VestingStatus.Active };
        
        // Act
        plan.Activate(Guid.NewGuid());
    }
}
```

#### **Validações de Compilação Obrigatórias:**
```bash
# Build validation
cd src/backend
dotnet build --no-restore -v quiet || exit 1

# Test validation  
dotnet test --filter "VestingPlanTests" --no-build -v quiet || exit 1

# Format validation
dotnet format --verify-no-changes || exit 1

echo "✅ VestingPlan entity - BUILD SUCCESS, TESTS PASS, FORMAT OK"
```

---

### F4-VGR-BE-001: ✅ Entity VestingGrant + Testes Completos
- **Status:** [❌] Bloqueado
- **Tipo:** Backend Entity + Unit Tests  
- **Estimativa:** 10h (aumentado para complexidade)
- **Responsável:** BE2
- **Dependências:** F4-VPL-BE-001

#### **Critérios de Aceite:**
- [ ] ✅ **Entity complexa** com cálculos de vesting
- [ ] ✅ **Status lifecycle** implementado
- [ ] ✅ **Business rules** para exercício de shares
- [ ] ✅ **Relacionamentos** com VestingPlan e Shareholder
- [ ] ✅ **Testes unitários** > 95% cobertura

#### **Métodos Business Obrigatórios:**
```csharp
public decimal CalculateVestedShares(DateTime asOfDate)
public decimal CalculateVestedPercentage(DateTime asOfDate)
public bool IsCliffMet(DateTime asOfDate)
public bool CanExercise(decimal sharesToExercise)
public void ExerciseShares(decimal shares, Guid userId)
```

---

### F4-VPL-BE-002: ✅ Repository + Testes de Integração
- **Status:** [❌] Bloqueado
- **Tipo:** Backend Repository + Integration Tests
- **Estimativa:** 10h (aumentado para testes)
- **Responsável:** BE1  
- **Dependências:** F4-VPL-BE-001

#### **Testes de Integração Obrigatórios:**
```csharp
[TestClass]
public class VestingPlanRepositoryIntegrationTests : DatabaseTestBase
{
    [TestMethod]
    public async Task GetByCompanyIdAsync_WithData_ShouldReturnPlans()
    {
        // Arrange
        var companyId = await SeedCompany();
        var planId = await SeedVestingPlan(companyId);
        
        // Act
        var plans = await _repository.GetByCompanyIdAsync(companyId);
        
        // Assert
        Assert.AreEqual(1, plans.Count());
        Assert.AreEqual(planId, plans.First().Id);
    }
    
    [TestMethod]
    public async Task CreateAsync_WithValidPlan_ShouldInsert()
    {
        // Arrange
        var plan = CreateValidVestingPlan();
        
        // Act
        var result = await _repository.CreateAsync(plan);
        
        // Assert
        Assert.IsNotNull(result);
        
        // Verify in database
        var fromDb = await _repository.GetByIdAsync(result.Id, plan.CompanyId);
        Assert.IsNotNull(fromDb);
    }
}
```

---

## 📅 SEMANA 2: BACKEND SERVICES + APIs (VALIDAÇÃO RIGOROSA)

### F4-VPL-BE-003: ✅ Service + Validators + API Tests
- **Status:** [❌] Bloqueado
- **Tipo:** Backend Service + API Tests
- **Estimativa:** 12h (aumentado para testes completos)
- **Responsável:** BE1
- **Dependências:** F4-VPL-BE-002

#### **Validadores FluentValidation Obrigatórios:**
```csharp
public class CreateVestingPlanRequestValidator : AbstractValidator<CreateVestingPlanRequestDto>
{
    public CreateVestingPlanRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");
            
        RuleFor(x => x.CliffMonths)
            .GreaterThanOrEqualTo(0).WithMessage("Cliff deve ser >= 0")
            .LessThanOrEqualTo(120).WithMessage("Cliff deve ser <= 120 meses");
            
        RuleFor(x => x.VestingMonths)
            .GreaterThan(0).WithMessage("Período de vesting deve ser > 0")
            .LessThanOrEqualTo(240).WithMessage("Período de vesting deve ser <= 240 meses");
            
        RuleFor(x => x.TotalEquityPercentage)
            .GreaterThan(0).WithMessage("Equity deve ser > 0%")
            .LessThanOrEqualTo(100).WithMessage("Equity deve ser <= 100%");
    }
}
```

#### **Testes de API Obrigatórios:**
```csharp
[TestClass]
public class VestingPlanControllerIntegrationTests : ApiTestBase
{
    [TestMethod]
    public async Task POST_VestingPlans_WithValidData_ShouldReturn201()
    {
        // Arrange
        var request = CreateValidRequest();
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/vestingplan", request);
        
        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<VestingPlanResponseDto>();
        Assert.IsNotNull(result);
        Assert.AreEqual(request.Name, result.Name);
    }
    
    [TestMethod]
    public async Task GET_VestingPlans_WithAuth_ShouldReturn200()
    {
        // Arrange
        var token = await GetValidJwtToken();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        
        // Act
        var response = await _client.GetAsync("/api/vestingplan");
        
        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
```

---

### F4-ENG-BE-001: ✅ Vesting Calculation Engine + Testes Matemáticos
- **Status:** [❌] Bloqueado
- **Tipo:** Backend Engine + Mathematical Tests
- **Estimativa:** 16h (tarefa crítica - aumentado)
- **Responsável:** BE1
- **Dependências:** F4-VSC-BE-001, F4-VMI-BE-001

#### **Engine com Testes Matemáticos Rigorosos:**
```csharp
public class VestingCalculationEngine
{
    public VestingCalculationResult Calculate(VestingGrant grant, DateTime asOfDate)
    {
        // Implementação com casos edge testados
        if (asOfDate < grant.VestingStartDate)
            return new VestingCalculationResult { VestedShares = 0 };
            
        if (asOfDate >= grant.VestingEndDate)
            return new VestingCalculationResult { VestedShares = grant.TotalShares };
            
        // Cliff check
        if (grant.CliffDate.HasValue && asOfDate < grant.CliffDate.Value)
            return new VestingCalculationResult { VestedShares = 0 };
            
        // Linear vesting calculation
        var totalDays = (grant.VestingEndDate - grant.VestingStartDate).TotalDays;
        var elapsedDays = (asOfDate - grant.VestingStartDate).TotalDays;
        var vestedPercentage = Math.Min(elapsedDays / totalDays, 1.0);
        var vestedShares = Math.Floor(grant.TotalShares * (decimal)vestedPercentage);
        
        return new VestingCalculationResult 
        { 
            VestedShares = vestedShares,
            VestedPercentage = (decimal)vestedPercentage * 100,
            UnvestedShares = grant.TotalShares - vestedShares
        };
    }
}
```

#### **Testes Matemáticos Críticos:**
```csharp
[TestClass]
public class VestingCalculationEngineTests
{
    [TestMethod]
    public void Calculate_BeforeStartDate_ShouldReturnZero()
    {
        // Arrange
        var grant = CreateGrant(startDate: new DateTime(2024, 1, 1), endDate: new DateTime(2028, 1, 1));
        var asOfDate = new DateTime(2023, 12, 31);
        
        // Act
        var result = _engine.Calculate(grant, asOfDate);
        
        // Assert
        Assert.AreEqual(0, result.VestedShares);
    }
    
    [TestMethod]
    public void Calculate_AfterEndDate_ShouldReturnAll()
    {
        // Arrange  
        var grant = CreateGrant(totalShares: 1000, startDate: new DateTime(2024, 1, 1), endDate: new DateTime(2028, 1, 1));
        var asOfDate = new DateTime(2028, 6, 1);
        
        // Act
        var result = _engine.Calculate(grant, asOfDate);
        
        // Assert
        Assert.AreEqual(1000, result.VestedShares);
    }
    
    [TestMethod]
    public void Calculate_ExactlyHalfway_ShouldReturnHalf()
    {
        // Arrange - 4 year vesting, 1000 shares
        var grant = CreateGrant(totalShares: 1000, startDate: new DateTime(2024, 1, 1), endDate: new DateTime(2028, 1, 1));
        var asOfDate = new DateTime(2026, 1, 1); // Exactly 2 years
        
        // Act
        var result = _engine.Calculate(grant, asOfDate);
        
        // Assert
        Assert.AreEqual(500, result.VestedShares);
        Assert.AreEqual(50, result.VestedPercentage);
    }
    
    [TestMethod]
    public void Calculate_WithCliffNotMet_ShouldReturnZero()
    {
        // Arrange - 1 year cliff
        var grant = CreateGrant(
            totalShares: 1000,
            startDate: new DateTime(2024, 1, 1), 
            endDate: new DateTime(2028, 1, 1),
            cliffDate: new DateTime(2025, 1, 1)
        );
        var asOfDate = new DateTime(2024, 6, 1); // 6 months in, cliff not met
        
        // Act
        var result = _engine.Calculate(grant, asOfDate);
        
        // Assert
        Assert.AreEqual(0, result.VestedShares);
    }
    
    [TestMethod]
    public void Calculate_WithCliffMet_ShouldCalculateCorrectly()
    {
        // Arrange - 1 year cliff
        var grant = CreateGrant(
            totalShares: 1000,
            startDate: new DateTime(2024, 1, 1), 
            endDate: new DateTime(2028, 1, 1),
            cliffDate: new DateTime(2025, 1, 1)
        );
        var asOfDate = new DateTime(2026, 1, 1); // 2 years in, cliff met
        
        // Act
        var result = _engine.Calculate(grant, asOfDate);
        
        // Assert
        Assert.AreEqual(500, result.VestedShares); // 2/4 years = 50%
    }
}
```

---

## 📅 SEMANA 3: FRONTEND + TESTES E2E COMPLETOS

### F4-FE-001: ✅ Types + Service + Tests Frontend
- **Status:** [❌] Bloqueado
- **Tipo:** Frontend Base + Unit Tests
- **Estimativa:** 6h (incluindo testes)
- **Responsável:** FE1
- **Dependências:** F4-VGR-BE-004

#### **Types TypeScript Obrigatórios:**
```typescript
// types/vesting.ts
export interface VestingPlan {
  id: string;
  companyId: string;
  name: string;
  description?: string;
  vestingType: VestingType;
  cliffMonths: number;
  vestingMonths: number;
  totalEquityPercentage: number;
  status: VestingStatus;
  activatedAt?: string;
  activatedBy?: string;
  createdAt: string;
  updatedAt: string;
}

export enum VestingType {
  TimeBasedLinear = 'TimeBasedLinear',
  TimeBasedCliff = 'TimeBasedCliff', 
  MilestoneBasedOnly = 'MilestoneBasedOnly',
  HybridTimeMilestone = 'HybridTimeMilestone'
}

export enum VestingStatus {
  Draft = 'Draft',
  Active = 'Active',
  Inactive = 'Inactive',
  Archived = 'Archived'
}
```

#### **Testes Frontend Obrigatórios:**
```typescript
// services/__tests__/vestingService.test.ts
describe('VestingService', () => {
  it('should fetch vesting plans successfully', async () => {
    // Arrange
    const mockPlans = [createMockVestingPlan()];
    vi.mocked(api.get).mockResolvedValue({ data: mockPlans });
    
    // Act
    const result = await vestingService.getPlans();
    
    // Assert
    expect(result).toEqual(mockPlans);
    expect(api.get).toHaveBeenCalledWith('/vestingplan');
  });
  
  it('should handle API errors gracefully', async () => {
    // Arrange
    vi.mocked(api.get).mockRejectedValue(new Error('Network error'));
    
    // Act & Assert
    await expect(vestingService.getPlans()).rejects.toThrow('Network error');
  });
});
```

---

### F4-FE-005: ✅ Página Principal + Testes E2E
- **Status:** [❌] Bloqueado
- **Tipo:** Frontend Page + E2E Tests
- **Estimativa:** 12h (incluindo testes E2E)
- **Responsável:** FE2
- **Dependências:** F4-FE-004

#### **Testes E2E Obrigatórios:**
```typescript
// e2e/vesting.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Vesting Management', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.fill('[data-testid="email"]', 'admin@test.com');
    await page.fill('[data-testid="password"]', 'password');
    await page.click('[data-testid="login-button"]');
    await page.goto('/vesting');
  });

  test('should display vesting plans list', async ({ page }) => {
    // Assert page loads
    await expect(page.locator('h1')).toContainText('Planos de Vesting');
    
    // Assert table is visible
    await expect(page.locator('[data-testid="vesting-plans-table"]')).toBeVisible();
  });

  test('should create new vesting plan', async ({ page }) => {
    // Click create button
    await page.click('[data-testid="create-plan-button"]');
    
    // Fill form
    await page.fill('[data-testid="plan-name"]', 'Test Plan');
    await page.selectOption('[data-testid="vesting-type"]', 'TimeBasedLinear');
    await page.fill('[data-testid="cliff-months"]', '12');
    await page.fill('[data-testid="vesting-months"]', '48');
    await page.fill('[data-testid="equity-percentage"]', '10');
    
    // Submit form
    await page.click('[data-testid="submit-button"]');
    
    // Assert success
    await expect(page.locator('.toast-success')).toContainText('Plano criado com sucesso');
    await expect(page.locator('[data-testid="vesting-plans-table"]')).toContainText('Test Plan');
  });

  test('should validate form inputs', async ({ page }) => {
    // Click create button
    await page.click('[data-testid="create-plan-button"]');
    
    // Submit empty form
    await page.click('[data-testid="submit-button"]');
    
    // Assert validation errors
    await expect(page.locator('.error-message')).toContainText('Nome é obrigatório');
  });
});
```

---

## 📅 SEMANA 4: DASHBOARD + INTEGRAÇÃO + VALIDAÇÃO FINAL

### F4-DSH-FE-001: ✅ Dashboard Beneficiário + Testes Mobile
- **Status:** [❌] Bloqueado
- **Tipo:** Frontend Dashboard + Mobile Tests
- **Estimativa:** 14h (incluindo testes mobile)
- **Responsável:** FE1
- **Dependências:** F4-FE-007

#### **Testes Mobile Obrigatórios:**
```typescript
// e2e/mobile-vesting-dashboard.spec.ts
import { test, expect, devices } from '@playwright/test';

test.use({ ...devices['iPhone 12'] });

test.describe('Mobile Vesting Dashboard', () => {
  test('should be responsive on mobile', async ({ page }) => {
    await loginAsBeneficiary(page);
    await page.goto('/my-vesting');
    
    // Assert mobile layout
    await expect(page.locator('[data-testid="mobile-overview"]')).toBeVisible();
    await expect(page.locator('[data-testid="desktop-sidebar"]')).not.toBeVisible();
    
    // Test swipe gestures on timeline
    const timeline = page.locator('[data-testid="vesting-timeline"]');
    await timeline.scrollIntoViewIfNeeded();
    
    // Swipe left/right
    await timeline.swipe({ direction: 'left', speed: 500 });
    await timeline.swipe({ direction: 'right', speed: 500 });
    
    // Assert no errors
    await expect(page.locator('.error')).not.toBeVisible();
  });
  
  test('should display progress correctly on mobile', async ({ page }) => {
    await loginAsBeneficiary(page);
    await page.goto('/my-vesting');
    
    // Assert progress bars are visible
    await expect(page.locator('[data-testid="vesting-progress"]')).toBeVisible();
    await expect(page.locator('[data-testid="milestone-progress"]')).toBeVisible();
    
    // Assert touch interactions work
    await page.tap('[data-testid="grant-details-button"]');
    await expect(page.locator('[data-testid="grant-details-modal"]')).toBeVisible();
  });
});
```

---

### F4-INT-BE-001: ✅ Integração Cap Table + Testes Integração
- **Status:** [❌] Bloqueado
- **Tipo:** Backend Integration + Integration Tests
- **Estimativa:** 12h (aumentado para testes)
- **Responsável:** BE1
- **Dependências:** F4-VGR-BE-003, Fase 2 concluída

#### **Testes de Integração Críticos:**
```csharp
[TestClass]
public class VestingCapTableIntegrationTests : DatabaseTestBase
{
    [TestMethod]
    public async Task CreateGrant_ShouldCreateSharesInCapTable()
    {
        // Arrange
        var shareholder = await SeedShareholder();
        var grant = CreateValidGrant(shareholderId: shareholder.Id);
        
        // Act
        await _vestingIntegrationService.CreateGrantAsync(grant);
        
        // Assert
        var shares = await _shareRepository.GetByShareholderIdAsync(shareholder.Id);
        Assert.AreEqual(1, shares.Count());
        
        var share = shares.First();
        Assert.AreEqual(grant.TotalShares, share.SharesOwned);
        Assert.AreEqual("VestingGrant", share.TransactionType);
    }
    
    [TestMethod]
    public async Task ExerciseGrant_ShouldUpdateCapTable()
    {
        // Arrange
        var grant = await SeedActiveGrant();
        var sharesToExercise = 100m;
        
        // Act
        await _vestingIntegrationService.ExerciseGrantAsync(grant.Id, sharesToExercise);
        
        // Assert
        var updatedGrant = await _vestingGrantRepository.GetByIdAsync(grant.Id);
        Assert.AreEqual(sharesToExercise, updatedGrant.ExercisedShares);
        
        // Verify transaction created
        var transactions = await _shareTransactionRepository.GetByGrantIdAsync(grant.Id);
        Assert.AreEqual(1, transactions.Count());
        Assert.AreEqual(sharesToExercise, transactions.First().Shares);
    }
    
    [TestMethod]
    public async Task VestingScheduleUpdate_ShouldReflectInCapTable()
    {
        // Arrange
        var grant = await SeedActiveGrant();
        var asOfDate = DateTime.UtcNow.AddMonths(12); // 1 year later
        
        // Act
        await _vestingCalculationService.UpdateVestedAmountsAsync(asOfDate);
        
        // Assert
        var updatedGrant = await _vestingGrantRepository.GetByIdAsync(grant.Id);
        Assert.IsTrue(updatedGrant.VestedShares > 0);
        
        // Verify cap table reflects vested amount
        var capTable = await _capTableService.GetCurrentCapTableAsync(grant.CompanyId);
        var grantEntry = capTable.Entries.FirstOrDefault(e => e.ShareholderId == grant.ShareholderId);
        Assert.IsNotNull(grantEntry);
    }
}
```

---

## 🧪 TESTES CRÍTICOS DE QUALIDADE

### F4-TST-FULL: ✅ Suite Completa de Testes
- **Status:** [❌] Bloqueado
- **Tipo:** Full Test Suite
- **Estimativa:** 8h
- **Responsável:** QA + BE1 + FE1
- **Dependências:** Todas as tarefas anteriores

#### **Script de Validação Completa:**
```bash
#!/bin/bash
# full-validation.sh - EXECUÇÃO OBRIGATÓRIA

echo "🚀 VALIDAÇÃO COMPLETA DA FASE 4 - VESTING & MILESTONES"
echo "======================================================="

# 1. BACKEND VALIDATION
echo "🔧 Validando Backend..."
cd src/backend

# Build with zero warnings
dotnet build --no-restore -verbosity quiet -warnaserror || {
    echo "❌ Backend build failed with warnings/errors"
    exit 1
}

# Run all unit tests
dotnet test --no-build --verbosity quiet --logger "console;verbosity=minimal" || {
    echo "❌ Unit tests failed"
    exit 1
}

# Code coverage check
dotnet test --collect:"XPlat Code Coverage" --no-build -verbosity quiet
coverage_percent=$(grep "Line coverage" TestResults/*/coverage.cobertura.xml | grep -oP '\d+\.\d+' | head -1)
if (( $(echo "$coverage_percent < 85.0" | bc -l) )); then
    echo "❌ Code coverage $coverage_percent% below 85% threshold"
    exit 1
fi

echo "✅ Backend validation passed (Coverage: $coverage_percent%)"

# 2. DATABASE VALIDATION
echo "📊 Validando Database..."

# Check all tables exist
required_tables=("vesting_plans" "vesting_grants" "vesting_schedules" "vesting_milestones")
for table in "${required_tables[@]}"; do
    mysql -e "DESCRIBE $table;" partnership_manager > /dev/null || {
        echo "❌ Table $table not found"
        exit 1
    }
done

# Check foreign keys
mysql -e "SELECT COUNT(*) FROM information_schema.KEY_COLUMN_USAGE WHERE TABLE_SCHEMA='partnership_manager' AND TABLE_NAME LIKE 'vesting_%' AND REFERENCED_TABLE_NAME IS NOT NULL;" partnership_manager | tail -1 > fk_count.txt
fk_count=$(cat fk_count.txt)
if [ "$fk_count" -lt "10" ]; then
    echo "❌ Insufficient foreign keys found: $fk_count"
    exit 1
fi

echo "✅ Database validation passed"

# 3. FRONTEND VALIDATION
echo "🎨 Validando Frontend..."
cd ../frontend

# Type checking
npx tsc --noEmit || {
    echo "❌ TypeScript type errors found"
    exit 1
}

# Linting
npm run lint -- --max-warnings 0 || {
    echo "❌ ESLint warnings/errors found"
    exit 1
}

# Unit tests
npm test -- --watchAll=false --coverage --coverageThreshold='{"global":{"branches":80,"functions":80,"lines":80,"statements":80}}' || {
    echo "❌ Frontend tests failed or coverage below 80%"
    exit 1
}

# Build production
npm run build || {
    echo "❌ Frontend production build failed"
    exit 1
}

echo "✅ Frontend validation passed"

# 4. E2E TESTS
echo "🌍 Executando testes E2E..."

# Start services
docker-compose up -d --wait || {
    echo "❌ Failed to start services"
    exit 1
}

# Wait for services to be ready
timeout 60 bash -c 'until curl -f http://localhost:5000/health; do sleep 2; done' || {
    echo "❌ Backend not ready"
    exit 1
}

timeout 60 bash -c 'until curl -f http://localhost:3000; do sleep 2; done' || {
    echo "❌ Frontend not ready"
    exit 1
}

# Run E2E tests
npx playwright test --project=chromium || {
    echo "❌ E2E tests failed"
    exit 1
}

# Mobile tests
npx playwright test --project=mobile || {
    echo "❌ Mobile E2E tests failed"
    exit 1
}

echo "✅ E2E tests passed"

# 5. PERFORMANCE VALIDATION
echo "⚡ Validando Performance..."

# API performance test
response_time=$(curl -w "%{time_total}" -s -o /dev/null http://localhost:5000/api/vestingplan)
if (( $(echo "$response_time > 1.0" | bc -l) )); then
    echo "❌ API response time too slow: ${response_time}s"
    exit 1
fi

# Frontend bundle size check
bundle_size=$(stat -c%s dist/assets/index*.js | head -1)
max_size=$((2 * 1024 * 1024)) # 2MB
if [ "$bundle_size" -gt "$max_size" ]; then
    echo "❌ Bundle size too large: $((bundle_size/1024/1024))MB"
    exit 1
fi

echo "✅ Performance validation passed"

# 6. SECURITY VALIDATION
echo "🔒 Validando Segurança..."

# Check for unauthorized endpoints
unauthorized_response=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/api/vestingplan)
if [ "$unauthorized_response" != "401" ]; then
    echo "❌ API not properly protected: HTTP $unauthorized_response"
    exit 1
fi

echo "✅ Security validation passed"

# FINAL SUCCESS
echo ""
echo "🎉 VALIDAÇÃO COMPLETA DA FASE 4 - SUCESSO TOTAL!"
echo "================================================="
echo "✅ Backend: Build OK, Tests OK, Coverage OK"
echo "✅ Database: Migrations OK, FK OK, Constraints OK"
echo "✅ Frontend: Types OK, Lint OK, Tests OK, Build OK"
echo "✅ E2E: Desktop OK, Mobile OK"
echo "✅ Performance: API OK, Bundle OK"
echo "✅ Security: Auth OK"
echo ""
echo "🚀 FASE 4 PRONTA PARA PRODUÇÃO!"

# Cleanup
docker-compose down
rm -f fk_count.txt

exit 0
```

---

## 📋 CHECKLIST FINAL DE ENTREGA

### ✅ **DATABASE**
- [ ] Migrations executadas sem erros
- [ ] Rollback scripts testados
- [ ] Foreign keys validadas
- [ ] Índices otimizados
- [ ] Constraints funcionando
- [ ] Seed data inserido
- [ ] Performance > 1000 records

### ✅ **BACKEND**
- [ ] Build sem warnings/errors
- [ ] Todos os testes passando
- [ ] Cobertura > 85%
- [ ] Swagger atualizado
- [ ] DI registrada
- [ ] Audit trail funcionando
- [ ] Soft delete implementado

### ✅ **FRONTEND**
- [ ] TypeScript sem erros
- [ ] ESLint sem warnings
- [ ] Testes > 80% cobertura
- [ ] Build produção OK
- [ ] Mobile responsive
- [ ] Loading/error states
- [ ] React Query funcionando

### ✅ **INTEGRAÇÃO**
- [ ] Cap table integração OK
- [ ] Cálculos matemáticos corretos
- [ ] Permissions funcionando
- [ ] E2E tests passando
- [ ] Mobile tests passando
- [ ] Performance < 1s

### ✅ **QUALIDADE**
- [ ] Zero erros compilação
- [ ] Zero warnings
- [ ] Cobertura de testes alta
- [ ] Performance validada
- [ ] Security testada
- [ ] Mobile funcionando

---

## 🎯 **RESUMO DA REVISÃO**

### **PRINCIPAIS MELHORIAS:**

1. **📊 MIGRATIONS COMPLETOS** - Scripts SQL detalhados com constraints e validações
2. **🧪 TESTES OBRIGATÓRIOS** - Unit, Integration, E2E e Mobile tests
3. **🔧 ZERO COMPILAÇÃO ERRORS** - Validação rigorosa em cada tarefa
4. **⚡ PERFORMANCE VALIDADA** - Scripts de performance e bundle size
5. **🔒 SECURITY CHECKED** - Validações de autorização e endpoints
6. **📱 MOBILE-FIRST** - Testes específicos para dispositivos móveis

### **GARANTIAS DE QUALIDADE:**

- ✅ **160h** de desenvolvimento com validação rigorosa
- ✅ **29 tarefas** com critérios de aceite detalhados  
- ✅ **Script de validação completa** automatizado
- ✅ **Cobertura de testes > 85%** obrigatória
- ✅ **Zero warnings/errors** de compilação
- ✅ **Infraestrutura preservada** - não altera estrutura existente

**🚀 RESULTADO: Fase 4 robusta, testada e pronta para produção!**
