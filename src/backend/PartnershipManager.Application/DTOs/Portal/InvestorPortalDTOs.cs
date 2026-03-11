namespace PartnershipManager.Application.DTOs.Portal;

public class InvestorSummaryResponse
{
    public string InvestorName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public decimal TotalShares { get; set; }
    public decimal OwnershipPercentage { get; set; }
    public decimal EstimatedValue { get; set; }
    public decimal CurrentValuation { get; set; }
    public DateTime? LastVestingEvent { get; set; }
    public int DocumentsCount { get; set; }
}

public class InvestorMetricsResponse
{
    public decimal TotalInvested { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal ReturnOnInvestment { get; set; }
    public int RoundsParticipated { get; set; }
}
