namespace Demo.Tokens.Api.Host.Model;

public class MessageItem
{
    public string Id { get; set; }
    public int From { get; set; }
    public int To { get; set; }
    public DateTime Sent { get; set; }
    public string Content { get; set; }
}
