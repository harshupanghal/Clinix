using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinix.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FinaleTesting3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "InitialNotificationSent",
                table: "FollowUps",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitialNotificationSent",
                table: "FollowUps");
        }
    }
}
