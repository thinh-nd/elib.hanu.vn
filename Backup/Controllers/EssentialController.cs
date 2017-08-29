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
        public IEnumerable<DocumentEssential> searchByContent(string keyword)
        {
            string[] keywordarr = keyword.ToLower().Split(' ');
            IEnumerable<DocWeightModel> listdocuments = (from dw in db.DocWeights
                                                         join dword in db.DocWords on dw.WordID equals dword.WordID
                                                         where keywordarr.Contains(dword.Word.ToLower())
                                                         group dw by dw.DocID into g
                                                         orderby g.Sum(dw => dw.Weight) descending
                                                         select new DocWeightModel
                                                         {
                                                             DocID = g.Key,
                                                             Weight = g.Sum(dw => dw.Weight)

                                                         });
            var resultList = new List<long>();
            if (listdocuments.Count() > 0)
            {
                foreach (var item in listdocuments)
                {
                    resultList.Add(item.DocID);
                }
            }

            IEnumerable<DocumentEssential> result = (from doc in db.Documents
                                                     join df in db.DocumentsFiles
                                                     on doc.DocumentID equals df.DocumentID
                                                     where resultList.Contains(df.DocumentID) && df.Status == "Hiển thị" && df.IsHasInfo == true && df.IsDeleted == false
                                                     select new DocumentEssential
                                                     {
                                                         Title = doc.Title,
                                                         Contributor = doc.Contributor,
                                                         Coverage = doc.Coverage,
                                                         Description = doc.Description,
                                                         Format = doc.Format,
                                                         FormatID = df.FormatID.Value,
                                                         Identifier = doc.Identifier,
                                                         Language = doc.Language,
                                                         Relation = doc.Relation,
                                                         Resource = doc.Resource,
                                                         Subject = doc.Subject,
                                                         Right = doc.Right,
                                                         Type = doc.Type,
                                                         DocumentID = doc.DocumentID,
                                                         CategoryID = df.CategoryID.Value,
                                                         Status = df.Status,
                                                         BookFee = df.BookFee.Value,
                                                         Thumbnail = df.Thumbnail,
                                                         ViewCount = df.ViewCount.Value,
                                                         IsHasInfo = df.IsHasInfo.Value,
                                                         IsDeleted = df.IsDeleted.Value,
                                                         Publisher = doc.Publisher,
                                                         Creator = doc.Creator,
                                                         FileSource = df.FileSource,
                                                         Date = doc.Date
                                                     });


            IEnumerable<DocumentEssential> view_model = null;

            if (IsPremium())
            {
                view_model = result.Where(p => p.IsHasInfo == true && p.IsDeleted == false).Take(9);
            }
            else
                view_model = result.Where(p => p.IsHasInfo == true && p.IsDeleted == false && p.BookFee == 0).Take(9);

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
                                                        Date = doc.Date
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
                                                        Date = doc.Date
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

        /**
         * All about searching
         * */
        public IEnumerable<DocumentEssential> SearchResult(string keyword, int attribute, int category, int format, 
            int FeeType, IEnumerable<DocumentEssential> resultDocumentEssential)
        {

            IEnumerable<DocumentEssential> result = null;          

            /*
             * @author hungnv
             * Check keyword for search history first 
             * if keyword exist and search date < 5days ago
             * Then load result of last search
             */

            if (!isKeywordExist(keyword) || !isKeywordAvailable(keyword, 30))
            {

                //keyword not exist || expired keyword =>find in db

                if (resultDocumentEssential == null)
                {
                    result = (from doc in db.Documents
                              join df in db.DocumentsFiles
                              on doc.DocumentID equals df.DocumentID
                              where df.Status == "Hiển thị" && df.IsHasInfo == true && df.IsDeleted == false
                              orderby df.ViewCount descending
                              select new DocumentEssential
                              {
                                  Title = doc.Title,
                                  Contributor = doc.Contributor,
                                  Coverage = doc.Coverage,
                                  Description = doc.Description,
                                  Format = doc.Format,
                                  FormatID = df.FormatID.Value,
                                  Identifier = doc.Identifier,
                                  Language = doc.Language,
                                  Relation = doc.Relation,
                                  Resource = doc.Resource,
                                  Subject = doc.Subject,
                                  Right = doc.Right,
                                  Type = doc.Type,
                                  DocumentID = doc.DocumentID,
                                  CategoryID = df.CategoryID.Value,
                                  Status = df.Status,
                                  BookFee = df.BookFee.Value,
                                  Thumbnail = df.Thumbnail,
                                  ViewCount = df.ViewCount.Value,
                                  IsHasInfo = df.IsHasInfo.Value,
                                  IsDeleted = df.IsDeleted.Value,
                                  Publisher = doc.Publisher,
                                  Creator = doc.Creator,
                                  FileSource = df.FileSource,
                                  Date = doc.Date
                              });
                }
                else {
                    result = (from doc in resultDocumentEssential
                              join df in db.DocumentsFiles
                              on doc.DocumentID equals df.DocumentID
                              where df.Status == "Hiển thị" && df.IsHasInfo == true && df.IsDeleted == false
                              orderby df.ViewCount descending
                              select new DocumentEssential
                              {
                                  Title = doc.Title,
                                  Contributor = doc.Contributor,
                                  Coverage = doc.Coverage,
                                  Description = doc.Description,
                                  Format = doc.Format,
                                  FormatID = df.FormatID.Value,
                                  Identifier = doc.Identifier,
                                  Language = doc.Language,
                                  Relation = doc.Relation,
                                  Resource = doc.Resource,
                                  Subject = doc.Subject,
                                  Right = doc.Right,
                                  Type = doc.Type,
                                  DocumentID = doc.DocumentID,
                                  CategoryID = df.CategoryID.Value,
                                  Status = df.Status,
                                  BookFee = df.BookFee.Value,
                                  Thumbnail = df.Thumbnail,
                                  ViewCount = df.ViewCount.Value,
                                  IsHasInfo = df.IsHasInfo.Value,
                                  IsDeleted = df.IsDeleted.Value,
                                  Publisher = doc.Publisher,
                                  Creator = doc.Creator,
                                  FileSource = df.FileSource,
                                  Date = doc.Date
                              });
                
                }
            }

            else
            {
                string searchHistoryResult = getResultFromSearchHistory(keyword);
                string[] arr = searchHistoryResult.Split(',');
                var resultList = new List<long>();
                for (var i = 0; i < arr.Length; i++)
                {
                    resultList.Add(long.Parse(arr[i]));
                }
                if (resultDocumentEssential == null)
                {
                    result = (from doc in db.Documents
                              join df in db.DocumentsFiles
                              on doc.DocumentID equals df.DocumentID
                              where resultList.Contains(df.DocumentID)
                              orderby df.ViewCount descending
                              select new DocumentEssential
                              {
                                  Title = doc.Title,
                                  Contributor = doc.Contributor,
                                  Coverage = doc.Coverage,
                                  Description = doc.Description,
                                  Format = doc.Format,
                                  FormatID = df.FormatID.Value,
                                  Identifier = doc.Identifier,
                                  Language = doc.Language,
                                  Relation = doc.Relation,
                                  Resource = doc.Resource,
                                  Subject = doc.Subject,
                                  Right = doc.Right,
                                  Type = doc.Type,
                                  DocumentID = doc.DocumentID,
                                  CategoryID = df.CategoryID.Value,
                                  Status = df.Status,
                                  BookFee = df.BookFee.Value,
                                  Thumbnail = df.Thumbnail,
                                  ViewCount = df.ViewCount.Value,
                                  IsHasInfo = df.IsHasInfo.Value,
                                  IsDeleted = df.IsDeleted.Value,
                                  Publisher = doc.Publisher,
                                  Creator = doc.Creator,
                                  FileSource = df.FileSource,
                                  Date = doc.Date
                              });
                }
                else {
                    result = (from doc in resultDocumentEssential
                              join df in db.DocumentsFiles
                              on doc.DocumentID equals df.DocumentID
                              where resultList.Contains(df.DocumentID)
                              orderby df.ViewCount descending
                              select new DocumentEssential
                              {
                                  Title = doc.Title,
                                  Contributor = doc.Contributor,
                                  Coverage = doc.Coverage,
                                  Description = doc.Description,
                                  Format = doc.Format,
                                  FormatID = df.FormatID.Value,
                                  Identifier = doc.Identifier,
                                  Language = doc.Language,
                                  Relation = doc.Relation,
                                  Resource = doc.Resource,
                                  Subject = doc.Subject,
                                  Right = doc.Right,
                                  Type = doc.Type,
                                  DocumentID = doc.DocumentID,
                                  CategoryID = df.CategoryID.Value,
                                  Status = df.Status,
                                  BookFee = df.BookFee.Value,
                                  Thumbnail = df.Thumbnail,
                                  ViewCount = df.ViewCount.Value,
                                  IsHasInfo = df.IsHasInfo.Value,
                                  IsDeleted = df.IsDeleted.Value,
                                  Publisher = doc.Publisher,
                                  Creator = doc.Creator,
                                  FileSource = df.FileSource,
                                  Date = doc.Date
                              });
                }
            }


            //find in documentFile 

            switch (attribute)
            {
                case 0:
                    List<DocumentEssential> all = new List<DocumentEssential>();                    
                    foreach (var item in searchByTitle(keyword, result))
                    {
                        all.Add(item);
                    }

                    foreach (var item in searchByAuthor(keyword, result))
                    {
                        all.Add(item);
                    }
                    foreach (var item in searchBySubject(keyword, result))
                    {
                        all.Add(item);
                    }
                    foreach (var item in searchByContributor(keyword, result))
                    {
                        all.Add(item);
                    }

                     all.AddRange(result.Where(p => p.Date.Value.Year.ToString().Contains(keyword)));

                     foreach (var item in searchByType(keyword, result))
                     {
                         all.Add(item);
                     }

                     foreach (var item in searchByFormat(keyword, result))
                     {
                         all.Add(item);
                     }

                     foreach (var item in searchByIdentifier(keyword, result))
                     {
                         all.Add(item);
                     }

                     foreach (var item in searchByRelation(keyword, result))
                     {
                         all.Add(item);
                     }

                     foreach (var item in searchByResource(keyword, result))
                     {
                         all.Add(item);
                     }

                     foreach (var item in searchByLanguage(keyword, result))
                     {
                         all.Add(item);
                     }

                     foreach (var item in searchByDescription(keyword, result))
                     {
                         all.Add(item);
                     }

                     foreach (var item in searchByRight(keyword, result))
                     {
                         all.Add(item);
                     }

                     foreach (var item in searchByCoverage(keyword, result))
                     {
                         all.Add(item);
                     }


                     foreach (var item in searchByPublisher(keyword, result))
                     {
                         all.Add(item);
                     }                                          

                     result = all.Distinct().AsEnumerable<DocumentEssential>();
                    break;

                case 1:
                    result = searchByTitle(keyword, result);
                    break;
                case 2:
                    result = searchByAuthor(keyword, result);
                    break;
                case 3:
                    result = searchBySubject(keyword, result);
                    break;
                case 4:
                    result = searchByContributor(keyword, result);
                    break;
                case 5:
                    result = result.Where(p => p.Date.Value.Year.ToString().Contains(keyword));
                    break;
                case 6:
                    result = searchByType(keyword, result);
                    break;
                case 7:
                    result = searchByFormat(keyword, result);
                    break;
                case 8:
                    result = searchByIdentifier(keyword, result);
                    break;
                case 9:
                    result = searchByRelation(keyword, result);
                    break;
                case 10:
                    result = searchByResource(keyword, result);
                    break;
                case 11:
                    result = searchByLanguage(keyword, result);
                    break;
                case 12:
                    result = searchByRight(keyword, result);
                    break;
                case 13:
                    result = searchByDescription(keyword, result);
                    break;
                case 14:
                    result = searchByPublisher(keyword, result);
                    break;
                case 15:
                    result = searchByCoverage(keyword, result);
                    break;               
            }

            if (format != 0)
            {
                result = result.Where(p => p.FormatID == format);
            }
            if (category != 0)
            {
                result = result.Where(p => p.CategoryID == category);
            }

            switch (FeeType)
            {
                case 1:
                    result = result.Where(p => p.BookFee <= 0.0);
                    break;
                case 2:
                    result = result.Where(p => p.BookFee > 0.0);
                    break;
                default:
                    break;

            }

            if (category == 0 && format == 0 && FeeType == 1)
            {
                return result;
            }

            //store result
            if (result.Count() >= 1)
            {
                saveSearchHistory(keyword, result);
            }
            return result;

        }
        /*@author: hungnv
         * Store search history for authenticated user
         */
        private void saveSearchHistory(string keyword, IEnumerable<DocumentEssential> result)
        {

            if (AuthManager.GetUser() != null && keyword != "")
            {
                string resultSet = getResultSet(result);

                SearchHistory history = new SearchHistory
                {
                    UserId = AuthManager.GetUser().UserId,
                    Keyword = keyword,
                    SearchDate = DateTime.Now,
                    ResultSet = resultSet
                };
                //new record add it
                if (!isKeywordExist(keyword))
                {
                    db.SearchHistories.AddObject(history);
                    db.SaveChanges();
                }
                //old record then update it with new result
                else
                {
                    updateExpiredKeyword(history);
                }

            }
        }
        /*@author: hungnv
            * check for keyword existence
            */
        public Boolean isKeywordExist(string keyword)
        {
            if (db.SearchHistories.FirstOrDefault(p => p.Keyword == keyword) != null)
                return true;
            else
                return false;
        }
        /*@author hungnv
         * Keyword is available only if SearchDate<numOfdays
         */
        public Boolean isKeywordAvailable(string keyword, int numOfDays = 6)
        {
            SearchHistory sh = db.SearchHistories.FirstOrDefault(p => p.Keyword == keyword);
            if (sh.SearchDate.Year != DateTime.Now.Year || sh.SearchDate.Month != DateTime.Now.Month)
                return false;
            //true when searchDate<numOfDays ago
            return (DateTime.Now.Day - sh.SearchDate.Day) < numOfDays;
        }
        /*@author hungnv
        * update existing keyword
        */
        private void updateExpiredKeyword(SearchHistory newSearchHistory)
        {
            if (AuthManager.GetUser() != null && newSearchHistory.Keyword != "")
            {
                SearchHistory sh = db.SearchHistories.FirstOrDefault(p => p.Keyword == newSearchHistory.Keyword);
                sh.SearchDate = newSearchHistory.SearchDate;
                sh.ResultSet = newSearchHistory.ResultSet;
                sh.UserId = newSearchHistory.UserId;
                db.SaveChanges();
            }

        }

        /*@author: hungnv
          * get resultSet contain Id of document by existency keyword
          */
        public string getResultFromSearchHistory(string keyword)
        {

            return db.SearchHistories.FirstOrDefault(p => p.Keyword == keyword).ResultSet;

        }
        /*@author: hungnv
          * get resultSet contain Id of document by Search Result
          */
        private string getResultSet(IEnumerable<DocumentEssential> result)
        {
            string resultSet = "";

            for (var i = 0; i < result.Count() - 1; i++)
            {
                resultSet += result.ElementAt(i).DocumentID + ",";
            }

            resultSet += result.ElementAt(result.Count() - 1).DocumentID;

            return resultSet;
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
        //
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

        public static string SearchHelperNgoacKep(string keyword)
        {

            string keywordFilter = keyword.Substring(1, keyword.Length - 2);
            return keywordFilter;
        }

        public static string[] SearchHelperPlus(string keyword)
        {
            string[] arrString = null;
            string keywordDiscardSpace = keyword.ToLower().Replace(" ", string.Empty);
            string finalKeyword = keywordDiscardSpace.Trim();
            // abg + bc => abg+bc
            string keywordSplittedFirst = finalKeyword.Substring(0, finalKeyword.IndexOf('+')); //==>abg
            string keywordSplittedSecond = finalKeyword.Substring(finalKeyword.IndexOf('+') + 1);//==>bc   
            arrString[0] = keywordSplittedFirst;
            arrString[1] = keywordSplittedSecond;
            return arrString;
        }

        public static string[] SearchHelperStraigtLine(string keyword)
        {
            string[] arrString = null;
            string keywordDiscardSpace = keyword.ToLower().Replace(" ", string.Empty);
            string finalKeyword = keywordDiscardSpace.Trim();
            // abg + bc => abg+bc
            string keywordSplittedFirst = finalKeyword.Substring(0, finalKeyword.IndexOf('|')); //==>abg
            string keywordSplittedSecond = finalKeyword.Substring(finalKeyword.IndexOf('|') + 1);//==>bc
            arrString[0] = keywordSplittedFirst;
            arrString[1] = keywordSplittedSecond;
            return arrString;
        }

        public static string[] SearchHelperMinus(string keyword)
        {
            string[] arrString = null;
            string keywordDiscardSpace = keyword.ToLower().Replace(" ", string.Empty);
            string finalKeyword = keywordDiscardSpace.Trim();
            // abg - bc => abg-bc
            string keywordSplittedFirst = finalKeyword.Substring(0, finalKeyword.IndexOf('-')); //==>abg
            string keywordSplittedSecond = finalKeyword.Substring(finalKeyword.IndexOf('-') + 1);//==>bc               
            arrString[0] = keywordSplittedFirst;
            arrString[1] = keywordSplittedSecond;
            return arrString;
        }

        //tìm kiếm bằng tiêu đề
        public IEnumerable<DocumentEssential> searchByTitle(string keyword, IEnumerable<DocumentEssential> result)
        {
            if (keyword.ToLower().IndexOf('"') == 0)
            {
                string keywordFilter = SearchHelperNgoacKep(keyword);
                result = result.Where(p => p.Title != null && p.Title.ToLower() == keywordFilter.ToLower());
            }
            else if (keyword.Contains('+'))
            {
                string[] keywordFilter = SearchHelperPlus(keyword);
                result = result.Where(p => p.Title != null && p.Title.ToLower().Contains(keywordFilter[0].ToLower()) && p.Title.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('|'))
            {
                string[] keywordFilter = SearchHelperStraigtLine(keyword);
                result = result.Where(p => p.Title != null && p.Title.ToLower().Contains(keywordFilter[0].ToLower()) || p.Title.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('-'))
            {
                string[] keywordFilter = SearchHelperMinus(keyword);
                result = result.Where(p => p.Title != null && (p.Title.ToLower().Contains(keywordFilter[0].ToLower()) && !p.Title.ToLower().Contains(keywordFilter[1].ToLower())));
            }
            else
            {
                result = result.Where(p => p.Title != null && p.Title.ToLower().Contains(keyword.ToLower()));
            }
            return result;
        }

        //tìm kiếm bằng tác giả
        public IEnumerable<DocumentEssential> searchByAuthor(string keyword, IEnumerable<DocumentEssential> result)
        {
            if (keyword.ToLower().IndexOf('"') == 0)
            {
                string keywordFilter = SearchHelperNgoacKep(keyword);
                result = result.Where(p => p.Creator != null && p.Creator.ToLower() == keywordFilter.ToLower());
            }
            else if (keyword.Contains('+'))
            {
                string[] keywordFilter = SearchHelperPlus(keyword);
                result = result.Where(p => p.Creator != null && p.Creator.ToLower().Contains(keywordFilter[0].ToLower()) && p.Creator.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('|'))
            {
                string[] keywordFilter = SearchHelperStraigtLine(keyword);
                result = result.Where(p => p.Creator != null && p.Creator.ToLower().Contains(keywordFilter[0].ToLower()) || p.Creator.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('-'))
            {
                string[] keywordFilter = SearchHelperMinus(keyword);
                result = result.Where(p => p.Creator != null && (p.Creator.ToLower().Contains(keywordFilter[0].ToLower()) && !p.Creator.ToLower().Contains(keywordFilter[1].ToLower())));
            }
            else
            {
                result = result.Where(p => p.Creator != null && p.Creator.ToLower().Contains(keyword.ToLower()));
            }
            return result;
        }

        //tìm kiếm theo chủ đề
        public IEnumerable<DocumentEssential> searchBySubject(string keyword, IEnumerable<DocumentEssential> result)
        {
            if (keyword.ToLower().IndexOf('"') == 0)
            {
                string keywordFilter = SearchHelperNgoacKep(keyword);
                result = result.Where(p => p.Subject != null && p.Subject.ToLower() == keywordFilter.ToLower());
            }
            else if (keyword.Contains('+'))
            {
                string[] keywordFilter = SearchHelperPlus(keyword);
                result = result.Where(p => p.Subject != null && p.Subject.ToLower().Contains(keywordFilter[0].ToLower()) && p.Subject.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('|'))
            {
                string[] keywordFilter = SearchHelperStraigtLine(keyword);
                result = result.Where(p => p.Subject != null && p.Subject.ToLower().Contains(keywordFilter[0].ToLower()) || p.Subject.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('-'))
            {
                string[] keywordFilter = SearchHelperMinus(keyword);
                result = result.Where(p => p.Subject != null && (p.Subject.ToLower().Contains(keywordFilter[0].ToLower()) && !p.Subject.ToLower().Contains(keywordFilter[1].ToLower())));
            }
            else
            {
                result = result.Where(p => p.Subject != null && p.Subject.ToLower().Contains(keyword.ToLower()));
            }
            return result;
        }

        //search theo contributor
        public IEnumerable<DocumentEssential> searchByContributor(string keyword, IEnumerable<DocumentEssential> result)
        {
            if (keyword.ToLower().IndexOf('"') == 0)
            {
                string keywordFilter = SearchHelperNgoacKep(keyword);
                result = result.Where(p => p.Contributor != null && p.Contributor.ToLower() == keywordFilter.ToLower());
            }
            else if (keyword.Contains('+'))
            {
                string[] keywordFilter = SearchHelperPlus(keyword);
                result = result.Where(p => p.Contributor != null && p.Contributor.ToLower().Contains(keywordFilter[0].ToLower()) && p.Contributor.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('|'))
            {
                string[] keywordFilter = SearchHelperStraigtLine(keyword);
                result = result.Where(p => p.Contributor != null && p.Contributor.ToLower().Contains(keywordFilter[0].ToLower()) || p.Contributor.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('-'))
            {
                string[] keywordFilter = SearchHelperMinus(keyword);
                result = result.Where(p => p.Contributor != null && (p.Contributor.ToLower().Contains(keywordFilter[0].ToLower()) && !p.Contributor.ToLower().Contains(keywordFilter[1].ToLower())));
            }
            else
            {
                result = result.Where(p => p.Contributor != null && p.Contributor.ToLower().Contains(keyword.ToLower()));
            }

            return result;
        }

        //search by type
        public IEnumerable<DocumentEssential> searchByType(string keyword, IEnumerable<DocumentEssential> result)
        {
            if (keyword.ToLower().IndexOf('"') == 0)
            {
                string keywordFilter = SearchHelperNgoacKep(keyword);
                result = result.Where(p => p.Type != null && p.Type.ToLower() == keywordFilter.ToLower());
            }
            else if (keyword.Contains('+'))
            {
                string[] keywordFilter = SearchHelperPlus(keyword);
                result = result.Where(p => p.Type != null && p.Type.ToLower().Contains(keywordFilter[0].ToLower()) && p.Type.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('|'))
            {
                string[] keywordFilter = SearchHelperStraigtLine(keyword);
                result = result.Where(p => p.Type != null && p.Type.ToLower().Contains(keywordFilter[0].ToLower()) || p.Type.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('-'))
            {
                string[] keywordFilter = SearchHelperMinus(keyword);
                result = result.Where(p => p.Type != null && (p.Type.ToLower().Contains(keywordFilter[0].ToLower()) && !p.Type.ToLower().Contains(keywordFilter[1].ToLower())));
            }
            else
            {
                result = result.Where(p => p.Type != null && p.Type.ToLower().Contains(keyword.ToLower()));
            }
            return result;
        }

        //tìm theo format
        public IEnumerable<DocumentEssential> searchByFormat(string keyword, IEnumerable<DocumentEssential> result)
        {
            if (keyword.ToLower().IndexOf('"') == 0)
            {
                string keywordFilter = SearchHelperNgoacKep(keyword);
                result = result.Where(p => p.Format != null && p.Format.ToLower() == keywordFilter.ToLower());
            }
            else if (keyword.Contains('+'))
            {
                string[] keywordFilter = SearchHelperPlus(keyword);
                result = result.Where(p => p.Format != null && p.Format.ToLower().Contains(keywordFilter[0].ToLower()) && p.Format.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('|'))
            {
                string[] keywordFilter = SearchHelperStraigtLine(keyword);
                result = result.Where(p => p.Format != null && p.Format.ToLower().Contains(keywordFilter[0].ToLower()) || p.Format.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('-'))
            {
                string[] keywordFilter = SearchHelperMinus(keyword);
                result = result.Where(p => p.Format != null && (p.Format.ToLower().Contains(keywordFilter[0].ToLower()) && !p.Format.ToLower().Contains(keywordFilter[1].ToLower())));
            }
            else
            {
                result = result.Where(p => p.Format != null && p.Format.ToLower().Contains(keyword.ToLower()));
            }
            return result;
        }

        //tìm theo identifier
        public IEnumerable<DocumentEssential> searchByIdentifier(string keyword, IEnumerable<DocumentEssential> result)
        {
            if (keyword.ToLower().IndexOf('"') == 0)
            {
                string keywordFilter = SearchHelperNgoacKep(keyword);
                result = result.Where(p => p.Identifier != null && p.Identifier.ToLower() == keywordFilter.ToLower());
            }
            else if (keyword.Contains('+'))
            {
                string[] keywordFilter = SearchHelperPlus(keyword);
                result = result.Where(p => p.Identifier != null && p.Identifier.ToLower().Contains(keywordFilter[0].ToLower()) && p.Identifier.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('|'))
            {
                string[] keywordFilter = SearchHelperStraigtLine(keyword);
                result = result.Where(p => p.Identifier != null && p.Identifier.ToLower().Contains(keywordFilter[0].ToLower()) || p.Identifier.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('-'))
            {
                string[] keywordFilter = SearchHelperMinus(keyword);
                result = result.Where(p => p.Identifier != null && (p.Identifier.ToLower().Contains(keywordFilter[0].ToLower()) && !p.Identifier.ToLower().Contains(keywordFilter[1].ToLower())));
            }
            else
            {
                result = result.Where(p => p.Identifier != null && p.Identifier.ToLower().Contains(keyword.ToLower()));
            }
            return result;
        }

        //tìm kiếm theo relation
        public IEnumerable<DocumentEssential> searchByRelation(string keyword, IEnumerable<DocumentEssential> result)
        {
            if (keyword.ToLower().IndexOf('"') == 0)
            {
                string keywordFilter = SearchHelperNgoacKep(keyword);
                result = result.Where(p => p.Relation != null && p.Relation.ToLower() == keywordFilter.ToLower());
            }
            else if (keyword.Contains('+'))
            {
                string[] keywordFilter = SearchHelperPlus(keyword);
                result = result.Where(p => p.Relation != null && p.Relation.ToLower().Contains(keywordFilter[0].ToLower()) && p.Relation.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('|'))
            {
                string[] keywordFilter = SearchHelperStraigtLine(keyword);
                result = result.Where(p => p.Relation != null && p.Relation.ToLower().Contains(keywordFilter[0].ToLower()) || p.Relation.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('-'))
            {
                string[] keywordFilter = SearchHelperMinus(keyword);
                result = result.Where(p => p.Relation != null && (p.Relation.ToLower().Contains(keywordFilter[0].ToLower()) && !p.Relation.ToLower().Contains(keywordFilter[1].ToLower())));
            }
            else
            {
                result = result.Where(p => p.Relation != null && p.Relation.ToLower().Contains(keyword.ToLower()));
            }
            return result;
        }

        //search theo Resource
        public IEnumerable<DocumentEssential> searchByResource(string keyword, IEnumerable<DocumentEssential> result)
        {
            if (keyword.ToLower().IndexOf('"') == 0)
            {
                string keywordFilter = SearchHelperNgoacKep(keyword);
                result = result.Where(p => p.Resource != null && p.Resource.ToLower() == keywordFilter.ToLower());
            }
            else if (keyword.Contains('+'))
            {
                string[] keywordFilter = SearchHelperPlus(keyword);
                result = result.Where(p => p.Resource != null && p.Resource.ToLower().Contains(keywordFilter[0].ToLower()) && p.Resource.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('|'))
            {
                string[] keywordFilter = SearchHelperStraigtLine(keyword);
                result = result.Where(p => p.Resource != null && p.Resource.ToLower().Contains(keywordFilter[0].ToLower()) || p.Resource.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('-'))
            {
                string[] keywordFilter = SearchHelperMinus(keyword);
                result = result.Where(p => p.Resource != null && (p.Resource.ToLower().Contains(keywordFilter[0].ToLower()) && !p.Resource.ToLower().Contains(keywordFilter[1].ToLower())));
            }
            else
            {
                result = result.Where(p => p.Resource != null && p.Resource.ToLower().Contains(keyword.ToLower()));
            }
            return result;
        }

        //search By Lang
        public IEnumerable<DocumentEssential> searchByLanguage(string keyword, IEnumerable<DocumentEssential> result)
        {
            if (keyword.ToLower().IndexOf('"') == 0)
            {
                string keywordFilter = SearchHelperNgoacKep(keyword);
                result = result.Where(p => p.Language != null && p.Language.ToLower() == keywordFilter.ToLower());
            }
            else if (keyword.Contains('+'))
            {
                string[] keywordFilter = SearchHelperPlus(keyword);
                result = result.Where(p => p.Language != null && p.Language.ToLower().Contains(keywordFilter[0].ToLower()) && p.Language.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('|'))
            {
                string[] keywordFilter = SearchHelperStraigtLine(keyword);
                result = result.Where(p => p.Language != null && p.Language.ToLower().Contains(keywordFilter[0].ToLower()) || p.Language.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('-'))
            {
                string[] keywordFilter = SearchHelperMinus(keyword);
                result = result.Where(p => p.Language != null && (p.Language.ToLower().Contains(keywordFilter[0].ToLower()) && !p.Language.ToLower().Contains(keywordFilter[1].ToLower())));
            }
            else
            {
                result = result.Where(p => p.Language != null && p.Language.ToLower().Contains(keyword.ToLower()));
            }
            return result;
        }

        //search By right
        public IEnumerable<DocumentEssential> searchByRight(string keyword, IEnumerable<DocumentEssential> result)
        {
            if (keyword.ToLower().IndexOf('"') == 0)
            {
                string keywordFilter = SearchHelperNgoacKep(keyword);
                result = result.Where(p => p.Right != null && p.Right.ToLower() == keywordFilter.ToLower());
            }
            else if (keyword.Contains('+'))
            {
                string[] keywordFilter = SearchHelperPlus(keyword);
                result = result.Where(p => p.Right != null && p.Right.ToLower().Contains(keywordFilter[0].ToLower()) && p.Right.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('|'))
            {
                string[] keywordFilter = SearchHelperStraigtLine(keyword);
                result = result.Where(p => p.Right != null && p.Right.ToLower().Contains(keywordFilter[0].ToLower()) || p.Right.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('-'))
            {
                string[] keywordFilter = SearchHelperMinus(keyword);
                result = result.Where(p => p.Right != null && (p.Right.ToLower().Contains(keywordFilter[0].ToLower()) && !p.Right.ToLower().Contains(keywordFilter[1].ToLower())));
            }
            else
            {
                result = result.Where(p => p.Right != null && p.Right.ToLower().Contains(keyword.ToLower()));
            }
            return result;
        }

        public IEnumerable<DocumentEssential> searchByDescription(string keyword, IEnumerable<DocumentEssential> result)
        {
            if (keyword.ToLower().IndexOf('"') == 0)
            {
                string keywordFilter = SearchHelperNgoacKep(keyword);
                result = result.Where(p => p.Description != null && p.Description.ToLower() == keywordFilter.ToLower());
            }
            else if (keyword.Contains('+'))
            {
                string[] keywordFilter = SearchHelperPlus(keyword);
                result = result.Where(p => p.Description != null && p.Description.ToLower().Contains(keywordFilter[0].ToLower()) && p.Description.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('|'))
            {
                string[] keywordFilter = SearchHelperStraigtLine(keyword);
                result = result.Where(p => p.Description != null && p.Description.ToLower().Contains(keywordFilter[0].ToLower()) || p.Description.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('-'))
            {
                string[] keywordFilter = SearchHelperMinus(keyword);
                result = result.Where(p => p.Description != null && (p.Description.ToLower().Contains(keywordFilter[0].ToLower()) && !p.Description.ToLower().Contains(keywordFilter[1].ToLower())));
            }
            else
            {
                result = result.Where(p => p.Description != null && p.Description.ToLower().Contains(keyword.ToLower()));
            }

            return result;
        }

        //search By Publisher
        public IEnumerable<DocumentEssential> searchByPublisher(string keyword, IEnumerable<DocumentEssential> result)
        {
            if (keyword.ToLower().IndexOf('"') == 0)
            {
                string keywordFilter = SearchHelperNgoacKep(keyword);
                result = result.Where(p => p.Publisher != null && p.Publisher.ToLower() == keywordFilter.ToLower());
            }
            else if (keyword.Contains('+'))
            {
                string[] keywordFilter = SearchHelperPlus(keyword);
                result = result.Where(p => p.Publisher != null && p.Publisher.ToLower().Contains(keywordFilter[0].ToLower()) && p.Publisher.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('|'))
            {
                string[] keywordFilter = SearchHelperStraigtLine(keyword);
                result = result.Where(p => p.Publisher != null && p.Publisher.ToLower().Contains(keywordFilter[0].ToLower()) || p.Publisher.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('-'))
            {
                string[] keywordFilter = SearchHelperMinus(keyword);
                result = result.Where(p => p.Publisher != null && (p.Publisher.ToLower().Contains(keywordFilter[0].ToLower()) && !p.Publisher.ToLower().Contains(keywordFilter[1].ToLower())));
            }
            else
            {
                result = result.Where(p => p.Publisher != null && p.Publisher.ToLower().Contains(keyword.ToLower()));
            }
            return result;
        }

        //searhc by Coverage
        public IEnumerable<DocumentEssential> searchByCoverage(string keyword, IEnumerable<DocumentEssential> result)
        {
            if (keyword.ToLower().IndexOf('"') == 0)
            {
                string keywordFilter = SearchHelperNgoacKep(keyword);
                result = result.Where(p => p.Coverage != null && p.Coverage.ToLower() == keywordFilter.ToLower());
            }
            else if (keyword.Contains('+'))
            {
                string[] keywordFilter = SearchHelperPlus(keyword);
                result = result.Where(p => p.Coverage != null && p.Coverage.ToLower().Contains(keywordFilter[0].ToLower()) && p.Coverage.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('|'))
            {
                string[] keywordFilter = SearchHelperStraigtLine(keyword);
                result = result.Where(p => p.Coverage != null && p.Coverage.ToLower().Contains(keywordFilter[0].ToLower()) || p.Coverage.ToLower().Contains(keywordFilter[1].ToLower()));
            }
            else if (keyword.Contains('-'))
            {
                string[] keywordFilter = SearchHelperMinus(keyword);
                result = result.Where(p => p.Coverage != null && (p.Coverage.ToLower().Contains(keywordFilter[0].ToLower()) && !p.Coverage.ToLower().Contains(keywordFilter[1].ToLower())));
            }
            else
            {
                result = result.Where(p => p.Coverage != null && p.Coverage.ToLower().Contains(keyword.ToLower()));
            }
            return result;
        }
    }
}
