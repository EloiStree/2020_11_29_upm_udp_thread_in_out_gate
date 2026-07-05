using System.Net;

[System.Serializable]
public class ReceivedMessageWithSourceAsByte
{
    public byte [] m_message;
    public string m_sourceIp;
    public string m_sourcePort;

    public ReceivedMessageWithSourceAsByte(byte[] message, IPEndPoint source)
    {
        m_message = message;
        m_sourceIp = source.Address.ToString();
        m_sourcePort = source.Port.ToString();
    }

    public ReceivedMessageWithSourceAsByte(byte[]  message, string sourceIp = "", string sourcePort = "")
    {
        m_message = message;
        m_sourceIp = sourceIp;
        m_sourcePort = sourcePort;
    }
    public string GetIpv4()
    {
        return m_sourceIp;
    }
    public byte[] GetMessage()
    {
        return m_message;
    }

}