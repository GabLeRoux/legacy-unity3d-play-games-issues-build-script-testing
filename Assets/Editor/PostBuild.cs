using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System;
using Debug = UnityEngine.Debug;
using UnityEditor.iOS.Xcode;

public class PostBuild : MonoBehaviour
{
	static void ExecuteShellScript (string pathToBuiltProject)
	{
		Debug.Log ("PostBuild started for " + pathToBuiltProject);
		string script = "postbuild.sh";
		string localPath = "Assets/Editor/" + script;
		try {
			var fullPath = Directory.GetCurrentDirectory () + Path.DirectorySeparatorChar + localPath.Replace ("/", Path.DirectorySeparatorChar.ToString ());
			var bin = fullPath + script;
			var process = new Process ();
			process.StartInfo.FileName = bin;
			process.StartInfo.WorkingDirectory = pathToBuiltProject;
			Debug.Log ("PostBuild running: " + bin);
			Debug.Log ("PostBuild WorkingDirectory: " + pathToBuiltProject);
			process.Start ();
		} catch (Exception e) {
			Debug.LogError (e);
		}
	}

	static void UpdateInfoPlist (string path)
	{
		// Add url schema to plist file
		string plistPath = path + "/Info.plist";
		PlistDocument plist = new PlistDocument ();
		plist.ReadFromString (File.ReadAllText (plistPath));
		// Get root
		PlistElementDict rootDict = plist.root;
		rootDict.SetBoolean ("UIRequiresFullScreen", true);
		rootDict.SetString ("NSCameraUsageDescription", "not used");
		rootDict.SetString ("NSCalendarsUsageDescription", "Some ad content may access calendar");
		rootDict.SetString ("NSPhotoLibraryUsageDescription", "Some ad content may access photo library");
		rootDict.SetBoolean ("ITSAppUsesNonExemptEncryption", false);

		plist.WriteToFile (plistPath);
	}

	static void GPGSXcodeProjectRequirements (string projPath, PBXProject proj, string target)
	{
		//proj.AddBuildProperty(target, "OTHER_LDFLAGS", "$(inherited)");
		//proj.AddBuildProperty(target, "HEADER_SEARCH_PATHS", "$(inherited)");
		//proj.AddBuildProperty(target, "OTHER_CFLAGS", "$(inherited)");
		proj.UpdateBuildProperty (target, "OTHER_LDFLAGS", new string[] { "$(inherited)" }, new string[] { });
		proj.UpdateBuildProperty (target, "HEADER_SEARCH_PATHS", new string[] { "$(inherited)" }, new string[] { });
		proj.UpdateBuildProperty (target, "OTHER_CFLAGS", new string[] { "$(inherited)" }, new string[] { });
		proj.SetBuildProperty (target, "ENABLE_BITCODE", "NO");

		string guid = proj.FindFileGuidByProjectPath ("Libraries/Plugins/iOS/GPGSAppController.mm");
		System.Collections.Generic.List<string> args = new System.Collections.Generic.List<string> {
			"-fobjc-arc"
		};

		proj.SetCompileFlagsForFile (target, guid, args);

		string dst = proj.WriteToString ();
		Debug.Log ("DST: " + dst);
		File.WriteAllText (projPath, dst);
	}

	[PostProcessBuild]
	public static void OnPostprocessBuild (BuildTarget buildTarget, string path)
	{
		#if UNITY_IOS

		if (buildTarget == BuildTarget.iOS) {
			try {
				Debug.Log ("BuildTarget: " + buildTarget);
				Debug.Log ("PATH: " + path);
				string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
				Debug.Log ("PROJPATH: " + projPath);

				PBXProject proj = new PBXProject ();
				string src = File.ReadAllText (projPath);
				Debug.Log ("SRC: " + src);
				proj.ReadFromString (src);

				string target = proj.TargetGuidByName ("Unity-iPhone");
				Debug.Log ("TARGET: " + target);

				GPGSXcodeProjectRequirements (projPath, proj, target);
				UpdateInfoPlist (path);
				ExecuteShellScript (path);

			} catch (Exception e) {
				Debug.LogError (e);
			}
		}
		#endif
	}
}
