using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Globalization;
using UnityEngine;
using System;


public class Server : MonoBehaviour
{
    Thread mThread;
    public string connectionIP = "127.0.0.1";
    public int connectionPort = 54955;
    IPAddress localAdd;
    TcpListener listener;
    TcpClient client;
    
    //these are the values
    public Vector3 receivedPos = Vector3.zero; //position
    public Vector3 Size = Vector3.zero;  //size
    public string object_catogory = ""; //object type
    public Quaternion rotation; //rotation
    public bool clear = false;
    public bool running;

    private void Start()
    {
        //creating a new thread for sockets
        ThreadStart ts = new ThreadStart(GetInfo);
        mThread = new Thread(ts);
        mThread.Start();
    }

    void GetInfo()
    {
        localAdd = IPAddress.Parse(connectionIP);
        listener = new TcpListener(IPAddress.Any, connectionPort);
        listener.Start();

        client = listener.AcceptTcpClient();

        running = true;
        while (running)
        {
            CheckFrameEnd();
            if (!running)
            {
                break;
            }
            ReceiveSize();
            ReceiveRotation();
            ReceiveCatogory();
            ReceiveTranslation();
        }
        listener.Stop();
    }

    private void CheckFrameEnd()
    {
        NetworkStream nwStream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];

        //---receiving translation Data from the Host----
        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize); //Getting data in Bytes from Python
        string CheckStr = Encoding.UTF8.GetString(buffer, 0, bytesRead); //Converting byte data to string
        if(CheckStr == "FRAME")
        {
            clear = true;
        }
        if(CheckStr == "DONE")
        {
            running = false;
        }
    }

    void ReceiveTranslation()
     {
        NetworkStream nwStream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];

        //---receiving translation Data from the Host----
        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize); //Getting data in Bytes from Python
        string TransDataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead); //Converting byte data to string

        if (TransDataReceived != null)
        {
            //---Using received data---
            receivedPos = StringToVector3(TransDataReceived); //<-- assigning receivedPos value from Python
            //print(receivedPos);
        }
    }

    void ReceiveSize()
    {
        NetworkStream nwStream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];

        //---receiving translation Data from the Host----
        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize); //Getting data in Bytes from Python
        string TransDataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead); //Converting byte data to string

        if (TransDataReceived != null)
        {
            //---Using received data---
            Size = StringToVector3(TransDataReceived); //<-- assigning receivedPos value from Python
            //print(Size);
        }
    }

    void ReceiveRotation()
    {
        NetworkStream nwStream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];

        //---receiving translation Data from the Host----
        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize); //Getting data in Bytes from Python
        string TransDataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead); //Converting byte data to string

        if (TransDataReceived != null)
        {
            //---Using received data---
            rotation = StringToQuadotrion(TransDataReceived); // convert string to quadotrion
            //print(TransDataReceived);
        }
    }

    void ReceiveCatogory()
    {
        NetworkStream nwStream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];

        //---receiving translation Data from the Host----
        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize); //Getting data in Bytes from Python
        string TransDataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead); //Converting byte data to string

        if (TransDataReceived != null)
        {
            object_catogory = TransDataReceived;
            //print(object_catogory);
        }
    }

    public static Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }

    public static Quaternion StringToQuadotrion(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Quaternion result = new Quaternion(
            float.Parse(sArray[0], CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(sArray[1], CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(sArray[2], CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(sArray[2], CultureInfo.InvariantCulture.NumberFormat));

        return result;
    }
}
public class spwaner : MonoBehaviour
    {
        public Transform spawnPos;
        public GameObject spawnee;
        Server sp = new Server();
     void Update()
        {
            print(sp.receivedPos);
            if (sp.receivedPos != Vector3.zero)
            {
                //clear env
                if (sp.clear)
                {
                    //clear function here
                    print("cleared");
                    sp.clear = false;
                }
                if(!sp.running)
                {
                    //exit program or do whatever you like
                    print("finished");
                }
               
                // you can do your things here

                spawnPos.position = sp.receivedPos;
                spawnPos.rotation = sp.rotation;
                Instantiate(spawnee,spawnPos.position,spawnPos.rotation);
                
                sp.receivedPos = Vector3.zero;
            }
        }
                
   
    }