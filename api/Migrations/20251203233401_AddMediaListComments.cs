using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaMatch.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaListComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MediaListComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SuggestedMediaId = table.Column<int>(type: "int", nullable: true),
                    SuggestedMediaType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SuggestedMediaTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SuggestedMediaPosterPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaListId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaListComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaListComments_MediaLists_MediaListId",
                        column: x => x.MediaListId,
                        principalTable: "MediaLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MediaListComments_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaListComments_AuthorId",
                table: "MediaListComments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaListComments_MediaListId",
                table: "MediaListComments",
                column: "MediaListId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaListComments");
        }
    }
}
