# Unity-UI-Blur
Simple UI blur without using post-processing layer on camera

# How it works.

The UI blur works by capturing a screenshot, then using that screen capture as a texture and running it through a shader which results in a beautiful blur image. 
The blur is optimized for both mobile and desktop so can be used without hassel on any device.

The Dynamic Blur feature still needs a little work as i need to find a way to layer the blur properly.
