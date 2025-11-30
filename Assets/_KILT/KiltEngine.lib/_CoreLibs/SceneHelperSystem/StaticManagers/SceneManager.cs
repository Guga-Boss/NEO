using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Kilt.Core
{
	/// <summary>
	/// A context that is always available in the game. It provides global game features and information.
	/// </summary>
	public class SceneManager : MonoBehaviour
	{
		#region Static Events

		public static Delegates.EventHandler<string, string> OnFadeBegin; //Param: Old Level / New Level
		public static Delegates.EventHandler<string, string> OnFadeEnd; //Param: Old Level / New Level
		public static Delegates.EventHandler<string, string> OnBeforeCallLevelLoad; //Param: Old Level / New Level
		public static Delegates.EventHandler<string, string> OnLevelLoaded; //Param: Old Level / New Level
		public static Delegates.EventHandler<string, string> OnAfterCallLevelLoad; //Param: Old Level / New Level

		public static Delegates.EventHandler<string, object> OnParameterAdded; //Param: string p_parameterName, object p_parameterValue
		
		#endregion

		#region Consts
		
		private const string LEVEL_FILE_NAME = "LevelInformation.txt" ;

		#endregion

		#region Static Properties

		private static bool _canUpdateNames = true;
		protected static bool CanUpdateNames
		{
			get
			{
				return _canUpdateNames;
			}
			set
			{
				if(_canUpdateNames == value)
					return;
				_canUpdateNames = value;
			}
		}

		private static string[] _scenesNames = null;
		public static string[] ScenesNames
		{
			get
			{
				if(_scenesNames == null)
					UpdateLevelNames();
				return _scenesNames;
			}
		}
		
		static string _oldLevelName = "";
		public static string OldLevelName
		{
			get
			{
				return _oldLevelName;
			}
			set
			{
				if(_oldLevelName == value)
					return;
				_oldLevelName = value;
			}
		}
		
		static string _currentLevelName = "";
		public static string CurrentLevelName
		{
			get
			{
				#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_4
				if(!IsLoading() && string.Equals(_currentLevelName, Application.loadedLevelName))
					_currentLevelName = Application.loadedLevelName;
				#else
				if(!IsLoading() && string.Equals(_currentLevelName, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))
					_currentLevelName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
				#endif
				return _currentLevelName;
			}
			set
			{
				if(_currentLevelName == value)
					return;
				_currentLevelName = value;
			}
		}
		
		//Change this Guy Before Change Scene to pass new this parameters to nextScene
		static SceneParameters _parametersToPassToNextScene = new SceneParameters();
		public static SceneParameters ParametersToPassToNextScene
		{
			get
			{
				if(_parametersToPassToNextScene == null)
					_parametersToPassToNextScene = new SceneParameters();
				return _parametersToPassToNextScene;
			}
			set
			{
				if(_parametersToPassToNextScene == value)
					return;
				_parametersToPassToNextScene = value;
			}
		}
		
		static SceneParameters _currentSceneParameters = new SceneParameters();
	 	static SceneParameters CurrentSceneParameters
		{
			get
			{
				if(_currentSceneParameters == null)
					_currentSceneParameters = new SceneParameters();
				return _currentSceneParameters;
			}
			set
			{
				if(_currentSceneParameters == value)
					return;
				_currentSceneParameters = value;
			}
		}

		#endregion

		#region Static Constructor

		static SceneManager()
		{
			UpdateLevelNames();
		}

		#endregion

		#region Unity Functions(Dummy)
		
		//Use this functions to update Parameters to next scene when level was loaded
		protected void Awake()
		{
			DontDestroyOnLoad(this.gameObject);
			gameObject.hideFlags = HideFlags.DontSaveInBuild|HideFlags.DontSaveInEditor;
		}
		
		//Use this functions to update Parameters to next scene when level was loaded
		protected void OnLevelWasLoaded(int level) 
		{
			Screen.SetResolution(Screen.width, Screen.height, Screen.fullScreen);
			ApplyNextParametersInCurrentParamenters();
			if(SceneManager.OnLevelLoaded != null)
				SceneManager.OnLevelLoaded(SceneManager.OldLevelName, SceneManager.CurrentLevelName);
			KiltUtils.Destroy(this.gameObject);
		}
		
		protected void OnDestroy() 
		{
			CanUpdateNames = true;
			_isLoading = false;
		}
		
		#endregion


		#region Static Functions

		#region Scene Paramenters Functions
		
		public static SceneParameters GetCurrentSceneParameters()
		{
			if(_currentSceneParameters == null)
				_currentSceneParameters = new SceneParameters();
			return _currentSceneParameters;
		}

		private static void ApplyNextParametersInCurrentParamenters() 
		{
			CurrentSceneParameters =  new SceneParameters();
			if(ParametersToPassToNextScene != null)
			{
				CurrentSceneParameters.Message = ParametersToPassToNextScene.Message;
				foreach(AOTKeyValuePair<string, object> v_pair in ParametersToPassToNextScene.GetAllParameters())
				{
					if(v_pair != null)
					{
						AddParameter(v_pair.Key, v_pair.Value, false, true);
					}
				}
			}
			ParametersToPassToNextScene = null;
		}
		
		public static void AddParameter(GenericSceneParameter p_parameter)
		{
			if(p_parameter != null)
			{
				//bool p_addInThisScene = p_parameter == 
				bool p_addInThisScene = p_parameter.WhenAdd == WhenAddParameterEnum.AlwaysAdd || p_parameter.WhenAdd == WhenAddParameterEnum.AddInCurrentScene;
				bool p_addInNextScene = p_parameter.WhenAdd == WhenAddParameterEnum.AlwaysAdd || p_parameter.WhenAdd == WhenAddParameterEnum.AddInNextScene;
				if(p_addInNextScene)
					AddParameter(p_parameter.ParameterName, p_parameter.ObjectParameterValue, true, p_parameter.ReplaceOldParameterWithSameName);
				if(p_addInThisScene)
					AddParameter(p_parameter.ParameterName, p_parameter.ObjectParameterValue, false, p_parameter.ReplaceOldParameterWithSameName);
			}
		}
		
		public static void AddParameter(string p_parameterName, object p_value, bool p_addToNextScene, bool p_replaceParametersWithSameName)
		{
			if(p_addToNextScene)
			{
				ParametersToPassToNextScene.AddParameter(p_parameterName, p_value, p_replaceParametersWithSameName);
			}
			else
			{
				bool p_containParameterWithName = CurrentSceneParameters.ContainsParameterName(p_parameterName);
				CurrentSceneParameters.AddParameter(p_parameterName, p_value, p_replaceParametersWithSameName);
				if(OnParameterAdded != null && (!p_containParameterWithName || p_replaceParametersWithSameName))
				{
					OnParameterAdded(p_parameterName, p_value);
				}
			}
		}
		
		#endregion

		#region LoadLevel Fading

		public static bool LoadLevelFading(string p_levelName, float p_fadeTime = 1.0f)
		{
			return LoadLevelFading(p_levelName, Color.black, p_fadeTime);
		}

		public static bool LoadLevelFading(string p_levelName, Color p_color, float p_fadeTime = 1.0f)
		{
			if(!IsLoading())
			{
				RegisterCameraFadeEvents();
				//_currentSceneParameters = ParametersToPassToNextScene;
				OldLevelName = CurrentLevelName;
				CameraFade.LoadSceneWithFadePingPong(p_color, p_fadeTime, p_levelName);
				CanUpdateNames = false;
				CurrentLevelName = p_levelName;
				if(OnFadeBegin != null)
					OnFadeBegin(OldLevelName , CurrentLevelName);
				return true;
			}
			return false;
		}

		public static bool LoadLevelFading(string p_levelName, SceneParameters p_parameter, Color p_color, float p_fadeTime = 1.0f)
		{
			ParametersToPassToNextScene = p_parameter;
			return LoadLevelFading(p_levelName, p_color, p_fadeTime);
		}

		public static bool LoadLevelFading(int p_index, float p_fadeTime = 1.0f)
		{
			return LoadLevelFading(p_index, Color.black, p_fadeTime);
		}

		public static bool LoadLevelFading(int p_index, Color p_color, float p_fadeTime = 1.0f)
		{
			return LoadLevelFading(GetLevelNameByIndex(p_index), p_color, p_fadeTime);
		}

		public static bool LoadLevelFading(int p_index, SceneParameters p_parameter, Color p_color, float p_fadeTime = 1.0f)
		{
			ParametersToPassToNextScene = p_parameter;
			return LoadLevelFading(p_index, p_color, p_fadeTime);
		}

		#endregion

		#region LoadLevel Immediate

		public static bool LoadLevel(string p_levelName, bool p_force = false)
		{
			bool v_return = false;
			if(Application.CanStreamedLevelBeLoaded(p_levelName))
			{
				if(!IsLoading() || p_force)
				{
					if(CanUpdateNames)
					{
						OldLevelName = CurrentLevelName;
						CurrentLevelName = p_levelName;
						CanUpdateNames = false;
					}
					_isLoading = true;
					CreateDummyObject(); // Parameter to Next Scene will be added by Dummy when 'OnLevelWasLoaded' called
					if(OnBeforeCallLevelLoad != null)
						OnBeforeCallLevelLoad(OldLevelName, p_levelName);
					#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_4
					Application.LoadLevel(p_levelName);
					#else
					UnityEngine.SceneManagement.SceneManager.LoadScene(p_levelName);
					#endif
					if(OnAfterCallLevelLoad != null)
						OnAfterCallLevelLoad(OldLevelName, p_levelName);
					//_isLoading will be setted to false when DummyObject Destroyed
					//_isLoading = false;

					v_return = true;
				}
			}
			else if(!string.IsNullOrEmpty(p_levelName))
				Debug.LogWarning("Scene " + p_levelName + " must be in build. Go to File->Build Settings and add this scene.");
			else
				Debug.LogWarning("Scene name is empty.");
			return v_return;
		}

		public static bool LoadLevel(string p_levelName, SceneParameters p_parameter, bool p_force = false)
		{
			ParametersToPassToNextScene = p_parameter;
			return LoadLevel(p_levelName, p_force);
		}
		
		public static bool LoadLevel(int p_index, bool p_force = false)
		{
			return LoadLevel(GetLevelNameByIndex(p_index), p_force);
		}

		public static bool LoadLevel(int p_index, SceneParameters p_parameter, bool p_force = false)
		{
			ParametersToPassToNextScene = p_parameter;
			return LoadLevel(p_index, p_force);
		}

		#endregion

		#region Camera Fade Event

		public static void HandleOnFinishFade()
		{
			UnregisterCameraFadeEvents();
			if(OnFadeEnd != null)
				OnFadeEnd(OldLevelName , CurrentLevelName);
		}

		#endregion

		#region Camera Fade Helper

		private static void UnregisterCameraFadeEvents()
		{
			CameraFade.OnFinishFade -= HandleOnFinishFade;
		}

		private static void RegisterCameraFadeEvents()
		{
			UnregisterCameraFadeEvents();
			CameraFade.OnFinishFade += HandleOnFinishFade;
		}

		#endregion

		#region Helper Functions

		protected static GameObject CreateDummyObject()
		{
			GameObject v_object = new GameObject();
			v_object.AddComponent<SceneManager>();
			v_object.name = "SceneManager_Dummy";
			return v_object;
		}

		static bool _isLoading = false;
		public static bool IsLoading()
		{
			if(CameraFade.IsFading())
				return true;
			return _isLoading;
		}

		public static string GetLevelNameByIndex(int p_index)
		{
			string v_name = "";
			if(ScenesNames != null && ScenesNames.Length > p_index && p_index >=0)
			{
				v_name = ScenesNames[p_index];
			}
			return v_name;
		}

		public static void UpdateLevelNames()
		{
			_scenesNames = ReadLevelsFromFile();
		}

		#endregion

		#region Read/Write
		
		public static string[] ReadLevelsFromFile()
		{	
			List<string> levelNames = SerializerHelper.Deserialize<List<string>>("", LEVEL_FILE_NAME, true, SerializationTypeEnum.JSON);
			if(levelNames == null)
				levelNames = new List<string>();
			return levelNames.ToArray();
		}

		public static void WriteLevelsToFile()
		{
			#if UNITY_EDITOR
			//Debug.Log(string.Format("Writing levels file to '{0}'.", LEVEL_FILE_NAME));
			List<string> v_temp = new List<string>();
			foreach (UnityEditor.EditorBuildSettingsScene v_sceneBuild in UnityEditor.EditorBuildSettings.scenes)
			{
				if (v_sceneBuild.enabled)
				{
					string v_name = v_sceneBuild.path.Substring(v_sceneBuild.path.LastIndexOf('/')+1);
					v_name = v_name.Substring(0,v_name.Length-6);
					v_temp.Add(v_name);
				}
			}
			SerializerHelper.Serialize(v_temp, "", LEVEL_FILE_NAME, true, SerializationTypeEnum.JSON);
			#endif
			UpdateLevelNames();
		}

		#endregion

		#region Post Processing

		#if UNITY_EDITOR
		[UnityEditor.Callbacks.PostProcessBuild]
		private static void PostProcessBuildFunction(UnityEditor.BuildTarget p_target, string p_path)
		{
			WriteLevelsToFile();
		}
		
		[UnityEditor.Callbacks.PostProcessScene]
		private static void PostProcessSceneFunctions()
		{
			if(!Application.isPlaying)
				WriteLevelsToFile();
		}

		[UnityEditor.Callbacks.DidReloadScripts]
		private static void PostProcessReloadScriptsFunctions()
		{
			WriteLevelsToFile();
		}
		
		#endif

		#endregion

		#endregion
	}

	[System.Serializable]
	public class SceneParameters
	{
		#region Private Variables

		[SerializeField]
		string m_message = "";
		[SerializeField]
		AOTDictionaryKV<string, object> m_parameters = new AOTDictionaryKV<string, object>();

		#endregion

		#region Public Properties

		public string Message
		{
			get
			{
				if(m_message == null)
					m_message = "";
				return m_message;
			}
			set
			{
				if(m_message == value)
					return;
				m_message = value;
			}
		}

		#endregion

		#region Protected Properties

		protected AOTDictionaryKV<string, object> Parameters
		{
			get
			{
				if(m_parameters == null)
					m_parameters = new AOTDictionaryKV<string, object>();
				return m_parameters;
			}
		}

		#endregion

		#region Parameters Functions

		public bool AddParameter(string p_parameterName, object p_value, bool p_replaceOldValues = true)
		{
			if(p_replaceOldValues)
			{
				Parameters.AddReplacing(p_parameterName, p_value);
				return true;
			}
			else if(!Parameters.ContainsKey(p_parameterName))
			{
				Parameters.Add(p_parameterName, p_value);
				return true;
			}
			return false;
		}

		public bool RemoveParameterByName(string p_parameterName)
		{
			if(Parameters.ContainsKey(p_parameterName))
			{
				Parameters.RemoveByKey(p_parameterName);
				return true;
			}
			return false;
		}

		public bool ContainsParameterName(string p_parameterName)
		{
			if(Parameters.ContainsKey(p_parameterName))
				return true;
			return false;
		}

		public object GetParameterValue(string p_parameterName)
		{
			object v_value = Parameters.GetValueByKey(p_parameterName);
			return v_value;
		}

		public AOTDictionaryKV<string, object> GetAllParameters()
		{
			AOTDictionaryKV<string, object> v_dict = Parameters.CloneDict() as AOTDictionaryKV<string, object>;
			return v_dict;
		}

		public AOTDictionaryKV<string, T> GetAllParametersWithValueType<T>()
		{
			AOTDictionaryKV<string, T> v_dict = new AOTDictionaryKV<string, T>();
			foreach(AOTKeyValuePair<string, object> v_pair in Parameters)
			{
				if(v_pair != null)
				{
					if(System.Object.ReferenceEquals(v_pair.Value, null))
						v_dict.Add(v_pair.Key, default(T));
					else if(v_pair.Value is T)
						v_dict.Add(v_pair.Key, (T)v_pair.Value);
				}
			}
			return v_dict;
		}

		public void ClearParameters()
		{
			Parameters.Clear();
		}

		#endregion
	}
}
