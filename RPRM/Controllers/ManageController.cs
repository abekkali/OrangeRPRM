using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPRM.Data;
using RPRM.Models.Metiers;
using RPRM.Models;
using System.Globalization;
using MySql.Data.MySqlClient;
using System.Linq;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Formats.Tar;
using Microsoft.Extensions.Caching.Memory;

namespace RPRM.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public ManageController(ApplicationDbContext context,IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllPays()
        {
            var pays = await _context.Pays.Select(p => new { p.Code_pays, p.Nom_pays }).ToListAsync();
            return Ok(pays);
        }
        // Action pour afficher la liste des pays
        [AuthorizePermission("Pays", "view")]
        public async Task<IActionResult> Pays()
        {
            var pays = await _context.Pays.ToListAsync();
            return View(pays);
        }
        [AuthorizePermission("Pays", "edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPays(Pays pays)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(pays.Code_pays))
                {
                    return Json(new { success = false, message = "Le code pays est requis." });
                }

                if (string.IsNullOrEmpty(pays.CC))
                {
                    return Json(new { success = false, message = "Le champ CC est requis." });
                }

                if (string.IsNullOrEmpty(pays.Nom_pays))
                {
                    return Json(new { success = false, message = "Le nom du pays est requis." });
                }

                // Vérifier si le code pays existe déjà dans la base de données
                var paysExistant = await _context.Pays.FirstOrDefaultAsync(p => p.Code_pays == pays.Code_pays);

                if (paysExistant != null)
                {
                    // Le code pays existe déjà
                    return Json(new { success = false, message = "Le code pays existe déjà." });
                }

                // Ajouter le nouveau pays à la base de données
                _context.Pays.Add(pays);
                await _context.SaveChangesAsync();

                // Rediriger vers la liste des pays
                return Json(new { success = true, message = "Pays ajouté avec succès." });
            }

            // Si le modèle n'est pas valide, retourner à la vue
            return Json(new { success = false, message = "Le modèle n'est pas valide." });
        }
        [AuthorizePermission("Pays", "edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePays(Pays p)
        {
            var pays = await _context.Pays.FindAsync(p.Code_pays);

            if (pays != null)
            {
                if (string.IsNullOrEmpty(p.MCC))
                {
                    return Json(new { success = false, message = "Le champ MCC est requis." });
                }

                if (string.IsNullOrEmpty(p.CC))
                {
                    return Json(new { success = false, message = "Le champ CC est requis." });
                }

                if (string.IsNullOrEmpty(p.Nom_pays))
                {
                    return Json(new { success = false, message = "Le nom du pays est requis." });
                }

                pays.Nom_pays = p.Nom_pays;
                pays.Nom_pays_anglais = p.Nom_pays_anglais;
                pays.Pass = p.Pass;
                pays.MCC = p.MCC;
                pays.CC =   p.CC;
                pays.Region = p.Region;
                pays.Continent = p.Continent;

                _context.Update(pays);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Pays modifié avec succès." });
            }
            else
            {
                return Json(new { success = false, message = "Le modèle n'est pas valide." });
            }
        }
        // Action pour afficher la liste des lookup tables
        [AuthorizePermission("LookupTable", "view")]
        public async Task<IActionResult> LookupTable()
        {
            var lookupTables = await _context.LookupTable.ToListAsync();
            return View(lookupTables);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizePermission("LookupTable", "edit")]
        public async Task<IActionResult> AddLookup(string type, string value)
        {
            try
            {
                // Vérification des données d'entrée
                if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(value))
                {
                    return Json(new { success = false, message = "Données d'entrée non valides." });
                }

                // Créer un nouveau LookupTable
                LookupTable newLookupTable = new LookupTable
                {
                    Lookup_Type = type,
                    Value = value.ToUpper()
                };

                // Ajoutez le nouveau LookupTable à la base de données et enregistrez les modifications
                _context.LookupTable.Add(newLookupTable);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "LookupTable ajouté avec succès." });
            }
            catch (Exception e)
            {
                // Enregistrez l'exception ici en utilisant votre système de journalisation
                return Json(new { success = false, message = "Erreur lors de l'ajout du LookupTable." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizePermission("LookupTable", "edit")]
        public async Task<IActionResult> UpdateLookup(int codelookup, string valeur)
        {
            Console.WriteLine($"Received data: codelookup={codelookup}, valeur={valeur}");

            var lookup = await _context.LookupTable.FindAsync(codelookup);
                if (lookup == null)
                {
                    return NotFound();
                }

                lookup.Value = valeur;

                try
                {
                    _context.Update(lookup);
                    await _context.SaveChangesAsync();
                _cache.Set("LastModified", DateTime.UtcNow );

            }
            catch (DbUpdateConcurrencyException)
                {
                    return NotFound();          
                }

                return Ok();   
        }

        // Action pour afficher la liste des Groupes
        [AuthorizePermission("Groupe", "view")]
        public async Task<IActionResult> Groupe()
        {
            var groupe = await _context.Groupe.ToListAsync();
            return View(groupe);
        }
        [AuthorizePermission("Groupe", "edit")]
        public async Task<IActionResult> AddGroupe(Groupe model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var normalizedGroupName = model.Nom_Groupe.Replace(" ", "").ToLower();
                    var existingGroup = await _context.Groupe.FirstOrDefaultAsync(g => g.Nom_Groupe.Replace(" ", "").ToLower() == normalizedGroupName);

                    if (existingGroup != null)
                    {
                        return Json(new { success = false, message = "Le nom du groupe existe déjà." });
                    }

                    _context.Groupe.Add(model);
                    await _context.SaveChangesAsync();
                    _cache.Set("LastModifiedGroupe", DateTime.UtcNow );


                    return Json(new { success = true, message = "Groupe ajouté avec succès." });
                }
                catch (Exception e)
                {
                    return Json(new { success = false, message = "Erreur lors de l'ajout du groupe." });
                }
            }

            return Json(new { success = false, message = "erreur de model." });
        }

        [AuthorizePermission("Groupe", "edit")]
        [HttpPost]
        public async Task<IActionResult> Updategroupe(Groupe model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingGroup = await _context.Groupe.FindAsync(model.Code_Groupe);

                    if (existingGroup != null)
                    {
                        existingGroup.Nom_Groupe = model.Nom_Groupe;
                        existingGroup.Eng_Val_In = model.Eng_Val_In;
                        existingGroup.Eng_Val_out = model.Eng_Val_out;
                        existingGroup.Date_d = model.Date_d;
                        existingGroup.Date_f = model.Date_f;

                        _context.Groupe.Update(existingGroup);
                        await _context.SaveChangesAsync();
                        _cache.Set("LastModifiedGroupe", DateTime.UtcNow );


                        return Json(new { success = true, message = "Groupe modifié avec succès." });
                    }
                    return Json(new { success = false, message = "Groupe introuvable." });
                }
                catch (Exception e)
                {
                    return Json(new { success = false, message = "Erreur lors de la modification du groupe." });
                }
            }

            return Json(new { success = false, message = "Invalid model state." });
        }


        // Action pour  operateurs
        public IActionResult CheckPlmnCode(string code)
        {
            bool exists = _context.operateurs.Any(o => o.Code_PLMN == code);
            return Json(new { exists = exists });
        }

        [AuthorizePermission("Operateurs", "view")]
        public async Task<IActionResult> Operateur()
        {
            return View();
        }
        [HttpGet]
        [AuthorizePermission("Operateurs", "view")]

        public async Task<JsonResult> getOperateur(string Code_PLMN)
        {

            var operateur = await _context.operateurs.FindAsync(Code_PLMN);
            return Json(operateur);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizePermission("Operateurs", "edit")]
        public IActionResult AddOperateur([FromBody] Operateurs operateur)
        {
            try
            {
            ModelState.Remove("Pays");
            ModelState.Remove("Groupe");
            ModelState.Remove("TypeOperateur");
            ModelState.Remove("TypeAccord");
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage)).ToList();
                return Json(new { error = true, message = errors });
            }


            // Vérifier que le Code PLMN n'existe pas déjà.
            if (_context.operateurs.Any(o => o.Code_PLMN == operateur.Code_PLMN))
            {
                return Json(new { error = true, message = "Code plmn existe deja" });
            }
            operateur.Code_PLMN = operateur.Code_PLMN.ToUpper();
            // Ajouter l'opérateur à la base de données.
            _context.operateurs.Add(operateur);
            _context.SaveChanges();

            return Json(new { error = false, message = "Operateur est ajouter avec success ." });
            }
            catch (Exception)
            {
                return Json(new { error = true, message = "Veuillez supprimer le cache et ressayer ." });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizePermission("Operateurs", "edit")]

        public async Task<IActionResult> UpdateOperateur(string code_plmn, string nom_op, int? Marketshare, int GrpName, string OpPrefered, string RNA, string RaTerminated, int TypeOp, int TypeAccord)
        {
            // Rechercher l'opérateur à mettre à jour
            var operateur = await _context.operateurs.FindAsync(code_plmn);

            // Vérifier si l'opérateur existe
            if (operateur == null)
            {
                return NotFound();
            }

            // Mettre à jour les propriétés de l'opérateur
            operateur.Nom_Op = nom_op;
            operateur.Op_prefered = OpPrefered;
            operateur.Marketshare= Marketshare;
            operateur.Code_Groupe = GrpName;
            operateur.RNA = RNA;
            operateur.RA_Teminated = RaTerminated;
            operateur.TypeOperateurId = TypeOp;
            operateur.TypeAccordId = TypeAccord;

            // Enregistrer les modifications dans la base de données
            _context.Entry(operateur).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Vérifier si l'opérateur existe toujours
                if (!_context.operateurs.Any(e => e.Code_PLMN == operateur.Code_PLMN))
                {
                    return Json(new { success = false, message = "Operateur n'existe pas" });

                }
                else
                {
                    return Json(new { success = false, message = "Veuillez supprimer le cache et ressayer ." });
                }
               

            }

            // Rediriger vers la page de gestion des opérateurs
            return Json(new { success = true, message = "Operateur est modifie avec success ." });
        }

        [AuthorizePermission("ServiceOuvert", "view")]
        public async Task<IActionResult> ServiceOuvert()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizePermission("ServiceOuvert", "edit")]

        public async Task<IActionResult> AddSerivceOuvert(ServiceOuvert sv)
        {
            // Vérification des éléments remplis ou sélectionnés
            if (string.IsNullOrEmpty(sv.Code_PLMN) || sv.Nom_Service_id <= 0 || sv.Direction_id <= 0)
            {
                return Json(new { success = false, message = "Tous les champs doivent être correctement remplis ou sélectionnés." });
            }

            // Vérifie si le Code_PLMN est valide
            if (!_context.operateurs.Any(o => o.Code_PLMN == sv.Code_PLMN))
            {
                return Json(new { success = false, message = "Le Code_PLMN n'est pas valide." });
            }

            _context.serviceOuverts.Add(sv);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Le service ouvert a été ajouté avec succès." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizePermission("ServiceOuvert", "edit")]

        public async Task<IActionResult> UpdateSerivceOuvert(int code_service, int nomService, string dd, string df, int Direction ,string Destination)
        {
            try
            {
                var serviceToUpdate = await _context.serviceOuverts.FirstOrDefaultAsync(s => s.Code_Service == code_service);
                if (serviceToUpdate != null)
                {
                    DateTime? date_d = string.IsNullOrEmpty(dd) ? (DateTime?)null : DateTime.ParseExact(dd, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    DateTime? date_f = string.IsNullOrEmpty(df) ? (DateTime?)null : DateTime.ParseExact(df, "yyyy-MM-dd", CultureInfo.InvariantCulture);


                    serviceToUpdate.Nom_Service_id = nomService;
                    serviceToUpdate.date_d = date_d;
                    serviceToUpdate.date_f = date_f;
                    serviceToUpdate.Direction_id = Direction;
                    serviceToUpdate.Destination = Destination;

                    _context.Update(serviceToUpdate);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Service est modifié avec succès." });
                }
                else
                {
                    return Json(new { success = false, message = "Service introuvable." });
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                return Json(new { success = false, message = "Impossible d'enregistrer les modifications. Réessayez, et si le problème persiste, consultez votre administrateur système." });
            }
        }
        [AuthorizePermission("Contact", "view")]

        public async Task<IActionResult> Contact()
        {
            ViewBag.Role = await _context.LookupTable
                .Where(lt => lt.Lookup_Type == "Role")
                .Select(lt => new { lt.Id, lt.Value })
                .ToListAsync();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizePermission("Contact", "edit")]

        public async Task<IActionResult> AddContact(Contact c)
        {
            if (string.IsNullOrEmpty(c.Code_PLMN) || c.Role_id <= 0)
            {
                return Json(new { success = false, message = "Tous les champs doivent être correctement remplis ou sélectionnés." });
            }

            try
            {
                _context.contacts.Add(c);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Contact ajouté avec succès." });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Erreur lors de l'ajout du contact." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizePermission("Contact", "edit")]

        public async Task<IActionResult> UpdateContact(Contact c)
        {
            try
            {
                var contactToUpdate = await _context.contacts.FindAsync(c.Code_Contact);

                if (contactToUpdate != null)
                {
                    contactToUpdate.Nom=c.Nom; 
                    contactToUpdate.Email=c.Email;
                    contactToUpdate.Type = c.Type;
                    contactToUpdate.Telephone = c.Telephone;
                    contactToUpdate.Role_id = c.Role_id;

                    _context.Update(contactToUpdate);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Contact modifié avec succès." });
                }

                return Json(new { success = false, message = "Contact introuvable." });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Erreur lors de la modification du contact." });
            }
        }
        [AuthorizePermission("DocOperateur", "view")]
        public async Task<IActionResult> DocOperateur()
        {
            var tarif = await _context.docOperateurs
                    .Include(o => o.TypeDocLookup).ToListAsync();
            ViewBag.DocOperateur = await _context.LookupTable
                .Where(lt => lt.Lookup_Type == "Type_Doc")
                .Select(lt => new { lt.Id, lt.Value })
                .ToListAsync();
            return View(tarif);
        }

        public async Task<IActionResult> Tarif()
        {

            var tarif = await _context.tarifs
                    .Include(o => o.TypeTarifLookup)
                    .Include(o => o.TypeTraficLookup)
                    .Include(o => o.IncrementLookup)
                    .Include(o => o.IncrementLookup)
                    .Include(o => o.DirectionLookup)
                    .ToListAsync();

            return View(tarif);
        }

        [AuthorizePermission("Tarif", "edit")]
        [HttpPost]
        public async Task<IActionResult> AddTarif([FromForm] Tarif tarif)
        {
            try
            {
                double exchangeRate;
                double rate;

                if (Request.Form.ContainsKey("Exchange_rate") &&
                    double.TryParse(Request.Form["Exchange_rate"], NumberStyles.Any, CultureInfo.InvariantCulture, out exchangeRate))
                {
                    tarif.Exchange_rate = exchangeRate;
                }
                if (Request.Form.ContainsKey("Rate") &&
                    double.TryParse(Request.Form["Rate"], NumberStyles.Any, CultureInfo.InvariantCulture, out rate))
                {
                    tarif.Rate = rate;
                }
                if (_context.tarifs.Any(t => t.Code_PLMN == tarif.Code_PLMN &&
                                            t.Type_Trafic_id == tarif.Type_Trafic_id &&
                                            t.Type_Tarif_id == tarif.Type_Tarif_id &&
                                            t.Increment_id == tarif.Increment_id &&
                                            t.Direction_id ==tarif.Direction_id))
                {
                    return Json(new { success = false, message = "Une entrée avec le même Code PLMN, Type Trafic, Type Tarif ,Increment et Direction existe déjà." });
                }

                _context.tarifs.Add(tarif);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Tarif ajouté avec succès." });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Une erreur s'est produite lors de l'ajout du tarif." });
            }
        }
        [AuthorizePermission("Tarif", "edit")]
        [HttpPost]
        public async Task<IActionResult> ModifyTarif([FromForm] Tarif tarif)
        {
            try
            {
                double? exchangeRate = null;
                double? rate = null;

                if (!string.IsNullOrEmpty(Request.Form["Exchange_rate"]))
                {
                    exchangeRate = Convert.ToDouble(Request.Form["Exchange_rate"], CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(Request.Form["Rate"]))
                {
                    rate = Convert.ToDouble(Request.Form["Rate"], CultureInfo.InvariantCulture);
                }

                tarif.Exchange_rate = exchangeRate;
                tarif.Rate = rate;

                var existingTarif = await _context.tarifs.FindAsync(tarif.Code_Tarif);
                if (existingTarif == null)
                {
                    return Json(new { success = false, message = "Le tarif spécifié n'existe pas." });
                }

                existingTarif.Exchange_rate = exchangeRate;
                existingTarif.Rate = rate;

                if (_context.tarifs.Any(t => t.Code_Tarif != existingTarif.Code_Tarif &&
                                            t.Code_PLMN == existingTarif.Code_PLMN &&
                                            t.Type_Trafic_id == existingTarif.Type_Trafic_id &&
                                            t.Type_Tarif_id == existingTarif.Type_Tarif_id &&
                                            t.Increment_id == existingTarif.Increment_id &&
                                            t.Direction_id == existingTarif.Direction_id))
                {
                    return Json(new { success = false, message = "Une entrée avec le même Code PLMN, Type Trafic, Type Tarif, Increment et Direction existe déjà." });
                }
                existingTarif.Date_d = tarif.Date_d;
                existingTarif.Date_f = tarif.Date_f;
                existingTarif.Type_Trafic_id = tarif.Type_Trafic_id;
                existingTarif.Type_Tarif_id = tarif.Type_Tarif_id;
                existingTarif.Increment_id = tarif.Increment_id;
                existingTarif.Direction_id = tarif.Direction_id;
                existingTarif.Commentaire = tarif.Commentaire;
                existingTarif.Auto_Renwal = tarif.Auto_Renwal;
                existingTarif.Devis = tarif.Devis;

                _context.Update(existingTarif);

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Tarif modifié avec succès." });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return Json(new { success = false, message = "Une erreur s'est produite lors de la modification du tarif."+e.Message });
            }
        }

        public async Task<IActionResult> Incident()
        {
            var incident = await _context.incidents
                    .Include(o => o.TypeIncidentLookup)
                    .Include(o => o.DirectionLookup)
                    .ToListAsync();

            return View(incident);
        }
        [AuthorizePermission("Incident", "edit")]
        [HttpPost]
        public async Task<IActionResult> AddIncident([FromForm] Incident incident)
        {
            if  (incident.Code_PLMN == null ||
            incident.Type_Incident_id == null ||
            incident.Direction_id == null)
            {
                return Json(new { success = false, message = "Toutes les clés doivent être non nulles." });
            }
            try
            {
                if (_context.incidents.Any(i => i.Code_PLMN == incident.Code_PLMN &&
                                                i.Type_Incident_id == incident.Type_Incident_id &&
                                                i.Direction_id == incident.Direction_id))
                {
                    return Json(new { success = false, message = "Une entrée avec le même Code PLMN, Type Trafic, Type Incident et Direction existe déjà." });
                }

                _context.incidents.Add(incident);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Incident ajouté avec succès." });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Une erreur s'est produite lors de l'ajout de l'incident." });
            }
        }
        [AuthorizePermission("Incident", "edit")]
        [HttpPost]
        public async Task<IActionResult> ModifyIncident([FromForm] Incident modifiedIncident)
        {
            try
            {
                var originalIncident = await _context.incidents
                    .FirstOrDefaultAsync(i => i.Code_Incident == modifiedIncident.Code_Incident);

                if (originalIncident == null)
                {
                    return Json(new { success = false, message = "L'incident original n'a pas été trouvé." });
                }

                originalIncident.IMSI = modifiedIncident.IMSI;
                originalIncident.MSISDN = modifiedIncident.MSISDN;
                originalIncident.date_d = modifiedIncident.date_d;
                originalIncident.date_f = modifiedIncident.date_f;
                originalIncident.Commentaire = modifiedIncident.Commentaire;
                originalIncident.Code_TT = modifiedIncident.Code_TT;
                originalIncident.Type_Incident_id = modifiedIncident.Type_Incident_id;
                originalIncident.Direction_id = modifiedIncident.Direction_id;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Incident mis à jour avec succès." });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Une erreur s'est produite lors de la mise à jour de l'incident." });
            }
        }

        public async Task<IActionResult> GetAllData()
        {
            var lookupData = await _context.LookupTable.ToListAsync();
            var groupeData = await _context.Groupe.OrderBy(groupe => groupe.Nom_Groupe)
                .Select(groupe => new { code = groupe.Code_Groupe, nom = groupe.Nom_Groupe })
                  .ToListAsync();

            return Json(new { lookup = lookupData, groupe = groupeData });
        }
        public IActionResult GetGroupes(string NomGroupe)
        {
            NomGroupe = System.Net.WebUtility.UrlDecode(NomGroupe);

            var result = (
                from g in _context.Groupe
                join o in _context.operateurs on g.Code_Groupe equals o.Code_Groupe
                join p in _context.Pays on o.Code_pays equals p.Code_pays
                where g.Nom_Groupe == NomGroupe
                select new
                {
                    Plmn = o.Code_PLMN,
                    NomOperateur = o.Nom_Op,
                    NomPays = p.Nom_pays
                }
            ).ToList();

            if (!result.Any())
            {
                return NotFound("Groupe non trouvé");
            }

            return Ok(result);
        }


        public IActionResult DeleteSelected(string[] selectedIds, string className)
        {
            IQueryable<object> itemsToDelete = null;

            switch (className)
            {
                case "Pays":
                    itemsToDelete = _context.Pays.Where(p => selectedIds.Contains(p.Code_pays));
                    break;
                case "LookupTable":
                    itemsToDelete = _context.LookupTable.Where(l => selectedIds.Contains(l.Id.ToString()));
                    break;
                case "Groupe":
                    itemsToDelete=_context.Groupe.Where(g => selectedIds.Contains(g.Code_Groupe.ToString()));
                    break;
                case "Operateur":
                    itemsToDelete = _context.operateurs.Where(g => selectedIds.Contains(g.Code_PLMN));
                    break;
                case "ServiceOuvert":
                    itemsToDelete = _context.serviceOuverts.Where(g => selectedIds.Contains(g.Code_Service.ToString()));
                    break;
                case "Tarif":
                    itemsToDelete = _context.tarifs.Where(g => selectedIds.Contains(g.Code_Tarif.ToString()));
                    break;
                case "Contact":
                    itemsToDelete = _context.contacts.Where(g => selectedIds.Contains(g.Code_Contact.ToString()));
                    break;
                case "DocOperateur":
                    itemsToDelete = _context.docOperateurs.Where(g => selectedIds.Contains(g.Code_DOC.ToString()));
                    break;
                case "Incident":
                    itemsToDelete = _context.incidents.Where(g => selectedIds.Contains(g.Code_Incident.ToString()));
                    break;
            }

            if (itemsToDelete != null)
            {

                _context.RemoveRange(itemsToDelete);
                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException is MySqlException mySqlEx && mySqlEx.Number == (int)MySqlErrorCode.RowIsReferenced2)
                    {
                        // Gérer l'erreur de contrainte de clé étrangère ici
                        TempData["ErrorMessage"] = "Impossible de supprimer cet élément car il est lié à d'autres éléments.";
                        return RedirectToAction(className);
                    }
                    else
                    {
                        throw; // Relancer l'exception si ce n'est pas une erreur de contrainte de clé étrangère
                    }
                }
            }
            if (className == "LookupTable")
            {
                _cache.Set("LastModified", DateTime.UtcNow );
            }
            if (className == "Groupe")
            {
                _cache.Set("LastModifiedGroupe", DateTime.UtcNow );
            }

            return RedirectToAction(className);
        }
    }
}
