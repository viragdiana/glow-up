# Glow Up — Azure Deployment Guide

This guide deploys Glow Up as **one public URL** that recruiters can open and use:
the .NET API serves the built React app, talks to Azure SQL, and uses Gemini (with a
mock fallback). All secrets live in Azure App Service configuration — never in code.

```
                ┌──────────────────────────────────────────────┐
   Browser ───▶ │  Azure App Service (one URL)                 │
                │   GlowUp.Api (.NET 10)                        │
                │   ├─ /            → React app (wwwroot)       │
                │   ├─ /api/...     → REST API                 │
                │   └─ Gemini  ──▶  Google Gemini (server-side) │
                └───────────────┬──────────────────────────────┘
                                │
                       ┌────────▼─────────┐
                       │  Azure SQL DB    │
                       └──────────────────┘
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

## 1. Create Azure resources

```bash
az group create --name $RG --location $LOCATION

# App Service (Linux, .NET 10)
az appservice plan create --name $PLAN --resource-group $RG --sku B1 --is-linux
az webapp create --name $APP --resource-group $RG --plan $PLAN --runtime "DOTNETCORE:10.0"

# Azure SQL
az sql server create --name $SQLSERVER --resource-group $RG --location $LOCATION \
  --admin-user $SQLADMIN --admin-password "$SQLPASSWORD"
az sql db create --name $SQLDB --resource-group $RG --server $SQLSERVER \
  --service-objective Basic

# Allow Azure services (App Service) to reach the SQL server
az sql server firewall-rule create --resource-group $RG --server $SQLSERVER \
  --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
```

---

## 2. Configure App Service settings (secrets live here, not in code)

### Connection string

```bash
az webapp config connection-string set --resource-group $RG --name $APP \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:$SQLSERVER.database.windows.net,1433;Initial Catalog=$SQLDB;User ID=$SQLADMIN;Password=$SQLPASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

App Service exposes this to the app as `ConnectionStrings:DefaultConnection`, which is
exactly what `Program.cs` reads.

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
| `AI:Provider` | `Gemini` or `Mock` | Which AI provider. `Mock` = no key needed |
| `AI:Gemini:ApiKey` | *your key* | Gemini key. **Omit to auto-fall back to mock** |
| `AI:Gemini:Model` | `gemini-2.5-flash` | Gemini model id |
| `Database:MigrateOnStartup` | `true` | Apply EF migrations on first boot (creates tables) |
| `Demo:Seed` | `true` | Seed portfolio sample data (idempotent; skips if data exists) |

To run **without Gemini**, set `AI__Provider=Mock` (or just omit the Gemini key) — the
app still works fully on the mock provider.

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
shows the "Alex Demo" profile with sample sections immediately.

### Manual alternative (no MSBuild target)

If you prefer to build the client yourself:

```bash
cd glowup.client && npm install && npm run build && cd ..
mkdir -p GlowUp.Api/wwwroot && cp -r glowup.client/dist/* GlowUp.Api/wwwroot/
dotnet publish GlowUp.Api/GlowUp.Api.csproj -c Release -o ./publish
```

---

## 4. Verify

- `https://<app>.azurewebsites.net/` — the app loads with demo data.
- `https://<app>.azurewebsites.net/api/profile` — returns the demo profile JSON.
- AI Chat page — ask a question; answers come from Gemini (or mock if no key). The UI is
  identical either way, and raw errors/secrets are never returned.

> Swagger is only enabled in Development, so it is not exposed on the production URL.

---

## Security notes

- No secret is committed: connection string and Gemini key live only in App Service
  settings (and user-secrets locally). `appsettings.json` holds only non-secret defaults.
- The Gemini key is used **server-side only** and never sent to the browser.
- Production returns a generic `500 { "error": "Something went wrong. Please try again." }`
  and never leaks exception details. AI failures fall back to the mock provider.
- AI questions are capped at 1000 characters.

## Local development is unchanged

Nothing here affects local dev. Continue to run the backend (`dotnet run --project GlowUp.Api`,
reads user-secrets) and the frontend (`cd glowup.client && npm run dev`) separately;
the frontend still calls `http://localhost:5288`. `Database:MigrateOnStartup` and
`Demo:Seed` default to `false`, so your local database and data are untouched.
