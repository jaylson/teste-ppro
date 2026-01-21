import { useState } from 'react';
import {
  Search,
  Filter,
  Download,
  Plus,
  TrendingUp,
  Calculator,
} from 'lucide-react';
import { Button, Card, StatCard, Badge, Avatar, ProgressBar, Input } from '@/components/ui';

// Mock data
const shareholders = [
  {
    id: 1,
    name: 'Ricardo Fundador',
    type: 'Fundador',
    shares: 400000,
    percentage: 40,
    value: 6000000,
    status: 'Ativo',
  },
  {
    id: 2,
    name: 'Ana Fundadora',
    type: 'Fundador',
    shares: 200000,
    percentage: 20,
    value: 3000000,
    status: 'Ativo',
  },
  {
    id: 3,
    name: 'Fundo ABC Capital',
    type: 'Investidor',
    shares: 150000,
    percentage: 15,
    value: 2250000,
    status: 'Ativo',
  },
  {
    id: 4,
    name: 'João Silva',
    type: 'Funcionário',
    shares: 50000,
    percentage: 5,
    value: 750000,
    status: 'Vesting',
  },
  {
    id: 5,
    name: 'Pool ESOP',
    type: 'ESOP',
    shares: 120000,
    percentage: 12,
    value: 1800000,
    status: 'Ativo',
  },
];

const distributionData = [
  { type: 'Fundadores', percentage: 60, color: 'bg-info' },
  { type: 'Investidores', percentage: 15, color: 'bg-purple' },
  { type: 'Funcionários', percentage: 8, color: 'bg-accent' },
  { type: 'Pool ESOP', percentage: 12, color: 'bg-success' },
  { type: 'Disponível', percentage: 5, color: 'bg-primary-300' },
];

export default function CapTable() {
  const [search, setSearch] = useState('');

  const filteredShareholders = shareholders.filter(
    (s) =>
      s.name.toLowerCase().includes(search.toLowerCase()) ||
      s.type.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="page-header">
        <div>
          <h1 className="page-title">Cap Table</h1>
          <p className="page-subtitle">
            Valuation atual: <span className="font-semibold text-success">R$ 15.000.000</span>
          </p>
        </div>
        <div className="flex gap-3">
          <Button variant="secondary" icon={<Download className="w-4 h-4" />}>
            Exportar
          </Button>
          <Button variant="success" icon={<Calculator className="w-4 h-4" />}>
            Simular Rodada
          </Button>
          <Button icon={<Plus className="w-4 h-4" />}>Novo Sócio</Button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          icon={<TrendingUp className="w-6 h-6" />}
          iconColor="bg-primary"
          value="1.000.000"
          label="Total de Ações"
          badge={{ value: 'Fully Diluted', variant: 'info' }}
        />
        <StatCard
          icon={<TrendingUp className="w-6 h-6" />}
          iconColor="bg-success"
          value="R$ 15,00"
          label="Preço por Ação"
        />
        <StatCard
          icon={<TrendingUp className="w-6 h-6" />}
          iconColor="bg-info"
          value="850.000"
          label="Em Circulação"
          badge={{ value: '85%', variant: 'info' }}
        />
        <StatCard
          icon={<TrendingUp className="w-6 h-6" />}
          iconColor="bg-purple"
          value="120.000"
          label="Pool Disponível"
          badge={{ value: '12%', variant: 'success' }}
        />
      </div>

      {/* Distribution Bar */}
      <Card>
        <h3 className="font-semibold text-primary mb-4">Distribuição Visual</h3>
        <div className="h-8 rounded-lg overflow-hidden flex">
          {distributionData.map((item, index) => (
            <div
              key={index}
              className={`${item.color} transition-all hover:opacity-80`}
              style={{ width: `${item.percentage}%` }}
              title={`${item.type}: ${item.percentage}%`}
            />
          ))}
        </div>
        <div className="flex flex-wrap gap-4 mt-4">
          {distributionData.map((item, index) => (
            <div key={index} className="flex items-center gap-2">
              <div className={`w-3 h-3 rounded ${item.color}`} />
              <span className="text-sm text-primary-600">
                {item.type} {item.percentage}%
              </span>
            </div>
          ))}
        </div>
        {/* Scale */}
        <div className="flex justify-between mt-4 text-xs text-primary-400">
          <span>0%</span>
          <span>25%</span>
          <span>50%</span>
          <span>75%</span>
          <span>100%</span>
        </div>
      </Card>

      {/* Shareholders Table */}
      <Card>
        <div className="flex items-center justify-between mb-4">
          <h3 className="font-semibold text-primary">Quadro Societário</h3>
          <div className="flex gap-3">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-primary-400" />
              <input
                type="text"
                placeholder="Buscar sócio..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="pl-10 pr-4 py-2 border border-primary-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-accent"
              />
            </div>
            <Button variant="secondary" size="sm" icon={<Filter className="w-4 h-4" />}>
              Filtrar
            </Button>
          </div>
        </div>

        <div className="table-container">
          <table className="table">
            <thead>
              <tr>
                <th>Sócio</th>
                <th>Tipo</th>
                <th>Ações</th>
                <th>Participação</th>
                <th>Valor</th>
                <th>Status</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              {filteredShareholders.map((shareholder) => (
                <tr key={shareholder.id}>
                  <td>
                    <div className="flex items-center gap-3">
                      <Avatar name={shareholder.name} size="sm" />
                      <span className="font-medium">{shareholder.name}</span>
                    </div>
                  </td>
                  <td>
                    <Badge
                      variant={
                        shareholder.type === 'Fundador'
                          ? 'founder'
                          : shareholder.type === 'Investidor'
                          ? 'investor'
                          : shareholder.type === 'ESOP'
                          ? 'active'
                          : 'vesting'
                      }
                    >
                      {shareholder.type}
                    </Badge>
                  </td>
                  <td className="font-mono">
                    {shareholder.shares.toLocaleString('pt-BR')}
                  </td>
                  <td>
                    <div className="flex items-center gap-2">
                      <ProgressBar
                        value={shareholder.percentage}
                        color={
                          shareholder.type === 'Fundador'
                            ? 'bg-info'
                            : shareholder.type === 'Investidor'
                            ? 'bg-purple'
                            : 'bg-success'
                        }
                      />
                      <span className="text-sm font-medium w-12">
                        {shareholder.percentage}%
                      </span>
                    </div>
                  </td>
                  <td className="text-success font-medium">
                    {shareholder.value.toLocaleString('pt-BR', {
                      style: 'currency',
                      currency: 'BRL',
                    })}
                  </td>
                  <td>
                    <Badge
                      variant={
                        shareholder.status === 'Ativo' ? 'active' : 'vesting'
                      }
                    >
                      {shareholder.status}
                    </Badge>
                  </td>
                  <td>
                    <Button variant="ghost" size="sm">
                      •••
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
}
