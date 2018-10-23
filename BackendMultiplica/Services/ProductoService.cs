using BackendMultiplica.Models;
using BackendMultiplica.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using AppContext = BackendMultiplica.Models.AppContext;

namespace BackendMultiplica.Services
{
    public class ProductoService
    {
        public static void _Añadir(Producto prod)
        {
            XmlDocument documento = new XmlDocument();
            ProductoService productoService = new ProductoService();
            //var doc = XElement.Load(ConfigurationManager.AppSettings["origen"]);
            //documento.Load(ConfigurationManager.AppSettings["origen"]);
            documento.Load(Constante.FILE_PATH_XML_PRODUCTO);

            XmlNode producto = CrearProducto(prod, documento);          

            XmlNode nodoRaiz = documento.DocumentElement;

            nodoRaiz.InsertAfter(producto, nodoRaiz.LastChild);
            documento.Save(Constante.FILE_PATH_XML_PRODUCTO);

        }

        private static XmlNode CrearProducto(Producto prod, XmlDocument documento)
        {
            
            // Crear el nodo Producto contenedor
            XmlNode producto = documento.CreateElement(Constante.NODE_ELEMENT_NAME_PRODUCTO);
            XmlNode categoria = documento.CreateElement(Constante.NODE_ELEMENT_NAME_CATEGORIA);

            // Agregar el ID a al nodo producto (unico para cada producto)
            Int64 id = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmssff"));

            XmlElement xid = documento.CreateElement("ProductoId");
            xid.InnerText = id.ToString();
            producto.AppendChild(xid);

            // Agregar el nombre al node producto
            XmlElement xnombre = documento.CreateElement("Nombre");
            xnombre.InnerText = prod.Nombre;
            producto.AppendChild(xnombre);

            // Agregar el precio al node producto
            XmlElement xprecio = documento.CreateElement("Precio");
            xprecio.InnerText = prod.Precio.ToString();
            producto.AppendChild(xprecio);

            // Agregar el stock al node producto
            XmlElement xstock = documento.CreateElement("Stock");
            xstock.InnerText = prod.Stock.ToString();
            producto.AppendChild(xstock);

            // Agregar la categoria al node producto
            XmlElement xcategoriaid = documento.CreateElement("CategoriaId");
            xcategoriaid.InnerText = prod.CategoriaId.ToString();
            producto.AppendChild(xcategoriaid);
           
            return producto;
        }

        public static List<Producto> ObtenerTodosFromXML()
        {
            List<Producto> productos = new List<Producto>();
           
            XmlDocument documento = new XmlDocument();             

            documento.Load(Constante.FILE_PATH_XML_PRODUCTO);

            XmlNodeList listadoProductos = documento.SelectNodes("Productos/Producto");

            XmlNode unProducto;          

            for (int i = 0; i < listadoProductos.Count; i++) {

                Producto productoItem = new Producto();
                unProducto = listadoProductos.Item(i);

                productoItem.ProductoId = Convert.ToInt64(unProducto.SelectSingleNode("ProductoId").InnerText);
                productoItem.Nombre = unProducto.SelectSingleNode("Nombre").InnerText;
                productoItem.Precio = Convert.ToDecimal(unProducto.SelectSingleNode("Precio").InnerText);
                productoItem.Stock = Convert.ToInt32(unProducto.SelectSingleNode("Stock").InnerText);
                productoItem.CategoriaId = Convert.ToInt32(unProducto.SelectSingleNode("CategoriaId").InnerText);               

                productos.Insert(i, productoItem);
                productoItem = null;
            }           

            return productos;
        }

        // Modificar el producto en el archivo XML
        public static Producto ModificarProductoFromXml(Producto prod)
        {
            long nuevoId = 0;

            XmlDocument documento = new XmlDocument();

            documento.Load(Constante.FILE_PATH_XML_PRODUCTO);

            XmlElement productos = documento.DocumentElement;

                      

            XmlNodeList listadoProductos = documento.SelectNodes("Productos/Producto");

            XmlNode nuevo_producto = CrearProducto(prod, documento);

            foreach (XmlNode item in listadoProductos) {

                if (Convert.ToInt64(item.SelectSingleNode("ProductoId").InnerText) == prod.ProductoId) {

                    XmlNode nodoViejo = item;
                    productos.ReplaceChild(nuevo_producto, nodoViejo);
                    nuevoId = Convert.ToInt64(nuevo_producto.SelectSingleNode("ProductoId").InnerText);
                }
            }

            documento.Save(Constante.FILE_PATH_XML_PRODUCTO);

            return ObtenerProductoPorIDFromXML(nuevoId);
        }

        // Borrar producto por su identificador unico en el archivo XML
        public static void BorrarProductoFromXml(long productoId) {

            XmlDocument documento = new XmlDocument();

            documento.Load(Constante.FILE_PATH_XML_PRODUCTO);

            XmlElement productos = documento.DocumentElement;

            XmlNodeList listadoProductos = documento.SelectNodes("Productos/Producto");

            foreach (XmlNode item in listadoProductos) {

                if (Convert.ToInt64(item.SelectSingleNode("ProductoId").InnerText) == productoId) {

                    XmlNode nodoViejo = item;

                    productos.RemoveChild(nodoViejo);
                }
            }

            documento.Save(Constante.FILE_PATH_XML_PRODUCTO);

        }
       

        // Recuperar un producto por su identificador unico
        public static Producto ObtenerProductoPorIDFromXML(Int64 id)
        {
            Producto producto = null;


            if (File.Exists(Constante.FILE_PATH_XML_PRODUCTO))
            {
                return producto;
            }

            XmlDocument documento = new XmlDocument();

            try {
                documento.Load(Constante.FILE_PATH_XML_PRODUCTO);
            }
            catch (Exception) {
                return null;
            }       

            XmlElement productos = documento.DocumentElement;

            XmlNodeList listadoProductos = documento.SelectNodes("Productos/Producto");

            foreach (XmlNode item in listadoProductos)
            {

                if (Convert.ToInt64(item.SelectSingleNode("ProductoId").InnerText) == id)
                {
                    
                    XmlNode nodoEcontrado = item;

                    producto.ProductoId = Convert.ToInt64(nodoEcontrado.SelectSingleNode("ProductoId").InnerText);
                    producto.Nombre = nodoEcontrado.SelectSingleNode("Nombre").InnerText;
                    producto.Precio = Convert.ToDecimal(nodoEcontrado.SelectSingleNode("Precio").InnerText);
                    producto.Stock = Convert.ToInt32(nodoEcontrado.SelectSingleNode("Stock").InnerText);
                    producto.CategoriaId = Convert.ToInt32(nodoEcontrado.SelectSingleNode("CategoriaId").InnerText);
                    return producto;
                }

                
            }

            return producto;
        }
    }
}