using Mono.CSharp;
using System.Collections;
#nullable enable

namespace UnityExplorer.CSConsole;

public sealed class ConsoleScriptEvaluator
{
    private FieldInfo? moduleInfo;
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
        if (moduleInfo is null)
        {
            moduleInfo = typeof(Evaluator).GetField("module", BindingFlags.NonPublic | BindingFlags.Instance);
        }

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

    public bool InvokeLastMain(out string error)
    {
        error = string.Empty;
        if (moduleInfo is null)
        {
            error = "Can't get FieldInfo 'Evaluator.module'";
            return false;
        }
        ModuleContainer? module = moduleInfo.GetValue(_evaluator) as ModuleContainer;
        if (module is null)
        {
            error = "Can't get module from evaluator";
            return false;
        }

        Assembly assembly = module.Builder.Assembly;
        foreach (var type in assembly.GetTypes())
        {
            var method = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                             .FirstOrDefault(m => m.Name == "Main");
            if (method is null) continue;

            var parameters = method.GetParameters(); 
            try
            {
                if (method.ReturnType == typeof(void) || method.ReturnType == typeof(int))
                {
                    if (parameters.Length == 0)
                    {
                        method.Invoke(null, null);
                        return true;
                    }
                    else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string[]))
                    {
                        method.Invoke(null, new object[] { new string[0] });
                        return true;
                    }
                }else if(method.ReturnType == typeof(IEnumerator))
                {
                    RuntimeHelper.StartCoroutine((IEnumerator)method.Invoke(null, null));
                    return true;
                }
                
            }
            catch (Exception e)
            {
                error = e.ToString();
                return false;
            }
        }
        // error = "Can't find Main method to invoke.";
        return false;
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
