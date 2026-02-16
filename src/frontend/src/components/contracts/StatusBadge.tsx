// F3-FE-UI: Status Badge component
// File: src/frontend/src/components/contracts/StatusBadge.tsx
// Author: GitHub Copilot
// Date: 13/02/2026

import React from 'react';
import { Badge } from '@/components/ui';
import {
  ContractStatus,
  SignatureStatus,
  ContractTemplateType,
  ClauseType
} from '@/types';
import {
  CONTRACT_STATUS_CONFIG,
  SIGNATURE_STATUS_CONFIG,
  CONTRACT_TEMPLATE_TYPE_CONFIG,
  CLAUSE_TYPE_CONFIG
} from '@/constants';
import { cn } from '@/utils/cn';

interface StatusBadgeProps {
  type: 'contract' | 'signature' | 'template' | 'clause';
  status: ContractStatus | SignatureStatus | ContractTemplateType | ClauseType;
  className?: string;
  showIcon?: boolean;
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({
  type,
  status,
  className,
  showIcon = false
}) => {
  let displayName: string;
  let config: any;

  switch (type) {
    case 'contract':
      config = CONTRACT_STATUS_CONFIG[status as ContractStatus];
      displayName = config.label;
      break;
    case 'signature':
      config = SIGNATURE_STATUS_CONFIG[status as SignatureStatus];
      displayName = config.label;
      break;
    case 'template':
      config = CONTRACT_TEMPLATE_TYPE_CONFIG[status as ContractTemplateType];
      displayName = config.label;
      break;
    case 'clause':
      config = CLAUSE_TYPE_CONFIG[status as ClauseType];
      displayName = config.label;
      break;
    default:
      return null;
  }

  if (!config) return null;

  return (
    <Badge className={cn(config.bgColor, config.textColor, className)}>
      {showIcon && config.icon && <span>{config.icon} </span>}
      {displayName}
    </Badge>
  );
};

export default StatusBadge;
