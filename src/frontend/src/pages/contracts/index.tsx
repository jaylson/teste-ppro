import { useState, useEffect } from 'react';
import { Plus, Search, Filter, Download, FileText, Loader2 } from 'lucide-react';
import { Button, Card } from '@/components/ui';
import { ContractCard } from '@/components/contracts';
import {
  CONTRACT_STATUS_CONFIG,
  CONTRACT_TEMPLATE_TYPE_CONFIG,
} from '@/constants/contractConstants';
import type {
  Contract,
  ContractStatus,
  ContractTemplateType,
} from '@/types/contract.types';
import { contractService } from '@/services/contractService';

/**
 * ContractsListPage
 * Página de listagem de contratos com filtros e busca
 * Rotas: /contracts
 */
function ContractsListPage() {
  const [contracts, setContracts] = useState<Contract[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedStatus, setSelectedStatus] = useState<ContractStatus | 'all'>('all');
  const [selectedType, setSelectedType] = useState<ContractTemplateType | 'all'>('all');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 12;

  // Carregar contratos da API
  useEffect(() => {
    loadContracts();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentPage, selectedStatus, selectedType, searchTerm]);

  const loadContracts = async () => {
    try {
      setIsLoading(true);
      setError(null);

      const filters = {
        page: currentPage,
        pageSize,
        search: searchTerm || undefined,
        status: selectedStatus !== 'all' ? selectedStatus : undefined,
        contractType: selectedType !== 'all' ? selectedType : undefined,
      };

      const response = await contractService.getContracts(filters);
      setContracts(response.items);
      setTotalPages(response.totalPages);
      setTotalCount(response.totalCount);
    } catch (err) {
      setError('Erro ao carregar contratos. Tente novamente.');
      console.error('Erro ao carregar contratos:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleRefresh = () => {
    loadContracts();
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Contratos</h1>
          <p className="mt-1 text-sm text-gray-500">
            Gerencie e acompanhe todos os seus contratos em um só lugar
          </p>
        </div>
        <Button
          variant="primary"
          size="lg"
          icon={<Plus className="w-5 h-5" />}
          onClick={() => (window.location.href = '/contracts/builder')}
        >
          Novo Contrato
        </Button>
      </div>

      {/* Filters and Search */}
      <Card className="p-4">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          {/* Search */}
          <div className="relative md:col-span-2">
            <Search className="absolute left-3 top-3 w-5 h-5 text-gray-400" />
            <input
              type="text"
              placeholder="Buscar contratos..."
              value={searchTerm}
              onChange={(e) => {
                setSearchTerm(e.target.value);
                setCurrentPage(1); // Reset to first page on search
              }}
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-cyan-600"
            />
          </div>

          {/* Status Filter */}
          <select
            value={selectedStatus}
            onChange={(e) => {
              setSelectedStatus(e.target.value as ContractStatus | 'all');
              setCurrentPage(1);
            }}
            className="px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-cyan-600"
          >
            <option value="all">Todos os Status</option>
            {Object.entries(CONTRACT_STATUS_CONFIG).map(([key, config]) => (
              <option key={key} value={key}>
                {config.label}
              </option>
            ))}
          </select>

          {/* Type Filter */}
          <select
            value={selectedType}
            onChange={(e) => {
              setSelectedType(e.target.value as ContractTemplateType | 'all');
              setCurrentPage(1);
            }}
            className="px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-cyan-600"
          >
            <option value="all">Todos os Tipos</option>
            {Object.entries(CONTRACT_TEMPLATE_TYPE_CONFIG).map(([key, config]) => (
              <option key={key} value={key}>
                {config.label}
              </option>
            ))}
          </select>
        </div>
      </Card>

      {/* Results Header */}
      <div className="flex items-center justify-between">
        <p className="text-sm text-gray-500">
          {totalCount} contrato{totalCount !== 1 ? 's' : ''}
          {totalPages > 1 && ` - Página ${currentPage} de ${totalPages}`}
        </p>
        <div className="flex gap-2">
          <Button
            variant="secondary"
            size="sm"
            icon={<Download className="w-4 h-4" />}
          >
            Exportar
          </Button>
          <Button
            variant="secondary"
            size="sm"
            icon={<Filter className="w-4 h-4" />}
          >
            Filtros Avançados
          </Button>
        </div>
      </div>

      {/* Loading State */}
      {isLoading && (
        <Card className="p-12 text-center">
          <Loader2 className="w-16 h-16 text-cyan-600 mx-auto mb-4 animate-spin" />
          <p className="text-gray-500">Carregando contratos...</p>
        </Card>
      )}

      {/* Error State */}
      {!isLoading && error && (
        <Card className="p-12 text-center">
          <div className="text-red-600 mb-4">⚠️</div>
          <h3 className="text-lg font-medium text-gray-900 mb-2">Erro ao carregar</h3>
          <p className="text-gray-500 mb-4">{error}</p>
          <Button variant="primary" onClick={handleRefresh}>
            Tentar Novamente
          </Button>
        </Card>
      )}

      {/* Contracts Grid */}
      {!isLoading && !error && contracts.length > 0 && (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {contracts.map((contract) => (
              <ContractCard
                key={contract.id}
                contract={contract}
                onUpdate={handleRefresh}
                onView={(id) => (window.location.href = `/contracts/${id}`)}
              />
            ))}
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex justify-center items-center gap-2 mt-6">
              <Button
                variant="secondary"
                size="sm"
                disabled={currentPage === 1}
                onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
              >
                Anterior
              </Button>
              <span className="text-sm text-gray-600">
                Página {currentPage} de {totalPages}
              </span>
              <Button
                variant="secondary"
                size="sm"
                disabled={currentPage === totalPages}
                onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
              >
                Próxima
              </Button>
            </div>
          )}
        </>
      )}

      {/* Empty State */}
      {!isLoading && !error && contracts.length === 0 && (
        <Card className="p-12 text-center">
          <FileText className="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            Nenhum contrato encontrado
          </h3>
          <p className="text-gray-500 mb-6">
            {searchTerm || selectedStatus !== 'all' || selectedType !== 'all'
              ? 'Tente ajustar seus filtros'
              : 'Crie seu primeiro contrato para começar'}
          </p>
          <Button
            variant="primary"
            icon={<Plus className="w-4 h-4" />}
            onClick={() => (window.location.href = '/contracts/builder')}
          >
            Criar Primeiro Contrato
          </Button>
        </Card>
      )}
    </div>
  );
}

export default ContractsListPage;
