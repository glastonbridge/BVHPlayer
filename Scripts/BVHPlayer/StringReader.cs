using System.IO;

public class StringReader
{
  private StreamReader data;
  private string _buffer;

  public StringReader(StreamReader data)
  {
    this.data = data;
  }

  public string PeekChars(int num)
  {
    needBuffer(num);
    return buffer.Substring(0, num);
  }

  public string ReadChars(int num)
  {
    string result = PeekChars(num);
    _buffer = _buffer.Substring(num);
    return result;
  }

  public bool EndOfStream
  {
    get
    {
      return _buffer.Length == 0 && data.EndOfStream;
    }
  }

  public string ReadString()
  {
    int i = buffer.IndexOfAny(new[] { ' ', '\n', '\t' });
    return ReadChars(i);
  }

  /**
   * returns the number of characters consumed
   */
  public int ConsumeWhitespace()
  {
    int untrimmed = buffer.Length;
    _buffer = _buffer.TrimStart();
    int trimmed = untrimmed - _buffer.Length;
    if (_buffer.Length == 0 && !EndOfStream)
    {
      pumpBuffer();
      trimmed += ConsumeWhitespace();
    }
    return trimmed;
  }

  private void needBuffer(int num)
  {
    while (buffer.Length < num && !EndOfStream)
    {
      pumpBuffer();
    }
  }

  private string buffer
  {
    get
    {
      if (_buffer == null)
      {
        pumpBuffer();
      }
      return _buffer;
    }
  }

  private void pumpBuffer()
  {
    _buffer += data.ReadLine() + '\n';
  }
}
