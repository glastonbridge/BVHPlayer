
using System.IO;

public class Tokeniser
{

  protected int index;
  protected StringReader data;

  public Tokeniser(StreamReader data)
  {
    this.data = new StringReader(data);
    index = 0;
  }

  public float consumeFloat()
  {
    consumeWhitespace();
    string val = consumeString();
    return float.Parse(val);
  }

  public int consumeInt()
  {
    consumeWhitespace();
    string val = consumeString();
    return int.Parse(val);
  }

  public bool expectToken(string tokenName)
  {
    return data.PeekChars(tokenName.Length) == tokenName;
  }

  public void consumeToken(string tokenName)
  {
    consumeWhitespace();
    if (!expectToken(tokenName))
    {
      throw new ParseException("Expected token " + tokenName + " at char " + index + ": " + data.PeekChars(tokenName.Length) + "...");
    }
    else
    {
      consume(tokenName.Length);
    }
  }

  public string consumeString()
  {
    try
    {
      consumeWhitespace();
      string result = data.ReadString();
      index += result.Length;
      return result;
    }
    catch (System.ArgumentOutOfRangeException)
    {
      throw new ParseException("End of file while reading string");
    }
  }

  public void consumeWhitespace()
  {
    index += data.ConsumeWhitespace();
  }

  public bool EndOfStream
  {
    get => data.EndOfStream;
  }

  private void consume(int chars)
  {
    data.ReadChars(chars);
    index += chars;
  }
}