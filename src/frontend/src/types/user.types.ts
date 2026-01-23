// Types para usuários - espelham os DTOs do backend

export interface User {
  id: string;
  companyId: string;
  email: string;
  name: string;
  avatarUrl?: string;
  phone?: string;
  status: UserStatus;
  language: Language;
  timezone: string;
  twoFactorEnabled: boolean;
  lastLoginAt?: string;
  roles: string[];
  createdAt: string;
  updatedAt: string;
}

export interface UserSummary {
  id: string;
  email: string;
  name: string;
  avatarUrl?: string;
  status: string;
  roles: string[];
}

export interface CreateUserRequest {
  name: string;
  email: string;
  password: string;
  phone?: string;
  initialRole: Role;
}

export interface UpdateUserRequest {
  name: string;
  phone?: string;
  avatarUrl?: string;
}

export interface ManageUserRoleRequest {
  role: Role;
  expiresAt?: string;
}

export type UserStatus = 'Active' | 'Inactive' | 'Pending';
export type Language = 'PT' | 'EN' | 'ES';
export type Role =
  | 'SuperAdmin'
  | 'Admin'
  | 'Founder'
  | 'BoardMember'
  | 'Legal'
  | 'Finance'
  | 'HR'
  | 'Employee'
  | 'Investor'
  | 'Viewer';
