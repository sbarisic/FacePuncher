﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using FacePuncher.Geometry;
using Microsoft.Win32.SafeHandles;

namespace FacePuncher
{
    static class Display
    {
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
            IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteConsoleOutput(
          SafeFileHandle hConsoleOutput,
          CharInfo[] lpBuffer,
          Coord dwBufferSize,
          Coord dwBufferCoord,
          ref SmallRect lpWriteRegion);

        [StructLayout(LayoutKind.Sequential)]
        struct Coord
        {
            public short X;
            public short Y;

            public Coord(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }
        };

        [StructLayout(LayoutKind.Explicit)]
        struct CharInfo
        {
            [FieldOffset(0)]
            public char Char;
            [FieldOffset(2)]
            public short Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SmallRect
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        static SafeFileHandle _sHandle;
        static CharInfo[] _sBuffer;
        static SmallRect _sRect;

        static short GetAttributes(ConsoleColor fore, ConsoleColor back)
        {
            return (short) ((int) fore | ((int) back << 4));
        }

        public static Rectangle Rect { get; private set; }

        /// <summary>
        /// Horizontal size of the display in characters.
        /// </summary>
        public static int Width { get { return Rect.Width; } }

        /// <summary>
        /// Vertical size of the display in characters.
        /// </summary>
        public static int Height { get { return Rect.Height; } }

        /// <summary>
        /// Prepare the display for rendering.
        /// </summary>
        /// <param name="width">Desired horizontal size of the display in characters.</param>
        /// <param name="height">Desired vertical size of the display in characters.</param>
        public static void Initialize(int width, int height)
        {
            Rect = new Rectangle(0, 0, width, height);

            _sHandle = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

            Console.SetWindowSize(Math.Min(width, Console.WindowWidth), Math.Min(height, Console.WindowHeight));

            Console.SetBufferSize(width, height);
            Console.SetWindowSize(width, height);

            Console.CursorVisible = false;

            _sBuffer = new CharInfo[width * height];
            _sRect = new SmallRect { Left = 0, Top = 0, Right = (short) width, Bottom = (short) height };

            Clear();
        }

        /// <summary>
        /// Wipe the display buffer.
        /// </summary>
        public static void Clear()
        {
            for (int i = 0; i < _sBuffer.Length; ++i) {
                _sBuffer[i].Char = ' ';
                _sBuffer[i].Attributes = GetAttributes(ConsoleColor.Black, ConsoleColor.Black);
            }
        }

        /// <summary>
        /// Set a specific character in the display buffer.
        /// </summary>
        /// <param name="x">Horizontal position of the character.</param>
        /// <param name="y">Vertical position of the character.</param>
        /// <param name="symbol">Character to display.</param>
        /// <param name="fore">Foreground color of the character.</param>
        /// <param name="back">Background color of the character.</param>
        public static void SetCell(int x, int y, char symbol, ConsoleColor fore = ConsoleColor.Gray, ConsoleColor back = ConsoleColor.Black)
        {
            if (x < 0 || y < 0 || x >= _sRect.Right || y >= _sRect.Bottom) return;

            int index = x + y * _sRect.Right;

            _sBuffer[index].Char = symbol;
            _sBuffer[index].Attributes = GetAttributes(fore, back);
        }

        /// <summary>
        /// Set a specific character in the display buffer.
        /// </summary>
        /// <param name="pos">Position of the character.</param>
        /// <param name="symbol">Character to display.</param>
        /// <param name="fore">Foreground color of the character.</param>
        /// <param name="back">Background color of the character.</param>
        public static void SetCell(Position pos, char symbol, ConsoleColor fore = ConsoleColor.Gray, ConsoleColor back = ConsoleColor.Black)
        {
            SetCell(pos.X, pos.Y, symbol, fore, back);
        }

        /// <summary>
        /// Send the display buffer to the console window.
        /// </summary>
        public static void Refresh()
        {
            var rect = _sRect;

            WriteConsoleOutput(_sHandle, _sBuffer,
                new Coord(_sRect.Right, _sRect.Bottom),
                new Coord(0, 0), ref rect);
        }
    }
}
