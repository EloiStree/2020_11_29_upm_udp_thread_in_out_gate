using System;
using System.Net;

[System.Serializable]
public class ReceivedMessageWithSourceAsUTF8
{
    public string m_message;
    public string m_sourceIp;
    public string m_sourcePort;

    public ReceivedMessageWithSourceAsUTF8(string message, IPEndPoint source)
    {
        m_message = message;
        m_sourceIp = source.Address.ToString();
        m_sourcePort = source.Port.ToString();
    }

    public ReceivedMessageWithSourceAsUTF8(string message, string sourceIp = "", string sourcePort="")
    {
        m_message = message;
        m_sourceIp = sourceIp;
        m_sourcePort = sourcePort;
    }

    public string GetIpv4()
    {
        return m_sourceIp;
    }
    public string GetMessage()
    {
        return m_message;
    }
}
