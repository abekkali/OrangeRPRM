using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RPRM.Data;
using RPRM.Models;
using RPRM.Models.Metiers;
using RPRM.Models.ViewModels;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace RPRM.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Api : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        public Api(ApplicationDbContext context, IConfiguration configuration, IMemoryCache cache) 
        {   
            _context = context;
            _configuration = configuration;
           _cache = cache;
        }
        [HttpGet("GetLookupData")]
        public IActionResult GetLookupData()
        {
            DateTime lastModified;
            if (!_cache.TryGetValue("LastModified", out lastModified))
            {
                lastModified = DateTime.UtcNow;
                _cache.Set("LastModified", lastModified);
            }
            if (HttpContext.Request.Headers.TryGetValue("If-Modified-Since", out var ifModifiedSinceValue))
            {
                if (DateTime.TryParseExact(ifModifiedSinceValue, "r", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime ifModifiedSince) && ifModifiedSince >= lastModified)
                {
                    return StatusCode(304);
                }
            }
            HttpContext.Response.Headers.Add("Last-Modified", lastModified.ToString("R"));
            var lookupData = _context.LookupTable.ToList();

            return Ok(lookupData);
        }
        [HttpGet("GetGroupData")]
        public IActionResult GetGroupData()
        {
            DateTime lastModified;
            if (!_cache.TryGetValue("LastModifiedGroupe", out lastModified))
            {
                lastModified = DateTime.UtcNow;
                _cache.Set("LastModifiedGroupe", lastModified);
            }

            if (HttpContext.Request.Headers.TryGetValue("If-Modified-Since", out var ifModifiedSinceValue))
            {
                if (DateTime.TryParseExact(ifModifiedSinceValue, "r", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime ifModifiedSince) && ifModifiedSince >= lastModified)
                {
                    return StatusCode(304);
                }
            }

            HttpContext.Response.Headers.Add("Last-Modified", lastModified.ToString("R"));
            var groupData = _context.Groupe.Select(g => new { g.Code_Groupe, g.Nom_Groupe }).ToList();
            return Ok(groupData);
        }

        [HttpGet("GetDashboardData")]
        public async Task<IActionResult> GetDashboardData()
        {
            var countryCount = await _context.operateurs.Select(o => o.Code_pays).Distinct().CountAsync();
            var operatorCount = await _context.operateurs.CountAsync();
            var incidentCount = await _context.incidents.CountAsync();
            var bilateralServiceCount = await _context.serviceOuverts
            .Include(service => service.DirectionLookup)
            .CountAsync(service => service.DirectionLookup.Value == "BILATERAL");
            var uniInServiceCount = await _context.serviceOuverts
            .Include(service => service.DirectionLookup)
            .CountAsync(service => service.DirectionLookup.Value == "RIN");
            var uniOutServiceCount = await _context.serviceOuverts
            .Include(service => service.DirectionLookup)
            .CountAsync(service => service.DirectionLookup.Value == "ROUT");

            var result = new
            {
                CountryCount = countryCount,
                OperatorCount = operatorCount,
                IncidentCount = incidentCount,
                ServiceBillateral=bilateralServiceCount,
                ServiceIn=uniInServiceCount,
                ServiceOut=uniOutServiceCount
            };

            return Ok(result);
        }

        [HttpGet("verifier-code-pays")]
        public async Task<IActionResult> VerifierCodePays(string codePays)
        {
            
            var paysExistant = await _context.Pays.FirstOrDefaultAsync(p => p.Code_pays == codePays);
            return Json(paysExistant != null);
        }
        
        [HttpPost]
        [Route("GetOperateurs")]
        public IActionResult GetOperateurs([FromBody] DataTablesRequest data)
        {
            var draw = data.Draw;
            var start = data.Start;
            var length = data.Length;
            var searchValue = data.SearchValue;
            var orderColumn = data.OrderColumn;
            var orderDirection = data.OrderDirection;

            var query = _context.operateurs.Include(o => o.TypeOperateur).Include(o => o.TypeAccord).Include(o => o.Groupe).AsQueryable();

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(o =>
                    o.Code_PLMN.Contains(searchValue) ||
                    o.Code_pays.Contains(searchValue) ||
                    o.Nom_Op.Contains(searchValue) ||
                    o.MCC.Contains(searchValue) ||
                    o.MNC.Contains(searchValue) ||
                    o.Op_prefered.Contains(searchValue) ||
                    o.RNA.Contains(searchValue) ||
                    o.RA_Teminated.Contains(searchValue) ||
                    o.TypeOperateur.Value.Contains(searchValue) ||
                    o.TypeAccord.Value.Contains(searchValue) ||
                    o.Groupe.Nom_Groupe.Contains(searchValue)
                );
            }


            // Appliquer le tri
            if (!string.IsNullOrEmpty(orderColumn))
            {
                var columnMapping = new Dictionary<string, Expression<Func<Operateurs, object>>>
        {
            {"code_PLMN", o => o.Code_PLMN},
            {"code_pays", o => o.Code_pays},
            {"nom_Op", o => o.Nom_Op},
            {"mcc", o => o.MCC},
            {"mnc", o => o.MNC},
            {"Marketshare", o => o.Marketshare},
            {"op_prefered", o => o.Op_prefered},
            {"rna", o => o.RNA},
            {"rA_Teminated", o => o.RA_Teminated},
            {"groupe", o => o.Groupe.Nom_Groupe},
            {"typeOperateur", o => o.TypeOperateur.Value},
            {"typeAccord", o => o.TypeAccord.Value}
        };

                if (columnMapping.TryGetValue(orderColumn, out var sortExpression))
                {
                    query = orderDirection == "asc" ? query.OrderBy(sortExpression) : query.OrderByDescending(sortExpression);
                }
            }

            var recordsTotal = query.Count();
            var recordsFiltered = query.Count();

            var operateurData = query.Skip(start).Take(length).ToList().Select(o => new
            {
                o.Code_PLMN,
                o.Code_pays,
                o.Nom_Op,
                MCC = o.MCC,
                MNC = o.MNC,
                Marketshare = o.Marketshare,
                Op_prefered = o.Op_prefered,
                RNA = o.RNA,
                RA_Teminated = o.RA_Teminated,
                Nom_Groupe = o.Groupe.Nom_Groupe,
                TypeOperateur = o.TypeOperateur.Value,
                TypeAccord = o.TypeAccord.Value
            });

            return Ok(new
            {
                draw,
                recordsTotal,
                recordsFiltered,
                data = operateurData
            });
        }
        [HttpPost]
        [Route("GetContacts")]
        public IActionResult GetContacts([FromBody] DataTablesRequest data)
        {
            var draw = data.Draw;
            var start = data.Start;
            var length = data.Length;
            var searchValue = data.SearchValue;
            var orderColumn = data.OrderColumn;
            var orderDirection = data.OrderDirection;

            var query = _context.contacts.Include(c => c.RoleLookup).AsQueryable();

            int searchValueInt;
            bool isSearchValueInt = int.TryParse(searchValue, out searchValueInt);

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(c =>
                    (isSearchValueInt && c.Code_Contact == searchValueInt) ||
                    //c.Code_Contact.Contains(searchValue) ||
                    c.Code_PLMN.Contains(searchValue) ||
                    c.Type.Contains(searchValue) ||
                    c.Nom.Contains(searchValue) ||
                    c.Telephone.Contains(searchValue) ||
                    c.Email.Contains(searchValue) ||
                    c.RoleLookup.Value.Contains(searchValue)
                );
            }

            // Appliquer le tri si une colonne de tri est spécifiée
            if (!string.IsNullOrEmpty(orderColumn))
            {
                var columnMapping = new Dictionary<string, Expression<Func<Contact, object>>>
                {
                    {"code_Contact", c => c.Code_Contact},
                    {"code_PLMN", c => c.Code_PLMN},
                    {"type", c => c.Type},
                    {"nom", c => c.Nom},
                    {"telephone", c => c.Telephone},
                    {"email", c => c.Email},
                    {"role", c => c.RoleLookup.Value},
                };

                if (columnMapping.TryGetValue(orderColumn, out var sortExpression))
                {
                    query = orderDirection == "asc" ? query.OrderBy(sortExpression) : query.OrderByDescending(sortExpression);
                }
            }


            // Calculer le nombre total d'enregistrements et le nombre d'enregistrements filtrés
            var recordsTotal = query.Count();
            var recordsFiltered = query.Count();

            // Extraire les données de la page demandée et les transformer en format approprié pour DataTables
            var contactData = query.Skip(start).Take(length).ToList().Select(c => new
            {
                Code_Contact = c.Code_Contact,
                Code_PLMN = c.Code_PLMN,
                Type = c.Type,
                Nom = c.Nom,
                Telephone = c.Telephone,
                Email = c.Email,
                Role = c.RoleLookup.Value
            });


            // Retourner les données au format attendu par DataTables
            return Ok(new
            {
                draw,
                recordsTotal,
                recordsFiltered,
                data = contactData
            });
        }
        [HttpPost]
        [Route("GetServiceOuverts")]
        public IActionResult GetServiceOuverts([FromBody] DataTablesRequest data)
        {
            var draw = data.Draw;
            var start = data.Start;
            var length = data.Length;
            var searchValue = data.SearchValue;
            var orderColumn = data.OrderColumn;
            var orderDirection = data.OrderDirection;

            var query = _context.serviceOuverts.Include(o => o.Operateur).Include(o => o.NomServiceLookup).Include(o => o.DirectionLookup).AsQueryable();

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(o =>
                    o.Code_Service.ToString().Contains(searchValue) ||
                    o.Code_PLMN.Contains(searchValue) ||
                    o.Destination.Contains(searchValue) ||
                    o.NomServiceLookup.Value.Contains(searchValue) ||
                    o.DirectionLookup.Value.Contains(searchValue) ||
                    o.Operateur.Nom_Op.Contains(searchValue)
                );
            }

            // Appliquer le tri
            if (!string.IsNullOrEmpty(orderColumn))
            {
                var columnMapping = new Dictionary<string, Expression<Func<ServiceOuvert, object>>>
    {
        {"Code_Service", o => o.Code_Service},
        {"Code_PLMN", o => o.Code_PLMN},
        {"Destination", o => o.Destination},
        {"NomServiceLookup.Value", o => o.NomServiceLookup.Value},
        {"date_d", o => o.date_d},
        {"date_f", o => o.date_f},
        {"DirectionLookup.Value", o => o.DirectionLookup.Value}
    };

                if (columnMapping.TryGetValue(orderColumn, out var sortExpression))
                {
                    query = orderDirection == "asc" ? query.OrderBy(sortExpression) : query.OrderByDescending(sortExpression);
                }
            }

            var recordsTotal = query.Count();
            var recordsFiltered = query.Count();

            var serviceOuvertData = query.Skip(start).Take(length).ToList().Select(o => new
            {
                o.Code_Service,
                o.Code_PLMN,
                OperateurName = o.Operateur.Nom_Op,
                o.Destination,
                NomServiceLookup = o.NomServiceLookup.Value,
                date_d = o.date_d.HasValue ? o.date_d.Value.ToString("dd/MM/yyyy") : null,
                date_f = o.date_f.HasValue ? o.date_f.Value.ToString("dd/MM/yyyy") : null,
                DirectionLookup = o.DirectionLookup.Value
            });

            return Ok(new
            {
                draw,
                recordsTotal,
                recordsFiltered,
                data = serviceOuvertData
            });
        }

        [HttpPost]
        [Route("GetTarifs")]
        public IActionResult GetTarifs([FromBody] DataTablesRequest data)
        {
            var draw = data.Draw;
            var start = data.Start;
            var length = data.Length;
            var searchValue = data.SearchValue;
            var orderColumn = data.OrderColumn;
            var orderDirection = data.OrderDirection;

            var query = _context.tarifs.Include(t => t.TypeTraficLookup).Include(t => t.TypeTarifLookup).Include(t => t.IncrementLookup).Include(t => t.DirectionLookup).AsQueryable();

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(t =>
                    t.Code_Tarif.ToString().Contains(searchValue) ||
                    t.Code_PLMN.Contains(searchValue) ||
                    t.TypeTraficLookup.Value.Contains(searchValue) ||
                    t.TypeTarifLookup.Value.Contains(searchValue) ||
                    t.Date_d.ToString().Contains(searchValue) ||
                    t.Date_f.ToString().Contains(searchValue) ||
                    t.IncrementLookup.Value.Contains(searchValue) ||
                    t.Exchange_rate.ToString().Contains(searchValue) ||
                    t.Rate.ToString().Contains(searchValue) ||
                    t.Commentaire.Contains(searchValue) ||
                    t.DirectionLookup.Value.Contains(searchValue) ||
                    t.Auto_Renwal.Contains(searchValue) ||
                    t.Devis.Contains(searchValue) ||
                    t.Document_DCH.Contains(searchValue) ||
                    t.Contact.Contains(searchValue)
                );
            }

            switch (orderColumn)
            {
                case "Code_Tarif":
                    query = orderDirection == "asc" ? query.OrderBy(t => t.Code_Tarif) : query.OrderByDescending(t => t.Code_Tarif);
                    break;
                case "Code_PLMN":
                    query = orderDirection == "asc" ? query.OrderBy(t => t.Code_PLMN) : query.OrderByDescending(t => t.Code_PLMN);
                    break;
                case "Type_Trafic_id":
                    query = orderDirection == "asc" ? query.OrderBy(t => t.TypeTraficLookup.Value) : query.OrderByDescending(t => t.TypeTraficLookup.Value);
                    break;
                case "Type_Tarif_id":
                    query = orderDirection == "asc" ? query.OrderBy(t => t.TypeTarifLookup.Value) : query.OrderByDescending(t => t.TypeTarifLookup.Value);
                    break;
                case "Date_d":
                    query = orderDirection == "asc" ? query.OrderBy(t => t.Date_d) : query.OrderByDescending(t => t.Date_d);
                    break;
                case "Date_f":
                    query = orderDirection == "asc" ? query.OrderBy(t => t.Date_f) : query.OrderByDescending(t => t.Date_f);
                    break;
                case "Increment_id":
                    query = orderDirection == "asc" ? query.OrderBy(t => t.IncrementLookup.Value) : query.OrderByDescending(t => t.IncrementLookup.Value);
                    break;
                case "Exchange_rate":
                    query = orderDirection == "asc" ? query.OrderBy(t => t.Exchange_rate) : query.OrderByDescending(t => t.Exchange_rate);
                    break;
                case "Rate":
                    query = orderDirection == "asc" ? query.OrderBy(t => t.Rate) : query.OrderByDescending(t => t.Rate);
                    break;
                case "Commentaire":
                    query = orderDirection == "asc" ? query.OrderBy(t => t.Commentaire) : query.OrderByDescending(t => t.Commentaire);
                    break;
                case "Direction_id":
                    query = orderDirection == "asc" ? query.OrderBy(t => t.DirectionLookup.Value) : query.OrderByDescending(t => t.DirectionLookup.Value);
                    break;
                case "Auto_Renwal":
                    query = orderDirection == "asc" ? query.OrderBy(t => t.Auto_Renwal) : query.OrderByDescending(t => t.Auto_Renwal);
                    break;
                case "Devis":
                    query = orderDirection == "asc" ? query.OrderBy(t => t.Devis) : query.OrderByDescending(t => t.Devis);
                    break;
                case "Document_DCH":
                    query = orderDirection == "asc" ? query.OrderBy(t => t.Document_DCH) : query.OrderByDescending(t => t.Document_DCH);
                    break;
                case "Contact":
                    query = orderDirection == "asc" ? query.OrderBy(t => t.Contact) : query.OrderByDescending(t => t.Contact);
                    break;
                default:
                    query = query.OrderBy(t => t.Code_Tarif);
                    break;
            }

            var recordsTotal = query.Count();
            var recordsFiltered = query.Count();

            var tarifData = query.Skip(start).Take(length).ToList().Select(t => new
            {
                t.Code_Tarif,
                t.Code_PLMN,
                TypeTrafic = t.TypeTraficLookup.Value,
                TypeTarif = t.TypeTarifLookup.Value,
                DateDebut = t.Date_d.HasValue ? t.Date_d.Value.ToString("dd/MM/yyyy") : "",
                DateFin = t.Date_f.HasValue ? t.Date_f.Value.ToString("dd/MM/yyyy") : "",
                Increment = t.IncrementLookup.Value,
                ExchangeRate = t.Exchange_rate.HasValue ? t.Exchange_rate.Value.ToString("0.#########") : "",
                Rate = t.Rate.HasValue ? t.Rate.Value.ToString("0.#########") : "",
                t.Commentaire,
                Direction = t.DirectionLookup.Value,
                AutoRenwal = t.Auto_Renwal,
                Devise = t.Devis, 
              
            });



            return Ok(new
            {
                draw,
                recordsTotal,
                recordsFiltered,
                data = tarifData
            });
        }
        [HttpGet("GetAllPLMNCodes")]
        public IActionResult GetAllPLMNCodes()
        {
            var query = _context.operateurs.AsQueryable();

            var codes = query.Select(o => new { o.Code_PLMN, o.Nom_Op ,o.Pays.Nom_pays }) .Distinct().ToList();

            return Ok(codes);
        }
        [HttpGet("GetAllPays")]
        public IActionResult GetAllPays()
        {
            var query = _context.Pays.AsQueryable();

            var countryNames = query.Select(c => c.Nom_pays).Distinct().ToList();

            return Ok(countryNames);
        }
        [HttpGet("OpByCountry")]
        public IActionResult GetOperatorCountByCountry()
        {
            var result = _context.operateurs
                .GroupBy(o => o.Code_pays)
                .Select(g => new
                {
                    countryCode = g.Key,
                    countryName = g.FirstOrDefault().Pays.Nom_pays,
                    cc=g.FirstOrDefault().Pays.CC,
                    operators = g.Select(o => new
                    {
                        Code = o.Code_PLMN,
                        Name = o.Nom_Op,
                        service_direction_pairs = _context.serviceOuverts
                            .Where(s => s.Code_PLMN == o.Code_PLMN)
                            .Select(s => new
                            {
                                service = s.Nom_Service_id,
                                direction = s.Direction_id
                            })
                            .ToList()
                    }).ToList(),
                    group = g.FirstOrDefault().Groupe.Nom_Groupe
                })
                .ToList();

            return Ok(result);
        }


        [HttpGet("{codePlmn}")]
        public ActionResult<PlmnGeneralView> GetPlmnGeneralView([FromQuery] string codePlmn)
        {
            if (!_context.operateurs.Any(o => o.Code_PLMN == codePlmn))
            {
                return NotFound($"No data found for codePlmn: {codePlmn}");
            }

            var operateur = _context.operateurs
                .Include(o => o.TypeOperateur)
                .Include(o => o.TypeAccord)
                .Include(o => o.Pays)
                .Include(o => o.Groupe)
                .FirstOrDefault(o => o.Code_PLMN == codePlmn);

            if (operateur == null)
            {
                return NotFound();
            }

            var plmnGeneralView = new PlmnGeneralView
            {
                Code_PLMN = operateur.Code_PLMN,
                Nom_Op = operateur.Nom_Op,
                TypeOperateur = operateur.TypeOperateur?.Value,
                TypeAccord = operateur.TypeAccord?.Value,
                NomPays = operateur.Pays?.Nom_pays,
                Groupe=operateur.Groupe.Nom_Groupe
            };

            var incidents = _context.incidents
                .Where(i => i.Code_PLMN == codePlmn)
                .ToList();

            if (incidents != null && incidents.Count > 0)
            {
                plmnGeneralView.Incidents = incidents.Select(i => new IncidentViewModel
                {
                    Code_Incident = i.Code_Incident,
                    Code_PLMN = i.Code_PLMN,
                    Commentaire = i.Commentaire,
                    Code_TT = i.Code_TT,
                    Date_d = i.date_d?.ToString("dd-MM-yyyy"),
                    Date_f = i.date_f?.ToString("dd-MM-yyyy")
                }).ToList();
            }

            var serviceOuvert = _context.serviceOuverts
                .Include(s => s.DirectionLookup)
                .Include(s => s.NomServiceLookup)
                .Where(s => s.Code_PLMN == codePlmn)
                .ToList();

            if (serviceOuvert != null && serviceOuvert.Count > 0)
            {
                plmnGeneralView.ServiceOuverts = serviceOuvert.Select(s=> new ServiceOuvertsViewModel 
                {
                 Destination= s.Destination,
                 NomService=s.NomServiceLookup.Value,
                 Direction=s.DirectionLookup.Value,
                 Date_d=s.date_d?.ToString("dd-MM-yyyy")
                }).ToList();
                //plmnGeneralView.NomService = string.Join(", ", serviceOuvert.Where(s => s.NomServiceLookup != null).Select(s => s.NomServiceLookup.Value));
                //plmnGeneralView.Direction = string.Join(", ", serviceOuvert.Where(s => s.DirectionLookup != null).Select(s => s.DirectionLookup.Value));
            }
            var contact = _context.contacts.Where(i => i.Code_PLMN == codePlmn)
                .ToList();
            if (contact != null && contact.Count > 0)
            {
                plmnGeneralView.Contacts = contact.Select(i => new ContactViewModel
                {
                    Type = i.Type,
                    Nom = i.Nom,
                    Telephone = i.Telephone,
                    Email = i.Email
                }).ToList();
            }
            plmnGeneralView.NbOperateurs = _context.operateurs.Count(o => o.Code_Groupe == operateur.Code_Groupe);

            var doc=_context.docOperateurs
                .Include(i => i.TypeDocLookup)
                .Where(d => d.Code_PLMN == codePlmn)
                .ToList();
            if (doc != null && doc.Count > 0)
            {
                plmnGeneralView.Documents = doc.Select(d => new DocumentViewModel
                {
                    Document=d.Document,
                    Type=d.TypeDocLookup.Value,
                    Date=d.date_d?.ToString("dd-MM-yyyy"),
                    
                }).ToList();
            }
            var tarifs = _context.tarifs
                .Include(t => t.TypeTraficLookup)
                .Include(t => t.TypeTarifLookup)
                .Include(t => t.IncrementLookup)
                .Include(t => t.DirectionLookup)
                .Where(t => t.Code_PLMN == codePlmn)
                .ToList();

            if (tarifs != null && tarifs.Count > 0)
            {
                plmnGeneralView.tarifs = tarifs.Select(t => new TarifViewModel
                {
                    Type_Trafic = t.TypeTraficLookup.Value,
                    Type_Tarif = t.TypeTarifLookup.Value,
                    Increment = t.IncrementLookup.Value,
                    Exchange_rate = t.Exchange_rate,
                    Rate = t.Rate,
                }).ToList();
            }

            return plmnGeneralView;
        }

        [HttpGet("mapcontrol")]
        public async Task<IActionResult> GetMapControl(string callback)
        {
            var apiKey = _configuration["BingMapsApiKey"];
            var url = $"https://www.bing.com/api/maps/mapcontrol?callback={callback}&key={apiKey}&setLang=fr";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/javascript");
            }
        }
        [HttpGet("GetIncidentsByOp")]
        public async Task<IActionResult> GetIncidentsByCountry()
        {
            try
            {
                var incidentsByCountry = await _context.operateurs
                    .Join(_context.incidents,
                        operateur => operateur.Code_PLMN,
                        incident => incident.Code_PLMN,
                        (operateur, incident) => new { Operateur = operateur, Incident = incident })
                    .Join(_context.Pays,
                        op_inc => op_inc.Operateur.Code_pays,
                        pays => pays.Code_pays,
                        (op_inc, pays) => new { Pays = pays, Incident = op_inc.Incident })
                    .GroupBy(p_i => new { p_i.Pays.Nom_pays, p_i.Pays.Continent })
                    .Select(g => new
                    {
                        Continent = g.Key.Continent,
                        Incidents = g.Select(p_i => new
                        {
                            TypeIncident = p_i.Incident.TypeIncidentLookup.Value 
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(incidentsByCountry);
            }
            catch (Exception e)
            {
                // Enregistrez l'exception ici en utilisant votre système de journalisation
                return BadRequest("Erreur lors de la récupération des incidents par pays.");
            }
        }

        [HttpGet("filteredServices")]
        public IActionResult GetFilteredServices([FromQuery] List<int> serviceIds, [FromQuery] List<int> directionIds)
        {
            var services = _context.serviceOuverts.AsQueryable();

            if (serviceIds != null && serviceIds.Count > 0)
            {
                services = services.Where(service => serviceIds.Contains(service.Nom_Service_id));
            }

            if (directionIds != null && directionIds.Count > 0)
            {
                services = services.Where(service => directionIds.Contains(service.Direction_id));
            }

            var groupedServices = services.GroupBy(service => service.NomServiceLookup.Value)
                                          .Select(group => new { ServiceName = group.Key, Count = group.Count() });

            return Ok(groupedServices);
        }

    }

    public class DataTablesRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public string SearchValue { get; set; }
        public string OrderColumn { get; set; }
        public string OrderDirection { get; set; }
    }
}
