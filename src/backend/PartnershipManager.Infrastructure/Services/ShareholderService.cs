using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.Features.Shareholders.DTOs;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Exceptions;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Services;

public interface IShareholderService
{
    Task<ShareholderListResponse> GetPagedAsync(Guid clientId, Guid? companyId, int page, int pageSize, string? search, string? type, string? status);
    Task<ShareholderResponse> GetByIdAsync(Guid id, Guid clientId);
    Task<ShareholderResponse> CreateAsync(Guid clientId, CreateShareholderRequest request, Guid? userId = null);
    Task<ShareholderResponse> UpdateAsync(Guid id, Guid clientId, UpdateShareholderRequest request, Guid? userId = null);
    Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null);
}

public class ShareholderService : IShareholderService
{
    private readonly IShareholderRepository _shareholderRepository;
    private readonly ICompanyRepository _companyRepository;

    public ShareholderService(
        IShareholderRepository shareholderRepository,
        ICompanyRepository companyRepository)
    {
        _shareholderRepository = shareholderRepository;
        _companyRepository = companyRepository;
    }

    public async Task<ShareholderListResponse> GetPagedAsync(Guid clientId, Guid? companyId, int page, int pageSize, string? search, string? type, string? status)
    {
        var (items, total) = await _shareholderRepository.GetPagedAsync(clientId, companyId, page, pageSize, search, type, status);
        var responseItems = items.Select(MapToResponse);
        return new ShareholderListResponse(responseItems, total, page, pageSize);
    }

    public async Task<ShareholderResponse> GetByIdAsync(Guid id, Guid clientId)
    {
        var shareholder = await _shareholderRepository.GetByIdAsync(id, clientId);
        if (shareholder == null)
        {
            throw new NotFoundException("Shareholder", id);
        }
        return MapToResponse(shareholder);
    }

    public async Task<ShareholderResponse> CreateAsync(Guid clientId, CreateShareholderRequest request, Guid? userId = null)
    {
        var company = await _companyRepository.GetByIdAsync(request.CompanyId);
        if (company == null || company.ClientId != clientId)
        {
            throw new NotFoundException("Company", request.CompanyId);
        }

        var documentExists = await _shareholderRepository.DocumentExistsAsync(clientId, request.Document);
        if (documentExists)
        {
            throw new ConflictException("Documento já cadastrado para este cliente");
        }

        var shareholder = Shareholder.Create(
            clientId,
            request.CompanyId,
            request.Name,
            request.Document,
            request.DocumentType,
            request.Type,
            request.Email,
            request.Phone,
            ShareholderStatus.Active,
            request.Notes,
            userId);

        shareholder.SetCompanyName(company.Name);

        await _shareholderRepository.AddAsync(shareholder);

        return MapToResponse(shareholder);
    }

    public async Task<ShareholderResponse> UpdateAsync(Guid id, Guid clientId, UpdateShareholderRequest request, Guid? userId = null)
    {
        var shareholder = await _shareholderRepository.GetByIdAsync(id, clientId);
        if (shareholder == null)
        {
            throw new NotFoundException("Shareholder", id);
        }

        if (request.CompanyId.HasValue && request.CompanyId.Value != shareholder.CompanyId)
        {
            var company = await _companyRepository.GetByIdAsync(request.CompanyId.Value);
            if (company == null || company.ClientId != clientId)
            {
                throw new NotFoundException("Company", request.CompanyId.Value);
            }
            shareholder.ChangeCompany(request.CompanyId.Value, userId);
            shareholder.SetCompanyName(company.Name);
        }

        if (!string.IsNullOrWhiteSpace(request.Document) && request.DocumentType.HasValue)
        {
            if (await _shareholderRepository.DocumentExistsAsync(clientId, request.Document, id))
            {
                throw new ConflictException("Documento já cadastrado para este cliente");
            }
            shareholder.UpdateDocument(request.Document, request.DocumentType.Value, userId);
        }

        shareholder.UpdateInfo(
            request.Name,
            request.Email,
            request.Phone,
            request.Type,
            request.Status,
            request.Notes,
            userId);

        await _shareholderRepository.UpdateAsync(shareholder);

        return MapToResponse(shareholder);
    }

    public async Task DeleteAsync(Guid id, Guid clientId, Guid? userId = null)
    {
        if (!await _shareholderRepository.ExistsAsync(id, clientId))
        {
            throw new NotFoundException("Shareholder", id);
        }

        await _shareholderRepository.SoftDeleteAsync(id, clientId, userId);
    }

    private static ShareholderResponse MapToResponse(Shareholder shareholder)
    {
        return new ShareholderResponse
        {
            Id = shareholder.Id,
            ClientId = shareholder.ClientId,
            CompanyId = shareholder.CompanyId,
            CompanyName = shareholder.CompanyName ?? string.Empty,
            Name = shareholder.Name,
            Document = shareholder.Document,
            DocumentFormatted = shareholder.DocumentFormatted,
            DocumentType = shareholder.DocumentType,
            Email = shareholder.Email,
            Phone = shareholder.Phone,
            Type = shareholder.Type,
            Status = shareholder.Status,
            Notes = shareholder.Notes,
            CreatedAt = shareholder.CreatedAt,
            UpdatedAt = shareholder.UpdatedAt
        };
    }
}
