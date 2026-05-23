using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace ProgrammingPractice_L19.Migrations
{
    /// <inheritdoc />
    public partial class InitializeMigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Boats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    Type = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    IsBusy = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Displacement = table.Column<double>(type: "double", nullable: false),
                    ConstructionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boats", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Fishermen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    FullName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "varchar(90)", maxLength: 90, nullable: false),
                    JobTitle = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fishermen", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Jars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Coordinate = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jars", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Voyages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    BoatId = table.Column<int>(type: "int", nullable: false),
                    VoyageNumber = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voyages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Voyages_Boats_BoatId",
                        column: x => x.BoatId,
                        principalTable: "Boats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VoyageFisherman",
                columns: table => new
                {
                    VoyageId = table.Column<int>(type: "int", nullable: false),
                    FishermanId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoyageFisherman", x => new { x.FishermanId, x.VoyageId });
                    table.ForeignKey(
                        name: "FK_VoyageFisherman_Fishermen_FishermanId",
                        column: x => x.FishermanId,
                        principalTable: "Fishermen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VoyageFisherman_Voyages_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "Voyages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VoyageJar",
                columns: table => new
                {
                    VoyageId = table.Column<int>(type: "int", nullable: false),
                    JarId = table.Column<int>(type: "int", nullable: false),
                    PeriodId = table.Column<int>(type: "int", nullable: false),
                    BoatArrive = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    BoatSillAway = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoyageJar", x => new { x.JarId, x.VoyageId, x.PeriodId });
                    table.ForeignKey(
                        name: "FK_VoyageJar_Jars_JarId",
                        column: x => x.JarId,
                        principalTable: "Jars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VoyageJar_Voyages_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "Voyages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FishGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    JarId = table.Column<int>(type: "int", nullable: false),
                    VoyageId = table.Column<int>(type: "int", nullable: false),
                    PeriodId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false),
                    Quality = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false),
                    Weight = table.Column<double>(type: "double", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FishGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FishGroups_VoyageJar_JarId_VoyageId_PeriodId",
                        columns: x => new { x.JarId, x.VoyageId, x.PeriodId },
                        principalTable: "VoyageJar",
                        principalColumns: new[] { "JarId", "VoyageId", "PeriodId" },
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FishGroups_JarId_VoyageId_PeriodId",
                table: "FishGroups",
                columns: new[] { "JarId", "VoyageId", "PeriodId" });

            migrationBuilder.CreateIndex(
                name: "IX_VoyageFisherman_VoyageId",
                table: "VoyageFisherman",
                column: "VoyageId");

            migrationBuilder.CreateIndex(
                name: "IX_VoyageJar_VoyageId",
                table: "VoyageJar",
                column: "VoyageId");

            migrationBuilder.CreateIndex(
                name: "IX_Voyages_BoatId",
                table: "Voyages",
                column: "BoatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FishGroups");

            migrationBuilder.DropTable(
                name: "VoyageFisherman");

            migrationBuilder.DropTable(
                name: "VoyageJar");

            migrationBuilder.DropTable(
                name: "Fishermen");

            migrationBuilder.DropTable(
                name: "Jars");

            migrationBuilder.DropTable(
                name: "Voyages");

            migrationBuilder.DropTable(
                name: "Boats");
        }
    }
}
