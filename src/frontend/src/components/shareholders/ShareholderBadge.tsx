import { cn } from '@/utils/cn';
import { ShareholderType, ShareholderStatus, shareholderTypeLabels, shareholderStatusLabels } from '@/types';

interface ShareholderTypeBadgeProps {
  type: ShareholderType;
  className?: string;
}

interface ShareholderStatusBadgeProps {
  status: ShareholderStatus;
  className?: string;
}

const typeColors: Record<ShareholderType, string> = {
  [ShareholderType.Founder]: 'bg-purple-100 text-purple-700 border-purple-200',
  [ShareholderType.Investor]: 'bg-blue-100 text-blue-700 border-blue-200',
  [ShareholderType.Employee]: 'bg-green-100 text-green-700 border-green-200',
  [ShareholderType.Advisor]: 'bg-amber-100 text-amber-700 border-amber-200',
  [ShareholderType.ESOP]: 'bg-cyan-100 text-cyan-700 border-cyan-200',
  [ShareholderType.Other]: 'bg-gray-100 text-gray-700 border-gray-200',
};

const statusColors: Record<ShareholderStatus, string> = {
  [ShareholderStatus.Active]: 'bg-success-100 text-success-700 border-success-200',
  [ShareholderStatus.Inactive]: 'bg-gray-100 text-gray-600 border-gray-200',
  [ShareholderStatus.Pending]: 'bg-warning-100 text-warning-700 border-warning-200',
  [ShareholderStatus.Exited]: 'bg-red-100 text-red-600 border-red-200',
};

export function ShareholderTypeBadge({ type, className }: ShareholderTypeBadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border',
        typeColors[type],
        className
      )}
    >
      {shareholderTypeLabels[type]}
    </span>
  );
}

export function ShareholderStatusBadge({ status, className }: ShareholderStatusBadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border',
        statusColors[status],
        className
      )}
    >
      {shareholderStatusLabels[status]}
    </span>
  );
}
