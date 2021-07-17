namespace Microsoft.AspNetCore.Builder;

public static class MapFallbackExtensions
{
    // TODO: https://github.com/dotnet/aspnetcore/issues/34450
    public static IEndpointConventionBuilder MapFallback(this IEndpointRouteBuilder routes, Delegate action)
    {
        return routes.MapFallback(RequestDelegateFactory.Create(action, routes.ServiceProvider));
    }
}
