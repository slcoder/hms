using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace HMSApp.Models
{
    [Table("Payments")]
    public class Payments
    {
        [Key]
        [Column(Order = 0)]
        [Required]
        [Display(Name = "Pay Code")]
        public Int32 PayCode { get; set; }

        [Display(Name = "Patient I D")]
        public Int32? PatientID { get; set; }

        [StringLength(50)]
        [Display(Name = "Description")]
        public String Description { get; set; }

        [Display(Name = "Doctor Fee")]
        public Decimal? DoctorFee { get; set; }

        [Display(Name = "Prescription Fee")]
        public Decimal? PrescriptionFee { get; set; }

        [Display(Name = "Checkup Fee")]
        public Decimal? CheckupFee { get; set; }

        [Display(Name = "Hospital Fee")]
        public Decimal? HospitalFee { get; set; }

        [Display(Name = "Total")]
        public Decimal? Total { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date")]
        public DateTime? Date { get; set; }

        // ComboBox
        public virtual Registration Registration { get; set; }

    }
}
 
