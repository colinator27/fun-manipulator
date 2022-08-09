using SFML.Window;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace FunManipulator;

public class Input
{
    public enum InputKind
    {
        Keyboard,
        Mouse
    }

    public InputKind Kind { get; set; }
    public Keyboard.Key KeyboardKey { get; set; }
    public Mouse.Button MouseButton { get; set; }

    public Input(Keyboard.Key keyboardKey)
    {
        Kind = InputKind.Keyboard;
        KeyboardKey = keyboardKey;
    }

    public Input(Mouse.Button mouseButton)
    {
        Kind = InputKind.Mouse;
        MouseButton = mouseButton;
    }

    public bool CheckInput()
    {
        switch (Kind)
        {
            case InputKind.Keyboard:
                return Keyboard.IsKeyPressed(KeyboardKey);
            case InputKind.Mouse:
                return Mouse.IsButtonPressed(MouseButton);
        }
        throw new Exception();
    }

    public override string ToString()
    {
        switch (Kind)
        {
            case InputKind.Keyboard:
                return KeyboardKey.ToString();
            case InputKind.Mouse:
                return MouseButton.ToString();
        }
        throw new Exception();
    }
}

public class InputConverter : JsonConverter<Input>
{
    private static Regex keyboardRegex = new Regex(@"Keyboard:([a-zA-Z\d]+)", RegexOptions.Compiled);
    private static Regex mouseRegex = new Regex(@"Mouse:([a-zA-Z\d]+)", RegexOptions.Compiled);

    public override Input? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string str = reader.GetString()!;

        Match keyboardMatch = keyboardRegex.Match(str);
        if (keyboardMatch.Success)
            return new Input(Enum.Parse<Keyboard.Key>(keyboardMatch.Groups[1].Value));

        Match mouseMatch = mouseRegex.Match(str);
        if (mouseMatch.Success)
            return new Input(Enum.Parse<Mouse.Button>(mouseMatch.Groups[1].Value));

        throw new JsonException("Invalid input");
    }

    public override void Write(Utf8JsonWriter writer, Input value, JsonSerializerOptions options)
    {
        switch (value.Kind)
        {
            case Input.InputKind.Keyboard:
                writer.WriteStringValue($"Keyboard:{value.KeyboardKey}");
                break;
            case Input.InputKind.Mouse:
                writer.WriteStringValue($"Mouse:{value.MouseButton}");
                break;
        }
    }
}

