using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeepSpace.JsonProtocol;
using DeepSpace.Udp;

public class DemoJsonConverter : JsonConverter
{
	protected override void ReceivedPlainMessage(string jsonString, UdpManager.ReceivedMessage.Sender sender)
	{
		// You can implement the parsing of your json based received data here.
		// Have a look into the base implementation in JsonConverter.cs to see a possible way how to do this.

		// If you cannot use the received data, call the base method to pass on the DevKit internal json messages.
		base.ReceivedPlainMessage(jsonString, sender);
	}

	protected override void ReceivedEventMessage(string jsonString, uint eventIdentifier, UdpManager.ReceivedMessage.Sender sender)
	{
		switch (eventIdentifier)
		{
			// Implement your own cases here and parse your own network messages.
			default:
				base.ReceivedEventMessage(jsonString, eventIdentifier, sender);
				break;
		}
	}
}
