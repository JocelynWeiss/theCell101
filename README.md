# theCell101
To manage The Cell...

This is using Unity 2019.3.7f1

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