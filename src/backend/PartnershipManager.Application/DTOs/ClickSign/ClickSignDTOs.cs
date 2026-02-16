namespace PartnershipManager.Application.DTOs.ClickSign;

public record CreateEnvelopeRequest(
    string Name,
    bool SequenceEnabled = false,
    bool AutoClose = true,
    int? RemindInterval = 3
);

public record AddDocumentRequest(
    string FileName,
    byte[] FileContent
);

public record AddSignerRequest(
    string Name,
    string Email,
    string? PhoneNumber = null,
    string? DocumentNumber = null
);

public record CreateRequirementsRequest(
    string DocumentId,
    string SignerId,
    string Role,
    string AuthMethod
);

public record ClickSignEnvelopeResponse(
    ClickSignData Data,
    ClickSignMeta? Meta = null
);

public record ClickSignDocumentResponse(
    ClickSignData Data,
    ClickSignMeta? Meta = null
);

public record ClickSignSignerResponse(
    ClickSignData Data,
    ClickSignMeta? Meta = null
);

public record ClickSignData(
    string Id,
    string Type,
    ClickSignAttributes Attributes,
    ClickSignLinks? Links = null
);

public record ClickSignAttributes(
    string? Name = null,
    string? Status = null,
    DateTime? Created = null,
    DateTime? Modified = null,
    string? Email = null,
    string? ExternalId = null,
    string? SignerEmail = null,
    string? RefusalReason = null
);

public record ClickSignLinks(
    string Self
);

public record ClickSignMeta(
    object? Pagination = null
);

public record ClickSignWebhookPayload(
    string Event,
    ClickSignData Data,
    DateTime Timestamp
);
