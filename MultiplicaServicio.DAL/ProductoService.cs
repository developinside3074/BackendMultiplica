using BackendMultiplica.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace MultiplicaServicio.DAL
{
    public class ProductoService
    {
        public static List<Producto> ObtenerTodosFromXML() {

            var doc = XElement.Load(ConfigurationManager.AppSettings["origen"]);
            var s = new XmlSerializer(typeof(List<Producto>));
            var productos = s.Deserialize(doc.CreateReader()) as List<Producto>;
            return productos;
        }

        // Recuperar la lista de productos del archivo en formato XML
        public static List<Producto> ObtenerTodosProductosFromXML()
        {

            return (from producto in ProductoService.ObtenerTodosFromXML() select producto).ToList();
        }

        // Recuperar un producto por su identificador unico
        public static Producto ObtenerProductoPorIDFromXML( int id)
        {

            return (from producto in ProductoService.ObtenerTodosFromXML() where producto.ProductoId.Equals(id) select producto).FirstOrDefault();
        }


    }

   
}
