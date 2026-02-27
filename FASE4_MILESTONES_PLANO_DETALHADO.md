# Partnership Manager - Fase 4
## Plano Específico: Milestones & Performance Vesting

**Versão:** 1.0  
**Data:** 16 de Fevereiro de 2026  
**Estimativa:** 24 horas (3 dias dedicados)  
**Objetivo:** Implementar sistema completo de milestones vinculados ao vesting  
**Integração:** Parte da Fase 4 - Vesting & Milestones

---

## 🎯 VISÃO GERAL DO SISTEMA DE MILESTONES

### **Problema Identificado:**
O plano original não detalhou como as **metas de performance** se conectam ao **cronograma de vesting**, criando lacuna na implementação do vesting baseado em performance.

### **Solução Proposta:**
Sistema completo de milestones que:
- ✅ **Define tipos de metas** (financeiras, operacionais, produto)
- ✅ **Vincula metas ao vesting** (aceleração, desbloqueio)
- ✅ **Rastreia progresso** em tempo real
- ✅ **Workflow de aprovação** de conquistas
- ✅ **Cálculo automático** de aceleração de vesting

---

## 🏗️ ARQUITETURA DO SISTEMA DE MILESTONES

### **Entidades Principais:**
```
📦 Sistema Milestones
├── 🎯 MilestoneTemplate      ← Templates de metas reutilizáveis
├── 🏆 VestingMilestone       ← Metas específicas por grant
├── 📊 MilestoneProgress      ← Tracking de progresso
├── ✅ MilestoneAchievement   ← Conquistas aprovadas
├── ⚡ VestingAcceleration    ← Cálculo de aceleração
└── 🔄 MilestoneWorkflow      ← Workflow de aprovação
```

### **Tipos de Milestones:**
| Categoria | Exemplos | Métrica | Impacto Vesting |
|-----------|----------|---------|-----------------|
| **Financeira** | ARR, Revenue, Profit | Valor monetário | 25-50% aceleração |
| **Operacional** | Headcount, Efficiency | Número/% | 15-30% aceleração |
| **Produto** | Features, Users, MAU | Quantidade | 20-40% aceleração |
| **Mercado** | Market Share, Expansion | % ou número | 30-60% aceleração |
| **Estratégica** | Partnerships, M&A | Binário (Sim/Não) | 50-100% aceleração |

---

## 📅 TAREFAS ATÔMICAS: MILESTONES & PERFORMANCE VESTING

### **GRUPO 1: DATABASE & ENTITIES (8h)**

#### F4-MIL-DB-001: Migrations para Milestones System
- **Estimativa:** 3h
- **Responsável:** BE1
- **Prioridade:** 🔥 Crítica

##### **Arquivos SQL:**
```sql
-- 039_create_milestone_templates_table.sql
CREATE TABLE milestone_templates (
    id CHAR(36) PRIMARY KEY,
    company_id CHAR(36) NOT NULL,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    category ENUM('Financial', 'Operational', 'Product', 'Market', 'Strategic') NOT NULL,
    metric_type ENUM('Currency', 'Percentage', 'Count', 'Boolean') NOT NULL,
    target_operator ENUM('GreaterThan', 'LessThan', 'Equals', 'GreaterOrEqual', 'LessOrEqual') NOT NULL,
    measurement_frequency ENUM('Daily', 'Weekly', 'Monthly', 'Quarterly', 'Annually', 'OneTime') NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    -- Acceleration settings
    vesting_acceleration_type ENUM('Percentage', 'Months', 'Shares') NOT NULL,
    acceleration_amount DECIMAL(8,4) NOT NULL,
    max_acceleration_cap DECIMAL(8,4) NULL,
    -- Audit fields
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by CHAR(36) NOT NULL,
    updated_by CHAR(36) NOT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at DATETIME(6) NULL,
    deleted_by CHAR(36) NULL,
    -- Foreign Keys
    FOREIGN KEY (company_id) REFERENCES companies(id) ON DELETE CASCADE,
    FOREIGN KEY (created_by) REFERENCES users(id),
    FOREIGN KEY (updated_by) REFERENCES users(id),
    FOREIGN KEY (deleted_by) REFERENCES users(id),
    -- Constraints
    CONSTRAINT chk_milestone_template_acceleration_amount CHECK (acceleration_amount > 0 AND acceleration_amount <= 100),
    CONSTRAINT chk_milestone_template_max_cap CHECK (max_acceleration_cap IS NULL OR max_acceleration_cap >= acceleration_amount)
);

-- 040_create_vesting_milestones_table.sql
CREATE TABLE vesting_milestones (
    id CHAR(36) PRIMARY KEY,
    vesting_grant_id CHAR(36) NOT NULL,
    milestone_template_id CHAR(36) NULL, -- NULL para milestones customizados
    name VARCHAR(200) NOT NULL,
    description TEXT,
    category ENUM('Financial', 'Operational', 'Product', 'Market', 'Strategic') NOT NULL,
    metric_type ENUM('Currency', 'Percentage', 'Count', 'Boolean') NOT NULL,
    target_value DECIMAL(18,4) NOT NULL,
    target_operator ENUM('GreaterThan', 'LessThan', 'Equals', 'GreaterOrEqual', 'LessOrEqual') NOT NULL,
    target_date DATE NOT NULL,
    measurement_frequency ENUM('Daily', 'Weekly', 'Monthly', 'Quarterly', 'Annually', 'OneTime') NOT NULL,
    -- Status tracking
    status ENUM('Pending', 'InProgress', 'Achieved', 'Failed', 'Cancelled') NOT NULL DEFAULT 'Pending',
    current_value DECIMAL(18,4) NULL,
    progress_percentage DECIMAL(5,2) NOT NULL DEFAULT 0,
    -- Achievement tracking
    achieved_at DATETIME(6) NULL,
    achieved_value DECIMAL(18,4) NULL,
    verified_at DATETIME(6) NULL,
    verified_by CHAR(36) NULL,
    -- Vesting acceleration
    vesting_acceleration_type ENUM('Percentage', 'Months', 'Shares') NOT NULL,
    acceleration_amount DECIMAL(8,4) NOT NULL,
    acceleration_applied BOOLEAN NOT NULL DEFAULT FALSE,
    acceleration_applied_at DATETIME(6) NULL,
    -- Audit fields
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    created_by CHAR(36) NOT NULL,
    updated_by CHAR(36) NOT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at DATETIME(6) NULL,
    deleted_by CHAR(36) NULL,
    -- Foreign Keys
    FOREIGN KEY (vesting_grant_id) REFERENCES vesting_grants(id) ON DELETE CASCADE,
    FOREIGN KEY (milestone_template_id) REFERENCES milestone_templates(id) ON DELETE SET NULL,
    FOREIGN KEY (created_by) REFERENCES users(id),
    FOREIGN KEY (updated_by) REFERENCES users(id),
    FOREIGN KEY (verified_by) REFERENCES users(id),
    FOREIGN KEY (deleted_by) REFERENCES users(id),
    -- Constraints
    CONSTRAINT chk_vesting_milestone_progress CHECK (progress_percentage >= 0 AND progress_percentage <= 100),
    CONSTRAINT chk_vesting_milestone_acceleration CHECK (acceleration_amount >= 0 AND acceleration_amount <= 100),
    CONSTRAINT chk_vesting_milestone_achieved CHECK (
        (status = 'Achieved' AND achieved_at IS NOT NULL AND achieved_value IS NOT NULL) OR 
        (status != 'Achieved')
    )
);

-- 041_create_milestone_progress_table.sql
CREATE TABLE milestone_progress (
    id CHAR(36) PRIMARY KEY,
    vesting_milestone_id CHAR(36) NOT NULL,
    recorded_date DATE NOT NULL,
    recorded_value DECIMAL(18,4) NOT NULL,
    progress_percentage DECIMAL(5,2) NOT NULL,
    notes TEXT,
    data_source VARCHAR(100), -- 'Manual', 'API Integration', 'System Calculation'
    recorded_by CHAR(36) NOT NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    -- Foreign Keys
    FOREIGN KEY (vesting_milestone_id) REFERENCES vesting_milestones(id) ON DELETE CASCADE,
    FOREIGN KEY (recorded_by) REFERENCES users(id),
    -- Index for time series queries
    INDEX idx_milestone_progress_date (vesting_milestone_id, recorded_date),
    -- Constraint
    CONSTRAINT chk_milestone_progress_percentage CHECK (progress_percentage >= 0 AND progress_percentage <= 100)
);

-- 042_create_vesting_accelerations_table.sql
CREATE TABLE vesting_accelerations (
    id CHAR(36) PRIMARY KEY,
    vesting_grant_id CHAR(36) NOT NULL,
    vesting_milestone_id CHAR(36) NOT NULL,
    acceleration_type ENUM('Percentage', 'Months', 'Shares') NOT NULL,
    acceleration_amount DECIMAL(8,4) NOT NULL,
    original_vesting_end_date DATE NOT NULL,
    new_vesting_end_date DATE NOT NULL,
    shares_accelerated DECIMAL(15,0) NOT NULL,
    applied_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    applied_by CHAR(36) NOT NULL,
    -- Audit
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    -- Foreign Keys
    FOREIGN KEY (vesting_grant_id) REFERENCES vesting_grants(id) ON DELETE CASCADE,
    FOREIGN KEY (vesting_milestone_id) REFERENCES vesting_milestones(id) ON DELETE CASCADE,
    FOREIGN KEY (applied_by) REFERENCES users(id),
    -- Constraints
    CONSTRAINT chk_vesting_acceleration_amount CHECK (acceleration_amount > 0),
    CONSTRAINT chk_vesting_acceleration_dates CHECK (new_vesting_end_date < original_vesting_end_date),
    CONSTRAINT chk_vesting_acceleration_shares CHECK (shares_accelerated > 0)
);
```

##### **Critérios de Aceite:**
- [ ] ✅ **4 tabelas criadas** (templates, milestones, progress, accelerations)
- [ ] ✅ **Foreign keys configuradas** e testadas
- [ ] ✅ **Constraints validadas** com dados inválidos
- [ ] ✅ **Índices otimizados** para queries de progresso
- [ ] ✅ **Seed data inserido** (10+ templates por categoria)

#### F4-MIL-BE-001: Entities para Milestones
- **Estimativa:** 3h
- **Responsável:** BE1
- **Dependências:** F4-MIL-DB-001

##### **Entities Obrigatórias:**
```csharp
public class MilestoneTemplate : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MilestoneCategory Category { get; set; }
    public MetricType MetricType { get; set; }
    public TargetOperator TargetOperator { get; set; }
    public MeasurementFrequency MeasurementFrequency { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Acceleration settings
    public VestingAccelerationType AccelerationType { get; set; }
    public decimal AccelerationAmount { get; set; }
    public decimal? MaxAccelerationCap { get; set; }
    
    // Navigation properties
    public virtual Company Company { get; set; } = null!;
    public virtual ICollection<VestingMilestone> Milestones { get; set; } = new List<VestingMilestone>();
}

public class VestingMilestone : BaseEntity
{
    public Guid VestingGrantId { get; set; }
    public Guid? MilestoneTemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MilestoneCategory Category { get; set; }
    public MetricType MetricType { get; set; }
    public decimal TargetValue { get; set; }
    public TargetOperator TargetOperator { get; set; }
    public DateTime TargetDate { get; set; }
    public MeasurementFrequency MeasurementFrequency { get; set; }
    
    // Status tracking
    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;
    public decimal? CurrentValue { get; set; }
    public decimal ProgressPercentage { get; set; } = 0;
    
    // Achievement tracking
    public DateTime? AchievedAt { get; set; }
    public decimal? AchievedValue { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedBy { get; set; }
    
    // Vesting acceleration
    public VestingAccelerationType AccelerationType { get; set; }
    public decimal AccelerationAmount { get; set; }
    public bool AccelerationApplied { get; set; } = false;
    public DateTime? AccelerationAppliedAt { get; set; }
    
    // Navigation properties
    public virtual VestingGrant VestingGrant { get; set; } = null!;
    public virtual MilestoneTemplate? Template { get; set; }
    public virtual User? VerifiedByUser { get; set; }
    public virtual ICollection<MilestoneProgress> ProgressHistory { get; set; } = new List<MilestoneProgress>();
    public virtual ICollection<VestingAcceleration> Accelerations { get; set; } = new List<VestingAcceleration>();
    
    // Business Methods
    public bool IsAchieved() => Status == MilestoneStatus.Achieved;
    public bool CanBeVerified() => Status == MilestoneStatus.Achieved && VerifiedAt == null;
    public decimal CalculateProgress(decimal currentValue)
    {
        if (TargetValue == 0) return 0;
        
        return TargetOperator switch
        {
            TargetOperator.GreaterThan => Math.Min((currentValue / TargetValue) * 100, 100),
            TargetOperator.LessThan => Math.Min(((TargetValue - currentValue) / TargetValue) * 100, 100),
            TargetOperator.Equals => currentValue == TargetValue ? 100 : 0,
            _ => 0
        };
    }
    
    public void UpdateProgress(decimal newValue, Guid updatedBy)
    {
        CurrentValue = newValue;
        ProgressPercentage = CalculateProgress(newValue);
        
        if (IsTargetMet(newValue) && Status != MilestoneStatus.Achieved)
        {
            Status = MilestoneStatus.Achieved;
            AchievedAt = DateTime.UtcNow;
            AchievedValue = newValue;
        }
        
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
    
    private bool IsTargetMet(decimal value)
    {
        return TargetOperator switch
        {
            TargetOperator.GreaterThan => value > TargetValue,
            TargetOperator.GreaterOrEqual => value >= TargetValue,
            TargetOperator.LessThan => value < TargetValue,
            TargetOperator.LessOrEqual => value <= TargetValue,
            TargetOperator.Equals => Math.Abs(value - TargetValue) < 0.01m,
            _ => false
        };
    }
}
```

##### **Enums Obrigatórios:**
```csharp
public enum MilestoneCategory
{
    Financial = 1,
    Operational = 2,
    Product = 3,
    Market = 4,
    Strategic = 5
}

public enum MilestoneStatus
{
    Pending = 1,
    InProgress = 2,
    Achieved = 3,
    Failed = 4,
    Cancelled = 5
}

public enum MetricType
{
    Currency = 1,    // Valores monetários
    Percentage = 2,  // Percentuais
    Count = 3,       // Contadores
    Boolean = 4      // Sim/Não
}

public enum TargetOperator
{
    GreaterThan = 1,
    LessThan = 2,
    Equals = 3,
    GreaterOrEqual = 4,
    LessOrEqual = 5
}

public enum VestingAccelerationType
{
    Percentage = 1,  // % do período total acelerado
    Months = 2,      // Número de meses acelerados
    Shares = 3       // Shares específicas desbloqueadas
}

public enum MeasurementFrequency
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Quarterly = 4,
    Annually = 5,
    OneTime = 6
}
```

#### F4-MIL-BE-002: Repositories para Milestones
- **Estimativa:** 2h
- **Responsável:** BE2
- **Dependências:** F4-MIL-BE-001

##### **Repositories Obrigatórios:**
- `IMilestoneTemplateRepository` + `MilestoneTemplateRepository`
- `IVestingMilestoneRepository` + `VestingMilestoneRepository`  
- `IMilestoneProgressRepository` + `MilestoneProgressRepository`

---

### **GRUPO 2: BUSINESS LOGIC & CALCULATIONS (10h)**

#### F4-MIL-BE-003: MilestoneTrackingService
- **Estimativa:** 4h
- **Responsável:** BE1
- **Dependências:** F4-MIL-BE-002

##### **Service Principal:**
```csharp
public class MilestoneTrackingService
{
    // Core milestone management
    public async Task<VestingMilestone> CreateMilestoneAsync(CreateMilestoneRequest request);
    public async Task<VestingMilestone> UpdateProgressAsync(Guid milestoneId, decimal newValue, Guid updatedBy);
    public async Task<VestingMilestone> MarkAsAchievedAsync(Guid milestoneId, decimal achievedValue, Guid userId);
    public async Task<bool> VerifyAchievementAsync(Guid milestoneId, Guid verifiedBy);
    
    // Progress tracking
    public async Task<List<MilestoneProgress>> GetProgressHistoryAsync(Guid milestoneId);
    public async Task RecordProgressAsync(Guid milestoneId, decimal value, string? notes, Guid recordedBy);
    public async Task<MilestoneProgress> GetLatestProgressAsync(Guid milestoneId);
    
    // Analytics & reporting
    public async Task<MilestoneAnalytics> GetMilestoneAnalyticsAsync(Guid grantId);
    public async Task<List<VestingMilestone>> GetMilestonesByStatusAsync(MilestoneStatus status, Guid companyId);
    public async Task<List<VestingMilestone>> GetUpcomingMilestonesAsync(DateTime beforeDate, Guid companyId);
    
    // Integration with vesting
    public async Task<bool> ShouldTriggerVestingAccelerationAsync(Guid milestoneId);
    public async Task<VestingAcceleration> CalculateAccelerationImpactAsync(Guid milestoneId);
}
```

#### F4-MIL-BE-004: VestingAccelerationEngine
- **Estimativa:** 6h (**TAREFA CRÍTICA**)
- **Responsável:** BE1
- **Dependências:** F4-MIL-BE-003, F4-ENG-BE-001

##### **Motor de Aceleração:**
```csharp
public class VestingAccelerationEngine
{
    public async Task<VestingAccelerationResult> CalculateAccelerationAsync(VestingMilestone milestone)
    {
        var grant = await _grantRepository.GetByIdAsync(milestone.VestingGrantId);
        var currentVesting = await _vestingEngine.Calculate(grant, DateTime.UtcNow);
        
        var acceleration = milestone.AccelerationType switch
        {
            VestingAccelerationType.Percentage => CalculatePercentageAcceleration(grant, milestone),
            VestingAccelerationType.Months => CalculateMonthsAcceleration(grant, milestone),
            VestingAccelerationType.Shares => CalculateSharesAcceleration(grant, milestone),
            _ => throw new ArgumentException("Invalid acceleration type")
        };
        
        return acceleration;
    }
    
    private VestingAccelerationResult CalculatePercentageAcceleration(VestingGrant grant, VestingMilestone milestone)
    {
        // Acelera X% do período total de vesting
        var totalPeriod = grant.VestingEndDate - grant.VestingStartDate;
        var accelerationDays = (int)(totalPeriod.TotalDays * (milestone.AccelerationAmount / 100m));
        var newEndDate = grant.VestingEndDate.AddDays(-accelerationDays);
        
        // Calcula shares aceleradas
        var totalDays = (grant.VestingEndDate - DateTime.UtcNow).TotalDays;
        var acceleratedShares = (decimal)(grant.TotalShares * (accelerationDays / totalDays));
        
        return new VestingAccelerationResult
        {
            AccelerationType = VestingAccelerationType.Percentage,
            OriginalEndDate = grant.VestingEndDate,
            NewEndDate = newEndDate,
            SharesAccelerated = acceleratedShares,
            AccelerationAmount = milestone.AccelerationAmount
        };
    }
    
    private VestingAccelerationResult CalculateMonthsAcceleration(VestingGrant grant, VestingMilestone milestone)
    {
        // Acelera X meses do cronograma
        var monthsToAccelerate = (int)milestone.AccelerationAmount;
        var newEndDate = grant.VestingEndDate.AddMonths(-monthsToAccelerate);
        
        // Calcula proporção de shares
        var originalMonths = ((grant.VestingEndDate - grant.VestingStartDate).Days / 30.44); // Média de dias por mês
        var acceleratedShares = grant.TotalShares * (monthsToAccelerate / (decimal)originalMonths);
        
        return new VestingAccelerationResult
        {
            AccelerationType = VestingAccelerationType.Months,
            OriginalEndDate = grant.VestingEndDate,
            NewEndDate = newEndDate,
            SharesAccelerated = acceleratedShares,
            AccelerationAmount = milestone.AccelerationAmount
        };
    }
    
    public async Task<bool> ApplyAccelerationAsync(Guid milestoneId, Guid approvedBy)
    {
        var milestone = await _milestoneRepository.GetByIdAsync(milestoneId);
        
        if (!milestone.CanBeAccelerated())
            throw new InvalidOperationException("Milestone cannot be accelerated");
            
        var acceleration = await CalculateAccelerationAsync(milestone);
        
        // Aplicar aceleração no grant
        var grant = await _grantRepository.GetByIdAsync(milestone.VestingGrantId);
        grant.VestingEndDate = acceleration.NewEndDate;
        
        // Registrar aceleração
        var accelerationRecord = new VestingAcceleration
        {
            Id = Guid.NewGuid(),
            VestingGrantId = grant.Id,
            VestingMilestoneId = milestone.Id,
            AccelerationType = acceleration.AccelerationType,
            AccelerationAmount = acceleration.AccelerationAmount,
            OriginalVestingEndDate = acceleration.OriginalEndDate,
            NewVestingEndDate = acceleration.NewEndDate,
            SharesAccelerated = acceleration.SharesAccelerated,
            AppliedBy = approvedBy
        };
        
        await _accelerationRepository.CreateAsync(accelerationRecord);
        await _grantRepository.UpdateAsync(grant);
        
        // Marcar milestone como aplicado
        milestone.AccelerationApplied = true;
        milestone.AccelerationAppliedAt = DateTime.UtcNow;
        await _milestoneRepository.UpdateAsync(milestone);
        
        // Integrar com cap table
        await _vestingIntegrationService.RecalculateCapTableAsync(grant.CompanyId);
        
        return true;
    }
}
```

##### **Testes Críticos para Motor de Aceleração:**
```csharp
[TestClass]
public class VestingAccelerationEngineTests
{
    [TestMethod]
    public async Task CalculatePercentageAcceleration_25Percent_ShouldAccelerateCorrectly()
    {
        // Arrange - Grant de 4 anos, milestone de 25% aceleração
        var grant = CreateGrant(startDate: new DateTime(2024, 1, 1), endDate: new DateTime(2028, 1, 1));
        var milestone = CreateMilestone(AccelerationType.Percentage, amount: 25m);
        
        // Act
        var result = await _engine.CalculateAccelerationAsync(milestone);
        
        // Assert
        var expectedNewEndDate = new DateTime(2027, 1, 1); // 1 ano acelerado
        Assert.AreEqual(expectedNewEndDate, result.NewEndDate);
        
        // Verificar shares aceleradas
        var expectedAcceleratedShares = grant.TotalShares * 0.25m;
        Assert.AreEqual(expectedAcceleratedShares, result.SharesAccelerated);
    }
    
    [TestMethod]
    public async Task CalculateMonthsAcceleration_12Months_ShouldReduceVestingPeriod()
    {
        // Arrange
        var grant = CreateGrant(startDate: new DateTime(2024, 1, 1), endDate: new DateTime(2028, 1, 1));
        var milestone = CreateMilestone(AccelerationType.Months, amount: 12m);
        
        // Act
        var result = await _engine.CalculateAccelerationAsync(milestone);
        
        // Assert
        var expectedNewEndDate = new DateTime(2027, 1, 1); // 12 meses acelerados
        Assert.AreEqual(expectedNewEndDate, result.NewEndDate);
    }
    
    [TestMethod]
    public async Task ApplyAcceleration_ValidMilestone_ShouldUpdateGrantAndCapTable()
    {
        // Arrange
        var milestone = await SeedAchievedMilestone();
        var originalGrant = await _grantRepository.GetByIdAsync(milestone.VestingGrantId);
        var originalEndDate = originalGrant.VestingEndDate;
        
        // Act
        var result = await _engine.ApplyAccelerationAsync(milestone.Id, Guid.NewGuid());
        
        // Assert
        Assert.IsTrue(result);
        
        var updatedGrant = await _grantRepository.GetByIdAsync(milestone.VestingGrantId);
        Assert.IsTrue(updatedGrant.VestingEndDate < originalEndDate);
        
        // Verificar registro de aceleração criado
        var accelerations = await _accelerationRepository.GetByMilestoneIdAsync(milestone.Id);
        Assert.AreEqual(1, accelerations.Count());
        
        // Verificar cap table recalculado
        var capTable = await _capTableService.GetCurrentAsync(originalGrant.CompanyId);
        Assert.IsTrue(capTable.LastCalculatedAt > DateTime.UtcNow.AddMinutes(-1));
    }
}
```

---

### **GRUPO 3: FRONTEND & UX (6h)**

#### F4-MIL-FE-001: Milestone Management Interface
- **Estimativa:** 4h
- **Responsável:** FE1
- **Dependências:** F4-MIL-BE-004

##### **Componentes Obrigatórios:**
- `MilestoneTemplateList.tsx` - Lista de templates
- `CreateMilestoneForm.tsx` - Criação de milestone
- `MilestoneProgressTracker.tsx` - Tracking de progresso
- `MilestoneCard.tsx` - Card individual de milestone
- `AccelerationCalculator.tsx` - Preview de aceleração

##### **Interface Principal:**
```typescript
// pages/vesting/milestones/index.tsx
export default function MilestonesPage() {
  const { data: milestones, isLoading } = useVestingMilestones();
  const { data: templates } = useMilestoneTemplates();

  return (
    <div className="space-y-6">
      <PageHeader
        title="Milestones de Performance"
        description="Gerencie metas vinculadas ao vesting"
        action={
          <Button onClick={() => setShowCreateModal(true)}>
            <Target className="w-4 h-4 mr-2" />
            Nova Meta
          </Button>
        }
      />
      
      {/* Filtros por categoria */}
      <MilestoneFilters />
      
      {/* Grid de milestones */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {milestones?.map(milestone => (
          <MilestoneCard 
            key={milestone.id}
            milestone={milestone}
            onUpdate={handleUpdate}
            onAccelerate={handleAccelerate}
          />
        ))}
      </div>
      
      {/* Modal de criação */}
      <CreateMilestoneModal 
        isOpen={showCreateModal}
        onClose={() => setShowCreateModal(false)}
        templates={templates}
      />
    </div>
  );
}
```

##### **Milestone Card com Progress:**
```typescript
export function MilestoneCard({ milestone, onUpdate, onAccelerate }: MilestoneCardProps) {
  const progressColor = getProgressColor(milestone.progressPercentage);
  const isAchieved = milestone.status === 'Achieved';
  
  return (
    <Card className={`transition-all hover:shadow-lg ${isAchieved ? 'ring-2 ring-green-500' : ''}`}>
      <Card.Header>
        <div className="flex items-start justify-between">
          <div>
            <h3 className="font-semibold text-lg">{milestone.name}</h3>
            <Badge variant={getMilestoneCategoryVariant(milestone.category)}>
              {milestone.category}
            </Badge>
          </div>
          <MilestoneStatusBadge status={milestone.status} />
        </div>
      </Card.Header>
      
      <Card.Body className="space-y-4">
        {/* Progress Bar */}
        <div className="space-y-2">
          <div className="flex justify-between text-sm">
            <span>Progresso</span>
            <span>{milestone.progressPercentage}%</span>
          </div>
          <ProgressBar 
            value={milestone.progressPercentage}
            color={progressColor}
            className="h-2"
          />
        </div>
        
        {/* Metric Display */}
        <div className="grid grid-cols-2 gap-4 text-sm">
          <div>
            <span className="text-gray-500">Meta</span>
            <div className="font-semibold">
              {formatMetricValue(milestone.targetValue, milestone.metricType)}
            </div>
          </div>
          <div>
            <span className="text-gray-500">Atual</span>
            <div className="font-semibold">
              {formatMetricValue(milestone.currentValue, milestone.metricType)}
            </div>
          </div>
        </div>
        
        {/* Target Date */}
        <div className="flex items-center text-sm text-gray-600">
          <Calendar className="w-4 h-4 mr-1" />
          Meta: {format(new Date(milestone.targetDate), 'dd/MM/yyyy')}
        </div>
        
        {/* Acceleration Preview */}
        {milestone.accelerationAmount > 0 && (
          <div className="bg-blue-50 p-3 rounded-lg">
            <div className="flex items-center text-sm text-blue-700">
              <Zap className="w-4 h-4 mr-1" />
              <span>Aceleração: {milestone.accelerationAmount}%</span>
            </div>
            <AccelerationPreview milestone={milestone} />
          </div>
        )}
      </Card.Body>
      
      <Card.Footer>
        <div className="flex gap-2">
          <Button 
            variant="outline" 
            size="sm"
            onClick={() => onUpdate(milestone)}
            disabled={isAchieved}
          >
            <Edit className="w-4 h-4 mr-1" />
            Atualizar
          </Button>
          
          {isAchieved && !milestone.accelerationApplied && (
            <Button 
              variant="primary"
              size="sm"
              onClick={() => onAccelerate(milestone)}
            >
              <Zap className="w-4 h-4 mr-1" />
              Acelerar Vesting
            </Button>
          )}
        </div>
      </Card.Footer>
    </Card>
  );
}
```

#### F4-MIL-FE-002: Progress Tracking Dashboard
- **Estimativa:** 2h
- **Responsável:** FE2
- **Dependências:** F4-MIL-FE-001

##### **Dashboard de Progresso:**
```typescript
export function MilestoneProgressDashboard({ grantId }: { grantId: string }) {
  const { data: milestones } = useGrantMilestones(grantId);
  const { data: analytics } = useMilestoneAnalytics(grantId);
  
  return (
    <div className="space-y-6">
      {/* Overview Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <MetricCard
          title="Metas Totais"
          value={analytics?.totalMilestones}
          icon={Target}
        />
        <MetricCard
          title="Conquistadas"
          value={analytics?.achievedMilestones}
          icon={CheckCircle}
          color="green"
        />
        <MetricCard
          title="Em Progresso"
          value={analytics?.inProgressMilestones}
          icon={Clock}
          color="blue"
        />
        <MetricCard
          title="Aceleração Total"
          value={`${analytics?.totalAcceleration}%`}
          icon={Zap}
          color="purple"
        />
      </div>
      
      {/* Timeline de Milestones */}
      <Card>
        <Card.Header>
          <h3 className="text-lg font-semibold">Timeline de Metas</h3>
        </Card.Header>
        <Card.Body>
          <MilestoneTimeline milestones={milestones} />
        </Card.Body>
      </Card>
      
      {/* Progress Chart */}
      <Card>
        <Card.Header>
          <h3 className="text-lg font-semibold">Progresso por Categoria</h3>
        </Card.Header>
        <Card.Body>
          <MilestoneCategoryChart milestones={milestones} />
        </Card.Body>
      </Card>
    </div>
  );
}
```

---

## 🧪 TESTES ESPECÍFICOS PARA MILESTONES

### **Testes Backend Obrigatórios:**

#### **Mathematical Tests:**
```csharp
[TestClass]
public class MilestoneAccelerationMathTests
{
    [TestMethod]
    public void PercentageAcceleration_25Percent_4YearGrant_ShouldAccelerate1Year()
    {
        // Test: 25% de aceleração em grant de 4 anos = 1 ano acelerado
    }
    
    [TestMethod]
    public void MonthsAcceleration_12Months_ShouldReduceEndDate()
    {
        // Test: 12 meses de aceleração devem reduzir data final
    }
    
    [TestMethod]
    public void SharesAcceleration_1000Shares_ShouldUnlock1000SharesImmediately()
    {
        // Test: 1000 shares de aceleração devem desbloquear imediatamente
    }
}
```

#### **Business Logic Tests:**
```csharp
[TestClass]
public class MilestoneBusinessLogicTests
{
    [TestMethod]
    public async Task AchieveMilestone_ShouldTriggerVestingRecalculation()
    {
        // Test: Achievement deve recalcular vesting automaticamente
    }
    
    [TestMethod]
    public async Task MultipleAccelerations_ShouldStackCorrectly()
    {
        // Test: Múltiplas acelerações devem se acumular corretamente
    }
    
    [TestMethod]
    public async Task FailedMilestone_ShouldNotAffectVesting()
    {
        // Test: Milestone falhou não deve afetar vesting
    }
}
```

### **Testes Frontend Obrigatórios:**

#### **E2E Tests:**
```typescript
test.describe('Milestone Management', () => {
  test('should create milestone and track progress', async ({ page }) => {
    // Test: Criar milestone e acompanhar progresso
    await page.goto('/vesting/milestones');
    await page.click('[data-testid="create-milestone"]');
    
    // Preencher formulário
    await page.fill('[data-testid="milestone-name"]', 'Revenue Milestone');
    await page.selectOption('[data-testid="category"]', 'Financial');
    await page.fill('[data-testid="target-value"]', '1000000');
    await page.fill('[data-testid="acceleration"]', '25');
    
    await page.click('[data-testid="save-milestone"]');
    
    // Verificar criação
    await expect(page.locator('.milestone-card')).toContainText('Revenue Milestone');
  });
  
  test('should apply acceleration when milestone achieved', async ({ page }) => {
    // Test: Aplicar aceleração quando milestone for conquistado
    const milestone = await seedAchievedMilestone();
    
    await page.goto(`/vesting/milestones/${milestone.id}`);
    await page.click('[data-testid="apply-acceleration"]');
    
    // Verificar confirmação
    await expect(page.locator('.success-toast')).toBeVisible();
    
    // Verificar vesting atualizado
    await page.goto('/vesting/grants');
    await expect(page.locator('.grant-end-date')).not.toContainText(originalEndDate);
  });
});
```

---

## 📋 INTEGRAÇÃO COM SISTEMA EXISTENTE

### **Modificações Necessárias:**

#### **VestingCalculationEngine (Integração):**
```csharp
// Adicionar ao engine existente
public async Task<VestingCalculationResult> CalculateWithMilestonesAsync(VestingGrant grant, DateTime asOfDate)
{
    // Cálculo base
    var baseResult = await Calculate(grant, asOfDate);
    
    // Aplicar acelerações de milestones
    var milestones = await _milestoneRepository.GetByGrantIdAsync(grant.Id);
    var appliedAccelerations = milestones.Where(m => m.AccelerationApplied);
    
    foreach (var milestone in appliedAccelerations)
    {
        baseResult = ApplyMilestoneAcceleration(baseResult, milestone);
    }
    
    return baseResult;
}
```

#### **VestingGrant Entity (Extensão):**
```csharp
// Adicionar à entity existente
public virtual ICollection<VestingMilestone> Milestones { get; set; } = new List<VestingMilestone>();

// Método para verificar milestones
public decimal GetTotalAcceleration()
{
    return Milestones
        .Where(m => m.AccelerationApplied)
        .Sum(m => m.AccelerationAmount);
}
```

---

## 🎯 CRONOGRAMA DE EXECUÇÃO

| Dia | Tarefas | Responsável | Horas |
|-----|---------|-------------|-------|
| **Dia 1** | F4-MIL-DB-001 + F4-MIL-BE-001 + F4-MIL-BE-002 | BE1 + BE2 | 8h |
| **Dia 2** | F4-MIL-BE-003 + F4-MIL-BE-004 (parte 1) | BE1 | 8h |
| **Dia 3** | F4-MIL-BE-004 (parte 2) + F4-MIL-FE-001 + F4-MIL-FE-002 | BE1 + FE1 + FE2 | 8h |

---

## ✅ CRITÉRIOS DE SUCESSO

### **Funcionalidades Implementadas:**
- [ ] ✅ **5 tipos de milestones** (Financial, Operational, Product, Market, Strategic)
- [ ] ✅ **3 tipos de aceleração** (Percentage, Months, Shares)
- [ ] ✅ **Progress tracking** em tempo real
- [ ] ✅ **Workflow de aprovação** de achievements
- [ ] ✅ **Cálculo automático** de aceleração
- [ ] ✅ **Integração cap table** atualizada
- [ ] ✅ **Dashboard visual** de progresso
- [ ] ✅ **Templates reutilizáveis** de milestones

### **Validações Técnicas:**
- [ ] ✅ **Testes matemáticos** > 95% cobertura
- [ ] ✅ **Performance** < 500ms para cálculos
- [ ] ✅ **E2E tests** cobrindo workflow completo
- [ ] ✅ **Mobile responsive** para tracking
- [ ] ✅ **API documentation** completa

---

## 🚀 IMPACTO NO SISTEMA

### **Benefícios:**
1. **Vesting Baseado em Performance** - Incentiva resultados
2. **Flexibilidade de Metas** - 5 categorias diferentes
3. **Transparência Total** - Dashboard de progresso
4. **Automation** - Cálculos e aplicação automática
5. **Compliance** - Workflow de aprovação auditável

### **Casos de Uso:**
- ✅ **Startup** com metas de revenue para founders
- ✅ **Scale-up** com milestones de produto para equipe
- ✅ **Corporate** com metas operacionais para executivos
- ✅ **Investment** com targets de market share

---

**🎯 RESULTADO: Sistema completo de milestones vinculados ao vesting, permitindo aceleração baseada em performance com tracking transparente e cálculos automáticos.**
