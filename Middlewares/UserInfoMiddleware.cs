using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestAPI.Helpers;

namespace RestAPI.Middlewares
{
    public class UserInfoMiddleware
    {
        private readonly RequestDelegate _next;

        public UserInfoMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                string? userId = context.User.FindFirst("userId")?.Value;
                string? role = context.User.FindFirst("position")?.Value;

                if (userId != null && role != null)
                {
                    var userInfo = new UserInfo
                    {
                        UserId = userId,
                        Role = role
                    };
                    // Store userInfo in HttpContext
                    context.Items["UserInfo"] = userInfo; // ADD THIS LINE
                }
            }
            await _next(context);
        }
    }

}