using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaceDay.Api.Data;

#nullable disable

namespace RaceDay.Api.Migrations
{
    [DbContext(typeof(RaceDayContext))]
    [Migration("20261109201500_AddEventBannerImage")]
    public partial class AddEventBannerImage
    {
    }
}