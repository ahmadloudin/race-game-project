using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SussyKart_Partie1.Data;
using SussyKart_Partie1.Models;
using SussyKart_Partie1.ViewModels;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace SussyKart_Partie1.Controllers
{
    public class UtilisateursController : Controller
    {
        readonly TP2_SussyKartContext _context;
        public UtilisateursController(TP2_SussyKartContext context)
        {
            _context = context;
        }

        public IActionResult Inscription()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Inscription(InscriptionVM ivm)
        {
            // Création d'un nouvel utilisateur

            // Si l'inscription réussit :
            bool existeDeja = await _context.Utilisateurs.AnyAsync(x => x.Pseudo == ivm.Pseudo);
            if (existeDeja)
            {
                ModelState.AddModelError("Pseudo", "Ce pseudonyme est déjà pris.");
                return View(ivm);
            }

            // On INSERT l'utilisateur avec une procédure stockée qui va s'occuper de
            // hacher le mot de passe
            string query = "EXEC Utilisateurs.USP_CreerUtilisateur @Pseudo, @Courriel, @NoBancaire, @MotDePasse";
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter{ParameterName = "@Pseudo", Value = ivm.Pseudo},
                new SqlParameter{ParameterName = "@Courriel", Value = ivm.Courriel},
                new SqlParameter{ParameterName = "@NoBancaire", Value = ivm.NoBancaire},
                new SqlParameter{ParameterName = "@MotDePasse", Value = ivm.MotDePasse}
            };
            try
            {
                await _context.Database.ExecuteSqlRawAsync(query, parameters.ToArray());
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Une erreur est survenue. Veuillez réessayez.");
                return View(ivm);
            }
            return RedirectToAction("Connexion", "Utilisateurs");
        }

        public IActionResult Connexion()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Connexion(ConnexionVM cvm)
        {
            // Procédure stockée qui compare le mot de passe fourni à celui dans la BD
            // Retourne juste l'utilisateur si le mot de passe est valide
            string query = "EXEC Utilisateurs.USP_AuthUtilisateur @Pseudo, @MotDePasse";
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter{ParameterName = "@Pseudo", Value = cvm.Pseudo},
                new SqlParameter{ParameterName = "@MotDePasse", Value = cvm.MotDePasse}
            };
            Utilisateur? utilisateur = (await _context.Utilisateurs.FromSqlRaw(query, parameters.ToArray()).ToListAsync()).FirstOrDefault();
            if (utilisateur == null)
            {
                ModelState.AddModelError("", "Nom d'utilisateur ou mot de passe invalide");
                return View(cvm);
            }

            // Construction du cookie d'authentification 
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, utilisateur.UtilisateurId.ToString()),
                new Claim(ClaimTypes.Name, utilisateur.Pseudo)
            };

            ClaimsIdentity identite = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new ClaimsPrincipal(identite);

            // Cette ligne fournit le cookie à l'utilisateur
            await HttpContext.SignInAsync(principal);

            return RedirectToAction("Index", "Jeu");
        }

        [HttpGet]
        public async Task<IActionResult> Deconnexion() {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Jeu");
        }

        public async Task<IActionResult> Profil()
        {
            // Dans tous les cas, on doit envoyer un ProfilVM à la vue.

            // Manière habituelle de récupérer un utilisateur
            IIdentity? identite = HttpContext.User.Identity;
            if (!identite.IsAuthenticated)
            {
                return RedirectToAction("Connexion", "Utilisateurs");
            }
            string pseudo = HttpContext.User.FindFirstValue(ClaimTypes.Name);
            Utilisateur? utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(x => x.Pseudo == pseudo);
            if (utilisateur == null) // Utilisateur supprimé entre-temps ... ?
            {
                return RedirectToAction("Connexion", "Utilisateurs");
            }

            string query = "EXEC Utilisateurs.USP_NoBancaire @Pseudo";
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter{ParameterName = "@Pseudo", Value = utilisateur.Pseudo}
            };
            ViewData["NbAmis"] = _context.Amities.Where(x => x.Utilisateur== utilisateur).Count();
            NoBancaire? noBancaire = (await _context.NoBancaires.FromSqlRaw(query, parameters.ToArray()).ToListAsync()).FirstOrDefault();
            ViewData["UtilisateurID"] = utilisateur.UtilisateurId;
            Avatar? avatar = await _context.Avatars.FirstOrDefaultAsync(x => x.UtilisateurId == utilisateur.UtilisateurId);
            string? url = avatar != null && avatar.Fichier != null ? $"data:images/png;base64, {Convert.ToBase64String(avatar.Fichier)}" : null;
            return View(new ProfilVM()
            {                
                ImageUrl = url,
                Pseudo = utilisateur.Pseudo,
                DateInscription = utilisateur.DateInscription,
                Courriel = utilisateur.Courriel,
                NoBancaire = noBancaire.NumBancaire.ToString()
            });
        }

        // Action qui mène vers une vue qui permet de choisir un avatar pour son profil.
        [Authorize]
        public async Task<IActionResult> Avatar()
        {
            // Trouver l'utilisateur grâce à son cookie.
            IIdentity? identite = HttpContext.User.Identity;
            if (!identite.IsAuthenticated)
            {
                return RedirectToAction("Connexion", "Utilisateurs");
            }
            string pseudo = HttpContext.User.FindFirstValue(ClaimTypes.Name);
            Utilisateur? utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(x => x.Pseudo == pseudo);
            if (utilisateur == null) // Utilisateur supprimé entre-temps ... ?
            {
                return RedirectToAction("Connexion", "Utilisateurs");
            }
            Avatar? avatar = await _context.Avatars.FirstOrDefaultAsync(x => x.UtilisateurId == utilisateur.UtilisateurId);
            // Si utilisateur trouvé, retourner la vue Avatar avec un ImageUploadVM qui contient le bon UtilisateurID.
            return View(new ImageUploadVM()
            {
                UtilisateurID = utilisateur.UtilisateurId          
            });
        }

        // Action qui est appelée suite à l'envoi d'un formulaire et qui change l'avatar
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Avatar(ImageUploadVM iuvm)
        {
            // Trouver l'utilisateur grâce à son cookie
            IIdentity? identite = HttpContext.User.Identity;
            if (!identite.IsAuthenticated)
            {
                return RedirectToAction("Connexion", "Utilisateurs");
            }
            string pseudo = HttpContext.User.FindFirstValue(ClaimTypes.Name);
            Utilisateur? utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(x => x.Pseudo == pseudo);
            if (utilisateur == null) // Utilisateur supprimé entre-temps ... ?
            {
            // Si aucun utilisateur trouvé, retourner la vue Connexion
                return RedirectToAction("Connexion", "Utilisateurs");
            }
            // Si le FormFile contient bel et bien un fichier, ajouter / remplacer 
            // un avatar dans la BD et retourner au Profil.
     
            if (ModelState.IsValid)
            {
                Avatar? avatar = await _context.Avatars.FirstOrDefaultAsync(x => x.UtilisateurId == iuvm.UtilisateurID);
                if (iuvm.FormFile!= null && iuvm.FormFile.Length >= 0)
                {
                    MemoryStream stream = new MemoryStream();
                    await iuvm.FormFile.CopyToAsync(stream);
                    byte[] fichierAvatar = stream.ToArray();
                    if (avatar == null)
                    {
                        avatar = new Avatar { Utilisateur = utilisateur, Fichier = fichierAvatar};
                        _context.Avatars.Add(avatar);
                    }
                    else
                    {
                        avatar.Fichier = fichierAvatar;
                    }
                    await _context.SaveChangesAsync();

                }
                return RedirectToAction(nameof(Profil)); 
            }
            // Si aucun fichier fourni, retourner à la vue Avatar.
            return RedirectToAction("Avatar");
        }

        // Action qui mène vers une vue qui affiche notre liste d'amis et permet d'en ajouter de nouveaux.
        [Authorize]
        public async Task<IActionResult> Amis()
        {
            // Trouver l'utilisateur grâce à son cookie
            IIdentity? identite = HttpContext.User.Identity;
            if (!identite.IsAuthenticated)
            {
                return RedirectToAction("Connexion", "Utilisateurs");
            }
            string pseudo = HttpContext.User.FindFirstValue(ClaimTypes.Name);
            Utilisateur? utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(x => x.Pseudo == pseudo);
            if (utilisateur == null) // Utilisateur supprimé entre-temps ... ?
            {
                // Si aucun utilisateur trouvé, retourner la vue Connexion
                return RedirectToAction("Connexion", "Utilisateurs");
            }
            Avatar? avatar = await _context.Avatars.FirstOrDefaultAsync(x => x.UtilisateurId == utilisateur.UtilisateurId);
            string? url = avatar != null && avatar.Fichier != null ? $"data:images/png;base64, {Convert.ToBase64String(avatar.Fichier)}" : null;
            // Sinon, retourner la vue Amis en lui transmettant une liste d'AmiVM
            // De plus, glisser dans ViewData["utilisateurID"] l'id de l'utilisateur qui a appelé l'action. (Car c'est utilisé dans Amis.cshtml)
            ViewData["UtilisateurID"] = utilisateur.UtilisateurId;
            List<AmiVM> amiVMs = await _context.Amities.Where(x => x.UtilisateurId == utilisateur.UtilisateurId).Select(x => new AmiVM()
            {
                AmiID = x.UtilisateurIdAmiNavigation.UtilisateurId,
                DateInscription = x.UtilisateurIdAmiNavigation.DateInscription,
                DernierePartie = x.UtilisateurIdAmiNavigation.ParticipationCourses.OrderByDescending(x => x.DateParticipation).Select(x => x.DateParticipation).FirstOrDefault(),
                ImageUrl = x.UtilisateurIdAmiNavigation.Avatar != null && x.UtilisateurIdAmiNavigation.Avatar.Fichier != null ? $"data:images/png;base64, {Convert.ToBase64String(x.UtilisateurIdAmiNavigation.Avatar.Fichier)}" : null,
                Pseudo = x.UtilisateurIdAmiNavigation.Pseudo

            }).ToListAsync();
            return View(amiVMs);
        }

        // Action appelée lorsque le formulaire pour ajouter un ami est rempli
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AjouterAmi(int utilisateurID, string pseudoAmi)
        {
            // Trouver l'utilisateur qui a appelé l'action ET l'utilisateur qui sera ajouté en ami
            Utilisateur? utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(x => x.UtilisateurId == utilisateurID);
            if (utilisateur == null)
            {
                // Si l'utilisateur qui appelle l'action n'existe pas, retourner la vue Connexion.
                return View("Connexion");
            }
            Utilisateur? utilisateurAmi = await _context.Utilisateurs.FirstOrDefaultAsync(x => x.Pseudo == pseudoAmi);
            if (utilisateurAmi == null)
            {
                // Si l'ami à ajouter n'existe pas rediriger vers la vue Amis.
                return RedirectToAction("Amis");
            }
            if(utilisateurAmi.EstSuppr == true)
            {
                return RedirectToAction("Amis");
            }
            //Si l'ami ne faisait pas déjà partie de la liste, créer une nouvelle amitié et l'ajouter dans la BD.
            bool amitieExistante = await _context.Amities.AnyAsync(x =>
                (x.UtilisateurId == utilisateur.UtilisateurId && x.UtilisateurIdAmi == utilisateurAmi.UtilisateurId) ||
                (x.UtilisateurId == utilisateurAmi.UtilisateurId && x.UtilisateurIdAmi == utilisateur.UtilisateurId)
            );
            if (!amitieExistante)
            {
                Amitie nouvelleAmitie = new Amitie()
                {               
                
                    Utilisateur = utilisateur,
                    UtilisateurIdAmiNavigation = utilisateurAmi,
                    UtilisateurId = utilisateur.UtilisateurId,
                    UtilisateurIdAmi = utilisateurAmi.UtilisateurId
                };
                await _context.Amities.AddAsync(nouvelleAmitie);
                await _context.SaveChangesAsync();
            }
            // Puis, dans tous les cas, rediriger vers la vue Amis.
            return RedirectToAction("Amis");
        }

        // Action qui est appelée lorsqu'on appuie sur le bouton qui supprime un ami
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SupprimerAmi(int utilisateurID, int amiID)
        {
            // Trouver l'utilisateur qui a appelé l'action ET l'utilisateur qui sera retiré des amis
            // Si l'utilisateur qui appelle l'action n'existe pas, retourner la vue Connexion.
            Utilisateur? utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(x => x.UtilisateurId == utilisateurID);
            if (utilisateur == null)
            {
                // Si l'utilisateur qui appelle l'action n'existe pas, retourner la vue Connexion.
                return View("Connexion");
            }
            Utilisateur? utilisateurAmi = await _context.Utilisateurs.FirstOrDefaultAsync(x => x.UtilisateurId == amiID);
            if (utilisateurAmi == null)
            {
                // Si l'ami à ajouter n'existe pas rediriger vers la vue Amis.
                return RedirectToAction("Amis");
            }
            Amitie? amitie = _context.Amities.Where(x => x.Utilisateur == utilisateur && x.UtilisateurIdAmiNavigation == utilisateurAmi).FirstOrDefault();
            // Supprimer l'amitié de la BD et redirigrer vers la vue Amis.
            _context.Amities.Remove(amitie);
            await _context.SaveChangesAsync();
            return RedirectToAction("Amis");
        }

        // Action qui est appelée lorsqu'un utilisateur appuie sur le bouton qui supprime son compte
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DesactiverCompte(int utilisateurID)
        {
            // Trouver l'utilisateur avec l'id utilisateurID et s'il n'existe pas retourner la vue Connexion
            Utilisateur? utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(x => x.UtilisateurId == utilisateurID);
            if (utilisateur == null)
            {
                // Si l'utilisateur qui appelle l'action n'existe pas, retourner la vue Connexion.
                return View("Connexion");
            }
            // " Suppimer " l'utilisateur de la BD. Votre déclencheur fera le reste.
            _context.Utilisateurs.Remove(utilisateur);
            await _context.SaveChangesAsync();
            // await HttpContext.SignOutAsync(); Même si mettre cette ligne de code serait judicieux, ne pas le faire !
            return RedirectToAction("Index", "Jeu");
        }
    }
}
