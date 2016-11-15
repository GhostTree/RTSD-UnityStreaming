using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace LiblinphonedotNET
{
    public class ChatRoomHandler
    {
        List<ChatRoom> chat_rooms;

        public ChatRoomHandler()
        {
            chat_rooms = new List<ChatRoom>();
        }

        /// <summary>
        /// Takes a "linphone chat room" and "linphone msg" as IntPtr:s and adds the msg to the correct ChatRoom 
        /// </summary>
        /// <param name="chat_room_param">linphone chat room IntPtr</param>
        /// <param name="message">linphone message IntPtr</param>
        public void receiveMessage(string peer, IntPtr chat_room_param, IntPtr message)
        {
            ChatRoom chat_room = findChatRoom(chat_room_param);
            if (chat_room == null)
            {
                chat_room = new ChatRoom(chat_room_param, peer);
                chat_room.addMessage(message);
                chat_rooms.Add(chat_room);
            }
            else
            {
                chat_room.addMessage(message);
            }
        }

        //Collection controls
        public int Count()
        {
            return chat_rooms.Count();
        }
        public ChatRoom findChatRoom(IntPtr chat_room_param)
        {
            for (int iterator = 0; iterator < chat_rooms.Count; iterator++)
                if (chat_rooms[iterator].isSame(chat_room_param))
                    return chat_rooms[iterator];
            return null;
        }
        public void destroyChatRoom(IntPtr chat_room_param)
        {
            for (int iterator = 0; iterator < chat_rooms.Count; iterator++)
                if (chat_rooms[iterator].isSame(chat_room_param))
                    chat_rooms.RemoveAt(iterator);
        }
        public void addChatRoom(string name, IntPtr chat_room)
        {
            if (findChatRoom(chat_room) == null)
                chat_rooms.Add(new ChatRoom(chat_room, name));
        }
    }
}
