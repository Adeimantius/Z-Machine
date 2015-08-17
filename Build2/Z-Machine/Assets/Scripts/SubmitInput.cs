using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SubmitInput : MonoBehaviour {

	// Use this for initialization
	void Start () {
        InputField input = gameObject.GetComponent<InputField>();
        //Adds a listener that invokes the "LockInput" method when the player finishes editing the main input field.
        //Passes the main input field into the method when "LockInput" is invoked
        input.onEndEdit.AddListener(SubmitName);
	}
    private void SubmitName(string arg0)
    {
        Debug.Log(arg0);
    }

}
