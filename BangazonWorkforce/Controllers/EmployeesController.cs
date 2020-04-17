using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using BangazonWorkforce.Models;
using BangazonWorkforce.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BangazonWorkforce.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly IConfiguration _config;

        public EmployeesController(IConfiguration config)
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

        // GET: Employees
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id, e.FirstName, e.LastName, e.DepartmentId, e.Email, e.IsSupervisor, e.ComputerId, d.Id AS DeptId, d.Name FROM Employee e
                                      LEFT JOIN Department d ON d.Id = e.DepartmentId";

                    var reader = cmd.ExecuteReader();
                    var employees = new List<Models.Employee>();

                    while (reader.Read())
                    {
                        var employee = new Employee()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                            ComputerId = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                            Department = new Department()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DeptId")),
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            }
                        };
                        employees.Add(employee);
                    }
                    reader.Close();
                    return View(employees);
                }
            }

        }
    

        // GET: Employees/Details/5
        public ActionResult Details(int id)
        {
            var programs = GetTrainingPrograms(id);
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id AS EmployeesId, e.FirstName, e.LastName, e.DepartmentId, t.Id AS TrainingProgramsId, t.Name, c.Id AS ComputersId, c.Make, c.Model, d.Name AS DepartmentName, t.StartDate
                                    FROM Employee e
                                    LEFT JOIN EmployeeTraining p ON e.Id = p.EmployeeId
                                    LEFT JOIN TrainingProgram t ON t.Id = p.TrainingProgramId
                                    LEFT JOIN Department d ON d.Id = e.DepartmentId
                                    LEFT JOIN Computer c on c.Id = e.ComputerId
                                    WHERE e.Id = @Id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();
                    EmployeeDetailsViewModel employee = null;

                    if (reader.Read())
                    {
                        employee = new EmployeeDetailsViewModel()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("EmployeesId")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            Computer = new Computer()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ComputersId")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Model = reader.GetString(reader.GetOrdinal("Model"))
                            },
                            Department = new Department()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                Name = reader.GetString(reader.GetOrdinal("DepartmentName"))
                            },
                            TrainingPrograms = programs, 
                            trainingProgram = new TrainingProgram()
                            {
                                StartDate= reader.GetDateTime(reader.GetOrdinal("StartDate"))
                            }
                        };
                    }
                    reader.Close();
                    return View(employee);
                }
            };
        }

        // GET: Employees/Create
        public ActionResult Create()
        {
            var departmentOptions = GetDepartmentOptions();
            var computerOptions = GetAvailableComputers();
            var viewModel = new EmployeeEditViewModel()
            {
                DepartmentOptions = departmentOptions,
                ComputerOptions = computerOptions
            };
            return View(viewModel);
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EmployeeEditViewModel employee)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Employee (FirstName, LastName, Email, IsSupervisor, DepartmentId, ComputerId)
                                            OUTPUT INSERTED.Id
                                            VALUES (@firstName, @lastName, @email, @isSupervisor, @departmentId, @computerId)";

                        cmd.Parameters.Add(new SqlParameter("@firstName", employee.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", employee.LastName));
                        cmd.Parameters.Add(new SqlParameter("@email", employee.Email));
                        cmd.Parameters.Add(new SqlParameter("@isSupervisor", employee.IsSupervisor));
                        cmd.Parameters.Add(new SqlParameter("@departmentId", employee.DepartmentId));
                        cmd.Parameters.Add(new SqlParameter("@computerId", employee.ComputerId));
                        
               


                        var id = (int)cmd.ExecuteScalar();
                        employee.EmployeeId = id;

                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        // GET: Employees/Edit/5
        public ActionResult Edit(int id)
        {
            var employee = GetEmployeeById(id);
            var departmentOptions = GetDepartmentOptions();
            var computerOptions = AvailableAndAssignedComputers(id);
            var viewModel = new EmployeeEditViewModel()
            {
                EmployeeId = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                DepartmentId = employee.DepartmentId,
                ComputerId = employee.ComputerId,
                IsSupervisor = employee.IsSupervisor,
                DepartmentOptions = departmentOptions,
                ComputerOptions = computerOptions
            };
            return View(viewModel);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [FromForm] EmployeeEditViewModel employee)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Employee 
                                            SET FirstName = @firstName, 
                                                LastName = @lastName, 
                                                Email = @email,
                                                DepartmentId = @departmentId,
                                                ComputerId = @computerId,
                                                IsSupervisor = @isSupervisor
                                                
                                            WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@firstName", employee.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", employee.LastName));
                        cmd.Parameters.Add(new SqlParameter("@email", employee.Email));
                        cmd.Parameters.Add(new SqlParameter("@departmentId", employee.DepartmentId));
                        cmd.Parameters.Add(new SqlParameter("@computerId", employee.ComputerId));
                        cmd.Parameters.Add(new SqlParameter("@isSupervisor", employee.IsSupervisor));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        var rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected < 1)
                        {
                            return NotFound();
                        }
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        // GET: Employees/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Employees/Delete/5
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

        // GET: Employees/AssignTP
        public ActionResult Assign(int id)
        {
            var employee = GetEmployeeById(id);
            var trainingProgramOptions = GetAvailableTrainingPrograms(id);
            var registeredTrainingPrograms= GetAssignedTrainingPrograms(id);
            trainingProgramOptions.AddRange(registeredTrainingPrograms); 
            var viewModel = new AddEmployeeToTPViewModel()
            {
                EmployeeId = employee.Id,
                TrainingProgramOptions = trainingProgramOptions,
                TrainingProgramIds = GetTrainingProgramIds(id)


            };
            return View(viewModel);
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Assign(int id, [FromForm] AddEmployeeToTPViewModel employeeTraining )
        {
            foreach (var item in employeeTraining.TrainingProgramIds)
            {

                try
                {
                    using (SqlConnection conn = Connection)
                    {
                        conn.Open();
                        using (SqlCommand cmd = conn.CreateCommand())
                        {

                            cmd.CommandText = @"INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId)
                                            OUTPUT INSERTED.Id
                                            VALUES (@employeeId, @trainingProgramId)";

                            cmd.Parameters.Add(new SqlParameter("@employeeId", id));
                            cmd.Parameters.Add(new SqlParameter("@trainingProgramId", item));


                            var EtId = (int)cmd.ExecuteScalar();

                            //employeeTraining.TrainingProgramIds.Add(employeeTraining.TrainingProgramId);

                           
                        }
                    }
                }
                catch (Exception ex)
                {
                    return View();
                }
            }
            return RedirectToAction(nameof(Index));

        }
        private List<SelectListItem> GetAssignedTrainingPrograms(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT tp.[Name], tp.Id
                               FROM TrainingProgram tp
                               WHERE Id IN (SELECT TrainingProgramId FROM EmployeeTraining WHERE EmployeeTraining.EmployeeId = @id)
                               AND tp.StartDate >= GETDATE()
                                         ";

                    cmd.Parameters.Add(new SqlParameter("@id", id));


                    var reader = cmd.ExecuteReader();

                    var options = new List<SelectListItem>();


                    while (reader.Read())
                    {
                        var option = new SelectListItem()
                        {
                            Text = reader.GetString(reader.GetOrdinal("Name")),
                            Value = reader.GetInt32(reader.GetOrdinal("Id")).ToString(), 
                            Disabled = true
                        };

                        options.Add(option);

                    }
                    reader.Close();
                    return options;
                }
            }
        }

        private List<int> GetTrainingProgramIds(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @" SELECT tp.[Name], tp.Id
                               FROM TrainingProgram tp
                               WHERE Id IN (SELECT TrainingProgramId FROM EmployeeTraining WHERE EmployeeTraining.EmployeeId = @id)
                               AND tp.StartDate >= GETDATE()
                                        ";


                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();

                    List<int> trainingProgramIds = new List<int>(); 

                    while (reader.Read())
                    {
                        trainingProgramIds.Add(reader.GetInt32(reader.GetOrdinal("Id"))); 

                    }
                    reader.Close();
                    return trainingProgramIds; 

                }
            }
        }

        private List<SelectListItem> GetAvailableTrainingPrograms(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @" SELECT tp.[Name], tp.Id
                               FROM TrainingProgram tp
                               WHERE Id NOT IN (SELECT TrainingProgramId FROM EmployeeTraining WHERE EmployeeTraining.EmployeeId = @id)
                               AND tp.StartDate >= GETDATE()
                                        ";


                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();
                    var options = new List<SelectListItem>();

                    options.Add(new SelectListItem()
                    {
                        Text = "Select a training program",
                        Value = null
                    });

                    while (reader.Read())
                    {
                        var option = new SelectListItem()
                        {
                            Text = reader.GetString(reader.GetOrdinal("Name")),
                            Value = reader.GetInt32(reader.GetOrdinal("Id")).ToString(), 
                           
                        };

                        options.Add(option);

                    }
                    reader.Close();
                    return options;
                }
            }
        }

        private List<TrainingProgram> GetTrainingPrograms(int EmployeeId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT p.TrainingProgramId, t.Name AS TrainingName
                                    FROM EmployeeTraining p
                                    LEFT JOIN TrainingProgram t ON t.Id = p.TrainingProgramId
                                    WHERE p.EmployeeId = @EmployeeId";
                    cmd.Parameters.Add(new SqlParameter("@Employeeid", EmployeeId));

                    var reader = cmd.ExecuteReader();
                    var programs = new List<TrainingProgram>();

                    while (reader.Read())
                    {
                        if(!reader.IsDBNull(reader.GetOrdinal("TrainingProgramId")))
                        {

                            var program = new TrainingProgram()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("TrainingProgramId")),
                                Name = reader.GetString(reader.GetOrdinal("TrainingName")), 
                           
                            };

                            programs.Add(program);

                        } else
                        {
                            var program = new TrainingProgram()
                            {
                                Id = null,
                                Name = null
                            };
                        }

                    }
                    reader.Close();
                    return programs;
                }
            }
        }

        private List<SelectListItem> GetDepartmentOptions()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name FROM Department";

                    var reader = cmd.ExecuteReader();
                    var options = new List<SelectListItem>();

                    while (reader.Read())
                    {
                        var option = new SelectListItem()
                        {
                            Text = reader.GetString(reader.GetOrdinal("Name")),
                            Value = reader.GetInt32(reader.GetOrdinal("Id")).ToString()
                        };

                        options.Add(option);

                    }
                    reader.Close();
                    return options;
                }
            }
        }

        private List<SelectListItem> GetAvailableComputers()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT * FROM Computer
                                      WHERE Id NOT IN (SELECT ComputerId FROM Employee)
                                      AND DecomissionDate IS NULL";

                    var reader = cmd.ExecuteReader();
                    var options = new List<SelectListItem>();

                    options.Add(new SelectListItem()
                    {
                        Text = "Assign a computer if you want!",
                        Value = null
                    });

                    while (reader.Read())
                    {
                        var option = new SelectListItem()
                        {
                            Text = reader.GetString(reader.GetOrdinal("Model")),
                            Value = reader.GetInt32(reader.GetOrdinal("Id")).ToString()
                        };

                        options.Add(option);

                    }
                    reader.Close();
                    return options;
                }
            }
        }

        private List<SelectListItem> AvailableAndAssignedComputers(int EmpId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id, c.Make, c.Model, e.FirstName, COALESCE( FirstName + ' ' + Model, 'Available Computer ' + Model) AS IdModel 
                                        FROM Computer c
                                        LEFT JOIN Employee e ON e.ComputerId = c.Id
                                        WHERE e.Id = @id AND c.Id = e.ComputerId
                                        OR c.Id NOT IN (SELECT ComputerId FROM Employee)
                                        AND DecomissionDate IS NULL";

                    cmd.Parameters.Add(new SqlParameter("@id", EmpId));

                    var reader = cmd.ExecuteReader();
                    var options = new List<SelectListItem>();

                    while (reader.Read())
                    {
                        var option = new SelectListItem()
                        {
                            Text = reader.GetString(reader.GetOrdinal("IdModel")),
                            Value = reader.GetInt32(reader.GetOrdinal("Id")).ToString()
                        };

                        options.Add(option);

                    }
                    reader.Close();
                    return options;
                }
            }
        }
        private Employee GetEmployeeById(int id)
        {
            var programs = GetTrainingPrograms(id);
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id AS EmployeesId, e.FirstName, e.LastName, e.Email, e.DepartmentId, e.IsSupervisor, c.Id AS ComputersId, c.Make, c.Model, d.Name AS DepartmentName
                                    FROM Employee e
                                    LEFT JOIN Department d ON d.Id = e.DepartmentId
                                    LEFT JOIN Computer c on c.Id = e.ComputerId
                                    WHERE e.Id = @Id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();
                    Employee employee = null;

                    if (reader.Read())
                    {
                        employee = new Employee()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("EmployeesId")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                            ComputerId = reader.GetInt32(reader.GetOrdinal("ComputersId")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            Computer = new Computer()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ComputersId")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Model = reader.GetString(reader.GetOrdinal("Model"))
                            },
                            Department = new Department()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                Name = reader.GetString(reader.GetOrdinal("DepartmentName"))
                            }
                        };
                    }
                    reader.Close();
                    return employee;
                }
            };
        }
    }
}