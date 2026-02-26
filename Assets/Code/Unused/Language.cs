using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector;
#endif

public class Language: MonoBehaviour
{
    [Tooltip("List of Google Sheets CSV URLs (one per published tab)")]
    public List<string> csvUrls = new List<string>();

    [Tooltip("Name of each tab corresponding to the URL above")]
    public List<string> TabNames = new List<string>();

    [Tooltip("Language code to load, e.g., EN, PT, ES")]
    public string languageCode = "EN";

    public Dictionary<string, Dictionary<string, string>> localizedSheets = new Dictionary<string, Dictionary<string, string>>();

    public static Language I;

    private static string SavePath { get { return Application.dataPath + "/Resources/Language/LanguageCache.json"; } }

    void Awake()
    {
        I = this; // Sets the singleton instance

#if UNITY_EDITOR
        if( !Application.isPlaying )
            return;
#endif

        LoadFromJson(); // Loads data on awake
    }

#if UNITY_EDITOR
    [MenuItem( "Tools/Update Language" )]
    private static void UpdateLanguageCacheMenu()
    {
        Language lang = GameObject.FindObjectOfType<Language>(); // Finds the Language object in scene
        if( lang == null )
        {
            Debug.LogError( "Could not find object with the Language script in the scene." ); // Error if script is missing
            return;
        }
        lang.UpdateLanguage(); // Triggers the update
    }
#endif

#if UNITY_EDITOR
    [Button( "Update Language", ButtonSizes.Gigantic ), GUIColor( 0, 0.7f, 1f )]
#endif
    public void UpdateLanguage()
    {
#if UNITY_EDITOR
        if( !Application.isPlaying )
        {
            DownloadAllCSVsEditorSync(); // Editor: synchronous download
            SaveToJson();
            Debug.Log( "Language cache saved at: " + SavePath ); // Logs save path
            return;
        }
#endif
        StartCoroutine( DownloadCSVsRuntimeForBuild() ); // Runtime: asynchronous download
    }

    private IEnumerator DownloadCSVsRuntimeForBuild()
    {
        localizedSheets.Clear(); // Clears current data

        string folderPath = Path.Combine(Application.dataPath, "Resources/Language");
        if( !Directory.Exists( folderPath ) )
            Directory.CreateDirectory( folderPath ); // Ensures directory exists

        string buildSavePath = Path.Combine(folderPath, "LanguageCache.json");

        if( File.Exists( buildSavePath ) )
        {
            File.Delete( buildSavePath ); // Deletes old cache
            Debug.Log( "Old cache deleted: " + buildSavePath ); // Logs deletion
        }

        if( csvUrls.Count != TabNames.Count )
        {
            Debug.LogError( "URLs and TabNames count mismatch." ); // Error on count mismatch
            yield break;
        }

        int loadedCount = 0;
        for( int i = 0; i < csvUrls.Count; i++ )
        {
            string url = csvUrls[i];
            string abaNome = TabNames[i].Trim();

            using( UnityWebRequest www = UnityWebRequest.Get( url ) )
            {
                yield return www.SendWebRequest(); // Downloads the CSV

                if( www.result != UnityWebRequest.Result.Success )
                {
                    Debug.LogError( "Error downloading CSV '" + abaNome + "': " + www.error ); // Logs web error
                    continue;
                }

                string text = www.downloadHandler.text; // Gets text from download

                if( text.Length > 0 && text[ 0 ] == '\uFEFF' )
                    text = text.Substring( 1 ); // Removes BOM character

                ParseCSV( text, abaNome ); // Parses data
                loadedCount++;
                Debug.Log( "Tab loaded (Runtime): " + abaNome ); // Logs success
            }
        }

        var wrapper = new LanguageWrapper(localizedSheets);
        File.WriteAllText( buildSavePath, JsonUtility.ToJson( wrapper, true ) ); // Saves JSON

        string editorFile = "C:/Users/alien/Desktop/NEO/Assets/Resources/Language/LanguageCache.json";
        if( Directory.Exists( Path.GetDirectoryName( editorFile ) ) )
            File.Copy( buildSavePath, editorFile, true ); // Copies to absolute editor path

        LoadFromJson(); // Reloads data
    }

    public void DownloadAllCSVsEditorSync()
    {
        localizedSheets.Clear(); // Clears data

        if( csvUrls.Count != TabNames.Count )
        {
            Debug.LogError( "URLs and TabNames count mismatch." ); // Error on count mismatch
            return;
        }

        int loadedCount = 0;
        for( int i = 0; i < csvUrls.Count; i++ )
        {
            string url = csvUrls[i];
            string abaNome = TabNames[i].Trim();

            UnityWebRequest www = UnityWebRequest.Get(url);
            www.SendWebRequest(); // Starts request

            while( !www.isDone ) { } // Synchronous wait for editor

            if( www.result != UnityWebRequest.Result.Success )
            {
                Debug.LogError( "Error downloading CSV '" + abaNome + "': " + www.error ); // Logs error
                continue;
            }

            string text = www.downloadHandler.text;
            if( text.Length > 0 && text[ 0 ] == '\uFEFF' ) text = text.Substring( 1 ); // Removes BOM

            ParseCSV( text, abaNome );
            loadedCount++;
            Debug.Log( "Tab loaded (Editor): " + abaNome ); // Logs editor load
            www.Dispose();
        }

        SaveToJson(); // Saves data
        LoadFromJson(); // Reloads data
    }

    void LoadFromJson()
    {
        if( !File.Exists( SavePath ) )
        {
            Debug.LogError( "Cache file not found: " + SavePath ); // Logs missing file
            return;
        }

        string json = File.ReadAllText(SavePath);
        var wrapper = JsonUtility.FromJson<LanguageWrapper>(json); // Deserializes
        localizedSheets = wrapper.ToDictionary(); // Converts to dictionary
    }

    void SaveToJson()
    {
        var wrapper = new LanguageWrapper(localizedSheets);
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText( SavePath, json ); // Writes JSON to disk
    }

    void ParseCSV( string csvText, string abaKey )
    {
        List<string> linesList = ReadCSVLines(csvText); // Reads CSV lines
        string[] lines = linesList.ToArray();

        if( lines.Length < 2 ) return;

        string[] headers = ParseCSVLine(lines[0]); // Parses headers
        int langIndex = Array.IndexOf(headers, languageCode);
        if( langIndex == -1 )
        {
            Debug.LogError( "Language not found: " + languageCode ); // Error if language is missing
            return;
        }

        Dictionary<string, string> dict = new Dictionary<string, string>();
        for( int i = 1; i < lines.Length; i++ )
        {
            string[] cols = ParseCSVLine(lines[i]); // Parses columns
            if( cols.Length > langIndex )
            {
                string key = cols[0].Trim();
                string value = cols[langIndex].Trim().Replace("\\n", "\n"); // Formats new lines

                if( !dict.ContainsKey( key ) ) dict.Add( key, value ); // Adds unique keys
                else Debug.LogWarning( "Duplicate key in '" + abaKey + "': " + key ); // Warning on duplicates
            }
        }
        localizedSheets[ abaKey ] = dict; // Stores dictionary
    }

    List<string> ReadCSVLines( string csvText )
    {
        List<string> lines = new List<string>();
        bool inQuotes = false;
        int start = 0;

        for( int i = 0; i < csvText.Length; i++ )
        {
            char c = csvText[i];
            if( c == '"' )
            {
                if( i + 1 < csvText.Length && csvText[ i + 1 ] == '"' ) i++; // Skips escaped quotes
                else inQuotes = !inQuotes;
            }
            else if( c == '\n' && !inQuotes )
            {
                lines.Add( csvText.Substring( start, i - start ).Trim( '\r' ) ); // Adds line
                start = i + 1;
            }
        }
        if( start < csvText.Length ) lines.Add( csvText.Substring( start ).Trim( '\r' ) ); // Adds final line
        return lines;
    }

    string[ ] ParseCSVLine( string line )
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string current = "";

        for( int i = 0; i < line.Length; i++ )
        {
            char c = line[i];
            if( c == '"' )
            {
                if( inQuotes && i + 1 < line.Length && line[ i + 1 ] == '"' ) { current += '"'; i++; } // Escaped quote
                else inQuotes = !inQuotes;
            }
            else if( c == ',' && !inQuotes ) { result.Add( current ); current = ""; } // Column separator
            else current += c;
        }
        result.Add( current ); // Adds last column
        return result.ToArray();
    }

    public static string Get( string key, string aba = "" )
    {
        if( I == null || I.localizedSheets == null ) return key; // Returns key if not initialized

        if( !string.IsNullOrEmpty( aba ) )
        {
            Dictionary<string, string> sheet;
            if( I.localizedSheets.TryGetValue( aba, out sheet ) && sheet != null )
            {
                string value;
                if( sheet.TryGetValue( key, out value ) ) return value; // Returns sheet specific value
            }
            return "## Invalid Key! ##\n##" + key + "##"; // Key not found error
        }

        foreach( var sheet in I.localizedSheets.Values )
        {
            if( sheet == null ) continue;
            string value;
            if( sheet.TryGetValue( key, out value ) ) return value; // Searches all sheets
        }
        return key; // Fallback to key
    }

    [Serializable]
    class LanguageWrapper
    {
        public List<SheetEntry> sheets = new List<SheetEntry>();
        public LanguageWrapper() { }
        public LanguageWrapper( Dictionary<string, Dictionary<string, string>> dict )
        {
            foreach( var kv in dict ) sheets.Add( new SheetEntry( kv.Key, kv.Value ) ); // Wraps dictionary
        }
        public Dictionary<string, Dictionary<string, string>> ToDictionary()
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            foreach( var sheet in sheets ) result[ sheet.name ] = sheet.ToDictionary(); // Unwraps to dictionary
            return result;
        }
    }

    [Serializable]
    class SheetEntry
    {
        public string name;
        public List<Entry> entriesList = new List<Entry>();
        public SheetEntry() { }
        public SheetEntry( string name, Dictionary<string, string> dict )
        {
            this.name = name;
            foreach( var kv in dict ) entriesList.Add( new Entry() { key = kv.Key, value = kv.Value } ); // Wraps entries
        }
        public Dictionary<string, string> ToDictionary()
        {
            var dict = new Dictionary<string, string>();
            foreach( var e in entriesList ) dict[ e.key ] = e.value; // Unwraps entries
            return dict;
        }
    }

    [Serializable] class Entry { public string key; public string value; }

    internal static void SwitchLanguage( string newLangCode ) { } // Placeholder for language switching
}