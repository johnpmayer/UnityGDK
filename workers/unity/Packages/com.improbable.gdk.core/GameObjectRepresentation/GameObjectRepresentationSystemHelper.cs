﻿using Unity.Entities;

namespace Improbable.Gdk.GameObjectRepresentation
{
    public static class GameObjectRepresentationSystemHelper
    {
        public static void AddSystems(World world)
        {
            world.GetOrCreateManager<GameObjectDispatcherSystem>();
            world.GetOrCreateManager<EntityGameObjectLinkerSystem>();
            world.GetOrCreateManager<GameObjectWorldCommandSystem>();
        }
    }
}
