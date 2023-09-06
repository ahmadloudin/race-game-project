using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SussyKart_Partie1.Data;
using SussyKart_Partie1.Models;
using SussyKart_Partie1.ViewModels;

namespace SussyKart_Partie1.Controllers
{
    public class StatsController : Controller
    {

            readonly TP2_SussyKartContext _context;

            public StatsController(TP2_SussyKartContext context)
            {
                _context = context;
            }

            public IActionResult Index()
            {
                return View();
            }

            // Section 1 : Compléter ToutesParticipations (Obligatoire)
            public async Task<IActionResult> ToutesParticipations()
            {
                // Obtenir les participations grâce à une vue SQL
                var participations = await _context.VwToutesParticipations
                    .OrderByDescending(p => p.DateParticipation)
                    .Skip(0)
                    .Take(30)
                    .ToListAsync();
                
                var filter = new FiltreParticipationVM
                {
                    Participations = participations
                };

                return View(filter);
            }

        public IActionResult ToutesParticipationsFiltre(FiltreParticipationVM fpvm)
        {
            //Obtenir les participations grâce à une vue SQL
            IEnumerable<VwToutesParticipation> participations = _context.VwToutesParticipations;

            if (fpvm.Pseudo != null)
            {
                participations = participations.Where(p => p.UtilisateurPseudo == fpvm.Pseudo);
            }

            if (fpvm.Course != "Toutes")
            {
                participations = participations.Where(p => p.Nom == fpvm.Course);
            }

            if (fpvm.Ordre == "Chrono")
            {
                if (fpvm.TypeOrdre == "asc")
                {
                    participations = participations.OrderBy(p => p.Chrono);
                }
                else
                {
                    participations = participations.OrderByDescending(p => p.Chrono);
                }
            }
            else
            {
                if (fpvm.TypeOrdre == "asc")
                {
                    participations = participations.OrderBy(p => p.DateParticipation);
                }
                else
                {
                    participations = participations.OrderByDescending(p => p.DateParticipation);
                }
            }

            participations = participations.Skip((fpvm.Page - 1) * 30).Take(30);

            fpvm.Participations = participations.ToList();

            return View("ToutesParticipations", fpvm);
            }

            // Section 2 : Compléter ParticipationsParCourse OU ChronoParCourseParTour
            public IActionResult ParticipationsParCourse()
            {
            var participationsParCourse = _context.ParticipationCourses
            .GroupBy(p => p.Course.Nom)
            .Select(g => new NbParticipationParCoursesVM
            {
                Nom = g.Key,
                NbUtilisateurs = g.Select(p => p.UtilisateurId).Distinct().Count()
            }).ToList();

            return View(participationsParCourse);
            }

            public IActionResult ChronoParCourseParTour()
            {
                return View();
            }

            // Section 3 : Compléter MeilleursChronosSolo ou MeilleursChronosQuatre
            public IActionResult MeilleursChronosSolo()
            {
                var meilleurTemps = _context.ParticipationCourses
                .Where(p => p.NbJoueurs == 1)
                .Where(p => p.Chrono != null)
                .OrderBy(p => p.Chrono)
                .Take(30)
                .Select(g => new MeilleursChronosVM
                {
                    Pseudo = g.Utilisateur.Pseudo,
                    NomCourse = g.Course.Nom,
                    Position = g.Position,
                    Chrono = g.Chrono,
                    Date = g.DateParticipation
                })
                .ToList();


                return View(meilleurTemps);
            }

            public IActionResult MeilleursChronosQuatre()
            {
                return View();
            }
        }
    }
