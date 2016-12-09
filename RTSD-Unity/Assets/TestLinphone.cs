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

    public GameObject loginPanel;
    public GameObject callPanel;
    public GameObject logoutPanel;
    public GameObject incomingPanel;
    public GameObject outgoingPanel;
    public GameObject infoPanel;

    float toNextSnapShot;
    float currentTime;
	bool takeSnapshot;

    long snapshotIndex;
    long textureIndex;
    RawImage videoCanvas;
    RawImage videoImage;
    Texture2D canvasTexture;

    Account account = null;
    Phone phone = null;
    bool callOn = false;
    string callName = "";

    enum InterfaceState {
        Null,
        Login,
        Base,
        Incoming,
        Outgoing,
        Video
    }

    InterfaceState state;
    InterfaceState lastState;

    void Start () {
		toNextSnapShot = 1.0f;
        currentTime = 0.0f;
        snapshotIndex = 0;
        textureIndex = 0;
        videoCanvas = GameObject.Find("Video").GetComponent<RawImage>();//(RawImage)GameObject.Find("Video").GetComponent<RawImage>();
		takeSnapshot = true;

        canvasTexture = new Texture2D(2, 2);
        //videoImage= videoCanvas.GetComponent<RawImage>();

        state = InterfaceState.Login;
        lastState = InterfaceState.Null;
        DisablePanels();
    }

    void OnDestroy()
    {
        if (phone != null)
        {
            phone.hangupCall();
            phone.Disconnect();
        }
    }

    void DisablePanels() {
        loginPanel.SetActive(false);
        callPanel.SetActive(false);
        logoutPanel.SetActive(false);
        incomingPanel.SetActive(false);
        outgoingPanel.SetActive(false);
        infoPanel.SetActive(false);
    }

    void Update () {
        if (state != lastState) {
            DisablePanels();
            switch (state) {
                case InterfaceState.Login:
                    loginPanel.SetActive(true);
                    break;
                case InterfaceState.Base:
                    callPanel.SetActive(true);
                    logoutPanel.SetActive(true);
                    break;
                case InterfaceState.Incoming:
                    incomingPanel.SetActive(true);
                    break;
                case InterfaceState.Outgoing:
                    outgoingPanel.SetActive(true);
                    break;
                case InterfaceState.Video:
                    infoPanel.SetActive(true);
                    break;
            }
            lastState = state;
        }

        if (incomingName != null && outgoingName != null && infoName != null) {
            incomingName.text = callName;
            outgoingName.text = callName;
            infoName.text = callName;
        }

        if (callOn) {
            currentTime += Time.deltaTime;
			if (takeSnapshot)//currentTime >= toNextSnapShot) 
			{
                currentTime = 0;
                if (phone != null) {
					phone.SnapShot (Application.dataPath + "/temp/snapshot.jpg");//" + snapshotIndex.ToString()+".jpg");
                    //Debug.Log("Saving screenshot to " + Application.dataPath + "/temp/snapshot" + snapshotIndex.ToString() + ".jpg");
                    snapshotIndex++;
					takeSnapshot = false;
                    //textureIndex = snapshotIndex - 1;


                }
            }
            //Texture2D canvasTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("temp/snapshot" + textureIndex.ToString() + ".jpg", typeof());
            
			if (System.IO.File.Exists(Application.dataPath + "/temp/snapshot.jpg"))// + textureIndex.ToString() + ".jpg"))
            {
				/*
                if(System.IO.File.Exists(Application.dataPath + "/temp/snapshot" + (textureIndex-1).ToString() + ".jpg"))
                {
                    //BE CAREFULL WITH THIS ONE!
                    System.IO.File.Delete(Application.dataPath + "/temp/snapshot" + (textureIndex - 1).ToString() + ".jpg");
                }*/
				byte[] fileData = System.IO.File.ReadAllBytes(Application.dataPath + "/temp/snapshot.jpg");// + textureIndex.ToString() + ".jpg");
                canvasTexture.LoadImage(fileData);
                //Debug.Log("File was found!");
				if (videoCanvas && canvasTexture)
				{
					videoCanvas.texture = canvasTexture;
					System.IO.File.Delete(Application.dataPath + "/temp/snapshot.jpg");
					takeSnapshot = true;
					textureIndex++;
				}
				else
				{
					if (videoCanvas == null)
					{
						Debug.Log("videoCanvas nt set.");
					}
					if (canvasTexture == null)
					{
						Debug.Log("canvasTexture not set.");
					}
				}
            }
            else
            {
                //Debug.Log("File does not exist!");
            }
            //RawImage canvasTexture = (RawImage)AssetDatabase.LoadAssetAtPath("temp/snapshot" + textureIndex.ToString() + ".jpg", typeof(RawImage));
           
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
            state = InterfaceState.Base;
        };
        phone.disconnectedEvent += delegate () {
            Debug.Log("Phone disconnected.");
            callOn = false;
            state = InterfaceState.Login;
        };
        phone.loginErrorEvent += delegate (Phone.RegisterError error_type, string message) {
            Debug.Log("Failed login. " + message);
        };
        phone.IncomingRingingEvent += delegate (Call call) {
            Debug.Log("Incoming call!");
            callName = call.from;
            state = InterfaceState.Incoming;
        };
        phone.OutgoingRingingEvent += delegate (Call call) {
            Debug.Log("Outgoing call!");
            callName = call.to;
            state = InterfaceState.Outgoing;
        };
        phone.StreamsRunningEvent += delegate (Call call) {
            Debug.Log("Answered. Call is active!");
            callOn = true;
            state = InterfaceState.Video;
        };
        phone.EndedEvent += delegate (Call call) {
            Debug.Log("Completed.");
            callOn = false;
            state = InterfaceState.Base;
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
