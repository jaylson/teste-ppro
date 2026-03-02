import { useState } from 'react';
import { PlusCircle, FolderOpen } from 'lucide-react';
import DocumentList from './DocumentList';
import DocumentUploader from './DocumentUploader';

// ─── Props ────────────────────────────────────────────────────────────────────

export interface DocumentManagerProps {
  companyId: string;
  referenceType?: 'valuation' | 'financial_period' | 'contract' | 'general';
  referenceId?: string;
  readonly?: boolean;
  allowedCategories?: string[];
  maxFileSizeMb?: number;
  title?: string;
}

// ─── Component ────────────────────────────────────────────────────────────────

export default function DocumentManager({
  companyId,
  referenceType,
  referenceId,
  readonly = false,
  maxFileSizeMb = 50,
  title = 'Documentos',
}: DocumentManagerProps) {
  const [showUploader, setShowUploader] = useState(false);
  const [refreshKey, setRefreshKey] = useState(0);

  function handleUploaded() {
    setShowUploader(false);
    setRefreshKey((k) => k + 1);
  }

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <FolderOpen className="w-5 h-5 text-blue-500" />
          <h3 className="text-sm font-semibold text-gray-800">{title}</h3>
        </div>
        {!readonly && !showUploader && (
          <button
            onClick={() => setShowUploader(true)}
            className="flex items-center gap-1.5 text-sm text-blue-600 hover:text-blue-700 font-medium"
          >
            <PlusCircle className="w-4 h-4" />
            Adicionar
          </button>
        )}
      </div>

      {/* Uploader */}
      {!readonly && showUploader && (
        <div className="p-4 bg-gray-50 border border-gray-200 rounded-xl">
          <DocumentUploader
            companyId={companyId}
            referenceType={referenceType}
            referenceId={referenceId}
            maxFileSizeMb={maxFileSizeMb}
            onUploaded={handleUploaded}
            onCancel={() => setShowUploader(false)}
          />
        </div>
      )}

      {/* List */}
      <DocumentList
        key={refreshKey}
        companyId={companyId}
        referenceType={referenceType}
        referenceId={referenceId}
        readonly={readonly}
      />
    </div>
  );
}
