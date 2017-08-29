using System;
using System.Collections.Generic;
using System.Linq;
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

namespace QML.Web.UI.Areas.DocumentAdmin.Controllers
{
    public class DocumentCategoryAdminController : SecuredController
    {
        //
        // GET: /DocumentAdmin/DocumentCategoryAdmin/
        HanuELibraryEntities db = new HanuELibraryEntities();
        [Menu(Title = "Danh sách danh mục", Path = "Tài liệu", Order = 1)]
        public ActionResult Index()
        {
            DocumentCategoryTreeViewHelper obj = new DocumentCategoryTreeViewHelper();            
            //retrieve first category           
            if(Request["collectionId"]==null){
                DocumentCategory doc = db.DocumentCategories.Where(p=>p.ParentID==0).FirstOrDefault();
                obj.BuildHierarchicalDataSpecific(0, 1, doc.CategoryID);
            }else{
            obj.BuildHierarchicalDataSpecific(0, 1, int.Parse(Request["collectionId"]));
            }
            IEnumerable<DocumentCategoriesModel> model = (IEnumerable<DocumentCategoriesModel>)obj.DocumentList;            
            return View(model);
        }

        //
        [HttpPost]        
        public ActionResult Index(FormCollection form)
        {
            IEnumerable<DocumentCategoriesModel> model = null;
            if (String.IsNullOrEmpty(form["collectionId"]))
            {
                DocumentCategoryTreeViewHelper obj = new DocumentCategoryTreeViewHelper();
                DocumentCategory entity = new DocumentCategory();
                entity.CategoryID = 0;
                entity.CategoryName = "Danh mục gốc";
                entity.ParentID = 0;
                entity.Position = 9999;
                obj.BuildHierarchicalData(0,1,entity.CategoryID);
                model = (IEnumerable<DocumentCategoriesModel>)obj.DocumentList;
            }
            else
            {
                DocumentCategoryTreeViewHelper obj = new DocumentCategoryTreeViewHelper();
                DocumentCategory entity = new DocumentCategory();
                entity.CategoryID = int.Parse(form["collectionId"]);
                entity.CategoryName = "Danh mục gốc";
                entity.ParentID = 0;
                entity.Position = 9999;
                obj.BuildHierarchicalDataSpecific(0, 1, entity.CategoryID);
                model = (IEnumerable<DocumentCategoriesModel>)obj.DocumentList;
            }
            ViewBag.selectedVal = int.Parse(form["collectionId"]).ToString();
            return View(model);
        }

        //
        // GET: /DocumentAdmin/DocumentCategoryAdmin/Create

        public ActionResult Create()
        {
            // Call TreeViewHelper
            DocumentCategoryTreeViewHelper obj = new DocumentCategoryTreeViewHelper();
            obj.BuildHierarchicalList(0, ""); // Build dropdownList
            List<DocumentCategoriesModel> items = obj.DocumentList;
            // Insert Default ROOT Value
            items.Insert(0, new DocumentCategoriesModel { CategoryID = 0, CategoryName = "ROOT", ParentID = -1 });
            // Assign ViewBag
            ViewBag.ParentList = new SelectList(items, "CategoryID", "CategoryName");
            return View();
        } 

        //
        // POST: /DocumentAdmin/DocumentCategoryAdmin/Create

        [HttpPost]
        public ActionResult Create(DocumentCategoriesModel model)
        {
            DocumentCategory entity = new DocumentCategory();
            entity.CategoryName = model.CategoryName;
            entity.ParentID = model.ParentID;
            entity.Position = model.Position;
            db.DocumentCategories.AddObject(entity);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
        
        //
        // GET: /DocumentAdmin/DocumentCategoryAdmin/Edit/5
 
        public ActionResult Edit(int id)
        {
            DocumentCategoriesModel model = new DocumentCategoriesModel();
            DocumentCategory entity = db.DocumentCategories.FirstOrDefault(p => p.CategoryID == id);
            model.CategoryID = entity.CategoryID;
            model.CategoryName = entity.CategoryName;
            model.ParentID = entity.ParentID;
            model.Position = entity.Position.Value;

            // Load Category Tree
            // Call TreeViewHelper
            DocumentCategoryTreeViewHelper obj = new DocumentCategoryTreeViewHelper();
            obj.BuildHierarchicalList(0, ""); // Build dropdownList
            List<DocumentCategoriesModel> items = obj.DocumentList;
            // Insert Default ROOT Value
            items.Insert(0, new DocumentCategoriesModel { CategoryID = 0, CategoryName = "ROOT", ParentID = -1 });
            // Assign ViewBag
            ViewBag.CurrentID = id;
            ViewBag.ParentList = new SelectList(items, "CategoryID", "CategoryName");

            return View(model);
        }

        //
        // POST: /DocumentAdmin/DocumentCategoryAdmin/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, DocumentCategoriesModel model)
        {
            DocumentCategory entity = db.DocumentCategories.FirstOrDefault(p => p.CategoryID == id);
            entity.CategoryName = model.CategoryName;
            entity.ParentID = model.ParentID;
            entity.Position = model.Position;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //
        // GET: /DocumentAdmin/DocumentCategoryAdmin/Delete/5

        public ActionResult Delete(int id)
        {
            DocumentCategoriesModel model =
                db.DocumentCategories.Where(p => p.CategoryID == id).Select(
                    p => new DocumentCategoriesModel { CategoryID = p.CategoryID }
                ).FirstOrDefault();
            return View(model);
        }

        //
        // POST: /DocumentAdmin/DocumentCategoryAdmin/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, DocumentCategoriesModel model)
        {
            DocumentCategory entity = db.DocumentCategories.FirstOrDefault(p => p.CategoryID == id);
            db.DocumentCategories.DeleteObject(entity);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
