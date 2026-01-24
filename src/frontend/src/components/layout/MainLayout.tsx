import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';
import Header from './Header';

export default function MainLayout() {
  return (
    <div className="min-h-screen bg-background">
      <Sidebar />
      <main className="main-content">
        <div className="p-6 space-y-6">
          <Header />
          <Outlet />
        </div>
      </main>
    </div>
  );
}
