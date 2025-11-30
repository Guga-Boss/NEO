using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public static class SystemIOEditorWebPlayer 
{
	public static void WriteAllText(string p_fullPathWithFileName, string p_textToWrite)
	{
		System.IO.File.WriteAllText(p_fullPathWithFileName, p_textToWrite);
	}

	public static string ReadAllText(string p_fullPathWithFileName)
	{
		return System.IO.File.ReadAllText(p_fullPathWithFileName);
	}

	public static string[] ReadAllLines(string p_fullPathWithFileName)
	{
		return System.IO.File.ReadAllLines(p_fullPathWithFileName);
	}

	public static byte[] ReadAllBytes(string p_fullPathWithFileName)
	{
		return System.IO.File.ReadAllBytes(p_fullPathWithFileName);
	}

	public static string[] GetFiles(string p_path, string p_searchPattern)
	{
		return System.IO.Directory.GetFiles(p_path, p_searchPattern, SearchOption.AllDirectories);
	}

	public static bool DirectoryExists(string p_path)
	{
		return System.IO.Directory.Exists(p_path);
	}

	public static bool FileExists(string p_path)
	{
		return System.IO.File.Exists(p_path);
	}

	public static DirectoryInfo CreateDirectory(string p_path)
	{
		return System.IO.Directory.CreateDirectory(p_path);
	}

	public static string PathCombine(string p_path1, string p_path2)
	{
		return System.IO.Path.Combine(p_path1, p_path2);
	}
}
