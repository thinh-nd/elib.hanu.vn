using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QML.Web.UI;
using QML.Library.Base.Controllers;

namespace QML.Web.UI.Areas.Core.Controllers
{ 
    public class ProfileController : SecuredController
    {
        private HanuELibraryEntities db = new HanuELibraryEntities();
        
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
                    Email = " "
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
                return RedirectToAction("UsersList", "User");
            }
            ViewBag.UserId = new SelectList(db.auth_Users, "UserId", "Username", profile.UserId);
            return View(profile);
        }

        public ActionResult DeleteUser(long id)
        {
            auth_Users user = db.auth_Users.FirstOrDefault(p => p.UserId == id);
            Profile profile = db.Profiles.FirstOrDefault(p => p.UserId == id);
            if (profile != null)
            {
                db.Profiles.DeleteObject(profile);
            }
            db.auth_Users.DeleteObject(user);
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