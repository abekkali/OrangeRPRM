using System.ComponentModel.DataAnnotations;

namespace RPRM.Models.ViewModels
{
    public class ContactViewModel
    {
        public string? Type { get; set; }
        public string? Nom { get; set; }
        public string? Telephone { get; set; }
     
        public string? Email { get; set; }
    }
}
