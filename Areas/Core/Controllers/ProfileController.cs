using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QML.Web.UI;
using QML.Library.Base.Controllers;
using System.Data;
using System.Web.Routing;
using QML.Web.UI.Controllers;

namespace QML.Web.UI.Areas.Core.Controllers
{ 
    public class ProfileController : SecuredController
    {
        private HanuELibraryEntities db = new HanuELibraryEntities();
        private EssentialController ec = new EssentialController();

        //
        // GET: /DocumentAdmin/Profile/Edit/5
        public ActionResult Edit(long id)
        {
            if (db.Profiles.FirstOrDefault(p => p.UserId == id) == null)
            {
                Profile new_profile = new Profile
                {
                    UserId = id,
                    Balance = 0.0,
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    CreatedUser = AuthManager.GetUser().UserId,
                    LastModifiedUser = AuthManager.GetUser().UserId,
                    Status = true,
                    Email = ""
                };
                auth_Users user = db.auth_Users.FirstOrDefault(p => p.UserId == id);
                db.Profiles.AddObject(new_profile);
                user.Profile = new_profile;           
                db.SaveChanges();
                ViewBag.UserId = new SelectList(db.auth_Users, "UserId", "Username", new_profile.UserId);
                ViewBag.Username = QML.Web.UI.Helpers.FrontLayoutHelper.getUsername(id);
                return View(new_profile);
            }
            else
            {
                Profile profile = db.Profiles.FirstOrDefault(p => p.UserId == id);
                ViewBag.UserId = new SelectList(db.auth_Users, "UserId", "Username", profile.UserId);
                ViewBag.Username = QML.Web.UI.Helpers.FrontLayoutHelper.getUsername(id);
                return View(profile);
            }
        }

        //
        // POST: /DocumentAdmin/Profile/Edit/5
        [HttpPost]
        public ActionResult Edit(Profile profile)
        {            
             
            if (ModelState.IsValid)
            {                                              
                DocumentStatistic statistic = db.DocumentStatistics.FirstOrDefault(p => p.CateID == null && p.Type == "income" && p.Year == DateTime.Now.Year);
                //nếu như có record
                if (statistic != null)
                {
                    db.DocumentStatistics.Detach(statistic);
                    statistic.Value = statistic.Value + profile.Balance;
                    db.DocumentStatistics.Attach(statistic);
                    db.ObjectStateManager.ChangeObjectState(statistic, EntityState.Modified);
                }
                //nếu k có record thì tạo mới
                else {
                    DocumentStatistic stat = new DocumentStatistic();
                    stat.Type = "income";
                    stat.CateID = null;
                    stat.Value = profile.Balance;
                    stat.Year = DateTime.Now.Year;
                    db.DocumentStatistics.AddObject(stat);                    
                }                             
                profile.LastModifiedDate = DateTime.Now;
                profile.LastModifiedUser = AuthManager.GetUser().UserId;
                Profile profilesDB = db.Profiles.FirstOrDefault(p => p.UserId == profile.UserId);
                db.Profiles.Detach(profilesDB);
                //ghi lại quá trình nạp tiền 
                MoneyLog _moneyLog = new MoneyLog();
                _moneyLog.userID = profile.UserId;
                _moneyLog.feeLog = profile.Balance;
                _moneyLog.dateInput = DateTime.Now;
                _moneyLog.month = DateTime.Now.Month;
                _moneyLog.year = DateTime.Now.Year;
                //cộng tiền nhập
                profile.Balance += profilesDB.Balance;
                db.Profiles.Attach(profile);                                
                db.ObjectStateManager.ChangeObjectState(profile, EntityState.Modified);
                db.MoneyLogs.AddObject(_moneyLog);
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

                    description = "nạp thêm tiền vào tài khoản",
                    object_id = profile.auth_Users.UserId.ToString(),
                    object_name = profile.auth_Users.Username,
                    additional_info = _moneyLog.feeLog.ToString()
                };

                db.ActionLogs.AddObject(action);
                db.SaveChanges();

                return RedirectToAction("UsersList", "User");
            }
            ViewBag.UserId = new SelectList(db.auth_Users, "UserId", "Username", profile.UserId);
            return View(profile);
        }

        public ActionResult DeleteUser(long id)
        {
            auth_Users user = db.auth_Users.FirstOrDefault(p => p.UserId == id);
            db.auth_Users.DeleteObject(user);
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

                description = "xóa dữ liệu người dùng",
                object_id = user.UserId.ToString(),
                object_name = user.Username,
                additional_info = null
            };

            db.ActionLogs.AddObject(action);
            db.SaveChanges();

            return RedirectToAction("UsersList", "User");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}