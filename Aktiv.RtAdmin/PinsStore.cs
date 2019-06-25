using System;
using System.Collections.Generic;
using System.IO;
using Aktiv.RtAdmin.Properties;

namespace Aktiv.RtAdmin
{
    public class PinsStore
    {
        // TODO: возможно тут надо BlockingCollection
        private Queue<string> _pins;

        public bool Initialized { get; private set; }

        public void Load(string pinsFilePath)
        {
            if (!File.Exists(pinsFilePath))
            {
                throw new FileNotFoundException(Resources.PinCodesFileNotFound, pinsFilePath);
            }

            _pins = new Queue<string>(File.ReadAllLines(pinsFilePath));
            if (_pins.Count % 2 != 0)
            {
                throw new InvalidOperationException(Resources.IncorrectPinCodesCount);
            }

            Initialized = true;
        }

        public string GetNextPin()
        {
            if (_pins.TryDequeue(out var pin))
            {
                return pin;
            }

            throw new InvalidOperationException(Resources.PinsEnded);
        }
    }
}
