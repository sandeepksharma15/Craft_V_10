// __TENANT__.* is the default if no pattern is provided and selects the first (or only) domain segment
// as the tenant identifier
// *.__TENANT__.? selects the main domain (or second to last) as the tenant identifier
// __TENANT__.example.com will always use the subdomain for the tenant identifier, but only if there
// are no prior subdomains and the overall host ends with "example.com"
// *.__TENANT__.?.? is similar to the above example except it will select the first subdomain even if
// others exist and doesn't require ".com"
// As a special case, a pattern string of just __TENANT__ will use the entire host as the tenant identifier,
// as opposed to a single segment

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Craft.MultiTenant;

public class HostStrategy : ITenantStrategy
{
    private readonly string _regex;

    public HostStrategy(string template = TenantConstants.TenantToken)
    {
        if (template == TenantConstants.TenantToken)
            template = template.Replace(TenantConstants.TenantToken, "(?<identifier>.+)");
        else
        {
            if (template.IsNullOrWhiteSpace())
                throw new MultiTenantException("Template cannot be null or whitespace.");

            // Wildcard "*" must be only occur once in template.
            if (template.Count(c => c == '*') > 1)
                throw new MultiTenantException("Wildcard \"*\" must occur only once in the template.");

            // Wildcard "*" must be only token in template segment.
            if (Regex.IsMatch(template, @"\*[^\.]|[^\.]\*"))
                throw new MultiTenantException("\"*\" wildcard must be only token in template segment.");

            // Wildcard "?" must be only token in template segment.
            if (Regex.IsMatch(template, @"\?[^\.]|[^\.]\?"))
                throw new MultiTenantException("\"?\" wildcard must be only token in template segment.");

            template = template.Trim().Replace(".", @"\.");

            string wildcardSegmentsPattern = @"(\.[^\.]+)*";
            const string singleSegmentPattern = @"[^\.]+";

            if (template.Substring(template.Length - 3, 3) == @"\.*")
                template = string.Concat(template.AsSpan(0, template.Length - 3), wildcardSegmentsPattern);

            wildcardSegmentsPattern = @"([^\.]+\.)*";
            template = template.Replace(@"*\.", wildcardSegmentsPattern);
            template = template.Replace("?", singleSegmentPattern);
            template = template.Replace(TenantConstants.TenantToken, @"(?<identifier>[^\.]+)");
        }

        _regex = $"^{template}$";
    }

    public Task<string?> GetIdentifierAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var host = context.Request.Host;

        if (!host.HasValue)
            return Task.FromResult<string?>(null);

        string? identifier = null;

        var match = Regex.Match(host.Host, _regex,
            RegexOptions.ExplicitCapture, TimeSpan.FromMilliseconds(100));

        if (match.Success)
            identifier = match.Groups["identifier"].Value;

        return Task.FromResult(identifier);
    }
}
