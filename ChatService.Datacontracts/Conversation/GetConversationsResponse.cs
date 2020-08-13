using System;
using System.Collections.Generic;
using System.Text;

namespace Aub.Eece503e.ChatService.Datacontracts
{
	public class GetConversationsResponse
	{
		public GetConversationsResponseEntry[] Conversations { get; set; }
		public string NextUri { get; set; }
	}
}
