using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CyberNote.Models;

namespace CyberNote.Services
{
    public static class JsonWriter
    {
        public static void AppendNote(string filePath, NoteCard note)
        {
            if (note == null) throw new ArgumentNullException(nameof(note));

            List<NoteCard> notes = File.Exists(filePath)
                ? JsonReader.LoadAllCard(filePath)
                : new List<NoteCard>();

            notes.Add(note);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
            };

            var json = JsonSerializer.Serialize(notes, typeof(List<NoteCard>), options);
            File.WriteAllText(filePath, json);
        }
    }
}