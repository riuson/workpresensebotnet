using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerApp.Migrations
{
    /// <summary>
    /// Migration: Database structure updated. Added table for pinned messages.
    /// </summary>
    public partial class AddTablePinnedMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_chats_users_user_id",
                table: "chats");

            migrationBuilder.DropIndex(
                name: "IX_statuses_chat_id",
                table: "statuses");

            migrationBuilder.DropIndex(
                name: "IX_chats_user_id",
                table: "chats");

            migrationBuilder.DropColumn(
                name: "chat_id",
                table: "chats");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "chats");

            migrationBuilder.AddColumn<long>(
                name: "user_id",
                table: "statuses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "pinned_statuses",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    chat_id = table.Column<long>(type: "INTEGER", nullable: false),
                    message_id = table.Column<long>(type: "INTEGER", nullable: false),
                    time = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pinned_statuses", x => x.id);
                    table.ForeignKey(
                        name: "FK_pinned_statuses_chats_chat_id",
                        column: x => x.chat_id,
                        principalTable: "chats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_statuses_chat_id",
                table: "statuses",
                column: "chat_id");

            migrationBuilder.CreateIndex(
                name: "IX_statuses_user_id",
                table: "statuses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_pinned_statuses_chat_id",
                table: "pinned_statuses",
                column: "chat_id");

            migrationBuilder.AddForeignKey(
                name: "FK_statuses_users_user_id",
                table: "statuses",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_statuses_users_user_id",
                table: "statuses");

            migrationBuilder.DropTable(
                name: "pinned_statuses");

            migrationBuilder.DropIndex(
                name: "IX_statuses_chat_id",
                table: "statuses");

            migrationBuilder.DropIndex(
                name: "IX_statuses_user_id",
                table: "statuses");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "statuses");

            migrationBuilder.AddColumn<long>(
                name: "chat_id",
                table: "chats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "user_id",
                table: "chats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_statuses_chat_id",
                table: "statuses",
                column: "chat_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_chats_user_id",
                table: "chats",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_chats_users_user_id",
                table: "chats",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
