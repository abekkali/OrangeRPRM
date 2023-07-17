using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RPRM.Models.Metiers
{
    [Table("network_info")]
    public class NetworkInfo
    {
        [Key]
        [Column("Code_Info")]
        public int CodeInfo { get; set; }

        [Column("Code_PLMN", TypeName = "varchar(5)")]
        public string CodePLMN { get; set; }

        [Column("Type_Info_id")]
        public int? TypeInfoId { get; set; }

        [Column("Valeur", TypeName = "varchar(50)")]
        public string Valeur { get; set; }

        // Foreign key relationships
        [ForeignKey(nameof(CodePLMN))]
        public virtual Operateurs Operateur { get; set; }

        [ForeignKey(nameof(TypeInfoId))]
        public virtual LookupTable TypeInfo { get; set; }
    }

}
