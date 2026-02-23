import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, Search, Filter, Download, FileText, Loader2, Eye } from 'lucide-react';
import { Button, Card } from '@/components/ui';
import { StatusBadge } from '@/components/contracts';
import {
  CONTRACT_STATUS_CONFIG,
  CONTRACT_TEMPLATE_TYPE_CONFIG,
} from '@/constants/contractConstants';
import { formatDate } from '@/utils/format';
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
  const navigate = useNavigate();
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
          onClick={() => navigate('/contracts/builder')}
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

      {/* Contracts Table */}
      {!isLoading && !error && contracts.length > 0 && (
        <>
          <Card className="overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-50 border-b border-gray-200">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Título
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Tipo
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Status
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Partes
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Criado em
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Ações
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {contracts.map((contract) => (
                    <tr
                      key={contract.id}
                      className="hover:bg-gray-50 cursor-pointer"
                      onClick={() => navigate(`/contracts/${contract.id}`)}
                    >
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-3">
                          <FileText className="w-5 h-5 text-cyan-600 flex-shrink-0" />
                          <div>
                            <p className="font-medium text-gray-900 text-sm">{contract.title}</p>
                            {contract.description && (
                              <p className="text-xs text-gray-500 truncate max-w-xs">
                                {contract.description}
                              </p>
                            )}
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-700">
                          {CONTRACT_TEMPLATE_TYPE_CONFIG[contract.contractType]?.label ||
                            contract.contractType}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <StatusBadge type="contract" status={contract.status} />
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {contract.parties?.length ?? 0} parte
                        {(contract.parties?.length ?? 0) !== 1 ? 's' : ''}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {formatDate(contract.createdAt)}
                      </td>
                      <td
                        className="px-6 py-4 whitespace-nowrap text-right"
                        onClick={(e) => e.stopPropagation()}
                      >
                        <Button
                          variant="ghost"
                          size="sm"
                          icon={<Eye className="w-4 h-4" />}
                          onClick={() => navigate(`/contracts/${contract.id}`)}
                        >
                          Ver
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </Card>

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
            onClick={() => navigate('/contracts/builder')}
          >
            Criar Primeiro Contrato
          </Button>
        </Card>
      )}
    </div>
  );
}

export default ContractsListPage;
