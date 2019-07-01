using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aktiv.RtAdmin.Properties;

namespace Aktiv.RtAdmin
{
    // TODO: сделать базовый класс для Store
    public class ConfigLinesStore
    {
        // TODO: возможно тут надо BlockingCollection
        private Queue<string> _configLines;

        public bool Initialized { get; private set; }

        public void Load(string configFilePath)
        {
            var lines = File.ReadAllLines(configFilePath);
            if (!lines.Any())
            {
                throw new InvalidOperationException(Resources.ConfigFileIsEmpty);
            }

            _configLines = new Queue<string>(lines);

            Initialized = true;
        }

        public string GetNext()
        {
            if (_configLines.TryDequeue(out var configLine))
            {
                return configLine;
            }

            // TODO: описание ошибки
            throw new InvalidOperationException();
        }
    }
}
