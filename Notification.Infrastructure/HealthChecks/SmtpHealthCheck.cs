using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Notification.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Infrastructure.HealthChecks
{
    public class SmtpHealthCheck : IHealthCheck
    {
        private readonly SmtpOptions _options;
        public SmtpHealthCheck(IOptions<SmtpOptions> options) => _options = options.Value;

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
        {
            try
            {
                using var client = new SmtpClient(_options.Host, _options.Port)
                {
                    EnableSsl = _options.EnableSsl,
                    Credentials = new NetworkCredential(_options.Username, _options.Password)
                };

                // فقط یک تست ساده اتصال
                await client.SendMailAsync(new MailMessage(_options.From, _options.From, "HealthCheck", "Ping"), ct);

                return HealthCheckResult.Healthy("SMTP is reachable");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("SMTP unreachable", ex);
            }
        }
    }

}
