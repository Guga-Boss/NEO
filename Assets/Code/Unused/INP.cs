using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

public class INP: MonoBehaviour
{
    public static INP I;

    [Button( "Reset Controls", ButtonSizes.Large ), GUIColor( 0, 1, 0 )]
    public void SetupControls()
    {
        MoveN = CreateProp( "numpad8", "8" );
        MoveNE = CreateProp( "numpad9", "9" );
        MoveE = CreateProp( "numpad6", "o" );
        MoveSE = CreateProp( "numpad3", "l" );
        MoveS = CreateProp( "numpad2", "k" );
        MoveSW = CreateProp( "numpad1", "j" );
        MoveW = CreateProp( "numpad4", "u" );
        MoveNW = CreateProp( "numpad7", "7" );
        RotCCW = CreateProp( "q", "c" );
        RotCW = CreateProp( "v", "w" );
        WaitKey = CreateProp( "x", "x" );
        SpecialKey = CreateProp( "v", "w" );
        BattleKey = CreateProp( "b", "b" );
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty( this );
#endif
        Debug.Log( "✅ Controles configurados!" );
    }

    private InputActionProperty CreateProp( string k1, string k2 )
    {
        var action = new InputAction();
        action.AddBinding( $"<Keyboard>/{k1}" );
        action.AddBinding( $"<Keyboard>/{k2}" );
        return new InputActionProperty( action );
    }

    [Header("Movement:")]
    public InputActionProperty MoveN, MoveNE, MoveE, MoveSE, MoveS, MoveSW, MoveW, MoveNW;
    [Header("Rotation:")]
    public InputActionProperty RotCCW, RotCW;
    [Header("Other:")]
    public InputActionProperty WaitKey, SpecialKey, BattleKey;

    public bool N => MoveN.action != null && MoveN.action.IsPressed();
    public bool NE => MoveNE.action != null && MoveNE.action.IsPressed();
    public bool E => MoveE.action != null && MoveE.action.IsPressed();
    public bool SE => MoveSE.action != null && MoveSE.action.IsPressed();
    public bool S => MoveS.action != null && MoveS.action.IsPressed();
    public bool SW => MoveSW.action != null && MoveSW.action.IsPressed();
    public bool W => MoveW.action != null && MoveW.action.IsPressed();
    public bool NW => MoveNW.action != null && MoveNW.action.IsPressed();
    public bool CCW => RotCCW.action != null && RotCCW.action.IsPressed();
    public bool CW => RotCW.action != null && RotCW.action.IsPressed();
    public bool Wait => WaitKey.action != null && WaitKey.action.IsPressed();
    public bool Special => SpecialKey.action != null && SpecialKey.action.IsPressed();
    public bool Battle => BattleKey.action != null && BattleKey.action.IsPressed();


    void Awake() => I = this;

    void OnEnable()
    {
        // Mandatory Activation of all actions to ensure they work when pressed
        MoveN.action?.Enable(); MoveNE.action?.Enable(); MoveE.action?.Enable();
        MoveSE.action?.Enable(); MoveS.action?.Enable(); MoveSW.action?.Enable();
        MoveW.action?.Enable(); MoveNW.action?.Enable();
        RotCCW.action?.Enable(); RotCW.action?.Enable(); WaitKey.action?.Enable();
        SpecialKey.action?.Enable(); BattleKey.action?.Enable();
    }

    public EActionType GetCurrentAction()
    {
        if( NE ) return EActionType.MOVE_NE;
        if( NW ) return EActionType.MOVE_NW;
        if( SE ) return EActionType.MOVE_SE;
        if( SW ) return EActionType.MOVE_SW;
        if( N ) return EActionType.MOVE_N;
        if( S ) return EActionType.MOVE_S;
        if( E ) return EActionType.MOVE_E;
        if( W ) return EActionType.MOVE_W;
        if( CCW ) return EActionType.ROTATE_CCW;
        if( CW ) return EActionType.ROTATE_CW;
        return EActionType.NONE;
    }
}