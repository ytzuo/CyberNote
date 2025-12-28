using System.Text.Json.Nodes;

namespace CyberNote.Models
{
    public interface NoteCard
    {
        string Id { get; set; }
        string Type { get; }
        string Title { get; set; }
        string Content { get; set; }
        public DateTime createDate { get; set; }
        //object Priority { get; set; }

        public JsonObject toJson();
    }
}
