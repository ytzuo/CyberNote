using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace CyberNote.Models
{
    public class CommonNote : NoteCard
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; } = "Common";
        public string Title { get; set; } = "无标题";
        public DateTime Schedule { get; set; }
        public DateTime createDate { get; set; }
        public bool Progress { get; set; } = false;
        public int Priority { get; set; }
        public string Content { get; set; } = string.Empty;

        //无参构造函数
        public CommonNote(){ }
        //有参构造函数
        public CommonNote(string id, string title, DateTime schedule, int priority, string content)
        {
            Id = id;
            Title = title;
            Schedule = schedule;
            Priority = priority;
            Content = content;
            Progress = false; // 默认状态
        }
        // Backward-compatible overload auto-generating id
        public CommonNote(string title, DateTime schedule, int priority, string content)
            : this(Guid.NewGuid().ToString(), title, schedule, priority, content) { }

        //切换完成情况
        public void shift_progress() => Progress = !Progress;
        //修改优先级
        public void shift_priority(int new_priority) => Priority = new_priority;
        //修改内容
        public void update_content(string new_content) => Content = new_content;
        //修改时间
        public void update_schedule(DateTime new_schedule) => Schedule = new_schedule;

        public JsonObject toJson() => new JsonObject
        {
            ["id"] = Id,
            ["Type"] = Type,
            ["Title"] = Title,
            ["Schedule"] = Schedule,
            ["createDate"] = createDate,
            ["Progress"] = Progress,
            ["Priority"] = Priority,
            ["Content"] = Content
        };
    }
}
