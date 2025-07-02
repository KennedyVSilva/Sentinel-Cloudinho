namespace MeuAppSeguranca.Models
{
    public class SecurityTestResult
    {
        public string PortScanResult { get; set; } = "Não implementado";
        public string WebScanResult { get; set; } = "Não implementado";
        public string SqlInjectionResult { get; set; } = "Não implementado";
        public string XssResult { get; set; } = "Não implementado";
        public string SslResult { get; set; } = "Não implementado";
        public string DnsResult { get; set; } = "Não implementado";
        public string TraversalResult { get; set; } = "Não implementado";
        public string HeaderResult { get; set; } = "Não implementado";
    }
}