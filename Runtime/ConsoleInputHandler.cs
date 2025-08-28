using System;
using System.Collections.Generic;
using System.Text;

namespace ECEngine.Runtime;

/// <summary>
/// Advanced console input handler with cursor movement, history navigation, and line editing support
/// </summary>
public class ConsoleInputHandler
{
    private readonly List<string> _history;
    private int _historyIndex;
    private string _currentLine;
    private int _cursorPosition;
    private readonly int _promptLength;

    public ConsoleInputHandler(List<string> history, string prompt = "ec> ")
    {
        _history = history ?? new List<string>();
        _historyIndex = _history.Count;
        _currentLine = "";
        _cursorPosition = 0;
        _promptLength = prompt.Length;
    }

    /// <summary>
    /// Read a line of input with full cursor support
    /// </summary>
    public string? ReadLine()
    {
        _currentLine = "";
        _cursorPosition = 0;
        _historyIndex = _history.Count;

        while (true)
        {
            var keyInfo = Console.ReadKey(true);

            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    var result = _currentLine;
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        // Add to history if it's different from the last entry
                        if (_history.Count == 0 || _history[_history.Count - 1] != result)
                        {
                            _history.Add(result);
                        }
                    }
                    return result;

                case ConsoleKey.Backspace:
                    HandleBackspace();
                    break;

                case ConsoleKey.Delete:
                    HandleDelete();
                    break;

                case ConsoleKey.LeftArrow:
                    HandleLeftArrow();
                    break;

                case ConsoleKey.RightArrow:
                    HandleRightArrow();
                    break;

                case ConsoleKey.UpArrow:
                    HandleUpArrow();
                    break;

                case ConsoleKey.DownArrow:
                    HandleDownArrow();
                    break;

                case ConsoleKey.Home:
                    HandleHome();
                    break;

                case ConsoleKey.End:
                    HandleEnd();
                    break;

                case ConsoleKey.Tab:
                    // Could implement auto-completion here in the future
                    break;

                case ConsoleKey.Escape:
                    HandleEscape();
                    break;

                default:
                    if (!char.IsControl(keyInfo.KeyChar))
                    {
                        HandleCharacterInput(keyInfo.KeyChar);
                    }
                    break;
            }
        }
    }

    private void HandleCharacterInput(char c)
    {
        _currentLine = _currentLine.Insert(_cursorPosition, c.ToString());
        _cursorPosition++;
        RedrawLine();
    }

    private void HandleBackspace()
    {
        if (_cursorPosition > 0)
        {
            _currentLine = _currentLine.Remove(_cursorPosition - 1, 1);
            _cursorPosition--;
            RedrawLine();
        }
    }

    private void HandleDelete()
    {
        if (_cursorPosition < _currentLine.Length)
        {
            _currentLine = _currentLine.Remove(_cursorPosition, 1);
            RedrawLine();
        }
    }

    private void HandleLeftArrow()
    {
        if (_cursorPosition > 0)
        {
            _cursorPosition--;
            UpdateCursorPosition();
        }
    }

    private void HandleRightArrow()
    {
        if (_cursorPosition < _currentLine.Length)
        {
            _cursorPosition++;
            UpdateCursorPosition();
        }
    }

    private void HandleUpArrow()
    {
        if (_historyIndex > 0)
        {
            _historyIndex--;
            _currentLine = _history[_historyIndex];
            _cursorPosition = _currentLine.Length;
            RedrawLine();
        }
    }

    private void HandleDownArrow()
    {
        if (_historyIndex < _history.Count - 1)
        {
            _historyIndex++;
            _currentLine = _history[_historyIndex];
            _cursorPosition = _currentLine.Length;
            RedrawLine();
        }
        else if (_historyIndex < _history.Count)
        {
            _historyIndex = _history.Count;
            _currentLine = "";
            _cursorPosition = 0;
            RedrawLine();
        }
    }

    private void HandleHome()
    {
        _cursorPosition = 0;
        UpdateCursorPosition();
    }

    private void HandleEnd()
    {
        _cursorPosition = _currentLine.Length;
        UpdateCursorPosition();
    }

    private void HandleEscape()
    {
        _currentLine = "";
        _cursorPosition = 0;
        RedrawLine();
    }

    private void RedrawLine()
    {
        // Clear the current line
        Console.Write('\r');
        Console.Write(new string(' ', Console.WindowWidth - 1));
        Console.Write('\r');

        // Write the prompt and current line
        Console.Write("ec> " + _currentLine);

        // Position cursor correctly
        UpdateCursorPosition();
    }

    private void UpdateCursorPosition()
    {
        var targetColumn = _promptLength + _cursorPosition;
        Console.SetCursorPosition(targetColumn, Console.CursorTop);
    }
}
