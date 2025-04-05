using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartUni.PublicApi.Migrations
{
    /// <inheritdoc />
    public partial class first_login_flag_to_asp_net_user_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_first_login",
                table: "asp_net_user",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "asp_net_user",
                keyColumn: "id",
                keyValue: new Guid("8edcd6b3-0489-4766-abed-284e8945f13d"),
                column: "is_first_login",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_first_login",
                table: "asp_net_user");
        }
    }
}
