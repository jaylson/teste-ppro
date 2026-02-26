namespace PartnershipManager.Application.Features.Simulation.DTOs;

/// <summary>
/// Tipo de aquisição na rodada
/// </summary>
public enum AcquisitionType
{
    /// <summary>Emissão primária: novas ações são criadas (diluição)</summary>
    Primary = 1,
    /// <summary>Aquisição secundária: compra de ações existentes (sem novas ações, sem diluição geral)</summary>
    Secondary = 2,
}

/// <summary>
/// Request para simular uma rodada de investimento
/// </summary>
public record RoundSimulationRequest
{
    /// <summary>
    /// ID da empresa
    /// </summary>
    public Guid CompanyId { get; init; }
    
    /// <summary>
    /// Valuation pre-money (valor da empresa antes do investimento)
    /// </summary>
    public decimal PreMoneyValuation { get; init; }
    
    /// <summary>
    /// Valor total do investimento
    /// </summary>
    public decimal InvestmentAmount { get; init; }
    
    /// <summary>
    /// Nome da rodada (ex: "Series A", "Seed")
    /// </summary>
    public string RoundName { get; init; } = string.Empty;
    
    /// <summary>
    /// Tipo da rodada
    /// </summary>
    public RoundType RoundType { get; init; } = RoundType.Equity;

    /// <summary>
    /// Tipo de aquisição: Primary (novas ações) ou Secondary (compra de ações existentes)
    /// </summary>
    public AcquisitionType AcquisitionType { get; init; } = AcquisitionType.Primary;
    
    /// <summary>
    /// ID da classe de ação a ser emitida (opcional, usa Common se não informado)
    /// </summary>
    public Guid? NewShareClassId { get; init; }
    
    /// <summary>
    /// Nome da nova classe de ação (se criar uma nova)
    /// </summary>
    public string? NewShareClassName { get; init; }
    
    /// <summary>
    /// Lista de novos investidores e seus valores
    /// </summary>
    public List<NewInvestorRequest> NewInvestors { get; init; } = new();
    
    /// <summary>
    /// Se deve incluir pool de opções na diluição
    /// </summary>
    public bool IncludeOptionPool { get; init; } = false;
    
    /// <summary>
    /// Percentual do pool de opções (se incluir)
    /// </summary>
    public decimal OptionPoolPercentage { get; init; } = 0;
    
    /// <summary>
    /// Se o pool de opções é pré-money (diluição ocorre antes da rodada)
    /// </summary>
    public bool OptionPoolPreMoney { get; init; } = true;

    /// <summary>
    /// Incluir grants de vesting na simulação (cap table fully diluted)
    /// </summary>
    public bool IncludeVesting { get; init; } = false;
}

/// <summary>
/// Dados de um novo investidor na rodada
/// </summary>
public record NewInvestorRequest
{
    public string Name { get; init; } = string.Empty;
    public decimal InvestmentAmount { get; init; }
    public string? Email { get; init; }
    public string? Document { get; init; }
}

/// <summary>
/// Tipo de rodada de investimento
/// </summary>
public enum RoundType
{
    Equity = 1,          // Equity direto
    ConvertibleNote = 2, // Nota conversível
    SAFE = 3,            // Simple Agreement for Future Equity
}

/// <summary>
/// Resposta da simulação de rodada
/// </summary>
public record RoundSimulationResponse
{
    /// <summary>
    /// Nome da rodada
    /// </summary>
    public string RoundName { get; init; } = string.Empty;
    
    /// <summary>
    /// Valuation pre-money
    /// </summary>
    public decimal PreMoneyValuation { get; init; }
    
    /// <summary>
    /// Valor do investimento
    /// </summary>
    public decimal InvestmentAmount { get; init; }
    
    /// <summary>
    /// Valuation post-money (pre-money + investimento)
    /// </summary>
    public decimal PostMoneyValuation { get; init; }
    
    /// <summary>
    /// Preço por ação nesta rodada
    /// </summary>
    public decimal PricePerShare { get; init; }
    
    /// <summary>
    /// Total de ações antes da rodada
    /// </summary>
    public decimal SharesBefore { get; init; }
    
    /// <summary>
    /// Novas ações a serem emitidas
    /// </summary>
    public decimal NewSharesIssued { get; init; }
    
    /// <summary>
    /// Total de ações após a rodada
    /// </summary>
    public decimal SharesAfter { get; init; }
    
    /// <summary>
    /// Diluição total (%)
    /// </summary>
    public decimal TotalDilution { get; init; }
    
    /// <summary>
    /// Cap Table antes da rodada
    /// </summary>
    public List<SimulatedShareholderEntry> CapTableBefore { get; init; } = new();
    
    /// <summary>
    /// Cap Table após a rodada
    /// </summary>
    public List<SimulatedShareholderEntry> CapTableAfter { get; init; } = new();
    
    /// <summary>
    /// Novos investidores com suas participações
    /// </summary>
    public List<SimulatedNewInvestor> NewInvestors { get; init; } = new();
    
    /// <summary>
    /// Informações do pool de opções (se aplicável)
    /// </summary>
    public OptionPoolInfo? OptionPool { get; init; }
    
    /// <summary>
    /// Tipo de aquisição utilizado
    /// </summary>
    public AcquisitionType AcquisitionType { get; init; } = AcquisitionType.Primary;

    /// <summary>
    /// Grants de vesting ativos (se IncludeVesting = true)
    /// </summary>
    public List<VestingSimulationEntry> VestingEntries { get; init; } = new();

    /// <summary>
    /// Cap Table fully diluted (incluindo todos os grants de vesting não exercidos)
    /// </summary>
    public List<SimulatedShareholderEntry> FullyDilutedCapTable { get; init; } = new();

    /// <summary>
    /// Total de ações fully diluted (shares after + unvested vesting grants)
    /// </summary>
    public decimal FullyDilutedShares { get; init; }
    
    /// <summary>
    /// Data da simulação
    /// </summary>
    public DateTime SimulatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Entrada de acionista na simulação
/// </summary>
public record SimulatedShareholderEntry
{
    public Guid? ShareholderId { get; init; }
    public string ShareholderName { get; init; } = string.Empty;
    public string ShareholderType { get; init; } = string.Empty;
    public decimal Shares { get; init; }
    public decimal Ownership { get; init; }
    public decimal Value { get; init; }
    public decimal DilutionPercentage { get; init; }
    public bool IsNewInvestor { get; init; } = false;
}

/// <summary>
/// Dados do novo investidor simulado
/// </summary>
public record SimulatedNewInvestor
{
    public string Name { get; init; } = string.Empty;
    public decimal InvestmentAmount { get; init; }
    public decimal SharesReceived { get; init; }
    public decimal OwnershipPercentage { get; init; }
    public decimal ValueAtRound { get; init; }
}

/// <summary>
/// Informações do pool de opções
/// </summary>
public record OptionPoolInfo
{
    public decimal Percentage { get; init; }
    public decimal Shares { get; init; }
    public bool IsPreMoney { get; init; }
    public decimal Value { get; init; }
}
/// <summary>
/// Entrada de vesting na simulação de rodada (fully diluted)
/// </summary>
public record VestingSimulationEntry
{
    public Guid GrantId { get; init; }
    public string ShareholderName { get; init; } = string.Empty;
    public string PlanName { get; init; } = string.Empty;
    public decimal TotalShares { get; init; }
    public decimal VestedShares { get; init; }
    public decimal UnvestedShares { get; init; }
    public decimal ExercisedShares { get; init; }
    public decimal RemainingShares { get; init; }  // TotalShares - ExercisedShares
    public decimal VestedPercentage { get; init; }
    /// <summary>Participação no cap table fully diluted pós-rodada</summary>
    public decimal FullyDilutedOwnership { get; init; }
    public DateTime VestingEndDate { get; init; }
    public string Status { get; init; } = string.Empty;
}