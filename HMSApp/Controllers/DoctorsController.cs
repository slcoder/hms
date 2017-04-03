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
    public class DoctorsController : Controller
    {
        private HMSDBDBContext db = new HMSDBDBContext();
        
        // GET: /Doctors/
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

            ViewData["SearchFields"] = GetFields((Session["SearchField"] == null ? "Doctor I D" : Convert.ToString(Session["SearchField"])));
            ViewData["SearchConditions"] = Library.GetConditions((Session["SearchCondition"] == null ? "Contains" : Convert.ToString(Session["SearchCondition"])));
            ViewData["SearchText"] = Session["SearchText"];
            ViewData["Exports"] = Library.GetExports((Session["Export"] == null ? "Pdf" : Convert.ToString(Session["Export"])));
            ViewData["PageSizes"] = Library.GetPageSizes();

            ViewData["CurrentSort"] = sortOrder;
            ViewData["DoctorIDSortParm"] = sortOrder == "DoctorID_asc" ? "DoctorID_desc" : "DoctorID_asc";
            ViewData["NameSortParm"] = sortOrder == "Name_asc" ? "Name_desc" : "Name_asc";
            ViewData["PhoneNoSortParm"] = sortOrder == "PhoneNo_asc" ? "PhoneNo_desc" : "PhoneNo_asc";
            ViewData["SpecializedAreaSortParm"] = sortOrder == "SpecializedArea_asc" ? "SpecializedArea_desc" : "SpecializedArea_asc";
            ViewData["HospitalSortParm"] = sortOrder == "Hospital_asc" ? "Hospital_desc" : "Hospital_asc";
            ViewData["GenderIDSortParm"] = sortOrder == "GenderID_asc" ? "GenderID_desc" : "GenderID_asc";
            ViewData["ConsultantDaySortParm"] = sortOrder == "ConsultantDay_asc" ? "ConsultantDay_desc" : "ConsultantDay_asc";
            ViewData["TimeSortParm"] = sortOrder == "Time_asc" ? "Time_desc" : "Time_asc";
            ViewData["ChannelingFeeSortParm"] = sortOrder == "ChannelingFee_asc" ? "ChannelingFee_desc" : "ChannelingFee_asc";

            var Query = db.Doctors.AsQueryable();

            try {
                if (!string.IsNullOrEmpty(Convert.ToString(Session["SearchField"])) && !string.IsNullOrEmpty(Convert.ToString(Session["SearchCondition"])) && !string.IsNullOrEmpty(Convert.ToString(Session["SearchText"])))
                {
                    SearchField = Convert.ToString(Session["SearchField"]);
                    SearchCondition = Convert.ToString(Session["SearchCondition"]);
                    SearchText = Convert.ToString(Session["SearchText"]);

                    if (SearchCondition == "Contains") {
                        Query = Query.Where(p => 
                                                 ("Doctor I D".ToString().Equals(SearchField) && p.DoctorID.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Name".ToString().Equals(SearchField) && p.Name.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Phone No".ToString().Equals(SearchField) && p.PhoneNo.Value.ToString().Contains(SearchText)) 
                                                 || ("Specialized Area".ToString().Equals(SearchField) && p.SpecializedArea.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Hospital".ToString().Equals(SearchField) && p.Hospital.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Gender I D".ToString().Equals(SearchField) && p.Gender.Name.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Consultant Day".ToString().Equals(SearchField) && p.ConsultantDay.Value.ToString().Contains(SearchText)) 
                                                 || ("Time".ToString().Equals(SearchField) && p.Time.Value.ToString().Contains(SearchText)) 
                                                 || ("Channeling Fee".ToString().Equals(SearchField) && p.ChannelingFee.Value.ToString().Contains(SearchText)) 
                                         );
                    } else if (SearchCondition == "Starts with...") {
                        Query = Query.Where(p => 
                                                 ("Doctor I D".ToString().Equals(SearchField) && p.DoctorID.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Name".ToString().Equals(SearchField) && p.Name.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Phone No".ToString().Equals(SearchField) && p.PhoneNo.Value.ToString().StartsWith(SearchText)) 
                                                 || ("Specialized Area".ToString().Equals(SearchField) && p.SpecializedArea.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Hospital".ToString().Equals(SearchField) && p.Hospital.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Gender I D".ToString().Equals(SearchField) && p.Gender.Name.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Consultant Day".ToString().Equals(SearchField) && p.ConsultantDay.Value.ToString().StartsWith(SearchText)) 
                                                 || ("Time".ToString().Equals(SearchField) && p.Time.Value.ToString().StartsWith(SearchText)) 
                                                 || ("Channeling Fee".ToString().Equals(SearchField) && p.ChannelingFee.Value.ToString().StartsWith(SearchText)) 
                                         );
                    } else if (SearchCondition == "Equals") {
                        if ("Doctor I D".Equals(SearchField)) { var mDoctorID = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.DoctorID == mDoctorID); }
                        else if ("Name".Equals(SearchField)) { var mName = System.Convert.ToString(SearchText); Query = Query.Where(p => p.Name == mName); }
                        else if ("Phone No".Equals(SearchField)) { var mPhoneNo = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.PhoneNo == mPhoneNo); }
                        else if ("Specialized Area".Equals(SearchField)) { var mSpecializedArea = System.Convert.ToString(SearchText); Query = Query.Where(p => p.SpecializedArea == mSpecializedArea); }
                        else if ("Hospital".Equals(SearchField)) { var mHospital = System.Convert.ToString(SearchText); Query = Query.Where(p => p.Hospital == mHospital); }
                        else if ("Gender I D".Equals(SearchField)) { var mGenderID = System.Convert.ToString(SearchText); Query = Query.Where(p => p.Gender.Name == mGenderID); }
                        else if ("Consultant Day".Equals(SearchField)) { var mConsultantDay = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.ConsultantDay == mConsultantDay); }
                        else if ("Time".Equals(SearchField)) { var mTime = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Time == mTime); }
                        else if ("Channeling Fee".Equals(SearchField)) { var mChannelingFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.ChannelingFee == mChannelingFee); }
                    } else if (SearchCondition == "More than...") { 
                        if (SearchField.Equals(SearchCondition)) { }
                        else if ("Doctor I D".Equals(SearchField)) { var mDoctorID = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.DoctorID > mDoctorID); }
                        else if ("Phone No".Equals(SearchField)) { var mPhoneNo = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.PhoneNo > mPhoneNo); }
                        else if ("Consultant Day".Equals(SearchField)) { var mConsultantDay = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.ConsultantDay > mConsultantDay); }
                        else if ("Time".Equals(SearchField)) { var mTime = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Time > mTime); }
                        else if ("Channeling Fee".Equals(SearchField)) { var mChannelingFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.ChannelingFee > mChannelingFee); }
                    } else if (SearchCondition == "Less than...") { 
                        if (SearchField.Equals(SearchCondition)) { }
                        else if ("Doctor I D".Equals(SearchField)) { var mDoctorID = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.DoctorID < mDoctorID); }
                        else if ("Phone No".Equals(SearchField)) { var mPhoneNo = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.PhoneNo < mPhoneNo); }
                        else if ("Consultant Day".Equals(SearchField)) { var mConsultantDay = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.ConsultantDay < mConsultantDay); }
                        else if ("Time".Equals(SearchField)) { var mTime = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Time < mTime); }
                        else if ("Channeling Fee".Equals(SearchField)) { var mChannelingFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.ChannelingFee < mChannelingFee); }
                    } else if (SearchCondition == "Equal or more than...") { 
                        if (SearchField.Equals(SearchCondition)) { }
                        else if ("Doctor I D".Equals(SearchField)) { var mDoctorID = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.DoctorID >= mDoctorID); }
                        else if ("Phone No".Equals(SearchField)) { var mPhoneNo = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.PhoneNo >= mPhoneNo); }
                        else if ("Consultant Day".Equals(SearchField)) { var mConsultantDay = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.ConsultantDay >= mConsultantDay); }
                        else if ("Time".Equals(SearchField)) { var mTime = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Time >= mTime); }
                        else if ("Channeling Fee".Equals(SearchField)) { var mChannelingFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.ChannelingFee >= mChannelingFee); }
                    } else if (SearchCondition == "Equal or less than...") { 
                        if (SearchField.Equals(SearchCondition)) { }
                        else if ("Doctor I D".Equals(SearchField)) { var mDoctorID = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.DoctorID <= mDoctorID); }
                        else if ("Phone No".Equals(SearchField)) { var mPhoneNo = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.PhoneNo <= mPhoneNo); }
                        else if ("Consultant Day".Equals(SearchField)) { var mConsultantDay = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.ConsultantDay <= mConsultantDay); }
                        else if ("Time".Equals(SearchField)) { var mTime = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Time <= mTime); }
                        else if ("Channeling Fee".Equals(SearchField)) { var mChannelingFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.ChannelingFee <= mChannelingFee); }
                    }
                }
            } catch (Exception) { }

            switch (sortOrder)
            {
                case "DoctorID_desc":
                    Query = Query.OrderByDescending(s => s.DoctorID);
                    break;
                case "DoctorID_asc":
                    Query = Query.OrderBy(s => s.DoctorID);
                    break;
                case "Name_desc":
                    Query = Query.OrderByDescending(s => s.Name);
                    break;
                case "Name_asc":
                    Query = Query.OrderBy(s => s.Name);
                    break;
                case "PhoneNo_desc":
                    Query = Query.OrderByDescending(s => s.PhoneNo);
                    break;
                case "PhoneNo_asc":
                    Query = Query.OrderBy(s => s.PhoneNo);
                    break;
                case "SpecializedArea_desc":
                    Query = Query.OrderByDescending(s => s.SpecializedArea);
                    break;
                case "SpecializedArea_asc":
                    Query = Query.OrderBy(s => s.SpecializedArea);
                    break;
                case "Hospital_desc":
                    Query = Query.OrderByDescending(s => s.Hospital);
                    break;
                case "Hospital_asc":
                    Query = Query.OrderBy(s => s.Hospital);
                    break;
                case "GenderID_desc":
                    Query = Query.OrderByDescending(s => s.Gender.Name);
                    break;
                case "GenderID_asc":
                    Query = Query.OrderBy(s => s.Gender.Name);
                    break;
                case "ConsultantDay_desc":
                    Query = Query.OrderByDescending(s => s.ConsultantDay);
                    break;
                case "ConsultantDay_asc":
                    Query = Query.OrderBy(s => s.ConsultantDay);
                    break;
                case "Time_desc":
                    Query = Query.OrderByDescending(s => s.Time);
                    break;
                case "Time_asc":
                    Query = Query.OrderBy(s => s.Time);
                    break;
                case "ChannelingFee_desc":
                    Query = Query.OrderByDescending(s => s.ChannelingFee);
                    break;
                case "ChannelingFee_asc":
                    Query = Query.OrderBy(s => s.ChannelingFee);
                    break;
                default:  // Name ascending 
                    Query = Query.OrderBy(s => s.DoctorID);
                    break;
            }

            if (command == "Export") {
                GridView gv = new GridView();
                DataTable dt = new DataTable();
                dt.Columns.Add("Doctor I D", typeof(string));
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("Phone No", typeof(string));
                dt.Columns.Add("Specialized Area", typeof(string));
                dt.Columns.Add("Hospital", typeof(string));
                dt.Columns.Add("Gender I D", typeof(string));
                dt.Columns.Add("Consultant Day", typeof(string));
                dt.Columns.Add("Time", typeof(string));
                dt.Columns.Add("Channeling Fee", typeof(string));
                foreach (var item in Query.ToList())
                {
                    dt.Rows.Add(
                        item.DoctorID
                       ,item.Name
                       ,item.PhoneNo
                       ,item.SpecializedArea
                       ,item.Hospital
                       ,item.Gender.Name
                       ,item.ConsultantDay
                       ,item.Time
                       ,item.ChannelingFee
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

        // GET: /Doctors/Details/<id>
        public ActionResult Details(
                                      Int32? DoctorID
                                   )
        {
            if (
                    DoctorID == null
               )
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Doctors Doctors = db.Doctors.Find(
                                                 DoctorID
                                            );
            if (Doctors == null)
            {
                return HttpNotFound();
            }
            return View(Doctors);
        }

        // GET: /Doctors/Create
        public ActionResult Create()
        {
        // ComboBox
            ViewData["GenderID"] = new SelectList(db.Gender, "GenderID", "Name");

            return View();
        }

        // POST: /Doctors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include=
				           "Name"
				   + "," + "PhoneNo"
				   + "," + "SpecializedArea"
				   + "," + "Hospital"
				   + "," + "GenderID"
				   + "," + "ConsultantDay"
				   + "," + "Time"
				   + "," + "ChannelingFee"
				  )] Doctors Doctors)
        {
            if (ModelState.IsValid)
            {
                db.Doctors.Add(Doctors);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        // ComboBox
            ViewData["GenderID"] = new SelectList(db.Gender, "GenderID", "Name", Doctors.GenderID);

            return View(Doctors);
        }

        // GET: /Doctors/Edit/<id>
        public ActionResult Edit(
                                   Int32? DoctorID
                                )
        {
            if (
                    DoctorID == null
               )
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Doctors Doctors = db.Doctors.Find(
                                                 DoctorID
                                            );
            if (Doctors == null)
            {
                return HttpNotFound();
            }
        // ComboBox
            ViewData["GenderID"] = new SelectList(db.Gender, "GenderID", "Name", Doctors.GenderID);

            return View(Doctors);
        }

        // POST: /Doctors/Edit/<id>
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Doctors Doctors)
        {
            if (ModelState.IsValid)
            {
                db.Entry(Doctors).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        // ComboBox
            ViewData["GenderID"] = new SelectList(db.Gender, "GenderID", "Name", Doctors.GenderID);

            return View(Doctors);
        }

        // GET: /Doctors/Delete/<id>
        public ActionResult Delete(
                                     Int32? DoctorID
                                  )
        {
            if (
                    DoctorID == null
               )
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Doctors Doctors = db.Doctors.Find(
                                                 DoctorID
                                            );
            if (Doctors == null)
            {
                return HttpNotFound();
            }
            return View(Doctors);
        }

        // POST: /Doctors/Delete/<id>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(
                                            Int32? DoctorID
                                            )
        {
            Doctors Doctors = db.Doctors.Find(
                                                 DoctorID
                                            );
            db.Doctors.Remove(Doctors);
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
            SelectListItem Item1 = new SelectListItem { Text = "Doctor I D", Value = "Doctor I D" };
            SelectListItem Item2 = new SelectListItem { Text = "Name", Value = "Name" };
            SelectListItem Item3 = new SelectListItem { Text = "Phone No", Value = "Phone No" };
            SelectListItem Item4 = new SelectListItem { Text = "Specialized Area", Value = "Specialized Area" };
            SelectListItem Item5 = new SelectListItem { Text = "Hospital", Value = "Hospital" };
            SelectListItem Item6 = new SelectListItem { Text = "Gender I D", Value = "Gender I D" };
            SelectListItem Item7 = new SelectListItem { Text = "Consultant Day", Value = "Consultant Day" };
            SelectListItem Item8 = new SelectListItem { Text = "Time", Value = "Time" };
            SelectListItem Item9 = new SelectListItem { Text = "Channeling Fee", Value = "Channeling Fee" };

                 if (select == "Doctor I D") { Item1.Selected = true; }
            else if (select == "Name") { Item2.Selected = true; }
            else if (select == "Phone No") { Item3.Selected = true; }
            else if (select == "Specialized Area") { Item4.Selected = true; }
            else if (select == "Hospital") { Item5.Selected = true; }
            else if (select == "Gender I D") { Item6.Selected = true; }
            else if (select == "Consultant Day") { Item7.Selected = true; }
            else if (select == "Time") { Item8.Selected = true; }
            else if (select == "Channeling Fee") { Item9.Selected = true; }

            list.Add(Item1);
            list.Add(Item2);
            list.Add(Item3);
            list.Add(Item4);
            list.Add(Item5);
            list.Add(Item6);
            list.Add(Item7);
            list.Add(Item8);
            list.Add(Item9);

            return list.ToList();
        }

        private void ExportData(String Export, GridView gv, DataTable dt)
        {
            if (Export == "Pdf")
            {
                PDFform pdfForm = new PDFform(dt, "Dbo. Doctors", "Many");
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
 
