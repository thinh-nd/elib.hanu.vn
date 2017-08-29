using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using QML.Library;
using QML.Library.Utilities;
using QML.Library.Helpers;
using QML.Library.Auth;
using QML.Library.Base.Controllers;
using QML.Library.Attributes;
using QML.Web.UI.Areas.DocumentAdmin.Helpers;
using QML.Web.UI.Areas.DocumentAdmin.Models;
using System.IO;
using System.Web.Helpers;

namespace QML.Web.UI.Areas.DocumentAdmin.Controllers
{
    public class DocumentFormatController : SecuredController
    {
        //
        // GET: /DocumentAdmin/DocumentFormat/
        HanuELibraryEntities db = new HanuELibraryEntities();
        [Menu(Title = "Danh sách", Path = "Định dạng văn bản", Order = 1)]
        public ActionResult Index()
        {
            var format = from f in db.DocumentFormats select f;
            return View(format.ToList());
        }

        //
        // GET: /DocumentAdmin/DocumentFormat/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /DocumentAdmin/DocumentFormat/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /DocumentAdmin/DocumentFormat/Create

        [HttpPost]
        public ActionResult Create(DocumentFormatModel model, HttpPostedFileBase file)
        {
            try
            {
                if ((file != null) && (file.ContentLength > 0))
                {
                    DocumentFormat entity = new DocumentFormat();
                    entity.Name = model.Name;
                    entity.Description = model.Description;
                    entity.MIME = model.MIME;
                    entity.FileFormat = model.FileFormat;

                    string fileName = Path.GetFileName(file.FileName);
                    string extension = "";
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        fileName = fileName.Replace(" ", "_");
                        extension = fileName.Split('.')[(fileName.Split('.').Length - 1)].ToLower();
                    }

                    entity.Avatar = fileName;
                    entity.Status = model.Status;
                    var path = Path.Combine(HttpContext.Server.MapPath("~/uploads"), fileName);
                    file.SaveAs(path);
                    FileInfo finfo = new FileInfo(HttpContext.Server.MapPath("~/uploads/" + fileName));
                    long FileInBytes = finfo.Length;
                    long FileInKB = finfo.Length / 1024;


                    db.DocumentFormats.AddObject(entity);
                    db.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return RedirectToAction("Index");
        }
        
        //
        // GET: /DocumentAdmin/DocumentFormat/Edit/5
 
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /DocumentAdmin/DocumentFormat/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /DocumentAdmin/DocumentFormat/Delete/5
 
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /DocumentAdmin/DocumentFormat/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
                DocumentFormat entity = db.DocumentFormats.FirstOrDefault(p => p.DocumentFormatID == id);
                db.DocumentFormats.DeleteObject(entity);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
