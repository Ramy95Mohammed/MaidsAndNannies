using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.AdminSettings.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.AdminSettings.Commands.UpdateSettings;

public sealed class UpdateSettingsCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<UpdateSettingsCommand, Unit>
{
    public async Task<Unit> Handle(UpdateSettingsCommand request, CancellationToken ct)
    {
        foreach (var item in request.Settings)
        {
            var setting = await dbContext.AppSettings
                .FirstOrDefaultAsync(s => s.Key == item.Key, ct);

            if (setting is null)
            {
                dbContext.AppSettings.Add(new Domain.Entities.AppSetting
                {
                    Key = item.Key,
                    Value = item.Value,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = request.UpdatedBy
                });
            }
            else
            {
                setting.Value = item.Value;
                setting.UpdatedAt = DateTime.UtcNow;
                setting.UpdatedBy = request.UpdatedBy;
            }
        }

        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}