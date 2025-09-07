using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TunCRM.Migrations
{
    /// <inheritdoc />
    public partial class AddGorevler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Gorevler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Baslik = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    Oncelik = table.Column<int>(type: "INTEGER", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TamamlanmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AtananKullaniciId = table.Column<int>(type: "INTEGER", nullable: true),
                    OlusturanKullaniciId = table.Column<int>(type: "INTEGER", nullable: true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: true),
                    FirsatId = table.Column<int>(type: "INTEGER", nullable: true),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gorevler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gorevler_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Gorevler_Firsatlar_FirsatId",
                        column: x => x.FirsatId,
                        principalTable: "Firsatlar",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Gorevler_Kullanicilar_AtananKullaniciId",
                        column: x => x.AtananKullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Gorevler_Kullanicilar_OlusturanKullaniciId",
                        column: x => x.OlusturanKullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gorevler_AtananKullaniciId",
                table: "Gorevler",
                column: "AtananKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Gorevler_FirmaId",
                table: "Gorevler",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Gorevler_FirsatId",
                table: "Gorevler",
                column: "FirsatId");

            migrationBuilder.CreateIndex(
                name: "IX_Gorevler_OlusturanKullaniciId",
                table: "Gorevler",
                column: "OlusturanKullaniciId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Gorevler");
        }
    }
}
