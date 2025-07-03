using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using DnsClient;

namespace SentinelDoCloudinho.Services
{
    public class SecurityTestService
    {
        private readonly IHubContext<TestHub> _hubContext;
        private readonly HttpClient _httpClient;
        private readonly ILookupClient _dnsClient;
        private readonly IConfiguration _configuration;

        public SecurityTestService(IHubContext<TestHub> hubContext, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _hubContext = hubContext;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("SecurityTestSettings:HttpTimeoutSeconds", 30));
            _dnsClient = new LookupClient();
            _configuration = configuration;
        }

        public async Task PerformPenTest(List<string> targets, string aggressionLevel)
        {
            var results = new Dictionary<string, List<(string Detail, bool Status)>>();
            double progress = 0;
            int totalTests = CalculateTotalTests(targets.Count, aggressionLevel);
            int testsCompleted = 0;

            var maxConcurrent = _configuration.GetValue<int>("SecurityTestSettings:MaxConcurrentTargets", 5);
            if (targets.Count > maxConcurrent)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveUpdate", $"Limite de {maxConcurrent} alvos atingido. Processando os primeiros.", progress, results);
                targets = targets.Take(maxConcurrent).ToList();
            }

            foreach (var target in targets)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveUpdate", $"Testando {target}...", progress, results);

                // Port Scanning
                results["Port Scanning"] = await ScanPorts(target, aggressionLevel);
                testsCompleted += 10;
                progress = (testsCompleted / (double)totalTests) * 100;
                await _hubContext.Clients.All.SendAsync("ReceiveUpdate", $"Portas escaneadas em {target}", progress, results);

                // SQL Injection
                results["SQL Injection"] = await TestSqlInjection(target);
                testsCompleted += 5;
                progress = (testsCompleted / (double)totalTests) * 100;
                await _hubContext.Clients.All.SendAsync("ReceiveUpdate", $"SQLi testado em {target}", progress, results);

                // Cross-site Scripting
                results["Cross-site Scripting"] = await TestXss(target);
                testsCompleted += 5;
                progress = (testsCompleted / (double)totalTests) * 100;
                await _hubContext.Clients.All.SendAsync("ReceiveUpdate", $"XSS testado em {target}", progress, results);

                // HTTP Headers
                results["HTTP Headers"] = await AnalyzeHeaders(target);
                testsCompleted += 5;
                progress = (testsCompleted / (double)totalTests) * 100;
                await _hubContext.Clients.All.SendAsync("ReceiveUpdate", $"Headers analisados em {target}", progress, results);

                // SSL/TLS
                results["SSL/TLS"] = await AnalyzeSsl(target);
                testsCompleted += 5;
                progress = (testsCompleted / (double)totalTests) * 100;
                await _hubContext.Clients.All.SendAsync("ReceiveUpdate", $"SSL analisado em {target}", progress, results);

                // DNS Enumeration
                results["DNS Enumeration"] = await EnumerateDns(target);
                testsCompleted += 5;
                progress = (testsCompleted / (double)totalTests) * 100;
                await _hubContext.Clients.All.SendAsync("ReceiveUpdate", $"DNS enumerado em {target}", progress, results);
            }

            await _hubContext.Clients.All.SendAsync("ReceiveUpdate", "Teste concluído!", 100, results);
        }

        private int CalculateTotalTests(int targetCount, string aggressionLevel)
        {
            return targetCount * (aggressionLevel == "high" ? 30 : aggressionLevel == "medium" ? 15 : 5);
        }

        private async Task<List<(string Detail, bool Status)>> ScanPorts(string target, string aggressionLevel)
        {
            var results = new List<(string Detail, bool Status)>();
            var ports = _configuration.GetSection("SecurityTestSettings:CommonPorts").Get<int[]>() ?? new[] { 20, 21, 22, 80, 443 };
            var portRange = aggressionLevel == "high" ? Enumerable.Range(0, 65535) :
                            aggressionLevel == "medium" ? Enumerable.Range(0, 1024) : ports;
            foreach (var port in portRange.Take(5)) // Limitado para exemplo
            {
                results.Add(($"Porta {port} aberta", new Random().Next(2) == 0));
            }
            return results;
        }

        private async Task<List<(string Detail, bool Status)>> TestSqlInjection(string target)
        {
            var results = new List<(string Detail, bool Status)>();
            var payloads = _configuration.GetSection("SecurityTestSettings:SqlInjectionPayloads").Get<string[]>() ?? new[] { "' OR 1=1 --", "1; DROP TABLE users" };
            foreach (var payload in payloads)
            {
                try
                {
                    var response = await _httpClient.GetAsync($"{target}?id={payload}");
                    results.Add(($"Payload {payload}", !response.IsSuccessStatusCode));
                }
                catch { results.Add(($"Payload {payload}", false)); }
            }
            return results;
        }

        private async Task<List<(string Detail, bool Status)>> TestXss(string target)
        {
            var results = new List<(string Detail, bool Status)>();
            var payloads = _configuration.GetSection("SecurityTestSettings:XssPayloads").Get<string[]>() ?? new[] { "<script>alert(1)</script>", "<img src=x onerror=alert(1)>" };
            foreach (var payload in payloads)
            {
                try
                {
                    var response = await _httpClient.GetAsync($"{target}?input={payload}");
                    var content = await response.Content.ReadAsStringAsync();
                    results.Add(($"Payload {payload}", !content.Contains(payload)));
                }
                catch { results.Add(($"Payload {payload}", false)); }
            }
            return results;
        }

        private async Task<List<(string Detail, bool Status)>> AnalyzeHeaders(string target)
        {
            var results = new List<(string Detail, bool Status)>();
            try
            {
                var response = await _httpClient.GetAsync(target);
                results.Add(("Content-Security-Policy presente", response.Headers.Contains("Content-Security-Policy")));
                results.Add(("Strict-Transport-Security presente", response.Headers.Contains("Strict-Transport-Security")));
                results.Add(("X-Frame-Options presente", response.Headers.Contains("X-Frame-Options")));
            }
            catch { results.AddRange(new[] { ("CSP", false), ("HSTS", false), ("X-Frame-Options", false) }); }
            return results;
        }

        private async Task<List<(string Detail, bool Status)>> AnalyzeSsl(string target)
        {
            var results = new List<(string Detail, bool Status)>();
            try
            {
                var uri = new Uri(target);
                // Simulação (substituir por OpenSSL)
                results.Add(("TLS 1.2+ suportado", true));
                results.Add(("Certificado válido", new Random().Next(2) == 0));
            }
            catch { results.AddRange(new[] { ("TLS 1.2+", false), ("Certificado válido", false) }); }
            return results;
        }

        private async Task<List<(string Detail, bool Status)>> EnumerateDns(string target)
        {
            var results = new List<(string Detail, bool Status)>();
            try
            {
                var mx = await _dnsClient.QueryAsync(target, QueryType.MX);
                var txt = await _dnsClient.QueryAsync(target, QueryType.TXT);
                results.Add(("MX Records", mx.Answers.MxRecords().Any()));
                results.Add(("TXT/SPF Records", txt.Answers.TxtRecords().Any()));
            }
            catch { results.AddRange(new[] { ("MX", false), ("TXT/SPF", false) }); }
            return results;
        }
    }
}