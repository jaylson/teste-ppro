using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Domain.Entities;

public class VestingPlan : BaseEntity
{
    public Guid ClientId { get; private set; }
    public Guid CompanyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public VestingType VestingType { get; private set; }
    public int CliffMonths { get; private set; }
    public int VestingMonths { get; private set; }
    public decimal TotalEquityPercentage { get; private set; }
    public VestingPlanStatus Status { get; private set; }
    public DateTime? ActivatedAt { get; private set; }
    public Guid? ActivatedBy { get; private set; }

    private VestingPlan() { }

    public static VestingPlan Create(
        Guid clientId,
        Guid companyId,
        string name,
        VestingType vestingType,
        int cliffMonths,
        int vestingMonths,
        decimal totalEquityPercentage,
        string? description = null,
        Guid? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome do plano é obrigatório.", nameof(name));

        if (cliffMonths < 0 || cliffMonths > 120)
            throw new ArgumentOutOfRangeException(nameof(cliffMonths), "Cliff deve ser entre 0 e 120 meses.");

        if (vestingMonths < 1 || vestingMonths > 240)
            throw new ArgumentOutOfRangeException(nameof(vestingMonths), "Período de vesting deve ser entre 1 e 240 meses.");

        if (totalEquityPercentage <= 0 || totalEquityPercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(totalEquityPercentage), "Percentual de equity deve ser entre 0 e 100.");

        if (cliffMonths >= vestingMonths)
            throw new ArgumentException("Cliff não pode ser maior ou igual ao período total de vesting.");

        return new VestingPlan
        {
            ClientId = clientId,
            CompanyId = companyId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            VestingType = vestingType,
            CliffMonths = cliffMonths,
            VestingMonths = vestingMonths,
            TotalEquityPercentage = totalEquityPercentage,
            Status = VestingPlanStatus.Draft,
            CreatedBy = createdBy,
            UpdatedBy = createdBy
        };
    }

    public bool CanBeActivated() => Status == VestingPlanStatus.Draft;
    public bool IsActive() => Status == VestingPlanStatus.Active;

    public void Activate(Guid userId)
    {
        if (!CanBeActivated())
            throw new InvalidOperationException($"Plano não pode ser ativado no status '{Status}'.");

        Status = VestingPlanStatus.Active;
        ActivatedAt = DateTime.UtcNow;
        ActivatedBy = userId;
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate(Guid userId)
    {
        if (Status != VestingPlanStatus.Active)
            throw new InvalidOperationException("Apenas planos ativos podem ser desativados.");

        Status = VestingPlanStatus.Inactive;
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive(Guid userId)
    {
        if (Status == VestingPlanStatus.Archived)
            throw new InvalidOperationException("Plano já está arquivado.");

        Status = VestingPlanStatus.Archived;
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(
        string name,
        string? description,
        int cliffMonths,
        int vestingMonths,
        decimal totalEquityPercentage,
        Guid updatedBy)
    {
        if (Status != VestingPlanStatus.Draft)
            throw new InvalidOperationException("Apenas planos em rascunho podem ser editados.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome do plano é obrigatório.", nameof(name));

        if (cliffMonths < 0 || cliffMonths > 120)
            throw new ArgumentOutOfRangeException(nameof(cliffMonths));

        if (vestingMonths < 1 || vestingMonths > 240)
            throw new ArgumentOutOfRangeException(nameof(vestingMonths));

        if (totalEquityPercentage <= 0 || totalEquityPercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(totalEquityPercentage));

        if (cliffMonths >= vestingMonths)
            throw new ArgumentException("Cliff não pode ser maior ou igual ao período total de vesting.");

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        CliffMonths = cliffMonths;
        VestingMonths = vestingMonths;
        TotalEquityPercentage = totalEquityPercentage;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reconstitutes a VestingPlan from persistence without domain validation.
    /// </summary>
    public static VestingPlan Reconstitute(
        Guid id,
        Guid clientId,
        Guid companyId,
        string name,
        string? description,
        VestingType vestingType,
        int cliffMonths,
        int vestingMonths,
        decimal totalEquityPercentage,
        VestingPlanStatus status,
        DateTime? activatedAt,
        Guid? activatedBy,
        Guid? createdBy,
        DateTime createdAt,
        DateTime updatedAt,
        bool isDeleted,
        DateTime? deletedAt)
    {
        return new VestingPlan
        {
            Id = id,
            ClientId = clientId,
            CompanyId = companyId,
            Name = name,
            Description = description,
            VestingType = vestingType,
            CliffMonths = cliffMonths,
            VestingMonths = vestingMonths,
            TotalEquityPercentage = totalEquityPercentage,
            Status = status,
            ActivatedAt = activatedAt,
            ActivatedBy = activatedBy,
            CreatedBy = createdBy,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            IsDeleted = isDeleted,
            DeletedAt = deletedAt
        };
    }
}
