﻿using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System;
using System.Text;

namespace Aktiv.RtAdmin
{
    public static class Helpers
    {
        public static string UTF8ToCp1251(string sourceStr)
        {
            var utf8 = Encoding.UTF8;
            var win1251 = Encoding.GetEncoding("windows-1251");
            var utf8Bytes = utf8.GetBytes(sourceStr);
            var win1251Bytes = Encoding.Convert(utf8, win1251, utf8Bytes);
            return win1251.GetString(win1251Bytes);
        }

        public static string StringToUtf8String(string source)
        {
            var bytes = Encoding.Default.GetBytes(source);
            var utf8 = Encoding.UTF8;

            var utf8Bytes = Encoding.Convert(Encoding.Default, utf8, bytes);

            return utf8.GetString(utf8Bytes);
        }

        public static string StringToCp1251String(string source)
        {
            var bytes = Encoding.Default.GetBytes(source);
            var win1251 = Encoding.GetEncoding("windows-1251");

            var win1251Bytes = Encoding.Convert(Encoding.Default, win1251, bytes);

            return win1251.GetString(win1251Bytes);
        }

        public static Slot GetUsableSlot(Pkcs11 pkcs11)
        {
            // Получить список слотов c подключенными токенами
            var slots = pkcs11.GetSlotList(SlotsType.WithTokenPresent);

            // Проверить, что слоты найдены
            if (slots == null)
            {
                throw new NullReferenceException("No available slots");
            }

            // Проверить, что число слотов больше 0
            if (slots.Count <= 0 )
            {
                throw new InvalidOperationException("No available slots");
            }

            // Получить первый доступный слот
            var slot = slots[0];

            return slot;
        }

        public static void PrintByteArray(byte[] array)
        {
            var hexString = new StringBuilder();
            var width = 16;
            int byteCounter = 1;
            foreach (var item in array)
            {
                hexString.AppendFormat(" 0x{0:x2}", item);
                if (byteCounter == width)
                {
                    hexString.AppendLine();
                    byteCounter = 0;
                }
                byteCounter++;
            }

            Console.WriteLine(hexString);
        }
    }
}
