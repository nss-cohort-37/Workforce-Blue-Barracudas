﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonWorkforce.Models.ViewModels
{
    public class EmployeeDetailsViewModel
    {
        public int Id { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public Computer Computer { get; set; }

        public Department Department { get; set; }

        public List<TrainingProgram> TrainingPrograms { get; set; }

        public TrainingProgram trainingProgram { get; set; }
    }
}
