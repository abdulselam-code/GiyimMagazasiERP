using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiyimMagazasiERP.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceCoreFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FaturaNo",
                table: "Satislar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FaturaSeri",
                table: "Satislar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "FAT");

            migrationBuilder.AddColumn<int>(
                name: "FaturaSiraNo",
                table: "Satislar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FaturaTarihi",
                table: "Satislar",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<string>(
                name: "BelgeTuru",
                table: "Satislar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "SatisBelgesi");

            migrationBuilder.AddColumn<string>(
                name: "FaturaDurumu",
                table: "Satislar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Olusturuldu");

            migrationBuilder.AddColumn<string>(
                name: "UUID",
                table: "Satislar",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MusteriTipi",
                table: "Musteriler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Bireysel");

            migrationBuilder.AddColumn<string>(
                name: "KurumsalUnvan",
                table: "Musteriler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Adres",
                table: "Musteriler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Il",
                table: "Musteriler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ilce",
                table: "Musteriler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TCKN",
                table: "Musteriler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VKN",
                table: "Musteriler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VergiDairesi",
                table: "Musteriler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TicariUnvan",
                table: "MagazaBilgileri",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Il",
                table: "MagazaBilgileri",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ilce",
                table: "MagazaBilgileri",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebAdresi",
                table: "MagazaBilgileri",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MersisNo",
                table: "MagazaBilgileri",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TicaretSicilNo",
                table: "MagazaBilgileri",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE Satislar
SET 
    FaturaSeri = 'FAT',
    FaturaSiraNo = Id,
    FaturaNo = 'FAT-' + RIGHT('000000' + CAST(Id AS varchar(20)), 6),
    FaturaTarihi = SatisTarihi,
    BelgeTuru = 'SatisBelgesi',
    FaturaDurumu = 'Olusturuldu'
WHERE FaturaNo IS NULL OR FaturaNo = '';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FaturaNo",
                table: "Satislar");

            migrationBuilder.DropColumn(
                name: "FaturaSeri",
                table: "Satislar");

            migrationBuilder.DropColumn(
                name: "FaturaSiraNo",
                table: "Satislar");

            migrationBuilder.DropColumn(
                name: "FaturaTarihi",
                table: "Satislar");

            migrationBuilder.DropColumn(
                name: "BelgeTuru",
                table: "Satislar");

            migrationBuilder.DropColumn(
                name: "FaturaDurumu",
                table: "Satislar");

            migrationBuilder.DropColumn(
                name: "UUID",
                table: "Satislar");

            migrationBuilder.DropColumn(
                name: "MusteriTipi",
                table: "Musteriler");

            migrationBuilder.DropColumn(
                name: "KurumsalUnvan",
                table: "Musteriler");

            migrationBuilder.DropColumn(
                name: "Adres",
                table: "Musteriler");

            migrationBuilder.DropColumn(
                name: "Il",
                table: "Musteriler");

            migrationBuilder.DropColumn(
                name: "Ilce",
                table: "Musteriler");

            migrationBuilder.DropColumn(
                name: "TCKN",
                table: "Musteriler");

            migrationBuilder.DropColumn(
                name: "VKN",
                table: "Musteriler");

            migrationBuilder.DropColumn(
                name: "VergiDairesi",
                table: "Musteriler");

            migrationBuilder.DropColumn(
                name: "TicariUnvan",
                table: "MagazaBilgileri");

            migrationBuilder.DropColumn(
                name: "Il",
                table: "MagazaBilgileri");

            migrationBuilder.DropColumn(
                name: "Ilce",
                table: "MagazaBilgileri");

            migrationBuilder.DropColumn(
                name: "WebAdresi",
                table: "MagazaBilgileri");

            migrationBuilder.DropColumn(
                name: "MersisNo",
                table: "MagazaBilgileri");

            migrationBuilder.DropColumn(
                name: "TicaretSicilNo",
                table: "MagazaBilgileri");
        }
    }
}