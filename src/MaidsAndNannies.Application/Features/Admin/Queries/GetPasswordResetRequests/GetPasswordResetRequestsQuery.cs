using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Admin.Queries.GetPasswordResetRequests;

public sealed record PasswordResetRequestDto(
    int Id,
    string Email,
    string FullName,
    string PhoneNumber,
    string Code,
    PasswordResetStatus Status,
    DateTime CreatedAt,
    DateTime ExpiresAt);

public sealed record GetPasswordResetRequestsQuery() : IRequest<List<PasswordResetRequestDto>>;

public sealed class GetPasswordResetRequestsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetPasswordResetRequestsQuery, List<PasswordResetRequestDto>>
{
    public async Task<List<PasswordResetRequestDto>> Handle(GetPasswordResetRequestsQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var items = await dbContext.PasswordResetRequests
            .Where(r => r.Status == PasswordResetStatus.Pending || r.Status == PasswordResetStatus.Sent)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        var userIds = items.Select(r => r.UserId).Distinct().ToList();
        var users = await dbContext.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName, u.PhoneNumber })
            .ToListAsync(ct);
        var usersById = users.ToDictionary(u => u.Id);

        var result = new List<PasswordResetRequestDto>();
        foreach (var r in items)
        {
            if (r.Status == PasswordResetStatus.Pending && r.ExpiresAt < now)
            {
                r.Status = PasswordResetStatus.Expired;
                r.ResolvedAt = now;
                continue;
            }
            var u = usersById.TryGetValue(r.UserId, out var found) ? found : null;
            result.Add(new PasswordResetRequestDto(
                r.Id, r.Email, u?.FullName ?? string.Empty, u?.PhoneNumber ?? string.Empty,
                r.Code, r.Status, r.CreatedAt, r.ExpiresAt));
        }

        if (items.Any(r => r.Status == PasswordResetStatus.Expired))
            await dbContext.SaveChangesAsync(ct);

        return result;
    }
}