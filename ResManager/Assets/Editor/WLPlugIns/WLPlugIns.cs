// facemao
//2014/2/27 16:14


using UnityEditor;
using UnityEngine;

//[ExecuteInEditMode]

class WLPlugIns 
{
	[UnityEditor.MenuItem ("Tools/WLPLugIns")]
	
	static void SetLightMapSize () 
	{
		LightmapEditorSettings.maxAtlasHeight = 1024;
		LightmapEditorSettings.maxAtlasWidth = 1024;
	}
}