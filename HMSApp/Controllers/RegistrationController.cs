using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;
using PagedList;
using PagedList.Mvc;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using HMSApp.Models;

namespace HMSApp.Controllers
{
    public class RegistrationController : Controller
    {
        private HMSDBDBContext db = new HMSDBDBContext();
        
        // GET: /Registration/
        public ActionResult Index(string sortOrder,  
                                  String SearchField,
                                  String SearchCondition,
                                  String SearchText,
                                  String Export,
                                  int? PageSize,
                                  int? page, 
                                  string command)
        {

            if (Session["UserSession"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            if (command == "Show All") {
                SearchField = null;
                SearchCondition = null;
                SearchText = null;
                Session["SearchField"] = null;
                Session["SearchCondition"] = null;
                Session["SearchText"] = null; } 
            else if (command == "Add New Record") { return RedirectToAction("Create"); } 
            else if (command == "Export") { Session["Export"] = Export; } 
            else if (command == "Search" | command == "Page Size") {
                if (!string.IsNullOrEmpty(SearchText)) {
                    Session["SearchField"] = SearchField;
                    Session["SearchCondition"] = SearchCondition;
                    Session["SearchText"] = SearchText; }
                } 
            if (command == "Page Size") { Session["PageSize"] = PageSize; }

            ViewData["SearchFields"] = GetFields((Session["SearchField"] == null ? "Patient I D" : Convert.ToString(Session["SearchField"])));
            ViewData["SearchConditions"] = Library.GetConditions((Session["SearchCondition"] == null ? "Contains" : Convert.ToString(Session["SearchCondition"])));
            ViewData["SearchText"] = Session["SearchText"];
            ViewData["Exports"] = Library.GetExports((Session["Export"] == null ? "Pdf" : Convert.ToString(Session["Export"])));
            ViewData["PageSizes"] = Library.GetPageSizes();

            ViewData["CurrentSort"] = sortOrder;
            ViewData["PatientIDSortParm"] = sortOrder == "PatientID_asc" ? "PatientID_desc" : "PatientID_asc";
            ViewData["NameSortParm"] = sortOrder == "Name_asc" ? "Name_desc" : "Name_asc";
            ViewData["AddressSortParm"] = sortOrder == "Address_asc" ? "Address_desc" : "Address_asc";
            ViewData["NICSortParm"] = sortOrder == "NIC_asc" ? "NIC_desc" : "NIC_asc";
            ViewData["GenderIDSortParm"] = sortOrder == "GenderID_asc" ? "GenderID_desc" : "GenderID_asc";
            ViewData["DOBSortParm"] = sortOrder == "DOB_asc" ? "DOB_desc" : "DOB_asc";
            ViewData["PhoneNoSortParm"] = sortOrder == "PhoneNo_asc" ? "PhoneNo_desc" : "PhoneNo_asc";
            ViewData["HabitsSortParm"] = sortOrder == "Habits_asc" ? "Habits_desc" : "Habits_asc";
            ViewData["AllergicSortParm"] = sortOrder == "Allergic_asc" ? "Allergic_desc" : "Allergic_asc";
            ViewData["WeightSortParm"] = sortOrder == "Weight_asc" ? "Weight_desc" : "Weight_asc";
            ViewData["HeightSortParm"] = sortOrder == "Height_asc" ? "Height_desc" : "Height_asc";
            ViewData["DateSortParm"] = sortOrder == "Date_asc" ? "Date_desc" : "Date_asc";

            var Query = db.Registration.AsQueryable();

            try {
                if (!string.IsNullOrEmpty(Convert.ToString(Session["SearchField"])) && !string.IsNullOrEmpty(Convert.ToString(Session["SearchCondition"])) && !string.IsNullOrEmpty(Convert.ToString(Session["SearchText"])))
                {
                    SearchField = Convert.ToString(Session["SearchField"]);
                    SearchCondition = Convert.ToString(Session["SearchCondition"]);
                    SearchText = Convert.ToString(Session["SearchText"]);

                    if (SearchCondition == "Contains") {
                        Query = Query.Where(p => 
                                                 ("Patient I D".ToString().Equals(SearchField) && p.PatientID.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Name".ToString().Equals(SearchField) && p.Name.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Address".ToString().Equals(SearchField) && p.Address.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("N I C".ToString().Equals(SearchField) && p.NIC.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Gender I D".ToString().Equals(SearchField) && p.Gender.Name.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("D O B".ToString().Equals(SearchField) && p.DOB.Value.ToString().Contains(SearchText)) 
                                                 || ("Phone No".ToString().Equals(SearchField) && p.PhoneNo.Value.ToString().Contains(SearchText)) 
                                                 || ("Habits".ToString().Equals(SearchField) && p.Habits.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Allergic".ToString().Equals(SearchField) && p.Allergic.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Weight".ToString().Equals(SearchField) && p.Weight.Value.ToString().Contains(SearchText)) 
                                                 || ("Height".ToString().Equals(SearchField) && p.Height.Value.ToString().Contains(SearchText)) 
                                                 || ("Date".ToString().Equals(SearchField) && p.Date.Value.ToString().Contains(SearchText)) 
                                         );
                    } else if (SearchCondition == "Starts with...") {
                        Query = Query.Where(p => 
                                                 ("Patient I D".ToString().Equals(SearchField) && p.PatientID.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Name".ToString().Equals(SearchField) && p.Name.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Address".ToString().Equals(SearchField) && p.Address.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("N I C".ToString().Equals(SearchField) && p.NIC.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Gender I D".ToString().Equals(SearchField) && p.Gender.Name.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("D O B".ToString().Equals(SearchField) && p.DOB.Value.ToString().StartsWith(SearchText)) 
                                                 || ("Phone No".ToString().Equals(SearchField) && p.PhoneNo.Value.ToString().StartsWith(SearchText)) 
                                                 || ("Habits".ToString().Equals(SearchField) && p.Habits.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Allergic".ToString().Equals(SearchField) && p.Allergic.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Weight".ToString().Equals(SearchField) && p.Weight.Value.ToString().StartsWith(SearchText)) 
                                                 || ("Height".ToString().Equals(SearchField) && p.Height.Value.ToString().StartsWith(SearchText)) 
                                                 || ("Date".ToString().Equals(SearchField) && p.Date.Value.ToString().StartsWith(SearchText)) 
                                         );
                    } else if (SearchCondition == "Equals") {
                        if ("Patient I D".Equals(SearchField)) { var mPatientID = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.PatientID == mPatientID); }
                        else if ("Name".Equals(SearchField)) { var mName = System.Convert.ToString(SearchText); Query = Query.Where(p => p.Name == mName); }
                        else if ("Address".Equals(SearchField)) { var mAddress = System.Convert.ToString(SearchText); Query = Query.Where(p => p.Address == mAddress); }
                        else if ("N I C".Equals(SearchField)) { var mNIC = System.Convert.ToString(SearchText); Query = Query.Where(p => p.NIC == mNIC); }
                        else if ("Gender I D".Equals(SearchField)) { var mGenderID = System.Convert.ToString(SearchText); Query = Query.Where(p => p.Gender.Name == mGenderID); }
                        else if ("D O B".Equals(SearchField)) { var mDOB = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.DOB == mDOB); }
                        else if ("Phone No".Equals(SearchField)) { var mPhoneNo = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.PhoneNo == mPhoneNo); }
                        else if ("Habits".Equals(SearchField)) { var mHabits = System.Convert.ToString(SearchText); Query = Query.Where(p => p.Habits == mHabits); }
                        else if ("Allergic".Equals(SearchField)) { var mAllergic = System.Convert.ToString(SearchText); Query = Query.Where(p => p.Allergic == mAllergic); }
                        else if ("Weight".Equals(SearchField)) { var mWeight = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.Weight == mWeight); }
                        else if ("Height".Equals(SearchField)) { var mHeight = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.Height == mHeight); }
                        else if ("Date".Equals(SearchField)) { var mDate = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Date == mDate); }
                    } else if (SearchCondition == "More than...") { 
                        if (SearchField.Equals(SearchCondition)) { }
                        else if ("Patient I D".Equals(SearchField)) { var mPatientID = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.PatientID > mPatientID); }
                        else if ("D O B".Equals(SearchField)) { var mDOB = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.DOB > mDOB); }
                        else if ("Phone No".Equals(SearchField)) { var mPhoneNo = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.PhoneNo > mPhoneNo); }
                        else if ("Weight".Equals(SearchField)) { var mWeight = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.Weight > mWeight); }
                        else if ("Height".Equals(SearchField)) { var mHeight = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.Height > mHeight); }
                        else if ("Date".Equals(SearchField)) { var mDate = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Date > mDate); }
                    } else if (SearchCondition == "Less than...") { 
                        if (SearchField.Equals(SearchCondition)) { }
                        else if ("Patient I D".Equals(SearchField)) { var mPatientID = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.PatientID < mPatientID); }
                        else if ("D O B".Equals(SearchField)) { var mDOB = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.DOB < mDOB); }
                        else if ("Phone No".Equals(SearchField)) { var mPhoneNo = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.PhoneNo < mPhoneNo); }
                        else if ("Weight".Equals(SearchField)) { var mWeight = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.Weight < mWeight); }
                        else if ("Height".Equals(SearchField)) { var mHeight = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.Height < mHeight); }
                        else if ("Date".Equals(SearchField)) { var mDate = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Date < mDate); }
                    } else if (SearchCondition == "Equal or more than...") { 
                        if (SearchField.Equals(SearchCondition)) { }
                        else if ("Patient I D".Equals(SearchField)) { var mPatientID = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.PatientID >= mPatientID); }
                        else if ("D O B".Equals(SearchField)) { var mDOB = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.DOB >= mDOB); }
                        else if ("Phone No".Equals(SearchField)) { var mPhoneNo = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.PhoneNo >= mPhoneNo); }
                        else if ("Weight".Equals(SearchField)) { var mWeight = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.Weight >= mWeight); }
                        else if ("Height".Equals(SearchField)) { var mHeight = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.Height >= mHeight); }
                        else if ("Date".Equals(SearchField)) { var mDate = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Date >= mDate); }
                    } else if (SearchCondition == "Equal or less than...") { 
                        if (SearchField.Equals(SearchCondition)) { }
                        else if ("Patient I D".Equals(SearchField)) { var mPatientID = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.PatientID <= mPatientID); }
                        else if ("D O B".Equals(SearchField)) { var mDOB = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.DOB <= mDOB); }
                        else if ("Phone No".Equals(SearchField)) { var mPhoneNo = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.PhoneNo <= mPhoneNo); }
                        else if ("Weight".Equals(SearchField)) { var mWeight = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.Weight <= mWeight); }
                        else if ("Height".Equals(SearchField)) { var mHeight = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.Height <= mHeight); }
                        else if ("Date".Equals(SearchField)) { var mDate = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Date <= mDate); }
                    }
                }
            } catch (Exception) { }

            switch (sortOrder)
            {
                case "PatientID_desc":
                    Query = Query.OrderByDescending(s => s.PatientID);
                    break;
                case "PatientID_asc":
                    Query = Query.OrderBy(s => s.PatientID);
                    break;
                case "Name_desc":
                    Query = Query.OrderByDescending(s => s.Name);
                    break;
                case "Name_asc":
                    Query = Query.OrderBy(s => s.Name);
                    break;
                case "Address_desc":
                    Query = Query.OrderByDescending(s => s.Address);
                    break;
                case "Address_asc":
                    Query = Query.OrderBy(s => s.Address);
                    break;
                case "NIC_desc":
                    Query = Query.OrderByDescending(s => s.NIC);
                    break;
                case "NIC_asc":
                    Query = Query.OrderBy(s => s.NIC);
                    break;
                case "GenderID_desc":
                    Query = Query.OrderByDescending(s => s.Gender.Name);
                    break;
                case "GenderID_asc":
                    Query = Query.OrderBy(s => s.Gender.Name);
                    break;
                case "DOB_desc":
                    Query = Query.OrderByDescending(s => s.DOB);
                    break;
                case "DOB_asc":
                    Query = Query.OrderBy(s => s.DOB);
                    break;
                case "PhoneNo_desc":
                    Query = Query.OrderByDescending(s => s.PhoneNo);
                    break;
                case "PhoneNo_asc":
                    Query = Query.OrderBy(s => s.PhoneNo);
                    break;
                case "Habits_desc":
                    Query = Query.OrderByDescending(s => s.Habits);
                    break;
                case "Habits_asc":
                    Query = Query.OrderBy(s => s.Habits);
                    break;
                case "Allergic_desc":
                    Query = Query.OrderByDescending(s => s.Allergic);
                    break;
                case "Allergic_asc":
                    Query = Query.OrderBy(s => s.Allergic);
                    break;
                case "Weight_desc":
                    Query = Query.OrderByDescending(s => s.Weight);
                    break;
                case "Weight_asc":
                    Query = Query.OrderBy(s => s.Weight);
                    break;
                case "Height_desc":
                    Query = Query.OrderByDescending(s => s.Height);
                    break;
                case "Height_asc":
                    Query = Query.OrderBy(s => s.Height);
                    break;
                case "Date_desc":
                    Query = Query.OrderByDescending(s => s.Date);
                    break;
                case "Date_asc":
                    Query = Query.OrderBy(s => s.Date);
                    break;
                default:  // Name ascending 
                    Query = Query.OrderBy(s => s.PatientID);
                    break;
            }

            if (command == "Export") {
                GridView gv = new GridView();
                DataTable dt = new DataTable();
                dt.Columns.Add("Patient I D", typeof(string));
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("Address", typeof(string));
                dt.Columns.Add("N I C", typeof(string));
                dt.Columns.Add("Gender I D", typeof(string));
                dt.Columns.Add("D O B", typeof(string));
                dt.Columns.Add("Phone No", typeof(string));
                dt.Columns.Add("Habits", typeof(string));
                dt.Columns.Add("Allergic", typeof(string));
                dt.Columns.Add("Weight", typeof(string));
                dt.Columns.Add("Height", typeof(string));
                dt.Columns.Add("Date", typeof(string));
                foreach (var item in Query.ToList())
                {
                    dt.Rows.Add(
                        item.PatientID
                       ,item.Name
                       ,item.Address
                       ,item.NIC
                       ,item.Gender.Name
                       ,item.DOB
                       ,item.PhoneNo
                       ,item.Habits
                       ,item.Allergic
                       ,item.Weight
                       ,item.Height
                       ,item.Date
                    );
                }
                gv.DataSource = dt;
                gv.DataBind();
                ExportData(Export, gv, dt);
            }

            int pageNumber = (page ?? 1);
            int? pageSZ = (Convert.ToInt32(Session["PageSize"]) == 0 ? 5 : Convert.ToInt32(Session["PageSize"]));
            return View(Query.ToPagedList(pageNumber, (pageSZ ?? 5)));
        }

        // GET: /Registration/Details/<id>
        public ActionResult Details(
                                      Int32? PatientID
                                   )
        {
            if (
                    PatientID == null
               )
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Registration Registration = db.Registration.Find(
                                                 PatientID
                                            );
            if (Registration == null)
            {
                return HttpNotFound();
            }
            return View(Registration);
        }

        // GET: /Registration/Create
        public ActionResult Create()
        {
        // ComboBox
            ViewData["GenderID"] = new SelectList(db.Gender, "GenderID", "Name");

            return View();
        }

        // POST: /Registration/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include=
				           "Name"
				   + "," + "Address"
				   + "," + "NIC"
				   + "," + "GenderID"
				   + "," + "DOB"
				   + "," + "PhoneNo"
				   + "," + "Habits"
				   + "," + "Allergic"
				   + "," + "Weight"
				   + "," + "Height"
				   + "," + "Date"
				  )] Registration Registration)
        {
            if (ModelState.IsValid)
            {
                db.Registration.Add(Registration);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        // ComboBox
            ViewData["GenderID"] = new SelectList(db.Gender, "GenderID", "Name", Registration.GenderID);

            return View(Registration);
        }

        // GET: /Registration/Edit/<id>
        public ActionResult Edit(
                                   Int32? PatientID
                                )
        {
            if (
                    PatientID == null
               )
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Registration Registration = db.Registration.Find(
                                                 PatientID
                                            );
            if (Registration == null)
            {
                return HttpNotFound();
            }
        // ComboBox
            ViewData["GenderID"] = new SelectList(db.Gender, "GenderID", "Name", Registration.GenderID);

            return View(Registration);
        }

        // POST: /Registration/Edit/<id>
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Registration Registration)
        {
            if (ModelState.IsValid)
            {
                db.Entry(Registration).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        // ComboBox
            ViewData["GenderID"] = new SelectList(db.Gender, "GenderID", "Name", Registration.GenderID);

            return View(Registration);
        }

        // GET: /Registration/Delete/<id>
        public ActionResult Delete(
                                     Int32? PatientID
                                  )
        {
            if (
                    PatientID == null
               )
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Registration Registration = db.Registration.Find(
                                                 PatientID
                                            );
            if (Registration == null)
            {
                return HttpNotFound();
            }
            return View(Registration);
        }

        // POST: /Registration/Delete/<id>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(
                                            Int32? PatientID
                                            )
        {
            Registration Registration = db.Registration.Find(
                                                 PatientID
                                            );
            db.Registration.Remove(Registration);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private static List<SelectListItem> GetFields(String select)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            SelectListItem Item1 = new SelectListItem { Text = "Patient I D", Value = "Patient I D" };
            SelectListItem Item2 = new SelectListItem { Text = "Name", Value = "Name" };
            SelectListItem Item3 = new SelectListItem { Text = "Address", Value = "Address" };
            SelectListItem Item4 = new SelectListItem { Text = "N I C", Value = "N I C" };
            SelectListItem Item5 = new SelectListItem { Text = "Gender I D", Value = "Gender I D" };
            SelectListItem Item6 = new SelectListItem { Text = "D O B", Value = "D O B" };
            SelectListItem Item7 = new SelectListItem { Text = "Phone No", Value = "Phone No" };
            SelectListItem Item8 = new SelectListItem { Text = "Habits", Value = "Habits" };
            SelectListItem Item9 = new SelectListItem { Text = "Allergic", Value = "Allergic" };
            SelectListItem Item10 = new SelectListItem { Text = "Weight", Value = "Weight" };
            SelectListItem Item11 = new SelectListItem { Text = "Height", Value = "Height" };
            SelectListItem Item12 = new SelectListItem { Text = "Date", Value = "Date" };

                 if (select == "Patient I D") { Item1.Selected = true; }
            else if (select == "Name") { Item2.Selected = true; }
            else if (select == "Address") { Item3.Selected = true; }
            else if (select == "N I C") { Item4.Selected = true; }
            else if (select == "Gender I D") { Item5.Selected = true; }
            else if (select == "D O B") { Item6.Selected = true; }
            else if (select == "Phone No") { Item7.Selected = true; }
            else if (select == "Habits") { Item8.Selected = true; }
            else if (select == "Allergic") { Item9.Selected = true; }
            else if (select == "Weight") { Item10.Selected = true; }
            else if (select == "Height") { Item11.Selected = true; }
            else if (select == "Date") { Item12.Selected = true; }

            list.Add(Item1);
            list.Add(Item2);
            list.Add(Item3);
            list.Add(Item4);
            list.Add(Item5);
            list.Add(Item6);
            list.Add(Item7);
            list.Add(Item8);
            list.Add(Item9);
            list.Add(Item10);
            list.Add(Item11);
            list.Add(Item12);

            return list.ToList();
        }

        private void ExportData(String Export, GridView gv, DataTable dt)
        {
            if (Export == "Pdf")
            {
                PDFform pdfForm = new PDFform(dt, "Dbo. Registration", "Many");
                Document document = pdfForm.CreateDocument();
                PdfDocumentRenderer renderer = new PdfDocumentRenderer(true);
                renderer.Document = document;
                renderer.RenderDocument();

                MemoryStream stream = new MemoryStream();
                renderer.PdfDocument.Save(stream, false);

                Response.Clear();
                Response.AddHeader("content-disposition", "attachment;filename=" + "Report.pdf");
                Response.ContentType = "application/Pdf.pdf";
                Response.BinaryWrite(stream.ToArray());
                Response.Flush();
                Response.End();
            }
            else
            {
                Response.ClearContent();
                Response.Buffer = true;
                if (Export == "Excel")
                {
                    Response.AddHeader("content-disposition", "attachment;filename=" + "Report.xls");
                    Response.ContentType = "application/Excel.xls";
                }
                else if (Export == "Word")
                {
                    Response.AddHeader("content-disposition", "attachment;filename=" + "Report.doc");
                    Response.ContentType = "application/Word.doc";
                }
                Response.Charset = "";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                gv.RenderControl(htw);
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
            }
        }

    }
}
 
