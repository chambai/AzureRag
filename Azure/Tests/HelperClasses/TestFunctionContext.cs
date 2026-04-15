using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;

public class TestFunctionContext : FunctionContext
{
    private readonly IServiceProvider _services;
    private readonly IInvocationFeatures _features = new TestInvocationFeatures();
    private readonly CancellationToken _cancellationToken;

    // Default constructor (no cancellation)
    public TestFunctionContext()
        : this(CancellationToken.None)
    {
    }

    // New constructor that accepts a token
    public TestFunctionContext(CancellationToken cancellationToken)
    {
        _services = new ServiceCollection().BuildServiceProvider();
        _cancellationToken = cancellationToken;
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

    // Now returns the injected token
    public override CancellationToken CancellationToken => _cancellationToken;

    public override RetryContext RetryContext => null!;

    public override IDictionary<object, object> Items { get; set; }
        = new Dictionary<object, object>();
}
