#!/usr/bin/env python3
"""
Teste de envio de e-mail via Google Workspace SMTP Relay.
Lê as configurações de appsettings.json e envia um e-mail de teste.

Uso:
  python3 test_email.py
  python3 test_email.py destinatario@email.com
"""

import sys
import json
import base64
import smtplib
import ssl
from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText
from datetime import datetime
from pathlib import Path

# ── Carrega appsettings.json ───────────────────────────────────────────────
APPSETTINGS = Path(__file__).parent / "src/backend/PartnershipManager.API/appsettings.json"

with open(APPSETTINGS, encoding="utf-8") as f:
    cfg = json.load(f)["Email"]

smtp_host  = cfg.get("SmtpHost", "smtp-relay.gmail.com")
smtp_port  = int(cfg.get("SmtpPort", 587))
enable_ssl = cfg.get("EnableSsl", "true").lower() == "true"
username   = cfg.get("Username", "")
password   = cfg.get("Password", "")
from_email = cfg.get("FromEmail", "")
from_name  = cfg.get("FromName", "Partnership Manager")

# Decodifica senha Base64 automaticamente (mesmo comportamento do C# service)
def decode_if_base64(value: str) -> str:
    try:
        decoded = base64.b64decode(value).decode("utf-8")
        if all(32 <= ord(c) < 127 for c in decoded):
            return decoded
    except Exception:
        pass
    return value

password = decode_if_base64(password)

# Google App Passwords funcionam com ou sem espaços, mas por segurança removemos
password_clean = password.replace(" ", "")

# ── Destinatário ───────────────────────────────────────────────────────────
to_email = sys.argv[1] if len(sys.argv) > 1 else from_email
to_name  = to_email

# ── Monta a mensagem ───────────────────────────────────────────────────────
now_str = datetime.now().strftime("%d/%m/%Y %H:%M:%S")

msg = MIMEMultipart("alternative")
msg["Subject"] = f"[TESTE] Partnership Manager — Verificação SMTP {now_str}"
msg["From"]    = f"{from_name} <{from_email}>"
msg["To"]      = to_email

html = f"""
<html>
<body style="font-family:Arial,sans-serif;background:#f3f4f6;padding:40px 0;margin:0">
  <table width="600" cellpadding="0" cellspacing="0" style="background:#fff;border-radius:8px;overflow:hidden;margin:0 auto;box-shadow:0 2px 8px rgba(0,0,0,.08)">
    <tr><td style="background:#111827;padding:28px 40px;text-align:center">
      <span style="color:#fff;font-size:20px;font-weight:bold">Partnership Manager</span>
    </td></tr>
    <tr><td style="padding:36px 40px">
      <p style="color:#0891B2;font-size:14px;font-weight:bold;margin:0 0 4px">E-MAIL DE TESTE</p>
      <p style="color:#111827;font-size:20px;font-weight:bold;margin:0 0 20px">Conexão SMTP verificada com sucesso ✓</p>
      <table style="width:100%;border-collapse:collapse;font-size:14px">
        <tr><td style="padding:8px 12px;background:#f9fafb;color:#6b7280;width:140px">Servidor SMTP</td>
            <td style="padding:8px 12px">{smtp_host}:{smtp_port}</td></tr>
        <tr><td style="padding:8px 12px;color:#6b7280">Usuário</td>
            <td style="padding:8px 12px">{username}</td></tr>
        <tr><td style="padding:8px 12px;background:#f9fafb;color:#6b7280">Remetente</td>
            <td style="padding:8px 12px">{from_name} &lt;{from_email}&gt;</td></tr>
        <tr><td style="padding:8px 12px;color:#6b7280">Destinatário</td>
            <td style="padding:8px 12px">{to_email}</td></tr>
        <tr><td style="padding:8px 12px;background:#f9fafb;color:#6b7280">Data/hora</td>
            <td style="padding:8px 12px">{now_str}</td></tr>
      </table>
      <p style="color:#6b7280;font-size:13px;margin:24px 0 0">
        Este é um e-mail de teste automático. Nenhuma ação é necessária.
      </p>
    </td></tr>
    <tr><td style="background:#f9fafb;padding:16px 40px;text-align:center">
      <p style="color:#9ca3af;font-size:12px;margin:0">© {datetime.now().year} Partnership Manager</p>
    </td></tr>
  </table>
</body>
</html>
"""

text = (
    f"TESTE DE E-MAIL - Partnership Manager\n"
    f"{'=' * 40}\n"
    f"Servidor : {smtp_host}:{smtp_port}\n"
    f"Usuário  : {username}\n"
    f"De       : {from_name} <{from_email}>\n"
    f"Para     : {to_email}\n"
    f"Data/hora: {now_str}\n\n"
    f"Conexão SMTP verificada com sucesso.\n"
)

msg.attach(MIMEText(text, "plain", "utf-8"))
msg.attach(MIMEText(html, "html", "utf-8"))

# ── Envia ──────────────────────────────────────────────────────────────────
print(f"\nConfiguração:")
print(f"  SMTP Host : {smtp_host}:{smtp_port}")
print(f"  STARTTLS  : {enable_ssl}")
print(f"  Usuário   : {username}")
print(f"  Senha     : {'(base64 → decodificada)' if password != cfg.get('Password','') else '(texto puro)'}")
print(f"  De        : {from_name} <{from_email}>")
print(f"  Para      : {to_email}")
print(f"\nConectando a {smtp_host}:{smtp_port}...")

try:
    context = ssl.create_default_context()
    with smtplib.SMTP(smtp_host, smtp_port, timeout=15) as server:
        server.set_debuglevel(0)
        if enable_ssl:
            print("  STARTTLS... ", end="", flush=True)
            server.starttls(context=context)
            print("OK")
        if username and password_clean:
            print(f"  Autenticando como {username}... ", end="", flush=True)
            server.login(username, password_clean)
            print("OK")
        print("  Enviando mensagem... ", end="", flush=True)
        server.sendmail(from_email, to_email, msg.as_string())
        print("OK")

    print(f"\n✓ E-mail enviado com sucesso para {to_email}!\n")

except smtplib.SMTPAuthenticationError as e:
    print(f"FALHOU\n\n✗ Erro de autenticação: {e}\n"
          f"\nDica: Verifique se a senha do App Google está correta.\n"
          f"      A senha decodificada usada foi: '{password}'\n")
    sys.exit(1)
except smtplib.SMTPException as e:
    print(f"FALHOU\n\n✗ Erro SMTP: {e}\n")
    sys.exit(1)
except Exception as e:
    print(f"FALHOU\n\n✗ Erro: {e}\n")
    sys.exit(1)
