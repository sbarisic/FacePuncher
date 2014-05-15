﻿using System;
using System.Collections.Generic;
using System.Linq;

using FacePuncher.Entities;

namespace FacePuncher.Geometry
{
    public enum TileState
    {
        Void = 0,
        Wall = 1,
        Floor = 2
    }

    /// <summary>
    /// Class representing an individual tile in a room.
    /// </summary>
    public class Tile : IEnumerable<Entity>
    {
        public static readonly Tile Default = new Tile(null, 0, 0);

        private TileState _state;
        private List<Entity> _entities;

        /// <summary>
        /// Parent room containing this tile.
        /// </summary>
        public Room Room { get; private set; }

        /// <summary>
        /// Position of the tile relative to its containing room.
        /// </summary>
        public Position RelativePosition { get; private set; }

        public int RelativeX { get { return RelativePosition.X; } }
        public int RelativeY { get { return RelativePosition.Y; } }

        public Position Position { get { return Room.Rect.TopLeft + RelativePosition; } }

        public int X { get { return Room.Left + RelativeX; } }
        public int Y { get { return Room.Top + RelativeY; } }

        public TileState State
        {
            get { return _state; }
            set
            {
                if (value == TileState.Void) _entities.Clear();

                _state = value;
            }
        }

        public IEnumerable<Entity> Entities { get { return _entities; } }

        public int EntityCount { get { return _entities.Count; } }
        
        public Tile(Room room, Position relPos)
        {
            Room = room;
            RelativePosition = relPos;

            State = TileState.Void;

            _entities = new List<Entity>();
        }

        public Tile(Room room, int relX, int relY)
            : this(room, new Position(relX, relY)) { }

        internal void AddEntity(Entity ent)
        {
            if (State == TileState.Void) return;

            if (ent.Tile != this) return;
            if (_entities.Contains(ent)) return;

            _entities.Add(ent);
        }

        internal void RemoveEntity(Entity ent)
        {
            if (ent.Tile == this) return;
            if (!_entities.Contains(ent)) return;

            _entities.Remove(ent);
        }

        public Tile GetNeighbour(Position offset)
        {
            return Room[RelativePosition + offset];
        }

        public Tile GetNeighbour(Direction dir)
        {
            return GetNeighbour(new Position(((int) dir) % 3 - 1, ((int) dir) / 3 - 1));
        }

        public void Think()
        {
            for (int i = _entities.Count - 1; i >= 0; --i) {
                _entities[i].Think();
            }
        }

        public bool IsVisibleFrom(Position pos, int maxRadius)
        {
            var diff = Position - pos;

            if (diff.LengthSquared > maxRadius * maxRadius) return false;

            return pos.BresenhamLine(Position)
                .All(x => x == Position || Room[x].State == TileState.Floor);
        }

        public IEnumerator<Entity> GetEnumerator()
        {
            return _entities.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _entities.GetEnumerator();
        }
    }
}
