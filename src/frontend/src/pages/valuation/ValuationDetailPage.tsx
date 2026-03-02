import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  ArrowLeft,
  CheckCircle,
  XCircle,
  Send,
  RotateCcw,
  Calculator,
  Star,
} from 'lucide-react';
import { Button, Card, Spinner, Badge } from '@/components/ui';
import {
  useValuation,
  useSubmitValuation,
  useApproveValuation,
  useRejectValuation,
  useReturnValuationToDraft,
  useSelectValuationMethod,
} from '@/hooks';
import {
  valuationStatusLabels,
  valuationStatusColors,
  valuationMethodLabels,
  valuationEventTypeLabels,
} from '@/types';
import { formatCurrency, formatDate } from '@/utils/format';

export default function ValuationDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [rejectReason, setRejectReason] = useState('');
  const [showRejectForm, setShowRejectForm] = useState(false);

  const { data: valuation, isLoading } = useValuation(id!);
  const submitValuation = useSubmitValuation();
  const approveValuation = useApproveValuation();
  const rejectValuation = useRejectValuation();
  const returnToDraft = useReturnValuationToDraft();
  const selectMethod = useSelectValuationMethod();

  if (isLoading) {
    return (
      <div className="flex justify-center py-20">
        <Spinner className="w-8 h-8" />
      </div>
    );
  }

  if (!valuation) {
    return (
      <div className="text-center py-20 text-gray-400">
        <p>Valuation não encontrado.</p>
      </div>
    );
  }

  const canSubmit = valuation.status === 'draft';
  const canApprove = valuation.status === 'pending_approval';
  const canReject = valuation.status === 'pending_approval';
  const canReturn = valuation.status === 'rejected';

  function handleReject() {
    if (!rejectReason.trim()) return;
    rejectValuation.mutate(
      { id: valuation!.id, data: { reason: rejectReason } },
      { onSuccess: () => setShowRejectForm(false) }
    );
  }

  return (
    <div className="space-y-6 animate-fade-in max-w-4xl">
      {/* Header */}
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate('/valuations')}
          className="p-2 rounded-lg hover:bg-gray-100 text-gray-400 transition-colors"
        >
          <ArrowLeft size={18} />
        </button>
        <div className="flex-1">
          <div className="flex items-center gap-3">
            <h1 className="text-xl font-bold text-gray-900">
              {valuationEventTypeLabels[valuation.eventType] ?? valuation.eventType}
              {valuation.eventName && ` — ${valuation.eventName}`}
            </h1>
            <Badge className={valuationStatusColors[valuation.status] ?? ''}>
              {valuationStatusLabels[valuation.status] ?? valuation.status}
            </Badge>
          </div>
          <p className="text-sm text-gray-500 mt-0.5">{formatDate(valuation.valuationDate)}</p>
        </div>
        {/* Actions */}
        <div className="flex gap-2">
          {canSubmit && (
            <Button
              icon={<Send size={14} />}
              size="sm"
              onClick={() => submitValuation.mutate(valuation.id)}
              loading={submitValuation.isPending}
            >
              Submeter
            </Button>
          )}
          {canApprove && (
            <Button
              icon={<CheckCircle size={14} />}
              size="sm"
              variant="success"
              onClick={() => approveValuation.mutate(valuation.id)}
              loading={approveValuation.isPending}
            >
              Aprovar
            </Button>
          )}
          {canReject && (
            <Button
              icon={<XCircle size={14} />}
              size="sm"
              variant="danger"
              onClick={() => setShowRejectForm(true)}
            >
              Rejeitar
            </Button>
          )}
          {canReturn && (
            <Button
              icon={<RotateCcw size={14} />}
              size="sm"
              variant="secondary"
              onClick={() => returnToDraft.mutate(valuation.id)}
              loading={returnToDraft.isPending}
            >
              Retornar
            </Button>
          )}
        </div>
      </div>

      {/* Reject form */}
      {showRejectForm && (
        <Card>
          <div className="p-4 space-y-3">
            <p className="text-sm font-medium text-gray-700">Motivo da rejeição (obrigatório)</p>
            <textarea
              rows={3}
              value={rejectReason}
              onChange={(e) => setRejectReason(e.target.value)}
              className="w-full text-sm border border-gray-200 rounded-lg p-3 focus:outline-none focus:ring-2 focus:ring-red-400"
              placeholder="Descreva o motivo da rejeição..."
            />
            <div className="flex gap-2 justify-end">
              <Button variant="secondary" size="sm" onClick={() => setShowRejectForm(false)}>
                Cancelar
              </Button>
              <Button
                variant="danger"
                size="sm"
                disabled={!rejectReason.trim()}
                onClick={handleReject}
                loading={rejectValuation.isPending}
              >
                Confirmar Rejeição
              </Button>
            </div>
          </div>
        </Card>
      )}

      {/* Summary */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <Card>
          <div className="p-4">
            <p className="text-xs text-gray-500 mb-1">Valor do Valuation</p>
            <p className="text-xl font-bold text-gray-900">
              {valuation.valuationAmount ? formatCurrency(valuation.valuationAmount) : '—'}
            </p>
          </div>
        </Card>
        <Card>
          <div className="p-4">
            <p className="text-xs text-gray-500 mb-1">Total de Ações</p>
            <p className="text-xl font-bold text-gray-900">
              {valuation.totalShares.toLocaleString('pt-BR')}
            </p>
          </div>
        </Card>
        <Card>
          <div className="p-4">
            <p className="text-xs text-gray-500 mb-1">Preço por Ação</p>
            <p className="text-xl font-bold text-gray-900">
              {valuation.pricePerShare ? `R$ ${valuation.pricePerShare.toFixed(4)}` : '—'}
            </p>
          </div>
        </Card>
        <Card>
          <div className="p-4">
            <p className="text-xs text-gray-500 mb-1">Metodologias</p>
            <p className="text-xl font-bold text-gray-900">{valuation.methods.length}</p>
          </div>
        </Card>
      </div>

      {/* Rejection reason */}
      {valuation.status === 'rejected' && valuation.rejectionReason && (
        <Card>
          <div className="p-4 bg-red-50 rounded-xl">
            <p className="text-sm font-medium text-red-700 mb-1">Motivo da Rejeição</p>
            <p className="text-sm text-red-600">{valuation.rejectionReason}</p>
          </div>
        </Card>
      )}

      {/* Methods */}
      <div>
        <h2 className="text-base font-semibold text-gray-900 mb-3">Metodologias de Cálculo</h2>
        {valuation.methods.length === 0 ? (
          <Card>
            <div className="p-8 text-center text-gray-400">
              <Calculator size={32} className="mx-auto mb-2 opacity-30" />
              <p className="text-sm">Nenhuma metodologia adicionada.</p>
            </div>
          </Card>
        ) : (
          <div className="space-y-3">
            {valuation.methods.map((m) => (
              <Card key={m.id}>
                <div className="p-4 flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    {m.isSelected && (
                      <Star size={16} className="text-yellow-500 fill-yellow-400 flex-shrink-0" />
                    )}
                    <div>
                      <p className="font-medium text-gray-900">
                        {valuationMethodLabels[m.methodType] ?? m.methodType}
                      </p>
                      {m.dataSource && (
                        <p className="text-xs text-gray-400">{m.dataSource}</p>
                      )}
                    </div>
                  </div>
                  <div className="flex items-center gap-3">
                    <div className="text-right">
                      <p className="font-semibold text-gray-900">
                        {m.calculatedValue ? formatCurrency(m.calculatedValue) : '—'}
                      </p>
                      {m.isSelected && (
                        <p className="text-xs text-yellow-600 font-medium">Principal</p>
                      )}
                    </div>
                    {!m.isSelected && m.calculatedValue && valuation.status === 'draft' && (
                      <Button
                        size="sm"
                        variant="secondary"
                        onClick={() =>
                          selectMethod.mutate({ valuationId: valuation.id, methodId: m.id })
                        }
                        loading={selectMethod.isPending}
                      >
                        Selecionar
                      </Button>
                    )}
                  </div>
                </div>
              </Card>
            ))}
          </div>
        )}
      </div>

      {/* Notes */}
      {valuation.notes && (
        <Card>
          <div className="p-4">
            <p className="text-sm font-medium text-gray-700 mb-1">Observações</p>
            <p className="text-sm text-gray-600">{valuation.notes}</p>
          </div>
        </Card>
      )}
    </div>
  );
}
