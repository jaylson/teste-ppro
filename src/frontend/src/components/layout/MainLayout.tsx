import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';

export default function MainLayout() {
  return (
    <div className="min-h-screen bg-background">
      <Sidebar />
      <main className="main-content">
        <div className="p-6">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
