using System.Text;

namespace Aktiv.RtAdmin
{
    public static class Helpers
    {
        public static byte[] UTF8ToCp1251(string sourceStr)
        {
            var utf8 = Encoding.UTF8;
            var win1251 = Encoding.GetEncoding("windows-1251");
            var utf8Bytes = utf8.GetBytes(sourceStr);
            return Encoding.Convert(utf8, win1251, utf8Bytes);
        }
    }
}
