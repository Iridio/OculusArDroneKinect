using System.Globalization;
using Microsoft.Speech.Recognition;

namespace OculusParrotKinect.Kinect
{
  public static class VoiceCommands
  {
    public const string TakeOff = "TakeOff";
    public const string Land = "Land";
    public const string Emergency = "Emergency";
    public const string ChangeCamera = "ChangeCamera";

    public static Grammar GetGrammar(CultureInfo culture)
    {
      //TODO handle different cultures
      var commands = new Choices();
      commands.Add(new SemanticResultValue("drone decolla", TakeOff));
      commands.Add(new SemanticResultValue("drone atterra", Land));
      commands.Add(new SemanticResultValue("emergenza", Emergency));
      commands.Add(new SemanticResultValue("cambia camera", ChangeCamera));
      var gb = new GrammarBuilder { Culture = culture };
      gb.Append(commands);
      return new Grammar(gb);
    }
  }
}
