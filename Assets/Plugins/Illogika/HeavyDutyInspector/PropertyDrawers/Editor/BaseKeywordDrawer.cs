using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;

namespace HeavyDutyInspector
{

public class BaseKeywordDrawer : IllogikaDrawer {

    private static Texture2D olPlus
    {
        get;
        set;
    }

    private static Texture2D olMinus
    {
        get;
        set;
    }

	protected KeywordsConfig scriptableConfig;
	protected List<KeywordCategory> config;

	private List<string> categories = new List<string> ();
	private List<string> keywords = new List<string>();

	string newValue;

	int currentCategory;

	protected void Init()
	{
        olPlus = (Texture2D)Resources.Load("OlPlusGreen");
        olMinus = (Texture2D)Resources.Load("OLMinusRed");

		PopulateLists();
	}

	protected void PopulateLists()
	{
		categories.Clear();
		keywords.Clear();

		foreach (KeywordCategory category in config)
		{
			if (!string.IsNullOrEmpty(category.name))
				categories.Add(category.name);

			foreach (string keyword in category.keywords)
			{
				if (!string.IsNullOrEmpty(keyword))
					keywords.Add(category.name + (string.IsNullOrEmpty(category.name) ? "" : "/") + keyword);
			}
		}

		categories.Sort();
		categories.Insert(0, "None");

		keywords.Sort();
		keywords.Insert(0, "None");
	}

	public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
	{
		return base.GetPropertyHeight(prop, label) * (isAddingString ? 2 : 1);
	}

	bool isAddingString;

	public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
	{
		string keyword = prop.FindPropertyRelative("_key").stringValue;

		EditorGUI.BeginProperty(position, label, prop);
		
		position = EditorGUI.PrefixLabel(position, EditorGUIUtility.GetControlID(FocusType.Passive), label);
		
		int originalIndentLevel = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;
		
		position.width -= 32;
		
		string temp;

		if(isAddingString)
		{
			position.height = base.GetPropertyHeight(prop, label);

			currentCategory = EditorGUI.Popup(position, currentCategory, categories.ToArray());

			position.y += position.height;

			EditorGUI.BeginChangeCheck();
			
			temp = EditorGUI.TextField(position, newValue);
			
			if(EditorGUI.EndChangeCheck())
			{
				newValue = temp;
			}
		}
		else
		{
			temp =  keyword;

			if(temp == "")
				temp = "None";

			Color originalColor = GUI.color;
			int index = keywords.IndexOf(temp);
			
			if(index < 0)
			{
				index = keywords.Count;
				keywords.Add(temp + " (Missing)");
				GUI.color = Color.red;
			}
			
			
			EditorGUI.BeginChangeCheck();
			

			index = EditorGUI.Popup(position, index, keywords.ToArray());

			temp = keywords[index];

			if(temp == "None")
				temp = "";

			GUI.color = originalColor;
			
			if(EditorGUI.EndChangeCheck())
			{
				prop.FindPropertyRelative("_key").stringValue = temp;
			}
		}

        position.y += 1;
		position.x += position.width;
		position.width = 16;
		
		if(GUI.Button(position, olPlus, "Label"))
		{
			if (temp.Contains(" (Missing)"))
			{
				KeywordCategory tempCategory = (from c in config where c.name == (keyword.LastIndexOf('/') < 0 ? "" : keyword.Substring(0, keyword.LastIndexOf('/'))) select c).FirstOrDefault();

				if(tempCategory == null)
				{
					config.Add(new KeywordCategory((keyword.LastIndexOf('/') < 0 ? "" : keyword.Substring(0, keyword.LastIndexOf('/')))));
					config.Last().keywords.Add(keyword);
				}
				else
				{
					tempCategory.keywords.Add(keyword);
				}
				EditorUtility.SetDirty(scriptableConfig);
				PopulateLists();
			}
			else
			{
				if (isAddingString)
				{
					config[currentCategory].keywords.Add(newValue);
					EditorUtility.SetDirty(scriptableConfig);

					keywords.Add(config[currentCategory].name + (currentCategory == 0 ? "" : "/") + newValue);

					config[currentCategory].keywords.Sort();
					keywords.RemoveAt(0);
					keywords.Sort();
					keywords.Insert(0, "None");

					SetReflectedFieldRecursively(prop, (Keyword)(config[currentCategory].name + (currentCategory == 0 ? "" : "/") + newValue));

					EditorUtility.SetDirty(prop.serializedObject.targetObject);
				}

				isAddingString = !isAddingString;
			}
		}
		
		position.x += 16;

		if(GUI.Button(position, olMinus, "Label"))
		{
			if(isAddingString)
			{
				newValue = "";
				isAddingString = false;
			}
			else
			{
				if(EditorUtility.DisplayDialog("Remove string?", string.Format("Are you sure you want to remove {0} from the string list?", temp), "Yes", "No"))
				{
					keywords.Remove(temp);

					if(temp.Contains('/'))
						(from c in config where c.name == temp.Substring(0, temp.LastIndexOf('/')) select c.keywords).ToList().FirstOrDefault().Remove(keyword);
					else
						config[0].keywords.Remove(keyword);

					EditorUtility.SetDirty(Keywords.Config);
				}
			}
		}

		EditorGUI.indentLevel = originalIndentLevel;
		
		EditorGUI.EndProperty();
	}
}

[CustomPropertyDrawer(typeof(Keyword))]
public class KeywordDrawer : BaseKeywordDrawer
{

	public KeywordDrawer()
	{
		scriptableConfig = Keywords.Config;
		config = scriptableConfig.keyWordCategories;

		base.Init();
	}
}

#if !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3

	public static partial class EditorGUIEx
	{
		private static Texture2D olPlus
		{
			get;
			set;
		}

		private static Texture2D olMinus
		{
			get;
			set;
		}

		private static List<KeywordCategory> config;

		private static List<string> categories = new List<string>();
		private static List<string> keywords = new List<string>();

		static string newValue;

		static int currentCategory;

		public static bool IsAddingKeyword
		{
			get { return isAddingString; }
		}

		static bool isAddingString;

		static void Init()
		{
			olPlus = (Texture2D)Resources.Load("OlPlusGreen");
			olMinus = (Texture2D)Resources.Load("OLMinusRed");

			PopulateLists();
		}

		static void PopulateLists()
		{
			categories.Clear();
			keywords.Clear();

			foreach (KeywordCategory category in config)
			{
				if (!string.IsNullOrEmpty(category.name))
					categories.Add(category.name);

				foreach (string keyword in category.keywords)
				{
					if (!string.IsNullOrEmpty(keyword))
						keywords.Add(category.name + (string.IsNullOrEmpty(category.name) ? "" : "/") + keyword);
				}
			}

			categories.Sort();
			categories.Insert(0, "None");

			keywords.Sort();
			keywords.Insert(0, "None");
		}

		public static string KeywordField(Rect position, string label, string keyword, KeywordsConfig keywordsConfig)
		{
			return KeywordField(position, label, (Keyword)keyword, keywordsConfig).fullPath;
		}

		public static Keyword KeywordField(Rect position, string label, Keyword keyword, KeywordsConfig keywordsConfig)
		{
			config = keywordsConfig.keyWordCategories;
			Init();

			position = EditorGUI.PrefixLabel(position, EditorGUIUtility.GetControlID(FocusType.Passive), new GUIContent(label));

			int originalIndentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			position.width -= 32;

			string temp = "";

			if (isAddingString)
			{
				position.height = EditorGUIUtility.singleLineHeight;

				currentCategory = EditorGUI.Popup(position, currentCategory, categories.ToArray());

				position.y += position.height;

				EditorGUI.BeginChangeCheck();

				temp = EditorGUI.TextField(position, newValue);

				if (EditorGUI.EndChangeCheck())
				{
					newValue = temp;
				}
			}
			else
			{
				temp = keyword.fullPath;

				if (temp == "")
					temp = "None";

				Color originalColor = GUI.color;
				int index = keywords.IndexOf(temp);

				if (index < 0)
				{
					index = keywords.Count;
					keywords.Add(temp + " (Missing)");
					GUI.color = Color.red;
				}


				EditorGUI.BeginChangeCheck();


				index = EditorGUI.Popup(position, index, keywords.ToArray());

				temp = keywords[index];

				if (temp == "None")
					temp = "";

				GUI.color = originalColor;
			}

			position.y += 1;
			position.x += position.width;
			position.width = 16;

			if (GUI.Button(position, olPlus, "Label"))
			{
				if (temp.Contains(" (Missing)"))
				{
					KeywordCategory tempCategory = (from c in config where c.name == keyword.category select c).FirstOrDefault();

					if (tempCategory == null)
					{
						config.Add(new KeywordCategory(keyword.category));
						config.Last().keywords.Add(keyword);
					}
					else
					{
						tempCategory.keywords.Add(keyword);
					}
					EditorUtility.SetDirty(keywordsConfig);
					PopulateLists();
					Debug.Log("Missing");
				}
				else
				{
					if (isAddingString)
					{
						config[currentCategory].keywords.Add(newValue);
						EditorUtility.SetDirty(keywordsConfig);

						keywords.Add(config[currentCategory].name + (currentCategory == 0 ? "" : "/") + newValue);

						config[currentCategory].keywords.Sort();
						keywords.RemoveAt(0);
						keywords.Sort();
						keywords.Insert(0, "None");

						temp = (Keyword)(config[currentCategory].name + (currentCategory == 0 ? "" : "/") + newValue);
					}

					isAddingString = !isAddingString;
				}
			}

			position.x += 16;

			if (GUI.Button(position, olMinus, "Label"))
			{
				if (isAddingString)
				{
					newValue = "";
					isAddingString = false;
				}
				else
				{
					if (EditorUtility.DisplayDialog("Remove string?", string.Format("Are you sure you want to remove {0} from the string list?", temp), "Yes", "No"))
					{
						keywords.Remove(temp);

						if (temp.Contains('/'))
							(from c in config where c.name == temp.Substring(0, temp.LastIndexOf('/')) select c.keywords).ToList().FirstOrDefault().Remove(keyword);
						else
							config[0].keywords.Remove(keyword);

						EditorUtility.SetDirty(keywordsConfig);
					}
				}
			}

			if (temp != null && temp.Contains(" (Missing)"))
			{
				temp = temp.Replace(" (Missing)", "");
			}

			EditorGUI.indentLevel = originalIndentLevel;

			return temp;
		}

	}

	public static partial class EditorGUILayoutEx
	{
		public static string KeywordField(string label, string keyword, KeywordsConfig keywordsConfig)
		{
			return KeywordField(label, (Keyword)keyword, keywordsConfig).fullPath;

		}

		public static Keyword KeywordField(string label, Keyword keyword, KeywordsConfig keywordsConfig)
		{
			EditorGUILayout.LabelField("");
			Rect position = GUILayoutUtility.GetLastRect();

			Keyword temp = EditorGUIEx.KeywordField(position, label, keyword, keywordsConfig);

			if (EditorGUIEx.IsAddingKeyword)
				EditorGUILayout.LabelField("");

			return temp;
		}
	}

#endif
}
