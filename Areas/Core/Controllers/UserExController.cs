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
using System.Web.UI.WebControls;
using System.Web.UI;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Threading;
using System.Globalization;
using System.Text;
using System.Data.Objects;
using QML.Web.UI.Controllers;
using System.Web.Routing;
using System.Data;
using System.Data.Entity;
using QML.Web.UI.Filters;


namespace QML.Web.UI.Areas.Core.Controllers
{
    public class UserExController : SecuredController
    {
        private HanuELibraryEntities db = new HanuELibraryEntities();
        private EssentialController ec = new EssentialController();

        public ActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateUser(ExtendRegisterModel model)
        {
            if (ModelState.IsValid)
            {
                //check existing record in db for duplicate username
                var record = db.auth_Users.Where(p => p.Username == model.UserName || p.StudentID == model.StudentID);
                if (record.Count() > 0)
                {
                    ViewBag.Message = "failed";
                    return RedirectToAction("UsersList", "User");
                }
                else
                {
                    try
                    {
                        var form = Request.Form;
                        if (form["date"].ToString().Length > 0)
                        {
                            model.DueDate = DateTime.ParseExact(form["date"].ToString(), "d/M/yyyy", CultureInfo.InvariantCulture);
                        }
                    }
                    catch (Exception)
                    {
                        return View(model);
                    }

                    model.Password = EncodePassword(model.Password);
                    var host = Request.Url.Host;
                    var query = db.app_Applications.FirstOrDefault(p => p.DomainName == host);

                    auth_Users user = new auth_Users
                    {
                        Username = model.UserName,
                        Password = model.Password,
                        ApplicationId = query.ApplicationId,
                        CreatedDate = DateTime.Now,
                        UserGroup = model.UserGroup,
                        Faculty = model.Faculty,
                        StudentID = model.StudentID,
                        StudentClass = model.StudentClass,
                        ActualName = model.ActualName,
                        YearLearnt = model.YearLearnt,
                        DueDate = model.DueDate
                    };

                    user.ApplicationId = query.ApplicationId;
                    db.auth_Users.AddObject(user);

                    Profile new_profile = new Profile
                    {
                        UserId = user.UserId,
                        Balance = 0.0,
                        CreatedDate = DateTime.Now,
                        LastModifiedDate = DateTime.Now,
                        CreatedUser = AuthManager.GetUser().UserId,
                        LastModifiedUser = AuthManager.GetUser().UserId,
                        Status = true,
                        Email = ""
                    };
                    db.Profiles.AddObject(new_profile);
                    user.Profile = new_profile;
                    user.auth_Roles.Add(db.auth_Roles.Single(r => r.RoleId == 20));
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

                        description = "tạo mới người dùng",
                        object_id = user.UserId.ToString(),
                        object_name = user.Username
                    };
                    
                    db.ActionLogs.AddObject(action);
                    db.SaveChanges();

                    ViewBag.Message = "success";
                    return RedirectToAction("UsersList", "User", new { pageSize = 20 });
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

            if (model.DueDate != null)
            {
                ViewBag.DueDate = model.DueDate.Value.ToString("d/M/yyyy");
            }
            else
            {
                ViewBag.DueDate = "";
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult EditUser(long id, ExtendRegisterModel model)
        {
            try
            {
                var form = Request.Form;
                if (form["date"].ToString().Length > 0)
                {
                    model.DueDate = DateTime.ParseExact(form["date"].ToString(), "d/M/yyyy", CultureInfo.InvariantCulture);
                }
            }
            catch (Exception)
            {
                return RedirectToAction("EditUser", new { id = id });
            }
            auth_Users entity = db.auth_Users.FirstOrDefault(p => p.UserId == id);
            string extraInfo = "";
            if (entity != null)
            {
                if (entity.Username != model.UserName)
                {
                    extraInfo += entity.Username + " -> " + model.UserName + ", ";
                    entity.Username = model.UserName;
                }
                if (entity.Faculty != model.Faculty)
                {
                    extraInfo += entity.Faculty + " -> " + model.Faculty + ", ";
                    entity.Faculty = model.Faculty;
                }
                if (entity.StudentClass != model.StudentClass)
                {
                    extraInfo += entity.StudentClass + " -> " + model.StudentClass + ", ";
                    entity.StudentClass = model.StudentClass;
                }
                if (entity.ActualName != model.ActualName)
                {
                    extraInfo += entity.ActualName + " -> " + model.ActualName + ", ";
                    entity.ActualName = model.ActualName;
                }
                if (entity.DueDate != model.DueDate)
                {
                    String s1 = ""; String s2 = "";
                    if (entity.DueDate != null) s1 = entity.DueDate.Value.ToShortDateString();
                    if (model.DueDate != null) s2 = model.DueDate.Value.ToShortDateString();
                    extraInfo += s1 + " -> " + s2 + ", ";
                    entity.DueDate = model.DueDate;
                } 
                if (entity.UserGroup != model.UserGroup)
                {
                    extraInfo += ec.getUserGroupString(entity.UserGroup) + " -> " + ec.getUserGroupString(model.UserGroup) + ", ";
                    entity.UserGroup = model.UserGroup;
                }
                if (entity.YearLearnt != model.YearLearnt)
                {
                    extraInfo += entity.YearLearnt + " -> " + model.YearLearnt + ", ";
                    entity.YearLearnt = model.YearLearnt;
                }
            }
            if (extraInfo.Length > 1) { extraInfo = extraInfo.Substring(0, extraInfo.Length - 2); }
            bool changesMade = db.ObjectStateManager.
                   GetObjectStateEntries(EntityState.Modified).Any();
            db.SaveChanges();
            
            //Logging
            if (changesMade)
            {
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

                    description = "thay đổi thông tin người dùng",
                    object_id = entity.UserId.ToString(),
                    object_name = entity.Username,
                    additional_info = extraInfo
                };

                db.ActionLogs.AddObject(action);
                db.SaveChanges();
            }

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

        public ActionResult ImportUserData()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ImportUserData(HttpPostedFileBase excelFile)
        {
            if (excelFile != null && excelFile.ContentLength > 0)
            {
                try
                {
                    var fileName = Path.GetFileName(excelFile.FileName);
                    var path = Path.Combine(Server.MapPath("~/Resources"), fileName);
                    excelFile.SaveAs(path);
                    FileInfo file = new FileInfo(HttpContext.Server.MapPath("~/Resources/" + fileName));
                    using (var package = new ExcelPackage(file))
                    {
                        ExcelWorksheet ws = package.Workbook.Worksheets.First();
                        var start = ws.Dimension.Start;
                        var end = ws.Dimension.End;
                        if (end.Column < 7) TempData["alertMsg"] = "3";

                        var userNames = (from u in db.auth_Users
                                         select u.Username).ToList();
                        var facultyEnum = (from f in db.FacultyGroups
                                           select f.fgroup_name).ToList();

                        auth_Users user = null;
                        int count = 0; string newUser = "";
                        for (int row = 2; row <= end.Row; row++)
                        {
                            //column1
                            //if StudentId column is null, empty or already exist in db, skip
                            if (ws.Cells[row, 1].Value == null) continue;
                            if (ws.Cells[row, 1].Value.ToString() == string.Empty) continue;
                            string id = ws.Cells[row, 1].Value.ToString().Trim();
                            if (userNames.Where(i => i == id).Count() > 0) continue;
                            else
                            {
                                user = new auth_Users();
                                user.Username = id;
                                user.Password = EncodePassword(id);
                                user.StudentID = id;
                            }

                            //column2
                            if (ws.Cells[row, 2].Value == null) continue;
                            if (ws.Cells[row, 2].Value.ToString() == string.Empty) continue;
                            string actualName = ws.Cells[row, 2].Value.ToString().Trim();
                            user.ActualName = actualName;

                            //column3
                            string group = ws.Cells[row, 3].Value.ToString().Trim();
                            int groupId;
                            if (group.Equals("Cán bộ,giảng viên")) groupId = 1;
                            else if (group.Equals("Sinh viên chính quy")) groupId = 2;
                            else if (group.Equals("Sinh viên tại chức-từ xa")) groupId = 3;
                            else if (group.Equals("Học viên cao học-dự án")) groupId = 4;
                            else if (group.Equals("Đối tượng ngoài trường")) groupId = 5;
                            else groupId = 2;
                            user.UserGroup = groupId;

                            //column4
                            if (ws.Cells[row, 4].Value == null) continue;
                            if (ws.Cells[row, 4].Value.ToString() == string.Empty) continue;
                            string faculty = ws.Cells[row, 4].Value.ToString().Trim();
                            if (facultyEnum.Where(f => f == faculty).Count() == 1)
                                user.Faculty = faculty;

                            //column5
                            if (ws.Cells[row, 5].Value == null) continue;
                            if (ws.Cells[row, 5].Value.ToString() == string.Empty) continue;
                            string className = ws.Cells[row, 5].Value.ToString().Trim();
                            user.StudentClass = className;

                            //column6
                            if (ws.Cells[row, 6].Value == null) continue;
                            if (ws.Cells[row, 6].Value.ToString() == string.Empty) continue;
                            string date = ws.Cells[row, 6].Value.ToString().Trim();
                            if (date.Contains('/'))
                            {
                                try { user.DueDate = DateTime.ParseExact(date, "d/M/yyyy", null); }
                                catch (Exception) { user.DueDate = null; }
                            }
                            else
                            {
                                try { user.DueDate = DateTime.FromOADate(Double.Parse(date)); }
                                catch (Exception) { user.DueDate = null; }
                            }

                            //column7
                            if (ws.Cells[row, 7].Value == null) continue;
                            if (ws.Cells[row, 7].Value.ToString() == string.Empty) continue;
                            string year = ws.Cells[row, 7].Value.ToString().Trim();
                            user.YearLearnt = year.Substring(0, 4);

                            //Add user and user profile to db
                            user.CreatedDate = DateTime.Now;
                            var host = Request.Url.Host;
                            var query = db.app_Applications.FirstOrDefault(p => p.DomainName == host);
                            user.ApplicationId = query.ApplicationId;
                            db.auth_Users.AddObject(user);
                            user.auth_Roles.Add(db.auth_Roles.Single(r => r.RoleId == 20));

                            Profile new_profile = new Profile
                            {
                                UserId = user.UserId,
                                Balance = 0.0,
                                CreatedDate = DateTime.Now,
                                LastModifiedDate = DateTime.Now,
                                CreatedUser = AuthManager.GetUser().UserId,
                                LastModifiedUser = AuthManager.GetUser().UserId,
                                Status = true,
                                Email = ""
                            };
                            db.Profiles.AddObject(new_profile);
                            user.Profile = new_profile;
                            count++;
                            newUser += id + ", ";
                        }
                        db.SaveChanges();

                        //Logging
                        RouteData routeData = ControllerContext.RouteData;
                        if (newUser.Length > 1) newUser.Substring(0, newUser.Length - 2);
                        ActionLog action = new ActionLog
                        {
                            user_id = ec.getUserId(),
                            executed_user = ec.getUserName(),
                            user_role = ec.getUserRole(ec.getUserId()),

                            area_name = routeData.DataTokens["area"].ToString(),
                            controller_name = routeData.Values["controller"].ToString(),
                            action_name = routeData.Values["action"].ToString(),
                            executed_time = DateTime.Now,

                            description = "thêm " + count + " người dùng từ file excel",
                            object_id = null,
                            object_name = fileName,
                            additional_info = newUser
                        };

                        db.ActionLogs.AddObject(action);
                        db.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    TempData["alertMsg"] = "3";
                }
            }
            else
            {
                TempData["alertMsg"] = "3";
            }

            return RedirectToAction("UsersList", "User", new { pageSize = 20 });
        }

        [HttpPost]
        public ActionResult ExportUserData(string filteredUsers)
        {
            List<ExcelOutputModel> userList = null;
            if (filteredUsers == null)
            {
                userList = (from u in db.auth_Users
                            join g in db.UserGroups on u.UserGroup equals g.groupId
                            where u.UserGroup != null
                            select new ExcelOutputModel
                            {
                                StudentID = u.StudentID,
                                ActualName = u.ActualName,
                                UserGroup = g.group_name,
                                Faculty = u.Faculty,
                                Class = u.StudentClass,
                                DueDate = "",
                                StartedYear = u.YearLearnt,
                                _DueDate = u.DueDate,
                                _UserGroup = u.UserGroup
                            }).ToList();
            }
            else
            {
                var idList = filteredUsers.Split('.').Select(long.Parse);
                var users = db.auth_Users.Where(u => idList.Contains(u.UserId)).ToList();
                userList = (from u in users
                            join g in db.UserGroups on u.UserGroup equals g.groupId
                            where u.UserGroup != null
                            select new ExcelOutputModel
                            {
                                StudentID = u.StudentID,
                                ActualName = u.ActualName,
                                UserGroup = g.group_name,
                                Faculty = u.Faculty,
                                Class = u.StudentClass,
                                DueDate = "",
                                StartedYear = u.YearLearnt,
                                _DueDate = u.DueDate,
                                _UserGroup = u.UserGroup
                            }).ToList();
            }

            Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            foreach (var user in userList)
            {
                if (user._DueDate != null)
                {
                    user.DueDate = user._DueDate.Value.ToShortDateString();
                }
                if (user.StartedYear != null)
                {
                    int sYear = Int32.Parse(user.StartedYear);
                    user.StartedYear = sYear + "-" + (sYear + 4);
                }
                
            }

            var orderUserList = userList.OrderBy(g => g._UserGroup == 2 ? 1 : 2).ThenBy(g => g._UserGroup);

            FileInfo file = new FileInfo(Server.MapPath("~/Resources/Danh_sach_ban_doc.xlsx"));
            if (file.Exists) { file.Delete(); }
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Sheet1");
                ws.Cells["A1"].LoadFromCollection(orderUserList, true);
                ws.DeleteColumn(8, 2);

                //styling
                string[] headerText = { "Tên đăng nhập (số thẻ)", "Tên sinh viên", "Nhóm bạn đọc", "Khoa chuyên ngành", "Lớp", "Ngày hết hạn thẻ", "Khóa" };
                var start = ws.Dimension.Start;
                var end = ws.Dimension.End;

                for (int col = start.Column; col <= end.Column; col++)
                {
                    //deleted columns, modify this condition if more columns needed
                    if (col > 7) { break; } 

                    var headerCell = ws.Cells[1, col];
                    headerCell.Value = headerText[col - 1];
                    headerCell.Style.WrapText = true;
                    headerCell.Style.Font.Bold = true;
                    headerCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headerCell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    headerCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                ws.Cells.AutoFitColumns();
                package.SaveAs(file);
            }

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

                description = "xuất dữ liệu người dùng từ file excel",
                object_id = null,
                object_name = null,
                additional_info = null
            };

            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment; filename=" + file.Name);
            Response.TransmitFile(file.ToString());
            Response.Flush();
            Response.End();

            db.ActionLogs.AddObject(action);
            db.SaveChanges();

            return RedirectToAction("UsersList", "User", new { pageSize = 20 });
        }

        [HttpPost]
        public ActionResult Management(FormCollection collection)
        {
            TempData["Page"] = collection["page"];
            auth_Users user;
            string[] listID = collection["idValue"].Split(',');

            RouteData routeData = ControllerContext.RouteData;

            switch (collection["actionName"])
            {
                case "deletePermanent":
                    foreach (string item in listID)
                    {
                        int id_user = Int32.Parse(item);
                        user = db.auth_Users.Where(d => d.UserId == id_user).FirstOrDefault();
                        db.auth_Users.DeleteObject(user);

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

                            description = "xóa dữ liệu người dùng",
                            object_id = user.UserId.ToString(),
                            object_name = user.Username,
                            additional_info = null
                        };

                        db.ActionLogs.AddObject(action);
                        db.SaveChanges();
                    }

                    return RedirectToAction("UsersList", "User", new { pageSize = 20 });

                case "ChangeRole":
                    foreach (string item in listID)
                    {
                        int id_user = Int32.Parse(item);
                        long roleId = Int64.Parse(collection["rolesList"]);
                        user = db.auth_Users.Where(d => d.UserId == id_user).FirstOrDefault();
                        var role = db.auth_Roles.Single(r => r.RoleId == roleId);
                        user.auth_Roles.Attach(role);

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

                            description = "thay đổi chức vụ người dùng",
                            object_id = user.UserId.ToString(),
                            object_name = user.Username,
                            additional_info = role.RoleName
                        };

                        db.ActionLogs.AddObject(action);
                        db.SaveChanges();
                    }
                    TempData["alertMsg"] = "2";

                    return RedirectToAction("UsersList", "User", new { pageSize = 20 });

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
                                pF.Email = "";
                                db.Profiles.AddObject(pF);

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

                                    description = "nạp thêm tiền vào tài khoản",
                                    object_id = user.UserId.ToString(),
                                    object_name = user.Username,
                                    additional_info = collection["moneyAmount"].ToString()
                                };

                                db.ActionLogs.AddObject(action);

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

                                    description = "nạp thêm tiền vào tài khoản",
                                    object_id = user.UserId.ToString(),
                                    object_name = user.Username,
                                    additional_info = collection["moneyAmount"].ToString()
                                };

                                db.ActionLogs.AddObject(action);
                                db.SaveChanges();
                                TempData["alertMsg"] = "0";
                            }
                        } 
                    }
                    return Redirect(Request.UrlReferrer.ToString());

                default:
                    return RedirectToAction("UsersList", "User", new { pageSize = 20 });
                    
            }
        }

        public ActionResult Faculty()
        {
            var faculty = from f in db.FacultyGroups
                          select new FacultyModel 
                          {
                            FacultyId = f.fgroup_id,
                            FacultyName = f.fgroup_name
                          };
            return View(faculty);
        }

        public ActionResult CreateFaculty()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateFaculty(FormCollection form)
        {
            var faculty = new FacultyGroup { fgroup_name = form["newFaculty"] };
            db.FacultyGroups.AddObject(faculty);
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

                description = "thêm khoa chuyên ngành",
                object_id = faculty.fgroup_id.ToString(),
                object_name = faculty.fgroup_name,
                additional_info = null
            };

            db.ActionLogs.AddObject(action);
            db.SaveChanges();

            return RedirectToAction("UsersList", "User", new { pageSize = 20 });
        }

        public ActionResult EditFaculty(int id)
        {
            var faculty = (from f in db.FacultyGroups
                          where id == f.fgroup_id
                          select new FacultyModel
                          {
                              FacultyId = f.fgroup_id,
                              FacultyName = f.fgroup_name
                          }).FirstOrDefault();
            return View(faculty);
        }

        [HttpPost]
        public ActionResult EditFaculty(FormCollection form)
        {
            int id = Int32.Parse(form["Id"]);
            string oldName = form["oldName"];
            string newName = form["newName"];
            var faculty = (from f in db.FacultyGroups
                           where id == f.fgroup_id
                           select f).FirstOrDefault();
            faculty.fgroup_name = form["newName"];

            //Change every user having old Faculty name,
            //Because by db design, user faculty store in name, not id
            var users = from u in db.auth_Users
                        where u.Faculty == oldName
                        select u;
            
            foreach (var user in users)
            {
                user.Faculty = newName;
            }

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

                description = "thay đổi tên khoa chuyên ngành",
                object_id = faculty.fgroup_id.ToString(),
                object_name = null,
                additional_info = oldName + "->" + newName
            };

            db.ActionLogs.AddObject(action);
            db.SaveChanges();

            return RedirectToAction("UsersList", "User", new { pageSize = 20 });
        }

        public ActionResult DeleteFaculty(int id)
        {
            var faculty = (from f in db.FacultyGroups
                           where id == f.fgroup_id
                           select new FacultyModel
                           {
                               FacultyId = f.fgroup_id,
                               FacultyName = f.fgroup_name
                           }).FirstOrDefault();

            int userCount = (from u in db.auth_Users
                            where u.Faculty == faculty.FacultyName
                             select u).Count();
            ViewBag.UserCount = userCount;

            return View(faculty);
        }

        [HttpPost]
        public ActionResult DeleteFaculty(FormCollection form)
        {
            int id = Int32.Parse(form["Id"]);
            var faculty = (from f in db.FacultyGroups
                           where id == f.fgroup_id
                           select f).FirstOrDefault();
            db.FacultyGroups.DeleteObject(faculty);
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

                description = "xóa khoa chuyên ngành",
                object_id = faculty.fgroup_id.ToString(),
                object_name = faculty.fgroup_name,
                additional_info = null
            };

            db.ActionLogs.AddObject(action);
            db.SaveChanges();

            return RedirectToAction("UsersList", "User", new { pageSize = 20 });
        }

        public ActionResult FilterUser()
        {
            var host = Request.Url.Host;
            var query = db.app_Applications.FirstOrDefault(p => p.DomainName == host);
            IEnumerable<QML.Web.UI.auth_Users> user = db.auth_Users.Where(p => p.ApplicationId == query.ApplicationId).ToList();
            if (Session["model"] != null)
            {
                return View(Session["model"]);
            }
            return View(user);
        }

        [HttpPost]
        public ActionResult FilterUser(FormCollection form)
        {
            if (form["searchType"].Equals("advanced"))
            {
                IEnumerable<auth_Users> _userYearLearnt = getUserFiltered(form);
                Session["model"] = _userYearLearnt;
                return View(_userYearLearnt.ToList());
            }
            else if (form["searchType"].Equals("normal"))
            {
                string keyword = form["keyword"];
                ViewBag.State = "Kết quả lọc người dùng theo từ khóa: " + form["keyword"];
                var host = Request.Url.Host;
                var query = db.app_Applications.FirstOrDefault(p => p.DomainName == host);
                var user = db.auth_Users.Where(p => p.ApplicationId == query.ApplicationId);
                user = user.Where(u => u.StudentID.Contains(keyword) ||
                    u.Username.Contains(keyword) || 
                    (u.ActualName != null && u.ActualName.Contains(keyword)) ||
                    (u.StudentClass != null && u.StudentClass.Contains(keyword)) ||
                    (u.YearLearnt != null && u.YearLearnt.Equals(keyword)));
                return View(user.AsEnumerable());
            }
            
            //else
            return RedirectToAction("UsersList", "User", new { pageSize = 20 });
        }
      
        public IEnumerable<auth_Users> getUserFiltered(FormCollection form) {
            string keywordStudentid = form["keywordStudentid"];
            string keywordUsername = form["keywordUsername"];
            string keywordActualname = form["keywordActualname"];
            string keywordGroup = form["keywordGroup"];
            string keywordFaculty = form["keywordFaculty"];
            string keywordClass = form["keywordClass"];
            string keywordDueDate = form["keywordDueDate"];
            string keywordYear = form["keywordYearStudy"];
            string state = "Kết quả lọc người dùng theo: "
                + (!String.IsNullOrEmpty(keywordStudentid) ? "Mã sinh viên: " + keywordStudentid + ", " : "")
                + (!String.IsNullOrEmpty(keywordUsername) ? "Tên tài khoản: " + keywordUsername + ", " : "")
                + (!String.IsNullOrEmpty(keywordActualname) ? "Tên : " + keywordActualname + ", " : "")
                + (!String.IsNullOrEmpty(keywordGroup) ? "Nhóm người dùng: " + ec.getUserGroupString(int.Parse(keywordGroup)) + ", " : "")
                + (!String.IsNullOrEmpty(keywordFaculty) ? "Khoa: " + keywordFaculty + ", " : "")
                + (!String.IsNullOrEmpty(keywordClass) ? "Lớp: " + keywordClass + ", " : "")
                + (!String.IsNullOrEmpty(keywordDueDate) ? keywordDueDate.Equals("0") ? "Hết hạn sử dụng thẻ, " : "Còn hạn sử dụng thẻ, " : "")
                + (!String.IsNullOrEmpty(keywordYear) ? "Khóa: " + keywordYear + ", " : "");
            ViewBag.State = state.Substring(0, state.Length - 2);
            var host = Request.Url.Host;
            var query = db.app_Applications.FirstOrDefault(p => p.DomainName == host);
            IEnumerable<auth_Users> _user = db.auth_Users.Where(p => p.ApplicationId == query.ApplicationId);
            IEnumerable<auth_Users> _userStudentId = FilterByStudentId(keywordStudentid, _user);
            IEnumerable<auth_Users> _userUsername = FilterByUsername(keywordUsername, _userStudentId);
            IEnumerable<auth_Users> _userActualName = FilterByActualName(keywordActualname, _userUsername);
            IEnumerable<auth_Users> _userGroup = FilterByGroup(keywordGroup, _userActualName);
            IEnumerable<auth_Users> _userFaculty = FilterByFaculty(keywordFaculty, _userGroup);
            IEnumerable<auth_Users> _userClass = FilterByClass(keywordClass, _userFaculty);
            IEnumerable<auth_Users> _userDueDate = FilterByDueDate(keywordDueDate, _userClass);
            IEnumerable<auth_Users> _userYearLearnt = FilterByYearLearnt(keywordYear, _userDueDate);
            return _userYearLearnt;
        }

        private IEnumerable<auth_Users> FilterByStudentId(string studentId, IEnumerable<auth_Users> model)
        {
            IEnumerable<auth_Users> _user = model.Where(p => p.StudentID != null && p.StudentID.ToLower().Contains(studentId.ToLower()));
            if (_user != null)
            {
                return _user;
            }
            else
            {
                return null;
            }
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
                    IEnumerable<auth_Users> _user1 = model.Where(e => e.DueDate != null && DateTime.Compare(e.DueDate.Value, DateTime.Now) < 0);
                    return _user1;
                }
                //còn hạn
                else if (keyword == 1)
                {
                    IEnumerable<auth_Users> _user2 = model.Where(e => e.DueDate != null && DateTime.Compare(e.DueDate.Value, DateTime.Now) >= 0);
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

        public ActionResult ChangePasswordUser(long? id)
        {
            ViewBag.UserID = id;
            return View();
        }

        [HttpPost]
        public ActionResult ChangePasswordUser(FormCollection form)
        {
            if (ModelState.IsValid)
            {
                long? id = long.Parse(form["Id"]);
                string pwd = form["newPassword"].ToString();
                var user = db.auth_Users.FirstOrDefault(u => u.UserId == id);
                user.Password = EncodePassword(pwd);
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

                    description = "thay đổi mật khẩu người dùng",
                    object_id = user.UserId.ToString(),
                    object_name = user.Username,
                    additional_info = pwd
                };

                db.ActionLogs.AddObject(action);
                db.SaveChanges();

                return RedirectToAction("UsersList", "User", new { pageSize = 20 });
            }
            return View(form);
        }
    }
}


