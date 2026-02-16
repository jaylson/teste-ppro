using FluentValidation;
using PartnershipManager.Application.Features.Contracts.DTOs;

namespace PartnershipManager.Application.Features.Contracts.Validators;

/// <summary>
/// Validator for CreateContractTemplateRequest
/// </summary>
public class CreateContractTemplateValidator : AbstractValidator<CreateContractTemplateRequest>
{
    public CreateContractTemplateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome do template é obrigatório")
            .MaximumLength(200)
            .WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Código do template é obrigatório")
            .MaximumLength(50)
            .WithMessage("Código deve ter no máximo 50 caracteres")
            .Matches("^[A-Z0-9-_]+$")
            .WithMessage("Código deve conter apenas letras maiúsculas, números, hífens e underscores");

        RuleFor(x => x.TemplateType)
            .IsInEnum()
            .WithMessage("Tipo de template inválido");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Conteúdo do template é obrigatório")
            .MinimumLength(10)
            .WithMessage("Conteúdo deve ter no mínimo 10 caracteres");

        RuleFor(x => x.DefaultStatus)
            .IsInEnum()
            .WithMessage("Status padrão inválido");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("Descrição deve ter no máximo 1000 caracteres");

        RuleFor(x => x.Tags)
            .Must(tags => tags.Count <= 10)
            .WithMessage("Máximo de 10 tags permitidas")
            .Must(tags => tags.All(t => t.Length <= 50))
            .When(x => x.Tags.Any())
            .WithMessage("Cada tag deve ter no máximo 50 caracteres");
    }
}

/// <summary>
/// Validator for UpdateContractTemplateRequest
/// </summary>
public class UpdateContractTemplateValidator : AbstractValidator<UpdateContractTemplateRequest>
{
    public UpdateContractTemplateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome do template é obrigatório")
            .MaximumLength(200)
            .WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Conteúdo do template é obrigatório")
            .MinimumLength(10)
            .WithMessage("Conteúdo deve ter no mínimo 10 caracteres");

        RuleFor(x => x.DefaultStatus)
            .IsInEnum()
            .WithMessage("Status padrão inválido");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("Descrição deve ter no máximo 1000 caracteres");

        RuleFor(x => x.Tags)
            .Must(tags => tags.Count <= 10)
            .WithMessage("Máximo de 10 tags permitidas")
            .Must(tags => tags.All(t => t.Length <= 50))
            .When(x => x.Tags.Any())
            .WithMessage("Cada tag deve ter no máximo 50 caracteres");
    }
}

/// <summary>
/// Validator for CloneContractTemplateRequest
/// </summary>
public class CloneContractTemplateValidator : AbstractValidator<CloneContractTemplateRequest>
{
    public CloneContractTemplateValidator()
    {
        RuleFor(x => x.NewCode)
            .NotEmpty()
            .WithMessage("Novo código é obrigatório")
            .MaximumLength(50)
            .WithMessage("Código deve ter no máximo 50 caracteres")
            .Matches("^[A-Z0-9-_]+$")
            .WithMessage("Código deve conter apenas letras maiúsculas, números, hífens e underscores");

        RuleFor(x => x.NewName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.NewName))
            .WithMessage("Nome deve ter no máximo 200 caracteres");
    }
}
