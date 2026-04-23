using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrossfitCoach.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddExercises : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Force = table.Column<string>(type: "text", nullable: true),
                    Level = table.Column<string>(type: "text", nullable: true),
                    Mechanic = table.Column<string>(type: "text", nullable: true),
                    Equipment = table.Column<string>(type: "text", nullable: true),
                    PrimaryMuscles = table.Column<string[]>(type: "text[]", nullable: false),
                    SecondaryMuscles = table.Column<string[]>(type: "text[]", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    IsCrossFit = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exercises");
        }
    }
}
