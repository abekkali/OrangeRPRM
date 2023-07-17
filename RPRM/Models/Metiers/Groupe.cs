using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RPRM.Models.Metiers
{
    public class Groupe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Code_Groupe { get; set; }

        [StringLength(50)]
        public string Nom_Groupe { get; set; }

        public int? Eng_Val_In { get; set; }

        public int? Eng_Val_out { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Date_d { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Date_f { get; set; }
    }
}
