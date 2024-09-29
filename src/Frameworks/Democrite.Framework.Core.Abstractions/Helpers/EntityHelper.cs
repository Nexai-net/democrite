// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Helpers
{
    using Democrite.Framework.Core.Abstractions.Repositories;

    using Elvex.Toolbox.Models;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// 
    /// </summary>
    public static class EntityHelper
    {
        #region Fields

        private static readonly Dictionary<Type, MethodInfo> s_entityExtractCache;
        private static readonly MethodInfo s_genericGetEntityId;
        private static readonly ReaderWriterLockSlim s_locker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="EntityHelper"/> class.
        /// </summary>
        static EntityHelper()
        {
            Expression<Func<IEntityWithId<Guid>, Guid>> tmpGetEntityId = e => GetEntityId<IEntityWithId<Guid>, Guid>(null!);
             s_genericGetEntityId = ((MethodCallExpression)tmpGetEntityId.Body).Method.GetGenericMethodDefinition();

            s_locker = new ReaderWriterLockSlim();
            s_entityExtractCache = new Dictionary<Type, MethodInfo>();
        }

        #endregion

        #region Methods        

        /// <summary>
        /// Gets the entity identifier from a generic type object
        /// </summary>
        public static object? GetEntityId(object? entity)
        {
            if (entity is null || entity is not IEntityWithId)
                return null;

            var key = entity.GetType();

            s_locker.EnterReadLock();
            try
            {
                if (s_entityExtractCache.TryGetValue(key, out var cachedMethod))
                    return cachedMethod.Invoke(null, new object[] { entity });
            }
            finally
            {
                s_locker.ExitReadLock();
            }

            var entityIdAbstractType = key.GetAllCompatibleAbstractTypes()
                                          .FirstOrDefault(k => k is ConcretType cc &&
                                                               cc.IsInterface &&
                                                               cc.IsGenericComposed &&
                                                               cc.GenericParameters.Any() &&
                                                               cc.DisplayName.StartsWith("IEntityWithId<")) as ConcretType;

            if (entityIdAbstractType is null)
                return null;

            var mtdh = s_genericGetEntityId.MakeGenericMethodWithCache(key, entityIdAbstractType.GenericParameters.First().ToType());

            s_locker.EnterWriteLock();
            try
            {
                s_entityExtractCache[key] = mtdh;
                return mtdh.Invoke(null, new object[] { entity });
            }
            finally
            {
                s_locker.ExitWriteLock();
            }
        }

        #region Tools

        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        public static TEntityId GetEntityId<TEntity, TEntityId>(TEntity entity)
            where TEntity : IEntityWithId<TEntityId>
            where TEntityId : IEquatable<TEntityId>
        {
            return entity.Uid;
        }

        #endregion

        #endregion
    }
}
