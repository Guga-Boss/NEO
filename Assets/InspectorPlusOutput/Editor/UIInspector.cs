using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using System;
using Object = UnityEngine.Object;
using InspectorPlusVar = UIInspector.InspectorPlusVar;

[CanEditMultipleObjects]
[CustomEditor(typeof(UI))]
public class UIInspector : Editor {
    public class InspectorPlusVar
    {
        public enum LimitType { None, Max, Min, Range };
        public enum VectorDrawType { None, Direction, Point, PositionHandle, Scale, Rotation };
        public LimitType limitType = LimitType.None;
        public float min = -0.0f;
        public float max = -0.0f;
        public bool progressBar;
        public int iMin = -0;
        public int iMax = -0;
        public bool active = true;
        public string type;
        public string name;
        public string dispName;
        public VectorDrawType vectorDrawType;
        public bool relative = false;
        public bool scale = false;
        public float space = 0.0f;
        public bool[] labelEnabled = new bool[4];
        public string[] label = new string[4];
        public bool[] labelBold = new bool[4];
        public bool[] labelItalic = new bool[4];
        public int[] labelAlign = new int[4];
        public bool[] buttonEnabled = new bool[16];
        public string[] buttonText = new string[16];
        public string[] buttonCallback = new string[16];
        public bool[] buttonCondense = new bool[4];

        public int numSpace = 0;
        public string classType;
        public Vector3 offset = new Vector3(0.5f, 0.5f);
        public bool QuaternionHandle;
	    public bool canWrite = true;
	    public string tooltip;
	    public bool hasTooltip = false;
        public bool toggleStart = false;
        public int toggleSize = 0;
        public int toggleLevel = 0;
        public bool largeTexture;
        public float textureSize;

		public string textFieldDefault;
		public bool textArea;

    public InspectorPlusVar(LimitType _limitType, float _min, float _max, bool _progressBar, int _iMin, int _iMax, bool _active, string _type, string _name, string _dispName,
                        VectorDrawType _vectorDrawType, bool _relative, bool _scale, float _space, bool[] _labelEnabled, string[] _label, bool[] _labelBold, bool[] _labelItalic, int[] _labelAlign, bool[] _buttonEnabled, string[] _buttonText,
                        string[] _buttonCallback, bool[] buttonCondense, int _numSpace, string _classType, Vector3 _offset, bool _QuaternionHandle, bool _canWrite, string _tooltip, bool _hasTooltip,
                        bool _toggleStart, int _toggleSize, int _toggleLevel, bool _largeTexture, float _textureSize, string _textFieldDefault, bool _textArea)
    {
        limitType = _limitType;
        min = _min;
        max = _max;
        progressBar = _progressBar;
        iMax = _iMax;
        iMin = _iMin;
        active = _active;
        type = _type;
        name = _name;
        dispName = _dispName;
        vectorDrawType = _vectorDrawType;
        relative = _relative;
        scale = _scale;
        space = _space;
        labelEnabled = _labelEnabled;
        label = _label;
        labelBold = _labelBold;
        labelItalic = _labelItalic;
        labelAlign = _labelAlign;
        buttonEnabled = _buttonEnabled;
        buttonText = _buttonText;
        buttonCallback = _buttonCallback;
        numSpace = _numSpace;
        classType = _classType;
        offset = _offset;
        QuaternionHandle = _QuaternionHandle;
        canWrite = _canWrite;
        tooltip = _tooltip;
        hasTooltip = _hasTooltip;
        toggleStart = _toggleStart;
        toggleSize = _toggleSize;
        toggleLevel = _toggleLevel;
        largeTexture = _largeTexture;
        textureSize = _textureSize;
		textFieldDefault = _textFieldDefault;
		textArea = _textArea;
    }
    }	
    SerializedObject so;
	SerializedProperty[] properties;
	new string name;
    string dispName;
	Rect tooltipRect;	
	List<InspectorPlusVar> vars;
	void RefreshVars(){for (int i = 0; i < vars.Count; i += 1) properties[i] = so.FindProperty (vars[i].name);}
	void OnEnable ()
	{
        vars = new List<InspectorPlusVar>();
        so = serializedObject;
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"tk2dTextMesh","HeroHpText","Hero Hp Text",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"tk2dTextMesh","MonstersHpText","Monsters Hp Text",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"tk2dTextMesh","LevelText","Level Text",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"tk2dTextMesh","EnterAreaTxt","Enter Area Txt",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"tk2dTextMesh","AreasText","Areas Text",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"tk2dTextMesh","ArtifactsText","Artifacts Text",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","ScoutLabel","Scout Label",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","TurnInfoLabel","Turn Info Label",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","MapLabel","Map Label",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","BlueRuneCountLabel","Blue Rune Count Label",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","RedRuneCountLabel","Red Rune Count Label",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","GreenRuneCountLabel","Green Rune Count Label",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","UseButtonLabel","Use Button Label",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","LosttHPLabel","Lostt H P Label",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","BigTextHelpLabel","Big Text Help Label",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","BigMessageText","Big Message Text",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","ArtifactInfoLabel","Artifact Info Label",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","FlashBackLabel","Flash Back Label",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"tk2dTextMesh","GameLevelText","Game Level Text",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"EnergyBar","MonstersHpBar","Monsters Hp Bar",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UI2DSprite","HorseIcon","Horse Icon",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UI2DSprite","MessageBoxIcon","Message Box Icon",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UI2DSprite","MessageBoxIcon2","Message Box Icon2",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UI2DSprite","OverlaySprite","Overlay Sprite",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"EnergyBar","UnitHealthBar","Unit Health Bar",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UI2DSprite[]","Stars","Stars",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"tk2dSprite","PerkInfoIcon","Perk Info Icon",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"tk2dSprite","PerkInfoIconBack","Perk Info Icon Back",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","PerkInfoTitleText","Perk Info Title Text",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","PerkInfoDescriptionText","Perk Info Description Text",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","DebugLabel","Debug Label",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","MessageBoxTextLabel","Message Box Text Label",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","MessageBoxTextLabel2","Message Box Text Label2",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","MessageBoxLevelLabel","Message Box Level Label",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","PerkInfoLevelLabel","Perk Info Level Label",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UIButton","RepeatButton","Repeat Button",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UIButton","ZoomButton","Zoom Button",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UIButton","FreeCamButton","Free Cam Button",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UIButton","ArtifactSeekButton","Artifact Seek Button",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UIButton","AreaSeekButton","Area Seek Button",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UIButton","GridButton","Grid Button",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UIButton","CompareHeroButton","Compare Hero Button",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UIButton","KadeWaitButton","Kade Wait Button",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UIButton","RestartAreaButton","Restart Area Button",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UIButton","BattleButton","Battle Button",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UIButton","UseAbilityButton","Use Ability Button",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UIButton","PerkInfoButton","Perk Info Button",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","PerkInfoTargetHeroText","Perk Info Target Hero Text",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","ScrollText","Scroll Text",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","FreeCamModeLabel","Free Cam Mode Label",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UILabel","ErrorMessageLabel","Error Message Label",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"List`1","PerkList","Perk List",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"GameObject","Menu","Menu",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"GameObject","HpPanel","Hp Panel",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"GameObject","PerksPanel","Perks Panel",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"GameObject","MessageBox","Message Box",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"GameObject","FarmUI","Farm U I",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"GameObject","PerkModel","Perk Model",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"GameObject","PerkInfoPanel","Perk Info Panel",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"GameObject","PerksFolder","Perks Folder",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Int32","LevelFactor","Level Factor",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"GameObject","SkullImage","Skull Image",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Int32","NumGamePerk","Num Game Perk",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Int32","NumLevelPerk","Num Level Perk",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Int32","NumAreaPerk","Num Area Perk",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Perk","MushroomPerk","Mushroom Perk",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Unit","CompareHero","Compare Hero",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Perk","DestroyBarricadePerk","Destroy Barricade Perk",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UISprite","ScrollBack","Scroll Back",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Perk","MonsterPushPerk","Monster Push Perk",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Perk","MonsterPressurePerk","Monster Pressure Perk",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Single","RKeyPressTimeCount","R Key Press Time Count",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Unit","SelUnit","Sel Unit",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UIGrid","Grid","Grid",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Vector2","LockedTile","Locked Tile",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UI2DSprite[]","PortraitList","Portrait List",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Int32","ActiveCompareHeroID","Active Compare Hero I D",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Int32","ArtifactLevelDifference","Artifact Level Difference",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"tk2dTextMesh","LoadingLevelText","Loading Level Text",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"String","AttackDescription","Attack Description",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"String","AttackType","Attack Type",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"String","AmbushInfo","Ambush Info",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"String","CorneringInfo","Cornering Info",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"String","SurplusInfo","Surplus Info",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"String","SprinterInfo","Sprinter Info",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Single","ShieldReduction","Shield Reduction",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Single","Shield","Shield",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Single","AmbushBonus","Ambush Bonus",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Single","SurplusBonus","Surplus Bonus",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Single","ConeringBonus","Conering Bonus",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Single","SprinterBonus","Sprinter Bonus",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Int32","MessageTurn","Message Turn",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Int32","OriginalLevel","Original Level",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Int32","MouseArtifactMult","Mouse Artifact Mult",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Boolean","ConsiderArtifactLevelDifference","Consider Artifact Level Difference",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,1,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Boolean","ShowMainArtifacts","Show Main Artifacts",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,1,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Boolean","ShowSecondaryArtifacts","Show Secondary Artifacts",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,1,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Boolean","MouseArtifact","Mouse Artifact",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,1,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"MyPathfinding","PathFind","Path Find",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"String[]","OrdinalNumberList","Ordinal Number List",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"GridHelper","GridHelper","Grid Helper",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"EPerkType","CurrentPerkType","Current Perk Type",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Boolean","MouseOverUI","Mouse Over U I",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,1,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Single","BigMessageTextTimeCounter","Big Message Text Time Counter",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"UI2DSprite","InfoIcon","Info Icon",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"Single","TempSelectedPerkTimer","Temp Selected Perk Timer",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"EPerkType","TempSelectedPerk","Temp Selected Perk",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));
        vars.Add(new InspectorPlusVar(InspectorPlusVar.LimitType.None,0,0,false,0,0,true,"EPerkType","SelectedPerk","Selected Perk",InspectorPlusVar.VectorDrawType.None,false,false,0,new System.Boolean[]{false,false,false,false},new System.String[]{"","","",""},new System.Boolean[]{false,false,false,false},new System.Boolean[]{false,false,false,false},new System.Int32[]{0,0,0,0},new System.Boolean[]{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},new System.String[]{"","","","","","","","","","","","","","","",""},new System.String[]{"","","","","","","","","","","","","","","",""},new System.Boolean[]{false,false,false,false},0,"UI",new Vector3(0.5f,0.5f,0f),false,true,"Tooltip",false,false,0,0,false,70,"",false));	
		int count = vars.Count;
		properties = new SerializedProperty[count];
	}
    
	void ProgressBar (float value, string label)
	{
		GUILayout.Space (3.0f);
		Rect rect = GUILayoutUtility.GetRect (18, 18, "TextField");
		EditorGUI.ProgressBar (rect, value, label);
		GUILayout.Space (3.0f);
	}
	void ArrayGUI (SerializedProperty sp, string name)
	{
		EditorGUIUtility.LookLikeControls (120.0f, 40.0f);
		GUILayout.Space (4.0f);
		EditorGUILayout.BeginVertical ("box", GUILayout.MaxWidth(Screen.width));

		int i = 0;
		int del = -1;

		SerializedProperty array = sp.Copy ();
		SerializedProperty size = null;
		bool first = true;

		while (true) {
			if (sp.propertyPath != name && !sp.propertyPath.StartsWith (name + "."))
				break;

			bool child;
            EditorGUI.indentLevel = sp.depth;

			if (sp.depth == 1 && !first) {
				EditorGUILayout.BeginHorizontal ();

				if (GUILayout.Button ("", "OL Minus", GUILayout.Width (24.0f)))
					del = i;

				child = EditorGUILayout.PropertyField (sp);

				GUI.enabled = i > 0;

				if (GUILayout.Button ("U", "ButtonLeft", GUILayout.Width (24.0f), GUILayout.Height(18.0f)))
					array.MoveArrayElement (i - 1, i);

				GUI.enabled = i < array.arraySize - 1;
                if (GUILayout.Button("D", "ButtonRight", GUILayout.Width(24.0f), GUILayout.Height(18.0f)))
					array.MoveArrayElement (i + 1, i);

				++i;

				GUI.enabled = true;
				EditorGUILayout.EndHorizontal ();
			} else if (sp.depth == 1) {
				first = false;
				size = sp.Copy ();

				EditorGUILayout.BeginHorizontal ();

                if (!size.hasMultipleDifferentValues && GUILayout.Button("", "OL Plus", GUILayout.Width(24.0f)))
					array.arraySize += 1;


				child = EditorGUILayout.PropertyField (sp);

				EditorGUILayout.EndHorizontal ();
			} else {
                child = EditorGUILayout.PropertyField(sp);
			}

			if (!sp.NextVisible (child))
				break;
		}

		sp.Reset ();

		if (del != -1)
			array.DeleteArrayElementAtIndex (del);

		if (array.isExpanded && !size.hasMultipleDifferentValues) {
			EditorGUILayout.BeginHorizontal ();

            if (GUILayout.Button("", "OL Plus", GUILayout.Width(24.0f)))
				array.arraySize += 1;

			GUI.enabled = false;
			EditorGUILayout.PropertyField (array.GetArrayElementAtIndex (array.arraySize - 1), new GUIContent ("" + array.arraySize));
			GUI.enabled = true;

			EditorGUILayout.EndHorizontal ();
		}


        EditorGUI.indentLevel = 0;
		EditorGUILayout.EndVertical ();
		EditorGUIUtility.LookLikeControls (170.0f, 80.0f);
	}
	void Vector2Field(SerializedProperty sp)
	{
        EditorGUI.BeginProperty(new Rect(0.0f, 0.0f, 0.0f, 0.0f), new GUIContent(), sp);
		EditorGUI.BeginChangeCheck ();
		var newValue = EditorGUILayout.Vector2Field (dispName, sp.vector2Value);

		if (EditorGUI.EndChangeCheck ())
			sp.vector2Value = newValue;
		
		EditorGUI.EndProperty ();
	}
	void FloatField(SerializedProperty sp, InspectorPlusVar v)
	{
		if (v.limitType == InspectorPlusVar.LimitType.Min && !sp.hasMultipleDifferentValues)
			sp.floatValue = Mathf.Max (v.min, sp.floatValue);
		else if (v.limitType == InspectorPlusVar.LimitType.Max && !sp.hasMultipleDifferentValues)
			sp.floatValue = Mathf.Min (v.max, sp.floatValue);
		
		if (v.limitType == InspectorPlusVar.LimitType.Range) {
			if (!v.progressBar)
				EditorGUILayout.Slider (sp, v.min, v.max);
			else {
				if (!sp.hasMultipleDifferentValues) {
					sp.floatValue = Mathf.Clamp (sp.floatValue, v.min, v.max);
					ProgressBar ((sp.floatValue - v.min) / v.max, dispName);
				} else
					ProgressBar ((sp.floatValue - v.min) / v.max, dispName);
			}
		}
        else EditorGUILayout.PropertyField(sp, new GUIContent(dispName));
	}
	void IntField(SerializedProperty sp, InspectorPlusVar v)
	{
		if (v.limitType == InspectorPlusVar.LimitType.Min && !sp.hasMultipleDifferentValues)
			sp.intValue = Mathf.Max (v.iMin, sp.intValue);
		else if (v.limitType == InspectorPlusVar.LimitType.Max && !sp.hasMultipleDifferentValues)
			sp.intValue = Mathf.Min (v.iMax, sp.intValue);
		
		if (v.limitType == InspectorPlusVar.LimitType.Range)
		{
			if (!v.progressBar)
			{
                EditorGUI.BeginProperty(new Rect(0.0f, 0.0f, 0.0f, 0.0f), new GUIContent(), sp);
				EditorGUI.BeginChangeCheck ();

                var newValue = EditorGUI.IntSlider(GUILayoutUtility.GetRect(18.0f, 18.0f), new GUIContent(dispName), sp.intValue, v.iMin, v.iMax);
				
				if (EditorGUI.EndChangeCheck ())
					sp.intValue = newValue;
				EditorGUI.EndProperty ();
			}
			else {
				if (!sp.hasMultipleDifferentValues) {
					sp.intValue = Mathf.Clamp (sp.intValue, v.iMin, v.iMax);
					ProgressBar ((float)(sp.intValue - v.iMin) / v.iMax, dispName);
				} else
					ProgressBar ((float)(sp.intValue - v.iMin) / v.iMax, dispName);
			}
		}
        else EditorGUILayout.PropertyField(sp, new GUIContent(dispName));
	}
    int BoolField(SerializedProperty sp, InspectorPlusVar v)
    {
        if (v.toggleStart)
        {
            EditorGUI.BeginProperty(new Rect(0.0f, 0.0f, 0.0f, 0.0f), new GUIContent(), sp);

            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.Toggle(dispName, sp.boolValue);
            
            if (EditorGUI.EndChangeCheck())
                sp.boolValue = newValue;
            
            EditorGUI.EndProperty();

            if (!sp.boolValue)
                return v.toggleSize;
        }
        else EditorGUILayout.PropertyField(sp, new GUIContent(dispName));

        return 0;
    }
	void PropertyField (SerializedProperty sp, string name)
	{
		if (sp.hasChildren) {
            GUILayout.BeginVertical();
			while (true) {
				if (sp.propertyPath != name && !sp.propertyPath.StartsWith (name + "."))
					break;

				EditorGUI.indentLevel = sp.depth;
                bool child = false;

                if (sp.depth == 0)
                    child = EditorGUILayout.PropertyField(sp, new GUIContent(dispName));
                else
                    child = EditorGUILayout.PropertyField(sp);

				if (!sp.NextVisible (child))
					break;
			}
            EditorGUI.indentLevel = 0;
            GUILayout.EndVertical();
		} else EditorGUILayout.PropertyField(sp, new GUIContent(dispName));
	}
	public override void OnInspectorGUI ()
	{	
		so.Update ();
		RefreshVars();
		
		EditorGUIUtility.LookLikeControls (135.0f, 50.0f);

		for (int i = 0; i < properties.Length; i += 1) 
		{
			InspectorPlusVar v = vars[i];
			
			if (v.active && properties[i] != null) 
			{
				SerializedProperty sp = properties [i];string s = v.type;
							 bool skip = false;
				name = v.name;
                dispName = v.dispName;

				GUI.enabled = v.canWrite;

                GUILayout.BeginHorizontal();

                if (v.toggleLevel != 0)
                   GUILayout.Space(v.toggleLevel * 10.0f);
                
                if (s == typeof(Vector2).Name){
                    Vector2Field(sp);
                    skip = true;
                }
                if (s == typeof(float).Name){
                    FloatField(sp, v);
                    skip = true;
                }
                if (s == typeof(int).Name){
                    IntField(sp, v);
                    skip = true;
                }
                if (s == typeof(bool).Name){
                    i += BoolField(sp, v);
                    skip = true;
                }
                if (sp.isArray && s != typeof(string).Name){
                    ArrayGUI(sp, name);
                    skip = true;
                }
                if (!skip)
                    PropertyField(sp, name);
                GUILayout.EndHorizontal();
                GUI.enabled = true;
			}
		}
        so.ApplyModifiedProperties (); 
        //NOTE NOTE NOTE: WATERMARK HERE
        //You are free to remove this
        //START REMOVE HERE
        GUILayout.BeginHorizontal();
        GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.3f);
        GUILayout.FlexibleSpace();
        GUILayout.Label("Created with");
        GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.6f);
        if (GUILayout.Button("Inspector++"))
            Application.OpenURL("http://forum.unity3d.com/threads/136727-Inspector-Meh-to-WOW-inspectors");
        GUI.color = new Color(1.0f, 1.0f, 1.0f);
		GUILayout.EndHorizontal();
        //END REMOVE HERE
    }
}