using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace HMSApp.Models
{
    [Table("Registration")]
    public class Registration
    {
        [Key]
        [Column(Order = 0)]
        [Required]
        [Display(Name = "Patient I D")]
        public Int32 PatientID { get; set; }

        [StringLength(50)]
        [Display(Name = "Name")]
        public String Name { get; set; }

        [StringLength(50)]
        [Display(Name = "Address")]
        public String Address { get; set; }

        [StringLength(12)]
        [Display(Name = "N I C")]
        public String NIC { get; set; }

        [Display(Name = "Gender I D")]
        public Int32? GenderID { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "D O B")]
        public DateTime? DOB { get; set; }

        [Display(Name = "Phone No")]
        public Int32? PhoneNo { get; set; }

        [StringLength(50)]
        [Display(Name = "Habits")]
        public String Habits { get; set; }

        [StringLength(50)]
        [Display(Name = "Allergic")]
        public String Allergic { get; set; }

        [Display(Name = "Weight")]
        public Decimal? Weight { get; set; }

        [Display(Name = "Height")]
        public Decimal? Height { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date")]
        public DateTime? Date { get; set; }

        // ComboBox
        public virtual Gender Gender { get; set; }

    }
}
 
