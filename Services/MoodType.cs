using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

[JsonConverter(typeof(MoodTypeConverter))]
public sealed class MoodType : IEquatable<MoodType>
{
    public const string HappyName = "Happy";        // 快乐
    public const string SadName = "Sad";            // 伤心
    public const string NeutralName = "Neutral";    // 中立
    public const string AngryName = "Angry";        // 生气
    public const string ExcitedName = "Excited";    // 兴奋
    public const string AnxiousName = "Anxious";    // 焦虑
    public const string CalmName = "Calm";          // 平静
    public const string BoredName = "Bored";        // 无聊
    public const string BusyName = "Busy";          // 忙碌
    public const string TiredName = "Tired";        // 疲倦
    public const string UnknownName = "Unknown"; // 未知

    public string Name { get; }
    private MoodType(string name) => Name = name;

    public static readonly MoodType Happy = new(HappyName);
    public static readonly MoodType Sad = new(SadName);
    public static readonly MoodType Neutral = new(NeutralName);
    public static readonly MoodType Angry = new(AngryName);
    public static readonly MoodType Excited = new(ExcitedName);
    public static readonly MoodType Anxious = new(AnxiousName);
    public static readonly MoodType Calm = new(CalmName);
    public static readonly MoodType Bored = new(BoredName);
    public static readonly MoodType Busy = new(BusyName);
    public static readonly MoodType Tired = new(TiredName);
    public static readonly MoodType Unknown = new(UnknownName);

    private static readonly Dictionary<string, MoodType> _map = new(10, StringComparer.Ordinal)
    {
        [HappyName]     = Happy,
        [SadName]       = Sad,
        [NeutralName]   = Neutral,
        [AngryName]     = Angry,
        [ExcitedName]   = Excited,
        [AnxiousName]   = Anxious,
        [CalmName]      = Calm,
        [BoredName]     = Bored,
        [BusyName]      = Busy,
        [TiredName]     = Tired,
        [UnknownName]   = Unknown
    };

    private static readonly Dictionary<string, string> _emojiMap = new(10, StringComparer.Ordinal)
    {
        [HappyName]     = "😊",
        [SadName]       = "😢",
        [NeutralName]   = "😐",
        [AngryName]     = "😠",
        [ExcitedName]   = "🥳",
        [AnxiousName]   = "😰",
        [CalmName]      = "😌",
        [BoredName]     = "🥱",
        [BusyName]      = "💼",
        [TiredName]     = "😴",
        [UnknownName]   = "❓"
    };

    public static IReadOnlyList<MoodType> All { get; } =
        new []
        {
            Happy, Sad, Neutral, Angry, Excited,
            Anxious, Calm, Bored, Busy, Tired
        };

    public static MoodType? FromName(string? name) =>
        name is null ? null : _map.GetValueOrDefault(name);

    /* 获取emoji */
    // 1. 给实例调用
    public string GetEmoji() => _emojiMap.GetValueOrDefault(Name)!;

    // 2. 只给字符串 name（静态版本）
    public static string? GetEmoji(string? name) =>
        name is null ? null : _emojiMap.GetValueOrDefault(name);

    // 3. 可选：重载运算符，让 moodTypeInstance.Emoji 一样能点出来
    public string Emoji => GetEmoji();

    public bool Equals(MoodType? other) =>
       other is not null && StringComparer.Ordinal.Equals(Name, other.Name);
    
}

public class MoodTypeConverter : JsonConverter<MoodType>
{
    public override MoodType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var name = reader.GetString();
        return MoodType.FromName(name) ?? MoodType.Unknown;
    }

    public override void Write(Utf8JsonWriter writer, MoodType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Name);
    }
}