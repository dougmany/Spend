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
        public DbDecimal Amount { get; set; }
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

    public struct DbDecimal
    {
        public static implicit operator decimal(DbDecimal v) => v.Value;
        public static implicit operator DbDecimal(decimal v) => new DbDecimal(v);

        public decimal Value { get; set; }

        public DbDecimal(decimal? v) => Value = (decimal)v;
    }

    internal class DbDecimalTypeHandler : SqlMapper.ITypeHandler
    {
        public void SetValue(IDbDataParameter parameter, object value)
        {
            parameter.Value = value;
        }

        public object Parse(Type destinationType, object value)
        {
            if (value == DBNull.Value || value == null)
                return null;

            var valueType = value.GetType();

            switch (value)
            {
                case decimal decimalValue:
                    return new DbDecimal(decimalValue);
                case int intValue:
                    return new DbDecimal(Convert.ToDecimal(intValue));
                case Int64 intValue:
                    return new DbDecimal(Convert.ToDecimal(intValue));
                case double doubleValue:
                    return new DbDecimal(Convert.ToDecimal(doubleValue));
                default:
                    throw new DataException();
            }
        }
    }
}