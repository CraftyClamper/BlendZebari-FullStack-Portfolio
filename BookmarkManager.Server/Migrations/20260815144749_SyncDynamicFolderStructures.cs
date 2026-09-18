using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookmarkManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class SyncDynamicFolderStructures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Folder",
                table: "Bookmarks");

            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "Bookmarks",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ParentFolderId = table.Column<int>(type: "INTEGER", nullable: true),
                    WorkspaceId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Folders_Folders_ParentFolderId",
                        column: x => x.ParentFolderId,
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_FolderId",
                table: "Bookmarks",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_ParentFolderId",
                table: "Folders",
                column: "ParentFolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookmarks_Folders_FolderId",
                table: "Bookmarks",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookmarks_Folders_FolderId",
                table: "Bookmarks");

            migrationBuilder.DropTable(
                name: "Folders");

            migrationBuilder.DropIndex(
                name: "IX_Bookmarks_FolderId",
                table: "Bookmarks");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "Bookmarks");

            migrationBuilder.AddColumn<int>(
                name: "Folder",
                table: "Bookmarks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
