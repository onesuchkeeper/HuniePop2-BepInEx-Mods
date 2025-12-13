using System;
using Hp2BaseMod;
namespace HuniePopUltimate;

public class TextWriter : ITextWriter
{
    public void ResetColor()
    {
        //throw new NotImplementedException();
    }

    public void SetColor(ConsoleColor color)
    {
        //throw new NotImplementedException();
    }

    public void WriteLine(string value) => ModInterface.Log.Message(value);
}