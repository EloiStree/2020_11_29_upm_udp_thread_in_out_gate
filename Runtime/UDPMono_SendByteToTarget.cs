using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPMono_SendByteToTarget : MonoBehaviour
{

    public string m_targetIp = "192.168.1.1";
    public int m_targetPort = 3615;

    public void SendByteToTarget(byte [] bytes)
    {
        SendByteTo(m_targetIp, m_targetPort, bytes);
    }
    void SendByteTo(string ip, int port, byte[] sendBytes)
    {
        UdpClient client = new UdpClient(ip,port);
        try
        {
            client.Send(sendBytes, sendBytes.Length);
            if (sendBytes.Length <= _maxByteDebugSize)
                m_lastByteSend = sendBytes;
        }
        catch (Exception e)
        {
            m_lastException = e.ToString();
        }
    }
    [TextArea(3, 10)]
    public string m_lastException = "";
    public int _maxByteDebugSize = 256;
    public byte[] m_lastByteSend = new byte[0];
}
