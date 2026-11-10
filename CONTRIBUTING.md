# Ghid de contribuție

Documentul descrie cum lucrăm împreună la TransAbon. Citește-l înainte de prima sarcină. Pașii de instalare sunt în [README.md](README.md).

## 1. Fluxul de lucru

1. Alege un issue care ți-a fost atribuit (sau cere unul). Citește-l complet, inclusiv secțiunea „Dependențe”.
2. Actualizează `main`: `git checkout main && git pull`, apoi `dotnet ef database update`.
3. Creează un branch din `main`: `git checkout -b feature/<nr-issue>-descriere-scurta`
   Exemple: `feature/2-comanda-abonament`, `fix/7-eticheta-aria-lable`.
4. Lucrează în pași mici și fă commit-uri frecvente.
5. Înainte de pull request: `git pull origin main` în branch-ul tău, rezolvă conflictele, verifică din nou că aplicația pornește.
6. Deschide un pull request către `main`. Completează șablonul și leagă issue-ul scriind `Closes #<nr>` în descriere.
7. Cere review de la cel puțin un coleg. Nu îți aprobi singur propriul PR.
8. După aprobare, face merge autorul PR-ului (varianta „Squash and merge”) și șterge branch-ul.

Nu se lucrează direct în `main` și nu se face `git push --force` pe branch-uri comune.

## 2. Commit-uri

Mesaj scurt, la imperativ, în română, cu prefix:

```
feat: adaugă salvarea feedback-ului în baza de date
fix: corectează calculul datei de expirare
docs: actualizează secțiunea despre migrări
refactor: mută generarea codului într-un serviciu
```

Un commit = o schimbare logică. Nu amesteca în același commit formatarea cu schimbări de funcționalitate.

## 3. Migrări EF Core (important)

Schema bazei de date este cod versionat în `Migrations/`. Greșelile aici blochează pe toată lumea, deci respectă regulile:

1. Modifici o entitate sau `AppDbContext`? Creezi **o singură** migrare pentru sarcina ta: `dotnet ef migrations add NumeSugestiv`. Apoi rulezi `dotnet ef database update` și verifici că aplicația pornește.
2. Comiți toate fișierele generate: migrarea, fișierul `.Designer.cs` și `AppDbContextModelSnapshot.cs`.
3. **Nu edita manual** migrări deja comise în `main` și nu le șterge.
4. Dacă la `git pull` apare un conflict în `AppDbContextModelSnapshot.cs`, **nu îl rezolva manual**:
   - șterge migrarea ta (fișierele `<data>_NumeSugestiv.cs` și `.Designer.cs`) sau, dacă nu a fost comisă, rulează `dotnet ef migrations remove`;
   - adu `AppDbContextModelSnapshot.cs` din `main`: `git checkout origin/main -- Migrations/AppDbContextModelSnapshot.cs`;
   - recreează migrarea cu `dotnet ef migrations add NumeSugestiv`.
5. Modelul de bază (entitățile din README) există deja. Schimbă-l doar dacă sarcina o cere explicit, și anunță echipa în issue.
6. Fișierul `.db` nu se comite niciodată (este în `.gitignore`).

## 4. Convenții de cod

- **Limba:** interfața și numele din domeniu sunt în română (`Abonament`, `Comanda`), la fel ca în caietul de sarcini. Codul tehnic (`Controller`, `Service`) rămâne în engleză. Comentariile și documentația sunt în română.
- **Securitate:**
  - fiecare acțiune care modifică date folosește `[HttpPost]` și `[ValidateAntiForgeryToken]`;
  - accesul pe roluri se verifică pe server cu `[Authorize(Roles = AppRoles.X)]`, nu doar prin ascunderea butoanelor;
  - datele introduse de utilizator se validează pe server (DataAnnotations în ViewModel);
  - nu se afișează date ale altui client (un client vede doar abonamentele lui);
  - nu se comit parole, chei sau date reale.
- **Structură:** controllere subțiri, logica reutilizabilă în `Services/` (înregistrată în `Program.cs`), entitățile în `Models/`, un ViewModel separat pentru fiecare formular.
- **Funcționalități noi în fișiere noi.** Pentru o pagină nouă creează un controller și un folder de view-uri proprii, în loc să extinzi `DashboardController` sau `Admin.cshtml`. Astfel colegii nu modifică aceleași fișiere și conflictele rămân rare. În `Admin.cshtml` adaugi doar un link sau un panel către pagina ta.
- **Fără date fictive în cod nou.** Machetele existente se înlocuiesc cu date din baza de date. Datele de test se adaugă în `DbSeeder`, doar pentru Development.
- **Interfață:** reutilizează clasele din `wwwroot/styles/` (`panel`, `btn`, `badge`, `form-group`, `msg-ok`, `msg-err`, `table-wrap`). Respectă cerințele de accesibilitate: `label` legat de fiecare câmp, contrast suficient, navigare de la tastatură.
- **Formatare:** lasă IDE-ul să formateze codul; nu reformata fișiere întregi în afara sarcinii tale.

## 5. Checklist pentru pull request

Înainte să ceri review, bifează:

- [ ] Aplicația pornește de la zero (bază ștearsă, `dotnet ef database update`, `dotnet run`).
- [ ] Am testat manual scenariile din criteriile de acceptare, cu rolul potrivit (client, controlor, administrator).
- [ ] Am verificat că rolurile care nu au voie nu pot accesa pagina (redirecționare spre autentificare sau `/acces-interzis`).
- [ ] Dacă am schimbat modelul: migrarea este inclusă, iar README-ul (diagrama și tabelul entităților) este actualizat.
- [ ] Nu am comis `.db`, `bin/`, `obj/`, parole sau fișiere personale de IDE.
- [ ] Am actualizat README-ul dacă s-a schimbat starea unei pagini (secțiunea „Starea funcționalităților”).
- [ ] Descrierea PR-ului conține `Closes #<nr>`.

## 6. Review

Cel care face review verifică:

1. Criteriile de acceptare din issue sunt îndeplinite.
2. Verificările de acces și de securitate din secțiunea 4 sunt respectate.
3. Codul este lizibil și nu conține resturi (cod comentat, `TODO` fără sarcină, date fictive).
4. Migrarea (dacă există) este una singură și a fost generată, nu scrisă de mână.

Comentariile sunt politicoase și concrete. Autorul răspunde la fiecare comentariu înainte de merge.

## 7. Dacă ești blocat

- Verifică mai întâi caietul de sarcini și secțiunea „Probleme frecvente” din README.
- Dacă o cerință este neclară sau două cerințe se contrazic, scrie întrebarea ca **comentariu în issue** și nu improviza.
- Dacă ai nevoie de o schimbare în ceva ce aparține altei sarcini, anunță în issue-ul respectiv.
