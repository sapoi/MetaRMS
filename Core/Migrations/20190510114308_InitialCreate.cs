using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Core.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "applications",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    descriptor = table.Column<string>(nullable: false),
                    login_application_name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "data",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    application_id = table.Column<long>(nullable: false),
                    data = table.Column<string>(nullable: false),
                    dataset_id = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data", x => x.id);
                    table.ForeignKey(
                        name: "FK_data_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "rights",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    application_id = table.Column<long>(nullable: false),
                    data = table.Column<string>(nullable: false),
                    name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rights", x => x.id);
                    table.ForeignKey(
                        name: "FK_rights_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    application_id = table.Column<long>(nullable: false),
                    data = table.Column<string>(nullable: false),
                    language_id = table.Column<int>(nullable: false),
                    password_hash = table.Column<string>(nullable: false),
                    password_salt = table.Column<string>(nullable: false),
                    rights_id = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_users_rights_rights_id",
                        column: x => x.rights_id,
                        principalTable: "rights",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_data_application_id",
                table: "data",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_rights_application_id",
                table: "rights",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_application_id",
                table: "users",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_rights_id",
                table: "users",
                column: "rights_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "rights");

            migrationBuilder.DropTable(
                name: "applications");
        }
    }
}
