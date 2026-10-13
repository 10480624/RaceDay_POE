using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "varchar(255)", nullable: false),
                    password_hash = table.Column<string>(type: "varchar(255)", nullable: false),
                    first_name = table.Column<string>(type: "varchar(100)", nullable: false),
                    last_name = table.Column<string>(type: "varchar(100)", nullable: false),
                    role = table.Column<string>(type: "varchar(20)", nullable: false, defaultValue: "Participant"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.user_id);
                    table.CheckConstraint("CHK_User_Role", "[role] IN ('Organiser', 'Participant')");
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    organiser_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(200)", nullable: false),
                    description = table.Column<string>(type: "varchar(1000)", nullable: true),
                    event_date = table.Column<DateTime>(type: "date", nullable: false),
                    location = table.Column<string>(type: "varchar(200)", nullable: false),
                    distance = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    event_type = table.Column<string>(type: "varchar(20)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.event_id);
                    table.CheckConstraint("CHK_Event_Type", "[event_type] IN ('run', 'walk', 'cycle')");
                    table.ForeignKey(
                        name: "FK_Events_Users_organiser_id",
                        column: x => x.organiser_id,
                        principalTable: "Users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    profile_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "date", nullable: true),
                    emergency_contact_name = table.Column<string>(type: "varchar(100)", nullable: true),
                    emergency_contact_phone = table.Column<string>(type: "varchar(20)", nullable: true),
                    medical_conditions = table.Column<string>(type: "varchar(500)", nullable: true),
                    profile_picture_url = table.Column<string>(type: "varchar(500)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.profile_id);
                    table.ForeignKey(
                        name: "FK_Profiles_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    event_id = table.Column<int>(type: "int", nullable: false),
                    category_name = table.Column<string>(type: "varchar(100)", nullable: false),
                    age_range = table.Column<string>(type: "varchar(50)", nullable: true),
                    distance = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.category_id);
                    table.ForeignKey(
                        name: "FK_Categories_Events_event_id",
                        column: x => x.event_id,
                        principalTable: "Events",
                        principalColumn: "event_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    notification_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    event_id = table.Column<int>(type: "int", nullable: true),
                    message = table.Column<string>(type: "varchar(500)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    is_read = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.notification_id);
                    table.ForeignKey(
                        name: "FK_Notifications_Events_event_id",
                        column: x => x.event_id,
                        principalTable: "Events",
                        principalColumn: "event_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enrolments",
                columns: table => new
                {
                    enrolment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    event_id = table.Column<int>(type: "int", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    enrolment_date = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    status = table.Column<string>(type: "varchar(20)", nullable: false, defaultValue: "Pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrolments", x => x.enrolment_id);
                    table.CheckConstraint("CHK_Enrolment_Status", "[status] IN ('Pending', 'Confirmed', 'Cancelled', 'Completed', 'Archived')");
                    table.ForeignKey(
                        name: "FK_Enrolments_Categories_category_id",
                        column: x => x.category_id,
                        principalTable: "Categories",
                        principalColumn: "category_id");
                    table.ForeignKey(
                        name: "FK_Enrolments_Events_event_id",
                        column: x => x.event_id,
                        principalTable: "Events",
                        principalColumn: "event_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Enrolments_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    result_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enrolment_id = table.Column<int>(type: "int", nullable: false),
                    finish_time = table.Column<TimeSpan>(type: "time", nullable: true),
                    finishing_position = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.result_id);
                    table.ForeignKey(
                        name: "FK_Results_Enrolments_enrolment_id",
                        column: x => x.enrolment_id,
                        principalTable: "Enrolments",
                        principalColumn: "enrolment_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_event_id",
                table: "Categories",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_category_id",
                table: "Enrolments",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_event_id",
                table: "Enrolments",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_user_id",
                table: "Enrolments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Events_organiser_id",
                table: "Events",
                column: "organiser_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_event_id",
                table: "Notifications",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_user_id",
                table: "Notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_user_id",
                table: "Profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Results_enrolment_id",
                table: "Results",
                column: "enrolment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_email",
                table: "Users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropTable(
                name: "Enrolments");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
