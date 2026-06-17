using System.Collections.Concurrent;

namespace ECommerceApp.Middleware
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, LoginAttempt> LoginAttempts = new();
        private const int MaxAttempts = 5;
        private const int LockoutMinutes = 15;

        public RateLimitMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/Account/Login" && context.Request.Method == "POST")
            {
                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                if (IsBlocked(clientIp))
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "Too many login attempts. Please try again later."
                    });
                    return;
                }
            }

            await _next(context);
        }

        private bool IsBlocked(string clientIp)
        {
            if (!LoginAttempts.TryGetValue(clientIp, out var attempt))
                return false;

            if (DateTime.UtcNow > attempt.LockedUntil)
            {
                LoginAttempts.TryRemove(clientIp, out _);
                return false;
            }

            return attempt.Count >= MaxAttempts;
        }

        public static void RecordFailedAttempt(string clientIp)
        {
            LoginAttempts.AddOrUpdate(clientIp,
                new LoginAttempt { Count = 1, LockedUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes) },
                (key, existing) =>
                {
                    existing.Count++;
                    if (existing.Count >= MaxAttempts)
                        existing.LockedUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                    return existing;
                });
        }

        public static void ResetAttempts(string clientIp)
        {
            LoginAttempts.TryRemove(clientIp, out _);
        }

        private class LoginAttempt
        {
            public int Count { get; set; }
            public DateTime LockedUntil { get; set; }
        }
    }
}