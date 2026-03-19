using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Annie_API.Migrations
{
    /// <inheritdoc />
    public partial class FixModelMismatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Instructors",
                table: "Sessions",
                newName: "Instructor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Instructor",
                table: "Sessions",
                newName: "Instructors");
        }
    }
}
