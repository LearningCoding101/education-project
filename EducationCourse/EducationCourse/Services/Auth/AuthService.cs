using EducationCourse.Data;
using EducationCourse.Data.config;
using EducationCourse.Helpers;
using EducationCourse.Models.Auth;
using FluentResults;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Supabase.Gotrue;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

namespace EducationCourse.Services.Auth
{
    public class AuthService
    {
        private const string AccessToken = nameof(AccessToken);
        private const string RefreshToken = nameof(RefreshToken);

        private readonly ProtectedLocalStorage _localStorage;
        private readonly NavigationManager _navigation;
        private readonly IConfiguration _configuration;
        private readonly SupabaseService _supabase;
        private readonly Supabase.Client _client;

        public AuthService(ProtectedLocalStorage localStorage, NavigationManager navigation, IConfiguration configuration, SupabaseService supabase)
        {
            _localStorage = localStorage;
            _navigation = navigation;
            _configuration = configuration;
            _supabase = supabase;
            _client = _supabase.GetClient();

        }

        public async Task<Result> LoginAsync( string email, string password)
        {
            try
            {
                var response = await _client.Auth.SignIn(email, password);
                if (string.IsNullOrEmpty(response?.AccessToken))
                {
                    return Result.Fail("An error occured");
                }

                await _localStorage.SetAsync(AccessToken, response.AccessToken);
                await _localStorage.SetAsync(RefreshToken, response.RefreshToken);

                return Result.Ok();
            } catch (Exception ex)
            {
                var error = JsonSerializer.Deserialize<AuthError>(ex.Message);

                return Result.Fail(GetIgoTrueError(error.error_code));


            }

        }

        public async Task<Result> RegisterAsync(RegisterModel model, string role = "student")
        {
            try
            {
                // Log the incoming registration details
                Console.WriteLine($"RegisterAsync called with Email: {model.Email}, Role: {role}");

                // Prepare sign-up options
                SignUpOptions signUpOptions = new()
                {
                    Data = new Dictionary<string, object>
            {
                { "first_name", model.FirstName },
                { "last_name", model.LastName },
                { "role", role }
            }
                };

                // Log the sign-up options that will be passed to the API
                Console.WriteLine($"SignUpOptions prepared: First Name: {model.FirstName}, Last Name: {model.LastName}, Role: {role}");

                // Call the API to sign up the user
                var data = await _client.Auth.SignUp(model.Email, model.Password, signUpOptions);

                // Log the response from the API
                Console.WriteLine($"API Response: {data}");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                // Log the exception message to understand what went wrong
                Console.WriteLine($"Exception occurred in RegisterAsync: {ex.Message}");

                // Attempt to deserialize the error to get more details
                try
                {
                    var error = JsonSerializer.Deserialize<AuthError>(ex.Message);
                    Console.WriteLine($"Error Details: Code: {error.error_code}");
                }
                catch (JsonException jsonEx)
                {
                    // Log if deserialization fails
                    Console.WriteLine($"Error deserializing exception: {jsonEx.Message}");
                }

                // Return a failure result with a detailed error message
                return Result.Fail(GetIgoTrueError(ex.Message));
            }
        }


        public async Task<Result> ForgotPasswordAsync(string email)
        {
            ResetPasswordForEmailOptions resetPasswordForEmailOptions = new ResetPasswordForEmailOptions(email)
            {
                RedirectTo = $"{_configuration["RedirectUrl"]}account/reset-password",
            };

            await _client.Auth.ResetPasswordForEmail(resetPasswordForEmailOptions);
            return Result.Ok();
        }

        public async Task<Result<User>> UpdatePassword(ResetPasswordModel model)
        {
            try
            {
                UserAttributes userAttributes = new()
                {
                    Password = model.Password
                };

                var session = await _client.Auth.SetSession(model.AccessToken, model.RefreshToken);
                var user = await _client.Auth.Update(userAttributes);

                if (user is null)
                    return Result.Fail("An error occured");

                return Result.Ok(user);
            }
            catch (Exception ex)
            {
                var error = JsonSerializer.Deserialize<AuthError>(ex.Message);

                return Result.Fail<User>(GetIgoTrueError(error.error_code));
            }
        }

        public async Task LogoutAsync()
        {
            await RemoveAuthDataFromStorageAsync();
            Thread.Sleep(300);
            _navigation.NavigateTo("/", true);
        }

        public async Task<List<Claim>> GetLoginInfoAsync()
        {
            var emptyResult = new List<Claim>();
            ProtectedBrowserStorageResult<string> accessToken;
            ProtectedBrowserStorageResult<string> refreshToken;

            try
            {
                accessToken = await _localStorage.GetAsync<string>(AccessToken);
                refreshToken = await _localStorage.GetAsync<string>(RefreshToken);
            }
            catch (CryptographicException)
            {
                await LogoutAsync();
                return emptyResult;
            }

            if (accessToken.Success is false || accessToken.Value == default)
                return emptyResult;

            var claims = JwtTokenHelpers.ValidateDecodeToken(accessToken.Value, _configuration);

            // Add the role claim if it exists in the JWT or Supabase metadata
            var role = claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "student"; // Default role is "student"
            claims.Add(new Claim(ClaimTypes.Role, role));

            if (claims.Count != 0)
                return claims;

            if (refreshToken.Value != default)
            {
                // TODO: Implement refresh token logic
            }
            else
            {
                await LogoutAsync();
            }
            return claims;
        }
        private async Task RemoveAuthDataFromStorageAsync()
        {
            await _localStorage.DeleteAsync(AccessToken);
            await _localStorage.DeleteAsync(RefreshToken);
        }

        private string GetIgoTrueError(string errorCode)
        {
            string cleanMessage = "";

            switch (errorCode)
            {
                case "user_already_exists":
                    cleanMessage = "An account already exists with this email, please log in";
                    break;
                case "weak_password":
                    cleanMessage = "Password is too weak, please enter at least 6 characters (lowercase, uppercase and numbers)";
                    break;
                case "invalid_credentials":
                    cleanMessage = "Incorrect email or password";
                    break;
                case "same_password":
                    cleanMessage = "The new password must be different from the old one";
                    break;
                case "email_not_confirmed":
                    cleanMessage = "Email not confirmed, please check your inbox";
                    break;
            }

            return cleanMessage;
        }
    }






}
