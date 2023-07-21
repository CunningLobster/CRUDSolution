using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    public partial class TIN_Length8 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CHK_TIN",
                table: "Persons");

            migrationBuilder.AlterColumn<string>(
                name: "TaxIdentificationNumber",
                table: "Persons",
                type: "varchar(8)",
                nullable: true,
                defaultValue: "ABC12345",
                oldClrType: typeof(string),
                oldType: "varchar(15)",
                oldNullable: true,
                oldDefaultValue: "ABC12345");

            migrationBuilder.AddCheckConstraint(
                name: "CHK_TIN",
                table: "Persons",
                sql: "len([TaxIdentificationNumber]) = 8");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CHK_TIN",
                table: "Persons");

            migrationBuilder.AlterColumn<string>(
                name: "TaxIdentificationNumber",
                table: "Persons",
                type: "varchar(15)",
                nullable: true,
                defaultValue: "ABC12345",
                oldClrType: typeof(string),
                oldType: "varchar(8)",
                oldNullable: true,
                oldDefaultValue: "ABC12345");

            migrationBuilder.AddCheckConstraint(
                name: "CHK_TIN",
                table: "Persons",
                sql: "len([TIN]) = 8");
        }
    }
}
