namespace backend.Application.Common.Constants;

public static class RateLimitConstants
{
    public const string FixedPolicy = "fixed";
    public const string SlidingPolicy = "sliding";
    public const string ConcurrencyPolicy = "concurrency";
    public const string TokenPolicy = "token";
    public const string UserPolicy = "user-policy";
}
