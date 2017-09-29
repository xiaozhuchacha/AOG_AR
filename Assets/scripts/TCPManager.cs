// communication using TCP connection
// receive robot data from ROS

using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

#if !UNITY_EDITOR
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Networking;
// using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using System.Threading;
using System.Threading.Tasks;
#endif

public class TCPManager : MonoBehaviour
{    
    public string port = "12345";
    private bool connectedEstablished = false;
    private bool ifListening = false;

    // flags
    // set to true when new data arrives, and buffer is updated
    // set to false when data in buffer is read
    public bool left_gripper_force_torque_updated = false;
    public bool tf_data_updated = false;
    public bool image_data_updated = false;

    // class of constants to help with package decode
    public static class TCPPackageConstants 
    {
        public const int TOTAL_NUM_TF = 19;

        public const int HEAD = 0;
        public const int BASE = 1;

        public const int LEFT_HAND = 2;
        public const int LEFT_UPPER_ELBOW = 3;
        public const int LEFT_LOWER_ELBOW = 4;
        public const int LEFT_UPPER_FOREARM = 5;
        public const int LEFT_LOWER_FOREARM = 6;
        public const int LEFT_UPPER_SHOULDER = 7;
        public const int LEFT_LOWER_SHOULDER = 8;
        public const int LEFT_WRIST = 9;

        public const int RIGHT_HAND = 10;
        public const int RIGHT_UPPER_ELBOW = 11;
        public const int RIGHT_LOWER_ELBOW = 12;
        public const int RIGHT_UPPER_FOREARM = 13;
        public const int RIGHT_LOWER_FOREARM = 14;
        public const int RIGHT_UPPER_SHOULDER = 15;
        public const int RIGHT_LOWER_SHOULDER = 16;
        public const int RIGHT_WRIST = 17;

        public const int LEFT_GRIPPER = 18;

        public const int LEFT_FORCE_TORQUE = 19;
    }


    public class tfData {
        public Vector3 position;        //x, y,z
        public Quaternion orientation;     //x,y,z,w
    }

    public class GripperInfo {
        public Vector3 force;           //x, y,z
        public Vector3 torque;          //x, y,z
        public float force_mag;
        public float torque_mag;
    }

    public GripperInfo left_gripper_info = new GripperInfo();


    // buffers to store data
    public static int image_buffer_size = SensorDisplay.image_height * SensorDisplay.image_width * 4;
    public byte[] image_buffer = new byte[image_buffer_size];
    public tfData[] tfDataArr = new tfData[TCPPackageConstants.TOTAL_NUM_TF];

    private SensorDisplay sensorDisplay = null;

#if !UNITY_EDITOR
    StreamSocketListener socketListener;
    StreamSocket streamSocket;
    // Stream inStream = null;
    HostName remoteAddress = null;
    // Stream outputStream = null;
#endif


#if !UNITY_EDITOR
    async void Start()
    {
        sensorDisplay = GetComponent<SensorDisplay>();
        Debug.Log("Waiting for a connection...");
        //System.Diagnostics.Debug.Write("Waiting for a connection...");
        try {
            socketListener = new StreamSocketListener();
            socketListener.ConnectionReceived += Socket_ConnectionReceived;
            await socketListener.BindServiceNameAsync(port);
        } catch (Exception e) {
            Debug.Log(e.ToString());
            return;
        }

        for (int i = 0; i < TCPPackageConstants.TOTAL_NUM_TF; i++) {
            tfDataArr[i] = new tfData();
        }
        Debug.Log("TF array initialized");

        Debug.Log("exit start");
    }

#else
    void Start()
    {
        Debug.Log("Unity Waiting for a connection...");
        // Debug.Log(TCPPackageConstants.test);
    }
#endif

#if !UNITY_EDITOR
    // Update is called once per frame
    async void Update()
    {   
        // Debug.Log("updating");
        if (connectedEstablished && !ifListening){
            // await Task.Run(() => 
            //     {
            //         read_data();
            //     });
            read_data();
        }

    }
#else
    void Update()
    {

    }
#endif

        public void executeAction(string a){
        // Debug.Log("test button pressed");
#if !UNITY_EDITOR
        // a for action 
        send_data("a" + a);
#endif
    }

    public void resetAndToggleForceData(){
        // Debug.Log("test button pressed");
#if !UNITY_EDITOR
        send_data("f");
#endif
    }

    public void toggleTfData(){
        // Debug.Log("test button pressed");
#if !UNITY_EDITOR
        send_data("t");
#endif
    }

    public void toggleImageData(){
        // Debug.Log("test button pressed");
#if !UNITY_EDITOR
        send_data("i");
#endif
    }

#if !UNITY_EDITOR
    private async void Socket_ConnectionReceived(Windows.Networking.Sockets.StreamSocketListener sender,
        Windows.Networking.Sockets.StreamSocketListenerConnectionReceivedEventArgs args)
    {
        Debug.Log("received connection");
        streamSocket = args.Socket;
        
        connectedEstablished = true;
    }

    private async void send_data(string dataToSend)
    {
        Debug.Log("sending message: " + dataToSend);
        using (var dataWriter = new Windows.Storage.Streams.DataWriter(streamSocket.OutputStream)){
            dataWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            dataWriter.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;

            dataWriter.WriteString(dataToSend);
            await dataWriter.StoreAsync();
            await dataWriter.FlushAsync();
            dataWriter.DetachStream();
        }
        Debug.Log("message sent");
    }

    private async void read_data(){
        ifListening = true;
        // int read_msg_counter = 2;
        using (var reader = new Windows.Storage.Streams.DataReader(streamSocket.InputStream)){
            reader.InputStreamOptions = Windows.Storage.Streams.InputStreamOptions.ReadAhead;
            reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            reader.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;

            await reader.LoadAsync(4);

            while (reader.UnconsumedBufferLength > 0)
            {
                // read_msg_counter--;
                int bytesToRerad = reader.ReadInt32();
                // Debug.Log("bytes to read: " + bytesToRerad);
                if (bytesToRerad <= 0){
                    return;
                }
                await reader.LoadAsync(Convert.ToUInt32(bytesToRerad));
                byte[] buffer = new byte[bytesToRerad];
                reader.ReadBytes(buffer);
                processReceivedData(buffer, bytesToRerad);

                await reader.LoadAsync(4);
            }

            reader.DetachStream();
        }
        ifListening = false;

    }

    private void processReceivedData(byte[] buffer, int size){
        // Debug.Log("processing data");
        if (size == 36)         // size of 9 doubles
        {
            processForceTorquePackage(buffer);
        }
        else if (size == 608) 
        {
            processTFPackage(buffer);
        } else {
            // it is an image data
            processImagePackage(buffer);
        }
    }

    private void setVector3Data (byte[] buffer, int startByte, ref Vector3 v3){
        v3.Set(BitConverter.ToSingle(buffer, startByte),
                BitConverter.ToSingle(buffer, startByte + 4),
                BitConverter.ToSingle(buffer, startByte + 8));
    }

    private void setQuarternionData (byte[] buffer, int startByte, ref Quaternion q){
        q.Set(BitConverter.ToSingle(buffer, startByte),
            BitConverter.ToSingle(buffer, startByte + 4),
            BitConverter.ToSingle(buffer, startByte + 8),
            BitConverter.ToSingle(buffer, startByte + 12));
    }

    private void setPoseData (byte[] buffer, int startByte, tfData tf_data){
        setVector3Data(buffer, startByte, ref tf_data.position);
        setQuarternionData(buffer, startByte + 12, ref tf_data.orientation);
    }

    private void processForceTorquePackage(byte[] buffer){
        if (!sensorDisplay.isShowingForceVisual){
            return;
        }
        int header = BitConverter.ToInt32(buffer, 0);
        // Debug.Log("force torque header: " + header);
        if (header == TCPPackageConstants.LEFT_FORCE_TORQUE)
        {
            Debug.Log("left hand");
            setVector3Data(buffer, 4, ref left_gripper_info.force);
            setVector3Data(buffer, 16, ref left_gripper_info.torque);
            left_gripper_info.force_mag = BitConverter.ToSingle(buffer, 28);
            left_gripper_info.torque_mag = BitConverter.ToSingle(buffer, 32);   
            left_gripper_force_torque_updated = true;
        }
    }

    private void processTFPackage(byte[] buffer){
        int startByte = 0;
        for (int i = 0; i < TCPPackageConstants.TOTAL_NUM_TF; i++){
            int header = BitConverter.ToInt32(buffer, startByte);
            startByte += 4;
            setPoseData(buffer, startByte, tfDataArr[header]);
            startByte += 28;
        }

        tf_data_updated = true;
        // Debug.Log("received tf data");
    }

    private void processImagePackage(byte[] buffer){
        Debug.Log("received image data");
        if (!sensorDisplay.isShowingCameraView){
            return;
        }
        Interlocked.Exchange(ref image_buffer, buffer);
        image_data_updated = true;
    }



#endif
}