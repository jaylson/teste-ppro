import { useState, useRef, DragEvent, ChangeEvent } from 'react';
import { UploadCloud, X } from 'lucide-react';
import { Button } from '@/components/ui';
import { useCreateDocument } from '@/hooks';
import { DocumentVisibility, type CreateDocumentRequest } from '@/types';
import toast from 'react-hot-toast';

// ─── Props ────────────────────────────────────────────────────────────────────

export interface DocumentUploaderProps {
  companyId: string;
  referenceType?: string;
  referenceId?: string;
  maxFileSizeMb?: number;
  onUploaded?: () => void;
  onCancel?: () => void;
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

const TYPE_OPTIONS = [
  { value: 'other', label: 'Outro' },
  { value: 'term_sheet', label: 'Term Sheet' },
  { value: 'sha', label: 'SHA' },
  { value: 'investment_contract', label: 'Contrato de Investimento' },
  { value: 'founding_agreement', label: 'Acordo de Fundadores' },
  { value: 'financial_report', label: 'Relatório Financeiro' },
  { value: 'valuation_report', label: 'Relatório de Valuation' },
  { value: 'vesting_agreement', label: 'Acordo de Vesting' },
  { value: 'board_resolution', label: 'Resolução do Conselho' },
];

function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

// ─── Component ────────────────────────────────────────────────────────────────

export default function DocumentUploader({
  companyId,
  referenceType,
  referenceId,
  maxFileSizeMb = 50,
  onUploaded,
  onCancel,
}: DocumentUploaderProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [dragOver, setDragOver] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [name, setName] = useState('');
  const [docType, setDocType] = useState('other');
  const [description, setDescription] = useState('');
  const [visibility, setVisibility] = useState(DocumentVisibility.Internal);

  const createDoc = useCreateDocument();

  function handleFile(file: File) {
    const maxBytes = maxFileSizeMb * 1024 * 1024;
    if (file.size > maxBytes) {
      toast.error(`Arquivo excede o limite de ${maxFileSizeMb} MB`);
      return;
    }
    setSelectedFile(file);
    if (!name) setName(file.name.replace(/\.[^.]+$/, ''));
  }

  function handleDrop(e: DragEvent<HTMLDivElement>) {
    e.preventDefault();
    setDragOver(false);
    const file = e.dataTransfer.files[0];
    if (file) handleFile(file);
  }

  function handleInput(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (file) handleFile(file);
  }

  async function handleSubmit() {
    if (!selectedFile) return;
    if (!name.trim()) {
      toast.error('Informe um nome para o documento');
      return;
    }

    // In a real implementation, the file would be uploaded to object storage (S3/Azure Blob)
    // and the returned storagePath/downloadUrl would be passed here.
    // For now, we register the metadata with a dummy path until file upload is integrated.
    const req: CreateDocumentRequest = {
      companyId,
      name: name.trim(),
      documentType: docType,
      description: description.trim() || undefined,
      fileName: selectedFile.name,
      fileSizeBytes: selectedFile.size,
      mimeType: selectedFile.type || 'application/octet-stream',
      storagePath: `uploads/${Date.now()}_${selectedFile.name}`,
      visibility,
      entityType: referenceType,
      entityId: referenceId,
    };

    await createDoc.mutateAsync(req);
    onUploaded?.();
    setSelectedFile(null);
    setName('');
    setDescription('');
  }

  return (
    <div className="space-y-4">
      {/* Drop zone */}
      {!selectedFile ? (
        <div
          onDragOver={(e) => { e.preventDefault(); setDragOver(true); }}
          onDragLeave={() => setDragOver(false)}
          onDrop={handleDrop}
          onClick={() => fileInputRef.current?.click()}
          className={`border-2 border-dashed rounded-xl p-8 text-center cursor-pointer transition-colors ${
            dragOver ? 'border-blue-400 bg-blue-50' : 'border-gray-200 hover:border-blue-300 hover:bg-gray-50'
          }`}
        >
          <UploadCloud className={`w-8 h-8 mx-auto mb-2 ${dragOver ? 'text-blue-500' : 'text-gray-400'}`} />
          <p className="text-sm text-gray-600">
            Arraste um arquivo ou <span className="text-blue-600 font-medium">clique para selecionar</span>
          </p>
          <p className="text-xs text-gray-400 mt-1">Tamanho máximo: {maxFileSizeMb} MB</p>
          <input
            ref={fileInputRef}
            type="file"
            className="hidden"
            onChange={handleInput}
          />
        </div>
      ) : (
        <div className="flex items-center gap-3 p-3 bg-blue-50 border border-blue-200 rounded-xl">
          <UploadCloud className="w-5 h-5 text-blue-500 flex-shrink-0" />
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium text-blue-900 truncate">{selectedFile.name}</p>
            <p className="text-xs text-blue-500">{formatFileSize(selectedFile.size)}</p>
          </div>
          <button
            onClick={() => { setSelectedFile(null); setName(''); }}
            className="text-blue-400 hover:text-blue-700"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
      )}

      {/* Metadata form */}
      {selectedFile && (
        <div className="space-y-3">
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">Nome *</label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Nome do documento"
            />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">Tipo</label>
              <select
                value={docType}
                onChange={(e) => setDocType(e.target.value)}
                className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                {TYPE_OPTIONS.map((o) => (
                  <option key={o.value} value={o.value}>{o.label}</option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">Visibilidade</label>
              <select
                value={visibility}
                onChange={(e) => setVisibility(e.target.value as DocumentVisibility)}
                className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value={DocumentVisibility.Internal}>Interno</option>
                <option value={DocumentVisibility.Admin}>Administrador</option>
                <option value={DocumentVisibility.Shareholder}>Acionista</option>
                <option value={DocumentVisibility.Public}>Público</option>
              </select>
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">Descrição (opcional)</label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              rows={2}
              className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none"
              placeholder="Descrição breve..."
            />
          </div>

          <div className="flex gap-2 justify-end">
            {onCancel && (
              <Button variant="secondary" size="sm" onClick={onCancel}>
                Cancelar
              </Button>
            )}
            <Button
              size="sm"
              onClick={handleSubmit}
              loading={createDoc.isPending}
              disabled={!name.trim()}
            >
              Registrar documento
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}
