using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace zmachine
{
    public class IO: MonoBehaviour
    {
        GameObject output;
		GameObject input;

        InputField typeInput;

		Text text;

        string inputtext;
//		bool var = false;

        private void Start()
        {
 
        }

        private string GetInput()
        {
			input = GameObject.FindWithTag("Input");
			typeInput = input.GetComponent<InputField>();
			typeInput.onEndEdit.AddListener(StoreInput);  
//			while (!var) {
//				var = Input.GetKeyDown(KeyCode.KeypadEnter);
//			}
            // Pause operation of method until user presses "Enter" key to store an input.
			inputtext = typeInput.text;
			Debug.Log("Reading text: " + typeInput.text);
			Debug.Log ("Reading IO stored string: " + inputtext);

            return inputtext;
        }

        private void StoreInput(string arg0)
        {
            // A way to pass an input to the inputtext string.
            Debug.Log("Storing text: " + arg0);
            inputtext = arg0;
        }

        public string ReadLine()
        {
            // When this runs, the input field will contain the previous command. 
            // If this is true, then wait for some new user input.
            //======================= Just for testing ===================


            // ===========================================================
            string text = GetInput();
		    Debug.Log("Input String:" + text);	

            return text;
        }

		// Reads single keypress
		//public ConsoleKey ReadKey()
		//{
		//	ConsoleKey key = new ConsoleKey ();
			// key = (ConsoleKey)Input.inputString;
		//	return key;
		//}

		// Writes given string to output module
        public void Write(String str)
        {

            output = GameObject.FindWithTag("Output");
            Text text = output.GetComponent<Text>();
			text.text += str;
			Debug.Log ("Printed String: " + str);
        }

		// Writes given string to output module and prints new line character
        public void WriteLine(String str)
        {
            output = GameObject.FindWithTag("Output");
            Text text = output.GetComponent<Text>();
			text.text += str + "\n";
			Debug.Log ("Printed Line" + str);
		}


    }
}
