using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System; // Update with the actual namespace where AttendanceRecord is located
using Trackpass.Models;

namespace AttendanceController.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly string connectionString = "Server=192.169.144.133;Database=mcctc_kodi;User=mcctc_kodi;Password=7200@Palmyra;";
        private string? _connectionString;


        private List<AttendanceRecord> GetAttendanceRecords(string badgeId)
        {
            var records = new List<AttendanceRecord>();
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = new MySqlCommand("SELECT ID, CheckInTime, CheckOutTime, Location FROM clock_times WHERE ID = @ID", connection);
                command.Parameters.AddWithValue("@ID", badgeId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        records.Add(new AttendanceRecord
                        {
                            ID = reader["ID"].ToString(),
                            CheckInTime = reader["CheckInTime"] != DBNull.Value ? (DateTime)reader["CheckInTime"] : (DateTime?)null,
                            CheckOutTime = reader["CheckOutTime"] != DBNull.Value ? (DateTime)reader["CheckOutTime"] : (DateTime?)null,
                            Location = reader["Location"].ToString() // Directly pulling the location name
                        });
                    }
                }
            }
            return records;
        }

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
        public IActionResult ProcessScan(string badgeId, string action, string locationName)
        {
            // Check if the badgeId is valid
            if (string.IsNullOrEmpty(badgeId) || !IsValidBadgeId(badgeId))
            {
                ViewBag.Message = "Please enter a valid ID.";
                return View("Scan"); // Return to the Scan view with the message
            }

            // Proceed with checking in or out based on action
            if (action.ToLower() == "leave")
            {
                ClockOut(badgeId, locationName);
                ViewBag.Message = "Successfully clocked out.";
            }
            else if (action.ToLower() == "return")
            {
                ClockIn(badgeId, locationName);
                ViewBag.Message = "Successfully clocked in.";
            }

            return RedirectToAction("Index"); // Redirect after processing
        }

        // Method to check if badgeId exists in the database
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


        [HttpPost]
        public IActionResult Submit(string badgeId, string actionType)
        {
            // Process the submitted badge ID and action (leave or return)
            if (string.IsNullOrEmpty(badgeId) || string.IsNullOrEmpty(actionType))
            {
                return BadRequest("Badge ID and Action Type are required.");
            }

            // Logic for leave/return can go here (interacting with the database, etc.)

            // Redirect to the Scan page after processing
            return RedirectToAction("Scan");
        }

        private void ClockOut(string badgeId, string locationName)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = new MySqlCommand("INSERT INTO clock_times (ID, CheckOutTime, Location) VALUES (@ID, @CheckOutTime, @Location)", connection);
                command.Parameters.AddWithValue("@ID", badgeId);
                command.Parameters.AddWithValue("@CheckOutTime", DateTime.Now);
                command.Parameters.AddWithValue("@Location", locationName); // Now using location name directly

                command.ExecuteNonQuery();
            }
        }

        private void ClockIn(string badgeId, string locationName)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = new MySqlCommand("INSERT INTO clock_times (ID, CheckInTime, Location) VALUES (@ID, @CheckInTime, @Location)", connection);
                command.Parameters.AddWithValue("@ID", badgeId);
                command.Parameters.AddWithValue("@CheckInTime", DateTime.Now);
                command.Parameters.AddWithValue("@Location", locationName); // Now using location name directly

                command.ExecuteNonQuery();
            }
        }
        private string GetLocationName(int locationId)
        {
            // Define your location mapping
            switch (locationId)
            {
                case 1: return "Restroom";
                case 2: return "Nurse";
                case 3: return "Leave building";
                case 4: return "Counselor";
                case 5: return "Office";
                case 6: return "Other";
                default: return "Unknown";
            }
        }

        private int? GetLocationId(string locationName)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = new MySqlCommand("SELECT ID FROM locations WHERE Name = @LocationName", connection);
                command.Parameters.AddWithValue("@LocationName", locationName);

                var result = command.ExecuteScalar();
                return result != null ? (int?)Convert.ToInt32(result) : null; // Return ID or null if not found
            }
        }

    }
}
