# Glow Up — Azure Deployment Guide (Free-Tier Portfolio Demo)

This guide deploys Glow Up as **one public URL** that recruiters can open and try, using
**free Azure tiers by default** to avoid surprise charges. The .NET API serves the built
React app, talks to Azure SQL, and uses Gemini (with a mock fallback). All secrets live in
Azure App Service configuration — never in code.

> ⚠️ **Cost warning — read first.** This guide is **free-first**, but Azure can still bill
> you if a resource is created on a paid tier by mistake. Two rules:
> 1. **If any `az` command or the Azure Portal shows a price or refuses the free option,
>    STOP** and do not continue until you understand the cost.
> 2. When you no longer need the demo, **delete the whole resource group** (see
>    [Cost control & cleanup](#cost-control--cleanup)). This is the only way to be sure
>    nothing keeps billing.

```
                ┌──────────────────────────────────────────────┐
   Browser ───▶ │  Azure App Service  (Free F1, one URL)       │
                │   GlowUp.Api (.NET 10)                        │
                │   ├─ /            → React app (wwwroot)       │
                │   ├─ /api/...     → REST API                 │
                │   └─ Gemini  ──▶  Google Gemini (server-side) │
                └───────────────┬──────────────────────────────┘
                                │
                       ┌────────▼──────────────────┐
                       │  Azure SQL DB             │
                       │  (free serverless offer)  │
                       └───────────────────────────┘
```

## How single-URL hosting works

- `dotnet publish` runs a publish-only MSBuild target (`PublishReactClient` in
  `GlowUp.Api.csproj`) that runs `npm install && npm run build` in `glowup.client`
  and copies the output into the publish `wwwroot/`. Local `dotnet build` / `dotnet run`
  never trigger npm.
- In production the API serves `wwwroot` via `UseDefaultFiles` + `UseStaticFiles`, and
  `MapFallbackToFile("index.html")` makes client-side routes survive a hard refresh.
- The frontend calls the API at a **relative** base URL in production (same origin), so
  there is no CORS and no API URL to configure. See `glowup.client/src/services/config.ts`.

---

## What "free" means here (and its limits)

This is a **demo/testing setup, not production.** Expect rough edges:

**App Service Free `F1`:**
- Shared infrastructure, ~1 GB RAM, ~1 GB storage, a 60 CPU-minutes/day quota.
- **No "Always On"** → the app **sleeps after ~20 minutes idle**. The first visit after
  that is **slow (cold start)** while it wakes up.
- Limited CPU/storage; not suitable for real traffic or production workloads.

**Azure SQL Database free offer (serverless, General Purpose):**
- A monthly free allowance of compute (vCore-seconds) plus a storage cap.
- Configured below to **auto-pause when the free allowance is used up**, so it stops
  instead of billing. When paused, the **first request that wakes the database is slow**.
- Typically **one free database per subscription** — if you already use it elsewhere, the
  free offer may not apply.

For a CV demo this is fine: it's free, public, and works — just slow on the first hit.

---

## Prerequisites

- Azure subscription + [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)
  (`az login`)
- .NET 10 SDK and Node.js + npm on the machine doing the publish
- A Google Gemini API key (optional — without it the app runs on the mock provider)

Pick names (must be globally unique where noted):

```bash
RG=glowup-rg
LOCATION=westeurope
PLAN=glowup-plan
APP=glowup-demo            # -> https://glowup-demo.azurewebsites.net  (must be unique)
SQLSERVER=glowup-sql-$RANDOM   # must be unique
SQLDB=GlowUpDb
SQLADMIN=glowupadmin
SQLPASSWORD='REPLACE_WITH_A_STRONG_PASSWORD'   # do not commit this
```

---

## 1. Create Azure resources (free-first)

```bash
az group create --name $RG --location $LOCATION

# App Service plan on the FREE F1 tier (Linux, .NET 10)
az appservice plan create --name $PLAN --resource-group $RG --sku F1 --is-linux
az webapp create --name $APP --resource-group $RG --plan $PLAN --runtime "DOTNETCORE:10.0"

# Azure SQL logical server
az sql server create --name $SQLSERVER --resource-group $RG --location $LOCATION \
  --admin-user $SQLADMIN --admin-password "$SQLPASSWORD"

# Azure SQL Database on the FREE serverless offer.
# --use-free-limit enables the free monthly allowance.
# --free-limit-exhaustion-behavior AutoPause pauses the DB when the allowance is used
#   up, so it stops instead of charging you.
az sql db create --name $SQLDB --resource-group $RG --server $SQLSERVER \
  --edition GeneralPurpose --compute-model Serverless --family Gen5 --capacity 2 \
  --use-free-limit --free-limit-exhaustion-behavior AutoPause

# Allow Azure services (App Service) to reach the SQL server
az sql server firewall-rule create --resource-group $RG --server $SQLSERVER \
  --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
```

> 🛑 **Confirm the SQL free offer carefully.** The free database depends on the
> `--use-free-limit` flag and the serverless General Purpose model above. If the command
> **errors, shows a price, or your subscription already has a free database**, do **not**
> fall back to a paid tier blindly — STOP and read the message. Verify in the Azure Portal
> (SQL Database → Compute + storage) that it shows the **free** offer before continuing.
> Only one free SQL database is allowed per subscription.

> 💡 **Do not use `B1` (App Service) or `Basic` (SQL) for the default demo** — those are
> paid. Paid options are listed at the end only as a fallback if the free tiers don't work
> for you.

---

## 2. Configure App Service settings (secrets live here, not in code)

### Connection string

```bash
az webapp config connection-string set --resource-group $RG --name $APP \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:$SQLSERVER.database.windows.net,1433;Initial Catalog=$SQLDB;User ID=$SQLADMIN;Password=$SQLPASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;"
```

App Service exposes this to the app as `ConnectionStrings:DefaultConnection`, which is
exactly what `Program.cs` reads. (Timeout is 60s so a paused free DB has time to wake.)

### Application settings

```bash
az webapp config appsettings set --resource-group $RG --name $APP --settings \
  ASPNETCORE_ENVIRONMENT=Production \
  AI__Provider=Gemini \
  AI__Gemini__ApiKey="YOUR_GEMINI_API_KEY_HERE" \
  AI__Gemini__Model="gemini-2.5-flash" \
  Database__MigrateOnStartup=true \
  Demo__Seed=true
```

> **Note the double underscores** (`__`). Azure App Service maps `AI__Provider` to the
> .NET config key `AI:Provider`, etc. Colons are not allowed in env var names.

| Setting | Value | Purpose |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Enables prod pipeline (static files, forwarded headers, generic error handler) |
| `ConnectionStrings:DefaultConnection` | *(connection string above)* | Azure SQL |
| `AI:Provider` | `Gemini` | AI provider for the demo (mock fallback built in) |
| `AI:Gemini:ApiKey` | *your key* | Gemini key. **Omit to auto-fall back to mock** |
| `AI:Gemini:Model` | `gemini-2.5-flash` | Gemini model id |
| `Database:MigrateOnStartup` | `true` | Apply EF migrations on first boot (creates tables) |
| `Demo:Seed` | `true` | Seed portfolio sample data, so the public demo never shows your private local data |

**Gemini on the demo:** `AI__Provider=Gemini` is the default. Gemini answers depend on
**Google's Gemini free-tier limits**. If the key is missing, invalid, or the free quota is
reached, the backend **automatically falls back to the Mock provider** — the app keeps
working and the UI looks identical. To run fully offline of Gemini, set `AI__Provider=Mock`.

---

## 3. Build, publish, and deploy

From the repo root:

```bash
# Produces a self-contained publish folder including the built React app in wwwroot
dotnet publish GlowUp.Api/GlowUp.Api.csproj -c Release -o ./publish

# Zip + deploy to App Service
cd publish && zip -r ../app.zip . && cd ..
az webapp deploy --resource-group $RG --name $APP --src-path app.zip --type zip
```

Then open: **https://$APP.azurewebsites.net**

On first request the app applies migrations and seeds the demo data, so the live site
shows the "Alex Demo" profile with sample sections. On `F1` + paused free SQL, that very
first request can take **30–60+ seconds** while the app and database wake up — this is
normal for free tiers.

### Manual alternative (no MSBuild target)

If you prefer to build the client yourself:

```bash
cd glowup.client && npm install && npm run build && cd ..
mkdir -p GlowUp.Api/wwwroot && cp -r glowup.client/dist/* GlowUp.Api/wwwroot/
dotnet publish GlowUp.Api/GlowUp.Api.csproj -c Release -o ./publish
```

---

## 4. Verify

- `https://<app>.azurewebsites.net/` — the app loads with demo data (allow for a slow
  first load on free tiers).
- `https://<app>.azurewebsites.net/api/profile` — returns the demo profile JSON.
- AI Chat page — ask a question; answers come from Gemini (or mock if the key is missing or
  the Gemini free limit is hit). The UI is identical either way, and raw errors/secrets are
  never returned.

> Swagger is only enabled in Development, so it is not exposed on the production URL.

---

## Cost control & cleanup

Even on free tiers, set up guardrails and clean up when you're done.

### Create a budget / cost alert (do this right after deploying)

Azure Portal → **Cost Management + Billing** → **Cost Management** → **Budgets** →
**+ Add**:
- Scope: your subscription (or the `glowup-rg` resource group).
- Amount: a small number, e.g. **€1–€5**.
- Add an **alert** at 80% and 100% of the budget, sent to your email.

So if anything unexpectedly starts billing, you get an email instead of a surprise invoice.
(CLI equivalent: `az consumption budget create-with-rg` — the Portal is simpler here.)

### Check spending after deployment

Azure Portal → **Cost Management + Billing** → **Cost analysis**. Filter to the
`glowup-rg` resource group and confirm the running cost is **€0 / near zero**. Check again
a day or two after deploying, since some charges appear with a delay.

### Delete everything when the demo is no longer needed

After your internship/application period, remove the whole resource group:

```bash
az group delete --name $RG --yes --no-wait
```

This deletes the App Service, SQL server/database, and all related resources in one go.

> ⚠️ **Stopping a resource is not the same as deleting it.** A *stopped* App Service or a
> *paused* SQL database can still incur some charges (e.g. storage). **Deleting the whole
> resource group is the safest way to guarantee billing stops.**

### Secret-safety warnings

- 🔒 **Never commit SQL passwords or the Gemini API key.** They belong only in Azure App
  Service configuration (and in `.NET user-secrets` locally). `appsettings.json` contains
  only non-secret defaults.
- 🔒 **The Gemini API key must stay server-side only**, in App Service configuration. It is
  used by the backend and is **never sent to the browser/React frontend**. Do not put it in
  any client code, `VITE_*` variable, or repo file.

---

## Use it on your CV / portfolio

- ❌ **`http://localhost:5288` cannot be used on a CV.** Localhost only works on your own
  machine — recruiters can't open it.
- ✅ Use the **public URL**: `https://<app>.azurewebsites.net`.
- ✅ Add **both links**: a **Live Demo** (the Azure URL) and the **GitHub repo**, so
  reviewers can both try the app and read the code.
- ✅ The live demo uses **demo profile data only** (`Demo:Seed=true`, the fictional "Alex
  Demo"). Keep your real/private self-knowledge data on your local machine, not on the
  public demo.
- ℹ️ It's worth adding a one-line note near the link like *"Hosted on Azure free tier —
  first load may be slow as the app wakes up."* so a slow cold start doesn't read as a bug.

---

## Local development is unchanged

Nothing here affects local dev. Continue to run the backend (`dotnet run --project GlowUp.Api`,
reads user-secrets) and the frontend (`cd glowup.client && npm run dev`) separately;
the frontend still calls `http://localhost:5288`. `Database:MigrateOnStartup` and
`Demo:Seed` default to `false`, so your local database and data are untouched.

---

## Optional: if the free tier does not work for you

Only use these **paid** options if the free tiers are unavailable for your subscription
(e.g. the free SQL offer is already used, or you need "Always On" so the demo never sleeps).
**These cost money — confirm pricing first.**

**Paid App Service (no sleep, "Always On" available):**
```bash
az appservice plan create --name $PLAN --resource-group $RG --sku B1 --is-linux
# Optional, reduces cold starts:
az webapp config set --resource-group $RG --name $APP --always-on true
```

**Paid Azure SQL Basic:**
```bash
az sql db create --name $SQLDB --resource-group $RG --server $SQLSERVER \
  --service-objective Basic
```

If you switch to paid tiers, the budget/alert and "delete the resource group when done"
steps above become **more** important, not less.
