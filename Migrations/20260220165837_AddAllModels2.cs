using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InventoryApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAllModels2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<string>(type: "text", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    CustomString1Name = table.Column<string>(type: "text", nullable: true),
                    CustomString1State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomString2Name = table.Column<string>(type: "text", nullable: true),
                    CustomString2State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomString3Name = table.Column<string>(type: "text", nullable: true),
                    CustomString3State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomInt1Name = table.Column<string>(type: "text", nullable: true),
                    CustomInt1State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomInt2Name = table.Column<string>(type: "text", nullable: true),
                    CustomInt2State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomInt3Name = table.Column<string>(type: "text", nullable: true),
                    CustomInt3State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomBool1Name = table.Column<string>(type: "text", nullable: true),
                    CustomBool1State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomBool2Name = table.Column<string>(type: "text", nullable: true),
                    CustomBool2State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomBool3Name = table.Column<string>(type: "text", nullable: true),
                    CustomBool3State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomText1Name = table.Column<string>(type: "text", nullable: true),
                    CustomText1State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomText2Name = table.Column<string>(type: "text", nullable: true),
                    CustomText2State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomText3Name = table.Column<string>(type: "text", nullable: true),
                    CustomText3State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomLink1Name = table.Column<string>(type: "text", nullable: true),
                    CustomLink1State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomLink2Name = table.Column<string>(type: "text", nullable: true),
                    CustomLink2State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomLink3Name = table.Column<string>(type: "text", nullable: true),
                    CustomLink3State = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventories_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inventories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InventoryId = table.Column<int>(type: "integer", nullable: false),
                    AuthorId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryAccesses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryAccesses_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTag",
                columns: table => new
                {
                    InventoriesId = table.Column<int>(type: "integer", nullable: false),
                    TagsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTag", x => new { x.InventoriesId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_InventoryTag_Inventories_InventoriesId",
                        column: x => x.InventoriesId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryTag_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomId = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InventoryId = table.Column<int>(type: "integer", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    CustomString1 = table.Column<string>(type: "text", nullable: true),
                    CustomString2 = table.Column<string>(type: "text", nullable: true),
                    CustomString3 = table.Column<string>(type: "text", nullable: true),
                    CustomInt1 = table.Column<int>(type: "integer", nullable: true),
                    CustomInt2 = table.Column<int>(type: "integer", nullable: true),
                    CustomInt3 = table.Column<int>(type: "integer", nullable: true),
                    CustomBool1 = table.Column<bool>(type: "boolean", nullable: true),
                    CustomBool2 = table.Column<bool>(type: "boolean", nullable: true),
                    CustomBool3 = table.Column<bool>(type: "boolean", nullable: true),
                    CustomText1 = table.Column<string>(type: "text", nullable: true),
                    CustomText2 = table.Column<string>(type: "text", nullable: true),
                    CustomText3 = table.Column<string>(type: "text", nullable: true),
                    CustomLink1 = table.Column<string>(type: "text", nullable: true),
                    CustomLink2 = table.Column<string>(type: "text", nullable: true),
                    CustomLink3 = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Items_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Likes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Likes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Likes_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AuthorId",
                table: "Comments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_InventoryId",
                table: "Comments",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_CategoryId",
                table: "Inventories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_CreatorId",
                table: "Inventories",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAccesses_InventoryId_UserId",
                table: "InventoryAccesses",
                columns: new[] { "InventoryId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAccesses_UserId",
                table: "InventoryAccesses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTag_TagsId",
                table: "InventoryTag",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_CreatedById",
                table: "Items",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Items_InventoryId",
                table: "Items",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_ItemId_UserId",
                table: "Likes",
                columns: new[] { "ItemId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserId",
                table: "Likes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "InventoryAccesses");

            migrationBuilder.DropTable(
                name: "InventoryTag");

            migrationBuilder.DropTable(
                name: "Likes");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Inventories");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
