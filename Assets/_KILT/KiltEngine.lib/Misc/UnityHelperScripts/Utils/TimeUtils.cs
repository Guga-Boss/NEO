using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class TimeUtils {

	#region Private Variables

	static bool _needGetNistTime = true;
	static System.DateTime _initialTimeSinceProgramStart = default(System.DateTime);
	static System.DateTime _systemDateTimeInStartUp = System.DateTime.Now;

	#endregion

	#region Time Functions

	//Dont Use Nist time, only internal time
	public static System.DateTime GetSystemTimeNow()
	{
		float v_timeSinceStart = Time.realtimeSinceStartup;
		#if UNITY_EDITOR
		_systemDateTimeInStartUp = System.DateTime.Now.AddSeconds(-v_timeSinceStart); //Just to prevent bugs in editor time!
		#endif
		System.DateTime v_return = _systemDateTimeInStartUp.AddSeconds(v_timeSinceStart);
		return v_return;
	}

	//Use NistTime
	public static System.DateTime GetTimeNow()
	{
		float v_timeSinceStart = Time.realtimeSinceStartup;
		System.DateTime v_return = default(System.DateTime);
		if(_needGetNistTime)
		{
			_needGetNistTime = false;
			v_return = TimeUtils.GetNistTime();
			_initialTimeSinceProgramStart = v_return.AddSeconds(-v_timeSinceStart);
		}
		else
		{
			v_return = _initialTimeSinceProgramStart.AddSeconds(v_timeSinceStart);
		}
		return v_return;
	}
	
	//Pick time from internet.... if cant, pick time from system (NOT SUPPORTED IN WP8 SO REVERTED TO RETURN ONLY SYSTEM TIME)
	public static System.DateTime GetNistTime()
	{
		System.DateTime v_dateTime = System.DateTime.Now;
		//System.DateTime v_dateTime = default(System.DateTime);
		//try
		//{
		//	System.Net.HttpWebRequest v_request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create("http://nist.time.gov/actualtime.cgi?lzbc=siqm9b");
		//	v_request.Method = "GET";
		//	v_request.Accept = "text/html, application/xhtml+xml, */*";
		//	v_request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
		//	v_request.ContentType = "application/x-www-form-urlencoded";
		//	v_request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore); //No caching
		//	System.Net.HttpWebResponse v_response = (System.Net.HttpWebResponse)v_request.GetResponse();
		//	if (v_response.StatusCode == System.Net.HttpStatusCode.OK)
		//	{
		//		System.IO.StreamReader v_stream = new System.IO.StreamReader(v_response.GetResponseStream());
		//		string v_html = v_stream.ReadToEnd();//<timestamp time=\"1395772696469995\" delay=\"1395772696469995\"/>
		//		string v_time = System.Text.RegularExpressions.Regex.Match(v_html, @"(?<=\btime="")[^""]*").Value;
		//		double v_milliseconds = System.Convert.ToInt64(v_time) / 1000.0;
		//		v_dateTime = new System.DateTime(1970, 1, 1).AddMilliseconds(v_milliseconds).ToLocalTime();
		//	}
		//	else
		//		v_dateTime = TimeUtils.GetSystemTimeNow();
		//}
		//catch
		//{
		//	v_dateTime = TimeUtils.GetSystemTimeNow();
		//}
		
		return v_dateTime;
	}

	#endregion

	#region Unity Editor Callbacks
	
	#if UNITY_EDITOR
	
	[UnityEditor.Callbacks.PostProcessBuild]
	static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) 
	{
		OnPostProcessScene(); 
	}

	[UnityEditor.Callbacks.DidReloadScripts]
	static void OnDidReloadScripts() 
	{
		OnPostProcessScene();
	}
	
	[UnityEditor.Callbacks.PostProcessScene]
	static void OnPostProcessScene() // Prevent bugs in editor Time
	{
		_needGetNistTime = true;
	}
	
	#endif
	
	#endregion
}
