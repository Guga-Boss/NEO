using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class GlobalAltar : SerializedMonoBehaviour
{
    public static GlobalAltar I;
    [Header( "Tables" )]
    public TechTable SlotsTable;
    public TechTable SpecialSlotsTable;
    public TechTable BonusQualityTable;

    [Header( "Spawn Butcher Chance:" )]
    public float FirstCubeSpawnChance;
    public float LastCubeSpawnChance;
    public float SpawnChanceCurve;

    [Header( "Levels:" )]
    public bool UseItemLevels;
    public float ButcherLevel;
    public float ButcherBonusCoinsLevel;
    public float ButcherBonusQualityLevel;
    public float ButcherSlotsLevel;
    public float ButcherPowerLevel;

    [Header( "Coins:" )]
    public float CoinCurve = 1.5f;
    public float CoinLevelCurve = 0.5f;
    public int CurrentCube = 30;
    public int TotalCubes = 30;
    public float FirstCubeCoin = 1;
    public float FinalCubeCoin = 8;
    public float CoinRandomFactor = 10;

    [Header( "Speed:" )]
    public float BaseButcherSpeed = 360;
    public float FinalButcherSpeed = 60;
    public float SpeedCurve;
    public float SpeedEndCurve;

    [Header( "Angle:" )]
    public float BaseButcherAngle = 5;
    public float FinalButcherAngle = 90;
    public float AngleCurve;
    public float AngleEndCurve;

    [Header( "Spawn:" )]
    public float ButchersToBeSpawned;
    [Header( "Items to be Overwriten by Special Bonuses" )]
    public List<ItemType> CheapItemList;

    [System.Serializable]
    [InlineProperty]                   // mostra os campos direto, sem box
    [HideReferenceObjectPicker]        // remove o dropdown/asterisco
    public class PassiveBonus
    {
        [HorizontalGroup( "Row1", Width = 0.8f )]  // 60% da linha
        [LabelWidth( 90 )] // nome
        public EAltarBonusType Type;
        [HorizontalGroup( "Row2", Width = 0.5f )]  // 40% da linha
        [LabelWidth( 90 )] // nome "Factor" curto, não rouba espaço
        public float MinFactor;
        [HorizontalGroup( "Row2", Width = 0.5f )]  // 40% da linha
        [LabelWidth( 90 )] // nome "Factor" curto, não rouba espaço
        public float MaxFactor;
        [HorizontalGroup( "Row3", Width = 0.5f )]  // 40% da linha
        [LabelWidth( 90 )] // nome "Factor" curto, não rouba espaço
        public float MinWeight;
        [HorizontalGroup( "Row3", Width = 0.5f )]  // 40% da linha
        [LabelWidth( 90 )] // nome "Factor" curto, não rouba espaço
        public float MaxWeight = 100;
    };

    public List<PassiveBonus> PassiveBonusList = new List<PassiveBonus>();

    public static List<int> CheapList, SlotsCreated;
    void Start()
    {
        I = this;
    }
    public bool Debugging, DisplayDebug;

#if UNITY_EDITOR
    [Button( "Spawn Butcher", ButtonSizes.Large ), GUIColor( 1f, 0.52f, 0.1f )]
    public void EditQuestCallBack()
    {
        for( int i = 0; i < ButchersToBeSpawned; i++ )
            CreateRandomButcher();
    }
#endif

    private List<float> Interpolate( float[] start, float[] end, float t )
    {
        List<float> result = new List<float>();
        for( int i = 0; i < start.Length; i++ )
            result.Add( start[ i ] + ( end[ i ] - start[ i ] ) * t );
        return result;
    }

    public List<float> GetWeights( int points, TechTable tb )
    {
        int numLists = tb.chanceMatrix.GetLength( 0 ); // number of rows in chance matrix
        if( tb.arrangeLimits.Count >= numLists )
            tb.arrangeLimits.RemoveRange( numLists - 1, tb.arrangeLimits.Count - ( numLists - 1 ) ); // keep arrangeLimits aligned with matrix

        int listLength = tb.chanceMatrix.GetLength( 1 ); // number of columns in chance matrix
        List<float> weights = new List<float>();

        int startIndex = 0; // index of the lower bound
        int endIndex = 0;   // index of the upper bound
        float t = 0;        // interpolation factor

        // find correct interval inside arrangeLimits
        for( int i = 0; i < tb.arrangeLimits.Count; i++ )
            if( i < numLists )
            {
                if( points <= tb.arrangeLimits[ i ] )
                {
                    startIndex = i; // lower interval
                    endIndex = i + 1; // upper interval
                    float prevLimit = ( i == 0 ) ? 0 : tb.arrangeLimits[ i - 1 ]; // previous limit
                    t = ( float ) ( points - prevLimit ) / ( tb.arrangeLimits[ i ] - prevLimit ); // normalized interpolation value
                    break;
                }
            }

        if( points > tb.arrangeLimits[ tb.arrangeLimits.Count - 1 ] ) // case when points exceed last limit
        {
            float over = points - tb.arrangeLimits[ tb.arrangeLimits.Count - 1 ]; // how much over the limit
            float growth = Mathf.Pow( over, tb.OverTableCurve ); // apply curve shaping to growth
            for( int i = 0; i < listLength; i++ )
                weights.Add( tb.chanceMatrix[ numLists - 1, i ] * ( 1 + tb.OverTableFactor * growth ) ); // scaled last row
        }
        else // inside normal range
        {
            float[] startArr = new float[ listLength ]; // row for lower bound
            float[] endArr = new float[ listLength ];   // row for upper bound
            for( int i = 0; i < listLength; i++ )
            {
                startArr[ i ] = tb.chanceMatrix[ startIndex, i ]; // copy from start row
                endArr[ i ] = tb.chanceMatrix[ endIndex, i ];     // copy from end row
            }

            float tCurved = Mathf.Pow( t, tb.Curve ); // apply curve to interpolation factor
            weights = Interpolate( startArr, endArr, tCurved ); // smooth interpolation with curve
        }

        return weights; // not normalized here
    }

    public void TestSampleActiveSides( TechTable tb )
    {
        int listLength = tb.chanceMatrix.GetLength( 1 );
        string header = "Level\t";
        for( int i = 1; i <= listLength; i++ )
            header += "L" + i + "             ";
        Debug.Log( header );

        foreach( int points in tb.TestPoints )
        {
            List<float> weights = GetWeights( points, tb );

            string str = points.ToString() + "\t";
            for( int i = 0; i < listLength; i++ )
                str += weights[ i ].ToString( "F2" ) + "\t";
            Debug.Log( str );
        }

        int rows = tb.chanceMatrix.GetLength( 0 );
        int cols = tb.chanceMatrix.GetLength( 1 );

        string line = "Row sums -> ";
        for( int row = 0; row < rows; row++ )
        {
            float sum = 0f;
            for( int col = 0; col < cols; col++ )
            {
                sum += tb.chanceMatrix[ row, col ];
            }
            line += "R" + ( row + 1 ) + ": " + sum + "  ";
        }
        Debug.Log( line );
        Debug.Log( "=== END DEBUG ===" );
    }

    public void CreateRandomButcher( bool cubeClear = true )
    {
        if( G.HS == null ) return;
        if( G.HS.AltarCount > 0 ) return;                                                          // do not create altars in Gameplay Altars cube
        if( Map.I.RM.RMD.EnableAlternateStartingCube == false ) return;                            // no butchers in linear quests

        if( Application.isPlaying )
            Debugging = false;

        RetrieveLevels();                                                                          // Retrieve levels from item levels

        Unit un = null;
        if( !Debugging )
        {
            un = SpawnButcher();                                                                   // Spawns the Butcher in a random position
            if( un == null ) return;
        }

        List<float> ChanceList = GetWeights( ( int ) ButcherSlotsLevel, SlotsTable );              // Sorts number of slots
        int slots = Mathf.Min( 1 + ( int ) Util.Sort( ChanceList ), 8 );

        ChanceList = GetWeights( ( int ) ButcherPowerLevel, SpecialSlotsTable );                   // Sorts number of Special slots
        int specialslots = ( int ) Util.Sort( ChanceList );
        if( specialslots >= 1 ) 
            specialslots = Util.FloatSort( specialslots );

        if( !Debugging ) un.Dir = Util.Chance( 50 ) ? EDirection.W : EDirection.E;                 // set random rotation

        List<int> freeslots = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
        Util.ShuffleList( freeslots );                                                             // shuffle free slots list

        int totalSlots = slots + specialslots;                                                     // total slots sum of normal + special
        int rest = totalSlots - 8;                                                                 // remaining slots
        totalSlots = Mathf.Min( totalSlots, 8 );                                                   // clamp total slots to max 8

        CheapList = new List<int>();                                                               // stores Shell/Cog slots for possible overwrite
        SlotsCreated = new List<int>();                                                            // stores all created slot IDs

        // ---- CREATE NORMAL SLOTS ----
        for( int i = slots - 1; i >= 0 && i < freeslots.Count; i-- )                               // loop through all normal slots
        {
            int sid = freeslots[ i ];                                                              // id from free slots
            AddItem( un, sid, false );                                                             // add normal bonus item
            freeslots.RemoveAt( i );                                                               // remove used slot from free list
        }

        // ---- CREATE SPECIAL SLOTS ----
        int specialToCreate = Mathf.Min( specialslots, freeslots.Count );                          // limit to available empty slots
        for( int i = specialToCreate - 1; i >= 0; i-- )                                            // create Special Powers over EMPTY slots
        {
            int sid = freeslots[ i ];                                                              // id from free slots
            AddItem( un, sid, true );                                                              // add special bonus
            freeslots.RemoveAt( i );                                                               // remove used slot from free list
        }

        // ---- OVERWRITE CHEAP SLOTS IF NECESSARY ----
        int cheapToCreate = Mathf.Min( rest, CheapList.Count );                                    // limit to available cheap slots
        for( int i = cheapToCreate - 1; i >= 0; i-- )                                              // create Special Powers over CHEAP slots
        {
            int sid = CheapList[ i ];                                                              // id from cheap slots
            AddItem( un, sid, true );                                                              // overwrite Shell/Cog with special
        }

        float speed = Util.GetCurveVal( ButcherLevel, 100, BaseButcherSpeed, 
        FinalButcherSpeed, SpeedCurve, SpeedEndCurve );
        if( !Debugging ) un.Body.RotationSpeed = speed;                                                          // rotation speed

        float vl1 = Util.GetCurveVal( CurrentCube, TotalCubes, FirstCubeCoin, FinalCubeCoin, CoinCurve );        // 1) Bonus coin per cube importance
        float vl2 = Util.GetCurveVal( ButcherBonusCoinsLevel, 100, 0.1f, 4, CoinLevelCurve );                    // 2) Bonus coin per Coin Level item
        float vl3 = Util.GetCurveVal( ButcherLevel, 100, 0.1f, 1, CoinLevelCurve );                              // 3) Bonus coin per Butcher Level item
        float vl4 = Util.GetCurveVal( ButcherSlotsLevel, 100, 0.1f, 1, CoinLevelCurve );                         // 4) Bonus coin per Butcher Slot Level item

        float total = vl1 + vl2 + vl3 + vl4;

        float perc = Random.Range( -CoinRandomFactor, CoinRandomFactor );

        float coinsort = total + Util.Percent( perc, total );

        float coin = Util.FloatSort( coinsort );

        if( DisplayDebug )
        {
            Debug.Log( "Normal Slots: " + slots + " Special Slots: " + specialslots + " Total: " + totalSlots );         // Slots Debug
            //DebugTable( 500, 100, 0, 4, .7f, 0 );
            Debug.Log( "Bonus Coins: " + total.ToString( "0.0" ) + " Importance:  " +                                    // Coin debug
            vl1.ToString( "0.0" ) + " Coin Level: " + vl2.ToString( "0.0" ) + " Butcher Level: " +
            vl3.ToString( "0.0" ) + " Slots Level: " + vl4 + " --- Final: " + coin.ToString( "0.0" ) +
            " Perc: " + perc.ToString( "0.0" ) + "% Coin sort: " + coinsort.ToString( "0.0" ) );
            //DebugTable( 500, 100, BaseButcherAngle, FinalButcherAngle, AngleCurve, AngleEndCurve );                    // Butcher angle TABLE Debug
            //DebugTable( 500, 100, BaseButcherSpeed, FinalButcherSpeed, AngleCurve, AngleEndCurve );                    // Butcher Speed TABLE Debug
        }

        if( cubeClear )
        if( !Debugging ) Item.SetAmt( ItemType.Res_Butcher_Coin, coin );                              // set coin as default cost Warning: Using set amount to avoid using coins gained ingame

        UpdateButcherCheckpoints();                                                                   // Updates checkpoint list for butchers Jumping
        G.HS.RandomButchersSpawned++;
    }
    private void RetrieveLevels()
    {
        if( Application.platform != RuntimePlatform.WindowsPlayer )
        if( UseItemLevels == false ) return;
        ButcherLevel = Item.GetNum( ItemType.Butcher_Level );
        ButcherBonusCoinsLevel = Item.GetNum( ItemType.Butcher_Coin_Level );
        ButcherBonusQualityLevel = Item.GetNum( ItemType.Butcher_Bonus_Quality_Level );
        ButcherSlotsLevel = Item.GetNum( ItemType.Butcher_Slots_Level );
        ButcherPowerLevel = Item.GetNum( ItemType.Butcher_Power_Level );
        CurrentCube = G.HS.Number;
        TotalCubes = Map.I.RM.RMD.MaxCubes;
    }

    private void AddItem( Unit un, int sid, bool special )
    {
        AltarBonusStruct bn = new AltarBonusStruct();

        List<float> ChanceList = GetWeights( ( int ) ButcherBonusQualityLevel, BonusQualityTable );         // Sorts Bonus type
        int qid = Util.Sort( ChanceList );

        ItemType type = BonusQualityTable.BonusList[ qid ];

        //Debug.Log( "  Bonus: " + type );                                                          //  uncheck to see amount of different intems created  

        float baseMinChance = 0;
        float extraMinChance = 0;
        float baseMaxChance = 50;
        float extraMaxChance = 0;

        float minchc = Random.Range( baseMinChance, baseMinChance + extraMinChance );               // sort min chance
        float maxchc = Random.Range( baseMaxChance, baseMaxChance + extraMaxChance );               // sort max chance

        float angle = Util.GetCurveVal( ButcherLevel, 100, BaseButcherAngle,
        FinalButcherAngle, AngleCurve, AngleEndCurve );

        bn.AltarBonusType = EAltarBonusType.Prize;
        bn.AltarBonusItem = type;
        bn.AltarBonusFactor = 1;
        bn.WorkingAngle = angle;
        bn.MinChance = minchc;
        bn.MaxChance = maxchc;
        bn.Activated = false;

        if( special )                                                                                 // SPECIAL passive bonus creation
        {
            CreateSpecialBonus( bn );
        }
        else
        {
            if( CheapItemList.Contains( bn.AltarBonusItem ) )
                CheapList.Add( sid );                                                                 // add cheap slot for potential overwrite
            bn.Activated = true;
        }

        //Debug.Log( " Slot: " + sid + "  Bonus: " + bn.AltarBonusItem + 
        //"  special: " + special + " passive: " + bn.AltarBonusType );
        SlotsCreated.Add( sid );    
        // store created slot
        if( !Debugging ) un.Altar.AltarBonusList[ sid ].Copy( bn );                                   // copy bonus data
    }

    private void CreateSpecialBonus( AltarBonusStruct bn )
    {
        List<float> ls = new List<float>();
        for( int i = 0; i < PassiveBonusList.Count; i++ )
        {
            PassiveBonus tpb = PassiveBonusList[ i ];
            float vl = Util.GetCurveVal( ButcherPowerLevel, 100, tpb.MinWeight, tpb.MaxWeight, 1 );   // Sorts a passive bonus from list based on min max weight
            ls.Add( vl );
        }
        int id = Util.Sort( ls );
        PassiveBonus pb = PassiveBonusList[ id ];
        bn.AltarBonusType = pb.Type;
        bn.AltarBonusItem = ItemType.NONE;
        bn.Scope = EAltarBonusScope.Cube;
        float pow = Util.GetCurveVal( ButcherPowerLevel, 100, pb.MinFactor, pb.MaxFactor, 1 );         // Sorts a passive bonus from list based on min max weight
        bn.AltarBonusFactor = pow;
        if( pb.Type == EAltarBonusType.Hang_Bonus )
        {
            bn.AltarBonusItem = ItemType.Res_Butcher_Coin;                                             // special cases that require bonus
        }
        if( pb.Type == EAltarBonusType.Hang_Bonus ||
            pb.Type == EAltarBonusType.Rotate_CCW || 
            pb.Type == EAltarBonusType.Rotate_CW  ||
            pb.Type == EAltarBonusType.Invert_Bonus ||
            pb.Type == EAltarBonusType.Blade )
        {
            bn.Activated = true;                                                                       // special cases that start activated
        }
    }

    private void UpdateButcherCheckpoints()
    {
        G.HS.CheckpointList = new List<Vector2>();
        for( int yy = ( int ) G.HS.Area.yMin - 1; yy < G.HS.Area.yMax + 1; yy++ )
        for( int xx = ( int ) G.HS.Area.xMin - 1; xx < G.HS.Area.xMax + 1; xx++ )
        if ( Map.PtOnMap( Map.I.Tilemap, new Vector2( xx, yy ) ) )
        {
            Unit but = Map.I.GetUnit( ETileType.ALTAR, new Vector2( xx, yy ) );                       // butcher List for jumping
            if( but && but.Altar.RandomAltar )
            {
                for( int d = 0; d < 8; d++ )
                {
                    Vector2 tg = new Vector2( xx, yy ) + Manager.I.U.DirCord[ ( int ) d ];
                    if( Map.I.IsTileOnlyGrass( tg ) )
                    {
                        G.HS.CheckpointList.Add( tg ); break;
                    }
                }
            }
        }
    }


    private void DebugTable( int times, int max, float startval, float finalval, float curve = 1, float aftermult = 0 )
    {
        for( int i = 1; i <= times; i++ )
        {
            float vl = Util.GetCurveVal( i, max, startval, finalval, curve, aftermult );
            Debug.Log( "Lev " + i + ": " + vl.ToString( "F2" ) );
        }
    }

    public static Unit SpawnButcher()
    {
        List<Vector2> ls = new List<Vector2>();
        for( int y = ( int ) G.HS.Area.yMin - 1; y < G.HS.Area.yMax + 1; y++ )
        for( int x = ( int ) G.HS.Area.xMin - 1; x < G.HS.Area.xMax + 1; x++ )                      // make a list of altars
        {
            if( Map.I.GetUnit( ETileType.ALTAR, new Vector2( x, y ) ) )
                ls.Add( new Vector2( x, y ) );
        }
        List<Vector2> pl = new List<Vector2>();

        for( int y = ( int ) G.HS.Area.yMin - 1; y < G.HS.Area.yMax + 1; y++ )
        for( int x = ( int ) G.HS.Area.xMin - 1; x < G.HS.Area.xMax + 1; x++ )                     // find good positions inside range
            {
                Vector2 pos = new Vector2( x, y );
                bool valid = false;
                if( ls.Count >= 1 )
                {
                    bool muitoPerto = false;
                    bool dentroMax = false;
                    foreach( Vector2 alt in ls )
                    {
                        int d = Util.Manhattan( alt, pos );
                        if( d < 3 )
                        {
                            muitoPerto = true; break;                                                // já está perto demais, não pode                            
                        }
                        if( d <= 6 )
                        {
                            dentroMax = true;                                                        // existe pelo menos um dentro da distância máxima
                        }
                    }

                    valid = ( !muitoPerto && dentroMax );
                }
                else
                    valid = true;                                                                    // se não tem altar, qualquer posição é válida

                if( valid )
                if( Map.I.IsTileOnlyGrass( pos ) )
                if( Map.GFU( ETileType.MINE, pos ) == null )
                if( G.Hero.Pos != pos )
                    pl.Add( pos );                                                                   // position ok
            }


        if( pl.Count < 1 ) return null;
        int id = Random.Range( 0, pl.Count );
        Unit prefab = Map.I.GetUnitPrefab( ETileType.ALTAR );
        GameObject go = Map.I.CreateUnit( prefab, pl[ id ] );                                        // Spawns new Butcher
        Unit un = go.GetComponent<Unit>();
        Map.I.DefaultButcherMod.ApplyMod( ( int ) pl[ id ].x, ( int ) pl[ id ].y, 0, un );           // Apply default butcher mod
        Map.I.RM.ModedUnitlList.Remove( un );
        un.Altar.RandomAltar = true;
        Map.I.LineEffect( G.Hero.Pos, pl[ id ], 10f, .5f, Color.blue, Color.blue );                  // Line FX
        Message.GreenMessage( "Butcher Created!" );
        for( int d = 0; d < 8; d++ )
        {
            Vector2 tg = un.Pos + Manager.I.U.DirCord[ ( int ) d ];
            Unit ga = Map.I.GetUnit( tg, ELayerType.GAIA );
            if( ga && ( ga.TileID == ETileType.SNOW || ga.TileID == ETileType.SAND ) )
                TKUtil.ClearLayer( tg, ELayerType.GAIA );                                   // clears ground around it from snow or sand
        }
        return un;
    }
    internal static void UpdateEvolution( string type )                                                                                        
    {
        float curve = 1;
        int maxCubes = Map.I.RM.RMD.MaxCubes;
        float fact = 0f;        
        if( maxCubes < 50 )                                                                          // a cada 10 cubos abaixo de 50 adiciona .1 ao fator multiplicativo para
        {
            fact = ( 50 - maxCubes ) * 0.01f;                                                   
        }

        float totalBonus = Map.I.RM.RMD.MaxCubes * ( 1 + fact );                                     // as default gives 1 bonus per cube x fact
        if( type == "chest" )
        {
            totalBonus = ( Map.I.RM.RMD.MaxCubes / 2f ) * ( 1f + fact );                             // for chests gives half
            curve = .8f;
        }

        totalBonus = Util.Percent( Map.I.RM.RMD.AltarEvolutionFactor, totalBonus );                  // applies factor %
                                                  
        float totalWeight = 0f;                                                                      // Primeiro, calcula o peso total de todos os cubos
        for( int i = 1; i <= maxCubes; i++ )
            totalWeight += Util.GetCurveVal( i, maxCubes, 0f, 1f, curve );

        float currentWeight = Util.GetCurveVal( G.HS.Number, maxCubes, 0f, 1f, curve );              // Agora, pega o peso do cubo atual e converte em bônus proporcional
        float bonus = ( currentWeight / totalWeight ) * totalBonus;                                  // Finally, calculates bonus
        float orig = bonus;
        bonus = Util.FloatSort( bonus );                                                             // float sort
        Debug.Log( "Cube " + G.HS.Number + " Bonus Altar Evolution: " + 
        orig.ToString( "F2" ) + " Float Sort: " + bonus );

        SortBonus( bonus, true );                                                                    // Sorts and give the bonus
    }
    public static void SortBonus( float bonus, bool msg )
    {
        int bntype = Random.Range( 0, 5 );                                                           // sort bonus type
        ItemType it = ItemType.NONE;
        switch( bntype )
        {
            case 0: it = ItemType.Butcher_Coin_Level; break;
            case 1: it = ItemType.Butcher_Bonus_Quality_Level; break;
            case 2: it = ItemType.Butcher_Slots_Level; break;
            case 3: it = ItemType.Butcher_Level; break;
            case 4: it = ItemType.Butcher_Power_Level; break;
        }
        Item.IgnoreMessage = msg;
        Item.AddItem( it, bonus );                                                                   // Gives the bonus
    }
    public void UpdateBumpFailed( Unit au, AltarBonusStruct bn, int ID )
    {
        if( au.Altar.RandomAltar == false ) return;
        if( bn.AltarBonusType != EAltarBonusType.Prize )
        {
            if( SortPassiveReset() )
                bn.Reset();
        }

        float vl = 0.5f + Util.GetCurveVal( ButcherPowerLevel, 100, 0f, 2.5f, 1 );                                  // Bonus coin per Butcher power level
        vl = Util.FloatSort( vl );
        Item.AddItem( ItemType.Blue_Coin, vl );
    }
    public bool SortPassiveReset()
    {
        return true;
        float chc = Util.GetCurveVal( ButcherSlotsLevel, 100, 0, 50, 1, 0 );                                      // Passive reset sort -------- curve Rethink later
        return Util.Chance( chc );
    }
    public void UpdateBumpSuccessful( Unit au, AltarBonusStruct bn )
    {
        if( au.Altar.RandomAltar == false ) return;
        if( bn.AltarBonusType == EAltarBonusType.Prize )                                                         // Reset bonus after success. consider a chance for remaining in the future
        {
            bn.Reset();
        }

        if( bn.AltarBonusType == EAltarBonusType.Spawn_Random_Altar )                                            // Spawn butcher power
        {
            for( int i = 0; i < bn.AltarBonusFactor; i++ )
                CreateRandomButcher( false );
            if( SortPassiveReset() )
                bn.Reset();
        }

        float vl = 0.5f + Util.GetCurveVal( ButcherPowerLevel, 100, 0f, 2.5f, 1 );                                  // Bonus coin per Butcher power level
        vl *= 3;                                                                                                 // Extra bonus for success
        vl = Util.FloatSort( vl );
        Item.AddItem( ItemType.Blue_Coin, vl );
    }

    internal void UpdateButcherSpawning()
    {
        if( G.HS.RandomButchersSpawned > 0 ) return;                                                             // Butcher already spawned. to avoid load + respawn

        RetrieveLevels();                                                                                       // Retrieve levels from item levels

        float curve = Util.GetCurveVal( CurrentCube, TotalCubes, 
        FirstCubeSpawnChance, LastCubeSpawnChance, SpawnChanceCurve );                                          // Initial Spawn chance by cube importance

        float chc = AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.SPAWN_BUTCHER_CHANCE );                 // upgrade Butcher spawning Tech

        float slot = Util.GetCurveVal( ButcherSlotsLevel, 50, 0f, 5, 1 );                                       // Butcher Slot Level                           ---- Rethink Later

        float chance = curve + chc + slot;                                                                      // apply final calculation 
        if( Util.Chance( chance ) )
        {
            CreateRandomButcher();                                                                              // Success!
        }
        Debug.Log( "NEO is a GREAT SUCCESS!   Curve: " + curve +
        "% Chance Tech: " + chc + "% Slot Level: " + slot + "% Total: " + chance + "%" );
    }
}

