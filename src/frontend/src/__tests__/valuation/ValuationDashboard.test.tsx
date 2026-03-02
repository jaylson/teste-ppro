import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import ValuationDashboardPage from '../../pages/valuation/ValuationDashboardPage';

// ─── Mocks ────────────────────────────────────────────────────────────────────

vi.mock('../../hooks', () => ({
  useValuations: vi.fn(),
}));

vi.mock('../../stores/clientStore', () => ({
  useClientStore: vi.fn(),
}));

import { useValuations } from '../../hooks';
import { useClientStore } from '../../stores/clientStore';

const mockValuations = [
  {
    id: 'val-1',
    clientId: 'client-1',
    companyId: 'company-1',
    valuationDate: '2026-01-15',
    eventType: 'series_a',
    eventName: 'Series A',
    valuationAmount: 10000000,
    totalShares: 1000000,
    pricePerShare: 10,
    status: 'approved',
    notes: null,
    submittedAt: '2026-01-14T10:00:00Z',
    approvedAt: '2026-01-15T10:00:00Z',
    rejectedAt: null,
    rejectionReason: null,
    createdAt: '2026-01-10T10:00:00Z',
    updatedAt: '2026-01-15T10:00:00Z',
    methods: [
      {
        id: 'method-1',
        valuationId: 'val-1',
        methodType: 'arr_multiple',
        isSelected: true,
        calculatedValue: 10000000,
        inputs: null,
        dataSource: null,
        notes: null,
        formulaVersionId: null,
        createdAt: '2026-01-10T10:00:00Z',
        updatedAt: '2026-01-10T10:00:00Z',
      },
    ],
  },
  {
    id: 'val-2',
    clientId: 'client-1',
    companyId: 'company-1',
    valuationDate: '2025-07-01',
    eventType: 'seed',
    eventName: 'Seed',
    valuationAmount: 3000000,
    totalShares: 1000000,
    pricePerShare: 3,
    status: 'approved',
    notes: null,
    submittedAt: '2025-06-30T10:00:00Z',
    approvedAt: '2025-07-01T10:00:00Z',
    rejectedAt: null,
    rejectionReason: null,
    createdAt: '2025-06-25T10:00:00Z',
    updatedAt: '2025-07-01T10:00:00Z',
    methods: [],
  },
];

describe('ValuationDashboardPage', () => {
  beforeEach(() => {
    vi.mocked(useClientStore).mockReturnValue({ selectedCompanyId: 'company-1' } as any);
  });

  it('shows "selecione uma empresa" when no company is selected', () => {
    vi.mocked(useClientStore).mockReturnValue({ selectedCompanyId: null } as any);
    vi.mocked(useValuations).mockReturnValue({ data: undefined, isLoading: false } as any);

    render(<MemoryRouter><ValuationDashboardPage /></MemoryRouter>);
    expect(screen.getByText(/Selecione uma empresa/i)).toBeInTheDocument();
  });

  it('shows spinner while loading', () => {
    vi.mocked(useValuations).mockReturnValue({ data: undefined, isLoading: true } as any);

    render(<MemoryRouter><ValuationDashboardPage /></MemoryRouter>);
    // Spinner renders <div class="spinner ...">
    expect(document.querySelector('.spinner')).toBeTruthy();
  });

  it('renders dashboard title', () => {
    vi.mocked(useValuations).mockReturnValue({
      data: { items: mockValuations, total: 2, page: 1, pageSize: 100, totalPages: 1 },
      isLoading: false,
    } as any);

    render(<MemoryRouter><ValuationDashboardPage /></MemoryRouter>);
    expect(screen.getByText('Dashboard de Valuation')).toBeInTheDocument();
  });

  it('renders 4 KPI cards', () => {
    vi.mocked(useValuations).mockReturnValue({
      data: { items: mockValuations, total: 2, page: 1, pageSize: 100, totalPages: 1 },
      isLoading: false,
    } as any);

    render(<MemoryRouter><ValuationDashboardPage /></MemoryRouter>);
    expect(screen.getByText('Valuation Atual')).toBeInTheDocument();
    expect(screen.getByText('Variação vs Anterior')).toBeInTheDocument();
    expect(screen.getByText('Price per Share')).toBeInTheDocument();
    expect(screen.getByText('Valuations')).toBeInTheDocument();
  });

  it('displays approved valuation count', () => {
    vi.mocked(useValuations).mockReturnValue({
      data: { items: mockValuations, total: 2, page: 1, pageSize: 100, totalPages: 1 },
      isLoading: false,
    } as any);

    render(<MemoryRouter><ValuationDashboardPage /></MemoryRouter>);
    // 2 approved valuations
    expect(screen.getByText('2')).toBeInTheDocument();
  });

  it('shows empty state message for charts with insufficient data', () => {
    // Only 1 approved valuation → LineChart needs at least 2
    vi.mocked(useValuations).mockReturnValue({
      data: { items: [mockValuations[0]], total: 1, page: 1, pageSize: 100, totalPages: 1 },
      isLoading: false,
    } as any);

    render(<MemoryRouter><ValuationDashboardPage /></MemoryRouter>);
    expect(screen.getByText(/Dados insuficientes para o gráfico/i)).toBeInTheDocument();
  });
});
