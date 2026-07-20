using UnityEngine;
using UnityEngine.Events;

public class UDPTBIOMono_DebugReceivedMessageWithSource : MonoBehaviour
{

    [Header("Relay")]
    public UnityEvent<string, string> m_onTextReceivedWithIpv4;
    public UnityEvent<string, byte[]> m_onByteReceivedWithIpv4;
    public UnityEvent<string> m_onLastReceivedIpv4;

    [Header("Format Join Debug")]
    public UnityEvent<string> m_receivedAsFormatJoin;
    public string m_formatString = "{0}: {1}";
    public string m_byteSplitter = ", ";
    public bool m_useByteMessageForFormatJoin = true;


    public void PushIn(ReceivedMessageWithSourceAsUTF8 received)
    {
        m_onTextReceivedWithIpv4.Invoke(received.GetIpv4(), received.GetMessage());
        m_onLastReceivedIpv4.Invoke(received.GetIpv4());
        m_receivedAsFormatJoin.Invoke(string.Format(m_formatString, received.GetIpv4(), received.GetMessage()));
    }
    public void PushIn(ReceivedMessageWithSourceAsByte received)
    {
        m_onByteReceivedWithIpv4.Invoke(received.GetIpv4()    , received.GetMessage());
        m_onLastReceivedIpv4.Invoke(received.GetIpv4());

        if (m_useByteMessageForFormatJoin)
            m_receivedAsFormatJoin.Invoke(string.Format(m_formatString, received.GetIpv4(), string.Join(m_byteSplitter,received.GetMessage())));
    }

}

