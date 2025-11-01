using Logitech;
using UnityEngine;

public class CarroVolante : MonoBehaviour
{
    void Start()
    {
        Debug.Log("SteeringInit:" + LogitechGSDK.LogiSteeringInitialize(false));
    }

    void OnApplicationQuit()
    {
        Debug.Log("SteeringShutdown:" + LogitechGSDK.LogiSteeringShutdown());
    }

    void Update()
    {
        //All the test functions are called on the first device plugged in(index = 0)
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
            LogitechGSDK.DIJOYSTATE2ENGINES rec;
            rec = LogitechGSDK.LogiGetStateUnity(0);

            // for (int i = 0; i < 128; i++)
            // {
            //     if (rec.rgbButtons[i] == 128)
            //     {
            //         Debug.Log("Button " + i + " pressed");
            //     }
            // }

            if (rec.rgbButtons[5] == 128)
            {
                Debug.Log("Button 5 pressed");
            }
            if (rec.rgbButtons[4] == 128)
            {
                Debug.Log("Button 4 pressed");
            }
            if (rec.rgbButtons[23] == 128)
            {
                Debug.Log("Button 23 pressed");
            }
            if (rec.rgbButtons[19] == 128)
            {
                Debug.Log("Button 19 pressed");
            }
            if (rec.rgbButtons[20] == 128)
            {
                Debug.Log("Button 20 pressed");
            }
            if (rec.rgbButtons[3] == 128)
            {
                Debug.Log("Button 3 pressed");
            }
        }
        else if (!LogitechGSDK.LogiIsConnected(0))
        {
            Debug.Log("PLEASE PLUG IN A STEERING WHEEL OR A FORCE FEEDBACK CONTROLLER");
        }
        else
        {
            Debug.Log("THIS WINDOW NEEDS TO BE IN FOREGROUND IN ORDER FOR THE SDK TO WORK PROPERLY");
        }
    }
}
