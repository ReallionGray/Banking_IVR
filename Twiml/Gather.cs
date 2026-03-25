using System.Xml.Linq;

namespace Banking_IVR.Twiml;

public class Gather
{
    private readonly List<XElement> _children = [];
    private readonly int? _numDigits;
    private readonly string _action;
    private readonly string _method;
    private readonly string? _finishOnKey;

    public Gather(int? numDigits, string action, string method, string? finishOnKey = null)
    {
        _numDigits = numDigits;
        _action = action;
        _method = method;
        _finishOnKey = finishOnKey;
    }

    public void Say(string text, string voice = "woman", string language = "en-US")
        => _children.Add(new XElement("Say",
            new XAttribute("voice", voice),
            new XAttribute("language", language),
            text));

    public void Play(string url)
        => _children.Add(new XElement("Play", url));

    public XElement ToXElement()
    {
        var element = new XElement("Gather",
            new XAttribute("action", _action),
            new XAttribute("method", _method));

        if (_numDigits.HasValue)
        {
            element.Add(new XAttribute("numDigits", _numDigits.Value));
        }

        if (!string.IsNullOrWhiteSpace(_finishOnKey))
        {
            element.Add(new XAttribute("finishOnKey", _finishOnKey));
        }

        element.Add(_children);
        return element;
    }
}
