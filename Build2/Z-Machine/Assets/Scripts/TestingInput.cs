using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class TestingInput : MonoBehaviour {


    public GameObject output;
    public GameObject input;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public string ReadLine()
    {
        input = GameObject.FindWithTag("Input");
        Text text = input.GetComponent<Text>();

        //            Write(text.text);
        //			Debug.Log("Input String:" + text.text);	

        return text.text;
    }
    public void Write(String str)
    {
        output = GameObject.FindWithTag("Output");
        Text text = output.GetComponent<Text>();
        text.text += str;
        Debug.Log("Printed String: " + str);
    }

    public void TestRead()
    {
        Write("Input Works" + ReadLine());
    }
}
