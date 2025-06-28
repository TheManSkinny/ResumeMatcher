using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeMatcher.Migrations
{
    /// <inheritdoc />
    public partial class AddCVModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobPosts");

            migrationBuilder.DropTable(
                name: "MatchResults");

            migrationBuilder.RenameColumn(
                name: "ParsedText",
                table: "CVs",
                newName: "RawText");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "CVs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "CVs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "CVs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "CVs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "CVs");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "CVs");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "CVs");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "CVs");

            migrationBuilder.RenameColumn(
                name: "RawText",
                table: "CVs",
                newName: "ParsedText");

            migrationBuilder.CreateTable(
                name: "JobPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPosts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MatchResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CVId = table.Column<int>(type: "int", nullable: false),
                    JobPostId = table.Column<int>(type: "int", nullable: false),
                    MatchScore = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchResults", x => x.Id);
                });
        }
    }
}
