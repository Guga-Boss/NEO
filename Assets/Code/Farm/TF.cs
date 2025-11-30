using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Reflection;
using Sirenix.OdinInspector;

[Serializable]
public class TaggedField
{
    #region Variables
    [HorizontalGroup( "Info", Width = 300 )]      // largura total do grupo
    [LabelWidth( 50 )]                             // largura do rótulo
    [LabelText( "Tag" )]
    public string Tag;

    [HorizontalGroup( "Info" )]
    [LabelWidth( 50 )]                             // largura do rótulo
    [LabelText( "Type" )]
    public TFType ValueType;

    [FoldoutGroup( "Advanced Values" )]
    public int IntValue;
    [FoldoutGroup( "Advanced Values" )]
    public float FloatValue;
    [FoldoutGroup( "Advanced Values" )]
    public string StringValue;
    [FoldoutGroup( "Advanced Values" )]
    public bool BoolValue;
    [FoldoutGroup( "Advanced Values" )]
    public Vector2 Vector2Value;
    [FoldoutGroup( "Advanced Values" )]
    public List<int> IntListValue = new List<int>();
    [FoldoutGroup( "Advanced Values" )]
    public List<bool> BoolListValue = new List<bool>();
    [FoldoutGroup( "Advanced Values" )]
    public List<string> StringListValue = new List<string>();
    [FoldoutGroup( "Advanced Values" )]
    public List<float> FloatListValue = new List<float>();
    [FoldoutGroup( "Advanced Values" )]
    public List<Vector2> Vector2ListValue = new List<Vector2>();
    #endregion

    public object GetValue()
    {
        switch( ValueType )
        {
            case TFType.Int: return IntValue; // Return int ; main retrieval for Int
            case TFType.Float: return FloatValue; // Return float ; main retrieval for Float
            case TFType.String: return StringValue; // Return string ; main retrieval for String
            case TFType.Bool: return BoolValue; // Return bool ; main retrieval for Bool
            case TFType.Vector2: return Vector2Value; // Return Vector2 ; main retrieval for Vector2
            case TFType.IntList: return IntListValue; // Return List<int> ; main retrieval for IntList
            case TFType.BoolList: return BoolListValue; // Return List<bool> ; main retrieval for BoolList
            case TFType.StringList: return StringListValue; // Return List<string> ; main retrieval for StringList
            case TFType.FloatList: return FloatListValue; // Return List<float> ; main retrieval for FloatList
            case TFType.Vector2List: return Vector2ListValue; // Return List<Vector2> ; main retrieval for Vector2List
            default: return null; // Unknown type ; fallback
        }
    }

    public void SetValue( object val )
    {
        switch( ValueType )
        {
            case TFType.Int:
            if( val is Enum )
                IntValue = Convert.ToInt32( val ); // Converte enum para int
            else
                IntValue = Convert.ToInt32( val ); // Set int value ; conversion
            break;
            case TFType.Float:
            FloatValue = Convert.ToSingle( val ); // Set float value ; conversion
            break;
            case TFType.String:
            StringValue = Convert.ToString( val ); // Set string value ; conversion
            break;
            case TFType.Bool:
            BoolValue = Convert.ToBoolean( val ); // Set string value ; conversion
            break;
            case TFType.Vector2:
            Vector2Value = ( Vector2 ) val; // Set Vector2 value ; cast
            break;
            case TFType.IntList:
            if( val is List<int> )
                IntListValue = ( List<int> ) val; // Set IntList ; safe cast
            else
                Debug.LogError( "Failed to set IntListValue: value is not List<int>" ); // Error ; type mismatch
            break;
            case TFType.BoolList:
            if( val is List<bool> )
                BoolListValue = ( List<bool> ) val; // Set BoolList ; safe cast
            else
                Debug.LogError( "Failed to set BoolListValue: value is not List<bool>" ); // Error ; type mismatch
            break;

            case TFType.StringList:
            if( val is List<string> )
                StringListValue = ( List<string> ) val; // Set StringList ; safe cast
            else
                Debug.LogError( "Failed to set StringListValue: value is not List<String>" ); // Error ; type mismatch
            break;

            case TFType.FloatList:
            if( val is List<float> )
                FloatListValue = ( List<float> ) val; // Set FloatList ; safe cast
            else
                Debug.LogError( "Failed to set FloatListValue: value is not List<float>" ); // Error ; type mismatch
            break;
            case TFType.Vector2List:
            if( val is List<Vector2> )
                Vector2ListValue = ( List<Vector2> ) val; // Set Vector2List ; safe cast
            else
                Debug.LogError( "Failed to set Vector2ListValue: value is not List<Vector2>" ); // Error ; type mismatch
            break;
        }
    }
}

public enum TFType
{
    NONE = -1,
    Int,
    Float,
    String,
    Vector2,
    Bool,
    IntList = 10,
    FloatList,
    BoolList,
    StringList,
    Vector2List = 20
}

[System.Serializable]
public class TaggedFieldList
{
    public List<TaggedField> Fields = new List<TaggedField>(); // Serializable list ; used for Unity serialization

    private Dictionary<string, TaggedField> fieldDictionary; // Quick access dictionary ; not serialized

    public void BuildDictionary()
    {
        fieldDictionary = new Dictionary<string, TaggedField>(); // Initialize dictionary ; build once
        foreach( var field in Fields )
        {
            if( !string.IsNullOrEmpty( field.Tag ) && !fieldDictionary.ContainsKey( field.Tag ) )
            {
                fieldDictionary.Add( field.Tag, field ); // Add field ; key = tag
            }
        }
    }

    public TaggedField GetField( string tag )
    {
        if( fieldDictionary == null )
        {
            BuildDictionary(); // Ensure dictionary built ; lazy init
        }

        TaggedField result;
        if( fieldDictionary.TryGetValue( tag, out result ) )
        {
            return result; // Found field ; return
        }
        else
        {
            Debug.LogError( "Tag not found: " + tag ); // Error ; missing tag
            return null;
        }
    }

    public bool ContainsKey( string tag )
    {
        if( fieldDictionary == null ) BuildDictionary(); // Lazy init
        return fieldDictionary.ContainsKey( tag ); // Check existence
    }

    public void AddField( TaggedField field )
    {
        Fields.Add( field ); // Add to list
        if( fieldDictionary != null && !fieldDictionary.ContainsKey( field.Tag ) )
        {
            fieldDictionary.Add( field.Tag, field ); // Add to dictionary ; keep sync
        }
    }
}

public class TF : MonoBehaviour
{
    public static TF I; // Singleton ; global access

    [SerializeField]
    public TaggedFieldList FieldList = new TaggedFieldList(); // Active list ; default
    public TaggedFieldList FarmFieldList = new TaggedFieldList(); // Farm fields ; optional
    public TaggedFieldList BuildingFieldList = new TaggedFieldList(); // Building fields ; optional
    public TaggedFieldList BlueprintFieldList = new TaggedFieldList(); // Building fields ; optional
    public TaggedFieldList InventoryFieldList = new TaggedFieldList(); // inventory fields ; optional

    private Dictionary<string, Action<BinaryReader, TaggedField>> loaders = new Dictionary<string, Action<BinaryReader, TaggedField>>(); // Custom loaders ; per tag
    private Dictionary<string, Action<BinaryWriter, TaggedField>> savers = new Dictionary<string, Action<BinaryWriter, TaggedField>>(); // Custom savers ; per tag

    void Awake()
    {
        I = this; // Set singleton ; ensure instance

        if( FarmFieldList != null ) FarmFieldList.BuildDictionary(); // Build dictionary ; once
        if( BuildingFieldList != null ) BuildingFieldList.BuildDictionary(); // Build dictionary ; once
        if( BlueprintFieldList != null ) BlueprintFieldList.BuildDictionary(); // Build dictionary ; once

        FieldList = FarmFieldList; // Default active ; Farm
    }

    public static void ActivateFieldList( string type )
    {
        switch( type )
        {
            case "Farm": I.FieldList = I.FarmFieldList; return; // Activate farm list ; switch
            case "Building": I.FieldList = I.BuildingFieldList; return; // Activate building list ; switch
            case "Blueprint": I.FieldList = I.BlueprintFieldList; return; // Activate building list ; switch
            case "Inventory": I.FieldList = I.InventoryFieldList; return; // Activate inventory list ; switch
        }
        Debug.LogError( "Bad Field List: " + type ); // Error ; unknown type
    }

    private static TaggedField GetFieldSafe( string tag, object value = null )
    {
        if( Helper.I.ReleaseVersion == false )
        if( I == null )
        {
            GameObject tfObj = GameObject.Find( "Tagged Field" ); // Find object in scene ; fallback
            if( tfObj != null ) I = tfObj.GetComponent<TF>();
            if( I == null )
            {
                Debug.LogError( "TF instance not found!" ); // Error ; singleton missing
                return null;
            }
        }

        var field = I.FieldList.GetField( tag ); // Retrieve field
        if( field == null )
        {
            Debug.LogError( "Field not registered: " + tag ); // Error ; missing
            return null;
        }

        if( !Helper.I.ReleaseVersion && value != null && !CheckTypeSafe( field, value ) )
        {
            Debug.LogError( "Type mismatch for field " + tag + ": expected " + field.ValueType + ", received " + ( value != null ? value.GetType().Name : "null" ) ); // Error ; type mismatch
            return null;
        }

        return field; // Valid field ; return
    }

    private static bool CheckTypeSafe( TaggedField field, object value )
    {
        switch( field.ValueType )
        {
            case TFType.Int: return value is int; // Type check ; Int
            case TFType.Float: return value is float; // Type check ; Float
            case TFType.String: return value is string; // Type check ; String
            case TFType.Vector2: return value is Vector2; // Type check ; Vector2
            case TFType.IntList: return value is List<int>; // Type check ; IntList
            case TFType.BoolList: return value is List<bool>; // Type check ; BoolList
            case TFType.StringList: return value is List<string>; // Type check ; StringList
            case TFType.FloatList: return value is List<float>; // Type check ; FloatList
            case TFType.Vector2List: return value is List<Vector2>; // Type check ; Vector2List
            default: return false; // Unknown type ; fallback
        }
    }

    // ===================== SAVE =====================
    public static void Save<T>( string tag, T value, TaggedField tf = null )
    {
        TaggedField field = tf ?? GetFieldSafe( tag, value ); // Usa o tf se fornecido, senão busca no dicionário
        if( field == null ) return;

        field.SetValue( value ); // Update field ; store value
        GS.W.Write( ( int ) field.ValueType ); // Write type ; main header

        switch( field.ValueType )
        {
            case TFType.Int:
            // Suporta enums convertendo para int de forma segura
            if( value is Enum )
                GS.W.Write( Convert.ToInt32( value ) ); // Usa o value original que pode ser enum
            else
                GS.W.Write( field.IntValue );
            break;

            case TFType.Float:
            GS.W.Write( field.FloatValue ); // Write float ; main
            break;

            case TFType.String:
            GS.W.Write( field.StringValue ); // Write string ; main
            break;

            case TFType.Bool:
            GS.W.Write( field.BoolValue ); // Write string ; main
            break;

            case TFType.Vector2:
            GS.W.Write( field.Vector2Value.x ); // Write Vector2.x ; main
            GS.W.Write( field.Vector2Value.y ); // Write Vector2.y ; main
            break;

            case TFType.IntList:
            int countInt = field.IntListValue.Count; // Count elements ; main
            GS.W.Write( countInt ); // Write count
            if( countInt > 0 )
            {
                int[] arr = field.IntListValue.ToArray(); // Convert to array ; block copy
                byte[] bytes = new byte[ arr.Length * sizeof( int ) ];
                Buffer.BlockCopy( arr, 0, bytes, 0, bytes.Length );
                GS.W.Write( bytes ); // Write bytes ; efficient
            }
            break;

            case TFType.BoolList:
            countInt = field.BoolListValue.Count; // Count elements ; main
            GS.W.Write( countInt ); // Write count
            if( countInt > 0 )
            {
                bool[] arr = field.BoolListValue.ToArray(); // Convert to array ; block copy
                byte[] bytes = new byte[ arr.Length * sizeof( bool ) ];
                Buffer.BlockCopy( arr, 0, bytes, 0, bytes.Length );
                GS.W.Write( bytes ); // Write bytes ; efficient
            }
            break;

            case TFType.StringList:
            int countString = field.StringListValue.Count; // Count elements
            GS.W.Write( countString ); // Write count
            foreach( var str in field.StringListValue )
            {
                GS.W.Write( str ?? "" ); // Write each string (handle null)
            }
            break;

            case TFType.FloatList:
            countInt = field.FloatListValue.Count; // Count elements ; main
            GS.W.Write( countInt ); // Write count
            if( countInt > 0 )
            {
                float[] arr = field.FloatListValue.ToArray(); // Convert to array ; block copy
                byte[] bytes = new byte[ arr.Length * sizeof( float ) ];
                Buffer.BlockCopy( arr, 0, bytes, 0, bytes.Length );
                GS.W.Write( bytes ); // Write bytes ; efficient
            }
            break;

            case TFType.Vector2List:
            int countVec = field.Vector2ListValue.Count; // Count elements ; main
            GS.W.Write( countVec ); // Write count
            if( countVec > 0 )
            {
                float[] arr = new float[ countVec * 2 ]; // Flatten list ; x,y pairs
                for( int i = 0; i < countVec; i++ )
                {
                    arr[ i * 2 ] = field.Vector2ListValue[ i ].x;
                    arr[ i * 2 + 1 ] = field.Vector2ListValue[ i ].y;
                }
                byte[] bytes = new byte[ arr.Length * sizeof( float ) ];
                Buffer.BlockCopy( arr, 0, bytes, 0, bytes.Length );
                GS.W.Write( bytes ); // Write bytes ; efficient
            }
            break;
        }
    }

    public static T Load<T>( string tag, TaggedField tf = null )
    {
        // Pega o campo do dicionário ou temporário
        TaggedField field = tf ?? GetFieldSafe( tag );
        if( field == null ) return default( T );

        // Se stream terminou, retorna valor default do field
        if( GS.R.BaseStream.Position >= GS.R.BaseStream.Length )
            return ( T ) field.GetValue();

        long startPos = GS.R.BaseStream.Position; // backup da posição

        try
        {
            // Tenta ler o tipo do stream
            if( GS.R.BaseStream.Position + sizeof( int ) > GS.R.BaseStream.Length )
            {
                // não tem nada salvo, rollback e retorna valor default
                GS.R.BaseStream.Position = startPos;
                return ( T ) field.GetValue();
            }

            TFType streamType = ( TFType ) GS.R.ReadInt32();

            // Se houver loader customizado e tipos baterem
            if( I.loaders.ContainsKey( tag ) && streamType == field.ValueType )
            {
                I.loaders[ tag ]( GS.R, field );
                return ( T ) field.GetValue();
            }

            // Switch baseado no tipo salvo
            switch( streamType )
            {
                case TFType.Int:
                if( field.ValueType != TFType.Int ) { GS.R.BaseStream.Position = startPos; return ( T ) field.GetValue(); }
                field.IntValue = GS.R.ReadInt32();
                if( typeof( T ).IsEnum ) return ( T ) Enum.ToObject( typeof( T ), field.IntValue );
                return ( T ) Convert.ChangeType( field.IntValue, typeof( T ) );

                case TFType.Float:
                if( field.ValueType != TFType.Float ) { GS.R.BaseStream.Position = startPos; return ( T ) field.GetValue(); }
                field.FloatValue = GS.R.ReadSingle();
                return ( T ) ( object ) field.FloatValue;

                case TFType.String:
                if( field.ValueType != TFType.String ) { GS.R.BaseStream.Position = startPos; return ( T ) field.GetValue(); }
                field.StringValue = GS.R.ReadString();
                return ( T ) ( object ) field.StringValue;

                case TFType.Bool:
                if( field.ValueType != TFType.Bool ) { GS.R.BaseStream.Position = startPos; return ( T ) field.GetValue(); }
                field.BoolValue = GS.R.ReadBoolean();
                return ( T ) ( object ) field.BoolValue;

                case TFType.Vector2:
                if( field.ValueType != TFType.Vector2 ) { GS.R.BaseStream.Position = startPos; return ( T ) field.GetValue(); }
                float x = GS.R.ReadSingle();
                float y = GS.R.ReadSingle();
                field.Vector2Value = new Vector2( x, y );
                return ( T ) ( object ) field.Vector2Value;

                case TFType.IntList:
                if( field.ValueType != TFType.IntList ) { GS.R.BaseStream.Position = startPos; return ( T ) field.GetValue(); }
                int countInt = GS.R.ReadInt32();
                if( countInt > 0 )
                {
                    byte[] bytes = GS.R.ReadBytes( countInt * sizeof( int ) );
                    int[] arr = new int[ countInt ];
                    Buffer.BlockCopy( bytes, 0, arr, 0, bytes.Length );
                    field.IntListValue = new List<int>( arr );
                }
                else field.IntListValue = new List<int>();
                return ( T ) ( object ) field.IntListValue;

                case TFType.BoolList:
                if( field.ValueType != TFType.BoolList ) { GS.R.BaseStream.Position = startPos; return ( T ) field.GetValue(); }
                int countBool = GS.R.ReadInt32();
                if( countBool > 0 )
                {
                    byte[] bytes = GS.R.ReadBytes( countBool * sizeof( bool ) );
                    bool[] arr = new bool[ countBool ];
                    Buffer.BlockCopy( bytes, 0, arr, 0, bytes.Length );
                    field.BoolListValue = new List<bool>( arr );
                }
                else field.BoolListValue = new List<bool>();
                return ( T ) ( object ) field.BoolListValue;

                case TFType.StringList:
                if( field.ValueType != TFType.StringList ) { GS.R.BaseStream.Position = startPos; return ( T ) field.GetValue(); }
                int countString = GS.R.ReadInt32();
                field.StringListValue = new List<string>( countString );
                for( int i = 0; i < countString; i++ )
                    field.StringListValue.Add( GS.R.ReadString() );
                return ( T ) ( object ) field.StringListValue;

                case TFType.FloatList:
                if( field.ValueType != TFType.FloatList ) { GS.R.BaseStream.Position = startPos; return ( T ) field.GetValue(); }
                int countFloat = GS.R.ReadInt32();
                if( countFloat > 0 )
                {
                    byte[] bytes = GS.R.ReadBytes( countFloat * sizeof( float ) );
                    float[] arr = new float[ countFloat ];
                    Buffer.BlockCopy( bytes, 0, arr, 0, bytes.Length );
                    field.FloatListValue = new List<float>( arr );
                }
                else field.FloatListValue = new List<float>();
                return ( T ) ( object ) field.FloatListValue;

                case TFType.Vector2List:
                if( field.ValueType != TFType.Vector2List ) { GS.R.BaseStream.Position = startPos; return ( T ) field.GetValue(); }
                int countVec = GS.R.ReadInt32();
                if( countVec > 0 )
                {
                    byte[] bytes = GS.R.ReadBytes( countVec * 2 * sizeof( float ) );
                    float[] arr = new float[ countVec * 2 ];
                    Buffer.BlockCopy( bytes, 0, arr, 0, bytes.Length );
                    field.Vector2ListValue = new List<Vector2>( countVec );
                    for( int i = 0; i < countVec; i++ )
                        field.Vector2ListValue.Add( new Vector2( arr[ i * 2 ], arr[ i * 2 + 1 ] ) );
                }
                else field.Vector2ListValue = new List<Vector2>();
                return ( T ) ( object ) field.Vector2ListValue;

                default:
                GS.R.BaseStream.Position = startPos; // rollback
                return ( T ) field.GetValue();
            }
        }
        catch
        {
            GS.R.BaseStream.Position = startPos; // rollback em caso de erro
            return ( T ) field.GetValue(); // retorna valor default
        }
    }
    
    // ===================== SAVE TEMP =====================
    public static void SaveT<T>( string tag, T value, TFType forcedType = TFType.NONE )
    {
        // Cria campo temporário
        var tempField = CreateTempField( tag, value, forcedType );

        // Chama o Save normal passando o campo temporário
        Save( tag, value, tempField );
    }

    // ===================== LOAD TEMP =====================
    public static T LoadT<T>( string tag )
    {
        // Cria campo temporário
        var tempField = CreateTempFieldForLoad<T>( tag );

        // Chama o Load normal passando o campo temporário
        return Load<T>( tag, tempField );
    }

    private static TaggedField CreateTempFieldForLoad<T>( string tag )
    {
        TFType valueType = GetValueTypeFromGeneric<T>();
        var tempField = new TaggedField
        {
            Tag = tag,
            ValueType = valueType
        };
        return tempField;
    }
    private static TFType GetValueTypeFromGeneric<T>()
    {
        Type type = typeof( T );

        if( type == typeof( int ) ) return TFType.Int;
        if( type == typeof( float ) ) return TFType.Float;
        if( type == typeof( string ) ) return TFType.String;
        if( type == typeof( bool ) ) return TFType.Bool;
        if( type == typeof( Vector2 ) ) return TFType.Vector2;
        if( type == typeof( List<int> ) ) return TFType.IntList;
        if( type == typeof( List<bool> ) ) return TFType.BoolList;
        if( type == typeof( List<string> ) ) return TFType.StringList;
        if( type == typeof( List<float> ) ) return TFType.FloatList;
        if( type == typeof( List<Vector2> ) ) return TFType.Vector2List;

        if( type.IsEnum ) return TFType.Int;

        throw new ArgumentException( "Unsupported type: " + type );
    }

    /*
    ATENÇÃO: listas de strings (List<string>) podem ser mal interpretadas como IntList
    se estiverem vazias ou se o tipo não for claramente inferido.  
    Para evitar erros no load, sempre que salvar List<string>, especifique o tipo:
        TF.SaveT("Tag", minhaListaString, TaggedValueType.StringList);
    Isso garante que o ValueType seja correto e previne leitura incorreta.
     * o forcedType foi criado para isso, pois teve bug no save
*/
    // Método auxiliar para criar TaggedField temporário
    private static TaggedField CreateTempField<T>( string tag, T value, TFType forcedType = TFType.NONE )
{
    TFType valueType = forcedType;
    if( forcedType == TFType.NONE )
        valueType = GetValueType<T>( value );

    var tempField = new TaggedField
    {
        Tag = tag,
        ValueType = valueType
    };
    tempField.SetValue(value);
    return tempField;
}

// Método para determinar o tipo
private static TFType GetValueType<T>( T value )
{
    Type type = typeof( T );

    // Se for object e value não for null, usa o tipo real
    if( type == typeof( object ) && value != null )
        type = value.GetType();

    // Verifica listas genéricas
    if( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( List<> ) )
    {
        Type innerType = type.GetGenericArguments()[ 0 ];
        if( innerType == typeof( int ) ) return TFType.IntList;
        if( innerType == typeof( bool ) ) return TFType.BoolList;
        if( innerType == typeof( string ) ) return TFType.StringList;
        if( innerType == typeof( float ) ) return TFType.FloatList;
        if( innerType == typeof( Vector2 ) ) return TFType.Vector2List;
    }

    // Tipos simples
    if( type == typeof( int ) ) return TFType.Int;
    if( type == typeof( float ) ) return TFType.Float;
    if( type == typeof( string ) ) return TFType.String;
    if( type == typeof( bool ) ) return TFType.Bool;
    if( type == typeof( Vector2 ) ) return TFType.Vector2;

    // Suporte a enums
    if( type.IsEnum ) return TFType.Int;

    throw new ArgumentException( "Unsupported type: " + type );
}
private static T Error<T>( string tag, TFType expected, TFType actual )
    {
        Debug.LogError( "Load mismatch for field " + tag + ": expected " + expected + ", got " + actual ); // Error ; type mismatch
        return default( T ); // Return default ; safe fallback
    }

    public void DebugAllRegisteredFields()
    {
        if( FieldList == null || FieldList.Fields.Count == 0 )
        {
            Debug.Log( "No fields registered!" ); // Debug ; empty
            return;
        }

        Debug.Log( "=== REGISTERED FIELDS ===" ); // Debug header
        foreach( var field in FieldList.Fields )
        {
            Debug.Log( "Tag: " + field.Tag + " | Type: " + field.ValueType + " | Value: " + field.GetValue() ); // Debug ; show content
        }
        Debug.Log( "Total: " + FieldList.Fields.Count + " fields" ); // Debug ; total count
    }

}
