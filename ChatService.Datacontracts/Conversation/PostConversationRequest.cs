using System;
using System.Collections.Generic;
using System.Text;

namespace Aub.Eece503e.ChatService.Datacontracts
{
	public class PostConversationRequest
	{
		public string[] Participants { get; set; }
		public PostMessageRequest FirstMessage { get; set; }
	}
}
