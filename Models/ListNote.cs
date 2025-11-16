using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberNote.Models
{
    //任务列表类
    public class ListNote : NoteCard
    {
        public string Type { get; } = "List";
        public string Title { get; set; } = "无标题";
        public int Priority { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Schedule { get; set; }
        public DateTime createDate { get; set; }
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();

        public ListNote(string T, int priority, string content, List<TaskItem> tasks)
        {
            Title = T;
            Priority = priority;
            Content = content;

            // 把任务列表接过来，并给自己建立反向引用
            Tasks = tasks;
            Tasks.ForEach(t => t.Owner = this);

            // 初始 Schedule 取第一个未完成任务
            RefreshSchedule();
        }

        /// <summary>
        /// 把 Schedule 设为“下一个未完成任务”的时间；若全部完成则保持原值即可
        /// </summary>
        internal void RefreshSchedule()
        {
            var next = Tasks.FirstOrDefault(t => t.Progress != "已完成");
            if (next != null)
                Schedule = next.Schedule;
            // 全部完成时不再变动 Schedule，也可按需改成 DateTime.MaxValue 等
        }

        public void ShiftPriority(int newPriority) => Priority = newPriority;
        public void UpdateContent(string newContent) => Content = newContent;

    }



    //任务项类
    public class TaskItem
    {
        public DateTime Schedule { get; set; }
        public string Progress { get; set; } = "未完成";
        public string Content { get; set; } = string.Empty;

        // 反向引用，初始化时由 ListNote 注入
        internal ListNote? Owner { get; set; }

        public TaskItem() { }

        public TaskItem(DateTime schedule, string content)
        {
            Schedule = schedule;
            Content = content;
            Progress = "未完成";
        }

        /// <summary>
        /// 切换进度，并在第一次变为“已完成”时通知 Owner 刷新 Schedule
        /// </summary>
        public void ShiftProgress()
        {
            var oldProgress = Progress;

            if (Progress == "未完成")
                Progress = "进行中";
            else if (Progress == "进行中")
                Progress = "已完成";

            // 如果刚刚完成，就通知 Owner
            if (oldProgress != "已完成" && Progress == "已完成")
                Owner?.RefreshSchedule();
        }
    }


}
