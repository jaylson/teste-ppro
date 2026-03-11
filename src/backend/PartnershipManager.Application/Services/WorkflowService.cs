using PartnershipManager.Application.Common.Models;
using PartnershipManager.Application.DTOs.Workflow;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Application.Services;

public interface IWorkflowService
{
    Task<Guid> CreateAsync(Guid companyId, CreateWorkflowRequest request, Guid requestedBy);
    Task<WorkflowResponse?> GetByIdAsync(Guid id, Guid companyId);
    Task<PagedResult<WorkflowResponse>> GetByCompanyAsync(Guid companyId, int page, int pageSize, string? status = null, string? workflowType = null);
    Task<IEnumerable<WorkflowResponse>> GetPendingByUserAsync(Guid userId, Guid companyId);
    Task ApproveStepAsync(Guid workflowId, Guid stepId, Guid companyId, Guid userId, string? comments = null);
    Task RejectStepAsync(Guid workflowId, Guid stepId, Guid companyId, Guid userId, string comments);
    Task CancelAsync(Guid workflowId, Guid companyId, Guid cancelledBy, string reason);
}

public class WorkflowService : IWorkflowService
{
    private readonly IWorkflowRepository _repo;

    public WorkflowService(IWorkflowRepository repo)
    {
        _repo = repo;
    }

    public async Task<Guid> CreateAsync(Guid companyId, CreateWorkflowRequest request, Guid requestedBy)
    {
        var workflow = new Workflow
        {
            CompanyId = companyId,
            WorkflowType = request.WorkflowType,
            ReferenceType = request.ReferenceType,
            ReferenceId = request.ReferenceId,
            Title = request.Title,
            Description = request.Description,
            Status = WorkflowStatuses.Pending,
            Priority = request.Priority,
            RequestedBy = requestedBy,
            RequestedAt = DateTime.UtcNow,
            DueDate = request.DueDate
        };
        workflow.CreatedBy = requestedBy;
        workflow.UpdatedBy = requestedBy;

        var steps = request.Steps.Select(s => new WorkflowStep
        {
            Name = s.Name,
            Description = s.Description,
            StepType = s.StepType,
            AssignedRole = s.AssignedRole,
            AssignedUserId = s.AssignedUserId,
            DueDate = s.DueDate
        }).ToList();

        return await _repo.CreateAsync(workflow, steps);
    }

    public async Task<WorkflowResponse?> GetByIdAsync(Guid id, Guid companyId)
    {
        var w = await _repo.GetByIdAsync(id, companyId);
        return w == null ? null : MapToResponse(w);
    }

    public async Task<PagedResult<WorkflowResponse>> GetByCompanyAsync(
        Guid companyId, int page, int pageSize, string? status = null, string? workflowType = null)
    {
        var (items, total) = await _repo.GetByCompanyAsync(companyId, page, pageSize, status, workflowType);
        return new PagedResult<WorkflowResponse>(items.Select(MapToResponse), total, page, pageSize);
    }

    public async Task<IEnumerable<WorkflowResponse>> GetPendingByUserAsync(Guid userId, Guid companyId)
    {
        var items = await _repo.GetPendingByUserAsync(userId, companyId);
        return items.Select(MapToResponse);
    }

    public async Task ApproveStepAsync(Guid workflowId, Guid stepId, Guid companyId, Guid userId, string? comments = null)
    {
        var approval = new WorkflowApproval
        {
            WorkflowStepId = stepId,
            UserId = userId,
            Decision = WorkflowDecisions.Approved,
            Comments = comments,
            DecidedAt = DateTime.UtcNow
        };
        await _repo.RecordApprovalAsync(approval);
        await _repo.UpdateStepStatusAsync(stepId, WorkflowStepStatuses.Completed, userId);

        var workflow = await _repo.GetByIdAsync(workflowId, companyId)
            ?? throw new InvalidOperationException("Workflow não encontrado.");

        if (workflow.CurrentStep >= workflow.TotalSteps)
            await _repo.CompleteWorkflowAsync(workflowId, WorkflowStatuses.Approved);
        else
            await _repo.AdvanceStepAsync(workflowId, workflow.CurrentStep + 1);
    }

    public async Task RejectStepAsync(Guid workflowId, Guid stepId, Guid companyId, Guid userId, string comments)
    {
        var approval = new WorkflowApproval
        {
            WorkflowStepId = stepId,
            UserId = userId,
            Decision = WorkflowDecisions.Rejected,
            Comments = comments,
            DecidedAt = DateTime.UtcNow
        };
        await _repo.RecordApprovalAsync(approval);
        await _repo.UpdateStepStatusAsync(stepId, WorkflowStepStatuses.Completed, userId);
        await _repo.CompleteWorkflowAsync(workflowId, WorkflowStatuses.Rejected);
    }

    public Task CancelAsync(Guid workflowId, Guid companyId, Guid cancelledBy, string reason)
        => _repo.CancelWorkflowAsync(workflowId, cancelledBy, reason);

    private static WorkflowResponse MapToResponse(Workflow w) => new()
    {
        Id = w.Id,
        CompanyId = w.CompanyId,
        WorkflowType = w.WorkflowType,
        ReferenceType = w.ReferenceType,
        ReferenceId = w.ReferenceId,
        Title = w.Title,
        Description = w.Description,
        Status = w.Status,
        Priority = w.Priority,
        CurrentStep = w.CurrentStep,
        TotalSteps = w.TotalSteps,
        RequestedBy = w.RequestedBy,
        RequestedAt = w.RequestedAt,
        DueDate = w.DueDate,
        CompletedAt = w.CompletedAt,
        CreatedAt = w.CreatedAt,
        Steps = w.Steps.Select(s => new WorkflowStepResponse
        {
            Id = s.Id,
            StepOrder = s.StepOrder,
            Name = s.Name,
            Description = s.Description,
            StepType = s.StepType,
            AssignedRole = s.AssignedRole,
            AssignedUserId = s.AssignedUserId,
            Status = s.Status,
            IsCurrent = s.IsCurrent,
            StartedAt = s.StartedAt,
            DueDate = s.DueDate,
            CompletedAt = s.CompletedAt,
            Notes = s.Notes
        }).ToList()
    };
}
