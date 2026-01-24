import { useEffect, useMemo, useState } from 'react';
import { Building2, ChevronsUpDown, Loader2, X } from 'lucide-react';
import { useAuthStore } from '@/stores/authStore';
import { useClientStore } from '@/stores/clientStore';
import { useClientCompanies } from '@/hooks';
import type { ClientCompany } from '@/types';
import { Button } from '@/components/ui';

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
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [tempCompanyId, setTempCompanyId] = useState('');

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

  useEffect(() => {
    if (!isModalOpen) {
      setTempCompanyId(selectedCompanyId || '');
    }
  }, [selectedCompanyId, isModalOpen]);

  const handleOpenModal = () => {
    setTempCompanyId(selectedCompanyId || '');
    setIsModalOpen(true);
  };

  const handleConfirmChange = () => {
    setSelectedCompanyId(tempCompanyId || null);
    setIsModalOpen(false);
  };

  return (
    <>
      <div className="flex items-center gap-3 rounded-lg bg-white px-3 py-2 border border-primary-100 text-primary">
        <div className="flex items-center gap-2 min-w-[160px]">
        <div className="w-10 h-10 rounded-lg bg-white/10 flex items-center justify-center">
          <Building2 className="w-5 h-5" />
        </div>
        <div className="flex flex-col">
          <span className="text-xs text-primary-600">Empresa</span>
          <span className="text-sm font-semibold truncate">
            {activeCompany?.name || 'Selecione uma empresa'}
          </span>
        </div>
        </div>

        <div className="flex-1 flex items-center justify-end">
          {isLoadingCompanies ? (
            <Loader2 className="w-4 h-4 animate-spin text-primary-500" />
          ) : (
            <Button
              type="button"
              variant="ghost"
              size="sm"
              onClick={handleOpenModal}
              className="h-9 w-9 p-0"
              aria-label="Trocar empresa"
              title="Trocar empresa"
            >
              <ChevronsUpDown className="w-4 h-4" />
            </Button>
          )}
        </div>
      </div>

      {isModalOpen && (
        <div className="modal-overlay" onClick={() => setIsModalOpen(false)}>
          <div className="modal" onClick={(event) => event.stopPropagation()}>
            <div className="modal-header">
              <h2 className="modal-title">Trocar empresa</h2>
              <button
                type="button"
                onClick={() => setIsModalOpen(false)}
                className="text-primary-400 hover:text-primary-600"
                aria-label="Fechar"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
            <div className="modal-body space-y-4">
              <div>
                <label className="block text-sm font-medium mb-2">Empresa</label>
                <select
                  className="input w-full"
                  value={tempCompanyId}
                  onChange={(e) => setTempCompanyId(e.target.value)}
                  disabled={isLoadingCompanies}
                >
                  <option value="">Selecione uma empresa</option>
                  {companies.map((company) => (
                    <option key={company.id} value={company.id}>
                      {company.name}
                    </option>
                  ))}
                </select>
              </div>
            </div>
            <div className="modal-footer">
              <Button type="button" variant="secondary" onClick={() => setIsModalOpen(false)}>
                Cancelar
              </Button>
              <Button type="button" onClick={handleConfirmChange} disabled={!tempCompanyId}>
                Confirmar
              </Button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

export default CompanySwitcher;
