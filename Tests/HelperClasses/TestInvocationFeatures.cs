using Microsoft.Azure.Functions.Worker;
using System.Collections;

internal sealed class TestInvocationFeatures : IInvocationFeatures
{
    private readonly Dictionary<Type, object> _features = new();

    T IInvocationFeatures.Get<T>()
    {
        return _features.TryGetValue(typeof(T), out var feature)
        ? (T)feature!
        : default(T)!;
    }


    void IInvocationFeatures.Set<T>(T instance)
    {
        _features[typeof(T)] = instance!;
    }

    public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
        => _features.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}