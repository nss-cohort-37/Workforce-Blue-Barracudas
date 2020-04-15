using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonWorkforce.Models.ViewModels
{
    public class TrainingProgramDetailsViewModel
    {


        [Display(Name = "Id")]
        public int TrainingProgramId { get; set; }

        [Display(Name = "Program Name")]
        public string TrainingProgramName { get; set; }

        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Max Attendees")]
        public int MaxAttendees { get; set; }

        [Display(Name = "Employees")]
        public List<Employee> employees { get; set; }

        [Display(Name = "First Name")]
        public string EmployeeFirstName { get; set; }

        [Display(Name = "Last Name")]
        public string EmployeeLastName { get; set; }

        [Display(Name = "Department")]
        public string EmployeeDeptName { get; set; }


    }
}
