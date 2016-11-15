using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace LiblinphonedotNET
{
	public class Phone
	{
		public enum ConnectState
		{
			Disconnected, // Idle
			Progress, // Registering on server
			Connected // Successfull registered
		};
        public enum LineState
        {
            Free,
            Busy
        }

        public enum CallError
        {
            LineIsBusyError, // trying to make/receive call while another call is active
            CallError, // call failed
        };

        public enum RegisterError
        {
            RegisterFailed, // registration error
            OrderError, // trying to connect while connected / connecting or disconnect when not connected
            UnknownError
        };

        //Define register delegates and events
        public delegate void OnPhoneConnected();
        public delegate void OnPhoneDisconnected();
        public delegate void OnRegisterError(RegisterError error, string message);

        public event OnPhoneConnected connectedEvent;
        public event OnPhoneDisconnected disconnectedEvent;
        public event OnRegisterError loginErrorEvent;

        //Define call delegates and events
        //Outgoing events
        public delegate void OnIdle(Call call);
        public delegate void OnOutgoingEarlyMedia(Call call);
        public delegate void OnOutgoingStart(Call call);
        public delegate void OnOutgoingProgress(Call call);
        public delegate void OnOutgoingRinging(Call call);
        //Incoming events
        public delegate void OnIncomingRinging(Call call);
        public delegate void OnIncomingEarlyMedia(Call call);
        //Call init
        public delegate void OnCallConnected(Call call);
        public delegate void OnStreamsRunning(Call call);
        //Call interruptions
        public delegate void OnPausing(Call call);
        public delegate void OnPausedByUs(Call call);
        public delegate void OnPausedByRemote(Call call);
        public delegate void OnResume(Call call);
        //Call parameter changes
        public delegate void OnUpdatedByUs(Call call);
        public delegate void OnUpdatedByRemote(Call call);
        //Call ends for a reason or another
        public delegate void OnReferred(Call call);
        public delegate void OnEnded(Call call);
        public delegate void OnReleased(Call call);
        //Call error
        public delegate void OnCallError(Call call, CallError error, string message);


        //Define events for delegates
        //Outgoing events
        public event OnIdle IdleEvent;
        public event OnOutgoingEarlyMedia OutgoingEarlyMediaEvent;
        public event OnOutgoingStart OutgoingStartEvent;
        public event OnOutgoingProgress InProgressEvent;
        public event OnOutgoingRinging OutgoingRingingEvent;
        //Incoming events
        public event OnIncomingEarlyMedia IncomingEarlyMediaEvent;
        public event OnIncomingRinging IncomingRingingEvent;
        //Call init
        public event OnCallConnected ConnectedEvent;
        public event OnStreamsRunning StreamsRunningEvent;
        //Call interruptions
        public event OnPausing PausingEvent;
        public event OnPausedByUs PausedByUsEvent;
        public event OnPausedByRemote PausedByRemoteEvent;
        public event OnResume ResumeEvent;
        //Call parameter changes
        public event OnUpdatedByUs UpdatedByUsEvent;
        public event OnUpdatedByRemote UpdatedByRemoteEvent;
        //Call ends for a reason or another
        public event OnReferred ReferredEvent;
        public event OnEnded EndedEvent;
        public event OnReleased ReleasedEvent;
        //Call error
        public event OnCallError CallErrorEvent;
        
 
        //Define message delegates and events
        public delegate void OnMessageReceived(ChatRoom room, LinphoneMessage message);
        public event OnMessageReceived MessageReceivedEvent;

        public delegate void OnCoreError();
        public event OnCoreError coreErrorEvent;

        //Variables
	    private string user_agent {
	        get { return "liblinphone"; }
	    }
        private string version {
            get { return "0.1.0"; }
        }

        private ConnectState connect_state;
        private LineState line_state;

	    public Account account;
		private CoreWrapper core_wrapper;
        private ChatRoomHandler chat_room_handler;
        private Call current_call;

		public Phone(Account account)
		{
            this.chat_room_handler = new ChatRoomHandler();
            this.account = account;
			
			this.core_wrapper = new CoreWrapper();
            linkRegistrationStateChangedEventBody();
            linkCallStateChangedEventBody();

            //Create body for message callbacks from core
            core_wrapper.MessageReceivedEvent += receiveMessage;

            //Create body for error callbacks from core
            core_wrapper.ErrorEvent += (call, message) =>
            {
                Console.WriteLine("Error: {0}!", message);
                if (coreErrorEvent != null)
                    coreErrorEvent();
            };
		}
        private void linkRegistrationStateChangedEventBody()
        {
            this.core_wrapper.RegistrationStateChangedEvent += (CoreWrapper.LinphoneRegistrationState state) =>
            {
                switch (state)
                {
                    case CoreWrapper.LinphoneRegistrationState.LinphoneRegistrationProgress:
                        connect_state = ConnectState.Progress;
                        break;

                    case CoreWrapper.LinphoneRegistrationState.LinphoneRegistrationFailed:
                        core_wrapper.destroyPhone();
                        if (loginErrorEvent != null)
                            loginErrorEvent(RegisterError.RegisterFailed, "Login has failed, please check your credentials.");
                        break;

                    case CoreWrapper.LinphoneRegistrationState.LinphoneRegistrationCleared:
                        connect_state = ConnectState.Connected;
                        if (disconnectedEvent != null)
                            disconnectedEvent(); //Trigger disconnect event
                        break;

                    case CoreWrapper.LinphoneRegistrationState.LinphoneRegistrationOk:
                        connect_state = ConnectState.Connected;
                        if (connectedEvent != null)
                            connectedEvent(); //Trigger connected event
                        break;

                    case CoreWrapper.LinphoneRegistrationState.LinphoneRegistrationNone:
                    default:
                        break;
                }
            };
        }
        private void linkCallStateChangedEventBody()
        {
            //Seperate the single CallStateChangedEvent event from core into CallState events
            core_wrapper.CallStateChangedEvent += (Call call) =>
            {
                current_call = call;
                Call.State state = call.state;
                switch (state)
                {
                    case Call.State.Idle:
                    case Call.State.OutgoingEarlyMedia:
                        line_state = LineState.Busy;
                        if (OutgoingEarlyMediaEvent != null)
                            OutgoingEarlyMediaEvent(call);
                        break;
                    case Call.State.OutgoingStart:
                        line_state = LineState.Busy;
                        if (OutgoingStartEvent != null)
                            OutgoingStartEvent(call);
                        break;
                    case Call.State.InProgress:
                        line_state = LineState.Busy;
                        if (InProgressEvent != null)
                            InProgressEvent(call);
                        break;
                    case Call.State.OutgoingRinging:
                        line_state = LineState.Busy;
                        if (OutgoingRingingEvent != null)
                            OutgoingRingingEvent(call);
                        break;

                    case Call.State.IncomingRinging:
                        line_state = LineState.Busy;
                        if (IncomingRingingEvent != null)
                            IncomingRingingEvent(call);
                        break;
                    case Call.State.IncomingEarlyMedia:
                        line_state = LineState.Busy;
                        if (IncomingEarlyMediaEvent != null)
                            IncomingEarlyMediaEvent(call);
                        break;

                    case Call.State.Connected:
                        line_state = LineState.Busy;
                        if (ConnectedEvent != null)
                            ConnectedEvent(call);
                        break;
                    case Call.State.StreamsRunning:
                        line_state = LineState.Busy;
                        if (StreamsRunningEvent != null)
                            StreamsRunningEvent(call);
                        break;

                    case Call.State.Pausing:
                        if (PausingEvent != null)
                            PausingEvent(call);
                        break;
                    case Call.State.PausedByUs:
                        if (PausedByUsEvent != null)
                            PausedByUsEvent(call);
                        break;
                    case Call.State.PausedByRemote:
                        if (PausedByRemoteEvent != null)
                            PausedByRemoteEvent(call);
                        break;
                    case Call.State.Resuming:
                        if (ResumeEvent != null)
                            ResumeEvent(call);
                        break;

                    case Call.State.UpdatedByUs:
                        if (UpdatedByUsEvent != null)
                            UpdatedByUsEvent(call);
                        break;
                    case Call.State.UpdatedByRemote:
                        if (UpdatedByRemoteEvent != null)
                            UpdatedByRemoteEvent(call);
                        break;

                    case Call.State.Referred:
                        if (ReferredEvent != null)
                            ReferredEvent(call);
                        break;
                    case Call.State.Ended:
                        if (EndedEvent != null)
                            EndedEvent(call);
                        break;
                    case Call.State.Released:
                        line_state = LineState.Free;
                        if (ReleasedEvent != null)
                            ReleasedEvent(call);
                        break;

                    case Call.State.Error:
                        if (CallErrorEvent != null)
                            CallErrorEvent(call, CallError.CallError, "Call has encountered an error.");
                        if (ReleasedEvent != null)
                            ReleasedEvent(call);
                        break;
                }
            };
        }

        private string getSenderUsername(IntPtr message)
        {
            IntPtr address = CoreWrapper.linphone_chat_message_get_from_address(message);
            IntPtr username = CoreWrapper.linphone_address_get_username(address);
            return Marshal.PtrToStringAnsi(username);
        }

        public void Connect()
		{
            if (this.connect_state == ConnectState.Disconnected)
            {
                this.connect_state = ConnectState.Progress;
                this.core_wrapper.createPhone(this.account.Username, this.account.Password, this.account.Server, this.account.Port, this.user_agent, this.version);
            }
            else if (loginErrorEvent != null)
                loginErrorEvent(RegisterError.OrderError, "Cannot connect a phone that is already connected.");
        }
        public void Disconnect()
        {
            if (connect_state == ConnectState.Connected)
                this.core_wrapper.destroyPhone();
            else if (loginErrorEvent != null)
                loginErrorEvent(RegisterError.OrderError, "Cannot disconnect a phone that is not connected.");
        }

        public void makeCall(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                throw new ArgumentNullException("uri");

            if (line_state == LineState.Free)
                core_wrapper.makeCall(uri);
            else if (CallErrorEvent != null)
                CallErrorEvent(null, CallError.LineIsBusyError, "The line is busy.");
        }
        public void answerCall()
        {
            if (current_call.state == Call.State.IncomingRinging)
                core_wrapper.answerCall(current_call);
        }
        public void hangupCall()
        {
            if (current_call.state == Call.State.StreamsRunning)
                core_wrapper.hangupCall(current_call);
            if (current_call.state == Call.State.IncomingRinging)
                core_wrapper.declineCall(current_call);
        }

		public void sendMessage(string uri, string raw_message)
		{
            if (string.IsNullOrEmpty(uri))
                throw new ArgumentNullException("uri");

            if (raw_message.Length == 0)
                return;

            IntPtr chat_room = core_wrapper.getChatRoom(uri);
            IntPtr message = CoreWrapper.linphone_chat_room_create_message(chat_room, raw_message);
            chat_room_handler.receiveMessage(uri.Split('@')[0].Split(':')[1], chat_room, message);
			CoreWrapper.linphone_chat_room_send_chat_message(chat_room, message);
		}
		private void receiveMessage(IntPtr chat_room, IntPtr message)
		{
            chat_room_handler.receiveMessage(getSenderUsername(message), chat_room, message);
            if (MessageReceivedEvent != null)
			{
                ChatRoom updated_chat_room = chat_room_handler.findChatRoom(chat_room);
                MessageReceivedEvent(updated_chat_room, updated_chat_room.getMessage(updated_chat_room.Count() - 1));
			}
		}

        public ChatRoom getCurrentChatRoom(string uri)
        {
            IntPtr chat_room = core_wrapper.getChatRoom(uri);
            return chat_room_handler.findChatRoom(chat_room);
        }
	}
}