using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OnScreenKeyboard : MonoBehaviour {

    TouchScreenKeyboard keyboard;
    // public static string keyboardText = "";
    Text textComp = null;

    void Start () {

    }

    public void openKeyboard(Text t){
    	keyboard = TouchScreenKeyboard.Open(t.text, TouchScreenKeyboardType.Default, false, false, false, false);
    	textComp = t;
    }

    void Update () {

        if (keyboard != null && keyboard.active == false && textComp != null)
        {
            if (keyboard.done == true)
            {
                // keyboardText = keyboard.text;
                textComp.text = keyboard.text;
                keyboard = null;
                textComp = null;
            }
        }
   }
}