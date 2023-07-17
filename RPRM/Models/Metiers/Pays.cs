using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPRM.Models.Metiers
{
    /// <summary>
    /// Représente un pays avec ses propriétés associées.
    /// </summary>
    public class Pays
    {
        /// <summary>
        /// Obtient ou définit le code du pays.
        /// </summary>
        [Key]
        [StringLength(3)]
        public string Code_pays { get; set; }

        /// <summary>
        /// Obtient ou définit le code de la zone CC (Country Code).
        /// </summary>
        [StringLength(4)]
        public string CC { get; set; }

        /// <summary>
        /// Obtient ou définit le nom du pays.
        /// </summary>
        [StringLength(100)]
        public string Nom_pays { get; set; }
        [StringLength(100)]
        public string Nom_pays_anglais { get; set; }

        /// <summary>
        /// Obtient ou définit le continent auquel appartient le pays.
        /// </summary>
        [StringLength(20)]
        public string Continent { get; set; }

        /// <summary>
        /// Obtient ou définit la région à laquelle appartient le pays.
        /// </summary>
        [StringLength(20)]
        public string Region { get; set; }

        /// <summary>
        /// Obtient ou définit le code MCC (Mobile Country Code) du pays.
        /// </summary>
        [StringLength(3)]
        public string MCC { get; set; }

        /// <summary>
        /// Obtient ou définit si le pays a un accès au PASS.
        /// Les valeurs possibles sont "oui" et "non".
        /// </summary>
        [Column(TypeName = "enum('oui','non')")]
        public string Pass { get; set; } = "non";
    }
}
