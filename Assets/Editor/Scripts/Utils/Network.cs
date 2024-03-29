using System.Net;

public static class Network {
	public static bool IsInternetAvailable() {
		try {
			Dns.GetHostEntry("www.baidu.com");
			return true;
		} catch {
			return false;
		}
	}
}
