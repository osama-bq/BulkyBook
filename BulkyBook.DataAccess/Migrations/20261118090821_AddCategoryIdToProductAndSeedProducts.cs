using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BulkyBook.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryIdToProductAndSeedProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Author", "CategoryId", "Description", "ISBN", "ImageUrl", "ListPrice", "Price", "Price100", "Price50", "Title" },
                values: new object[,]
                {
                    { 1, "Billy Spark", 1, "Praesent vitae sodales libero. Praesent molestie orci eu massa.", "SWD9999001", "", 99.0, 90.0, 80.0, 85.0, "Fortune of Time" },
                    { 2, "Nancy Hoover", 1, "Praesent vitae sodales libero. Praesent molestie orci eu massa.", "CAW777777701", "", 40.0, 30.0, 20.0, 25.0, "Dark Skies" },
                    { 3, "Julian Button", 1, "Praesent vitae sodales libero. Praesent molestie orci eu massa.", "RITO5555501", "", 55.0, 50.0, 35.0, 40.0, "Vanish in the Sunset" },
                    { 4, "Abby Muscles", 2, "Praesent vitae sodales libero. Praesent molestie orci eu massa.", "WS3333333301", "", 70.0, 65.0, 55.0, 60.0, "Cotton Candy" },
                    { 5, "Ron Parker", 1, "Praesent vitae sodales libero. Praesent molestie orci eu massa.", "RITO5555501", "", 55.0, 50.0, 35.0, 40.0, "Rock in the Ocean" },
                    { 6, "Laura Phantom", 1, "Praesent vitae sodales libero. Praesent molestie orci eu massa.", "RITO5555501", "", 55.0, 50.0, 35.0, 40.0, "Leaves and Wonders" },
                    { 7, "Sam Spark", 1, "Praesent vitae sodales libero. Praesent molestie orci eu massa.", "RITO5555501", "", 55.0, 50.0, 35.0, 40.0, "Lighthouse" },
                    { 8, "John Doe", 3, "A comprehensive history of the First World War.", "HIST1234567", "", 60.0, 55.0, 45.0, 50.0, "The Great War" },
                    { 9, "Jane Smith", 3, "Exploring the wonders of ancient civilizations.", "HIST7654321", "", 70.0, 65.0, 55.0, 60.0, "Ancient Civilizations" },
                    { 10, "Arthur C. Clarke", 2, "A thrilling journey through the cosmos.", "SCIFI1234567", "", 80.0, 75.0, 65.0, 70.0, "Galactic Odyssey" },
                    { 11, "H.G. Wells", 2, "An adventure through time and space.", "SCIFI7654321", "", 90.0, 85.0, 75.0, 80.0, "Time Travelers" },
                    { 12, "John Scalzi", 2, "A gripping tale of humanity's struggle in a distant galaxy.", "SCIFI9876543", "", 100.0, 95.0, 85.0, 90.0, "The Last Colony" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                table: "Products");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Products");
        }
    }
}
