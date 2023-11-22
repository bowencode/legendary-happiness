using System.Text.Json.Serialization;

namespace Demo.Tokens.Common.Model
{
    public class BasicTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        public string? Scope { get; set; }
    }
}
