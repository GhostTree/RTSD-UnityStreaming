using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using LiblinphonedotNET;

public class TestLinphone : MonoBehaviour {
    public InputField username;
    public InputField password;
    public InputField callee;

    public Text incomingName;
    public Text outgoingName;
    public Text infoName;

	public float toNextSnapShot;
    float currentTime;

    long snapshotIndex;
    long textureIndex;
    RawImage videoCanvas;
    RawImage videoImage;
    Texture2D canvasTexture;

    Account account = null;
    Phone phone = null;
    bool callOn = false;
    string callName = "";

    void Start () {
		toNextSnapShot = 0.06f;
        currentTime = 0.0f;
        snapshotIndex = 0;
        textureIndex = 0;
        videoCanvas = GameObject.Find("Video").GetComponent<RawImage>();//(RawImage)GameObject.Find("Video").GetComponent<RawImage>();

        canvasTexture = new Texture2D(2, 2);
        //videoImage= videoCanvas.GetComponent<RawImage>();
    }

    void OnDestroy()
    {
        if(phone != null)
        {
            phone.hangupCall();
            phone.Disconnect();

        }
    }

    void Update () {
        if (incomingName != null && outgoingName != null) {
            incomingName.text = callName;
            outgoingName.text = callName;
            infoName.text = callName;
        }

        if (callOn) {
            currentTime += Time.deltaTime;
            if (currentTime >= toNextSnapShot) {
                currentTime = 0;
                if (phone != null) {
                    phone.SnapShot(Application.dataPath + "/temp/snapshot" + snapshotIndex.ToString()+".jpg");
                    Debug.Log("Saving screenshot to " + Application.dataPath + "/temp/snapshot" + snapshotIndex.ToString() + ".jpg");
                    snapshotIndex++;
                    textureIndex = snapshotIndex - 1;


                }
            }
            //Texture2D canvasTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("temp/snapshot" + textureIndex.ToString() + ".jpg", typeof());
            
            if (System.IO.File.Exists(Application.dataPath + "/temp/snapshot" + textureIndex.ToString() + ".jpg"))
            {
                if(System.IO.File.Exists(Application.dataPath + "/temp/snapshot" + (textureIndex-1).ToString() + ".jpg"))
                {
                    //BE CAREFULL WITH THIS ONE!
                    System.IO.File.Delete(Application.dataPath + "/temp/snapshot" + (textureIndex - 1).ToString() + ".jpg");
                }
                byte[] fileData = System.IO.File.ReadAllBytes(Application.dataPath + "/temp/snapshot" + textureIndex.ToString() + ".jpg");
                canvasTexture.LoadImage(fileData);
                Debug.Log("File was found!");
            }
            else
            {
                Debug.Log("File does not exist!");
            }
            //RawImage canvasTexture = (RawImage)AssetDatabase.LoadAssetAtPath("temp/snapshot" + textureIndex.ToString() + ".jpg", typeof(RawImage));
            if (videoCanvas && canvasTexture)
            {
                videoCanvas.texture = canvasTexture;
            }
            else
            {
                if (videoCanvas == null)
                {
                    Debug.Log("videoCanvas not set.");
                }
                if (canvasTexture == null)
                {
                    Debug.Log("canvasTexture not set.");
                }
            }
        }
    }

    public void Login() {
        if (username == null || password == null) {
            Debug.Log("Inputfields not set.");
            return;
        }

        if (username.text.Length <= 0 || password.text.Length <= 0) {
            Debug.Log("Login info not filled.");
            return;
        }

        //user:testacclin, pass: linphone
        account = new Account(username.text, password.text, "sip.linphone.org");
        phone = new Phone(account);
        phone.connectedEvent += delegate () {
            Debug.Log("Phone connected.");
            callOn = false;
        };
        phone.disconnectedEvent += delegate () {
            Debug.Log("Phone disconnected.");
            callOn = false;
        };
        phone.loginErrorEvent += delegate (Phone.RegisterError error_type, string message) {
            Debug.Log("Failed login. " + message);
        };
        phone.IncomingRingingEvent += delegate (Call call) {
            Debug.Log("Incoming call!");
            callName = call.from;
        };
        phone.OutgoingRingingEvent += delegate (Call call) {
            Debug.Log("Outgoing call!");
            callName = call.to;
        };
        phone.StreamsRunningEvent += delegate (Call call) {
            Debug.Log("Answered. Call is active!");
            callOn = true;
        };
        phone.EndedEvent += delegate (Call call) {
            Debug.Log("Completed.");
            callOn = false;
        };
        phone.Connect();
    }

    public void Call() {
        if (phone == null || callee == null) {
            Debug.Log("Not logged in or callee inputfield not set.");
            return;
        }

        if (callee.text.Length <= 0) {
            Debug.Log("Callee info not filled.");
            return;
        }

        phone.makeCall(callee.text);
        Debug.Log("Calling...");
    }

    public void Answer() {
        if (phone == null) {
            Debug.Log("Not logged in.");
            return;
        }

        phone.answerCall();
        Debug.Log("Answering...");
    }

    public void HangUp() {
        if (phone == null) {
            Debug.Log("Not logged in.");
            return;
        }

        phone.hangupCall();
        Debug.Log("Hanging up...");
    }

    public void Disconnect() {
        if (phone == null) {
            Debug.Log("Not logged in.");
            return;
        }

        phone.Disconnect();
        Debug.Log("Disconnecting...");

        account = null;
        phone = null;
    }
}
