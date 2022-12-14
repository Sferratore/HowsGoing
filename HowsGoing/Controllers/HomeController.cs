using HowsGoing.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Data;
using System.Diagnostics;

namespace HowsGoing.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult SendRecord(string message, int satisfaction) //, IConfigurationRoot config)
        {
            //string connectionString = config.GetSection("ConnectionStrings")["HowsGoingContext"]; come cazzo si usa?
            using (MySqlConnection con = new MySqlConnection("Server=localhost;Port=3306;User=root;Password=DB09Gennaio;Database=howsgoing"))
            {
                MySqlCommand cmd = new MySqlCommand("INSERT INTO Records (RECORD_CONTENT, SATISFACTION) VALUES ('" + message + "', '" + satisfaction + "');", con);
                cmd.CommandType = CommandType.Text;
                con.Open();
                cmd.ExecuteNonQuery();
            }
            return Content("The record has been inserted successfully!");
        }
    }
}