using DnsClient;
using MeuAppSeguranca.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace MeuAppSeguranca.Services
{
    public class SecurityTestService
    {
        private readonly HttpClient _httpClient;
        private readonly ILookupClient _dnsClient;

        public SecurityTestService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _dnsClient = new LookupClient();
        }

        public async Task<string> RunPortScan(string target)
        {
            try
            {
                return await Task.FromResult($"Portas abertas em {target}: 80, 443 (simulado)");
            }
            catch (Exception)
            {
                return "Erro ao simular Port Scan";
            }
        }

        public async Task<string> RunWebScan(string target)
        {
            try
            {
                return await Task.FromResult($"Vulnerabilidades encontradas em {target}: Nenhuma (simulado)");
            }
            catch (Exception)
            {
                return "Erro ao simular Web Scan";
            }
        }

        public async Task<string> TestSqlInjection(string target)
        {
            try
            {
                var url = $"{target}?id=1' OR '1'='1";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return "Possível SQL Injection detectado em " + target;
            }
            catch (HttpRequestException)
            {
                return "Nenhuma vulnerabilidade SQL Injection detectada";
            }
            catch (Exception)
            {
                return "Erro ao testar SQL Injection";
            }
        }

        public async Task<string> TestXss(string target)
        {
            try
            {
                var url = $"{target}?input=<script>alert('xss')</script>";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return content.Contains("<script>") ? "XSS detectado em " + target : "Nenhuma vulnerabilidade XSS";
            }
            catch (HttpRequestException)
            {
                return "Nenhuma vulnerabilidade XSS detectada";
            }
            catch (Exception)
            {
                return "Erro ao testar XSS";
            }
        }

        public async Task<string> AnalyzeSsl(string target)
        {
            try
            {
                return await Task.FromResult($"SSL/TLS em {target}: Grau A (simulado)");
            }
            catch (Exception)
            {
                return "Erro ao analisar SSL/TLS";
            }
        }

        public async Task<string> EnumerateDns(string host)
        {
            try
            {
                var result = await _dnsClient.QueryAsync(host, QueryType.ANY);
                return $"Registros DNS para {host}: {string.Join(", ", result.Answers.Select(a => a.ToString()))}";
            }
            catch (Exception)
            {
                return "Erro ao enumerar DNS";
            }
        }

        public async Task<string> TestDirectoryTraversal(string target)
        {
            try
            {
                var url = $"{target}/../../etc/passwd";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return "Possível Directory Traversal detectado em " + target;
            }
            catch (HttpRequestException)
            {
                return "Nenhuma vulnerabilidade Directory Traversal detectada";
            }
            catch (Exception)
            {
                return "Erro ao testar Directory Traversal";
            }
        }

        public async Task<string> AnalyzeHeaders(string target)
        {
            try
            {
                var response = await _httpClient.GetAsync(target);
                response.EnsureSuccessStatusCode();
                return response.Headers.Any(h => h.Key == "X-Content-Type-Options") 
                    ? $"Cabeçalhos seguros em {target}" 
                    : "Cabeçalhos inseguros detectados em " + target;
            }
            catch (HttpRequestException)
            {
                return "Nenhum cabeçalho analisado devido a erro de conexão";
            }
            catch (Exception)
            {
                return "Erro ao analisar cabeçalhos";
            }
        }
    }
}
