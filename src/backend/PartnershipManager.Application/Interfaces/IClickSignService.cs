using PartnershipManager.Application.DTOs.ClickSign;

namespace PartnershipManager.Application.Interfaces;

public interface IClickSignService
{
    Task<ClickSignEnvelopeResponse> CreateEnvelopeAsync(CreateEnvelopeRequest request);
    Task<ClickSignDocumentResponse> AddDocumentAsync(string envelopeId, AddDocumentRequest request);
    Task<ClickSignSignerResponse> AddSignerAsync(string envelopeId, AddSignerRequest request);
    Task CreateRequirementsAsync(string envelopeId, CreateRequirementsRequest request);
    Task StartEnvelopeAsync(string envelopeId);
    Task<ClickSignEnvelopeResponse> GetEnvelopeAsync(string envelopeId);
    Task<byte[]> DownloadDocumentAsync(string envelopeId, string documentId);
}
