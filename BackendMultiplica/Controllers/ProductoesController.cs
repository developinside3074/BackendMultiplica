using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml;
using System.Xml.Linq;
using BackendMultiplica.Models;
using BackendMultiplica.Services;
using BackendMultiplica.Utils;
using log4net;
using log4net.Config;
using Newtonsoft.Json.Linq;
using AppContext = BackendMultiplica.Models.AppContext;

namespace BackendMultiplica.Controllers
{
    [RoutePrefix("api/productos")]    
    public class ProductoesController : ApiController
    {
        private AppContext db = new AppContext();

        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // GET: api/Productoes/origen/bd
        // GET: api/Productoes/origen/xml
        [Route("origen/{origen:alpha}")]
        [HttpGet]
        public HttpResponseMessage GetProductos(string origen)
        {
            List<Producto> productos = new List<Producto> { };
            List<Producto> productos_out = new List<Producto> { };

            XmlConfigurator.Configure();

            _log.Info("Solicitud al metodo GetProductos() ");

            try {

                if (origen.ToUpper() == Destino.DB)
                {
                    _log.Info("Recuperando la informacion desde el origen de Base de Datos");
                    productos = db.Productos.ToList();
                    _log.Info("Informacion recuperada: " + productos);                   
                    return Request.CreateResponse(HttpStatusCode.Accepted, productos);
                    
                }
                if (origen.ToUpper() == Destino.XML)
                {
                    Categoria categoria;

                    _log.Info("Recuperando la informacion desde el origen XML");

                    // Retornar lista vacia, sino hay archivo xml
                    if ((!File.Exists(Constante.FILE_PATH_XML_PRODUCTO))) {

                        return Request.CreateResponse(HttpStatusCode.Accepted, productos);
                    }

                    productos = ProductoService.ObtenerTodosFromXML();

                    //Recupera las categorias por producto y añadirla al producto en cuestion
                    productos.ForEach(prod => {

                        categoria = db.Categorias.Find(prod.CategoriaId);
                        prod.Categoria = categoria;
                        productos_out.Add(prod);

                    });

                    _log.Info("Informacion recuperada: " + productos);
                    return Request.CreateResponse(HttpStatusCode.Accepted, productos_out);                  

                }
                

            }
            catch (Exception e) {
                _log.Error("Error al solicitar informacion a GetProductos()" + e.Source);
                
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest, "El origen de los datos que ha especificado esta erroneo, posibles origenes correctos db o xml");
        }

        // GET: api/Productoes/2/origen/bd
        // GET: api/Productoes/54655654/origen/xml
        [Route("{id:long}/origen/{origen:alpha}")]
        [HttpGet]
        [ResponseType(typeof(Producto))]
        public HttpResponseMessage GetProducto(long id, string origen)
        {
            Producto producto = null;          

            if (origen.ToUpper().Equals(Destino.DB))
            {
                _log.Info("Solicitud de informacion de producto de origen Base de Datos, metodo responsable GetProducto()");
                producto = db.Productos.Find(id);

            }

            else if (origen.ToUpper().Equals(Destino.XML))
            {
                _log.Info("Solicitud de informacion de producto de origen XML, metodo responsable GetProducto()");
                producto = ProductoService.ObtenerProductoPorIDFromXML(id);

                if (producto == null) {
                    _log.Info("El Producto con el identificador " + id + " no fue encontrado");
                    return Request.CreateResponse(HttpStatusCode.NotFound, "El Producto con el identificador " + id + " no fue encontrado con el destino " + origen);
                }

                // Recuperar la categoria y asignarla al producto en cuestion
                Categoria categoria = db.Categorias.Find(producto.CategoriaId);
                producto.Categoria = categoria;
            }

            else {

                _log.Error("Error de argumentos, usted no especifico el origen de los datos, posibles origenes BD o XML");
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Error de argumentos, usted no especifico el origen de los datos, posibles origenes BD o XML");
            }

            if (producto == null)
            {
                _log.Info("El Producto con el identificador " + id + " no fue encontrado");
                return Request.CreateResponse(HttpStatusCode.NotFound, "El Producto con el identificador " + id + " no fue encontrado");
            }

            _log.Info("Producto recuperado: " + producto);
            return Request.CreateResponse(HttpStatusCode.OK, producto); 
        }

        // PUT: api/Productoes/origen/bd
        // PUT: api/Productoes/origen/xml
        [Route("origen/{origen:alpha}")]
        [HttpPut]
        [ResponseType(typeof(void))]
        public HttpResponseMessage PutProducto(Producto producto, string origen)
        {
            if (!ModelState.IsValid)
            {
                _log.Error(" Existen problemas con los parametros recibidos, no se puede procesar dicha solicitud");               
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            }          


            if (origen.ToUpper().Equals(Destino.DB)) {

                db.Entry(producto).State = EntityState.Modified;

                try
                {
                    db.SaveChanges();
                    _log.Info("Los datos modificados fueron almacenados con exito!!");

                    Producto producto_out = db.Productos.Find(producto.ProductoId);
                    Categoria categoria = db.Categorias.Find(producto.CategoriaId);
                    producto_out.Categoria = categoria;

                    return Request.CreateResponse(HttpStatusCode.OK, producto_out);

                }
                catch (Exception e)
                {                  
                    _log.Info("Error ha acurrido un conflicto en servidor, mensaje: " + e.Source);
                    return Request.CreateResponse(HttpStatusCode.Conflict, "Ha ocurrido una excecion al intentar modificar el producto: " + producto.ProductoId);
                   
                }

            }

            if (origen.ToUpper().Equals(Destino.XML)) {


                Producto productoEncontrado = ProductoService.ObtenerProductoPorIDFromXML(producto.ProductoId);
                Producto producto_out;

                if (productoEncontrado == null) {

                    _log.Info("El Producto con el identificador " + producto.ProductoId + " no fue encontrado");
                    return Request.CreateResponse(HttpStatusCode.NotFound, "El Producto con el identificador " + producto.ProductoId + " no fue encontrado en la base de datos");
                }

                producto_out = ProductoService.ModificarProductoFromXml(producto);
                _log.Info("Los datos modificados fueron almacenados con exito!!");

                // Recuperar la categoria de producto en cuestion y añadirla mismo
                Categoria categoria = db.Categorias.Find(producto.CategoriaId);               

                producto_out.Categoria = categoria;

                return Request.CreateResponse(HttpStatusCode.OK, producto_out);

            }

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        //POST: api/Productoes/origen/bd
        //POST: api/Productoes/origen/xml
        [Route("destino/{destino:alpha}")]
        [HttpPost]
        [ResponseType(typeof(Producto))]
        public HttpResponseMessage PostProducto(string destino, Producto producto)
        {
            XmlConfigurator.Configure();

            _log.Info("Solicitud de operacion de guardado de producto, metodo responsable PostProducto() ");

            if (!ModelState.IsValid)
            {
                _log.Error("Parametros invalidos, para el producto: " + producto);

                return Request.CreateResponse(HttpStatusCode.BadRequest, "Json invalido, por favor revise los parametros o sintaxis, para procesar su solicitud");
                
            }

            if (destino != "")
            {
                if (destino.ToUpper() == Utils.Destino.DB)
                {
                    _log.Info("Los datos seran guardados con destino a Base de Datos");
                    try {
                        db.Productos.Add(producto);
                        db.SaveChanges();
                        _log.Info("Operacion de guardado exitosa, para el producto: " + producto.ToString());
                        return Request.CreateResponse(HttpStatusCode.Created, "Producto Creado Satisfactoriamente");
                    }
                    catch (Exception e) {
                        _log.Error("Error al solicitar informacion a GetProductos()" + e.Source);
                    }
                    
                }

                if (destino.ToUpper() == Utils.Destino.XML)
                {
                    _log.Info("Los datos seran guardados con formato XML en el disco principal");

                    try {

                        if (File.Exists(Constante.FILE_PATH_XML_PRODUCTO))
                        {

                            ProductoService._Añadir(producto);
                            _log.Info("El producto: " + producto + " se ha guardado satisfactoriamente en la direccion " + Constante.FILE_PATH_XML_PRODUCTO);
                            return Request.CreateResponse(HttpStatusCode.Accepted, "Producto creado, satisfactoriamente.");

                        }
                        else if (!File.Exists(Constante.FILE_PATH_XML_PRODUCTO))
                        {

                            CrearDocumentoXML(Constante.FILE_PATH_XML, "Productos");
                            ProductoService._Añadir(producto);
                            _log.Info("El producto: " + producto + " se ha guardado satisfactoriamente en la direccion " + Constante.FILE_PATH_XML_PRODUCTO);
                            return Request.CreateResponse(HttpStatusCode.Accepted, "Producto creado, satisfactoriamente.");
                        }                        

                    }
                    catch (Exception e) {
                        _log.Error("Error al intentar grabar los datos en formato XML, metodo responsable PostProducto(), detalles: " + e.Source);
                    }
                    
                }

            }
            else {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No especifico el destino de los datos, este dato es obligatorio: BD o XML");
            }

            return Request.CreateResponse(HttpStatusCode.NoContent);

        }

        // DELETE: api/Productoes/2/origen/bd
        // DELETE: api/Productoes/4654/origen/xml
        [Route("{id:int}/origen/{origen:alpha}")]
        [HttpDelete]
        [ResponseType(typeof(Producto))]
        public HttpResponseMessage DeleteProducto(int id, string origen)
        {

            if (origen.ToUpper() == Utils.Destino.DB)
            {

                Producto producto = db.Productos.Find(id);

                if (producto == null)
                {
                    _log.Info("El producto con identificador: " + id + " no se encuentra almacenado");
                    return Request.CreateResponse(HttpStatusCode.NotFound, "El producto con identificador: " + id + " no se encuentra almacenado");
                }

                db.Productos.Remove(producto);
                db.SaveChanges();
                _log.Info("El producto con identificador: " + id + " ha sido borrado satisfactoriamente.");               
                return Request.CreateResponse(HttpStatusCode.OK, "Producto eliminado con exito");

            }

            if (origen.ToUpper() == Utils.Destino.XML) {

                Producto producto = ProductoService.ObtenerProductoPorIDFromXML(id);

                if (producto == null)
                {
                    _log.Info("El producto con identificador: " + id + " no se encuentra almacenado");
                    return Request.CreateResponse(HttpStatusCode.NotFound, "El producto con identificador: " + id + " no se encuentra almacenado");
                }

                ProductoService.BorrarProductoFromXml(id);
                _log.Info("El producto con identificador: " + id + " ha sido borrado satisfactoriamente.");                
                return Request.CreateResponse(HttpStatusCode.OK, "El producto ha sido borrado satisfactoriamente.");
            }

            return Request.CreateResponse(HttpStatusCode.Conflict, "Hay conflicto en servidor, por alguna razon no se ha podido eliminar el producto");
        }

        // Metodo encargado de crear en el disco principal el archivo XML, para guardar todos los productos.
        protected void CrearDocumentoXML(string fILE_PATH_XML, string nodoRaiz)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlNode root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlNode element1 = doc.CreateElement(nodoRaiz);
            doc.AppendChild(element1);

            try
            {
                // Crear el directorio Data en el disco principal
                System.IO.Directory.CreateDirectory(Constante.FILE_PATH_XML);
                _log.Info("Se ha creado el directoro en la direccion " + fILE_PATH_XML + " de forma satisfactoria.");
                doc.Save(@fILE_PATH_XML + @"\productos.xml");
                _log.Info("Se ha creado un recurso en la direccion " + fILE_PATH_XML + @"\productos.xml satisfactoriamente.");
            }
            catch (Exception e)
            {
                _log.Error("Error ha ocurrido una excepcion de tipo IO, al intentar escribir sobre el discoprincipal. Mensaje: " + e.Source);
            }

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProductoExists(int id)
        {
            return db.Productos.Count(e => e.ProductoId == id) > 0;
        }
    }
}