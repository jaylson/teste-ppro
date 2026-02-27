using FluentValidation;
using PartnershipManager.Application.Features.Vesting.DTOs;
using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Application.Features.Vesting.Validators;

public class CreateVestingPlanValidator : AbstractValidator<CreateVestingPlanRequest>
{
    public CreateVestingPlanValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("Empresa é obrigatória.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome do plano é obrigatório.")
            .MaximumLength(200).WithMessage("Nome não pode ultrapassar 200 caracteres.");

        RuleFor(x => x.VestingType)
            .IsInEnum().WithMessage("Tipo de vesting inválido.");

        RuleFor(x => x.CliffMonths)
            .InclusiveBetween(0, 120).WithMessage("Cliff deve ser entre 0 e 120 meses.");

        RuleFor(x => x.VestingMonths)
            .InclusiveBetween(1, 240).WithMessage("Período de vesting deve ser entre 1 e 240 meses.");

        RuleFor(x => x)
            .Must(x => x.CliffMonths < x.VestingMonths)
            .WithMessage("O cliff não pode ser maior ou igual ao período total de vesting.")
            .WithName("CliffMonths");

        RuleFor(x => x.TotalEquityPercentage)
            .GreaterThan(0).WithMessage("Percentual de equity deve ser maior que 0.")
            .LessThanOrEqualTo(100).WithMessage("Percentual de equity não pode ultrapassar 100%.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Descrição não pode ultrapassar 2000 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

public class UpdateVestingPlanValidator : AbstractValidator<UpdateVestingPlanRequest>
{
    public UpdateVestingPlanValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome do plano é obrigatório.")
            .MaximumLength(200).WithMessage("Nome não pode ultrapassar 200 caracteres.");

        RuleFor(x => x.CliffMonths)
            .InclusiveBetween(0, 120).WithMessage("Cliff deve ser entre 0 e 120 meses.");

        RuleFor(x => x.VestingMonths)
            .InclusiveBetween(1, 240).WithMessage("Período de vesting deve ser entre 1 e 240 meses.");

        RuleFor(x => x)
            .Must(x => x.CliffMonths < x.VestingMonths)
            .WithMessage("O cliff não pode ser maior ou igual ao período total de vesting.")
            .WithName("CliffMonths");

        RuleFor(x => x.TotalEquityPercentage)
            .GreaterThan(0).WithMessage("Percentual de equity deve ser maior que 0.")
            .LessThanOrEqualTo(100).WithMessage("Percentual de equity não pode ultrapassar 100%.");
    }
}

public class CreateVestingGrantValidator : AbstractValidator<CreateVestingGrantRequest>
{
    public CreateVestingGrantValidator()
    {
        RuleFor(x => x.VestingPlanId)
            .NotEmpty().WithMessage("Plano de vesting é obrigatório.");

        RuleFor(x => x.ShareholderId)
            .NotEmpty().WithMessage("Sócio é obrigatório.");

        RuleFor(x => x.GrantDate)
            .NotEmpty().WithMessage("Data do grant é obrigatória.")
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
            .WithMessage("Data do grant não pode ser futura.");

        RuleFor(x => x.TotalShares)
            .GreaterThan(0).WithMessage("Total de ações deve ser positivo.");

        RuleFor(x => x.SharePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Preço de exercício não pode ser negativo.");

        RuleFor(x => x.EquityPercentage)
            .GreaterThan(0).WithMessage("Percentual de equity deve ser maior que 0.")
            .LessThanOrEqualTo(100).WithMessage("Percentual de equity não pode ultrapassar 100%.");

        RuleFor(x => x.VestingStartDate)
            .NotEmpty().WithMessage("Data de início do vesting é obrigatória.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notas não podem ultrapassar 1000 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}

public class ExerciseSharesValidator : AbstractValidator<ExerciseSharesRequest>
{
    public ExerciseSharesValidator()
    {
        RuleFor(x => x.SharesToExercise)
            .GreaterThan(0).WithMessage("Quantidade de ações a exercer deve ser positiva.");

        RuleFor(x => x.SharePriceAtExercise)
            .GreaterThan(0).WithMessage("Preço de mercado no exercício deve ser positivo.");

        RuleFor(x => x.ExerciseDate)
            .NotEmpty().WithMessage("Data do exercício é obrigatória.")
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
            .WithMessage("Data do exercício não pode ser futura.");

        RuleFor(x => x.TransactionType)
            .IsInEnum().WithMessage("Tipo de transação inválido.");
    }
}

public class CreateVestingMilestoneValidator : AbstractValidator<CreateVestingMilestoneRequest>
{
    public CreateVestingMilestoneValidator()
    {
        RuleFor(x => x.VestingPlanId)
            .NotEmpty().WithMessage("Plano de vesting é obrigatório.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome do milestone é obrigatório.")
            .MaximumLength(200).WithMessage("Nome não pode ultrapassar 200 caracteres.");

        RuleFor(x => x.MilestoneType)
            .IsInEnum().WithMessage("Tipo de milestone inválido.");

        RuleFor(x => x.AccelerationPercentage)
            .InclusiveBetween(0, 100).WithMessage("Percentual de aceleração deve ser entre 0 e 100.");

        RuleFor(x => x.TargetDate)
            .GreaterThan(DateTime.UtcNow.Date)
            .WithMessage("Data alvo deve ser futura.")
            .When(x => x.TargetDate.HasValue);

        RuleFor(x => x.TargetUnit)
            .MaximumLength(50).WithMessage("Unidade não pode ultrapassar 50 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.TargetUnit));
    }
}

public class AchieveMilestoneValidator : AbstractValidator<AchieveMilestoneRequest>
{
    public AchieveMilestoneValidator()
    {
        RuleFor(x => x.AchievedDate)
            .NotEmpty().WithMessage("Data de atingimento é obrigatória.")
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
            .WithMessage("Data de atingimento não pode ser futura.");
    }
}
// ─── Grant Milestones Validators ──────────────────────────────────────────────

public class CreateMilestoneTemplateValidator : AbstractValidator<CreateMilestoneTemplateRequest>
{
    public CreateMilestoneTemplateValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("Empresa é obrigatória.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome do template é obrigatório.")
            .MaximumLength(200).WithMessage("Nome não pode ultrapassar 200 caracteres.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Categoria inválida.");

        RuleFor(x => x.MetricType)
            .IsInEnum().WithMessage("Tipo de métrica inválido.");

        RuleFor(x => x.TargetOperator)
            .IsInEnum().WithMessage("Operador alvo inválido.");

        RuleFor(x => x.MeasurementFrequency)
            .IsInEnum().WithMessage("Frequência de medição inválida.");

        RuleFor(x => x.AccelerationType)
            .IsInEnum().WithMessage("Tipo de aceleração inválido.");

        RuleFor(x => x.AccelerationAmount)
            .GreaterThan(0).WithMessage("Valor de aceleração deve ser maior que 0.")
            .LessThanOrEqualTo(100).WithMessage("Valor de aceleração não pode ultrapassar 100.");

        RuleFor(x => x.MaxAccelerationCap)
            .GreaterThanOrEqualTo(x => x.AccelerationAmount)
            .WithMessage("Cap máximo não pode ser menor que o valor de aceleração individual.")
            .LessThanOrEqualTo(100).WithMessage("Cap máximo não pode ultrapassar 100.")
            .When(x => x.MaxAccelerationCap.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Descrição não pode ultrapassar 2000 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

public class UpdateMilestoneTemplateValidator : AbstractValidator<UpdateMilestoneTemplateRequest>
{
    public UpdateMilestoneTemplateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome do template é obrigatório.")
            .MaximumLength(200).WithMessage("Nome não pode ultrapassar 200 caracteres.");

        RuleFor(x => x.Category).IsInEnum().WithMessage("Categoria inválida.");
        RuleFor(x => x.MetricType).IsInEnum().WithMessage("Tipo de métrica inválido.");
        RuleFor(x => x.TargetOperator).IsInEnum().WithMessage("Operador alvo inválido.");
        RuleFor(x => x.MeasurementFrequency).IsInEnum().WithMessage("Frequência de medição inválida.");
        RuleFor(x => x.AccelerationType).IsInEnum().WithMessage("Tipo de aceleração inválido.");

        RuleFor(x => x.AccelerationAmount)
            .GreaterThan(0).WithMessage("Valor de aceleração deve ser maior que 0.")
            .LessThanOrEqualTo(100).WithMessage("Valor de aceleração não pode ultrapassar 100.");

        RuleFor(x => x.MaxAccelerationCap)
            .GreaterThanOrEqualTo(x => x.AccelerationAmount)
            .WithMessage("Cap máximo não pode ser menor que o valor de aceleração individual.")
            .When(x => x.MaxAccelerationCap.HasValue);
    }
}

public class CreateGrantMilestoneValidator : AbstractValidator<CreateGrantMilestoneRequest>
{
    public CreateGrantMilestoneValidator()
    {
        RuleFor(x => x.VestingGrantId)
            .NotEmpty().WithMessage("Grant de vesting é obrigatório.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome do milestone é obrigatório.")
            .MaximumLength(200).WithMessage("Nome não pode ultrapassar 200 caracteres.");

        RuleFor(x => x.Category).IsInEnum().WithMessage("Categoria inválida.");
        RuleFor(x => x.MetricType).IsInEnum().WithMessage("Tipo de métrica inválido.");
        RuleFor(x => x.TargetOperator).IsInEnum().WithMessage("Operador alvo inválido.");
        RuleFor(x => x.MeasurementFrequency).IsInEnum().WithMessage("Frequência de medição inválida.");
        RuleFor(x => x.AccelerationType).IsInEnum().WithMessage("Tipo de aceleração inválido.");

        RuleFor(x => x.TargetValue)
            .GreaterThan(0).WithMessage("Valor alvo deve ser positivo.");

        RuleFor(x => x.TargetDate)
            .NotEmpty().WithMessage("Data alvo é obrigatória.")
            .GreaterThan(DateTime.UtcNow.Date).WithMessage("Data alvo deve ser futura.");

        RuleFor(x => x.AccelerationAmount)
            .GreaterThan(0).WithMessage("Valor de aceleração deve ser maior que 0.")
            .LessThanOrEqualTo(100).WithMessage("Valor de aceleração não pode ultrapassar 100.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Descrição não pode ultrapassar 2000 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

public class AchieveGrantMilestoneValidator : AbstractValidator<AchieveGrantMilestoneRequest>
{
    public AchieveGrantMilestoneValidator()
    {
        RuleFor(x => x.AchievedValue)
            .GreaterThanOrEqualTo(0).WithMessage("Valor atingido não pode ser negativo.");
    }
}

public class RecordMilestoneProgressValidator : AbstractValidator<RecordMilestoneProgressRequest>
{
    public RecordMilestoneProgressValidator()
    {
        RuleFor(x => x.RecordedDate)
            .NotEmpty().WithMessage("Data de registro é obrigatória.")
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
            .WithMessage("Data de registro não pode ser futura.");

        RuleFor(x => x.RecordedValue)
            .GreaterThanOrEqualTo(0).WithMessage("Valor registrado não pode ser negativo.");

        RuleFor(x => x.DataSource)
            .IsInEnum().WithMessage("Fonte de dados inválida.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notas não podem ultrapassar 1000 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}