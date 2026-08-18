using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taskify.Migrations
{
    /// <inheritdoc />
    public partial class AddEnumCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_TeamMembers_Role",
                table: "TeamMembers",
                sql: "\"Role\" IN (0, 1, 2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TaskItems_Status",
                table: "TaskItems",
                sql: "\"Status\" IN (0, 1, 2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Projects_Status",
                table: "Projects",
                sql: "\"Status\" IN (0, 1, 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TeamMembers_Role",
                table: "TeamMembers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TaskItems_Status",
                table: "TaskItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Projects_Status",
                table: "Projects");
        }
    }
}
