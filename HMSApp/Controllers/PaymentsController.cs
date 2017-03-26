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
    public class PaymentsController : Controller
    {
        private HMSDBDBContext db = new HMSDBDBContext();
        
        // GET: /Payments/
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

            ViewData["SearchFields"] = GetFields((Session["SearchField"] == null ? "Pay Code" : Convert.ToString(Session["SearchField"])));
            ViewData["SearchConditions"] = Library.GetConditions((Session["SearchCondition"] == null ? "Contains" : Convert.ToString(Session["SearchCondition"])));
            ViewData["SearchText"] = Session["SearchText"];
            ViewData["Exports"] = Library.GetExports((Session["Export"] == null ? "Pdf" : Convert.ToString(Session["Export"])));
            ViewData["PageSizes"] = Library.GetPageSizes();

            ViewData["CurrentSort"] = sortOrder;
            ViewData["PayCodeSortParm"] = sortOrder == "PayCode_asc" ? "PayCode_desc" : "PayCode_asc";
            ViewData["PatientIDSortParm"] = sortOrder == "PatientID_asc" ? "PatientID_desc" : "PatientID_asc";
            ViewData["DescriptionSortParm"] = sortOrder == "Description_asc" ? "Description_desc" : "Description_asc";
            ViewData["DoctorFeeSortParm"] = sortOrder == "DoctorFee_asc" ? "DoctorFee_desc" : "DoctorFee_asc";
            ViewData["PrescriptionFeeSortParm"] = sortOrder == "PrescriptionFee_asc" ? "PrescriptionFee_desc" : "PrescriptionFee_asc";
            ViewData["CheckupFeeSortParm"] = sortOrder == "CheckupFee_asc" ? "CheckupFee_desc" : "CheckupFee_asc";
            ViewData["HospitalFeeSortParm"] = sortOrder == "HospitalFee_asc" ? "HospitalFee_desc" : "HospitalFee_asc";
            ViewData["TotalSortParm"] = sortOrder == "Total_asc" ? "Total_desc" : "Total_asc";
            ViewData["DateSortParm"] = sortOrder == "Date_asc" ? "Date_desc" : "Date_asc";

            var Query = db.Payments.AsQueryable();

            try {
                if (!string.IsNullOrEmpty(Convert.ToString(Session["SearchField"])) && !string.IsNullOrEmpty(Convert.ToString(Session["SearchCondition"])) && !string.IsNullOrEmpty(Convert.ToString(Session["SearchText"])))
                {
                    SearchField = Convert.ToString(Session["SearchField"]);
                    SearchCondition = Convert.ToString(Session["SearchCondition"]);
                    SearchText = Convert.ToString(Session["SearchText"]);

                    if (SearchCondition == "Contains") {
                        Query = Query.Where(p => 
                                                 ("Pay Code".ToString().Equals(SearchField) && p.PayCode.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Patient I D".ToString().Equals(SearchField) && p.Registration.Name.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Description".ToString().Equals(SearchField) && p.Description.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Doctor Fee".ToString().Equals(SearchField) && p.DoctorFee.Value.ToString().Contains(SearchText)) 
                                                 || ("Prescription Fee".ToString().Equals(SearchField) && p.PrescriptionFee.Value.ToString().Contains(SearchText)) 
                                                 || ("Checkup Fee".ToString().Equals(SearchField) && p.CheckupFee.Value.ToString().Contains(SearchText)) 
                                                 || ("Hospital Fee".ToString().Equals(SearchField) && p.HospitalFee.Value.ToString().Contains(SearchText)) 
                                                 || ("Total".ToString().Equals(SearchField) && p.Total.Value.ToString().Contains(SearchText)) 
                                                 || ("Date".ToString().Equals(SearchField) && p.Date.Value.ToString().Contains(SearchText)) 
                                         );
                    } else if (SearchCondition == "Starts with...") {
                        Query = Query.Where(p => 
                                                 ("Pay Code".ToString().Equals(SearchField) && p.PayCode.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Patient I D".ToString().Equals(SearchField) && p.Registration.Name.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Description".ToString().Equals(SearchField) && p.Description.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Doctor Fee".ToString().Equals(SearchField) && p.DoctorFee.Value.ToString().StartsWith(SearchText)) 
                                                 || ("Prescription Fee".ToString().Equals(SearchField) && p.PrescriptionFee.Value.ToString().StartsWith(SearchText)) 
                                                 || ("Checkup Fee".ToString().Equals(SearchField) && p.CheckupFee.Value.ToString().StartsWith(SearchText)) 
                                                 || ("Hospital Fee".ToString().Equals(SearchField) && p.HospitalFee.Value.ToString().StartsWith(SearchText)) 
                                                 || ("Total".ToString().Equals(SearchField) && p.Total.Value.ToString().StartsWith(SearchText)) 
                                                 || ("Date".ToString().Equals(SearchField) && p.Date.Value.ToString().StartsWith(SearchText)) 
                                         );
                    } else if (SearchCondition == "Equals") {
                        if ("Pay Code".Equals(SearchField)) { var mPayCode = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.PayCode == mPayCode); }
                        else if ("Patient I D".Equals(SearchField)) { var mPatientID = System.Convert.ToString(SearchText); Query = Query.Where(p => p.Registration.Name == mPatientID); }
                        else if ("Description".Equals(SearchField)) { var mDescription = System.Convert.ToString(SearchText); Query = Query.Where(p => p.Description == mDescription); }
                        else if ("Doctor Fee".Equals(SearchField)) { var mDoctorFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.DoctorFee == mDoctorFee); }
                        else if ("Prescription Fee".Equals(SearchField)) { var mPrescriptionFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.PrescriptionFee == mPrescriptionFee); }
                        else if ("Checkup Fee".Equals(SearchField)) { var mCheckupFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.CheckupFee == mCheckupFee); }
                        else if ("Hospital Fee".Equals(SearchField)) { var mHospitalFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.HospitalFee == mHospitalFee); }
                        else if ("Total".Equals(SearchField)) { var mTotal = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.Total == mTotal); }
                        else if ("Date".Equals(SearchField)) { var mDate = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Date == mDate); }
                    } else if (SearchCondition == "More than...") { 
                        if (SearchField.Equals(SearchCondition)) { }
                        else if ("Pay Code".Equals(SearchField)) { var mPayCode = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.PayCode > mPayCode); }
                        else if ("Doctor Fee".Equals(SearchField)) { var mDoctorFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.DoctorFee > mDoctorFee); }
                        else if ("Prescription Fee".Equals(SearchField)) { var mPrescriptionFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.PrescriptionFee > mPrescriptionFee); }
                        else if ("Checkup Fee".Equals(SearchField)) { var mCheckupFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.CheckupFee > mCheckupFee); }
                        else if ("Hospital Fee".Equals(SearchField)) { var mHospitalFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.HospitalFee > mHospitalFee); }
                        else if ("Total".Equals(SearchField)) { var mTotal = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.Total > mTotal); }
                        else if ("Date".Equals(SearchField)) { var mDate = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Date > mDate); }
                    } else if (SearchCondition == "Less than...") { 
                        if (SearchField.Equals(SearchCondition)) { }
                        else if ("Pay Code".Equals(SearchField)) { var mPayCode = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.PayCode < mPayCode); }
                        else if ("Doctor Fee".Equals(SearchField)) { var mDoctorFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.DoctorFee < mDoctorFee); }
                        else if ("Prescription Fee".Equals(SearchField)) { var mPrescriptionFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.PrescriptionFee < mPrescriptionFee); }
                        else if ("Checkup Fee".Equals(SearchField)) { var mCheckupFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.CheckupFee < mCheckupFee); }
                        else if ("Hospital Fee".Equals(SearchField)) { var mHospitalFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.HospitalFee < mHospitalFee); }
                        else if ("Total".Equals(SearchField)) { var mTotal = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.Total < mTotal); }
                        else if ("Date".Equals(SearchField)) { var mDate = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Date < mDate); }
                    } else if (SearchCondition == "Equal or more than...") { 
                        if (SearchField.Equals(SearchCondition)) { }
                        else if ("Pay Code".Equals(SearchField)) { var mPayCode = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.PayCode >= mPayCode); }
                        else if ("Doctor Fee".Equals(SearchField)) { var mDoctorFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.DoctorFee >= mDoctorFee); }
                        else if ("Prescription Fee".Equals(SearchField)) { var mPrescriptionFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.PrescriptionFee >= mPrescriptionFee); }
                        else if ("Checkup Fee".Equals(SearchField)) { var mCheckupFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.CheckupFee >= mCheckupFee); }
                        else if ("Hospital Fee".Equals(SearchField)) { var mHospitalFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.HospitalFee >= mHospitalFee); }
                        else if ("Total".Equals(SearchField)) { var mTotal = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.Total >= mTotal); }
                        else if ("Date".Equals(SearchField)) { var mDate = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Date >= mDate); }
                    } else if (SearchCondition == "Equal or less than...") { 
                        if (SearchField.Equals(SearchCondition)) { }
                        else if ("Pay Code".Equals(SearchField)) { var mPayCode = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.PayCode <= mPayCode); }
                        else if ("Doctor Fee".Equals(SearchField)) { var mDoctorFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.DoctorFee <= mDoctorFee); }
                        else if ("Prescription Fee".Equals(SearchField)) { var mPrescriptionFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.PrescriptionFee <= mPrescriptionFee); }
                        else if ("Checkup Fee".Equals(SearchField)) { var mCheckupFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.CheckupFee <= mCheckupFee); }
                        else if ("Hospital Fee".Equals(SearchField)) { var mHospitalFee = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.HospitalFee <= mHospitalFee); }
                        else if ("Total".Equals(SearchField)) { var mTotal = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.Total <= mTotal); }
                        else if ("Date".Equals(SearchField)) { var mDate = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Date <= mDate); }
                    }
                }
            } catch (Exception) { }

            switch (sortOrder)
            {
                case "PayCode_desc":
                    Query = Query.OrderByDescending(s => s.PayCode);
                    break;
                case "PayCode_asc":
                    Query = Query.OrderBy(s => s.PayCode);
                    break;
                case "PatientID_desc":
                    Query = Query.OrderByDescending(s => s.Registration.Name);
                    break;
                case "PatientID_asc":
                    Query = Query.OrderBy(s => s.Registration.Name);
                    break;
                case "Description_desc":
                    Query = Query.OrderByDescending(s => s.Description);
                    break;
                case "Description_asc":
                    Query = Query.OrderBy(s => s.Description);
                    break;
                case "DoctorFee_desc":
                    Query = Query.OrderByDescending(s => s.DoctorFee);
                    break;
                case "DoctorFee_asc":
                    Query = Query.OrderBy(s => s.DoctorFee);
                    break;
                case "PrescriptionFee_desc":
                    Query = Query.OrderByDescending(s => s.PrescriptionFee);
                    break;
                case "PrescriptionFee_asc":
                    Query = Query.OrderBy(s => s.PrescriptionFee);
                    break;
                case "CheckupFee_desc":
                    Query = Query.OrderByDescending(s => s.CheckupFee);
                    break;
                case "CheckupFee_asc":
                    Query = Query.OrderBy(s => s.CheckupFee);
                    break;
                case "HospitalFee_desc":
                    Query = Query.OrderByDescending(s => s.HospitalFee);
                    break;
                case "HospitalFee_asc":
                    Query = Query.OrderBy(s => s.HospitalFee);
                    break;
                case "Total_desc":
                    Query = Query.OrderByDescending(s => s.Total);
                    break;
                case "Total_asc":
                    Query = Query.OrderBy(s => s.Total);
                    break;
                case "Date_desc":
                    Query = Query.OrderByDescending(s => s.Date);
                    break;
                case "Date_asc":
                    Query = Query.OrderBy(s => s.Date);
                    break;
                default:  // Name ascending 
                    Query = Query.OrderBy(s => s.PayCode);
                    break;
            }

            if (command == "Export") {
                GridView gv = new GridView();
                DataTable dt = new DataTable();
                dt.Columns.Add("Pay Code", typeof(string));
                dt.Columns.Add("Patient I D", typeof(string));
                dt.Columns.Add("Description", typeof(string));
                dt.Columns.Add("Doctor Fee", typeof(string));
                dt.Columns.Add("Prescription Fee", typeof(string));
                dt.Columns.Add("Checkup Fee", typeof(string));
                dt.Columns.Add("Hospital Fee", typeof(string));
                dt.Columns.Add("Total", typeof(string));
                dt.Columns.Add("Date", typeof(string));
                foreach (var item in Query.ToList())
                {
                    dt.Rows.Add(
                        item.PayCode
                       ,item.Registration.Name
                       ,item.Description
                       ,item.DoctorFee
                       ,item.PrescriptionFee
                       ,item.CheckupFee
                       ,item.HospitalFee
                       ,item.Total
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

        // GET: /Payments/Details/<id>
        public ActionResult Details(
                                      Int32? PayCode
                                   )
        {
            if (
                    PayCode == null
               )
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payments Payments = db.Payments.Find(
                                                 PayCode
                                            );
            if (Payments == null)
            {
                return HttpNotFound();
            }
            return View(Payments);
        }

        // GET: /Payments/Create
        public ActionResult Create()
        {
        // ComboBox
            ViewData["PatientID"] = new SelectList(db.Registration, "PatientID", "Name");

            return View();
        }

        // POST: /Payments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include=
				           "PatientID"
				   + "," + "Description"
				   + "," + "DoctorFee"
				   + "," + "PrescriptionFee"
				   + "," + "CheckupFee"
				   + "," + "HospitalFee"
				   + "," + "Total"
				   + "," + "Date"
				  )] Payments Payments)
        {
            if (ModelState.IsValid)
            {
                db.Payments.Add(Payments);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        // ComboBox
            ViewData["PatientID"] = new SelectList(db.Registration, "PatientID", "Name", Payments.PatientID);

            return View(Payments);
        }

        // GET: /Payments/Edit/<id>
        public ActionResult Edit(
                                   Int32? PayCode
                                )
        {
            if (
                    PayCode == null
               )
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payments Payments = db.Payments.Find(
                                                 PayCode
                                            );
            if (Payments == null)
            {
                return HttpNotFound();
            }
        // ComboBox
            ViewData["PatientID"] = new SelectList(db.Registration, "PatientID", "Name", Payments.PatientID);

            return View(Payments);
        }

        // POST: /Payments/Edit/<id>
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Payments Payments)
        {
            if (ModelState.IsValid)
            {
                db.Entry(Payments).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        // ComboBox
            ViewData["PatientID"] = new SelectList(db.Registration, "PatientID", "Name", Payments.PatientID);

            return View(Payments);
        }

        // GET: /Payments/Delete/<id>
        public ActionResult Delete(
                                     Int32? PayCode
                                  )
        {
            if (
                    PayCode == null
               )
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payments Payments = db.Payments.Find(
                                                 PayCode
                                            );
            if (Payments == null)
            {
                return HttpNotFound();
            }
            return View(Payments);
        }

        // POST: /Payments/Delete/<id>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(
                                            Int32? PayCode
                                            )
        {
            Payments Payments = db.Payments.Find(
                                                 PayCode
                                            );
            db.Payments.Remove(Payments);
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
            SelectListItem Item1 = new SelectListItem { Text = "Pay Code", Value = "Pay Code" };
            SelectListItem Item2 = new SelectListItem { Text = "Patient I D", Value = "Patient I D" };
            SelectListItem Item3 = new SelectListItem { Text = "Description", Value = "Description" };
            SelectListItem Item4 = new SelectListItem { Text = "Doctor Fee", Value = "Doctor Fee" };
            SelectListItem Item5 = new SelectListItem { Text = "Prescription Fee", Value = "Prescription Fee" };
            SelectListItem Item6 = new SelectListItem { Text = "Checkup Fee", Value = "Checkup Fee" };
            SelectListItem Item7 = new SelectListItem { Text = "Hospital Fee", Value = "Hospital Fee" };
            SelectListItem Item8 = new SelectListItem { Text = "Total", Value = "Total" };
            SelectListItem Item9 = new SelectListItem { Text = "Date", Value = "Date" };

                 if (select == "Pay Code") { Item1.Selected = true; }
            else if (select == "Patient I D") { Item2.Selected = true; }
            else if (select == "Description") { Item3.Selected = true; }
            else if (select == "Doctor Fee") { Item4.Selected = true; }
            else if (select == "Prescription Fee") { Item5.Selected = true; }
            else if (select == "Checkup Fee") { Item6.Selected = true; }
            else if (select == "Hospital Fee") { Item7.Selected = true; }
            else if (select == "Total") { Item8.Selected = true; }
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
                PDFform pdfForm = new PDFform(dt, "Dbo. Payments", "Many");
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
 
