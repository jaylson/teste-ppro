# Credenciais de Acesso — Ambiente Demo

> **Senha padrão de todos os usuários demo:** `Demo@2024!`

---

## SuperAdmin

| Campo  | Valor                  |
|--------|------------------------|
| E-mail | `admin@sistema.com`    |
| Senha  | `SysAdmin@2024!`       |
| Role   | SuperAdmin             |
| Acesso | Irrestrito — todos os tenants |

> ⚠️ Alterar esta senha imediatamente em ambientes não-locais via `POST /api/auth/change-password`.

testes para publicação.

---

## InovaTech

> Startup de tecnologia B2B SaaS — Series A concluído (R$2,25M)

| Nome             | E-mail                               | Role              |
|------------------|--------------------------------------|-------------------|
| Carlos Silva     | `carlos.silva@inovatech.com.br`      | Admin + Founder   |
| Ana Santos       | `ana.santos@inovatech.com.br`        | Founder           |
| Roberto Lima     | `roberto.lima@inovatech.com.br`      | Employee          |
| Thiago Barbosa   | `thiago.dev@inovatech.com.br`        | Employee          |
| Beatriz Rocha    | `beatriz.design@inovatech.com.br`    | Employee          |
| Gabriel Sousa    | `gabriel.hr@inovatech.com.br`        | HR                |

---

## VidaSaúde

> Healthtech — Seed Round fechado (R$2M com Marina Torres Health Fund)

| Nome              | E-mail                                    | Role            |
|-------------------|-------------------------------------------|-----------------|
| João Ferreira     | `joao.ferreira@vidasaude.com.br`          | Admin + Founder |
| Mariana Oliveira  | `mariana.oliveira@vidasaude.com.br`       | Founder         |

---

## FinanGrow

> Fintech de gestão financeira — Pre-Seed convertido (R$450k)

| Nome          | E-mail                            | Role            |
|---------------|-----------------------------------|-----------------|
| Pedro Alves   | `pedro.alves@finangrow.com.br`    | Admin + Founder |
| Lúcia Mendes  | `lucia.mendes@finangrow.com.br`   | Founder         |

---

## Ecossistema Ventures — Acesso Cross-empresa

> Estes usuários têm acesso a múltiplas empresas do portfólio.

| Nome                   | E-mail                                      | Role        | Empresas                              |
|------------------------|---------------------------------------------|-------------|---------------------------------------|
| Dr. Marcos Rodrigues   | `marcos@boardmember.ecossistema.com.br`     | BoardMember | InovaTech, VidaSaúde, FinanGrow       |
| Dra. Cláudia Pereira   | `claudia@boardmember.ecossistema.com.br`    | BoardMember | InovaTech                             |
| Rafael Cunha           | `rafael@legal.ecossistema.com.br`           | Legal       | InovaTech, VidaSaúde, FinanGrow       |
| Fernanda Lima          | `fernanda@finance.ecossistema.com.br`       | Finance     | InovaTech, VidaSaúde, FinanGrow       |
| João Paulo Anjos       | `jp.angel@venture.com.br`                   | Investor    | InovaTech, FinanGrow                  |
| Marina Torres          | `marina@healthfund.com.br`                  | Investor    | VidaSaúde                             |

---

## URLs de Acesso

| Serviço  | URL                              |
|----------|----------------------------------|
| Frontend | http://localhost                 |
| API      | http://localhost:5000            |
| Swagger  | http://localhost:5000/swagger    |

---

## Dados do Banco de Dados (Azure MySQL)

| Campo    | Valor                                        |
|----------|----------------------------------------------|
| Host     | `newdataserver.mysql.database.azure.com`     |
| Porta    | `3306`                                       |
| Schema   | `ppro`                                       |
| Usuário  | `dataserver_wp`                              |
| SSL      | Obrigatório (`--ssl-mode=REQUIRED`)          |
