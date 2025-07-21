using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserRoles.Migrations
{
    /// <inheritdoc />
    public partial class Assigningtask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Tasks_TasksId1",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_TasksId1",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TasksId1",
                table: "Tickets");

            migrationBuilder.AlterColumn<int>(
                name: "TasksId",
                table: "Tickets",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TasksId",
                table: "Tickets",
                column: "TasksId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Tasks_TasksId",
                table: "Tickets",
                column: "TasksId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Tasks_TasksId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_TasksId",
                table: "Tickets");

            migrationBuilder.AlterColumn<string>(
                name: "TasksId",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TasksId1",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TasksId1",
                table: "Tickets",
                column: "TasksId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Tasks_TasksId1",
                table: "Tickets",
                column: "TasksId1",
                principalTable: "Tasks",
                principalColumn: "Id");
        }
    }
}
