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
using QML.Web.UI.Helpers;
using PagedList;
using System.Web.Routing;
using Ionic.Zip;

namespace QML.Web.UI.Areas.DocumentAdmin.Controllers
{
    public class DocumentController : SecuredController
    {
        //
        // GET: /DocumentAdmin/Document/
        HanuELibraryEntities db = new HanuELibraryEntities();
        EssentialController ec = new EssentialController();

        [Menu(Title = "Danh sách tài liệu", Path = "Tài liệu", Order = 1)]
        public ActionResult Index(int? page, long? id_cate = 0)
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
                                CheckHasInfo = d.IsHasInfo.Value == true ? "checked" : "",
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
                                CheckHasInfo = d.IsHasInfo.Value == true ? "checked" : "",
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
            int currPage = int.Parse(collection["page"]) + 1;
            long id_cate = long.Parse(collection["cate"]);
            string keyword = collection["keyword"];
            string attribute = collection["attribute"];
            string value = collection["value"];
            string[] listID = collection["idValue"].Split(',');
            DocumentsFile document;
            long id_document = Int64.Parse(listID[0]);
            switch (collection["actionName"])
            {
                case "cataloging":
                    //nếu chọn 1 file để biên mục
                    if (listID.Count() == 1)
                    {
                        return RedirectToAction("ExtraInfo", new { id = Int64.Parse(listID[0]), page = currPage.ToString(), id_cate = id_cate, keyword = keyword, attribute = attribute, value = value });
                    }
                    string listId = collection["idValue"];
                    return RedirectToAction("ExtraInfoMultipleFile", new { listId = listId, page = currPage.ToString(), id_cate = id_cate, keyword = keyword, attribute = attribute, value = value });

                case "thumbnail":
                    foreach (string item in listID)
                    {
                        long id_doc = long.Parse(item);
                        var df = db.DocumentsFiles.Where(d => d.DocumentID == id_doc).FirstOrDefault();

                        if (df.DocumentFormat.FileFormat.Equals("pdf"))
                        {
                            try
                            {
                                string filePath = Path.Combine(Server.MapPath("~/Resources"), df.FileSource);
                                //first page of df.FileSource
                                string pdfThumbPage = Path.Combine(Server.MapPath("~/Resources"), "thumbnail-temp.pdf");
                                //converted image of pdfThumbPage, however if this image after convension dimension is too large, this image will have extra white space
                                string pngName0 = "temp.png";
                                string pngName0Path = Path.Combine(Server.MapPath("~/uploads"), pngName0);
                                //pngName0 after trim out all white space, and assign as document thumbnail
                                string pngName = DateTime.Now.ToOADate() + ".png";
                                string pngNamePath = Path.Combine(Server.MapPath("~/uploads"), pngName);

                                PdfHelper.ExtractPage(filePath, pdfThumbPage, 1);
                                PdfHelper.CreatePdfThumbnail(pdfThumbPage, pngName0);
                                PdfHelper.ImageTrim(pngName0Path, pngNamePath);
                                try { System.IO.File.Delete(pngName0Path); }
                                catch (Exception) { } //do nothing

                                df.Thumbnail = pngName;
                                db.SaveChanges();
                                TempData["Message"] = "1";
                            }
                            catch (Exception) { TempData["Message"] = "0"; }
                        }
                    }

                    if (keyword.Length > 0 || attribute.Length > 0 || value.Length > 0)
                        return RedirectToAction("Filter", new { keyword = keyword, attribute = attribute, value = value, id_cate = id_cate });
                    return RedirectToAction("Index", new { page = currPage.ToString(), id_cate = id_cate });

                case "moveCate":
                    // Call TreeViewHelper
                    DocumentCategoryTreeViewHelper obj = new DocumentCategoryTreeViewHelper();
                    obj.BuildHierarchicalList(0, ""); // Build dropdownList
                    List<DocumentCategoriesModel> items = obj.DocumentList;
                    ViewBag.ListID = collection["idValue"];
                    ViewBag.ParentList = new SelectList(items, "CategoryID", "CategoryName");
                    ViewBag.IndexPage = currPage;
                    ViewBag.Category = id_cate;
                    ViewBag.Keyword = keyword;
                    ViewBag.Attribute = attribute;
                    ViewBag.Value = value;
                    return View("../Document/moveCate", new { page = currPage, cate = id_cate });

                case "setStatus":
                    ViewBag.ListID = collection["idValue"];
                    var values = new[]
                    {
                        new { Value = "Không hiển thị", Text = "Không hiển thị" },
                        new { Value = "Hiển thị", Text = "Hiển thị" },
                        new { Value = "Mật", Text = "Mật" },
                    };
                    ViewBag.StatusList = new SelectList(values, "Value", "Text");
                    ViewBag.IndexPage = currPage;
                    ViewBag.Category = id_cate;
                    ViewBag.Keyword = keyword;
                    ViewBag.Attribute = attribute;
                    ViewBag.Value = value;
                    return View("../Document/setStatus", new { page = currPage, cate = id_cate });

                case "setFee":
                    DocumentsFileModel doc = db.DocumentsFiles.Where(p => p.DocumentID == id_document).Select(p => new DocumentsFileModel
                    {
                        BookFee = p.BookFee.Value,
                        FileName = p.FileName,
                        DocumentID = p.DocumentID
                    }).FirstOrDefault();
                    ViewBag.IndexPage = currPage;
                    ViewBag.Category = id_cate;
                    ViewBag.Keyword = keyword;
                    ViewBag.Attribute = attribute;
                    ViewBag.Value = value;
                    ViewBag.ListID = collection["idValue"];
                    return View("../Document/setFee", new { page = currPage, cate = id_cate });

                case "deletePermanent":
                    foreach (string item in listID)
                    {
                        long id_doc = long.Parse(item);
                        document = db.DocumentsFiles.Where(d => d.DocumentID == id_doc).FirstOrDefault();
                        try
                        {
                            System.IO.File.Delete(Server.MapPath("~/Resources/" + document.FileSource));
                            if (document.DocumentFormat.FileFormat.Equals("pdf"))
                            {
                                System.IO.File.Delete(Server.MapPath("~/uploads/" + document.Thumbnail));
                            }
                            if (!document.FileName.Equals(document.FileSource)) //in case of docx file
                            {
                                System.IO.File.Delete(Server.MapPath("~/Resources/" + document.FileName));
                            }
                        }
                        catch (Exception) { }
                        db.DocumentsFiles.DeleteObject(document);

                        //Logging
                        RouteData routeData = ControllerContext.RouteData;
                        ActionLog action = new ActionLog
                        {
                            user_id = ec.getUserId(),
                            executed_user = ec.getUserName(),
                            user_role = ec.getUserRole(ec.getUserId()),

                            area_name = routeData.DataTokens["area"].ToString(),
                            controller_name = routeData.Values["controller"].ToString(),
                            action_name = routeData.Values["action"].ToString(),
                            executed_time = DateTime.Now,

                            description = "xóa tài liệu",
                            object_id = id_doc.ToString(),
                            object_name = document.FileName,
                            additional_info = null
                        };

                        db.ActionLogs.AddObject(action);
                        db.SaveChanges();
                    }

                    if (keyword.Length > 0 || attribute.Length > 0 || value.Length > 0)
                        return RedirectToAction("Filter", new { keyword = keyword, attribute = attribute, value = value, id_cate = id_cate });
                    return RedirectToAction("Index", new { page = currPage.ToString(), id_cate = id_cate });

                case "Download":
                    if (listID.Count() == 1)
                    {
                        long docId = long.Parse(listID[0]);
                        var docFile = db.DocumentsFiles.Where(d => d.DocumentID == docId).FirstOrDefault();
                        FileInfo file = new FileInfo(Server.MapPath("~/Resources/" + docFile.FileSource));

                        Response.Clear();
                        Response.ContentType = "application/pdf";
                        Response.AddHeader("content-disposition", "attachment; filename=" + file.Name);
                        Response.TransmitFile(file.ToString());
                        Response.Flush();
                        Response.End();
                    }
                    else
                    {
                        List<string> fileNames = new List<string>();
                        foreach (var item in listID)
                        {
                            int docId = int.Parse(item);
                            var docFile = db.DocumentsFiles.Where(d => d.DocumentID == docId).FirstOrDefault();
                            var path = Path.Combine(Server.MapPath("~/Resources"), docFile.FileSource);
                            fileNames.Add(path);
                        }

                        using (var zip = new ZipFile())
                        {
                            foreach (string fileName in fileNames)
                            {
                                zip.AddFile(fileName, "");
                            }
                            var zipPath = Path.Combine(Server.MapPath("~/Resources"), "download.zip");
                            zip.Save(zipPath);

                            Response.Clear();
                            Response.ContentType = "application/zip";
                            Response.AddHeader("content-disposition", "attachment; filename=" + DateTime.Now.ToOADate() + "_elib_document_download.zip");
                            Response.TransmitFile(zipPath);
                            Response.Flush();
                            Response.End();
                        } 
                    }

                    if (keyword.Length > 0 || attribute.Length > 0 || value.Length > 0)
                        return RedirectToAction("Filter", new { keyword = keyword, attribute = attribute, value = value, id_cate = id_cate });
                    return RedirectToAction("Index", new { page = currPage.ToString(), id_cate = id_cate });
                default:
                    return RedirectToAction("Index");

            }
        }

        [HttpPost]
        public ActionResult setFee(string page, string cate, string keyword, string attribute, string value, FormCollection collection)
        {
            long id_cate = long.Parse(cate);
            string[] list = collection["listID"].Split(',');
            DocumentsFile entity;
            foreach (string item in list)
            {
                long id_doc = Int64.Parse(item);
                entity = db.DocumentsFiles.Where(d => d.DocumentID == id_doc).FirstOrDefault();
                string oldFee = entity.BookFee.ToString();
                entity.BookFee = float.Parse(collection["fee"]);
                db.SaveChanges();

                //Logging
                RouteData routeData = ControllerContext.RouteData;
                ActionLog action = new ActionLog
                {
                    user_id = ec.getUserId(),
                    executed_user = ec.getUserName(),
                    user_role = ec.getUserRole(ec.getUserId()),

                    area_name = routeData.DataTokens["area"].ToString(),
                    controller_name = routeData.Values["controller"].ToString(),
                    action_name = routeData.Values["action"].ToString(),
                    executed_time = DateTime.Now,

                    description = "thay đổi phí tài liệu",
                    object_id = id_doc.ToString(),
                    object_name = entity.FileName,
                    additional_info = oldFee + " -> " + entity.BookFee.ToString()
                };

                db.ActionLogs.AddObject(action);
                db.SaveChanges();
            }

            if (keyword.Length > 0 || attribute.Length > 0 || value.Length > 0)
                return RedirectToAction("Filter", new { keyword = keyword, attribute = attribute, value = value, id_cate = id_cate });
            return RedirectToAction("Index", new { page = page, id_cate = id_cate });
        }

        [HttpPost]
        public ActionResult setStatus(string page, string cate, string keyword, string attribute, string value, FormCollection collection)
        {
            long id_cate = long.Parse(cate);
            DocumentsFile document;
            string[] list = collection["listID"].Split(',');
            foreach (string item in list)
            {
                long id_doc = Int64.Parse(item);
                document = db.DocumentsFiles.Where(d => d.DocumentID == id_doc).FirstOrDefault();
                string oldStatus = document.Status;
                document.Status = collection["Status"];

                //Logging
                RouteData routeData = ControllerContext.RouteData;
                ActionLog action = new ActionLog
                {
                    user_id = ec.getUserId(),
                    executed_user = ec.getUserName(),
                    user_role = ec.getUserRole(ec.getUserId()),

                    area_name = routeData.DataTokens["area"].ToString(),
                    controller_name = routeData.Values["controller"].ToString(),
                    action_name = routeData.Values["action"].ToString(),
                    executed_time = DateTime.Now,

                    description = "thay đổi trạng thái tài liệu",
                    object_id = id_doc.ToString(),
                    object_name = document.FileName,
                    additional_info = oldStatus + " -> " + document.Status
                };

                db.ActionLogs.AddObject(action);
                db.SaveChanges();
            }

            if (keyword.Length > 0 || attribute.Length > 0 || value.Length > 0)
                return RedirectToAction("Filter", new { keyword = keyword, attribute = attribute, value = value, id_cate = id_cate });
            return RedirectToAction("Index", new { page = page, id_cate = id_cate });
        }

        [HttpPost]
        public ActionResult moveCate(string page, string cate, string keyword, string attribute, string value, FormCollection collection)
        {
            long id_cate0 = long.Parse(cate);
            DocumentsFile document;
            int id_cate = Int32.Parse(collection["CateID"]);
            string[] list = collection["listID"].Split(',');
            //List<int> ListID = new List<int>();
            foreach (string item in list)
            {
                long id_doc = Int64.Parse(item);
                document = db.DocumentsFiles.Where(d => d.DocumentID == id_doc).FirstOrDefault();
                int? oldCate = document.CategoryID;
                document.CategoryID = id_cate;

                //Logging
                RouteData routeData = ControllerContext.RouteData;
                ActionLog action = new ActionLog
                {
                    user_id = ec.getUserId(),
                    executed_user = ec.getUserName(),
                    user_role = ec.getUserRole(ec.getUserId()),

                    area_name = routeData.DataTokens["area"].ToString(),
                    controller_name = routeData.Values["controller"].ToString(),
                    action_name = routeData.Values["action"].ToString(),
                    executed_time = DateTime.Now,

                    description = "thay đổi danh mục(category) tài liệu",
                    object_id = id_doc.ToString(),
                    object_name = document.FileName,
                    additional_info = ec.getDocCategoryString(oldCate) + " -> " 
                        + ec.getDocCategoryString(document.CategoryID)
                };

                db.ActionLogs.AddObject(action);
                db.SaveChanges();
            }

            if (keyword.Length > 0 || attribute.Length > 0 || value.Length > 0)
                return RedirectToAction("Filter", new { keyword = keyword, attribute = attribute, value = value, id_cate = id_cate });
            return RedirectToAction("Index", new { page = page, id_cate = id_cate0 });
        }

        public ActionResult Filter(string keyword, string attribute, string value, long? id_cate = 0)
        {
            DocumentCategoryTreeViewHelper tree = new DocumentCategoryTreeViewHelper();
            List<DocumentsFile> document = db.DocumentsFiles.ToList();
            if (id_cate != 0)
            {
                document = document.Where(d => d.CategoryID == id_cate).ToList();
            }
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
                header += "<b>Từ khóa: " + keyword + "</b>";                                
                //lọc theo từ khóa
                switch(value){
                    //tất cả
                    case "0":
                        List<DocumentsFile> _ls = new List<DocumentsFile>();
                        //nhan đề
                        var _query = db.DocumentsFiles.Where(z => z.Document.Title.ToLower().Contains(keyword.ToLower()));
                        //bộ sưu tập
                        var _queryc = db.DocumentsFiles.Where(z => z.DocumentCategory.CategoryName.ToLower().Contains(keyword.ToLower()));
                        //tên file
                        var _queryn = db.DocumentsFiles.Where(z => z.FileName.ToLower().Contains(keyword.ToLower()));
                        //tác giả
                        var _query1 = db.DocumentsFiles.Where(z => z.Document.Creator.ToLower().Contains(keyword.ToLower()));
                        //chủ đề
                        var _query2 = db.DocumentsFiles.Where(z => z.Document.Subject.ToLower().Contains(keyword.ToLower()));
                        //tác giả phụ
                        var _query3 = db.DocumentsFiles.Where(z => z.Document.Contributor.ToLower().Contains(keyword.ToLower()));
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
                       var _query4 = db.DocumentsFiles.Where(z => z.Document.Date.Value.Year == _year);
                        //loại tài liệu
                       var _query5 = db.DocumentsFiles.Where(z => z.Document.Type.ToLower().Contains(keyword.ToLower()));
                        //mô tả vật lý
                       var _query6 = db.DocumentsFiles.Where(z => z.Document.Format.ToLower().Contains(keyword.ToLower()));
                        //định danh
                       var _query7 = db.DocumentsFiles.Where(z => z.Document.Identifier.ToLower().Contains(keyword.ToLower()));
                        //liên kết
                       var _query8 = db.DocumentsFiles.Where(z => z.Document.Relation.ToLower().Contains(keyword.ToLower()));
                        //nguồn
                       var _query9 = db.DocumentsFiles.Where(z => z.Document.Resource.ToLower().Contains(keyword.ToLower()));
                        //ngôn ngữ
                       var _query10 = db.DocumentsFiles.Where(z => z.Document.Language.ToLower().Contains(keyword.ToLower()));
                        //bản quyền
                       var _query11 = db.DocumentsFiles.Where(z => z.Document.Right.ToLower().Contains(keyword.ToLower()));
                        //tóm tắt
                       var _query12 = db.DocumentsFiles.Where(z => z.Document.Description.Contains(keyword));
                        //nhà xuất bản
                       var _query13 = db.DocumentsFiles.Where(z => z.Document.Publisher.ToLower().Contains(keyword.ToLower()));
                        //địa chỉ lưu trữ
                       var _query14 = db.DocumentsFiles.Where(z => z.Document.Coverage.ToLower().Contains(keyword.ToLower()));

                        foreach (var item in _query.ToList())
                        {
                            _ls.Add(item);
                        }
                        foreach (var item in _queryc.ToList())
                        {
                            _ls.Add(item);
                        }
                        foreach (var item in _queryn.ToList())
                        {
                            _ls.Add(item);
                        }
                        foreach (var item in _query1.ToList())
                        {
                            _ls.Add(item);
                        }
                        foreach (var item in _query2.ToList())
                        {
                            _ls.Add(item);
                        }
                        foreach (var item in _query3.ToList())
                        {
                            _ls.Add(item);
                        }
                        foreach (var item in _query4.ToList())
                        {
                            _ls.Add(item);
                        }
                        foreach (var item in _query5.ToList())
                        {
                            _ls.Add(item);
                        }
                        foreach (var item in _query6.ToList())
                        {
                            _ls.Add(item);
                        }
                        foreach (var item in _query7.ToList())
                        {
                            _ls.Add(item);
                        }
                        foreach (var item in _query8.ToList())
                        {
                            _ls.Add(item);
                        }
                        foreach (var item in _query9.ToList())
                        {
                            _ls.Add(item);
                        }
                        foreach (var item in _query10.ToList())
                        {
                            _ls.Add(item);
                        }
                        foreach (var item in _query11.ToList())
                        {
                            _ls.Add(item);
                        }
                        foreach (var item in _query12.ToList())
                        {
                            _ls.Add(item);
                        }
                        foreach (var item in _query13.ToList())
                        {
                            _ls.Add(item);
                        }
                        foreach (var item in _query14.ToList())
                        {
                            _ls.Add(item);
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
                    //bộ sưu tập
                    case "c":
                       List<DocumentsFile> lsc = new List<DocumentsFile>();
                       var queryc = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.DocumentFile.DocumentCategory.CategoryName.ToLower().Contains(keyword.ToLower()));
                       foreach (var item in queryc.ToList())
                       {
                           lsc.Add(item.DocumentFile);
                       }

                       document = lsc;
                       break;
                    //tên file
                    case "n":
                       List<DocumentsFile> lsn = new List<DocumentsFile>();
                       var queryn = db.DocumentsFiles.Join(db.Documents, q => q.DocumentID, r => r.DocumentID, (q, r) => new { DocumentFile = q, Doc = r }).Where(z => z.DocumentFile.FileName.ToLower().Contains(keyword.ToLower()));
                       foreach (var item in queryn.ToList())
                       {
                           lsn.Add(item.DocumentFile);
                       }

                       document = lsn;
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
            ViewBag.Attribute = attribute;
            ViewBag.value = value;
            ViewBag.id_cate = id_cate;
            return View("../Document/Index", models);
        }

        public ActionResult CreateGroup()
        {
            DocumentCategoryTreeViewHelper obj = new DocumentCategoryTreeViewHelper();
            obj.BuildHierarchicalList(0, ""); // Build dropdownList
            List<DocumentCategoriesModel> items = obj.DocumentList;
            items.Insert(0, new DocumentCategoriesModel { CategoryID = 0, CategoryName = "ROOT", ParentID = -1 });
            ViewBag.CateList = new SelectList(items, "CategoryID", "CategoryName");

            return View();
        }

        [HttpPost]
        public ActionResult CreateGroup(FormCollection form)
        {
            var category = new DocumentCategory
            {
                CategoryName = form["category"],
                ParentID = long.Parse(form["parentCategory"]),
                CreatedDate = DateTime.Now,
                CreatedUser = ec.getUserId(),
                Position = 0,
            };
            db.DocumentCategories.AddObject(category);
            db.SaveChanges();

            RouteData routeData = ControllerContext.RouteData;
            ActionLog action0 = new ActionLog
            {
                user_id = ec.getUserId(),
                executed_user = ec.getUserName(),
                user_role = ec.getUserRole(ec.getUserId()),

                area_name = routeData.DataTokens["area"].ToString(),
                controller_name = routeData.Values["controller"].ToString(),
                action_name = routeData.Values["action"].ToString(),
                executed_time = DateTime.Now,

                description = "thêm bộ sưu tập",
                object_id = category.CategoryID.ToString(),
                object_name = category.CategoryName,
                additional_info = "thuộc bộ sưu tập " + ec.getDocCategoryString((int)category.ParentID),
            };

            db.ActionLogs.AddObject(action0);
            db.SaveChanges();

            for (int i = 0; i < Request.Files.Count; i++)
            {
                HttpPostedFileBase file = Request.Files[i];
                if (file != null && file.ContentLength > 0)
                {
                    DocumentsFile entity = new DocumentsFile();
                    entity.CategoryID = category.CategoryID;
                    string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                    string extension = Path.GetExtension(file.FileName);
                    string fullName = fileName + extension;
                    //check format
                    if (!extension.Equals(".pdf") && !extension.Equals(".mp3")
                        && !extension.Equals(".mp4") && !extension.Equals(".avi"))
                    {
                        TempData["fileFormatError"] = true;
                        return RedirectToAction("Create");
                    }
                    //check for duplication
                    bool isDuplicate = db.DocumentsFiles.Any(p => p.FileName == fullName);
                    if (isDuplicate)
                    {
                        TempData["duplicateError"] = true;
                        return RedirectToAction("CreateGroup");
                    }
                    else
                    {
                        entity.FileName = fullName;
                        string path = Path.Combine(Server.MapPath("~/Resources"), fullName);
                        file.SaveAs(path);

                        FileInfo finfo = new FileInfo(HttpContext.Server.MapPath("~/Resources/" + fullName));
                        long FileInBytes = finfo.Length;
                        long FileInKB = finfo.Length / 1024;
                        entity.Size = FileInKB;
                        switch (extension.Substring(1))
                        {
                            /*
                                 case "doc": case "docx": case "ppt": case "pptx":
                                    entity.FileSource = fileName + ".pdf";
                                    entity.Thumbnail = "pdf.png";
                                    break;
                                */
                            case "pdf":
                                entity.FileSource = fullName;
                                entity.Thumbnail = "pdf.png";
                                entity.FormatID = 1;
                                break;
                            case "mp3":
                                entity.FileSource = fullName;
                                entity.Thumbnail = "mp3.png";
                                entity.FormatID = 3;
                                break;
                            case "mp4":
                                entity.FileSource = fullName;
                                //apparently avi.png is a thumbnail for video files
                                entity.Thumbnail = "avi.png";
                                entity.FormatID = 2;
                                break;
                            case "flv":
                                entity.FileSource = fullName;
                                entity.Thumbnail = "avi.png";
                                entity.FormatID = 4;
                                break;
                        }

                        entity.BookFee = 0;
                        entity.ViewCount = 0;
                        entity.Status = "Không hiển thị";
                        entity.IsDeleted = false;
                        entity.IsHasInfo = false;
                        entity.CreatedDate = entity.LastModifiedDate = DateTime.Now;
                        entity.CreatedUser = entity.LastModifiedUser = AuthManager.GetUser().Username;
                        db.DocumentsFiles.AddObject(entity);
                        db.SaveChanges();

                        //Logging
                        ActionLog action = new ActionLog
                        {
                            user_id = ec.getUserId(),
                            executed_user = ec.getUserName(),
                            user_role = ec.getUserRole(ec.getUserId()),

                            area_name = routeData.DataTokens["area"].ToString(),
                            controller_name = routeData.Values["controller"].ToString(),
                            action_name = routeData.Values["action"].ToString(),
                            executed_time = DateTime.Now,

                            description = "thêm tài liệu",
                            object_id = entity.DocumentID.ToString(),
                            object_name = fullName,
                            additional_info = entity.DocumentCategory.CategoryName,
                        };

                        db.ActionLogs.AddObject(action);
                        db.SaveChanges();
                    }
                }
            }
                return RedirectToAction("Index");
        }

        public ActionResult Create()
        {
            // Call TreeViewHelper
            DocumentCategoryTreeViewHelper obj = new DocumentCategoryTreeViewHelper();
            obj.BuildHierarchicalList(0, ""); // Build dropdownList
            List<DocumentCategoriesModel> items = obj.DocumentList;
            // Assign ViewBag            
            ViewBag.ParentList = new SelectList(items, "CategoryID", "CategoryName");
            //ViewBag.Status = TempData["statusError"].ToString(); 
            //DocumentModel model = new DocumentModel();
            //model.Date = DateTime.Today;
            return View();
        }

        //
        // POST: /DocumentAdmin/Document/Create

        [HttpPost]
        public ActionResult Create(DocumentsFileModel model)
        {
            try
            {
                RouteData routeData = ControllerContext.RouteData;
                
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFileBase file = Request.Files[i];
                    if (file != null && file.ContentLength > 0)
                    {
                        DocumentsFile entity = new DocumentsFile();
                        entity.CategoryID = model.CategoryID;
                        string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                        string extension = Path.GetExtension(file.FileName);
                        string fullName = fileName + extension;
                        //check format
                        if (!extension.Equals(".pdf") && !extension.Equals(".mp3") 
                            && !extension.Equals(".mp4") && !extension.Equals(".flv"))
                        {
                            TempData["fileFormatError"] = true;
                            return RedirectToAction("Create");
                        }

                        //check for duplication
                        bool isDuplicate = db.DocumentsFiles.Any(p => p.FileName == fullName);
                        if (isDuplicate)
                        {
                            TempData["duplicateError"] = true;
                            return RedirectToAction("Create");
                        }
                        else
                        {
                            entity.FileName = fullName;
                            string path = Path.Combine(Server.MapPath("~/Resources"), fullName);
                            file.SaveAs(path);

                            /*
                            if (extension != ".pdf")
                            {
                                bool converted = PdfHelper.ConvertToPdf(path);

                                if (!converted)
                                {
                                    TempData["fileFormatError"] = true;
                                    return RedirectToAction("Create");
                                }
                            }
                            */

                            FileInfo finfo = new FileInfo(HttpContext.Server.MapPath("~/Resources/" + fullName));
                            long FileInBytes = finfo.Length;
                            long FileInKB = finfo.Length / 1024;
                            entity.Size = FileInKB;
                            switch (extension.Substring(1))
                            {
                                /*
                                 case "doc": case "docx": case "ppt": case "pptx":
                                    entity.FileSource = fileName + ".pdf";
                                    entity.Thumbnail = "pdf.png";
                                    break;
                                */
                                case "pdf":
                                    entity.FileSource = fullName;
                                    entity.Thumbnail = "pdf.png";
                                    entity.FormatID = 1;
                                    break;
                                case "mp3":
                                    entity.FileSource = fullName;
                                    entity.Thumbnail = "mp3.png";
                                    entity.FormatID = 3;
                                    break;
                                case "mp4": 
                                    entity.FileSource = fullName;
                                    //apparently avi.png is a thumbnail for video files
                                    entity.Thumbnail = "avi.png"; 
                                    entity.FormatID = 2;
                                    break;
                                case "flv":
                                    entity.FileSource = fullName;
                                    entity.Thumbnail = "avi.png";
                                    entity.FormatID = 4;
                                    break;
                            }

                            entity.BookFee = 0;
                            entity.ViewCount = 0;
                            entity.Status = "Không hiển thị";
                            entity.IsDeleted = false;
                            entity.IsHasInfo = false;
                            entity.CreatedDate = entity.LastModifiedDate = DateTime.Now;
                            entity.CreatedUser = entity.LastModifiedUser = AuthManager.GetUser().Username;
                            db.DocumentsFiles.AddObject(entity);
                            db.SaveChanges();

                            //Logging
                            ActionLog action = new ActionLog
                            {
                                user_id = ec.getUserId(),
                                executed_user = ec.getUserName(),
                                user_role = ec.getUserRole(ec.getUserId()),

                                area_name = routeData.DataTokens["area"].ToString(),
                                controller_name = routeData.Values["controller"].ToString(),
                                action_name = routeData.Values["action"].ToString(),
                                executed_time = DateTime.Now,

                                description = "thêm tài liệu",
                                object_id = entity.DocumentID.ToString(),
                                object_name = fullName,
                                additional_info = entity.DocumentCategory.CategoryName,
                            };
                            
                            db.ActionLogs.AddObject(action);
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return RedirectToAction("Index");
        }

        public ActionResult ExtraInfo(long id, string page, long id_cate, string keyword, string attribute, string value)
        {
            ViewBag.IndexPage = page;
            ViewBag.Category = id_cate;
            ViewBag.Keyword = keyword;
            ViewBag.Attribute = attribute;
            ViewBag.Value = value;
            var docInfo = (from d in db.DocumentsFiles
                          where d.DocumentID == id
                          select new DocumentEssential
                          {
                              Title = d.Document.Title,
                              Creator = d.Document.Creator,
                              Subject = d.Document.Subject,
                              Description = d.Document.Description,
                              Publisher = d.Document.Publisher,
                              Contributor = d.Document.Contributor,
                              Year = d.Document.Date.Value.Year,
                              Type = d.Document.Type,
                              Format = d.Document.Format,
                              Identifier = d.Document.Identifier,
                              Resource = d.Document.Resource,
                              Language = d.Document.Language,
                              Relation = d.Document.Relation,
                              Coverage = d.Document.Coverage,
                              Right = d.Document.Right,
                              DocumentID = d.DocumentID,
                              FileName = d.FileName,
                              CategoryID = d.CategoryID.Value,
                              CategoryName = d.DocumentCategory.CategoryName,
                              BookFee = d.BookFee.Value,
                              Status = d.Status,
                              Thumbnail = d.Thumbnail,
                          }).FirstOrDefault();

            return View(docInfo);
        }

        [HttpPost]
        public ActionResult ExtraInfo(long id, string page, string cate, string keyword, string attribute, string value, DocumentEssential model, HttpPostedFileBase img)
        {
            string fileName = null;
            long id_cate = long.Parse(cate);
            if (img != null)
            {
                fileName = DateTime.Now.Ticks.ToString() + Path.GetFileName(img.FileName);
                string path = Path.Combine(Server.MapPath("~/uploads"), fileName);
                img.SaveAs(path);
            }
            Document entity = db.Documents.FirstOrDefault(p => p.DocumentID == id);
            DocumentsFile fileEntity = db.DocumentsFiles.FirstOrDefault(p => p.DocumentID == id);
            
            if (entity != null)
            {
                entity.Title = model.Title;
                entity.Creator = model.Creator;
                entity.Subject = model.Subject;
                entity.Description = model.Description;
                entity.Publisher = model.Publisher;
                entity.Contributor = model.Contributor;               
                entity.Date = (model.Year != null) ? new DateTime(model.Year.Value, 1, 1) : (DateTime?) null;
                entity.Type = model.Type;
                entity.Format = model.Format;
                entity.Identifier = model.Identifier;
                entity.Resource = model.Resource;
                entity.Language = model.Language;
                entity.Relation = model.Relation;
                entity.Coverage = model.Coverage;
                entity.Right = model.Right;
                fileEntity.CategoryID = model.CategoryID;
                fileEntity.Status = model.Status;
                fileEntity.BookFee = model.BookFee;
                if (fileName != null) fileEntity.Thumbnail = fileName;
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
                doc.Date = (model.Year != null) ? new DateTime(model.Year.Value, 1, 1) : (DateTime?) null;
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
                docfile.CategoryID = model.CategoryID;
                docfile.Status = model.Status;
                docfile.BookFee = model.BookFee;
                if (fileName != null) docfile.Thumbnail = fileName;
            }
            db.SaveChanges();

            //Logging
            RouteData routeData = ControllerContext.RouteData;
            var document = db.DocumentsFiles.FirstOrDefault(p => p.DocumentID == id);
            ActionLog action = new ActionLog
            {
                user_id = ec.getUserId(),
                executed_user = ec.getUserName(),
                user_role = ec.getUserRole(ec.getUserId()),

                area_name = routeData.DataTokens["area"].ToString(),
                controller_name = routeData.Values["controller"].ToString(),
                action_name = routeData.Values["action"].ToString(),
                executed_time = DateTime.Now,

                description = "biên mục/thay đổi thông tin tài liệu",
                object_id = id.ToString(),
                object_name = document.FileName,
                additional_info = null
            };

            db.ActionLogs.AddObject(action);
            db.SaveChanges();

            if (keyword.Length > 0 || attribute.Length > 0 || value.Length > 0)
                return RedirectToAction("Filter", new { keyword = keyword, attribute = attribute, value = value, id_cate = id_cate });
            return RedirectToAction("Index", new { id_cate = id_cate, page = page });
        }

        public ActionResult ExtraInfoMultipleFile(string listId, string page, long id_cate, string keyword, string attribute, string value)
        {
            ViewBag.IndexPage = page;
            ViewBag.Category = id_cate;
            ViewBag.Keyword = keyword;
            ViewBag.Attribute = attribute;
            ViewBag.Value = value;
            ViewBag.ListID = listId;
            
            return View();
        }

        [HttpPost]
        public ActionResult ExtraInfoMultipleFile(string page, string cate, string keyword, string attribute, string value, FormCollection collection)
        {
            long id_cate = long.Parse(cate);
            string[] list = collection["listID"].Split(',');
            long[] docIds = new long[list.Length];
            for (int i = 0; i < docIds.Length; i++)
            {
                docIds[i] = Int64.Parse(list[i]);
            }
            var documents = db.DocumentsFiles.Where(d => docIds.Contains(d.DocumentID));
            documents.Where(d => d.Document != null).ToList().ForEach(d =>
            {
                d.Document.Title = collection["Title"];
                d.Document.Creator = collection["Creator"];
                d.Document.Subject = collection["Subject"];
                d.Document.Description = collection["Description"];
                d.Document.Publisher = collection["Publisher"];
                d.Document.Contributor = collection["Contributor"];
                d.Document.Date = string.IsNullOrEmpty(collection["Year"]) ? (DateTime?)null : new DateTime(int.Parse(collection["Year"]), 1, 1);
                d.Document.Type = collection["Type"];
                d.Document.Format = collection["Format"];
                d.Document.Identifier = collection["Identifier"];
                d.Document.Resource = collection["Resource"];
                d.Document.Language = collection["Language"];
                d.Document.Relation = collection["Relation"];
                d.Document.Coverage = collection["Coverage"];
                d.Document.Right = collection["Right"];
                d.IsHasInfo = true;
            });

            documents.Where(d => d.Document == null).ToList().ForEach(d =>
            {
                d.Document = new Document
                {
                    Title = collection["Title"],
                    Creator = collection["Creator"],
                    Subject = collection["Subject"],
                    Description = collection["Description"],
                    Publisher = collection["Publisher"],
                    Contributor = collection["Contributor"],
                    Date = string.IsNullOrEmpty(collection["Year"]) ? (DateTime?)null : new DateTime(int.Parse(collection["Year"]), 1, 1),
                    Type = collection["Type"],
                    Format = collection["Format"],
                    Identifier = collection["Identifier"],
                    Resource = collection["Resource"],
                    Language = collection["Language"],
                    Relation = collection["Relation"],
                    Coverage = collection["Coverage"],
                    Right = collection["Right"]
                };
                d.IsHasInfo = true;
            });
            
            db.SaveChanges();

            //Logging
            RouteData routeData = ControllerContext.RouteData;
            string objIds = ""; string objNames = "";
            List<String> cateList = new List<String>();
            foreach (var document in documents)
            {
                objIds += document.DocumentID + ", ";
                objNames += document.FileName + ", ";
                cateList.Add(document.DocumentCategory.CategoryName);
            }
            ActionLog action = new ActionLog
            {
                user_id = ec.getUserId(),
                executed_user = ec.getUserName(),
                user_role = ec.getUserRole(ec.getUserId()),

                area_name = routeData.DataTokens["area"].ToString(),
                controller_name = routeData.Values["controller"].ToString(),
                action_name = routeData.Values["action"].ToString(),
                executed_time = DateTime.Now,

                description = "biên mục/thay đổi thông tin " + docIds.Length + " tài liệu",
                object_id = objIds.Substring(0, objIds.Length - 2),
                object_name = objNames.Substring(0, objNames.Length - 2),
                additional_info = String.Join(", ", cateList.Distinct().ToArray())
            };

            db.ActionLogs.AddObject(action);
            db.SaveChanges();

            if (keyword.Length > 0 || attribute.Length > 0 || value.Length > 0)
                return RedirectToAction("Filter", new { keyword = keyword, attribute = attribute, value = value, id_cate = id_cate });
            return RedirectToAction("Index", new { id_cate = id_cate, page = page });
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

        [Menu(Title = "Số đầu ấn phẩm", Path = "Thống kê", Order = 2)]
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

        [Menu(Title = "Khoản thu phí hàng năm", Path = "Thống kê", Order = 4)]
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

        [Menu(Title = "Lịch sử các thao tác", Path = "Thống kê")]
        public ActionResult LoggedAction()
        {
            var yesterday = DateTime.Now.AddDays(-1);
            var actions = (from a in db.ActionLogs
                          select new ActionModel
                          {
                              UserId = a.user_id,
                              UserName = a.executed_user,
                              UserRole = a.user_role,
                              Description = a.description,
                              ObjectId = a.object_id,
                              ObjectName = a.object_name,
                              ExtraInfo = a.additional_info,
                              ActionTime = a.executed_time,
                              Area = a.area_name,
                              Controller = a.controller_name,
                              Action = a.action_name
                          }).ToList();
            foreach (var action in actions)
            {
                if (action.ObjectName != null)
                {
                    action.ObjectName = action.ObjectName.Replace(", ", "\n");
                }
                if (action.ExtraInfo != null)
                {
                    action.ExtraInfo = action.ExtraInfo.Replace(", ", "\n");
                }
            }

            actions = actions.OrderByDescending(a => a.ActionTime).ToList();
            return View(actions);
        }

        //Modify this method if any new action need logging or change action name
        [HttpPost]
        public ActionResult LoggedAction(FormCollection form)
        {
            var actions = (from a in db.ActionLogs
                           select new ActionModel
                           {
                               UserId = a.user_id,
                               UserName = a.executed_user,
                               UserRole = a.user_role,
                               Description = a.description,
                               ObjectId = a.object_id,
                               ObjectName = a.object_name,
                               ExtraInfo = a.additional_info,
                               ActionTime = a.executed_time,
                               Area = a.area_name,
                               Controller = a.controller_name,
                               Action = a.action_name
                           }).ToList();

            foreach (var action in actions)
            {
                if (action.ObjectName != null)
                {
                    action.ObjectName = action.ObjectName.Replace(", ", "\n");
                }
                if (action.ExtraInfo != null)
                {
                    action.ExtraInfo = action.ExtraInfo.Replace(", ", "\n");
                }
            }

            try
            {
                if (!form["timeFrom"].Equals(string.Empty))
                {
                    string from = form["timeFrom"];
                    DateTime timeFrom = DateTime.ParseExact(from, "d/M/yyyy", CultureInfo.InvariantCulture);
                    actions = actions.Where(a => a.ActionTime != null && a.ActionTime >= timeFrom).ToList();
                    ViewBag.TimeFrom = from;
                }
                if (!form["timeTo"].Equals(string.Empty))
                {
                    string to = form["timeTo"];
                    DateTime timeTo = DateTime.ParseExact(to, "d/M/yyyy", CultureInfo.InvariantCulture);
                    actions = actions.Where(a => a.ActionTime != null && a.ActionTime <= timeTo).ToList();
                    ViewBag.TimeTo = to;
                }
            }
            catch (Exception)
            {
                TempData["alertMsg"] = "1";
            }
            if (!form["area"].Equals(string.Empty))
            {
                string cmd = form["area"];
                if (cmd.Equals("docAdmin"))
                {
                    actions = actions.Where(a => a.Area != null).Where(a => a.Area.Equals("DocumentAdmin") 
                        && (a.Controller.Equals("Document") || a.Controller.Equals("DocumentCategoryAdmin"))).ToList();
                }
                else if (cmd.Equals("userAdmin"))
                {
                    actions = actions.Where(a => a.Area != null).Where(a => a.Area.Equals("Core") 
                        && a.Controller.Equals("UserEx")
                        && (a.Action.Contains("User") || a.Action.Equals("Management") 
                        || a.Action.Contains("Role"))).ToList();
                }
                else if (cmd.Equals("manageRequest"))
                {
                    actions = actions.Where(a => a.Area != null).Where(a => a.Area.Equals("DocumentAdmin")
                        && a.Controller.Equals("DocumentOrder")).ToList();
                }
                else if (cmd.Equals("facultyAdmin"))
                {
                    actions = actions.Where(a => a.Area != null).Where(a => a.Area.Equals("Core") 
                        && a.Controller.Equals("UserEx")
                        && a.Action.Contains("Faculty")).ToList();
                }
                else if (cmd.Equals("viewBook"))
                {
                    actions = actions.Where(a => a.Area == null && a.Controller.Equals("Home")
                        && (a.Action.Equals("ViewBook") || a.Action.Equals("ViewMedia"))).ToList();
                }
                else if (cmd.Equals("changePass"))
                {
                    actions = actions.Where(a => a.Area == null && a.Controller.Equals("Home")
                        && a.Action.Equals("ChangePassword")).ToList();
                }
                else if (cmd.Equals("requestDoc"))
                {
                    actions = actions.Where(a => a.Area == null && a.Controller.Equals("Home")
                        && a.Action.Equals("OrderDocument")).ToList();
                }
                ViewBag.Area = cmd;
            }
            if (!form["user"].Equals(string.Empty))
            {
                string user = form["user"].ToLower();
                actions = actions.Where(a => a.UserName.ToLower().Contains(user)).ToList();
                ViewBag.User = user;
            }
            if (!form["object"].Equals(string.Empty))
            {
                string obj = form["object"].ToLower();
                actions = actions.Where(a => a.ObjectName.ToLower().Contains(obj)).ToList();
                ViewBag.Object = obj;
            }

            actions = actions.OrderByDescending(a => a.ActionTime).ToList();
            return View(actions);
        }
    }
}
