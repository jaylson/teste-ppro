import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import FinancialDashboardPage from '../../pages/financial/FinancialDashboardPage';

// ─── Mocks ────────────────────────────────────────────────────────────────────

vi.mock('../../hooks', () => ({
  useFinancialDashboard: vi.fn(),
}));

vi.mock('../../stores/clientStore', () => ({
  useClientStore: vi.fn(),
}));

import { useFinancialDashboard } from '../../hooks';
import { useClientStore } from '../../stores/clientStore';

const mockDashboard = {
  companyId: 'company-1',
  year: 2026,
  periods: [
    {
      id: 'period-1',
      clientId: 'client-1',
      companyId: 'company-1',
      year: 2026,
      month: 1,
      periodLabel: 'Jan/2026',
      status: 'approved',
      notes: null,
      submittedAt: '2026-01-31T10:00:00Z',
      approvedAt: '2026-02-01T10:00:00Z',
      lockedAt: null,
      createdAt: '2026-01-01T10:00:00Z',
      updatedAt: '2026-02-01T10:00:00Z',
      metrics: {
        id: 'metric-1',
        periodId: 'period-1',
        grossRevenue: 500000,
        netRevenue: 450000,
        mrr: 100000,
        arr: 1200000,
        cashBalance: 3000000,
        burnRate: 150000,
        runwayMonths: 20,
        runwayStatus: 'green',
        customerCount: 85,
        churnRate: 2.5,
        cac: 1200,
        ltv: 24000,
        ltvToCacRatio: 20,
        nps: 72,
        ebitda: -50000,
        ebitdaMargin: -11.1,
        netIncome: -60000,
        createdAt: '2026-01-01T10:00:00Z',
        updatedAt: '2026-02-01T10:00:00Z',
      },
    },
  ],
  trend: {
    mrrGrowthPercent: 8.5,
    arrCurrentMonth: 1200000,
    avgBurnRate3Months: 145000,
    runwayMonths: 20,
    runwayStatus: 'green',
    avgChurnRate3Months: 2.5,
  },
};

describe('FinancialDashboardPage', () => {
  beforeEach(() => {
    vi.mocked(useClientStore).mockReturnValue({ selectedCompanyId: 'company-1' } as any);
  });

  it('shows "selecione uma empresa" when no company is selected', () => {
    vi.mocked(useClientStore).mockReturnValue({ selectedCompanyId: null } as any);
    vi.mocked(useFinancialDashboard).mockReturnValue({ data: undefined, isLoading: false } as any);

    render(<MemoryRouter><FinancialDashboardPage /></MemoryRouter>);
    expect(screen.getByText(/Selecione uma empresa/i)).toBeInTheDocument();
  });

  it('shows spinner while loading', () => {
    vi.mocked(useFinancialDashboard).mockReturnValue({ data: undefined, isLoading: true } as any);

    render(<MemoryRouter><FinancialDashboardPage /></MemoryRouter>);
    // Spinner renders <div class="spinner ...">
    expect(document.querySelector('.spinner')).toBeTruthy();
  });

  it('renders dashboard title', () => {
    vi.mocked(useFinancialDashboard).mockReturnValue({ data: mockDashboard, isLoading: false } as any);

    render(<MemoryRouter><FinancialDashboardPage /></MemoryRouter>);
    expect(screen.getByText('Dashboard Financeiro')).toBeInTheDocument();
  });

  it('renders all 4 KPI labels', () => {
    vi.mocked(useFinancialDashboard).mockReturnValue({ data: mockDashboard, isLoading: false } as any);

    render(<MemoryRouter><FinancialDashboardPage /></MemoryRouter>);
    expect(screen.getByText('MRR Atual')).toBeInTheDocument();
    expect(screen.getByText('ARR Projetado')).toBeInTheDocument();
    expect(screen.getByText('Burn Rate')).toBeInTheDocument();
    expect(screen.getByText('Runway')).toBeInTheDocument();
  });

  it('shows runway gauge with green status', () => {
    vi.mocked(useFinancialDashboard).mockReturnValue({ data: mockDashboard, isLoading: false } as any);

    render(<MemoryRouter><FinancialDashboardPage /></MemoryRouter>);
    expect(screen.getByText('Saudável (>12 meses)')).toBeInTheDocument();
  });

  it('shows current year in chart headings', () => {
    const currentYear = new Date().getFullYear();
    vi.mocked(useFinancialDashboard).mockReturnValue({ data: mockDashboard, isLoading: false } as any);

    render(<MemoryRouter><FinancialDashboardPage /></MemoryRouter>);
    const yearHeadings = screen.getAllByText(new RegExp(String(currentYear)));
    expect(yearHeadings.length).toBeGreaterThan(0);
  });

  it('changes year when navigation buttons are clicked', () => {
    const currentYear = new Date().getFullYear();
    vi.mocked(useFinancialDashboard).mockReturnValue({ data: mockDashboard, isLoading: false } as any);

    render(<MemoryRouter><FinancialDashboardPage /></MemoryRouter>);
    
    // Current year shown
    expect(screen.getByText(String(currentYear))).toBeInTheDocument();
    
    // Click previous year button
    fireEvent.click(screen.getByText('‹'));
    expect(screen.getByText(String(currentYear - 1))).toBeInTheDocument();
    
    // Click next year (back to current)
    fireEvent.click(screen.getByText('›'));
    expect(screen.getByText(String(currentYear))).toBeInTheDocument();
  });

  it('renders "Sem dados" message when no periods exist', () => {
    vi.mocked(useFinancialDashboard).mockReturnValue({
      data: { ...mockDashboard, periods: [], trend: null },
      isLoading: false,
    } as any);

    render(<MemoryRouter><FinancialDashboardPage /></MemoryRouter>);
    // Multiple chart empty state messages
    expect(screen.getAllByText('Sem dados para o período.').length).toBeGreaterThanOrEqual(2);
  });
});
