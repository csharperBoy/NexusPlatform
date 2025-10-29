using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Models;
using Microsoft.Extensions.Options;
using Polly.Registry;
using Polly;
namespace Notification.Infrastructure.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _options;
        private readonly IReadOnlyPolicyRegistry<string> _policies;
        public SmtpEmailSender(IOptions<SmtpOptions> options, IReadOnlyPolicyRegistry<string> policies)
        {
            _options = options.Value;
            _policies = policies;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var policy = _policies.Get<IAsyncPolicy>("SmtpBreaker"); // نام از appsettings
            await policy.ExecuteAsync(async ct =>
            {
                using var client = new SmtpClient(_options.Host, _options.Port)
                {
                    Credentials = new NetworkCredential(_options.Username, _options.Password),
                    EnableSsl = _options.EnableSsl
                };
                var mail = new MailMessage(_options.From, to, subject, body);
                await client.SendMailAsync(mail);
            }, CancellationToken.None);
        }
    }
}