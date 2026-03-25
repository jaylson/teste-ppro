import { useState, useRef, useEffect } from 'react';
import { Bell } from 'lucide-react';
import { useQueryClient } from '@tanstack/react-query';
import { useUnreadNotificationsCount } from '@/hooks/useNotifications';
import NotificationDropdown from './NotificationDropdown';

export default function NotificationBell() {
  const [isOpen, setIsOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);
  const queryClient = useQueryClient();
  const { data: count = 0 } = useUnreadNotificationsCount();

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (ref.current && !ref.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  function handleToggle() {
    if (!isOpen) {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
    }
    setIsOpen((prev) => !prev);
  }

  return (
    <div ref={ref} className="relative">
      <button
        onClick={handleToggle}
        className="relative p-2 rounded-lg text-primary-600 hover:bg-primary-50 hover:text-primary transition-colors"
        aria-label="Notificações"
      >
        <Bell className="w-5 h-5" />
        {count > 0 && (
          <span className="absolute -top-0.5 -right-0.5 min-w-[18px] h-[18px] flex items-center justify-center bg-red-500 text-white text-[10px] font-bold rounded-full px-1">
            {count > 99 ? '99+' : count}
          </span>
        )}
      </button>

      {isOpen && <NotificationDropdown onClose={() => setIsOpen(false)} />}
    </div>
  );
}
