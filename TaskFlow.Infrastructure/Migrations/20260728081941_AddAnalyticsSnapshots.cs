using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "analytics_daily_snapshots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    snapshot_date = table.Column<DateOnly>(type: "date", nullable: false),
                    tasks_created = table.Column<int>(type: "integer", nullable: false),
                    tasks_completed = table.Column<int>(type: "integer", nullable: false),
                    avg_completion_hours = table.Column<decimal>(type: "numeric(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_analytics_daily_snapshots", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_analytics_daily_snapshots_organization_id_project_id_snapsh",
                table: "analytics_daily_snapshots",
                columns: new[] { "organization_id", "project_id", "snapshot_date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analytics_daily_snapshots");
        }
    }
}
