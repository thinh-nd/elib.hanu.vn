using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QML.Web.UI.Areas.DocumentAdmin.Models;
namespace QML.Web.UI.Models
{
    public class SearchModel
    {
        public string Keyword { get; set; }
        public IQueryable<DocumentEssential> ResultSet { get; set; }
    }
}