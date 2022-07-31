namespace DeepSpace.JsonProtocol
{
	public class EventIdentifierBase
	{
		public const uint UNKNOWN = 0;
		public const uint ASSET_TRANSFORM = 1;  // Changing an assets transform (position, rotation, scale).
		public const uint ASSET_SPAWN = 2;      // Creating a new asset.
		public const uint ASSET_DESTROY = 3;    // Destroy an asset with a given ID.
		public const uint ASSET_FOV = 4;        // Change the Field of View of the cameras.
		public const uint SETTING_MESSAGE = 5;  // Change Settings like Stereo Eye-Distance.

		public static string GetIdentifierName(uint identifier)
		{
			string name = "UNKNOWN";

			switch(identifier)
			{
				case ASSET_TRANSFORM:
					name = "ASSET_TRANSFORM";
					break;
				case ASSET_SPAWN:
					name = "ASSET_SPAWN";
					break;
				case ASSET_DESTROY:
					name = "ASSET_DESTROY";
					break;
				case ASSET_FOV:
					name = "ASSET_FOV";
					break;
				case SETTING_MESSAGE:
					name = "SETTING_MESSAGE";
					break;
				default:
					break;
			}

			return name;
		}
	}
}