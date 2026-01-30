using Microsoft.Azure.Functions.Worker;
using System.Collections.Immutable;

internal sealed class TestFunctionDefinition : FunctionDefinition
{
    public override string PathToAssembly => string.Empty;
    public override string EntryPoint => string.Empty;
    public override string Id => "test";
    public override string Name => "TestFunction";
    public override IImmutableDictionary<string, BindingMetadata> InputBindings =>
        ImmutableDictionary<string, BindingMetadata>.Empty;
    public override IImmutableDictionary<string, BindingMetadata> OutputBindings =>
        ImmutableDictionary<string, BindingMetadata>.Empty;
    public override ImmutableArray<FunctionParameter> Parameters =>
        ImmutableArray<FunctionParameter>.Empty;
}