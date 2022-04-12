using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerApp.Migrations
{
    /// <summary>
    /// Migration: Add MessageType to PinnedMessage.
    /// </summary>
    public partial class AddMessageTypeToPinnedMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pinned_statuses");

            migrationBuilder.CreateTable(
                name: "pinned_messages",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    chat_id = table.Column<long>(type: "INTEGER", nullable: false),
                    message_id = table.Column<long>(type: "INTEGER", nullable: false),
                    message_type = table.Column<int>(type: "INTEGER", nullable: false),
                    time = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pinned_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_pinned_messages_chats_chat_id",
                        column: x => x.chat_id,
                        principalTable: "chats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pinned_messages_chat_id",
                table: "pinned_messages",
                column: "chat_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pinned_messages");

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
                name: "IX_pinned_statuses_chat_id",
                table: "pinned_statuses",
                column: "chat_id");
        }
    }
}
