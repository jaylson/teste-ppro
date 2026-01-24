import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { Client } from '@/types';

interface ClientState {
  currentClient: Client | null;
  selectedCompanyId: string | null;
  isLoading: boolean;

  // Actions
  setCurrentClient: (client: Client | null) => void;
  setSelectedCompanyId: (companyId: string | null) => void;
  setLoading: (loading: boolean) => void;
  clearClient: () => void;
}

export const useClientStore = create<ClientState>()(
  persist(
    (set) => ({
      currentClient: null,
      selectedCompanyId: null,
      isLoading: false,

      setCurrentClient: (client) =>
        set({
          currentClient: client,
          isLoading: false,
        }),

      setSelectedCompanyId: (companyId) =>
        set({
          selectedCompanyId: companyId,
        }),

      setLoading: (loading) => set({ isLoading: loading }),

      clearClient: () =>
        set({
          currentClient: null,
          selectedCompanyId: null,
          isLoading: false,
        }),
    }),
    {
      name: 'pm-client-storage',
      partialize: (state) => ({
        currentClient: state.currentClient,
        selectedCompanyId: state.selectedCompanyId,
      }),
    }
  )
);
