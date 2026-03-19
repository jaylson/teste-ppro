"""
Captura screenshots de todas as telas do Partnership Manager.
Executa: python3 scripts/capture_screenshots.py
"""

from playwright.sync_api import sync_playwright
import time, os

BASE_URL  = "http://localhost:3000"
OUT_DIR   = "/workspaces/teste-ppro/prints"
EMAIL     = "carlos.silva@inovatech.com.br"
PASSWORD  = "Demo@2024!"

SCREENS = [
    # (filename_sem_ext, route, group_para_abrir_se_necessário)
    ("01_login",                      "/login",                          None),
    ("02_dashboard",                  "/dashboard",                      None),
    ("03_valuation_dashboard",        "/valuations/dashboard",           None),
    ("04_financial_dashboard",        "/financial/dashboard",            None),
    ("05_meu_vesting",                "/my-vesting",                     None),
    ("06_portal_investidor",          "/investor",                       None),
    ("07_empresas",                   "/companies",                      None),
    ("08_socios",                     "/shareholders",                   None),
    ("09_cap_table",                  "/cap-table",                      None),
    ("10_cap_table_transacoes",       "/cap-table/transactions",         None),
    ("11_vesting_planos",             "/vesting",                        None),
    ("12_vesting_grants",             "/vesting/grants",                 None),
    ("13_valuation_lista",            "/valuations",                     None),
    ("14_financeiro",                 "/financial",                      None),
    ("15_documentos",                 "/documents",                      None),
    ("16_data_room",                  "/dataroom",                       None),
    ("17_contratos",                  "/contracts",                      None),
    ("18_contratos_templates",        "/contracts/templates",            None),
    ("19_comunicacoes",               "/communications",                 None),
    ("20_notificacoes",               "/notifications",                  None),
    ("21_aprovacoes_fluxos",          "/approvals/flows",                None),
    ("22_aprovacoes_aprovadores",     "/approvals/approvers",            None),
    ("23_aprovacoes",                 "/approvals",                      None),
    ("24_admin_usuarios",             "/settings/users",                 None),
    ("25_admin_perfis",               "/settings/roles",                 None),
    ("26_acessorios_formulas",        "/valuations/custom-formulas",     None),
    ("27_acessorios_clausulas",       "/contracts/clauses",              None),
    ("28_acessorios_milestones",      "/vesting/milestone-templates",    None),
]

os.makedirs(OUT_DIR, exist_ok=True)

def run():
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        context = browser.new_context(
            viewport={"width": 1440, "height": 900},
            locale="pt-BR",
        )
        page = context.new_page()

        # ── Login ──────────────────────────────────────────────────────────────
        print("→ Fazendo login...")
        page.goto(f"{BASE_URL}/login", wait_until="domcontentloaded", timeout=30000)
        page.wait_for_selector('input[type="email"]', timeout=15000)
        page.fill('input[type="email"]', EMAIL)
        page.fill('input[type="password"]', PASSWORD)
        page.screenshot(path=f"{OUT_DIR}/01_login.png")
        print("  Screenshot: 01_login.png")
        page.click('button[type="submit"]')
        # Aguarda redirecionar para qualquer rota autenticada
        page.wait_for_selector('[class*="sidebar"], aside, nav', timeout=25000)
        print("  Login OK")

        # ── Telas autenticadas ─────────────────────────────────────────────────
        for filename, route, _ in SCREENS:
            if filename == "01_login":
                continue  # já capturado acima
            url = f"{BASE_URL}{route}"
            print(f"→ {filename}  ({route})")
            try:
                page.goto(url, wait_until="domcontentloaded", timeout=25000)
                # Aguarda possível animação/spinner
                time.sleep(2)
                # Fecha modais/popovers abertos acidentalmente
                page.keyboard.press("Escape")
                time.sleep(0.3)
                page.screenshot(
                    path=f"{OUT_DIR}/{filename}.png",
                    full_page=False,
                    clip={"x": 0, "y": 0, "width": 1440, "height": 900},
                )
                print(f"  Salvo: {filename}.png")
            except Exception as e:
                print(f"  ERRO em {route}: {e}")
                # Mesmo com erro tenta salvar o que renderizou
                try:
                    page.screenshot(path=f"{OUT_DIR}/{filename}_error.png")
                except Exception:
                    pass

        browser.close()
        print("\n✓ Capturas concluídas em:", OUT_DIR)

if __name__ == "__main__":
    run()
