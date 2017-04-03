using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HMSApp.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column(Order = 0)]
        [Required]
        public Int32 UserID { get; set; }

        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [StringLength(50)]
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        public string Password { get; set; }

        [NotMapped]
        public string Message { get; set; }
    }
}