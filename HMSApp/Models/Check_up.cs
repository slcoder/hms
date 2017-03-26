using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace HMSApp.Models
{
    [Table("Check_up")]
    public class Check_up
    {
        [Key]
        [Column(Order = 0)]
        [Required]
        [Display(Name = "Test I D")]
        public Int32 TestID { get; set; }

        [Display(Name = "Patient I D")]
        public Int32? PatientID { get; set; }

        [StringLength(50)]
        [Display(Name = "Test Name")]
        public String TestName { get; set; }

        [StringLength(50)]
        [Display(Name = "O I C")]
        public String OIC { get; set; }

        [Display(Name = "Charges")]
        public Decimal? Charges { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date")]
        public DateTime? Date { get; set; }

        // ComboBox
        public virtual Registration Registration { get; set; }

    }
}
 
