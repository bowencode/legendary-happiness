namespace Demo.Tokens.Common.Model;

public class UserMessage
{
    public string Id { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public System.DateTime Sent { get; set; }
    public string Content { get; set; }
}