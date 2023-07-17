using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RPRM.Models.Metiers
{
    [Table("sim_received")]
    public class SimReceived
    {
        [Key]
        [Column("Code_SIM")]
        public int CodeSim { get; set; }

        [Column("Code_PLMN")]
        [StringLength(5)]
        public string CodePLMN { get; set; }

        [ForeignKey("Code_PLMN")]
        public virtual Operateurs Operateur { get; set; }

        [Column("IMSI")]
        [StringLength(15)]
        public string IMSI { get; set; }

        [Column("MSISDN")]
        [StringLength(20)]
        public string MSISDN { get; set; }

        [Column("date_d")]
        public DateTime? DateDebut { get; set; }

        [Column("date_f")]
        public DateTime? DateFin { get; set; }
    }
}
