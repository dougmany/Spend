using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spend.Data;
using Spend.Models;
using System.Threading.Tasks;
using Vonage.Messaging;
using Vonage.Utility;
using Vonage.SubAccounts;
using Vonage;
using Vonage.Request;
using Twilio.Http;

namespace Spend.Controllers
{
    public class SmsVController : Controller
    {
        private readonly ILogger<SmsController> _logger;
        private readonly ISpendRepository _repository;
        private readonly ISpendSettings _spendSettings;

        public SmsVController(ILogger<SmsController> logger, ISpendRepository repository, SpendSettings spendSettings)
        {
            _logger = logger;
            _repository = repository;
            _spendSettings = spendSettings;
        }

        [HttpGet("smsv/inbound-sms")]
        public async Task<IActionResult> InboundSmsAsync()
        {
            var sms = WebhookParser.ParseQuery<InboundSms>(Request.Query);
            var message = sms.Text.Split(":");
            var amount = 0m;
            var isNumber = false;

            if (message.Length > 1)
            {
                var amountString = message[1].Replace("$", "");
                isNumber = Decimal.TryParse(amountString, out amount);
            }

            Console.WriteLine($"SMS Received with message: {sms.Text}");

            var entry = new Entry
            {
                FromPhone = sms.Msisdn,
                Entered = DateTime.Now,
                Description = sms.Text,
                Name = message[0],
                Amount = amount.ToString("F2")
            };


            var credentials = Credentials.FromApiKeyAndSecret(
                _spendSettings.VonageApiKey,
                _spendSettings.VonageApiSecret
            );

            await _repository.Create(entry);
            
            var vonageClient = new VonageClient(credentials);

            var responseMessage = "";

            if (isNumber)
            {
                responseMessage = $"Got it: ${amount}";
            }
            else if(sms.Text.Contains("friend") || sms.Text.Contains("help"))
            {

                responseMessage = sms.Text switch
                {
                    string a when a.Contains("friend", StringComparison.OrdinalIgnoreCase) => "Yes",
                    string b when b.Contains("help", StringComparison.OrdinalIgnoreCase) => "OK",
                    _ => "I am not sure what you want"
                };
            }
            else
            {
                responseMessage=$"Um: {message[0]}, {message[1]}";
            }
            
            var response = await vonageClient.SmsClient.SendAnSmsAsync(new Vonage.Messaging.SendSmsRequest()
            {
                To = sms.Msisdn,
                From = _spendSettings.VonageBrandName,
                Text = responseMessage
            });

            return Ok();
        }
    }
}