using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SmartUni.PublicApi.Common.Domain;

#nullable disable

namespace SmartUni.PublicApi.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
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
                .Annotation("Npgsql:Enum:role_type", "staff,student,tutor");

            migrationBuilder.CreateTable(
                name: "asp_net_role",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_login_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_first_login = table.Column<bool>(type: "boolean", nullable: false),
                    role = table.Column<Enums.RoleType>(type: "role_type", nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "blog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    cover_image = table.Column<byte[]>(type: "bytea", nullable: true),
                    attachment = table.Column<byte[]>(type: "bytea", nullable: true),
                    attachment_name = table.Column<string>(type: "text", nullable: true),
                    author_name = table.Column<string>(type: "text", nullable: false),
                    author_avatar = table.Column<byte[]>(type: "bytea", nullable: true),
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
                name: "chatmessage",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_id = table.Column<string>(type: "text", nullable: false),
                    sender_name = table.Column<string>(type: "text", nullable: false),
                    sender_type = table.Column<int>(type: "integer", nullable: false),
                    chat_room_id = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chatmessage", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "chatparticipant",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    chat_room_id = table.Column<string>(type: "text", nullable: false),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chatparticipant", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "page",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    page_name = table.Column<int>(type: "integer", nullable: false),
                    view_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_page", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_role_claim",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_role_claim", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_role_claim_asp_net_role_role_id",
                        column: x => x.role_id,
                        principalTable: "asp_net_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_claim",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_claim", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_user_claim_asp_net_user_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_login",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_login", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_asp_net_user_login_asp_net_user_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_role",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_role", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_asp_net_user_role_asp_net_role_role_id",
                        column: x => x.role_id,
                        principalTable: "asp_net_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_asp_net_user_role_asp_net_user_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_token",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_token", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_asp_net_user_token_asp_net_user_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staff",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    gender = table.Column<Enums.GenderType>(type: "gender", nullable: false),
                    identity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_staff", x => x.id);
                    table.ForeignKey(
                        name: "fk_staff_users_identity_id",
                        column: x => x.identity_id,
                        principalTable: "asp_net_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    gender = table.Column<Enums.GenderType>(type: "gender", nullable: false),
                    major = table.Column<Enums.MajorType>(type: "major", nullable: false),
                    identity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_student", x => x.id);
                    table.ForeignKey(
                        name: "fk_student_users_identity_id",
                        column: x => x.identity_id,
                        principalTable: "asp_net_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tutor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    gender = table.Column<Enums.GenderType>(type: "gender", nullable: false),
                    major = table.Column<Enums.MajorType>(type: "major", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    identity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tutor", x => x.id);
                    table.ForeignKey(
                        name: "fk_tutor_users_identity_id",
                        column: x => x.identity_id,
                        principalTable: "asp_net_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "blogcomment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    blog_id = table.Column<Guid>(type: "uuid", nullable: false),
                    commenter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: false),
                    commented_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "blogreaction",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    blog_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reacter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reacted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "allocation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tutor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_allocation", x => x.id);
                    table.ForeignKey(
                        name: "fk_allocation_student_student_id",
                        column: x => x.student_id,
                        principalTable: "student",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "meeting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<Enums.MeetingStatus>(type: "meeting_status", nullable: false),
                    organizer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_online = table.Column<bool>(type: "boolean", nullable: false),
                    location = table.Column<string>(type: "text", nullable: true),
                    link_type = table.Column<Enums.MeetingLinkType>(type: "meeting_link_type", nullable: true),
                    url = table.Column<string>(type: "text", nullable: true),
                    agenda = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_meeting", x => x.id);
                    table.ForeignKey(
                        name: "fk_meeting_tutor_organizer_id",
                        column: x => x.organizer_id,
                        principalTable: "tutor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "meetingparticipant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    meeting_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attendance = table.Column<Enums.AttendanceStatus>(type: "attendance_status", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_meetingparticipant", x => x.id);
                    table.ForeignKey(
                        name: "fk_meetingparticipant_meeting_meeting_id",
                        column: x => x.meeting_id,
                        principalTable: "meeting",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_meetingparticipant_student_student_id",
                        column: x => x.student_id,
                        principalTable: "student",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "asp_net_user",
                columns: new[] { "id", "access_failed_count", "concurrency_stamp", "email", "email_confirmed", "is_first_login", "last_login_date", "lockout_enabled", "lockout_end", "normalized_email", "normalized_user_name", "password_hash", "phone_number", "phone_number_confirmed", "role", "security_stamp", "two_factor_enabled", "user_name" },
                values: new object[] { new Guid("8edcd6b3-0489-4766-abed-284e8945f13d"), 0, "eba2f237-2092-401e-9c31-3371ff170cdf", "super@gmail.com", false, false, null, false, null, "SUPER@GMAIL.COM", "super@gmail.com", "AQAAAAIAAYagAAAAEBO76UEQJKnMJnRWMaqsAZS3Qbuua1nQ47HoHOEDwe20rlsfO42Eqt1o58vU539ZhA==", "0948827282", false, Enums.RoleType.Staff, null, false, "super@gmail.com" });

            migrationBuilder.InsertData(
                table: "staff",
                columns: new[] { "id", "created_by", "created_on", "gender", "identity_id", "image", "is_deleted", "name", "updated_by", "updated_on" },
                values: new object[] { new Guid("8fb67550-b862-4a0f-94fd-c212f5e35802"), new Guid("8fb67550-b862-4a0f-94fd-c212f5e35802"), new DateTime(2025, 3, 16, 17, 0, 0, 0, DateTimeKind.Utc), Enums.GenderType.Male, new Guid("8edcd6b3-0489-4766-abed-284e8945f13d"), null, false, "super staff", null, null });

            migrationBuilder.CreateIndex(
                name: "ix_allocation_student_id",
                table: "allocation",
                column: "student_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "asp_net_role",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_role_claim_role_id",
                table: "asp_net_role_claim",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "asp_net_user",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "asp_net_user",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_claim_user_id",
                table: "asp_net_user_claim",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_login_user_id",
                table: "asp_net_user_login",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_role_role_id",
                table: "asp_net_user_role",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_blogcomment_blog_id",
                table: "blogcomment",
                column: "blog_id");

            migrationBuilder.CreateIndex(
                name: "ix_blogcomment_commenter_id",
                table: "blogcomment",
                column: "commenter_id");

            migrationBuilder.CreateIndex(
                name: "ix_blogreaction_blog_id_reacter_id",
                table: "blogreaction",
                columns: new[] { "blog_id", "reacter_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_blogreaction_reacter_id",
                table: "blogreaction",
                column: "reacter_id");

            migrationBuilder.CreateIndex(
                name: "ix_meeting_organizer_id_start_time",
                table: "meeting",
                columns: new[] { "organizer_id", "start_time" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_meetingparticipant_meeting_id",
                table: "meetingparticipant",
                column: "meeting_id");

            migrationBuilder.CreateIndex(
                name: "ix_meetingparticipant_student_id_meeting_id",
                table: "meetingparticipant",
                columns: new[] { "student_id", "meeting_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_staff_identity_id",
                table: "staff",
                column: "identity_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_student_identity_id",
                table: "student",
                column: "identity_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tutor_identity_id",
                table: "tutor",
                column: "identity_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "allocation");

            migrationBuilder.DropTable(
                name: "asp_net_role_claim");

            migrationBuilder.DropTable(
                name: "asp_net_user_claim");

            migrationBuilder.DropTable(
                name: "asp_net_user_login");

            migrationBuilder.DropTable(
                name: "asp_net_user_role");

            migrationBuilder.DropTable(
                name: "asp_net_user_token");

            migrationBuilder.DropTable(
                name: "blogcomment");

            migrationBuilder.DropTable(
                name: "blogreaction");

            migrationBuilder.DropTable(
                name: "chatmessage");

            migrationBuilder.DropTable(
                name: "chatparticipant");

            migrationBuilder.DropTable(
                name: "meetingparticipant");

            migrationBuilder.DropTable(
                name: "page");

            migrationBuilder.DropTable(
                name: "staff");

            migrationBuilder.DropTable(
                name: "asp_net_role");

            migrationBuilder.DropTable(
                name: "blog");

            migrationBuilder.DropTable(
                name: "meeting");

            migrationBuilder.DropTable(
                name: "student");

            migrationBuilder.DropTable(
                name: "tutor");

            migrationBuilder.DropTable(
                name: "asp_net_user");
        }
    }
}
