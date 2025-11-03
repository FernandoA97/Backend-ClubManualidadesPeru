using System;
using System.ComponentModel.DataAnnotations;

namespace MachineLearning.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string NombreCliente { get; set; }

        [MaxLength(50)]
        public string Telefono { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(300)]
        public string Direccion { get; set; }

        [MaxLength(50)]
        public string TipoCliente { get; set; }

        [MaxLength(50)] 
        public string RUC { get; set; }
    }

}
