import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuthStore } from './stores/authStore';

// Layouts
import MainLayout from './components/layout/MainLayout';
import AuthLayout from './components/layout/AuthLayout';

// Pages
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import CapTable from './pages/CapTable';
import Users from './pages/settings/Users';
import { Plans, ClientsSubscriptions, BillingDashboard, Invoices } from './pages/billing';
import { ShareholdersListPage, ShareholderDetailPage } from './pages/shareholders';

function App() {
  const { isAuthenticated } = useAuthStore();

  return (
    <Routes>
      {/* Auth Routes */}
      <Route element={<AuthLayout />}>
        <Route path="/login" element={<Login />} />
      </Route>

      {/* Protected Routes */}
      <Route
        element={
          isAuthenticated ? <MainLayout /> : <Navigate to="/login" replace />
        }
      >
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/cap-table" element={<CapTable />} />
        <Route path="/partners" element={<Navigate to="/shareholders" replace />} />
        
        {/* Shareholders Routes */}
        <Route path="/shareholders" element={<ShareholdersListPage />} />
        <Route path="/shareholders/:id" element={<ShareholderDetailPage />} />
        
        {/* Settings Routes */}
        <Route path="/settings/users" element={<Users />} />
        
        {/* Billing Routes */}
        <Route path="/billing" element={<BillingDashboard />} />
        <Route path="/billing/plans" element={<Plans />} />
        <Route path="/billing/clients" element={<ClientsSubscriptions />} />
        <Route path="/billing/invoices" element={<Invoices />} />
      </Route>

      {/* 404 */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default App;
