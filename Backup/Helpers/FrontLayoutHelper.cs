using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QML.Web.UI.Areas.DocumentAdmin.Helpers;
using QML.Web.UI.Areas.DocumentAdmin.Models;
using QML.Library.Base.Controllers;
using System.Globalization;

namespace QML.Web.UI.Helpers
{
    public class FrontLayoutHelper : Controller
    {
        //
        // GET: /FrontLayoutHelper/
        private static HanuELibraryEntities db = new HanuELibraryEntities();        
        public static List<int> list_id_relatives = new List<int>();
        public static IEnumerable<DocumentCategory> GetCategories()
        {
            HanuELibraryEntities db = new HanuELibraryEntities();
            IEnumerable<DocumentCategory> model = (IEnumerable<DocumentCategory>)db.DocumentCategories.Select(p => new DocumentCategory
            {
                CategoryName = p.CategoryName,
                CategoryID = p.CategoryID
            }
            );
            return model;
        }

        public static string BuildRightCategoryTree(UrlHelper url)
        {
            string Result = "";
            DocumentCategoryTreeViewHelper helper = new DocumentCategoryTreeViewHelper();
            helper.BuildRightCategory(0, 1);
            IEnumerable<DocumentCategoriesModel> list = (IEnumerable<DocumentCategoriesModel>)helper.DocumentList;
            helper.renderTreeView(0, "#", 0, true);
            foreach (var item in list)
            {
                
                    //<li><a href="@Url.Action("ViewByCategory", "Home", new { id = item.CategoryID })">@item.CategoryName</a></li>
                    Result += "<li>";
                    Result += "<a href='" + url.Action("ViewByCategory", "Home", new { id = item.CategoryID }) + "'>";
                    Result += item.CategoryName;                    
                    Result += "("+DocumentsHelper.getNumberDoc(item.CategoryID, 0)+")";                    
                    Result += "</a>";
                    Result += "</li>";                
            }
            return Result;
        }

        public static IEnumerable<DocumentEssential> GetLatestBooks(){
            IEnumerable<DocumentEssential> model = (from df in db.DocumentsFiles
                                                    join doc in db.Documents on df.DocumentID equals doc.DocumentID
                                                    where df.Status == "Hiển thị"
                                                    select new DocumentEssential
                                                    {
                                                        DocumentID = df.DocumentID,
                                                        BookFee = df.BookFee.Value,
                                                        FormatID = df.FormatID.Value,
                                                        IsDeleted = df.IsDeleted.Value,
                                                        Status = df.Status,
                                                        IsHasInfo = df.IsHasInfo.Value,
                                                        Thumbnail = df.Thumbnail,
                                                        Title = doc.Title,
                                                    });
            IEnumerable<DocumentEssential> view_model = null;
            view_model = model.Where(p => p.IsHasInfo == true && p.IsDeleted == false).OrderByDescending(p => p.DocumentID).Take(3);
            return view_model;
        }

        public static IEnumerable<DocumentEssential> GetMostViewBooks()
        {
            IEnumerable<DocumentEssential> model = (from df in db.DocumentsFiles
                                                    join doc in db.Documents on df.DocumentID equals doc.DocumentID
                                                    where df.Status == "Hiển thị"
                                                    select new DocumentEssential
                                                    {
                                                        DocumentID = df.DocumentID,
                                                        BookFee = df.BookFee.Value,
                                                        FormatID = df.FormatID.Value,
                                                        IsDeleted = df.IsDeleted.Value,
                                                        Status = df.Status,
                                                        IsHasInfo = df.IsHasInfo.Value,
                                                        Thumbnail = df.Thumbnail,
                                                        Title = doc.Title,
                                                        ViewCount = df.ViewCount.Value,
                                                    });
            IEnumerable<DocumentEssential> view_model = null;
            view_model = model.Where(p => p.IsHasInfo == true && p.IsDeleted == false).OrderByDescending(p => p.ViewCount).Take(3);
            return view_model;
        }

        public static IEnumerable<DocumentFormatModel> GetBookFormat()
        {
            IEnumerable<DocumentFormatModel> model = (from df in db.DocumentFormats
                                                      select new DocumentFormatModel
                                                      {
                                                          DocumentFormatID = df.DocumentFormatID,
                                                          Name = df.Name,
                                                      });

            return model;
        }

        public static int getBookCount(int id)
        {
            //nếu là root
            int root = db.DocumentCategories.Count(c=>c.CategoryID == id && c.ParentID == 0);
            if (root != null)
            {
                return 0;
            }
            else {
                int docFiles = db.DocumentsFiles.Count(c => c.CategoryID == id && c.Status == "Hiển thị");
                return docFiles;
            }            
        }

        //method để lấy trong view history xem đã đọc hay chưa
        public static Boolean getViewHistory(long id,long DocumentId) {            
            ViewHistory viewHistory = db.ViewHistories.FirstOrDefault(p => p.UserId == id && p.BookId == DocumentId);
            if (viewHistory != null) { return true; }
            return false;
        }
        //get title
        public static string getDocumentTitle(long? id){
            Document title = db.Documents.FirstOrDefault(p => p.DocumentID == id);
            if (title != null)
            {
                return title.Title ?? "";
            }
            return "NULL";
        }      

        //get creator
        public static string getDocumentCreator(long id)
        {
            Document doc = db.Documents.FirstOrDefault(p => p.DocumentID == id);
            if (doc != null)
            {
                return doc.Creator;
            } 
            return "";
        }
        //get publisher
        public static string getDocumentPublisher(long id)
        {
            Document doc  = db.Documents.FirstOrDefault(p => p.DocumentID == id);
            if (doc != null)
            {
                return doc.Publisher;
            }
            return "";
        }

        //get price
        public static double getDocumentPrice(long? id)
        {
            ViewHistory fee = db.ViewHistories.FirstOrDefault(p => p.BookId == id);
            if (fee != null)
            {
                return fee.Fee;
            }
            return -1;
        }
        //get publisher
        public static string getDocumentDate(long id)
        {
            Document _date = db.Documents.FirstOrDefault(p => p.DocumentID == id);
            if (_date != null)
            {
                DateTime dateString = _date.Date ?? DateTime.Now;
                string timeToString = dateString.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                return timeToString;
            }
            return "00/00/0000";
        }

        //get thumbnail
        public static string getDocumentThumbnail(long id)
        {
            DocumentsFile _thumb = db.DocumentsFiles.FirstOrDefault(p => p.DocumentID == id);
            if (_thumb != null)
            {
                return _thumb.Thumbnail;
            }
            
            return "";
        }
        //get user name
        public static string getUsername(long? userID)
        {
            auth_Users _username = db.auth_Users.FirstOrDefault(p => p.UserId == userID);
            if(_username!=null){
            return _username.Username;
            }
            return "";
        }

        public static string convertToMoneyFromDouble(double number) {
            return String.Format("{0:0,0.0}", number.ToString());
        }

        public static bool recursive(long id, long root)
        {
            if (id == root) return true;
                IEnumerable<DocumentCategory> docCat = db.DocumentCategories.Where(p => p.CategoryID == id);
                if (docCat != null)
                {
                    foreach (var item in docCat.ToList())
                    {
                        long parentID = item.ParentID;
                        if (parentID == root)
                        {
                            return true;
                        }
                        else if (parentID == 0) { return false; }
                        recursive(item.ParentID, root);
                    }
                
            }
            return false;

        }
        //check has childItem
        public static bool HasChild(int CategoryID)
        {
            DocumentCategory entity = db.DocumentCategories.FirstOrDefault(p => p.ParentID == CategoryID);
            if (entity != null) return true;
            else return false;
        }

        public static List<DocumentCategoriesModel> DocumentList = new List<DocumentCategoriesModel>();
        public static IEnumerable<QML.Web.UI.DocumentCategory> populateDropdown(int rootID,int currentLevel)
        {
            IEnumerable<QML.Web.UI.DocumentCategory> ls = db.DocumentCategories.Where(p => p.CreatedDate == null).ToList();     
            return ls;
        }

        //get all child item
        public static IEnumerable<DocumentEssential> getChildItem(long? id_cate = 0)
        {
            IEnumerable<DocumentEssential> document = null;
            DocumentCategoryTreeViewHelper tree = new DocumentCategoryTreeViewHelper();
            if (id_cate != 0)
            {
                tree.getIdRelatives(Int32.Parse(id_cate.ToString()));
                List<int> list = tree.list_id_relatives;
                list.Add(Int32.Parse(id_cate.ToString()));
                //list.Add(1); list.Add(3); list.Add(4);
                document = (from d in db.DocumentsFiles
                            join doc in db.Documents on d.DocumentID equals doc.DocumentID                            
                            where list.Contains(d.CategoryID ?? 0) && d.IsDeleted == false
                            orderby d.CreatedDate descending
                            select new DocumentEssential
                            {
                                DocumentID = d.DocumentID,
                                BookFee = d.BookFee.Value,
                                CategoryID = d.CategoryID.Value,
                                Description = doc.Description,
                                FormatID = d.FormatID.Value,
                                IsDeleted = d.IsDeleted.Value,
                                Status = d.Status,
                                Subject = doc.Subject,
                                IsHasInfo = d.IsHasInfo.Value,
                                Language = doc.Language,
                                Thumbnail = d.Thumbnail,
                                Title = doc.Title,
                                ViewCount = d.ViewCount.Value,
                                Creator = doc.Creator,
                                Identifier = doc.Identifier,
                                Publisher = doc.Publisher,
                                Format = doc.Format,
                                FileSource = d.FileSource,
                                Date = doc.Date
                            });
            }
            
            return document.OrderBy(p=>p.Title);
        }        

        //get action name
        public static IEnumerable<string> getAction(long controller) {
            IEnumerable<string> action = db.core_Actions.Where(p=>p.ControllerId==controller).Select(p=>p.Name).Distinct().ToList();
            return action;
        }

        //get action name
        public static IEnumerable<string> getController(string area)
        {
            IEnumerable<string> action = db.auth_RolePermissions.Where(p => p.Area == area).Select(p => p.Controller).Distinct().ToList();
            return action;
        }

        //get area name
        public static QML.Web.UI.core_Controllers getArea(long controller)
        {
            core_Controllers auth = db.core_Controllers.Where(p => p.ControllerId == controller).FirstOrDefault();
            return auth;
        }

        //get area name
        public static QML.Web.UI.core_Areas getArea1(long AreaId)
        {
            core_Areas auth = db.core_Areas.Where(p => p.AreaId == AreaId).FirstOrDefault();
            return auth;
        }

        //get action id list
        public static IEnumerable<core_Actions> getAction1(long cID)
        {
            IEnumerable<core_Actions> action = db.core_Actions.Where(p => p.ControllerId == cID).ToList();
            return action;
        }
        
        //check if role has rolePermission or not
        public static Boolean hasPermission(long roleID,string action,string controller, string area){
            auth_RolePermissions auth = db.auth_RolePermissions.Where(p=>p.RoleId==roleID && String.Compare(p.Action,action)==0 
                && String.Compare(p.Controller,controller)==0 && String.Compare(p.Area,area)==0).FirstOrDefault();
            if (auth != null) return true;
            return false;
        }

        //get book fee
        public static double? getBookFee(long id) {
            DocumentsFile df = db.DocumentsFiles.Where(p => p.DocumentID == id).FirstOrDefault();
            if (df != null) return df.BookFee;
            return 0;
        }

        // có thumbnail của ảnh hay chưa
        public static Boolean hasAvatar(long id){
            DocumentsFile df = db.DocumentsFiles.Where(p => p.DocumentID == id).FirstOrDefault();
            if (df != null){
                if (String.Compare(df.Thumbnail,"pdf.png")!=0 && String.Compare(df.Thumbnail,"avi.png")!=0 && 
                    String.Compare(df.Thumbnail,"mp3.png")!=0)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
        
        //get Number of pages
    }
}
