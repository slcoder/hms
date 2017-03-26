using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace HMSApp.Models
{
    [Table("Doctors")]
    public class Doctors
    {
        [Key]
        [Column(Order = 0)]
        [Required]
        [Display(Name = "Doctor I D")]
        public Int32 DoctorID { get; set; }

        [StringLength(50)]
        [Display(Name = "Name")]
        public String Name { get; set; }

        [Display(Name = "Phone No")]
        public Int32? PhoneNo { get; set; }

        [StringLength(50)]
        [Display(Name = "Specialized Area")]
        public String SpecializedArea { get; set; }

        [StringLength(50)]
        [Display(Name = "Hospital")]
        public String Hospital { get; set; }

        [Display(Name = "Gender I D")]
        public Int32? GenderID { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Consultant Day")]
        public DateTime? ConsultantDay { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Time")]
        public DateTime? Time { get; set; }

        [Display(Name = "Channeling Fee")]
        public Decimal? ChannelingFee { get; set; }

        // ComboBox
        public virtual Gender Gender { get; set; }

    }
}
 
