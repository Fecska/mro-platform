# Deploy Guide — Railway (API) + Vercel (Web)

## Előfeltételek
- GitHub repo (push a kód oda)
- Railway account: railway.app (ingyenes)
- Vercel account: vercel.com (ingyenes)

---

## 1. Backend deploy — Railway

### 1.1 Projekt létrehozása
1. Menj: railway.app → **New Project** → **Deploy from GitHub repo**
2. Válaszd ki a repót
3. **Root Directory**: `apps/api`
4. Railway automatikusan megtalálja a `Dockerfile`-t

### 1.2 PostgreSQL hozzáadása
1. A projekten belül: **+ New** → **Database** → **Add PostgreSQL**
2. Railway automatikusan kitölti a `DATABASE_URL` változót
3. De az alkalmazásunk `ConnectionStrings__Postgres`-t vár, ezért:
   - Kattints a PostgreSQL service-re → **Variables**
   - Másold ki a `DATABASE_URL` értékét
   - A saját API service Variables tabján add hozzá:
     ```
     ConnectionStrings__Postgres = [a másolt connection string postgres:// formátumban]
     ```
   - **Fontos**: Railway Postgres connection string `postgres://user:pass@host:port/db` formátumban van,
     de a .NET Npgsql `Host=...;Port=...;Database=...;Username=...;Password=...` formátumot vár.
     Alakítsd át, vagy használd a Railway beépített Variable Reference-t:
     ```
     ConnectionStrings__Postgres=Host=${{Postgres.PGHOST}};Port=${{Postgres.PGPORT}};Database=${{Postgres.PGDATABASE}};Username=${{Postgres.PGUSER}};Password=${{Postgres.PGPASSWORD}}
     ```

### 1.3 Környezeti változók beállítása
A Railway API service **Variables** tabján add hozzá:

```
Jwt__SecretKey          = [random 32+ karakteres string, pl. openssl rand -hex 32 paranccsal]
Jwt__Issuer             = https://[a-railway-url].up.railway.app
Jwt__Audience           = mro-api
ASPNETCORE_ENVIRONMENT  = Production
```

### 1.4 Deploy
- Mentés után Railway automatikusan buildet indít
- Kövesd a **Deployments** tabban
- Ha kész: másold ki a generált URL-t (pl. `https://mro-api-production.up.railway.app`)

---

## 2. Frontend deploy — Vercel

### 2.1 Vercel config frissítése
**Mielőtt pusholsz**, frissítsd a `apps/web/vercel.json` fájlt a Railway URL-lel:

```json
{
  "rewrites": [
    {
      "source": "/api/:path*",
      "destination": "https://mro-api-production.up.railway.app/api/:path*"
    },
    {
      "source": "/(.*)",
      "destination": "/index.html"
    }
  ]
}
```

### 2.2 Vercel projekt létrehozása
1. Menj: vercel.com → **Add New Project** → Importáld a GitHub repót
2. **Root Directory**: `apps/web`
3. **Framework Preset**: Vite
4. **Build Command**: `npm run build`
5. **Output Directory**: `dist`
6. Kattints **Deploy**

### 2.3 Kész!
Vercel ad egy URL-t (pl. `https://mro-web.vercel.app`).

---

## 3. CORS finomhangolás (opcionális, biztonságosabb)

Ha csak a saját Vercel URL-ről akarod engedélyezni a kéréseket, add hozzá a Railway Variables-hez:

```
Cors__AllowedOrigins__0 = https://mro-web.vercel.app
```

---

## 4. Ellenőrzés

| URL | Mit kell látni |
|-----|----------------|
| `https://[railway-url]/health` | `{"status":"healthy","timestamp":"..."}` |
| `https://[railway-url]/swagger` | Swagger UI |
| `https://[vercel-url]` | MRO Platform login oldal |

---

## 5. Seed adatok (demo)

A demo-hoz töltsd fel az adatbázist seed adatokkal:
```
https://[railway-url]/swagger → POST /api/auth/register (vagy ami elérhető)
```

Vagy futtass egy seed script-et lokálisan a Railway Postgres ellen
(connection string a Railway Variables tabból).
