using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public sealed class DateName : IEquatable<DateName>
{
    /* -------------- 月份 -------------- */
    private static readonly string[] MonthNames =
    {
        "Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
    };

    /* -------------- 星期 -------------- */
    private static readonly string[] WeekNames =
       {
        "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"
    };

    /* -------------- 缓存实例 -------------- */
    private static readonly DateName[] MonthCache = new DateName[12];
    private static readonly DateName[] WeekCache = new DateName[7];

    static DateName()
    {
        for (int i = 0; i < 12; i++) MonthCache[i] = new DateName(MonthNames[i]);
        for (int i = 0; i < 7; i++) WeekCache[i] = new DateName(WeekNames[i]);
    }

    /* -------------- 对外工厂 -------------- */
    public static DateName FromMonth(int month)     // 1~12月
    {
        if (month is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(month));
        return MonthCache[month - 1];
    }

    public static DateName FromDayOfWeek(int day)   // 0~6  0=Sunday
    {
        if (day is < 0 or > 6) throw new ArgumentOutOfRangeException(nameof(day));
        return WeekCache[day];
    }

    public static DateName FromDayOfWeek(DayOfWeek dow) => FromDayOfWeek((int)dow);

    public string Name { get; }
    private DateName(string name) => Name = name;

    /* -------------- 比较逻辑 -------------- */
    public bool Equals(DateName? other) => other is not null && Name == other.Name;
    public override bool Equals(object? obj) => obj is DateName dn && Equals(dn);
    public override int GetHashCode() => Name.GetHashCode();
    public static bool operator ==(DateName? left, DateName? right) => Equals(left, right);
    public static bool operator !=(DateName? left, DateName? right) => !Equals(left, right);

    /* -------------- 其他 -------------- */
    public override string ToString() => Name;
}