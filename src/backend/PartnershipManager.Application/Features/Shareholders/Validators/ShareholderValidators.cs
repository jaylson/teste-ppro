using FluentValidation;
using PartnershipManager.Application.Features.Shareholders.DTOs;
using PartnershipManager.Domain.Constants;
using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Application.Features.Shareholders.Validators;

public class CreateShareholderValidator : AbstractValidator<CreateShareholderRequest>
{
    public CreateShareholderValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(SystemConstants.MaxNameLength);

        RuleFor(x => x.Document)
            .NotEmpty()
            .Must((req, document) => ValidateDocument(document, req.DocumentType))
            .WithMessage("Documento inválido para o tipo informado");

        RuleFor(x => x.DocumentType)
            .IsInEnum();

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }

    private static bool ValidateDocument(string document, DocumentType type)
    {
        var clean = new string((document ?? string.Empty).Where(char.IsDigit).ToArray());
        return type switch
        {
            DocumentType.Cpf => clean.Length == SystemConstants.CpfLength,
            DocumentType.Cnpj => clean.Length == SystemConstants.CnpjLength,
            _ => false
        };
    }
}

public class UpdateShareholderValidator : AbstractValidator<UpdateShareholderRequest>
{
    public UpdateShareholderValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(SystemConstants.MaxNameLength);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        When(x => !string.IsNullOrWhiteSpace(x.Document) || x.DocumentType.HasValue, () =>
        {
            RuleFor(x => x.Document)
                .NotEmpty();

            RuleFor(x => x.DocumentType)
                .NotNull();

            RuleFor(x => x)
                .Must(x => ValidateDocument(x.Document ?? string.Empty, x.DocumentType ?? DocumentType.Cpf))
                .WithMessage("Documento inválido para o tipo informado");
        });
    }

    private static bool ValidateDocument(string document, DocumentType type)
    {
        var clean = new string((document ?? string.Empty).Where(char.IsDigit).ToArray());
        return type switch
        {
            DocumentType.Cpf => clean.Length == SystemConstants.CpfLength,
            DocumentType.Cnpj => clean.Length == SystemConstants.CnpjLength,
            _ => false
        };
    }
}
