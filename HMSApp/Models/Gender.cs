using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace HMSApp.Models
{
    [Table("Gender")]
    public class Gender
    {
        [Key]
        [Column(Order = 0)]
        [Required]
        [Display(Name = "Gender I D")]
        public Int32 GenderID { get; set; }

        [StringLength(50)]
        [Display(Name = "Name")]
        public String Name { get; set; }

    }
}
 
