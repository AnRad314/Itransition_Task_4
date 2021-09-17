using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Itransition_Task_4.Models
{
	public class ApplicationUser: IdentityUser
	{		
		public string LoginUser { get; set; }

		[DataType(DataType.Date)]
		public DateTime DataLastVisit { get; set; }

		[DataType(DataType.Date)]
		public DateTime DataRegistarion { get; set; }
	}
}
