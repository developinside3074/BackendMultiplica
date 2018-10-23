using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BackendMultiplica.Models
{
    public class Categoria
    {
        [Key]
        public int CategoriaId { get; set; }
        [Required]
        [StringLength(100)]
        [MinLength(2)]
        public string Descripcion { get; set; }
    }
}