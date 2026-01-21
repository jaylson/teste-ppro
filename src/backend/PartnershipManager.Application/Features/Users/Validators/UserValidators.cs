using FluentValidation;
using PartnershipManager.Application.Features.Users.DTOs;
using PartnershipManager.Domain.Constants;

namespace PartnershipManager.Application.Features.Users.Validators;

/// <summary>
/// Validador para CreateUserRequest
/// </summary>
public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
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
        
        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage(string.Format(ErrorMessages.MaxLength, "Telefone", 20))
            .When(x => !string.IsNullOrEmpty(x.Phone));
        
        RuleFor(x => x.InitialRole)
            .IsInEnum().WithMessage(ErrorMessages.InvalidRole);
    }
}

/// <summary>
/// Validador para UpdateUserRequest
/// </summary>
public class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ErrorMessages.NameRequired)
            .MaximumLength(SystemConstants.MaxNameLength)
                .WithMessage(string.Format(ErrorMessages.MaxLength, "Nome", SystemConstants.MaxNameLength));
        
        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage(string.Format(ErrorMessages.MaxLength, "Telefone", 20))
            .When(x => !string.IsNullOrEmpty(x.Phone));
        
        RuleFor(x => x.AvatarUrl)
            .MaximumLength(SystemConstants.MaxUrlLength)
                .WithMessage(string.Format(ErrorMessages.MaxLength, "URL do Avatar", SystemConstants.MaxUrlLength))
            .Must(BeValidUrl).WithMessage(ErrorMessages.InvalidUrl)
            .When(x => !string.IsNullOrEmpty(x.AvatarUrl));
    }
    
    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;
        
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

/// <summary>
/// Validador para UpdateUserPreferencesRequest
/// </summary>
public class UpdateUserPreferencesValidator : AbstractValidator<UpdateUserPreferencesRequest>
{
    private static readonly string[] ValidTimezones = 
    {
        "America/Sao_Paulo", "America/New_York", "America/Los_Angeles",
        "Europe/London", "Europe/Paris", "Asia/Tokyo", "UTC"
    };
    
    public UpdateUserPreferencesValidator()
    {
        RuleFor(x => x.Language)
            .IsInEnum().WithMessage("Idioma inválido");
        
        RuleFor(x => x.Timezone)
            .NotEmpty().WithMessage(string.Format(ErrorMessages.Required, "Fuso horário"))
            .Must(BeValidTimezone).WithMessage("Fuso horário inválido");
    }
    
    private static bool BeValidTimezone(string timezone)
    {
        return ValidTimezones.Contains(timezone);
    }
}

/// <summary>
/// Validador para ManageUserRoleRequest
/// </summary>
public class ManageUserRoleValidator : AbstractValidator<ManageUserRoleRequest>
{
    public ManageUserRoleValidator()
    {
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage(ErrorMessages.InvalidRole);
        
        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Data de expiração deve ser futura")
            .When(x => x.ExpiresAt.HasValue);
    }
}
