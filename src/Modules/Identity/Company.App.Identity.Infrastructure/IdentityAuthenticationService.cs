using Company.App.Identity.Application;
using Microsoft.AspNetCore.Identity;

namespace Company.App.Identity.Infrastructure;

public sealed class IdentityAuthenticationService : IIdentityAuthenticationService
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityAuthenticationService(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<IdentityLoginResult> PasswordSignInAsync(IdentityLoginRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, request.RememberMe, lockoutOnFailure: true);
        return new IdentityLoginResult(result.Succeeded, result.IsLockedOut);
    }

    public Task SignOutAsync()
    {
        return _signInManager.SignOutAsync();
    }
}
