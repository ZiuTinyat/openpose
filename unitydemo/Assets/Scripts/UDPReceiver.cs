using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace opdemo
{
    public class UDPReceiver : MonoBehaviour
    {
        // Singleton
        private static UDPReceiver instance = null;

        // Threading
        private bool CloseThreadFlag = true;
        private static Mutex ReceivingMutex = new Mutex();
        private Thread receiveThread;

        // UDP receiver
        private UdpClient client;
        public int port = 8051; // define > init

        private string receivedData = "";

        // Interface
        public static int PortNumber { get { return instance.port; } }
        public static string ReceivedData { get { return instance.receivedData; } }
        public static void BeginReceiving()
        {
            instance.StartReceivingThread();
        }
        public static void StopReceiving()
        {
            instance.StopReceivingThread();
        }
        public static bool IsRunning { get { return !instance.CloseThreadFlag; } }

        // Private
        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            if (Controller.Mode == PlayMode.Stream) BeginReceiving(); // Automatic start receiving
        }

        private void StartReceivingThread()
        {
            if (!CloseThreadFlag) return; // already started

            CloseThreadFlag = false;
            receiveThread = new Thread(new ThreadStart(Receiving));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        private void Receiving()
        {
            Debug.Log("Receive start");
            ReceivingMutex.WaitOne();

            client = new UdpClient(port);
            while (!CloseThreadFlag)
            {
                try
                {
                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                    client.Receive(ref anyIP);
                    byte[] data = client.Receive(ref anyIP);
                    string text = Encoding.UTF8.GetString(data);
                    Debug.Log(">> " + text);
                    InputText(text);
                }
                catch (Exception err)
                {
                    Debug.Log("Receive error: " + err.ToString());
                }
            }

            Debug.Log("Receive end");
            ReceivingMutex.ReleaseMutex();
        }

        private void StopReceivingThread()
        {
            if (CloseThreadFlag) return; // already stopped

            client.Close();
            CloseThreadFlag = true;

            ReceivingMutex.WaitOne();
            receiveThread.Abort();
            Debug.Log("thread aborted");
            ReceivingMutex.ReleaseMutex();
        }

        private void InputText(string text)
        {
            receivedData = text;
        }

        // In case of abrupt exit of program
        private void OnDestroy()
        {
            StopReceiving();
        }
    }
}
