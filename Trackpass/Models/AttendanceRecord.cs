namespace Trackpass.Models
{
    public class AttendanceRecord
    {
        public string ID { get; set; }  // Non-nullable
        public DateTime? CheckInTime { get; set; } // Nullable
        public DateTime? CheckOutTime { get; set; } // Nullable
        public string Location { get; set; } // Non-nullable
    }
}
