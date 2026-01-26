import { Building2, Users, Wallet, PieChart } from 'lucide-react';
import { StatCard } from '@/components/ui';
import { CapTable } from '@/types';
import { formatCurrency, formatNumber } from '@/utils/format';

interface CapTableStatsProps {
  data?: CapTable | null;
  isLoading?: boolean;
}

export function CapTableStats({ data, isLoading }: CapTableStatsProps) {
  if (isLoading) {
    return (
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {[...Array(4)].map((_, i) => (
          <div key={i} className="stat-card animate-pulse">
            <div className="stat-card-icon bg-gray-200"></div>
            <div className="flex-1 space-y-2">
              <div className="h-6 bg-gray-200 rounded w-20"></div>
              <div className="h-4 bg-gray-200 rounded w-24"></div>
            </div>
          </div>
        ))}
      </div>
    );
  }

  const totalShares = data?.totalShares ?? 0;
  const totalValue = data?.totalValue ?? 0;
  const shareholderCount = data?.entries?.length ?? 0;
  const shareClassCount = new Set(data?.entries?.map(e => e.shareClassName) ?? []).size;

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
      <StatCard
        icon={<PieChart className="w-5 h-5 text-white" />}
        iconColor="bg-primary"
        value={formatNumber(totalShares)}
        label="Total de Ações"
      />
      
      <StatCard
        icon={<Wallet className="w-5 h-5 text-white" />}
        iconColor="bg-success"
        value={formatCurrency(totalValue)}
        label="Valor Total"
      />
      
      <StatCard
        icon={<Users className="w-5 h-5 text-white" />}
        iconColor="bg-info"
        value={shareholderCount}
        label="Acionistas"
      />
      
      <StatCard
        icon={<Building2 className="w-5 h-5 text-white" />}
        iconColor="bg-warning"
        value={shareClassCount}
        label="Classes de Ações"
      />
    </div>
  );
}
