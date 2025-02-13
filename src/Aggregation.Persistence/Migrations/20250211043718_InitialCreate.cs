using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aggregation.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "WeatherForecasts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                City = table.Column<string>(type: "text", nullable: false),
                FeedProvider = table.Column<string>(type: "text", nullable: false),
                Date = table.Column<string>(type: "character varying(10)", nullable: false),
                Temperature = table.Column<string>(type: "text", nullable: false),
                Summary = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WeatherForecasts", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_WeatherForecasts_City_Date_FeedProvider",
            table: "WeatherForecasts",
            columns: ["City", "Date", "FeedProvider"]);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(
            name: "WeatherForecasts");
}
