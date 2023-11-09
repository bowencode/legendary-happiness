using Demo.Notes.Common.Model;

namespace Demo.Notes.Web.Blazor.Client.Pages
{
    public class UserNotesSummary : UserSummary
    {
        public List<NoteData> Notes { get; set; } = new List<NoteData>();
    }
}