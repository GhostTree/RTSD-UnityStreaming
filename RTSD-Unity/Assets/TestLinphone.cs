using UnityEngine;
using System.Collections;
using LiblinphonedotNET;

public class TestLinphone : MonoBehaviour {
    Account account;
    Phone phone;

    void Start () {
        account = new Account("testacclin", "linphone", "sip.linphone.org");
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
        };
        phone.OutgoingRingingEvent += delegate (Call call) {
            Debug.Log("Outgoing call!");
        };
        phone.StreamsRunningEvent += delegate (Call call) {
            Debug.Log("Answered. Call is active!");
        };
        phone.EndedEvent += delegate (Call call) {
            Debug.Log("Completed.");
        };
        phone.Connect();
    }
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.A)) {
            phone.makeCall("throwaway2016");
            Debug.Log("Calling...");
        }
        if (Input.GetKeyDown(KeyCode.B)) {
            phone.answerCall();
            Debug.Log("Answering...");
        }
        if (Input.GetKeyDown(KeyCode.Return)) {
            phone.Disconnect();
            Debug.Log("Disconnecting...");
        }
	}
}
