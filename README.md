OculusArDroneKinect
===================
An XNA project to control an [Ar.Drone](http://ardrone2.parrot.com/) with [Kinect](http://www.microsoft.com/en-us/kinectforwindows/) gestures and voice commands while the video feed of the cameras is streamed to the [Oculus Rift](http://www.oculusvr.com/) (can be disabled by setting drawOculus=false in the class OculusParrotKinect.cs)

## Voice commands
Voice commands are specified in the class VoiceCommands.cs<br />
Commands supported are: take off, land, emergency, change camera<br />

## Gesture
With both arms forward -> move the drone forward<br />
With both hands near the shoulders -> move the drone backward<br />
With left arm extended to the left and right arm along the body -> move the drone to the left<br />
With left arm extended to the left and right arm forward -> move forward and to the left the drone<br />
With left arm extended to the left and right hand near the right shoulder -> move backward and to the left the drone<br />
The right movements are simply the left ones mirrored.<br />

## Credits
For the Ar.Drone library, I used the one from [Ruslan Balanukhin](https://github.com/Ruslan-B).<br />
The [AR.Drone projet](https://github.com/Ruslan-B/AR.Drone) and the [.NET FFMpeg wrapper](https://github.com/Ruslan-B/FFmpeg.AutoGen).<br />
For the Oculus implementation, I used the [Sunburn StereoscopicRenderer plugin](http://www.synapsegaming.com/downloads/resource.aspx?guid=46f57a92-57b3-4e34-80e0-418d5cf737f3) as a starting point. It's an implementation made by the guy behind the [Holophone3D](http://holophone3d.com).<br />
I learned a lot from these projects.<br />

## Notes
This is just a project I made for fun and as an excuse to mess around with some nice gadgets.<br />
The code is a mess and don't expect fancy HUD or whatever, it just works :).<br />
Till now I was only focused on having the pieces to work together. I plan to organize and better implement the code in my spare time.<br />
[Here you can see a simple video of it working](http://www.youtube.com/watch?v=8VuLyabbkWg)
