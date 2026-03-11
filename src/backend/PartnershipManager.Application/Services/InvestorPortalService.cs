using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.DTOs.Communication;
using PartnershipManager.Application.DTOs.Portal;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Application.Services;

public interface IInvestorPortalService
{
    Task<InvestorSummaryResponse> GetSummaryAsync(Guid userId, Guid companyId);
    Task<PagedResult<CommunicationListResponse>> GetCommunicationsAsync(Guid userId, Guid companyId, int page, int pageSize);
    Task<IEnumerable<Document>> GetDocumentsAsync(Guid userId, Guid companyId);
}

public class InvestorPortalService : IInvestorPortalService
{
    private readonly ICommunicationRepository _communicationRepo;
    private readonly IDocumentRepository _documentRepo;

    public InvestorPortalService(
        ICommunicationRepository communicationRepo,
        IDocumentRepository documentRepo)
    {
        _communicationRepo = communicationRepo;
        _documentRepo = documentRepo;
    }

    public async Task<InvestorSummaryResponse> GetSummaryAsync(Guid userId, Guid companyId)
    {
        var (_, docsTotal) = await _documentRepo.GetPagedAsync(
            clientId: Guid.Empty, companyId: companyId, page: 1, pageSize: 1,
            visibility: "investors");

        return new InvestorSummaryResponse
        {
            InvestorName = string.Empty,
            CompanyName = string.Empty,
            TotalShares = 0,
            OwnershipPercentage = 0,
            EstimatedValue = 0,
            CurrentValuation = 0,
            DocumentsCount = docsTotal
        };
    }

    public async Task<PagedResult<CommunicationListResponse>> GetCommunicationsAsync(
        Guid userId, Guid companyId, int page, int pageSize)
    {
        var items = await _communicationRepo.GetForRoleAsync(companyId, "investors", pageSize * page);
        var paged = items.Skip((page - 1) * pageSize).Take(pageSize);
        var mapped = paged.Select(c => new CommunicationListResponse
        {
            Id = c.Id,
            Title = c.Title,
            CommType = c.CommType,
            Visibility = c.Visibility,
            IsPinned = c.IsPinned,
            PublishedAt = c.PublishedAt,
            CreatedAt = c.CreatedAt,
            ViewsCount = c.ViewsCount,
            Summary = c.Summary
        });
        return new PagedResult<CommunicationListResponse>(mapped, items.Count(), page, pageSize);
    }

    public async Task<IEnumerable<Document>> GetDocumentsAsync(Guid userId, Guid companyId)
    {
        var (items, _) = await _documentRepo.GetPagedAsync(
            clientId: Guid.Empty, companyId: companyId, page: 1, pageSize: 200,
            visibility: "investors");
        return items;
    }
}
