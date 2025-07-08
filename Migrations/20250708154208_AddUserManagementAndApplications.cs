using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeMatcher.Migrations
{
    /// <inheritdoc />
    public partial class AddUserManagementAndApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApplicationDeadline",
                table: "JobPosts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Company",
                table: "JobPosts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExperienceLevel",
                table: "JobPosts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "JobPosts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JobType",
                table: "JobPosts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "JobPosts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostedBy",
                table: "JobPosts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequiredSkills",
                table: "JobPosts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalaryRange",
                table: "JobPosts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "JobPosts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Education",
                table: "CVs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtractedSkills",
                table: "CVs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "QualityScore",
                table: "CVs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "CVs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "CVs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearsOfExperience",
                table: "CVs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Company = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Analytics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MetricType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MetricName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    Dimensions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobPostId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analytics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Analytics_JobPosts_JobPostId",
                        column: x => x.JobPostId,
                        principalTable: "JobPosts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Analytics_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "JobApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobPostId = table.Column<int>(type: "int", nullable: false),
                    CVId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MatchScore = table.Column<float>(type: "real", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HRFeedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InterviewScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InterviewNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsShortlisted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobApplications_CVs_CVId",
                        column: x => x.CVId,
                        principalTable: "CVs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobApplications_JobPosts_JobPostId",
                        column: x => x.JobPostId,
                        principalTable: "JobPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobApplications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobPosts_IsActive",
                table: "JobPosts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_JobPosts_PostedAt",
                table: "JobPosts",
                column: "PostedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JobPosts_UserId",
                table: "JobPosts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CVs_UserId",
                table: "CVs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Analytics_JobPostId",
                table: "Analytics",
                column: "JobPostId");

            migrationBuilder.CreateIndex(
                name: "IX_Analytics_UserId",
                table: "Analytics",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_AppliedAt",
                table: "JobApplications",
                column: "AppliedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_CVId",
                table: "JobApplications",
                column: "CVId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobPostId",
                table: "JobApplications",
                column: "JobPostId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_Status",
                table: "JobApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_UserId",
                table: "JobApplications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CVs_Users_UserId",
                table: "CVs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosts_Users_UserId",
                table: "JobPosts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CVs_Users_UserId",
                table: "CVs");

            migrationBuilder.DropForeignKey(
                name: "FK_JobPosts_Users_UserId",
                table: "JobPosts");

            migrationBuilder.DropTable(
                name: "Analytics");

            migrationBuilder.DropTable(
                name: "JobApplications");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_JobPosts_IsActive",
                table: "JobPosts");

            migrationBuilder.DropIndex(
                name: "IX_JobPosts_PostedAt",
                table: "JobPosts");

            migrationBuilder.DropIndex(
                name: "IX_JobPosts_UserId",
                table: "JobPosts");

            migrationBuilder.DropIndex(
                name: "IX_CVs_UserId",
                table: "CVs");

            migrationBuilder.DropColumn(
                name: "ApplicationDeadline",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "Company",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "ExperienceLevel",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "JobType",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "PostedBy",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "RequiredSkills",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "SalaryRange",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "Education",
                table: "CVs");

            migrationBuilder.DropColumn(
                name: "ExtractedSkills",
                table: "CVs");

            migrationBuilder.DropColumn(
                name: "QualityScore",
                table: "CVs");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "CVs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CVs");

            migrationBuilder.DropColumn(
                name: "YearsOfExperience",
                table: "CVs");
        }
    }
}
