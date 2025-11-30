using UnityEngine;
using System.Collections;
using PathologicalGames;

public class Message : MonoBehaviour
{
    public bool MoveRight, MoveUp, ShowIcon;
    public float DurationTime, Delay;
    public tk2dTextMesh MessageTxt;
    public tk2dSprite Icon;
    public float SpeedFactor, Size;
    // Use this for initialization
    void OnSpawn()
    {
        DurationTime = 3.5f;
        SpeedFactor = Random.Range( 0.4f, 0.8f );
    }

    // Update is called once per frame
    void Update()
    {
        Delay -= Time.deltaTime;

        if( Delay < 0 )
        {
            if( !MessageTxt.gameObject.activeSelf ) 
                MessageTxt.gameObject.SetActive( true );
            if( !Icon.gameObject.activeSelf )
            if( ShowIcon )
                Icon.gameObject.SetActive( true );
        }
        else return;

        if( SpeedFactor == 0 ) SpeedFactor = Random.Range( 0.4f, 0.8f );

        float spdx = -1;
        float spdy = 1;
        if( MoveRight ) spdx = 1;
        if( MoveUp ) spdy = -1;

        spdx *= SpeedFactor;
        spdy *= SpeedFactor;

        if( Map.I.AreaCleared == true || ( Map.I.CurArea == null || Map.I.CurArea.Cleared == true ) )
        {
            float fact = Size / 100;
            transform.localScale = new Vector3( 3 * fact, 3 * fact, 3 );    
        }
        else
        if( Map.I.CurrentArea != -1 )
        {
            spdx /= 2;
            spdy /= 2;
        }           
        if( SpeedFactor != -1 )
            transform.position = new Vector3( transform.position.x + spdx * Time.deltaTime, transform.position.y + spdy * Time.deltaTime, -2 );
        
        DurationTime -= Time.deltaTime;
        if( DurationTime < 0 )
        {
            PoolManager.Pools[ "Pool" ].Despawn( transform );
        }

        MessageTxt.color = new Color( MessageTxt.color.r, MessageTxt.color.g, MessageTxt.color.b, MessageTxt.color.a - ( Time.deltaTime / DurationTime ) );
        Icon.color = new Color( 1, 1, 1, MessageTxt.color.a - ( Time.deltaTime / DurationTime ) );
    }

    public static void RedMessage( string txt )
    {
        Message.CreateMessage( ETileType.NONE, txt, Map.I.Hero.Pos, new Color( 1, 0, 0, 1 ), Util.Chance( 50 ), Util.Chance( 50 ), 10 );
    }

    public static void RedMessage( string txt, Vector2 pos )
    {
        Message.CreateMessage( ETileType.NONE, txt, pos, new Color( 1, 0, 0, 1 ), Util.Chance( 50 ), Util.Chance( 50 ), 10 );
    }

    public static void GreenMessage( string txt )
    {
        Message.CreateMessage( ETileType.NONE, txt, Map.I.Hero.Pos, new Color( 0, 1, 0, 1 ), Util.Chance( 50 ), Util.Chance( 50 ), 10 );
    }

    public static void GreenMessage( string txt, Vector2 pos )
    {
        Message.CreateMessage( ETileType.NONE, txt, pos, new Color( 0, 1, 0, 1 ), Util.Chance( 50 ), Util.Chance( 50 ), 10 );
    }
    public static void StaticMsg( string txt, Vector2 pos, Color col, float time, float size = 100 )
    {
        Message.CreateMessage( ETileType.NONE, ItemType.NONE, txt, pos + 
        new Vector2( -0.3f, -0.1f ), col, false, false, time, 0, -1, size );
    }

    public static void CreateMessage( ETileType icon,  string txt, Vector3 pos, Color col, 
    bool moveright = true, bool moveup = true, float time = 4, float delay = 0, float speed = 0, float size = 100 )
    {
        Message.CreateMessage( icon, ItemType.NONE, txt, pos, col, moveright, moveup, time, delay, speed, size );
    }

    public static Message CreateMessage( ETileType icon, ItemType itemicon, string txt, Vector3 pos, Color col,
    bool moveright = true, bool moveup = true, float time = 4, float delay = 0, float speed = 0, float size = 100 )
    {
        Transform tr = null;
        Message msg = null;

        if( itemicon != ItemType.NONE )
        {
            tr = PoolManager.Pools[ "Pool" ].Spawn( "Message Items" );
            msg = tr.gameObject.GetComponent<Message>();
            msg.Icon.gameObject.SetActive( true );
            msg.ShowIcon = true;
            tk2dSprite sp = G.GIT( itemicon ).TKSprite;
            if( sp != null ) msg.Icon.spriteId = sp.spriteId;
            else msg.ShowIcon = false;
            msg.transform.position += new Vector3( .5f, 0, 0 );
        }
        else
        if( icon != ETileType.NONE )
        {
            tr = PoolManager.Pools[ "Pool" ].Spawn( "Message Tiles" );
            msg = tr.gameObject.GetComponent<Message>();
            msg.Icon.spriteId = ( int ) icon;
            msg.Icon.gameObject.SetActive( true );
            msg.ShowIcon = true;
            msg.transform.position += new Vector3( .5f, 0, 0 );
        }
        else
        {
            tr = PoolManager.Pools[ "Pool" ].Spawn( "Message Tiles" );
            msg = tr.gameObject.GetComponent<Message>();
            msg.Icon.spriteId = -1;
            msg.Icon.gameObject.SetActive( false );
            msg.ShowIcon = false;
        }

        msg.transform.position = pos;

        if( Map.I.CurrentArea != -1 )
        {
            msg.transform.localScale = new Vector3( 1.5f, 1.5f, 1 );
        }

        msg.MessageTxt.text = txt;
        msg.MoveRight = moveright;
        msg.MoveUp = moveup;
        msg.MessageTxt.color = col;
        msg.DurationTime = time;
        msg.Delay = delay;
        msg.SpeedFactor = speed;
        msg.Size = size;
        float fact = size / 100;
        msg.transform.localScale = new Vector3( 1 * fact, 1 * fact, 1 );

        if( delay > 0 )
        {
            msg.MessageTxt.gameObject.SetActive( false );
            msg.Icon.gameObject.SetActive( false );
        }
        return msg;
    }
}
