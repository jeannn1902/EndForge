using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectCreator.Data.Migrations
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreUsuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HashPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UltimoAcceso = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConcesionXPDbs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Clave = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CantidadXP = table.Column<int>(type: "int", nullable: false),
                    FechaUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PracticaId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemaId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConcesionXPDbs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConcesionXPDbs_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvaluacionDbs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    PracticaId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Calificacion = table.Column<double>(type: "float", nullable: false),
                    FechaEvaluacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Aprobada = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluacionDbs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluacionDbs_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgresoUsuarioDbs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    PracticaId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RutaProyecto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PuntosXP = table.Column<int>(type: "int", nullable: false),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgresoUsuarioDbs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgresoUsuarioDbs_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConcesionXPDbs_UsuarioId",
                table: "ConcesionXPDbs",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluacionDbs_UsuarioId",
                table: "EvaluacionDbs",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgresoUsuarioDbs_UsuarioId",
                table: "ProgresoUsuarioDbs",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConcesionXPDbs");

            migrationBuilder.DropTable(
                name: "EvaluacionDbs");

            migrationBuilder.DropTable(
                name: "ProgresoUsuarioDbs");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
