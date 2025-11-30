using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioCaller))]
public class AudioCallerInspector : Editor 
{
	public override void OnInspectorGUI ()
	{
		AudioCaller v_controller = target as AudioCaller;

		v_controller.Clip = EditorGUILayout.ObjectField("Clip", v_controller.Clip, typeof(AudioClip), false) as AudioClip;

		v_controller.SoundPlayerOption = (SoundTriggerEnum)EditorGUILayout.EnumPopup("Sound Player Option", v_controller.SoundPlayerOption);
		v_controller.SoundStopperOption = (SoundTriggerEnum)EditorGUILayout.EnumPopup("Sound Stopper Option", v_controller.SoundStopperOption);
		v_controller.CanPlayInFade = EditorGUILayout.Toggle("Can Play In Fade", v_controller.CanPlayInFade);
		v_controller.IsBackgroundMusic = EditorGUILayout.Toggle("Is Background Music", v_controller.IsBackgroundMusic);
		if(!v_controller.IsBackgroundMusic)
		{
			v_controller.Loop = EditorGUILayout.Toggle("Loop", v_controller.Loop);
			v_controller.OneSoundOfThisTypeOnly = EditorGUILayout.Toggle("One Sound Of This Type Only", v_controller.OneSoundOfThisTypeOnly);
			v_controller.SpartialBlend = EditorGUILayout.Slider("SpartialBlend", v_controller.SpartialBlend, 0, 1);
		}


		if(GUI.changed)
			EditorUtility.SetDirty(v_controller);
	}
}
