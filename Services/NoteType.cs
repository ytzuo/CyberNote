internal sealed class NoteType : IEquatable<NoteType>
{
    public const string CommonName = "Common";
    public const string ListName = "List";
    public const string RichTextName = "RichText";

    /* ---------- 1. 焊死的三个实例 ---------- */
    public static readonly NoteType Common = new(CommonName);
    public static readonly NoteType List = new(ListName);
    public static readonly NoteType RichText = new(RichTextName);

    /* ---------- 2. 焊死的字典 ---------- */
    private static readonly Dictionary<string, NoteType> _map = new(3, StringComparer.Ordinal)
    {
        [CommonName] = Common,
        [ListName] = List,
        [RichTextName] = RichText
    };

    public static IReadOnlyList<NoteType> All { get; } =
        new[] { Common, List, RichText };

    /* ---------- 3. 解析 ---------- */
    public static NoteType? FromName(string? name) =>
        name is null ? null : (_map.TryGetValue(name, out var v) ? v : null);

    /* ---------- 4. 数据 ---------- */
    public string Name { get; }

    private NoteType(string name) => Name = name;

    /* ---------- 5. 相等性 ---------- */
    public bool Equals(NoteType? other) =>
        other is not null && StringComparer.Ordinal.Equals(Name, other.Name);

    public override bool Equals(object? obj) => Equals(obj as NoteType);
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Name);
    public override string ToString() => Name;

    public static bool operator ==(NoteType? l, NoteType? r) => Equals(l, r);
    public static bool operator !=(NoteType? l, NoteType? r) => !Equals(l, r);
}
