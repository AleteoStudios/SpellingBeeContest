using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayManager : MonoBehaviour
{

    void Start()
    {
        for(int i = 0; 1<Display.displays.Length; ++i)
        {
            Display.displays[i].Activate(1920,1080,60);
        }
    }

   
}
