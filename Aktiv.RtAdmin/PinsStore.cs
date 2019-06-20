using System;
using System.Collections.Generic;
using System.IO;

namespace Aktiv.RtAdmin
{
    public class PinsStore
    {
        // TODO: возможно тут надо BlockingCollection
        private Queue<string> pins;

        public bool Initialized { get; private set; }

        public void Load(string pinsFilePath)
        {
            if (!File.Exists(pinsFilePath))
            {
                throw new FileNotFoundException("Нет файла", pinsFilePath);
            }

            pins = new Queue<string>(File.ReadAllLines(pinsFilePath));
            Initialized = true;
        }

        public string GetNextPin()
        {
            if (pins.TryDequeue(out var pin))
            {
                return pin;
            }

            throw new InvalidOperationException("Пин-коды закончились");
        }
    }
}
