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

public class Language : MonoBehaviour
{
    [Tooltip( "Lista de URLs CSV do Google Sheets (uma por aba publicada)" )]
    public List<string> csvUrls = new List<string>();

    [Tooltip( "Nome de cada aba correspondente à URL acima" )]
    public List<string> TabNames = new List<string>();

    [Tooltip( "Código do idioma que deseja carregar, ex: EN, PT, ES" )]
    public string languageCode = "EN";

    public Dictionary<string, Dictionary<string, string>> localizedSheets = new Dictionary<string, Dictionary<string, string>>();

    public static Language I;

    private static string SavePath { get { return Application.dataPath + "/Resources/Language/LanguageCache.json"; } }

    void Awake()
    {
        I = this;

#if UNITY_EDITOR
        if( !Application.isPlaying )
            return;
#endif

        LoadFromJson();
    }
#if UNITY_EDITOR
    [MenuItem( "Tools/Update Language" )]
    private static void UpdateLanguageCacheMenu()
    {
        Language lang = GameObject.FindObjectOfType<Language>();
        if( lang == null )
        {
            Debug.LogError( "Não encontrou objeto com o script Language na cena." );
            return;
        }
        lang.UpdateLanguage();
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
            // Editor: download síncrono e salva no Assets/Resources
            DownloadAllCSVsEditorSync();
            SaveToJson(); // Salva no SavePath original
            Debug.Log( "Language cache salvo em: " + SavePath );
            return;
        }
#endif

        // PlayMode ou Build PC
        StartCoroutine( DownloadCSVsRuntimeForBuild() );
    }

    private IEnumerator DownloadCSVsRuntimeForBuild()
    {
        localizedSheets.Clear();

        // Caminho do JSON no build ou editor
        string folderPath = Path.Combine( Application.dataPath, "Resources/Language" );
        if( !Directory.Exists( folderPath ) )
            Directory.CreateDirectory( folderPath );

        string buildSavePath = Path.Combine( folderPath, "LanguageCache.json" );

        // 🗑 Apaga cache antigo antes de baixar
        if( File.Exists( buildSavePath ) )
        {
            File.Delete( buildSavePath );
            Debug.Log( "Cache antigo apagado: " + buildSavePath );
        }

        if( csvUrls.Count != TabNames.Count )
        {
            Debug.LogError( "A quantidade de URLs e nomes de abas é diferente. Corrija no inspector." );
            yield break;
        }

        int loadedCount = 0;

        for( int i = 0; i < csvUrls.Count; i++ )
        {
            string url = csvUrls[ i ];
            string abaNome = TabNames[ i ].Trim();

            WWW www = new WWW( url );
            while( !www.isDone )
                yield return null;

            if( !string.IsNullOrEmpty( www.error ) )
            {
                Debug.LogError( "Erro ao baixar CSV da aba '" + abaNome + "': " + www.error );
                continue;
            }

            string text = System.Text.Encoding.UTF8.GetString( www.bytes );

            if( text.Length > 0 && text[ 0 ] == '\uFEFF' )
                text = text.Substring( 1 );

            if( string.IsNullOrEmpty( text ) )
            {
                Debug.LogWarning( "CSV vazio para a aba '" + abaNome + "'." );
                continue;
            }

            ParseCSV( text, abaNome );
            loadedCount++;
            Debug.Log( "Aba carregada (PlayMode / PC Build): " + abaNome );
        }

        Debug.Log( "PlayMode / PC Build: " + loadedCount + "/" + csvUrls.Count + " abas carregadas com sucesso." );

        // Salva o JSON atualizado
        var wrapper = new LanguageWrapper( localizedSheets );
        File.WriteAllText( buildSavePath, JsonUtility.ToJson( wrapper, true ) );
        Debug.Log( "Language cache salvo em: " + buildSavePath );
        string editorFile = "C:/Users/alien/Desktop/NEO/Assets/Resources/Language/LanguageCache.json"; 
        File.Copy( buildSavePath, editorFile, true );                                                                 // copy to editor, too
        LoadFromJson();
        Debug.Log( "Language cache atualizado e carregado." );
    }
       
    public void DownloadAllCSVsEditorSync()
    {
        localizedSheets.Clear();

        if( csvUrls.Count != TabNames.Count )
        {
            Debug.LogError( "A quantidade de URLs e nomes de abas é diferente. Corrija no inspector." );
            return;
        }

        int loadedCount = 0;

        for( int i = 0; i < csvUrls.Count; i++ )
        {
            string url = csvUrls[ i ];
            string abaNome = TabNames[ i ].Trim();

            WWW www = new WWW( url );
            while( !www.isDone ) { }

            if( !string.IsNullOrEmpty( www.error ) )
            {
                Debug.LogError( "Erro ao baixar CSV da aba '" + abaNome + "': " + www.error );
                continue;
            }

            // converte bytes para UTF-8, ignorando Content-Type
            string text = System.Text.Encoding.UTF8.GetString( www.bytes );

            // remove BOM se existir
            if( text.Length > 0 && text[ 0 ] == '\uFEFF' )
                text = text.Substring( 1 );

            if( string.IsNullOrEmpty( text ) )
            {
                Debug.LogWarning( "CSV vazio para a aba '" + abaNome + "'." );
                continue;
            }

            ParseCSV( text, abaNome );
            loadedCount++;
            Debug.Log( "Aba carregada (Editor): " + abaNome );
        }

        Debug.Log( "Editor: " + loadedCount + "/" + csvUrls.Count + " abas carregadas com sucesso." );
        SaveToJson();
        LoadFromJson();
    }

    private IEnumerator DownloadCSVsRuntime()
    {
        localizedSheets.Clear();

        if( csvUrls.Count != TabNames.Count )
        {
            Debug.LogError( "A quantidade de URLs e nomes de abas é diferente. Corrija no inspector." );
            yield break;
        }

        int loadedCount = 0;

        for( int i = 0; i < csvUrls.Count; i++ )
        {
            string url = csvUrls[ i ];
            string abaNome = TabNames[ i ].Trim();

            WWW www = new WWW( url );
            yield return www;

            if( !string.IsNullOrEmpty( www.error ) )
            {
                Debug.LogError( "Erro ao baixar CSV da aba '" + abaNome + "' no PlayMode: " + www.error );
                continue;
            }

            // converte bytes para UTF-8, ignorando Content-Type
            string text = System.Text.Encoding.UTF8.GetString( www.bytes );

            // remove BOM se existir
            if( text.Length > 0 && text[ 0 ] == '\uFEFF' )
                text = text.Substring( 1 );

            if( string.IsNullOrEmpty( text ) )
            {
                Debug.LogWarning( "CSV vazio para a aba '" + abaNome + "' no PlayMode." );
                continue;
            }

            ParseCSV( text, abaNome );
            loadedCount++;
            Debug.Log( "Aba carregada (PlayMode): " + abaNome );
        }

        Debug.Log( "PlayMode: " + loadedCount + "/" + csvUrls.Count + " abas carregadas com sucesso." );
        SaveToJson();
        LoadFromJson();

        Debug.Log( "Language cache atualizado e salvo no PlayMode." );
    }

    void LoadFromJson()
    {
        if( !File.Exists( SavePath ) )
        {
            Debug.LogError( "Arquivo de cache não encontrado: " + SavePath );
            return;
        }

        string json = File.ReadAllText( SavePath );
        var wrapper = JsonUtility.FromJson<LanguageWrapper>( json );
        localizedSheets = wrapper.ToDictionary();
        //Debug.Log( "Language cache carregado com " + localizedSheets.Count + " abas." );
    }

    void SaveToJson()
    {
        var wrapper = new LanguageWrapper( localizedSheets );
        string json = JsonUtility.ToJson( wrapper, true );
        File.WriteAllText( SavePath, json );
    }

    void ParseCSV( string csvText, string abaKey )
    {
        List<string> linesList = ReadCSVLines( csvText );
        string[] lines = linesList.ToArray();

        if( lines.Length < 2 )
            return;

        string[] headers = ParseCSVLine( lines[ 0 ] );
        int langIndex = Array.IndexOf( headers, languageCode );
        if( langIndex == -1 )
        {
            Debug.LogError( "Idioma não encontrado: " + languageCode );
            return;
        }

        Dictionary<string, string> dict = new Dictionary<string, string>();

        for( int i = 1; i < lines.Length; i++ )
        {
            string[] cols = ParseCSVLine( lines[ i ] );
            if( cols.Length > langIndex )
            {
                string key = cols[ 0 ].Trim();
                string value = cols[ langIndex ].Trim().Replace( "\\n", "\n" );

                if( !dict.ContainsKey( key ) )
                    dict.Add( key, value );
                else
                    Debug.LogWarning( "Chave duplicada na aba '" + abaKey + "': " + key );
            }
        }

        localizedSheets[ abaKey ] = dict;
    }

    List<string> ReadCSVLines( string csvText )
    {
        List<string> lines = new List<string>();
        bool inQuotes = false;
        int start = 0;

        for( int i = 0; i < csvText.Length; i++ )
        {
            char c = csvText[ i ];
            if( c == '"' )
            {
                if( i + 1 < csvText.Length && csvText[ i + 1 ] == '"' )
                {
                    i++; // pula aspas dupla
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if( c == '\n' && !inQuotes )
            {
                string line = csvText.Substring( start, i - start );
                lines.Add( line.Trim( '\r' ) );
                start = i + 1;
            }
        }
        if( start < csvText.Length )
        {
            string line = csvText.Substring( start );
            lines.Add( line.Trim( '\r' ) );
        }
        return lines;
    }

    string[] ParseCSVLine( string line )
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string current = "";

        for( int i = 0; i < line.Length; i++ )
        {
            char c = line[ i ];

            if( c == '"' )
            {
                if( inQuotes && i + 1 < line.Length && line[ i + 1 ] == '"' )
                {
                    current += '"';
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if( c == ',' && !inQuotes )
            {
                result.Add( current );
                current = "";
            }
            else
            {
                current += c;
            }
        }

        result.Add( current );
        return result.ToArray();
    }

    public static string Get( string key, string aba = "" )
    {
        if( I == null || I.localizedSheets == null )
            return key;

        if( !string.IsNullOrEmpty( aba ) )
        {
            Dictionary<string, string> sheet;
            if( I.localizedSheets.TryGetValue( aba, out sheet ) && sheet != null )
            {
                string value;
                if( sheet.TryGetValue( key, out value ) )
                    return value;
            }
            return "## Invalid Key! ##\n##" + key + "##"; // fallback se não achar a chave na aba ou aba null
        }

        foreach( var sheet in I.localizedSheets.Values )
        {
            if( sheet == null )
                continue;

            string value;
            if( sheet.TryGetValue( key, out value ) )
                return value;
        }

        return key;
    }

    [Serializable]
    class LanguageWrapper
    {
        public List<SheetEntry> sheets = new List<SheetEntry>();

        public LanguageWrapper() { sheets = new List<SheetEntry>(); }

        public LanguageWrapper( Dictionary<string, Dictionary<string, string>> dict )
        {
            sheets = new List<SheetEntry>();
            foreach( var kv in dict )
                sheets.Add( new SheetEntry( kv.Key, kv.Value ) );
        }

        public Dictionary<string, Dictionary<string, string>> ToDictionary()
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            foreach( var sheet in sheets )
                result[ sheet.name ] = sheet.ToDictionary();
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
            entriesList = new List<Entry>();
            foreach( var kv in dict )
                entriesList.Add( new Entry() { key = kv.Key, value = kv.Value } );
        }

        public Dictionary<string, string> ToDictionary()
        {
            var dict = new Dictionary<string, string>();
            foreach( var e in entriesList )
                dict[ e.key ] = e.value;
            return dict;
        }
    }

    [Serializable]
    class Entry
    {
        public string key;
        public string value;
    }

    internal static void SwitchLanguage( string newLangCode )
    {
        //Debug.Log( "SwitchLanguage ainda não implementado: " + newLangCode );
    }
}
