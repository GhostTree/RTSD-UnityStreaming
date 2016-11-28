using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LiblinphonedotNET;

public class TestLinphone : MonoBehaviour {
    public InputField username;
    public InputField password;
    public InputField callee;

    public Text incomingName;
    public Text outgoingName;
    public Text infoName;

    Account account = null;
    Phone phone = null;
    string callName = "";

    void Start () {

    }
	
	void Update () {
        if (incomingName != null && outgoingName != null) {
            incomingName.text = callName;
            outgoingName.text = callName;
            infoName.text = callName;
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
        };
        phone.disconnectedEvent += delegate () {
            Debug.Log("Phone disconnected.");
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
        };
        phone.EndedEvent += delegate (Call call) {
            Debug.Log("Completed.");
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
