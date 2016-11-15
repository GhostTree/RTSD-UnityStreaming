using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//PODO Namespace possible naming issue, change if something arises
namespace LiblinphonedotNET
{
    public class Call
    {
        public enum CallType
        {
            None,
            Incoming,
            Outcoming
        }
        public enum State
        {
            Idle,
            OutgoingEarlyMedia,
            OutgoingStart,
            InProgress,
            OutgoingRinging,

            IncomingRinging,
            IncomingEarlyMedia,

            Connected,
            StreamsRunning,

            Pausing,
            PausedByUs,
            PausedByRemote,
            Resuming,
            
            UpdatedByUs,
            UpdatedByRemote,

            Referred,
            Ended,
            Released,

            Error
        }

        public CallType call_type { get; set; }
        public State state { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string peer_name { get; set; }

        public Call()
        {
            call_type = CallType.None;
            state = State.Idle;

            from = "";
            to = "";

            peer_name = "Undefined";
        }
    }
}
