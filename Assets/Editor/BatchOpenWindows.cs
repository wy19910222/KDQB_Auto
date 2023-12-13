using UnityEditor;

public static class BatchOpenWindows {
	[MenuItem("Window/Default/OpenAll")]
	private static void OpenDefaultWindows() {
		EditorApplication.ExecuteMenuItem("Window/Default/Follow");
		EditorApplication.ExecuteMenuItem("Window/Default/Jungle");
		EditorApplication.ExecuteMenuItem("Window/Default/Gather");
		EditorApplication.ExecuteMenuItem("Window/Default/AttackMarshal");
		EditorApplication.ExecuteMenuItem("Window/Default/FreeDiamond");
		EditorApplication.ExecuteMenuItem("Window/Default/AllianceMechaDonate");
		EditorApplication.ExecuteMenuItem("Window/Default/ConnectingMonitoring");
		EditorApplication.ExecuteMenuItem("Window/Default/BlockBreak");
		EditorApplication.ExecuteMenuItem("Window/Default/AutoFix");
		EditorApplication.ExecuteMenuItem("Window/Default/ExtensionalScreen");
	}
}