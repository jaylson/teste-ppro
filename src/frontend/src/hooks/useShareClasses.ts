import { useQuery } from '@tanstack/react-query';
import { shareClassService } from '@/services/shareClassService';

const SHARE_CLASSES_KEY = ['shareClasses'];

/**
 * Hook para listar classes de ações
 */
export function useShareClasses(companyId?: string) {
  return useQuery({
    queryKey: [...SHARE_CLASSES_KEY, companyId],
    queryFn: () => shareClassService.getShareClasses(companyId),
    staleTime: 60000, // 1 minuto
  });
}

/**
 * Hook para listar classes de ações de uma empresa (resumido)
 */
export function useShareClassesByCompany(companyId: string | undefined) {
  return useQuery({
    queryKey: [...SHARE_CLASSES_KEY, 'by-company', companyId],
    queryFn: () => shareClassService.getByCompany(companyId!),
    enabled: !!companyId,
    staleTime: 60000,
  });
}

/**
 * Hook para obter uma classe de ações específica
 */
export function useShareClass(id: string | undefined) {
  return useQuery({
    queryKey: [...SHARE_CLASSES_KEY, id],
    queryFn: () => shareClassService.getById(id!),
    enabled: !!id,
  });
}
