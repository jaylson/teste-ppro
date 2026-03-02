import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import DocumentCard from '../../components/documents/DocumentCard';
import type { Document } from '../../types';

const mockDocument: Document = {
  id: 'doc-1',
  clientId: 'client-1',
  companyId: 'company-1',
  name: 'Relatório Valuation Q1 2026',
  documentType: 'valuation_report',
  description: 'Relatório de valuation do primeiro trimestre',
  fileName: 'valuation-q1-2026.pdf',
  fileSizeBytes: 1048576,
  fileSizeFormatted: '1,0 MB',
  mimeType: 'application/pdf',
  storagePath: '/storage/docs/valuation-q1-2026.pdf',
  downloadUrl: 'https://example.com/download/valuation-q1-2026.pdf',
  entityType: 'valuation',
  entityId: 'val-1',
  visibility: 'internal',
  isVerified: false,
  verifiedAt: null,
  createdAt: '2026-03-01T10:00:00Z',
  updatedAt: '2026-03-01T10:00:00Z',
};

describe('DocumentCard', () => {
  it('renders document name and filename', () => {
    render(<DocumentCard document={mockDocument} />);
    expect(screen.getByText('Relatório Valuation Q1 2026')).toBeInTheDocument();
    expect(screen.getByText('valuation-q1-2026.pdf')).toBeInTheDocument();
  });

  it('renders file size and visibility badge', () => {
    render(<DocumentCard document={mockDocument} />);
    expect(screen.getByText('1,0 MB')).toBeInTheDocument();
    expect(screen.getByText('Interno')).toBeInTheDocument();
  });

  it('shows download link when downloadUrl is present', () => {
    render(<DocumentCard document={mockDocument} />);
    const downloadLink = screen.getByTitle('Baixar');
    expect(downloadLink).toBeInTheDocument();
    expect(downloadLink).toHaveAttribute('href', 'https://example.com/download/valuation-q1-2026.pdf');
  });

  it('does not show delete button when readonly=true', () => {
    const onDelete = vi.fn();
    render(<DocumentCard document={mockDocument} readonly={true} onDelete={onDelete} />);
    expect(screen.queryByTitle('Excluir')).not.toBeInTheDocument();
  });

  it('calls onDelete when delete button is clicked', () => {
    const onDelete = vi.fn();
    render(<DocumentCard document={mockDocument} readonly={false} onDelete={onDelete} />);
    fireEvent.click(screen.getByTitle('Excluir'));
    expect(onDelete).toHaveBeenCalledWith('doc-1');
  });

  it('calls onVerify when verify button is clicked', () => {
    const onVerify = vi.fn();
    render(
      <DocumentCard document={mockDocument} readonly={false} onVerify={onVerify} />
    );
    fireEvent.click(screen.getByTitle('Verificar'));
    expect(onVerify).toHaveBeenCalledWith('doc-1');
  });

  it('shows "Verificado" badge when document is verified', () => {
    const verifiedDoc = { ...mockDocument, isVerified: true, verifiedAt: '2026-03-01T12:00:00Z' };
    render(<DocumentCard document={verifiedDoc} />);
    expect(screen.getByText('Verificado')).toBeInTheDocument();
  });

  it('does not show verify button when document is already verified', () => {
    const verifiedDoc = { ...mockDocument, isVerified: true };
    const onVerify = vi.fn();
    render(<DocumentCard document={verifiedDoc} readonly={false} onVerify={onVerify} />);
    expect(screen.queryByTitle('Verificar')).not.toBeInTheDocument();
  });
});
