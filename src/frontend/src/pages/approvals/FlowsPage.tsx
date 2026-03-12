import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Plus, Filter, Clock, CheckCircle, XCircle, AlertCircle,
  List, ChevronRight, Search,
} from 'lucide-react';
import { Button, Card, Spinner } from '@/components/ui';
import { useWorkflows, useCreateWorkflow } from '@/hooks/useWorkflows';
import type { Workflow, CreateWorkflowRequest } from '@/types/phase6';
import { formatDate } from '@/utils/format';
import { cn } from '@/utils/cn';

const WORKFLOW_TYPES = [
  { value: '', label: 'Todos os tipos' },
  { value: 'contract_approval', label: 'Aprovação de Contrato' },
  { value: 'shareholder_change', label: 'Alteração Societária' },
  { value: 'communication_approval', label: 'Aprovação de Comunicado' },
  { value: 'document_verification', label: 'Verificação de Documento' },
  { value: 'vesting_approval', label: 'Aprovação de Vesting' },
];

const WORKFLOW_TYPE_LABELS: Record<string, string> = {
  contract_approval: 'Aprovação de Contrato',
  shareholder_change: 'Alteração Societária',
  communication_approval: 'Aprovação de Comunicado',
  document_verification: 'Verificação de Documento',
  vesting_approval: 'Aprovação de Vesting',
};

const STATUS_LABELS: Record<string, string> = {
  pending: 'Pendente',
  in_progress: 'Em andamento',
  approved: 'Aprovado',
  rejected: 'Rejeitado',
  cancelled: 'Cancelado',
};

const STATUS_COLORS: Record<string, string> = {
  pending: 'bg-amber-100 text-amber-700',
  in_progress: 'bg-blue-100 text-blue-700',
  approved: 'bg-green-100 text-green-700',
  rejected: 'bg-red-100 text-red-700',
  cancelled: 'bg-gray-100 text-gray-500',
};

const PRIORITY_LABELS: Record<string, string> = {
  low: 'Baixa', medium: 'Média', high: 'Alta', urgent: 'Urgente',
};

const PRIORITY_COLORS: Record<string, string> = {
  low: 'bg-gray-100 text-gray-600',
  medium: 'bg-blue-100 text-blue-700',
  high: 'bg-amber-100 text-amber-700',
  urgent: 'bg-red-100 text-red-700',
};

const STATUS_ICONS: Record<string, React.ReactNode> = {
  pending: <Clock className="w-4 h-4 text-amber-500" />,
  in_progress: <AlertCircle className="w-4 h-4 text-blue-500" />,
  approved: <CheckCircle className="w-4 h-4 text-green-500" />,
  rejected: <XCircle className="w-4 h-4 text-red-500" />,
  cancelled: <XCircle className="w-4 h-4 text-gray-400" />,
};

const STEP_TYPES = [
  { value: 'approval', label: 'Aprovação' },
  { value: 'review', label: 'Revisão' },
  { value: 'notification', label: 'Notificação' },
  { value: 'automated', label: 'Automático' },
];

const ROLES = [
  'Admin', 'Founder', 'BoardMember', 'Legal', 'Finance', 'HR', 'Employee', 'Investor',
];

// ─── Create Workflow Modal ────────────────────────────────────────────────────

// ─── Due Date Helpers ─────────────────────────────────────────────────────────

type StepDueDateMode = 'none' | 'calendar_days' | 'business_days';

const STEP_DUE_MODE_LABELS: Record<StepDueDateMode, string> = {
  none: 'Sem prazo',
  calendar_days: 'Dias corridos',
  business_days: 'Dias úteis',
};

function addCalendarDays(base: Date, days: number): Date {
  const d = new Date(base);
  d.setDate(d.getDate() + days);
  return d;
}

function addBusinessDays(base: Date, days: number): Date {
  const d = new Date(base);
  let added = 0;
  while (added < days) {
    d.setDate(d.getDate() + 1);
    const day = d.getDay();
    if (day !== 0 && day !== 6) added++;
  }
  return d;
}

function toDateInputValue(d: Date): string {
  return d.toISOString().split('T')[0];
}

function formatPreviewDate(iso: string): string {
  if (!iso) return '';
  const [year, month, day] = iso.split('-');
  return `${day}/${month}/${year}`;
}

/** Avança `base` de acordo com o modo e quantidade de dias de uma etapa */
function advanceByStep(base: Date, mode: StepDueDateMode, days: number): Date {
  if (mode === 'calendar_days' && days > 0) return addCalendarDays(base, days);
  if (mode === 'business_days' && days > 0) return addBusinessDays(base, days);
  return base;
}

/** Dado um índice de etapa, calcula quando ela deve terminar (base = hoje + soma das anteriores) */
function computeStepDueDate(
  steps: StepDraft[],
  targetIdx: number,
): string {
  let cursor = new Date();
  for (let i = 0; i <= targetIdx; i++) {
    cursor = advanceByStep(cursor, steps[i].dueDateMode, steps[i].dueDateDays);
  }
  const mode = steps[targetIdx].dueDateMode;
  return mode !== 'none' && steps[targetIdx].dueDateDays > 0 ? toDateInputValue(cursor) : '';
}

/** Prazo total = hoje + soma acumulada de todos os steps com prazo */
function computeTotalDueDate(steps: StepDraft[]): string {
  let cursor = new Date();
  let hasAny = false;
  for (const s of steps) {
    if (s.dueDateMode !== 'none' && s.dueDateDays > 0) {
      cursor = advanceByStep(cursor, s.dueDateMode, s.dueDateDays);
      hasAny = true;
    }
  }
  return hasAny ? toDateInputValue(cursor) : '';
}

// ─── Step Draft ───────────────────────────────────────────────────────────────

interface StepDraft {
  name: string;
  description: string;
  stepType: string;
  assignedRole: string;
  dueDateMode: StepDueDateMode;
  dueDateDays: number;
}

interface CreateModalProps {
  onClose: () => void;
}

function CreateWorkflowModal({ onClose }: CreateModalProps) {
  const createWorkflow = useCreateWorkflow();
  const [form, setForm] = useState({
    title: '',
    description: '',
    workflowType: 'contract_approval',
    referenceType: 'manual',
    referenceId: crypto.randomUUID(),
    priority: 'medium',
  });
  const [steps, setSteps] = useState<StepDraft[]>([
    { name: '', description: '', stepType: 'approval', assignedRole: 'Admin', dueDateMode: 'none', dueDateDays: 3 },
  ]);

  function addStep() {
    setSteps((s) => [...s, { name: '', description: '', stepType: 'approval', assignedRole: 'Admin', dueDateMode: 'none', dueDateDays: 3 }]);
  }

  function removeStep(idx: number) {
    setSteps((s) => s.filter((_, i) => i !== idx));
  }

  function updateStep(idx: number, field: keyof StepDraft, value: string) {
    setSteps((s) => s.map((step, i) => {
      if (i !== idx) return step;
      if (field === 'dueDateDays') return { ...step, dueDateDays: Number(value) };
      return { ...step, [field]: value };
    }));
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!form.title || steps.some((s) => !s.name)) return;
    const totalDueDate = computeTotalDueDate(steps);
    const payload: CreateWorkflowRequest = {
      title: form.title,
      description: form.description || undefined,
      workflowType: form.workflowType,
      referenceType: form.referenceType,
      referenceId: form.referenceId,
      priority: form.priority,
      dueDate: totalDueDate || undefined,
      steps: steps.map((s, idx) => ({
        name: s.name,
        stepType: s.stepType,
        assignedRole: s.assignedRole || undefined,
        dueDate: computeStepDueDate(steps, idx) || undefined,
      })),
    };
    createWorkflow.mutate(payload, { onSuccess: onClose });
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 overflow-y-auto">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-2xl my-4">
        <div className="flex items-center justify-between px-6 py-4 border-b border-primary-100">
          <h3 className="font-semibold text-primary text-lg">Novo Fluxo de Aprovação</h3>
          <button onClick={onClose} className="text-primary-400 hover:text-primary text-xl leading-none">&times;</button>
        </div>
        <form onSubmit={handleSubmit}>
          <div className="px-6 py-4 space-y-4 max-h-[70vh] overflow-y-auto">
            {/* Título */}
            <div>
              <label className="input-label">Título <span className="text-red-500">*</span></label>
              <input
                className="input"
                value={form.title}
                onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))}
                placeholder="Ex: Aprovação do Contrato Social"
                required
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="input-label">Tipo de Fluxo <span className="text-red-500">*</span></label>
                <select className="input" value={form.workflowType} onChange={(e) => setForm((f) => ({ ...f, workflowType: e.target.value }))}>
                  {WORKFLOW_TYPES.filter((t) => t.value).map((t) => (
                    <option key={t.value} value={t.value}>{t.label}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="input-label">Prioridade</label>
                <select className="input" value={form.priority} onChange={(e) => setForm((f) => ({ ...f, priority: e.target.value }))}>
                  <option value="low">Baixa</option>
                  <option value="medium">Média</option>
                  <option value="high">Alta</option>
                  <option value="urgent">Urgente</option>
                </select>
              </div>
            </div>
            <div>
              <label className="input-label">Descrição</label>
              <textarea
                className="input resize-none"
                rows={2}
                value={form.description}
                onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
                placeholder="Descreva o objetivo deste fluxo..."
              />
            </div>
            {/* Steps */}
            <div>
              <div className="flex items-center justify-between mb-2">
                <label className="input-label mb-0">Etapas <span className="text-red-500">*</span></label>
                <button type="button" onClick={addStep} className="text-sm text-accent hover:underline">+ Adicionar etapa</button>
              </div>
              <div className="space-y-3">
                {steps.map((step, idx) => (
                  <div key={idx} className="border border-primary-100 rounded-xl p-3 bg-primary-50 relative">
                    <div className="flex items-center gap-2 mb-2">
                      <span className="text-xs font-semibold text-primary-500">Etapa {idx + 1}</span>
                      {steps.length > 1 && (
                        <button type="button" onClick={() => removeStep(idx)} className="ml-auto text-red-400 hover:text-red-600 text-xs">Remover</button>
                      )}
                    </div>
                    <div className="grid grid-cols-2 gap-2">
                      <div>
                        <label className="input-label text-xs">Nome <span className="text-red-500">*</span></label>
                        <input className="input text-sm py-1.5" value={step.name} onChange={(e) => updateStep(idx, 'name', e.target.value)} placeholder="Nome da etapa" required />
                      </div>
                      <div>
                        <label className="input-label text-xs">Tipo</label>
                        <select className="input text-sm py-1.5" value={step.stepType} onChange={(e) => updateStep(idx, 'stepType', e.target.value)}>
                          {STEP_TYPES.map((t) => <option key={t.value} value={t.value}>{t.label}</option>)}
                        </select>
                      </div>
                      <div>
                        <label className="input-label text-xs">Papel Responsável</label>
                        <select className="input text-sm py-1.5" value={step.assignedRole} onChange={(e) => updateStep(idx, 'assignedRole', e.target.value)}>
                          <option value="">Qualquer</option>
                          {ROLES.map((r) => <option key={r} value={r}>{r}</option>)}
                        </select>
                      </div>
                      <div>
                        <label className="input-label text-xs">Descrição</label>
                        <input className="input text-sm py-1.5" value={step.description} onChange={(e) => updateStep(idx, 'description', e.target.value)} placeholder="Opcional" />
                      </div>
                    </div>

                    {/* Prazo da etapa */}
                    <div className="mt-2 pt-2 border-t border-primary-100">
                      <label className="input-label text-xs mb-1.5">Prazo desta etapa</label>
                      <div className="flex flex-wrap gap-1.5 mb-1.5">
                        {(Object.keys(STEP_DUE_MODE_LABELS) as StepDueDateMode[]).map((mode) => (
                          <button
                            key={mode}
                            type="button"
                            onClick={() => updateStep(idx, 'dueDateMode', mode)}
                            className={cn(
                              'px-2.5 py-0.5 rounded-full text-xs font-medium border transition-colors',
                              step.dueDateMode === mode
                                ? 'bg-accent text-white border-accent'
                                : 'bg-white text-primary-500 border-primary-200 hover:border-accent hover:text-accent',
                            )}
                          >
                            {STEP_DUE_MODE_LABELS[mode]}
                          </button>
                        ))}
                      </div>
                      {step.dueDateMode !== 'none' && (
                        <div className="flex items-center gap-2">
                          <input
                            type="number"
                            className="input text-sm py-1 w-20"
                            min={1}
                            max={365}
                            value={step.dueDateDays}
                            onChange={(e) => updateStep(idx, 'dueDateDays', String(Math.max(1, Number(e.target.value))))}
                          />
                          <span className="text-xs text-primary-500">
                            {step.dueDateMode === 'business_days' ? 'dias úteis' : 'dias corridos'}
                          </span>
                          {(() => {
                            const preview = computeStepDueDate(steps, idx);
                            return preview ? (
                              <span className="text-xs text-primary-400 ml-1">
                                → <span className="font-semibold text-primary-600">{formatPreviewDate(preview)}</span>
                              </span>
                            ) : null;
                          })()}
                        </div>
                      )}
                    </div>
                  </div>
                ))}
              </div>

              {/* Prazo total do fluxo */}
              {(() => {
                const total = computeTotalDueDate(steps);
                return total ? (
                  <div className="mt-3 flex items-center gap-2 px-3 py-2 bg-accent/5 border border-accent/20 rounded-lg">
                    <Clock className="w-4 h-4 text-accent flex-shrink-0" />
                    <span className="text-sm text-primary-600">
                      Prazo total do fluxo:{' '}
                      <span className="font-semibold text-accent">{formatPreviewDate(total)}</span>
                      <span className="text-xs text-primary-400 ml-1">(soma cumulativa das etapas)</span>
                    </span>
                  </div>
                ) : null;
              })()}
            </div>
          </div>
          <div className="flex justify-end gap-3 px-6 py-4 border-t border-primary-100">
            <Button variant="secondary" type="button" onClick={onClose} disabled={createWorkflow.isPending}>Cancelar</Button>
            <Button type="submit" loading={createWorkflow.isPending} disabled={!form.title || steps.some((s) => !s.name)}>
              Criar Fluxo
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}

// ─── Main Page ────────────────────────────────────────────────────────────────

export default function FlowsPage() {
  const navigate = useNavigate();
  const [typeFilter, setTypeFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [search, setSearch] = useState('');
  const [showCreate, setShowCreate] = useState(false);
  const [page, setPage] = useState(1);

  const { data, isLoading } = useWorkflows({
    workflowType: typeFilter || undefined,
    status: statusFilter || undefined,
    page,
    pageSize: 12,
  });

  const items: Workflow[] = data?.items ?? [];
  const filtered = search
    ? items.filter(
        (w) =>
          w.title.toLowerCase().includes(search.toLowerCase()) ||
          (w.description ?? '').toLowerCase().includes(search.toLowerCase())
      )
    : items;

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div>
          <h1 className="page-title">Fluxos de Aprovação</h1>
          <p className="page-subtitle">Gerencie e acompanhe todos os fluxos de aprovação da empresa</p>
        </div>
        <Button icon={<Plus className="w-4 h-4" />} onClick={() => setShowCreate(true)}>
          Novo Fluxo
        </Button>
      </div>

      {/* Filters */}
      <Card>
        <div className="p-4 flex flex-wrap gap-3 items-center">
          <div className="relative flex-1 min-w-[200px]">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-primary-400" />
            <input
              className="input pl-9 text-sm"
              placeholder="Buscar por título..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
          <div className="flex items-center gap-2">
            <Filter className="w-4 h-4 text-primary-400 shrink-0" />
            <select
              className="input text-sm py-2"
              value={typeFilter}
              onChange={(e) => { setTypeFilter(e.target.value); setPage(1); }}
            >
              {WORKFLOW_TYPES.map((t) => (
                <option key={t.value} value={t.value}>{t.label}</option>
              ))}
            </select>
          </div>
          <select
            className="input text-sm py-2 min-w-[150px]"
            value={statusFilter}
            onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
          >
            <option value="">Todos os status</option>
            {Object.entries(STATUS_LABELS).map(([v, l]) => (
              <option key={v} value={v}>{l}</option>
            ))}
          </select>
        </div>
      </Card>

      {/* Content */}
      {isLoading ? (
        <div className="flex justify-center py-12"><Spinner /></div>
      ) : filtered.length === 0 ? (
        <Card>
          <div className="flex flex-col items-center justify-center py-16 text-primary-400 gap-3">
            <List className="w-10 h-10 opacity-40" />
            <p className="text-sm font-medium">Nenhum fluxo encontrado</p>
            <p className="text-xs text-primary-400">Crie um novo fluxo clicando no botão "Novo Fluxo"</p>
          </div>
        </Card>
      ) : (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
            {filtered.map((workflow) => (
              <Card
                key={workflow.id}
                className="cursor-pointer hover:shadow-md transition-shadow"
                onClick={() => navigate(`/approvals/${workflow.id}`)}
              >
                <div className="p-4">
                  {/* Header row */}
                  <div className="flex items-start justify-between gap-2 mb-3">
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-1.5 mb-1">
                        {STATUS_ICONS[workflow.status]}
                        <span className={cn('px-2 py-0.5 rounded-full text-xs font-medium', STATUS_COLORS[workflow.status])}>
                          {STATUS_LABELS[workflow.status] ?? workflow.status}
                        </span>
                      </div>
                      <h3 className="font-semibold text-primary text-sm truncate">{workflow.title}</h3>
                    </div>
                    <span className={cn('px-2 py-0.5 rounded-full text-xs font-medium shrink-0', PRIORITY_COLORS[workflow.priority])}>
                      {PRIORITY_LABELS[workflow.priority] ?? workflow.priority}
                    </span>
                  </div>

                  {/* Type */}
                  <p className="text-xs text-primary-500 mb-3">
                    {WORKFLOW_TYPE_LABELS[workflow.workflowType] ?? workflow.workflowType}
                  </p>

                  {/* Progress + Due date */}
                  <div className="flex items-center gap-3 text-xs text-primary-500">
                    <div className="flex items-center gap-1.5 flex-1">
                      <div className="flex-1 bg-primary-100 rounded-full h-1.5">
                        <div
                          className="bg-accent rounded-full h-1.5 transition-all"
                          style={{ width: `${Math.round((workflow.currentStep / workflow.totalSteps) * 100)}%` }}
                        />
                      </div>
                      <span className="shrink-0">{workflow.currentStep}/{workflow.totalSteps}</span>
                    </div>
                    {workflow.dueDate && (
                      <span className={cn('flex items-center gap-1 shrink-0', new Date(workflow.dueDate) < new Date() ? 'text-red-500' : '')}>
                        <Clock className="w-3 h-3" />
                        {formatDate(workflow.dueDate)}
                      </span>
                    )}
                    <ChevronRight className="w-3.5 h-3.5 shrink-0 text-primary-300" />
                  </div>
                </div>
              </Card>
            ))}
          </div>

          {/* Pagination */}
          {data && data.totalPages > 1 && (
            <div className="flex items-center justify-between">
              <p className="text-sm text-primary-500">
                {data.total} fluxo{data.total !== 1 ? 's' : ''} encontrado{data.total !== 1 ? 's' : ''}
              </p>
              <div className="flex gap-2">
                <Button variant="secondary" size="sm" disabled={page === 1} onClick={() => setPage((p) => p - 1)}>Anterior</Button>
                <Button variant="secondary" size="sm" disabled={page === data.totalPages} onClick={() => setPage((p) => p + 1)}>Próximo</Button>
              </div>
            </div>
          )}
        </>
      )}

      {showCreate && <CreateWorkflowModal onClose={() => setShowCreate(false)} />}
    </div>
  );
}
