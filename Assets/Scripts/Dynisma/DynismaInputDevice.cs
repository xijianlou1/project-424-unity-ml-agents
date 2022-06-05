
// Integration with Dynisma motion platform simulator


using UnityEngine;
using VehiclePhysics.InputManagement;
using EdyCommonTools;
using System;


namespace Perrinn424
{

public class DynismaInputDevice : InputDevice
	{
	public Settings settings = new Settings();

	[Serializable]
	public class Settings
		{
		public int listeningPort = 56234;
		[Range(90,900)]
		public float steerRange = 400;
		}


	struct InputData
		{
		public double throttle;
		public double brake;
		public double steerAngle;
		public bool upShift;
		public bool downShift;
		public byte button;				// 1 bit per button
		public byte rotary;				// two 8-position rotaries on the wheel, split the uint8 into two 4-bit values
		}


	UdpConnection m_listener = new UdpConnection();
	UdpListenThread m_thread = new UdpListenThread();
	byte[] m_buffer = new byte[1024];
	int m_size = 0;
	InputData m_inputData = new InputData();
	bool m_newInputData = false;


	public override void Open ()
		{
		m_listener.StartConnection(settings.listeningPort);
		m_thread.threadSleepIntervalMs = 1;
		m_thread.Start(m_listener, OnReceiveData);
		m_newInputData = false;

		ClearState();
		TakeControlSnapshot();
		}


	public override void Close ()
		{
		m_thread.Stop();
		m_listener.StopConnection();

		ClearState();
		StorePreviousState();
		TakeControlSnapshot();
		}


	public override void Update ()
		{
		StorePreviousState();

		lock (m_buffer)
			{
			if (m_size > 0)
				{
				// New packet available. Convert buffer to input data.

				m_inputData = ObjectUtility.GetStructFromBytes<InputData>(m_buffer);
				m_size = 0;
				}
			}

		// New input data available? Convert to state.
		// Thread might be writing to the byte buffer meanwhile, no problem.

		if (m_newInputData)
			{
			// Analog steer, throttle and brake

			m_state.analog[0] = (int)(Mathf.Clamp(-1.0f, 1.0f, (float)(m_inputData.steerAngle / settings.steerRange * 2.0)) * 32767);
			m_state.analog[1] = (int)(Mathf.Clamp01((float)(m_inputData.throttle)) * 32767 * 2 - 32767);
			m_state.analog[2] = (int)(Mathf.Clamp01((float)(m_inputData.brake)) * 32767 * 2 - 32767);

			// Buttons

			m_state.button[0] = (byte)((m_inputData.button >> 0) & 1);
			m_state.button[1] = (byte)((m_inputData.button >> 1) & 1);
			m_state.button[2] = (byte)((m_inputData.button >> 2) & 1);
			m_state.button[3] = (byte)((m_inputData.button >> 3) & 1);
			m_state.button[4] = (byte)((m_inputData.button >> 4) & 1);
			m_state.button[5] = (byte)((m_inputData.button >> 5) & 1);
			m_state.button[6] = (byte)((m_inputData.button >> 6) & 1);
			m_state.button[7] = (byte)((m_inputData.button >> 7) & 1);

			m_state.button[8] = (byte)(m_inputData.upShift? 1 : 0);
			m_state.button[9] = (byte)(m_inputData.downShift? 1 : 0);

			// Rotaries encoded as individual buttons for each position

			int rotary0 = (m_inputData.rotary & 0x0F);
			int rotary1 = (m_inputData.rotary >> 4);

			m_state.button[10] = (byte)(rotary0 == 0? 1 : 0);
			m_state.button[11] = (byte)(rotary0 == 1? 1 : 0);
			m_state.button[12] = (byte)(rotary0 == 2? 1 : 0);
			m_state.button[13] = (byte)(rotary0 == 3? 1 : 0);
			m_state.button[14] = (byte)(rotary0 == 4? 1 : 0);
			m_state.button[15] = (byte)(rotary0 == 5? 1 : 0);
			m_state.button[16] = (byte)(rotary0 == 6? 1 : 0);
			m_state.button[17] = (byte)(rotary0 == 7? 1 : 0);
			m_state.button[18] = (byte)(rotary0 == 8? 1 : 0);
			m_state.button[19] = (byte)(rotary0 == 9? 1 : 0);
			m_state.button[20] = (byte)(rotary0 == 10? 1 : 0);
			m_state.button[21] = (byte)(rotary0 == 11? 1 : 0);
			m_state.button[22] = (byte)(rotary0 == 12? 1 : 0);
			m_state.button[23] = (byte)(rotary0 == 13? 1 : 0);
			m_state.button[24] = (byte)(rotary0 == 14? 1 : 0);
			m_state.button[25] = (byte)(rotary0 == 15? 1 : 0);

			m_state.button[30] = (byte)(rotary1 == 0? 1 : 0);
			m_state.button[31] = (byte)(rotary1 == 1? 1 : 0);
			m_state.button[32] = (byte)(rotary1 == 2? 1 : 0);
			m_state.button[33] = (byte)(rotary1 == 3? 1 : 0);
			m_state.button[34] = (byte)(rotary1 == 4? 1 : 0);
			m_state.button[35] = (byte)(rotary1 == 5? 1 : 0);
			m_state.button[36] = (byte)(rotary1 == 6? 1 : 0);
			m_state.button[37] = (byte)(rotary1 == 7? 1 : 0);
			m_state.button[38] = (byte)(rotary1 == 8? 1 : 0);
			m_state.button[39] = (byte)(rotary1 == 9? 1 : 0);
			m_state.button[40] = (byte)(rotary1 == 10? 1 : 0);
			m_state.button[41] = (byte)(rotary1 == 11? 1 : 0);
			m_state.button[42] = (byte)(rotary1 == 12? 1 : 0);
			m_state.button[43] = (byte)(rotary1 == 13? 1 : 0);
			m_state.button[44] = (byte)(rotary1 == 14? 1 : 0);
			m_state.button[45] = (byte)(rotary1 == 15? 1 : 0);

			m_newInputData = false;
			}
		}


	// Comprehensive names for the controls

	public override bool DetectPressedControl (ref ControlDefinition control)
		{
		if (!base.DetectPressedControl(ref control))
			return false;

		if (control.type == ControlType.Analog)
			{
			switch (control.id0)
				{
				case 0: control.name = "STEER"; break;
				case 1: control.name = "THROTTLE"; break;
				case 2: control.name = "BRAKE"; break;
				}
			}

		if (control.type == ControlType.Binary)
			{
			if (control.id0 == 8)
				{
				control.name = "UPSHIFT";
				}
			else
			if (control.id0 == 9)
				{
				control.name = "DOWNSHIFT";
				}
			else
			if (control.id0 >= 10 && control.id1 <= 25)
				{
				control.name = $"ROT0-{control.id0}";
				}
			else
			if (control.id0 >= 20 && control.id1 <= 35)
				{
				control.name = $"ROT1-{control.id0}";
				}
			}

		return true;
		}


	// This is called from the listener thread

	void OnReceiveData ()
		{
		lock (m_buffer)
			{
			m_size = m_listener.GetMessageBinary(m_buffer);
			}
		}
	}

}