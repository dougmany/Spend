using Dapper;
using System;
using System.Data;

namespace Spend.Models
{
    public class Entry
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public Decimal Amount { get; set; }
        public DateTime Entered { get; set; }
        public String FromPhone { get; set; }

        public String DateString
        {
            get
            {
                return Entered.AddHours(-8).ToString("g");
            }
        }
    }
}