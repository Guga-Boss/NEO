using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class StringHelper  
{
	public static string ReplaceFirst(this string p_text, string p_oldText, string p_newText)
	{
		try
		{
			p_oldText = p_oldText != null? p_oldText : "";
			p_newText = p_newText != null? p_newText : "";
			p_text = p_text != null? p_text : "";

			int v_pos = string.IsNullOrEmpty(p_oldText)? -1 : p_text.IndexOf(p_oldText);
			if (v_pos < 0)
				return p_text;
			string v_result = p_text.Substring(0, v_pos) + p_newText + p_text.Substring(v_pos + p_oldText.Length);
			return v_result;
		}
		catch{}
		return p_text;
	}

	public static string ReplaceLast(this string p_text, string p_oldText, string p_newText)
	{
		try
		{
			p_oldText = p_oldText != null? p_oldText : "";
			p_newText = p_newText != null? p_newText : "";
			p_text = p_text != null? p_text : "";

			int v_pos = string.IsNullOrEmpty(p_oldText)? -1 : p_text.LastIndexOf(p_oldText);
			if (v_pos < 0)
				return p_text;
			
			string v_result = p_text.Remove(v_pos, p_oldText.Length).Insert(v_pos, p_newText);
			return v_result;
		}
		catch{}
		return p_text;
	}

	public static bool IsUrl(string p_string)
	{
		if(!string.IsNullOrEmpty(p_string))
		{
			string v_lowerString = p_string.ToLower();
			if(v_lowerString.Contains("http://") || v_lowerString.Contains("https://"))
				return true;
		}
		return false;
	}
}
