using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
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

namespace QML.Web.UI.Areas.DocumentAdmin.Controllers
{ 
    public class TopDocumentController : SecuredController
    {
        private HanuELibraryEntities db = new HanuELibraryEntities();

        //
        // GET: /DocumentAdmin/TopDocument/
        [Menu(Title = "Tài liệu được đọc nhiều nhất", Path = "Thống kê", Order = 3)]
        public ViewResult Index(FormCollection c)
        {
            IEnumerable<DocumentEssential> model = (from df in db.DocumentsFiles
                                                    join doc in db.Documents on df.DocumentID equals doc.DocumentID
                                                    select new DocumentEssential
                                                    {
                                                        DocumentID = df.DocumentID,
                                                        BookFee = df.BookFee.Value,
                                                        CategoryID = df.CategoryID.Value,
                                                        Description = doc.Description,
                                                        FormatID = df.FormatID.Value,
                                                        IsDeleted = df.IsDeleted.Value,
                                                        Status = df.Status,
                                                        Subject = doc.Subject,
                                                        IsHasInfo = df.IsHasInfo.Value,
                                                        Language = doc.Language,
                                                        Thumbnail = df.Thumbnail,
                                                        Title = doc.Title,
                                                        ViewCount = df.ViewCount.Value,
                                                        Creator = doc.Creator,
                                                        Identifier = doc.Identifier,
                                                        Publisher = doc.Publisher,
                                                        Format = doc.Format,
                                                        FileSource = df.FileSource,
                                                        Contributor = doc.Contributor,
                                                        Coverage = doc.Coverage,
                                                        Relation = doc.Relation,
                                                        Resource = doc.Resource,
                                                        Right = doc.Right,
                                                        Type = doc.Type,
                                                        CategoryName = db.DocumentCategories.FirstOrDefault(p => p.CategoryID == df.CategoryID).CategoryName,
                                                    });
            if (c.GetValue("top") == null)
                return View(model.OrderByDescending(p => p.ViewCount).Take(10).ToList());
            else
            {
                int top = int.Parse(c.GetValue("top").AttemptedValue);
                return View(model.OrderByDescending(p => p.ViewCount).Take(top).ToList());
            } 
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}