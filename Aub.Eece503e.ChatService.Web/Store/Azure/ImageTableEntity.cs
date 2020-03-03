using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aub.Eece503e.ChatService.Web.Store.Azure
{
	public class ImageTableEntity: TableEntity
	{
		public byte[] ImageData { get; set; }
	}
}
