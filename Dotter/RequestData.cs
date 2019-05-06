using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Dotter
{
    public class RequestData
    {
        private const byte HEADER_INFO = 0x54;
        private const byte HEADER_PLAYER = 0x55;

        private UdpClient udpClient;
        private IPEndPoint endPoint;

        public class QueryData
        {
            public QueryData(byte[] A2S_CHALLENGE,  byte[] A2S_PLAYER, byte[] A2S_INFO, string QUERYING_LOG)
            {
                this.A2S_CHALLENGE = A2S_CHALLENGE;
                this.A2S_PLAYER = A2S_PLAYER;
                this.A2S_INFO = A2S_INFO;
                this.QUERYING_LOG = QUERYING_LOG;
            }

            public byte[] A2S_CHALLENGE { get; set; }
            public byte[] A2S_PLAYER { get; set; }
            public byte[] A2S_INFO { get; set; }
            public string QUERYING_LOG { get; set; }
        }

        public QueryData Querying(string targetIP, int targetPort)
        {
            bool isExceptionOccured = false;
            byte[] info = { }, challenge = { }, player = { };
            string log = string.Empty;

            try
            {
                udpClient = new UdpClient();
                endPoint = new IPEndPoint(IPAddress.Parse(targetIP), targetPort);
                udpClient.Client.SendTimeout = 5000;
                udpClient.Client.ReceiveTimeout = 5000;

                udpClient.Connect(endPoint);

                log += "UDPCLIENT : Connected\n";
            }
            catch (Exception)
            {
                isExceptionOccured = true;
                log += "UDPCLIENT : Connection failed\n";
            }
            
            // request challenge code
            try
            {
                udpClient.Send(new byte[] {
                    0xFF, 0xFF, 0xFF, 0xFF
                    , HEADER_PLAYER
                    , 0xFF, 0xFF, 0xFF, 0xFF
                }, 9);
                challenge = udpClient.Receive(ref endPoint);

                log += "UDPCLIENT : Server response to request (A2S_CHALLENGE)\n";
            }
            catch (Exception)
            {
                isExceptionOccured = true;
                log += "UDPCLIENT : Failed to request A2S_CHALLENGE\n";
            }
            
            // request player list
            try
            {
                udpClient.Send(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF
                    , HEADER_PLAYER
                    , challenge[5], challenge[6], challenge[7], challenge[8]
                }, 9);
                player = udpClient.Receive(ref endPoint);

                log += "UDPCLIENT : Server response to request (A2S_PLAYER)\n";
            }
            catch (Exception)
            {
                isExceptionOccured = true;
                log += "UDPCLIENT : Failed to request A2S_PLAYER\n";
            }
            
            // request server information
            try
            {
                udpClient.Send(new byte[] {
                    0xFF, 0xFF, 0xFF, 0xFF, HEADER_INFO,
                    0x53, 0x6F, 0x75, 0x72, 0x63,
                    0x65, 0x20, 0x45, 0x6E, 0x67,
                    0x69, 0x6E, 0x65, 0x20, 0x51,
                    0x75, 0x65, 0x72, 0x79, 0x00
                }, 25);
                info = udpClient.Receive(ref endPoint);

                log += "UDPCLIENT : Server response to request (A2S_INFO)\n";
            }
            catch (Exception)
            {
                isExceptionOccured = true;
                log += "UDPCLIENT : Failed to request A2S_INFO\n";
            }

            if(isExceptionOccured == true)
            {
                throw new Exception();
            }

            return new QueryData(challenge, player, info, log);
        }
    }
}
