using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA.Input;
using UnityEngine.UI;

#if !UNITY_EDITOR
using System.Threading.Tasks;
#endif

public class ManualDisplayer : MonoBehaviour {

    GestureRecognizer recognizer;
    TCPManager udpManager = null;

    public RawImage currentManualPage;

    // Use this for initialization
//     void Start()
//     {
//         udpManager = GetComponent<TCPManager>();
//         recognizer = new GestureRecognizer();
// #if !UNITY_EDITOR
//         recognizer.TappedEvent += Recognizer_TappedEvent;
// #endif
//         recognizer.StartCapturingGestures();
//     }

// #if !UNITY_EDITOR
//     private async void Recognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay)
//     {
//         // var myGUITexture = (Texture2D)Resources.Load("page7");

//         // currentManualPage.texture = myGUITexture;

//         // Debug.Log("Loaded page 7");

//         await Task.Run(() => 
//         {
//             udpManager.send_data();
//         });
//     }
// #endif

}