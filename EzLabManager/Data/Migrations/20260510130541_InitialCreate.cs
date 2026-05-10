using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzLabManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsumableItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoryName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ModelName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumableItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LabTechnicians",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EmployeeNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabTechnicians", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsumableInboundRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConsumableItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    BatchNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    InboundDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    InboundById = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumableInboundRecords", x => x.Id);
                    table.CheckConstraint("CK_ConsumableInboundRecords_Quantity_Positive", "Quantity > 0");
                    table.ForeignKey(
                        name: "FK_ConsumableInboundRecords_ConsumableItems_ConsumableItemId",
                        column: x => x.ConsumableItemId,
                        principalTable: "ConsumableItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsumableInboundRecords_LabTechnicians_InboundById",
                        column: x => x.InboundById,
                        principalTable: "LabTechnicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsumableOutboundRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InboundRecordId = table.Column<int>(type: "INTEGER", nullable: false),
                    BatchNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    OutboundDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OutboundById = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumableOutboundRecords", x => x.Id);
                    table.CheckConstraint("CK_ConsumableOutboundRecords_Quantity_Positive", "Quantity > 0");
                    table.ForeignKey(
                        name: "FK_ConsumableOutboundRecords_ConsumableInboundRecords_InboundRecordId",
                        column: x => x.InboundRecordId,
                        principalTable: "ConsumableInboundRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsumableOutboundRecords_LabTechnicians_OutboundById",
                        column: x => x.OutboundById,
                        principalTable: "LabTechnicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumableInboundRecords_ConsumableItemId",
                table: "ConsumableInboundRecords",
                column: "ConsumableItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumableInboundRecords_ConsumableItemId_BatchNumber_ExpirationDate",
                table: "ConsumableInboundRecords",
                columns: new[] { "ConsumableItemId", "BatchNumber", "ExpirationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumableInboundRecords_InboundById",
                table: "ConsumableInboundRecords",
                column: "InboundById");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumableItems_CategoryName_ModelName",
                table: "ConsumableItems",
                columns: new[] { "CategoryName", "ModelName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsumableOutboundRecords_BatchNumber_OutboundDate",
                table: "ConsumableOutboundRecords",
                columns: new[] { "BatchNumber", "OutboundDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumableOutboundRecords_InboundRecordId",
                table: "ConsumableOutboundRecords",
                column: "InboundRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumableOutboundRecords_OutboundById",
                table: "ConsumableOutboundRecords",
                column: "OutboundById");

            migrationBuilder.CreateIndex(
                name: "IX_LabTechnicians_EmployeeNumber",
                table: "LabTechnicians",
                column: "EmployeeNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumableOutboundRecords");

            migrationBuilder.DropTable(
                name: "ConsumableInboundRecords");

            migrationBuilder.DropTable(
                name: "ConsumableItems");

            migrationBuilder.DropTable(
                name: "LabTechnicians");
        }
    }
}
