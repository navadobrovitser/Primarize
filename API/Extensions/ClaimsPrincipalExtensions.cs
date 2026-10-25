using System;
using System.Security.Claims;

namespace API.Extensions // התאימי את שם ה-Namespace לפרויקט שלך
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(value, out var id))
                throw new InvalidOperationException("User id claim is missing.");

            return id;
        }
    }
}