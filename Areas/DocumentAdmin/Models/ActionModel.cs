using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QML.Web.UI.Areas.DocumentAdmin.Models
{
    public class ActionModel
    {
        public string Area { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public long? UserId { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public string Description { get; set; }
        public string ObjectId { get; set; }
        public string ObjectName { get; set; }
        public string ExtraInfo { get; set; }
        [DisplayFormat(DataFormatString = "0:d/M/yyyy H:mm:ss")]
        public DateTime? ActionTime { get; set; }
    }
}