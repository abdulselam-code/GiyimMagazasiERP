using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiyimMagazasiERP.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesDetailTaxAndSnapshotFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "KdvOrani",
                table: "Urunler",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 20m);

            migrationBuilder.AddColumn<decimal>(
                name: "ToplamKdvTutari",
                table: "Satislar",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VergiDahilToplam",
                table: "Satislar",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VergiHaricToplam",
                table: "Satislar",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BarkodSnapshot",
                table: "SatisDetaylari",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BedenSnapshot",
                table: "SatisDetaylari",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "KdvOrani",
                table: "SatisDetaylari",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 20m);

            migrationBuilder.AddColumn<decimal>(
                name: "KdvTutari",
                table: "SatisDetaylari",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RenkSnapshot",
                table: "SatisDetaylari",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SatirIndirimTutari",
                table: "SatisDetaylari",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UrunAdiSnapshot",
                table: "SatisDetaylari",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "VergiDahilTutar",
                table: "SatisDetaylari",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VergiHaricTutar",
                table: "SatisDetaylari",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(@"
UPDATE Urunler
SET KdvOrani = 20
WHERE KdvOrani = 0;

UPDATE sd
SET
    UrunAdiSnapshot = ISNULL(u.UrunAdi, ''),
    BarkodSnapshot = ISNULL(u.Barkod, ''),
    BedenSnapshot = ISNULL(u.Beden, ''),
    RenkSnapshot = ISNULL(u.Renk, ''),
    KdvOrani = ISNULL(NULLIF(u.KdvOrani, 0), 20),
    SatirIndirimTutari = 0,
    VergiDahilTutar = sd.ToplamTutar,
    VergiHaricTutar = ROUND(
        sd.ToplamTutar / (1 + ISNULL(NULLIF(u.KdvOrani, 0), 20) / 100.0),
        2
    ),
    KdvTutari = sd.ToplamTutar - ROUND(
        sd.ToplamTutar / (1 + ISNULL(NULLIF(u.KdvOrani, 0), 20) / 100.0),
        2
    )
FROM SatisDetaylari sd
INNER JOIN Urunler u ON u.Id = sd.UrunId;

UPDATE s
SET
    VergiDahilToplam = s.NetTutar,
    VergiHaricToplam = ROUND(s.NetTutar / 1.20, 2),
    ToplamKdvTutari = s.NetTutar - ROUND(s.NetTutar / 1.20, 2)
FROM Satislar s;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KdvOrani",
                table: "Urunler");

            migrationBuilder.DropColumn(
                name: "ToplamKdvTutari",
                table: "Satislar");

            migrationBuilder.DropColumn(
                name: "VergiDahilToplam",
                table: "Satislar");

            migrationBuilder.DropColumn(
                name: "VergiHaricToplam",
                table: "Satislar");

            migrationBuilder.DropColumn(
                name: "BarkodSnapshot",
                table: "SatisDetaylari");

            migrationBuilder.DropColumn(
                name: "BedenSnapshot",
                table: "SatisDetaylari");

            migrationBuilder.DropColumn(
                name: "KdvOrani",
                table: "SatisDetaylari");

            migrationBuilder.DropColumn(
                name: "KdvTutari",
                table: "SatisDetaylari");

            migrationBuilder.DropColumn(
                name: "RenkSnapshot",
                table: "SatisDetaylari");

            migrationBuilder.DropColumn(
                name: "SatirIndirimTutari",
                table: "SatisDetaylari");

            migrationBuilder.DropColumn(
                name: "UrunAdiSnapshot",
                table: "SatisDetaylari");

            migrationBuilder.DropColumn(
                name: "VergiDahilTutar",
                table: "SatisDetaylari");

            migrationBuilder.DropColumn(
                name: "VergiHaricTutar",
                table: "SatisDetaylari");
        }
    }
}