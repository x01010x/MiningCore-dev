using System.Collections.Concurrent;
using Autofac;

namespace Miningcore.Crypto.Hashing.Progpow;

public static class ProgpowFactory
{
    private static readonly ConcurrentDictionary<string, IProgpowLight> cacheFull = new();

    public static IProgpowLight GetProgpow(IComponentContext ctx, string name)
    {
        if(name == "")
            return null;

        // check cache
        if(cacheFull.TryGetValue(name, out var result))
            return result;

        result = ctx.ResolveNamed<IProgpowLight>(name);

        cacheFull.TryAdd(name, result);

        return result;
    }
}