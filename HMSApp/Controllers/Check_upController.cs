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
    public class Check_upController : Controller
    {
        private HMSDBDBContext db = new HMSDBDBContext();
        
        // GET: /Check_up/
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

            ViewData["SearchFields"] = GetFields((Session["SearchField"] == null ? "Test I D" : Convert.ToString(Session["SearchField"])));
            ViewData["SearchConditions"] = Library.GetConditions((Session["SearchCondition"] == null ? "Contains" : Convert.ToString(Session["SearchCondition"])));
            ViewData["SearchText"] = Session["SearchText"];
            ViewData["Exports"] = Library.GetExports((Session["Export"] == null ? "Pdf" : Convert.ToString(Session["Export"])));
            ViewData["PageSizes"] = Library.GetPageSizes();

            ViewData["CurrentSort"] = sortOrder;
            ViewData["TestIDSortParm"] = sortOrder == "TestID_asc" ? "TestID_desc" : "TestID_asc";
            ViewData["PatientIDSortParm"] = sortOrder == "PatientID_asc" ? "PatientID_desc" : "PatientID_asc";
            ViewData["TestNameSortParm"] = sortOrder == "TestName_asc" ? "TestName_desc" : "TestName_asc";
            ViewData["OICSortParm"] = sortOrder == "OIC_asc" ? "OIC_desc" : "OIC_asc";
            ViewData["ChargesSortParm"] = sortOrder == "Charges_asc" ? "Charges_desc" : "Charges_asc";
            ViewData["DateSortParm"] = sortOrder == "Date_asc" ? "Date_desc" : "Date_asc";

            var Query = db.Check_up.AsQueryable();

            try {
                if (!string.IsNullOrEmpty(Convert.ToString(Session["SearchField"])) && !string.IsNullOrEmpty(Convert.ToString(Session["SearchCondition"])) && !string.IsNullOrEmpty(Convert.ToString(Session["SearchText"])))
                {
                    SearchField = Convert.ToString(Session["SearchField"]);
                    SearchCondition = Convert.ToString(Session["SearchCondition"]);
                    SearchText = Convert.ToString(Session["SearchText"]);

                    if (SearchCondition == "Contains") {
                        Query = Query.Where(p => 
                                                 ("Test I D".ToString().Equals(SearchField) && p.TestID.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Patient I D".ToString().Equals(SearchField) && p.Registration.Name.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Test Name".ToString().Equals(SearchField) && p.TestName.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("O I C".ToString().Equals(SearchField) && p.OIC.ToString().Trim().ToLower().Contains(SearchText.Trim().ToLower())) 
                                                 || ("Charges".ToString().Equals(SearchField) && p.Charges.Value.ToString().Contains(SearchText)) 
                                                 || ("Date".ToString().Equals(SearchField) && p.Date.Value.ToString().Contains(SearchText)) 
                                         );
                    } else if (SearchCondition == "Starts with...") {
                        Query = Query.Where(p => 
                                                 ("Test I D".ToString().Equals(SearchField) && p.TestID.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Patient I D".ToString().Equals(SearchField) && p.Registration.Name.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Test Name".ToString().Equals(SearchField) && p.TestName.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("O I C".ToString().Equals(SearchField) && p.OIC.ToString().Trim().ToLower().StartsWith(SearchText.Trim().ToLower())) 
                                                 || ("Charges".ToString().Equals(SearchField) && p.Charges.Value.ToString().StartsWith(SearchText)) 
                                                 || ("Date".ToString().Equals(SearchField) && p.Date.Value.ToString().StartsWith(SearchText)) 
                                         );
                    } else if (SearchCondition == "Equals") {
                        if ("Test I D".Equals(SearchField)) { var mTestID = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.TestID == mTestID); }
                        else if ("Patient I D".Equals(SearchField)) { var mPatientID = System.Convert.ToString(SearchText); Query = Query.Where(p => p.Registration.Name == mPatientID); }
                        else if ("Test Name".Equals(SearchField)) { var mTestName = System.Convert.ToString(SearchText); Query = Query.Where(p => p.TestName == mTestName); }
                        else if ("O I C".Equals(SearchField)) { var mOIC = System.Convert.ToString(SearchText); Query = Query.Where(p => p.OIC == mOIC); }
                        else if ("Charges".Equals(SearchField)) { var mCharges = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.Charges == mCharges); }
                        else if ("Date".Equals(SearchField)) { var mDate = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Date == mDate); }
                    } else if (SearchCondition == "More than...") { 
                        if (SearchField.Equals(SearchCondition)) { }
                        else if ("Test I D".Equals(SearchField)) { var mTestID = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.TestID > mTestID); }
                        else if ("Charges".Equals(SearchField)) { var mCharges = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.Charges > mCharges); }
                        else if ("Date".Equals(SearchField)) { var mDate = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Date > mDate); }
                    } else if (SearchCondition == "Less than...") { 
                        if (SearchField.Equals(SearchCondition)) { }
                        else if ("Test I D".Equals(SearchField)) { var mTestID = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.TestID < mTestID); }
                        else if ("Charges".Equals(SearchField)) { var mCharges = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.Charges < mCharges); }
                        else if ("Date".Equals(SearchField)) { var mDate = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Date < mDate); }
                    } else if (SearchCondition == "Equal or more than...") { 
                        if (SearchField.Equals(SearchCondition)) { }
                        else if ("Test I D".Equals(SearchField)) { var mTestID = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.TestID >= mTestID); }
                        else if ("Charges".Equals(SearchField)) { var mCharges = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.Charges >= mCharges); }
                        else if ("Date".Equals(SearchField)) { var mDate = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Date >= mDate); }
                    } else if (SearchCondition == "Equal or less than...") { 
                        if (SearchField.Equals(SearchCondition)) { }
                        else if ("Test I D".Equals(SearchField)) { var mTestID = System.Convert.ToInt32(SearchText); Query = Query.Where(p => p.TestID <= mTestID); }
                        else if ("Charges".Equals(SearchField)) { var mCharges = System.Convert.ToDecimal(SearchText); Query = Query.Where(p => p.Charges <= mCharges); }
                        else if ("Date".Equals(SearchField)) { var mDate = System.Convert.ToDateTime(SearchText); Query = Query.Where(p => p.Date <= mDate); }
                    }
                }
            } catch (Exception) { }

            switch (sortOrder)
            {
                case "TestID_desc":
                    Query = Query.OrderByDescending(s => s.TestID);
                    break;
                case "TestID_asc":
                    Query = Query.OrderBy(s => s.TestID);
                    break;
                case "PatientID_desc":
                    Query = Query.OrderByDescending(s => s.Registration.Name);
                    break;
                case "PatientID_asc":
                    Query = Query.OrderBy(s => s.Registration.Name);
                    break;
                case "TestName_desc":
                    Query = Query.OrderByDescending(s => s.TestName);
                    break;
                case "TestName_asc":
                    Query = Query.OrderBy(s => s.TestName);
                    break;
                case "OIC_desc":
                    Query = Query.OrderByDescending(s => s.OIC);
                    break;
                case "OIC_asc":
                    Query = Query.OrderBy(s => s.OIC);
                    break;
                case "Charges_desc":
                    Query = Query.OrderByDescending(s => s.Charges);
                    break;
                case "Charges_asc":
                    Query = Query.OrderBy(s => s.Charges);
                    break;
                case "Date_desc":
                    Query = Query.OrderByDescending(s => s.Date);
                    break;
                case "Date_asc":
                    Query = Query.OrderBy(s => s.Date);
                    break;
                default:  // Name ascending 
                    Query = Query.OrderBy(s => s.TestID);
                    break;
            }

            if (command == "Export") {
                GridView gv = new GridView();
                DataTable dt = new DataTable();
                dt.Columns.Add("Test I D", typeof(string));
                dt.Columns.Add("Patient I D", typeof(string));
                dt.Columns.Add("Test Name", typeof(string));
                dt.Columns.Add("O I C", typeof(string));
                dt.Columns.Add("Charges", typeof(string));
                dt.Columns.Add("Date", typeof(string));
                foreach (var item in Query.ToList())
                {
                    dt.Rows.Add(
                        item.TestID
                       ,item.Registration.Name
                       ,item.TestName
                       ,item.OIC
                       ,item.Charges
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

        // GET: /Check_up/Details/<id>
        public ActionResult Details(
                                      Int32? TestID
                                   )
        {
            if (
                    TestID == null
               )
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Check_up Check_up = db.Check_up.Find(
                                                 TestID
                                            );
            if (Check_up == null)
            {
                return HttpNotFound();
            }
            return View(Check_up);
        }

        // GET: /Check_up/Create
        public ActionResult Create()
        {
        // ComboBox
            ViewData["PatientID"] = new SelectList(db.Registration, "PatientID", "Name");

            return View();
        }

        // POST: /Check_up/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include=
				           "PatientID"
				   + "," + "TestName"
				   + "," + "OIC"
				   + "," + "Charges"
				   + "," + "Date"
				  )] Check_up Check_up)
        {
            if (ModelState.IsValid)
            {
                db.Check_up.Add(Check_up);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        // ComboBox
            ViewData["PatientID"] = new SelectList(db.Registration, "PatientID", "Name", Check_up.PatientID);

            return View(Check_up);
        }

        // GET: /Check_up/Edit/<id>
        public ActionResult Edit(
                                   Int32? TestID
                                )
        {
            if (
                    TestID == null
               )
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Check_up Check_up = db.Check_up.Find(
                                                 TestID
                                            );
            if (Check_up == null)
            {
                return HttpNotFound();
            }
        // ComboBox
            ViewData["PatientID"] = new SelectList(db.Registration, "PatientID", "Name", Check_up.PatientID);

            return View(Check_up);
        }

        // POST: /Check_up/Edit/<id>
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Check_up Check_up)
        {
            if (ModelState.IsValid)
            {
                db.Entry(Check_up).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        // ComboBox
            ViewData["PatientID"] = new SelectList(db.Registration, "PatientID", "Name", Check_up.PatientID);

            return View(Check_up);
        }

        // GET: /Check_up/Delete/<id>
        public ActionResult Delete(
                                     Int32? TestID
                                  )
        {
            if (
                    TestID == null
               )
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Check_up Check_up = db.Check_up.Find(
                                                 TestID
                                            );
            if (Check_up == null)
            {
                return HttpNotFound();
            }
            return View(Check_up);
        }

        // POST: /Check_up/Delete/<id>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(
                                            Int32? TestID
                                            )
        {
            Check_up Check_up = db.Check_up.Find(
                                                 TestID
                                            );
            db.Check_up.Remove(Check_up);
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
            SelectListItem Item1 = new SelectListItem { Text = "Test I D", Value = "Test I D" };
            SelectListItem Item2 = new SelectListItem { Text = "Patient I D", Value = "Patient I D" };
            SelectListItem Item3 = new SelectListItem { Text = "Test Name", Value = "Test Name" };
            SelectListItem Item4 = new SelectListItem { Text = "O I C", Value = "O I C" };
            SelectListItem Item5 = new SelectListItem { Text = "Charges", Value = "Charges" };
            SelectListItem Item6 = new SelectListItem { Text = "Date", Value = "Date" };

                 if (select == "Test I D") { Item1.Selected = true; }
            else if (select == "Patient I D") { Item2.Selected = true; }
            else if (select == "Test Name") { Item3.Selected = true; }
            else if (select == "O I C") { Item4.Selected = true; }
            else if (select == "Charges") { Item5.Selected = true; }
            else if (select == "Date") { Item6.Selected = true; }

            list.Add(Item1);
            list.Add(Item2);
            list.Add(Item3);
            list.Add(Item4);
            list.Add(Item5);
            list.Add(Item6);

            return list.ToList();
        }

        private void ExportData(String Export, GridView gv, DataTable dt)
        {
            if (Export == "Pdf")
            {
                PDFform pdfForm = new PDFform(dt, "Dbo. Check Up", "Many");
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
 
