using UnityEditor;

public static class BatchOpenWindows {
	[MenuItem("Tools_Window/Default/OpenAll")]
	private static void OpenDefaultWindows() {
		EditorApplication.ExecuteMenuItem("Tools_Window/Default/Follow");
		EditorApplication.ExecuteMenuItem("Tools_Window/Default/Jungle");
		EditorApplication.ExecuteMenuItem("Tools_Window/Default/Gather");
		EditorApplication.ExecuteMenuItem("Tools_Window/Default/AttackMarshal");
		EditorApplication.ExecuteMenuItem("Tools_Window/Default/FreeDiamond");
		EditorApplication.ExecuteMenuItem("Tools_Window/Default/AllianceMechaDonate");
		EditorApplication.ExecuteMenuItem("Tools_Window/Default/ConnectingMonitoring");
		EditorApplication.ExecuteMenuItem("Tools_Window/Default/BlockBreak");
		EditorApplication.ExecuteMenuItem("Tools_Window/Default/AutoFix");
		EditorApplication.ExecuteMenuItem("Tools_Window/Default/ExtensionalScreen");
	}
}