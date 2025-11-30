#if UNITY_WINRT && !UNITY_EDITOR && !UNITY_WP8

using System;
using System.Reflection;
using System.Collections.Generic;

public sealed class CandleWinRT_AppDomain
{
	public static CandleWinRT_AppDomain CurrentDomain { get; private set; }
	
	static CandleWinRT_AppDomain()
	{
		CurrentDomain = new CandleWinRT_AppDomain();
	}
	
	public Assembly[] GetAssemblies()
	{
		return new List<System.Reflection.Assembly>(GetAssemblyListAsync().Result).ToArray();
	}
	
	private async System.Threading.Tasks.Task<IEnumerable<Assembly>> GetAssemblyListAsync()
	{
		var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
		
		List<Assembly> assemblies = new List<Assembly>();
		foreach (Windows.Storage.StorageFile file in await folder.GetFilesAsync())
		{
			if (file.FileType == ".dll" || file.FileType == ".exe")
			{
				AssemblyName name = new AssemblyName() { Name = file.Name };
				Assembly asm = Assembly.Load(name);
				assemblies.Add(asm);
			}
		}
		
		return assemblies;
	}
}

#endif