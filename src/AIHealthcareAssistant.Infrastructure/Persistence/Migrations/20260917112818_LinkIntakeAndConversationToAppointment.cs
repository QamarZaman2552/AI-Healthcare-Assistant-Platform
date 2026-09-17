using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIHealthcareAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LinkIntakeAndConversationToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MedicalHistory",
                table: "PatientProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AIConversationId",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PatientIntakeId",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MedicalHistory",
                table: "PatientProfiles");

            migrationBuilder.DropColumn(
                name: "AIConversationId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PatientIntakeId",
                table: "Appointments");
        }
    }
}
