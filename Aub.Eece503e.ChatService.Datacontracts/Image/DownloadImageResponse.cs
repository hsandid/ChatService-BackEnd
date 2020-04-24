using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aub.Eece503e.ChatService.Datacontracts
{
    public class DownloadImageResponse
    {
        public byte[] ImageData { get; set; }

        public override bool Equals(object obj)
        {
            return obj is DownloadImageResponse image &&
                   ImageData.SequenceEqual(image.ImageData);
        }

        public override int GetHashCode()
        {
            var hashCode = -256925990;
            hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(ImageData);
            return hashCode;
        }
    }
}
