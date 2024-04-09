using System;
using UnityEngine;
using UnityEngine.UI;

namespace Sja_UI
{
    class Sja_InputField : MonoBehaviour
    {
        // You will want to edit the name. This is the name the VNyanParameter will have in VNyan!
        public string fieldName;
        // If no other value was set we have the field set to 0. 
        public float fieldValue = 0;
        private InputField mainField;
        private Button mainButton;

        public void Start()
        {
            // We add the inputfield as the mainfield
            mainField = GetComponent(typeof(InputField)) as InputField;
            // We add a button as confirmation to change the inputted value
            mainButton = GetComponentInChildren(typeof(Button)) as Button;
            
            // We add a listener that will run ButtonPressCheck if the button is pressed.
            mainButton.onClick.AddListener(delegate {ButtonPressCheck(); });

            // Here we either want to load a existing parameter or add one!
            // A loaded parameter comes from Sja_UICore when it loads the setting Json into the dictionary!
            if (Sja_UICore.VNyanParameters.ContainsKey(fieldName))
            {
                // If the parameter exist, set fieldValue to that value.
                fieldValue = Convert.ToSingle(Sja_UICore.VNyanParameters[fieldName]);
                // We want to try to show the user the current value. This is done by converting the value to a string.
                // And setting the field text to it.
                mainField.text = Convert.ToString(fieldValue);
                // Lastly we set the VNyanParameter to this value!
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(fieldName, fieldValue);
            }
            else
            {
                // If it was the first time and there is no parameter we want to add it!
                // field name is the name of the parameter and fieldvalue will be the value.
                Sja_UICore.VNyanParameters.Add(fieldName, Convert.ToString(fieldValue));
                // We want to try to show the value at all times.
                mainField.text = Convert.ToString(fieldValue);
                // Set the Vnyan parameter to this name and value!
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(fieldName, fieldValue);
            }
        }
        
        public void ButtonPressCheck()
        {
            // We need to sanitate the input a bit. Unless the input can be converted to a float we can't use it.
            if (float.TryParse(mainField.text, out float fieldValue))
            {
                // Set the new value!
                Sja_UICore.VNyanParameters[fieldName] = Convert.ToString(fieldValue);
                // Set the VNyanparameter!
                VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(fieldName, fieldValue);
            }
            else
            {
                // If the value was not able to be converted we just want to show the current value.
                // This overwrites what the user typed.
                mainField.text = Sja_UICore.VNyanParameters[fieldName];
            }
        }
    }
}
