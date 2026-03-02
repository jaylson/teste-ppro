import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import DocumentManager from '../../components/documents/DocumentManager';

// Mock child components to isolate DocumentManager logic
vi.mock('../../components/documents/DocumentList', () => ({
  default: ({ companyId, referenceType, referenceId }: { companyId: string; referenceType?: string; referenceId?: string }) => (
    <div data-testid="document-list" data-company={companyId} data-ref-type={referenceType} data-ref-id={referenceId}>
      Document List
    </div>
  ),
}));

vi.mock('../../components/documents/DocumentUploader', () => ({
  default: ({ onUploaded, onCancel }: { onUploaded: () => void; onCancel: () => void }) => (
    <div data-testid="document-uploader">
      <button onClick={onUploaded}>Confirmar Upload</button>
      <button onClick={onCancel}>Cancelar</button>
    </div>
  ),
}));

describe('DocumentManager', () => {
  const defaultProps = {
    companyId: 'company-1',
    referenceType: 'valuation' as const,
    referenceId: 'val-1',
  };

  it('renders title and document list by default', () => {
    render(<DocumentManager {...defaultProps} />);
    expect(screen.getByText('Documentos')).toBeInTheDocument();
    expect(screen.getByTestId('document-list')).toBeInTheDocument();
  });

  it('renders custom title', () => {
    render(<DocumentManager {...defaultProps} title="Docs do Valuation" />);
    expect(screen.getByText('Docs do Valuation')).toBeInTheDocument();
  });

  it('shows Add button when not readonly', () => {
    render(<DocumentManager {...defaultProps} readonly={false} />);
    expect(screen.getByText('Adicionar')).toBeInTheDocument();
  });

  it('does not show Add button when readonly', () => {
    render(<DocumentManager {...defaultProps} readonly={true} />);
    expect(screen.queryByText('Adicionar')).not.toBeInTheDocument();
  });

  it('shows uploader when Add button is clicked', async () => {
    render(<DocumentManager {...defaultProps} />);
    fireEvent.click(screen.getByText('Adicionar'));
    expect(screen.getByTestId('document-uploader')).toBeInTheDocument();
    // Add button should be hidden while uploader is open
    expect(screen.queryByText('Adicionar')).not.toBeInTheDocument();
  });

  it('hides uploader and refreshes list after upload', async () => {
    render(<DocumentManager {...defaultProps} />);
    fireEvent.click(screen.getByText('Adicionar'));
    expect(screen.getByTestId('document-uploader')).toBeInTheDocument();
    fireEvent.click(screen.getByText('Confirmar Upload'));
    await waitFor(() => {
      expect(screen.queryByTestId('document-uploader')).not.toBeInTheDocument();
    });
    // Add button should reappear
    expect(screen.getByText('Adicionar')).toBeInTheDocument();
  });

  it('hides uploader on cancel', async () => {
    render(<DocumentManager {...defaultProps} />);
    fireEvent.click(screen.getByText('Adicionar'));
    fireEvent.click(screen.getByText('Cancelar'));
    await waitFor(() => {
      expect(screen.queryByTestId('document-uploader')).not.toBeInTheDocument();
    });
  });

  it('passes companyId and referenceType/Id to DocumentList', () => {
    render(<DocumentManager {...defaultProps} />);
    const list = screen.getByTestId('document-list');
    expect(list).toHaveAttribute('data-company', 'company-1');
    expect(list).toHaveAttribute('data-ref-type', 'valuation');
    expect(list).toHaveAttribute('data-ref-id', 'val-1');
  });
});
