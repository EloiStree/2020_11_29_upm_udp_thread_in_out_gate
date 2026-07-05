using System;
using System.Net.Sockets;
using UnityEngine;

public class UDPMono_SendTextToTarget : MonoBehaviour
{
    public string m_targetIp = "192.168.1.1";
    public int m_targetPort = 3614;
    public void SendTextToTarget(string text)
    {
        SendText(m_targetIp, m_targetPort, text);
    }
    void SendText(string ip, int port, string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        UdpClient client = new UdpClient(ip, port);
        byte [] sendBytes = System.Text.Encoding.UTF8.GetBytes(text);
        try
        {
            client.Send(sendBytes, sendBytes.Length);
            if (sendBytes.Length <= _maxByteDebugSize)
                m_lastBytesSend = sendBytes;
                m_lastTextSend = text;
           
        }
        catch (Exception e)
        {
            m_lastException = e.ToString();
        }
    }
    public int _maxByteDebugSize = 256;
    public byte[] m_lastBytesSend = new byte[0];
    [TextArea(3, 10)]
    public string m_lastTextSend = "";


    [TextArea(3, 10)]
    public string m_lastException = "";
}