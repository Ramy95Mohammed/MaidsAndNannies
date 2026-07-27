using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.AdminSettings.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.AdminSettings.Queries.GetSettings;

public sealed class GetSettingsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetSettingsQuery, IReadOnlyList<SettingDto>>
{
    public async Task<IReadOnlyList<SettingDto>> Handle(GetSettingsQuery request, CancellationToken ct)
        => await dbContext.AppSettings
            .OrderBy(s => s.Key)
            .Select(s => new SettingDto(s.Key, s.Value, s.Description))
            .ToListAsync(ct);
}