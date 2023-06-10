using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookify.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDefaultValueForEndDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "RentalCopies",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CAST(GETDATE() + 7 AS Date)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "RentalCopies",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CAST(GETDATE() + 7 AS Date)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}
