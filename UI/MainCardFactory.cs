using CyberNote.Models;
using CyberNote.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CyberNote.UI
{
    /* 根据一个缩略选项卡创建一个主卡片的工厂 */
    internal static class MainCardFactory
    {
        private static readonly Dictionary<string, Func<NoteCard, object>> _creators =
            new Dictionary<string, Func<NoteCard, object>>(StringComparer.OrdinalIgnoreCase)
            {
                ["common"] = data => new CommonCardView((CommonNote)data),
                ["list"] = data => new ListCardView((ListNote)data),
                ["richText"] = data => new RichTextCardView((RichTextNote)data),
                // 新增类型只需在这里添加一行
            };

        public static FrameworkElement Create(NoteCard note)
        {
            if (note is null) throw new ArgumentNullException(nameof(note));
            var key = note.Type?.ToLowerInvariant() ?? string.Empty;
            if (_creators.TryGetValue(key, out var builder))
                return (FrameworkElement)builder(note);
            throw new InvalidOperationException($"未注册的卡片类型：{note.Type}");
        }
        
        /* 允许在程序运行时（或初始化阶段）向工厂中“添加”新的卡片类型及其对应的 UI 构建方式，而无需修改工厂类本身的源代码。*/     
        public static void Register(string typeKey, Func<NoteCard, FrameworkElement> builder)
        {
            if (string.IsNullOrWhiteSpace(typeKey)) throw new ArgumentException("类型键不能为空", nameof(typeKey));
            _creators[typeKey] = builder ?? throw new ArgumentNullException(nameof(builder));
        }
    }
}
