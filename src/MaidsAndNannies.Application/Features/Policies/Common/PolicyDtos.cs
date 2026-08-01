namespace MaidsAndNannies.Application.Features.Policies.Common;

public sealed record PolicyDto(
    string Key,
    string TitleAr,
    string TitleEn,
    string ContentAr,
    string ContentEn,
    int SortOrder);