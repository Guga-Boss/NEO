using UnityEngine;
using System.Collections;
using PathologicalGames;
using DarkTonic.MasterAudio;

public enum EBoltType
{
    NONE = -1, Arrow, Rock, Hugger, Spear, Altar,
    Item, Spell, Shit,
    Throwing_Axe,
    Hook,
    Glove
}
public class ArcherArrowAnimation : MonoBehaviour {

	public Vector2 OrigPos, TileTarget;
    public Vector3 RandomFactor;
    public GameObject TargetObject;
	public string MyName;
    public tk2dSprite Sprite;
	Transform Trans;
    public EBoltType Type = EBoltType.Arrow;
    public float LastDistance, AnimationTime, StartDelay = 0, AfterReachTime = 0;
    public float FadeAnimationTime = -1;
    float FrameCount = 0;
    public Unit TargetUnit, AuxUnit;
    public float ArrowSpeed = 16;
    public bool TargetReached = false, FadeAnimation = true;
    public static int PlayExplosion;
    public static int SpriteID;
    public static EBoltType BoltType;
    public static int SpellPosition;
    public static float TotalFadeAnimationTime = 6;
    public static float FixedRotation;
    public Color SprCol;
    public static Vector3 FixedPosition;
    public static Spell Spell;
    public static void Reset()
    {
        BoltType = EBoltType.NONE;
        SpriteID = -1;
        SpellPosition = -1;
        FixedRotation = 999;
        FixedPosition = new Vector3( -1, -1, -1 );
        Spell = null;
    }
	// Use this for initialization
    public ArcherArrowAnimation Create( Vector2 Pos, Unit _unit, Transform tr, Vector2 tiletg, EBoltType type, GameObject targetobj = null )
	{
        Type = type;
        if( BoltType != EBoltType.NONE ) Type = BoltType;
        TargetUnit = _unit;
        OrigPos = Pos;
		Trans = tr;
        TileTarget = tiletg;

        TargetObject = targetobj;
        LastDistance = 999;
        AfterReachTime = 0;
        TotalFadeAnimationTime = 6;
        PlayExplosion = 0;
        float rad = .35f;
        if( _unit && _unit.UnitType == EUnitType.HERO ) rad = .11f;
        AnimationTime = 0;
        FadeAnimationTime = -1;
        TargetReached = false;
        StartDelay = 0;
        FadeAnimation = true;
        Sprite.spriteId = 352;
        FrameCount = 0;
        Sprite.Collection = Map.I.SpriteCollectionList[ ( int ) ESpriteCol.MONSTER_ANIM ];
        tr.localScale = new Vector3( 1, 1, 1 );
        SprCol = Color.white;
        string snd = "";
        
        if( _unit && _unit.TileID == ETileType.ROACH ) 
            FadeAnimation = false;

        ArrowSpeed = Random.Range( 33f, 44f );
        if( Type == EBoltType.Rock )
        {
            Sprite.spriteId = 70;
            ArrowSpeed = 15;
        }
        if( Type == EBoltType.Hugger )
        {
            Sprite.spriteId = 142;
        }
        if( Type == EBoltType.Hook )
        {
            Sprite.spriteId = 245;
            ArrowSpeed = 15;
            TotalFadeAnimationTime = 1;
            rad = 0;
        }

        if( Type == EBoltType.Shit )
        {
            Sprite.spriteId = 68;
            SprCol = new Color( 1, 1, 1, .2f );
            TargetUnit = null;
        }

        if( Type == EBoltType.Spear )
        {
            Sprite.spriteId = 553;
            StartDelay = Attack.CurrentHit * .1f;
            ArrowSpeed = Random.Range( 12, 22 ) + ( Vector2.Distance( OrigPos, _unit.Pos ) * 2 );
            snd ="Axe Throw";            
        }

        if( Type == EBoltType.Throwing_Axe )
        {
            Sprite.spriteId = 551;
            ArrowSpeed = 18 + ( Vector2.Distance( OrigPos, _unit.Pos ) * 2 );
            snd = "Axe Throw";
        }
        if( Type == EBoltType.Glove )
        {
            Sprite.spriteId = 305;
            tr.localScale = new Vector3( 1.3f, 1.3f, 1 );
            TotalFadeAnimationTime = 1;
        }
        if( Type == EBoltType.Altar )
        {
            Sprite.spriteId = AltarBonusStruct.BoltSpriteID;
            FadeAnimation = false;
            ArrowSpeed = 10f;
            tr.localScale = new Vector3( .6f, .6f, 1 );
        }
        if( Type == EBoltType.Item )
        {
            Sprite.Collection = Map.I.SpriteCollectionList[ ( int ) ESpriteCol.ITEM ];
            Sprite.spriteId = G.GIT( SpriteID ).TKSprite.spriteId;
            ArrowSpeed = 20f;
            tr.localScale = new Vector3( .5f, .5f, .5f );
        }
        else
        if( Type == EBoltType.Spell )
        {
            Sprite.Collection = Map.I.SpriteCollectionList[ ( int ) ESpriteCol.ITEM ];
            Sprite.spriteId = G.GIT( SpriteID ).TKSprite.spriteId;
            ArrowSpeed = 20f;
            tr.localScale = new Vector3( .5f, .5f, .5f );
            FadeAnimation = false;
            if( ArcherArrowAnimation.SpriteID == ( int ) ItemType.Res_Hook_CW ||
                ArcherArrowAnimation.SpriteID == ( int ) ItemType.Res_Hook_CCW )
            {
                TotalFadeAnimationTime = 4;
                FadeAnimation = true;
                snd = "Trap 1";
                DestroyMe();
                return this;
            }
        }
        if( Type == EBoltType.Arrow )
        {
            Sprite.SetSprite( Map.I.SpriteCollectionList[ ( int ) ESpriteCol.MONSTER_ANIM ], 351 ); // To avoid the green square instead of arrow bug
            Sprite.SetSprite( Map.I.SpriteCollectionList[ ( int ) ESpriteCol.MONSTER_ANIM ], 352 );
        }

        transform.position = new Vector3( Pos.x, Pos.y, -4 );

        if( Sprite )
            Sprite.color = SprCol;
        RandomFactor = new Vector3( Random.Range( -rad, +rad ), Random.Range( -rad, +rad ), 0 );
        if( _unit != null )
        {
            if( _unit.Control.IsFlyingUnit== false ||                                                  // this has been added to allowe multiple shots of spear in a single turn, if 2 lives monster died first shot stayed stopped
                _unit.Body.IsDead )
            {
                TileTarget = _unit.Pos;
                TargetUnit = null;
            }
        }

        if( snd != "" )
            MasterAudio.PlaySound3DAtVector3( snd, transform.position );                                 // Sound FX
        return this;
	}

	// Update is called once per frame
    void Update()
    {
        if( Map.I.IsPaused() ) return;
        StartDelay -= Time.deltaTime;
        if( StartDelay > 0 ) return;

        float z = -3.5f;
        if( Type == EBoltType.Glove )
        {
            z = -4;
            if( G.HS.GloveTarget.x != -1 )
                FadeAnimationTime = .1f;
        }

        Vector3 tgt = new Vector3( TileTarget.x, TileTarget.y, z ) + RandomFactor;
        float step = ArrowSpeed * Time.deltaTime;
        Vector2 tg = TileTarget;

        if( TargetUnit != null )                                                                                                        // Arrow follows unit position                                
        {
            tgt = TargetUnit.transform.position + RandomFactor;
        }

        if( TargetObject != null )                                                                                                        // Arrow follows unit position                                
        {
            tgt = TargetObject.transform.position;
        }

        if( FixedPosition.x != -1 )
            transform.position = new Vector3( FixedPosition.x, FixedPosition.y, -1.85f );

        float dist = -1;
        if( TargetReached == false )
        {
            transform.position = Vector3.MoveTowards( transform.position, tgt, step );
            int x = -1;
            int y = -1;
            Map.I.Tilemap.GetTileAtPosition( transform.position, out x, out y );
            AnimationTime += Time.deltaTime;

            if( TargetObject != null )
            {
                tg = TargetObject.transform.position;
                if( x == ( int ) TargetObject.transform.position.x &&
                    y == ( int ) TargetObject.transform.position.y )
                    TargetReached = true;
            }
            else
            if( TargetUnit != null )
            {
                tg = TargetUnit.transform.position;
                if( TargetUnit.Body.IsDead ) TargetReached = true;
                if( x == ( int ) TargetUnit.transform.position.x &&
                    y == ( int ) TargetUnit.transform.position.y ) 
                    TargetReached = true;
            }
            else
            {
                if( transform.position.x == TileTarget.x )
                if( transform.position.y == TileTarget.y ) 
                    TargetReached = true;

                if( transform.position.x == tgt.x )
                if( transform.position.y == tgt.y ) 
                    TargetReached = true;
            }

            dist = ( int ) Vector2.Distance( transform.position, tg );
            if( LastDistance < dist ) TargetReached = true;
        }

        if( TargetUnit && TargetUnit.UnitType == EUnitType.HERO )
        {
            transform.position = new Vector3( TargetUnit.Graphic.transform.position.x, 
            TargetUnit.Graphic.transform.position.y, -1.85f ) + RandomFactor;
            TargetReached = true;
        }

        if( Type == EBoltType.Throwing_Axe )
        if( TargetReached == false )
            {
                float rot = transform.eulerAngles.z;                                        // axe rotation animation
                rot += Time.deltaTime * 500;
                if( rot > 360 ) rot -= 360;
                transform.eulerAngles = new Vector3( 0, 0, rot );
            }

        if( Type == EBoltType.Hook )                                                       // hook specific:
        {
            transform.eulerAngles = new Vector3( 0, 0, FixedRotation );
            if( TargetReached )
            if( FadeAnimationTime == -1 )
            {
                transform.position = new Vector3( tgt.x, tgt.y, -2.85f );
                MasterAudio.PlaySound3DAtVector3( "Raft Merge", G.Hero.Pos );
                Controller.CreateMagicEffect( tgt );                                       // FX
            }
            if( AuxUnit )
            {
                AuxUnit.Body.RopeDestination.transform.position = transform.position;      //  needs to animate chain too
                AuxUnit.Body.Rope.AutoCalculateAmountOfNodes = true;
            }
        }

        if( TargetReached )
        {
            if( AfterReachTime > 0 ) 
              { AfterReachTime -= Time.deltaTime; return; }                               // start counting time after reaching target

            if( FixedRotation != 999 )
            {
                transform.eulerAngles = new Vector3( 0, 0, FixedRotation );
            }
            else
            if( FadeAnimationTime == -1 )
            {
                FadeAnimationTime = 0;
                Vector3 euler = transform.eulerAngles;                                                          // random rotation for better effect
                euler.z += Random.Range( -15f, 15f );
                transform.eulerAngles = euler;
            }
            
            FadeAnimationTime += Time.deltaTime;
            float fact = TotalFadeAnimationTime;
            float alpha = 1 - ( FadeAnimationTime / fact );
            if( alpha > SprCol.a ) alpha = SprCol.a;

            if( Sprite )
                Sprite.color = new Color( SprCol.r, SprCol.g, SprCol.b, alpha );

            bool kill = false;
            if( FadeAnimationTime / fact > 1 || FadeAnimation == false ) kill = true;

            if( Type == EBoltType.Spell && TargetUnit == null ) kill = true;

            if( kill )
                DestroyMe();               
        }
        LastDistance = dist;
        FrameCount++;
    }
    public void DestroyMe()
    {
        if( Type == EBoltType.Spell ) Spell.AttachSpell( TargetUnit, SpellPosition );
        if( PlayExplosion > 0 )
            Map.I.CreateExplosionFX( transform.position, "Fire Explosion", "" );                                   // Explosion FX
        Sprite.SetSprite( Map.I.SpriteCollectionList[ ( int ) ESpriteCol.MONSTER_ANIM ], 351 ); // To avoid the green square instead of arrow bug
        Sprite.SetSprite( Map.I.SpriteCollectionList[ ( int ) ESpriteCol.MONSTER_ANIM ], 352 );
        Reset();
        PoolManager.Pools[ "Pool" ].Despawn( Trans );
    }

    public static ArcherArrowAnimation Create( Vector2 from, Vector2 to, EBoltType type, int spr, float speed = 30 )
    {
        Transform tr = PoolManager.Pools[ "Pool" ].Spawn( "Archer Arrow" );            
        ArcherArrowAnimation ar = tr.gameObject.GetComponent<ArcherArrowAnimation>();
        ar.Create( from, null, tr, to, type );
        ar.ArrowSpeed = speed;
        ar.Sprite.spriteId = spr;
        return ar;
    }
}
