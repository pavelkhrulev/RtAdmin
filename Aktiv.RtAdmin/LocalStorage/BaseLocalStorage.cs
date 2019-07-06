﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Aktiv.RtAdmin
{
    public abstract class BaseLocalStorage
    {
        private readonly string _fileNotFoundMessage;
        private readonly string _incorrentEntitiesCountMessage;
        private readonly string _entitiesHaveEndedMessage;

        protected BaseLocalStorage(
            string fileNotFoundMessage, 
            string incorrentEntitiesCountMessage, 
            string entitiesHaveEndedMessage)
        {
            _fileNotFoundMessage = fileNotFoundMessage;
            _incorrentEntitiesCountMessage = incorrentEntitiesCountMessage;
            _entitiesHaveEndedMessage = entitiesHaveEndedMessage;
        }

        public bool Initialized { get; private set; }

        protected Queue<string> Entities { get; private set; }

        public void Load(string storageFilePath)
        {
            if (!File.Exists(storageFilePath))
            {
                throw new FileNotFoundException(_fileNotFoundMessage, storageFilePath);
            }

            Entities = new Queue<string>(File.ReadAllLines(storageFilePath));

            if (Entities.Count == 0)
            {
                throw new InvalidOperationException(_incorrentEntitiesCountMessage);
            }

            ValidateAfterLoad();

            Initialized = true;
        }

        public string GetNext()
        {
            if (Entities.TryDequeue(out var entity))
            {
                return entity;
            }

            throw new InvalidOperationException(_entitiesHaveEndedMessage);
        }

        protected virtual void ValidateAfterLoad() { }
    }
}