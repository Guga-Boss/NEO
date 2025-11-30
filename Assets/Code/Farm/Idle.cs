using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class Idle : MonoBehaviour
{
    public string FileName;
    public DateTime StartTime;
    public float OffSeconds = 0;
    public float TotalSeconds = 0;

	public void StartIt () 
    {
        if( Load() == false )
            StartTime = Manager.I.Reward.now;
	}
    public DateTime GetNow()
    {
        return Manager.I.Reward.now;
    }
	void Update () 
    {
        DateTime now = GetNow();
        TimeSpan difference = now.Subtract( StartTime );
        TotalSeconds = ( float ) difference.TotalSeconds + OffSeconds;
        //Debug.Log( "dif :" + difference.TotalSeconds.ToString( "0.0" ) + " off: " 
        //+ OffSeconds.ToString( "0.0" ) + " tot:  " + TotalSeconds.ToString( "0.0" ) );  
	}

    public void Save()
    {
        if( Manager.I.SaveOnEndGame == false ) return;
        if( G.Tutorial.CanSave() == false ) return;

        string file = Manager.I.GetProfileFolder() + "Idle.NEO";

        using( MemoryStream ms = new MemoryStream() )
        using( BinaryWriter writer = new BinaryWriter( ms ) )                                // Open Memory Stream
        {
            GS.W = writer;                                                                   // Assign BinaryWriter to GS.W for TF

            int Version = Security.SaveHeader( 1 );                                          // Save Header Defining Current Save Version  
          
            DateTime now = GetNow();
            GS.W.Write( now.ToBinary() );                                                    // salva como long no binário
          
            GS.W.Flush();                                                                    // Flush the writer

            Security.FinalizeSave( ms, file );                                               // Finalize save
        }       
    }

    public bool Load()
    {
        string file = Manager.I.GetProfileFolder() + "Idle.NEO";

        if( !System.IO.File.Exists( file ) )
        {
            OffSeconds = 0;
            return false;
        }

        byte[] fileData = File.ReadAllBytes( file );                                       // Read full file
        byte[] content = Security.CheckLoad( fileData );                                   // Validate HMAC and get clean content

        using( GS.R = new BinaryReader( new MemoryStream( content ) ) )                    // Use MemoryStream for TF
        {
            int SaveVersion = Security.LoadHeader();                                       // Load Header

            long l = GS.R.ReadInt64();

            StartTime = DateTime.FromBinary( l );
            DateTime now = GetNow();
            Debug.Log( now );
            TimeSpan difference = now.Subtract( StartTime );
            OffSeconds = ( float ) difference.TotalSeconds;

            Debug.Log( "Idle Engine: " + FileName + " - You have Stayed for: " + Util.ToTime( OffSeconds ) + " Offline!" );
            return true;
        }
    }
}
