using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlickrClient.Helpers
{
    public class HttpHelper
    {
        public delegate byte[] HmacSha1Delegate(byte[] key, byte[] data);

        public static byte[] HmacSha1(byte[] key, byte[] data)
        {
            var crypt = Windows.Security.Cryptography.Core.MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
            var keyBuffer = Windows.Security.Cryptography.CryptographicBuffer.CreateFromByteArray(key);
            var cryptKey = crypt.CreateKey(keyBuffer);

            var dataBuffer = Windows.Security.Cryptography.CryptographicBuffer.CreateFromByteArray(data);
            var signBuffer = Windows.Security.Cryptography.Core.CryptographicEngine.Sign(cryptKey, dataBuffer);

            byte[] result;
            Windows.Security.Cryptography.CryptographicBuffer.CopyToByteArray(signBuffer, out result);

            return result;
        }

    }
}
