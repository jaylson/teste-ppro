// F3-FE-UI: Contract Actions component
// File: src/frontend/src/components/contracts/ContractActions.tsx
// Author: GitHub Copilot
// Date: 13/02/2026

import React from 'react';
import {
  Eye,
  Edit3,
  Trash2
} from 'lucide-react';
import { Contract, ContractStatus } from '@/types';
import { Button } from '@/components/ui';
import { cn } from '@/utils/cn';

interface ContractActionsProps {
  contract: Contract;
  onView?: (id: string) => void;
  onEdit?: (id: string) => void;
  onDelete?: (id: string) => void;
  className?: string;
}

export const ContractActions: React.FC<ContractActionsProps> = ({
  contract,
  onView,
  onEdit,
  onDelete,
  className,
}) => {
  const canEdit =
    contract.status === ContractStatus.Draft ||
    contract.status === ContractStatus.PendingReview;

  // Inline actions for common operations
  return (
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
      {onDelete && contract.status === ContractStatus.Draft && (
        <Button
          variant="ghost"
          size="sm"
          icon={<Trash2 className="w-4 h-4 text-red-600" />}
          onClick={() => onDelete(contract.id)}
        />
      )}
    </div>
  );
};

export default ContractActions;
