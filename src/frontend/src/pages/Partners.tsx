import { useState } from 'react';
import { Search, Plus, Mail, Phone, MoreVertical } from 'lucide-react';
import { Button, Card, Badge, Avatar } from '@/components/ui';

// Mock data
const partners = [
  {
    id: 1,
    name: 'Ricardo Fundador',
    role: 'CEO & Co-Founder',
    type: 'Fundador',
    shares: 400000,
    percentage: 40,
    email: 'ricardo@demo.com',
    phone: '+55 11 99999-0001',
    status: 'Ativo',
    entryDate: '2020-01-15',
  },
  {
    id: 2,
    name: 'Ana Fundadora',
    role: 'CTO & Co-Founder',
    type: 'Fundador',
    shares: 200000,
    percentage: 20,
    email: 'ana@demo.com',
    phone: '+55 11 99999-0002',
    status: 'Ativo',
    entryDate: '2020-01-15',
  },
  {
    id: 3,
    name: 'Fundo ABC Capital',
    role: 'Lead Investor - Series A',
    type: 'Investidor',
    shares: 150000,
    percentage: 15,
    email: 'contato@abccapital.com',
    phone: '+55 11 3333-0000',
    status: 'Ativo',
    entryDate: '2022-06-01',
  },
  {
    id: 4,
    name: 'João Silva',
    role: 'VP of Engineering',
    type: 'Funcionário',
    shares: 50000,
    percentage: 5,
    email: 'joao@demo.com',
    phone: '+55 11 99999-0004',
    status: 'Vesting',
    entryDate: '2023-01-10',
  },
  {
    id: 5,
    name: 'Maria Santos',
    role: 'Head of Sales',
    type: 'Funcionário',
    shares: 30000,
    percentage: 3,
    email: 'maria@demo.com',
    phone: '+55 11 99999-0005',
    status: 'Vesting',
    entryDate: '2023-03-15',
  },
  {
    id: 6,
    name: 'Pedro Costa',
    role: 'Advisor',
    type: 'Advisor',
    shares: 10000,
    percentage: 1,
    email: 'pedro@advisor.com',
    phone: '+55 11 99999-0006',
    status: 'Pendente',
    entryDate: '2024-01-01',
  },
];

export default function Partners() {
  const [search, setSearch] = useState('');
  const [filter, setFilter] = useState<string>('all');

  const filteredPartners = partners.filter((p) => {
    const matchesSearch =
      p.name.toLowerCase().includes(search.toLowerCase()) ||
      p.role.toLowerCase().includes(search.toLowerCase());
    const matchesFilter = filter === 'all' || p.type === filter;
    return matchesSearch && matchesFilter;
  });

  const activeCount = partners.filter((p) => p.status === 'Ativo').length;
  const pendingCount = partners.filter((p) => p.status === 'Pendente').length;

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="page-header">
        <div>
          <h1 className="page-title">Sócios</h1>
          <p className="page-subtitle">
            <span className="text-success font-medium">{activeCount} ativos</span>
            {pendingCount > 0 && (
              <span className="text-warning font-medium ml-2">
                • {pendingCount} pendentes
              </span>
            )}
          </p>
        </div>
        <Button icon={<Plus className="w-4 h-4" />}>Adicionar Sócio</Button>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-4">
        <div className="relative flex-1 max-w-md">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-primary-400" />
          <input
            type="text"
            placeholder="Buscar por nome ou cargo..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-10 pr-4 py-2.5 border border-primary-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-accent"
          />
        </div>
        <div className="flex gap-2">
          {['all', 'Fundador', 'Investidor', 'Funcionário', 'Advisor'].map(
            (type) => (
              <button
                key={type}
                onClick={() => setFilter(type)}
                className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                  filter === type
                    ? 'bg-primary text-white'
                    : 'bg-white text-primary-600 hover:bg-primary-50 border border-primary-200'
                }`}
              >
                {type === 'all' ? 'Todos' : type}
              </button>
            )
          )}
        </div>
      </div>

      {/* Cards Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {filteredPartners.map((partner) => (
          <Card key={partner.id} className="relative group">
            {/* Menu Button */}
            <button className="absolute top-4 right-4 p-1 rounded-lg opacity-0 group-hover:opacity-100 hover:bg-primary-100 transition-all">
              <MoreVertical className="w-5 h-5 text-primary-400" />
            </button>

            {/* Header */}
            <div className="flex items-start gap-4 mb-4">
              <Avatar name={partner.name} size="lg" />
              <div className="flex-1 min-w-0">
                <h3 className="font-semibold text-primary truncate">
                  {partner.name}
                </h3>
                <p className="text-sm text-primary-500 truncate">{partner.role}</p>
                <div className="flex items-center gap-2 mt-2">
                  <Badge
                    variant={
                      partner.type === 'Fundador'
                        ? 'founder'
                        : partner.type === 'Investidor'
                        ? 'investor'
                        : partner.status === 'Vesting'
                        ? 'vesting'
                        : partner.status === 'Pendente'
                        ? 'pending'
                        : 'active'
                    }
                  >
                    {partner.type}
                  </Badge>
                  <Badge
                    variant={
                      partner.status === 'Ativo'
                        ? 'active'
                        : partner.status === 'Vesting'
                        ? 'vesting'
                        : 'pending'
                    }
                  >
                    {partner.status}
                  </Badge>
                </div>
              </div>
            </div>

            {/* Details */}
            <div className="space-y-3 py-4 border-t border-primary-100">
              <div className="flex justify-between text-sm">
                <span className="text-primary-500">Ações</span>
                <span className="font-medium text-primary">
                  {partner.shares.toLocaleString('pt-BR')}
                </span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-primary-500">Participação</span>
                <span className="font-medium text-primary">
                  {partner.percentage}%
                </span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-primary-500">Entrada</span>
                <span className="font-medium text-primary">
                  {new Date(partner.entryDate).toLocaleDateString('pt-BR')}
                </span>
              </div>
            </div>

            {/* Actions */}
            <div className="flex gap-2 pt-4 border-t border-primary-100">
              <Button variant="secondary" size="sm" className="flex-1">
                Ver Detalhes
              </Button>
              <Button
                variant="ghost"
                size="sm"
                className="btn-icon"
                onClick={() => window.location.href = `mailto:${partner.email}`}
              >
                <Mail className="w-4 h-4" />
              </Button>
              <Button
                variant="ghost"
                size="sm"
                className="btn-icon"
                onClick={() => window.location.href = `tel:${partner.phone}`}
              >
                <Phone className="w-4 h-4" />
              </Button>
            </div>
          </Card>
        ))}
      </div>

      {/* Empty State */}
      {filteredPartners.length === 0 && (
        <Card className="text-center py-12">
          <div className="w-16 h-16 bg-primary-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <Search className="w-8 h-8 text-primary-400" />
          </div>
          <h3 className="font-semibold text-primary mb-2">
            Nenhum sócio encontrado
          </h3>
          <p className="text-primary-500 mb-4">
            Tente ajustar os filtros ou realizar uma nova busca.
          </p>
          <Button variant="secondary" onClick={() => { setSearch(''); setFilter('all'); }}>
            Limpar filtros
          </Button>
        </Card>
      )}
    </div>
  );
}
