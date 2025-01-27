using EducationCourse.Components;
using Microsoft.Extensions.Options;
using Supabase;
using static Supabase.Gotrue.Constants;
using FluentResults;
using System;

namespace EducationCourse.Data.config
{
    public class SupabaseService
    {
        private readonly Client _client;
        private readonly SupabaseSetting _setting;

        public SupabaseService(IOptions<SupabaseSetting> setting)
        {
            _setting = setting.Value;

            // Log the configuration values for debugging (but avoid logging sensitive data like the AnonKey in production).
            Console.WriteLine($"Initializing Supabase Client with URL: {_setting.Url}");

            // Initialize the Supabase client.
            _client = new Client(_setting.Url, _setting.AnonKey);

            try
            {
                _client.InitializeAsync().Wait();
                Console.WriteLine("Successfully connected to Supabase!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to Supabase: {ex.Message}");
                throw; // Rethrow the exception to avoid hiding connection issues.
            }
        }

        public Client GetClient() => _client;
    }
}
