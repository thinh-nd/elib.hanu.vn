using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QML.Web.UI.Areas.Core.Models
{
    public class ExcelOutputModel
    {
        public string StudentID { get; set; }
        public string ActualName { get; set; }
        public string UserGroup { get; set; }
        public string Faculty { get; set; }
        public string Class { get; set; }
        public string DueDate { get; set; }
        public string StartedYear { get; set; }
        public DateTime? _DueDate { get; set; }
        public int? _UserGroup { get; set; }
    }

    public class FacultyModel
    {
        public int FacultyId { get; set; }
        public string FacultyName { get; set; }
    }
}