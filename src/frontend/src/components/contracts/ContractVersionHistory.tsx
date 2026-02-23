// F4-VER-FE-001: Contract Version History Component
// File: src/frontend/src/components/contracts/ContractVersionHistory.tsx
// Author: GitHub Copilot
// Date: 14/02/2026

import React, { useState, useEffect, useCallback } from 'react';
import { History, Download, FileText, Upload, Loader2, AlertCircle } from 'lucide-react';
import type { ContractVersion } from '@/types/contract.types';
import { contractVersionService } from '@/services/contractService';

interface ContractVersionHistoryProps {
  contractId: string;
}

const SOURCE_BADGE: Record<ContractVersion['source'], { label: string; className: string }> = {
  builder: { label: 'Builder', className: 'bg-blue-100 text-blue-700' },
  upload: { label: 'Upload', className: 'bg-green-100 text-green-700' },
};

const FILE_TYPE_BADGE: Record<ContractVersion['fileType'], { label: string; className: string }> = {
  pdf: { label: 'PDF', className: 'bg-red-100 text-red-700' },
  docx: { label: 'DOCX', className: 'bg-indigo-100 text-indigo-700' },
};

const formatBytes = (bytes?: number): string => {
  if (!bytes) return '—';
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
};

const formatDate = (dateStr: string): string => {
  try {
    return new Intl.DateTimeFormat('pt-BR', {
      day: '2-digit', month: '2-digit', year: 'numeric',
      hour: '2-digit', minute: '2-digit',
    }).format(new Date(dateStr));
  } catch {
    return dateStr;
  }
};

export const ContractVersionHistory: React.FC<ContractVersionHistoryProps> = ({
  contractId,
}) => {
  const [versions, setVersions] = useState<ContractVersion[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [downloadingId, setDownloadingId] = useState<string | null>(null);

  const loadVersions = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await contractVersionService.getVersions(contractId);
      setVersions(data);
    } catch {
      setError('Não foi possível carregar o histórico de versões.');
    } finally {
      setIsLoading(false);
    }
  }, [contractId]);

  useEffect(() => {
    loadVersions();
  }, [loadVersions]);

  const handleDownload = async (version: ContractVersion) => {
    setDownloadingId(version.id);
    try {
      const blob = await contractVersionService.downloadVersion(contractId, version.id);
      const extension = version.fileType === 'pdf' ? 'pdf' : 'docx';
      const mimeType = version.fileType === 'pdf'
        ? 'application/pdf'
        : 'application/vnd.openxmlformats-officedocument.wordprocessingml.document';
      const url = URL.createObjectURL(new Blob([blob], { type: mimeType }));
      const link = document.createElement('a');
      link.href = url;
      link.download = `contrato_v${version.versionNumber}.${extension}`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
    } catch {
      // silently fail – could show toast
    } finally {
      setDownloadingId(null);
    }
  };

  return (
    <div className="bg-white rounded-lg border border-gray-200">
      {/* Header */}
      <div className="flex items-center gap-3 px-6 py-4 border-b border-gray-100">
        <History className="w-5 h-5 text-gray-500" />
        <h3 className="text-base font-semibold text-gray-900">Histórico de versões</h3>
        {!isLoading && !error && (
          <span className="ml-auto text-xs text-gray-500">
            {versions.length} {versions.length === 1 ? 'versão' : 'versões'}
          </span>
        )}
      </div>

      {/* Loading */}
      {isLoading && (
        <div className="flex items-center justify-center py-10 gap-2 text-gray-500">
          <Loader2 className="w-5 h-5 animate-spin" />
          <span className="text-sm">Carregando versões...</span>
        </div>
      )}

      {/* Error */}
      {!isLoading && error && (
        <div className="flex items-center gap-2 px-6 py-4 text-sm text-red-700">
          <AlertCircle className="w-4 h-4 shrink-0" />
          {error}
          <button
            onClick={loadVersions}
            className="ml-auto text-xs underline hover:no-underline"
          >
            Tentar novamente
          </button>
        </div>
      )}

      {/* Empty */}
      {!isLoading && !error && versions.length === 0 && (
        <div className="flex flex-col items-center justify-center py-10 text-gray-400">
          <FileText className="w-10 h-10 mb-2" />
          <p className="text-sm">Nenhuma versão registrada ainda.</p>
        </div>
      )}

      {/* Version list */}
      {!isLoading && !error && versions.length > 0 && (
        <ul className="divide-y divide-gray-100">
          {versions.map((v) => {
            const sourceBadge = SOURCE_BADGE[v.source] ?? SOURCE_BADGE.upload;
            const typeBadge = FILE_TYPE_BADGE[v.fileType] ?? FILE_TYPE_BADGE.pdf;
            const isDownloading = downloadingId === v.id;

            return (
              <li key={v.id} className="flex items-center gap-4 px-6 py-4">
                {/* Version number */}
                <div className="w-10 h-10 rounded-full bg-gray-100 flex items-center justify-center shrink-0">
                  <span className="text-sm font-bold text-gray-600">v{v.versionNumber}</span>
                </div>

                {/* Info */}
                <div className="flex-1 min-w-0">
                  <div className="flex flex-wrap items-center gap-2 mb-1">
                    {/* Source badge */}
                    <span
                      className={`inline-flex items-center gap-1 text-xs px-2 py-0.5 rounded-full font-medium ${sourceBadge.className}`}
                    >
                      {v.source === 'builder' ? (
                        <FileText className="w-3 h-3" />
                      ) : (
                        <Upload className="w-3 h-3" />
                      )}
                      {sourceBadge.label}
                    </span>

                    {/* File type badge */}
                    <span
                      className={`inline-flex text-xs px-2 py-0.5 rounded-full font-medium ${typeBadge.className}`}
                    >
                      {typeBadge.label}
                    </span>

                    {/* File size */}
                    <span className="text-xs text-gray-400">{formatBytes(v.fileSize)}</span>
                  </div>

                  <div className="flex flex-wrap items-center gap-3 text-xs text-gray-500">
                    <span>{formatDate(v.createdAt)}</span>
                    {v.createdBy && <span>por {v.createdBy}</span>}
                    {v.notes && (
                      <span className="text-gray-600 italic truncate max-w-xs" title={v.notes}>
                        {v.notes}
                      </span>
                    )}
                  </div>
                </div>

                {/* Download button */}
                <button
                  onClick={() => handleDownload(v)}
                  disabled={isDownloading}
                  className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium text-cyan-700 bg-cyan-50 hover:bg-cyan-100 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed shrink-0"
                  title={`Baixar versão ${v.versionNumber}`}
                >
                  {isDownloading ? (
                    <Loader2 className="w-3.5 h-3.5 animate-spin" />
                  ) : (
                    <Download className="w-3.5 h-3.5" />
                  )}
                  Baixar
                </button>
              </li>
            );
          })}
        </ul>
      )}
    </div>
  );
};

export default ContractVersionHistory;
