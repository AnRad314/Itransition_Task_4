using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Itransition_Task_4.ViewModel
{
	public class UsersViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Login")]
        public string LoginName { get; set; }
        public string Provider { get; set; }

        [Display(Name = "Registration Date")]       
        public DateTime DataRegistration { get; set; }

        [Display(Name = "Date of last visit ")]        
        public DateTime DataLastVisit { get; set; }

        [Display(Name = "Status")]    
        public string Islockedout { get; set; }
    }
}
