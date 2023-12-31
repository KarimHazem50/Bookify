using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookifyTest.Data.Migrations
{
    public partial class EditColumnName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsDelated",
                table: "Categories",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "IsDelated",
                table: "Books",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "IsDelated",
                table: "Authors",
                newName: "IsDeleted");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "Categories",
                newName: "IsDelated");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "Books",
                newName: "IsDelated");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "Authors",
                newName: "IsDelated");
        }
    }
}
