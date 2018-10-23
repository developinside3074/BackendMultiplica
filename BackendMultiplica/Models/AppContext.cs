using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BackendMultiplica.Models
{
    public class AppContext: DbContext
    {
        public AppContext()
            :base("DefaultConnection")
        { }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
    }
}