import { FileText, Calendar, Users, Hash } from 'lucide-react';
import { Card, Badge } from '@/components/ui';
import { StatusBadge } from '@/components/contracts/StatusBadge';
import { CONTRACT_TEMPLATE_TYPE_CONFIG } from '@/constants/contractConstants';
import { formatDate, formatDateTime } from '@/utils/format';
import type { Contract } from '@/types/contract.types';

interface ContractDetailsProps {
  contract: Contract;
}

export function ContractDetails({ contract }: ContractDetailsProps) {
  const templateType = CONTRACT_TEMPLATE_TYPE_CONFIG[contract.contractType];
  const parties = contract.parties || [];
  const clauses = contract.clauses || [];

  return (
    <div className="space-y-6">
      <Card className="p-6">
        <div className="flex items-start justify-between gap-4">
          <div>
            <h2 className="text-2xl font-bold text-gray-900">{contract.title}</h2>
            {contract.description && (
              <p className="text-sm text-gray-600 mt-2">{contract.description}</p>
            )}
          </div>
          <StatusBadge type="contract" status={contract.status} />
        </div>

        <div className="mt-6 grid grid-cols-1 md:grid-cols-2 gap-4 text-sm text-gray-600">
          <div className="flex items-center gap-2">
            <FileText className="w-4 h-4 text-gray-400" />
            <span>
              {templateType?.label || contract.contractType}
            </span>
          </div>
          <div className="flex items-center gap-2">
            <Hash className="w-4 h-4 text-gray-400" />
            <span>ID: {contract.id}</span>
          </div>
          {contract.contractDate && (
            <div className="flex items-center gap-2">
              <Calendar className="w-4 h-4 text-gray-400" />
              <span>Contrato: {formatDate(contract.contractDate)}</span>
            </div>
          )}
          {contract.expirationDate && (
            <div className="flex items-center gap-2">
              <Calendar className="w-4 h-4 text-gray-400" />
              <span>Expira: {formatDate(contract.expirationDate)}</span>
            </div>
          )}
          <div className="flex items-center gap-2">
            <Users className="w-4 h-4 text-gray-400" />
            <span>
              {parties.length} parte{parties.length !== 1 ? 's' : ''}
            </span>
          </div>
          <div className="flex items-center gap-2">
            <Calendar className="w-4 h-4 text-gray-400" />
            <span>Criado: {formatDateTime(contract.createdAt)}</span>
          </div>
        </div>
      </Card>

      <Card className="p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Partes</h3>
        {parties.length === 0 ? (
          <p className="text-sm text-gray-500">Nenhuma parte registrada.</p>
        ) : (
          <div className="space-y-3">
            {parties.map((party) => (
              <div key={party.id} className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-gray-900">{party.partyName}</p>
                  <p className="text-xs text-gray-500">{party.partyEmail}</p>
                </div>
                <Badge className="bg-gray-100 text-gray-700">
                  {party.partyType}
                </Badge>
              </div>
            ))}
          </div>
        )}
      </Card>

      <Card className="p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Clausulas</h3>
        {clauses.length === 0 ? (
          <p className="text-sm text-gray-500">Nenhuma clausula associada.</p>
        ) : (
          <div className="space-y-3">
            {clauses.map((clause) => (
              <div key={clause.id} className="flex items-start justify-between gap-4">
                <div>
                  <p className="text-sm font-medium text-gray-900">
                    {clause.baseClause?.name || 'Clausula personalizada'}
                  </p>
                  {clause.baseClause?.description && (
                    <p className="text-xs text-gray-500">{clause.baseClause.description}</p>
                  )}
                </div>
                <Badge className="bg-slate-100 text-slate-700">
                  Ordem {clause.displayOrder}
                </Badge>
              </div>
            ))}
          </div>
        )}
      </Card>
    </div>
  );
}
