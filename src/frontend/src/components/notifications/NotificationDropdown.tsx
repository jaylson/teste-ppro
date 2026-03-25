import { useNavigate } from 'react-router-dom';
import {
  Bell,
  CheckCheck,
  Info,
  AlertTriangle,
  CheckCircle,
  FileText,
  ArrowRight,
} from 'lucide-react';
import { useRecentNotifications, useMarkAsRead, useMarkAllAsRead } from '@/hooks/useNotifications';
import type { Notification } from '@/types/phase6';
import { cn } from '@/utils/cn';

function formatRelativeTime(dateStr: string): string {
  const now = new Date();
  const date = new Date(dateStr);
  const diffMs = now.getTime() - date.getTime();
  const diffSec = Math.floor(diffMs / 1000);
  const diffMin = Math.floor(diffSec / 60);
  const diffHour = Math.floor(diffMin / 60);
  const diffDay = Math.floor(diffHour / 24);

  if (diffSec < 60) return 'agora mesmo';
  if (diffMin < 60) return `${diffMin}min atrás`;
  if (diffHour < 24) return `${diffHour}h atrás`;
  if (diffDay === 1) return 'ontem';
  if (diffDay < 7) return `${diffDay} dias atrás`;
  return date.toLocaleDateString('pt-BR');
}

function NotificationIcon({ type }: { type: string }) {
  const iconClass = 'w-4 h-4';
  if (type.includes('approval') || type.includes('approved')) return <CheckCircle className={cn(iconClass, 'text-green-500')} />;
  if (type.includes('alert') || type.includes('rejected')) return <AlertTriangle className={cn(iconClass, 'text-red-500')} />;
  if (type.includes('document') || type.includes('contract')) return <FileText className={cn(iconClass, 'text-blue-500')} />;
  return <Info className={cn(iconClass, 'text-primary-500')} />;
}

interface NotificationDropdownProps {
  onClose: () => void;
}

export default function NotificationDropdown({ onClose }: NotificationDropdownProps) {
  const navigate = useNavigate();
  const { data: notifications = [], isLoading } = useRecentNotifications();
  const markAsRead = useMarkAsRead();
  const markAllAsRead = useMarkAllAsRead();

  function resolveNavPath(url: string): string {
    try {
      return new URL(url).pathname;
    } catch {
      return url;
    }
  }

  function handleClickNotification(n: Notification) {
    if (!n.isRead) {
      markAsRead.mutate(n.id);
    }
    if (n.actionUrl) {
      navigate(resolveNavPath(n.actionUrl));
    }
    onClose();
  }

  function handleMarkAll() {
    markAllAsRead.mutate();
  }

  function handleViewAll() {
    navigate('/notifications');
    onClose();
  }

  return (
    <div className="absolute right-0 top-full mt-2 w-80 bg-white rounded-xl shadow-lg border border-primary-100 z-50 overflow-hidden">
      {/* Header */}
      <div className="flex items-center justify-between px-4 py-3 border-b border-primary-100">
        <div className="flex items-center gap-2">
          <Bell className="w-4 h-4 text-primary-600" />
          <span className="font-semibold text-sm text-primary">Notificações</span>
        </div>
        <button
          onClick={handleMarkAll}
          className="text-xs text-primary-500 hover:text-primary flex items-center gap-1 transition-colors"
        >
          <CheckCheck className="w-3.5 h-3.5" />
          Marcar todas
        </button>
      </div>

      {/* List */}
      <div className="max-h-80 overflow-y-auto">
        {isLoading ? (
          <div className="flex items-center justify-center py-8">
            <div className="spinner" />
          </div>
        ) : notifications.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-8 text-primary-400 gap-2">
            <Bell className="w-8 h-8 opacity-40" />
            <p className="text-sm">Nenhuma notificação</p>
          </div>
        ) : (
          notifications.map((n) => (
            <button
              key={n.id}
              onClick={() => handleClickNotification(n)}
              className={cn(
                'w-full flex items-start gap-3 px-4 py-3 text-left hover:bg-primary-50 transition-colors border-b border-primary-50 last:border-b-0',
                !n.isRead && 'bg-blue-50 hover:bg-blue-100'
              )}
            >
              <div className="mt-0.5 shrink-0">
                <NotificationIcon type={n.notificationType} />
              </div>
              <div className="flex-1 min-w-0">
                <p className={cn('text-sm leading-tight', !n.isRead ? 'font-semibold text-primary' : 'text-primary-700')}>
                  {n.title}
                </p>
                <p className="text-xs text-primary-500 mt-0.5 line-clamp-2">{n.body}</p>
                <p className="text-[10px] text-primary-400 mt-1">{formatRelativeTime(n.createdAt)}</p>
              </div>
              {!n.isRead && (
                <span className="mt-1 w-2 h-2 rounded-full bg-blue-500 shrink-0" />
              )}
            </button>
          ))
        )}
      </div>

      {/* Footer */}
      <div className="px-4 py-3 border-t border-primary-100">
        <button
          onClick={handleViewAll}
          className="w-full flex items-center justify-center gap-2 text-sm text-primary-600 hover:text-primary font-medium transition-colors"
        >
          Ver todas as notificações
          <ArrowRight className="w-4 h-4" />
        </button>
      </div>
    </div>
  );
}
