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
    public class PrescriptionController : Controller
    {
        private HMSDBDBContext db = new HMSDBDBContext();
        
        // GET: /Prescription/
        public ActionResult Index(string sortOrder,  
                                  String SearchField,
                                  String SearchCondition,
                                  String SearchText,
                                  String Export,
                                  int? PageSize,
                                  int? page, 
                                  string command)
        {

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

            ViewData["SearchFields"] = GetFields((Session["SearchField"] == null ? "Medi I D" : Convert.ToString(Session["SearchField"])));
            ViewData["SearchConditions"] = Library.GetConditions((Session["SearchCondition"] == null ? "Contains" : Convert.ToString(Session["SearchCondition"])));
            ViewData["SearchText"] = Session["SearchText"];
            ViewData["Exports"] = Library.GetExports((Session["Export"] == null ? "Pdf" : Convert.ToString(Session["Export"])));
            ViewData["PageSizes"] = Library.GetPageSizes();

            ViewData["CurrentSort"] = sortOrder;
            ViewData["MediIDSortParm"] = sortOrder == "MediID_asc" ? "MediID_desc" : "MediID_asc";
            ViewData["DoctorIDSortParm"] = sortOrder == "DoctorID_asc" ? "DoctorID_desc" : "DoctorID_asc";
            ViewData["PatientIDSortParm"] = sortOrder == "PatientID_asc" ? "PatientID_desc" : "PatientID_asc";
            ViewData["MediNameSortParm"] = sortOrder == "MediName_asc" ? "MediName_desc" : "MediName_asc";
            ViewData["DosageSortParm"] = sortOrder == "Dosage_asc" ? "Dosage_desc" : "Dosage_asc";
            ViewData["TimePeriodSortParm"] = sortOrder == "TimePeriod_asc" ? "TimePeriod_desc" : "TimePeriod_asc";
            ViewData["NoOfDaysSortParm"] = sortOrder == "NoOfDays_asc" ? "NoOfDays_desc" : "NoOfDays_asc";
            ViewData["TotalPaymentSortParm"] = sortOrder == "TotalPayment_asc" ? "TotalPayment_desc" : "TotalPayment_asc";
            ViewData["DateSortParm"] = sortOrder == "Date_asc" ? "Date_desc" : "Date_asc";

            var Query = db.Prescription.AsQueryable();

            try {
                if (!string.IsNullOrEmpty(Convert.ToString(Session["SearchField"])) && !string.IsNullOrEmpty(Convert.ToString(Session["SearchCondition"])) && !string.IsNullOrEmpty(Convert.ToString(Session["SearchText"])))
                {
                    SearchField = Convert.ToString(Session["SearchField"]);
                    SearchCondition = Convert.ToString(Session["SearchCondition"]);
                    SearchText = Convert.ToString(Session["SearchText"]);

                    if (SearchCondition == "Contains") {
                        Query = Query.Where(p => 
                                                 ("Medi I D".ToString().Equals(SearchField) && p.MediID.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Doctor I D".ToString().Equals(SearchField) && p.Doctors.Name.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Patient I D".ToString().Equals(SearchField) && p.Registration.Name.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Medi Name".ToString().Equals(SearchField) && p.MediName.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Dosage".ToString().Equals(SearchField) && p.Dosage.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Time Period".ToString().Equals(SearchField) && p.TimePeriod.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("No Of Days".ToString().Equals(SearchField) && p.NoOfDays.Value.ToString().Contains(SearchText)) 
                                                 || ("Total Payment".ToString().Equals(SearchField) && p.TotalPayment.Value.ToString().Contains(SearchText)) 
                                                 || ("Date".ToString().Equals(SearchField) && p.Date.Value.ToString().Contains(SearchText)) 
                                         );
                    } else if (SearchCondition == "Starts with...") {
                        Query = Query.Where(p => 
                                                 ("Medi I D".ToString().Equals(SearchField) && p.MediID.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Doctor I D".ToString().Equals(SearchField) && p.Doctors.Name.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Patient I D".ToString().Equals(SearchField) && p.Registration.Name.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Medi Name".ToString().Equals(SearchField) && p.MediName.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Dosage".ToString().Equals(SearchField) && p.Dosage.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Time Period".ToString().Equals(SearchField) && p.TimePeriod.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("No Of Days".ToString().Equals(SearchField) && p.NoOfDays.Value.ToString().StartsWith(SearchText)) 
                                                 || ("Total Payment".ToString().Equals(SearchField) && p.TotalPayment.Value.ToString().StartsWith(SearchText)) 
                                                 || ("Date".ToString().Equals(SearchField) && p.Date.Value.ToString().StartsWith(SearchText)) 
                                         );
                    } else if (SearchCondition == "Equals") {
                        if ("Medi I D".Equals(SearchField)) { var mMediID = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.MediID == mMediID); }
                        else if ("Doctor I D".Equals(SearchField)) { var mDoctorID = System.Convert.ToString(SearchText); Query = Query.Where(p => p.Doctors.Name == mDoctorID); }
                        else if ("Patient I D".Equals(SearchField)) { var mPatientID = System.Convert.ToString(SearchText); Query = Query.Where(p => p.Registration.Name == mPatientID); }
                        else if ("Medi Name".Equals(SearchField)) { var mMediName = System.Convert.ToString(SearchText); Query = Query.Where(p => p.MediName == mMediName); }
                        else if ("Dosage".Equals(SearchField)) { var mDosage = System.Convert.ToString(SearchText); Query = Query.Where(p => p.Dosage == mDosage); }
                        else if ("Time Period".Equals(SearchField)) { var mTimePeriod = System.Convert.ToString(SearchText); Query = Query.Where(p => p.TimePeriod == mTimePeriod); }
                        else if ("No Of Days".Equals(SearchField)) { var mNoOfDays = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.NoOfDays == mNoOfDays); }
                        else if ("Total Payment".Equals(SearchField)) { var mTotalPayment = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.TotalPayment == mTotalPayment); }
                        else if ("Date".Equals(SearchField)) { var mDate = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Date == mDate); }
                    } else if (SearchCondition == "More than...") { 
                        if (SearchField.Equals(SearchCondition)) { }
                        else if ("Medi I D".Equals(SearchField)) { var mMediID = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.MediID > mMediID); }
                        else if ("No Of Days".Equals(SearchField)) { var mNoOfDays = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.NoOfDays > mNoOfDays); }
                        else if ("Total Payment".Equals(SearchField)) { var mTotalPayment = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.TotalPayment > mTotalPayment); }
                        else if ("Date".Equals(SearchField)) { var mDate = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Date > mDate); }
                    } else if (SearchCondition == "Less than...") { 
                        if (SearchField.Equals(SearchCondition)) { }
                        else if ("Medi I D".Equals(SearchField)) { var mMediID = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.MediID < mMediID); }
                        else if ("No Of Days".Equals(SearchField)) { var mNoOfDays = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.NoOfDays < mNoOfDays); }
                        else if ("Total Payment".Equals(SearchField)) { var mTotalPayment = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.TotalPayment < mTotalPayment); }
                        else if ("Date".Equals(SearchField)) { var mDate = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Date < mDate); }
                    } else if (SearchCondition == "Equal or more than...") { 
                        if (SearchField.Equals(SearchCondition)) { }
                        else if ("Medi I D".Equals(SearchField)) { var mMediID = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.MediID >= mMediID); }
                        else if ("No Of Days".Equals(SearchField)) { var mNoOfDays = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.NoOfDays >= mNoOfDays); }
                        else if ("Total Payment".Equals(SearchField)) { var mTotalPayment = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.TotalPayment >= mTotalPayment); }
                        else if ("Date".Equals(SearchField)) { var mDate = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Date >= mDate); }
                    } else if (SearchCondition == "Equal or less than...") { 
                        if (SearchField.Equals(SearchCondition)) { }
                        else if ("Medi I D".Equals(SearchField)) { var mMediID = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.MediID <= mMediID); }
                        else if ("No Of Days".Equals(SearchField)) { var mNoOfDays = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.NoOfDays <= mNoOfDays); }
                        else if ("Total Payment".Equals(SearchField)) { var mTotalPayment = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.TotalPayment <= mTotalPayment); }
                        else if ("Date".Equals(SearchField)) { var mDate = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Date <= mDate); }
                    }
                }
            } catch (Exception) { }

            switch (sortOrder)
            {
                case "MediID_desc":
                    Query = Query.OrderByDescending(s => s.MediID);
                    break;
                case "MediID_asc":
                    Query = Query.OrderBy(s => s.MediID);
                    break;
                case "DoctorID_desc":
                    Query = Query.OrderByDescending(s => s.Doctors.Name);
                    break;
                case "DoctorID_asc":
                    Query = Query.OrderBy(s => s.Doctors.Name);
                    break;
                case "PatientID_desc":
                    Query = Query.OrderByDescending(s => s.Registration.Name);
                    break;
                case "PatientID_asc":
                    Query = Query.OrderBy(s => s.Registration.Name);
                    break;
                case "MediName_desc":
                    Query = Query.OrderByDescending(s => s.MediName);
                    break;
                case "MediName_asc":
                    Query = Query.OrderBy(s => s.MediName);
                    break;
                case "Dosage_desc":
                    Query = Query.OrderByDescending(s => s.Dosage);
                    break;
                case "Dosage_asc":
                    Query = Query.OrderBy(s => s.Dosage);
                    break;
                case "TimePeriod_desc":
                    Query = Query.OrderByDescending(s => s.TimePeriod);
                    break;
                case "TimePeriod_asc":
                    Query = Query.OrderBy(s => s.TimePeriod);
                    break;
                case "NoOfDays_desc":
                    Query = Query.OrderByDescending(s => s.NoOfDays);
                    break;
                case "NoOfDays_asc":
                    Query = Query.OrderBy(s => s.NoOfDays);
                    break;
                case "TotalPayment_desc":
                    Query = Query.OrderByDescending(s => s.TotalPayment);
                    break;
                case "TotalPayment_asc":
                    Query = Query.OrderBy(s => s.TotalPayment);
                    break;
                case "Date_desc":
                    Query = Query.OrderByDescending(s => s.Date);
                    break;
                case "Date_asc":
                    Query = Query.OrderBy(s => s.Date);
                    break;
                default:  // Name ascending 
                    Query = Query.OrderBy(s => s.MediID);
                    break;
            }

            if (command == "Export") {
                GridView gv = new GridView();
                DataTable dt = new DataTable();
                dt.Columns.Add("Medi I D", typeof(string));
                dt.Columns.Add("Doctor I D", typeof(string));
                dt.Columns.Add("Patient I D", typeof(string));
                dt.Columns.Add("Medi Name", typeof(string));
                dt.Columns.Add("Dosage", typeof(string));
                dt.Columns.Add("Time Period", typeof(string));
                dt.Columns.Add("No Of Days", typeof(string));
                dt.Columns.Add("Total Payment", typeof(string));
                dt.Columns.Add("Date", typeof(string));
                foreach (var item in Query.ToList())
                {
                    dt.Rows.Add(
                        item.MediID
                       ,item.Doctors.Name
                       ,item.Registration.Name
                       ,item.MediName
                       ,item.Dosage
                       ,item.TimePeriod
                       ,item.NoOfDays
                       ,item.TotalPayment
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

        // GET: /Prescription/Details/<id>
        public ActionResult Details(
                                      Int32? MediID
                                   )
        {
            if (
                    MediID == null
               )
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Prescription Prescription = db.Prescription.Find(
                                                 MediID
                                            );
            if (Prescription == null)
            {
                return HttpNotFound();
            }
            return View(Prescription);
        }

        // GET: /Prescription/Create
        public ActionResult Create()
        {
        // ComboBox
            ViewData["DoctorID"] = new SelectList(db.Doctors, "DoctorID", "Name");
            ViewData["PatientID"] = new SelectList(db.Registration, "PatientID", "Name");

            return View();
        }

        // POST: /Prescription/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include=
				           "DoctorID"
				   + "," + "PatientID"
				   + "," + "MediName"
				   + "," + "Dosage"
				   + "," + "TimePeriod"
				   + "," + "NoOfDays"
				   + "," + "TotalPayment"
				   + "," + "Date"
				  )] Prescription Prescription)
        {
            if (ModelState.IsValid)
            {
                db.Prescription.Add(Prescription);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        // ComboBox
            ViewData["DoctorID"] = new SelectList(db.Doctors, "DoctorID", "Name", Prescription.DoctorID);
            ViewData["PatientID"] = new SelectList(db.Registration, "PatientID", "Name", Prescription.PatientID);

            return View(Prescription);
        }

        // GET: /Prescription/Edit/<id>
        public ActionResult Edit(
                                   Int32? MediID
                                )
        {
            if (
                    MediID == null
               )
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Prescription Prescription = db.Prescription.Find(
                                                 MediID
                                            );
            if (Prescription == null)
            {
                return HttpNotFound();
            }
        // ComboBox
            ViewData["DoctorID"] = new SelectList(db.Doctors, "DoctorID", "Name", Prescription.DoctorID);
            ViewData["PatientID"] = new SelectList(db.Registration, "PatientID", "Name", Prescription.PatientID);

            return View(Prescription);
        }

        // POST: /Prescription/Edit/<id>
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Prescription Prescription)
        {
            if (ModelState.IsValid)
            {
                db.Entry(Prescription).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        // ComboBox
            ViewData["DoctorID"] = new SelectList(db.Doctors, "DoctorID", "Name", Prescription.DoctorID);
            ViewData["PatientID"] = new SelectList(db.Registration, "PatientID", "Name", Prescription.PatientID);

            return View(Prescription);
        }

        // GET: /Prescription/Delete/<id>
        public ActionResult Delete(
                                     Int32? MediID
                                  )
        {
            if (
                    MediID == null
               )
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Prescription Prescription = db.Prescription.Find(
                                                 MediID
                                            );
            if (Prescription == null)
            {
                return HttpNotFound();
            }
            return View(Prescription);
        }

        // POST: /Prescription/Delete/<id>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(
                                            Int32? MediID
                                            )
        {
            Prescription Prescription = db.Prescription.Find(
                                                 MediID
                                            );
            db.Prescription.Remove(Prescription);
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
            SelectListItem Item1 = new SelectListItem { Text = "Medi I D", Value = "Medi I D" };
            SelectListItem Item2 = new SelectListItem { Text = "Doctor I D", Value = "Doctor I D" };
            SelectListItem Item3 = new SelectListItem { Text = "Patient I D", Value = "Patient I D" };
            SelectListItem Item4 = new SelectListItem { Text = "Medi Name", Value = "Medi Name" };
            SelectListItem Item5 = new SelectListItem { Text = "Dosage", Value = "Dosage" };
            SelectListItem Item6 = new SelectListItem { Text = "Time Period", Value = "Time Period" };
            SelectListItem Item7 = new SelectListItem { Text = "No Of Days", Value = "No Of Days" };
            SelectListItem Item8 = new SelectListItem { Text = "Total Payment", Value = "Total Payment" };
            SelectListItem Item9 = new SelectListItem { Text = "Date", Value = "Date" };

                 if (select == "Medi I D") { Item1.Selected = true; }
            else if (select == "Doctor I D") { Item2.Selected = true; }
            else if (select == "Patient I D") { Item3.Selected = true; }
            else if (select == "Medi Name") { Item4.Selected = true; }
            else if (select == "Dosage") { Item5.Selected = true; }
            else if (select == "Time Period") { Item6.Selected = true; }
            else if (select == "No Of Days") { Item7.Selected = true; }
            else if (select == "Total Payment") { Item8.Selected = true; }
            else if (select == "Date") { Item9.Selected = true; }

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
                PDFform pdfForm = new PDFform(dt, "Dbo. Prescription", "Many");
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
 
