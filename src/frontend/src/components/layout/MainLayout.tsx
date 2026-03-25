import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';
import Header from './Header';
import { useSessionTimeout } from '@/hooks/useSessionTimeout';

function SessionWarningModal({
  remainingSeconds,
  onExtend,
  onLogout,
}: {
  remainingSeconds: number;
  onExtend: () => void;
  onLogout: () => void;
}) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-sm p-6 space-y-4">
        <div className="flex items-center gap-3">
          <span className="flex items-center justify-center w-10 h-10 rounded-full bg-amber-100 text-amber-600 flex-shrink-0">
            <svg xmlns="http://www.w3.org/2000/svg" className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v4m0 4h.01M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z" />
            </svg>
          </span>
          <h2 className="text-lg font-semibold text-gray-800">Sessão prestes a expirar</h2>
        </div>
        <p className="text-sm text-gray-600">
          Por inatividade, sua sessão será encerrada em{' '}
          <span className="font-bold text-amber-600">{remainingSeconds}s</span>.
          Deseja continuar?
        </p>
        <div className="flex gap-3 pt-1">
          <button
            onClick={onExtend}
            className="flex-1 bg-primary text-white rounded-lg px-4 py-2 text-sm font-medium hover:opacity-90 transition"
          >
            Continuar sessão
          </button>
          <button
            onClick={onLogout}
            className="flex-1 border border-gray-300 text-gray-700 rounded-lg px-4 py-2 text-sm font-medium hover:bg-gray-50 transition"
          >
            Sair agora
          </button>
        </div>
      </div>
    </div>
  );
}

export default function MainLayout() {
  const { showWarning, remainingSeconds, extendSession, handleLogout } = useSessionTimeout();

  return (
    <div className="min-h-screen bg-background">
      <Sidebar />
      <main className="main-content">
        <div className="p-6 space-y-6">
          <Header />
          <Outlet />
        </div>
      </main>
      {showWarning && (
        <SessionWarningModal
          remainingSeconds={remainingSeconds}
          onExtend={extendSession}
          onLogout={handleLogout}
        />
      )}
    </div>
  );
}
