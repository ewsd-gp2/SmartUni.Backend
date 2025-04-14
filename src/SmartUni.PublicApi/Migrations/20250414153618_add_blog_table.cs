using System;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartUni.PublicApi.Common.Domain;

#nullable disable

namespace SmartUni.PublicApi.Migrations
{
    /// <inheritdoc />
    public partial class add_blog_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:attendance_status", "absent,leave,present")
                .Annotation("Npgsql:Enum:blog_type", "announcement,knowledge_sharing,news_letter")
                .Annotation("Npgsql:Enum:gender", "female,male")
                .Annotation("Npgsql:Enum:major", "computing,information_systems,networking")
                .Annotation("Npgsql:Enum:meeting_link_type", "google_meet,microsoft_teams,zoom")
                .Annotation("Npgsql:Enum:meeting_status", "cancelled,completed,new")
                .Annotation("Npgsql:Enum:role_type", "staff,student,tutor")
                .OldAnnotation("Npgsql:Enum:attendance_status", "absent,leave,present")
                .OldAnnotation("Npgsql:Enum:gender", "female,male")
                .OldAnnotation("Npgsql:Enum:major", "computing,information_systems,networking")
                .OldAnnotation("Npgsql:Enum:meeting_link_type", "google_meet,microsoft_teams,zoom")
                .OldAnnotation("Npgsql:Enum:meeting_status", "cancelled,completed,new")
                .OldAnnotation("Npgsql:Enum:role_type", "staff,student,tutor");

            migrationBuilder.CreateTable(
                name: "blog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    cover_image = table.Column<byte[]>(type: "bytea", nullable: true),
                    attachment = table.Column<byte[]>(type: "bytea", nullable: true),
                    type = table.Column<Enums.BlogType>(type: "blog_type", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "blogcomment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    commenter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: false),
                    commented_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    blog_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blogcomment", x => x.id);
                    table.ForeignKey(
                        name: "fk_blogcomment_baseuser_commenter_id",
                        column: x => x.commenter_id,
                        principalTable: "asp_net_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_blogcomment_blog_blog_id",
                        column: x => x.blog_id,
                        principalTable: "blog",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "blogreaction",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reacter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reacted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    blog_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blogreaction", x => x.id);
                    table.ForeignKey(
                        name: "fk_blogreaction_baseuser_reacter_id",
                        column: x => x.reacter_id,
                        principalTable: "asp_net_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_blogreaction_blog_blog_id",
                        column: x => x.blog_id,
                        principalTable: "blog",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_blogcomment_blog_id",
                table: "blogcomment",
                column: "blog_id");

            migrationBuilder.CreateIndex(
                name: "ix_blogcomment_commenter_id",
                table: "blogcomment",
                column: "commenter_id");

            migrationBuilder.CreateIndex(
                name: "ix_blogreaction_blog_id",
                table: "blogreaction",
                column: "blog_id");

            migrationBuilder.CreateIndex(
                name: "ix_blogreaction_reacter_id",
                table: "blogreaction",
                column: "reacter_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blogcomment");

            migrationBuilder.DropTable(
                name: "blogreaction");

            migrationBuilder.DropTable(
                name: "blog");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:attendance_status", "absent,leave,present")
                .Annotation("Npgsql:Enum:gender", "female,male")
                .Annotation("Npgsql:Enum:major", "computing,information_systems,networking")
                .Annotation("Npgsql:Enum:meeting_link_type", "google_meet,microsoft_teams,zoom")
                .Annotation("Npgsql:Enum:meeting_status", "cancelled,completed,new")
                .Annotation("Npgsql:Enum:role_type", "staff,student,tutor")
                .OldAnnotation("Npgsql:Enum:attendance_status", "absent,leave,present")
                .OldAnnotation("Npgsql:Enum:blog_type", "announcement,knowledge_sharing,news_letter")
                .OldAnnotation("Npgsql:Enum:gender", "female,male")
                .OldAnnotation("Npgsql:Enum:major", "computing,information_systems,networking")
                .OldAnnotation("Npgsql:Enum:meeting_link_type", "google_meet,microsoft_teams,zoom")
                .OldAnnotation("Npgsql:Enum:meeting_status", "cancelled,completed,new")
                .OldAnnotation("Npgsql:Enum:role_type", "staff,student,tutor");
        }
    }
}
