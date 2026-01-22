using System.ComponentModel.DataAnnotations;

namespace PartnershipManager.Application.Features.Billing.DTOs;

public record ClientCreateDto
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Documento é obrigatório")]
    [StringLength(20, ErrorMessage = "Documento deve ter no máximo 20 caracteres")]
    public string Document { get; init; } = string.Empty;

    [Required]
    public string Type { get; init; } = "company"; // "individual" ou "company"

    [Phone(ErrorMessage = "Telefone inválido")]
    public string? Phone { get; init; }

    public string? Address { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? ZipCode { get; init; }
    public string? Country { get; init; } = "Brasil";
}

public record ClientUpdateDto
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Documento é obrigatório")]
    [StringLength(20, ErrorMessage = "Documento deve ter no máximo 20 caracteres")]
    public string Document { get; init; } = string.Empty;

    [Required]
    public string Type { get; init; } = "company";

    [Required]
    public string Status { get; init; } = "active"; // "active", "suspended", "cancelled"

    [Phone(ErrorMessage = "Telefone inválido")]
    public string? Phone { get; init; }

    public string? Address { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? ZipCode { get; init; }
    public string? Country { get; init; } = "Brasil";
}

public record ClientResponseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Document { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? ZipCode { get; init; }
    public string? Country { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int SubscriptionsCount { get; init; }
}

public record ClientListResponseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Document { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public int SubscriptionsCount { get; init; }
}
