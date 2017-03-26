using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace HMSApp.Models
{
    [Table("Prescription")]
    public class Prescription
    {
        [Key]
        [Column(Order = 0)]
        [Required]
        [Display(Name = "Medi I D")]
        public Int32 MediID { get; set; }

        [Display(Name = "Doctor I D")]
        public Int32? DoctorID { get; set; }

        [Display(Name = "Patient I D")]
        public Int32? PatientID { get; set; }

        [StringLength(50)]
        [Display(Name = "Medi Name")]
        public String MediName { get; set; }

        [StringLength(50)]
        [Display(Name = "Dosage")]
        public String Dosage { get; set; }

        [StringLength(50)]
        [Display(Name = "Time Period")]
        public String TimePeriod { get; set; }

        [Display(Name = "No Of Days")]
        public Int32? NoOfDays { get; set; }

        [Display(Name = "Total Payment")]
        public Decimal? TotalPayment { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date")]
        public DateTime? Date { get; set; }

        // ComboBox
        public virtual Doctors Doctors { get; set; }
        public virtual Registration Registration { get; set; }

    }
}
 
