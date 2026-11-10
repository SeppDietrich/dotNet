using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TransAbon.Models;

namespace TransAbon.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Categorie> Categorii => Set<Categorie>();
    public DbSet<TipAbonament> TipuriAbonament => Set<TipAbonament>();
    public DbSet<Comanda> Comenzi => Set<Comanda>();
    public DbSet<Abonament> Abonamente => Set<Abonament>();
    public DbSet<Control> Controale => Set<Control>();
    public DbSet<EvenimentJurnal> Jurnal => Set<EvenimentJurnal>();
    public DbSet<Feedback> Feedbackuri => Set<Feedback>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<ApplicationUser>(e =>
        {
            e.Property(x => x.Nume).HasMaxLength(100);
            e.Property(x => x.Prenume).HasMaxLength(100);
            e.Property(x => x.CodLegitimatie).HasMaxLength(50);
        });

        b.Entity<Categorie>(e =>
        {
            e.Property(x => x.Nume).IsRequired().HasMaxLength(200);
            e.Property(x => x.Descriere).HasMaxLength(500);
            e.HasIndex(x => x.Nume).IsUnique();
        });

        b.Entity<TipAbonament>(e =>
        {
            e.HasOne(x => x.Categorie).WithMany(c => c.Tipuri)
                .HasForeignKey(x => x.CategorieId).OnDelete(DeleteBehavior.Restrict);
            // O categorie are un singur tip pentru fiecare durată.
            e.HasIndex(x => new { x.CategorieId, x.DurataLuni }).IsUnique();
        });

        b.Entity<Comanda>(e =>
        {
            e.HasOne(x => x.Client).WithMany()
                .HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.TipAbonament).WithMany()
                .HasForeignKey(x => x.TipAbonamentId).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.Stare).HasConversion<string>().HasMaxLength(20);
        });

        b.Entity<Abonament>(e =>
        {
            e.Property(x => x.Cod).IsRequired().HasMaxLength(32);
            e.HasIndex(x => x.Cod).IsUnique();
            e.Property(x => x.MotivAnulare).HasMaxLength(500);
            e.Property(x => x.Stare).HasConversion<string>().HasMaxLength(20);

            e.HasOne(x => x.Client).WithMany()
                .HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.TipAbonament).WithMany()
                .HasForeignKey(x => x.TipAbonamentId).OnDelete(DeleteBehavior.Restrict);
            // O comandă confirmată emite exact un abonament.
            e.HasOne(x => x.Comanda).WithOne(c => c.Abonament)
                .HasForeignKey<Abonament>(x => x.ComandaId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Control>(e =>
        {
            e.Property(x => x.CodIntrodus).IsRequired().HasMaxLength(64);
            e.Property(x => x.Cursa).HasMaxLength(100);
            e.Property(x => x.Rezultat).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.DataOra);

            e.HasOne(x => x.Controlor).WithMany()
                .HasForeignKey(x => x.ControlorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Abonament).WithMany()
                .HasForeignKey(x => x.AbonamentId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<EvenimentJurnal>(e =>
        {
            e.Property(x => x.Actiune).IsRequired().HasMaxLength(100);
            e.Property(x => x.Obiect).HasMaxLength(300);
            e.HasIndex(x => x.DataOra);

            // Jurnalul se păstrează chiar dacă utilizatorul este șters.
            e.HasOne(x => x.Utilizator).WithMany()
                .HasForeignKey(x => x.UtilizatorId).OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Feedback>(e =>
        {
            e.Property(x => x.Nume).IsRequired().HasMaxLength(200);
            e.Property(x => x.Email).IsRequired().HasMaxLength(200);
            e.Property(x => x.Mesaj).IsRequired().HasMaxLength(2000);
        });
    }
}
