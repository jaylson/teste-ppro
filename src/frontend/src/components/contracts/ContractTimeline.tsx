import { Clock, CheckCircle, AlertTriangle } from 'lucide-react';
import { Card } from '@/components/ui';
import { formatDateTime } from '@/utils/format';
import type { Contract, ContractParty } from '@/types/contract.types';

interface TimelineEvent {
  id: string;
  title: string;
  description?: string;
  date?: string;
  type: 'info' | 'success' | 'warning';
}

interface ContractTimelineProps {
  contract: Contract;
}

const getPartyStatusLabel = (party: ContractParty) => {
  if (party.signatureDate) {
    return `Assinado por ${party.partyName}`;
  }
  return `Aguardando assinatura de ${party.partyName}`;
};

export function ContractTimeline({ contract }: ContractTimelineProps) {
  const events: TimelineEvent[] = [];

  events.push({
    id: 'created',
    title: 'Contrato criado',
    description: contract.createdBy || 'Sistema',
    date: contract.createdAt,
    type: 'info',
  });

  if (contract.updatedAt && contract.updatedAt !== contract.createdAt) {
    events.push({
      id: 'updated',
      title: 'Contrato atualizado',
      description: contract.updatedBy || 'Sistema',
      date: contract.updatedAt,
      type: 'info',
    });
  }

  (contract.parties || []).forEach((party) => {
    events.push({
      id: `party-${party.id}`,
      title: getPartyStatusLabel(party),
      description: party.partyEmail,
      date: party.signatureDate,
      type: party.signatureDate ? 'success' : 'warning',
    });
  });

  const sortedEvents = events.sort((a, b) => {
    if (!a.date && !b.date) return 0;
    if (!a.date) return 1;
    if (!b.date) return -1;
    return new Date(b.date).getTime() - new Date(a.date).getTime();
  });

  return (
    <Card className="p-6">
      <h3 className="text-lg font-semibold text-gray-900 mb-4">Linha do tempo</h3>
      <div className="space-y-4">
        {sortedEvents.map((event) => {
          const icon =
            event.type === 'success'
              ? <CheckCircle className="w-4 h-4 text-green-600" />
              : event.type === 'warning'
                ? <AlertTriangle className="w-4 h-4 text-amber-600" />
                : <Clock className="w-4 h-4 text-gray-500" />;

          return (
            <div key={event.id} className="flex gap-3">
              <div className="flex flex-col items-center">
                <div className="w-8 h-8 rounded-full bg-gray-100 flex items-center justify-center">
                  {icon}
                </div>
                <div className="flex-1 w-px bg-gray-200" />
              </div>
              <div className="pb-4">
                <p className="text-sm font-medium text-gray-900">{event.title}</p>
                {event.description && (
                  <p className="text-xs text-gray-500">{event.description}</p>
                )}
                {event.date && (
                  <p className="text-xs text-gray-400 mt-1">
                    {formatDateTime(event.date)}
                  </p>
                )}
              </div>
            </div>
          );
        })}
      </div>
    </Card>
  );
}
