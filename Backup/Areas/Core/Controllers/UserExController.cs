using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QML.Web.UI.Areas.Core.Models;
using QML.Library.Attributes;
using QML.Library.Data;
using QML.Library.Base.Controllers;
using System.Web.Helpers;

namespace QML.Web.UI.Areas.Core.Controllers
{
    public class UserExController : SecuredController
    {
        private HanuELibraryEntities db = new HanuELibraryEntities();

        public ActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateUser(auth_Users user)
        {
            if (ModelState.IsValid)
            {
                //check existing record in db for duplicate username
                var record = db.auth_Users.Where(p => p.Username == user.Username);
                if (record.Count() > 0)
                {
                    ViewBag.Message = "failed";
                    return RedirectToAction("UsersList", "User");
                }
                else
                {
                    user.CreatedDate = DateTime.Now;
                    user.Password = EncodePassword(user.Password);
                    var host = Request.Url.Host;
                    var query = db.app_Applications.FirstOrDefault(p => p.DomainName == host);
                    user.ApplicationId = query.ApplicationId;
                    db.auth_Users.AddObject(user);
                    db.SaveChanges();
                    ViewBag.Message = "success";
                    return RedirectToAction("UsersList", "User", new { pageSize=20});
                }
            }
            return View();
        }

        public ActionResult EditUser(long id)
        {
            ExtendRegisterModel model = db.auth_Users.Where(d => d.UserId == id).Select(d => new ExtendRegisterModel
            {
                UserName = d.Username,
                Password = d.Password,
                Faculty = d.Faculty,
                StudentClass = d.StudentClass,
                StudentID = d.StudentID,
                ActualName = d.ActualName,
                DueDate = d.DueDate.Value,
                UserGroup = d.UserGroup,
                YearLearnt = d.YearLearnt,
            }).FirstOrDefault();
            return View(model);
        }

        [HttpPost]
        public ActionResult EditUser(long id, ExtendRegisterModel model)
        {
            auth_Users entity = db.auth_Users.FirstOrDefault(p => p.UserId == id);
            if (entity != null)
            {
                entity.Username = model.UserName;
                entity.Password = EncodePassword(model.Password);
                entity.Faculty = model.Faculty;
                entity.StudentClass = model.StudentClass;
                entity.ActualName = model.ActualName;
                entity.DueDate = model.DueDate;
                entity.UserGroup = model.UserGroup;
                entity.YearLearnt = model.YearLearnt;
            }
            db.SaveChanges();
            return RedirectToAction("UsersList", "User");
        }

        //encode password
        private string EncodePassword(string input)
        {
            string output = "";
            output = Crypto.SHA1(input);
            return output.ToUpper();
        }

        [HttpPost]
        public ActionResult AddRole(auth_Roles role)
        {
            //set all priviledge                                                
            db.auth_Roles.AddObject(role);
            var host = Request.Url.Host;
            var query = db.app_Applications.FirstOrDefault(p => p.DomainName == host);
            role.ApplicationId = query.ApplicationId;
            db.SaveChanges();
            return RedirectToAction("RolesList", "User");

        }

        public ActionResult UserRoles(long id)
        {
            return View();
        }

        public ActionResult Management(FormCollection collection)
        {
            auth_Users user;
            string[] listID = collection["idValue"].Split(',');            
            switch (collection["actionName"])
            {
                case "deletePermanent":
                    foreach (string item in listID)
                    {
                        int id_user = Int32.Parse(item);
                        user = db.auth_Users.Where(d => d.UserId == id_user).FirstOrDefault();
                        Profile profile = db.Profiles.FirstOrDefault(p => p.UserId == id_user);
                        if (profile != null)
                        {
                            db.Profiles.DeleteObject(profile);
                        }
                        IEnumerable<DocumentOrder> docOrder = db.DocumentOrders.Where(p => p.UserId == id_user);
                        foreach (var itemOrder in docOrder)
                        {
                            db.DocumentOrders.DeleteObject(itemOrder);
                        }
                        db.auth_Users.DeleteObject(user);
                    }
                    db.SaveChanges();
                    return RedirectToAction("UsersList", "User");


                case "chargeMoney":
                    foreach (string item in listID)
                    {
                        int id_user = Int32.Parse(item);
                        user = db.auth_Users.Where(d => d.UserId == id_user).FirstOrDefault();
                        //ghi lại stat


                        Profile profile = db.Profiles.FirstOrDefault(p => p.UserId == id_user);
                        if (profile == null)
                        {
                            string s = collection["moneyAmount"].ToString();
                            int number;
                            if (!int.TryParse(s, out number))
                            {
                                TempData["alertMsg"] = "1";
                            }
                            else
                            {
                                //ghi lại stat
                                DocumentStatistic statistic = db.DocumentStatistics.FirstOrDefault(p => p.CateID == null && p.Type == "income" && p.Year == DateTime.Now.Year);
                                //nếu như có record
                                if (statistic != null)
                                {
                                    statistic.Value = statistic.Value + int.Parse(collection["moneyAmount"].ToString());                                                                        
                                }
                                //nếu k có record thì tạo mới
                                else
                                {
                                    DocumentStatistic stat = new DocumentStatistic();
                                    stat.Type = "income";
                                    stat.CateID = null;
                                    stat.Value = int.Parse(collection["moneyAmount"].ToString());
                                    stat.Year = DateTime.Now.Year;
                                    db.DocumentStatistics.AddObject(stat);
                                }        

                                
                                MoneyLog _moneyLog = new MoneyLog();
                                _moneyLog.userID = id_user;
                                _moneyLog.feeLog = int.Parse(collection["moneyAmount"].ToString());
                                _moneyLog.dateInput = DateTime.Now;
                                _moneyLog.month = DateTime.Now.Month;
                                _moneyLog.year = DateTime.Now.Year;
                                db.MoneyLogs.AddObject(_moneyLog);


                                Profile pF = new Profile();
                                pF.UserId = id_user;
                                pF.Balance = int.Parse(collection["moneyAmount"].ToString());
                                pF.CreatedUser = AuthManager.GetUser().UserId;
                                pF.CreatedDate = DateTime.Now;
                                pF.LastModifiedDate = DateTime.Now;
                                pF.LastModifiedUser = AuthManager.GetUser().UserId;
                                pF.Status = true;
                                pF.Email = "empty@gmail.com";
                                db.Profiles.AddObject(pF);
                                db.SaveChanges();
                                TempData["alertMsg"] = "0";
                            }                          
                        }
                        else
                        {
                            string s1 = collection["moneyAmount"].ToString();
                            int number1;
                            if (!int.TryParse(s1, out number1))
                            {
                                TempData["alertMsg"] = "1";
                            }
                            else
                            {
                                DocumentStatistic statistic = db.DocumentStatistics.FirstOrDefault(p => p.CateID == null && p.Type == "income" && p.Year == DateTime.Now.Year);
                                //nếu như có record
                                if (statistic != null)
                                {
                                    statistic.Value = statistic.Value + int.Parse(collection["moneyAmount"].ToString());
                                }
                                //nếu k có record thì tạo mới
                                else
                                {
                                    DocumentStatistic stat = new DocumentStatistic();
                                    stat.Type = "income";
                                    stat.CateID = null;
                                    stat.Value = int.Parse(collection["moneyAmount"].ToString());
                                    stat.Year = DateTime.Now.Year;
                                    db.DocumentStatistics.AddObject(stat);
                                }        

                                MoneyLog _moneyLog = new MoneyLog();
                                _moneyLog.userID = id_user;
                                _moneyLog.feeLog = int.Parse(collection["moneyAmount"].ToString());
                                _moneyLog.dateInput = DateTime.Now;
                                _moneyLog.month = DateTime.Now.Month;
                                _moneyLog.year = DateTime.Now.Year;
                                db.MoneyLogs.AddObject(_moneyLog);

                                profile.Balance = profile.Balance + int.Parse(collection["moneyAmount"].ToString());                               
                                db.SaveChanges();
                                TempData["alertMsg"] = "0";
                            }
                        }

                        
                    }
                    return Redirect(Request.UrlReferrer.ToString());
                    

                default:
                    return RedirectToAction("UsersList", "User");
                    
            }
        }

        public ActionResult FilterUser()
        {
            var host = Request.Url.Host;
            var query = db.app_Applications.FirstOrDefault(p => p.DomainName == host);
            IEnumerable<QML.Web.UI.auth_Users> user = db.auth_Users.Where(p=>p.ApplicationId==query.ApplicationId).ToList();
            if (Session["model"] != null)
            {
                return View(Session["model"]);
            }
            return View(user);
        }

        [HttpPost]
        public ActionResult FilterUser(FormCollection form)
        {
            IEnumerable<auth_Users> _userYearLearnt = getUserFiltered(form);                                    
            Session["model"] = _userYearLearnt;
            return View(_userYearLearnt.ToList());            
        }
      
        public IEnumerable<auth_Users> getUserFiltered(FormCollection form) {
            string keywordUsername = form["keywordUsername"];
            string keywordActualname = form["keywordActualname"];
            string keywordGroup = form["keywordGroup"];
            string keywordFaculty = form["keywordFaculty"];
            string keywordClass = form["keywordClass"];
            string keywordDueDate = form["keywordueDate"];
            string yearLearnt = form["keywordYearStudy"];
            var host = Request.Url.Host;
            var query = db.app_Applications.FirstOrDefault(p => p.DomainName == host);
            IEnumerable<auth_Users> _user = db.auth_Users.Where(p => p.ApplicationId == query.ApplicationId);
            IEnumerable<auth_Users> _userUsername = FilterByUsername(keywordUsername, _user);
            IEnumerable<auth_Users> _userActualName = FilterByActualName(keywordActualname, _userUsername);
            IEnumerable<auth_Users> _userGroup = FilterByGroup(keywordGroup, _userActualName);
            IEnumerable<auth_Users> _userFaculty = FilterByFaculty(keywordFaculty, _userGroup);
            IEnumerable<auth_Users> _userClass = FilterByClass(keywordClass, _userFaculty);
            IEnumerable<auth_Users> _userDueDate = FilterByDueDate(keywordDueDate, _userClass);
            IEnumerable<auth_Users> _userYearLearnt = FilterByYearLearnt(yearLearnt, _userDueDate);
            return _userYearLearnt;
        }

        private IEnumerable<auth_Users> FilterByYearLearnt(string yearLearnt, IEnumerable<auth_Users> model)
        {
            if (!String.IsNullOrEmpty(yearLearnt))
            {
                IEnumerable<auth_Users> _user = model.Where(p => p.YearLearnt != null && p.YearLearnt.ToLower().Contains(yearLearnt.ToLower()));
                if (_user != null)
                {
                    return _user;
                }
                else
                {
                    return null;
                }
            }
            return model;
        }

        //helper classes:username
        public IEnumerable<auth_Users> FilterByUsername(string keywordUsername, IEnumerable<auth_Users> model)
        {
            if (!String.IsNullOrEmpty(keywordUsername))
            {
                IEnumerable<auth_Users> _user = model.Where(p => p.Username != null && p.Username.ToLower().Contains(keywordUsername.ToLower()));
                if (_user != null)
                {
                    return _user;
                }
                else
                {
                    return null;
                }
            }
            return model;
        }

        //actualname
        public IEnumerable<auth_Users> FilterByActualName(string keyword, IEnumerable<auth_Users> model)
        {
            if (!String.IsNullOrEmpty(keyword))
            {
                IEnumerable<auth_Users> _user = db.auth_Users.Where(p => p.ActualName != null && p.ActualName.ToLower().Contains(keyword.ToLower()));
                if (_user != null)
                {
                    return _user;
                }
                else
                {
                    return null;
                }
            }
            return model;
        }

        //duedate
        public IEnumerable<auth_Users> FilterByDueDate(string keywordString, IEnumerable<auth_Users> model)
        {
            if (!String.IsNullOrEmpty(keywordString))
            {
                //hết hạn
                int keyword = int.Parse(keywordString);
                if (keyword == 0)
                {
                    IEnumerable<auth_Users> _user1 = model.Where(e => DateTime.Compare(e.DueDate.Value, DateTime.Now) < 0);
                    return _user1;
                }
                //còn hạn
                else if (keyword == 1)
                {
                    IEnumerable<auth_Users> _user2 = model.Where(e => DateTime.Compare(e.DueDate.Value, DateTime.Now) >= 0);
                    return _user2;
                }
            }
            return model;
        }

        //group
        public IEnumerable<auth_Users> FilterByGroup(string keywordString, IEnumerable<auth_Users> model)
        {
            if (!String.IsNullOrEmpty(keywordString))
            {
                int keyword = int.Parse(keywordString);
                IEnumerable<auth_Users> _user = model.Where(p => p.UserGroup == keyword);
                if (_user != null)
                {
                    return _user;
                }
                else
                {
                    return null;
                }
            }
            return model;
        }

        //faculty
        public IEnumerable<auth_Users> FilterByFaculty(string keyword, IEnumerable<auth_Users> model)
        {
            if (!String.IsNullOrEmpty(keyword))
            {
                IEnumerable<auth_Users> _user = model.Where(p => p.Faculty != null && p.Faculty.ToLower().Contains(keyword.ToLower()));
                if (_user != null)
                {
                    return _user;
                }
                else
                {
                    return null;
                }
            }
            return model;
        }

        //Class
        public IEnumerable<auth_Users> FilterByClass(string keyword, IEnumerable<auth_Users> model)
        {
            if (!String.IsNullOrEmpty(keyword))
            {
                IEnumerable<auth_Users> _user = model.Where(p => p.StudentClass != null && p.StudentClass.ToLower().Contains(keyword.ToLower()));
                if (_user != null)
                {
                    return _user;
                }
                else
                {
                    return null;
                }
            }
            return model;
        }

        //phân quyền
        public ActionResult RolePermissions(int id)
        {
            IEnumerable<core_Controllers> controller = db.core_Controllers.ToList();
            ViewBag.roleId = id;
            return View(controller);
        }

        [HttpPost]
        public ActionResult RolePermissions(FormCollection form, int id)
        {
            //delete het role ung vói id            
            IEnumerable<auth_RolePermissions> ls = db.auth_RolePermissions.Where(p => p.RoleId == id);
            foreach (var item in ls)
            {
                db.auth_RolePermissions.DeleteObject(item);
            }
            db.SaveChanges();
            IEnumerable<core_Controllers> controller = db.core_Controllers.ToList();
            //them role            
            for (int i = 0; i < controller.Count(); i++)
            {
                IEnumerable<core_Actions> ls1 = QML.Web.UI.Helpers.FrontLayoutHelper.getAction1(controller.ElementAt(i).ControllerId);
                for (int j = 0; j < ls1.Count(); j++)
                {
                    //lấy area/controller/action     
                    if (form["ca" + i + "db" + j] == "1")
                    {
                        auth_RolePermissions auth = new auth_RolePermissions();
                        auth.RoleId = id;
                        auth.Action = form["Controllers[" + i + "].Actions[" + j + "].ActionName"];
                        auth.Controller = form["Controllers[" + i + "].ControllerName"];
                        auth.Area = form["Controllers[" + i + "].Area"];
                        db.auth_RolePermissions.AddObject(auth);
                    }
                    db.SaveChanges();
                }
            }
            return RedirectToAction("RolesList", "User");
        }
    }

}


