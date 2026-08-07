using MaidsAndNannies.Application.Features.AdminSettings.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaidsAndNannies.Application.Features.AdminSettings.Queries.GetSettingByKey
{

    public sealed record GetSettingByKey(string key) : IRequest<SettingDto?>;   
}
