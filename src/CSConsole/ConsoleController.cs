using Mono.CSharp;
using System.Collections;
using System.Text;
using UnityExplorer.UI;
using UnityExplorer.UI.Panels;
using UniverseLib.Input;
using UniverseLib.UI.Models;

#nullable enable

namespace UnityExplorer.CSConsole;

public class ConsoleController
{
    private readonly ConsoleScriptEvaluator _evaluator;
    private readonly LexerBuilder _lexer;
    private readonly CSAutoCompleter _completer;
    private readonly HashSet<string> usingDirectives = [];

    private bool sreNotSupported { get; set; }
    private int lastCaretPosition { get;  set; }

    public static float DefaultInputFieldAlpha
    {
        set
        {
            if (_instance is null)
            {
                return;
            }
            _instance.defaultInputFieldAlpha = value;
        }
    }
    private float defaultInputFieldAlpha;

    private bool enableCtrlRShortcut { get; set; } = true;
    private bool enableAutoIndent { get; set; } = true;
    private bool enableSuggestions { get; set; } = true;

    private float timeOfLastCtrlR;

    private bool settingCaretCoroutine;
    private string previousInput = "";
    private int previousContentLength = 0;

    private static CSConsolePanel _panel => UIManager.GetPanel<CSConsolePanel>(UIManager.Panels.CSConsole);
    public static InputFieldRef Input => _panel.Input;

    public static string ScriptsFolder => Path.Combine(ExplorerCore.ExplorerFolder, "Scripts");

    static readonly string[] DefaultUsing = [

        "System",
        "System.Linq",
        "System.Text",
        "System.Collections",
        "System.Collections.Generic",
        "System.Reflection",
        "UnityEngine",
        "UniverseLib",
#if CPP
#if INTEROP
        "Il2CppInterop.Runtime",
        "Il2CppInterop.Runtime.Attributes",
        "Il2CppInterop.Runtime.Injection",
        "Il2CppInterop.Runtime.InteropTypes.Arrays",
#else
        "UnhollowerBaseLib",
        "UnhollowerRuntimeLib",
#endif
#endif
    ];

    const int CSCONSOLE_LINEHEIGHT = 18;

    private static ConsoleController? _instance { get; set; }

    public ConsoleController()
    {
        _evaluator = new ConsoleScriptEvaluator();
        // Setup console
        _lexer = new LexerBuilder();
        _completer = new CSAutoCompleter();

        SetupHelpInteraction();

        try
        {
            ResetConsole(false);
            // ensure the compiler is supported (if this fails then SRE is probably stripped)
            _evaluator.Compile("0 == 0");
        }
        catch (Exception ex)
        {
            DisableConsole(ex);
            return;
        }

        _panel.OnInputChanged += OnInputChanged;
        _panel.InputScroller.OnScroll += OnInputScrolled;
        _panel.OnCompileClicked += Evaluate;
        _panel.OnResetClicked += ResetConsole;
        _panel.OnDropdownChanged += SelectedDropDown;
        _panel.OnAutoIndentToggled += OnToggleAutoIndent;
        _panel.OnCtrlRToggled += OnToggleCtrlRShortcut;
        _panel.OnSuggestionsToggled += OnToggleSuggestions;
        _panel.OnPanelResized += OnInputScrolled;

        // Run startup script
        try
        {
            if (!Directory.Exists(ScriptsFolder))
            {
                Directory.CreateDirectory(ScriptsFolder);
            }

            string startupPath = Path.Combine(ScriptsFolder, "startup.cs");
            if (File.Exists(startupPath))
            {
                int index = codes.FindIndex(x => x.Label == "startup.cs");
                if (index <= 0)
                {
                    throw new Exception("Can't loaded script");
                }
                SelectedDropDown(index);
                Evaluate();
            }
        }
        catch (Exception ex)
        {
            ExplorerCore.LogWarning($"Exception executing startup script: {ex}");
        }
    }

    public static void Init()
    {
        _instance = new ConsoleController();
    }

    public static string[]? GetCompletions(string inputs, out string prefix)
    {
        prefix = "";
        return _instance?._evaluator.GetCompletions(inputs, out prefix);
    }
    public static void Update()
        => _instance?.UpdateImp();

    #region Evaluating

    public void ResetConsole() => ResetConsole(true);

    public void ResetConsole(bool logSuccess = true)
    {
        if (sreNotSupported)
        {
            return;
        }

        _evaluator.Recreate();

        usingDirectives.Clear();
        foreach (string use in DefaultUsing)
        {
            AddUsing(use);
        }

        ReloadAndRefreshScript();

        if (logSuccess)
        {
            ExplorerCore.Log($"C# Console reset");//. Using directives:\r\n{Evaluator.GetUsing()}");
        }
    }

    public void AddUsing(string assemblyName)
    {
        if (!usingDirectives.Contains(assemblyName))
        {
            Evaluate($"using {assemblyName};", true);
            usingDirectives.Add(assemblyName);
        }
    }

    public void Evaluate()
    {
        if (sreNotSupported)
        {
            return;
        }

        string text = Input.Text;
        Evaluate(text);
        TryUpdateScript(text);
    }

    public void Evaluate(string input, bool supressLog = false)
    {
        if (sreNotSupported)
            return;

        _evaluator.Initialize();

        try
        {
            Compile(input, supressLog);
        }
        catch (FormatException fex)
        {
            if (!supressLog)
            {
                ExplorerCore.LogWarning(fex.Message);
            }
        }
        catch (Exception ex)
        {
            if (!supressLog)
            {
                ExplorerCore.LogWarning(ex);
            }
        }
    }

    private void Compile(string input, bool supressLog = false)
    {
        // Compile the code. If it returned a CompiledMethod, it is REPL.
        CompiledMethod? repl = _evaluator.Compile(input);

        if (repl != null)
        {
            REPLInvoke(repl);
        }
        else
        {
            CsCompile(supressLog);
        }
    }

    private void REPLInvoke(CompiledMethod repl)
    {
        // Valid REPL, we have a delegate to the evaluation.
        try
        {
            object? ret = null;
            repl.Invoke(ref ret);
            string? result = ret?.ToString();
            if (!string.IsNullOrEmpty(result))
            {
                ExplorerCore.Log($"Invoked REPL, result: {ret}");
            }
            else
            {
                ExplorerCore.Log($"Invoked REPL (no return value)");
            }
        }
        catch (Exception ex)
        {
            ExplorerCore.LogWarning($"Exception invoking REPL: {ex}");
        }
    }

    private void CsCompile(bool supressLog = false)
    {
        // The compiled code was not REPL, so it was a using directive or it defined classes.

        string? output = _evaluator.ToString();
        if (output == null ||
            string.IsNullOrEmpty(output))
        {
            return;
        }

        string[] outputSplit = output.Split('\n');
        if (outputSplit.Length >= 2)
        {
            output = outputSplit[outputSplit.Length - 2];
        }

        _evaluator.ClearOutput();

        if (ScriptEvaluator._reportPrinter?.ErrorsCount > 0)
        {
            throw new FormatException($"Unable to compile the code. Evaluator's last output was:\r\n{output}");
        }
        else if (!supressLog)
        {
            ExplorerCore.Log($"Code compiled without errors.");
        }
    }

    private void TryUpdateScript(string newScript)
    {
        int selected = _panel.Dropdown.value;

        if (selected <= 4 || codes.Count <= selected)
        {
            return;
        }


        ExplorerCore.Log($"Try Update exists code....");

        var code = codes[selected];

        string file = code.Label;
        code.Code = newScript;

        string path = Path.Combine(ScriptsFolder, file);
        if (!File.Exists(path))
        {
            ExplorerCore.Log($"File not found!!");
            return;
        }
        try
        {
            File.WriteAllText(path, newScript);
            ExplorerCore.Log($"Success!! override");
        }
        catch
        {
            ExplorerCore.LogWarning("Can't override code");
        }
    }

    #endregion


    #region Update loop and event listeners

    public void UpdateImp()
    {
        if (sreNotSupported)
        {
            return;
        }

        if (!InputManager.GetKey(KeyCode.LeftControl) && !InputManager.GetKey(KeyCode.RightControl))
        {
            if (InputManager.GetKeyDown(KeyCode.Home))
            {
                JumpToStartOrEndOfLine(true);
            }
            else if (InputManager.GetKeyDown(KeyCode.End))
            {
                JumpToStartOrEndOfLine(false);
            }
        }

        UpdateCaret(out bool caretMoved);

        if (!settingCaretCoroutine && enableSuggestions)
        {
            if (AutoCompleteModal.CheckEscape(_completer))
            {
                OnAutocompleteEscaped();
                return;
            }

            if (caretMoved)
            {
                AutoCompleteModal.Instance.ReleaseOwnership(_completer);
            }
        }

        if (enableCtrlRShortcut
            && (InputManager.GetKey(KeyCode.LeftControl) || InputManager.GetKey(KeyCode.RightControl))
            && InputManager.GetKeyDown(KeyCode.R)
            && timeOfLastCtrlR.OccuredEarlierThanDefault())
        {
            timeOfLastCtrlR = Time.realtimeSinceStartup;
            Evaluate();
        }
    }

    private void OnInputScrolled() => HighlightVisibleInput(out _);

    private void OnInputChanged(string value)
    {
        if (sreNotSupported)
        {
            return;
        }

        // prevent escape wiping input
        if (InputManager.GetKeyDown(KeyCode.Escape))
        {
            Input.Text = previousInput;

            if (enableSuggestions && AutoCompleteModal.CheckEscape(_completer))
                OnAutocompleteEscaped();

            return;
        }

        previousInput = value;

        if (enableSuggestions && AutoCompleteModal.CheckEnter(_completer))
        {
            OnAutocompleteEnter();
        }

        if (!settingCaretCoroutine && enableAutoIndent)
        {
            DoAutoIndent();
        }

        HighlightVisibleInput(out bool inStringOrComment);

        if (!settingCaretCoroutine && enableSuggestions)
        {
            if (inStringOrComment)
            {
                AutoCompleteModal.Instance.ReleaseOwnership(_completer);
            }
            else
            {

                _completer.CheckAutocompletes();
            }
        }

        UpdateCaret(out _);
    }

    private void OnToggleAutoIndent(bool value)
    {
        enableAutoIndent = value;
    }

    private void OnToggleCtrlRShortcut(bool value)
    {
        enableCtrlRShortcut = value;
    }

    private void OnToggleSuggestions(bool value)
    {
        enableSuggestions = value;
    }

    #endregion


    #region Caret position

    private void UpdateCaret(out bool caretMoved)
    {
        int prevCaret = lastCaretPosition;
        caretMoved = false;

        // Override up/down arrow movement when autocompleting
        if (enableSuggestions && AutoCompleteModal.CheckNavigation(_completer))
        {
            Input.Component.caretPosition = lastCaretPosition;
            return;
        }

        if (Input.Component.isFocused)
        {
            lastCaretPosition = Input.Component.caretPosition;
            caretMoved = lastCaretPosition != prevCaret;
        }

        if (Input.Text.Length == 0)
        {
            return;
        }

        // If caret moved, ensure caret is visible in the viewport
        if (caretMoved)
        {
            UICharInfo charInfo = Input.TextGenerator.characters[lastCaretPosition];
            float charTop = charInfo.cursorPos.y;
            float charBot = charTop - CSCONSOLE_LINEHEIGHT;

            float viewportMin = Input.Transform.rect.height - Input.Transform.anchoredPosition.y - (Input.Transform.rect.height * 0.5f);
            float viewportMax = viewportMin - _panel.InputScroller.ViewportRect.rect.height;

            float diff = 0;
            if (charTop > viewportMin)
            {
                diff = charTop - viewportMin;
            }
            else if (charBot < viewportMax)
            {
                diff = charBot - viewportMax;
            }

            if (Math.Abs(diff) > 1)
            {
                RectTransform rect = Input.Transform;
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y - diff);
            }
        }
    }

    public void SetCaretPosition(int caretPosition)
    {
        Input.Component.caretPosition = caretPosition;

        // Fix to make sure we always really set the caret position.
        // Yields a frame and fixes text-selection issues.
        settingCaretCoroutine = true;
        Input.Component.readOnly = true;
        RuntimeHelper.StartCoroutine(DoSetCaretCoroutine(caretPosition));
    }

    private IEnumerator DoSetCaretCoroutine(int caretPosition)
    {
        Color color = Input.Component.selectionColor;
        color.a = 0f;
        Input.Component.selectionColor = color;

        EventSystemHelper.SetSelectionGuard(false);
        Input.Component.Select();

        yield return null; // ~~~~~~~ YIELD FRAME ~~~~~~~~~

        Input.Component.caretPosition = caretPosition;
        Input.Component.selectionFocusPosition = caretPosition;
        lastCaretPosition = Input.Component.caretPosition;

        color.a = defaultInputFieldAlpha;
        Input.Component.selectionColor = color;

        Input.Component.readOnly = false;
        settingCaretCoroutine = false;
    }

    // For Home and End keys
    private void JumpToStartOrEndOfLine(bool toStart)
    {
        // Determine the current and next line
        UILineInfo thisline = default;
        UILineInfo? nextLine = null;
        for (int i = 0; i < Input.Component.cachedInputTextGenerator.lineCount; i++)
        {
            UILineInfo line = Input.Component.cachedInputTextGenerator.lines[i];

            if (line.startCharIdx > lastCaretPosition)
            {
                nextLine = line;
                break;
            }
            thisline = line;
        }

        if (toStart)
        {
            // Determine where the indented text begins
            int endOfLine = nextLine == null ? Input.Text.Length : nextLine.Value.startCharIdx;
            int indentedStart = thisline.startCharIdx;
            while (indentedStart < endOfLine - 1 && char.IsWhiteSpace(Input.Text[indentedStart]))
            {
                indentedStart++;
            }

            // Jump to either the true start or the non-whitespace position,
            // depending on which one we are not at.
            SetCaretPosition(
                lastCaretPosition == indentedStart ?
                thisline.startCharIdx : indentedStart);
        }
        else
        {
            // If there is no next line, jump to the end of this line (+1, to the invisible next character position)
            // jump to the next line start index - 1, ie. end of this line
            SetCaretPosition(
                nextLine == null ?
                Input.Text.Length : nextLine.Value.startCharIdx - 1);
        }
    }

    #endregion


    #region Lexer Highlighting

    private void HighlightVisibleInput(out bool inStringOrComment)
    {
        inStringOrComment = false;
        if (string.IsNullOrEmpty(Input.Text))
        {
            _panel.HighlightText.text = "";
            _panel.LineNumberText.text = "1";
            return;
        }

        // Calculate the visible lines

        int topLine = -1;
        int bottomLine = -1;

        // the top and bottom position of the viewport in relation to the text height
        // they need the half-height adjustment to normalize against the 'line.topY' value.
        float viewportMin = Input.Transform.rect.height - Input.Transform.anchoredPosition.y - (Input.Transform.rect.height * 0.5f);
        float viewportMax = viewportMin - _panel.InputScroller.ViewportRect.rect.height;

        for (int i = 0; i < Input.TextGenerator.lineCount; i++)
        {
            UILineInfo line = Input.TextGenerator.lines[i];
            // if not set the top line yet, and top of line is below the viewport top
            if (topLine == -1 && line.topY <= viewportMin)
            {
                topLine = i;
            }
            // if bottom of line is below the viewport bottom
            if ((line.topY - line.height) >= viewportMax)
            {
                bottomLine = i;
            }
        }

        topLine = Math.Max(0, topLine - 1);
        bottomLine = Math.Min(Input.TextGenerator.lineCount - 1, bottomLine + 1);

        int startIdx = Input.TextGenerator.lines[topLine].startCharIdx;
        int endIdx = (bottomLine >= Input.TextGenerator.lineCount - 1)
            ? Input.Text.Length - 1
            : (Input.TextGenerator.lines[bottomLine + 1].startCharIdx - 1);


        // Highlight the visible text with the LexerBuilder

        _panel.HighlightText.text = _lexer.BuildHighlightedString(Input.Text, startIdx, endIdx, topLine, lastCaretPosition, out inStringOrComment);

        // Set the line numbers

        // determine true starting line number (not the same as the cached TextGenerator line numbers)
        int realStartLine = 0;
        for (int i = 0; i < startIdx; i++)
        {
            if (LexerBuilder.IsNewLine(Input.Text[i]))
            {
                realStartLine++;
            }
        }
        realStartLine++;
        char lastPrev = '\n';

        StringBuilder sb = new();

        // append leading new lines for spacing (no point rendering line numbers we cant see)
        for (int i = 0; i < topLine; i++)
        {
            sb.Append('\n');
        }

        // append the displayed line numbers
        for (int i = topLine; i <= bottomLine; i++)
        {
            if (i > 0)
            {
                lastPrev = Input.Text[Input.TextGenerator.lines[i].startCharIdx - 1];
            }

            // previous line ended with a newline character, this is an actual new line.
            if (LexerBuilder.IsNewLine(lastPrev))
            {
                sb.Append(realStartLine.ToString());
                realStartLine++;
            }

            sb.Append('\n');
        }

        _panel.LineNumberText.text = sb.ToString();
    }

    #endregion


    #region Autocompletes

    public static void InsertSuggestionAtCaret(string suggestion)
        => _instance?.InsertSuggestionAtCaretImp(suggestion);

    private void InsertSuggestionAtCaretImp(string suggestion)
    {
        settingCaretCoroutine = true;
        Input.Text = Input.Text.Insert(lastCaretPosition, suggestion);

        SetCaretPosition(lastCaretPosition + suggestion.Length);
        lastCaretPosition = Input.Component.caretPosition;
    }

    private void OnAutocompleteEnter()
    {
        // Remove the new line
        int lastIdx = Input.Component.caretPosition - 1;
        Input.Text = Input.Text.Remove(lastIdx, 1);

        // Use the selected suggestion
        Input.Component.caretPosition = lastCaretPosition;
        _completer.OnSuggestionClicked(AutoCompleteModal.SelectedSuggestion);
    }

    private void OnAutocompleteEscaped()
    {
        AutoCompleteModal.Instance.ReleaseOwnership(_completer);
        SetCaretPosition(lastCaretPosition);
    }


    #endregion


    #region Auto indenting

    private void DoAutoIndent()
    {
        if (Input.Text.Length > previousContentLength)
        {
            int inc = Input.Text.Length - previousContentLength;

            if (inc == 1)
            {
                int caret = Input.Component.caretPosition;
                Input.Text = _lexer.IndentCharacter(Input.Text, ref caret);
                Input.Component.caretPosition = caret;
                lastCaretPosition = caret;
            }
            else
            {
                // todo indenting for copy+pasted content

                //ExplorerCore.Log("Content increased by " + inc);
                //var comp = Input.Text.Substring(PreviousCaretPosition, inc);
                //ExplorerCore.Log("composition string: " + comp);
            }
        }

        previousContentLength = Input.Text.Length;
    }

    #endregion


    #region "Help" interaction

    private void DisableConsole(Exception ex)
    {
        sreNotSupported = true;
        Input.Component.readOnly = true;
        Input.Component.textComponent.color = "5d8556".ToColor();

        if (ex is NotSupportedException)
        {
            Input.Text = $@"The C# Console has been disabled because System.Reflection.Emit threw a NotSupportedException.

Easy, dirty fix: (will likely break on game updates)
    * Download the corlibs for the game's Unity version from here: https://unity.bepinex.dev/corlibs/
    * Unzip and copy mscorlib.dll (and System.Reflection.Emit DLLs, if present) from the folder
    * Paste and overwrite the files into <Game>_Data/Managed/

With UnityDoorstop: (BepInEx only, or if you use UnityDoorstop + Standalone release):
    * Download the corlibs for the game's Unity version from here: https://unity.bepinex.dev/corlibs/
    * Unzip and copy mscorlib.dll (and System.Reflection.Emit DLLs, if present) from the folder
    * Find the folder which contains doorstop_config.ini (the game folder, or your r2modman/ThunderstoreModManager profile folder)
    * Make a subfolder called 'corlibs' inside this folder.
    * Paste the DLLs inside the corlibs folder.
    * In doorstop_config.ini, set 'dllSearchPathOverride=corlibs'.

Doorstop example:
- <Game>\
    - <Game>_Data\...
    - BepInEx\...
    - corlibs\
        - mscorlib.dll
    - doorstop_config.ini (with dllSearchPathOverride=corlibs)
    - <Game>.exe
    - winhttp.dll";
        }
        else
        {
            Input.Text = $"The C# Console has been disabled. {ex}";
        }
    }

    private class CodeInfo(string label, string code)
    {
        public string Label { get; } = label;
        public string Code { get; set; } = code;
    }
    private readonly List<CodeInfo> codes = [
        new CodeInfo("Welcome", STARTUP_TEXT),
        new CodeInfo("Usings", HELP_USINGS),
        new CodeInfo("REPL", HELP_REPL),
        new CodeInfo("Classes", HELP_CLASSES),
        new CodeInfo("Coroutines", HELP_COROUTINES)
   ];

    public void SetupHelpInteraction()
    {
        Dropdown drop = _panel.Dropdown;

        foreach (var c in codes)
        {
            drop.options.Add(new Dropdown.OptionData(c.Label));
        }

        ReloadAndRefreshScript();
    }

    public void SelectedDropDown(int index)
    {
        if (codes.Count <= index)
        {
            return;
        }

        Input.Text = codes[index].Code;
    }

    public void ReloadAndRefreshScript()
    {
        if (!Directory.Exists(ScriptsFolder))
        {
            return;
        }

        string[] files = Directory.GetFiles(ScriptsFolder, "*.cs", SearchOption.AllDirectories);
        if (files.Length == 0)
        {
            return;
        }

        ExplorerCore.Log($"Reloading all script...");

        var help = _panel.Dropdown;

        int prevSelect = help.value;
        int size = help.options.Count;

        if (size > 5)
        {
            help.options.RemoveRange(5, size - 5);
        }
        size = codes.Count;
        if (size > 5)
        {
            codes.RemoveRange(5, size - 5);
        }

        foreach (string file in files)
        {
            try
            {
                ExplorerCore.Log($"loading... : {file}");
                string label = Path.GetFileName(file); // ファイル名のみ表示
                string code = File.ReadAllText(
                    Path.Combine(ScriptsFolder, file));

                codes.Add(new CodeInfo(label, code));
                help.options.Add(new Dropdown.OptionData(label));
            }
            catch
            {
                ExplorerCore.LogWarning($"fail load : {file}");
            }
        }
        SelectedDropDown(prevSelect);
    }

    internal const string STARTUP_TEXT = @"<color=#5d8556>// Welcome to the UnityExplorer C# Console!

// It is recommended to use the Log panel (or a console log window) while using this tool.
// Use the Help dropdown to see detailed examples of how to use the console.

// To execute a script automatically on startup, put the script at 'sinai-dev-UnityExplorer\Scripts\startup.cs'</color>";

    internal const string HELP_USINGS = @"// You can add a using directive to any namespace, but you must compile for it to take effect.
// It will remain in effect until you Reset the console.
using UnityEngine.UI;

// To see your current usings, use the ""GetUsing();"" helper.
// Note: You cannot add usings and evaluate REPL at the same time.";

    internal const string HELP_REPL = @"/* REPL (Read-Evaluate-Print-Loop) is a way to execute code immediately.
 * REPL code cannot contain any using directives or classes.
 * The return value of the last line of your REPL will be printed to the log.
 * Variables defined in REPL will exist until you Reset the console.
*/

// eg: This code would print 'Hello, World!', and then print 6 as the return value.
Log(""Hello, world!"");
var x = 5;
++x;

/* The following helpers are available in REPL mode:
 * CurrentTarget;     - System.Object, the target of the active Inspector tab
 * AllTargets;        - System.Object[], the targets of all Inspector tabs
 * Log(obj);          - prints a message to the console log
 * Inspect(obj);      - inspect the object with the Inspector
 * Inspect(someType); - inspect a Type with static reflection
 * Start(enumerator); - Coroutine, starts the IEnumerator as a Coroutine, and returns the Coroutine.
 * Stop(coroutine);   - stop the Coroutine ONLY if it was started with Start(ienumerator).
 * Copy(obj);         - copies the object to the UnityExplorer Clipboard
 * Paste();           - System.Object, the contents of the Clipboard.
 * GetUsing();        - prints the current using directives to the console log
 * GetVars();         - prints the names and values of the REPL variables you have defined
 * GetClasses();      - prints the names and members of the classes you have defined
 * help;              - the default REPL help command, contains additional helpers.
*/";

    internal const string HELP_CLASSES = @"// Classes you compile will exist until the application closes.
// You can soft-overwrite a class by compiling it again with the same name. The old class will still technically exist in memory.

// Compiled classes can be accessed from both inside and outside this console.
// Note: in IL2CPP, you must declare a Namespace to inject these classes with ClassInjector or it will crash the game.

public class HelloWorld
{
    public static void Main()
    {
        UnityExplorer.ExplorerCore.Log(""Hello, world!"");
    }
}

// In REPL, you could call the example method above with ""HelloWorld.Main();""
// Note: The compiler does not allow you to run REPL code and define classes at the same time.

// In REPL, use the ""GetClasses();"" helper to see the classes you have defined since the last Reset.";

    internal const string HELP_COROUTINES = @"// To start a Coroutine directly, use ""Start(SomeCoroutine());"" in REPL mode.

// To declare a coroutine, you will need to compile it separately. For example:
public class MyCoro
{
    public static IEnumerator Main()
    {
        yield return null;
        UnityExplorer.ExplorerCore.Log(""Hello, world after one frame!"");
    }
}
// To run this Coroutine in REPL, it would look like ""Start(MyCoro.Main());""";

    #endregion
}
