using Demo.Tokens.Common.Model;

namespace Demo.Tokens.Web.Blazor.Client.Pages
{
    public class UserNotesSummary : UserSummary
    {
        public List<NoteData> Notes { get; set; } = new List<NoteData>();
    }
}