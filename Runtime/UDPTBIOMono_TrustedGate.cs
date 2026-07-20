using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public class UDPTBIOMono_TrustedGate : MonoBehaviour
{

    [Header("Port for callback")]
    public int portCallBackBytes = 3615;
    public int portCallBackText = 3614;

    [Header("Client Found List")]
    public HashSet<string> m_clientIpv4 = new HashSet<string>();
    public string[] m_clientIpv4List = new string[0]; 

    [Header("Client to Server")]
    public UnityEvent<byte[]> m_onByteReceivedFromClientToServer;
    public UnityEvent<string> m_onTextReceivedFromClientToServer;
    public UnityEvent<string, byte[]> m_onByteReceivedFromClientToServerWithIpv4;
    public UnityEvent<string, string> m_onTextReceivedFromClientToServerWithIpv4;

    [Header("Server to Client")]
    public UnityEvent<byte[]> m_onByteSendFromServerToClient;
    public UnityEvent<string> m_onTextSendFromServerToClient;
    public UnityEvent<string, byte[]> m_onByteSendFromServerToTargetClient;
    public UnityEvent<string, string> m_onTextSendFromServerToTargetClient;




    [Header("Client to Server Integer")]
    [Header("Try to parse text from client to index integer format")]
    public bool m_parseTextToInteger = true;
    public UnityEvent<int> m_onClientIntegerReceived;
    public UnityEvent<int, int> m_onClientIndexIntegerReceived;
    public UnityEvent<string, int> m_onClientIntegerReceivedWithSourceIpv4;
    public UnityEvent<string, int, int> m_onClientIndexIntegerReceivedWithSourceIpv4;


    [Header("Client List Changed")]
    public UnityEvent<string> m_onIpv4NewClient ;
    public UnityEvent<string[]> m_onIpv4ClientsChanged;

    Dictionary<string, UdpClientState> m_clients = new Dictionary<string, UdpClientState>();


    public int m_maxByteDebugSizeSendReceived = 256;
    [Header("Debug Server To Client")]
    public bool m_debugSendMessage = true;
    public byte[] m_lastBytesSend = new byte[0];
    public string m_lastTextSend = "";
    [TextArea(3, 10)]
    public string m_lastException = "";


    [Header("Debug Client To Server")]
    public bool m_debugReceivedMessage = true;
    public string m_lastReceivedIpv4 = "";
    public byte[] m_lastReceivedBytes = new byte[0];
    public string m_lastReceivedText = "";

    private void NotifyDebugReceivedText(string ipv4, string message)
    {
        if (m_debugReceivedMessage)
            return;
        m_lastReceivedIpv4 = ipv4;
        if (message.Length <= m_maxByteDebugSizeSendReceived)
            m_lastReceivedText = message;
        else
            m_lastReceivedText = message.Substring(0, m_maxByteDebugSizeSendReceived) + "...";

        TryToParseToInteger(ipv4, message);
    }

  

    private void NotifyDebugReceivedBytes(string ipv4, byte[] message)
    {
        if (m_debugReceivedMessage)
            return;
        m_lastReceivedIpv4 = ipv4;
        if (message.Length <= m_maxByteDebugSizeSendReceived)
            m_lastReceivedBytes = message;
        else
        {
            m_lastReceivedBytes = new byte[m_maxByteDebugSizeSendReceived];
            Array.Copy(message, m_lastReceivedBytes, m_maxByteDebugSizeSendReceived);
        }
        TryToParseToInteger(ipv4, message);
    }

    
    public void NotifyReceivedMessage(ReceivedMessageWithSourceAsUTF8 message)
    {
        AddPlayerIfNotInList(message.GetIpv4());
        m_onTextReceivedFromClientToServer?.Invoke(message.GetMessage());
        m_onTextReceivedFromClientToServerWithIpv4?.Invoke(message.GetIpv4(), message.GetMessage());
        NotifyDebugReceivedText(message.GetIpv4(), message.GetMessage());
    }

    public void NotifyReceiveMessage(ReceivedMessageWithSourceAsByte message)
    { 
        AddPlayerIfNotInList(message.GetIpv4());
        m_onByteReceivedFromClientToServer?.Invoke(message.GetMessage());
        m_onByteReceivedFromClientToServerWithIpv4?.Invoke(message.GetIpv4(), message.GetMessage());
        NotifyDebugReceivedBytes(message.GetIpv4(), message.GetMessage());  
    }
    private void AddPlayerIfNotInList(string ipv4)
    {
        if (!m_clientIpv4.Contains(ipv4))
        {
            m_clientIpv4.Add(ipv4);
            m_clientIpv4List = m_clientIpv4.ToArray();
            m_onIpv4NewClient?.Invoke(ipv4);
            m_onIpv4ClientsChanged?.Invoke(m_clientIpv4List);
        }
    }



    public UdpClientState GetClient(string ip, int port)
    {
        string key = ip + ":" + port;
        if (!m_clients.ContainsKey(key))
        {
            UdpClientState client = new UdpClientState(ip, port);
            m_clients.Add(key, client);
        }
        return m_clients[key];
    }
     void SendMessageTo(string ip, int port, string message)
    {
        UdpClientState client = GetClient(ip, port);
        Byte[] sendBytes = Encoding.UTF8.GetBytes(message);
        try
        {
            client.m_client.Send(sendBytes, sendBytes.Length);

            if (m_debugSendMessage) { 
                if (message.Length <= m_maxByteDebugSizeSendReceived)
                    m_lastTextSend = message;
                else 
                    m_lastTextSend = message.Substring(0, m_maxByteDebugSizeSendReceived) + "...";
            }
        }
        catch (Exception e)
        {
            m_lastException = e.ToString();
        }

    }
    
     void SendBytesTo(string ip, int port, byte[] sendBytes)
    {
        UdpClientState client = GetClient(ip, port);
        try
        {
            client.m_client.Send(sendBytes, sendBytes.Length);
            if (m_debugSendMessage) { 
                if (sendBytes.Length <= m_maxByteDebugSizeSendReceived)
                    m_lastBytesSend = sendBytes;
            }
        }
        catch (Exception e)
        {
            m_lastException = e.ToString();
        }

    }

    public void SendTextToAll(string message)
    {
        m_onTextSendFromServerToClient.Invoke(message);
        foreach (var ipv4 in m_clientIpv4)
        {
            SendTextToClient(ipv4, message);
        }
    }

    public void SendBytesToAll(byte[] message)
    {
        m_onByteSendFromServerToClient.Invoke(message);
        foreach (var ipv4 in m_clientIpv4)
        {
            SendBytesToClient(ipv4, message);
        }
    }

    public void SendTextToClient(string ipv4, string message)
    {
        m_onTextSendFromServerToTargetClient.Invoke(ipv4, message);
        SendMessageTo(ipv4, portCallBackText, message);
    }
    public void SendBytesToClient(string ipv4, byte[] message)
    {
        m_onByteSendFromServerToTargetClient.Invoke(ipv4, message);
        SendBytesTo(ipv4, portCallBackBytes, message);
    }


    private void TryToParseToInteger(string ipv4, byte[] message)
    {
        if (message.Length == 4  || message.Length == 8)
        {
            if (message.Length == 4)
            {
                int value = BitConverter.ToInt32(message, 0);
                m_onClientIntegerReceived?.Invoke(value);
                m_onClientIntegerReceivedWithSourceIpv4?.Invoke(ipv4, value);
            }
            else if (message.Length == 8)
            {
                int index = BitConverter.ToInt32(message, 0);
                int value = BitConverter.ToInt32(message, 4);
                m_onClientIndexIntegerReceived?.Invoke(index, value);
                m_onClientIndexIntegerReceivedWithSourceIpv4?.Invoke(ipv4, index, value);
            }
        }
    }

    const int MAX_INDEX_INTEGER_TEXT_LENGHT = 25;  //"-2147483648:-2147483648";
    const int MAX_INTEGER_TEXT_LENGHT = 12;  //"-2147483648";


    private void TryToParseToInteger(string ipv4, string message)
    {
        if (m_parseTextToInteger)
        {
            if (message.Length > MAX_INDEX_INTEGER_TEXT_LENGHT)
                return;
            message = message.Trim();
            if (message.Length<= MAX_INTEGER_TEXT_LENGHT && int.TryParse(message, out int value))
            {
                m_onClientIntegerReceived?.Invoke(value);
                m_onClientIntegerReceivedWithSourceIpv4?.Invoke(ipv4, value);
            }
            if (message.Length <= MAX_INDEX_INTEGER_TEXT_LENGHT)
            {
                string[] parts = message.Split(':');
                if (parts.Length == 2)
                {
                    if (int.TryParse(parts[0], out int index) && int.TryParse(parts[1], out int valueInteger))
                    {
                        m_onClientIndexIntegerReceived?.Invoke(index, valueInteger);
                        m_onClientIndexIntegerReceivedWithSourceIpv4?.Invoke(ipv4, index, valueInteger);
                    }
                }
            }
        }
    }
}
