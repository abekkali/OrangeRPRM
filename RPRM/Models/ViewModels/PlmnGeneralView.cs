using RPRM.Models.Metiers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RPRM.Models.ViewModels
{
    public class PlmnGeneralView
    {
        public string Code_PLMN { get; set; }
        public string Nom_Op { get; set; }
        public string TypeOperateur { get; set; }
        public string TypeAccord { get; set; }
        public string NomPays { get; set; }
        public string Groupe { get; set; }
        public int NbOperateurs { get; set; }
        public List<IncidentViewModel> Incidents { get; set; }
        public List<ContactViewModel> Contacts { get; set; }
        public List<ServiceOuvertsViewModel> ServiceOuverts { get; set; }
        public List<DocumentViewModel> Documents { get; set; }
        public List<TarifViewModel> tarifs { get; set; }


    }
}
