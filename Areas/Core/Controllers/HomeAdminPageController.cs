using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QML.Web.UI.Areas.DocumentAdmin.Models;
using QML.Web.UI.Areas.DocumentAdmin.Helpers;

namespace QML.Web.UI.Areas.Core.Controllers
{
    public class HomeAdminPageController : Controller
    {
        //
        // GET: /Core/HomeAdminPage/
        HanuELibraryEntities db = new HanuELibraryEntities();
        
        public ActionResult Index(long? id_cate = 0)
        {
            IEnumerable<DocumentsFileModel> document;
            DocumentCategoryTreeViewHelper tree = new DocumentCategoryTreeViewHelper();
            // By Category
            if (id_cate != 0)
            {
                tree.getIdRelatives(Int32.Parse(id_cate.ToString()));
                List<int> list = tree.list_id_relatives;
                list.Add(Int32.Parse(id_cate.ToString()));
                document = (from d in db.DocumentsFiles
                            where list.Contains(d.CategoryID ?? 0) && d.IsDeleted == false
                            orderby d.CreatedDate descending
                            select new DocumentsFileModel
                            {
                                DocumentID = d.DocumentID,
                                FileName = d.FileName,
                                CategoryID = d.CategoryID.Value,
                                FormatID = d.FormatID.Value,
                                Size = d.Size.Value,
                                CheckHasInfo = d.IsHasInfo.Value == true ? "checked='checked'" : "",
                                CreatedDate = d.CreatedDate.Value,
                                Status = d.Status
                            });
            }
            else
            {
                document = (from d in db.DocumentsFiles
                            where d.IsDeleted == false
                            orderby d.CreatedDate descending
                            select new DocumentsFileModel
                            {
                                DocumentID = d.DocumentID,
                                FileName = d.FileName,
                                CategoryID = d.CategoryID.Value,
                                FormatID = d.FormatID.Value,
                                Size = d.Size.Value,
                                CheckHasInfo = d.IsHasInfo.Value == true ? "checked='checked'" : "",
                                CreatedDate = d.CreatedDate.Value,
                                Status = d.Status
                            });
            }


            tree.renderTreeView(0, Url.Action("Index"));
            ViewBag.tree_cate = tree.trees;
            ViewBag.id_cate = id_cate;
            ViewBag.name_cate = DocumentsHelper.getCategoryName(Int32.Parse(id_cate.ToString()));
            return View(document);
        }

        

    }
}
