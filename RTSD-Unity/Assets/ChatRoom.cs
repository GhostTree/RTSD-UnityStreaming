using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace LiblinphonedotNET
{
    public class ChatRoom
    {
        string name;
        IntPtr chat_room;
        List<LinphoneMessage> messages;

        public ChatRoom(IntPtr chat_room, string name)
        {
            this.name = name;
            this.chat_room = chat_room;
            this.messages = new List<LinphoneMessage>();
        }

        public string getPeer()
        {
            return name;
        }

        public LinphoneMessage parseMessage(IntPtr msg)
        {     
            //time
            long time = CoreWrapper.linphone_chat_message_get_time(msg);
            DateTime formatted_time = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(time);
            //+2 gmt
            formatted_time = formatted_time.AddHours(2);

            //message
            IntPtr text_pointer = CoreWrapper.linphone_chat_message_get_text(msg);
            byte[] bytes = Encoding.Default.GetBytes(Marshal.PtrToStringAnsi(text_pointer));
            string text = Encoding.UTF8.GetString(bytes);

            //sender
            IntPtr address = CoreWrapper.linphone_chat_message_get_from_address(msg);
            string sender = "Self";
            if (address != IntPtr.Zero)
            {
                IntPtr display_name = CoreWrapper.linphone_address_get_username(address);
                sender = Marshal.PtrToStringAnsi(display_name);
            }

            //Return the final product
            return new LinphoneMessage(formatted_time, text, sender);
        }
        public void addMessage(LinphoneMessage msg)
        {
            this.messages.Add(msg);
        }
        public void addMessage(IntPtr msg)
        {
            this.messages.Add(parseMessage(msg));
        }

        public LinphoneMessage getMessage(int index)
        {
            return messages[index];
        }
        public int Count()
        {
            return messages.Count;
        }

        public string getTextLog()
        {
            string text = "";
            foreach (LinphoneMessage msg in messages)
            {
                text += msg.toString() + "\n";
            }
            return text;
        }

        public bool isSame(IntPtr chat_room_param)
        {
            IntPtr address = CoreWrapper.linphone_chat_room_get_peer_address(chat_room_param);
            IntPtr display_name = CoreWrapper.linphone_address_get_username(address);
            string text = Marshal.PtrToStringAnsi(display_name);
            string my_name = this.getPeer();
            return text.Equals(my_name);
        }
    }

    public struct LinphoneMessage
    {
        public DateTime time;
        public string message;
        public string sender;

        public LinphoneMessage(DateTime time, string message, string sender)
        {
            this.time = time;
            this.message = message;
            this.sender = sender;
        }

        public string toString()
        {
            return time.ToShortTimeString() + " " + sender + ": " + message;
        }
    }
}
