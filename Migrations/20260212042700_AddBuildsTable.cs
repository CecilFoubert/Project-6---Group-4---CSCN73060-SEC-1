using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project_6___Group_4___CSCN73060_SEC_1.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Builds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CpuId = table.Column<int>(type: "int", nullable: true),
                    GpuId = table.Column<int>(type: "int", nullable: true),
                    MotherboardId = table.Column<int>(type: "int", nullable: true),
                    MemoryId = table.Column<int>(type: "int", nullable: true),
                    StorageId = table.Column<int>(type: "int", nullable: true),
                    CaseId = table.Column<int>(type: "int", nullable: true),
                    PowerSupplyId = table.Column<int>(type: "int", nullable: true),
                    CpuCoolerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Builds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Builds_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Builds_CpuCoolers_CpuCoolerId",
                        column: x => x.CpuCoolerId,
                        principalTable: "CpuCoolers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Builds_Cpus_CpuId",
                        column: x => x.CpuId,
                        principalTable: "Cpus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Builds_Gpus_GpuId",
                        column: x => x.GpuId,
                        principalTable: "Gpus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Builds_Memories_MemoryId",
                        column: x => x.MemoryId,
                        principalTable: "Memories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Builds_Motherboards_MotherboardId",
                        column: x => x.MotherboardId,
                        principalTable: "Motherboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Builds_PowerSupplies_PowerSupplyId",
                        column: x => x.PowerSupplyId,
                        principalTable: "PowerSupplies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Builds_Storages_StorageId",
                        column: x => x.StorageId,
                        principalTable: "Storages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Builds_CaseId",
                table: "Builds",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Builds_CpuCoolerId",
                table: "Builds",
                column: "CpuCoolerId");

            migrationBuilder.CreateIndex(
                name: "IX_Builds_CpuId",
                table: "Builds",
                column: "CpuId");

            migrationBuilder.CreateIndex(
                name: "IX_Builds_GpuId",
                table: "Builds",
                column: "GpuId");

            migrationBuilder.CreateIndex(
                name: "IX_Builds_MemoryId",
                table: "Builds",
                column: "MemoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Builds_MotherboardId",
                table: "Builds",
                column: "MotherboardId");

            migrationBuilder.CreateIndex(
                name: "IX_Builds_PowerSupplyId",
                table: "Builds",
                column: "PowerSupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_Builds_StorageId",
                table: "Builds",
                column: "StorageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Builds");
        }
    }
}
