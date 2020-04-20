# theCell101
To manage The Cell...

This is using Unity 2019.3.10f1

## Player Settings
Company Name: StudioHG  
Product Name: TheCell  
Package Name: com.StudioHG.TheCell  
Minimum API Level: 19  
Scripting Backend: IL2CPP  
Api Compatibility Level: .NET 4.x  
Optimize Mesh Data: Off  

## XR Plugin Management
### Oculus
Stereo Rendering Mode: Multi Pass  
V2 Signing (Quest): On

## Vulkan support
https://developer.oculus.com/blog/vulkan-support-for-oculus-quest-in-unity-experimental/  

First, navigate to Package Manager and make sure the “Oculus Android” package **is not** installed.  
Next, under Project Settings, go to “XR Plugin Management”.  
Make sure the Android Tab is selected under XR Plugin Management.  
Install the “Oculus XR Plugin”, and have your project use it by selecting the “Oculus Loader” in the “Plugin Providers” region.  
Finally, to configure Oculus-specific settings, navigate to the “Oculus” tab (lower left hand corner in the below image).  

---------------------------------------------

## Install apk
From the Build folder where the apk is:
```
adb install -r TheCell01.apk
``` 
You can check for the list of connected devices using:  
``` adb devices ``` 

---------------------------------------------  
  
## Interactions
For the moment only 3 programmed:  
### Moving from cell to cell 
To move from one cell to another, there are the big transparent cyan zones.  
Both hands must be immersed in it for a minimum of 2 seconds. The code is on the NorthDoor game object for example in the CellInteract group.  
### To open hatches  
You can open hatches to look to the next room. The hatch will automatically close itself after few seconds.  
You have to put your hand (right or left) on the hand scanner on the right side of the hatches. Both the index and the pinky finger must be inside the trigger.  
Code is in HandScanTrigger.cs  
### Push a row of cells
There is big handles floating in the air at the moment, one on each cardinal point. They push the row forward.  
To activate them, the thumb and the index must be pinching inside the trigger while the lever is going down.  
The action take place at the end of the mouvement. Code is in the CellInteract group on the game object Mechanism_N for instance.

---------------------------------------------  