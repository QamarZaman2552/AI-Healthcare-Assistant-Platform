using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIHealthcareAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DoctorStatusAndPatientIntakeEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientIntakes_Appointments_AppointmentId",
                table: "PatientIntakes");

            migrationBuilder.DropIndex(
                name: "IX_PatientIntakes_AppointmentId",
                table: "PatientIntakes");

            migrationBuilder.AlterColumn<Guid>(
                name: "AppointmentId",
                table: "PatientIntakes",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "AIConversationId",
                table: "PatientIntakes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AISummary",
                table: "PatientIntakes",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecommendedSpecialtyId",
                table: "PatientIntakes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PatientIntakes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Doctors",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakes_AIConversationId",
                table: "PatientIntakes",
                column: "AIConversationId",
                unique: true,
                filter: "[AIConversationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakes_AppointmentId",
                table: "PatientIntakes",
                column: "AppointmentId",
                unique: true,
                filter: "[AppointmentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakes_RecommendedSpecialtyId",
                table: "PatientIntakes",
                column: "RecommendedSpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_IsActive_IsVerified",
                table: "Doctors",
                columns: new[] { "IsActive", "IsVerified" });

            migrationBuilder.AddForeignKey(
                name: "FK_PatientIntakes_AIConversations_AIConversationId",
                table: "PatientIntakes",
                column: "AIConversationId",
                principalTable: "AIConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PatientIntakes_Appointments_AppointmentId",
                table: "PatientIntakes",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PatientIntakes_Specialties_RecommendedSpecialtyId",
                table: "PatientIntakes",
                column: "RecommendedSpecialtyId",
                principalTable: "Specialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientIntakes_AIConversations_AIConversationId",
                table: "PatientIntakes");

            migrationBuilder.DropForeignKey(
                name: "FK_PatientIntakes_Appointments_AppointmentId",
                table: "PatientIntakes");

            migrationBuilder.DropForeignKey(
                name: "FK_PatientIntakes_Specialties_RecommendedSpecialtyId",
                table: "PatientIntakes");

            migrationBuilder.DropIndex(
                name: "IX_PatientIntakes_AIConversationId",
                table: "PatientIntakes");

            migrationBuilder.DropIndex(
                name: "IX_PatientIntakes_AppointmentId",
                table: "PatientIntakes");

            migrationBuilder.DropIndex(
                name: "IX_PatientIntakes_RecommendedSpecialtyId",
                table: "PatientIntakes");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_IsActive_IsVerified",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "AIConversationId",
                table: "PatientIntakes");

            migrationBuilder.DropColumn(
                name: "AISummary",
                table: "PatientIntakes");

            migrationBuilder.DropColumn(
                name: "RecommendedSpecialtyId",
                table: "PatientIntakes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PatientIntakes");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Doctors");

            migrationBuilder.AlterColumn<Guid>(
                name: "AppointmentId",
                table: "PatientIntakes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakes_AppointmentId",
                table: "PatientIntakes",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PatientIntakes_Appointments_AppointmentId",
                table: "PatientIntakes",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
