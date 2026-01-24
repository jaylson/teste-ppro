import { useState } from 'react';
import { Plus, Search, Edit, Trash2, Building2, DollarSign, Users } from 'lucide-react';
import { Button, Input, Badge, Card } from '@/components/ui';
import { useCompanies, useDeleteCompany } from '@/hooks/useCompanies';
import { CompanyForm } from '@/components/companies/CompanyForm';
import type { Company } from '@/types';

const formatCurrency = (value: number, currency: string = 'BRL') => {
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency,
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(value);
};

export default function Companies() {
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [showForm, setShowForm] = useState(false);
  const [editingCompany, setEditingCompany] = useState<Company | null>(null);

  const { data, isLoading, error } = useCompanies({ page, pageSize: 10, search });
  const deleteCompany = useDeleteCompany();

  const handleDelete = async (company: Company) => {
    if (confirm(`Deseja realmente excluir a empresa "${company.name}"?`)) {
      deleteCompany.mutate(company.id);
    }
  };

  const handleEdit = (company: Company) => {
    setEditingCompany(company);
    setShowForm(true);
  };

  const handleCloseForm = () => {
    setShowForm(false);
    setEditingCompany(null);
  };

  const getStatusBadge = (status: string) => {
    const variants: Record<string, 'active' | 'pending' | 'inactive' | 'vesting' | 'investor' | 'founder'> = {
      Active: 'active',
      Pending: 'pending',
      Inactive: 'inactive',
    };
    const labels: Record<string, string> = {
      Active: 'Ativa',
      Pending: 'Pendente',
      Inactive: 'Inativa',
    };
    return <Badge variant={variants[status] || 'inactive'}>{labels[status] || status}</Badge>;
  };

  if (error) {
    return (
      <div className="p-6">
        <Card className="p-8 text-center">
          <p className="text-red-500">Erro ao carregar empresas</p>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-primary">Empresas</h1>
          <p className="text-primary-500">Gerencie as empresas cadastradas</p>
        </div>
        <Button
          onClick={() => setShowForm(true)}
          icon={<Plus className="w-4 h-4" />}
        >
          Nova Empresa
        </Button>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card className="p-4">
          <div className="flex items-center gap-4">
            <div className="p-3 bg-primary-100 rounded-lg">
              <Building2 className="w-6 h-6 text-primary" />
            </div>
            <div>
              <p className="text-sm text-primary-500">Total de Empresas</p>
              <p className="text-2xl font-bold text-primary">{data?.totalCount || 0}</p>
            </div>
          </div>
        </Card>
        <Card className="p-4">
          <div className="flex items-center gap-4">
            <div className="p-3 bg-success-100 rounded-lg">
              <DollarSign className="w-6 h-6 text-success" />
            </div>
            <div>
              <p className="text-sm text-primary-500">Valuation Total</p>
              <p className="text-2xl font-bold text-primary">
                {formatCurrency(
                  data?.items?.reduce((acc, c) => acc + (c.valuation || 0), 0) || 0
                )}
              </p>
            </div>
          </div>
        </Card>
        <Card className="p-4">
          <div className="flex items-center gap-4">
            <div className="p-3 bg-accent-100 rounded-lg">
              <Users className="w-6 h-6 text-accent" />
            </div>
            <div>
              <p className="text-sm text-primary-500">Empresas Ativas</p>
              <p className="text-2xl font-bold text-primary">
                {data?.items?.filter((c) => c.status === 'Active').length || 0}
              </p>
            </div>
          </div>
        </Card>
      </div>

      {/* Search */}
      <Card className="p-4">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-primary-400" />
          <Input
            placeholder="Buscar por nome ou CNPJ..."
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(1);
            }}
            className="pl-10"
          />
        </div>
      </Card>

      {/* Table */}
      <Card>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-primary-50">
              <tr>
                <th className="px-4 py-3 text-left text-sm font-medium text-primary-600">
                  Empresa
                </th>
                <th className="px-4 py-3 text-left text-sm font-medium text-primary-600">
                  CNPJ
                </th>
                <th className="px-4 py-3 text-left text-sm font-medium text-primary-600">
                  Forma Jurídica
                </th>
                <th className="px-4 py-3 text-right text-sm font-medium text-primary-600">
                  Valuation
                </th>
                <th className="px-4 py-3 text-center text-sm font-medium text-primary-600">
                  Status
                </th>
                <th className="px-4 py-3 text-right text-sm font-medium text-primary-600">
                  Ações
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-primary-100">
              {isLoading ? (
                <tr>
                  <td colSpan={6} className="px-4 py-8 text-center text-primary-400">
                    Carregando...
                  </td>
                </tr>
              ) : data?.items?.length === 0 ? (
                <tr>
                  <td colSpan={6} className="px-4 py-8 text-center text-primary-400">
                    Nenhuma empresa encontrada
                  </td>
                </tr>
              ) : (
                data?.items?.map((company) => (
                  <tr key={company.id} className="hover:bg-primary-25">
                    <td className="px-4 py-4">
                      <div className="flex items-center gap-3">
                        <div className="w-10 h-10 bg-primary-100 rounded-lg flex items-center justify-center">
                          {company.logoUrl ? (
                            <img
                              src={company.logoUrl}
                              alt={company.name}
                              className="w-10 h-10 rounded-lg object-cover"
                            />
                          ) : (
                            <Building2 className="w-5 h-5 text-primary" />
                          )}
                        </div>
                        <div>
                          <p className="font-medium text-primary">{company.name}</p>
                          {company.tradingName && (
                            <p className="text-sm text-primary-400">{company.tradingName}</p>
                          )}
                        </div>
                      </div>
                    </td>
                    <td className="px-4 py-4 text-primary-600 font-mono text-sm">
                      {company.cnpjFormatted}
                    </td>
                    <td className="px-4 py-4 text-primary-600">
                      {company.legalForm}
                    </td>
                    <td className="px-4 py-4 text-right font-medium text-primary">
                      {formatCurrency(company.valuation, company.currency)}
                    </td>
                    <td className="px-4 py-4 text-center">
                      {getStatusBadge(company.status)}
                    </td>
                    <td className="px-4 py-4">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => handleEdit(company)}
                          className="p-2 hover:bg-primary-100 rounded-lg transition-colors"
                          title="Editar"
                        >
                          <Edit className="w-4 h-4 text-primary-500" />
                        </button>
                        <button
                          onClick={() => handleDelete(company)}
                          className="p-2 hover:bg-red-100 rounded-lg transition-colors"
                          title="Excluir"
                        >
                          <Trash2 className="w-4 h-4 text-red-500" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        {data && data.totalPages > 1 && (
          <div className="px-4 py-3 border-t flex items-center justify-between">
            <p className="text-sm text-primary-500">
              Mostrando {(page - 1) * 10 + 1} a {Math.min(page * 10, data.totalCount)} de{' '}
              {data.totalCount} empresas
            </p>
            <div className="flex gap-2">
              <Button
                variant="secondary"
                size="sm"
                disabled={page === 1}
                onClick={() => setPage(page - 1)}
              >
                Anterior
              </Button>
              <Button
                variant="secondary"
                size="sm"
                disabled={page === data.totalPages}
                onClick={() => setPage(page + 1)}
              >
                Próximo
              </Button>
            </div>
          </div>
        )}
      </Card>

      {/* Form Modal */}
      {showForm && (
        <CompanyForm company={editingCompany} onClose={handleCloseForm} />
      )}
    </div>
  );
}
