using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spend.Data;
using Spend.Models;
using System.Threading.Tasks;
using Twilio.AspNet.Common;
using Twilio.AspNet.Core;
using Twilio.TwiML;

namespace Spend.Controllers
{
    public class SmsController : TwilioController
    {
        private readonly ILogger<SmsController> _logger;
        private readonly ISpendRepository _repository;

        public SmsController(ILogger<SmsController> logger, ISpendRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        [HttpPost]
        public async Task<TwiMLResult> Create(SmsRequest request)
        {
            var message = request.Body.Split(":");
            var amount = 0m;
            var isNumber = false;

            if (message.Length > 1)
            {
                var amountString = message[1].Replace("$", "");
                isNumber = Decimal.TryParse(amountString, out amount);
            }

            var entry = new Entry
            {
                FromPhone = request.From,
                Entered = DateTime.Now,
                Description = request.Body,
                Name = message[0],
                Amount = amount.ToString("F2")
            };

            await _repository.Create(entry);

            var response = new MessagingResponse();
            if (isNumber)
            {
                response.Message($"Got it: ${amount}");
            }
            else
            {
                response.Message($"Um: {message[0]}, {message[1]}");
            }

            return TwiML(response);
        }
    }
}