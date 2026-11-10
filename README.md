# TransAbon

**TransAbon – Sistemul de gestionare a abonamentelor de transport.** Aplicație Web (ASP.NET Core MVC) prin care clienții comandă online abonamente de transport interurban, controlorii verifică valabilitatea lor prin codul unic, iar administratorul gestionează catalogul, conturile și rapoartele.

Proiect realizat în baza caietului de sarcini (versiunea 1.0, Colegiul Universității Tehnice a Moldovei, 2026). Beneficiar: Iurcu Nicolae, operator de transport interurban.

## Cuprins

1. [Specificații](#1-specificații)
2. [Tehnologii](#2-tehnologii)
3. [Pornire rapidă](#3-pornire-rapidă)
4. [Structura proiectului](#4-structura-proiectului)
5. [Baza de date](#5-baza-de-date)
6. [Configurare](#6-configurare)
7. [Starea funcționalităților](#7-starea-funcționalităților)
8. [Lucrul în echipă](#8-lucrul-în-echipă)
9. [Probleme frecvente](#9-probleme-frecvente)

---

## 1. Specificații

### Roluri

| Rol | Ce poate face | Pagina de start |
| --- | --- | --- |
| Vizitator | Consultă catalogul public, trimite feedback | `/` |
| Client | Își creează cont, comandă abonamente, își vede istoricul, anulează înainte de start | `/cont` |
| Controlor | Introduce codul unui abonament și primește rezultatul: valid, expirat, anulat sau cod inexistent | `/validare` |
| Administrator | Gestionează tipurile de abonament și tarifele, conturile, jurnalul și rapoartele | `/admin` |

Un abonament nu este legat de un traseu, ci de o **perioadă de valabilitate** (1, 3 sau 6 luni) și de o **categorie** de călători (General, Elev, Student etc.). Este valabil pe toate mijloacele de transport din regiune, deci controlorul verifică un singur lucru: dacă abonamentul este activ la momentul controlului.

### Ce include această etapă

Comandă online, gestiunea contului, validare, administrare, rapoarte.

### Ce nu include această etapă

- plata online cu cardul (decontarea se face în afara aplicației);
- aplicație mobilă separată;
- scanarea codului QR (codul se introduce manual);
- migrarea automată a istoricului vechi de abonamente.

### Reguli importante din caietul de sarcini

- Parolele se păstrează doar sub formă de hash; comunicarea se face prin HTTPS.
- Fiecare pagină și acțiune este restricționată pe server în funcție de rol.
- Datele personale ale clienților nu apar în catalogul public.
- Interfața respectă WCAG 2.1 nivel AA (contrast, navigare de la tastatură, etichete la formulare).
- Cerințele sunt marcate M (obligatorie), R (recomandată), O (opțională).

## 2. Tehnologii

| Componentă | Tehnologie |
| --- | --- |
| Platformă | .NET 10, ASP.NET Core MVC (Razor views) |
| Limbaj | C# (nullable activat) |
| Bază de date | SQLite (fișier local) |
| Acces la date | Entity Framework Core 10 cu migrări |
| Autentificare și roluri | ASP.NET Core Identity (cookie, blocare cont după 5 încercări greșite) |
| Front-end | HTML, CSS simplu (`wwwroot/styles/`), jQuery Validation pentru formulare |
| Unelte | `dotnet-ef` (instalat local în repo, vezi `.config/dotnet-tools.json`) |

SQLite a fost ales pentru simplitate: nu cere server de baze de date, iar fiecare dezvoltator pornește proiectul cu o singură comandă. Caietul de sarcini permite o alternativă la SQL Server, cu justificare tehnică. Pentru producție se schimbă doar pachetul (`UseSqlServer`) și connection string-ul, restul codului rămâne la fel.

## 3. Pornire rapidă

### Cerințe

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Git
- Un IDE (Rider, Visual Studio sau VS Code)

### Pași

```bash
git clone <adresa-repository-ului>
cd <folderul-proiectului>

dotnet tool restore                  # instalează dotnet-ef (o singură dată)
dotnet restore
dotnet ef database update            # creează baza de date locală
dotnet run --launch-profile https
```

Aplicația pornește la https://localhost:7081 (sau http://localhost:5250).

La pornire, aplicația aplică automat migrările și încarcă datele inițiale, deci `dotnet ef database update` este necesar doar dacă vrei să creezi baza înainte de prima rulare. **După fiecare `git pull`** rulează din nou `dotnet ef database update`, pentru că un coleg poate să fi schimbat modelul de date.

### Conturi de test (doar în Development)

| Rol | E-mail | Parolă |
| --- | --- | --- |
| Administrator | `admin@transabon.md` | `Demo123!` |
| Controlor | `controlor@transabon.md` | `Demo123!` |
| Client | `client@transabon.md` | `Demo123!` |

Conturile sunt definite în `appsettings.Development.json`. Același seed încarcă și datele demo: catalogul de abonamente, trei abonamente ale clientului (expirat, activ, programat) și trei controale (coduri `TA-DEMO-0002`, `TA-DEMO-0004`, `TA-DEMO-0005`).

### Imagini care trebuie copiate manual

- `wwwroot/images/`: `dropdown.png`, `loggin.png`, `previous.png`, `next.png`
- `wwwroot/carousel-images/`: imaginile caruselului (jpg, jpeg, png, gif, webp). Se încarcă automat, în ordine alfabetică. Dacă folderul e gol, caruselul nu se afișează.

## 4. Structura proiectului

```
Controllers/     Controllere MVC (Home, Account, Dashboard, Abonamente, Feedback)
Data/            AppDbContext, DbSeeder
Migrations/      Migrările EF Core (generate, se comit în Git)
Models/          Entități, enum-uri, ViewModel-uri, opțiuni de configurare
Services/        Logica reutilizabilă (carusel, claims pentru Identity)
Views/           Pagini Razor, organizate pe controller; Shared/ pentru layout și componente
wwwroot/         Fișiere statice: styles/, images/, carousel-images/, lib/
App_Data/        Baza de date SQLite locală (ignorată de Git)
docs/            Documentația echipei (lista sarcinilor)
.github/         Șabloane pentru issue-uri și pull request-uri
```

Numele proiectului, `WebApplication5`, este istoric. Namespace-ul este `TransAbon`. Nu redenumi proiectul fără acordul echipei, pentru că ar afecta configurările locale din IDE.

## 5. Baza de date

### Modelul de date

```mermaid
erDiagram
    ApplicationUser ||--o{ Comanda : plaseaza
    ApplicationUser ||--o{ Abonament : detine
    ApplicationUser ||--o{ Control : efectueaza
    ApplicationUser ||--o{ EvenimentJurnal : genereaza
    Categorie ||--o{ TipAbonament : grupeaza
    TipAbonament ||--o{ Comanda : alesa_in
    TipAbonament ||--o{ Abonament : bazat_pe
    Comanda ||--o| Abonament : emite
    Abonament ||--o{ Control : verificat_prin
```

| Entitate | Rol | Câmpuri principale |
| --- | --- | --- |
| `ApplicationUser` | Cont de sistem (client, controlor, administrator) | Nume, Prenume, Activ, DataInregistrarii, CodLegitimatie (doar controlori) |
| `Categorie` | Categorie de călători | Nume, Descriere, Activ |
| `TipAbonament` | Șablon comercial: categorie + durată + preț | CategorieId, DurataLuni (1/3/6), Pret (lei, 0 = gratuit), Activ |
| `Comanda` | Solicitarea clientului | ClientId, TipAbonamentId, DataComenzii, DataInceput, Stare |
| `Abonament` | Dreptul de a călători | Cod (unic), DataInceput, DataExpirare, Stare, MotivAnulare |
| `Control` | O verificare făcută de controlor | ControlorId, AbonamentId (nul dacă nu există), CodIntrodus, DataOra, Rezultat, Cursa |
| `EvenimentJurnal` | Jurnal de audit | DataOra, UtilizatorId (nul permis), Actiune, Obiect |
| `Feedback` | Mesaj din formularul public | Nume, Email, Evaluare (1–5), Mesaj, DataTrimiterii |

Detalii de care trebuie ținut cont:

- Starea **programat** a unui abonament nu se memorează: se deduce din `DataInceput > azi`. La fel, un abonament cu `DataExpirare` trecută este expirat chiar dacă `Stare` nu a fost actualizată. Verificarea corectă se face după date.
- `DataExpirare = DataInceput + durata în luni − 1 zi` (început 2026-09-23, 1 lună → expiră 2026-10-22).
- Relațiile folosesc `Restrict` (nu se șterge în cascadă). Un client sau un tip de abonament cu istoric se **dezactivează**, nu se șterge.

### Migrări EF Core

Schema bazei de date se versionează în folderul `Migrations/`. Fișierul `.db` **nu** se comite în Git: fiecare dezvoltator își creează propria bază din migrări, iar datele de test vin din `DbSeeder`.

| Situație | Comandă |
| --- | --- |
| Ai făcut `git pull` | `dotnet ef database update` |
| Ai schimbat o entitate sau `AppDbContext` | `dotnet ef migrations add NumeSugestiv` apoi `dotnet ef database update` |
| Vrei baza de la zero | șterge `App_Data/transabon.db*`, apoi `dotnet ef database update` |

Reguli de colaborare pentru migrări sunt în [CONTRIBUTING.md](CONTRIBUTING.md).

> **Pentru mentenanța proiectului (o singură dată):** repository-ul trebuie să conțină migrarea inițială. Dacă folderul `Migrations/` lipsește, rulează `dotnet ef migrations add InitialCreate` și comite rezultatul.

### Date inițiale (seed)

`DbSeeder` rulează la fiecare pornire și este idempotent:

1. Creează rolurile `Client`, `Controlor`, `Administrator`.
2. Creează conturile din `SeedUsers` (dacă există în configurare).
3. Încarcă categoriile și tipurile de abonament din `AbonamenteData`, doar dacă tabelele sunt goale.
4. Doar în Development: abonamente și controale demo pentru contul de client.

## 6. Configurare

| Fișier | Conținut |
| --- | --- |
| `appsettings.json` | `AppInfo` (autor, versiune, grupă afișate în footer), `ConnectionStrings:Default` |
| `appsettings.Development.json` | Conturile de test (`SeedUsers`), doar pentru dezvoltare |

În producție, conturile inițiale **nu** se păstrează în fișiere din repository. Se setează prin variabile de mediu sau user-secrets:

```bash
dotnet user-secrets init
dotnet user-secrets set "SeedUsers:Administrator:Email" "admin@exemplu.md"
dotnet user-secrets set "SeedUsers:Administrator:Parola" "<parola-puternică>"
```

Nu comite niciodată parole, chei sau fișiere `.db` reale.

## 7. Starea funcționalităților

| Rută | Pagină | Stare |
| --- | --- | --- |
| `/` | Acasă (carusel, categorii, pași de comandă) | Funcțională (tarife din `AbonamenteData`) |
| `/despre` | Despre | Funcțională |
| `/abonamente` | Catalog și tarife | Funcțională (tarife din `AbonamenteData`) |
| `/contact` | Contact | Funcțională |
| `/feedback` | Formular feedback | Validează datele, **nu le salvează** |
| `/autentificare`, `/inregistrare`, `/deconectare` | Autentificare, creare cont | Funcțională (Identity) |
| `/acces-interzis` | Acces refuzat | Funcțională |
| `/cont` | Contul clientului | Machetă cu date fictive |
| `/validare` | Validare abonamente | Funcțională |
| `/admin` | Administrare (indicatori reali) | Funcțională |
| `/admin/utilizatori` | Administrare conturi și controlori | Funcțională |
| `/admin/jurnal` | Jurnal de evenimente (audit) | Funcțională |
| `/admin/rapoarte` | Rapoarte și export CSV | Funcțională |

Paginile marcate „machetă” sunt înlocuite de sarcinile din [docs/sarcini.md](docs/sarcini.md).

## 8. Lucrul în echipă

- Sarcinile sunt descrise în [docs/sarcini.md](docs/sarcini.md) și se creează ca issue-uri în GitHub, folosind șablonul din `.github/ISSUE_TEMPLATE/`.
- Regulile de lucru (branch-uri, commit-uri, pull request-uri, migrări, convenții de cod) sunt în [CONTRIBUTING.md](CONTRIBUTING.md).
- Referința funcțională este caietul de sarcini: orice neclaritate se verifică acolo înainte să se ia o decizie.

## 9. Probleme frecvente

**`table "AspNetRoles" already exists` sau `table ... already exists` la pornire.** Ai o bază de date veche, creată înainte de introducerea migrărilor. Șterge `App_Data/transabon.db*` și pornește din nou.

**`no such table: ...` sau `no such column: ...`.** Ai făcut `git pull` și baza ta nu are ultimele migrări. Rulează `dotnet ef database update`.

**`Could not execute because the specified command or file was not found` la `dotnet ef`.** Rulează o singură dată `dotnet tool restore` în folderul proiectului.

**Conflict în `AppDbContextModelSnapshot.cs` la merge.** Vezi secțiunea despre migrări din [CONTRIBUTING.md](CONTRIBUTING.md).

**Nu pot să mă autentific cu un cont de test.** Conturile se creează doar dacă `appsettings.Development.json` conține `SeedUsers` și aplicația rulează în Development (`ASPNETCORE_ENVIRONMENT=Development`, setat în `launchSettings.json`).

**Caruselul nu apare.** Folderul `wwwroot/carousel-images/` este gol.
