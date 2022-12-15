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
        private readonly IConfiguration config;

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            this.config = config;
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
        public IActionResult SendRecord(string message, int satisfaction)
        {
            string connectionString = config.GetSection("ConnectionStrings")["HowsGoingContext"];
            using (MySqlConnection con = new MySqlConnection(connectionString))
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