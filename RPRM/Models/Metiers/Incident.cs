using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RPRM.Models.Metiers
{
    [Table("incident")]
    public class Incident
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Code_Incident { get; set; }

        [ForeignKey("Operateur")]
        [StringLength(5)]
        public string Code_PLMN { get; set; }
        public virtual Operateurs Operateur { get; set; }

        [StringLength(15)]
        public string? IMSI { get; set; }

        [StringLength(20)]
        public string? MSISDN { get; set; }

        [Column(TypeName = "date")]
        public DateTime? date_d { get; set; }

        [Column(TypeName = "date")]
        public DateTime? date_f { get; set; }

        public string? Commentaire { get; set; }

        [StringLength(20)]
        public string? Code_TT { get; set; }

        [ForeignKey("TypeIncidentLookup")]
        public int? Type_Incident_id { get; set; }
        public virtual LookupTable TypeIncidentLookup { get; set; }

        [ForeignKey("DirectionLookup")]
        public int? Direction_id { get; set; }
        public virtual LookupTable DirectionLookup { get; set; }
    }
}
