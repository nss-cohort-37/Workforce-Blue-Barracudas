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
        //COMPUTED PROPERTY FOR THE CONNECTION
        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }


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
            return View();
        }

        // POST: TrainingPrograms/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        private TrainingProgramDetailsViewModel GetTrainingProgramById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT tp.Id AS TrainingProgramId, tp.Name AS TrainingProgramName, tp.StartDate, tp.EndDate, tp.MaxAttendees, e.Id AS EmployeeId, e.FirstName, e.LastName, d.Name AS EmployeeDeptName
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
        private bool TrainingProgramExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, StartDate, EndDate, MaxAttendees
                        FROM TrainingProgram
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
    