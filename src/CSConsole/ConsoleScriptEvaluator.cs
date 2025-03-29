using Mono.CSharp;

#nullable enable

namespace UnityExplorer.CSConsole;

public sealed class ConsoleScriptEvaluator
{
    private ScriptEvaluator? _evaluator;
    private ScriptEvaluatorResult? _result;

    public override string? ToString()
        => _evaluator?.ToString();

    public string[]? GetCompletions(string inputs, out string prefix)
    {
        prefix = string.Empty;
        return _evaluator?.GetCompletions(inputs, out prefix);
    }

    public void ClearOutput()
        => _result?.Clear();

    public CompiledMethod? Compile(string text)
        => _evaluator?.Compile(text);

    public void Initialize()
    {
        if (_evaluator is null)
        {
            Recreate();
        }

        if (_evaluator is null)
        {
            ExplorerCore.LogError("Can't init evaluator");
            return;
        }

        if (_result is null ||
            _result.IsNull())
        {
            _result = new ScriptEvaluatorResult();
            _evaluator.TextWriter = _result.Writer;
        }
    }

    public void Recreate()
    {
        _evaluator?.Dispose();

        _result = new ScriptEvaluatorResult();
        _evaluator = new ScriptEvaluator(_result.Writer)
        {
            InteractiveBaseClass = typeof(ScriptInteraction)
        };
    }
}
