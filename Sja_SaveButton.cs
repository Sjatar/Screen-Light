using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.EventSystems;

namespace Sja_UI
{
    class Sja_SaveButton : MonoBehaviour
    {
        private Button mainButton;
        public string setting_name;

        public void Start()
        {
            // Get button!
            mainButton = GetComponent(typeof(Button)) as Button;
            // Add listener to if button is pressed. It will run ButtonPressCheck if it is!
            mainButton.onClick.AddListener(delegate { ButtonPressCheck(); });
        }
        
        public void ButtonPressCheck()
        {
            // If the dictionary exists, which it always should but just in case.
            if (Sja_UICore.VNyanParameters != null)
            {
                // Write the dictionary to a settings file!
                VNyanInterface.VNyanInterface.VNyanSettings.saveSettings(setting_name,Sja_UICore.VNyanParameters);
            }
        }
    }
}