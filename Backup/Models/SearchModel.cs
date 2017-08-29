using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QML.Web.UI.Areas.DocumentAdmin.Models;
namespace QML.Web.UI.Models
{
    public class SearchModel
    {
        public int DocumentType { get; set; }
        public int DocumentCategory { get; set; }
        public int Attribute { get; set; }
        public string Keyword { get; set; }

        public IEnumerable<DocumentEssential> Result { get; set; }
    }
}