using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
public class MasterAudioClipManager: EditorWindow
{
    private const string NoClipsSelected = "There are no clips selected.";
    private const string CacheFilePath = "Assets/DarkTonic/MasterAudio/audioImportSettings.xml";
    private const string AllFoldersKey = "[All]";
    private const int MaxPageSize = 200;

    private readonly AudioInfoData _clipList = new AudioInfoData();

    private int _bulkBitrate = 156000;
    private bool _bulkForceMono;
    private AudioCompressionFormat _bulkFormat = AudioCompressionFormat.Vorbis;
    private AudioClipLoadType _bulkLoadType = AudioClipLoadType.CompressedInMemory;
    private int _pageNumber;

    private List<AudioInformation> _filterClips;
    private List<AudioInformation> _filteredOut;
    private Vector2 _scrollPos;
    private Vector2 _outsideScrollPos;
    private readonly List<string> _folderPaths = new List<string>();
    private string _selectedFolderPath = AllFoldersKey;

    [MenuItem( "Window/Master Audio/Master Audio Clip Manager" )]
    static void Init()
    {
        GetWindow( typeof( MasterAudioClipManager ) );
    }

    void OnGUI()
    {
        _outsideScrollPos = GUI.BeginScrollView( new Rect( 0, 0, position.width, position.height ), _outsideScrollPos, new Rect( 0, 0, 900, 666 ) );

        EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );

        GUI.contentColor = Color.white;
        if( GUILayout.Button( new GUIContent( "Scan Project" ), EditorStyles.toolbarButton, GUILayout.Width( 100 ) ) )
        {
            BuildCache();
            return;
        }

        GUILayout.Space( 10 );
        if( GUILayout.Button( new GUIContent( "Revert Selected" ), EditorStyles.toolbarButton, GUILayout.Width( 100 ) ) )
        {
            RevertSelected();
            return;
        }

        GUILayout.Space( 10 );
        if( GUILayout.Button( new GUIContent( "Apply Selected" ), EditorStyles.toolbarButton, GUILayout.Width( 100 ) ) )
        {
            ApplySelected();
            return;
        }

        GUILayout.Space( 10 );

        GUILayout.Label( "Full Path Filter" );
        var oldFilter = _clipList.SearchFilter;
        var newFilter = GUILayout.TextField(_clipList.SearchFilter, EditorStyles.toolbarTextField, GUILayout.Width(200));
        if( newFilter != oldFilter )
        {
            _clipList.SearchFilter = newFilter;
            RebuildFilteredList();
        }

        //gg mudei estilo abaixo
        var myPosition = GUILayoutUtility.GetRect(10, 10, EditorStyles.miniButtonRight);
        myPosition.x -= 5;
        if( GUI.Button( myPosition, "", EditorStyles.boldLabel ) )
        {
            _clipList.SearchFilter = string.Empty;
            RebuildFilteredList();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if( !File.Exists( CacheFilePath ) )
        {
            EditorGUILayout.HelpBox( "Click 'Scan Project' to generate list of Audio Clips.", MessageType.Info );
            GUI.EndScrollView();
            return;
        }

        if( _clipList.AudioInfor.Count == 0 || _clipList.NeedsRefresh )
        {
            if( !LoadAndTranslateFile() )
            {
                GUI.EndScrollView();
                return;
            }
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label( "Folder" );
        var selectedIndex = _folderPaths.IndexOf(_selectedFolderPath);
        var newIndex = EditorGUILayout.Popup(selectedIndex, _folderPaths.ToArray(), GUILayout.Width(800));
        if( newIndex != selectedIndex )
        {
            _selectedFolderPath = _folderPaths[ newIndex ];
            RebuildFilteredList();
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        var totalClips = _clipList.AudioInfor.Count;
        var dynamicText = string.Format("{0}/{1} clips selected.", SelectedClips.Count, FilteredClips.Count);
        dynamicText += " Total clips: " + totalClips;

        double clipCount = totalClips;
        if( _filteredOut != null )
        {
            clipCount = _filteredOut.Count;
        }

        var pageCount = (int)Math.Ceiling(clipCount / MaxPageSize);
        var pageNames = new string[pageCount];
        var pageNums = new int[pageCount];
        for( var i = 0; i < pageCount; i++ )
        {
            pageNames[ i ] = "Page " + ( i + 1 );
            pageNums[ i ] = i;
        }

        EditorGUILayout.LabelField( dynamicText );

        var oldPage = _pageNumber;
        EditorGUILayout.BeginHorizontal();
        _pageNumber = EditorGUILayout.IntPopup( "", _pageNumber, pageNames, pageNums, GUILayout.Width( 100 ) );
        if( oldPage != _pageNumber )
        {
            RebuildFilteredList( true );
        }
        GUILayout.Label( "of " + pageCount );
        EditorGUILayout.EndHorizontal();

        DisplayClips();
        ShowBulkOperations();

        GUI.EndScrollView();
    }

    private void RebuildFilteredList( bool keepPageNumber = false )
    {
        if( !keepPageNumber ) { _pageNumber = 0; }
        _filterClips = null;
        _filteredOut = null;
    }

    private void ShowBulkOperations()
    {
        GUILayout.BeginArea( new Rect( 0, 616, 895, 200 ) );
        GUILayout.BeginHorizontal( EditorStyles.helpBox );
        GUI.contentColor = Color.white;
        GUILayout.Label( "Bulk Settings: Click Copy buttons to copy setting to all selected." );
        GUILayout.Space( 26 );

        if( GUILayout.Button( new GUIContent( "Copy", "Copy Compression bitrate" ), EditorStyles.toolbarButton, GUILayout.Width( 45 ) ) )
        {
            foreach( var clip in SelectedClips ) { clip.CompressionBitrate = _bulkBitrate; }
        }
        GUILayout.Space( 6 );
        if( GUILayout.Button( new GUIContent( "Copy", "Copy Force Mono" ), EditorStyles.toolbarButton, GUILayout.Width( 45 ) ) )
        {
            foreach( var clip in SelectedClips ) { clip.ForceMono = _bulkForceMono; }
        }
        GUILayout.Space( 26 );
        if( GUILayout.Button( new GUIContent( "Copy", "Copy Audio Format" ), EditorStyles.toolbarButton, GUILayout.Width( 45 ) ) )
        {
            foreach( var clip in SelectedClips ) { clip.Format = _bulkFormat; }
        }
        GUILayout.Space( 101 );
        if( GUILayout.Button( new GUIContent( "Copy", "Copy Load Type" ), EditorStyles.toolbarButton, GUILayout.Width( 45 ) ) )
        {
            foreach( var clip in SelectedClips ) { clip.LoadType = _bulkLoadType; }
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUI.contentColor = Color.white;

        GUILayout.BeginHorizontal();
        GUILayout.Space( 246 );
        _bulkBitrate = EditorGUILayout.IntSlider( "", _bulkBitrate / 1000, 32, 256, GUILayout.Width( 202 ) ) * 1000;
        GUILayout.Space( 36 );
        _bulkForceMono = GUILayout.Toggle( _bulkForceMono, "Force Mono" );
        GUILayout.Space( 35 );
        _bulkFormat = (AudioCompressionFormat) EditorGUILayout.EnumPopup( _bulkFormat, GUILayout.Width( 136 ) );
        GUILayout.Space( 6 );
        _bulkLoadType = (AudioClipLoadType) EditorGUILayout.EnumPopup( _bulkLoadType, GUILayout.Width( 140 ) );
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void ApplyClipChanges( AudioInformation info, bool writeChanges )
    {
        Selection.objects = new Object[ ] { };

        var importer = AssetImporter.GetAtPath(info.FullPath) as AudioImporter;
        if( importer == null )
        {
            Debug.LogError( $"Failed to get AudioImporter for: {info.FullPath}" );
            return;
        }

        // Configurar as novas propriedades do AudioImporter
        var settings = importer.defaultSampleSettings;
        settings.compressionFormat = info.Format;
        settings.loadType = info.LoadType;

        // Para formatos que suportam bitrate (Vorbis, MP3, etc)
        if( info.Format == AudioCompressionFormat.Vorbis ||
            info.Format == AudioCompressionFormat.MP3 ||
            info.Format == AudioCompressionFormat.AAC )
        {
            settings.quality = Mathf.Clamp01( info.CompressionBitrate / 256000f ); // Converter bitrate para qualidade 0-1
        }

        importer.defaultSampleSettings = settings;
        importer.forceToMono = info.ForceMono;

        // Configuração de som espacial (3D/2D)
        var spatialSettings = importer.GetOverrideSampleSettings("Standalone");
        if( spatialSettings.Equals( default( AudioImporterSampleSettings ) ) )
        {
            spatialSettings = settings;
        }

        // Para configuração de som espacial, você pode ajustar:
        // spatialSettings.loadType = info.LoadType;
        // importer.SetOverrideSampleSettings("Standalone", spatialSettings);

        AssetDatabase.ImportAsset( info.FullPath, ImportAssetOptions.ForceUpdate );
        info.HasChanged = true;

        if( writeChanges )
        {
            WriteFile( _clipList );
        }
    }

    private void BuildCache()
    {
        var filePaths = AssetDatabase.GetAllAssetPaths();
        var audioInfo = new AudioInfoData();
        _filterClips = null;
        _pageNumber = 0;
        var updatedTime = DateTime.Now.Ticks;

        foreach( var aPath in filePaths )
        {
            if( !aPath.EndsWith( ".wav", StringComparison.OrdinalIgnoreCase )
                && !aPath.EndsWith( ".mp3", StringComparison.OrdinalIgnoreCase )
                && !aPath.EndsWith( ".ogg", StringComparison.OrdinalIgnoreCase )
                && !aPath.EndsWith( ".aiff", StringComparison.OrdinalIgnoreCase )
                && !aPath.EndsWith( ".aif", StringComparison.OrdinalIgnoreCase ) )
            {
                continue;
            }

            var importer = AssetImporter.GetAtPath(aPath) as AudioImporter;
            if( importer == null ) continue;

            var settings = importer.defaultSampleSettings;
            var bitrate = 156000;

            // Converter qualidade para bitrate aproximado
            if( settings.compressionFormat == AudioCompressionFormat.Vorbis ||
                settings.compressionFormat == AudioCompressionFormat.MP3 ||
                settings.compressionFormat == AudioCompressionFormat.AAC )
            {
                bitrate = Mathf.RoundToInt( settings.quality * 256000 );
            }

            var newClip = new AudioInformation(
                aPath,
                Path.GetFileNameWithoutExtension(aPath),
                false,
                bitrate,
                importer.forceToMono,
                settings.compressionFormat,
                settings.loadType
            );

            newClip.LastUpdated = updatedTime;
            audioInfo.AudioInfor.Add( newClip );
        }

        if( WriteFile( audioInfo ) )
        {
            LoadAndTranslateFile();
        }
    }

    private bool LoadAndTranslateFile()
    {
        XmlDocument xFiles;
        try
        {
            xFiles = new XmlDocument();
            xFiles.Load( CacheFilePath );
        }
        catch
        {
            EditorGUILayout.HelpBox( "Cache file is malformed. Click 'Scan Project' to regenerate it.", MessageType.Error );
            return false;
        }

        if( _clipList.AudioInfor.Count == 0 ) { _clipList.AudioInfor.Clear(); }

        var files = xFiles.SelectNodes("/Files//File");
        if( files == null || files.Count == 0 )
        {
            EditorGUILayout.HelpBox( "You have no audio files in this project.", MessageType.Info );
            return false;
        }

        try
        {
            _clipList.SearchFilter = xFiles.DocumentElement.Attributes[ "searchFilter" ].Value;
            _clipList.SortColumn = (ClipSortColumn) Enum.Parse( typeof( ClipSortColumn ), xFiles.DocumentElement.Attributes[ "sortColumn" ].Value );
            _clipList.SortDir = (ClipSortDirection) Enum.Parse( typeof( ClipSortDirection ), xFiles.DocumentElement.Attributes[ "sortDir" ].Value );

            var currentPaths = new List<string>();
            for( var i = 0; i < files.Count; i++ )
            {
                var aNode = files[i];
                var path = aNode.Attributes["path"].Value.Trim();
                var clipName = aNode.Attributes["name"].Value.Trim();
                var bitrate = int.Parse(aNode.Attributes["bitRate"].Value);
                var forceMono = bool.Parse(aNode.Attributes["forceMono"].Value);

                // Usar Enum.TryParse para compatibilidade com versões antigas
                AudioCompressionFormat format;
                //gg//if( !Enum.TryParse( aNode.Attributes[ "format" ].Value, out format ) )
                //{
                //    // Converter de enum antigo se necessário
                    format = AudioCompressionFormat.Vorbis;
                //}

                AudioClipLoadType loadType;
               //gg if( !Enum.TryParse( aNode.Attributes[ "loadType" ].Value, out loadType ) )
                {
                    loadType = AudioClipLoadType.CompressedInMemory;
                }

                currentPaths.Add( path );
                var folderPath = Path.GetDirectoryName(path);
                if( !string.IsNullOrEmpty( folderPath ) && !_folderPaths.Contains( folderPath ) )
                {
                    _folderPaths.Add( folderPath );
                }

                var matchingClip = _clipList.AudioInfor.Find(obj => obj.FullPath == path);
                if( matchingClip == null )
                {
                    var aud = new AudioInformation(path, clipName, false, bitrate, forceMono, format, loadType);
                    _clipList.AudioInfor.Add( aud );
                }
                else
                {
                    matchingClip.OrigFormat = format;
                    matchingClip.OrigLoadType = loadType;
                    matchingClip.OrigForceMono = forceMono;
                    matchingClip.OrigCompressionBitrate = bitrate;
                }
            }
            _clipList.AudioInfor.RemoveAll( obj => !currentPaths.Contains( obj.FullPath ) );
        }
        catch( Exception e )
        {
            Debug.LogError( $"Error loading cache file: {e.Message}" );
            return false;
        }

        _clipList.NeedsRefresh = false;
        return true;
    }

    private bool WriteFile( AudioInfoData audInfo )
    {
        try
        {
            var sb = new StringBuilder();
            var safeFilter = audInfo.SearchFilter.Replace("'", "").Replace("\"", "");
            sb.Append( string.Format( "<Files searchFilter='{0}' sortColumn='{1}' sortDir='{2}'>",
                safeFilter, audInfo.SortColumn, audInfo.SortDir ) );

            foreach( var aud in audInfo.AudioInfor )
            {
                var bitrate = aud.HasChanged ? aud.CompressionBitrate : aud.OrigCompressionBitrate;
                var mono = aud.HasChanged ? aud.ForceMono : aud.OrigForceMono;
                var fmt = aud.HasChanged ? aud.Format : aud.OrigFormat;
                var loadType = aud.HasChanged ? aud.LoadType : aud.OrigLoadType;

                sb.Append( string.Format( "<File path='{0}' name='{1}' is3d='{2}' bitRate='{3}' forceMono='{4}' format='{5}' loadType='{6}' />",
                    aud.FullPath, aud.Name, false, bitrate, mono, fmt, loadType ) );
            }

            sb.Append( "</Files>" );
            File.WriteAllText( CacheFilePath, sb.ToString() );
            return true;
        }
        catch( Exception e )
        {
            Debug.LogError( $"Error writing cache file: {e.Message}" );
            return false;
        }
    }

    private void DisplayClips()
    {
        EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );
        if( GUILayout.Button( "All", EditorStyles.toolbarButton, GUILayout.Width( 36 ) ) )
        {
            foreach( var t in FilteredClips ) t.IsSelected = true;
        }
        if( GUILayout.Button( "None", EditorStyles.toolbarButton, GUILayout.Width( 36 ) ) )
        {
            foreach( var t in _clipList.AudioInfor ) t.IsSelected = false;
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        _scrollPos = GUI.BeginScrollView( new Rect( 0, 123, 896, 485 ), _scrollPos, new Rect( 0, 124, 880, 24 * FilteredClips.Count + 4 ) );

        foreach( var aClip in FilteredClips )
        {
            EditorGUILayout.BeginHorizontal( EditorStyles.miniButtonMid );
            aClip.IsSelected = GUILayout.Toggle( aClip.IsSelected, "", GUILayout.Width( 20 ) );
            GUILayout.Label( aClip.Name, GUILayout.Width( 150 ) );

            // Slider para bitrate/qualidade
            var bitrateKbps = aClip.CompressionBitrate / 1000;
            bitrateKbps = EditorGUILayout.IntSlider( bitrateKbps, 32, 256, GUILayout.Width( 200 ) );
            aClip.CompressionBitrate = bitrateKbps * 1000;

            aClip.ForceMono = GUILayout.Toggle( aClip.ForceMono, "Mono", GUILayout.Width( 60 ) );
            aClip.Format = (AudioCompressionFormat) EditorGUILayout.EnumPopup( aClip.Format, GUILayout.Width( 120 ) );
            aClip.LoadType = (AudioClipLoadType) EditorGUILayout.EnumPopup( aClip.LoadType, GUILayout.Width( 120 ) );

            EditorGUILayout.EndHorizontal();
        }

        GUI.EndScrollView();
    }

    private void RevertSelected()
    {
        foreach( var aClip in SelectedClips )
        {
            aClip.CompressionBitrate = aClip.OrigCompressionBitrate;
            aClip.ForceMono = aClip.OrigForceMono;
            aClip.Format = aClip.OrigFormat;
            aClip.LoadType = aClip.OrigLoadType;
        }
    }

    private void ApplySelected()
    {
        foreach( var aClip in SelectedClips )
        {
            ApplyClipChanges( aClip, false );
        }
        WriteFile( _clipList );
    }

    private List<AudioInformation> SelectedClips
    {
        get { return FilteredClips.FindAll( c => c.IsSelected ); }
    }

    private List<AudioInformation> FilteredClips
    {
        get
        {
            if( _filterClips != null ) return _filterClips;

            _filterClips = _clipList.AudioInfor.FindAll( c =>
                ( string.IsNullOrEmpty( _clipList.SearchFilter ) ||
                 c.FullPath.ToLower().Contains( _clipList.SearchFilter.ToLower() ) ) &&
                ( _selectedFolderPath == AllFoldersKey ||
                 Path.GetDirectoryName( c.FullPath ) == _selectedFolderPath )
            );

            return _filterClips;
        }
    }

    public enum ClipSortColumn { Name, Bitrate, ForceMono, AudioFormat, LoadType }
    public enum ClipSortDirection { Ascending, Descending }

    public class AudioInfoData
    {
        public List<AudioInformation> AudioInfor = new List<AudioInformation>();
        public string SearchFilter = string.Empty;
        public ClipSortColumn SortColumn = ClipSortColumn.Name;
        public ClipSortDirection SortDir = ClipSortDirection.Ascending;
        public bool NeedsRefresh;
    }

    public class AudioInformation
    {
        public int OrigCompressionBitrate, CompressionBitrate;
        public AudioCompressionFormat OrigFormat, Format;
        public AudioClipLoadType OrigLoadType, LoadType;
        public bool OrigForceMono, ForceMono, IsSelected, HasChanged;
        public string FullPath, Name;
        public long LastUpdated;

        public AudioInformation( string path, string name, bool is3d, int bitrate, bool mono,
                              AudioCompressionFormat fmt, AudioClipLoadType lt )
        {
            FullPath = path;
            Name = name;
            CompressionBitrate = OrigCompressionBitrate = bitrate;
            ForceMono = OrigForceMono = mono;
            Format = OrigFormat = fmt;
            LoadType = OrigLoadType = lt;
        }
    }
}
