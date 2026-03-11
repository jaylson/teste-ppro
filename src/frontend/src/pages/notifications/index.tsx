import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Bell, CheckCheck, Info, AlertTriangle, CheckCircle, FileText } from 'lucide-react';
import { Button, Card, Spinner } from '@/components/ui';
import { useNotifications, useMarkAsRead, useMarkAllAsRead } from '@/hooks/useNotifications';
import type { Notification } from '@/types/phase6';
import { cn } from '@/utils/cn';

function formatRelativeTime(dateStr: string): string {
  const now = new Date();
  const date = new Date(dateStr);
  const diffMs = now.getTime() - date.getTime();
  const diffMin = Math.floor(diffMs / 60000);
  const diffHour = Math.floor(diffMin / 60);
  const diffDay = Math.floor(diffHour / 24);

  if (diffMin < 1) return 'agora mesmo';
  if (diffMin < 60) return `${diffMin}min atrás`;
  if (diffHour < 24) return `${diffHour}h atrás`;
  if (diffDay === 1) return 'ontem';
  if (diffDay < 7) return `${diffDay} dias atrás`;
  return date.toLocaleDateString('pt-BR');
}

function NotificationIcon({ type }: { type: string }) {
  const cls = 'w-5 h-5';
  if (type.includes('approval') || type.includes('approved')) return <CheckCircle className={cn(cls, 'text-green-500')} />;
  if (type.includes('alert') || type.includes('rejected')) return <AlertTriangle className={cn(cls, 'text-red-500')} />;
  if (type.includes('document') || type.includes('contract')) return <FileText className={cn(cls, 'text-blue-500')} />;
  return <Info className={cn(cls, 'text-primary-500')} />;
}

const TYPE_FILTER_OPTIONS = [
  { value: '', label: 'Todos os tipos' },
  { value: 'approval', label: 'Aprovações' },
  { value: 'alert', label: 'Alertas' },
  { value: 'document', label: 'Documentos' },
  { value: 'contract', label: 'Contratos' },
];

export default function NotificationsPage() {
  const navigate = useNavigate();
  const [typeFilter, setTypeFilter] = useState('');
  const [readFilter, setReadFilter] = useState('');
  const [page, setPage] = useState(1);

  const { data, isLoading } = useNotifications({
    notificationType: typeFilter || undefined,
    isRead: readFilter === '' ? undefined : readFilter === 'read',
    page,
    pageSize: 20,
  });

  const markAsRead = useMarkAsRead();
  const markAllAsRead = useMarkAllAsRead();

  const items = data?.items ?? [];
  const totalPages = data ? Math.ceil(data.total / 20) : 1;

  function handleClick(n: Notification) {
    if (!n.isRead) {
      markAsRead.mutate(n.id);
    }
    if (n.actionUrl) {
      navigate(n.actionUrl);
    }
  }

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="page-title">Notificações</h1>
          <p className="page-subtitle">Todas as suas notificações em um só lugar</p>
        </div>
        <Button
          variant="secondary"
          icon={<CheckCheck className="w-4 h-4" />}
          onClick={() => markAllAsRead.mutate()}
          loading={markAllAsRead.isPending}
        >
          Marcar todas como lidas
        </Button>
      </div>

      {/* Filters */}
      <Card>
        <div className="p-4 flex flex-wrap gap-3">
          <select
            className="input w-auto"
            value={typeFilter}
            onChange={(e) => { setTypeFilter(e.target.value); setPage(1); }}
          >
            {TYPE_FILTER_OPTIONS.map((opt) => (
              <option key={opt.value} value={opt.value}>{opt.label}</option>
            ))}
          </select>
          <select
            className="input w-auto"
            value={readFilter}
            onChange={(e) => { setReadFilter(e.target.value); setPage(1); }}
          >
            <option value="">Todas</option>
            <option value="unread">Não lidas</option>
            <option value="read">Lidas</option>
          </select>
        </div>
      </Card>

      {/* List */}
      {isLoading ? (
        <div className="flex justify-center py-12"><Spinner /></div>
      ) : items.length === 0 ? (
        <Card>
          <div className="flex flex-col items-center justify-center py-12 text-primary-400 gap-2">
            <Bell className="w-10 h-10 opacity-40" />
            <p className="text-sm font-medium">Nenhuma notificação encontrada</p>
          </div>
        </Card>
      ) : (
        <div className="space-y-2">
          {items.map((n) => (
            <button
              key={n.id}
              onClick={() => handleClick(n)}
              className={cn(
                'w-full flex items-start gap-4 p-4 rounded-xl border text-left transition-all hover:shadow-sm',
                n.isRead
                  ? 'bg-white border-primary-100 hover:border-primary-200'
                  : 'bg-blue-50 border-blue-200 hover:border-blue-300'
              )}
            >
              <div className="mt-0.5 shrink-0">
                <NotificationIcon type={n.notificationType} />
              </div>
              <div className="flex-1 min-w-0">
                <div className="flex items-start justify-between gap-2">
                  <p className={cn('text-sm leading-tight', !n.isRead ? 'font-semibold text-primary' : 'text-primary-700')}>
                    {n.title}
                  </p>
                  <span className="text-xs text-primary-400 shrink-0 mt-0.5">
                    {formatRelativeTime(n.createdAt)}
                  </span>
                </div>
                <p className="text-sm text-primary-500 mt-1 line-clamp-2">{n.body}</p>
              </div>
              {!n.isRead && (
                <span className="mt-1.5 w-2.5 h-2.5 rounded-full bg-blue-500 shrink-0" />
              )}
            </button>
          ))}
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex justify-center gap-2">
          <Button
            variant="secondary"
            size="sm"
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
          >
            Anterior
          </Button>
          <span className="flex items-center text-sm text-primary-500 px-3">
            {page} / {totalPages}
          </span>
          <Button
            variant="secondary"
            size="sm"
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            disabled={page === totalPages}
          >
            Próxima
          </Button>
        </div>
      )}
    </div>
  );
}
