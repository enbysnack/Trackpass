using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using Trackpass.Models;

namespace AttendanceController.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly string connectionString = "Server=192.169.144.133;Database=mcctc_kodi;User=mcctc_kodi;Password=7200@Palmyra;";

        public AttendanceController(IConfiguration configuration) { }

        // GET: /Attendance/Scan
        public IActionResult Scan() => View(); // Returns the Scan.cshtml view

        // POST: /Attendance/ProcessScan
        [HttpPost]
        public IActionResult ProcessScan(string badgeId, string action, int locationId)
        {
            // Validate the badge ID before processing
            if (!IsValidBadgeId(badgeId))
            {
                ViewBag.Message = "Please enter a valid badge ID.";
                return View("Scan");
            }

            string locationName = GetLocationName(locationId);

            if (action.ToLower() == "clockin")
            {
                ClockIn(badgeId, locationName);
                ViewBag.Message = "Successfully clocked in.";
            }
            else if (action.ToLower() == "clockout")
            {
                ClockOut(badgeId, locationName);
                ViewBag.Message = "Successfully clocked out.";
            }

            return View("Scan");
        }

        private string GetLocationName(int locationId)
        {
            return locationId switch
            {
                1 => "Restroom",
                2 => "Nurse",
                3 => "Leave building",
                4 => "Counselor",
                5 => "Office",
                6 => "Other",
                _ => "Unknown",
            };
        }

        private void ClockOut(string badgeId, string locationName)
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            var command = new MySqlCommand("INSERT INTO clock_times (ID, CheckOutTime, Location) VALUES (@ID, @CheckOutTime, @Location)", connection);
            command.Parameters.AddWithValue("@ID", badgeId);
            command.Parameters.AddWithValue("@CheckOutTime", DateTime.Now);
            command.Parameters.AddWithValue("@Location", locationName);
            command.ExecuteNonQuery();
        }

        private void ClockIn(string badgeId, string locationName)
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            var command = new MySqlCommand("INSERT INTO clock_times (ID, CheckInTime, Location) VALUES (@ID, @CheckInTime, @Location)", connection);
            command.Parameters.AddWithValue("@ID", badgeId);
            command.Parameters.AddWithValue("@CheckInTime", DateTime.Now);
            command.Parameters.AddWithValue("@Location", locationName);
            command.ExecuteNonQuery();
        }

        private bool IsValidBadgeId(string badgeId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = new MySqlCommand("SELECT COUNT(*) FROM students WHERE ID = @ID", connection);
                command.Parameters.AddWithValue("@ID", badgeId);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0; // Return true if exists
            }
        }

        // GET: /Attendance/ObjectRecognition
        public IActionResult ObjectRecognition() => View(); // Returns the ObjectRecognition.cshtml view
    }
}
