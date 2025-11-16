using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberNote.Models
{
    interface NoteCard
    {
        string Type { get; }
        string Title { get; set; }
        string Content { get; set; }
    }
}
