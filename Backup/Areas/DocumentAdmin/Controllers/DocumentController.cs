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
using System.Globalization;
using QML.Web.UI.Controllers;

namespace QML.Web.UI.Areas.DocumentAdmin.Controllers
{
    public class DocumentController : SecuredController
    {
        //
        // GET: /DocumentAdmin/Document/
        HanuELibraryEntities db = new HanuELibraryEntities();

        [Menu(Title = "Danh sách tài liệu", Path = "Tài liệu", Order = 1)]
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
                //list.Add(1); list.Add(3); list.Add(4);
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
                                Thumbnail = d.Thumbnail,
                                CreatedDate = d.CreatedDate.Value,
                                Status = d.Status,                                
                                BookFee = (double)d.BookFee,
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
                                Thumbnail = d.Thumbnail,
                                CreatedDate = d.CreatedDate.Value,
                                Status = d.Status,                                
                                BookFee = (double)d.BookFee,
                            });
            }


            tree.renderTreeView(0, Url.Action("Index"));
            ViewBag.tree_cate = tree.trees;
            ViewBag.id_cate = id_cate;
            ViewBag.name_cate = DocumentsHelper.getCategoryName(Int32.Parse(id_cate.ToString()));      
            
            return View(document);
        }

        [HttpPost]
        public ActionResult Management(FormCollection collection)
        {
            string[] listID = collection["idValue"].Split(',');
            DocumentsFile document;
            long id_document = Int64.Parse(listID[0]);
            switch (collection["actionName"])
            {
                case "cataloging":
                    //nếu chọn 1 file để biên mục
                    if(listID.Count()==1){
                    return RedirectToAction("ExtraInfo", new { id = Int64.Parse(listID[0]) });
                    }
                    ViewBag.ListID = collection["idValue"];
                    return View("../Document/ExtraInfoMultipleFile");

                case "thumbnail":

                    DocumentsFileModel df = db.DocumentsFiles.Where(p => p.DocumentID == id_document).Select(p => new DocumentsFileModel
                    {
                        Thumbnail = p.Thumbnail,
                        FileName = p.FileName,
                        DocumentID = p.DocumentID
                    }).FirstOrDefault();
                    ViewBag.thumbnail = df.Thumbnail;
                    ViewBag.DocumentName = df.FileName;
                    ViewBag.id = df.DocumentID;
                    return View("../Document/Thumbnail");

                case "moveCate":
                    // Call TreeViewHelper
                    DocumentCategoryTreeViewHelper obj = new DocumentCategoryTreeViewHelper();
                    obj.BuildHierarchicalList(0, ""); // Build dropdownList
                    List<DocumentCategoriesModel> items = obj.DocumentList;
                    ViewBag.ListID = collection["idValue"];
                    ViewBag.ParentList = new SelectList(items, "CategoryID", "CategoryName");
                    return View("../Document/moveCate");

                case "setStatus":
                    ViewBag.ListID = collection["idValue"];
                    var values = new[]
                    {
                        new { Value = "Không hiển thị", Text = "Không hiển thị" },
                        new { Value = "Hiển thị", Text = "Hiển thị" },
                        new { Value = "Mật", Text = "Mật" },
                    };
                    ViewBag.StatusList = new SelectList(values, "Value", "Text");
                    return View("../Document/setStatus");

                case "setFee":

                    DocumentsFileModel doc = db.DocumentsFiles.Where(p => p.DocumentID == id_document).Select(p => new DocumentsFileModel
                    {
                        BookFee = p.BookFee.Value,
                        FileName = p.FileName,
                        DocumentID = p.DocumentID
                    }).FirstOrDefault();
                    ViewBag.DocumentFee = doc.BookFee;
                    ViewBag.DocumentName = doc.FileName;
                    ViewBag.id = doc.DocumentID;
                    return View("../Document/setFee");

                case "uploadSWF":
                    return View("../Document/UploadSWF");
                case "deleteCurrent":

                    //List<int> ListID = new List<int>();
                    foreach (string item in listID)
                    {
                        int id_doc = Int32.Parse(item);
                        document = db.DocumentsFiles.Where(d => d.DocumentID == id_doc).FirstOrDefault();
                        document.IsDeleted = true;
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index");

                case "deletePermanent":
                    foreach (string item in listID)
                    {
                        int id_doc = Int32.Parse(item);
                        document = db.DocumentsFiles.Where(d => d.DocumentID == id_doc).FirstOrDefault();
                        db.DocumentsFiles.DeleteObject(document);
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index");

                default:
                    return RedirectToAction("Index");

            }

            //return RedirectToAction("Index", new { result = collection["actionName"], count = listID.Count() });
        }

        [HttpPost]
        public ActionResult Thumbnail(FormCollection collection)
        {

            long id = Int64.Parse(collection["DocumentID"]);
            DocumentsFile entity = db.DocumentsFiles.FirstOrDefault(p => p.DocumentID == id);
            // TODO: Add logic here

            // File Upload     
            var file = WebImage.GetImageFromRequest();
            string fileName = "";
            if (file != null)
            {
                fileName = DateTime.Now.Ticks.ToString() + Path.GetFileName(file.FileName);
                var path = Path.Combine(Server.MapPath("~/uploads"), fileName);
                file.Save(path);
                if (Path.GetExtension(file.FileName) == "gif")
                {
                    fileName = fileName + ".gif";
                }
                entity.Thumbnail = fileName;                
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult setFee(FormCollection collection)
        {

            long id = Int64.Parse(collection["DocumentID"]);
            DocumentsFile entity = db.DocumentsFiles.FirstOrDefault(p => p.DocumentID == id);
            entity.BookFee = float.Parse(collection["fee"]);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult setStatus(FormCollection collection)
        {

            DocumentsFile document;
            string[] list = collection["listID"].Split(',');
            foreach (string item in list)
            {
                int id_doc = Int32.Parse(item);
                document = db.DocumentsFiles.Where(d => d.DocumentID == id_doc).FirstOrDefault();
                document.Status = collection["Status"];
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        

        [HttpPost]
        public ActionResult ExtraInfoMultipleFile(FormCollection collection) {
            Document document;
            string[] list = collection["listID"].Split(',');
            
            foreach (string item in list)
            {
                int id_doc = Int32.Parse(item);
                document = db.Documents.Where(d => d.DocumentID == id_doc).FirstOrDefault();
                if (document != null)
                {
                    document.Title = collection["Title"];
                    document.Creator = collection["Creator"];
                    document.Subject = collection["Subject"];
                    document.Description = collection["Description"];
                    document.Publisher = collection["Publisher"];
                    document.Contributor = collection["Contributor"];
                    document.Date = Convert.ToDateTime(collection["Date"]);
                    document.Type = collection["Type"];
                    document.Format = collection["Format"];
                    document.Identifier = collection["Identifier"];
                    document.Resource = collection["Resource"];
                    document.Language = collection["Language"];
                    document.Relation = collection["Relation"];
                    document.Coverage = collection["Coverage"];
                    document.Right = collection["Right"];
                }
                else {
                    Document doc = new Document();
                    doc.Title = collection["Title"];
                    doc.Creator = collection["Creator"];
                    doc.Subject = collection["Subject"];
                    doc.Description = collection["Description"];
                    doc.Publisher = collection["Publisher"];
                    doc.Contributor = collection["Contributor"];
                    doc.Date = DateTime.ParseExact(collection["Date"], "MMM dd yyyy", CultureInfo.InvariantCulture); ;
                    doc.Type = collection["Type"];
                    doc.Format = collection["Format"];
                    doc.Identifier = collection["Identifier"];
                    doc.Resource = collection["Resource"];
                    doc.Language = collection["Language"];
                    doc.Relation = collection["Relation"];
                    doc.Coverage = collection["Coverage"];
                    doc.Right = collection["Right"];
                }
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult moveCate(FormCollection collection)
        {

            DocumentsFile document;
            int id_cate = Int32.Parse(collection["CateID"]);
            string[] list = collection["listID"].Split(',');
            //List<int> ListID = new List<int>();
            foreach (string item in list)
            {
                int id_doc = Int32.Parse(item);
                document = db.DocumentsFiles.Where(d => d.DocumentID == id_doc).FirstOrDefault();
                document.CategoryID = id_cate;
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /*
        public ActionResult Search(string attribute, string keyword, long? id_cate = 0)
        {
            DocumentCategoryTreeViewHelper tree = new DocumentCategoryTreeViewHelper();
            List<DocumentsFile> document = db.DocumentsFiles.ToList();
            IEnumerable<DocumentsFileModel> models;
            string filter = "";
            if (string.IsNullOrEmpty(keyword))
            {
                return RedirectToAction("Index");
            }

            switch (attribute)
            {
                case "Publisher":
                    document = document.Where(d => d.Publisher.ToUpper().Contains(keyword.ToUpper())).ToList();
                    filter += "Nhà xuất bản";
                    break;
                case "Type":
                    document = document.Where(d => d.Type.ToUpper().Contains(keyword.ToUpper())).ToList();
                    filter += "Thể loại";
                    break;
                case "Creator":
                    document = document.Where(d => d.Creator.ToUpper().Contains(keyword.ToUpper())).ToList();
                    filter += "Tác giả";
                    break;
                default:
                    document = document.Where(d => d.Title.ToUpper().Contains(keyword.ToUpper())).ToList();
                    filter += "Tiêu đề tài liệu";
                    break;
            }
            if (id_cate != 0)
            {
                document = document.Where(d => d.SubjectId == id_cate).ToList();
            }

            models = document.Join(
                                        db.DocumentsExtraInfoes,
                                        d => d.DocumentID,
                                        de => de.DocumentID,
                                        (d, de) => new DocumentModel
                                        {
                                            DocumentID = d.DocumentID,
                                            Title = d.Title,
                                            SubjectID = d.SubjectId.Value,
                                            Creator = d.Creator,
                                            Type = d.Type,
                                            Format = d.Format.Value,
                                            Status = de.Status.Value
                                        }
                                       );
            tree.renderTreeView(0, Url.Action("Index"));
            ViewBag.tree_cate = tree.trees;
            ViewBag.id_cate = id_cate;
            ViewBag.name_cate = DocumentsHelper.getCategoryName(Int32.Parse(id_cate.ToString()));
            ViewBag.filter = filter;
            ViewBag.keyword = keyword;
            return View("../Document/Index", models);
        }
        */
        //
        // GET: /DocumentAdmin/Document/Details/5
        /*
        public ActionResult Check(long id)
        {
            DocumentsExtraInfo entity = db.DocumentsExtraInfoes.FirstOrDefault(p => p.DocumentID == id);
            if (entity.Status == true)
            {
                entity.Status = false;
            }
            else
            {
                entity.Status = true;
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        */
        //
        // GET: /DocumentAdmin/Document/Create

        public ActionResult Filter(string keyword,string attribute, string value, long? id_cate = 0)
        {
            DocumentCategoryTreeViewHelper tree = new DocumentCategoryTreeViewHelper();
            List<DocumentsFile> document = db.DocumentsFiles.ToList();
            IEnumerable<DocumentsFileModel> models;
            string header = "Lọc theo thuộc tính ";
            if (attribute == "Status")
            {
                header += "<b>Trạng thái: </b>";
                if (value == "public")
                {
                    document = document.Where(d => d.Status == "Hiển thị").ToList();
                    header += "<i>Hiển thị</i>";
                }
                else if (value == "unpublic")
                {
                    document = document.Where(d => d.Status == "Không hiển thị").ToList();
                    header += "<i>Không hiển thị</i>";
                }
                else if (value == "private")
                {
                    document = document.Where(d => d.Status == "Mật").ToList();
                    header += "<i>Mật</i>";
                }

            }
            else if (attribute == "Catalogue")
            {

                if (value == "cataloged")
                {
                    document = document.Where(d => d.IsHasInfo == true).ToList();
                    header += "<b>Đã biên mục</b>";
                }
                else if (value == "notcataloged")
                {
                    document = document.Where(d => d.IsHasInfo == false).ToList();
                    header += "<b>Chưa biên mục</b>";
                }
            }
            else if (attribute == "keyword") {
                header += "<b>Từ khóa:</b>";                                
                //lọc theo từ khóa
                switch(value){
                        //tất cả
                    case "0":
                        List<DocumentsFile> _ls = new List<DocumentsFile>();
                        //nhan đề
                        var _query = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Title.ToLower().Contains(keyword.ToLower()));
                        //tác giả
                        var _query1 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Creator.ToLower().Contains(keyword.ToLower()));
                        //chủ đề
                        var _query2 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Subject.ToLower().Contains(keyword.ToLower()));
                        //tác giả phụ
                        var _query3 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Contributor.ToLower().Contains(keyword.ToLower()));
                        //năm xuất bản
                        int _num;
                        int _year;
                        if (int.TryParse(keyword, out _num))
                        {
                            _year = int.Parse(keyword);
                        }
                        else {
                            _year = 0;
                        }
                       var _query4 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Date.Value.Year == _year);
                        //loại tài liệu
                       var _query5 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Type.ToLower().Contains(keyword.ToLower()));
                        //mô tả vật lý
                        var _query6 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Format.ToLower().Contains(keyword.ToLower()));
                        //định danh
                        var _query7 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Identifier.ToLower().Contains(keyword.ToLower()));
                        //liên kết
                        var _query8 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Relation.ToLower().Contains(keyword.ToLower()));
                        //nguồn
                        var _query9 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Resource.ToLower().Contains(keyword.ToLower()));
                        //ngôn ngữ
                        var _query10 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Language.ToLower().Contains(keyword.ToLower()));
                        //bản quyền
                        var _query11 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Right.ToLower().Contains(keyword.ToLower()));
                        //tóm tắt
                        var _query12 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Description.Contains(keyword));
                        //nhà xuất bản
                        var _query13 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Publisher.ToLower().Contains(keyword.ToLower()));
                        //địa chỉ lưu trữ
                        var _query14 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Coverage.ToLower().Contains(keyword.ToLower()));

                        foreach (var item in _query.ToList())
                        {
                            _ls.Add(item.DocumentFile);
                        }

                        foreach (var item in _query1.ToList())
                        {
                            _ls.Add(item.DocumentFile);
                        }
                        foreach (var item in _query2.ToList())
                        {
                            _ls.Add(item.DocumentFile);
                        }
                        foreach (var item in _query3.ToList())
                        {
                            _ls.Add(item.DocumentFile);
                        }
                        foreach (var item in _query4.ToList())
                        {
                            _ls.Add(item.DocumentFile);
                        }
                        foreach (var item in _query5.ToList())
                        {
                            _ls.Add(item.DocumentFile);
                        }
                        foreach (var item in _query6.ToList())
                        {
                            _ls.Add(item.DocumentFile);
                        }
                        foreach (var item in _query7.ToList())
                        {
                            _ls.Add(item.DocumentFile);
                        }
                        foreach (var item in _query8.ToList())
                        {
                            _ls.Add(item.DocumentFile);
                        }
                        foreach (var item in _query9.ToList())
                        {
                            _ls.Add(item.DocumentFile);
                        }
                        foreach (var item in _query10.ToList())
                        {
                            _ls.Add(item.DocumentFile);
                        }
                        foreach (var item in _query11.ToList())
                        {
                            _ls.Add(item.DocumentFile);
                        }
                        foreach (var item in _query12.ToList())
                        {
                            _ls.Add(item.DocumentFile);
                        }
                        foreach (var item in _query13.ToList())
                        {
                            _ls.Add(item.DocumentFile);
                        }
                        foreach (var item in _query14.ToList())
                        {
                            _ls.Add(item.DocumentFile);
                        }

                        document = _ls.Distinct().ToList();
                        break;
                    //nhan đề
                    case "1": 
                         List<DocumentsFile> ls = new List<DocumentsFile>();
                        var query = db.DocumentsFiles.Join(db.Documents,q=>q.DocumentID,r=>r.DocumentID,(q,r)=>new{DocumentFile = q, Doc = r}).Where(z=>z.Doc.Title.ToLower().Contains(keyword.ToLower()));
                        foreach(var item in query.ToList()){
                            ls.Add(item.DocumentFile);
                        }
                        
                       document = ls;
                       break;
                //tác giả
                    case "2":
                       List<DocumentsFile> ls1 = new List<DocumentsFile>();
                       var query1 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Creator.ToLower().Contains(keyword.ToLower()));
                       foreach (var item in query1.ToList())
                       {
                           ls1.Add(item.DocumentFile);
                       }

                       document = ls1;
                       break;
                   //chủ đề
                    case "3":
                       List<DocumentsFile> ls2 = new List<DocumentsFile>();
                       var query2 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Subject.ToLower().Contains(keyword.ToLower()));
                       foreach (var item in query2.ToList())
                       {
                           ls2.Add(item.DocumentFile);
                       }

                       document = ls2;
                       break;
                    //tác giả phụ
                    case "4":
                       List<DocumentsFile> ls3 = new List<DocumentsFile>();
                       var query3 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Contributor.ToLower().Contains(keyword.ToLower()));
                       foreach (var item in query3.ToList())
                       {
                           ls3.Add(item.DocumentFile);
                       }

                       document = ls3;
                       break;
                    //năm xuất bản
                    case "5":
                       List<DocumentsFile> ls4 = new List<DocumentsFile>();
                        int num;
                        int year;
                        if (int.TryParse(keyword, out num))
                        {
                            year = int.Parse(keyword);
                        }
                        else {
                            year = 0;
                        }
                       var query4 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Date.Value.Year == year);
                       foreach (var item in query4.ToList())
                       {
                           ls4.Add(item.DocumentFile);
                       }

                       document = ls4;
                       break;
                    //loại tài liệu
                    case "6":
                       List<DocumentsFile> ls5 = new List<DocumentsFile>();
                       var query5 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Type.ToLower().Contains(keyword.ToLower()));
                       foreach (var item in query5.ToList())
                       {
                           ls5.Add(item.DocumentFile);
                       }

                       document = ls5;
                       break;
                    //mô tả vật lý
                    case "7":
                       List<DocumentsFile> ls6 = new List<DocumentsFile>();
                       var query6 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Format.ToLower().Contains(keyword.ToLower()));
                       foreach (var item in query6.ToList())
                       {
                           ls6.Add(item.DocumentFile);
                       }

                       document = ls6;
                       break;

                    //định danh
                    case "8":
                        List<DocumentsFile> ls7 = new List<DocumentsFile>();
                       var query7 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Identifier.ToLower().Contains(keyword.ToLower()));
                       foreach (var item in query7.ToList())
                       {
                           ls7.Add(item.DocumentFile);
                       }

                       document = ls7;
                       break;
                    //liên kết
                    case "9":
                       List<DocumentsFile> ls8 = new List<DocumentsFile>();
                       var query8 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Relation.ToLower().Contains(keyword.ToLower()));
                       foreach (var item in query8.ToList())
                       {
                           ls8.Add(item.DocumentFile);
                       }

                       document = ls8;
                       break;
                    //nguồn
                    case "10":
                       List<DocumentsFile> ls9 = new List<DocumentsFile>();
                       var query9 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Resource.ToLower().Contains(keyword.ToLower()));
                       foreach (var item in query9.ToList())
                       {
                           ls9.Add(item.DocumentFile);
                       }

                       document = ls9;
                       break;  
                    //ngôn ngữ
                    case "11":
                       List<DocumentsFile> ls10 = new List<DocumentsFile>();
                       var query10 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Language.ToLower().Contains(keyword.ToLower()));
                       foreach (var item in query10.ToList())
                       {
                           ls10.Add(item.DocumentFile);
                       }

                       document = ls10;
                       break;
                    //bản quyền
                    case "12":
                       List<DocumentsFile> ls11 = new List<DocumentsFile>();
                       var query11 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Right.ToLower().Contains(keyword.ToLower()));
                       foreach (var item in query11.ToList())
                       {
                           ls11.Add(item.DocumentFile);
                       }

                       document = ls11;
                       break;
                    //tóm tắt
                    case "13":
                       List<DocumentsFile> ls12 = new List<DocumentsFile>();
                       var query12 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Description.Contains(keyword));
                       foreach (var item in query12.ToList())
                       {
                           ls12.Add(item.DocumentFile);
                       }

                       document = ls12;
                       break;
                    //nhà xuất bản
                    case "14":
                       List<DocumentsFile> ls13 = new List<DocumentsFile>();
                       var query13 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Publisher.ToLower().Contains(keyword.ToLower()));
                       foreach (var item in query13.ToList())
                       {
                           ls13.Add(item.DocumentFile);
                       }

                       document = ls13;
                       break;
                    //Địa chỉ lưu trữ
                    case "15":
                        List<DocumentsFile> ls14 = new List<DocumentsFile>();
                       var query14 = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.Doc.Coverage.ToLower().Contains(keyword.ToLower()));
                       foreach (var item in query14.ToList())
                       {
                           ls14.Add(item.DocumentFile);
                       }

                       document = ls14;
                       break;
                }

                //hết lọc
            }
            else
            {
                // return RedirectToAction("Index");
                if (value == "fee")
                {
                    document = document.Where(d => d.BookFee > 0).ToList();
                    header += "<b>Thu phí</b>";
                }
                else if (value == "free")
                {
                    document = document.Where(d => d.BookFee == 0).ToList();
                    header += "<b>Miễn phí</b>";
                }
            }

            if (id_cate != 0)
            {
                document = document.Where(d => d.CategoryID == id_cate).ToList();
            }

            models = document.Where(d => d.IsDeleted == false).Select(d => new DocumentsFileModel
            {
                DocumentID = d.DocumentID,
                FileName = d.FileName,
                CategoryID = d.CategoryID.Value,
                FormatID = d.FormatID.Value,
                Size = d.Size.Value,
                CheckHasInfo = d.IsHasInfo.Value == true ? "checked='checked'" : "",
                CreatedDate = d.CreatedDate.Value,
                Status = d.Status
            }).OrderByDescending(d => d.CreatedDate);

            tree.renderTreeView(0, Url.Action("Index"));
            ViewBag.tree_cate = tree.trees;
            ViewBag.id_cate = id_cate;
            ViewBag.name_cate = DocumentsHelper.getCategoryName(Int32.Parse(id_cate.ToString()));
            ViewBag.header = header;
            ViewBag.keyword = keyword;
            return View("../Document/Index", models);
        }

        public ActionResult Create()
        {
            // Call TreeViewHelper
            DocumentCategoryTreeViewHelper obj = new DocumentCategoryTreeViewHelper();
            obj.BuildHierarchicalList(0, ""); // Build dropdownList
            List<DocumentCategoriesModel> items = obj.DocumentList;
            List<DocumentFormat> type_items = DocumentsHelper.GetDocumentTypeList();
            // Assign ViewBag            
            ViewBag.ParentList = new SelectList(items, "CategoryID", "CategoryName");
            ViewBag.TypeList = new SelectList(type_items, "DocumentFormatID", "Name");
            //ViewBag.Status = TempData["statusError"].ToString(); 
            //DocumentModel model = new DocumentModel();
            //model.Date = DateTime.Today;
            return View();
        }

        //
        // POST: /DocumentAdmin/Document/Create

        [HttpPost]
        public ActionResult Create(DocumentsFileModel model, HttpPostedFileBase file)
        {
            try
            {
                if ((file != null) && (file.ContentLength > 0))
                {
                    DocumentsFile entity = new DocumentsFile();
                    entity.CategoryID = model.CategoryID;
                    entity.FormatID = model.FormatID;
                    string fileName = Path.GetFileName(file.FileName);
                    //check for duplication
                    DocumentsFile checkDuplication = db.DocumentsFiles.FirstOrDefault(p => p.FileName == fileName);
                    if (checkDuplication != null)
                    {
                        TempData["statusError"] = true;
                        return RedirectToAction("Create");
                    }
                    else
                    {
                        string extension = "";
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            fileName = fileName.Replace(" ", "_");
                            extension = fileName.Split('.')[(fileName.Split('.').Length - 1)].ToLower();
                        }

                        entity.FileName = fileName;
                        var path = Path.Combine(Server.MapPath("~/Resources"), fileName);
                        file.SaveAs(path);
                        FileInfo finfo = new FileInfo(HttpContext.Server.MapPath("~/Resources/" + fileName));
                        long FileInBytes = finfo.Length;
                        long FileInKB = finfo.Length / 1024;
                        entity.Size = FileInKB;
                        switch (extension)
                        {
                            case "pdf":
                                entity.FileSource = fileName.Replace("." + extension, "") + ".swf";
                                entity.Thumbnail = "pdf.png";
                                break;
                            case "mp3":
                                entity.FileSource = fileName;
                                entity.Thumbnail = "mp3.png";
                                break;
                            case "swf":
                                return RedirectToAction("Index");
                            default:
                                entity.FileSource = fileName;
                                entity.Thumbnail = "avi.png";
                                break;
                        }

                        entity.BookFee = 0;
                        entity.ViewCount = 0;
                        entity.Status = "Không hiển thị";
                        entity.IsDeleted = false;
                        entity.IsHasInfo = false;
                        entity.CreatedDate = entity.LastModifiedDate = DateTime.Now;
                        entity.CreatedUser = entity.LastModifiedUser = "root";
                        db.DocumentsFiles.AddObject(entity);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return RedirectToAction("Index");
        }

        public ActionResult ExtraInfo(long id)
        {
            DocumentModel model = db.Documents.Where(d => d.DocumentID == id).Select(d => new DocumentModel
            {
                Title = d.Title,
                Creator = d.Creator,
                Subject = d.Subject,
                Description = d.Description,
                Publisher = d.Publisher,
                Contributor = d.Contributor,
                Date = d.Date.Value == null ? new DateTime(1000, 1, 1) :d.Date.Value,
                Type = d.Type,
                Format = d.Format,
                Identifier = d.Identifier,
                Resource = d.Resource,
                Language = d.Language,
                Relation = d.Relation,
                Coverage = d.Coverage,
                Right = d.Right,
            }).FirstOrDefault();
            var df = (from d in db.DocumentsFiles
                      where d.DocumentID == id
                      select d).SingleOrDefault();

            ViewBag.DocumentName = df.FileName;
            if (model != null)
            {
                ViewBag.Year = model.Date.Year.ToString();
            }
            return View(model);
        }

        //
        // POST: /DocumentAdmin/Document/Edit/5

        [HttpPost]
        public ActionResult ExtraInfo(long id, DocumentModel model)
        {
            Document entity = db.Documents.FirstOrDefault(p => p.DocumentID == id);
            
            if (entity != null)
            {
                entity.Title = model.Title;
                entity.Creator = model.Creator;
                entity.Subject = model.Subject;
                entity.Description = model.Description;
                entity.Publisher = model.Publisher;
                entity.Contributor = model.Contributor;               
                entity.Date = model.Date;
                entity.Type = model.Type;
                entity.Format = model.Format;
                entity.Identifier = model.Identifier;
                entity.Resource = model.Resource;
                entity.Language = model.Language;
                entity.Relation = model.Relation;
                entity.Coverage = model.Coverage;
                entity.Right = model.Right;
            }
            else
            {
                Document doc = new Document();
                doc.DocumentID = id;
                doc.Title = model.Title;
                doc.Creator = model.Creator;
                doc.Subject = model.Subject;
                doc.Description = model.Description;
                doc.Publisher = model.Publisher;
                doc.Contributor = model.Contributor;                
                doc.Date = model.Date;
                doc.Type = model.Type;
                doc.Format = model.Format;
                doc.Identifier = model.Identifier;
                doc.Resource = model.Resource;
                doc.Language = model.Language;
                doc.Relation = model.Relation;
                doc.Coverage = model.Coverage;
                doc.Right = model.Right;
                db.Documents.AddObject(doc);
                DocumentsFile docfile = db.DocumentsFiles.FirstOrDefault(p => p.DocumentID == id);
                docfile.IsHasInfo = true;
            }
            db.SaveChanges();
            return RedirectToAction("Index");

        }


        //
        // GET: /DocumentAdmin/Document/Edit/5

        public ActionResult Edit(long id)
        {
            // Fetch model
            DocumentsFile entity = db.DocumentsFiles.FirstOrDefault(p => p.DocumentID == id);

            // Call TreeViewHelper
            DocumentCategoryTreeViewHelper obj = new DocumentCategoryTreeViewHelper();
            obj.BuildHierarchicalList(0, ""); // Build dropdownList
            List<DocumentCategoriesModel> items = obj.DocumentList;
            List<DocumentFormat> type_items = DocumentsHelper.GetDocumentTypeList();
            // Assign ViewBag            
            ViewBag.ParentList = new SelectList(items, "CategoryID", "CategoryName");
            ViewBag.TypeList = new SelectList(type_items, "DocumentFormatID", "Name");

            return View(entity);
        }

        //
        // POST: /DocumentAdmin/Document/Edit/5
        /*
        [HttpPost]
        public ActionResult Edit(long id, DocumentsFileModel model, HttpPostedFileBase file)
        {
                    DocumentsFile entity = db.DocumentsFiles.FirstOrDefault(p => p.DocumentID == id);
                    entity.FileName = model.FileName;
                    entity.Creator = model.Creator;
                    entity.SubjectId = model.SubjectId;
                    entity.Description = model.Description;
                    entity.Publisher = model.Publisher;
                    entity.Contributor = model.Contributor;
                    entity.Date = model.Date;
                    entity.Type = model.Type;
                    entity.Format = model.Format;
                    entity.Resource = model.Resource;
                    entity.Language = model.Language;
                    entity.Relation = model.Relation;
                    entity.Coverage = model.Coverage;
                    entity.CopyRight = model.CopyRight;
                    string fileName = "";
                    if ((file != null) && (file.ContentLength > 0))
                    {
                        fileName = DateTime.Now.Ticks.ToString() + Path.GetFileName(file.FileName);
                        var path = Path.Combine(Server.MapPath("~/Resources"), fileName);
                        file.SaveAs(path);
                        if (fileName.Split('.')[(fileName.Split('.').Length - 1)].ToLower() == "pdf")
                        {

                            // Convert to swf                
                            string cmdStr = HttpContext.Server.MapPath("~/pdf2swf.exe");
                            string savePath = HttpContext.Server.MapPath("~/Resources");
                            string filePath = HttpContext.Server.MapPath("~/Resources/" + fileName);
                            string args = " -t " + filePath + " -o " + savePath + "\\" + fileName + ".swf";
                            DocumentsHelper.ExecuteCmd(cmdStr, args);

                            entity.Identifier = fileName + ".swf";
                        }
                        else
                        {
                            entity.Identifier = fileName;
                        }
                
                     }
                     db.SaveChanges();
                     return RedirectToAction("ExtraInfo", new { id = entity.DocumentID });
            
        }
         * */

        //
        // GET: /DocumentAdmin/Document/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /DocumentAdmin/Document/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            return RedirectToAction("Index");

        }

        [Menu(Title = "Thống kê hệ thống", Path = "Thống kê")]
        public ActionResult SystemStatistics(string viewYear, string docCatId, string viewMonth, string docType = "Fee")
        {
            int? paramYear = GetInt(viewYear) ?? DateTime.Now.Year;
            int? categoryId = GetInt(docCatId);
            int? paramMonth = GetInt(viewMonth);
            ViewBag.tableTitle = "Năm: " + paramYear;
            IEnumerable<SystemStatisticModel> systemStatistic = null;

            systemStatistic = (from viewhistory in db.ViewHistories
                               where viewhistory.Year == paramYear && viewhistory.Type.Equals(docType, StringComparison.OrdinalIgnoreCase)
                               group viewhistory by new { viewhistory.Month, viewhistory.CatId } into grp
                               select new SystemStatisticModel
                               {
                                   CateId = grp.Key.CatId.Value,
                                   Views = grp.Count(),
                                   Month = grp.Key.Month.Value,
                                   Fee = grp.Sum(doc => doc.Fee),

                               }
                              );
            if (paramMonth != null)
            {
                systemStatistic = systemStatistic.Where(d => d.Month == paramMonth);
                ViewBag.tableTitle += " Tháng: " + viewMonth;
            }

            if (categoryId != null)
            {
                systemStatistic = systemStatistic.Where(d => d.CateId == categoryId);
                ViewBag.tableTitle += " Chuyên mục: " + DocumentsHelper.getCategoryNameById(categoryId);
            }
            String vnDocTypeTitle = docType.Equals("Fee", StringComparison.OrdinalIgnoreCase) ? "Trả phí" : "Miễn phí";
            ViewBag.tableTitle += " Thể loại: " + vnDocTypeTitle;

            ViewBag.Nums = systemStatistic.Count();
            // Call TreeViewHelper
            DocumentCategoryTreeViewHelper obj = new DocumentCategoryTreeViewHelper();
            obj.BuildHierarchicalList(0, ""); // Build dropdownList
            List<DocumentCategoriesModel> items = obj.DocumentList;
            // Assign ViewBag            
            ViewBag.ParentList = new SelectList(items, "CategoryID", "CategoryName");
            ViewBag.YearListItem = DocumentsHelper.GetYearListData();
            ViewBag.MonthListItem = DocumentsHelper.GetMonthListData();
            ViewBag.TypeListitem = DocumentsHelper.GetDocTypeListData();
            return View(systemStatistic);

        }
        public static int? GetInt(string stringVal)
        {
            int? value;
            int dummy;
            if (Int32.TryParse(stringVal, out dummy))
            {
                value = dummy;
            }
            else
            {
                value = null;
            }
            return value;
        }
        [Menu(Title = "Lượt xem hàng năm", Path = "Thống kê", Order = 3)]
        public ActionResult StatisticDocumentView(string Cate_Id,string yearView)
        {
            IEnumerable<DocumentSatisticModel> documentstatistic;
            string paramYears = "";
            string feedoc = "";
            string freedoc = "";
            if (string.IsNullOrEmpty(Cate_Id) == true)
            {
                if (string.IsNullOrEmpty(yearView) == true)
                {
                    long yearParse = DateTime.Now.Year;
                    documentstatistic = (from ds in db.DocumentStatistics
                                         where ds.Type == "FeeDocumentView" || ds.Type == "FreeDocumentView"
                                         group ds by new { ds.Year, ds.Type }
                                             into grp
                                             select new DocumentSatisticModel
                                             {
                                                 Year = grp.Key.Year.Value,
                                                 Type = grp.Key.Type,
                                                 Value = grp.Sum(t => t.Value).Value
                                             }).Where(p=>p.Year >= yearParse-4 && p.Year<=yearParse).Take(10).ToList();
                    var paramYear = (from y in db.DocumentStatistics
                                     where y.Type == "FeeDocumentView" || y.Type == "FreeDocumentView"
                                     select new { paramYear = y.Year }).Where(p => p.paramYear >= yearParse-4 && p.paramYear<=yearParse).Take(10).Distinct().ToList();

                    for (int i = 0; i < paramYear.Count(); i++)
                    {
                        if (i != paramYear.Count() - 1)
                        {
                            paramYears += paramYear.ElementAt(i).paramYear.Value + ", ";

                        }
                        else
                        {
                            paramYears += paramYear.ElementAt(i).paramYear.Value;

                        }
                    }
                    for (int i = 0; i < documentstatistic.Count(); i++)
                    {
                        //if (i != documentstatistic.Count() - 1)
                        //{
                        if (documentstatistic.ElementAt(i).Type == "FeeDocumentView")
                        {
                            feedoc += documentstatistic.ElementAt(i).Value + ", ";
                        }
                        else
                        {
                            freedoc += documentstatistic.ElementAt(i).Value + ", ";
                        }
                    }
                    ViewBag.name_cate = "Toàn bộ chuyên mục";
                }
                    //if year!=null
                else {
                    long yearParse = long.Parse(yearView);          
                    documentstatistic = (from ds in db.DocumentStatistics
                                         where ds.Type == "FeeDocumentView" || ds.Type == "FreeDocumentView"
                                         group ds by new { ds.Year, ds.Type }
                                             into grp
                                             select new DocumentSatisticModel
                                             {
                                                 Year = grp.Key.Year.Value,
                                                 Type = grp.Key.Type,
                                                 Value = grp.Sum(t => t.Value).Value
                                             }).Where(p=>p.Year>=yearParse && p.Year<=yearParse+4).Take(10).ToList();
                    var paramYear = (from y in db.DocumentStatistics
                                     where y.Type == "FeeDocumentView" || y.Type == "FreeDocumentView"
                                     select new { paramYear = y.Year }).Where(p => p.paramYear >= yearParse && p.paramYear<=yearParse+4).Take(10).Distinct().ToList();

                    for (int i = 0; i < paramYear.Count(); i++)
                    {
                        if (i != paramYear.Count() - 1)
                        {
                            paramYears += paramYear.ElementAt(i).paramYear.Value + ", ";

                        }
                        else
                        {
                            paramYears += paramYear.ElementAt(i).paramYear.Value;

                        }
                    }
                    for (int i = 0; i < documentstatistic.Count(); i++)
                    {
                        //if (i != documentstatistic.Count() - 1)
                        //{
                        if (documentstatistic.ElementAt(i).Type == "FeeDocumentView")
                        {
                            feedoc += documentstatistic.ElementAt(i).Value + ", ";
                        }
                        else
                        {
                            freedoc += documentstatistic.ElementAt(i).Value + ", ";
                        }
                    }
                    ViewBag.name_cate = "Toàn bộ chuyên mục";
                }
               
            }
            //category not null
            else
            {
                if (string.IsNullOrEmpty(yearView) == true)
                {
                    int cate_id = Int32.Parse(Cate_Id);
                    long yearParse = DateTime.Now.Year;
                    ViewBag.name_cate = "Chuyên mục: " + DocumentsHelper.getCategoryName(cate_id);
                    documentstatistic = (from ds in db.DocumentStatistics
                                         where (ds.Type == "FeeDocumentView" || ds.Type == "FreeDocumentView") && ds.CateID == cate_id
                                         group ds by new { ds.Year, ds.Type }
                                             into grp
                                             select new DocumentSatisticModel
                                             {
                                                 Year = grp.Key.Year.Value,
                                                 Type = grp.Key.Type,
                                                 Value = grp.Sum(t => t.Value).Value
                                             }).Where(p => p.Year >= yearParse-4 && p.Year<=yearParse).Take(10).ToList();

                    var paramYear = (from y in db.DocumentStatistics
                                     where (y.Type == "FeeDocumentView" || y.Type == "FreeDocumentView") && y.CateID == cate_id
                                     select new { paramYear = y.Year }).Where(p => p.paramYear >= yearParse - 4 && p.paramYear<=yearParse).Distinct().ToList();

                    for (int i = 0; i < paramYear.Count(); i++)
                    {
                        if (i != paramYear.Count() - 1)
                        {
                            paramYears += paramYear.ElementAt(i).paramYear.Value + ", ";

                        }
                        else
                        {
                            paramYears += paramYear.ElementAt(i).paramYear.Value;

                        }
                    }
                    for (int i = 0; i < documentstatistic.Count(); i++)
                    {
                        //if (i != documentstatistic.Count() - 1)
                        //{
                        if (documentstatistic.ElementAt(i).Type == "FeeDocumentView")
                        {
                            feedoc += documentstatistic.ElementAt(i).Value + ", ";
                        }
                        else
                        {
                            freedoc += documentstatistic.ElementAt(i).Value + ", ";
                        }
                    }
                }
                else {
                    int cate_id = Int32.Parse(Cate_Id);
                    long yearParse = long.Parse(yearView);
                    ViewBag.name_cate = "Chuyên mục: " + DocumentsHelper.getCategoryName(cate_id);
                    documentstatistic = (from ds in db.DocumentStatistics
                                         where (ds.Type == "FeeDocumentView" || ds.Type == "FreeDocumentView") && ds.CateID == cate_id
                                         group ds by new { ds.Year, ds.Type }
                                             into grp
                                             select new DocumentSatisticModel
                                             {
                                                 Year = grp.Key.Year.Value,
                                                 Type = grp.Key.Type,
                                                 Value = grp.Sum(t => t.Value).Value
                                             }).Where(p=>p.Year>=yearParse && p.Year<=yearParse+4).Take(10).ToList();
                    var paramYear = (from y in db.DocumentStatistics
                                     where (y.Type == "FeeDocumentView" || y.Type == "FreeDocumentView") && y.CateID == cate_id
                                     select new { paramYear = y.Year }).Where(p => p.paramYear >= yearParse && p.paramYear <= yearParse + 4).Take(10).Distinct().ToList();
                    for (int i = 0; i < paramYear.Count(); i++)
                    {
                        if (i != paramYear.Count() - 1)
                        {
                            paramYears += paramYear.ElementAt(i).paramYear.Value + ", ";

                        }
                        else
                        {
                            paramYears += paramYear.ElementAt(i).paramYear.Value;

                        }
                    }
                    for (int i = 0; i < documentstatistic.Count(); i++)
                    {
                        //if (i != documentstatistic.Count() - 1)
                        //{
                        if (documentstatistic.ElementAt(i).Type == "FeeDocumentView")
                        {
                            feedoc += documentstatistic.ElementAt(i).Value + ", ";
                        }
                        else
                        {
                            freedoc += documentstatistic.ElementAt(i).Value + ", ";
                        }
                    }
                }
            }


            // Call TreeViewHelper
            DocumentCategoryTreeViewHelper obj = new DocumentCategoryTreeViewHelper();
            obj.BuildHierarchicalList(0, ""); // Build dropdownList
            List<DocumentCategoriesModel> items = obj.DocumentList;
            
            // Assign ViewBag            
            ViewBag.ParentList = new SelectList(items, "CategoryID", "CategoryName");
            ViewBag.YearList = QML.Web.UI.Areas.DocumentAdmin.Helpers.DocumentsHelper.GetYearView();
            ViewBag.paramYear = paramYears;
            ViewBag.feedoc = feedoc;
            ViewBag.freedoc = freedoc;
            return View(documentstatistic);
        }

        [Menu(Title = "Các đầu mục tài liệu", Path = "Thống kê", Order = 2)]
        public ActionResult StatisticDocumentNum(string Year)
        {
            DocumentCategoryTreeViewHelper tree = new DocumentCategoryTreeViewHelper();
            int total = 0;
            if (string.IsNullOrEmpty(Year) == true)
            {
                tree.renderTreeView(0, "#", 0, true);
                total = db.DocumentsFiles.Where(d => d.IsDeleted == false).Count();
            }
            else
            {
                int paramYear = Int32.Parse(Year);
                total = db.DocumentsFiles.Where(d => d.IsDeleted == false && d.CreatedDate.Value.Year == paramYear).Count();
                tree.renderTreeView(0, "#", paramYear, true);
            }
            ViewBag.tree_cate = tree.trees;
            var paramYears = (from y in db.DocumentsFiles
                              where y.IsDeleted == false
                              select new { paramYear = y.CreatedDate.Value.Year }).Distinct().ToList();
            ViewBag.YearList = new SelectList(paramYears, "paramYear", "paramYear");
            ViewBag.Total = total;
            return View();
        }

        [Menu(Title = "Khoản thu phí hàng năm", Path = "Thống kê", Order = 1)]
        public ActionResult StatisticIncome(string yearView)
        {            
                string amount = "";
                string paramYear = "";
                //var income = (from ds in db.DocumentStatistics
                //              where ds.Type == "income"
                //              orderby ds.Year descending
                //              select new
                //              {
                //                  paramYear = ds.Year.Value,
                //                  amount = ds.Value
                //              }).Distinct().ToList();
           if (string.IsNullOrEmpty(yearView) == true)
            {
                var income = db.DocumentStatistics.Where(ds => ds.Type == "income").Select(ds => new
                {
                    paramYear = ds.Year.Value,
                    amount = ds.Value
                }).OrderByDescending(ds => ds.paramYear).Take(5).ToList();
                for (int i = 0; i < income.Count(); i++)
                {

                    if (i != (income.Count() - 1))
                    {
                        amount += income.ElementAt(i).amount + ", ";
                        paramYear += income.ElementAt(i).paramYear + ", ";
                    }
                    else
                    {
                        amount += income.ElementAt(i).amount;
                        paramYear += income.ElementAt(i).paramYear;
                    }
                }

            }
               //if yearView #null
            else {
               long _yearView = long.Parse(yearView);
                var income = db.DocumentStatistics.Where(ds => ds.Type == "income"&& ds.Year>=_yearView).Select(ds => new
                {
                    paramYear = ds.Year.Value,
                    amount = ds.Value
                }).Take(5).OrderByDescending(ds => ds.paramYear).ToList();
                for (int i = 0; i < income.Count(); i++)
                {

                    if (i != (income.Count() - 1))
                    {
                        amount += income.ElementAt(i).amount + ", ";
                        paramYear += income.ElementAt(i).paramYear + ", ";
                    }
                    else
                    {
                        amount += income.ElementAt(i).amount;
                        paramYear += income.ElementAt(i).paramYear;
                    }
                }
            }
           ViewBag.yearView = QML.Web.UI.Areas.DocumentAdmin.Helpers.DocumentsHelper.GetYearViewIncome();
            ViewBag.paramYear = paramYear;
            ViewBag.income = amount;

            return View();
        }

        [Menu(Title = "Lịch sử lần đọc mất phí", Path = "Thống kê", Order = 1)]
        public ActionResult FeeReadHistory(string username, string month, string year)
        {
            IEnumerable<FeeHistoryView> _moneyLog;
            /*convert string to id for searching*/
            if (username == ""||username==null)
            {
                //first time run
                if (month == null && year == null||month == "Tất cả" && year=="Tất cả")
                {
                    _moneyLog = db.FeeHistoryViews.ToList().OrderByDescending(p => p.TimeRecorded);
                }
                    //chỉ search theo ngày tháng           
                else
                {
                    int? paramYear = GetInt(year) ?? DateTime.Now.Year;
                    int? paramMonth = GetInt(month);
                        //Tất cả cả tháng cả năm
                    if(month!="Tất cả" && year !="Tất cả")
                    {
                        _moneyLog = db.FeeHistoryViews.ToList().Where(p => p.month == paramMonth && p.year == paramYear).OrderByDescending(p => p.TimeRecorded);
                    }
                    //chỉ Tất cả tháng
                    else if (month != "Tất cả") {
                        _moneyLog = db.FeeHistoryViews.ToList().Where(p => p.month == paramMonth).OrderByDescending(p => p.TimeRecorded);
                    }
                        //chỉ Tất cả năm
                    else if (year != "Tất cả")
                    {
                        _moneyLog = db.FeeHistoryViews.ToList().Where(p => p.year == paramYear).OrderByDescending(p => p.TimeRecorded);
                    }
                    else {
                        _moneyLog = null;
                    }
                    
                }
            }
            else
            {
                IEnumerable<auth_Users> _user = db.auth_Users.Where(p => p.Username == username);
                if (_user != null)
                {
                    long _userID = 0;
                    foreach (var item in _user.ToList())
                    {
                        _userID = item.UserId;
                    }
                    //chỉ Tất cả có người dùng, k tháng k năm
                    if (month == null && year == null || month == "Tất cả" && year == "Tất cả")
                    {
                        _moneyLog = db.FeeHistoryViews.ToList().OrderByDescending(p => p.TimeRecorded).Where(p => p.UserId == _userID);
                    }
                    else
                    {

                        int? paramYear = GetInt(year) ?? DateTime.Now.Year;
                        int? paramMonth = GetInt(month);
                        //Tất cả ng dùng và tháng và năm
                        if (month != "Tất cả" && year != "Tất cả")
                        {
                            _moneyLog = db.FeeHistoryViews.ToList().Where(p => p.month == paramMonth && p.year == paramYear && p.UserId == _userID).OrderByDescending(p => p.TimeRecorded);
                        }
                        //Tất cả ng dùng và Tất cả tháng
                        else if (month != "Tất cả")
                        {
                            _moneyLog = db.FeeHistoryViews.ToList().Where(p => p.month == paramMonth).OrderByDescending(p => p.TimeRecorded).Where(p => p.UserId == _userID);
                        }
                        //Tất cả ng dùng và Tất cả năm
                        else if (year != "Tất cả")
                        {
                            _moneyLog = db.FeeHistoryViews.ToList().Where(p => p.year == paramYear).OrderByDescending(p => p.TimeRecorded).Where(p => p.UserId == _userID);
                        }
                        else
                        {
                            _moneyLog = null;
                        }
                    }                
                    ViewBag.uName = username;
                }
                //không có người dùng
                else
                {
                    _moneyLog = db.FeeHistoryViews.ToList().OrderByDescending(p => p.TimeRecorded).Where(p => p.UserId == 0);
                }
            }                
                ViewBag.Username = QML.Web.UI.Areas.DocumentAdmin.Helpers.DocumentsHelper.getUserList();
                ViewBag.Month = QML.Web.UI.Areas.DocumentAdmin.Helpers.DocumentsHelper.GetMonthListDataDefault();
                ViewBag.Year = QML.Web.UI.Areas.DocumentAdmin.Helpers.DocumentsHelper.GetYearListDataDefault();                
                return View(_moneyLog);                    
        }

        [Menu(Title = "Lịch sử nạp tiền gần nhất", Path = "Thống kê", Order = 1)]
        public ActionResult MoneyLogHistory(string username, string month, string year)
        {            
            IEnumerable<MoneyLog> _moneyLog;
            /*convert string to id for searching*/
            if (username == ""||username==null)
            {
                //first time run
                if ((month == null && year == null)||(month == "Tất cả" && year=="Tất cả"))
                {
                    _moneyLog = db.MoneyLogs.ToList().OrderByDescending(p => p.dateInput);
                }
                    //chỉ search theo ngày tháng           
                else
                {
                    int? paramYear = GetInt(year);
                    int? paramMonth = GetInt(month);
                    
                    //Tất cả cả tháng cả năm
                    if (month != "Tất cả" && year != "Tất cả")
                    {
                        _moneyLog = db.MoneyLogs.ToList().Where(p => p.month == paramMonth && p.year == paramYear).OrderByDescending(p => p.dateInput);
                    }
                    //chỉ Tất cả tháng
                    else if (month != "Tất cả") {
                        _moneyLog = db.MoneyLogs.ToList().Where(p => p.month == paramMonth).OrderByDescending(p => p.dateInput);
                    }
                        //chỉ Tất cả năm
                    else if (year != "Tất cả")
                    {
                        _moneyLog = db.MoneyLogs.ToList().Where(p => p.year == paramYear).OrderByDescending(p => p.dateInput);
                    }                    
                    else { 
                        _moneyLog = null;
                    }
                }
            }
            else
            {
                IEnumerable<auth_Users> _user = db.auth_Users.Where(p => p.Username == username);
                if (_user != null)
                {
                    long _userID = 0;
                    foreach (var item in _user.ToList())
                    {
                        _userID = item.UserId;
                    }
                    //chỉ Tất cả có người dùng, k tháng k năm
                    if (month == null && year == null || month == "Tất cả" && year == "Tất cả")
                    {
                        _moneyLog = db.MoneyLogs.ToList().OrderByDescending(p => p.dateInput).Where(p => p.userID == _userID);
                    }
                    else
                    {
                        int? paramYear = GetInt(year);
                        int? paramMonth = GetInt(month);
                        //Tất cả ng dùng và tháng và năm
                        if (month != "Tất cả" && year != "Tất cả")
                        {
                            _moneyLog = db.MoneyLogs.ToList().Where(p => p.month == paramMonth && p.year == paramYear && p.userID == _userID).OrderByDescending(p => p.dateInput);
                        }
                        //Tất cả ng dùng và Tất cả tháng
                        else if (month != "Tất cả")
                        {
                            _moneyLog = db.MoneyLogs.ToList().Where(p => p.month == paramMonth).OrderByDescending(p => p.dateInput).Where(p => p.userID == _userID);
                        }
                        //Tất cả ng dùng và Tất cả năm
                        else if (year != "Tất cả")
                        {
                            _moneyLog = db.MoneyLogs.ToList().Where(p => p.year == paramYear).OrderByDescending(p => p.dateInput).Where(p => p.userID == _userID);
                        }
                        
                        else {
                            _moneyLog = null;
                        }
                    }
                    ViewBag.uName = username;
                }
                //không có người dùng
                else
                {
                    _moneyLog = db.MoneyLogs.ToList().OrderByDescending(p => p.dateInput).Where(p => p.userID == 0);
                }
            }                
                ViewBag.Username = QML.Web.UI.Areas.DocumentAdmin.Helpers.DocumentsHelper.getUserList();
                ViewBag.Month = QML.Web.UI.Areas.DocumentAdmin.Helpers.DocumentsHelper.GetMonthListDataDefault();
                ViewBag.Year = QML.Web.UI.Areas.DocumentAdmin.Helpers.DocumentsHelper.GetYearListDataDefault();                
                return View(_moneyLog);            
        }

        //tải file SWF
        [HttpPost]
        public ActionResult UploadSWF(HttpPostedFileBase file)
        {
            string fileName = Path.GetFileName(file.FileName);
            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = fileName.Replace(" ", "_");                
            }
            var path = Path.Combine(Server.MapPath("~/Resources"), fileName);
            file.SaveAs(path);
            return RedirectToAction("Index");
        }
        
        }
    


}
