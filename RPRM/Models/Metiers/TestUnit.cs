using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RPRM.Models.Metiers
{
    [Table("test_unit")]
    public class TestUnit
    {
        [Key]
        [Column("Code_test")]
        public int CodeTest { get; set; }

        [Column("Direction_id")]
        public int? DirectionId { get; set; }

        [ForeignKey("Direction_id")]
        public virtual LookupTable Direction { get; set; }

        [Column("Nom_Service_id")]
        public int? NomServiceId { get; set; }

        [ForeignKey("Nom_Service_id")]
        public virtual LookupTable NomService { get; set; }

        [Column("Etat_Test_id")]
        public int? EtatTestId { get; set; }

        [ForeignKey("Etat_Test_id")]
        public virtual LookupTable EtatTest { get; set; }

        [Column("Commentaire_long")]
        public string CommentaireLong { get; set; }

        [Column("New_Dest")]
        public string NewDest { get; set; }

        [Column("Afrique")]
        public string Afrique { get; set; }

        [Column("Privilégié")]
        public string Privilegie { get; set; }

        [Column("Engagement")]
        public string Engagement { get; set; }

        [Column("Groupe_Privilégié")]
        public string GroupePrivilegie { get; set; }

        [Column("Test_Owner_id")]
        public int? TestOwnerId { get; set; }

        [ForeignKey("Test_Owner_id")]
        public virtual LookupTable TestOwner { get; set; }

        [Column("date_d")]
        public DateTime? DateDebut { get; set; }

        [Column("date_fin")]
        public DateTime? DateFin { get; set; }
    }
}
