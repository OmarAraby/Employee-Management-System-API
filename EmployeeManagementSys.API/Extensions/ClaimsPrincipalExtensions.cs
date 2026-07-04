using System.Security.Claims;

namespace EmployeeManagementSys.API.Extensions
{
    /// <summary>
    /// Helpers for reading the authenticated user's claims at the API boundary,
    /// giving the business layer a non-null contract for authorization decisions.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// The caller's role, or an empty string when no role claim is present.
        /// Never null — a missing role fails closed against the managers'
        /// <c>userRole != "Admin"</c>-style checks rather than throwing.
        /// </summary>
        public static string GetRole(this ClaimsPrincipal user)
            => user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        /// <summary>
        /// The caller's id from the NameIdentifier claim.
        /// Returns <c>false</c> (with <see cref="Guid.Empty"/>) when the claim
        /// is absent or unparsable, so callers can fail closed.
        /// </summary>
        public static bool TryGetUserId(this ClaimsPrincipal user, out Guid userId)
            => Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId);
    }
}
