namespace DeepSpace.Udp
{
	// TODO: This class is not yet fully developped!

	// Use this class to create a new and unique Network ID for UdpObjects.
	public class UniqueNetID
	{
		public string GetId()
		{
			string timestamp = System.DateTime.UtcNow.ToString("yyyyMMdd_HHmmssfff", System.Globalization.CultureInfo.InvariantCulture);
			string mode = string.Empty;
			if (CmdConfigManager.Instance)
			{
				mode = CmdConfigManager.Instance.applicationType.ToString().ToLower();
			}
			string id = mode + "_" + timestamp;

			return id;
		}
	}
}