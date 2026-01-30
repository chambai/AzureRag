using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;

internal sealed class TestFunctionContext : FunctionContext
{
    private readonly IServiceProvider _services;
    private readonly IInvocationFeatures _features = new TestInvocationFeatures();

    public TestFunctionContext()
    {
        _services = new ServiceCollection().BuildServiceProvider();
    }

    public override string InvocationId { get; } = Guid.NewGuid().ToString();
    public override string FunctionId { get; } = "test-function";

    public override IServiceProvider InstanceServices
    {
        get => _services;
        set { }
    }

    public override IInvocationFeatures Features => _features;

    public override FunctionDefinition FunctionDefinition { get; }
        = new TestFunctionDefinition();

    public override TraceContext TraceContext => null!;
    public override BindingContext BindingContext => null!;
    public override CancellationToken CancellationToken => CancellationToken.None;

    public override RetryContext RetryContext => null!;
    public override IDictionary<object, object> Items { get; set; }
        = new Dictionary<object, object>();
}