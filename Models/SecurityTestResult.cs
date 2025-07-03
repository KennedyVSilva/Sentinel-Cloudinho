namespace SentinelDoCloudinho.Models
{
    public class SecurityTestResult
    {
        public string Category { get; set; }
        public List<(string Detail, bool Status)> Details { get; set; }
    }
}