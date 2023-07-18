using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Mysqlx.Session;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using RPRM.Data;
using RPRM.Data.ProgHub;
using RPRM.Models;
using RPRM.Models.Metiers;
using RPRM.Models.User;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Channels;

namespace RPRM.Controllers
{
    public class ImportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private string updateMessage;
        private int count;
        private List<int> errorRows;
        private List<int> updateRows;
        private readonly ILogger _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IMemoryCache _cache;
        public ImportController(ApplicationDbContext context, UserManager<User> userManager,IMemoryCache cache, IWebHostEnvironment env, ILogger<ImportController> logger)
        {
            _context = context;
            _cache = cache;
            errorRows = new List<int>();
            updateRows = new List<int>();
            _userManager = userManager; 
            _logger = logger;
            _env = env;
        }
        public IActionResult Index()
        {
            ViewBag.ClassNames = GetClassNamesFromNamespace("RPRM.Models.Metiers");
            return View();
        }
        [AuthorizePermission("DocOperateur", "edit")]

        public async Task<IActionResult> ImportDoc()
        {
            ViewBag.PLMNCodes = _context.operateurs
            .ToList()
    .       Select(o => new SelectListItem
            {
                Value = o.Code_PLMN, 
                Text = o.Code_PLMN  
            });

            ViewBag.DocTypes = _context.LookupTable
                .Where(lt => lt.Lookup_Type == "Type_Doc")
                .ToList()
                .Select(lt => new SelectListItem
                {
                    Value = lt.Id.ToString(), 
                    Text = lt.Value 
                }); return View();
        }
        [AuthorizePermission("DocOperateur", "edit")]
        [HttpPost]
        public async Task<IActionResult> Document(IFormFile Document, string Code_PLMN, int Type_Doc_id, DateTime? date_d, DateTime? date_f)
        {
            if (Document != null)
            {
                var operateur = await _context.operateurs.FirstOrDefaultAsync(o => o.Code_PLMN == Code_PLMN);
                if (operateur == null)
                {
                    return Json(new { success = false, message = "Le Code PLMN n'existe pas dans la base de données" });
                }
                var pays = await _context.Pays.FirstOrDefaultAsync(p => p.Code_pays == operateur.Code_pays);
                var fileName = Document.FileName;

                var typeDocExists = await _context.LookupTable.AnyAsync(lt => lt.Id == Type_Doc_id);
                if (!typeDocExists)
                {
                    return Json(new { success = false, message = "Le Type Doc ID n'existe pas dans la base de données." });
                }
                var entryExists = await _context.docOperateurs.AnyAsync(doc => doc.Code_PLMN == Code_PLMN && doc.Type_Doc_id == Type_Doc_id && doc.Document == fileName);
                if (entryExists)
                {
                    return Json(new { success = false, message = "Une entrée avec le même Code PLMN, Type Doc ID et Document existe déjà dans la base de données." });
                }
                var rootPath = Path.GetPathRoot(Environment.SystemDirectory);
                var savePath = Path.Combine(rootPath ?? Directory.GetCurrentDirectory(), "rprm_doc", pays.Nom_pays, $"{operateur.Nom_Op}-{Code_PLMN}");

                Directory.CreateDirectory(savePath);

                var filePath = Path.Combine(savePath, fileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                await Document.CopyToAsync(fileStream);
                var docOperateur = new DocOperateur
                {
                    Code_PLMN = Code_PLMN,
                    Type_Doc_id = Type_Doc_id,
                    date_d = date_d,
                    date_f = date_f,
                    Document = fileName
                };

                _context.docOperateurs.Add(docOperateur);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Le document a été ajouté avec succès." });
            }
            
            else
            {
                return Json(new { success = false, message = "Le document n'est pas valide." });
            }
        }
        public async Task<IActionResult> GetFile(string codePLMN, string fileName)
        {
            var operateur = await _context.operateurs.FirstOrDefaultAsync(o => o.Code_PLMN == codePLMN);
            if (operateur == null)
            {
                return Json(new { success = false, message = "Le Code PLMN n'existe pas dans la base de données" });
            }
            var pays = await _context.Pays.FirstOrDefaultAsync(p => p.Code_pays == operateur.Code_pays);
            var rootPath = Path.GetPathRoot(Environment.SystemDirectory);
            var savePath = Path.Combine(rootPath ?? Directory.GetCurrentDirectory(), "rprm_doc", pays.Nom_pays, $"{operateur.Nom_Op}-{codePLMN}");

            var filePath = Path.Combine(savePath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                Console.WriteLine("File does not exist.");
                return NotFound();
            }

            var extension = Path.GetExtension(filePath).ToLower();
            var contentType = "application/octet-stream";

            if (extension == ".jpg" || extension == ".jpeg")
            {
                contentType = "image/jpeg";
            }
            else if (extension == ".png")
            {
                contentType = "image/png";
            }
            else if (extension == ".pdf")
            {
                contentType = "application/pdf";
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

            if (contentType == "application/octet-stream")
            {
                Response.Headers.Add("Content-Disposition", $"attachment; filename={Path.GetFileName(filePath)}");
            }

            return File(fileBytes, contentType);
        }

        private static readonly Dictionary<string, List<string>> RequiredColumns = new Dictionary<string, List<string>>
        {
            ["Pays"] = new List<string> { "Code_pays", "Nom_pays","Nom_pays_anglais", "Continent", "Region", "MCC", "Pass", "CC" },
            ["Operateurs"] = new List<string> { "Pays", "PLMN", "Opérateur Roaming", "MCC-MNC","Type" },
            ["LookupTable"] = new List<string> { "Nom_Service", "Type_Accord", "Type_Operateur", "Type_Doc", "Type_Trafic", "Increment", "Direction", "Type_Info", "Type_tarif", "Type_Incident", "Role", "Etat_Test" },
            ["Contact"] = new List<string> { "TADIG code", "Type", "Name", "Phone", "Email"},
            ["Groupe"] = new List<string> { "PLMN", "Groupe" },
            ["ServiceOuvert"] = new List<string> { "PLMN", "Destination", "Date Ouverture", "Close Date si dispo", "VOICE", "CAMEL", "GPRS", "3G", "4G" },
            ["Contact"] = new List<string> { "TADIG code", "Type", "Name", "Phone", "Email"},
            ["Incident"] = new List<string> { "PLMN", "Direction", "Service", "N°Ticket", "Date Envoi", "Date Résolution", "Commentaires" },
            ["Tarif"] = new List<string> { "PLMN","Type_Trafic", "Rate", "Direction", "Increment", "Date_Start","End_Date", "Commentaire", "Auto_Renewal", "Exchange_Rate", "Type_Tarif", "DEVISE" }
        };
        public async Task<JsonResult> ImportExcel(ImportViewModel importViewModel)
        {
            count= 0;
            IFormFile formFile = importViewModel.File;
            if (formFile == null || formFile.Length <= 0)
            {
                return Json(new { ErrorMessage = "Fichier non sélectionné." });
            }

            if (!Path.GetExtension(formFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { ErrorMessage = "Format de fichier non supporté ." });
            }

            using (var stream = new MemoryStream())
            {
                await formFile.CopyToAsync(stream);
                bool sheetFound = false;
                using (var package = new ExcelPackage(stream))
                {
                    int numberOfSheets = package.Workbook.Worksheets.Count;
                    int sheetIndex;
                    for ( sheetIndex = 0; sheetIndex < numberOfSheets; sheetIndex++)
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[sheetIndex];
                        // Si la feuille est vide, passer à la suivante
                        if (worksheet.Dimension == null)
                        {
                            continue;
                        }
                        // Obtenir les coordonnées de départ
                        var (startRow, startCol) = GetStartRowAndCol(worksheet);

                        // Obtenir les correspondances de colonnes
                         var columnMappings = GetColumnMappings(worksheet, startRow);

                        // Vérifier si le dictionnaire RequiredColumns contient une entrée pour ImportType
                        if (RequiredColumns.ContainsKey(importViewModel.ImportType))
                {
                    var requiredColumns = RequiredColumns[importViewModel.ImportType];

                    // Vérifier si toutes les colonnes requises sont présentes dans la feuille de calcul
                    if (requiredColumns.All(c => columnMappings.ContainsKey(c)))
                    {
                        sheetFound = true;
                        break;  // Arrêter la boucle une fois qu'une feuille correspondante a été trouvée
                    }
                }
                    }

                    if (!sheetFound)
                    {
                        return Json(new { ErrorMessage = "Aucune feuille correspondante trouvée dans le fichier." });                  
                    }
                    int rowCount = package.Workbook.Worksheets[sheetIndex].Dimension.End.Row;
                    var messge = "";
                   
                    switch (importViewModel.ImportType)
                    {
                        case "Pays":
                            if (!await CheckImportPermissions("Pays"))
                            {
                                return Json(new { ErrorMessage = "Vous n'avez pas l'autorisation d'importer des données pour 'Pays'." });
                            }
                            messge = await ImportPaysAsync(package.Workbook.Worksheets[sheetIndex], rowCount);
                            break;

                        case "LookupTable":
                            if (!await CheckImportPermissions("LookupTable"))
                            {
                                return Json(new { ErrorMessage = "Vous n'avez pas l'autorisation d'importer des données pour 'LookupTable'." });
                            }
                            messge = await ImportLookupTableAsync(package.Workbook.Worksheets[sheetIndex], rowCount);
                            break;

                        case "Contact":
                            if (!await CheckImportPermissions("Contact"))
                            {
                                return Json(new { ErrorMessage = "Vous n'avez pas l'autorisation d'importer des données pour 'Contact'." });
                            }
                            messge = await ImportContactAsync(package.Workbook.Worksheets[sheetIndex], rowCount);
                            break;

                        case "Operateurs":
                            if (!await CheckImportPermissions("Operateurs"))
                            {
                                return Json(new { ErrorMessage = "Vous n'avez pas l'autorisation d'importer des données pour 'Operateurs'." });
                            }
                            messge = await ImportOperateursAsync(package.Workbook.Worksheets[sheetIndex], rowCount);
                            break;
                        case "Groupe":
                            if (!await CheckImportPermissions("Groupe"))
                            {
                                return Json(new { ErrorMessage = "Vous n'avez pas l'autorisation d'importer des données pour 'Groupe'." });
                            }
                            messge = await ImportGroupeAsync(package.Workbook.Worksheets[sheetIndex], rowCount);
                            break;
                        case "ServiceOuvert":
                            if (!await CheckImportPermissions("ServiceOuvert"))
                            {
                                return Json(new { ErrorMessage = "Vous n'avez pas l'autorisation d'importer des données pour 'ServiceOuvert'." });
                            }
                            messge = await ImportServiceOuvertAsync(package.Workbook.Worksheets[sheetIndex], rowCount);
                            break;
                        case "Incident":
                            if (!await CheckImportPermissions("Incident"))
                            {
                                return Json(new { ErrorMessage = "Vous n'avez pas l'autorisation d'importer des données pour 'Incident'." });
                            }
                            messge = await ImportIncidentsAsync(package.Workbook.Worksheets[sheetIndex], rowCount);
                            break;
                        case "Tarif":
                            if (!await CheckImportPermissions("Tarif"))
                            {
                                return Json(new { ErrorMessage = "Vous n'avez pas l'autorisation d'importer des données pour 'Tarif'." });
                            }
                            messge = await ImportTarifAsync(package.Workbook.Worksheets[sheetIndex], rowCount);
                            break;
                        default:
                            return Json(new { ErrorMessage = "Type d'importation inconnu." });
                    }
                    if (!String.IsNullOrEmpty(messge))
                    {
                        updateMessage=messge;
                      //  return Json(new { UpdateMessage = messge });
                    }
                }
            }
            string successMessage = $"{count} enregistrement(s) ont été importés avec succès.";
            String infoMessage =+ updateRows.Count > 0 ? $"{updateRows.Count} enregistrement(s) ont été modifiés avec succès. Ligne(s) mises à jour : {string.Join(", ", updateRows)}" : "";
            string errorMessage = errorRows.Count > 0 ? $"{errorRows.Count} enregistrement(s) ont échoué à l'importation en raison d'erreurs. Lignes avec erreurs : {string.Join(", ", errorRows)}" : "";

            return Json(new { SuccessMessage = successMessage, UpdateMessage = infoMessage, ErrorMessage = errorMessage+ updateMessage });
        }

        private async Task<string> ImportPaysAsync(ExcelWorksheet worksheet, int rowCount)
        {
            (int startRow, int startCol) = GetStartRowAndCol(worksheet);
            var columnMappings = GetColumnMappings(worksheet, startRow);

            // Récupérez tous les codes de pays existants
            var existingCountryCodes = new HashSet<string>(_context.Pays.Select(p => p.Code_pays));
            var existingCountryNames = new HashSet<string>(_context.Pays.Select(p => p.Nom_pays));
            var existingEnglishCountryNames = new HashSet<string>(_context.Pays.Select(p => p.Nom_pays_anglais));

            // Utiliser HashSet pour éviter les doublons
            var repeatedCountryCodes = new HashSet<string>();
            var repeatedCountryNames = new HashSet<string>();
            var repeatedEnglishCountryNames = new HashSet<string>();

            for (int row = startRow + 1; row < rowCount + startRow; row++)
            {
                var entity = new Pays();
                bool isValidRow = true;  // Variable pour marquer la validité de la ligne

                foreach (var columnMapping in columnMappings)
                {
                    var propertyName = columnMapping.Key;
                    var columnIndex = columnMapping.Value;

                    var propertyInfo = typeof(Pays).GetProperty(propertyName);

                    if (propertyInfo != null)
                    {
                        var cellValue = worksheet.Cells[row, columnIndex].Value?.ToString().Trim();

                        // Effectuez la validation avant de tenter de convertir pour éviter les exceptions
                        if (propertyName == "Pass" && cellValue != "oui" && cellValue != "non")
                        {
                            errorRows.Add(row);
                            isValidRow = false;  // Marquer la ligne comme invalide
                            break;  // Sortir de la boucle
                        }
                        else if (propertyName == "CC" && (cellValue == null || cellValue.Length < 1 || cellValue.Length > 3 || !Regex.IsMatch(cellValue, @"^\d+$")))
                        {
                            errorRows.Add(row);
                            isValidRow = false;  // Marquer la ligne comme invalide
                            break;  // Sortir de la boucle
                        }
                        else if (propertyName == "Code_pays" && (cellValue == null || cellValue.Length != 3 || !Regex.IsMatch(cellValue, @"^[a-zA-Z]+$")))
                        {
                            errorRows.Add(row);
                            isValidRow = false;  // Marquer la ligne comme invalide
                            break;  // Sortir de la boucle
                        }
                        else if (propertyName == "MCC" && (cellValue == null || cellValue.Length != 3 || !Int32.TryParse(cellValue, out _)))
                        {
                            errorRows.Add(row);
                            isValidRow = false;  // Marquer la ligne comme invalide
                            break;  // Sortir de la boucle
                        }

                        var convertedValue = Convert.ChangeType(cellValue, propertyInfo.PropertyType);
                        propertyInfo.SetValue(entity, convertedValue);
                    }
                }

                // Vérifiez si la ligne est valide avant d'ajouter l'entité
                if (isValidRow)
                {
                    if (!existingCountryCodes.Contains(entity.Code_pays)
                        && !existingCountryNames.Contains(entity.Nom_pays)
                        && !existingEnglishCountryNames.Contains(entity.Nom_pays_anglais))
                    {
                        _context.Pays.Add(entity);
                        count++;
                        // Ajoutez le nouveau code de pays et les noms à la liste
                        existingCountryCodes.Add(entity.Code_pays);
                        existingCountryNames.Add(entity.Nom_pays);
                        existingEnglishCountryNames.Add(entity.Nom_pays_anglais);
                    }
                    else
                    {
                        // Ajoutez le code pays/nom pays à la liste des codes/noms pays répétés
                        if (existingCountryCodes.Contains(entity.Code_pays))
                        {
                            repeatedCountryCodes.Add(entity.Code_pays);
                        }
                        if (existingCountryNames.Contains(entity.Nom_pays))
                        {
                            repeatedCountryNames.Add(entity.Nom_pays);
                        }
                        if (existingEnglishCountryNames.Contains(entity.Nom_pays_anglais))
                        {
                            repeatedEnglishCountryNames.Add(entity.Nom_pays_anglais);
                        }

                        var existingEntity = await _context.Pays.FindAsync(entity.Code_pays);
                        if (existingEntity != null)
                        {
                            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
                        }
                    }
                }

            }

            // Construisez le message final à partir des HashSets de répétition, pas des HashSets existants
            string existingCountryCodeMessage = repeatedCountryCodes.Count > 0
            ? $"Code pays existant trouvé. Codes: {String.Join(", ", repeatedCountryCodes)}"
            : String.Empty;

            string existingCountryNameMessage = repeatedCountryNames.Count > 0
                ? $"Nom de pays existant trouvé. Noms: {String.Join(", ", repeatedCountryNames)}"
                : String.Empty;

            string existingEnglishCountryNameMessage = repeatedEnglishCountryNames.Count > 0
                ? $"Nom de pays anglais existant trouvé. Noms: {String.Join(", ", repeatedEnglishCountryNames)}"
                : String.Empty;

            string finalMessage = String.Join(" ", existingCountryCodeMessage, existingCountryNameMessage, existingEnglishCountryNameMessage);
            finalMessage = Regex.Replace(finalMessage, @"\s+", " ").Trim();

            await _context.SaveChangesAsync();
            return finalMessage;
        }
        private async Task<string> ImportOperateursAsync(ExcelWorksheet worksheet, int rowCount)
        {
            (int startRow, int startCol) = GetStartRowAndCol(worksheet);
            var columnMappings = GetColumnMappings(worksheet, startRow);

            // Charger tous les pays, opérateurs, groupes et type d'opérateur dans des dictionnaires pour une recherche plus rapide
            var paysList = await _context.Pays.ToListAsync();
            var uniquePaysList = paysList.GroupBy(p => p.Code_pays).Select(g => g.First()).ToList();
            var allPays = uniquePaysList
                .ToLookup(p => p.Code_pays, p => p.Nom_pays)
                .ToDictionary(g => g.Key, g => g.First());

            var allOperateurs = await _context.operateurs
                .GroupBy(o => o.Code_PLMN)
                .Select(g => g.First())
                .ToDictionaryAsync(o => o.Code_PLMN);
            var allGroupes = await _context.Groupe.ToDictionaryAsync(g => g.Nom_Groupe, g => g.Code_Groupe);
            var allTypeOps = await _context.LookupTable
                .Where(r => r.Lookup_Type == "Type_Operateur")
                .ToDictionaryAsync(r => r.Value, r => r.Id);
            var allPaysByNom = await _context.Pays.ToDictionaryAsync(p => p.Nom_pays);
            var allPaysByNomAng = await _context.Pays.ToDictionaryAsync(p => p.Nom_pays_anglais);

            // Obtenir les valeurs par défaut pour le type d'opérateur, le type d'accord et le groupe(if not exist in excel)
            var typeOp = await _context.LookupTable.FirstOrDefaultAsync(r => r.Lookup_Type == "Type_Operateur" && r.Value == "GSM");
            var typeAcc = await _context.LookupTable.FirstOrDefaultAsync(r => r.Lookup_Type == "Type_Accord" && r.Value == "DIRECT");
            var defaultGroup = await _context.Groupe.FirstOrDefaultAsync(g => g.Nom_Groupe == "Autres");
            if (typeOp == null)
            {
                return "Type operateur GSM est intouvable dans Lookuptable";
            }
            if (typeAcc == null)
            {
                // return an error message if typeOp is null
                return "Type Accord DIRECT est intouvable dans Lookuptable.";
            }
            if (defaultGroup == null)
            {
                defaultGroup = new Groupe { Nom_Groupe = "Autres" };
                _context.Groupe.Add(defaultGroup); // Ajoutez cette ligne
                await _context.SaveChangesAsync();
            }
            var defaultGroupeCode = defaultGroup.Code_Groupe;
            var newOperateurs = new List<Operateurs>();
            var missingPays = new List<string>();
            var missingOp = new List<string>();
            for (int row = startRow + 1; row < rowCount + startRow; row++)
            {
                var Code_PLMN = worksheet.Cells[row, columnMappings["PLMN"]].Value?.ToString().Trim();

                // Vérifiez que le Code_PLMN est valide et n'existe pas déjà
                if (string.IsNullOrEmpty(Code_PLMN) || allOperateurs.ContainsKey(Code_PLMN) || newOperateurs.Any(o => o.Code_PLMN == Code_PLMN))
                {
                    continue;
                }

                var codePays = Code_PLMN.Substring(0, 3);

                // Vérifiez que le code du pays existe dans la table pays
                var nomPays = worksheet.Cells[row, columnMappings["Pays"]].Value?.ToString().Trim();
                if (!allPays.ContainsKey(codePays))
                {
                    // Chercher le pays dans la base de données par son nom
                    Pays pays;
                    if (allPaysByNom.TryGetValue(nomPays, out var temp1))
                    {
                        pays = temp1;
                    }
                    else if (allPaysByNomAng.TryGetValue(nomPays, out var temp2))
                    {
                        pays = temp2;
                    }
                    else
                    {
                        missingPays.Add(nomPays);
                        continue;
                    }
                    codePays = pays.Code_pays;
                }
                // Obtenir les valeurs des autres colonnes
                var Nom_Operateur = worksheet.Cells[row, columnMappings["Opérateur Roaming"]].Value?.ToString().Trim();
                var mccmnc = worksheet.Cells[row, columnMappings["MCC-MNC"]].Value?.ToString().Trim();
                var nomGroupe = worksheet.Cells[row, columnMappings["Groupe"]].Value?.ToString().Trim() ?? "AUTRES";
                var nomTypeOp = worksheet.Cells[row, columnMappings["Type"]]?.Value?.ToString().Trim() ?? "GSM";

                // Obtenir le code du groupe et l'ID du type d'opérateur, en utilisant les valeurs par défaut si nécessaire
                var codeGroupe = allGroupes.TryGetValue(nomGroupe, out var temp) ? temp : defaultGroupeCode;
                var typeOpId = allTypeOps.TryGetValue(nomTypeOp, out temp) ? temp : typeOp.Id;

                // Extraire MCC et MNC de mccmnc, s'ils sont présents
                mccmnc = Regex.Replace(mccmnc ?? "", @"\D", "");// gardr que les chiffres
                if (mccmnc.Length>6)
                {
                    errorRows.Add(row);
                }
                var MCC = mccmnc?.Length >= 3 ? mccmnc.Substring(0, 3) : null;
                var MNC = mccmnc?.Length >= 5 ? mccmnc.Substring(3) : null;

                string opPriv = "non", rna = "non", raTerminated = "non";
                if (columnMappings.TryGetValue("Op Priv", out var opPrivCol))
                {
                    opPriv = worksheet.Cells[row, opPrivCol]?.Value?.ToString().Trim().ToLower() == "yes" ? "oui" : "non";
                }

                if (columnMappings.TryGetValue("RNA=Yes", out var rnaCol))
                {
                    rna = worksheet.Cells[row, rnaCol]?.Value != null ? "oui" : "non";
                }

                if (columnMappings.TryGetValue("Close Date si dispo", out var raTerminatedCol))
                {
                    raTerminated = worksheet.Cells[row, raTerminatedCol]?.Value != null ? "oui" : "non";
                }

                var newOperateur = new Operateurs
                {
                    Code_PLMN = Code_PLMN.ToUpper(),
                    Nom_Op = Nom_Operateur,
                    MCC = MCC,
                    MNC = MNC,
                    Op_prefered= opPriv,
                    RNA=rna,
                    RA_Teminated= raTerminated,
                    Code_pays = codePays,
                    TypeOperateurId = typeOpId,
                    TypeAccordId = typeAcc.Id,

                    Code_Groupe = codeGroupe
                };
                count++;
                newOperateurs.Add(newOperateur);
            }

            _context.operateurs.AddRange(newOperateurs);
            await _context.SaveChangesAsync();

            return $"Le groupe est attribué par défaut (créé s'il n'existe pas). Veuillez le mettre à jour avec 'import groupe'.\nPays introuvables : {string.Join(", ", missingPays)}";
        }
        private async Task<string> ImportLookupTableAsync(ExcelWorksheet worksheet, int rowCount)
        {
            (int startRow, int startCol) = GetStartRowAndCol(worksheet);

            var columnMappings = GetColumnMappings(worksheet, startRow);

            // Récupérez tous les enregistrements existants de LookupTable
            var existingRecordsQuery = _context.LookupTable
                .Select(lt => new { lt.Lookup_Type, lt.Value })
                .ToList();
            var existingRecords = new HashSet<(string, string)>(existingRecordsQuery.Select(x => (x.Lookup_Type, x.Value)));

            var errors = new List<string>();

            for (int col = startCol; col <= columnMappings.Count + startCol; col++)
            {
                string lookuptype = worksheet.Cells[startRow, col].Value?.ToString();

                if (string.IsNullOrEmpty(lookuptype))
                {
                    errors.Add($"Erreur : la colonne {col} est vide.");
                    continue;
                }
                bool valueExists = RequiredColumns["LookupTable"].Contains(lookuptype);

                if (valueExists)
                {
                    for (int row = startRow + 1; row <= rowCount + startRow - 1; row++)
                    {
                        string value = worksheet.Cells[row, col].Value?.ToString().Trim();

                        if (!existingRecords.Contains((lookuptype, value)) && !string.IsNullOrEmpty(value))
                        {
                            LookupTable newLookupTable = new LookupTable
                            {
                                Lookup_Type = lookuptype,
                                Value = value.ToUpper()
                            };
                            count++;
                            _context.LookupTable.Add(newLookupTable);           
                            existingRecords.Add((newLookupTable.Lookup_Type, newLookupTable.Value)); 
                        }
                    }
                    _cache.Set("LastModified", DateTime.UtcNow );

                }
                else
                {
                    errors.Add($"Erreur : le type de recherche '{lookuptype}' à la colonne {col} est invalide.");
                }
            }
            await _context.SaveChangesAsync();

            return string.Empty;
        }
        private async Task<string> ImportContactAsync(ExcelWorksheet worksheet, int rowCount)
        {
            (int startRow, int startCol) = GetStartRowAndCol(worksheet);
            var columnMappings = GetColumnMappings(worksheet, startRow);

            // Get all PLMN codes from Operateurs tables
            var operateurCodes = await _context.operateurs.Select(o => o.Code_PLMN).ToListAsync();

            // Lists to store row numbers where the PLMN code does not exist
            var missingInOperateurs = new List<int>();

            // List to store new Contact entities
            var newTarifs = new List<Contact>();
            var role = await _context.LookupTable.FirstOrDefaultAsync(r => r.Lookup_Type == "Role" && r.Value == "User");

            // HashSet to store unique entries from the Excel file
            var uniqueEntries = new HashSet<string>();
            var existingContacts = _context.contacts.Select(c => new { c.Code_PLMN, c.Type, c.Nom, c.Telephone, c.Email }).ToList();
            var existingContactsSet = new HashSet<string>(existingContacts.Select(c => $"{c.Code_PLMN},{c.Type},{c.Nom},{c.Telephone},{c.Email}"));

            for (int row = startRow + 1; row < rowCount + startRow; row++)
            {
                var code_plmn = worksheet.Cells[row, columnMappings["TADIG code"]].Value?.ToString().Trim();
                var type = worksheet.Cells[row, columnMappings["Type"]].Value?.ToString().Trim();
                var nom = worksheet.Cells[row, columnMappings["Name"]].Value?.ToString().Trim();
                var tel = worksheet.Cells[row, columnMappings["Phone"]].Value?.ToString().Trim();
                var email = worksheet.Cells[row, columnMappings["Email"]].Value?.ToString().Trim();

                // Vérifier si le code PLMN existe dans la liste des codes Operateurs
                if (!operateurCodes.Contains(code_plmn))
                {
                    missingInOperateurs.Add(row);
                    continue;
                }

                // Créer une chaîne de caractères représentant l'entrée unique
                var uniqueEntry = $"{code_plmn},{type},{nom},{tel},{email}";

                // Vérifiez si l'entrée est unique dans le fichier Excel
                if (!uniqueEntries.Add(uniqueEntry) || existingContactsSet.Contains(uniqueEntry))
                {
                    continue;
                }


                if (role != null)
                {
                    var newContact = new Contact
                    {
                        Code_PLMN = code_plmn,
                        Type = type,
                        Nom = nom,
                        Telephone = tel,
                        Email = email,
                        Role_id = role.Id,
                    };
                    count += 1;
                    newTarifs.Add(newContact);
                }
                else
                {
                    return ($"Erreur : le type Role dans LookupTable à la ligne {row} est invalide.");
                }
            }

            // Ajoutez toutes les nouvelles entités Contact à la base de données en une seule fois
            _context.contacts.AddRange(newTarifs);
            await _context.SaveChangesAsync();

            var errorMessages = new List<string>();
            if (missingInOperateurs.Any())
            {
                errorMessages.Add($"Erreur : les codes PLMN aux lignes {string.Join(", ", missingInOperateurs)} n'existent pas dans la table Operateurs.");
            }

            return string.Join(" ", errorMessages);
        }
        private async Task<string> ImportGroupeAsync(ExcelWorksheet worksheet, int rowCount)
        {
            (int startRow, int startCol) = GetStartRowAndCol(worksheet);
            var columnMappings = GetColumnMappings(worksheet, startRow);
            var allGroupes = await _context.Groupe.ToDictionaryAsync(g => g.Nom_Groupe.ToUpper().Trim());
            var allOperateurs = await _context.operateurs.ToDictionaryAsync(o => o.Code_PLMN);

            for (int row = startRow + 1; row <= rowCount; row++)
            {
                var Code_PLMN = worksheet.Cells[row, columnMappings["PLMN"]].Value?.ToString().Trim();
                var Nom_Groupe = worksheet.Cells[row, columnMappings["Groupe"]].Value?.ToString().Trim().ToUpper();

                if (string.IsNullOrEmpty(Nom_Groupe))
                {
                    continue;
                }
                if (!allGroupes.ContainsKey(Nom_Groupe))
                {
                    var newGroupe = new Groupe
                    {
                        Nom_Groupe = Nom_Groupe
                    };
                    count += 1;
                    _context.Groupe.Add(newGroupe);
                    await _context.SaveChangesAsync();
                    allGroupes.Add(Nom_Groupe, newGroupe); // Add new group to allGroupes
                }
                if (allOperateurs.TryGetValue(Code_PLMN, out var operateur))
                {
                    // If 'Code_Groupe' of 'operateur' does not match with 'Code_Groupe' of 'Nom_Groupe' in 'allGroupes', update it
                    if (operateur.Code_Groupe != allGroupes[Nom_Groupe].Code_Groupe)
                    {
                        operateur.Code_Groupe = allGroupes[Nom_Groupe].Code_Groupe;
                    }
                }
            }

            await _context.SaveChangesAsync();
            _cache.Set("LastModifiedGroupe", DateTime.UtcNow );
            return "Les groupes ont été importés avec succès et les opérateurs ont été mis à jour.";
        }
        private async Task<string> ImportServiceOuvertAsync(ExcelWorksheet worksheet, int rowCount)
        {
            (int startRow, int startCol) = GetStartRowAndCol(worksheet);
            var columnMappings = GetColumnMappings(worksheet, startRow);

            // Get all services from database.
            var allServices = await _context.LookupTable
                .Where(lt => lt.Lookup_Type == "Nom_Service")
                .ToDictionaryAsync(lt => lt.Value);

            // Map excel direction values to database direction values.
            var directionNames = new Dictionary<string, string>
                {
                    { "Bilatéral", "BILATERAL" },
                    { "Unilatéral IN", "RIN" },
                    { "Unilatéral OUT", "ROUT" },
                };

            // Get all directions from the database that are in the directionNames map.
            var allDirections = await _context.LookupTable
                .Where(lt => lt.Lookup_Type == "Direction" && directionNames.Values.Contains(lt.Value))
                .ToDictionaryAsync(lt => lt.Value);

            var allOperateurs = await _context.operateurs.ToDictionaryAsync(o => o.Code_PLMN);
            var newServicesOuverts = new List<ServiceOuvert>();
            var missingPlmns = new List<string>();
            var missingDirections = new List<string>();

            var existingServiceOuverts = await _context.serviceOuverts
                .Where(so => allOperateurs.Keys.Contains(so.Code_PLMN) && allServices.Values.Select(lt => lt.Id).Contains(so.Nom_Service_id))
                .Select(so => new { so.Code_PLMN, so.Nom_Service_id })
                .ToListAsync();

            var existingServiceOuvertSet = new HashSet<(string, int)>(existingServiceOuverts.Select(so => (so.Code_PLMN, so.Nom_Service_id)));

            for (int row = startRow + 1; row <= rowCount; row++)
            {
                var Code_PLMN = worksheet.Cells[row, columnMappings["PLMN"]].Value?.ToString().Trim();
                DateTime? date_d = null;
                string dateValue = worksheet.Cells[row, columnMappings["Date Ouverture"]].Value?.ToString().Trim();

                if (string.IsNullOrEmpty(dateValue))
                {
                    date_d = null;
                }
                else
                {
                    if (DateTime.TryParse(dateValue, out var parsedDate))
                    {
                        date_d = parsedDate;
                    }
                    else if (double.TryParse(dateValue, out var excelDate))
                    {
                        if (excelDate < 1 || excelDate >= 2958466)
                        {
                            return $"Date excel non valide ligne {row}";
                        }
                        else if (excelDate < 60)
                        {
                            date_d = new DateTime(1899, 12, 31).AddDays(excelDate);
                        }
                        else
                        {
                            date_d = new DateTime(1899, 12, 30).AddDays(excelDate);
                        }
                    }
                    else
                    {
                        return $"Erreur lors de l'analyse de la date '{dateValue}' à la ligne : {row}";
                    }
                }

                if (!allOperateurs.ContainsKey(Code_PLMN))
                {
                    missingPlmns.Add(Code_PLMN);
                    continue;
                }
                if (worksheet.Cells[row, columnMappings["Close Date si dispo"]].Value?.ToString().Trim() == "RA Terminated")
                {
                    var operateur = allOperateurs[Code_PLMN];
                    operateur.RA_Teminated = "oui";
                    _context.Entry(operateur).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                foreach (var columnMapping in columnMappings)
                {
                    var serviceName = columnMapping.Key;
                    var mappedServiceName = serviceName;
                    if (serviceName == "VOICE")
                    {
                        serviceName = "GSM";
                    }
                    var serviceValue = worksheet.Cells[row, columnMappings[mappedServiceName]].Value?.ToString().Trim();
                    if (string.IsNullOrEmpty(serviceValue) || !allServices.ContainsKey(serviceName.ToUpper()) || !directionNames.ContainsKey(serviceValue))
                    {
                        continue;
                    }

                    var directionKey = directionNames[serviceValue];

                    if (!allDirections.ContainsKey(directionKey))
                    {
                        if (!missingDirections.Contains(serviceValue))
                        {
                            missingDirections.Add(serviceValue);
                        }

                        continue;
                    }
                   
                    
                    if (existingServiceOuvertSet.Contains((Code_PLMN, allServices[serviceName].Id)))
                    {
                        continue;
                    }
                    var destination = worksheet.Cells[row, columnMappings["Destination"]].Value?.ToString().Trim();
                    if (destination.Length > 50)
                    {
                        destination = destination.Substring(0, 50);
                    }
                    var newServiceOuvert = new ServiceOuvert
                    {
                        Code_PLMN = Code_PLMN,
                        Nom_Service_id = allServices[serviceName].Id,
                        date_d = date_d,
                        Destination = destination,
                        Direction_id = allDirections[directionNames[serviceValue]].Id,
                    };

                    newServicesOuverts.Add(newServiceOuvert);
                    count++;
                }
            }
            _context.serviceOuverts.AddRange(newServicesOuverts);
            await _context.SaveChangesAsync();
            if (missingPlmns.Any())
            {
                return $"Les services ouverts ont été importés avec succès, mais les codes PLMN suivants n'existent pas : {string.Join(", ", missingPlmns)}";
            }
            return String.Empty;
        }
        private async Task<string> ImportIncidentsAsync(ExcelWorksheet worksheet, int rowCount)
        {
            (int startRow, int startCol) = GetStartRowAndCol(worksheet);
            var columnMappings = GetColumnMappings(worksheet, startRow);

            var allOperateurs = await _context.operateurs.ToDictionaryAsync(o => o.Code_PLMN);
            var allTypeIncidents = await _context.LookupTable
                .Where(r => r.Lookup_Type == "Type_Incident")
                .ToDictionaryAsync(r => r.Value, r => r.Id);
            var allDirections = await _context.LookupTable
                .Where(r => r.Lookup_Type == "Direction")
                .ToDictionaryAsync(r => r.Value, r => r.Id);

            var newIncidents = new List<Incident>();
            var missingOperateurs = new List<string>();
            var missingTypeIncidents = new HashSet<string>();
            var existingIncidents = await _context.incidents
                .Select(i => new { i.Code_PLMN, i.Code_TT, i.Commentaire })
                .ToListAsync();

            for (int row = startRow + 1; row < rowCount + startRow; row++)
            {
                var Code_PLMN = worksheet.Cells[row, columnMappings["PLMN"]].Value?.ToString().Trim();

                if (string.IsNullOrEmpty(Code_PLMN) || !allOperateurs.ContainsKey(Code_PLMN))
                {
                    missingOperateurs.Add(Code_PLMN);
                    continue;
                }

                var directionVal = worksheet.Cells[row, columnMappings["Direction"]].Value?.ToString().Trim().ToUpper();
                var serviceVal = worksheet.Cells[row, columnMappings["Service"]].Value?.ToString().Trim().ToUpper();
                var ticketNum = worksheet.Cells[row, columnMappings["N°Ticket"]].Value?.ToString().Trim();
                var commentaires = worksheet.Cells[row, columnMappings["Commentaires"]].Value?.ToString().Trim();
                var date_d = worksheet.Cells[row, columnMappings["Date Envoi"]].Value?.ToString();
                var date_f = worksheet.Cells[row, columnMappings["Date Résolution"]].Value?.ToString();

                var directionId = directionVal == "IN" ? allDirections["RIN"] : allDirections["ROUT"];

                int temp;
                var serviceId = allTypeIncidents.TryGetValue(serviceVal, out temp) ? (int?)temp : null;

                if (serviceId == null)
                {
                    missingTypeIncidents.Add(serviceVal);
                    continue;
                }

                if (existingIncidents.Any(i => i.Code_PLMN == Code_PLMN && i.Code_TT == ticketNum && i.Commentaire == commentaires))
                {
                    continue;
                }

                var newIncident = new Incident
                {
                    Code_PLMN = Code_PLMN,
                    Code_TT = ticketNum,
                    Commentaire = commentaires,
                    Direction_id = directionId,
                    Type_Incident_id = serviceId,
                    date_d = !string.IsNullOrEmpty(date_d) ? DateTime.Parse(date_d) : (DateTime?)null,
                    date_f = !string.IsNullOrEmpty(date_f) ? DateTime.Parse(date_f) : (DateTime?)null
                };

                existingIncidents.Add(new { Code_PLMN = newIncident.Code_PLMN, Code_TT = newIncident.Code_TT, Commentaire = newIncident.Commentaire });
                newIncidents.Add(newIncident);
                count++;
            }

            _context.incidents.AddRange(newIncidents);
            await _context.SaveChangesAsync();

            var errorMessages = new List<string>();
            if (missingOperateurs.Any())
                errorMessages.Add($"Opérateurs manquants : {string.Join(", ", missingOperateurs)}.");
            if (missingTypeIncidents.Any())
                errorMessages.Add($"Types d'incidents manquants : {string.Join(", ", missingTypeIncidents)}.");

            return string.Join("\n", errorMessages);

        }
        private async Task<int> AddNewLookupType(string lookupType, string value)
        {
            var newLookupType = new LookupTable
            {
                Lookup_Type = lookupType,
                Value = value
            };

            _context.LookupTable.Add(newLookupType);
            await _context.SaveChangesAsync();

            return newLookupType.Id;
        }

        private async Task<string> ImportTarifAsync(ExcelWorksheet worksheet, int rowCount)
        {
            (int startRow, int startCol) = GetStartRowAndCol(worksheet);
            var columnMappings = GetColumnMappings(worksheet, startRow);

            // Get all PLMN codes from Operateurs tables
            var operateurCodes = await _context.operateurs.Select(o => o.Code_PLMN).ToListAsync();

            // Get lookup values from the database
            var typeTraficLookup = await _context.LookupTable.Where(lt => lt.Lookup_Type == "Type_Trafic").ToDictionaryAsync(lt => lt.Value, lt => lt.Id);
            var typeTarifLookup = await _context.LookupTable.Where(lt => lt.Lookup_Type == "Type_tarif").ToDictionaryAsync(lt => lt.Value, lt => lt.Id);

            var directionLookup = await _context.LookupTable.Where(lt => lt.Lookup_Type == "Direction").ToDictionaryAsync(lt => lt.Value, lt => lt.Id);
            var incrementLookup = await _context.LookupTable.Where(lt => lt.Lookup_Type == "Increment").ToDictionaryAsync(lt => lt.Value, lt => lt.Id);

            // Lists to store row numbers where the PLMN code does not exist
            var missingInOperateurs = new List<string>();
            var missingInLookup = new List<string>();

            // List to store new Tarif entities
            var newTarifs = new List<Tarif>();

            // HashSet to store unique Tarifs (based on PLMN code, type traffic and rate)
            var uniqueTarifs = new HashSet<(string plmn, string typeTrafic, double rate)>();

            for (int row = startRow + 1; row <= rowCount; row++)
            {
                var code_plmn = worksheet.Cells[row, columnMappings["PLMN"]].Value?.ToString().Trim();
                var typeTrafic = worksheet.Cells[row, columnMappings["Type_Trafic"]].Value?.ToString().Trim();
                var typetarif = worksheet.Cells[row, columnMappings["Type_Tarif"]].Value?.ToString().Trim();
                string rateString = worksheet.Cells[row, columnMappings["Rate"]].Value?.ToString().Trim();
                double rate;

                //  avec le format français (virgule comme séparateur décimal)
                if (double.TryParse(rateString, NumberStyles.Any, new CultureInfo("fr-FR"), out rate))
                {
                }
                // avec le format américain (point comme séparateur décimal)
                else if (double.TryParse(rateString, NumberStyles.Any, new CultureInfo("en-US"), out rate))
                {
                }
                else
                {
                    // La conversion a échoué avec les deux formats format inconus 
                }

                var direction = worksheet.Cells[row, columnMappings["Direction"]].Value?.ToString().Trim();
                var increment = worksheet.Cells[row, columnMappings["Increment"]].Value?.ToString().Trim();
                DateTime? dateStart = null;
                DateTime? dateEnd = null;

                string dateStartString = worksheet.Cells[row, columnMappings["Date_Start"]].Value?.ToString().Trim();
                string dateEndString = worksheet.Cells[row, columnMappings["End_Date"]].Value?.ToString().Trim();

                if (!string.IsNullOrEmpty(dateStartString))
                {
                    dateStart = DateTime.Parse(dateStartString);
                }

                if (!string.IsNullOrEmpty(dateEndString))
                {
                    dateEnd = DateTime.Parse(dateEndString);
                }
                var commentaire = worksheet.Cells[row, columnMappings["Commentaire"]].Value?.ToString().Trim();
                var autoRenewal = worksheet.Cells[row, columnMappings["Auto_Renewal"]].Value?.ToString().Trim();
                var devise = worksheet.Cells[row, columnMappings["DEVISE"]].Value?.ToString().Trim();
                double? exchangeRate = null;

                string exchangeRateString = worksheet.Cells[row, columnMappings["Exchange_Rate"]].Value?.ToString().Trim();

                if (!string.IsNullOrEmpty(exchangeRateString))
                {
                    exchangeRate = Double.Parse(exchangeRateString);
                }

                int directionId = 0;

                if (direction == "IN & OUT")
                {
                    directionId = directionLookup.ContainsKey(direction) ? directionLookup[direction] : directionLookup["BILATERAL"];
                }
                else if (direction == "IN")
                {
                    directionId = directionLookup.ContainsKey(direction) ? directionLookup[direction] : directionLookup["UNILATERAL IN"];
                }
                else if (direction == "OUT")
                {
                    directionId = directionLookup.ContainsKey(direction) ? directionLookup[direction] : directionLookup["UNILATERAL OUT"];
                }

                uniqueTarifs.Add((code_plmn, typeTrafic, rate)); 


                if (!operateurCodes.Contains(code_plmn))
                {
                    missingInOperateurs.Add(code_plmn);
                    continue;
                }
                if (!typeTarifLookup.ContainsKey(typetarif))
                {
                    missingInLookup.Add($"'{typetarif}' ligne {row}");
                    continue;
                }

                var typetarifId = typeTarifLookup[typetarif];

                if (!typeTraficLookup.ContainsKey(typeTrafic))
                {
                    missingInLookup.Add($"'{typeTrafic}' ligne {row}");
                    continue;
                }

                var typetraficId = typeTraficLookup[typeTrafic];

                if (!incrementLookup.ContainsKey(increment))
                {
                    missingInLookup.Add($"L'Increment '{increment}'ligne {row}");
                    continue;
                }
                var incrementId = incrementLookup[increment];

                if (!string.IsNullOrEmpty(exchangeRateString))
                {
                    exchangeRate = Double.Parse(exchangeRateString);
                }
                bool existeDeja = await _context.tarifs.AnyAsync(t =>
                    t.Code_PLMN == code_plmn &&
                    t.Type_Trafic_id == typetraficId &&
                    t.Rate == rate &&
                    t.Direction_id == directionId &&
                    t.Increment_id == incrementId &&
                    t.Type_Tarif_id == typetarifId &&
                    t.Date_d == dateStart &&
                    t.Date_f == dateEnd &&
                    t.Commentaire == commentaire &&
                    t.Auto_Renwal == autoRenewal &&
                    t.Exchange_rate == exchangeRate &&
                    t.Devis == devise
                    );

                if (existeDeja)
                {
                    continue;
                }
                var tarif = new Tarif
                {
                    Code_PLMN = code_plmn,
                    Type_Trafic_id = typetraficId,
                    Rate = rate,
                    Direction_id = directionId,
                    Increment_id = incrementId,
                    Type_Tarif_id = typetarifId,
                    Date_d = dateStart,
                    Date_f = dateEnd,
                    Commentaire = commentaire,
                    Auto_Renwal = autoRenewal,
                    Exchange_rate = exchangeRate,
                    Devis = devise,
                };
                count++;
                newTarifs.Add(tarif);
            }

            _context.tarifs.AddRange(newTarifs);
            await _context.SaveChangesAsync();


            if (missingInOperateurs.Any())
            {
                return $"Les codes PLMN suivants n'existent pas dans la table Operateurs : {string.Join(", ", missingInOperateurs)}";
            }
            if (missingInLookup.Any())
            {
                return $"Lookup resource manquants : {string.Join(", ", missingInLookup)}";
            }
            return string.Empty;
        }


        private (int startRow, int startCol) GetStartRowAndCol(ExcelWorksheet worksheet)
        {
            int startRow = worksheet.Dimension.Start.Row;
            int startCol = worksheet.Dimension.Start.Column;
            return (startRow, startCol);
        }

        private Dictionary<string, int> GetColumnMappings(ExcelWorksheet worksheet, int headerRow)
        {
            var mappings = new Dictionary<string, int>();

            for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
            {
                var columnName = worksheet.Cells[headerRow, col].Value?.ToString().Trim();

                if (!string.IsNullOrEmpty(columnName))
                {
                    mappings[columnName] = col;
                }
            }

            return mappings;
        }


        public List<string> GetClassNamesFromNamespace(string namespaceFilter)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var classNames = new List<string>();

            foreach (Type type in assembly.GetTypes())
            {
                if (type.Namespace == namespaceFilter && type.IsClass)
                {
                    classNames.Add(type.Name);
                }
            }

            return classNames;
        }
        private async Task<bool> CheckImportPermissions(string importType)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return false;
            }
            var roles = await _userManager.GetRolesAsync(user);

            // Si l'utilisateur est un admin ou un manager, il a accès à tout par défaut
            if (roles.Contains("Admin") || roles.Contains("Manager"))
            {
                return true;
            }

            var permissionHandler = new PermissionHandler(_userManager, _context);
            return await permissionHandler.HasPermissionAsync(user.Id, importType, "edit");
        }

    }
}
