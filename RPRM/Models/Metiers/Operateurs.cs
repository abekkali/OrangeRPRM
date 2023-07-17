using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RPRM.Models.Metiers
{
    public class Operateurs
    {
        [Key]
        [Required]
        [StringLength(5)]
        public string Code_PLMN { get; set; }

        [StringLength(3)]
        [ForeignKey("Pays")]
        public string Code_pays { get; set; }

        [StringLength(100)]
        public string Nom_Op { get; set; }

        [StringLength(3)]
        public string? MCC { get; set; }

        [StringLength(3)]
        public string ?MNC { get; set; }

        public int? Marketshare { get; set; }

        [Column(TypeName = "enum('oui', 'non')")]
        public string? Op_prefered { get; set; }

        [Column(TypeName = "enum('oui', 'non')")]
        public string? RNA { get; set; }

        [Column(TypeName = "enum('oui', 'non')")]
        public string? RA_Teminated { get; set; }

        [Required]
        [Column("Type_operateur_id")]
        public int TypeOperateurId { get; set; }

        [Required]
        [Column("type_accord_id")]
        public int TypeAccordId { get; set; }

        [Required]
        [ForeignKey("Groupe")]
        public int Code_Groupe { get; set; }

        // Navigation properties

        public virtual Pays Pays { get; set; }
        public virtual Groupe Groupe { get; set; }
        [ForeignKey(nameof(TypeOperateurId))]
        public virtual LookupTable TypeOperateur { get; set; }

        [ForeignKey(nameof(TypeAccordId))]
        public virtual LookupTable TypeAccord { get; set; }
    }
}
