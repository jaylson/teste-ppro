import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuthStore } from './stores/authStore';

// Layouts
import MainLayout from './components/layout/MainLayout';
import AuthLayout from './components/layout/AuthLayout';

// Pages
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import { CapTablePage, TransactionsPage } from './pages/captable';
import Companies from './pages/Companies';
import Users from './pages/settings/Users';
import { Plans, ClientsSubscriptions, BillingDashboard, Invoices } from './pages/billing';
import { ShareholdersListPage, ShareholderDetailPage } from './pages/shareholders';
import ContractsListPage from './pages/contracts';
import ContractTemplatesPage from './pages/contracts/templates';
import ClausesPage from './pages/contracts/clauses';
import ContractBuilderPage from './pages/contracts/builder';
import ContractDetailsPage from './pages/contracts/[id]';
import { VestingPage, VestingGrantDetailPage, MyVestingPage } from './pages/vesting';

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
        <Route path="/cap-table" element={<CapTablePage />} />
        <Route path="/cap-table/transactions" element={<TransactionsPage />} />
        <Route path="/companies" element={<Companies />} />
        <Route path="/partners" element={<Navigate to="/shareholders" replace />} />
        
        {/* Shareholders Routes */}
        <Route path="/shareholders" element={<ShareholdersListPage />} />
        <Route path="/shareholders/:id" element={<ShareholderDetailPage />} />
        
        {/* Contracts Routes */}
        <Route path="/contracts" element={<ContractsListPage />} />
        <Route path="/contracts/:id" element={<ContractDetailsPage />} />
        <Route path="/contracts/templates" element={<ContractTemplatesPage />} />
        <Route path="/contracts/clauses" element={<ClausesPage />} />
        <Route path="/contracts/builder" element={<ContractBuilderPage />} />
        
        {/* Vesting Routes */}
        <Route path="/vesting" element={<VestingPage />} />
        <Route path="/vesting/:id" element={<VestingPage />} />
        <Route path="/vesting/grant/:id" element={<VestingGrantDetailPage />} />
        <Route path="/my-vesting" element={<MyVestingPage />} />
        
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
