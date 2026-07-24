namespace MaidsAndNannies.Application.Features.Currency.Common;

public sealed record CurrencyDto(
    int Id,
    string Code,
    string Symbol,
    string NameAr,
    string NameEn,
    decimal RateToEgp,
    bool IsActive);

public sealed record CreateCurrencyRequest(
    string Code,
    string Symbol,
    string NameAr,
    string NameEn,
    decimal RateToEgp,
    bool IsActive = true);

public sealed record UpdateCurrencyRequest(
    string Code,
    string Symbol,
    string NameAr,
    string NameEn,
    decimal RateToEgp,
    bool IsActive);