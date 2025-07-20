using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserRoles.Migrations
{
    /// <inheritdoc />
    public partial class Assigntasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TasksId",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.DropColumn(
                name: "TasksId",
                table: "Tickets");
        }
    }
}
