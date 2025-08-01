using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagementSys.DL.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "SignatureId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPasswordResetDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresPasswordReset",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPasswordResetDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RequiresPasswordReset",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<Guid>(
                name: "SignatureId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
