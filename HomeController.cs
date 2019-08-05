using DBApp.CustomFilters;
using DBApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;

namespace DBApp.Controllers
{
    public class HomeController : Controller
    {
        
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Index()
        {
            //return encodedData;
            #region OldCode
            //string strConString = ConfigurationManager.AppSettings["AutoAppDatabase"];
            //SqlConnection con = new SqlConnection(strConString);
            //con.Open();
            //SqlCommand cmd = new SqlCommand("Select * from Queries", con);
            //SqlDataAdapter sd = new SqlDataAdapter(cmd);
            //DataTable dt = new DataTable();
            //sd.Fill(dt);
            //List<QueriesModel> getQueryList = new List<QueriesModel>();
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    QueriesModel student = new QueriesModel();
            //    student.QueryID = Convert.ToInt32(dt.Rows[i]["QueryID"]);
            //    student.Query1 = dt.Rows[i]["Query"].ToString();
            //    student.QueryAlias = dt.Rows[i]["QueryAlias"].ToString();
            //    getQueryList.Add(student);
            //}
            #endregion
            QueryModel queryModel = new QueryModel();
            var database = new QueryDBAppEntities1();
            var getQueryList = database.Queries.ToList();
            getQueryList = getQueryList.OrderBy(p => p.QueryAlias).ToList();
            List<SelectListItem> list = null;
            IEnumerable<SelectListItem> queryList = getQueryList.Select(f => new SelectListItem
            {
                Value = f.Query1.ToString(),
                Text = f.QueryAlias
            });
            list = queryList.ToList();
            var newItem = new SelectListItem { Value = "", Text = "Select a Query" };
            list.Insert(0, newItem);
            queryModel.QueryItems = list;

            var getDBList = database.EPIDatabases.ToList();
            List<SelectListItem> dblist = null;
            IEnumerable<SelectListItem> queryDBList = getDBList.Select(f => new SelectListItem
            {
                Value = f.DatabaseFullName,
                Text = f.DatabaseShortName
            });
            dblist = queryDBList.ToList();
            var newDBItem = new SelectListItem { Value = "", Text = "Select a Query" };
            dblist.Insert(0, newDBItem);
            queryModel.DatabaseItems = dblist;
            return View(queryModel);
        }
        //public ActionResult ExeQuery(string query, string database)
        //{
        //    try
        //    {
        //        DynamicDataReader dynData = new DynamicDataReader();
        //    var data = new List<dynamic>();
        //    string constring = ConfigurationManager.AppSettings["DataSource"].ToString() + database + ConfigurationManager.AppSettings["IntegratedSecurity"].ToString();
        //    SqlConnection con = new SqlConnection(constring);
        //    SqlCommand cmd = new SqlCommand(query, con);
        //    //con = new SqlConnection(constring);
        //    con.Open();
        //    SqlDataAdapter da = new SqlDataAdapter(cmd);
        //    // this will query your database and return the result to your datatable
        //    DataTable dt = new DataTable();
        //    da.Fill(dt);
           
        //    DataTable dtt = dt;
        //    List<WebGridColumn> columns = new List<WebGridColumn>();
        //    foreach (DataColumn col in dt.Columns)
        //    {
        //        columns.Add(new WebGridColumn()
        //        {
        //            ColumnName = col.ColumnName,
        //            Header = col.ColumnName
        //        });
        //    }

        //    dynData.QueryColumns = columns;
        //    foreach (var item in dt.AsEnumerable())
        //    {
        //        IDictionary<string, object> dn = new ExpandoObject();
        //        foreach (var column in dt.Columns.Cast<DataColumn>())
        //        {
                    
        //            dn[column.ColumnName] = item[column];
        //        }

        //        data.Add(dn);
                
                
        //    }
        //    dynData.QueryData = data;
        //    dynData.QueryDataCount = data.Count();
        //    return PartialView("_WebGrid", dynData);
        //    }
        //    catch (Exception ex)
        //    {
        //        string message = string.Format("ErrorMessage: {0}", ex.Message);
        //        message = message.TrimEnd('\r', '\n');
        //        return Json(message, JsonRequestBehavior.AllowGet);
        //    }
        //}
        public JsonResult ExecuteQuery(string query, string database)
        {
            try
            {
                if (query != "")
                {
                    string constring = ConfigurationManager.AppSettings["DataSource"].ToString() + database + ConfigurationManager.AppSettings["IntegratedSecurity"].ToString();
                    SqlConnection con = new SqlConnection(constring);
                    //con = new SqlConnection(constring);
                    con.Open();
                    SqlCommand cmd = new SqlCommand(query, con);
                    var reader = cmd.ExecuteReader();
                    List<Dictionary<string, object>> expandolist = new List<Dictionary<string, object>>();
                    var names = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
                    var Types = Enumerable.Range(0, reader.FieldCount).Select(reader.GetDataTypeName).ToList();
                    var newList = names.Zip(Types, (s, i) => new { sv = s, iv = i }).ToList();
                    IDictionary<string, object> expando = new ExpandoObject();
                    foreach (IDataRecord record in reader as IEnumerable)
                    {
                        int count = record.FieldCount;
                        for (int i = 0; i < count; i++)
                        {
                            Type x = record.GetFieldType(i);
                            string field = record.GetName(i);
                            if (reader.IsDBNull(i))
                            {
                                expando[names.ElementAt(i)] = string.Empty;
                            }

                            if (x.Name == "DBNull")
                            {
                                var z = record.GetValue(i);
                            }
                            if (x.Name == "DateTime")
                            {
                                var y = record.GetValue(i);
                                if (y != null && y.ToString() != string.Empty)
                                {
                                    string asString = Convert.ToDateTime(y).ToString("yyyy/MM/dd");
                                    expando[names.ElementAt(i)] = asString;
                                }
                                else
                                    expando[names.ElementAt(i)] = string.Empty;
                            }
                            else
                            {
                                expando[names.ElementAt(i)] = record[names.ElementAt(i)];
                            }
                        }
                        expandolist.Add(new Dictionary<string, object>(expando));
                        
                    }
                    con.Close();
                    var jsonResult = Json(expandolist, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue;
                    return jsonResult;
                    //return Json(expandolist, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json("Please select the query", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string message = string.Format("ErrorMessage: {0}", ex.Message);
                return Json(message, JsonRequestBehavior.AllowGet);
            }
        }

   

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User user, string returnUrl)
        {
            using (var database = new QueryDBAppEntities1())
            {
                SHA1 sha = new SHA1CryptoServiceProvider();
                ASCIIEncoding encoder = new ASCIIEncoding();
                byte[] combined = encoder.GetBytes(user.Password);
                string hash = BitConverter.ToString(sha.ComputeHash(combined)).Replace("-", "");
                //return result;

                var Verify = database.Users.FirstOrDefault(usr => usr.UserName == user.UserName && usr.Password == hash);
                if (Verify != null)
                {
                    FormsAuthentication.SetAuthCookie(Verify.UserName, false);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/") && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("AddQuery");
                    }
                }
                else {
                    ModelState.AddModelError("CustomError", "Either your credentials are not correct or you do not have access as you are not an admin");
                    return View("Login");
                }
            }
        }
        //[HttpPost]
        //public void SubmitCredentials(User user)
        //{

        //}
        [Authorize]
        public ActionResult AddQuery()
        {
            return View();
        }
        //[AuthLog(Roles ="Admin")]
        [HttpPost]
        public JsonResult DisplayQueryList(int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            //List<Dictionary<string, object>> expandolist = new List<Dictionary<string, object>>();
            List<Query> list = new List<Query>();
            //expandolist = getResult();
            var database = new QueryDBAppEntities1();
            var expandolist = database.Queries.ToList();
            if (jtSorting.Equals("QueryID DESC"))
            {
                expandolist = expandolist.OrderByDescending(p => p.QueryID).ToList();
            }
            list = expandolist.Skip(jtStartIndex).Take(jtPageSize).ToList();
            var queryCount = expandolist.Count();
            return Json(new { Result = "OK", Records = list, TotalRecordCount = queryCount }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateQuery(Query QueriesModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { Result = "ERROR", Message = "Form is not valid! Please correct it and try again." });
                }
                //string constring = ConfigurationManager.AppSettings["AutoAppDatabase"];
                //SqlConnection con = new SqlConnection(constring);
                //string insertQuery = "Insert into Queries (Query, QueryAlias) values(@query, @queryAlias)";
                //con.Open();
                //SqlCommand cmd = new SqlCommand(insertQuery, con);
                //cmd.Parameters.AddWithValue("@query", QueriesModel.Query1);
                //cmd.Parameters.AddWithValue("@queryAlias", QueriesModel.QueryAlias);
                //cmd.ExecuteNonQuery();
                //con.Close();
                var database = new QueryDBAppEntities1();
                database.Queries.Add(QueriesModel);
                database.SaveChanges();
                return Json(new { Result = "OK", Record = QueriesModel }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult UpdateQuery(Query QueriesModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { Result = "ERROR", Message = "Form is not valid! Please correct it and try again." });
                }
                //string constring = ConfigurationManager.AppSettings["AutoAppDatabase"];
                //SqlConnection con = new SqlConnection(constring);
                //string updateQuery = "update Queries set Query= '" + QueriesModel.Query1 + "', QueryAlias= '" + QueriesModel.QueryAlias + "' where QueryID ='"+ QueriesModel.QueryID + "'"; 
                //con.Open();
                //SqlCommand cmd = new SqlCommand(updateQuery, con);
                //cmd.ExecuteNonQuery();
                //con.Close();
                var database = new QueryDBAppEntities1();
                database.Entry(QueriesModel).State = EntityState.Modified;
                database.SaveChanges();
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult DeleteQuery(int QueryID)
        {
            try
            {

                //string constring = ConfigurationManager.AppSettings["AutoAppDatabase"];
                //SqlConnection con = new SqlConnection(constring);
                //string updateQuery = "delete from Queries where QueryID = '" + QueryID + "'";
                //con.Open();
                //SqlCommand cmd = new SqlCommand(updateQuery, con);
                //cmd.ExecuteNonQuery();
                //con.Close();
                var database = new QueryDBAppEntities1();
                Query queriesDetail = database.Queries.Find(QueryID);

                database.Queries.Remove(queriesDetail);

                database.SaveChanges();
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DisplayDBList(int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            //List<Dictionary<string, object>> expandolist = new List<Dictionary<string, object>>();
            List<EPIDatabases> list = new List<EPIDatabases>();
            //expandolist = getResult();
            var database = new QueryDBAppEntities1();
            var expandolist = database.EPIDatabases.ToList();
            if (jtSorting.Equals("DatatabseID DESC"))
            {
                expandolist = expandolist.OrderByDescending(p => p.DatatabseID).ToList();
            }
            list = expandolist.Skip(jtStartIndex).Take(jtPageSize).ToList();
            var queryCount = expandolist.Count();
            return Json(new { Result = "OK", Records = list, TotalRecordCount = queryCount }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult CreateDB(EPIDatabases DBModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { Result = "ERROR", Message = "Form is not valid! Please correct it and try again." });
                }
                var database = new QueryDBAppEntities1();
                database.EPIDatabases.Add(DBModel);
                database.SaveChanges();
                return Json(new { Result = "OK", Record = DBModel }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult UpdateDB(EPIDatabases DBModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { Result = "ERROR", Message = "Form is not valid! Please correct it and try again." });
                }
                var database = new QueryDBAppEntities1();
                database.Entry(DBModel).State = EntityState.Modified;
                database.SaveChanges();
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult DeleteDB(int DatatabseID)
        {
            try
            {
                var database = new QueryDBAppEntities1();
                EPIDatabases dbDetail = database.EPIDatabases.Find(DatatabseID);

                database.EPIDatabases.Remove(dbDetail);

                database.SaveChanges();
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }
    }
}