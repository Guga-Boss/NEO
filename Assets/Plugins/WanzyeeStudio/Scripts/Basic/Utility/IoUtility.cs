
/*WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW*\     (   (     ) )
|/                                                      \|       )  )   _((_
||  (c) Wanzyee Studio  < wanzyeestudio.blogspot.com >  ||      ( (    |_ _ |=n
|\                                                      /|   _____))   | !  ] U
\.ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ./  (_(__(S)   |___*/

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace WanzyeeStudio{

	/// <summary>
	/// Include some convenient methods to extend IO operation.
	/// </summary>
	public static class IoUtility{

		/// <summary>
		/// Determine if the path can be used to create a file or directory.
		/// </summary>
		/// 
		/// <remarks>
		/// Optional to throw an exception message or just return <c>false</c> if invalid.
		/// A legal path might not be in good format, e.g., "C:dir\ //file" or "/\pc\share\\new.txt".
		/// But it's safe to pass to <c>Directory</c> or <c>FileInfo</c> to create.
		/// Path in situations below is invalid, even dangerous:
		/// 	1. Nothing but empty or white-spaces, nowhere to go.
		/// 	2. Starts with 3 slashes, this causes crash while system looking for parent directories.
		/// 	3. Includes invalid chars, can't name a file.
		/// 	4. A name in path starts or ends with space, we can't get the created file, even delete.
		/// </remarks>
		/// 
		/// <returns><c>true</c> if is creatable; otherwise, <c>false</c>.</returns>
		/// <param name="path">Path.</param>
		/// <param name="exception">Flag to throw an exception or return <c>false</c>.</param>
		/// 
		public static bool CheckCreatable(string path, bool exception = false){

			try{
				
				if(null == path || string.Empty == path.Trim())
					throw new Exception("Path cannot be null, empty, or white-spaces only.");

				if(Regex.IsMatch(path, @"\A[\\/]{3}"))
					throw new Exception("Path cannot start with 3 slashes.");

				var _c = Regex.IsMatch(path, @"\A\w:") ? path[0] + path.Substring(2) : path;
				_c = Regex.Replace(_c, @"[\\/]", string.Empty);

				if(-1 != _c.IndexOfAny(Path.GetInvalidFileNameChars()))
					throw new Exception("Illegal characters in path.");

				if(path.Split('\\', '/').Any(_v => Regex.IsMatch(_v, @"(^\s+\S|\S\s+$)")))
					throw new Exception("Directory or file name cannot start or end with white-space.");

				return true;

			}catch(Exception e){

				if(!exception) return false;
				throw new ArgumentException(e.Message, "path");

			}

		}

		/// <summary>
		/// Try to delete a file or directory at the specified path.
		/// </summary>
		/// 
		/// <remarks>
		/// This doesn't work in Web Player.
		/// Note, the operation is permanently and irreversibly.
		/// Optional to trace and delete ancestor directories if became empty.
		/// </remarks>
		/// 
		/// <param name="path">Path.</param>
		/// <param name="ancestor">If set to <c>true</c> delete ancestor directories if empty.</param>
		/// 
		public static void Delete(string path, bool ancestor = false){

			#if UNITY_WEBPLAYER

			throw new NotSupportedException("'Delete' is not supported in Web Player.");

			#else

			try{

				var _f = new FileInfo(path);
				if(_f.Exists) _f.Delete();

				var _d = new DirectoryInfo(path);
				if(_d.Exists && null != _d.Parent) _d.Delete(true);

				if(!ancestor) return;

				for(var p = _d.Parent; null != p; p = p.Parent){
					if(p.Exists && null != p.Parent) p.Delete(false);
				}

			}catch{}

			#endif

		}

	}

}
