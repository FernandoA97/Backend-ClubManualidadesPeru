using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace MachineLearning.Models
{

    [Table("HistorialImportacion")]
    public class HistorialImportacion
    {
        [Key]
        public int Id { get; set; }
        public DateTime FechaImportacion { get; set; }
        public string Usuario { get; set; }
        public int RegistrosValidos { get; set; }
        public int RegistrosRechazados { get; set; }
        public int RegistrosDuplicados { get; set; }
        public string NombreArchivo { get; set; }
        public string Estado { get; set; }
    }
}
