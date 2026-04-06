# Stack & Design System — Guia de Replicação

> Documento de referência para replicar a arquitetura frontend, o design system e os padrões de desenvolvimento do Partnership Manager em um novo aplicativo React.

---

## 1. Visão Geral da Stack Frontend

| Camada | Tecnologia | Versão | Papel |
|---|---|---|---|
| Framework UI | React | 18.3 | Renderização de componentes |
| Linguagem | TypeScript | 5.5 | Tipagem estática estrita |
| Build Tool | Vite | 5.3 | Dev server + bundler |
| CSS | Tailwind CSS | 3.4 | Utility-first styling |
| Roteamento | React Router DOM | 6.26 | SPA client-side routing |
| Estado servidor | TanStack React Query | 5.51 | Cache e fetch async |
| Estado global | Zustand | 4.5 | Estado leve (auth, seleções) |
| Formulários | React Hook Form + Zod | 7.52 / 3.23 | Forms + validação de schema |
| HTTP | Axios | 1.7 | Cliente HTTP com interceptors |
| Notificações | react-hot-toast | 2.4 | Toast feedback |
| Ícones | Lucide React | 0.427 | Ícones SVG tree-shakeable |
| Gráficos | Recharts | 2.12 | Visualizações de dados |
| Internacionalização | i18next + react-i18next | 25 / 16 | Suporte pt/en/es |
| Utilitários de classe | clsx + tailwind-merge | — | Merge de classes Tailwind |
| Datas | date-fns | 3.6 | Manipulação de datas |
| Testes | Vitest + Testing Library | 2.1 | Testes unitários e de componentes |

---

## 2. Inicializar Projeto do Zero

```bash
# 1. Scaffold com Vite
npm create vite@latest my-app -- --template react-ts
cd my-app

# 2. Deps de produção
npm install \
  react-router-dom \
  @tanstack/react-query \
  zustand \
  react-hook-form @hookform/resolvers zod \
  axios \
  react-hot-toast \
  lucide-react \
  recharts \
  date-fns \
  clsx tailwind-merge \
  i18next react-i18next i18next-browser-languagedetector

# 3. Deps de desenvolvimento
npm install -D \
  tailwindcss postcss autoprefixer \
  @vitejs/plugin-react \
  @testing-library/react @testing-library/jest-dom @testing-library/user-event \
  vitest @vitest/coverage-v8 jsdom \
  eslint @typescript-eslint/eslint-plugin @typescript-eslint/parser \
  eslint-plugin-react-hooks eslint-plugin-react-refresh \
  prettier

# 4. Inicializar Tailwind
npx tailwindcss init -p
```

---

## 3. Configuração do Vite (`vite.config.ts`)

```ts
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    port: 3000,
    host: '0.0.0.0',
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false,
      },
    },
  },
  build: {
    outDir: 'dist',
    sourcemap: true,
    rollupOptions: {
      output: {
        manualChunks: {
          'vendor-react':  ['react', 'react-dom', 'react-router-dom'],
          'vendor-query':  ['@tanstack/react-query'],
          'vendor-forms':  ['react-hook-form', '@hookform/resolvers', 'zod'],
          'vendor-utils':  ['axios', 'date-fns', 'zustand', 'clsx', 'tailwind-merge'],
          'vendor-charts': ['recharts'],
          'vendor-ui':     ['lucide-react', 'react-hot-toast'],
        },
      },
    },
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: ['./src/test/setup.ts'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'html'],
      include: ['src/**/*.{ts,tsx}'],
      exclude: ['src/test/**', 'src/main.tsx', 'src/**/*.d.ts'],
    },
  },
});
```

---

## 4. TypeScript (`tsconfig.json`)

```json
{
  "compilerOptions": {
    "target": "ES2020",
    "useDefineForClassFields": true,
    "lib": ["ES2020", "DOM", "DOM.Iterable"],
    "module": "ESNext",
    "skipLibCheck": true,
    "moduleResolution": "bundler",
    "allowImportingTsExtensions": true,
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": true,
    "jsx": "react-jsx",
    "strict": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "noFallthroughCasesInSwitch": true,
    "types": ["vitest/globals"],
    "baseUrl": ".",
    "paths": {
      "@/*": ["./src/*"]
    }
  },
  "include": ["src"]
}
```

> O alias `@/` mapeia para `./src/` — use em todos os imports internos.

---

## 5. Design System — Tailwind Config (`tailwind.config.js`)

O design system usa uma paleta baseada em **tons de cinza escuro** como cor primária, **cyan** como accent, e semânticas fixas para success/warning/error/info/purple.

```js
/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        // Primária — cinza escuro #111827
        primary: {
          DEFAULT: '#111827',
          50:  '#f9fafb', 100: '#f3f4f6', 200: '#e5e7eb',
          300: '#d1d5db', 400: '#9ca3af', 500: '#6b7280',
          600: '#4b5563', 700: '#374151', 800: '#1f2937',
          900: '#111827', 950: '#030712',
        },
        secondary: {
          DEFAULT: '#333333',
          light:   '#666666',
          dark:    '#1a1a1a',
        },
        // Background cinza claro
        background: {
          DEFAULT: '#F3F4F6',
          paper:   '#FFFFFF',
          dark:    '#E5E7EB',
        },
        // Accent — Cyan
        accent: {
          DEFAULT: '#0891B2',
          50: '#ecfeff', 100: '#cffafe', 200: '#a5f3fc',
          300: '#67e8f9', 400: '#22d3ee', 500: '#06b6d4',
          600: '#0891b2', 700: '#0e7490', 800: '#155e75', 900: '#164e63',
        },
        // Semânticas
        success: { DEFAULT: '#059669', /* ... escalas 50–900 */ },
        warning: { DEFAULT: '#D97706', /* ... escalas 50–900 */ },
        error:   { DEFAULT: '#DC2626', /* ... escalas 50–900 */ },
        info:    { DEFAULT: '#2563EB', /* ... escalas 50–900 */ },
        purple:  { DEFAULT: '#7C3AED', /* ... escalas 50–900 */ },
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', '-apple-system', 'sans-serif'],
        mono: ['JetBrains Mono', 'Fira Code', 'monospace'],
      },
      fontSize: {
        '2xs': ['0.625rem', { lineHeight: '0.75rem' }],
      },
      boxShadow: {
        'soft':       '0 2px 8px rgba(0,0,0,0.08)',
        'medium':     '0 4px 16px rgba(0,0,0,0.12)',
        'hard':       '0 8px 32px rgba(0,0,0,0.16)',
        'inner-soft': 'inset 0 2px 4px rgba(0,0,0,0.06)',
      },
      borderRadius: {
        'xl': '1rem', '2xl': '1.5rem', '3xl': '2rem',
      },
      animation: {
        'fade-in':   'fadeIn 0.3s ease-out',
        'slide-up':  'slideUp 0.3s ease-out',
        'slide-down':'slideDown 0.3s ease-out',
        'scale-in':  'scaleIn 0.2s ease-out',
        'spin-slow': 'spin 2s linear infinite',
      },
      keyframes: {
        fadeIn:    { '0%': { opacity: '0' }, '100%': { opacity: '1' } },
        slideUp:   { '0%': { opacity: '0', transform: 'translateY(10px)' }, '100%': { opacity: '1', transform: 'translateY(0)' } },
        slideDown: { '0%': { opacity: '0', transform: 'translateY(-10px)' }, '100%': { opacity: '1', transform: 'translateY(0)' } },
        scaleIn:   { '0%': { opacity: '0', transform: 'scale(0.95)' }, '100%': { opacity: '1', transform: 'scale(1)' } },
      },
      spacing: {
        '18': '4.5rem', '88': '22rem', '112': '28rem', '128': '32rem',
      },
      zIndex: {
        '60': '60', '70': '70', '80': '80', '90': '90', '100': '100',
      },
    },
  },
  plugins: [],
};
```

---

## 6. CSS Global (`src/styles/globals.css`)

Crie `src/styles/globals.css` com o conteúdo abaixo. Ele define:
- CSS vars raiz (sidebar/header dimensions, transições)
- Classes de componentes reutilizáveis via `@layer components`
- Utilitários extras via `@layer utilities`

```css
@tailwind base;
@tailwind components;
@tailwind utilities;

@layer base {
  :root {
    --color-primary:    #111827;
    --color-accent:     #0891B2;
    --color-success:    #059669;
    --color-warning:    #D97706;
    --color-error:      #DC2626;
    --sidebar-width:    280px;
    --header-height:    64px;
    --transition-fast:  150ms;
    --transition-normal:250ms;
    --transition-slow:  350ms;
  }

  * { @apply border-primary-200; }
  html { @apply scroll-smooth; }
  body { @apply bg-background text-primary font-sans antialiased; }
  *:focus-visible { @apply outline-none ring-2 ring-accent ring-offset-2; }
  ::selection { @apply bg-accent/20 text-primary; }

  /* Scrollbar personalizada */
  ::-webkit-scrollbar       { @apply w-2 h-2; }
  ::-webkit-scrollbar-track { @apply bg-primary-100 rounded-full; }
  ::-webkit-scrollbar-thumb { @apply bg-primary-300 rounded-full hover:bg-primary-400; }
}

@layer components {
  /* ---------- BUTTONS ---------- */
  .btn {
    @apply inline-flex items-center justify-center gap-2 px-4 py-2
           font-medium rounded-lg transition-all duration-200
           focus:outline-none focus:ring-2 focus:ring-offset-2
           disabled:opacity-50 disabled:cursor-not-allowed;
  }
  .btn-primary   { @apply btn bg-primary text-white hover:bg-primary-800 focus:ring-primary active:scale-[0.98]; }
  .btn-secondary { @apply btn bg-white text-primary border border-primary-300 hover:bg-primary-50 focus:ring-primary-300; }
  .btn-success   { @apply btn bg-gradient-to-r from-purple-600 to-purple-700 text-white hover:from-purple-700 hover:to-purple-800 focus:ring-purple-500; }
  .btn-danger    { @apply btn bg-error text-white hover:bg-error-700 focus:ring-error; }
  .btn-ghost     { @apply btn bg-transparent text-primary-600 hover:bg-primary-100; }
  .btn-sm        { @apply px-3 py-1.5 text-sm; }
  .btn-lg        { @apply px-6 py-3 text-lg; }
  .btn-icon      { @apply p-2.5 aspect-square inline-flex items-center justify-center min-w-[2.5rem] min-h-[2.5rem]; }

  /* ---------- CARDS ---------- */
  .card      { @apply bg-white rounded-xl shadow-soft p-6 transition-shadow hover:shadow-medium; }
  .card-flat { @apply bg-white rounded-xl border border-primary-200 p-6; }
  .stat-card { @apply card flex items-start gap-4; }
  .stat-card-icon  { @apply w-12 h-12 rounded-xl flex items-center justify-center text-white; }
  .stat-card-value { @apply text-2xl font-bold text-primary; }
  .stat-card-label { @apply text-sm text-primary-500; }
  .stat-card-badge { @apply inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium; }

  /* ---------- INPUTS ---------- */
  .input {
    @apply w-full px-4 py-2.5 bg-white border border-primary-300 rounded-lg
           text-primary placeholder:text-primary-400
           focus:border-accent focus:ring-2 focus:ring-accent/20
           disabled:bg-primary-50 disabled:cursor-not-allowed
           transition-colors duration-200;
  }
  .input-error         { @apply border-error focus:border-error focus:ring-error/20; }
  .input-label         { @apply block text-sm font-medium text-primary-700 mb-1.5; }
  .input-error-message { @apply text-sm text-error mt-1; }
  .input-hint          { @apply text-sm text-primary-500 mt-1; }

  /* ---------- BADGES ---------- */
  .badge          { @apply inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-medium; }
  .badge-active   { @apply bg-success-100 text-success-700; }
  .badge-pending  { @apply bg-warning-100 text-warning-700; }
  .badge-inactive { @apply bg-primary-100 text-primary-600; }
  .badge-vesting  { @apply bg-info-100 text-info-700; }
  .badge-investor { @apply bg-purple-100 text-purple-700; }

  /* ---------- TABLES ---------- */
  .table-container { @apply overflow-x-auto rounded-xl border border-primary-200; }
  .table           { @apply w-full text-left; }
  .table th        { @apply px-4 py-3 text-xs font-semibold text-primary-500 uppercase tracking-wider bg-primary-50 border-b border-primary-200; }
  .table td        { @apply px-4 py-4 text-sm text-primary-700 border-b border-primary-100; }
  .table tbody tr  { @apply hover:bg-primary-50/50 transition-colors; }

  /* ---------- SIDEBAR ---------- */
  .sidebar          { @apply fixed left-0 top-0 h-screen w-[280px] bg-primary text-white flex flex-col z-50; }
  .sidebar-logo     { @apply px-6 py-5 border-b border-white/10; }
  .sidebar-nav      { @apply flex-1 overflow-y-auto py-4 px-3; }
  .sidebar-item     { @apply flex items-center gap-3 px-4 py-2.5 rounded-lg text-white/70 hover:bg-white/10 hover:text-white transition-all duration-200; }
  .sidebar-item-active { @apply bg-white/10 text-white; }
  .sidebar-footer   { @apply px-4 py-4 border-t border-white/10; }

  /* ---------- LAYOUT ---------- */
  .main-content   { @apply ml-[280px] min-h-screen bg-background; }
  .page-header    { @apply flex items-center justify-between mb-6; }
  .page-title     { @apply text-2xl font-bold text-primary; }
  .page-subtitle  { @apply text-primary-500 mt-1; }

  /* ---------- MODALS ---------- */
  .modal-overlay  { @apply fixed inset-0 bg-black/50 backdrop-blur-sm z-50 flex items-center justify-center p-4; }
  .modal          { @apply bg-white rounded-2xl shadow-hard max-w-lg w-full max-h-[90vh] overflow-hidden animate-scale-in; }
  .modal-header   { @apply flex items-center justify-between px-6 py-4 border-b border-primary-200; }
  .modal-title    { @apply text-lg font-semibold text-primary; }
  .modal-body     { @apply px-6 py-4 overflow-y-auto; }
  .modal-footer   { @apply flex items-center justify-end gap-3 px-6 py-4 border-t border-primary-200 bg-primary-50; }

  /* ---------- PROGRESS / AVATARS / LOADING ---------- */
  .progress-bar      { @apply h-2 bg-primary-100 rounded-full overflow-hidden; }
  .progress-bar-fill { @apply h-full rounded-full transition-all duration-500; }
  .avatar    { @apply rounded-full bg-primary-200 flex items-center justify-center text-primary font-medium overflow-hidden; }
  .avatar-sm { @apply w-8 h-8 text-sm; }
  .avatar-md { @apply w-10 h-10 text-base; }
  .avatar-lg { @apply w-12 h-12 text-lg; }
  .avatar-xl { @apply w-16 h-16 text-xl; }
  .skeleton { @apply bg-primary-200 animate-pulse rounded; }
  .spinner  { @apply w-5 h-5 border-2 border-primary-200 border-t-primary rounded-full animate-spin; }

  /* ---------- UTILITÁRIOS ---------- */
  .text-gradient { @apply bg-gradient-to-r from-purple-600 to-info-600 bg-clip-text text-transparent; }
  .glass         { @apply bg-white/80 backdrop-blur-md; }
  .divider       { @apply border-t border-primary-200; }
  .truncate-2    { display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
}

@layer utilities {
  .scrollbar-hide { -ms-overflow-style: none; scrollbar-width: none; }
  .scrollbar-hide::-webkit-scrollbar { display: none; }
}
```

---

## 7. Estrutura de Pastas Recomendada

```
src/
├── components/
│   ├── layout/          # Sidebar, Header, MainLayout, AuthLayout
│   ├── ui/              # Alert, ConfirmDialog, componentes genéricos
│   └── <domínio>/       # Componentes específicos por feature
├── pages/               # Um arquivo/pasta por rota
├── stores/              # Zustand: authStore.ts, <domínio>Store.ts
├── services/            # api.ts + <domínio>Service.ts (Axios)
├── hooks/               # use<Domínio>.ts (React Query hooks)
├── types/               # <domínio>.types.ts + index.ts
├── constants/           # Mensagens e constantes globais
├── utils/               # cn.ts, formatters, helpers
├── i18n/
│   ├── index.ts
│   └── locales/         # pt.json, en.json, es.json
├── styles/
│   └── globals.css
├── test/
│   └── setup.ts
├── App.tsx
└── main.tsx
```

---

## 8. Utilitário `cn` (merge de classes Tailwind)

Criar em `src/utils/cn.ts`:

```ts
import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}
```

Usar em componentes:
```tsx
<div className={cn('btn btn-primary', isLoading && 'opacity-50', className)} />
```

---

## 9. Entrada da Aplicação (`src/main.tsx`)

```tsx
import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'react-hot-toast';
import './i18n';
import App from './App';
import './styles/globals.css';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5, // 5 minutos
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <App />
        <Toaster
          position="top-right"
          toastOptions={{
            duration: 4000,
            style: { background: '#111827', color: '#fff', borderRadius: '12px' },
            success: { iconTheme: { primary: '#059669', secondary: '#fff' } },
            error:   { iconTheme: { primary: '#DC2626', secondary: '#fff' } },
          }}
        />
      </BrowserRouter>
    </QueryClientProvider>
  </React.StrictMode>
);
```

---

## 10. Padrão de Layout (`MainLayout`)

O layout padrão é: **sidebar fixa 280px à esquerda** + **conteúdo principal com padding**.

```
┌──────────────────┬───────────────────────────────────────┐
│  Sidebar 280px   │  Header (usuário + empresa switcher)   │
│  bg-primary      │──────────────────────────────────────  │
│  (dark #111827)  │                                        │
│                  │  <Outlet /> — conteúdo da rota         │
│                  │  padding: p-6, espaçamento: space-y-6  │
└──────────────────┴───────────────────────────────────────┘
```

```tsx
// MainLayout.tsx
export default function MainLayout() {
  return (
    <div className="min-h-screen bg-background">
      <Sidebar />
      <main className="main-content">
        <div className="p-6 space-y-6">
          <Header />
          <Outlet />
        </div>
      </main>
    </div>
  );
}
```

---

## 11. Cliente HTTP — Axios com Interceptors (`src/services/api.ts`)

```ts
import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import { useAuthStore } from '@/stores/authStore';

const API_URL = import.meta.env.VITE_API_URL || '/api';

export const api = axios.create({
  baseURL: API_URL,
  headers: { 'Content-Type': 'application/json' },
});

// Injeta Bearer token em todas as requests
api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = useAuthStore.getState().accessToken;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Refresh automático em 401
api.interceptors.response.use(
  (res) => res,
  async (error: AxiosError) => {
    const original = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
    if (error.response?.status === 401 && !original._retry) {
      original._retry = true;
      // lógica de refresh token → redireciona para /login se falhar
    }
    return Promise.reject(error);
  }
);
```

---

## 12. Estado Global — Zustand Auth Store

```ts
// src/stores/authStore.ts
import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface User {
  id: string;
  email: string;
  name: string;
  companyId: string;
  companyName: string;
  roles: string[];
  language: string;
}

interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  setAuth: (user: User, accessToken: string, refreshToken: string) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
      setAuth: (user, accessToken, refreshToken) =>
        set({ user, accessToken, refreshToken, isAuthenticated: true }),
      logout: () =>
        set({ user: null, accessToken: null, refreshToken: null, isAuthenticated: false }),
    }),
    {
      name: 'auth-storage',
      // Usar sessionStorage para segurança
      storage: {
        getItem: (key) => { const v = sessionStorage.getItem(key); return v ? JSON.parse(v) : null; },
        setItem: (key, v) => sessionStorage.setItem(key, JSON.stringify(v)),
        removeItem: (key) => sessionStorage.removeItem(key),
      },
      partialize: (s) => ({ user: s.user, accessToken: s.accessToken, refreshToken: s.refreshToken, isAuthenticated: s.isAuthenticated }),
    }
  )
);
```

> **Segurança:** tokens armazenados em `sessionStorage` (limpa ao fechar aba), não `localStorage`.

---

## 13. Padrão de Hooks com React Query

Um hook por domínio em `src/hooks/use<Domínio>.ts`:

```ts
// src/hooks/useUsers.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { userService } from '@/services/userService';

export function useUsers() {
  return useQuery({
    queryKey: ['users'],
    queryFn: userService.getAll,
    staleTime: 1000 * 60 * 5,
  });
}

export function useCreateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: userService.create,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['users'] }),
  });
}
```

---

## 14. Padrão de Formulário com React Hook Form + Zod

```tsx
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';

const schema = z.object({
  name:  z.string().min(1, 'Obrigatório'),
  email: z.string().email('E-mail inválido'),
});

type FormData = z.infer<typeof schema>;

export function UserForm({ onSubmit }: { onSubmit: (data: FormData) => void }) {
  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div>
        <label className="input-label">Nome</label>
        <input {...register('name')} className={cn('input', errors.name && 'input-error')} />
        {errors.name && <p className="input-error-message">{errors.name.message}</p>}
      </div>
      <button type="submit" className="btn btn-primary">Salvar</button>
    </form>
  );
}
```

---

## 15. Internacionalização (i18n)

```ts
// src/i18n/index.ts
import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';
import pt from './locales/pt.json';
import en from './locales/en.json';

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: { pt: { translation: pt }, en: { translation: en } },
    fallbackLng: 'pt',
    detection: { order: ['localStorage', 'navigator'], lookupLocalStorage: 'app-language', caches: ['localStorage'] },
    interpolation: { escapeValue: false },
  });

export default i18n;
```

Uso nos componentes:
```tsx
const { t } = useTranslation();
<h1>{t('nav.dashboard')}</h1>
```

---

## 16. Padrão de Página

```tsx
// pages/Users.tsx
import { useUsers } from '@/hooks/useUsers';
import { Users as UsersIcon } from 'lucide-react';

export default function UsersPage() {
  const { data: users, isLoading } = useUsers();

  return (
    <div className="space-y-6">
      {/* Header da página */}
      <div className="page-header">
        <div>
          <h1 className="page-title">Usuários</h1>
          <p className="page-subtitle">Gerencie os usuários do sistema</p>
        </div>
        <button className="btn btn-primary">
          <UsersIcon className="w-4 h-4" />
          Novo Usuário
        </button>
      </div>

      {/* Cards de stats */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="stat-card">
          <div className="stat-card-icon bg-accent">
            <UsersIcon className="w-5 h-5" />
          </div>
          <div>
            <p className="stat-card-value">{users?.length ?? 0}</p>
            <p className="stat-card-label">Total de usuários</p>
          </div>
        </div>
      </div>

      {/* Tabela */}
      {isLoading ? (
        <div className="card"><div className="spinner mx-auto" /></div>
      ) : (
        <div className="card">
          <div className="table-container">
            <table className="table">
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>E-mail</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {users?.map((u) => (
                  <tr key={u.id}>
                    <td>{u.name}</td>
                    <td>{u.email}</td>
                    <td><span className="badge badge-active">Ativo</span></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
```

---

## 17. Roteamento Protegido (`App.tsx`)

```tsx
import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuthStore } from './stores/authStore';
import MainLayout from './components/layout/MainLayout';
import AuthLayout from './components/layout/AuthLayout';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';

function App() {
  const { isAuthenticated } = useAuthStore();

  return (
    <Routes>
      {/* Rotas públicas */}
      <Route element={<AuthLayout />}>
        <Route path="/login" element={<Login />} />
      </Route>

      {/* Rotas protegidas */}
      <Route element={isAuthenticated ? <MainLayout /> : <Navigate to="/login" replace />}>
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="/dashboard" element={<Dashboard />} />
        {/* adicionar rotas aqui */}
      </Route>
    </Routes>
  );
}
```

---

## 18. Convenções de Código

| Convenção | Regra |
|---|---|
| Componentes | PascalCase, um por arquivo |
| Hooks | Prefixo `use`, um por domínio |
| Serviços | `<dominio>Service.ts`, só chamadas Axios |
| Tipos | `<dominio>.types.ts`, sem `any` |
| Imports | Sempre via alias `@/` |
| Classes CSS | Classes do design system antes de overrides inline |
| Formulários | Sempre React Hook Form + Zod, nunca `onChange` manual |
| Estado servidor | Sempre React Query, nunca `useState` para fetch |
| Estado global | Zustand apenas para auth e seleções globais |
| Estilo de string | Aspas simples em TS, aspas duplas apenas em JSX attrs |

---

## 19. Scripts do Projeto

```json
{
  "dev":           "vite",
  "build":         "tsc && vite build",
  "preview":       "vite preview",
  "test":          "vitest run",
  "test:watch":    "vitest",
  "test:coverage": "vitest run --coverage",
  "lint":          "eslint . --ext ts,tsx --report-unused-disable-directives --max-warnings 0",
  "format":        "prettier --write \"src/**/*.{ts,tsx,css}\""
}
```

> **Zero warnings**: o lint está configurado com `--max-warnings 0`. Todo warning vira erro de CI.

---

## 20. Checklist de Setup

- [ ] `npm create vite@latest` com template `react-ts`
- [ ] Instalar todas as dependências listadas na seção 2
- [ ] Copiar `tailwind.config.js` completo (seção 5)
- [ ] Copiar `globals.css` completo (seção 6)
- [ ] Criar estrutura de pastas (seção 7)
- [ ] Criar `src/utils/cn.ts` (seção 8)
- [ ] Configurar `vite.config.ts` com alias `@/` e proxy `/api` (seção 3)
- [ ] Configurar `tsconfig.json` com paths `@/*` (seção 4)
- [ ] Criar `src/main.tsx` com QueryClient e Toaster (seção 9)
- [ ] Criar `src/stores/authStore.ts` com sessionStorage (seção 12)
- [ ] Criar `src/services/api.ts` com interceptors (seção 11)
- [ ] Configurar i18n com LanguageDetector (seção 15)
- [ ] Criar `src/test/setup.ts` com `@testing-library/jest-dom` imports
