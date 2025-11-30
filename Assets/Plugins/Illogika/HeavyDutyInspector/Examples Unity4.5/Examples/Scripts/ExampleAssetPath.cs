//----------------------------------------------
//
//      Copyright © 2013 - 2014  Illogika
//----------------------------------------------
using UnityEngine;
using System.Collections;
using HeavyDutyInspector;

public class ExampleAssetPath : MonoBehaviour {

	[Comment("Don't let typing errors slow you down. Slide an asset in what looks like a reference and store its path as a string instead. The path is also convieniently displayed under the reference in a selectable Label to allow easy Copy/Pasting.", CommentType.Info)]
	// Get a path relative to the Asset folder
	[AssetPath(typeof(Texture2D), PathOptions.RelativeToAssets)]
	public string illogikaLogoPath;

	// Get a path relative a folder named Resource, with no file extension, to use with Resource.Load
	[AssetPath(typeof(Texture2D), PathOptions.RelativeToResources)]
	public string heavyDutyInspectorLogoPath;

	// Get the filename only (with the extension)
	[AssetPath(typeof(TextAsset), PathOptions.FilenameOnly)]
	public string scriptTemplatePath;
}
