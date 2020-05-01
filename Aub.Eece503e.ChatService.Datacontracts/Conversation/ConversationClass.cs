using System;
using System.Collections.Generic;
using System.Text;

namespace Aub.Eece503e.ChatService.Datacontracts
{
    //Rename this class !!!
    public class ConversationClass
    {
        public string[] Participants { get; set;}
        public long LastModifiedUnixTime { get; set;}
    }
}
