using FluentValidation;
using PartnershipManager.Application.Features.Contracts.DTOs;

namespace PartnershipManager.Application.Features.Contracts.Validators;

/// <summary>
/// Validator for CreateContractRequest
/// </summary>
public class CreateContractValidator : AbstractValidator<CreateContractRequest>
{
    public CreateContractValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("ID da empresa é obrigatório");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Título do contrato é obrigatório")
            .MaximumLength(300)
            .WithMessage("Título deve ter no máximo 300 caracteres");

        RuleFor(x => x.ContractType)
            .IsInEnum()
            .WithMessage("Tipo de contrato inválido");

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.ExpirationDate.HasValue)
            .WithMessage("Data de expiração deve ser futura");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("Descrição deve ter no máximo 2000 caracteres");
    }
}

/// <summary>
/// Validator for UpdateContractRequest
/// </summary>
public class UpdateContractValidator : AbstractValidator<UpdateContractRequest>
{
    public UpdateContractValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Título do contrato é obrigatório")
            .MaximumLength(300)
            .WithMessage("Título deve ter no máximo 300 caracteres");

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.ExpirationDate.HasValue)
            .WithMessage("Data de expiração deve ser futura");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("Descrição deve ter no máximo 2000 caracteres");
    }
}

/// <summary>
/// Validator for AddContractPartyRequest
/// </summary>
public class AddContractPartyValidator : AbstractValidator<AddContractPartyRequest>
{
    public AddContractPartyValidator()
    {
        RuleFor(x => x.PartyType)
            .NotEmpty()
            .WithMessage("Tipo de parte é obrigatório")
            .Must(type => new[] { "signer", "recipient", "witness" }.Contains(type.ToLower()))
            .WithMessage("Tipo de parte inválido. Use: signer, recipient ou witness");

        RuleFor(x => x.PartyName)
            .NotEmpty()
            .WithMessage("Nome da parte é obrigatório")
            .MaximumLength(200)
            .WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.PartyEmail)
            .NotEmpty()
            .WithMessage("E-mail da parte é obrigatório")
            .EmailAddress()
            .WithMessage("E-mail inválido")
            .MaximumLength(200)
            .WithMessage("E-mail deve ter no máximo 200 caracteres");

        RuleFor(x => x.SequenceOrder)
            .GreaterThan(0)
            .WithMessage("Ordem de sequência deve ser maior que zero");

        // UserId e ShareholderId são opcionais, mas se fornecidos devem ser válidos
        When(x => x.UserId.HasValue || x.ShareholderId.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(x => !(x.UserId.HasValue && x.ShareholderId.HasValue))
                .WithMessage("Não é possível vincular a parte a um usuário e um sócio simultaneamente");
        });
    }
}

/// <summary>
/// Validator for AddContractClauseRequest
/// </summary>
public class AddContractClauseValidator : AbstractValidator<AddContractClauseRequest>
{
    public AddContractClauseValidator()
    {
        RuleFor(x => x.ClauseId)
            .NotEmpty()
            .WithMessage("ID da cláusula é obrigatório");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Ordem de exibição deve ser maior ou igual a zero");

        RuleFor(x => x.CustomContent)
            .MinimumLength(10)
            .When(x => !string.IsNullOrWhiteSpace(x.CustomContent))
            .WithMessage("Conteúdo customizado deve ter no mínimo 10 caracteres");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage("Notas devem ter no máximo 1000 caracteres");
    }
}

/// <summary>
/// Validator for UpdateContractStatusRequest
/// </summary>
public class UpdateContractStatusValidator : AbstractValidator<UpdateContractStatusRequest>
{
    public UpdateContractStatusValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status inválido");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .When(x => x.Status == Domain.Enums.ContractStatus.Cancelled)
            .WithMessage("Motivo é obrigatório ao cancelar um contrato");
            
        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Reason))
            .WithMessage("Motivo deve ter no máximo 500 caracteres");
    }
}

/// <summary>
/// Validator for AttachDocumentRequest
/// </summary>
public class AttachDocumentValidator : AbstractValidator<AttachDocumentRequest>
{
    public AttachDocumentValidator()
    {
        RuleFor(x => x.DocumentPath)
            .NotEmpty()
            .WithMessage("Caminho do documento é obrigatório")
            .MaximumLength(500)
            .WithMessage("Caminho deve ter no máximo 500 caracteres");

        RuleFor(x => x.DocumentSize)
            .GreaterThan(0)
            .WithMessage("Tamanho do documento deve ser maior que zero")
            .LessThanOrEqualTo(50 * 1024 * 1024) // 50MB
            .WithMessage("Tamanho do documento não pode exceder 50MB");

        RuleFor(x => x.DocumentHash)
            .NotEmpty()
            .WithMessage("Hash do documento é obrigatório para verificação de integridade")
            .MinimumLength(32)
            .WithMessage("Hash do documento inválido (muito curto)")
            .MaximumLength(128)
            .WithMessage("Hash do documento inválido (muito longo)");
    }
}
