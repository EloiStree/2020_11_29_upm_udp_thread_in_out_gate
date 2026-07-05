
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Events;


namespace Eloi.UDP
{

    public class UDPMono_WifiHotspotIsServer : MonoBehaviour
    {



        [Tooltip("Address finishing by 1 that is not 127.0.0.1")]
        public UnityEvent<string> m_onIsServerIpFound;
        [Tooltip("Address starting by 192 and not finishing by 1")]
        public UnityEvent<string> m_onIsClientIpFound;
        public UnityEvent m_onNotClientOrServerIp;


        public bool m_ipFinishingByOneIsServer = true;
        public bool m_remove127001 = true;

        public List<string> m_exactIpIsServer = new List<string>() { "10.82.45.6" };


        [Tooltip("Regex patterns that identify SERVER IPs (hotspot creator)")]
        public List<string> m_serverRegexPatterns = new List<string>()
{
    // Starting by 192 and finishing by 1
    @"192\.168\.\d+\.1", // Any 192.168.x.1 (fallback)
    // Starting by 10 and finishing by 1
    @"10\.\d+\.\d+\.1", // Any 10.x.x.1 (fallback)
};

        // Define the 2-255 pattern once to keep your code clean
        const string octet2to255 = @"(?:[2-9]|1\d{1,2}|2[0-4]\d|25[0-5])";

        [Tooltip("Regex patterns that identify CLIENT IPs (connected to hotspot)")]
        public List<string> m_clientRegexPatterns = new List<string>()
{
    // Starting by 192 and not finishing by 0 or 1
    $@"192\.168\.\d+\.{octet2to255}", 
    // Starting by 10 and not finishing by 0 or 1
    $@"10\.\d+\.\d+\.{octet2to255}",
};
        public bool m_autoSearchOnEnable = false;
        public float m_delayBeforeSearch = 0.5f;


        [Header("Debug")]
        public string m_ipAddressFound;
        public List<string> m_foundIps = new List<string>();


        public void OnEnable()
        {
            if (m_autoSearchOnEnable)
                Invoke(nameof(SearchForHotspotIp), m_delayBeforeSearch);
        }

        [ContextMenu("Search For Hotspot Ip")]
        public void SearchForHotspotIp()
        {
            List<string> ipFound = GetLocalIPv4Addresses();
            m_foundIps = ipFound;
            if (m_ipFinishingByOneIsServer)
            {
                TryToFoundAnIpAddressFinishingByOne(out bool ipAdressFoundIsServer, out m_ipAddressFound);
                if (ipAdressFoundIsServer)
                    m_onIsServerIpFound?.Invoke(m_ipAddressFound);
                return;
            }

            foreach (string ip in ipFound)
            {
                foreach (string ipTarget in m_exactIpIsServer)
                {
                    if (ip == ipTarget)
                    {
                        m_ipAddressFound = ip;
                        m_onIsServerIpFound?.Invoke(ip);
                        return;
                    }
                }
            }

            foreach (string ip in ipFound)

            {
                foreach (string regexIpv4 in m_serverRegexPatterns)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(ip, regexIpv4))
                    {
                        Debug.Log($"Found server ip {ip} matching regex {regexIpv4}");
                        m_ipAddressFound = ip;
                        m_onIsServerIpFound?.Invoke(ip);
                        return;
                    }
                }
            }
            foreach (string ip in ipFound)

            {
                foreach (string regexIpv4 in m_clientRegexPatterns)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(ip, regexIpv4))
                    {
                        Debug.Log($"Found client ip {ip} matching regex {regexIpv4}");
                        ChangeIpv4ToFinishByOne(ip, out string ipChanged);
                        m_ipAddressFound = ipChanged;
                        m_onIsClientIpFound?.Invoke(ipChanged);
                        return;
                    }
                }
            }

            m_ipAddressFound = "";
            m_onNotClientOrServerIp?.Invoke();
        }


        public void ChangeIpv4ToFinishByOne(string ip, out string newIp)
        {
            string[] parts = ip.Split('.');
            if (parts.Length == 4)
            {
                parts[3] = "1"; // Change the last octet to 1
                newIp = string.Join(".", parts);
            }
            else
            {
                newIp = ip; // Return the original IP if it's not valid
            }
        }


        public void TryToFoundAnIpAddressFinishingByOne(out bool found, out string ip)
        {
            List<string> mipsFound = GetLocalIPv4Addresses();
            foreach (string m in mipsFound)
            {
                if (m.EndsWith(".1"))
                {
                    found = true;
                    ip = m;
                    return;
                }
            }
            found = false;
            ip = null;
        }

        private List<string> GetLocalIPv4Addresses()
        {
            List<string> ipAddresses = new List<string>();

            // Get all network interfaces
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                // Filter out loopback and non-operational interfaces
                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    networkInterface.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                foreach (UnicastIPAddressInformation ipAddressInfo in ipProperties.UnicastAddresses)
                {

                    if (ipAddressInfo.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        continue;
                    }
                    ipAddresses.Add(ipAddressInfo.Address.ToString());
                }
            }

            if (m_remove127001)
                ipAddresses.Remove("127.0.0.1");

            return ipAddresses;
        }
    }
}