using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RPRM.Models.Metiers
{
    [Table("lookUp_table")]
    public class LookupTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("Lookup_Type")]
        public string Lookup_Type { get; set; }

        [StringLength(50)]
        public string Value { get; set; }
    }
}
