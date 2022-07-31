using UnityEngine;
using System.Collections;
using DeepSpace;

public class DemoWallFloorStarter : WallFloorStarter
{
	public WallNetworkHandler wallNetworkHandler;
	public FloorNetworkHandler floorNetworkHandler;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void StartWall()
	{
		base.StartWall();

		// Disable everything that is not needed or shall not be seen on the floor.
		floorNetworkHandler.gameObject.SetActive(false);
	}

	protected override void StartFloor()
	{
		base.StartFloor();

		// Disable everything that is not needed or shall not be seen on the wall.
		wallNetworkHandler.gameObject.SetActive(false);
	}
}
