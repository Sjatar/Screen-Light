// Dependencies for the script. Not all of these have to be present. System and UnityEngine are defaults. 
using System;
using UnityEngine;
using VNyanInterface;

// Using a namespace as default for the script makes it easier to build into a dll later.
namespace Sja_BlitLightCookie
{
    // Default unity script setup. Monobehaviour gives access to Start() and Update() etc.
    public class BlitLightCookie : MonoBehaviour
    {
//######Sets public variables that can be changed by the user###########################################################
        // This will be the texture used as the light cookie. Can be used with any render texture.
        // Default use will be to process a spout2 output of a screen or game from OBS.
        // Might only work with landscape 16:9 aspect ratios.
        [Header("Texture to use:")]
        public RenderTexture InputTexture1;
        public RenderTexture InputTexture2;
        public RenderTexture InputTexture3;

        private RenderTexture InputTexture;
        
        // It's also possible to change between multiple textures. I have only added support for 2 but more could also be
        // added. If multiple textures is not needed, both can be filled with the same. This will hopefully have no 
        // functional difference. 
        public string parameterNameInputTexture = "LightCookie_Texture";
        public int textureNumber = 1;
        
        // Custom shader which has the purpose of processing the Input texture. Has 4 passes:
        // Pass 0: Fix aspect. This makes the texture square by padding up and down.
        //         Also pads to the left and right of the texture. This improves blur as it can spread from the sides.
        // Pass 1: Boxfilter blur. This samples 4 pixels and averages their values. Halving width and height in the process.
        // Pass 2: Greyscale, based on human sight sums together colors from the texture to estimate a greyscale image.
        //         0.2126 of red, 0.7152 of green and 0.0722 of blue contribution.
        // Pass 3: Does nothing. Used for testing!
        [Header("Shader to use:")]
        public Shader LightCookieShader;
        
        // Number of iterations of blur that should be applied. Also downscales the input texture to res/(2^iterations).
        // A value of 10 works well with 1920 width. 1920/2-> 960-> 480-> 240-> 120-> 60-> 30 -> 15-> 7-> 3
        // As the resolution can only be int values a value of 15/2 = 7.5 is rounded down.
        // Parameter name addressable inside VNyan and it's default value. (Can be changed by the user)
        [Header("Blur iterations:")]
        public string parameterNameIterations = "LightCookie_Iterations";
        // Default Value when script is added.
        [Range(1,16)]
        public int iterations = 9;
        
        // Decides if additional processing should be done to change the color of the spotlight.
        // Parameter name addressable inside VNyan and it's default value. (Can be changed by the user)
        [Header("Calculate color?:")]
        public string parameterNameColorBool = "LightCookie_ColorBool";
        // Default Value when script is added.
        public bool ColorBool = true;
        
        // Luminosity control, can be used to make the color more or less white. Intensity might do the same thing.
        // Parameter name addressable inside VNyan and it's default value. (Can be changed by the user)
        [Header("Adjust luminosity of light:")]
        public string parameterNameLuminosityCorrection = "LightCookie_Luminosity";
        // Default Value when script is added.
        public float LuminosityCorrection = 0.0f;
        
        // Sets luminosity to 1 for any color calculated. I think this looks more correct.
        [Header("Sets luminosity to max independent of color:")]
        public string parameterNameLuminosityBool = "LightCookie_LumBool";
        public bool LumBool = true;
        
        // Intensity of the spotlight!
        // Parameter name addressable inside VNyan and it's default value. (Can be changed by the user)
        [Header("Intensity of light:")]
        public string parameterNameLightIntensity = "LightCookie_Intensity";
        // Default Value when script is added.
        public float LightIntensity = 2.5f;
        
        // Additional processing of Hue and Saturation. Due to the fact we need to downsample and sample 1 pixel of the 
        // texture, it causes seemingly small changes in Hue and Saturation to present in a large change.
        [Header("Prevents big changes in Hue, to limit flickering")]
        public string parameterNameHueSatRelaxBool = "LightCookie_HueSatRelaxBool";
        public bool HueSatRelaxBool = true;

        // We relax the change in hue and saturation if the change is less then 23% of the max change.
        // The relax is limited to 1% of the max change per frame by default by the parameter HueSatRelaxValue.
        // The change is also reduced if the saturation/hue value is closed to the sampled value. 
        // Otherwise the new Relax function introduces more flickering.
        public string parameterNameHueSatRelaxValue = "LightCookie_HueSatRelaxValue";
        public float HueSatRelaxValue = 0.01f;

        [Header("Lightens the entire texture a bit to further reduce flickering in dark scenes")]
        public string parameterNameDarkFlickerBool = "LightCookie_DarkFlickerBool";
        public bool DarkFlickerBool = true;
        
//######Sets private for the script#####################################################################################
        // Graphics.Blit function cannot take just a shader reference but uses a material as proxy.
        // LightCookieMat is created in Start() with the only input being the input shader.
        private Material LightCookieMat;

        // It's slow to look for the component variables in Update() so we make a reference in Start().
        private Light cLight;
        
        // We want to save historical hue values, saturation and luminosity values for colour estimation.
        private float H, Hest = 0.5f, S, Sest = 0.5f, V;
        
//######################################################################################################################
        // Start is called before the first frame update
        void Start()
        {   
            // GetComponent is slow so we reference the light component now to save time!
            cLight = GetComponent(typeof(Light)) as Light;
            
            // We create a material using the shader we gave the script for later!
            LightCookieMat = new Material(LightCookieShader);
            
            // VNyanInterface can be used to change and set parameters inside VNyan. In the unity editor this will give
            // errors as there is no parameter system available for us to set or get data from. We check if it exists by
            // seeing if its null. It's always null in Unity editor but is false when the world/model/item is loaded in VNyan!
            if (!(VNyanInterface.VNyanInterface.VNyanParameter == null))
            {   
                // We can change the texture used by the script if there is multiple spout texture one would want to use
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(parameterNameInputTexture, (float)textureNumber);
                
                // If we are inside VNyan we want to set the parameters name and default value set by the user.
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(parameterNameIterations, (float)iterations);
                
                // bools can be represented as a float with "Convert.ToSingle". 
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(parameterNameColorBool, Convert.ToSingle(ColorBool));
                
                // Default luminosity correction value set in VNyan.
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(parameterNameLuminosityCorrection, LuminosityCorrection);
                
                // bools can be represented as a float with "Convert.ToSingle".
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(parameterNameLuminosityBool, Convert.ToSingle(LumBool));
                
                // Default intensity value set in VNyan.
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(parameterNameLightIntensity, LightIntensity);
                
                // bools can be represented as a float with "Convert.ToSingle".
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(parameterNameHueSatRelaxBool, Convert.ToSingle(HueSatRelaxBool));
                
                // Default HueSatRelax value set in VNyan.
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(parameterNameHueSatRelaxValue, HueSatRelaxValue);
                
                // bools can be represented as a float with "Convert.ToSingle".
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(parameterNameDarkFlickerBool, Convert.ToSingle(DarkFlickerBool));
            }
        }
        
//######################################################################################################################
        // Update is called once per frame
        void Update()
        {   
            // VNyanInterface gives error if used inside unity editor, so we see if it exists. 
            // It should exist when the world/model/item is loaded inside VNyan!
            if (VNyanInterface.VNyanInterface.VNyanParameter != null)
            {
                // Get the texture that is needed.
                textureNumber = (int)VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameInputTexture);
                // VNyanInterface only works with floats so we cast (int) it to be a int.
                iterations = (int)VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameIterations); 
                // Hacky way to make a float into a bool!
                ColorBool = (int)VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameColorBool) == 1;
                LuminosityCorrection = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameLuminosityCorrection);
                LumBool = (int)VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameLuminosityBool) == 1;
                LightIntensity = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameLightIntensity);
                HueSatRelaxBool = (int)VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameHueSatRelaxBool) == 1;
                DarkFlickerBool = (int)VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameDarkFlickerBool) == 1;
            }

            // Switch case to choose the input texture of the 3 available. 
            switch (textureNumber)
            {
                case 1:
                    InputTexture = InputTexture1;
                    break;
                case 2:
                    InputTexture = InputTexture2;
                    break;
                case 3:
                    InputTexture = InputTexture3;
                    break;
            }
            
            // Resolution can only be whole numbers so int is used.
            int width = InputTexture.width;
            int height = InputTexture.height;
            
            // Due to that a light cookie can only be a square texture we see what dimension is the largest.
            // We also want to pad the sides of the texture. Here 100% on each side is used, x3.
            // This should not be changed. But if it is the shader "Pass 0" also needs to change.
            int dimension = Math.Max(width, height) * 3;
            
//######################################################################################################################
// Boring part is over, this is where the processing starts. First we create a double buffer render texture setup.
// We get a temporary "currentDestination" texture that receives "Input texture" through Graphics.Blit. Graphics.Blit
// works fully in the GPU and is a very fast way to process textures through the help of shaders.
// This is done through Pass 0 of the shader through the material we have created. This makes "currentDestination"
// square and adds padding. We then copy "currentDestination" into "currentSource" for the main iteration loop, it then
// releases it. In the next step we want to do the first blur pass so we divide dimension by 2 in preparation.
            RenderTexture currentDestination = RenderTexture.GetTemporary(dimension, dimension, 0);
            currentDestination.filterMode = FilterMode.Trilinear;
            currentDestination.wrapMode = TextureWrapMode.MirrorOnce;
            Graphics.Blit(InputTexture, currentDestination, LightCookieMat, 0);
            RenderTexture currentSource = currentDestination;
            
//######################################################################################################################
// This is a later comment. I realised that in very dark scenes colour can flicker a lot. This is probably due to not
// a lot of colour data. With the current method small changes in colour in dark scenes can cause large differences
// in sampled colour. Therefor a method is added to the shader to brighten the entire texture. As this is in shader it 
// is not possible to change this and a static value is chosen. In this case we increase the brightness by 0.001, 
// 1 being the max value. We do this for the extended canvas as it is the massive black padding added in the above
// process which causes the flicker. This process causes a very black scene to be white rather then whatever small
// amount of colour there is in the texture. 

            if (DarkFlickerBool)
            {
                currentDestination = RenderTexture.GetTemporary(dimension, dimension, 0);
                currentDestination.filterMode = FilterMode.Trilinear;
                currentDestination.wrapMode = TextureWrapMode.MirrorOnce;
                Graphics.Blit(currentSource, currentDestination, LightCookieMat,3);
                RenderTexture.ReleaseTemporary(currentSource);
                currentSource = currentDestination;
                RenderTexture.ReleaseTemporary(currentDestination);
            }
            
            // We wanted to do this on pass 0 just above but doing it here, as if flickerfix is used then we want to 
            // divide dimension after that.
            dimension /= 2;
            
//######################################################################################################################
// This for loop will run for as long as we have specified either in unity editor or default values set in editor for
// Vnyan or overwritten default values in VNyan. It recreates "currentDestination" now with half width and half height.
// It uses Pass 1 of the shader which is a boxfilter. This averages 4 pixels values into 1 pixel. Which handily reduces
// the number of pixels to 1/4. Which is also the result of running "dimension /=2".
// After the texture is used we release it as quickly as possible. Not releasing textures is bad! Fills memory fast
// We break out if dimension becomes equal to 1. It will always become 1 with enough iterations due to rounding down a
// division by 2. 
            for (int i = 1; i < (iterations); i++)
            {
                currentDestination = RenderTexture.GetTemporary(dimension, dimension, 0);
                currentDestination.filterMode = FilterMode.Trilinear;
                currentDestination.wrapMode = TextureWrapMode.MirrorOnce;
                Graphics.Blit(currentSource, currentDestination, LightCookieMat, 1);
                RenderTexture.ReleaseTemporary(currentSource);
                currentSource = currentDestination;
                RenderTexture.ReleaseTemporary(currentDestination);
                
                dimension /= 2;
                
                if (dimension <= 0)
                {
                    break;
                }
            }
            
//######################################################################################################################
// We create a LightCookie texture we will set as the light cookie, this is set as 200x200. I'd love to set this equal
// to the dimensions of currentSource but there seems to be a issue where if the texture is set to the same dimensions  
// as current source then the output texture will only sample black. Might be a issue with the shader.
// We use Pass 2 of the shader which creates a greyscale image of the resulting texture from the for loop.
// This greyscale image is then set as the light cookie for the spotlight! Mission done.
            RenderTexture LightCookie = RenderTexture.GetTemporary(400, 400, 0);
            LightCookie.filterMode = FilterMode.Trilinear;
            LightCookie.wrapMode = TextureWrapMode.Clamp;
            Graphics.Blit(currentSource, LightCookie, LightCookieMat, 2);
            cLight.cookie = LightCookie;
            
//######################################################################################################################
// If the ColorBool option is set to true then we will also take time to estimate the colour of the screen.
// This is done by continuing to boxfilter and downscale the input texture in the exact same way as in the for loop above.
// This is done until the output texture is 1x1 in dimensions. This will have then successfully estimated the colour of
// the screen. Up to this point the script is very efficient. We only tell the GPU to do work and never download or
// upload data to and from the CPU. But to change the colour of the spotlight we need to sample this texture of 1x1 to
// get the colour. This is done with the very expensive command ReadPixels. In this case it's as efficient as possible.
// Reading 1 pixel from a 1x1 texture. But to read the texture we often need to wait for the GPU to finish some task, 
// so this single command takes my FPS down from ~775fps (6 averaged samples) to ~282 fps, from a baseline 849 fps.
// Sadly I'm unable to find a more efficient way of getting the colour data and this is necessary.
// Another option however could be AsyncGPUReadback which can request data and wait for it to arrive while other work
// proceeds. But this adds latency to the colour sample and could take several frames to be delivered.
            if (ColorBool)
            {
                while (dimension > 0)
                {
                    currentDestination = RenderTexture.GetTemporary(dimension, dimension, 0);
                    currentDestination.filterMode = FilterMode.Trilinear;
                    currentDestination.wrapMode = TextureWrapMode.MirrorOnce;
                    Graphics.Blit(currentSource, currentDestination, LightCookieMat, 1);
                    RenderTexture.ReleaseTemporary(currentSource);
                    currentSource = currentDestination;
                    RenderTexture.ReleaseTemporary(currentDestination);
                
                    dimension /= 2;
                }
                
                RenderTexture.active = currentSource;
                Texture2D ColorData = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
                ColorData.wrapMode = TextureWrapMode.Repeat;
                ColorData.filterMode = FilterMode.Trilinear;
                
                // The expensive part :<
                // It's expensive due to having to wait for the GPU to give up the information we need.
                ColorData.ReadPixels(new Rect(0,0,1,1), 0, 0, false);
                Color pixelCol = ColorData.GetPixel(1,1, 0);
                
                Color.RGBToHSV(pixelCol, out H, out S, out V);
//######################################################################################################################
// Due to the padding with black to the input texture to better blur and make the input texture square.  
// A lot of the texture that is downscaled is black. The resulting colour is therefor darker then expected.
// The difference has been measured using a full screen pure colour and a color picker to iteratively give the correction.
// This change is done in HSV colour space and not RGB as it makes the correction trivial.
               
                if (LumBool)
                {
                    V = 1;
                }
                else
                {
                    // For some unknown reason you can set a luminosity level over 1 in code. Clamp will prevent that.
                    V = Mathf.Clamp(V * (3.88f + LuminosityCorrection), 0f, 1f);
                }
                
//######################################################################################################################
// Due to the way colour is sampled seemingly small changes as seen by the eye can have large changes in the colour
// estimated by the script. This can cause flickering and looks generally ugly. The solution I have come up with is
// a relax function, which relaxes changes under 18% of the max value to 0.5% change per frame.
// But allows changes above 18% to change saturation and hue instantly. 
                
                if (HueSatRelaxBool)
                {
                    // If the difference is less then 23%, hue is changed slowly to the sampled value.
                    // The change is lowered the closer Hest is to the sampled value, otherwise it causes flickering.
                    if (Math.Abs(H - Hest) < 0.23f)
                    {
                        if (H - Hest < 0)
                        {
                            Hest -= HueSatRelaxValue * Mathf.Clamp(Math.Abs(H-Hest)/0.23f,0f,1f);
                        }
                        else
                        {
                            Hest += HueSatRelaxValue * Mathf.Clamp(Math.Abs(H-Hest)/0.23f,0f,1f);
                        }
                    }
                    // If the difference is more then 23%, hue is changed instantly to the new sampled value.
                    else
                    {
                        Hest = H;
                    }
                    
                    // Same function but for saturation.
                    if (Math.Abs(S - Sest) < 0.23f)
                    {
                        if (S - Sest < 0)
                        {
                            Sest -= HueSatRelaxValue * Mathf.Clamp(Math.Abs(S-Sest)/0.23f,0f,1f);   
                        }
                        else
                        {
                            Sest += HueSatRelaxValue * Mathf.Clamp(Math.Abs(S-Sest)/0.23f,0f,1f);
                        }
                    }
                    else
                    {
                        Sest = S;
                    }
                }
                else
                {
                    Hest = H;
                    Sest = S;
                }
                
                pixelCol = Color.HSVToRGB(Hest, Sest, V);
                pixelCol.a = 255;
                
                // Colour is set! Hurray!
                cLight.color = pixelCol;
                Texture2D.Destroy(ColorData);
            }
            else
            {
                // If ColorBool is not wanted the colour is set to white.
                cLight.color = Color.white;
            }
            
            // If the user wants to change the Intensity of the light. This should be the same as a change in luminosity
            // if ColorBool is set to true. 
            cLight.intensity = LightIntensity;
            
            // Release all temporary textures! Unless you want a full GPU.
            RenderTexture.ReleaseTemporary(currentSource);
            RenderTexture.ReleaseTemporary(currentDestination);
            RenderTexture.ReleaseTemporary(LightCookie);
        }
    }
}
