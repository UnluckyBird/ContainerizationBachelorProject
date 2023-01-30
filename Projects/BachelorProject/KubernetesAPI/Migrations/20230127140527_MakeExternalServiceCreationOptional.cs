using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KubernetesAPI.Migrations
{
    /// <inheritdoc />
    public partial class MakeExternalServiceCreationOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExposedPort",
                table: "ConnectorType",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExposedPort",
                table: "ConnectorType");
        }
    }
}
