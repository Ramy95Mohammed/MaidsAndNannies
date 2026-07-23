using MediatR;
using MaidsAndNannies.Application.Features.Admin.Common;

namespace MaidsAndNannies.Application.Features.Admin.Queries.GetAdminDashboard;

public sealed record GetAdminDashboardQuery : IRequest<AdminDashboardDto>;