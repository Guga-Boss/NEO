using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Watch : MonoBehaviour 
{
    public static Stopwatch sw = new Stopwatch();
    public static void Run()
    {          
        sw.Start();
    }
    public static void Stop()
    {
        sw.Stop();
        UnityEngine.Debug.Log( "Tempo de execução: " + sw.ElapsedMilliseconds + " ms" );
    }
}
