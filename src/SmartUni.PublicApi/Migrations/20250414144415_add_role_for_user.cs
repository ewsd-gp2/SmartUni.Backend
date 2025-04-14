using System;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartUni.PublicApi.Common.Domain;

#nullable disable

namespace SmartUni.PublicApi.Migrations
{
    /// <inheritdoc />
    public partial class add_role_for_user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:attendance_status", "absent,leave,present")
                .Annotation("Npgsql:Enum:gender", "female,male")
                .Annotation("Npgsql:Enum:major", "computing,information_systems,networking")
                .Annotation("Npgsql:Enum:meeting_link_type", "google_meet,microsoft_teams,zoom")
                .Annotation("Npgsql:Enum:meeting_status", "cancelled,completed,new")
                .Annotation("Npgsql:Enum:role_type", "staff,student,tutor")
                .OldAnnotation("Npgsql:Enum:attendance_status", "absent,leave,present")
                .OldAnnotation("Npgsql:Enum:gender", "female,male")
                .OldAnnotation("Npgsql:Enum:major", "computing,information_systems,networking")
                .OldAnnotation("Npgsql:Enum:meeting_link_type", "google_meet,microsoft_teams,zoom")
                .OldAnnotation("Npgsql:Enum:meeting_status", "cancelled,completed,new");

            migrationBuilder.AddColumn<Enums.RoleType>(
                name: "role",
                table: "asp_net_user",
                type: "role_type",
                nullable: false,
                defaultValue: Enums.RoleType.Staff);

            migrationBuilder.UpdateData(
                table: "asp_net_user",
                keyColumn: "id",
                keyValue: new Guid("8edcd6b3-0489-4766-abed-284e8945f13d"),
                column: "role",
                value: Enums.RoleType.Staff);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "role",
                table: "asp_net_user");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:attendance_status", "absent,leave,present")
                .Annotation("Npgsql:Enum:gender", "female,male")
                .Annotation("Npgsql:Enum:major", "computing,information_systems,networking")
                .Annotation("Npgsql:Enum:meeting_link_type", "google_meet,microsoft_teams,zoom")
                .Annotation("Npgsql:Enum:meeting_status", "cancelled,completed,new")
                .OldAnnotation("Npgsql:Enum:attendance_status", "absent,leave,present")
                .OldAnnotation("Npgsql:Enum:gender", "female,male")
                .OldAnnotation("Npgsql:Enum:major", "computing,information_systems,networking")
                .OldAnnotation("Npgsql:Enum:meeting_link_type", "google_meet,microsoft_teams,zoom")
                .OldAnnotation("Npgsql:Enum:meeting_status", "cancelled,completed,new")
                .OldAnnotation("Npgsql:Enum:role_type", "staff,student,tutor");
        }
    }
}
