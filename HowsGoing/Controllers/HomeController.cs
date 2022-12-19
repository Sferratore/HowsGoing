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
            if (HttpContext.Session.Get("username") == null)
                return View("Login");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Postpage()
        {
            if (HttpContext.Session.Get("username") == null)
                return View("Login");

            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult LoginFailed()
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
            if (HttpContext.Session.Get("username") == null)
                return View("Login");

            string connectionString = config.GetSection("ConnectionStrings")["HowsGoingContext"];
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                MySqlCommand cmd = new MySqlCommand("INSERT INTO Records (RECORD_CONTENT, SATISFACTION, USER_ID) VALUES ('" + message + "', '" + satisfaction + "', '" + HttpContext.Session.GetString("username") + "');", con);
                cmd.CommandType = CommandType.Text;
                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    return Content("An error has occurred with a Database operation: " + ex.ToString());
                }
                catch (Exception ex)
                {
                    return Content("An error has occurred with an operation: " + ex.ToString());
                }
            }
            return View("RecordSent");
        }


        public IActionResult GetAllRecords()
        {
            if (HttpContext.Session.Get("username") == null)
                return View("Login");

            string connectionString = config.GetSection("ConnectionStrings")["HowsGoingContext"];
            List<Record> records = new List<Record>();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM RECORDS", con);
                cmd.CommandType = CommandType.Text;
                con.Open();
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    records.Add(new Record(dataReader.GetInt32("RECORD_ID"), dataReader.GetString("RECORD_CONTENT"), dataReader.GetInt32("SATISFACTION"), dataReader.GetString("USER_ID")));
                }
                dataReader.Close();
            }

            ViewBag.records = records;
            return View();
        }


        [HttpPost]
        public IActionResult LoginCheck(string username, string password)
        {
            string connectionString = config.GetSection("ConnectionStrings")["HowsGoingContext"];
            List<User> user = new List<User>();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM USERS WHERE USERNAME ='" + username + "' AND PASSWORD = '" + password + "';", con);
                cmd.CommandType = CommandType.Text;
                con.Open();
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    HttpContext.Session.SetString("username", username);
                    dataReader.Close();
                    return View("Index");
                }
                dataReader.Close();
                return View("LoginFailed");
            }


        }

    }
}