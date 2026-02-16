using FluentValidation;
using PartnershipManager.Application.Features.Contracts.DTOs;

namespace PartnershipManager.Application.Features.Contracts.Validators;

/// <summary>
/// Validator for CreateClauseRequest </summary>
public class CreateClauseValidator : AbstractValidator<CreateClauseRequest>
{
    public CreateClauseValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome da cláusula é obrigatório")
            .MaximumLength(200)
            .WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Código da cláusula é obrigatório")
            .MaximumLength(50)
            .WithMessage("Código deve ter no máximo 50 caracteres")
            .Matches("^[A-Z0-9-_]+$")
            .WithMessage("Código deve conter apenas letras maiúsculas, números, hífens e underscores");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Conteúdo da cláusula é obrigatório")
            .MinimumLength(10)
            .WithMessage("Conteúdo deve ter no mínimo 10 caracteres");

        RuleFor(x => x.ClauseType)
            .IsInEnum()
            .WithMessage("Tipo de cláusula inválido");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Ordem de exibição deve ser maior ou igual a zero");

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
/// Validator for UpdateClauseRequest
/// </summary>
public class UpdateClauseValidator : AbstractValidator<UpdateClauseRequest>
{
    public UpdateClauseValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome da cláusula é obrigatório")
            .MaximumLength(200)
            .WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Conteúdo da cláusula é obrigatório")
            .MinimumLength(10)
            .WithMessage("Conteúdo deve ter no mínimo 10 caracteres");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Ordem de exibição deve ser maior ou igual a zero");

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
