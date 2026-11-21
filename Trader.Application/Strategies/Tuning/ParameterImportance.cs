// Trader.Application/Strategies/Tuning/ParameterImportance.cs
namespace Trader.Application.Strategies.Tuning;

public enum ParameterImportance
{
    Structural = 0, Noise = 1, Optional = 2, Important = 3, Critical = 4,
}

public static class ParameterImportanceLabels
{
    public static string GetLabel(this ParameterImportance i) => i switch
    {
        ParameterImportance.Structural => "ساختاری",
        ParameterImportance.Noise => "کم‌اثر",
        ParameterImportance.Optional => "اختیاری",
        ParameterImportance.Important => "مهم",
        ParameterImportance.Critical => "حیاتی",
        _ => "—",
    };
}