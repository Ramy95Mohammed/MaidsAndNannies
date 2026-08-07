using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.AdminSettings.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaidsAndNannies.Application.Features.AdminSettings.Queries.GetSettingByKey
{
    public class GetSettingByKeyHandler(IApplicationDbContext dbContext) : IRequestHandler<GetSettingByKey, SettingDto?>
    {
        public async Task<SettingDto?> Handle(GetSettingByKey request, CancellationToken cancellationToken)
        {
            var setting = await dbContext.AppSettings.FirstOrDefaultAsync(s => s.Key == request.key);
            if (setting != null)
                return new SettingDto(setting.Key, setting.Value, setting.Description);
            return null;                
        }
    }
}
