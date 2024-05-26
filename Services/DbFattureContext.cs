using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using FatturaElettronica_PRISM_WPF_V2.Models;

namespace FatturaElettronica_PRISM_WPF_V2.Services;

public partial class DbFattureContext : DbContext
{
    public string ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=DBFatture;Integrated Security=True";

    public DbFattureContext() { }

    public DbFattureContext(DbContextOptions<DbFattureContext> options) : base(options) { }

    public virtual DbSet<Cliente> Clienti { get; set; }

    public virtual DbSet<Fattura> Fatture { get; set; }

    public virtual DbSet<Fornitore> Fornitori { get; set; }

    public virtual DbSet<RigaDettaglio> RigheDettaglio { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //	=> optionsBuilder.UseSqlServer("Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=DBFattureProva;Integrated Security=True");

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(ConnectionString);
        optionsBuilder.EnableSensitiveDataLogging(true);
    }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.PartitaIVA);
            entity.HasIndex(e => e.Comune, "IX_Clienti_Comune");
            entity.HasIndex(e => e.Denominazione, "IX_Clienti_Denominazione")
                .IsUnique();
            entity.HasIndex(e => e.Provincia, "IX_Clienti_Provincia");
            entity.Property(e => e.PartitaIVA)
                .IsRequired()
                .HasMaxLength(13)
                .IsFixedLength();
            entity.Property(e => e.CAP)
                .HasMaxLength(5)
                .IsFixedLength();
            entity.Property(e => e.Comune)
                 .IsRequired()
                 .HasMaxLength(50);
            entity.Property(e => e.Denominazione)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Indirizzo)
                .HasMaxLength(50);
            entity.Property(e => e.Provincia)
                 .IsRequired()
                 .HasMaxLength(2)
                 .IsFixedLength();
        });

        modelBuilder.Entity<Fornitore>(entity =>
        {
            entity.HasKey(e => e.PartitaIVA);
            entity.HasIndex(e => e.Comune, "IX_Fornitori_Comune");
            entity.HasIndex(e => e.Denominazione, "IX_Fornitori_Denominazione")
                .IsUnique();
            entity.HasIndex(e => e.Provincia, "IX_Fornitori_Provincia");
            entity.Property(e => e.PartitaIVA)
                .IsRequired()
                .HasMaxLength(13)
                .IsFixedLength();
            entity.Property(e => e.CAP)
                .HasMaxLength(5)
                .IsFixedLength();
            entity.Property(e => e.Comune)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Denominazione)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Indirizzo)
                .HasMaxLength(50);
            entity.Property(e => e.Provincia)
                .IsRequired()
                .HasMaxLength(2)
                .IsFixedLength();
        });

        modelBuilder.Entity<Fattura>(entity =>
        {
            entity.HasKey(e => e.IdFattura);

            entity.HasIndex(e => e.Anno, "IX_Fatture_Anno");

            entity.HasIndex(e => e.IdCliente, "IX_Fatture_IdCliente");

            entity.HasIndex(e => e.IdFornitore, "IX_Fatture_IdFornitore");

            entity.HasIndex(e => e.Mese, "IX_Fatture_Mese");

            entity.HasIndex(e => e.Tipo, "IX_Fatture_Tipo");

            entity.Property(e => e.IdFattura)
                .IsRequired()
                .HasMaxLength(16)
                .IsFixedLength();
            entity.Property(e => e.NomeFile)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Anno)
                .IsRequired()
                .HasMaxLength(4)
                .IsFixedLength();
            entity.Property(e => e.Giorno)
                .IsRequired()
                .HasMaxLength(2)
                .IsFixedLength();
            entity.Property(e => e.IdCliente)
                .IsRequired()
                .HasMaxLength(13);
            entity.Property(e => e.IdFornitore)
                .IsRequired()
                .HasMaxLength(13);
            entity.Property(e => e.Imponibile)
                .IsRequired()
                .HasColumnType("money");
            entity.Property(e => e.Importo)
                .IsRequired()
                .HasColumnType("money");
            entity.Property(e => e.Imposta)
                .HasColumnType("money");
            entity.Property(e => e.Mese)
                .IsRequired()
                .HasMaxLength(2)
                .IsFixedLength();
            entity.Property(e => e.Numero)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Tipo)
                .IsRequired()
                .HasMaxLength(1)
                .IsFixedLength();
            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.FattureClienteOC)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Fatture_Clienti");
            entity.HasOne(d => d.IdFornitoreNavigation).WithMany(p => p.FattureFornitoreOC)
                .HasForeignKey(d => d.IdFornitore)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Fatture_Fornitori");
        });

        modelBuilder.Entity<RigaDettaglio>(entity =>
        {
            entity.HasKey(e => e.IdRigaDettaglio);
            entity.HasIndex(e => e.AliquotaIva, "IX_RigheDettaglio_AliquotaIVA");
            entity.HasIndex(e => e.CodiceArticolo, "IX_RigheDettaglio_CodiceArticolo");
            entity.HasIndex(e => e.IdFattura, "IX_RigheDettaglio_IdFattura");
            entity.Property(e => e.IdRigaDettaglio)
                .IsRequired()
                .HasMaxLength(20)
                .IsFixedLength()
                .ValueGeneratedNever();
            entity.Property(e => e.IdFattura)
                .IsRequired()
                .HasMaxLength(13)
                .IsFixedLength();
            entity.Property(e => e.CodiceArticolo)
               .IsRequired()
               .HasMaxLength(15);
            entity.Property(e => e.DescrizioneArticolo)
                .HasMaxLength(50);
            entity.Property(e => e.Quantità)
                .IsRequired()
                .HasColumnType("decimal(8, 3)");
            entity.Property(e => e.UnitàMisura)
                .HasMaxLength(10);
            entity.Property(e => e.PrezzoUnitario)
                .IsRequired()
                .HasColumnType("money");
            entity.Property(e => e.Imponibile)
                .IsRequired()
                .HasColumnType("money");
            entity.Property(e => e.AliquotaIva)
                 .HasColumnName("AliquotaIVA");
            entity.Property(e => e.Imposta)
                .HasColumnType("money");
            entity.Property(e => e.PrezzoTotale)
                .IsRequired()
                .HasColumnType("money");
            entity.HasOne(d => d.IdFatturaNavigation).WithMany(p => p.RigheDettaglioFatturaOC)
                .HasForeignKey(d => d.IdFattura)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RigheDettaglio_Fatture");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
