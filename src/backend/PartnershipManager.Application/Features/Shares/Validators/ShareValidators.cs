using FluentValidation;
using PartnershipManager.Application.Features.Shares.DTOs;

namespace PartnershipManager.Application.Features.Shares.Validators;

public class IssueSharesValidator : AbstractValidator<IssueSharesRequest>
{
    public IssueSharesValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("ID da empresa é obrigatório");

        RuleFor(x => x.ShareholderId)
            .NotEmpty()
            .WithMessage("ID do sócio é obrigatório");

        RuleFor(x => x.ShareClassId)
            .NotEmpty()
            .WithMessage("ID da classe de ações é obrigatório");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantidade deve ser maior que zero");

        RuleFor(x => x.PricePerShare)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Preço por ação não pode ser negativo");

        RuleFor(x => x.ReferenceDate)
            .NotEmpty()
            .WithMessage("Data de referência é obrigatória")
            .LessThanOrEqualTo(DateTime.Today.AddDays(1))
            .WithMessage("Data de referência não pode ser futura");

        RuleFor(x => x.CertificateNumber)
            .MaximumLength(50)
            .WithMessage("Número do certificado deve ter no máximo 50 caracteres");

        RuleFor(x => x.TransactionNumber)
            .MaximumLength(50)
            .WithMessage("Número da transação deve ter no máximo 50 caracteres");

        RuleFor(x => x.Reason)
            .MaximumLength(200)
            .WithMessage("Motivo deve ter no máximo 200 caracteres");

        RuleFor(x => x.DocumentReference)
            .MaximumLength(200)
            .WithMessage("Referência do documento deve ter no máximo 200 caracteres");
    }
}

public class TransferSharesValidator : AbstractValidator<TransferSharesRequest>
{
    public TransferSharesValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("ID da empresa é obrigatório");

        RuleFor(x => x.FromShareholderId)
            .NotEmpty()
            .WithMessage("ID do sócio de origem é obrigatório");

        RuleFor(x => x.ToShareholderId)
            .NotEmpty()
            .WithMessage("ID do sócio de destino é obrigatório")
            .NotEqual(x => x.FromShareholderId)
            .WithMessage("Sócio de origem e destino devem ser diferentes");

        RuleFor(x => x.ShareClassId)
            .NotEmpty()
            .WithMessage("ID da classe de ações é obrigatório");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantidade deve ser maior que zero");

        RuleFor(x => x.PricePerShare)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Preço por ação não pode ser negativo");

        RuleFor(x => x.ReferenceDate)
            .NotEmpty()
            .WithMessage("Data de referência é obrigatória")
            .LessThanOrEqualTo(DateTime.Today.AddDays(1))
            .WithMessage("Data de referência não pode ser futura");
    }
}

public class CancelSharesValidator : AbstractValidator<CancelSharesRequest>
{
    public CancelSharesValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("ID da empresa é obrigatório");

        RuleFor(x => x.ShareholderId)
            .NotEmpty()
            .WithMessage("ID do sócio é obrigatório");

        RuleFor(x => x.ShareClassId)
            .NotEmpty()
            .WithMessage("ID da classe de ações é obrigatório");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantidade deve ser maior que zero");

        RuleFor(x => x.ReferenceDate)
            .NotEmpty()
            .WithMessage("Data de referência é obrigatória");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Motivo do cancelamento é obrigatório")
            .MaximumLength(200)
            .WithMessage("Motivo deve ter no máximo 200 caracteres");
    }
}

public class ConvertSharesValidator : AbstractValidator<ConvertSharesRequest>
{
    public ConvertSharesValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("ID da empresa é obrigatório");

        RuleFor(x => x.ShareholderId)
            .NotEmpty()
            .WithMessage("ID do sócio é obrigatório");

        RuleFor(x => x.FromShareClassId)
            .NotEmpty()
            .WithMessage("ID da classe de origem é obrigatório");

        RuleFor(x => x.ToShareClassId)
            .NotEmpty()
            .WithMessage("ID da classe de destino é obrigatório")
            .NotEqual(x => x.FromShareClassId)
            .WithMessage("Classes de origem e destino devem ser diferentes");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantidade deve ser maior que zero");

        RuleFor(x => x.ReferenceDate)
            .NotEmpty()
            .WithMessage("Data de referência é obrigatória");
    }
}

public class CreateShareValidator : AbstractValidator<CreateShareRequest>
{
    public CreateShareValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("ID da empresa é obrigatório");

        RuleFor(x => x.ShareholderId)
            .NotEmpty()
            .WithMessage("ID do sócio é obrigatório");

        RuleFor(x => x.ShareClassId)
            .NotEmpty()
            .WithMessage("ID da classe de ações é obrigatório");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantidade deve ser maior que zero");

        RuleFor(x => x.AcquisitionPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Preço de aquisição não pode ser negativo");

        RuleFor(x => x.AcquisitionDate)
            .NotEmpty()
            .WithMessage("Data de aquisição é obrigatória");
    }
}
