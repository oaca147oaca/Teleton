using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Repositorio.Migrations
{
    public partial class databaseNUEVAAA : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alcancias",
                columns: table => new
                {
                    AlcanciaId = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EsHabilitada = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alcancias", x => x.AlcanciaId);
                });

            migrationBuilder.CreateTable(
                name: "Campanias",
                columns: table => new
                {
                    CampaniaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinMVD = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinFB = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campanias", x => x.CampaniaId);
                });

            migrationBuilder.CreateTable(
                name: "Responsables",
                columns: table => new
                {
                    ResponsableId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Cedula = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Retira = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responsables", x => x.ResponsableId);
                });

            migrationBuilder.CreateTable(
                name: "Retiran",
                columns: table => new
                {
                    RetiraId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Cedula = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Retiran", x => x.RetiraId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RolId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RolId);
                });

            migrationBuilder.CreateTable(
                name: "TipoColaboradores",
                columns: table => new
                {
                    TipoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoColaboradores", x => x.TipoId);
                });

            migrationBuilder.CreateTable(
                name: "Voluntarios",
                columns: table => new
                {
                    VoluntarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Cedula = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fin = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voluntarios", x => x.VoluntarioId);
                });

            migrationBuilder.CreateTable(
                name: "AlcanciasExternas",
                columns: table => new
                {
                    AlcExtId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaniaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaDevolucion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MontoPesos = table.Column<int>(type: "int", nullable: true),
                    MontoDolares = table.Column<int>(type: "int", nullable: true),
                    NumeroTicket = table.Column<int>(type: "int", nullable: true),
                    Impreso = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlcanciasExternas", x => x.AlcExtId);
                    table.ForeignKey(
                        name: "FK_AlcanciasExternas_Campanias_CampaniaId",
                        column: x => x.CampaniaId,
                        principalTable: "Campanias",
                        principalColumn: "CampaniaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RolId = table.Column<int>(type: "int", nullable: false),
                    Contrasenia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioId);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "RolId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Colaboradores",
                columns: table => new
                {
                    ColaboradorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    RazonSocial = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Departamento = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Localidad = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    TipoColId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colaboradores", x => x.ColaboradorId);
                    table.ForeignKey(
                        name: "FK_Colaboradores_TipoColaboradores_TipoColId",
                        column: x => x.TipoColId,
                        principalTable: "TipoColaboradores",
                        principalColumn: "TipoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Semaforos",
                columns: table => new
                {
                    SemaforoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColaboradorId = table.Column<int>(type: "int", nullable: false),
                    CantAlcanciasDevueltas = table.Column<int>(type: "int", nullable: false),
                    CantAlcanciasSinDevolver = table.Column<int>(type: "int", nullable: false),
                    TotalCampanias = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semaforos", x => x.SemaforoId);
                    table.ForeignKey(
                        name: "FK_Semaforos_Colaboradores_ColaboradorId",
                        column: x => x.ColaboradorId,
                        principalTable: "Colaboradores",
                        principalColumn: "ColaboradorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Solicitud",
                columns: table => new
                {
                    SolicitudId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaniaId = table.Column<int>(type: "int", nullable: false),
                    ColaboradorId = table.Column<int>(type: "int", nullable: false),
                    ResponsableId = table.Column<int>(type: "int", nullable: false),
                    RetiraId = table.Column<int>(type: "int", nullable: false),
                    CantSolicitadas = table.Column<int>(type: "int", nullable: false),
                    CantEntregadas = table.Column<int>(type: "int", nullable: false),
                    CantDevueltas = table.Column<int>(type: "int", nullable: false),
                    LugarEntrega = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitud", x => x.SolicitudId);
                    table.ForeignKey(
                        name: "FK_Solicitud_Campanias_CampaniaId",
                        column: x => x.CampaniaId,
                        principalTable: "Campanias",
                        principalColumn: "CampaniaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Solicitud_Colaboradores_ColaboradorId",
                        column: x => x.ColaboradorId,
                        principalTable: "Colaboradores",
                        principalColumn: "ColaboradorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Solicitud_Responsables_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Responsables",
                        principalColumn: "ResponsableId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Solicitud_Retiran_RetiraId",
                        column: x => x.RetiraId,
                        principalTable: "Retiran",
                        principalColumn: "RetiraId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlcanciaSolicitudes",
                columns: table => new
                {
                    IdAlcancia = table.Column<string>(type: "nvarchar(300)", nullable: false),
                    IdSolicitud = table.Column<int>(type: "int", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumeroTicket = table.Column<int>(type: "int", nullable: false),
                    Impreso = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MontoPesos = table.Column<int>(type: "int", nullable: false),
                    MontoDolares = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlcanciaSolicitudes", x => new { x.IdAlcancia, x.IdSolicitud });
                    table.ForeignKey(
                        name: "FK_AlcanciaSolicitudes_Alcancias_IdAlcancia",
                        column: x => x.IdAlcancia,
                        principalTable: "Alcancias",
                        principalColumn: "AlcanciaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlcanciaSolicitudes_Solicitud_IdSolicitud",
                        column: x => x.IdSolicitud,
                        principalTable: "Solicitud",
                        principalColumn: "SolicitudId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comentarios",
                columns: table => new
                {
                    ComentarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comentarios", x => x.ComentarioId);
                    table.ForeignKey(
                        name: "FK_Comentarios_Solicitud_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitud",
                        principalColumn: "SolicitudId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlcanciasExternas_CampaniaId",
                table: "AlcanciasExternas",
                column: "CampaniaId");

            migrationBuilder.CreateIndex(
                name: "IX_AlcanciaSolicitudes_IdSolicitud",
                table: "AlcanciaSolicitudes",
                column: "IdSolicitud");

            migrationBuilder.CreateIndex(
                name: "IX_Colaboradores_TipoColId",
                table: "Colaboradores",
                column: "TipoColId");

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_SolicitudId",
                table: "Comentarios",
                column: "SolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_Semaforos_ColaboradorId",
                table: "Semaforos",
                column: "ColaboradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_CampaniaId",
                table: "Solicitud",
                column: "CampaniaId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_ColaboradorId",
                table: "Solicitud",
                column: "ColaboradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_ResponsableId",
                table: "Solicitud",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_RetiraId",
                table: "Solicitud",
                column: "RetiraId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolId",
                table: "Usuarios",
                column: "RolId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlcanciasExternas");

            migrationBuilder.DropTable(
                name: "AlcanciaSolicitudes");

            migrationBuilder.DropTable(
                name: "Comentarios");

            migrationBuilder.DropTable(
                name: "Semaforos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Voluntarios");

            migrationBuilder.DropTable(
                name: "Alcancias");

            migrationBuilder.DropTable(
                name: "Solicitud");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Campanias");

            migrationBuilder.DropTable(
                name: "Colaboradores");

            migrationBuilder.DropTable(
                name: "Responsables");

            migrationBuilder.DropTable(
                name: "Retiran");

            migrationBuilder.DropTable(
                name: "TipoColaboradores");
        }
    }
}
