// F3-FE-UI: Base contract components
// File: src/frontend/src/components/contracts/ContractCard.tsx
// Author: GitHub Copilot
// Date: 13/02/2026

import React, { useState } from 'react';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  Eye,
  Edit3,
  Trash2,
  Users,
  Calendar,
  FileText,
  Download,
  Send,
  Loader2
} from 'lucide-react';
import { Contract, ContractStatus } from '@/types';
import { CONTRACT_STATUS_CONFIG } from '@/constants';
import { formatDate } from '@/utils/dateUtils';
import { cn } from '@/lib/utils';
import { contractService } from '@/services/contractService';

interface ContractCardProps {
  contract: Contract;
  onView?: (id: string) => void;
  onEdit?: (id: string) => void;
  onDelete?: (id: string) => void;
  onSendSignature?: (id: string) => void;
  onUpdate?: () => void; // Callback after update/delete
  className?: string;
}

export const ContractCard: React.FC<ContractCardProps> = ({
  contract,
  onView,
  onEdit,
  onDelete,
  onSendSignature,
  onUpdate,
  className
}) => {
  const [isDeleting, setIsDeleting] = useState(false);
  const [isDownloading, setIsDownloading] = useState(false);
  const statusConfig = CONTRACT_STATUS_CONFIG[contract.status];
  const partyCount = contract.parties?.length || 0;

  const handleDelete = async () => {
    if (!confirm('Tem certeza que deseja excluir este contrato?')) {
      return;
    }

    try {
      setIsDeleting(true);
      await contractService.deleteContract(contract.id);
      if (onDelete) {
        onDelete(contract.id);
      }
      if (onUpdate) {
        onUpdate();
      }
    } catch (error) {
      console.error('Erro ao excluir contrato:', error);
      alert('Erro ao excluir contrato. Tente novamente.');
    } finally {
      setIsDeleting(false);
    }
  };

  const handleDownload = async () => {
    if (!contract.documentPath) {
      return;
    }

    try {
      setIsDownloading(true);
      const blob = await contractService.downloadContractPdf(contract.id);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${contract.title}.pdf`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (error) {
      console.error('Erro ao baixar contrato:', error);
      alert('Erro ao baixar contrato. Tente novamente.');
    } finally {
      setIsDownloading(false);
    }
  };

  return (
    <Card className={cn('hover:shadow-medium transition-shadow', className)}>
      <CardHeader className="pb-3">
        <div className="flex items-start justify-between">
          <div className="flex-1">
            <h3 className="text-lg font-semibold text-gray-900 line-clamp-2">
              {contract.title}
            </h3>
            {contract.description && (
              <p className="text-sm text-gray-600 mt-1 line-clamp-1">
                {contract.description}
              </p>
            )}
          </div>
          <Badge className={statusConfig.bgColor + ' ' + statusConfig.textColor}>
            {statusConfig.label}
          </Badge>
        </div>
      </CardHeader>

      <CardContent>
        <div className="space-y-3">
          {/* Meta Information */}
          <div className="grid grid-cols-2 gap-4 text-sm">
            <div className="flex items-center text-gray-600">
              <FileText className="w-4 h-4 mr-2 text-gray-400" />
              <span>{contract.contractType}</span>
            </div>
            {contract.expirationDate && (
              <div className="flex items-center text-gray-600">
                <Calendar className="w-4 h-4 mr-2 text-gray-400" />
                <span>Expira: {formatDate(contract.expirationDate)}</span>
              </div>
            )}
          </div>

          {/* Parties Info */}
          {partyCount > 0 && (
            <div className="flex items-center text-sm text-gray-600">
              <Users className="w-4 h-4 mr-2 text-gray-400" />
              <span>{partyCount} signatário{partyCount > 1 ? 's' : ''}</span>
            </div>
          )}

          {/* Timestamps */}
          <div className="text-xs text-gray-500 flex gap-4">
            <span>Criado: {formatDate(contract.createdAt)}</span>
            {contract.updatedAt !== contract.createdAt && (
              <span>Atualizado: {formatDate(contract.updatedAt)}</span>
            )}
          </div>

          {/* Actions */}
          <div className="flex gap-2 pt-3 border-t border-gray-100">
            {onView && (
              <Button
                variant="ghost"
                size="sm"
                icon={<Eye className="w-4 h-4" />}
                onClick={() => onView(contract.id)}
              >
                Visualizar
              </Button>
            )}
            {onEdit && [ContractStatus.Draft, ContractStatus.PendingReview].includes(contract.status) && (
              <Button
                variant="secondary"
                size="sm"
                icon={<Edit3 className="w-4 h-4" />}
                onClick={() => onEdit(contract.id)}
              >
                Editar
              </Button>
            )}
            {onSendSignature && contract.status === ContractStatus.Approved && (
              <Button
                variant="primary"
                size="sm"
                icon={<Send className="w-4 h-4" />}
                onClick={() => onSendSignature(contract.id)}
              >
                Enviar Assinatura
              </Button>
            )}
            {contract.documentPath && (
              <Button
                variant="ghost"
                size="sm"
                icon={isDownloading ? <Loader2 className="w-4 h-4 animate-spin" /> : <Download className="w-4 h-4" />}
                onClick={handleDownload}
                disabled={isDownloading}
              >
                Download
              </Button>
            )}
            {contract.status === ContractStatus.Draft && (
              <Button
                variant="ghost"
                size="sm"
                icon={isDeleting ? <Loader2 className="w-4 h-4 animate-spin text-red-600" /> : <Trash2 className="w-4 h-4 text-red-600" />}
                onClick={handleDelete}
                disabled={isDeleting}
              >
              </Button>
            )}
          </div>
        </div>
      </CardContent>
    </Card>
  );
};

export default ContractCard;
