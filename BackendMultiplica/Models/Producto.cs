using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BackendMultiplica.Models
{
    //[Serializable]
    public class Producto
    {
       

        [Key]
        public long ProductoId { get; set; }
        [Required]
        [StringLength(45)]
        [MinLength(2)]
        public string Nombre { get; set; }
        [Required]
        public decimal Precio { get; set; }
        [Required]
        public int Stock { get; set; }

        [ForeignKey("Categoria")]
        public int CategoriaId { get; set; }

        public Producto()
        {
        }

        public virtual Categoria Categoria { get; set; }



        public override string ToString()
        {
            return "Producto: " + Nombre + " Precio: " + Precio + " Stock: " + Stock;
        }

        public static explicit operator Producto(DbSet<Producto> v)
        {
            throw new NotImplementedException();
        }
    }
}