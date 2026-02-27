import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { vestingAccelerationService } from '@/services/vestingService';
import toast from 'react-hot-toast';

const QUERY_KEY = ['vesting-accelerations'];

export function useVestingAccelerationsByGrant(grantId: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, 'by-grant', grantId],
    queryFn: () => vestingAccelerationService.getByGrant(grantId),
    enabled: !!grantId,
    staleTime: 60000,
  });
}

export function useAccelerationPreview(milestoneId: string, enabled = true) {
  return useQuery({
    queryKey: [...QUERY_KEY, 'preview', milestoneId],
    queryFn: () => vestingAccelerationService.getPreview(milestoneId),
    enabled: !!milestoneId && enabled,
    staleTime: 30000,
  });
}

export function useApplyAcceleration() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (milestoneId: string) =>
      vestingAccelerationService.applyAcceleration(milestoneId),
    onSuccess: (result) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: ['grant-milestones'] });
      queryClient.invalidateQueries({ queryKey: ['vesting-grants', result.vestingGrantId] });
      toast.success(
        `Aceleração aplicada — vesting adiantado em ${result.monthsAccelerated.toFixed(1)} meses`
      );
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao aplicar aceleração');
    },
  });
}
