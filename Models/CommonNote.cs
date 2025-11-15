using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberNote.Models
{
    public class CommonNote : NoteCard
    {
        public string Type { get; } = "common";
        public DateTime Schedule { get; set; }
        public DateTime createDate { get; set; }
        public string Progress { get; set; } = "未完成";
        public int Priority { get; set; }
        public string Content { get; set; } = string.Empty;

        //无参构造函数
        public CommonNote(){ }
        //有参构造函数
        public CommonNote(DateTime schedule, int priority, string content)
        {
            Schedule = schedule;
            Priority = priority;
            Content = content;
            Progress = "未完成"; // 默认状态
        }
        //切换完成情况
        public void shift_progress()
        {
            if (Progress == "未完成")
            {
                Progress = "进行中";
            }
            else if (Progress == "进行中")
            {
                Progress = "已完成";
            }
          
        }
        //重置完成情况
        public void reset_progress()
        {
            Progress = "未完成";
        }
        //修改优先级
        public void shift_priority(int new_priority)
        {
            Priority = new_priority;
        }
        //修改内容
        public void update_content(string new_content)
        {
            Content = new_content;
        }
        //修改时间
        public void update_schedule(DateTime new_schedule)
        {
            Schedule = new_schedule;
        }

    }
}
