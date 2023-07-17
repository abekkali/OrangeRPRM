using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RPRM.Models.Metiers
{
    [Table("doc_operateur")]
    public class DocOperateur
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Code_DOC { get; set; }

        [ForeignKey("Operateur")]
        [StringLength(5)]
        public string Code_PLMN { get; set; }
        public virtual Operateurs Operateur { get; set; }

        [StringLength(255)]
        public string Document { get; set; }

        [ForeignKey("TypeDocLookup")]
        public int? Type_Doc_id { get; set; }
        public virtual LookupTable TypeDocLookup { get; set; }

        [Column(TypeName = "date")]
        public DateTime? date_d { get; set; }

        [Column(TypeName = "date")]
        public DateTime? date_f { get; set; }
    }
}
