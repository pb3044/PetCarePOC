using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetCarePlatform.Web.Migrations.HealthChecks
{
    /// <inheritdoc />
    public partial class InitialHealthChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uri = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DiscoveryService = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Executions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OnStateFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastExecuted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Uri = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DiscoveryService = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Executions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Failures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HealthCheckName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LastNotified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUpAndRunning = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Failures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HealthCheckExecutionEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HealthCheckExecutionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthCheckExecutionEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthCheckExecutionEntries_Executions_HealthCheckExecutionId",
                        column: x => x.HealthCheckExecutionId,
                        principalTable: "Executions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HealthCheckExecutionHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    On = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HealthCheckExecutionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthCheckExecutionHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthCheckExecutionHistories_Executions_HealthCheckExecutionId",
                        column: x => x.HealthCheckExecutionId,
                        principalTable: "Executions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthCheckExecutionEntries_HealthCheckExecutionId",
                table: "HealthCheckExecutionEntries",
                column: "HealthCheckExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthCheckExecutionHistories_HealthCheckExecutionId",
                table: "HealthCheckExecutionHistories",
                column: "HealthCheckExecutionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "Failures");

            migrationBuilder.DropTable(
                name: "HealthCheckExecutionEntries");

            migrationBuilder.DropTable(
                name: "HealthCheckExecutionHistories");

            migrationBuilder.DropTable(
                name: "Executions");
        }
    }
}
