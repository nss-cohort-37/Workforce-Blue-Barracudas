using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BangazonWorforce.Models;
using BangazonWorkforce.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BangazonWorkforce.Controllers
{
    public class ComputersController : Controller
    {
        private readonly IConfiguration _config;

        public ComputersController(IConfiguration config)
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
        // GET: Computers
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id, c.Make, c.Model, c.PurchaseDate
                                       FROM Computer c";


                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Computer> computers = new List<Computer>();

                    while (reader.Read())
                    {
                        Computer computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Model = reader.GetString(reader.GetOrdinal("Model")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate"))
                           
                        };

                      computers.Add(computer);
                    }

                    reader.Close();

                    return View(computers);
                }
            }
        }

        // GET: Computers/Details/5
        public ActionResult Details(int id)
        {
            var computerDetail = GetComputerById(id); 
            return View(computerDetail);
        }

        // GET: Computers/Create
        public ActionResult Create()
        {
            var viewModel = new AddComputerViewModel()
            {
                EmployeeOptions = GetEmployeeOptions()
            };

            return View(viewModel);
        }

        // POST: Computers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AddComputerViewModel computer)
        {
            try
            {
                // TODO: Add insert logic here

                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Computer (Make, Model, PurchaseDate, DecomissionDate)
                                            OUTPUT INSERTED.Id
                                            VALUES (@make, @model, @purchaseDate, @decomissionDate)
                                           ";

                        cmd.Parameters.Add(new SqlParameter("@make", computer.Make));
                        cmd.Parameters.Add(new SqlParameter("@model", computer.Model));
                        cmd.Parameters.Add(new SqlParameter("@purchaseDate", computer.PurchaseDate));
                        cmd.Parameters.Add(new SqlParameter("@decomissionDate", DBNull.Value));

                        cmd.Parameters.Add(new SqlParameter("@employeeId", computer.EmployeeId));

                        var id = (int)cmd.ExecuteScalar();
                        computer.ComputerId = id;


                        cmd.CommandText = @"UPDATE Employee  
                                            SET ComputerId = @computerComputerId
                                            WHERE Id = @computerEmployeeId";

                        cmd.Parameters.Add(new SqlParameter("@computerComputerId", computer.ComputerId));
                        cmd.Parameters.Add(new SqlParameter("@computerEmployeeId", computer.EmployeeId));

                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        // GET: Computers/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Computers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Computers/Delete/5
        public ActionResult Delete(int id)

        {
            var computerDetail = GetComputerById(id);
            return View(computerDetail);
        }

        // POST: Computers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Computer computer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Computer WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        cmd.ExecuteNonQuery();

                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View();
            }
        }



        private ComputerDetailsViewModel GetComputerById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id, c.Make, c.Model, c.DecomissionDate, c.PurchaseDate, COALESCE(e.[FirstName] + ' ' + e.LastName, 'N/A' ) AS EmployeeName
                        FROM Computer c
                         LEFT JOIN Employee e ON c.Id = e.ComputerId
                        WHERE c.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();
                    ComputerDetailsViewModel computer= null;

                    if (reader.Read())
                    {
                        computer = new ComputerDetailsViewModel()
                        {
                            ComputerId = reader.GetInt32(reader.GetOrdinal("Id")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Model = reader.GetString(reader.GetOrdinal("Model")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            employee = new Employee
                            {
                                FirstName = reader.GetString(reader.GetOrdinal("EmployeeName"))
                            }
                        };
                        if (!reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                        {
                            computer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                        }
                        else
                        {
                            computer.DecomissionDate = null;
                        }

                    }
                    reader.Close();
                    return computer;
                }
            }
        }

        private List<SelectListItem> GetEmployeeOptions()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, COALESCE([FirstName] + ' ' + [LastName], 'N/A' ) AS EmployeeName FROM Employee";



                    var reader = cmd.ExecuteReader();
                    var options = new List<SelectListItem>();

                    while (reader.Read())
                    {
                        var option = new SelectListItem()
                        {
                            Text = reader.GetString(reader.GetOrdinal("EmployeeName")),
                            Value = reader.GetInt32(reader.GetOrdinal("Id")).ToString()
                        };
                        options.Add(option);
                    }


                    reader.Close();
                    return options;
                }
            }
        }

    }
}