using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Security.Cryptography;
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
using QML.Web.UI.Areas.Core.Models;
using QML.Web.UI.Areas.Core.Controllers;
using QML.Web.UI.Models;
using QML.Web.UI.ViewModels;
using System.Collections;

namespace QML.Web.UI.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        HanuELibraryEntities db = new HanuELibraryEntities();
        EssentialController ec = new EssentialController();

        public ActionResult Index(string id, string mod)
        {
            var x = Request.QueryString;
            if (!string.IsNullOrWhiteSpace(id))
                return View(id);
            if (!string.IsNullOrWhiteSpace(mod))
                return View(mod);

            IEnumerable<DocumentEssential> model = ec.getLatestBooks();
            ViewBag.BookLatest = model;
            return View();
        }

        public ActionResult CategoryView()
        {
            return View();
        }
        /*
        public ActionResult Test()
        {
            IEnumerable < DocumentEssential > t= null;
            string keyword = "quicksort";
            IEnumerable<DocWeightModel> view_model= ec.searchByContent(t,keyword);    
            return View(view_model);
        }
         */

        public ActionResult ViewByCategory(int id)
        {
            IEnumerable<DocumentEssential> view_model = ec.getFromCategory1(id);
            ViewBag.CategoryName = db.DocumentCategories.FirstOrDefault(p => p.CategoryID == id).CategoryName;
            return View(view_model);
        }

        public ActionResult ViewByFormat(int id)
        {
            IEnumerable<DocumentEssential> view_model = ec.getFromFormat(id);
            ViewBag.Format = db.DocumentFormats.FirstOrDefault(p => p.DocumentFormatID == id).Name;
            return View(view_model);
        }

        public ActionResult ViewBook(int id)
        {
            if (ec.IsAuthenticated())
            {
                //Get document from id
                Document doc = db.Documents.FirstOrDefault(p => p.DocumentID == id);
                DocumentsFile file = db.DocumentsFiles.FirstOrDefault(p => p.DocumentID == id && p.IsDeleted == false && p.IsHasInfo == true && p.Status == "Hiển thị");
                //get viewHistory
                var userId = ec.getUserId();
                ViewHistory _viewHistory = db.ViewHistories.FirstOrDefault(p => p.BookId == id && p.UserId == userId);
                DocumentEssential model = new DocumentEssential
                {
                    DocumentID = doc.DocumentID,
                    BookFee = file.BookFee.Value,
                    CategoryID = file.CategoryID.Value,
                    Status = file.Status,
                    Creator = doc.Creator,
                    Subject = doc.Subject,
                    Description = doc.Description,
                    Format = doc.Format,
                    FormatID = file.FormatID.Value,
                    Identifier = doc.Identifier,
                    IsDeleted = file.IsDeleted.Value,
                    IsHasInfo = file.IsHasInfo.Value,
                    Language = doc.Language,
                    Publisher = doc.Publisher,
                    Thumbnail = file.Thumbnail,
                    Title = doc.Title,
                    ViewCount = file.ViewCount.Value,
                    FileSource = file.FileSource,
                    FileName = file.FileName
                };

                double fee = model.BookFee;
                string type = (fee > 0) ? "Fee" : "Free";

                //Minus the fee in user account, check if the action is completed
                bool x = ec.minusFee(model.DocumentID, fee);
                ec.DocumentStatisticUpdate(model);

                if (x == true)
                {
                    file.ViewCount = file.ViewCount + 1;
                    ViewHistory history = null;
                    //nếu như ng dùng đã đọc sách có phí rồi thì sẽ set fee= 0 , nếu k thì set fee như bt
                    if (_viewHistory != null)
                    {
                         history = new ViewHistory
                        {
                            BookId = id,
                            UserId = ec.getUserId(),
                            ViewDate = DateTime.Now,
                            DocumentPublisher = model.Publisher,
                            DocumentTitle = model.Title,
                            CatId = model.CategoryID,
                            Type = "Free",
                            Fee = 0,
                            Year = DateTime.Now.Year,
                            Month = DateTime.Now.Month

                        };
                    }
                    else {
                        history = new ViewHistory
                        {
                            BookId = id,
                            UserId = ec.getUserId(),
                            ViewDate = DateTime.Now,
                            DocumentPublisher = model.Publisher,
                            DocumentTitle = model.Title,
                            CatId = model.CategoryID,
                            Type = type,
                            Fee = fee,
                            Year = DateTime.Now.Year,
                            Month = DateTime.Now.Month

                        };
                    
                    }
                    db.ViewHistories.AddObject(history);
                    db.SaveChanges();
                    string doc_format = QML.Web.UI.Areas.DocumentAdmin.Helpers.DocumentsHelper.getFormatModel(file.FormatID.Value).FileFormat;
                    ViewBag.doc_format = doc_format;
                    if (doc_format == "pdf")
                    {
                        return View("ViewBook", model);
                    }

                    return View("ViewMedia", model);
                }
                else
                    return RedirectToAction("BalanceWarning");
            }
            else
            {
                return RedirectToAction("ViewBookDetail", new { id = id });
            }

        }
        public ActionResult ViewBookDetail(int id)
        {

            //Get document from id
            Document doc = db.Documents.FirstOrDefault(p => p.DocumentID == id);
            DocumentsFile file = db.DocumentsFiles.FirstOrDefault(p => p.DocumentID == id && p.IsDeleted == false && p.IsHasInfo == true && p.Status == "Hiển thị");
            DateTime? dateOrNull = doc.Date;
            DocumentEssential model = new DocumentEssential
            {
                DocumentID = doc.DocumentID,
                BookFee = file.BookFee.Value,
                CategoryID = file.CategoryID.Value,
                Status = file.Status,
                Creator = doc.Creator,
                Subject = doc.Subject,
                Description = doc.Description,
                Format = doc.Format,
                FormatID = file.FormatID.Value,
                Identifier = doc.Identifier,
                IsDeleted = file.IsDeleted.Value,
                IsHasInfo = file.IsHasInfo.Value,
                Language = doc.Language,
                Publisher = doc.Publisher,
                Thumbnail = file.Thumbnail,
                Title = doc.Title,
                ViewCount = file.ViewCount.Value,
                FileSource = file.FileSource,
                Date = doc.Date
            };
            if (ec.IsAuthenticated())
            {
                double fee = model.BookFee;
                string type = (fee > 0) ? "Fee" : "Free";

                file.ViewCount = file.ViewCount + 1;
                ViewHistory history = new ViewHistory
                {
                    BookId = id,
                    UserId = ec.getUserId(),
                    ViewDate = DateTime.Now,
                    DocumentPublisher = model.Publisher,
                    DocumentTitle = model.Title,
                    CatId = model.CategoryID,
                    Type = type,
                    Fee = fee,
                    Year = DateTime.Now.Year,
                    Month = DateTime.Now.Month

                };
                db.ViewHistories.AddObject(history);
                db.SaveChanges();

                string doc_format = QML.Web.UI.Areas.DocumentAdmin.Helpers.DocumentsHelper.getFormatModel(file.FormatID.Value).FileFormat;
                ViewBag.doc_format = doc_format;
                ViewBag.yearPublished = model.Date.Value.Year.ToString();
                return View("ViewBookDetail", model);
            }
            else
            {
                ViewBag.yearPublished = model.Date.Value.Year.ToString();
                return View("ViewBookDetail", model);
            }
        }


        public ActionResult LogOff()
        {
            AccountController controller = new AccountController();
            controller.LogOff();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logon()
        {
            return View();
        }

        //Cảnh báo khi chưa đăng nhập
        public ActionResult AuthWarning()
        {
            return View();
        }

        //Cảnh báo khi hết tiền trong tài khoản
        public ActionResult BalanceWarning()
        {
            return View();
        }

        /**
         * Search by document title
         **/
        public ActionResult SearchByTitle(FormCollection c)
        {
            string title = c.GetValue("search").AttemptedValue;
            IEnumerable<DocumentEssential> view_model = ec.SearchResult(title, 1, 0, 0, 0,null);
            SearchHistory history = new SearchHistory
            {
                UserId = ec.AuthManager.GetUser().UserId,
                Keyword = title,
                SearchDate = DateTime.Now
            };
            if (db.SearchHistories.FirstOrDefault(p => p.Keyword == title) == null)
            {
                db.SearchHistories.AddObject(history);
                db.SaveChanges();
            }
            IEnumerable<SearchHistory> histories = db.SearchHistories;
            foreach (var item in histories)
            {
                if (DateTime.Now.Date.AddDays(-20) == item.SearchDate.Date)
                {
                    db.SearchHistories.DeleteObject(item);
                }
            }
            db.SaveChanges();
            histories = db.SearchHistories.Take(5).OrderByDescending(p => p.SearchDate);
            ViewBag.History = histories;
            ViewBag.Search = title;
            return View(view_model.ToList());
        }

        //Search by title
        public ActionResult NormalSearch(string keyword)
        {
            IEnumerable<DocumentEssential> view_model = ec.SearchResult(keyword, 1, 0, 0, 1,null);
            IEnumerable<SearchHistory> history = db.SearchHistories;
            foreach (var item in history)
            {
                if (DateTime.Now.Date.AddDays(-20) == item.SearchDate.Date)
                {
                    db.SearchHistories.DeleteObject(item);
                }
            }
            db.SaveChanges();
            history = db.SearchHistories.Take(5).OrderByDescending(p => p.SearchDate);
            ViewBag.Keyword = keyword;
            ViewBag.History = history;
            return View(view_model);
        }

        //Advanced search
        public ActionResult Search()
        {
            IEnumerable<SearchHistory> history = db.SearchHistories;
            foreach (var item in history)
            {
                if (DateTime.Now.Date.AddDays(-20) == item.SearchDate.Date)
                {
                    db.SearchHistories.DeleteObject(item);
                }
            }
            db.SaveChanges();
            history = db.SearchHistories.Take(5).Distinct().OrderByDescending(p => p.SearchDate);
            ViewBag.History = history;
            return View();
        }

        [HttpPost]
        public ActionResult SearchResult(FormCollection collection)
        {
            IEnumerable<DocumentEssential> docEs = null;
            IEnumerable<DocumentEssential> docEs1 = null;
            
            List<DocumentEssential> list = null;
            //   ViewBag.FormAttributes = Keyword + ", " + Attribute + ", " + DocumentCategory + ", " + DocumentType + ", " + FeeType;
            if (collection.GetValue("filterLimited").AttemptedValue != null)
            {
                
                
                if (collection.GetValue("filterLimited").AttemptedValue == "filterLimited")
                {
                    docEs1 = getDocumentEssential(collection, null);     
                }
                    //lọc lần 1
                else if (collection.GetValue("filterLimited").AttemptedValue == "filterUnlimited")
                {
                    docEs1 = getDocumentEssential(collection,null);                    
                }
            }
            
            return View(docEs1.ToList());
        }

        /**
         * History
         * */
        public ActionResult History(long id)
        {
            IEnumerable<ViewHistory> histories = db.ViewHistories.Where(p => p.UserId == id);
            foreach (var item in histories)
            {
                if (DateTime.Now.Date.AddDays(-20) >= item.ViewDate.Date)
                {
                    db.ViewHistories.DeleteObject(item);
                }
            }
            db.SaveChanges();
            histories = db.ViewHistories.Where(p => p.UserId == id).Take(50).OrderByDescending(p => p.ViewDate);
            return View(histories.ToList());
        }

        // lấy danh sách lịch sử nạp tiền
        public ActionResult MoneyLog(long id)
        {
            IEnumerable<MoneyLog> histories = db.MoneyLogs.Where(p => p.userID == id);                        
            histories = db.MoneyLogs.Where(p => p.userID == id).Take(50).OrderByDescending(p => p.dateInput);
            return View(histories.ToList());
        }

        /**
         * Unable to resolve! 
         */
        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            model = new LogOnModel();
            returnUrl = "Home/Index";
            AccountController controller = new AccountController();
            controller.LogOn();
            return RedirectToAction("Index", "Home");
        }

        //Change password
        //Change password
        //modified
        public ActionResult ChangePassword(long id)
        {
            HanuElibrary2012 hanuElib = new HanuElibrary2012();
            hanuElib.UserAuth = db.auth_Users.Include(p => p.Profile).FirstOrDefault(p => p.UserId == id);
            IEnumerable<ViewHistory> histories = db.ViewHistories.Where(p => p.UserId == id);
            IEnumerable<MoneyLog> moneyLog = db.MoneyLogs.Where(p => p.userID == id).OrderByDescending(p=>p.dateInput).Take(50);
            foreach (var item in histories)
            {
                if (DateTime.Now.Date.AddDays(-20) >= item.ViewDate.Date)
                {
                    db.ViewHistories.DeleteObject(item);
                }
            }
            db.SaveChanges();
            histories = db.ViewHistories.Where(p => p.UserId == id).OrderByDescending(p => p.ViewDate).Take(50);
            hanuElib.ViewHistoryENum = histories;
            hanuElib._moneyLog = moneyLog;
            // auth_Users user = db.auth_Users.Include(p => p.Profile).FirstOrDefault(p => p.UserId == id);
            return View(hanuElib);
        }

        //modified
        [HttpPost]
        public ActionResult ChangePassword(HanuElibrary2012 user, String OldPassword)
        {
            //check old password
            var oldPass = EncodePassword(OldPassword);
            var recordCount = db.auth_Users.Where(p => p.Password == oldPass);
            if (recordCount.Count() > 0)
            {
                // update new password
                user.UserAuth.Password = EncodePassword(user.UserAuth.Password);
                db.auth_Users.Attach(user.UserAuth);
                db.Profiles.Attach(user.UserAuth.Profile);
                db.ObjectStateManager.ChangeObjectState(user.UserAuth, EntityState.Modified);
                db.ObjectStateManager.ChangeObjectState(user.UserAuth.Profile, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View();
        }

        //Handling document order
        public ActionResult OrderDocument()
        {
            return View();
        }

        [HttpPost]
        public ActionResult OrderDocument(FormCollection c)
        {
            long id = ec.AuthManager.GetUser().UserId;
            string email = "";
            if (db.auth_Users.FirstOrDefault(p => p.UserId == id).Profile != null)
            {
                email = db.auth_Users.FirstOrDefault(p => p.UserId == id).Profile.Email;
            }
            DocumentOrder order = new DocumentOrder
            {
                UserId = id,
                Status = false,
                OrderContent = c.GetValue("Content").AttemptedValue,
                CreatedDate = DateTime.Now,
                CreatedUser = id,
                LastModifiedDate = DateTime.Now,
                LastModifiedUser = id,
                UserEmail = email,
                DocumentName = c.GetValue("DocumentName").AttemptedValue,
                DocumentCategory = c.GetValue("DocumentCategory").AttemptedValue,
                DocumentCreator = c.GetValue("DocumentCreator").AttemptedValue,
                DocumentPublisher = c.GetValue("DocumentPublisher").AttemptedValue,
                DocumentType = c.GetValue("DocumentType").AttemptedValue,
                PublishedYear = c.GetValue("PublishedYear").AttemptedValue,
                Delivery = c.GetValue("Delivery").AttemptedValue

            };
            db.DocumentOrders.AddObject(order);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private string EncodePassword(string input)
        {
            string output = "";
            output = Crypto.SHA1(input);
            return output.ToUpper();
        }

        public ActionResult Contact()
        {
            return View();
        }

        //tai lieu da mua
        public ViewResult BoughtDocument(long id)
        {

            HanuElibrary2012 hanuElib = new HanuElibrary2012();

            IQueryable<long> docID = db.ViewHistories.OrderByDescending(p=>p.ViewDate).Where(p => p.UserId == id && p.Fee > 0).OrderByDescending(p=>p.ViewDate).Select(p => p.BookId).Distinct();            
            hanuElib.DocumentID = docID;            
            // auth_Users user = db.auth_Users.Include(p => p.Profile).FirstOrDefault(p => p.UserId == id);
            return View(hanuElib);
        }

        [HttpPost]
        public ViewResult BoughtDocument(FormCollection c)
        {
            HanuElibrary2012 hanuElib = new HanuElibrary2012();
            long categoryID = long.Parse(c.GetValue("DocumentCategory").AttemptedValue);
            long userID = long.Parse(c.GetValue("userID").AttemptedValue);
            IQueryable<long> docID = null;
            if (categoryID != 0)
            {
                docID = db.ViewHistories.OrderByDescending(p => p.ViewDate).Where(p => p.UserId == userID && p.Fee > 0 && p.CatId == categoryID).Select(p => p.BookId).Distinct();
            }
            else {
                docID = db.ViewHistories.OrderByDescending(p => p.ViewDate).Where(p => p.UserId == userID && p.Fee > 0).Select(p => p.BookId).Distinct();
            }
            hanuElib.DocumentID = docID;
            ViewBag.catId = categoryID;
            // auth_Users user = db.auth_Users.Include(p => p.Profile).FirstOrDefault(p => p.UserId == id);
            return View(hanuElib);
        }

        public ViewResult LoginMessage() {
            return View();
        }

        //get search result
        public IEnumerable<DocumentEssential> getDocumentEssential(FormCollection collection,IEnumerable<DocumentEssential>resultSet)
        {
            int DocumentType = 0;
            int DocumentCategory = 0;
            int Attribute = 0;
            int FeeType = 1;
            string Keyword = "";
            IEnumerable<DocumentEssential> Result = null;
            if (collection.GetValue("DocumentType").AttemptedValue != null)
                DocumentType = int.Parse(collection.GetValue("DocumentType").AttemptedValue);
            if (collection.GetValue("DocumentCategory").AttemptedValue != null)
                DocumentCategory = int.Parse(collection.GetValue("DocumentCategory").AttemptedValue);
            if (collection.GetValue("Attribute").AttemptedValue != null)
                Attribute = int.Parse(collection.GetValue("Attribute").AttemptedValue);
            if (collection.GetValue("Keyword").AttemptedValue != null)
                Keyword = collection.GetValue("Keyword").AttemptedValue;
            if (collection.GetValue("FeeType").AttemptedValue != null)
                FeeType = int.Parse(collection.GetValue("FeeType").AttemptedValue);
            if (resultSet == null)
            {
                Result = ec.SearchResult(Keyword, Attribute, DocumentCategory, DocumentType, FeeType, null);
            }
            else {
                Result = ec.SearchResult(Keyword, Attribute, DocumentCategory, DocumentType, FeeType, resultSet);
            }
             

            IEnumerable<SearchHistory> history = db.SearchHistories;
            foreach (var item in history)
            {
                if (DateTime.Now.Date.AddDays(-20) == item.SearchDate.Date)
                {
                    db.SearchHistories.DeleteObject(item);
                }
            }
            db.SaveChanges();
            history = db.SearchHistories.Take(5).Distinct().OrderByDescending(p => p.SearchDate);
            ViewBag.Keyword = Keyword;
            ViewBag.History = history;

            return Result;
        }
    }
}
