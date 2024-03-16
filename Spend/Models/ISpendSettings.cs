namespace Spend.Models
{
    public interface ISpendSettings
    {
        string ReplyToText { get; set; }
        string ToNumber { get; set; }
        string VonageApiKey { get; set; }
        string VonageApiSecret { get; set; }
        string VonageBrandName { get; set; }
    }
}