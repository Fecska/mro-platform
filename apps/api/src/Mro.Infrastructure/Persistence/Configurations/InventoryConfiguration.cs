using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mro.Domain.Aggregates.Inventory;

namespace Mro.Infrastructure.Persistence.Configurations;

public sealed class PartConfiguration : IEntityTypeConfiguration<Part>
{
    public void Configure(EntityTypeBuilder<Part> builder)
    {
        builder.ToTable("parts");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PartNumber).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Description).IsRequired().HasMaxLength(200);
        builder.Property(p => p.AtaChapter).HasMaxLength(10);
        builder.Property(p => p.UnitOfMeasure).IsRequired().HasMaxLength(10);
        builder.Property(p => p.Manufacturer).HasMaxLength(100);
        builder.Property(p => p.ManufacturerPartNumber).HasMaxLength(50);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.MinStockLevel).HasPrecision(10, 3);

        builder.HasIndex(p => new { p.PartNumber, p.OrganisationId })
            .IsUnique()
            .HasDatabaseName("ix_parts_number_org");
    }
}

public sealed class BinLocationConfiguration : IEntityTypeConfiguration<BinLocation>
{
    public void Configure(EntityTypeBuilder<BinLocation> builder)
    {
        builder.ToTable("bin_locations");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Code).IsRequired().HasMaxLength(30);
        builder.Property(b => b.Description).HasMaxLength(200);
        builder.Property(b => b.StoreRoom).HasMaxLength(60);

        builder.HasIndex(b => new { b.Code, b.OrganisationId })
            .IsUnique()
            .HasDatabaseName("ix_bin_locations_code_org");
    }
}

public sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("stock_items");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.QuantityOnHand).HasPrecision(10, 3);
        builder.Property(s => s.UnitCost).HasPrecision(10, 4);
        builder.Property(s => s.BatchNumber).HasMaxLength(50);
        builder.Property(s => s.SerialNumber).HasMaxLength(50);

        builder.HasIndex(s => new { s.PartId, s.BinLocationId, s.OrganisationId })
            .HasDatabaseName("ix_stock_items_part_bin");

        builder.Ignore(s => s.Reservations);
        builder.Ignore(s => s.Issues);
        builder.Ignore(s => s.Returns);
        builder.Ignore(s => s.QuantityReserved);
        builder.Ignore(s => s.QuantityAvailable);

        builder.HasMany<MaterialReservation>("_reservations")
            .WithOne()
            .HasForeignKey(r => r.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<MaterialIssue>("_issues")
            .WithOne()
            .HasForeignKey(i => i.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<MaterialReturn>("_returns")
            .WithOne()
            .HasForeignKey(r => r.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class MaterialReturnConfiguration : IEntityTypeConfiguration<MaterialReturn>
{
    public void Configure(EntityTypeBuilder<MaterialReturn> builder)
    {
        builder.ToTable("material_returns");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.QuantityReturned).HasPrecision(10, 3);
        builder.Property(r => r.Reason).IsRequired().HasMaxLength(500);
        builder.Property(r => r.BatchNumber).HasMaxLength(50);
        builder.Property(r => r.SerialNumber).HasMaxLength(50);
    }
}

public sealed class MaterialReservationConfiguration : IEntityTypeConfiguration<MaterialReservation>
{
    public void Configure(EntityTypeBuilder<MaterialReservation> builder)
    {
        builder.ToTable("material_reservations");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.QuantityReserved).HasPrecision(10, 3);
        builder.Property(r => r.QuantityIssued).HasPrecision(10, 3);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);

        builder.Ignore(r => r.QuantityOutstanding);
    }
}

public sealed class MaterialIssueConfiguration : IEntityTypeConfiguration<MaterialIssue>
{
    public void Configure(EntityTypeBuilder<MaterialIssue> builder)
    {
        builder.ToTable("material_issues");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.QuantityIssued).HasPrecision(10, 3);
        builder.Property(i => i.BatchNumber).HasMaxLength(50);
        builder.Property(i => i.SerialNumber).HasMaxLength(50);
        builder.Property(i => i.IssueSlipRef).HasMaxLength(30);
    }
}
