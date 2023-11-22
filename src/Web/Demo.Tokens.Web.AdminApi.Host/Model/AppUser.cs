namespace Demo.Tokens.Web.AdminApi.Host.Model
{
    public class AppUser
    {
        public string SubjectId { get; set; } = null!;
        public string? Username { get; set; }
        public bool IsActive { get; set; }
        public List<UserMetadata> Claims { get; set; } = new List<UserMetadata>();
    }
}