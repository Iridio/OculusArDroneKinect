using Microsoft.Speech.Recognition;
using System.Globalization;

namespace OculusParrotKinect.Kinect
{
  public static class VoiceCommands
  {
    public const string TakeOff = "TakeOff";
    public const string Land = "Land";
    public const string Emergency = "Emergency";
    public const string ChangeCamera = "ChangeCamera";
    public const string DetectFacesOn = "DetectFacesOn";
    public const string DetectFacesOff = "DetectFacesOff";

    public static Grammar GetCommandsGrammar(CultureInfo culture)
    {
      var commands = new Choices();
      //TODO handle different cultures
      //if (culture == "it-IT")
      //{
      commands.Add(new SemanticResultValue("drone decolla", TakeOff));
      commands.Add(new SemanticResultValue("drone atterra", Land));
      commands.Add(new SemanticResultValue("emergenza", Emergency));
      commands.Add(new SemanticResultValue("cambia camera", ChangeCamera));
      commands.Add(new SemanticResultValue("drone identifica", DetectFacesOn));
      commands.Add(new SemanticResultValue("drone privacy", DetectFacesOff));
      //}
      var gb = new GrammarBuilder { Culture = culture };
      gb.Append(commands);
      return new Grammar(gb);
    }
  }
}
