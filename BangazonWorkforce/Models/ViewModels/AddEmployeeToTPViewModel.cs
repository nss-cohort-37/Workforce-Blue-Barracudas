using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonWorkforce.Models.ViewModels
{
    public class AddEmployeeToTPViewModel
    {
        public int TrainingProgramId { get; set; }

        public int EmployeeId { get; set; }


        public List<SelectListItem> TrainingProgramOptions { get; set; }

        public List<SelectListItem> AlreadyInTrainingProgramOptions { get; set; }

        public List<int> TrainingProgramIds { get; set; }

       
         
    }
}