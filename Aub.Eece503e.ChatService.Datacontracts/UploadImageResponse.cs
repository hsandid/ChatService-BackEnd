using System;
using System.Collections.Generic;
using System.Text;

namespace Aub.Eece503e.ChatService.Datacontracts
{
    public class UploadImageResponse
    {
        public string ImageId { get; set; }

        public override bool Equals(object obj)
        {
            return obj is UploadImageResponse image &&
                   ImageId == image.ImageId;
        }

        public override int GetHashCode()
        {
            var hashCode = -256925990;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ImageId);
            return hashCode;
        }
    }
}
