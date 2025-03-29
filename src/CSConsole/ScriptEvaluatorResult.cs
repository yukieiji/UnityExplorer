using System.Text;

#nullable enable

namespace UnityExplorer.CSConsole;

public sealed class ScriptEvaluatorResult
{
    private readonly StringBuilder _output;
    public StringWriter Writer { get; }

    public ScriptEvaluatorResult()
    {
        _output = new StringBuilder();
        Writer = new StringWriter(_output);
    }

    public void Clear()
        => _output.Clear();

    public bool IsNull()
        => _output == null || Writer == null;
}
