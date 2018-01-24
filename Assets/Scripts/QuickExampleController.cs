using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using WSAControllerPlugin;

/// <summary>
/// A sample MonoBehaviour class that demonstrates the use of the <see cref="HandheldControllerBridge"/> for connecting a Bluetooth Low Energy handheld controller device with a HoloLens application built with Unity. 
/// This sample is a limited example of the essentials required to establish and use a connection with a handheld controller device. It connects automatically with a handheld controller device without requiring any user initiation. 
/// Once a handheld controller device is connected the user may press the Main button to pop random balloons.</summary>
public class QuickExampleController : MonoBehaviour
{
    public enum ManipulationMode
    {
        MODE_LOOK,
        MODE_TRANSLATE,
        MODE_SCALE,
        MODE_ROTATE
    };

    public static Color COLOR_LASER_ACTIVE = Color.green;
    public static Color COLOR_LASER_CALIBRATING = Color.yellow;
    private float scaleAcc = 0.0f;
    private float transZAcc = 0.0f;
    private float rotZAcc = 0.0f;
    private bool isCalibrated = false;

    /// HandheldControllerBridge Game Object that has a HandheldControllerBridge component and communicates with the ControllerPlugin. 
    public GameObject handheldControllerBridge;

    /// The on-screen GameObject representing the handheld controller device in the scene.
    public GameObject controllerDisplay;

    // The HandheldControllerBridge component Monobehaviour class instance
    private HandheldControllerBridge _controllerBridge;

    private Vector3 _positionOffsetRightHand = new Vector3(0.2f, -0.5f, 0.2f);
    private Vector3 _positionOffsetLeftHand = new Vector3(-0.2f, -0.6f, 0.0f);
    private Vector3 _controllerOffsetPosition = new Vector3(0.0f,0.0f,0.0f);
    private Vector3 _controllerVelocity = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 _controllerAcceleration = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 _controllerOldAcceleration = new Vector3(0.0f,0.0f,0.0f);
    private Vector3 _gravity = new Vector3(0.0f, 0.0f, 0.0f);
    private int initReads = 0;

    bool firstRead = true;

    private string _statusMsgConnecting = "Connecting to a Handheld Controller Device...";
    private string _statusMsgActive = "Connected. Wait a few seconds then press the Main button to pop balloons!";

    private ManipulationMode mode = ManipulationMode.MODE_LOOK;

    public Vector3 lookAtPoint;
    private GameObject activeSelection;
    private bool showAnnotations = true;
    private GameObject parent;

    // Laser
    private GameObject _controllerDisplayLaser;

    public GameObject ActiveSelection
    {
        get
        {
            return activeSelection;
        }
    }

    public ManipulationMode Mode
    {
        get
        {
            return mode;
        }
        set
        {
            GameObject surface = GameObject.Find("SpatialSurface");
            transZAcc = 0.0f;
            scaleAcc = 0.0f;
            rotZAcc = 0.0f;
            if (value == ManipulationMode.MODE_LOOK)
            {
                surface.SetActive(false);
            }
            else
            {
                surface.SetActive(true);
            }
            mode = value;
        }
    }

    void Start()
    {
        Debug.Log("Started Daydream Controller");
        // Get a reference to the HandheldControllerBridge Monobehaviour
        _controllerBridge = handheldControllerBridge.GetComponent<HandheldControllerBridge>();
        _controllerBridge.SetDataStorageSize(60);


        // Position the Handheld Controller Game Object
        controllerDisplay.transform.position = new Vector3( 0, 0, 0.5f );

        // Locate the Laser Game Object and hide it.
        _controllerDisplayLaser = controllerDisplay.transform.Find("beam").gameObject;
        _controllerDisplayLaser.SetActive( false );

        // Subscribe to HandheldController connection status change events
        _controllerBridge.BLEActive += BLEActiveHandler;

        // Subscribe to the Main button press Action
        _controllerBridge.MainBtnDown += MainBtnDownHandler;
        _controllerBridge.AppBtnDown += AppBtnDown;
        _controllerBridge.VolMinusBtnDown += VolMinusBtnDown;
        _controllerBridge.VolPlusBtnDown += VolPlusBtnDown;
        _controllerBridge.TouchBtnDown += TouchBtnDown;

        // Establish the connection with a handheld controller device 
        _controllerBridge.CalibrationComplete += callibrationComplete;
        _controllerBridge.DelayedCalibrationCancelled += callibrationCanceled;
        _controllerBridge.DelayedCalibrationBegan += callibrationBegan;

        _controllerBridge.AutoConnect();
        _controllerBridge.BeginControllerCalibration();

        parent = GameObject.Find("Holograms");

    }

    private void callibrationBegan()
    {
        _controllerDisplayLaser.GetComponent<MeshRenderer>().material.color = COLOR_LASER_CALIBRATING;
        _controllerDisplayLaser.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", COLOR_LASER_CALIBRATING);
        _controllerDisplayLaser.SetActive(false);
        isCalibrated = true;
    }

    private void callibrationCanceled()
    {
        /*
        List<byte[]> rawList = _controllerBridge.GetRawData();
        if (rawList.Count > 0)
        {
            byte[] raw = rawList[0];
            byte accZ1 = (byte)((raw[6]) & 0x7);
            byte accZ2 = (byte)(raw[7]);
            byte accZ3 = (byte)((raw[8] & 0xC0) >> 6);
            byte accY1 = (byte)(raw[8] & 0x3F);
            byte accY2 = (byte)((raw[9] & 0xFE) >> 1);
            byte accX1 = (byte)(raw[9] & 0x01);
            byte accX2 = (byte)(raw[10]);
            byte accX3 = (byte)((raw[11] & 0xF0) >> 4);

            int accX = (accX1 << 12) | (accX2 << 4) | (accX3);
            accX = (accX << 19) >> 19;
            int accY = (accY1 << 7) | (accY2);
            accY = (accY << 19) >> 19;
            int accZ = (accZ1 << 10) | (accZ2 << 2) | accZ3;
            accZ = (accZ << 19) >> 19;

            float accXf = accX / (4096.0f);
            float accYf = accY / (4096.0f);
            float accZf = accZ / (4096.0f);

            Debug.Log(accXf + " " + accYf + " " + accZf);
            _gravity = new Vector3(accXf, accYf, accZf);

        }*/
        isCalibrated = true;
        Debug.Log("INITIALIZED");
        //double[] acc = _controllerBridge.GetAccScaled();
        //_gravity = new Vector3(((float)_controllerBridge.GetXAcc()) / sizeof(int), ((float)_controllerBridge.GetYAcc()) / sizeof(int), ((float)_controllerBridge.GetZAcc()) / sizeof(int));

        _controllerDisplayLaser.GetComponent<MeshRenderer>().material.color = COLOR_LASER_ACTIVE;
        _controllerDisplayLaser.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", COLOR_LASER_ACTIVE);
        _controllerDisplayLaser.SetActive(true);
    }

    private void callibrationComplete()
    {
        /*
        List<byte[]> rawList = _controllerBridge.GetRawData();
        if (rawList.Count > 0)
        {
            byte[] raw = rawList[0];
            byte accZ1 = (byte)((raw[6]) & 0x7);
            byte accZ2 = (byte)(raw[7]);
            byte accZ3 = (byte)((raw[8] & 0xC0) >> 6);
            byte accY1 = (byte)(raw[8] & 0x3F);
            byte accY2 = (byte)((raw[9] & 0xFE) >> 1);
            byte accX1 = (byte)(raw[9] & 0x01);
            byte accX2 = (byte)(raw[10]);
            byte accX3 = (byte)((raw[11] & 0xF0) >> 4);

            int accX = (accX1 << 12) | (accX2 << 4) | (accX3);
            accX = (accX << 19) >> 19;
            int accY = (accY1 << 7) | (accY2);
            accY = (accY << 19) >> 19;
            int accZ = (accZ1 << 10) | (accZ2 << 2) | accZ3;
            accZ = (accZ << 19) >> 19;

            float accXf = accX / (4096.0f);
            float accYf = accY / (4096.0f);
            float accZf = accZ / (4096.0f);

            Debug.Log(accXf + " " + accYf + " " + accZf);
            _gravity = new Vector3(accXf,accYf,accZf);

        }*/
        isCalibrated = true;
        Debug.Log("CALIBRATED");

        //double[] acc = _controllerBridge.GetAccScaled();
        //_gravity = new Vector3(((float)_controllerBridge.GetXAcc())/sizeof(int), ((float)_controllerBridge.GetYAcc()) / sizeof(int), ((float)_controllerBridge.GetZAcc()) / sizeof(int));

        _controllerDisplayLaser.GetComponent<MeshRenderer>().material.color = COLOR_LASER_ACTIVE;
        _controllerDisplayLaser.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", COLOR_LASER_ACTIVE);
        _controllerDisplayLaser.SetActive(true);
    }

    void Update()
    {
        /*
        List<byte[]> rawList = _controllerBridge.GetRawData();
        if (rawList.Count > 0)
        {
            byte[] raw = rawList[0];
            byte accZ1 = (byte)((raw[6]) & 0x7);
            byte accZ2 = (byte)(raw[7]);
            byte accZ3 = (byte)((raw[8] & 0xC0) >> 6);
            byte accY1 = (byte)(raw[8] & 0x3F);
            byte accY2 = (byte)((raw[9] & 0xFE) >> 1);
            byte accX1 = (byte)(raw[9] & 0x01);
            byte accX2 = (byte)(raw[10]);
            byte accX3 = (byte)((raw[11] & 0xF0) >> 4);

            int accX = (accX1 << 12) | (accX2 << 4) | (accX3);
            accX = (accX << 19) >> 19;
            int accY = (accY1 << 7) | (accY2);
            accY = (accY << 19) >> 19;
            int accZ = (accZ1 << 10) | (accZ2 << 2) | accZ3;
            accZ = (accZ << 19) >> 19;

            float accXf = accX / (2048.0f);
            float accYf = accY / (2048.0f);
            float accZf = accZ / (2048.0f);

            Vector3 accT = new Vector3(accXf, accYf, accZf);

            //Debug.Log(accT-_gravity);
        }*/



        double[] acc = _controllerBridge.GetAccScaled();
        double[] gyro = _controllerBridge.GetGyroScaled();
        double[] orient = _controllerBridge.GetOrientationScaled();

        Quaternion rot = _controllerBridge.GetRotation();
        //Vector3 accVec = new Vector3((float)acc[0],(float)acc[1],(float)acc[2]);

        Vector3 accVec = new Vector3((float)acc[0], (float)acc[1], (float)acc[2]);
        Quaternion gyroQuat = new Quaternion((float)gyro[0], (float)gyro[1], (float)gyro[2],0.0f);
        Quaternion orientQuat = new Quaternion((float)orient[0], (float)orient[1], (float)orient[2], 0.0f);
        //_gravity = new Vector3(0.0f, 10.0f, 0.0f);
        //accVec = orientQuat*accVec;
        

        controllerDisplay.transform.position = Camera.main.transform.position + _controllerOffsetPosition + new Vector3(0.0f,0.0f,1.0f);
        _controllerOldAcceleration = accVec;
        if (!_controllerBridge.IsCalibrationDelayActive && isCalibrated)
        {
            controllerDisplay.transform.rotation = _controllerBridge.GetRotation();
            //accVec = rot * accVec;
            _gravity = 0.8f * _gravity + 0.2f * accVec;
            accVec -= /*rot */ _gravity;

            if (initReads >= 200)
            {
                //Integrating to Velocity
                _controllerVelocity = (5.0f / 60.0f) * (rot * accVec);
                Debug.Log(_controllerVelocity);
                //Integrating to Position
                _controllerOffsetPosition += 5.0f/60.0f * _controllerVelocity;
            }
            else
            {
                initReads++;
            }
        }
        else
        {
            controllerDisplay.transform.localRotation = Quaternion.identity;
        }
        Vector3 controllerPos = _controllerDisplayLaser.transform.position;
        Vector3 controllerForward = _controllerDisplayLaser.transform.forward;

        /*if (mode == ManipulationMode.MODE_LOOK)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(controllerPos, controllerForward, out hitInfo))
            {
                activeSelection = hitInfo.collider.gameObject;
                lookAtPoint = hitInfo.point;
                this.transform.position = hitInfo.point;
                this.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            }
            else
            {
                activeSelection = null;
            }
        }
        else if (mode == ManipulationMode.MODE_TRANSLATE)
        {
            float xTouch = _controllerBridge.GetXTouch() / 255.0f;
            float yTouch = _controllerBridge.GetYTouch() / 255.0f;
            activeSelection.transform.position = (controllerPos + 10.0f * new Vector3(xTouch, yTouch, transZAcc));
        }
        else if (mode == ManipulationMode.MODE_SCALE)
        {
            activeSelection.transform.localScale += new Vector3(scaleAcc, scaleAcc, scaleAcc);
        }
        else if(mode==ManipulationMode.MODE_ROTATE)
        {
            float xTouch = _controllerBridge.GetXTouch() / 255.0f;
            float yTouch = _controllerBridge.GetYTouch() / 255.0f;
            Vector3 axis = new Vector3(yTouch, xTouch, rotZAcc);
            activeSelection.transform.Rotate(axis.normalized, 0.1f, Space.Self);
        }*/
    }

    //
    // HandheldControllerBridge Button Down/Up Handlers
    // 


    //Accept Manipulation
    private void MainBtnDownHandler()
    {

        // Hide the status text label once the user has clicked once
        mode = ManipulationMode.MODE_LOOK;
        _controllerBridge.BeginControllerCalibration();
        Debug.Log("MainBtnDown");

    }

    //Show/Hide Annotations
    private void AppBtnDown()
    {
        showAnnotations = !showAnnotations;
        parent.GetComponent<GameObjectManager>().hideAnnotations(parent.GetComponentInChildren<QuickExampleController>().ActiveSelection.name, showAnnotations);
    }

    private void VolMinusBtnDown()
    {

        // Hide the status text label once the user has clicked once
        Debug.Log("MainBtnDown");
        scaleAcc -= 0.01f;
        transZAcc -= 0.1f;
        rotZAcc -= 0.01f;

    }

    private void VolPlusBtnDown()
    {

        // Hide the status text label once the user has clicked once
        Debug.Log("MainBtnDown");
        scaleAcc += 0.01f;
        transZAcc += 0.1f;
        rotZAcc += 0.01f;

    }

    //Create Annotation
    private void TouchBtnDown()
    {

    }


    //
    // HandheldControllerBridge BLE Status Action Event Handlers 
    // 

    private void BLEActiveHandler()
    {
        Debug.Log("_statusMsgActive");
    }


}