using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BangazonWorkforce.Models;
using BangazonWorkforce.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BangazonWorkforce.Controllers
{
    public class TrainingProgramsController : Controller
    {
        private readonly IConfiguration _config;
        public TrainingProgramsController(IConfiguration config)
        {
            _config = config;
        }
        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        //This functions gets all training programs after the current date and time

        // GET: TrainingPrograms
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name, StartDate, EndDate, MaxAttendees FROM TrainingProgram WHERE StartDate >= GETDATE()";

                    var reader = cmd.ExecuteReader();
                    var trainingPrograms = new List<TrainingProgram>();

                    while (reader.Read())
                    {
                        trainingPrograms.Add(new TrainingProgram()

                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                            MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees"))
                        }
                        );

                    }
                    reader.Close();

                    return View(trainingPrograms);

                }
            }
        }

        //GET: TrainingPrograms/Details/5
        public ActionResult Details(int id)
        {
            var trainingProgram = GetTrainingProgramById(id);
            return View(trainingProgram);
        }

        //This views renders a form to with Name, StartDate, EndDate, and Max Attendees fields 
        //Once created a specific Id should be assigned and you should be redirected to the Index view

        // GET: TrainingPrograms/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TrainingPrograms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TrainingProgram trainingProgram)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO TrainingProgram (Name, StartDate, EndDate, MaxAttendees)
                                        OUTPUT INSERTED.Id
                                        VALUES ( @Name, @StartDate, @EndDate, @MaxAttendees)";

                        cmd.Parameters.Add(new SqlParameter("@Name", trainingProgram.Name));
                        cmd.Parameters.Add(new SqlParameter("@StartDate", trainingProgram.StartDate));
                        cmd.Parameters.Add(new SqlParameter("@EndDate", trainingProgram.EndDate));
                        cmd.Parameters.Add(new SqlParameter("@MaxAttendees", trainingProgram.MaxAttendees));

                        var id = (int)cmd.ExecuteScalar();
                        trainingProgram.Id = id;
                        return RedirectToAction(nameof(Index));
                    }
                }


            }
            catch (Exception ex)
            {
                return View();
            }
        }

     //This allows an edit on Training Progam information ONLY. You can not edit any past programs. This edit does not affect employees enrolled in program

        // GET: TrainingPrograms/Edit/5
        public ActionResult Edit(int id)
        {
            var trainingProgram = GetTrainingProgramById(id);
            var viewModel = new TrainingProgramDetailsViewModel
            {
                TrainingProgramId = trainingProgram.TrainingProgramId,
                TrainingProgramName = trainingProgram.TrainingProgramName,
                StartDate = trainingProgram.StartDate,
                EndDate = trainingProgram.EndDate,
                MaxAttendees = trainingProgram.MaxAttendees

            };
            return View(viewModel);
        }

        // POST: TrainingPrograms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, TrainingProgramDetailsViewModel trainingProgram)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE TrainingProgram
                                            SET Name = @Name,
                                                StartDate = @StartDate,
                                                EndDate = @EndDate,
                                                MaxAttendees = @MaxAttendees
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@Name", trainingProgram.TrainingProgramName));
                        cmd.Parameters.Add(new SqlParameter("@StartDate", trainingProgram.StartDate));
                        cmd.Parameters.Add(new SqlParameter("@EndDate", trainingProgram.EndDate));
                        cmd.Parameters.Add(new SqlParameter("@MaxAttendees", trainingProgram.MaxAttendees));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return RedirectToAction(nameof(Index));
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }


            catch (Exception)
            {
                return View();
            }
        }

        // GET: TrainingPrograms/Delete/5
        public ActionResult Delete(int id)
        {
            var trainingProgram = GetTrainingProgramById(id);
            var viewModel = new TrainingProgramDetailsViewModel
            {
                TrainingProgramId = trainingProgram.TrainingProgramId,
                TrainingProgramName = trainingProgram.TrainingProgramName,
                StartDate = trainingProgram.StartDate,
                EndDate = trainingProgram.EndDate,
                MaxAttendees = trainingProgram.MaxAttendees,
                employees = trainingProgram.employees

            };
            return View(viewModel);
        }

        //In this delete before deleting the training program it must delete employees from "EmployeeTraining" that are connected to training program and then removes the training program
        //This delete does not allow any programs that have already taken place to be deleted, there is no "delete" button that renders for past programs.

        // POST: TrainingPrograms/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, TrainingProgramDetailsViewModel trainingProgram)
        {
            
                DeleteEmployeesBeforeProgram(id);

                try
                {
                    using (SqlConnection conn = Connection)
                    {
                        conn.Open();
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"DELETE FROM TrainingProgram WHERE Id = @id";
                            cmd.Parameters.Add(new SqlParameter("@id", id));

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                     return RedirectToAction(nameof(Index));
                        
                        }
                            throw new Exception("No rows affected");
                        }
                    }
                }
                catch
            {
                return View();
            }
        }
        //This is the function that removes all employees connections to training programs before deleting selected training program
        private void DeleteEmployeesBeforeProgram(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"  DELETE FROM EmployeeTraining WHERE TrainingProgramId = @TrainingProgramId";
                    cmd.Parameters.Add(new SqlParameter("@TrainingProgramId", id));

                    int rowsAffected = cmd.ExecuteNonQuery();
                }
            }

        }
        
        //This gets each training program by ID with the list of employee information
        
        private TrainingProgramDetailsViewModel GetTrainingProgramById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT tp.Id AS TrainingProgramId, tp.Name AS TrainingProgramName, tp.StartDate, tp.EndDate, tp.MaxAttendees, 
                                                e.Id AS EmployeeId, e.FirstName, e.LastName, d.Name AS EmployeeDeptName
                                    FROM TrainingProgram tp 
                                    LEFT JOIN EmployeeTraining et ON et.TrainingProgramId = tp.Id 
                                    LEFT JOIN Employee e ON et.EmployeeId = e.Id 
                                    LEFT JOIN Department d ON e.DepartmentId = d.Id
                                    WHERE tp.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();
                    TrainingProgramDetailsViewModel trainingProgram = null;

                    while (reader.Read())
                    {
                        if (trainingProgram == null)
                        {
                            trainingProgram = new TrainingProgramDetailsViewModel
                            {
                                TrainingProgramId = reader.GetInt32(reader.GetOrdinal("TrainingProgramId")),
                                TrainingProgramName = reader.GetString(reader.GetOrdinal("TrainingProgramName")),
                                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                                MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees")),
                                employees = new List<Employee>()
                            };
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("EmployeeId")))
                        {

                            trainingProgram.employees.Add(new Employee
                            {
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                Department = new Department()
                                {
                                    Name = reader.GetString(reader.GetOrdinal("EmployeeDeptName"))
                                }
                            });
                        }

                    }
                    reader.Close();
                    return trainingProgram;
                }
            }
        }


        //separate VIEW for getting programs before current date and time
        //there is now a "GetPastPrograms.cshtml" file for TrainingProgram Model
        //This will also show the employees who attended the program on the Details link 

        public ActionResult GetPastPrograms()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name, StartDate, EndDate, MaxAttendees FROM TrainingProgram WHERE StartDate <= GETDATE()";

                    var reader = cmd.ExecuteReader();
                    var trainingPrograms = new List<TrainingProgram>();

                    while (reader.Read())
                    {
                        trainingPrograms.Add(new TrainingProgram()

                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                            MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees"))
                        }
                        );

                    }
                    reader.Close();

                    return View(trainingPrograms);

                }
            }
        }
    }
      
    }

    