using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GIGLite.Auth.Migrations
{
    public partial class addEmployeeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateJoined",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Terminal",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "UserType",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(nullable: true),
                    EmployeeCode = table.Column<string>(nullable: true),
                    DateJoined = table.Column<DateTime>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    MiddleName = table.Column<string>(nullable: true),
                    Gender = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    Otp = table.Column<string>(nullable: true),
                    OtpIsUsed = table.Column<bool>(nullable: false),
                    TicketRemovalOtp = table.Column<string>(nullable: true),
                    TicketRemovalOtpIsUsed = table.Column<bool>(nullable: false),
                    OTPLastUsedDate = table.Column<DateTime>(nullable: true),
                    OtpNoOfTimeUsed = table.Column<int>(nullable: true),
                    EmployeePhoto = table.Column<string>(nullable: true),
                    NextOfKin = table.Column<string>(nullable: true),
                    NextOfKinPhone = table.Column<string>(nullable: true),
                    WalletId = table.Column<int>(nullable: true),
                    WalletNumber = table.Column<string>(nullable: true),
                    DepartmentId = table.Column<int>(nullable: true),
                    DepartmentName = table.Column<string>(nullable: true),
                    PartnerId = table.Column<int>(nullable: true),
                    PartnerName = table.Column<string>(nullable: true),
                    PositionId = table.Column<int>(nullable: true),
                    PositionName = table.Column<string>(nullable: true),
                    TerminalId = table.Column<int>(nullable: true),
                    TerminalName = table.Column<string>(nullable: true),
                    TotalSales = table.Column<decimal>(nullable: false),
                    TotalCashSales = table.Column<decimal>(nullable: false),
                    TotalExpenseSales = table.Column<decimal>(nullable: false),
                    TotalCashRemittance = table.Column<decimal>(nullable: false),
                    ReferralCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_Employees_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ApplicationUserId",
                table: "Employees",
                column: "ApplicationUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropColumn(
                name: "UserType",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "DateJoined",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Terminal",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
