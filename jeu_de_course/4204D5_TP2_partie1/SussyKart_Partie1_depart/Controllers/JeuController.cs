using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SussyKart_Partie1.Data;
using SussyKart_Partie1.Models;
using SussyKart_Partie1.ViewModels;
using System.Security.Claims;
using System.Security.Principal;

namespace SussyKart_Partie1.Controllers
{
    public class JeuController : Controller
    {


        readonly TP2_SussyKartContext _context;

        public JeuController(TP2_SussyKartContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Tutoriel()
        {
            return View();
        }

        public IActionResult Jouer()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Jouer(ParticipationVM pvm)
        {
            // Le paramètre pvm est déjà rempli par la View Jouer et il est reçu par cette action... qui est vide.
            IIdentity? identite = HttpContext.User.Identity;
            string pseudo = HttpContext.User.FindFirstValue(ClaimTypes.Name);
            Utilisateur? utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(x => x.Pseudo == pseudo);
            if (utilisateur == null)
            {
                return RedirectToAction("Connexion", "Utilisateurs");
            }
            string query = "EXEC Courses.USP_ParticipationCourses @Position, @Chrono, @NbJoueurs, @NomCourse, @UtilisateurId";

            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter{ParameterName = "@Position", Value = pvm.Position},
                new SqlParameter{ParameterName = "@Chrono", Value = pvm.Chrono},
                new SqlParameter{ParameterName = "@NbJoueurs", Value = pvm.NbJoueurs},
                new SqlParameter{ParameterName = "@NomCourse", Value = pvm.NomCourse},
                new SqlParameter{ParameterName = "@UtilisateurId", Value = utilisateur?.UtilisateurId}
            };

            await _context.Database.ExecuteSqlRawAsync(query, parameters.ToArray());
            return View();
        }
    }
}
