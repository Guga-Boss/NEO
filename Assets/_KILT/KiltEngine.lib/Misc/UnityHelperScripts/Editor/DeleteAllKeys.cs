using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public static class DeleteAllKeys
{
	[MenuItem("KILT/Helper/Others/Delete All Player Prefs Keys")]
	public static void DeleteAllPlayerPrefsKeys()
	{
		PlayerPrefs.DeleteAll();
	}
}
