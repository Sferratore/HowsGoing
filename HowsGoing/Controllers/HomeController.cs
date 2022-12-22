using HowsGoing.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;


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

        public IActionResult Register()
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
            HttpContext.Session.Clear();
            return View();
        }

        public IActionResult LoginFailed()
        {
            return View();
        }

        public IActionResult Friends()
        {
            if (HttpContext.Session.Get("username") == null)
                return View("Login");


            string connectionString = config.GetSection("ConnectionStrings")["HowsGoingContext"];
            List<User> usersfriendreq = new List<User>();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM FRIENDREQUESTS WHERE RECEIVER = '" + HttpContext.Session.GetString("username") + "';", con);
                cmd.CommandType = CommandType.Text;
                con.Open();
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    usersfriendreq.Add(new User(dataReader.GetString("SENDER"), "//"));
                }
                dataReader.Close();
            }

            ViewBag.friendreq = usersfriendreq;

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

            string friendString = "('" + HttpContext.Session.GetString("username");
            int i = 1;

            while (HttpContext.Session.GetString("friend" + i) != null)
            {
                if (HttpContext.Session.GetString("friend" + (i + 1)) != null)
                {
                    friendString += "', '" + HttpContext.Session.GetString("friend" + i) + "', '";
                }
                else
                {
                    friendString += "', '" + HttpContext.Session.GetString("friend" + i);
                    break;
                }
                i++;
            }

            friendString += "'); ";


            string connectionString = config.GetSection("ConnectionStrings")["HowsGoingContext"];
            List<Record> records = new List<Record>();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM RECORDS WHERE USER_ID IN " + friendString, con);  //bug friend
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
            List<User> friends = new List<User>();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM USERS WHERE USERNAME ='" + username + "' AND PASSWORD = '" + password + "';", con);
                cmd.CommandType = CommandType.Text;
                con.Open();
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    HttpContext.Session.SetString("username", username);
                }
                dataReader.Close();

                if (HttpContext.Session.GetString("username") != username || username == null)
                {
                    return View("LoginFailed");
                }

                cmd = new MySqlCommand("SELECT USER_ID2 AS FRIEND FROM FRIENDSHIPS WHERE USER_ID1 ='" + HttpContext.Session.GetString("username") + "' UNION SELECT USER_ID1 AS FRIEND FROM FRIENDSHIPS WHERE USER_ID2 = '" + HttpContext.Session.GetString("username") + "';", con);
                cmd.CommandType = CommandType.Text;
                dataReader = cmd.ExecuteReader();
                int i = 1;
                while (dataReader.Read())
                {
                    HttpContext.Session.SetString("friend" + i, dataReader.GetString("FRIEND"));
                    i++;
                }
                dataReader.Close();
                return View("Index");
            }


        }

        [HttpPost]
        public IActionResult FriendSearch(string username)
        {

            if (HttpContext.Session.Get("username") == null)
                return View("Login");

            string connectionString = config.GetSection("ConnectionStrings")["HowsGoingContext"];
            List<User> users = new List<User>();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM USERS WHERE USERNAME NOT IN (SELECT USER_ID1 FROM FRIENDSHIPS WHERE USER_ID2 = '" + HttpContext.Session.GetString("username") + "') AND USERNAME NOT IN (SELECT USER_ID2 FROM FRIENDSHIPS WHERE USER_ID1 = '" + HttpContext.Session.GetString("username") + "') AND USERNAME NOT IN (SELECT SENDER FROM FRIENDREQUESTS WHERE RECEIVER = '" + HttpContext.Session.GetString("username") + "') AND USERNAME NOT IN (SELECT RECEIVER FROM FRIENDREQUESTS WHERE SENDER = '" + HttpContext.Session.GetString("username") + "') AND USERNAME = '" + username + "';", con);
                cmd.CommandType = CommandType.Text;
                con.Open();
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    users.Add(new User(dataReader.GetString("USERNAME"), dataReader.GetString("PASSWORD")));
                }
                dataReader.Close();
            }

            ViewBag.users = users;

            return View("Friends");

        }


        [HttpPost]
        public IActionResult AddUser(string usertoadd)
        {
            string connectionString = config.GetSection("ConnectionStrings")["HowsGoingContext"];
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                MySqlCommand cmd = new MySqlCommand("INSERT INTO FRIENDREQUESTS VALUES ('" + HttpContext.Session.GetString("username") + "', '" + usertoadd + "');", con);
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
            return View("Friends");
        }


        [HttpPost]
        public IActionResult ConfirmRequest(string usertoadd)
        {
            string connectionString = config.GetSection("ConnectionStrings")["HowsGoingContext"];
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                MySqlCommand cmd = new MySqlCommand("INSERT INTO FRIENDSHIPS VALUES ('" + HttpContext.Session.GetString("username") + "', '" + usertoadd + "');", con);
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

                cmd = new MySqlCommand("DELETE FROM FRIENDREQUESTS WHERE SENDER ='" + usertoadd + "' AND RECEIVER ='" + HttpContext.Session.GetString("username") + "';", con);
                try
                {
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
                int i = 1;
                while (HttpContext.Session.GetString("friend" + i) != null)
                {
                    i++;
                }
                HttpContext.Session.SetString("friend" + i, usertoadd);
            }
            return View("Friends");
        }

        [HttpPost]
        public IActionResult RegistrationCheck(string username, string password, string email)
        {

            if (username == null || password == null || email == null)
            {
                ViewBag.nullValues = "it is";
                return View("Register");
            }

            string connectionString = config.GetSection("ConnectionStrings")["HowsGoingContext"];
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM USERS WHERE USERNAME ='" + username + "';", con);
                cmd.CommandType = CommandType.Text;
                con.Open();
                MySqlDataReader dataReader = cmd.ExecuteReader();
                if (dataReader.Read())
                {
                    ViewBag.usernameInvalid = "it is";
                    return View("Register");
                }
                dataReader.Close();

                cmd = new MySqlCommand("SELECT * FROM USERS WHERE EMAIL ='" + email + "';", con);
                dataReader = cmd.ExecuteReader();
                if (dataReader.Read())
                {
                    ViewBag.emailTaken = "it is";
                    return View("Register");
                }
                dataReader.Close();

                Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                Match match = regex.Match(email);
                if (match.Success)
                {
                    cmd = new MySqlCommand("INSERT INTO USERS VALUES ('" + username + "', '" + password + "', '" + email + "');", con);
                    try
                    {
                        cmd.ExecuteNonQuery();
                        ViewBag.newUserName = username;
                        return View("RegisterSuccess");
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
                else
                {
                    ViewBag.emailNotRegex = "it is";
                    return View("Register");
                }
            }
        }


    }
}