using System;
using System.Collections.Generic;
using System.Linq;

namespace Hp2BaseMod;

public class GameStateFactoryRegistry
{
    private interface IFactoryAdapter
    {
        RelativeId Id {get;}
        Type PayloadType {get;}
        public IGameStateFactory Factory {get;}
        IGameState Create(object payload);
    }

    private class FactoryAdapter<T>(IGameStateFactory<T> factory, RelativeId id, Type payloadType) : IFactoryAdapter
        where T : IGameStatePayload
    {
        public RelativeId Id => _id;
        private readonly RelativeId _id = id;

        public Type PayloadType => _payloadType;
        private readonly Type _payloadType = payloadType;

        public IGameStateFactory Factory => _factory;
        private readonly IGameStateFactory<T> _factory = factory;

        public IGameState Create(object payload)
        {
            return _factory.Create((T)payload);
        }
    }

    private readonly Dictionary<Type, IFactoryAdapter> _factoriesByPayloadType = new();
    private readonly Dictionary<RelativeId, IFactoryAdapter> _factoriesById = new();

    public void Register<T>(RelativeId id, IGameStateFactory<T> factory)
        where T : IGameStatePayload
    {
        if (_factoriesByPayloadType.ContainsKey(typeof(T)))
            throw new InvalidOperationException(
                $"A factory is already registered for payload type {typeof(T)}.");

        if (_factoriesById.ContainsKey(id))
            throw new InvalidOperationException(
                $"A factory is already registered for ID {id}.");

        var payloadType = typeof(T);

        var adapter = new FactoryAdapter<T>(factory, id, payloadType);

        _factoriesByPayloadType.Add(payloadType, adapter);
        _factoriesById.Add(id, adapter);
    }

    public IGameState Create(IGameStatePayload payload)
    {
        if (payload == null) throw new ArgumentNullException(nameof(payload));

        if (!_factoriesByPayloadType.TryGetValue(payload.GetType(), out var factory))
        {
            throw new InvalidOperationException($"No factory registered for payload type {payload.GetType()}.");
        }

        return factory.Create(payload);
    }

    public bool TryGetFactory(RelativeId id, out IGameStateFactory factory)
    {
        if (!_factoriesById.TryGetValue(id, out var adapter))
        {
            factory = null;
            return false;
        }

        factory = adapter.Factory;
        return true;
    }

    public bool TryGetFactory(Type payloadType, out IGameStateFactory factory)
    {
        if (!_factoriesByPayloadType.TryGetValue(payloadType, out var adapter))
        {
            factory = null;
            return false;
        }

        factory = adapter.Factory;
        return true;
    }

    public bool TryGetId(Type payloadType, out RelativeId id)
    {
        if (!_factoriesByPayloadType.TryGetValue(payloadType, out var adapter))
        {
            id = RelativeId.Default;
            return false;
        }

        id = adapter.Id;
        return true;
    }

    public bool TryGetPayloadType(RelativeId id, out Type payloadType)
    {
        if (!_factoriesById.TryGetValue(id, out var adapter))
        {
            payloadType = default;
            return false;
        }

        payloadType = adapter.PayloadType;
        return true;
    }
}
