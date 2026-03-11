import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { CheckSquare, Clock, CheckCircle, XCircle, AlertCircle } from 'lucide-react';
import { Card, Spinner } from '@/components/ui';
import { useWorkflows, usePendingWorkflows } from '@/hooks/useWorkflows';
import type { Workflow } from '@/types/phase6';
import { formatDate } from '@/utils/format';
import { cn } from '@/utils/cn';

type Tab = 'pending' | 'in_progress' | 'completed' | 'all';

const PRIORITY_COLORS: Record<string, string> = {
  low: 'bg-gray-100 text-gray-600',
  medium: 'bg-blue-100 text-blue-700',
  high: 'bg-amber-100 text-amber-700',
  urgent: 'bg-red-100 text-red-700',
};

const PRIORITY_LABELS: Record<string, string> = {
  low: 'Baixa',
  medium: 'Média',
  high: 'Alta',
  urgent: 'Urgente',
};

const STATUS_ICONS: Record<string, React.ReactNode> = {
  pending: <Clock className="w-4 h-4 text-amber-500" />,
  in_progress: <AlertCircle className="w-4 h-4 text-blue-500" />,
  approved: <CheckCircle className="w-4 h-4 text-green-500" />,
  rejected: <XCircle className="w-4 h-4 text-red-500" />,
  cancelled: <XCircle className="w-4 h-4 text-gray-400" />,
};

function WorkflowCard({ workflow }: { workflow: Workflow }) {
  const navigate = useNavigate();

  return (
    <Card
      className="cursor-pointer hover:shadow-md transition-shadow"
      onClick={() => navigate(`/approvals/${workflow.id}`)}
    >
      <div className="p-4">
        <div className="flex items-start justify-between gap-3 mb-3">
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 mb-1">
              {STATUS_ICONS[workflow.status]}
              <h3 className="font-semibold text-primary text-sm truncate">{workflow.title}</h3>
            </div>
            <p className="text-xs text-primary-500">{workflow.workflowType}</p>
          </div>
          <span className={cn('px-2 py-0.5 rounded-full text-xs font-medium shrink-0', PRIORITY_COLORS[workflow.priority] || 'bg-gray-100 text-gray-600')}>
            {PRIORITY_LABELS[workflow.priority] || workflow.priority}
          </span>
        </div>

        <div className="flex items-center gap-4 text-xs text-primary-500">
          <div className="flex items-center gap-1.5 flex-1">
            <div className="flex-1 bg-primary-100 rounded-full h-1.5">
              <div
                className="bg-accent rounded-full h-1.5 transition-all"
                style={{ width: `${(workflow.currentStep / workflow.totalSteps) * 100}%` }}
              />
            </div>
            <span className="shrink-0">{workflow.currentStep}/{workflow.totalSteps}</span>
          </div>
          {workflow.dueDate && (
            <span className={cn('flex items-center gap-1', new Date(workflow.dueDate) < new Date() ? 'text-red-500' : '')}>
              <Clock className="w-3 h-3" />
              {formatDate(workflow.dueDate)}
            </span>
          )}
        </div>
      </div>
    </Card>
  );
}

export default function ApprovalsPage() {
  const [activeTab, setActiveTab] = useState<Tab>('pending');

  const { data: allData, isLoading: allLoading } = useWorkflows(
    activeTab !== 'pending' ? { status: activeTab === 'all' ? undefined : activeTab } : undefined
  );
  const { data: pendingList, isLoading: pendingLoading } = usePendingWorkflows();

  const isLoading = activeTab === 'pending' ? pendingLoading : allLoading;
  const items: Workflow[] =
    activeTab === 'pending'
      ? pendingList ?? []
      : allData?.items ?? [];

  const tabs: { key: Tab; label: string; icon: React.ReactNode }[] = [
    { key: 'pending', label: 'Pendentes para mim', icon: <Clock className="w-4 h-4" /> },
    { key: 'in_progress', label: 'Em andamento', icon: <AlertCircle className="w-4 h-4" /> },
    { key: 'completed', label: 'Concluídos', icon: <CheckCircle className="w-4 h-4" /> },
    { key: 'all', label: 'Todos', icon: <CheckSquare className="w-4 h-4" /> },
  ];

  return (
    <div className="space-y-6 animate-fade-in">
      <div>
        <h1 className="page-title">Aprovações</h1>
        <p className="page-subtitle">Acompanhe e gerencie aprovações e workflows</p>
      </div>

      <div className="flex gap-1 p-1 bg-primary-50 rounded-xl w-fit flex-wrap">
        {tabs.map((tab) => (
          <button
            key={tab.key}
            onClick={() => setActiveTab(tab.key)}
            className={cn(
              'flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-all',
              activeTab === tab.key
                ? 'bg-white text-primary shadow-sm'
                : 'text-primary-500 hover:text-primary'
            )}
          >
            {tab.icon}
            {tab.label}
          </button>
        ))}
      </div>

      {isLoading ? (
        <div className="flex justify-center py-12"><Spinner /></div>
      ) : items.length === 0 ? (
        <Card>
          <div className="flex flex-col items-center justify-center py-12 text-primary-400 gap-2">
            <CheckSquare className="w-10 h-10 opacity-40" />
            <p className="text-sm font-medium">Nenhum workflow encontrado</p>
          </div>
        </Card>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
          {items.map((workflow) => (
            <WorkflowCard key={workflow.id} workflow={workflow} />
          ))}
        </div>
      )}
    </div>
  );
}
