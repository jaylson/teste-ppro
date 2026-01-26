// Re-export all types
export * from './auth.types';
export * from './user.types';
export * from './client.types';
export * from './company.types';
export * from './shareholder.types';
export * from './share.types';

// Common types
export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface PaginationParams {
  page?: number;
  pageSize?: number;
  search?: string;
}
