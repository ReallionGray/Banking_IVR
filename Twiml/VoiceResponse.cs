using System.Xml.Linq;

namespace Banking_IVR.Twiml;

public class VoiceResponse
{
    private readonly List<object> _verbs = [];

    public void Append(Gather gather) => _verbs.Add(gather.ToXElement());

    public void Redirect(Uri uri)
        => _verbs.Add(new XElement("Redirect", uri.ToString()));

    public void Say(string text, string voice = "woman", string language = "en-US")
        => _verbs.Add(new XElement("Say",
            new XAttribute("voice", voice),
            new XAttribute("language", language),
            text));

    public void Play(string url)
        => _verbs.Add(new XElement("Play", url));

    public void Hangup() => _verbs.Add(new XElement("Hangup"));

    public override string ToString()
        => new XDocument(new XElement("Response", _verbs)).ToString(SaveOptions.DisableFormatting);
}
