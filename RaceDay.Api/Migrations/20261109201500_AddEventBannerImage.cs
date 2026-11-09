using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace RaceDay.Api.Migrations;
public partial class AddEventBannerImage : Migration
{
 protected override void Up(MigrationBuilder migrationBuilder) { migrationBuilder.AddColumn<string>(name:"banner_image_url",table:"Events",type:"varchar(500)",nullable:true); }
 protected override void Down(MigrationBuilder migrationBuilder) { migrationBuilder.DropColumn(name:"banner_image_url",table:"Events"); }
}