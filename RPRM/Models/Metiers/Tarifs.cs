using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RPRM.Models.Metiers
{
    [Table("tarifs")]
    public class Tarif
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Code_Tarif { get; set; }

        [ForeignKey("Operateur")]
        [StringLength(5)]
        public string Code_PLMN { get; set; }
        public virtual Operateurs Operateur { get; set; }

        [ForeignKey("TypeTraficLookup")]
        public int Type_Trafic_id { get; set; }
        public virtual LookupTable TypeTraficLookup { get; set; }

        [ForeignKey("TypeTarifLookup")]
        public int Type_Tarif_id { get; set; }
        public virtual LookupTable TypeTarifLookup { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Date_d { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Date_f { get; set; }

        [ForeignKey("IncrementLookup")]
        public int Increment_id { get; set; }
        public virtual LookupTable IncrementLookup { get; set; }

        public double? Exchange_rate { get; set; }

        public double? Rate { get; set; }

        [StringLength(255)]
        public string? Commentaire { get; set; }

        [ForeignKey("DirectionLookup")]
        public int Direction_id { get; set; }
        public virtual LookupTable DirectionLookup { get; set; }

        [Column(TypeName = "enum")]
        public string? Auto_Renwal { get; set; }

        [StringLength(255)]
        public string? Devis { get; set; }

        [StringLength(255)]
        public string? Document_DCH { get; set; }

        [StringLength(255)]
        public string? Contact { get; set; }
    }
}
