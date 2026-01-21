import { forwardRef, ButtonHTMLAttributes, InputHTMLAttributes } from 'react';
import { cn } from '@/utils/cn';
import { Loader2 } from 'lucide-react';

/* =====================================================
   BUTTON
   ===================================================== */
export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'success' | 'danger' | 'ghost';
  size?: 'sm' | 'md' | 'lg';
  loading?: boolean;
  icon?: React.ReactNode;
}

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      className,
      variant = 'primary',
      size = 'md',
      loading = false,
      icon,
      children,
      disabled,
      ...props
    },
    ref
  ) => {
    const variants = {
      primary: 'btn-primary',
      secondary: 'btn-secondary',
      success: 'btn-success',
      danger: 'btn-danger',
      ghost: 'btn-ghost',
    };

    const sizes = {
      sm: 'btn-sm',
      md: '',
      lg: 'btn-lg',
    };

    return (
      <button
        ref={ref}
        className={cn('btn', variants[variant], sizes[size], className)}
        disabled={disabled || loading}
        {...props}
      >
        {loading ? (
          <Loader2 className="w-4 h-4 animate-spin" />
        ) : icon ? (
          icon
        ) : null}
        {children}
      </button>
    );
  }
);

Button.displayName = 'Button';

/* =====================================================
   INPUT
   ===================================================== */
export interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  hint?: string;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ className, label, error, hint, id, ...props }, ref) => {
    const inputId = id || label?.toLowerCase().replace(/\s/g, '-');

    return (
      <div className="w-full">
        {label && (
          <label htmlFor={inputId} className="input-label">
            {label}
          </label>
        )}
        <input
          ref={ref}
          id={inputId}
          className={cn('input', error && 'input-error', className)}
          {...props}
        />
        {error && <p className="input-error-message">{error}</p>}
        {hint && !error && <p className="input-hint">{hint}</p>}
      </div>
    );
  }
);

Input.displayName = 'Input';

/* =====================================================
   CARD
   ===================================================== */
export interface CardProps extends React.HTMLAttributes<HTMLDivElement> {
  variant?: 'default' | 'flat';
}

export const Card = forwardRef<HTMLDivElement, CardProps>(
  ({ className, variant = 'default', children, ...props }, ref) => {
    return (
      <div
        ref={ref}
        className={cn(variant === 'flat' ? 'card-flat' : 'card', className)}
        {...props}
      >
        {children}
      </div>
    );
  }
);

Card.displayName = 'Card';

/* =====================================================
   STAT CARD
   ===================================================== */
export interface StatCardProps {
  icon: React.ReactNode;
  iconColor?: string;
  value: string | number;
  label: string;
  badge?: {
    value: string;
    variant?: 'success' | 'warning' | 'error' | 'info';
  };
}

export function StatCard({ icon, iconColor = 'bg-primary', value, label, badge }: StatCardProps) {
  const badgeColors = {
    success: 'bg-success-100 text-success-700',
    warning: 'bg-warning-100 text-warning-700',
    error: 'bg-error-100 text-error-700',
    info: 'bg-info-100 text-info-700',
  };

  return (
    <div className="stat-card">
      <div className={cn('stat-card-icon', iconColor)}>{icon}</div>
      <div className="flex-1">
        <div className="flex items-center gap-2">
          <span className="stat-card-value">{value}</span>
          {badge && (
            <span
              className={cn(
                'stat-card-badge',
                badgeColors[badge.variant || 'success']
              )}
            >
              {badge.value}
            </span>
          )}
        </div>
        <span className="stat-card-label">{label}</span>
      </div>
    </div>
  );
}

/* =====================================================
   BADGE
   ===================================================== */
export interface BadgeProps extends React.HTMLAttributes<HTMLSpanElement> {
  variant?: 'active' | 'pending' | 'inactive' | 'vesting' | 'investor' | 'founder';
}

export function Badge({ className, variant = 'active', children, ...props }: BadgeProps) {
  const variants = {
    active: 'badge-active',
    pending: 'badge-pending',
    inactive: 'badge-inactive',
    vesting: 'badge-vesting',
    investor: 'badge-investor',
    founder: 'badge-founder',
  };

  return (
    <span className={cn('badge', variants[variant], className)} {...props}>
      {children}
    </span>
  );
}

/* =====================================================
   AVATAR
   ===================================================== */
export interface AvatarProps extends React.HTMLAttributes<HTMLDivElement> {
  src?: string;
  name?: string;
  size?: 'sm' | 'md' | 'lg' | 'xl';
}

export function Avatar({ src, name, size = 'md', className, ...props }: AvatarProps) {
  const sizes = {
    sm: 'avatar-sm',
    md: 'avatar-md',
    lg: 'avatar-lg',
    xl: 'avatar-xl',
  };

  const initials = name
    ?.split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);

  return (
    <div className={cn('avatar', sizes[size], className)} {...props}>
      {src ? (
        <img src={src} alt={name} className="w-full h-full object-cover" />
      ) : (
        initials || '?'
      )}
    </div>
  );
}

/* =====================================================
   PROGRESS BAR
   ===================================================== */
export interface ProgressBarProps {
  value: number;
  max?: number;
  color?: string;
  showLabel?: boolean;
  size?: 'sm' | 'md' | 'lg';
}

export function ProgressBar({
  value,
  max = 100,
  color = 'bg-accent',
  showLabel = false,
  size = 'md',
}: ProgressBarProps) {
  const percentage = Math.min(Math.max((value / max) * 100, 0), 100);

  const sizes = {
    sm: 'h-1',
    md: 'h-2',
    lg: 'h-3',
  };

  return (
    <div className="w-full">
      <div className={cn('progress-bar', sizes[size])}>
        <div
          className={cn('progress-bar-fill', color)}
          style={{ width: `${percentage}%` }}
        />
      </div>
      {showLabel && (
        <span className="text-xs text-primary-500 mt-1">{percentage.toFixed(0)}%</span>
      )}
    </div>
  );
}

/* =====================================================
   LOADING SPINNER
   ===================================================== */
export function Spinner({ className }: { className?: string }) {
  return <div className={cn('spinner', className)} />;
}

/* =====================================================
   SKELETON
   ===================================================== */
export function Skeleton({ className }: { className?: string }) {
  return <div className={cn('skeleton', className)} />;
}
