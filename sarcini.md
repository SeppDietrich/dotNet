# Sarcini de dezvoltare

Șapte sarcini, gata de copiat în issue-uri GitHub (șablonul din `.github/ISSUE_TEMPLATE/sarcina.md`). Fiecare sarcină are un titlu, context, criterii de acceptare, fișiere afectate și dependențe.

## Înainte de a începe (pentru toate sarcinile)

- Citește [README.md](../README.md) și [CONTRIBUTING.md](../CONTRIBUTING.md).
- Modelul de date (entitățile, `AppDbContext`, seed-ul) este deja făcut. Nu crea alte entități decât dacă sarcina o cere explicit.
- Datele demo din Development sunt: conturile `admin@`, `controlor@`, `client@transabon.md` (parola `Demo123!`) și abonamentele `TA-DEMO-0002`, `TA-DEMO-0004`, `TA-DEMO-0005`.
- Lucrează în fișiere noi (controller și folder de view-uri proprii). Nu rescrie `DashboardController` și nu extinde `Admin.cshtml` cu logică: adaugi acolo doar un link sau un panel către pagina ta.

## Ordinea recomandată și dependențe

| Sarcina | Depinde de | Se poate începe imediat |
| --- | --- | --- |
| 1. Feedback | – | da |
| 2. Client, comandă | – | da |
| 3. Controlor, validare | – | da |
| 4. Admin, catalog | – | da (vezi nota despre sarcina 2) |
| 5. Admin, conturi | – | da |
| 6. Admin, jurnal și rapoarte | – | da; rapoartele au sens complet după 2 și 3 |
| 7. Client extra și curățenie | 2 (doar notificările) | partea de curățenie, da |

**Jurnalul de evenimente (sarcina 6).** Sarcina 6 începe cu un PR mic care creează `Services/JurnalService` (metoda `LogAsync(utilizator, actiune, obiect)`). Până la acel merge, celelalte sarcini nu scriu în jurnal. După merge, fiecare acțiune de administrare, comandă și validare se jurnalizează într-un PR separat sau în sarcina proprie.

**Catalogul (sarcinile 2 și 4).** Sarcina 2 citește tipurile de abonament direct din baza de date (`TipuriAbonament`, populate de seed), nu din `AbonamenteData`. Sarcina 4 înlocuiește `AbonamenteData` și în paginile publice.

---

## Sarcina 1. Formularul de Feedback: salvare și consultare

**Context.** Formularul `/feedback` validează datele, dar le aruncă. Mesajele trebuie păstrate, iar administratorul trebuie să le poată citi.

**Descriere.** Salvează mesajele în tabelul `Feedback` și creează o pagină de consultare pentru administrator.

**Criterii de acceptare**
- [ ] La trimiterea unui formular valid, mesajul se salvează în baza de date (entitatea `Feedback`, cu `DataTrimiterii`).
- [ ] Validările existente rămân (nume minim 2 caractere, e-mail valid, evaluare 1–5, mesaj minim 10 caractere), iar mesajele de eroare se afișează la fel ca acum.
- [ ] Pagina `/admin/feedback` este accesibilă doar administratorului (alt rol → `/acces-interzis`, nelogat → `/autentificare`).
- [ ] Pagina afișează mesajele de la cel mai nou la cel mai vechi: dată, nume, e-mail, evaluare, mesaj. Dacă nu sunt mesaje, afișează un text explicativ.
- [ ] Pagina afișează media evaluărilor și numărul total de mesaje.
- [ ] Textul introdus de utilizator este afișat codificat (fără HTML executabil).
- [ ] Din `Admin.cshtml` există un link către pagina nouă.
- [ ] Comentariul `TODO (sarcina 1)` din `FeedbackController` este eliminat.

**Fișiere afectate:** `Controllers/FeedbackController.cs` (modificat); `Controllers/AdminFeedbackController.cs`, `Views/AdminFeedback/Index.cshtml` (noi); `Views/Dashboard/Admin.cshtml` (doar un link).

**Dependențe.** Niciuna.

**Referință caiet.** Nu este în caietul de sarcini (funcționalitate adăugată de echipă); formularul există deja în aplicație.

**Note.** Nu se cere ștergerea sau răspunsul la mesaje. Dacă ți se pare util, propune în issue.

---

## Sarcina 2. Client: comandă abonament, listă proprie, anulare

**Context.** Pagina `/cont` este o machetă cu date fictive. Aceasta este funcționalitatea centrală a aplicației.

**Descriere.** Clientul alege categoria, durata și data de început, confirmă comanda și primește un abonament cu cod unic. Își vede abonamentele reale și poate anula unul care nu a început.

**Criterii de acceptare**
- [ ] Formularul de comandă folosește categoriile și duratele active din baza de date (`Categorii`, `TipuriAbonament`), cu prețul afișat înainte de confirmare (Legea 284/2004: condițiile comerciale se arată înainte de confirmare).
- [ ] La confirmare se creează o `Comanda` (stare `Confirmata`) și un `Abonament` legat de ea, cu `Cod` unic, `DataInceput` aleasă și `DataExpirare` calculată (început + durata în luni − 1 zi).
- [ ] Codul este generat de aplicație, alfanumeric, de forma `TA-XXXXXXXX` (minim 8 caractere aleatoare, generate cu un generator criptografic) și nu se poate repeta (index unic existent; la coliziune se regenerează).
- [ ] Data de început nu poate fi în trecut. Se afișează o eroare clară la dată invalidă.
- [ ] Lista „Abonamentele mele” arată doar abonamentele clientului conectat: cod, categorie, perioadă, început, expirare, stare (activ, programat, expirat, anulat).
- [ ] Starea „programat” și „expirat” se deduc din date; nu depinde doar de câmpul `Stare`.
- [ ] Clientul poate anula un abonament doar înainte de `DataInceput`. După anulare, `Stare = Anulat` și comanda devine `Anulata`. Încercarea de a anula un abonament început sau al altui client este refuzată pe server.
- [ ] Toate acțiunile sunt `[HttpPost]` cu `[ValidateAntiForgeryToken]` și `[Authorize(Roles = Client)]`.
- [ ] Mesaj de confirmare după comandă, cu codul abonamentului.
- [ ] Nota „Plata se face în afara aplicației” rămâne vizibilă.
- [ ] Datele fictive din `Views/Dashboard/Client.cshtml` sunt eliminate.

**Fișiere afectate:** `Controllers/ComenziController.cs`, `Services/AbonamentService.cs` (generare cod, calcul expirare), ViewModel-uri noi (noi); `Controllers/DashboardController.cs` și `Views/Dashboard/Client.cshtml` (acțiunea `Client` se înlocuiește; ruta `/cont` se păstrează); `Program.cs` (înregistrare serviciu).

**Dependențe.** Niciuna. Citește din baza de date, nu din `AbonamenteData`.

**Referință caiet.** 3.1 (fluxul „Comanda online”), 5.2 (cerințele M: cont, comandă, emitere, listă, anulare).

**Note.** Anularea de către administrator este în sarcina 5 sau 6. Notificarea de reînnoire este în sarcina 7. Rolul `Administrator` are acces la comandă conform tabelului 4.2 din caiet: decideți în issue dacă acceptați și această variantă acum.

---

## Sarcina 3. Controlor: validare după cod și istoricul zilei

**Context.** Pagina `/validare` este o machetă. Controlorul trebuie să verifice real un cod.

**Descriere.** Controlorul introduce codul unui abonament și vede rezultatul. Fiecare verificare se salvează.

**Criterii de acceptare**
- [ ] Introducerea unui cod caută abonamentul după `Cod` (fără diferență între litere mari și mici, fără spații la capete).
- [ ] Rezultatul afișat este unul dintre: **valid**, **expirat**, **anulat**, **cod inexistent**. Se calculează după `Stare` și după data curentă (valid doar dacă `Stare = Activ` și data de azi este între `DataInceput` și `DataExpirare`).
- [ ] Pentru un abonament valid se afișează perioada de valabilitate.
- [ ] Aplicația nu cere și nu verifică nicio informație despre traseu.
- [ ] Fiecare verificare creează un `Control` (controlor, abonament sau nul, cod introdus, data și ora, rezultat). Se salvează și codurile inexistente.
- [ ] Sub formular se afișează validările controlorului conectat din ziua curentă (ora, cod, rezultat), cele mai noi primele.
- [ ] Un abonament care nu a început încă (programat) este tratat și afișat clar; **notează în issue ce rezultat se afișează** (caietul nu definește explicit această situație).
- [ ] Acțiunea este `[HttpPost]` cu `[ValidateAntiForgeryToken]` și `[Authorize(Roles = Controlor)]`.
- [ ] Timpul de la trimitere la afișarea rezultatului este sub 5 secunde.
- [ ] Datele fictive din `Views/Dashboard/Controlor.cshtml` sunt eliminate.
- [ ] Nu se dezvăluie datele personale ale clientului (nume, e-mail) controlorului.

**Fișiere afectate:** `Controllers/ValidareController.cs`, `Services/ValidareService.cs`, ViewModel-uri (noi); `Controllers/DashboardController.cs` și `Views/Dashboard/Controlor.cshtml` (acțiunea `Controlor` se înlocuiește; ruta `/validare` se păstrează).

**Dependențe.** Niciuna. Poți testa cu `TA-DEMO-0002` (expirat), `TA-DEMO-0004` (valid), `TA-DEMO-0005` (programat).

**Referință caiet.** 3.2 (fluxul „Validarea”), 5.3 (cerințele M; cerințele R: istoricul zilei, selectarea cursei sunt opționale în această sarcină).

**Note.** Selectarea cursei (cerință R) se poate omite; câmpul `Cursa` din `Control` rămâne nul.

---

## Sarcina 4. Admin, catalog: categorii, tipuri de abonament și tarife

**Context.** Tarifele sunt scrise în cod (`AbonamenteData`). Caietul de sarcini cere ca administratorul să le modifice din panou, fără programator.

**Descriere.** CRUD pentru categorii și tipuri de abonament (durată și preț) și trecerea paginilor publice pe baza de date.

**Criterii de acceptare**
- [ ] Pagina `/admin/catalog` (doar administrator) listează categoriile cu tipurile lor: durată, preț, stare.
- [ ] Administratorul poate adăuga o categorie, modifica numele sau descrierea, o poate dezactiva și reactiva.
- [ ] Administratorul poate adăuga un tip (categorie, durată în luni, preț în lei întreg ≥ 0; 0 = gratuit), modifica prețul, dezactiva și reactiva.
- [ ] Nu se poate crea un tip duplicat pentru aceeași categorie și durată (mesaj clar, indexul unic există în bază).
- [ ] Nimic nu se șterge fizic dacă are istoric: se **dezactivează**. Abonamentele deja emise nu se modifică la schimbarea prețului.
- [ ] Paginile `/`, `/abonamente` și formularul de comandă afișează doar categoriile și tipurile **active**, din baza de date.
- [ ] Clasa `AbonamenteData` este eliminată, iar seed-ul din `DbSeeder` este adaptat să conțină datele inițiale direct (fără a depinde de ea). Tarifele inițiale rămân neschimbate.
- [ ] Validare pe server (nume obligatoriu, preț ≥ 0, durată > 0), `[ValidateAntiForgeryToken]` pe toate acțiunile, acces doar `Administrator`.
- [ ] Cerința O (import din fișier de calcul) nu este obligatorie.

**Fișiere afectate:** `Controllers/AdminCatalogController.cs`, `Views/AdminCatalog/*`, ViewModel-uri (noi); `Controllers/HomeController.cs`, `Controllers/AbonamenteController.cs`, `Views/Home/Index.cshtml`, `Views/Abonamente/Index.cshtml`, `Models/HomeViewModel.cs`, `Data/DbSeeder.cs` (modificate); `Models/AbonamenteData.cs` (șters); `Views/Dashboard/Admin.cshtml` (panelul de tarife înlocuit cu un link).

**Dependențe.** Niciuna. Dacă sarcina 2 nu este încă integrată, coordonați-vă pentru `Views/Dashboard/Client.cshtml`.

**Referință caiet.** 5.4 (cerința M: tipuri de abonament), capitolul 8 (conținut și administrare).

---

## Sarcina 5. Admin, conturi: creare controlori, activare și dezactivare

**Context.** Panoul de administrare arată conturi fictive. Administratorul trebuie să gestioneze real conturile.

**Descriere.** Pagină cu lista conturilor, crearea conturilor de controlor și dezactivarea sau activarea conturilor.

**Criterii de acceptare**
- [ ] Pagina `/admin/utilizatori` (doar administrator) listează toate conturile: e-mail, nume, rol, stare, data înregistrării. Căutare după e-mail sau nume.
- [ ] Administratorul creează un cont de **controlor** (nume, prenume, e-mail, parolă inițială, cod de legitimație). Rolul se stabilește în cod, nu din formular. Parola respectă regulile din Identity (min. 8 caractere, literă mare, literă mică, cifră).
- [ ] Mesajele de eroare (e-mail duplicat, parolă slabă) sunt în română, ca la înregistrare.
- [ ] Administratorul poate dezactiva și reactiva orice cont de client sau controlor. Un cont dezactivat nu se poate autentifica (verificarea există deja în `AccountController`).
- [ ] Administratorul nu își poate dezactiva propriul cont, iar ultimul administrator activ nu poate fi dezactivat.
- [ ] Conturile nu se șterg.
- [ ] Toate acțiunile sunt `[HttpPost]` cu `[ValidateAntiForgeryToken]` și `[Authorize(Roles = Administrator)]`.
- [ ] Parolele nu se afișează și nu se loghează.
- [ ] Panelul „Conturi și roluri” din `Admin.cshtml` este înlocuit cu un link către pagina nouă.

**Fișiere afectate:** `Controllers/AdminUtilizatoriController.cs`, `Views/AdminUtilizatori/*`, ViewModel-uri (noi); `Views/Dashboard/Admin.cshtml` (panelul de conturi înlocuit cu un link).

**Dependențe.** Niciuna.

**Referință caiet.** 4.2 (matricea drepturilor), 5.4 (cerințele M: conturi de controlori, dezactivare conturi).

**Note.** Schimbarea rolului unui cont existent nu este cerută; butonul „Schimbă rol” din macheta veche se elimină.

---

## Sarcina 6. Admin, jurnal de evenimente și rapoarte

**Context.** Caietul cere jurnal de audit și rapoarte pe perioadă. În prezent panoul arată cifre și bare fictive.

**Descriere.** Serviciu de jurnalizare, pagină de consultare filtrabilă, rapoarte pe perioadă cu export CSV și indicatori reali în panou.

**Criterii de acceptare**
- [ ] **Primul PR (mic):** `Services/JurnalService` cu `LogAsync(utilizatorId, actiune, obiect)`, înregistrat în `Program.cs`. Se integrează înaintea restului sarcinii, ca ceilalți colegi să-l poată folosi.
- [ ] Se jurnalizează: autentificarea reușită și eșuată, crearea unui cont, comenzile, validările și anulările. Pentru acțiunile din sarcinile 2–5 adaugi apelul în `AccountController` și anunți colegii să-l adauge în codul lor.
- [ ] Pagina `/admin/jurnal` afișează evenimentele, cele mai noi primele, cu paginare, și filtre după utilizator, acțiune și interval de dată.
- [ ] Pagina `/admin/rapoarte` generează: (a) abonamentele emise într-o perioadă selectată, grupate pe categorie și durată; (b) validările dintr-o perioadă, cu numărul de validări valide și invalide.
- [ ] Ambele rapoarte se pot exporta în CSV (cu codificare UTF-8 corectă pentru diacritice).
- [ ] Panoul `/admin` afișează indicatori reali: abonamente active, comenzi în ultima săptămână, validări azi, număr de conturi.
- [ ] Acces doar `Administrator`.
- [ ] Datele fictive (cifre, bare, rânduri) din `Admin.cshtml` sunt eliminate. Eliminați și resturile de test (`placeholder="HERE"`, rândul „Dominisoara Magdalena”).
- [ ] Interogările sunt scrise pentru a funcționa corect cu 10 000 de conturi (filtrare în baza de date, nu în memorie).

**Fișiere afectate:** `Services/IJurnalService.cs`, `Services/JurnalService.cs`, `Controllers/AdminJurnalController.cs`, `Controllers/AdminRapoarteController.cs`, `Views/AdminJurnal/*`, `Views/AdminRapoarte/*` (noi); `Controllers/DashboardController.cs` și `Views/Dashboard/Admin.cshtml` (indicatori reali); `Controllers/AccountController.cs` (apeluri de jurnalizare); `Program.cs`.

**Dependențe.** Rapoartele au conținut complet după integrarea sarcinilor 2 și 3; până atunci se pot testa cu datele demo din seed.

**Referință caiet.** 5.4 (jurnal filtrabil, cerință M), 5.5 (rapoarte: M; export CSV: R; tablou de bord: O), 6 (auditabilitate).

---

## Sarcina 7. Client extra, curățenie și accesibilitate

**Context.** Cerințele R/O pentru client, resturi din șablonul .NET și cerința de accesibilitate WCAG 2.1 AA.

**Descriere.** Notificarea de expirare, editarea contului, ștergerea fișierelor nefolosite și o trecere de bază pentru accesibilitate.

**Criterii de acceptare**
- [ ] **Editare cont** (independent de celelalte sarcini): clientul își poate modifica numele, prenumele, e-mailul și parola din `/cont/profil`. Pentru parolă se cere parola curentă. Acțiuni `[HttpPost]` cu `[ValidateAntiForgeryToken]`.
- [ ] **Curățenie:** eliminarea `Views/Home/Privacy.cshtml` și `Views/Shared/_Layout.cshtml.css` (resturi din șablon, nefolosite) și verificarea dacă `wwwroot/css/` și `wwwroot/js/` sunt folosite; dacă nu sunt, se șterg (menționează în PR ce ai verificat).
- [ ] **Accesibilitate de bază:** `aria-lable` greșit scris corectat; fiecare `input`, `select` și `textarea` are `label` asociat; contrast suficient pentru textul principal și butoane; paginile pot fi parcurse doar de la tastatură (meniul dropdown din `header.css` funcționează și cu Tab); fiecare imagine are `alt`. Notează în PR ce ai corectat.
- [ ] **Notificare de expirare** (după integrarea sarcinii 2): cu 3 zile înainte de expirarea unui abonament activ, clientul vede în `/cont` o notificare cu opțiunea de a comanda o reînnoire. Notificarea se calculează din date, nu se salvează în baza de date.
- [ ] Nu se modifică logica din sarcinile 2–6.

**Fișiere afectate:** `Controllers/ProfilController.cs`, `Views/Profil/*`, ViewModel-uri (noi); `Views/Shared/_Navbar.cshtml`, `wwwroot/styles/*.css`, view-urile cu formulare (accesibilitate); `Views/Home/Privacy.cshtml`, `Views/Shared/_Layout.cshtml.css` (șterse); `Views/Dashboard/Client.cshtml` (doar pentru notificare, după sarcina 2).

**Dependențe.** Notificarea de expirare depinde de sarcina 2. Restul se poate face imediat.

**Referință caiet.** 5.2 (cerințele R: notificare, editare cont), 6 (accesibilitate WCAG 2.1 AA), 1.5 (tabelul 1.2, rândul 5).

**Note.** Caietul cere interfața în engleză la cerința de localizare (R, capitolul 6), dar aplicația este în română. Nu schimbați limba; discutați în issue dacă vreți să propuneți altceva.
