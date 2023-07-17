using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RPRM.Models.Metiers
{
    [Table("contact")]
    public class Contact
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Code_Contact { get; set; }

        [ForeignKey("Operateur")]
        [StringLength(5)]
        public string Code_PLMN { get; set; }
        public virtual Operateurs Operateur { get; set; }

        [StringLength(60)]
        public string? Type { get; set; }
        [StringLength(200)]
        public string? Nom { get; set; }
        [StringLength(20)]
        public string? Telephone { get; set; }
        [StringLength(110)]
        public string? Email { get; set; }

        [ForeignKey("RoleLookup")]
        public int Role_id { get; set; }
        public virtual LookupTable RoleLookup { get; set; }
       
    }
}
