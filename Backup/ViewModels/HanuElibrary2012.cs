using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QML.Web.UI.ViewModels
{
    public class HanuElibrary2012
    {
        public IEnumerable<QML.Web.UI.ViewHistory> ViewHistoryENum { get; set; }
        public QML.Web.UI.auth_Users UserAuth { get; set; }
        public IQueryable<long> DocumentID { get; set; }
        public IQueryable<string> title { get; set; }
        public IEnumerable<QML.Web.UI.Document> Document { get; set; }
        public IEnumerable<ViewHistory> test { get; set; }
        public IEnumerable<QML.Web.UI.MoneyLog> _moneyLog { get; set; }
    }
}