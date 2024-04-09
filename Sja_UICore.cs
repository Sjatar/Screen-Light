// Dependencies for the script. Not all of these have to be present. System and UnityEngine are defaults.
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using VNyanInterface;
using System;

// Using a namespace as default for the script makes it easier to build into a dll later.
namespace Sja_UI
{
    // Default unity script setup. Monobehaviour gives access to Awake() etc.
    // VNyanInterface.IButtonClickHandler gives access to pluginButtonClicked
    public class Sja_UICore : MonoBehaviour, VNyanInterface.IButtonClickedHandler
    {
        // Most functionality is in the different sub scripts.
        
        //##############################################################################################################
        
        // Sja_SaveButton, manages saving of parameters such as slider/button/input field values!
            // When the save button is pressed a Json settings file will be written.
            // Buttons/sliders/input fields use this to load their values if they saved one before!
        
        // Sja_Button, manages buttons. 
            // Changes color when clicked. 1 is green, 0 is red!
            // Loads a 1 or 0 from the dictionary if it already exists. Changes color accordingly.
        
        // Sja_Slider, manages sliders.
            // Has a value between 1 and 0, shows this with a little slider! Cute :3
            // Loads it's values from the dictionary if it already exists. The slider will update automatically.
        
        // Sja_InputField, manages input fields.
            // Has any float value. Input field will always try to show the current value.
            // Loads it's value from the dictionary if it already exists. Input field will show the loaded value.
            // To confirm the inputted value a button is needed to be pressed next to the field.
            // This button just has to be a child to the Inputfield to work! Simple :3
            
        // Sja_MainWindow, manages main window.
            // Attaches to the windowPrefab and allows dragging the window and also focusing it so it does not hide
            // behind other windows in VNyan.
            
        // Sja_UICore
            // This code right here. Mostly some VNyan magic to add plugin button. I did not write most of it,
            // example code was given by Suvidriel. 
        
        //##############################################################################################################
        
        // We need some info about where our objects are! WindowPrefab is where our UI assets exist
        // We create a window object to add the window prefab to when plugin button in VNyan is pressed
        public GameObject windowPrefab;
        private GameObject window;

        public string setting_name;
        public string plugin_name;

        private float KeyValue;
        
        // We want to keep track of our VNyanParameters somewhere. This Dictionary makes it easier.
        // Each button, slider and input field will automatically add to this depending on it's name!
        // This dictionary is also used to save and load.
        // At Awake, we look to see if the setting file exists. If it does we load it into the dictionary.
        // Before a button etc adds a new parameter it will look to see if it already exists.
        // If it does exist it will set it's value to the dictionary value and not add anything.
        public static Dictionary<string, string> VNyanParameters = new Dictionary<string, string>();
        
        // This happens when VNyan starts.
        public void Awake()
        {
            // If the setting file exists. Load it into the dictionary! If the user clicks the save button,
            // this settings file will update with new values.
            
            if (null != VNyanInterface.VNyanInterface.VNyanSettings.loadSettings(setting_name))
            {
                // This loads a dictionary from a file named settings_name.
                // Each slider/button/field will load their own parameter into VNyans parameters on load!
                // So there is no need to also add each value in the dictionary to VNyan here.
                VNyanParameters = VNyanInterface.VNyanInterface.VNyanSettings.loadSettings(setting_name);
            }
            
            // VNyan magic to add a plugin button to it's interface!
            VNyanInterface.VNyanInterface.VNyanUI.registerPluginButton(plugin_name, (IButtonClickedHandler) this);
            this.window = (GameObject) VNyanInterface.VNyanInterface.VNyanUI.instantiateUIPrefab((object) this.windowPrefab);
            if ((UnityEngine.Object)this.window != (UnityEngine.Object)null)
            {
                this.window.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 0.0f);
                this.window.SetActive(false);
            }
        }
        
        // More magic, when plugin button is pressed, it will show the window!
        public void pluginButtonClicked()
        {
            if ((UnityEngine.Object) this.window == (UnityEngine.Object) null)
                return;
            this.window.SetActive(!this.window.activeSelf);
            if (!this.window.activeSelf)
                return;
            this.window.transform.SetAsLastSibling();
        }
    }
}