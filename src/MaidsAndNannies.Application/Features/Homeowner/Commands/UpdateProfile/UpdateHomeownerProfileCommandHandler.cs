using FluentValidation.Results;
using MaidsAndNannies.Application.Common.Exceptions;
using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Contracts;
using MaidsAndNannies.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Homeowner.Commands.UpdateProfile;

public sealed class UpdateHomeownerProfileCommandHandler(
    IApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IFileStorage fileStorage)
    : IRequestHandler<UpdateHomeownerProfileCommand, Unit>
{ 
    public async Task<Unit> Handle(UpdateHomeownerProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await dbContext.HomeownerProfiles            
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile is null)
            throw new NotFoundException("HomeownerProfile", request.UserId);

        if (request.Address is not null) profile.Address = request.Address;
        if (request.City is not null) profile.City = request.City;
        if (request.State is not null) profile.State = request.State;
        if (request.WhatsAppNumber is not null) profile.WhatsAppNumber = request.WhatsAppNumber;
        if (request.District is not null) profile.District = request.District;
        if (request.NationalIdNumber is not null) profile.NationalIdNumber = request.NationalIdNumber;

        if (request.NationalIdImageContent is not null && request.NationalIdImageFileName is not null)
            profile.NationalIdImage = await ReplaceFileAsync(
                profile.NationalIdImage, request.NationalIdImageContent, request.NationalIdImageFileName, request.UserId, cancellationToken);

        if (request.SelfieImageContent is not null && request.SelfieImageFileName is not null)
            profile.SelfieImage = await ReplaceFileAsync(
                profile.SelfieImage, request.SelfieImageContent, request.SelfieImageFileName, request.UserId, cancellationToken);

        if (request.ProofOfAddressImageContent is not null && request.ProofOfAddressImageFileName is not null)
            profile.ProofOfAddressImage = await ReplaceFileAsync(
                profile.ProofOfAddressImage, request.ProofOfAddressImageContent, request.ProofOfAddressImageFileName, request.UserId, cancellationToken);

        profile.UpdatedAt = DateTime.UtcNow;         
        await dbContext.SaveChangesAsync(cancellationToken);


        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null) throw new NotFoundException("User", request.UserId);
        if (request.FullName is not null) user.FullName = request.FullName;            
        if (request.PhoneNumber is not null) user.PhoneNumber = request.PhoneNumber;            
        
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new ValidationException(result.Errors.Select(e => new ValidationFailure {ErrorMessage = e.Description }));        
        return Unit.Value;
    }

    private async Task<string> ReplaceFileAsync(string? oldUrl, Stream content, string fileName, string userId, CancellationToken ct)
    {
        var newUrl = await fileStorage.SavePublicAsync(content, fileName, $"homeowners/{userId}", ct);
        if (!string.IsNullOrEmpty(oldUrl))
            fileStorage.DeletePublic(oldUrl);
        return newUrl;
    }
}