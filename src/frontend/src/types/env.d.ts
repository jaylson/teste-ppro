/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_URL?: string;
  readonly VITE_DEFAULT_COMPANY_ID?: string;
  readonly VITE_CLICKSIGN_SANDBOX?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
