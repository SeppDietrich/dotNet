# TransAbon (ASP.NET Core MVC, net10.0)

Portare a site-ului PHP TransAbon în ASP.NET Core MVC.

## Pornire
```
dotnet restore
dotnet run --launch-profile https
```
(sau din Rider / Visual Studio, profilul `https`: https://localhost:7081)

## Imagini care trebuie copiate manual
- `wwwroot/images/`: `dropdown.png`, `loggin.png`, `previous.png`, `next.png`
- `wwwroot/carousel-images/`: imaginile caruselului (jpg, jpeg, png, gif, webp). Se încarcă automat, în ordine alfabetică. Dacă folderul e gol, caruselul nu se afișează.

## Rute
`/` Acasă · `/despre` · `/abonamente` · `/feedback` · `/contact` · `/autentificare`

## Configurare (appsettings.json)
- `AppInfo`: Autor, Versiune, Grupa (afișate în footer)
- `Database`: `Enabled` (false = login demo), plus Host, Name, User, Password, ConnectionString

## Conectarea bazei de date (mai târziu)
Se completează doar blocul TODO din `Services/AuthService.cs`; controllerul și view-ul rămân neschimbate.
Atenție: hash-urile create cu `password_hash` din PHP nu sunt compatibile cu `PasswordHasher` din .NET.

## Tarife
`Models/AbonamenteData.cs` (echivalentul `data/abonamente.php`), folosit de Acasă și de Abonamente.
