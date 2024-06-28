// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Models
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Models.Surrogates;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// State used to store blackboard minial information for retreive and creation
    /// </summary>
    internal sealed class BlackboardRegistryState
    {
        private readonly Dictionary<Guid, BlackboardId> _boardIndexedIds;
        private readonly Dictionary<string, Guid> _indexedByCoupleNameBoardTmpl;
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardRegistryState"/> class.
        /// </summary>
        public BlackboardRegistryState(IEnumerable<BlackboardId> boardIds)
        {
            var boardIndexedIds = boardIds?.ToDictionary(kv => kv.Uid, kv => kv) ?? new Dictionary<Guid, BlackboardId>(42);

            var indexedByCoupleNameBoardTmpl = boardIds?.GroupBy(kv => GenerateNamesKey(kv.BoardName, kv.BoardTemplateKey))
                                                        .ToDictionary(kv => kv.Key, kv => kv.Single().Uid) ?? new Dictionary<string, Guid>();

            // Assign after to ensure recorded blackboard are unique by uid and couple  kv.BoardName + "/" + kv.BoardTemplateKey
            this._boardIndexedIds = boardIndexedIds;
            this._indexedByCoupleNameBoardTmpl = indexedByCoupleNameBoardTmpl;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Try to get a board by his unique id
        /// </summary>
        public BlackboardId? TryGetAsync(Guid uid)
        {
            if (this._boardIndexedIds.TryGetValue(uid, out var board))
                return board;

            return null;
        }

        /// <summary>
        /// Try to get a board by his unique couple <paramref name="boardName"/> '/' <paramref name="blackboardTemplateKey"/>
        /// </summary>
        public BlackboardId? TryGet(string boardName, string blackboardTemplateKey)
        {
            var key = GenerateNamesKey(boardName, blackboardTemplateKey);
            if (this._indexedByCoupleNameBoardTmpl.TryGetValue(key, out var boardUid))
                return this._boardIndexedIds[boardUid];

            return null;
        }

        /// <summary>
        /// Converts to surrogate.
        /// </summary>
        public BlackboardRegistryStateSurrogate ToSurrogate()
        {
            return new BlackboardRegistryStateSurrogate()
            {
                BoardIds = this._boardIndexedIds.Values.ToReadOnly()
            };
        }

        /// <summary>
        /// Unregisters a black board from the registry
        /// </summary>
        public void Unregister(Guid uid)
        {
            var item = this._indexedByCoupleNameBoardTmpl.FirstOrDefault(k => k.Value == uid);

            if (!string.IsNullOrEmpty(item.Key))
                this._indexedByCoupleNameBoardTmpl.Remove(item.Key);

            this._boardIndexedIds.Remove(uid);
        }

        /// <summary>
        /// Creates the new blackboard identifier.
        /// </summary>
        public BlackboardId CreateNewBlackboardId(string boardName, string blackboardTemplateKey)
        {
            var existingId = TryGet(boardName, blackboardTemplateKey);
            if (existingId is not null)
                return existingId.Value;

            Guid id;
            do
            {
                id = Guid.NewGuid();
                
                // Loop to ensure the unicity of the id
            } while (this._boardIndexedIds.ContainsKey(id));

            var boardId = new BlackboardId(id, boardName, blackboardTemplateKey);
            
            this._boardIndexedIds.Add(id, boardId);
            this._indexedByCoupleNameBoardTmpl.Add(GenerateNamesKey(boardName, blackboardTemplateKey), id);

            return boardId;
        }

        #region Tools

        /// <summary>
        /// Generates the names key.
        /// </summary>
        private static string GenerateNamesKey(string boardName, string boardTemplateKey)
        {
            return boardName + "/" + boardTemplateKey;
        }

        #endregion

        #endregion
    }
}
