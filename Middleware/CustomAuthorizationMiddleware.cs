using System.Security.Claims;

namespace MovieTicketBooking.Middleware
{
    public class CustomAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // STEP 1: Allow public authentication endpoints
            var path = context.Request.Path.Value?.ToLowerInvariant();

            if (path == "/api/auth/register" ||
                path == "/api/auth/login")
            {
                await _next(context);
                return;
            }

            // STEP 2: Check whether the user is authenticated
            if (context.User.Identity == null ||
                !context.User.Identity.IsAuthenticated)
            {
                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                await context.Response.WriteAsJsonAsync(new
                {
                    message = "You must log in to access this resource."
                });

                return;
            }

            // STEP 3: Read the user's role from JWT claims
            var role = context.User.FindFirst(ClaimTypes.Role)?.Value
                       ?? context.User.FindFirst("role")?.Value;

            // STEP 4: Admin-only endpoint protection
            if (path != null &&
                path.StartsWith("/api/admin"))
            {
                if (role != "Admin")
                {
                    context.Response.StatusCode =
                        StatusCodes.Status403Forbidden;

                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "Access denied. Admin permission required."
                    });

                    return;
                }
            }

            // STEP 5: Read the logged-in user's ID from JWT claims
            var loggedInUserId =
                context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? context.User.FindFirst("sub")?.Value
                ?? context.User.FindFirst("userId")?.Value;

            // STEP 6: Example ownership check
            // Protects URLs such as /api/customers/5
            var pathSegments = path?.Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries);

            if (pathSegments != null &&
                pathSegments.Length == 3 &&
                pathSegments[0] == "api" &&
                pathSegments[1] == "customers")
            {
                var requestedCustomerId = pathSegments[2];

                if (loggedInUserId != requestedCustomerId)
                {
                    context.Response.StatusCode =
                        StatusCodes.Status403Forbidden;

                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "You cannot access another customer's data."
                    });

                    return;
                }
            }

            // STEP 7: Authorization passed.
            // Continue to the next middleware.
            await _next(context);
        }
    }
}