import { useEffect, useMemo } from 'react';
import { Building2, Loader2 } from 'lucide-react';
import { useAuthStore } from '@/stores/authStore';
import { useClientStore } from '@/stores/clientStore';
import { useClientCompanies } from '@/hooks';
import type { ClientCompany } from '@/types';

export function CompanySwitcher() {
  const { user } = useAuthStore();
  const {
    currentClient,
    selectedCompanyId,
    setSelectedCompanyId,
    setLoading,
    isLoading,
  } = useClientStore();

  const clientId = currentClient?.id || user?.clientId;

  const { data: companies = [], isLoading: isLoadingCompanies } = useClientCompanies(
    clientId || ''
  );

  useEffect(() => {
    if (isLoadingCompanies !== isLoading) {
      setLoading(isLoadingCompanies);
    }
  }, [isLoadingCompanies, isLoading, setLoading]);

  // Define seleção inicial: prioriza store, depois empresa do usuário, depois primeira da lista
  useEffect(() => {
    if (!selectedCompanyId) {
      if (user?.companyId) {
        setSelectedCompanyId(user.companyId);
      } else if (companies.length > 0) {
        setSelectedCompanyId(companies[0].id);
      }
    }
  }, [selectedCompanyId, companies, setSelectedCompanyId, user]);

  const activeCompany: ClientCompany | undefined = useMemo(
    () => companies.find((company) => company.id === selectedCompanyId),
    [companies, selectedCompanyId]
  );

  return (
    <div className="flex items-center gap-3 rounded-lg bg-white/5 px-3 py-2 border border-white/10 text-white">
      <div className="flex items-center gap-2 min-w-[160px]">
        <div className="w-10 h-10 rounded-lg bg-white/10 flex items-center justify-center">
          <Building2 className="w-5 h-5" />
        </div>
        <div className="flex flex-col">
          <span className="text-xs text-white/60">Empresa</span>
          <span className="text-sm font-semibold truncate">
            {activeCompany?.name || 'Selecione uma empresa'}
          </span>
        </div>
      </div>

      <div className="flex-1 flex items-center justify-end">
        {isLoadingCompanies ? (
          <Loader2 className="w-4 h-4 animate-spin text-white/70" />
        ) : (
          <select
            className="input bg-white/10 border-white/20 text-white max-w-xs"
            value={selectedCompanyId || ''}
            onChange={(e) => setSelectedCompanyId(e.target.value || null)}
          >
            <option value="" className="text-gray-900">
              Selecione uma empresa
            </option>
            {companies.map((company) => (
              <option key={company.id} value={company.id} className="text-gray-900">
                {company.name}
              </option>
            ))}
          </select>
        )}
      </div>
    </div>
  );
}

export default CompanySwitcher;
