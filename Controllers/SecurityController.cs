using Microsoft.AspNetCore.Mvc;
using MeuAppSeguranca.Models;
using MeuAppSeguranca.Services;
using System.Threading.Tasks;
using System;

namespace MeuAppSeguranca.Controllers
{
    public class SecurityController : Controller
    {
        private readonly SecurityTestService _securityService;

        public SecurityController(SecurityTestService securityService)
        {
            _securityService = securityService;
        }

        public IActionResult Index()
        {
            return View(new SecurityTestResult());
        }

        [HttpPost]
        public async Task<IActionResult> RunTests(string targetUrl)
        {
            if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out var uriResult) 
                || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                ModelState.AddModelError("", "URL inválida. Informe uma URL completa com http:// ou https://");
                return View("Index", new SecurityTestResult());
            }

            var result = new SecurityTestResult
            {
                PortScanResult = await _securityService.RunPortScan(targetUrl),
                WebScanResult = await _securityService.RunWebScan(targetUrl),
                SqlInjectionResult = await _securityService.TestSqlInjection(targetUrl),
                XssResult = await _securityService.TestXss(targetUrl),
                SslResult = await _securityService.AnalyzeSsl(targetUrl),
                DnsResult = await _securityService.EnumerateDns(uriResult.Host), // Passa só hostname para DNS
                TraversalResult = await _securityService.TestDirectoryTraversal(targetUrl),
                HeaderResult = await _securityService.AnalyzeHeaders(targetUrl)
            };
            return View("Index", result);
        }
    }
}
