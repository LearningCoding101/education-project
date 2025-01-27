using EducationCourse.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace EducationCourse.Providers.Auth
{
    public class SupabaseAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly AuthService _authService;

        public SupabaseAuthenticationStateProvider(AuthService authService)
        {
            _authService = authService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Get the claims from the AuthService
                var claims = await _authService.GetLoginInfoAsync();

                // Add the role claim if it exists in the user metadata
                var role = claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "student"; // Default role is "student"
                claims.Add(new Claim(ClaimTypes.Role, role));

                // Create a ClaimsIdentity and ClaimsPrincipal
                var claimsIdentity = claims.Count != 0
                    ? new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme, "name", "role")
                    : new ClaimsIdentity();

                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Return the AuthenticationState
                return new AuthenticationState(claimsPrincipal);
            }
            catch (Exception)
            {
                // Handle errors (e.g., user is not authenticated)
                return new AuthenticationState(new ClaimsPrincipal());
            }
        }
    }
}