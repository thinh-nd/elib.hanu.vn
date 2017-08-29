using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QML.Web.UI.Areas.DocumentAdmin.Models;
using System.Web.Mvc;
using System.Diagnostics;
namespace QML.Web.UI.Areas.DocumentAdmin.Helpers
{
    public class DocumentsHelper
    {
        public static HanuELibraryEntities db = new HanuELibraryEntities();
        public static string GetCatgoryName(long CategoryID)
        {

            DocumentCategory entity = db.DocumentCategories.FirstOrDefault(p => p.CategoryID == CategoryID);
            if (entity != null) return entity.CategoryName;
            else return null;
        }

        public static int getNumberDoc(int cate_id, int? year = 0)
        {
            DocumentCategoryTreeViewHelper tree = new DocumentCategoryTreeViewHelper();
            tree.getNumberDocument(cate_id, year);
            return tree.number_doc;
        }

        public static string getCategoryName(int cate_id)
        {
            string cate_name = "Toàn bộ";
            if (cate_id != 0)
            {
                var cate = db.DocumentCategories.FirstOrDefault(c => c.CategoryID == cate_id);
                cate_name = cate.CategoryName;
            }
            return cate_name;

        }
        public static string getCategoryNameById(int? cate_id)
        {
            string cate_name = "Toàn bộ";
            if (cate_id != 0)
            {
                var cate = db.DocumentCategories.FirstOrDefault(c => c.CategoryID == cate_id);
                cate_name = cate.CategoryName;
            }
            return cate_name;

        }

        public static DocumentFormatModel getFormatModel(int id_format)
        {
            DocumentFormatModel model = db.DocumentFormats.Where(f => f.DocumentFormatID == id_format).Select(f => new DocumentFormatModel
            {
                DocumentFormatID = f.DocumentFormatID,
                Name = f.Name,
                MIME = f.MIME,
                FileFormat = f.FileFormat,
                Avatar = f.Avatar
            }).SingleOrDefault();
            return model;
        }

        public static List<DocumentFormat> GetDocumentTypeList()
        {
            List<DocumentFormat> list = db.DocumentFormats.ToList();
            return list;
        }

        public static void ExecuteCmd(string cmd, string args)
        {
            using (Process p = new Process())
            {

                p.StartInfo.FileName = cmd;

                p.StartInfo.Arguments = args;

                p.StartInfo.UseShellExecute = false;

                p.StartInfo.RedirectStandardOutput = false;

                p.StartInfo.CreateNoWindow = true;

                p.Start();

                p.PriorityClass = ProcessPriorityClass.Normal;

                p.WaitForExit();

            }

        }

        public static string EncodeMD5(string input)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
            bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            string password = s.ToString();
            return password;
        }
        public static SelectList GetYearListData()
        {

            List<String> l = new List<String>();
            Int32 currentYear = DateTime.Now.Year;
            l.Add(currentYear.ToString());
            for (int i = 1; i < 10; i++)
            {
                int tmp = currentYear - i;
                l.Add(tmp.ToString());
            }

            SelectList sl = new SelectList(l);
            return sl;
        }
        public static SelectList GetDocTypeListData()
        {
            List<String> l = new List<String>();
            l.Add("Fee");
            l.Add("Free");

            SelectList sl = new SelectList(l);
            return sl;
        }
        public static SelectList GetMonthListData()
        {
            List<String> l = new List<String>();
            for (int i = 1; i <= 12; i++)
            {
                l.Add(i.ToString());
            }

            SelectList sl = new SelectList(l);
            return sl;
        }       

        //lấy ds người dùng
        public static SelectList getUserList()
        {
            List<long?> ls = db.MoneyLogs.Select(p => p.userID).Distinct().ToList();
            List<string> _username = new List<string>();
            _username.Add("Tất cả người dùng");
            foreach(var item in ls){
                string username = QML.Web.UI.Helpers.FrontLayoutHelper.getUsername(item);
                _username.Add(username);
            }
            _username.Sort();
            SelectList sl = new SelectList(_username);
            return sl;
        }

        //filter month
        public static SelectList GetMonthListDataDefault()
        {
            List<String> l = new List<String>();
            l.Add("Tất cả");
            for (int i = 1; i <= 12; i++)
            {
                l.Add(i.ToString());
            }

            SelectList sl = new SelectList(l);
            return sl;
        }

        //filter month
        public static SelectList GetYearListDataDefault()
        {
            List<String> l = new List<String>();
            Int32 currentYear = DateTime.Now.Year;
            l.Add("Tất cả");
            l.Add(currentYear.ToString());
            for (int i = 1; i < 10; i++)
            {
                int tmp = currentYear - i;
                l.Add(tmp.ToString());
            }

            SelectList sl = new SelectList(l);
            return sl;
        }
        //get year of statistic
        public static SelectList GetYearView() {
            List<long?> ls = db.DocumentStatistics.Where(p=>p.Type=="FeeDocumentView"||p.Type=="FreeDocumentView").Select(p => p.Year).Distinct().ToList();
            List<string> _year = new List<string>();            
            foreach (var item in ls)
            {               
                _year.Add(item.ToString());
            }
            _year.Reverse();
            SelectList sl = new SelectList(_year);
            return sl;
        }
        //get income year
        public static SelectList GetYearViewIncome()
        {
            List<long?> ls = db.DocumentStatistics.Where(p=>p.Type=="income").Select(p => p.Year).Distinct().ToList();
            List<string> _year = new List<string>();
            foreach (var item in ls)
            {
                _year.Add(item.ToString());
            }
            _year.Reverse();
            SelectList sl = new SelectList(_year);
            return sl;
        }
    }

}