using UnityEngine;
using System.Collections;

public class SectorHint : MonoBehaviour {

    public enum ESectorHintOperation
    {
        NONE, SUMMA, ADDUS, SUBTRA, NULLA, SORTA, CHAANCE
    }

    public enum ESectorHintStrenght
    {
        NONE, NULLUS, TENTH, DOZEN, QUARTAR, MEDIUS, TREQUARTAR, TOTUM, UNNA, DUNNA, TRINNA, QUARTA
    }

    [Multiline]
    public string HintDescription;
    [Range( 0, 64 )]
    public int MinSectorNumber = 0;
    [Range( 0, 64 )]
    public int MaxSectorNumber = 64;
    [Range( 1, 200 )]
    public int Height = 100;
    public bool Absolute = false;
    public int UseCount;
    public ESectorHintOperation[] OperationList;
    public ESectorHintStrenght[] StrenghtList;
    public static Sector S;

    public void Reset()
    {
        UseCount = 0;
    }

    public static float GetStrenght( ESectorHintStrenght id )
    {
        float perc = 0;
        switch( id )
        {
            case ESectorHintStrenght.NULLUS: perc = 0; break;
            case ESectorHintStrenght.TENTH: perc = 10; break;
            case ESectorHintStrenght.DOZEN: perc = 12; break;
            case ESectorHintStrenght.QUARTAR: perc = 25; break;
            case ESectorHintStrenght.MEDIUS: perc = 50; break;
            case ESectorHintStrenght.TREQUARTAR: perc = 75; break;
            case ESectorHintStrenght.TOTUM: perc = 100; break;

            case ESectorHintStrenght.UNNA: perc = 1; break;
            case ESectorHintStrenght.DUNNA: perc = 2; break;
            case ESectorHintStrenght.TRINNA: perc = 3; break;
            case ESectorHintStrenght.QUARTA: perc = 4; break;
        }
        return perc;
    }

    public static float ApplyEffect( float val )
    {
        float perc = GetStrenght( S.HintStrenght );

        bool absolute = S.SectorHint.Absolute;

        if( S.HintOperation == ESectorHintOperation.ADDUS )
        {
            if( !absolute )
            val += Util.Percent( perc, val );
            else
                val +=  perc;
        }

        if( S.HintOperation == ESectorHintOperation.SUMMA )
        {
            if( !absolute )
                val += Util.Percent( perc, val );
            else
                val += perc;
        }

        if( S.HintOperation == ESectorHintOperation.SUBTRA )
        {
            if( !absolute )
                val -= Util.Percent( perc, val );
            else
                val -= perc;
        }

        if( S.HintOperation == ESectorHintOperation.NULLA )
        {
            if( !absolute )
                val -= Util.Percent( perc, val );
            else
                val -= perc;
        }

        if( S.HintOperation == ESectorHintOperation.SORTA )
        {
            if( S.HintSortSuccess ) val = 1; else val = 0;
        }

        if( S.HintOperation == ESectorHintOperation.CHAANCE )
        {
            if( S.HintSortSuccess ) val = 1; else val = 0;
        }

        return val;
    }

    public static float GetHintBonus( string hint, float val )
    {
        if( S )
        if( hint == S.HintTypeText )
        {
            val = SectorHint.ApplyEffect( val );
        }
        return val;
    }
    
    public static bool SortHintChance( string hint )
    {
        if( hint != "" )
            if( S.HintTypeText != hint ) return false;

        Vector2 pos = Map.I.Hero.Pos + new Vector2( Random.Range( -1, 1 ), Random.Range( -1, 1 ) );

        bool res = false;
        if( S.HintOperation == SectorHint.ESectorHintOperation.CHAANCE ||                                               // Sorts chance operator 
        S.HintOperation == SectorHint.ESectorHintOperation.SORTA )
        {
            float num = Random.Range( 0, 100 );
            float str = SectorHint.GetStrenght( S.HintStrenght );
            if( num < str )
            {
                res = true;
            }
        }
        return res;
    }
    public static void UpdateSectorHintColor( Sector s )
    {
        if( s != null && s.Type == Sector.ESectorType.NORMAL )                                                             // Choose Sector hint color quality
        if( s.HintText.gameObject.activeSelf )
        {
            if( s.SectorHintQuality == 1 )
            {
                s.SectorHintQuality = 2;
                s.HintText.color = Color.red;
            }
            else
                if( s.SectorHintQuality == 2 )
                {
                    s.SectorHintQuality = 0;
                    s.HintText.color = Color.green;
                }
                else
                    if( s.SectorHintQuality == 0 )
                    {
                        s.SectorHintQuality = 1;
                        s.HintText.color = Color.white;
                    }
        }
    }
    
    public static string GetHintInfo( Sector hs, bool showhead = true )
    {
        string oper = "";
        string res = "";
        bool showsort = false;
        if( hs.HintTypeText == "METAAGRESS"   ) showsort = true;
        if( hs.HintTypeText == "INSOMNIAC"    ) showsort = true;
        if( hs.HintTypeText == "HYPERSOMNIAC" ) showsort = true;
        if( hs.HintTypeText == "MOTUSAGILE"   ) showsort = true;
        if( hs.HintTypeText == "LIBERREDITUM" ) showsort = true;
        if( hs.HintTypeText == "FORTEMONSTAR" ) showsort = true;
        if( hs.HintTypeText == "WEEKAMONSTAR" ) showsort = true;        
        
        string head = Map.I.Hero.PrefabName + " Entered a new Cube: \n" + hs.AllHintText + "\n";
        if(!showhead ) head = "";

        if( hs.HintOperation == SectorHint.ESectorHintOperation.ADDUS ||
            hs.HintOperation == SectorHint.ESectorHintOperation.SUMMA )
        {
            oper = "Increased by";
        }

        if( hs.HintOperation == SectorHint.ESectorHintOperation.NULLA ||
            hs.HintOperation == SectorHint.ESectorHintOperation.SUBTRA )
        {
            oper = "Decreased by";
        }

        string percsignal = "%";
        if(!showsort ) percsignal = "";

        res = "" + head;
        res += "" + hs.HintDescription + " " + oper + ": " + SectorHint.GetStrenght( hs.HintStrenght ) + percsignal + "\n";

        if( hs.HintOperation == SectorHint.ESectorHintOperation.CHAANCE ||
            hs.HintOperation == SectorHint.ESectorHintOperation.SORTA )
        {
            if( showsort )
            if( hs.HintSortSuccess ) res += "Sort Successful!";
            else res += "Sort Failed!";
        }

        if( hs.MorphCount > 0 )
        {
            if( hs.HintTypeText == "ROACHILIA" )
                res += "Roach Morphs: " + hs.MorphCount;

            if( hs.HintTypeText == "SCARABIA" )
                res += "Scarab Morphs: " + hs.MorphCount;
        }

        if( hs.HintTypeText == "LIBERREDITUM" )
        {
            res += "\nRemaining: " + hs.NumFreeReentrances;
        }

        return res;
    }

    public static void UpdateSectorEntranceMessage()
    {
        return;
        if( Map.I.RM.HeroSector.Type == Sector.ESectorType.GATES ) return;
        Color col = Color.green;        

        if( Map.I.RM.HeroSector == null || Map.I.RM.HeroSector.Type == Sector.ESectorType.LAB )
        {
            Message.CreateMessage( ETileType.NONE, "Entering Main Base...", Map.I.Hero.Pos, col, false, false, 7 );
            return;
        }

        if( S == null ) return;

        if( S.HintSortSuccess == false )
        {
            if( S.HintTypeText == "METAAGRESS"   ) col = Color.red;
            if( S.HintTypeText == "INSOMNIAC"    ) col = Color.red;
            if( S.HintTypeText == "HYPERSOMNIAC" ) col = Color.red;
            if( S.HintTypeText == "BARRICUSS"    ) col = Color.red;
            if( S.HintTypeText == "BARRACASS"    ) col = Color.red;
            if( S.HintTypeText == "MOTUSAGILE"   ) col = Color.red;
            if( S.HintTypeText == "LIBERREDITUM" ) col = Color.red;
            if( S.HintTypeText == "WEEKAMONSTAR" ) col = Color.red;
            if( S.HintTypeText == "FORTEMONSTAR" ) col = Color.red;    
        }

        int id = Map.I.RM.HeroSector.Number - 1;
        string cn = UI.I.OrdinalNumberList[ id ] + " Cube.\n";
        string txt = "Entering " + cn + SectorHint.GetHintInfo( S, false );
        Message.CreateMessage( ETileType.NONE, txt, Map.I.Hero.Pos, col, false, false, 7 );
        Map.I.SectorStats.Reset();
    }
}