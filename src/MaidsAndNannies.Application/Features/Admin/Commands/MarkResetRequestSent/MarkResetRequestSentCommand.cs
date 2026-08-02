using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Admin.Commands.MarkResetRequestSent;

public sealed record MarkResetRequestSentCommand(int Id) : IRequest<Unit>;

public sealed class MarkResetRequestSentCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<MarkResetRequestSentCommand, Unit>
{
    public async Task<Unit> Handle(MarkResetRequestSentCommand request, CancellationToken ct)
    {
        var entity = await dbContext.PasswordResetRequests
            .FirstOrDefaultAsync(r => r.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("الطلب غير موجود");

        if (entity.Status == PasswordResetStatus.Pending)
        {
            entity.Status = PasswordResetStatus.Sent;
            await dbContext.SaveChangesAsync(ct);
        }

        return Unit.Value;
    }
}