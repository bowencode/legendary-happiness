using Demo.Tokens.Common.Model;

namespace Demo.Tokens.Api.Host.Model;

public class MessageData
{
    public static List<MessageItem> AllMessages { get; } = new List<MessageItem>
    { 
        new MessageItem { Id = "1", From = 1, To = 2, Sent = DateTime.UtcNow.AddMinutes(-10000 * Random.Shared.NextDouble()), Content = "Lorem ipsum" },
        new MessageItem { Id = "2", From = 2, To = 1, Sent = DateTime.UtcNow.AddMinutes(-10000 * Random.Shared.NextDouble()), Content = "Dolor sit amet" },
        new MessageItem { Id = "3", From = 1, To = 2, Sent = DateTime.UtcNow.AddMinutes(-10000 * Random.Shared.NextDouble()), Content = "Consectetur adipiscing elit" },
        new MessageItem { Id = "4", From = 2, To = 3, Sent = DateTime.UtcNow.AddMinutes(-10000 * Random.Shared.NextDouble()), Content = "Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua" },
        new MessageItem { Id = "5", From = 3, To = 1, Sent = DateTime.UtcNow.AddMinutes(-10000 * Random.Shared.NextDouble()), Content = "Ut enim ad minim veniam" },
        new MessageItem { Id = "6", From = 1, To = 3, Sent = DateTime.UtcNow.AddMinutes(-10000 * Random.Shared.NextDouble()), Content = "Quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat" },
        new MessageItem { Id = "7", From = 3, To = 5, Sent = DateTime.UtcNow.AddMinutes(-10000 * Random.Shared.NextDouble()), Content = "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur" },
        new MessageItem { Id = "8", From = 5, To = 3, Sent = DateTime.UtcNow.AddMinutes(-10000 * Random.Shared.NextDouble()), Content = "Excepteur sint occaecat cupidatat non proident" },
        new MessageItem { Id = "9", From = 3, To = 1, Sent = DateTime.UtcNow.AddMinutes(-10000 * Random.Shared.NextDouble()), Content = "Sunt in culpa qui officia deserunt mollit anim id est laborum" },
        new MessageItem { Id = "10", From = 1, To = 4, Sent = DateTime.UtcNow.AddMinutes(-10000 * Random.Shared.NextDouble()), Content = "Lorem ipsum" },
        new MessageItem { Id = "11", From = 4, To = 5, Sent = DateTime.UtcNow.AddMinutes(-10000 * Random.Shared.NextDouble()), Content = "Dolor sit amet" },
        new MessageItem { Id = "12", From = 5, To = 4, Sent = DateTime.UtcNow.AddMinutes(-10000 * Random.Shared.NextDouble()), Content = "Consectetur adipiscing elit" },
        new MessageItem { Id = "13", From = 4, To = 2, Sent = DateTime.UtcNow.AddMinutes(-10000 * Random.Shared.NextDouble()), Content = "Sed do eiusmod tempor incididunt" },
        new MessageItem { Id = "14", From = 5, To = 2, Sent = DateTime.UtcNow.AddMinutes(-10000 * Random.Shared.NextDouble()), Content = "Ut labore et dolore magna aliqua" },
        new MessageItem { Id = "15", From = 4, To = 3, Sent = DateTime.UtcNow.AddMinutes(-10000 * Random.Shared.NextDouble()), Content = "Ut enim ad minim veniam" },
        new MessageItem { Id = "16", From = 1, To = 4, Sent = DateTime.UtcNow.AddMinutes(-10000 * Random.Shared.NextDouble()), Content = "Quis nostrud exercitation ullamco" },
        new MessageItem { Id = "17", From = 2, To = 3, Sent = DateTime.UtcNow.AddMinutes(-10000 * Random.Shared.NextDouble()), Content = "Duis aute irure dolor in reprehenderit" },
        new MessageItem { Id = "18", From = 5, To = 2, Sent = DateTime.UtcNow.AddMinutes(-10000 * Random.Shared.NextDouble()), Content = "Voluptate velit esse cillum dolore" },
    };
}