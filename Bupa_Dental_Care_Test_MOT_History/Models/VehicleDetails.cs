using Newtonsoft.Json;

namespace Bupa_Dental_Care_Test_MOT_History.Models
{
    public class VehicleDetails
    {
        public string registration { get; set; }
        public string make { get; set; }
        public string model { get; set; }
        public string firstUsedDate { get; set; }
        public string fuelType { get; set; }
        public string primaryColour { get; set; }
        public List<MOTTestHistory> motTests { get; set; }

    }

    public class MOTTestHistory
    {
        public string completedDate { get; set;}
        public string testResult { get; set;}
        public string expiryDate { get; set;}
        public string odometerValue { get; set;}
        public string odometerUnit { get; set;}
        public string motTestNumber { get; set; }
    }
}

