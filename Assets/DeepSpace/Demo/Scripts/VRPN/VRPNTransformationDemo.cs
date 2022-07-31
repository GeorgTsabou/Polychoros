using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DeepSpace;
using UnityVRPN;

public class VRPNTransformationDemo : MonoBehaviour {

	[SerializeField]
	public GameObject _gameObjectToBind;
	[SerializeField]
	private TrackerHostSettings _hostSettings;
	[SerializeField]
	private string _objectName = "";

	private int _lastColorId;
	private int _positionChannel = 0;
	private int _rotationChannel = 0;
	private Vector3 _targetPosition;
	private Vector3 _lastVRPNPosition;
	private Quaternion _targetRotation;
	private List<Color> _colors;


	private void MoveObject(Vector3 pos)
	{
		//ignore the first touch event, in case the distance to the last touch position is longer than a treshold. This makes movements being more smooth 
		if (Vector3.Distance (pos, _lastVRPNPosition) > 2f) {
			_lastVRPNPosition = pos;
			return;
		} 
		float xDelta = (_lastVRPNPosition.x - pos.x);
		float yDelta = (_lastVRPNPosition.z - pos.z);
		_targetPosition = new Vector3 (_targetPosition.x-xDelta, _targetPosition.y-yDelta, _targetPosition.z);
		_lastVRPNPosition = pos;
	}

	private void ChangeColor(Color color)
	{
		GameObjectToBind.GetComponent<Renderer> ().material.color = color;
	}

	private bool CheckColorForChannel(int channel)
	{
		if (_hostSettings.GetButton(_objectName, channel) && _lastColorId != channel) {
			_lastColorId = channel;
			return true;
		} 
		return false;
	}

	private IEnumerator Listen()
	{
		while (true)
		{
			MoveObject (_hostSettings.GetPosition(_objectName, _positionChannel));
			_targetRotation = _hostSettings.GetRotation (_objectName, _rotationChannel);
			if (CheckColorForChannel(1)) 
			{
				ChangeColor (_colors[0]);
			} 
			else if (CheckColorForChannel(2)) 
			{
				ChangeColor (_colors[1]);
			} 
			else if (CheckColorForChannel(3)) 
			{
				ChangeColor (_colors[2]);
			} 
			yield return null;
		}
	}

	void Awake()
	{
		// Overwriting the setting from TrackerHostSettings with the IP that was set via Command Line:
		UdpCmdConfigMgr cmdConfigMgr = CmdConfigManager.Instance as UdpCmdConfigMgr;
		if(cmdConfigMgr != null)
		{
			_hostSettings.Hostname = cmdConfigMgr.vrpnHost;
		}
		else
		{
			Debug.LogWarning("UdpCmdConfigMgr could not be found.");
		}

		_colors = new List<Color> ();
		_colors.Add(new Color(255,0,0));
		_colors.Add(new Color(0,255,0));
		_colors.Add(new Color(0,0,255));
	}

	void Start () {
		_targetPosition = GameObjectToBind.transform.position;
		_lastVRPNPosition = Vector3.zero;
		_lastColorId = 0;
		StartCoroutine("Listen");
	}
	
	protected void Update()
	{
		_gameObjectToBind.transform.rotation = Quaternion.Lerp (GameObjectToBind.transform.rotation, _targetRotation, Time.deltaTime);
		_gameObjectToBind.transform.position = Vector3.Lerp (GameObjectToBind.transform.position, _targetPosition, Time.deltaTime);
	}

	//Properties
	public TrackerHostSettings HostSettings
	{
		get { return _hostSettings; }
		set
		{
			_hostSettings = value;
		}
	}

	public string ObjectName
	{
		get { return _objectName; }
		set
		{
			_objectName = value;
		}
	}

	public GameObject GameObjectToBind {
		get {
			return _gameObjectToBind;
		}
		set {
			_gameObjectToBind = value;
		}
	}
}
