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

            if (message.Length > 1)
            {
                Decimal.TryParse(message[1].Replace("$", ""), out amount);
            }

            var entry = new Entry
            {
                FromPhone = request.From,
                Entered = DateTime.Now,
                Description = request.Body,
                Name = message[0],
                Amount = amount
            };

            await _repository.Create(entry);

            var response = new MessagingResponse();
            if (amount > 0m)
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