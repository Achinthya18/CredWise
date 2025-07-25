﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CredWise.Migrations
{
    /// <inheritdoc />
    public partial class deletechanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KYC_APPROVALS_Admin_AdminId",
                table: "KYC_APPROVALS");

            migrationBuilder.DropForeignKey(
                name: "FK_KYC_APPROVALS_Customers_CustomerId",
                table: "KYC_APPROVALS");

            migrationBuilder.DropForeignKey(
                name: "FK_LOAN_APPLICATIONS_LoanProducts_LoanProductId",
                table: "LOAN_APPLICATIONS");

            migrationBuilder.DropForeignKey(
                name: "FK_REPAYMENTS_LOAN_APPLICATIONS_ApplicationId",
                table: "REPAYMENTS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_REPAYMENTS",
                table: "REPAYMENTS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KYC_APPROVALS",
                table: "KYC_APPROVALS");

            migrationBuilder.RenameTable(
                name: "REPAYMENTS",
                newName: "Repayments");

            migrationBuilder.RenameTable(
                name: "KYC_APPROVALS",
                newName: "KYC_APPROVAL");

            migrationBuilder.RenameIndex(
                name: "IX_REPAYMENTS_ApplicationId",
                table: "Repayments",
                newName: "IX_Repayments_ApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_KYC_APPROVALS_CustomerId",
                table: "KYC_APPROVAL",
                newName: "IX_KYC_APPROVAL_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_KYC_APPROVALS_AdminId",
                table: "KYC_APPROVAL",
                newName: "IX_KYC_APPROVAL_AdminId");

            migrationBuilder.AlterColumn<int>(
                name: "LoanProductId",
                table: "LOAN_APPLICATIONS",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Repayments",
                table: "Repayments",
                column: "RepaymentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KYC_APPROVAL",
                table: "KYC_APPROVAL",
                column: "KycID");

            migrationBuilder.AddForeignKey(
                name: "FK_KYC_APPROVAL_Admin_AdminId",
                table: "KYC_APPROVAL",
                column: "AdminId",
                principalTable: "Admin",
                principalColumn: "AdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_KYC_APPROVAL_Customers_CustomerId",
                table: "KYC_APPROVAL",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LOAN_APPLICATIONS_LoanProducts_LoanProductId",
                table: "LOAN_APPLICATIONS",
                column: "LoanProductId",
                principalTable: "LoanProducts",
                principalColumn: "LoanProductId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Repayments_LOAN_APPLICATIONS_ApplicationId",
                table: "Repayments",
                column: "ApplicationId",
                principalTable: "LOAN_APPLICATIONS",
                principalColumn: "ApplicationId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KYC_APPROVAL_Admin_AdminId",
                table: "KYC_APPROVAL");

            migrationBuilder.DropForeignKey(
                name: "FK_KYC_APPROVAL_Customers_CustomerId",
                table: "KYC_APPROVAL");

            migrationBuilder.DropForeignKey(
                name: "FK_LOAN_APPLICATIONS_LoanProducts_LoanProductId",
                table: "LOAN_APPLICATIONS");

            migrationBuilder.DropForeignKey(
                name: "FK_Repayments_LOAN_APPLICATIONS_ApplicationId",
                table: "Repayments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Repayments",
                table: "Repayments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KYC_APPROVAL",
                table: "KYC_APPROVAL");

            migrationBuilder.RenameTable(
                name: "Repayments",
                newName: "REPAYMENTS");

            migrationBuilder.RenameTable(
                name: "KYC_APPROVAL",
                newName: "KYC_APPROVALS");

            migrationBuilder.RenameIndex(
                name: "IX_Repayments_ApplicationId",
                table: "REPAYMENTS",
                newName: "IX_REPAYMENTS_ApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_KYC_APPROVAL_CustomerId",
                table: "KYC_APPROVALS",
                newName: "IX_KYC_APPROVALS_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_KYC_APPROVAL_AdminId",
                table: "KYC_APPROVALS",
                newName: "IX_KYC_APPROVALS_AdminId");

            migrationBuilder.AlterColumn<int>(
                name: "LoanProductId",
                table: "LOAN_APPLICATIONS",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_REPAYMENTS",
                table: "REPAYMENTS",
                column: "RepaymentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KYC_APPROVALS",
                table: "KYC_APPROVALS",
                column: "KycID");

            migrationBuilder.AddForeignKey(
                name: "FK_KYC_APPROVALS_Admin_AdminId",
                table: "KYC_APPROVALS",
                column: "AdminId",
                principalTable: "Admin",
                principalColumn: "AdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_KYC_APPROVALS_Customers_CustomerId",
                table: "KYC_APPROVALS",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LOAN_APPLICATIONS_LoanProducts_LoanProductId",
                table: "LOAN_APPLICATIONS",
                column: "LoanProductId",
                principalTable: "LoanProducts",
                principalColumn: "LoanProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_REPAYMENTS_LOAN_APPLICATIONS_ApplicationId",
                table: "REPAYMENTS",
                column: "ApplicationId",
                principalTable: "LOAN_APPLICATIONS",
                principalColumn: "ApplicationId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
