import { api } from './api';
import type {
  VestingPlan,
  VestingPlanListResponse,
  CreateVestingPlanRequest,
  UpdateVestingPlanRequest,
  VestingPlanFilters,
  VestingGrant,
  VestingGrantListResponse,
  CreateVestingGrantRequest,
  ExerciseSharesRequest,
  VestingGrantFilters,
  VestingMilestone,
  VestingMilestoneListResponse,
  CreateVestingMilestoneRequest,
  AchieveMilestoneRequest,
  VestingMilestoneFilters,
  VestingCalculationResult,
  VestingProjectionResponse,
  VestingTransaction,
} from '@/types';

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
}

// ─── Vesting Plans ────────────────────────────────────────────────────────────

export const vestingPlanService = {
  async getPlans(filters?: VestingPlanFilters): Promise<VestingPlanListResponse> {
    const params: Record<string, string | number | undefined> = {};
    if (filters?.companyId) params.companyId = filters.companyId;
    if (filters?.page) params.page = filters.page;
    if (filters?.pageSize) params.pageSize = filters.pageSize;
    if (filters?.search) params.search = filters.search;
    if (filters?.status) params.status = filters.status;

    const response = await api.get<ApiResponse<VestingPlanListResponse>>('/vesting-plans', { params });
    return response.data.data;
  },

  async getPlansByCompany(companyId: string, status?: string): Promise<VestingPlan[]> {
    const params: Record<string, string | undefined> = {};
    if (status) params.status = status;
    const response = await api.get<ApiResponse<VestingPlan[]>>(
      `/vesting-plans/by-company/${companyId}`,
      { params }
    );
    return response.data.data;
  },

  async getPlanById(id: string): Promise<VestingPlan> {
    const response = await api.get<ApiResponse<VestingPlan>>(`/vesting-plans/${id}`);
    return response.data.data;
  },

  async createPlan(data: CreateVestingPlanRequest): Promise<VestingPlan> {
    const response = await api.post<ApiResponse<VestingPlan>>('/vesting-plans', data);
    return response.data.data;
  },

  async updatePlan(id: string, data: UpdateVestingPlanRequest): Promise<VestingPlan> {
    const response = await api.put<ApiResponse<VestingPlan>>(`/vesting-plans/${id}`, data);
    return response.data.data;
  },

  async activatePlan(id: string): Promise<VestingPlan> {
    const response = await api.patch<ApiResponse<VestingPlan>>(`/vesting-plans/${id}/activate`);
    return response.data.data;
  },

  async deactivatePlan(id: string): Promise<VestingPlan> {
    const response = await api.patch<ApiResponse<VestingPlan>>(`/vesting-plans/${id}/deactivate`);
    return response.data.data;
  },

  async archivePlan(id: string): Promise<VestingPlan> {
    const response = await api.patch<ApiResponse<VestingPlan>>(`/vesting-plans/${id}/archive`);
    return response.data.data;
  },

  async deletePlan(id: string): Promise<void> {
    await api.delete(`/vesting-plans/${id}`);
  },
};

// ─── Vesting Grants ───────────────────────────────────────────────────────────

export const vestingGrantService = {
  async getGrants(filters?: VestingGrantFilters): Promise<VestingGrantListResponse> {
    const params: Record<string, string | number | undefined> = {};
    if (filters?.companyId) params.companyId = filters.companyId;
    if (filters?.vestingPlanId) params.vestingPlanId = filters.vestingPlanId;
    if (filters?.shareholderId) params.shareholderId = filters.shareholderId;
    if (filters?.status) params.status = filters.status;
    if (filters?.page) params.page = filters.page;
    if (filters?.pageSize) params.pageSize = filters.pageSize;

    const response = await api.get<ApiResponse<VestingGrantListResponse>>('/vesting-grants', { params });
    return response.data.data;
  },

  async getGrantsByShareholder(
    shareholderId: string,
    companyId?: string
  ): Promise<VestingGrant[]> {
    const params: Record<string, string | undefined> = {};
    if (companyId) params.companyId = companyId;
    const response = await api.get<ApiResponse<VestingGrant[]>>(
      `/vesting-grants/by-shareholder/${shareholderId}`,
      { params }
    );
    return response.data.data;
  },

  async getGrantById(id: string): Promise<VestingGrant> {
    const response = await api.get<ApiResponse<VestingGrant>>(`/vesting-grants/${id}`);
    return response.data.data;
  },

  async createGrant(data: CreateVestingGrantRequest): Promise<VestingGrant> {
    const response = await api.post<ApiResponse<VestingGrant>>('/vesting-grants', data);
    return response.data.data;
  },

  async approveGrant(id: string): Promise<VestingGrant> {
    const response = await api.patch<ApiResponse<VestingGrant>>(`/vesting-grants/${id}/approve`);
    return response.data.data;
  },

  async activateGrant(id: string): Promise<VestingGrant> {
    const response = await api.patch<ApiResponse<VestingGrant>>(`/vesting-grants/${id}/activate`);
    return response.data.data;
  },

  async cancelGrant(id: string): Promise<VestingGrant> {
    const response = await api.patch<ApiResponse<VestingGrant>>(`/vesting-grants/${id}/cancel`);
    return response.data.data;
  },

  async exerciseShares(id: string, data: ExerciseSharesRequest): Promise<VestingTransaction> {
    const response = await api.post<ApiResponse<VestingTransaction>>(
      `/vesting-grants/${id}/exercise`,
      data
    );
    return response.data.data;
  },

  async calculateVesting(id: string, asOfDate?: string): Promise<VestingCalculationResult> {
    const params: Record<string, string | undefined> = {};
    if (asOfDate) params.asOfDate = asOfDate;
    const response = await api.get<ApiResponse<VestingCalculationResult>>(
      `/vesting-grants/${id}/calculate`,
      { params }
    );
    return response.data.data;
  },

  async getProjection(id: string): Promise<VestingProjectionResponse> {
    const response = await api.get<ApiResponse<VestingProjectionResponse>>(
      `/vesting-grants/${id}/projection`
    );
    return response.data.data;
  },

  async getTransactions(
    id: string,
    page = 1,
    pageSize = 10
  ): Promise<VestingTransaction[]> {
    const response = await api.get<ApiResponse<VestingTransaction[]>>(
      `/vesting-grants/${id}/transactions`,
      { params: { page, pageSize } }
    );
    return response.data.data ?? [];
  },

  async deleteGrant(id: string): Promise<void> {
    await api.delete(`/vesting-grants/${id}`);
  },
};

// ─── Milestones ───────────────────────────────────────────────────────────────

export const vestingMilestoneService = {
  async getMilestones(filters?: VestingMilestoneFilters): Promise<VestingMilestoneListResponse> {
    const params: Record<string, string | number | undefined> = {};
    if (filters?.vestingPlanId) params.vestingPlanId = filters.vestingPlanId;
    if (filters?.companyId) params.companyId = filters.companyId;
    if (filters?.page) params.page = filters.page;
    if (filters?.pageSize) params.pageSize = filters.pageSize;

    const response = await api.get<ApiResponse<VestingMilestoneListResponse>>('/milestones', { params });
    return response.data.data;
  },

  async getMilestonesByPlan(vestingPlanId: string): Promise<VestingMilestone[]> {
    const response = await api.get<ApiResponse<VestingMilestone[]>>(
      `/milestones/by-plan/${vestingPlanId}`
    );
    return response.data.data;
  },

  async getMilestoneById(id: string): Promise<VestingMilestone> {
    const response = await api.get<ApiResponse<VestingMilestone>>(`/milestones/${id}`);
    return response.data.data;
  },

  async createMilestone(data: CreateVestingMilestoneRequest): Promise<VestingMilestone> {
    const response = await api.post<ApiResponse<VestingMilestone>>('/milestones', data);
    return response.data.data;
  },

  async achieveMilestone(id: string, data: AchieveMilestoneRequest): Promise<VestingMilestone> {
    const response = await api.patch<ApiResponse<VestingMilestone>>(
      `/milestones/${id}/achieve`,
      data
    );
    return response.data.data;
  },

  async failMilestone(id: string): Promise<VestingMilestone> {
    const response = await api.patch<ApiResponse<VestingMilestone>>(`/milestones/${id}/fail`);
    return response.data.data;
  },

  async deleteMilestone(id: string): Promise<void> {
    await api.delete(`/milestones/${id}`);
  },
};
