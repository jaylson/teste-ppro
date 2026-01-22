import { useState, useEffect } from 'react';
import { X, CheckCircle, AlertCircle, Info, AlertTriangle } from 'lucide-react';

export type AlertType = 'success' | 'error' | 'info' | 'warning';

export interface AlertProps {
  type: AlertType;
  message: string;
  onClose?: () => void;
  autoClose?: boolean;
  autoCloseDelay?: number;
}

export function Alert({ type, message, onClose, autoClose = true, autoCloseDelay = 5000 }: AlertProps) {
  const [isVisible, setIsVisible] = useState(true);

  useEffect(() => {
    if (autoClose) {
      const timer = setTimeout(() => {
        handleClose();
      }, autoCloseDelay);

      return () => clearTimeout(timer);
    }
  }, [autoClose, autoCloseDelay]);

  const handleClose = () => {
    setIsVisible(false);
    if (onClose) {
      setTimeout(() => onClose(), 300); // Aguarda animação de saída
    }
  };

  if (!isVisible) return null;

  const alertConfig = {
    success: {
      bg: 'bg-green-50',
      border: 'border-green-200',
      text: 'text-green-800',
      icon: <CheckCircle className="w-5 h-5 text-green-400" />,
    },
    error: {
      bg: 'bg-red-50',
      border: 'border-red-200',
      text: 'text-red-800',
      icon: <AlertCircle className="w-5 h-5 text-red-400" />,
    },
    warning: {
      bg: 'bg-yellow-50',
      border: 'border-yellow-200',
      text: 'text-yellow-800',
      icon: <AlertTriangle className="w-5 h-5 text-yellow-400" />,
    },
    info: {
      bg: 'bg-blue-50',
      border: 'border-blue-200',
      text: 'text-blue-800',
      icon: <Info className="w-5 h-5 text-blue-400" />,
    },
  };

  const config = alertConfig[type];

  return (
    <div
      className={`${config.bg} ${config.border} ${config.text} border rounded-lg p-4 mb-4 flex items-start gap-3 animate-slide-in-top shadow-sm`}
    >
      {config.icon}
      <div className="flex-1">
        <p className="text-sm font-medium">{message}</p>
      </div>
      <button
        onClick={handleClose}
        className="flex-shrink-0 text-gray-400 hover:text-gray-600 transition-colors"
      >
        <X className="w-4 h-4" />
      </button>
    </div>
  );
}

// Hook para gerenciar alertas
export interface AlertState {
  id: number;
  type: AlertType;
  message: string;
}

let alertIdCounter = 0;

export function useAlerts() {
  const [alerts, setAlerts] = useState<AlertState[]>([]);

  const showAlert = (type: AlertType, message: string) => {
    const id = alertIdCounter++;
    setAlerts((prev) => [...prev, { id, type, message }]);
  };

  const removeAlert = (id: number) => {
    setAlerts((prev) => prev.filter((alert) => alert.id !== id));
  };

  return {
    alerts,
    showAlert,
    showSuccess: (message: string) => showAlert('success', message),
    showError: (message: string) => showAlert('error', message),
    showInfo: (message: string) => showAlert('info', message),
    showWarning: (message: string) => showAlert('warning', message),
    removeAlert,
  };
}

// Container de alertas para exibir no topo da página
export interface AlertContainerProps {
  alerts: AlertState[];
  onRemove: (id: number) => void;
}

export function AlertContainer({ alerts, onRemove }: AlertContainerProps) {
  return (
    <div className="fixed top-4 right-4 z-50 max-w-md w-full space-y-2">
      {alerts.map((alert) => (
        <Alert
          key={alert.id}
          type={alert.type}
          message={alert.message}
          onClose={() => onRemove(alert.id)}
        />
      ))}
    </div>
  );
}
