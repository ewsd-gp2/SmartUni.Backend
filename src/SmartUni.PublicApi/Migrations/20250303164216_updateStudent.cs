using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartUni.PublicApi.Migrations
{
    /// <inheritdoc />
    public partial class updateStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_student_allocations_allocation_id",
                table: "student");

            migrationBuilder.DropIndex(
                name: "ix_student_allocation_id",
                table: "student");

            migrationBuilder.DropColumn(
                name: "allocation_id",
                table: "student");

            migrationBuilder.DropColumn(
                name: "allocation_id1",
                table: "student");

            migrationBuilder.CreateIndex(
                name: "ix_allocation_student_id",
                table: "allocation",
                column: "student_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_allocation_student_student_id",
                table: "allocation",
                column: "student_id",
                principalTable: "student",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_allocation_student_student_id",
                table: "allocation");

            migrationBuilder.DropIndex(
                name: "ix_allocation_student_id",
                table: "allocation");

            migrationBuilder.AddColumn<Guid>(
                name: "allocation_id",
                table: "student",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "allocation_id1",
                table: "student",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_student_allocation_id",
                table: "student",
                column: "allocation_id1");

            migrationBuilder.AddForeignKey(
                name: "fk_student_allocations_allocation_id",
                table: "student",
                column: "allocation_id1",
                principalTable: "allocation",
                principalColumn: "id");
        }
    }
}
