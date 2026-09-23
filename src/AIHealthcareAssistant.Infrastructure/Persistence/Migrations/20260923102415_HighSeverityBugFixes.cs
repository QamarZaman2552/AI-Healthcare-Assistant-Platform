using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIHealthcareAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HighSeverityBugFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIConversations_Patients_PatientId",
                table: "AIConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Users_UserId",
                table: "Doctors");

            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Users_UserId",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Specialties_Name",
                table: "Specialties");

            migrationBuilder.DropIndex(
                name: "IX_Patients_UserId",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_PatientIntakes_AIConversationId",
                table: "PatientIntakes");

            migrationBuilder.DropIndex(
                name: "IX_PatientIntakes_AppointmentId",
                table: "PatientIntakes");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_LicenseNumber",
                table: "Doctors");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_UserId",
                table: "Doctors");

            migrationBuilder.DropIndex(
                name: "IX_AIConversations_AppointmentId",
                table: "AIConversations");

            migrationBuilder.DropIndex(
                name: "IX_AdminUsers_UserId",
                table: "AdminUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Specialties_Name",
                table: "Specialties",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_UserId",
                table: "Patients",
                column: "UserId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakes_AIConversationId",
                table: "PatientIntakes",
                column: "AIConversationId",
                unique: true,
                filter: "[AIConversationId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakes_AppointmentId",
                table: "PatientIntakes",
                column: "AppointmentId",
                unique: true,
                filter: "[AppointmentId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_LicenseNumber",
                table: "Doctors",
                column: "LicenseNumber",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_UserId",
                table: "Doctors",
                column: "UserId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AIConversations_AppointmentId",
                table: "AIConversations",
                column: "AppointmentId",
                unique: true,
                filter: "[AppointmentId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_UserId",
                table: "AdminUsers",
                column: "UserId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_AIConversations_Patients_PatientId",
                table: "AIConversations",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Users_UserId",
                table: "Doctors",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Users_UserId",
                table: "Patients",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIConversations_Patients_PatientId",
                table: "AIConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Users_UserId",
                table: "Doctors");

            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Users_UserId",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Specialties_Name",
                table: "Specialties");

            migrationBuilder.DropIndex(
                name: "IX_Patients_UserId",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_PatientIntakes_AIConversationId",
                table: "PatientIntakes");

            migrationBuilder.DropIndex(
                name: "IX_PatientIntakes_AppointmentId",
                table: "PatientIntakes");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_LicenseNumber",
                table: "Doctors");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_UserId",
                table: "Doctors");

            migrationBuilder.DropIndex(
                name: "IX_AIConversations_AppointmentId",
                table: "AIConversations");

            migrationBuilder.DropIndex(
                name: "IX_AdminUsers_UserId",
                table: "AdminUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Specialties_Name",
                table: "Specialties",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_UserId",
                table: "Patients",
                column: "UserId",
                unique: true);

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
                name: "IX_Doctors_LicenseNumber",
                table: "Doctors",
                column: "LicenseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_UserId",
                table: "Doctors",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIConversations_AppointmentId",
                table: "AIConversations",
                column: "AppointmentId",
                unique: true,
                filter: "[AppointmentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_UserId",
                table: "AdminUsers",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AIConversations_Patients_PatientId",
                table: "AIConversations",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Users_UserId",
                table: "Doctors",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Users_UserId",
                table: "Patients",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
