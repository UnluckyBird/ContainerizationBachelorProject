using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KubernetesAPI.Migrations
{
    /// <inheritdoc />
    public partial class addMultipleExposedPorts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExposedPort",
                table: "ConnectorType");

            migrationBuilder.CreateTable(
                name: "ExposedPort",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Port = table.Column<int>(type: "int", nullable: false),
                    ConnectorTypeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExposedPort", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExposedPort_ConnectorType_ConnectorTypeId",
                        column: x => x.ConnectorTypeId,
                        principalTable: "ConnectorType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExposedPort_ConnectorTypeId",
                table: "ExposedPort",
                column: "ConnectorTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExposedPort");

            migrationBuilder.AddColumn<int>(
                name: "ExposedPort",
                table: "ConnectorType",
                type: "int",
                nullable: true);
        }
    }
}
