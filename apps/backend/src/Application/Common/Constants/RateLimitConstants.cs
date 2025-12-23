namespace backend.Application.Common.Constants;

public static class RateLimitConstants
{
    public const string DefaultFixedPolicy = "fixed";
    public const string SlidingPolicy = "sliding";
    public const string ConcurrencyPolicy = "concurrency";
    public const string TokenPolicy = "token";
    public const string PerUserPolicy = "per-user-policy";
}
