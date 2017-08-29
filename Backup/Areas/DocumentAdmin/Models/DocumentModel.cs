using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using QML.Web.UI.Areas.DocumentAdmin.Helpers;
using QML.Web.UI.Areas.DocumentAdmin.Models;

namespace QML.Web.UI.Areas.DocumentAdmin.Models
{    
    public class DocumentEssential
    {
        public long DocumentID { get; set; }
        public int CategoryID { get; set; }
        public int FormatID { get; set; }
        public string Thumbnail { get; set; }
        public double BookFee { get; set; }
        public long ViewCount { get; set; }
        public string Status { get; set; }
        public bool IsHasInfo { get; set; }
        public bool IsDeleted { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Subject { get; set; }
        public string Language { get; set; }
        public string Creator { get; set; }
        public string Publisher { get; set; }
        public string Identifier { get; set; }
        public string Format { get; set; }
        public string Contributor { get; set; }
        public string Type { get; set; }
        public string Resource { get; set; }
        public string Relation { get; set; }
        public string Coverage { get; set; }
        public string Right { get; set; }
        public string FileSource { get; set; }
        public string FileName { get; set; }
        public string CategoryName { get; set; }
        public long ParentID { get; set; }
        [DataType(DataType.Date)]
        public DateTime ? Date { get; set; }        
    }

    public class DocumentCategoriesModel
    {
        public int CategoryID { get; set; }

        public long ParentID { get; set; }

        [Required(ErrorMessage = "Tên danh mục cần phải nhập")]
        public string CategoryName { get; set; }

        public long Position { get; set; }

        public DateTime CreatedDate { get; set; }

        public long CreatedUser { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public long LastModifiedUser { get; set; }
    }
    public class DocWordsModel
    {
        public long WordID { get; set; }
        public string Word { get; set; }
    }
    public class DocWeightModel
    {
        public long DocID { get; set; }
        public long WordID { get; set; }
        public int Weight { get; set; }
    }

    public class DocumentModel
    {
        public long DocumentID { get; set; }

        [Required(ErrorMessage = "Tên danh mục cần phải nhập")]
        public string Title { get; set; }

        public string Creator { get; set; }

        public string Subject { get; set; }

        public string Description { get; set; }

        public string Publisher { get; set; }

        public string Contributor { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public string Type { get; set; }

        public string Format { get; set; }

        public string Identifier { get; set; }

        public string Resource { get; set; }

        public string Language { get; set; }

        public string Relation { get; set; }

        public string Coverage { get; set; }

        public string Right { get; set; }

        //public long ViewCount { get; set; }

        public string Thumbnail { get; set; }

        //public bool Status { get; set; }

    }

    public class DocumentsFileModel
    {
        public long DocumentID { get; set; }

        [Required(ErrorMessage = "Tên tệp cần phải nhập")]
        public string FileName { get; set; }

        public int CategoryID { get; set; }

        public int FormatID { get; set; }

        public string FileSource { get; set; }

        public string Thumbnail { get; set; }

        public double BookFee { get; set; }

        public double Size { get; set; }

        public long ViewCount { get; set; }

        public string Status { get; set; }

        public bool IsHasInfo { get; set; }        

        public string CheckHasInfo { get; set; }        

        public string CheckHasAvatar { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; }

        public long CreatedUser { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public long LastModifiedUser { get; set; }

    }

    public class DocumentFormatModel
    {
        public int DocumentFormatID { get; set; }

        public string Name { get; set; }
  
        public string Description { get; set; }
        
        public string MIME { get; set; }
        
        public string FileFormat { get; set; }
       
        public bool Status { get; set; }

        public string Avatar { get; set; }
    }

    public class DocumentOrder
    {
        public long OrderID { get; set; }
        public long UserId { get; set; }
        public bool Status { get; set; }
        public string OrderContent { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<long> CreatedUser { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public Nullable<long> LastModifiedUser { get; set; }
    }

    public class DocumentSatisticModel
    {
        public double Value { get; set; }
        public int CateID { get; set; }
        public string Type { get; set; }
        public long Year { get; set; }
    }


    public class SystemStatisticModel
    {
        public int? CateId { get; set; }
        public int? Month { get; set; }
        public long Views { get; set; }
        public double Fee { get; set; }
        public string Type { get; set; }

    }

}