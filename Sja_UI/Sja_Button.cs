using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace Sja_UI
{
    class Sja_Button : MonoBehaviour
    {
        // You want to change this name, it will be the name used in VNyan.
        public string buttonName;
        private float buttonState = 1f;
        private Button mainButton;
        
        public void Start()
        {
            // Get the button component of the button this script is attached to!
            mainButton = GetComponent(typeof(Button)) as Button;
            
            // Add a listener to see when the button is pressed. It will call ButtonPressCheck when it is!
            mainButton.onClick.AddListener(delegate { ButtonPressCheck(); });
            
            // Here we either want to load a existing parameter or add one!
            // A loaded parameter comes from Sja_UICore when it loads the setting Json into the dictionary!
            if (Sja_UICore.VNyanParameters.ContainsKey(buttonName))
            {
                // If it did exist set the button state to the loaded value.
                buttonState = Convert.ToSingle(Sja_UICore.VNyanParameters[buttonName]);
                // Set the VNyanParameter to the loaded value.
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(buttonName, buttonState);
                // Change the color to green if the button state is 1. Otherwise this will be false.
                // False will make the button red!
                ChangeButtonColor(buttonState == 1f);
            }
            else
            {
                // Ohno the parameter did not exist! Add it to the dictionary :3
                Sja_UICore.VNyanParameters.Add(buttonName, Convert.ToString(buttonState));
                // Change the VNyan parameter to the correct button state.
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(buttonName, buttonState);
                // 1 -> green as 1 == 1 is true, 0 -> red as 0 == 1 is false
                ChangeButtonColor(buttonState == 1f);
            }
            
        }

        // It would be weird if the button did not show if you pressed it or not. Change the color depending on the state
        public void ChangeButtonColor(bool boolbuttonState)
        {
            // If the input is true make it green
            if (boolbuttonState)
            {
                ColorBlock cb = mainButton.colors;
                cb.normalColor = new Color(0.5f, 1f, 0.5f);
                cb.highlightedColor = new Color(0.5f, 0.9f, 0.5f);
                cb.pressedColor = new Color(0.4f, 0.8f, 0.4f);
                cb.selectedColor = new Color(0.5f, 1f, 0.5f);
                mainButton.colors = cb;
            }
            // Else make it red! 
            else
            {
                ColorBlock cb = mainButton.colors;
                cb.normalColor = new Color(1f, 0.5f, 0.5f); 
                cb.highlightedColor = new Color(0.9f, 0.5f, 0.5f);
                cb.pressedColor = new Color(0.8f, 0.4f, 0.4f);
                cb.selectedColor = new Color(1f, 0.5f, 0.5f);
                mainButton.colors = cb;
            }
        }
        
        // Cool stuff happening!
        public void ButtonPressCheck()
        {
            // If button state was off, run on part of script
            if (buttonState == 0f)
            {
                buttonState = 1f;
                Sja_UICore.VNyanParameters[buttonName] = Convert.ToString(buttonState);
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(buttonName, buttonState);
                ChangeButtonColor(true);
            }
            // If the button was not off, run off part of the script!
            else
            {
                buttonState = 0f;
                Sja_UICore.VNyanParameters[buttonName] = Convert.ToString(buttonState);
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(buttonName, buttonState);
                ChangeButtonColor(false);
            }
        }
    }
}
