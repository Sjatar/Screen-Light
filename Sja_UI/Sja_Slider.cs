using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace Sja_UI
{
    class Sja_Slider : MonoBehaviour
    {
        // This name will be the name of the VNyanParameter, you want to change it!
        public string sliderName;
        // If we want the slider to go from 0 to some other max value add a multiplier to it!
        public int max_value = 1;
        // Minimum value, for example min_value = 1, slider will go from 1 to slider_multiplier
        // Sharp eyed might notice that min might influence max value, but this is accounted for down below.
        // I have made it so it only allow int values. Might need change if float is needed.
        public int min_value = 0;
        // If we want the slider to only output int values enable this!
        // If slider_multiplier is 1 then this allows only 0 and 1.
        public bool only_allow_int;
        // If you want the first time value to be different change it here.
        public float default_value = 0f;
        
        private Slider mainSlider;

        public void Start()
        {
            // Get the slider component of the object the script is attached to!
            mainSlider = GetComponent(typeof(Slider)) as Slider;
            // We add a listener, it will run ValueChangeCheck when the value changes on the slider!
            mainSlider.onValueChanged.AddListener(delegate {ValueChangeCheck(); });
            
            // If a min value is added this will mess with the set default and max value. This is correct here.
            max_value -= min_value;
            default_value -= min_value;
            
            // Here we either want to load a existing parameter or add one!
            // A loaded parameter comes from Sja_UICore when it loads the setting Json into the dictionary!
            if (Sja_UICore.VNyanParameters.ContainsKey(sliderName))
            {
                // If the parameter exist, set slider value to that value.
                mainSlider.value = (Convert.ToSingle(Sja_UICore.VNyanParameters[sliderName]) - min_value) / max_value;
                // Set the VNyanParameter to this value!
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(sliderName, (mainSlider.value * max_value) + min_value);
            }
            else
            {
                // If it was the first time and there is no parameter we want to add it!
                // slider name is the name of the parameter and mainSlider.value will be the value.
                // Default for a untouched slider is 0. Unless default value is set to something else.
                mainSlider.value = default_value / max_value;
                Sja_UICore.VNyanParameters.Add(sliderName, Convert.ToString((mainSlider.value * max_value) + min_value));
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(sliderName, (mainSlider.value * max_value) + min_value);
            }
        }
        
        public void ValueChangeCheck()
        {
            if (!only_allow_int)
            {
                // When the value is changed we want to set the dictionary value to the new value
                Sja_UICore.VNyanParameters[sliderName] = Convert.ToString((mainSlider.value * max_value) + min_value);
                // and also set the VNyan parameter value to it!
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(sliderName, (mainSlider.value * max_value) + min_value);
            }
            else
            {
                // We only want to allow ints, so we floor the value, it converts it to a double, which we cast to float
                Sja_UICore.VNyanParameters[sliderName] = Convert.ToString(Math.Floor((mainSlider.value * max_value) + min_value));
                // and also set the VNyan parameter value to it!
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(sliderName, (float)System.Math.Floor((mainSlider.value * max_value) + min_value));

                // It would be akward if the slider would not reflect this behaviour. So we set to a value corresponding
                // to the int value.
                mainSlider.value = (float)System.Math.Floor(mainSlider.value * max_value) / max_value;
            }
        }
    }
}
