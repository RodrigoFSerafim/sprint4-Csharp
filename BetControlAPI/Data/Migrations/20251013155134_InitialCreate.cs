using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BetControlAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Saldo = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Apostas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Valor = table.Column<decimal>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ganhou = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apostas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Apostas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Limites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorMaximoMensal = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorAtual = table.Column<decimal>(type: "TEXT", nullable: false),
                    MesReferencia = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Limites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Limites_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Apostas_UsuarioId",
                table: "Apostas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Limites_UsuarioId_MesReferencia",
                table: "Limites",
                columns: new[] { "UsuarioId", "MesReferencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Apostas");

            migrationBuilder.DropTable(
                name: "Limites");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
