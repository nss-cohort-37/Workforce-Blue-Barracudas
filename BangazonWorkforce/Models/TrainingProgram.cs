using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonWorkforce.Models
{
    public class TrainingProgram
    {
        public int? Id { get; set; }

        [Display(Name = "Program Name")]
        public string Name { get; set; }

        [Display(Name = "Start Date")]
        [DisplayFormat(DataFormatString = "{0:dddd, MMMM dd, yyyy}")]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "{0:dddd, MMMM dd, yyyy}")]
        public DateTime EndDate { get; set; }


        [Display(Name = "Max Attendees")]
        public int MaxAttendees { get; set; }
        public List<Employee> Employees { get; set; }


  }
}
