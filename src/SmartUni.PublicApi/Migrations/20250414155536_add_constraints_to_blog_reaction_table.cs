using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartUni.PublicApi.Migrations
{
    /// <inheritdoc />
    public partial class add_constraints_to_blog_reaction_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_blogcomment_blog_blog_id",
                table: "blogcomment");

            migrationBuilder.DropForeignKey(
                name: "fk_blogreaction_blog_blog_id",
                table: "blogreaction");

            migrationBuilder.DropIndex(
                name: "ix_blogreaction_blog_id",
                table: "blogreaction");

            migrationBuilder.AlterColumn<Guid>(
                name: "blog_id",
                table: "blogreaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "blog_id",
                table: "blogcomment",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_blogreaction_blog_id_reacter_id",
                table: "blogreaction",
                columns: new[] { "blog_id", "reacter_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_blogcomment_blog_blog_id",
                table: "blogcomment",
                column: "blog_id",
                principalTable: "blog",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_blogreaction_blog_blog_id",
                table: "blogreaction",
                column: "blog_id",
                principalTable: "blog",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_blogcomment_blog_blog_id",
                table: "blogcomment");

            migrationBuilder.DropForeignKey(
                name: "fk_blogreaction_blog_blog_id",
                table: "blogreaction");

            migrationBuilder.DropIndex(
                name: "ix_blogreaction_blog_id_reacter_id",
                table: "blogreaction");

            migrationBuilder.AlterColumn<Guid>(
                name: "blog_id",
                table: "blogreaction",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "blog_id",
                table: "blogcomment",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "ix_blogreaction_blog_id",
                table: "blogreaction",
                column: "blog_id");

            migrationBuilder.AddForeignKey(
                name: "fk_blogcomment_blog_blog_id",
                table: "blogcomment",
                column: "blog_id",
                principalTable: "blog",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_blogreaction_blog_blog_id",
                table: "blogreaction",
                column: "blog_id",
                principalTable: "blog",
                principalColumn: "id");
        }
    }
}
