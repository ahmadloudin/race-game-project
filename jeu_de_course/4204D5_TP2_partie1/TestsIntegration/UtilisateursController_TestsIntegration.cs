using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SussyKart_Partie1.Controllers;
using SussyKart_Partie1.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsIntegration
{
    public class UtilisateursController_TestsIntegration : IClassFixture<BDTestFixture>
    {
        private BDTestFixture Fixture { get; }

        public UtilisateursController_TestsIntegration(BDTestFixture fixture)
        {
            Fixture = fixture;
        }


        [Fact]
        public async Task AjouterAmi()
        {
            using TP2_SussyKartContext context = Fixture.CreateContext();
            context.Database.BeginTransaction();
            UtilisateursController controller = new UtilisateursController(context);

            IActionResult actionResult = await controller.AjouterAmi(3, "dreamyprince3");
            RedirectToActionResult redirectToAction = Assert.IsType<RedirectToActionResult>(actionResult);

            IActionResult actionResult1 = await controller.AjouterAmi(3, "dreamyprince3");

            context.ChangeTracker.Clear();
            Assert.Equal(1, await context.Amities.Where(x => x.UtilisateurId == 3 && x.UtilisateurIdAmiNavigation.Pseudo == "dreamyprince3").CountAsync());
            context.Database.RollbackTransaction();
        }

        [Fact]
        public async Task DesactiverCompte()
        {
            using TP2_SussyKartContext context = Fixture.CreateContext();
            context.Database.BeginTransaction();
            UtilisateursController controller = new UtilisateursController(context);

            IActionResult actionResult = await controller.DesactiverCompte(6);
            context.ChangeTracker.Clear();
            Assert.True(await context.Utilisateurs.Where(x => x.UtilisateurId == 6).Select(x => x.EstSuppr).FirstAsync());
            context.Database.RollbackTransaction();
        }
    }
}
