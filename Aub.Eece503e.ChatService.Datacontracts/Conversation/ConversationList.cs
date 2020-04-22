using System;
using System.Collections.Generic;
using System.Text;

namespace Aub.Eece503e.ChatService.Datacontracts
{
	public class ConversationList
	{
		public GetConversationsResponseEntry[] Conversations { get; set; }
		public string ContinuationToken { get; set; }
	}
}
