using Microsoft.EntityFrameworkCore.Migrations;

namespace MoviesAPI.Migrations
{
    public partial class AddAdminRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            Insert into AspNetRoles (Id, [name], [NormalizedName]) 
            values ('0fc9a172 - e4d1 - 4376 - 98d3 - b8bcb04b76d8', 'Admin', 'Admin')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"delete AspNetRoles
            where id = '0fc9a172 - e4d1 - 4376 - 98d3 - b8bcb04b76d8'");
        }
    }
}
