using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;

namespace AttendanceController.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly string connectionString = "Server=192.169.144.133;Database=mcctc_kodi;User=mcctc_kodi;Password=7200@Palmyra;";
        private string? _connectionString;

        public AttendanceController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: /Attendance/Scan
        public IActionResult Scan()
        {
            return View(); // Returns the Scan.cshtml view
        }

        // POST: /Attendance/ProcessScan
        [HttpPost]
        public IActionResult ProcessScan(string badgeId, string action)
        {
            // Handle the logic for checking in or out
            // You can access the badgeId and action parameters here

            // For example, if the action is "leave", call your ClockOut method
            // If the action is "return", call your ClockIn method

            return RedirectToAction("Index"); // Redirect to a suitable action after processing
        }
        public IActionResult SubmitScan(string badgeId, string action, int? locationId = null)
        {
            string studentName = GetStudentName(badgeId);

            if (studentName == null)
            {
                ViewBag.Message = "Error: Invalid ID. Please try again.";
                return View("Scan");
            }

            // Check if it's a "leave" or "return"
            if (action == "leave")
            {
                ClockOut(badgeId, locationId);
                ViewBag.Message = $"{studentName}, you have successfully clocked out!";
            }
            else if (action == "return")
            {
                ClockIn(badgeId, locationId);
                ViewBag.Message = $"{studentName}, you have successfully clocked in!";
            }

            return View("Scan");
        }

        private string GetStudentName(string badgeId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = new MySqlCommand("SELECT FirstName FROM students WHERE ID = @ID", connection);
                command.Parameters.AddWithValue("@ID", badgeId);

                var result = command.ExecuteScalar();
                // If result is null, return an empty string
                return result?.ToString() ?? string.Empty;
            }
        }

        private void ClockOut(string badgeId, int? locationId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = new MySqlCommand("INSERT INTO clock_times (ID, CheckOutTime, Location) VALUES (@ID, @CheckOutTime, @Location)", connection);
                command.Parameters.AddWithValue("@ID", badgeId);
                command.Parameters.AddWithValue("@CheckOutTime", DateTime.Now);

                // Check if locationId is null, if yes, use DBNull.Value
                if (locationId.HasValue)
                {
                    command.Parameters.AddWithValue("@Location", locationId.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("@Location", DBNull.Value);
                }

                command.ExecuteNonQuery();
            }
        }

        private void ClockIn(string badgeId, int? locationId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = new MySqlCommand("INSERT INTO clock_times (ID, CheckInTime, Location) VALUES (@ID, @CheckInTime, @Location)", connection);
                command.Parameters.AddWithValue("@ID", badgeId);
                command.Parameters.AddWithValue("@CheckInTime", DateTime.Now);

                // Check if locationId is null, if yes, use DBNull.Value
                if (locationId.HasValue)
                {
                    command.Parameters.AddWithValue("@Location", locationId.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("@Location", DBNull.Value);
                }

                command.ExecuteNonQuery();
            }
        }

    }
}
