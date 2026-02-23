// F3-FE-UI: Contract Actions component
// File: src/frontend/src/components/contracts/ContractActions.tsx
// Author: GitHub Copilot
// Date: 13/02/2026

import React, { useState } from 'react';
import {
  Eye,
  Edit3,
  Trash2,
  Upload,
} from 'lucide-react';
import { Contract, ContractStatus } from '@/types';
import { Button } from '@/components/ui';
import { cn } from '@/utils/cn';
import { UploadNewVersion } from '@/components/contracts/UploadNewVersion';

interface ContractActionsProps {
  contract: Contract;
  onView?: (id: string) => void;
  onEdit?: (id: string) => void;
  onDelete?: (id: string) => void;
  onVersionUploaded?: () => void;
  className?: string;
}

export const ContractActions: React.FC<ContractActionsProps> = ({
  contract,
  onView,
  onEdit,
  onDelete,
  onVersionUploaded,
  className,
}) => {
  const [showUploadModal, setShowUploadModal] = useState(false);

  const canEdit =
    contract.status === ContractStatus.Draft ||
    contract.status === ContractStatus.PendingReview;

  const canUploadVersion =
    contract.status !== ContractStatus.Cancelled &&
    contract.status !== ContractStatus.Expired;

  const handleVersionUploaded = () => {
    setShowUploadModal(false);
    onVersionUploaded?.();
  };

  // Inline actions for common operations
  return (
    <>
      <div className={cn('flex gap-2', className)}>
        {onView && (
          <Button
            variant="ghost"
            size="sm"
            icon={<Eye className="w-4 h-4" />}
            onClick={() => onView(contract.id)}
          >
            Ver
          </Button>
        )}

        {onEdit && canEdit && (
          <Button
            variant="ghost"
            size="sm"
            icon={<Edit3 className="w-4 h-4" />}
            onClick={() => onEdit(contract.id)}
          >
            Editar
          </Button>
        )}

        {onEdit && !canEdit && (
          <div title="Edição bloqueada após geração. Use 'Upload nova versão' para atualizar.">
            <Button
              variant="ghost"
              size="sm"
              icon={<Edit3 className="w-4 h-4 text-gray-400" />}
              disabled
            >
              Editar
            </Button>
          </div>
        )}

        {canUploadVersion && (
          <Button
            variant="ghost"
            size="sm"
            icon={<Upload className="w-4 h-4 text-cyan-600" />}
            onClick={() => setShowUploadModal(true)}
          >
            Upload nova versão
          </Button>
        )}

        {onDelete && contract.status === ContractStatus.Draft && (
          <Button
            variant="ghost"
            size="sm"
            icon={<Trash2 className="w-4 h-4 text-red-600" />}
            onClick={() => onDelete(contract.id)}
          />
        )}
      </div>

      {showUploadModal && (
        <UploadNewVersion
          contractId={contract.id}
          onSuccess={handleVersionUploaded}
          onClose={() => setShowUploadModal(false)}
        />
      )}
    </>
  );
};

export default ContractActions;
