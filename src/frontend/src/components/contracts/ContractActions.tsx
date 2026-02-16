// F3-FE-UI: Contract Actions component
// File: src/frontend/src/components/contracts/ContractActions.tsx
// Author: GitHub Copilot
// Date: 13/02/2026

import React, { useState } from 'react';
import {
  MoreVertical,
  Eye,
  Edit3,
  Trash2,
  Send,
  Download,
  Copy,
  Archive
} from 'lucide-react';
import { Contract, ContractStatus } from '@/types';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuDivider,
  DropdownMenuItem,
  DropdownMenuTrigger
} from '@/components/ui/dropdown-menu';
import { cn } from '@/lib/utils';

interface ContractActionsProps {
  contract: Contract;
  onView?: (id: string) => void;
  onEdit?: (id: string) => void;
  onClone?: (id: string) => void;
  onDelete?: (id: string) => void;
  onArchive?: (id: string) => void;
  onSendSignature?: (id: string) => void;
  onDownload?: (id: string) => void;
  className?: string;
  variant?: 'icon-only' | 'text' | 'full';
}

export const ContractActions: React.FC<ContractActionsProps> = ({
  contract,
  onView,
  onEdit,
  onClone,
  onDelete,
  onArchive,
  onSendSignature,
  onDownload,
  className,
  variant = 'icon-only'
}) => {
  const [open, setOpen] = useState(false);

  const canEdit =
    contract.status === ContractStatus.Draft ||
    contract.status === ContractStatus.PendingReview;

  const canSendSignature =
    contract.status === ContractStatus.Approved ||
    contract.status === ContractStatus.Draft;

  const hasDocument = !!contract.documentPath;

  // Inline actions for common operations
  if (variant === 'text') {
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
  }

  // Dropdown menu for full action list
  return (
    <DropdownMenu open={open} onOpenChange={setOpen}>
      <DropdownMenuTrigger asChild>
        <Button
          variant="ghost"
          size="sm"
          icon={<MoreVertical className="w-4 h-4" />}
          className={className}
          aria-label="Mais ações"
        />
      </DropdownMenuTrigger>

      <DropdownMenuContent align="end" className="w-48">
        {/* View Action */}
        {onView && (
          <DropdownMenuItem onClick={() => { onView(contract.id); setOpen(false); }}>
            <Eye className="w-4 h-4 mr-2" />
            <span>Visualizar</span>
          </DropdownMenuItem>
        )}

        {/* Edit Action */}
        {onEdit && canEdit && (
          <DropdownMenuItem onClick={() => { onEdit(contract.id); setOpen(false); }}>
            <Edit3 className="w-4 h-4 mr-2" />
            <span>Editar</span>
          </DropdownMenuItem>
        )}

        {/* Clone Action */}
        {onClone && (
          <DropdownMenuItem onClick={() => { onClone(contract.id); setOpen(false); }}>
            <Copy className="w-4 h-4 mr-2" />
            <span>Clonar</span>
          </DropdownMenuItem>
        )}

        {/* Download Action */}
        {onDownload && hasDocument && (
          <DropdownMenuItem onClick={() => { onDownload(contract.id); setOpen(false); }}>
            <Download className="w-4 h-4 mr-2" />
            <span>Baixar PDF</span>
          </DropdownMenuItem>
        )}

        <DropdownMenuDivider />

        {/* Send for Signature */}
        {onSendSignature && canSendSignature && (
          <DropdownMenuItem onClick={() => { onSendSignature(contract.id); setOpen(false); }}>
            <Send className="w-4 h-4 mr-2 text-blue-600" />
            <span>Enviar para Assinatura</span>
          </DropdownMenuItem>
        )}

        {/* Archive Action */}
        {onArchive && contract.status === ContractStatus.Executed && (
          <DropdownMenuItem onClick={() => { onArchive(contract.id); setOpen(false); }}>
            <Archive className="w-4 h-4 mr-2" />
            <span>Arquivar</span>
          </DropdownMenuItem>
        )}

        <DropdownMenuDivider />

        {/* Delete Action */}
        {onDelete && contract.status === ContractStatus.Draft && (
          <DropdownMenuItem
            onClick={() => { onDelete(contract.id); setOpen(false); }}
            className="text-red-600"
          >
            <Trash2 className="w-4 h-4 mr-2" />
            <span>Deletar</span>
          </DropdownMenuItem>
        )}
      </DropdownMenuContent>
    </DropdownMenu>
  );
};

export default ContractActions;
