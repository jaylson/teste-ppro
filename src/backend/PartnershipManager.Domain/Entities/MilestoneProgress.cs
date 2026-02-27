using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Domain.Entities;

/// <summary>
/// Immutable time-series record of a single progress measurement for a GrantMilestone.
/// Progress history is append-only and is never soft-deleted.
/// </summary>
public class MilestoneProgress
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid GrantMilestoneId { get; private set; }

    public DateTime RecordedDate { get; private set; }
    public decimal RecordedValue { get; private set; }
    public decimal ProgressPercentage { get; private set; }
    public string? Notes { get; private set; }
    public ProgressDataSource? DataSource { get; private set; }

    public Guid RecordedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation
    public virtual GrantMilestone? Milestone { get; private set; }

    private MilestoneProgress() { }

    public static MilestoneProgress Create(
        Guid clientId,
        Guid grantMilestoneId,
        DateTime recordedDate,
        decimal recordedValue,
        decimal progressPercentage,
        Guid recordedBy,
        string? notes = null,
        ProgressDataSource? dataSource = null)
    {
        if (progressPercentage < 0 || progressPercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(progressPercentage), "ProgressPercentage deve ser entre 0 e 100.");

        return new MilestoneProgress
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            GrantMilestoneId = grantMilestoneId,
            RecordedDate = recordedDate.Date,
            RecordedValue = recordedValue,
            ProgressPercentage = progressPercentage,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            DataSource = dataSource,
            RecordedBy = recordedBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>Reconstitutes from persistence. For repositories only.</summary>
    public static MilestoneProgress Reconstitute(
        Guid id, Guid clientId, Guid grantMilestoneId,
        DateTime recordedDate, decimal recordedValue, decimal progressPercentage,
        string? notes, ProgressDataSource? dataSource, Guid recordedBy, DateTime createdAt)
    {
        return new MilestoneProgress
        {
            Id = id,
            ClientId = clientId,
            GrantMilestoneId = grantMilestoneId,
            RecordedDate = recordedDate,
            RecordedValue = recordedValue,
            ProgressPercentage = progressPercentage,
            Notes = notes,
            DataSource = dataSource,
            RecordedBy = recordedBy,
            CreatedAt = createdAt
        };
    }
}
