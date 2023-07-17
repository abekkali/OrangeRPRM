using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RPRM.Models.Metiers
{
    [Table("services_ouverts")]
    public class ServiceOuvert
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Code_Service { get; set; }

        [ForeignKey("Operateur")]
        [StringLength(5)]
        public string Code_PLMN { get; set; }
        public virtual Operateurs Operateur { get; set; }
        [StringLength(50)]
        public string? Destination { get; set; }

        [ForeignKey("NomServiceLookup")]
        public int Nom_Service_id { get; set; }
        public virtual LookupTable NomServiceLookup { get; set; }

        [Column(TypeName = "date")]
        public DateTime? date_d { get; set; }

        [Column(TypeName = "date")]
        public DateTime? date_f { get; set; }

        [ForeignKey("DirectionLookup")]
        public int Direction_id { get; set; }
        public virtual LookupTable DirectionLookup { get; set; }
    }
}
