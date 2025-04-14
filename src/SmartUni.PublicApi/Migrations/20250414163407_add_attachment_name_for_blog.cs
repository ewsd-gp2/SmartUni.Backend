using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartUni.PublicApi.Migrations
{
    /// <inheritdoc />
    public partial class add_attachment_name_for_blog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "attachment_name",
                table: "blog",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "attachment_name",
                table: "blog");
        }
    }
}
