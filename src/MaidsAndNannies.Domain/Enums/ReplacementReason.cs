namespace MaidsAndNannies.Domain.Enums;

/// <summary>
/// سبب طلب الاستبدال — يحدد كيفية التعامل مع فرق العمولة.
/// </summary>
public enum ReplacementReason
{
    /// <summary>
    /// تقصير أو مشكلة من العاملة (انسحاب، سوء أداء، عدم مطابقة للمواصفات).
    /// لا تُحتسب عمولة إضافية عن الفترة المتبقية.
    /// </summary>
    WorkerFault = 0,

    /// <summary>
    /// رغبة شخصية من صاحبة المنزل من غير عيب في العاملة.
    /// تُحتسب عمولة جديدة عن الفترة المتبقية فقط.
    /// </summary>
    HomeownerPreference = 1
}
