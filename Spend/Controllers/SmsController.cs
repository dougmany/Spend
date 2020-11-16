using System;
using Microsoft.AspNetCore.Mvc;
using Twilio.AspNet.Core;
using Twilio.TwiML;

namespace Spend.Controllers
{
    public class SmsController : TwilioController
    {
        [HttpPost]
        public TwiMLResult Index()
        {
            var messagingResponse = new MessagingResponse();
            messagingResponse.Message("The Robots are coming! Head for the hills!");

            return TwiML(messagingResponse);
        }
    }
}