using FluentValidation;
using PartnershipManager.Application.Features.Auth.DTOs;
using PartnershipManager.Domain.Constants;

namespace PartnershipManager.Application.Features.Auth.Validators;

/// <summary>
/// Validador para LoginRequest
/// </summary>
public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(string.Format(ErrorMessages.Required, "Email"))
            .EmailAddress().WithMessage(ErrorMessages.InvalidEmail)
            .MaximumLength(SystemConstants.MaxEmailLength)
                .WithMessage(string.Format(ErrorMessages.MaxLength, "Email", SystemConstants.MaxEmailLength));
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ErrorMessages.PasswordRequired);
        
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage(string.Format(ErrorMessages.Required, "Empresa"));
    }
}

/// <summary>
/// Validador para RegisterRequest
/// </summary>
public class RegisterValidator : AbstractValidator<RegisterRequest>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ErrorMessages.NameRequired)
            .MaximumLength(SystemConstants.MaxNameLength)
                .WithMessage(string.Format(ErrorMessages.MaxLength, "Nome", SystemConstants.MaxNameLength));
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(string.Format(ErrorMessages.Required, "Email"))
            .EmailAddress().WithMessage(ErrorMessages.InvalidEmail)
            .MaximumLength(SystemConstants.MaxEmailLength)
                .WithMessage(string.Format(ErrorMessages.MaxLength, "Email", SystemConstants.MaxEmailLength));
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ErrorMessages.PasswordRequired)
            .MinimumLength(SystemConstants.MinPasswordLength)
                .WithMessage(string.Format(ErrorMessages.PasswordMinLength, SystemConstants.MinPasswordLength))
            .MaximumLength(SystemConstants.MaxPasswordLength)
                .WithMessage(string.Format(ErrorMessages.PasswordMaxLength, SystemConstants.MaxPasswordLength))
            .Matches("[A-Z]").WithMessage(ErrorMessages.PasswordUppercase)
            .Matches("[a-z]").WithMessage(ErrorMessages.PasswordLowercase)
            .Matches("[0-9]").WithMessage(ErrorMessages.PasswordNumber)
            .Matches("[^a-zA-Z0-9]").WithMessage(ErrorMessages.PasswordSpecial);
        
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage(ErrorMessages.PasswordMismatch);
        
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage(string.Format(ErrorMessages.Required, "Empresa"));
    }
}

/// <summary>
/// Validador para ChangePasswordRequest
/// </summary>
public class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage(string.Format(ErrorMessages.Required, "Senha atual"));
        
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(string.Format(ErrorMessages.Required, "Nova senha"))
            .MinimumLength(SystemConstants.MinPasswordLength)
                .WithMessage(string.Format(ErrorMessages.PasswordMinLength, SystemConstants.MinPasswordLength))
            .MaximumLength(SystemConstants.MaxPasswordLength)
                .WithMessage(string.Format(ErrorMessages.PasswordMaxLength, SystemConstants.MaxPasswordLength))
            .Matches("[A-Z]").WithMessage(ErrorMessages.PasswordUppercase)
            .Matches("[a-z]").WithMessage(ErrorMessages.PasswordLowercase)
            .Matches("[0-9]").WithMessage(ErrorMessages.PasswordNumber)
            .Matches("[^a-zA-Z0-9]").WithMessage(ErrorMessages.PasswordSpecial)
            .NotEqual(x => x.CurrentPassword).WithMessage(ErrorMessages.PasswordSameAsCurrent);
        
        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword).WithMessage(ErrorMessages.PasswordMismatch);
    }
}

/// <summary>
/// Validador para ForgotPasswordRequest
/// </summary>
public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(string.Format(ErrorMessages.Required, "Email"))
            .EmailAddress().WithMessage(ErrorMessages.InvalidEmail);
        
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage(string.Format(ErrorMessages.Required, "Empresa"));
    }
}

/// <summary>
/// Validador para ResetPasswordRequest
/// </summary>
public class ResetPasswordValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(string.Format(ErrorMessages.Required, "Token"));
        
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(string.Format(ErrorMessages.Required, "Nova senha"))
            .MinimumLength(SystemConstants.MinPasswordLength)
                .WithMessage(string.Format(ErrorMessages.PasswordMinLength, SystemConstants.MinPasswordLength))
            .MaximumLength(SystemConstants.MaxPasswordLength)
                .WithMessage(string.Format(ErrorMessages.PasswordMaxLength, SystemConstants.MaxPasswordLength))
            .Matches("[A-Z]").WithMessage(ErrorMessages.PasswordUppercase)
            .Matches("[a-z]").WithMessage(ErrorMessages.PasswordLowercase)
            .Matches("[0-9]").WithMessage(ErrorMessages.PasswordNumber)
            .Matches("[^a-zA-Z0-9]").WithMessage(ErrorMessages.PasswordSpecial);
        
        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword).WithMessage(ErrorMessages.PasswordMismatch);
    }
}

/// <summary>
/// Validador para RefreshTokenRequest
/// </summary>
public class RefreshTokenValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage(string.Format(ErrorMessages.Required, "Refresh Token"));
    }
}
