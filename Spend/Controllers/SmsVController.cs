using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spend.Data;
using Spend.Models;
using System.Threading.Tasks;
using Vonage.Messaging;
using Vonage.Utility;

namespace Spend.Controllers
{
    public class SmsVController : Controller
    {
        private readonly ILogger<SmsVController> _logger;
        private readonly ISpendRepository _repository;

        public SmsVController(ILogger<SmsVController> logger, ISpendRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        [HttpGet("smsv/inbound-sms")]
        public async Task<IActionResult> InboundSmsAsync()
        {
            var sms = WebhookParser.ParseQuery<InboundSms>(Request.Query);

            if (string.IsNullOrWhiteSpace(sms?.Text))
            {
                _logger.LogWarning("Received SMS with empty or null text.");
                return Ok();
            }

            _logger.LogInformation("SMS received: {Text} from {From}", sms.Text, sms.Msisdn);

            var parts = sms.Text.Split(":", 2);
            string name;
            decimal amount;

            if (parts.Length >= 2)
            {
                // Standard format: description:amount
                var amountString = parts[1].Replace("$", "").Trim();
                if (!Decimal.TryParse(amountString, out amount))
                {
                    _logger.LogWarning("SMS ignored — could not parse amount '{AmountString}' from: {Text}", amountString, sms.Text);
                    return Ok();
                }
                name = parts[0].Trim();
            }
            else
            {
                // Fallback: find a number anywhere in the text
                var match = Regex.Match(sms.Text, @"\$?([\d]+(?:\.[\d]+)?)");
                if (!match.Success || !Decimal.TryParse(match.Groups[1].Value, out amount))
                {
                    _logger.LogWarning("SMS ignored — no amount found. Expected 'description:amount'. Got: {Text}", sms.Text);
                    return Ok();
                }
                name = sms.Text.Substring(0, match.Index).Trim().TrimEnd('-', ':', '_', ' ');
                if (string.IsNullOrWhiteSpace(name))
                    name = sms.Text;
                _logger.LogInformation("SMS parsed via fallback (no ':' separator): Name={Name}, Amount={Amount}", name, amount);
            }

            var entry = new Entry
            {
                FromPhone = sms.Msisdn,
                Entered = DateTime.UtcNow,
                Description = sms.Text,
                Name = name,
                Amount = amount.ToString("F2")
            };

            await _repository.Create(entry);
            return Ok();
        }
    }
}
