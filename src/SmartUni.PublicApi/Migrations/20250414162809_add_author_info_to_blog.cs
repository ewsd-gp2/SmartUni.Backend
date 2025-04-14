using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartUni.PublicApi.Migrations
{
    /// <inheritdoc />
    public partial class add_author_info_to_blog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "author_avatar",
                table: "blog",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "author_name",
                table: "blog",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "author_avatar",
                table: "blog");

            migrationBuilder.DropColumn(
                name: "author_name",
                table: "blog");
        }
    }
}
