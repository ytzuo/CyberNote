using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace CyberNote.Models
{
    public interface NoteCard
    {
        string Type { get; }
        string Title { get; set; }
        string Content { get; set; }
        public JsonObject toJson();
    }
}
