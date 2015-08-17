using UnityEngine;
using System.Collections;
using zmachine;


class RunZork:MonoBehaviour 
{
    zmachine.Machine machine = new zmachine.Machine ("ZORK1.DAT");
    int numInstructionsProcessed = 0;
    void Start()
    {
        Debug.Log("Starting machine Coroutine");
        StartCoroutine(Run());
        
    }
    //public void Update (){

    //    while (!machine.isFinished())
    //    {
    //        machine.processInstruction();
    //        ++numInstructionsProcessed;
    //    }
    //    Debug.Log("Instructions processed: " + numInstructionsProcessed);
    //}

    IEnumerator Run()
    {
        // process instructions
        yield return "ProcessInstruction Coroutine in progress";
            while (!machine.isFinished())
            {
                machine.processInstruction();
                ++numInstructionsProcessed;
            }
            Debug.Log("Coroutine executed");

    }
    IEnumerator Example()
    {
        print("Starting " + Time.time);

        // Start function RunZork as a coroutine
        yield return StartCoroutine(Run());
        print("Done " + Time.time);
    }
	}
			

