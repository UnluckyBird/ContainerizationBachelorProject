using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KubernetesAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDBModelsToAlignWithDockerHub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReleaseDate",
                table: "Image",
                newName: "LastPushed");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Image",
                newName: "Tag");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ConnectorType",
                newName: "Type");

            migrationBuilder.AddColumn<string>(
                name: "Digest",
                table: "Image",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Repository",
                table: "ConnectorType",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Digest",
                table: "Image");

            migrationBuilder.DropColumn(
                name: "Repository",
                table: "ConnectorType");

            migrationBuilder.RenameColumn(
                name: "Tag",
                table: "Image",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "LastPushed",
                table: "Image",
                newName: "ReleaseDate");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "ConnectorType",
                newName: "Name");
        }
    }
}
