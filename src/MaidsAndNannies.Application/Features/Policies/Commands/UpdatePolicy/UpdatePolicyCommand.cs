using MaidsAndNannies.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Policies.Commands.UpdatePolicy;

public sealed record UpdatePolicyCommand(
    string Key,
    string TitleAr,
    string TitleEn,
    string ContentAr,
    string ContentEn,
    int SortOrder,
    bool IsActive) : IRequest<Unit>;

public sealed class UpdatePolicyCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<UpdatePolicyCommand, Unit>
{
    public async Task<Unit> Handle(UpdatePolicyCommand request, CancellationToken ct)
    {
        var policy = await dbContext.Policies
            .FirstOrDefaultAsync(p => p.Key == request.Key, ct)
            ?? throw new KeyNotFoundException("السياسة غير موجودة");

        policy.TitleAr = request.TitleAr;
        policy.TitleEn = request.TitleEn;
        policy.ContentAr = request.ContentAr;
        policy.ContentEn = request.ContentEn;
        policy.SortOrder = request.SortOrder;
        policy.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}