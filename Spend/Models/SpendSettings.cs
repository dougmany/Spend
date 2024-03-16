using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spend.Models
{
    public class SpendSettings : ISpendSettings
    {
        public String ReplyToText { get; set; }
        public String ToNumber { get; set; }
        public String VonageBrandName { get; set; }
        public String VonageApiKey { get; set; }
        public String VonageApiSecret { get; set; }

    }
}
