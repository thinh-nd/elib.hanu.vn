using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data;
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
using System.Web.Routing;

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

        /*
         * To enable print/download, remove the hidden element on its button in this page AND remove comment around line 2864-2874 in viewer.js
         */
        public ActionResult ViewBook(int id)
        {
            if (ec.IsAuthenticated())
            {
                //Get document from id
                Document doc = db.Documents.FirstOrDefault(p => p.DocumentID == id);
                DocumentsFile file = db.DocumentsFiles.FirstOrDefault(p => p.DocumentID == id && p.IsDeleted == false && p.IsHasInfo == true && p.Status == "Hiển thị");
                //get viewHistory
                var userId = ec.getUserId();
                var user = db.auth_Users.FirstOrDefault(u => u.UserId == userId);
                var oldBalance = user.Profile.Balance;
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
                bool IsBalanceEnough = ec.minusFee(model.DocumentID, fee);
                ec.DocumentStatisticUpdate(model);

                if (IsBalanceEnough == true)
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
                    string extraDescription = "";
                    if (history.Fee > 0) extraDescription = "(phí: " + history.Fee + ")";
                    db.ViewHistories.AddObject(history);
                    db.SaveChanges();

                    //Logging
                    RouteData routeData = ControllerContext.RouteData;
                    ActionLog action = new ActionLog
                    {
                        user_id = ec.getUserId(),
                        executed_user = ec.getUserName(),
                        user_role = ec.getUserRole(ec.getUserId()),

                        controller_name = routeData.Values["controller"].ToString(),
                        action_name = routeData.Values["action"].ToString(),
                        executed_time = DateTime.Now,

                        description = "xem tài liệu" + extraDescription,
                        object_id = id.ToString(),
                        object_name = doc.Title,
                        additional_info = "Tài khoản :" + oldBalance + " -> " + user.Profile.Balance
                    };

                    db.ActionLogs.AddObject(action);
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
                Year = doc.Date.Value.Year,
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
                ViewBag.yearPublished = model.Year.ToString();
                return View("ViewBookDetail", model);
            }
            else
            {
                ViewBag.yearPublished = model.Year.ToString();
                return View("ViewBookDetail", model);
            }
        }


        public ActionResult LogOff()
        {
            AccountController controller = new AccountController();
            controller.LogOff();
            return RedirectToAction("Index", "Home");
        }

        

        // Modified
        public ActionResult ChangePassword(long id)
        {
            HanuElibrary2012 hanuElib = new HanuElibrary2012();
            hanuElib.UserAuth = db.auth_Users.Include(p => p.Profile).FirstOrDefault(p => p.UserId == id);
            IEnumerable<ViewHistory> histories = db.ViewHistories.Where(p => p.UserId == id);
            IEnumerable<MoneyLog> moneyLog = db.MoneyLogs.Where(p => p.userID == id).OrderByDescending(p => p.dateInput).Take(50);
            IEnumerable<ViewHistory> mostView = db.ViewHistories.Where(p => p.UserId == id && p.Fee > 0).OrderByDescending(p => p.ViewDate).GroupBy(p => p.BookId).Select(p => p.FirstOrDefault());  
            foreach (var item in histories)
            {
                if (DateTime.Now.Date.AddDays(-90) > item.ViewDate.Date)
                {
                    db.ViewHistories.DeleteObject(item);
                }
            }
            db.SaveChanges();
            histories = db.ViewHistories.Where(p => p.UserId == id).OrderByDescending(p => p.ViewDate).Take(50);
            hanuElib.ViewHistoryENum = histories;
            hanuElib._moneyLog = moneyLog;
            hanuElib.MostView = mostView;

            return View(hanuElib);
        }

        // Modified
        [HttpPost]
        public ActionResult ChangePassword(HanuElibrary2012 user, String OldPassword)
        {
            //check old password
            var oldPass = EncodePassword(OldPassword);
            var recordCount = db.auth_Users.Where(p => p.Password == oldPass);
            if (recordCount.Count() > 0) //if OldPassword is correct
            {
                // update new password
                user.UserAuth.Password = EncodePassword(user.UserAuth.Password);
                db.auth_Users.Attach(user.UserAuth);
                db.Profiles.Attach(user.UserAuth.Profile);
                db.ObjectStateManager.ChangeObjectState(user.UserAuth, EntityState.Modified);
                db.ObjectStateManager.ChangeObjectState(user.UserAuth.Profile, EntityState.Modified);
                db.SaveChanges();

                //Logging
                RouteData routeData = ControllerContext.RouteData;
                ActionLog action = new ActionLog
                {
                    user_id = ec.getUserId(),
                    executed_user = ec.getUserName(),
                    user_role = ec.getUserRole(ec.getUserId()),

                    controller_name = routeData.Values["controller"].ToString(),
                    action_name = routeData.Values["action"].ToString(),
                    executed_time = DateTime.Now,

                    description = "thay đổi mật khẩu",
                    object_id = ec.getUserId().ToString(),
                    object_name = ec.getUserName()
                };

                db.ActionLogs.AddObject(action);
                db.SaveChanges();

                TempData["alertMsg"] = "0";
                return RedirectToAction("ChangePassword", new { id = user.UserAuth.UserId });
            }
            TempData["alertMsg"] = "1";
            return RedirectToAction("ChangePassword", new { id = user.UserAuth.UserId });
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

        //Search result for home page search box
        [HttpPost]
        public ActionResult NormalSearch(string keyword)
        {
            keyword = keyword.Trim();
            if (keyword.Length == 0) return RedirectToAction("Index");
            string authorKeyword = ec.GetAuthorName(keyword); //for author searching
            SearchModel Result = new SearchModel();
            Result.ResultSet = (from doc in db.Documents
                          join docFile in db.DocumentsFiles
                          on doc.DocumentID equals docFile.DocumentID
                          where doc.Title.ToLower().Contains(keyword.ToLower()) ||
                          doc.Creator.ToLower().Contains(keyword.ToLower()) ||
                          doc.Creator.Replace(",", "").Replace(".", "").ToLower().Contains(keyword.ToLower()) ||
                          doc.Creator.ToLower().Contains(authorKeyword) || 
                          doc.Subject.ToLower().Contains(keyword.ToLower()) ||
                          doc.Description.ToLower().Contains(keyword.ToLower()) || 
                          doc.Publisher.ToLower().Contains(keyword.ToLower()) ||
                          doc.Contributor.ToLower().Contains(keyword.ToLower()) ||
                          doc.Contributor.ToLower().Contains(authorKeyword) ||
                          doc.Contributor.Replace(",", "").Replace(".", "").ToLower().Contains(keyword.ToLower()) ||
                          docFile.DocumentCategory.CategoryName.ToLower().Contains(keyword.ToLower())
                          orderby docFile.ViewCount descending
                          select new DocumentEssential
                          {
                              Title = doc.Title,
                              Contributor = doc.Contributor,
                              Coverage = doc.Coverage,
                              Description = doc.Description,
                              Format = doc.Format,
                              FormatID = docFile.FormatID.Value,
                              Identifier = doc.Identifier,
                              Language = doc.Language,
                              Relation = doc.Relation,
                              Resource = doc.Resource,
                              Subject = doc.Subject,
                              Right = doc.Right,
                              Type = doc.Type,
                              DocumentID = doc.DocumentID,
                              CategoryID = docFile.CategoryID.Value,
                              Status = docFile.Status,
                              BookFee = docFile.BookFee.Value,
                              Thumbnail = docFile.Thumbnail,
                              ViewCount = docFile.ViewCount.Value,
                              IsHasInfo = docFile.IsHasInfo.Value,
                              IsDeleted = docFile.IsDeleted.Value,
                              Publisher = doc.Publisher,
                              Creator = doc.Creator,
                              FileSource = docFile.FileSource,
                              Year = doc.Date.Value.Year,
                          });
            Result.Keyword = keyword;
            return View(Result);
        }

        //Advanced search
        public ActionResult Search()
        {
            return View();
        }

        // Return search result collected from search page
        // Search box 1 are mandatory, if left empty, return to search page
        // (note: display error should be better)
        [HttpPost]
        public ActionResult SearchResult(FormCollection data)
        {
            if (data["Keyword0"].Trim().Length > 0) //search box 1
            {
                SearchModel sModel = new SearchModel();
                int attribute; // search id by title, author,...
                int boolOpr; // search id by AND, OR, NOT operator
                int.TryParse(data["Attribute0"], out attribute);
                sModel.Keyword = data["Keyword0"];
                var result0 = ec.GetBasicSearchResult(data["Keyword0"], attribute);
                if (data["Keyword1"].Trim().Length > 0) 
                {
                    int.TryParse(data["Attribute1"], out attribute);
                    int.TryParse(data["Boolean1"], out boolOpr);
                    sModel.Keyword += (boolOpr == 0) ? " + " : (boolOpr == 1) ? " | " : " - ";
                    sModel.Keyword += data["Keyword1"];
                    
                    var result1 = ec.GetBasicSearchResult(data["Keyword1"], attribute);
                    result0 = ec.GetBooleanSearchResult(result0, result1, boolOpr);
                }
                if (data["Keyword2"].Trim().Length > 0)
                {
                    int.TryParse(data["Attribute2"], out attribute);
                    int.TryParse(data["Boolean2"], out boolOpr);
                    sModel.Keyword += (boolOpr == 0) ? " +" : (boolOpr == 1) ? " |" : " -";
                    sModel.Keyword += data["Keyword2"];

                    var result2 = ec.GetBasicSearchResult(data["Keyword2"], attribute);
                    result0 = ec.GetBooleanSearchResult(result0, result2, boolOpr);
                }
                if (data["Keyword3"].Trim().Length > 0)
                {
                    int.TryParse(data["Attribute3"], out attribute);
                    int.TryParse(data["Boolean3"], out boolOpr);
                    sModel.Keyword += (boolOpr == 0) ? " +" : (boolOpr == 1) ? " |" : " -";
                    sModel.Keyword += data["Keyword3"];

                    var result3 = ec.GetBasicSearchResult(data["Keyword3"], attribute);
                    result0 = ec.GetBooleanSearchResult(result0, result3, boolOpr);
                }

                // order search result 
                int order;
                int.TryParse(data["OrderBy"], out order);
                if (order == 1) result0 = result0.OrderByDescending(r => r.Year);
                else if (order == 2) result0 = result0.OrderBy(r => r.BookFee);
                else result0 = result0.OrderByDescending(r => r.ViewCount);

                // filter search result by its category
                int category;
                int.TryParse(data["DocumentCategory"], out category);
                if (category != 0) result0 = result0.Where(r => r.CategoryID == category);

                // filter search result by document type
                // modify this when updating document type in database
                if (!data["DocumentType"].Equals("All"))
                {
                    string type = data["DocumentType"];
                    if (type.Equals("Document"))
                    {
                        result0 = result0.Where(r => r.FormatID == 1);
                    }
                    if (type.Equals("Audio"))
                    {
                        result0 = result0.Where(r => r.FormatID == 3);
                    }
                    if (type.Equals("Video"))
                    {
                        result0 = result0.Where(r => r.FormatID == 2 || r.FormatID == 4);
                    }
                }

                //filter search result by document fee type
                int feeType;
                int.TryParse(data["FeeType"], out feeType);
                if (feeType == 1) result0 = result0.Where(r => r.BookFee == 0);
                else if (feeType == 2) result0 = result0.Where(r => r.BookFee > 0);

                sModel.ResultSet = result0;
                return View(sModel);
            }
            
            return RedirectToAction("Search");
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

            //Logging
            RouteData routeData = ControllerContext.RouteData;
            ActionLog action = new ActionLog
            {
                user_id = ec.getUserId(),
                executed_user = ec.getUserName(),
                user_role = ec.getUserRole(ec.getUserId()),

                controller_name = routeData.Values["controller"].ToString(),
                action_name = routeData.Values["action"].ToString(),
                executed_time = DateTime.Now,

                description = "yêu cầu tài liệu",
                object_id = order.OrderID.ToString(),
                object_name = order.OrderContent,
                additional_info = null
            };

            db.ActionLogs.AddObject(action);
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
    }
}
