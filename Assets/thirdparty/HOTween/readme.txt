HOTWEEN
A Unity Tween Engine by Daniele Giardini - Holoville
First release date: january 1, 2012

You can find detailed information, documentation, API, more examples, and most recent versions of HOTween on its website: http://www.holoville.com/hotween
Open-source code is available on Google Code: http://code.google.com/p/hotween

---------------------------
IMPORTANT
---------------------------
Regular HOTween is not compatible with micro mscorlib (which is used when compiling for iOS with max stripping level). You can download a micro mscorlib compatible version from HOTween's website (http://www.holoville.com/hotween). Just replace your current HOTween DLL with HOTweenMicro DLL at any moment: the API is identical and everything else stays the same (but HOTween will run a little slower - that's why you should use the regular version unless you really need micro mscorlib).

---------------------------
IN SHORT
---------------------------
HOTween is composed of:
[HOTween]
Controls all Tweeners and Sequences, and is directly used to create Tweeners (via HOTween.To() or HOTween.From()).
[Tweeners]
Created by HOTween.To() or HOTween.From(), they're the real workers, and directly animate the given properties of the given target.
[Sequences]
Used to create a tween timeline. By adding Tweeners or other Sequences to them, they can be used to create complex but easy to control animations.

:: GET READY TO CODE ::
Before using HOTween, you will have to import its namespaces in your script:
[C#]
using Holoville.HOTween;
using Holoville.HOTween.Plugins; // only if you need a plugin
[UnityScript]
import Holoville.HOTween;
import Holoville.HOTween.Plugins; // only if you need a plugin

:: HOTWEEN BASICS ::
The tween creation logic is quite simple, and will be somehow familiar to you if you're used to other tween engines. You always use HOTween.To() (or HOTween.From()), and give it a target (which is the object that contains the property or field you wish to tween), a duration, and a series of parameters (like the properties to tween, the type of easing, the type and number of loops, eventual callbacks, and so on).
Check the demo included in the package to see some practical example.

:: HOTWEEN SEQUENCE BASICS ::
Each time you call HOTween.To/From, a new Tweener is created and returned. You can let that object go by itself, or you can place it inside a Sequence, to create complex sequences of tweens and control them dinamically as if they were one. Also, you can place Sequences inside other Sequences, to make things more fun.
Think of a Sequence as a timeline, where you place Tweeners or other Sequences at the position you decide. Also, it can be controlled as any video timeline would: play and pause it, but also rewind, restart, reverse, complete, or directly set its position (either with GoTo(position) or with mySequence.position=[time]).
Again, check the included demo for some practical example.

:: CALLBACKS ::
Callbacks are added to a Tweener via TweenParms, and to a Sequence via SequenceParms, and they can be chained like the rest of the parameters.
You can check all the available callbacks in the Documentation (http://www.holoville.com/hotween/documentation.html).

:: PLUGINS ::
All HOTween's animations work with plugins, but they are usually managed automatically. When adding a Prop parameter to HOTween.To, you normally just write the name of the property to tween and the end value, then HOTween finds by himself the correct plugin to apply, based on the property's type. This system works if you want to animate values in the default way, but sometimes you wish to use special kinds of animations. That's when you can directly assign special plugins to a Prop parameter.
For example, if you animate a Vector3 property, HOTween automatically uses the PlugVector3 plugin, which animates the x/y/z values linearly from the starting vector to the ending one. If you want to animate only the Y value of the vector, you might instead tell him to use the PlugVector3Y plugin. Or, if you wish to move along a path, you might use the PlugVector3Path plugin.