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

            if (template.Count(c => c == '*') > 1)
                throw new MultiTenantException("Wildcard \"*\" must occur only once in the template.");

            if (Regex.IsMatch(template, @"\*[^\.]|[^\.]\*"))
                throw new MultiTenantException("\"*\" wildcard must be only token in template segment.");

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
