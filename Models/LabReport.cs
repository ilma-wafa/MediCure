namespace MediCure.Models
{
    public class LabReport
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime TestDate { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}