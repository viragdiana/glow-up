using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlowUp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomSectionsSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProfileSections_ProfileId_SectionType",
                table: "ProfileSections");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileSections_ProfileId_SectionType",
                table: "ProfileSections",
                columns: new[] { "ProfileId", "SectionType" },
                unique: true,
                filter: "[SectionType] <> 'Custom'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProfileSections_ProfileId_SectionType",
                table: "ProfileSections");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileSections_ProfileId_SectionType",
                table: "ProfileSections",
                columns: new[] { "ProfileId", "SectionType" },
                unique: true);
        }
    }
}
