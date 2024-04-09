# Screen-Light
This plugin for VNyan will approximate the light coming of a screen to light up your avatar!

Here is a short clip of the current version being used on stream! I like the effect to be a little muted and I think it gives a nice immersion. I use all default settings, and the prop is based on the setup seen in this clip. Live demonstrations might be provided at scheduled times <3

https://clips.twitch.tv/SilkyRoundOkapiAMPEnergy-1v35Wh-icf2U_-yv

I have not found this plugin to not affect performance on my system and from what I understand it should be relatively efficent! If you have large performance issues, do tell, it might come down to GPU model or other issues.

-------------------------

This plugin can be used in two ways!
1. Used with the premade prop, this can be added and moved around your existing world!
2. Export your own world with your own screen light addition!

-------------------------
Part 1: Premade prop!
-------------------------

Make sure you have a spout2 plugin installed to OBS, I recommend and use: https://github.com/Off-World-Live/obs-spout2-plugin/

Make sure you have "Allow 3rd Party Mods/Plugins" enabled in VNyan!

![image](https://github.com/Sjatar/Screen-Light/assets/56020444/88cdbbfe-93d6-40d9-8803-c267b81f1a59)

-------------------------

I recomend the premade prop and I will detail first how to setup the prop!

1. Download the latest release from the release page: https://github.com/Sjatar/Screen-Light/releases
2. Inside you will find these items: 
   - Sja_BlitLightCookie.dll
     - Main code for screen light functionality
   - Sja_UI.dll
     - Code for plugin window
   - Sja_PluginUI.vnobj
     - Plugin window itself
   - ScreenLamp.vnprop
     - Screen light prop
3. The zip should contain two folders, drop both into VNyan/Items to add the above items!
   - Assemblies
   - Props
4. You can now open VNyan! Open props and add the ScreenLamp.vnprop located in VNyan/Items/Props
   - Note: it will not instantly work! But position it so the little cylinder is pointing towards your avatar, in line with the mouth or so. I recommend using the handy move gizmo!

![image](https://github.com/Sjatar/Screen-Light/assets/56020444/6e7bb289-6fce-4456-aa2a-500b31f829ff)
   
5.  Open OBS! For this to work you need spout2 installed. If you have a game/display or window capture, right click it and add a Spout filter! In the example I added a browser source.
    - Currently the plugin supports two spout names: "screen" and "game". By default the plugin uses the spout2 texture name "screen". See second part how to use second texture!
    - Type in "screen" in the spout filter text field! Spout2 can be very stubborn. If something seems to not work, restart either/both obs or/and VNyan!

![image](https://github.com/Sjatar/Screen-Light/assets/56020444/59fdd2ec-8cdc-4d7d-95a3-4525822d7cf5)

6. If the prop is rotated the right way, the dlls are in the right places, OBS is sending and VNyan is recieving the spout2 correctly. You should now see a faint light on your character! 

![image](https://github.com/Sjatar/Screen-Light/assets/56020444/819ba650-6a97-4cd5-81c7-fefd32311c5f)

Part 2: Plugin Window!
-------------------------

For the second part I will explain how the plugin window works!

1. If you click the plugin menu item in Vnyan, a little window will pop up. If you installed the Sja_UI.dll and Sja_PluginUI.vnobj you should see a item called "Screen Light Plugin" in this window.

![image](https://github.com/Sjatar/Screen-Light/assets/56020444/5afb9518-6f55-494e-93c4-5798b1e11986)

2. Here you can change all settings for the screen light plugin and even save one preset!
   - Note: I recommend to use node graphs to use the plugin in your setup but the plugin window is handy to debug and find settings you like!
3. If you want to use a second spout2 texture, here you can change the texture number to "2". It will then grab spout2 texture called "game".
   - Note: It is with multiple textures spout2 can mess up, so if it does not change textures. Restart VNyan!
4. Blur is how the plugin estimates the light distribution! I recommend a blur of 8 if your source is 1080p. You might want to increase this if you use 1440p or 4k.
   - To check how many iteration you use. Open VNyans monitor and search for "light". You should see multiple "lightcookie_" values. Iterations note your current setting!
   - If you set blur to 1, it's easier to position the prop! Artistic freedom on how it should look, I recomend middle of the light aimed for the mouth/nose and head should be in the middle 1/3 of the texture! I got a square head so I just fill my entire face!

![image](https://github.com/Sjatar/Screen-Light/assets/56020444/f689a8cc-7190-4e0e-85dc-e695ee30745a)

5. The remaining settings are best to leave at default.
   - Estimate color: will blur the texture down to 1x1 pixel and sample the colour! Usually pretty accurate, but there might be issues with very dark scenes.
   - Lum adjustment: If max lum is false, you can add a constant increase to the luminosity of the light! Can be useful.
   - Max lum: This sets luminosity to maximum at all times. Because the plugin already creates a black and white version of the texture, luminosity is already kinda calculated through that.
   - Light intensity: If you want to increase the brightness of the screen!
   - Relax Hue and Sat: In dark scenes, hue and saturation of estimated colours might shift widly. This method makes it spread the change over multiple frames to remove some flickering.
   - Ralax value: Can be changed to change relax method speed, faster (higher value) or slower (lower value).
   - Dark Flicker Fix: Due to the way the plugin works, it's overestimating small details contribution to colour for dark scenes. This setting might cause slight decrease in colour accuracy, but will cause dark scenes to not flicker (as much).

-------------------------
Part 3: Node graphs!
-------------------------
I promise this part will not be to long <3

1. Instead of changing the settings manually through the plugin window. I recommend setting up node graphs to setup the prop on VNyan startup or changing scenes!
2. You can load the example graph: "GitHubShowcase.json". This can be downloaded here: [GitHubShowcase.json](https://github.com/Sjatar/Screen-Light/files/14923281/GitHubShowcase.json)
3. On changing scene or on application start, set the parameters to the values you found to work the best

-------------------------
Exporting your own world!
-------------------------
WIP, I have a long video explaining how to do it. But I want a shorter more to the point text version here!
The video is not 100% up to date, as I made it before some additional features where added. 
https://youtu.be/ME599kHcZRc
