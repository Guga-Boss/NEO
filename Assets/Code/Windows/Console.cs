using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class Console : MonoBehaviour {

    public UIInput InputText;
    public void Submit()
    {
        string text = InputText.value;
        text.ToLower();

        if( InputText.value == "1864830753" )
        {
            Manager.I.FullVersion = true;
            string file = Manager.I.GetGameFolder() + "Data/Camera Data.dat";
            ES2.Save( Manager.I.FullVersion, file + "?tag=Camera Data" );
            InputText.value = "Full Version Activated.";
        }
        else
        if( InputText.value == "Demote WQ" )
            {
                Manager.I.FullVersion = false;
                string file = Manager.I.GetGameFolder() + "Data/Camera Data.dat";
                ES2.Save( Manager.I.FullVersion, file + "?tag=Camera Data" );
                InputText.value = "Full Version De-Activated.";
            }
            else
        {
            Map.I.RM.RMD.ProcessScript( InputText.value );
        }

        Map.I.InvalidateInputTimer = .5f;
    }
}
