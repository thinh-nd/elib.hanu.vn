using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QML.Web.UI.Areas.DocumentAdmin.Models;
using System.Web.Mvc;
using System.Diagnostics;

namespace QML.Web.UI.Areas.DocumentAdmin.Helpers
{
    public class DocumentCategoryTreeViewHelper
    {
        HanuELibraryEntities db = new HanuELibraryEntities();
        public String trees;
        public int number_doc;
        public List<int> list_id_relatives = new List<int>();
        
        public List<DocumentCategoriesModel> DocumentList;

        public DocumentCategoryTreeViewHelper()
        {
            DocumentList = new List<DocumentCategoriesModel>();
        }


        public void BuildHierarchicalData(long RootID, int CurrentLevel,int catId)
        {
            // Load all nodes with the current RootID
            List<DocumentCategory> tmp = null;                        
            tmp = db.DocumentCategories.Where(p => p.ParentID == RootID).ToList();           
            foreach (var child in tmp)
            {                
                    DocumentCategoriesModel item = db.DocumentCategories.Where(p => p.CategoryID == child.CategoryID).Select(p => new DocumentCategoriesModel
                    {
                        CategoryID = p.CategoryID,
                        CategoryName = p.CategoryName,
                        ParentID = p.ParentID,
                        Position = p.Position.Value
                    }).FirstOrDefault();

                    // Add level prefix
                    item.CategoryName = this.BuildLevelPrefix(CurrentLevel) + item.CategoryName;
                    this.DocumentList.Add(item);

                    // Check whether the current node has child or not
                    if (this.HasChild(Convert.ToInt32(RootID)))
                    {
                        CurrentLevel++;
                        // Foreach child. Repeat the recursion                    
                        this.BuildHierarchicalData(child.CategoryID, CurrentLevel, catId);
                        CurrentLevel--;
                    }                
            }
        }
        int countLoop = 0;
        //build hierachical for specific category
        public void BuildHierarchicalDataSpecific(long RootID, int CurrentLevel, int catId)
        {
            // Load all nodes with the current RootID
            List<DocumentCategory> tmp = null;
            if (countLoop == 1)
            {
                tmp = db.DocumentCategories.Where(p => p.ParentID == RootID).ToList();
            }
            //chạy lần đầu tiên
            else {
                tmp = db.DocumentCategories.Where(p => p.CategoryID == catId).ToList();
            }
            foreach (var child in tmp)
            {
                DocumentCategoriesModel item = db.DocumentCategories.Where(p => p.CategoryID == child.CategoryID).Select(p => new DocumentCategoriesModel
                {
                    CategoryID = p.CategoryID,
                    CategoryName = p.CategoryName,
                    ParentID = p.ParentID,
                    Position = p.Position.Value
                }).FirstOrDefault();

                // Add level prefix
                item.CategoryName = this.BuildLevelPrefix(CurrentLevel) + item.CategoryName;
                this.DocumentList.Add(item);

                // Check whether the current node has child or not
                if (this.HasChild(Convert.ToInt32(RootID)))
                {
                    CurrentLevel++;
                    // Foreach child. Repeat the recursion                    
                    countLoop = 1;
                    this.BuildHierarchicalData(child.CategoryID, CurrentLevel, catId);
                    CurrentLevel--;
                }
            }
        }


        public void BuildRightCategory(long RootID, int CurrentLevel)
        {
            // Load all nodes with the current RootID
            List<DocumentCategory> tmp = db.DocumentCategories.Where(p => p.ParentID == RootID).ToList();
            foreach (var child in tmp)
            {
                // Add current node to list
                DocumentCategoriesModel item = db.DocumentCategories.Where(p => p.CategoryID == child.CategoryID).Select(p => new DocumentCategoriesModel
                {
                    CategoryID = p.CategoryID,
                    CategoryName = p.CategoryName,
                    ParentID = p.ParentID,
                    Position = p.Position.Value
                }).FirstOrDefault();

                // Add level prefix
                item.CategoryName = this.BuildRightCategoryPrefix(CurrentLevel) + item.CategoryName;

                this.DocumentList.Add(item);

                // Check whether the current node has child or not
                if (this.HasChild(Convert.ToInt32(RootID)))
                {
                    CurrentLevel++;
                    // Foreach child. Repeat the recursion                    
                    this.BuildRightCategory(child.CategoryID, CurrentLevel);
                    CurrentLevel--;
                }
            }
        }

        public void BuildHierarchicalList(long RootID, string CurrentPath)
        {
            // Load all nodes with the current RootID
            List<DocumentCategory> tmp = db.DocumentCategories.Where(p => p.ParentID == RootID).ToList();
            foreach (var child in tmp)
            {
                // Add current node to list
                DocumentCategoriesModel item = db.DocumentCategories.Where(p => p.CategoryID == child.CategoryID).Select(p => new DocumentCategoriesModel
                {
                    CategoryID = p.CategoryID,
                    CategoryName = p.CategoryName,
                    ParentID = p.ParentID,
                    Position = p.Position.Value
                }).FirstOrDefault();

                // Add level arrow
                if (RootID != 0)
                    item.CategoryName = CurrentPath + " > " + item.CategoryName;

                this.DocumentList.Add(item);

                // Check whether the current node has child or not
                if (this.HasChild(Convert.ToInt32(RootID)))
                {
                    // Foreach child. Repeat the recursion                    
                    this.BuildHierarchicalList(child.CategoryID, item.CategoryName);
                }
            }
        }

        public bool HasChild(int CategoryID)
        {
            DocumentCategory entity = db.DocumentCategories.FirstOrDefault(p => p.ParentID == CategoryID);
            if (entity != null) return true;
            else return false;
        }

        public string BuildLevelPrefix(int CurrentLevel)
        {
            string Result = "";
            if (CurrentLevel >= 2)
            {
                for (int i = 0; i < (CurrentLevel - 1); i++)
                {
                    if (i == CurrentLevel - 2)
                        Result += "|-- ";
                    else Result += "      ";
                }
            }    
            return Result;
        }

        public string BuildRightCategoryPrefix(int CurrentLevel)
        {
            string Result = "";
            if (CurrentLevel >= 2)
            {
                for (int i = 0; i < (CurrentLevel - 1); i++)
                {
                    if (i == CurrentLevel - 2)
                        Result += "|-- ";
                    else Result += "|&nbsp;&nbsp;&nbsp;";
                }
            }    
            return Result;
        }

        public DocumentCategoriesModel ConvertToModel(DocumentCategory entity)
        {
            DocumentCategoriesModel model = new DocumentCategoriesModel
            {
                CategoryID = entity.CategoryID,
                CategoryName = entity.CategoryName,
                Position = entity.Position.Value,
                ParentID = entity.ParentID
            };
            return model;
        }

        public void getNumberDocument(int CateID, int? year=0)
        {
                    int doc = 0;
                    if (year != 0)
                    {
                        doc = db.DocumentsFiles.Where(p => p.CategoryID == CateID).Where(p => p.IsDeleted == false && p.CreatedDate.Value.Year == year).Count();
                    }                    
                    else
                    {
                        doc = db.DocumentsFiles.Where(p => p.CategoryID == CateID).Where(p => p.IsDeleted == false).Count();
                    }

                    number_doc = number_doc + doc;
                    if (HasChild(CateID))
                    {
                        var tmp = db.DocumentCategories.Where(p => p.ParentID == CateID);
                        if (year != 0)
                        {
                            tmp.Where(c => c.CreatedDate.Value.Year == year);
                        }
                        var tmpList = tmp.ToList();
                        foreach (var child in tmpList)
                        {
                            getNumberDocument(child.CategoryID, year);
                        }
                    }

        }

        public void getIdRelatives(long CateID)
        {

            var tmp = db.DocumentCategories.Where(p => p.ParentID == CateID);
            var tmpList = tmp.ToList();
            //UrlHelper.GenerateUrl(
            //var url = new UrlHelper(DocumentController.ControllerContext.RequestContext);
            foreach (var child in tmpList)
            {
                if (HasChild(child.CategoryID))
                {
                    list_id_relatives.Add(child.CategoryID);
                    getIdRelatives(child.CategoryID);
                }
                else
                {
                    list_id_relatives.Add(child.CategoryID);
                }

            }
        }

        public void renderTreeView(long RootID, string url, int? year = 0, bool? isStatistic = false)
        {
            // Load all nodes with the current RootID
            //UrlHelper u = new UrlHelper(helper.ViewContext.RequestContext);
            var tmp = db.DocumentCategories.Where(p => p.ParentID == RootID);
            //if (year != 0)
            //{
            //    tmp.Where(d => d.CreatedDate.Value.Year == year);
            //}
            var tmpList = tmp.ToList();
            //UrlHelper.GenerateUrl(
            //var url = new UrlHelper(DocumentController.ControllerContext.RequestContext);
            foreach (var child in tmpList)
            {
                
                if (HasChild(child.CategoryID))
                {
                    trees += "<li class='closed'><span class='folder'><a href='" + url + "?id_cate=" + child.CategoryID + "'>" + child.CategoryName;
                    if (isStatistic == true)
                    {
                        trees += ": <span style='color:black;' >lưu trữ <b style='color:red;'>" + DocumentsHelper.getNumberDoc(child.CategoryID, year) + "</b> tài liệu</span>";
                    }
                    else
                    {
                        trees += " (" + DocumentsHelper.getNumberDoc(child.CategoryID, year) + ")"; 
                    }
                    // Open the UL 
                    trees += "</a></span><ul>";
                    renderTreeView(child.CategoryID, url, year, isStatistic);
                    trees += "</ul>";
                    trees += "</li>";
                }
                else
                {
                    trees += "<li><span class='file'><a href='" + url + "?id_cate=" + child.CategoryID + "'>" + child.CategoryName;
                    if (isStatistic == true)
                    {
                        trees += ": <span style='color:black;' >lưu trữ <b style='color:red;'>" + DocumentsHelper.getNumberDoc(child.CategoryID, year) + "</b> tài liệu</span>";
                    }
                    else
                    {
                        trees += " (" + DocumentsHelper.getNumberDoc(child.CategoryID, year) + ")";
                    }
                    trees += "</a></span></li>";
                }
            }
        }
    }    
}