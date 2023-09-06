using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SussyKart_Partie1.Controllers;
using SussyKart_Partie1.Data;
using SussyKart_Partie1.Models;
using SussyKart_Partie1.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsIntegration
{
    public class StatsController_TestsIntegration : IClassFixture<BDTestFixture>
    {
        private BDTestFixture Fixture { get; }

        public StatsController_TestsIntegration(BDTestFixture fixture)
        {
            Fixture = fixture;
        }

        [Fact]
        public async Task ToutesParticipationsFiltre()
        {
            using TP2_SussyKartContext context = Fixture.CreateContext();
            StatsController controller = new StatsController(context);

            var filter = new FiltreParticipationVM
            {
                Pseudo = "sussyprincessZ", 
                Course = "Donut"
            };

            IActionResult actionResult = controller.ToutesParticipationsFiltre(filter);

            ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);

            Assert.Equal(3, filter.Participations.Count);
        }


    }
}
