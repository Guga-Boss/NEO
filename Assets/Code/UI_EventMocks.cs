// UI_EventMocks.cs
// Mock ajustado baseada na sua versão, liberando Networking para o CloudClock
using System;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

// === MOCK DAILY REWARDS ===
namespace DailyRewards
{
    public class GetInstance
    {
        public static GetInstance Instance => new GetInstance();
    }

    public class TimedRewardUI: MonoBehaviour
    {
        public GameObject button;
    }
}

// === MOCK UNITY ENGINE NETWORKING ===
// [COMENTADO] Precisamos desativar esse Mock para que o CloudClockBuilder.cs 
// consiga acessar o 'UnityWebRequest' real da Unity e pegar a hora online.
/*
namespace UnityEngine.Networking
{
    public class DownloadHandler { }
    public class UploadHandler { }
}
*/

// === MOCK UI SYSTEM ===
namespace UnityEngine.UI
{
    public class GraphicRaycaster: MonoBehaviour { }
    public class Selectable: MonoBehaviour { }
    public class InputField: MonoBehaviour
    {
        public string text;
    }

    public class MaskableGraphic: MonoBehaviour
    {
        public Color color { get; set; }
    }

    public class Button: MonoBehaviour
    {
        public UnityEngine.Events.UnityEvent onClick = new UnityEngine.Events.UnityEvent();
    }
}

// === MOCK EVENT SYSTEM ===
namespace UnityEngine.EventSystems
{
    public interface IEventSystemHandler { }

    public enum EventTriggerType
    {
        PointerDown, PointerUp, PointerClick,
        PointerEnter, PointerExit,
        Drag, Drop, Scroll,
        UpdateSelected, Select, Deselect,
        Move, InitializePotentialDrag, BeginDrag,
        EndDrag, Submit, Cancel
    }

    public class EventSystem: MonoBehaviour
    {
        public static EventSystem current;
        public GameObject currentSelectedGameObject;
        public void SetSelectedGameObject( GameObject go ) { currentSelectedGameObject = go; }
    }

    public class BaseEventData
    {
        public EventSystem eventSystem;
        public BaseEventData( EventSystem es ) { eventSystem = es; }
    }

    public class RaycastResult { }

    public class PointerEventData
    {
        public GameObject pointerEnter;
        public GameObject pointerPress;
        public GameObject pointerDrag;
    }

    public class EventTrigger: MonoBehaviour
    {
        public class Entry
        {
            public EventTriggerType eventID;
            public TriggerEvent callback = new TriggerEvent();
        }

        [Serializable]
        public class TriggerEvent
        {
            private Action<BaseEventData> _listeners;
            public void AddListener( Action<BaseEventData> action ) { _listeners += action; }
            public void RemoveListener( Action<BaseEventData> action ) { _listeners -= action; }
            public void Invoke( BaseEventData data ) { if( _listeners != null ) _listeners( data ); }
        }

        public List<Entry> triggers = new List<Entry>();
    }
}

// === MOCK TK2D / UNITY CORE ===
namespace UnityEngine
{
    // CUIDADO: Se sua Unity for muito nova, PhysicsMaterial pode já não ter esse nome antigo.
    // Mas mantive conforme seu código.
    [Obsolete( "PhysicMaterial has been renamed to PhysicsMaterial" )]
    public class PhysicMaterial: PhysicsMaterial { }

    // Se a Unity reclamar que PhysicsMaterial já existe, comente a linha abaixo:
    // public class PhysicsMaterial { } 

    // WWW Mock mantido para outros scripts, já que o CloudClock não usa mais ele.
    public class WWW
    {
        public AudioClip GetAudioClip() => null;
        public WWW( string url ) { }
        public byte[ ] bytes => new byte[ 0 ];
        public string text => ""; // Adicionei .text pois costuma ser usado
        public string error => null; // Adicionei .error para evitar quebras
        public bool isDone => true; // Adicionei .isDone
    }

    public static class ImageConversion
    {
        public static byte[ ] EncodeToPNG( Texture2D tex ) => new byte[ 0 ];
        public static bool LoadImage( Texture2D tex, byte[ ] bytes ) => true;
    }
}

// === MOCK ENERGY BAR TOOLKIT (ATUALIZADO) ===
namespace EnergyBarToolkit
{
    public class EnergyBar: MonoBehaviour
    {
        public float valueCurrent;
        public float valueMax = 100;
        public float valueMin = 0;

        public void SetValueCurrent( int val ) { valueCurrent = val; }
        public void SetValueMax( int val ) { valueMax = val; }
        public void SetValueMin( int val ) { valueMin = val; }

        public void SetValueCurrent( float val ) { valueCurrent = val; }
        public void SetValueMax( float val ) { valueMax = val; }
        public void SetValueMin( float val ) { valueMin = val; }
    }

    public class MadPanel: MonoBehaviour
    {
        public static List<MadPanel> allPanels = new List<MadPanel>();
        public object AllSpritesForScreenPoint( Vector2 point ) => null;
        protected virtual void Update() { }
    }

    public class EnergyBarBase: MonoBehaviour { }

    public class MadSprite: MonoBehaviour
    {
        public Color color = Color.white;
        public bool visible = true;
    }

    public class MadAtlas: ScriptableObject
    {
        public class Item { }
    }

    public class MadAtlasBuilder
    {
        public void PackTextures( Texture2D[ ] textures, string path, List<MadAtlas.Item> items ) { }
    }
}

// === MOCK TK2D EDITOR ===
namespace TK2DROOT.tk2d.Editor
{
    public class tk2dEditorUtility { }
    public class tk2dSkin { }
}

// Compatibilidade global
public class EnergyBar: EnergyBarToolkit.EnergyBar { }


// === MOCK EASY SAVE 2 ===
namespace UnityEngine
{
    public static class ES2ApplicationMock
    {
        public static bool isWebPlayer => false;
    }
}

//public class ES2
//{
//    public static void Save<T>( T data, string identifier ) { }
//    public static T Load<T>( string identifier ) => default( T );
//    public static bool Exists( string identifier ) => false;
//    public static void Delete( string identifier ) { }

//    public static string[ ] GetFiles( string path ) => new string[ 0 ];
//    public static void Rename( string source, string dest ) { }
//    public static string[ ] GetFiles( string path, string extension ) => new string[ 0 ];
//}

//public class ES2Settings
//{
//    public ES2Settings() { }
//    public ES2Settings( string identifier ) { }
//}

public interface IColorable
{
    Color color { get; set; }
}

namespace Moodkie.EasySave2
{
    public class ES2WebPlayerFix
    {
        public static bool isWebPlayer = false;
    }




}
public class cInput: MonoBehaviour
{
    // --- Propriedades de Estado ---
    public static bool scanning = false;
    public static float scanningDeadzone = 0.5f;

    // --- Propriedades de Contagem (Resolve erros no cInputGUI) ---
    // Defina aqui quantos inputs você tinha no sistema antigo
    public static int length = 10;

    // --- Métodos de Verificação de Botão ---
    public static bool GetButton( string name ) => Input.GetButton( name );
    public static bool GetButtonDown( string name ) => Input.GetButtonDown( name );
    public static bool GetButtonUp( string name ) => Input.GetButtonUp( name );

    // --- Métodos de Eixo ---
    public static float GetAxis( string name ) => Input.GetAxis( name );
    public static float GetAxisRaw( string name ) => Input.GetAxisRaw( name );

    // --- Métodos de Texto e UI (Resolve erros no cInputGUI) ---
    // n é o índice, i é 0 para Primário e 1 para Secundário
    public static string GetText( int n, int i )
    {
        return "Input " + n;
    }

    public static string GetKeyText( string actionName )
    {
        return "Key";
    }
    public static float GetKey( string name ) => Input.GetAxis( name );
    // --- Métodos de Configuração (Mocks vazios para não dar erro) ---
    public static void SetKey( string name, string primaryKey ) { }
    public static void SetKey( string name, string primaryKey, string secondaryKey ) { }

    public static void ChangeKey( int index, int inputNum, string newKey ) { }

    // --- Métodos de Salvamento ---
    public static void Save() { Debug.Log( "cInput Mock: Save chamado" ); }
    public static void Load() { Debug.Log( "cInput Mock: Load chamado" ); }

    // --- Sistema de Scan (Usado no cInputGUI para remapeamento) ---
    public static void Scan() { scanning = true; }
}

// --- Mock da Classe Keys (Resolve erros de referências a Keys.A, Keys.Jump, etc) ---
public static class Keys
{
    public const string Escape = "Escape";
    public const string Return = "Return";
    public const string Space = "Space";
    public const string LeftShift = "LeftShift";
    public const string RightShift = "RightShift";
    public const string LeftControl = "LeftControl";
    // Adicione as letras conforme a necessidade dos erros
    public const string A = "A"; public const string B = "B"; public const string C = "C";
    public const string D = "D"; public const string W = "W"; public const string S = "S";

    // Mock para eixos de controle (Xbox/Playstation)
    public static string Joy1Axis1Positive = "Joy1Axis1+";
    public static string Joy1Axis1Negative = "Joy1Axis1-";
}