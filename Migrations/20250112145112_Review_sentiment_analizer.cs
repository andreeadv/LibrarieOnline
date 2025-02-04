using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibrarieOnline.Migrations
{
    /// <inheritdoc />
    public partial class Review_sentiment_analizer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sentiment",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sentiment",
                table: "Comments");
        }
    }
}
