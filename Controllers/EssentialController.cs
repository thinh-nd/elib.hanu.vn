using System;
using System.Collections.Generic;
using System.Linq;
using QML.Library.Base.Controllers;
using QML.Web.UI.Areas.DocumentAdmin.Models;

namespace QML.Web.UI.Controllers
{
    public class EssentialController : SecuredController
    {
        //
        // GET: /Essential/
        HanuELibraryEntities db = new HanuELibraryEntities();

        public IEnumerable<DocumentEssential> getLatestBooks()
        {
            IEnumerable<DocumentEssential> model = (from df in db.DocumentsFiles
                                                    join doc in db.Documents on df.DocumentID equals doc.DocumentID
                                                    where df.Status == "Hiển thị"
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
                                                    });
            IEnumerable<DocumentEssential> view_model = null;
            if (IsPremium())
            {
                view_model = model.Where(p => p.IsHasInfo == true && p.IsDeleted == false).OrderByDescending(p => p.DocumentID).Take(78);
            }
            else
                view_model = model.Where(p => p.IsHasInfo == true && p.IsDeleted == false && p.BookFee == 0).OrderByDescending(p => p.DocumentID).Take(78);
            return view_model;
        }

        public IEnumerable<DocumentEssential> getFromCategory(long id)
        {
            IEnumerable<DocumentEssential> view_model = null;
            List<DocumentEssential> view_modelFinal = new List<DocumentEssential>();
            IEnumerable<DocumentEssential> model = (from df in db.DocumentsFiles
                                                    join doc in db.Documents on df.DocumentID equals doc.DocumentID
                                                    where df.Status == "Hiển thị"
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
                                                        Year = doc.Date.Value.Year,
                                                    });
            if (IsPremium())
            {
                view_model = model.Where(p => p.IsHasInfo == true && p.IsDeleted == false).OrderBy(p => p.Title);
                foreach (var item in view_model.ToList())
                {
                    if (QML.Web.UI.Helpers.FrontLayoutHelper.recursive(Convert.ToInt64(item.CategoryID), id))
                    {
                        view_modelFinal.Add(item);
                    }
                }
            }
            else
            {
                view_model = model.Where(p => p.IsHasInfo == true && p.IsDeleted == false && p.BookFee == 0).OrderBy(p => p.Title);
                foreach (var item in view_model.ToList())
                {
                    if (QML.Web.UI.Helpers.FrontLayoutHelper.recursive(item.CategoryID, id))
                    {
                        view_modelFinal.Add(item);
                    }
                }
            }

            return view_modelFinal.AsEnumerable();
        }

        //getItem from parent category
        public IEnumerable<DocumentEssential> getFromCategory1(long id)
        {
            IEnumerable<DocumentEssential> dfM = QML.Web.UI.Helpers.FrontLayoutHelper.getChildItem(id);
            return dfM;
        }

        public IEnumerable<DocumentEssential> getFromFormat(int id)
        {
            IEnumerable<DocumentEssential> view_model = null;
            IEnumerable<DocumentEssential> model = (from df in db.DocumentsFiles
                                                    join doc in db.Documents on df.DocumentID equals doc.DocumentID
                                                    where df.FormatID == id && df.Status == "Hiển thị"
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
                                                        Year = doc.Date.Value.Year,
                                                    });

            if (IsPremium())
            {
                view_model = model.Where(p => p.IsHasInfo == true && p.IsDeleted == false).OrderBy(p => p.Title);
            }
            else
                view_model = model.Where(p => p.IsHasInfo == true && p.IsDeleted == false && p.BookFee == 0).OrderBy(p => p.Title);
            return view_model;
        }

        //Minus the balance of the user when he/she reads book
        public bool minusFee(long DocumentId, double fee)
        {
            long id = AuthManager.GetUser().UserId;
            auth_Users user = db.auth_Users.FirstOrDefault(p => p.UserId == id);
            ViewHistory viewHistory = db.ViewHistories.FirstOrDefault(p => p.UserId == id && p.BookId == DocumentId);
            if (user.Profile != null)
            {
                //kiểm tra xem đã đọc tài liệu này hay chưa, nếu chưa thì trừ tiền, nếu đọc rồi thì k trừ
                if (viewHistory != null)
                {
                    return true;
                }
                else
                {
                    if (user.Profile.Balance >= fee)
                    {
                        user.Profile.Balance = user.Profile.Balance - fee;
                        FeeHistoryView newFee = new FeeHistoryView();
                        newFee.DocumentID = DocumentId;
                        newFee.Fee = fee;
                        newFee.UserId = id;
                        newFee.TimeRecorded = DateTime.Now;
                        newFee.month = DateTime.Now.Month;
                        newFee.year = DateTime.Now.Year;
                        db.FeeHistoryViews.AddObject(newFee);
                        db.SaveChanges();
                        return true;
                    }
                    else if (fee == 0.0)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }

        //Update view count for statistical details
        public void DocumentStatisticUpdate(DocumentEssential doc)
        {
            //document tổng
            DocumentStatistic statistic;
            if (doc.BookFee == 0)
            {
                statistic = db.DocumentStatistics.FirstOrDefault(p => p.CateID == doc.CategoryID
                    && p.Year == DateTime.Now.Year && p.Type == "FreeDocumentView");
            }
            else
            {
                statistic = db.DocumentStatistics.FirstOrDefault(p => p.CateID == doc.CategoryID
                    && p.Year == DateTime.Now.Year && p.Type == "FeeDocumentView");
            }
            if (statistic != null)
            {
                statistic.Value = statistic.Value + 1;
                db.SaveChanges();
            }
            //nếu k có thống kê thì tạo mới
            else
            {
                DocumentStatistic stat = new DocumentStatistic();
                stat.Value = 1;
                stat.CateID = doc.CategoryID;
                stat.Year = DateTime.Now.Year;
                if (doc.BookFee == 0)
                {
                    stat.Type = "FreeDocumentView";
                }
                else
                {
                    stat.Type = "FeeDocumentView";
                }
                db.DocumentStatistics.AddObject(stat);
                db.SaveChanges();
            }
        }

        //All about authentication

        //Check if user is of type premium (need to pay) or not
        public bool IsPremium()
        {
            if (AuthManager.GetUser() != null)
            {
                var roles = AuthManager.GetUser().Roles;
                foreach (var role in roles)
                {
                    if (role.RoleName == "Premium"||role.RoleName=="Student")
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        //get role of user

        public string getUserRole(long id)
        {

            if (AuthManager.GetUser(id) != null)
            {
                var roles = AuthManager.GetUser(id).Roles;
                foreach (var role in roles)
                {
                    return role.RoleName;
                }
            }
            return null;
        }

        public bool IsAuthenticated()
        {
            if (AuthManager.GetUser() != null)
            {
                return true;
            }
            else
                return false;
        }

        public long getUserId()
        {
            return AuthManager.GetUser().UserId;
        }

        public string getUserName()
        {
            return AuthManager.GetUser().Username;
        }

        private IEnumerable<DocumentEssential> getFromAllFields(IEnumerable<DocumentEssential> result, string keyword)
        {
            keyword = keyword.ToLower();
            result = result.Where(p => p.Title != null && p.Title.ToLower().Contains(keyword)
                || p.Creator != null && p.Creator.ToLower().Contains(keyword)
                || p.Subject != null && p.Subject.ToLower().Contains(keyword)
                || p.Description != null && p.Description.ToLower().Contains(keyword)
                || p.Publisher != null && p.Publisher.ToLower().Contains(keyword)
                || p.Contributor != null && p.Contributor.ToLower().Contains(keyword)
                || p.Type != null && p.Type.ToLower().Contains(keyword)
                || p.Format != null && p.Format.ToLower().Contains(keyword)
                || p.Identifier != null && p.Identifier.ToLower().Contains(keyword)
                || p.Resource != null && p.Resource.ToLower().Contains(keyword)
                || p.Language != null && p.Language.ToLower().Contains(keyword)
                || p.Relation != null && p.Relation.ToLower().Contains(keyword)
                || p.Coverage != null && p.Coverage.ToLower().Contains(keyword)
                || p.Right != null && p.Right.ToLower().Contains(keyword)
                );
            return result;
        }

        public bool IsLibrarian()
        {
            if (AuthManager.GetUser() != null)
            {
                var roles = AuthManager.GetUser().Roles;
                foreach (var role in roles)
                {
                    if (role.RoleName == "Librarian")
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        public bool IsThuThu()
        {
            if (AuthManager.GetUser() != null)
            {
                var roles = AuthManager.GetUser().Roles;
                foreach (var role in roles)
                {
                    if (role.RoleName == "Thủ thư")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public string getUserGroupString(int? id)
        {
            if (id == null) return null;
            var group = (from g in db.UserGroups
                         where g.groupId == id
                         select g).FirstOrDefault();
            return group.group_name;
        }

        public string getDocCategoryString(int? id)
        {
            if (id == null) return null;
            if (id == 0) return "ROOT";
            var category = (from c in db.DocumentCategories
                            where c.CategoryID == id
                            select c).FirstOrDefault();
            return category.CategoryName;
        }

        //return Dublin Core standard author name
        //i.e: "Brown, Erik" instead of "Erik Brown"
        public string GetAuthorName(string keyword)
        {
            string[] words = keyword.Trim().Split(' ');
            if (words.Length < 2)
            {
                return keyword;
            }
            else
            {
                string lastName = words[words.Length - 1] + ",";
                for (int i = 0; i < words.Length - 1; i++)
                {
                    words[i + 1] = words[i];
                }
                words[0] = lastName;

                return string.Join(" ", words); //author name
            }
        }

        // Return a basic search result such as
        // SELECT * from [Documents],[DocumentFiles]
        // WHERE [Document].[IsHasInfo] == true (đã biên mục)
        //
        // Then search the keyword in the specify field (attribute): title, author....
        // 
        // Additional filter such as boolean(AND/OR/NOT) and category can be add later when use
        public IQueryable<DocumentEssential> GetBasicSearchResult(string keyword, int attributeId)
        {
            keyword = keyword.Trim();
            var result = (from doc in db.Documents
                          join docFile in db.DocumentsFiles
                          on doc.DocumentID equals docFile.DocumentID
                          where docFile.IsHasInfo.Value == true
                          select new DocumentEssential
                          {
                              Title = doc.Title,
                              Contributor = doc.Contributor,
                              Coverage = doc.Coverage,
                              Description = doc.Description, 
                              Format = doc.Format,
                              FormatID = docFile.FormatID.Value,
                              Identifier = doc.Identifier,
                              Language = doc.Language,
                              Relation = doc.Relation,
                              Resource = doc.Resource,
                              Subject = doc.Subject,
                              Right = doc.Right,
                              Type = doc.Type,
                              DocumentID = doc.DocumentID,
                              CategoryID = docFile.CategoryID.Value,
                              CategoryName = docFile.DocumentCategory.CategoryName,
                              Status = docFile.Status,
                              BookFee = docFile.BookFee.Value,
                              Thumbnail = docFile.Thumbnail,
                              ViewCount = docFile.ViewCount.Value,
                              IsHasInfo = docFile.IsHasInfo.Value,
                              IsDeleted = docFile.IsDeleted.Value,
                              Publisher = doc.Publisher,
                              Creator = doc.Creator,
                              FileSource = docFile.FileSource,
                              Year = doc.Date.Value.Year,
                          });

            result = SearchAttributeFilter(result, keyword, attributeId);
            return result;
        }

        // Combine or subtract final result from another result using AND, OR, NOT chose by user
        public IQueryable<DocumentEssential> GetBooleanSearchResult(IQueryable<DocumentEssential> result0, IQueryable<DocumentEssential> result1, int boolOperator)
        {
            if (boolOperator == 0) result0 = result0.Intersect(result1); //AND
            else if (boolOperator == 1) result0 = result0.Union(result1); //OR
            else result0 = result0.Except(result1); //NOT

            return result0;
        }

        // Attribute seach filter for each case
        public IQueryable<DocumentEssential> SearchAttributeFilter(IQueryable<DocumentEssential> result, string keyword, int attributeId)
        {
            string authorKeyword = GetAuthorName(keyword); //for author search
            switch (attributeId)
            {
                case 0:
                    if (keyword[0] == '"' && keyword[keyword.Length - 1] == '"')
                    {
                        return result.Where(r => r.Title.ToLower().Equals(keyword.Substring(1, keyword.Length - 2).ToLower()) || 
                            r.Creator.ToLower().Equals(keyword.Substring(1, keyword.Length - 2).ToLower()) ||
                            r.Creator.ToLower().Equals(authorKeyword.Substring(1, authorKeyword.Length - 2).ToLower()) ||
                            r.Creator.Replace(",", "").Replace(".", "").ToLower().Equals(authorKeyword.Substring(1, authorKeyword.Length - 2).ToLower()) ||
                            r.Contributor.ToLower().Equals(keyword.Substring(1, keyword.Length - 2).ToLower()) ||
                            r.Contributor.ToLower().Equals(authorKeyword.Substring(1, authorKeyword.Length - 2).ToLower()) ||
                            r.Contributor.Replace(",", "").Replace(".", "").ToLower().Equals(authorKeyword.Substring(1, authorKeyword.Length - 2).ToLower()) ||
                            r.Subject.ToLower().Equals(keyword.Substring(1, keyword.Length - 2).ToLower()) || 
                            r.Language.ToLower().Equals(keyword.Substring(1, keyword.Length - 2).ToLower()) || 
                            r.Publisher.ToLower().Equals(keyword.Substring(1, keyword.Length - 2).ToLower()) || 
                            r.Description.ToLower().Equals(keyword.Substring(1, keyword.Length - 2).ToLower()) || 
                            r.CategoryName.ToLower().Equals(keyword.Substring(1, keyword.Length - 2).ToLower()));
                    }
                    else
                    {
                        return result.Where(r => r.Title.ToLower().Contains(keyword.ToLower()) ||
                            r.Creator.ToLower().Contains(keyword.ToLower()) ||
                            r.Creator.ToLower().Contains(authorKeyword.ToLower()) ||
                            r.Creator.Replace(",", "").Replace(".", "").ToLower().Contains(keyword.ToLower()) ||
                            r.Contributor.ToLower().Contains(keyword.ToLower()) ||
                            r.Contributor.ToLower().Contains(authorKeyword.ToLower()) ||
                            r.Contributor.Replace(",", "").Replace(".", "").ToLower().Contains(keyword.ToLower()) ||
                            r.Subject.ToLower().Contains(keyword.ToLower()) ||
                            r.Language.ToLower().Contains(keyword.ToLower()) ||
                            r.Publisher.ToLower().Contains(keyword.ToLower()) ||
                            r.Description.ToLower().Contains(keyword.ToLower()) ||
                            r.CategoryName.ToLower().Contains(keyword.ToLower()));
                    }
                case 1:
                    if (keyword[0] == '"' && keyword[keyword.Length - 1] == '"') //exact search
                    {
                        return result.Where(r => r.Title.ToLower().Equals(keyword.Substring(1, keyword.Length - 2).ToLower()));
                    }
                    else
                    {
                        return result.Where(r => r.Title.ToLower().Contains(keyword.ToLower()));
                    }
                case 2:
                    if (keyword[0] == '"' && keyword[keyword.Length - 1] == '"')
                    {
                        return result.Where(
                            r => r.Creator.ToLower().Equals(keyword.Substring(1, keyword.Length - 2).ToLower()) || 
                            r.Contributor.ToLower().Equals(keyword.Substring(1, keyword.Length - 2).ToLower()) || 
                            r.Creator.ToLower().Equals(authorKeyword.Substring(1, authorKeyword.Length - 2).ToLower()) ||
                            r.Contributor.ToLower().Equals(authorKeyword.Substring(1, authorKeyword.Length - 2).ToLower()));
                    }
                    else
                    {
                        return result.Where(
                            r => r.Creator.ToLower().Contains(keyword.ToLower()) || 
                            r.Contributor.ToLower().Contains(keyword.ToLower()) || 
                            r.Creator.ToLower().Contains(authorKeyword.ToLower()) || 
                            r.Contributor.ToLower().Contains(authorKeyword.ToLower()));
                    }
                case 3:
                    if (keyword[0] == '"' && keyword[keyword.Length - 1] == '"')
                    {
                        return result.Where(r => r.Subject.ToLower().Equals(keyword.Substring(1, keyword.Length - 2).ToLower()));
                    }
                    else
                    {
                        return result.Where(r => r.Subject.ToLower().Contains(keyword.ToLower()));
                    }
                case 4:
                    return result.Where(r => r.Year.ToString().Equals(keyword));
                case 5:
                    if (keyword[0] == '"' && keyword[keyword.Length - 1] == '"')
                    {
                        return result.Where(r => r.Language.ToLower().Equals(keyword.Substring(1, keyword.Length - 2).ToLower()));
                    }
                    else
                    {
                        return result.Where(r => r.Language.ToLower().Contains(keyword.ToLower()));
                    }    
                case 6:
                    if (keyword[0] == '"' && keyword[keyword.Length - 1] == '"')
                    {
                        return result.Where(r => r.Publisher.ToLower().Equals(keyword.Substring(1, keyword.Length - 2).ToLower()));
                    }
                    else
                    {
                        return result.Where(r => r.Publisher.ToLower().Contains(keyword.ToLower()));
                    }
                case 7:
                    if (keyword[0] == '"' && keyword[keyword.Length - 1] == '"')
                    {
                        return result.Where(r => r.Description.ToLower().Equals(keyword.Substring(1, keyword.Length - 2).ToLower()));
                    }
                    else
                    {
                        return result.Where(r => r.Description.ToLower().Contains(keyword.ToLower()));
                    }
                case 8:
                    if (keyword[0] == '"' && keyword[keyword.Length - 1] == '"')
                    {
                        return result.Where(r => r.CategoryName.ToLower().Equals(keyword.Substring(1, keyword.Length - 2).ToLower()));
                    }
                    else
                    {
                        return result.Where(r => r.CategoryName.ToLower().Contains(keyword.ToLower()));
                    }
                default:
                    return result;
            }
        }
    }
}
