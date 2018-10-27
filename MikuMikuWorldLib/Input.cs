using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MikuMikuWorld
{
    public static class Input
    {
        static KeyboardState keyboard;
        static KeyboardState keyboardPrev;
        static MouseState mouse;
        static MouseState mousePrev;
        static Vector2 mousePos;
        static Vector2 mousePosPrev;

        private static bool acceptKeyboard = true;
        public static bool AcceptKeyboard
        {
            get
            {
                return acceptKeyboard && MMW.Window.Focused;
            }
            set
            {
                acceptKeyboard = value;
            }
        }
        private static bool acceptMouse = true;
        public static bool AcceptMouse
        {
            get
            {
                return acceptMouse && MMW.Window.Focused;
            }
            set
            {
                acceptMouse = value;
            }
        }

        static internal void Update()
        {
            keyboardPrev = keyboard;
            mousePrev = mouse;
            mousePosPrev = mousePos;
            prevDownKeys = DownKeys.ToArray();
            prevDownButtons = DownButtons.ToArray();

            keyboard = Keyboard.GetState();
            mouse = Mouse.GetState();
            
            if (AcceptMouse) mousePos = new Vector2(Cursor.Position.X - MMW.X, Cursor.Position.Y - MMW.Y);

            DownKeys = GetDownKeys();
            {
                var list = new List<Key>();
                foreach (var k in DownKeys)
                {
                    if (!prevDownKeys.Contains(k)) list.Add(k);
                }
                PressedKeys = list.ToArray();
            }
            {
                var list = new List<Key>();
                foreach (var k in prevDownKeys)
                {
                    if (!DownKeys.Contains(k)) list.Add(k);
                }
                ReleasedKeys = list.ToArray();
            }

            DownButtons = GetDownButtons();
            {
                var list = new List<MouseButton>();
                foreach (var b in DownButtons)
                {
                    if (!prevDownButtons.Contains(b)) list.Add(b);
                }
                PressedButtons = list.ToArray();
            }
            {
                var list = new List<MouseButton>();
                foreach (var b in prevDownButtons)
                {
                    if (!DownButtons.Contains(b)) list.Add(b);
                }
                ReleasedButtons = list.ToArray();
            }
        }

        #region Keyboard
        public static bool Shift => (keyboard.IsKeyDown(Key.LShift) || keyboard.IsKeyDown(Key.RShift));
        public static bool Ctrl => (keyboard.IsKeyDown(Key.LControl) || keyboard.IsKeyDown(Key.RControl));
        public static bool Alt => (keyboard.IsKeyDown(Key.LAlt) || keyboard.IsKeyDown(Key.RAlt));
        public static Key[] prevDownKeys { get; private set; } = new Key[0];
        public static Key[] DownKeys { get; private set; } = new Key[0];
        public static Key[] PressedKeys { get; private set; } = new Key[0];
        public static Key[] ReleasedKeys { get; private set; } = new Key[0];

        public static bool IsAnyKeyDown()
        {
            return AcceptKeyboard && keyboard.IsAnyKeyDown;
        }
        public static bool IsAnyKeyPressed()
        {
            return AcceptKeyboard && keyboard.IsAnyKeyDown && !keyboardPrev.IsAnyKeyDown;
        }
        public static bool IsAnyKeyReleased()
        {
            return AcceptKeyboard && !keyboard.IsAnyKeyDown && keyboardPrev.IsAnyKeyDown;
        }
        
        public static bool IsKeyDown(Key key)
        {
            return AcceptKeyboard && keyboard.IsKeyDown(key);
        }
        public static bool IsKeyUp(Key key)
        {
            return AcceptKeyboard && keyboard.IsKeyUp(key);
        }
        public static bool IsKeyPressed(Key key)
        {
            return AcceptKeyboard && keyboard.IsKeyDown(key) && keyboardPrev.IsKeyUp(key);
        }
        public static bool IsKeyReleased(Key key)
        {
            return AcceptKeyboard && keyboard.IsKeyUp(key) && keyboardPrev.IsKeyDown(key);
        }

        public static bool IsAnyKeyDown(params Key[] keys)
        {
            if (!AcceptKeyboard) return false;
            foreach (var k in keys)
            {
                if (keyboard.IsKeyDown(k)) return true;
            }
            return false;
        }
        public static bool IsAnyKeyUp(params Key[] keys)
        {
            if (!AcceptKeyboard) return false;
            foreach (var k in keys)
            {
                if (keyboard.IsKeyUp(k)) return true;
            }
            return false;
        }
        public static bool IsAnyKeyPressed(params Key[] keys)
        {
            if (!AcceptKeyboard) return false;
            foreach (var k in keys)
            {
                if (keyboard.IsKeyDown(k) && keyboardPrev.IsKeyUp(k)) return true;
            }
            return false;
        }
        public static bool IsAnyKeyReleased(params Key[] keys)
        {
            if (!AcceptKeyboard) return false;
            foreach (var k in keys)
            {
                if (keyboard.IsKeyUp(k) && keyboardPrev.IsKeyDown(k)) return true;
            }
            return false;
        }

        public static Key[] GetDownKeys()
        {
            if (!AcceptKeyboard) return new Key[0];
            List<Key> list = new List<Key>();
            for (var i = 0; i < 128; i++)
            {
                if (keyboard.IsKeyDown((Key)i)) list.Add((Key)i);
            }
            return list.ToArray();
        }
        #endregion

        #region Mouse
        public static MouseButton[] prevDownButtons { get; private set; } = new MouseButton[0];
        public static MouseButton[] DownButtons { get; private set; } = new MouseButton[0];
        public static MouseButton[] PressedButtons { get; private set; } = new MouseButton[0];
        public static MouseButton[] ReleasedButtons { get; private set; } = new MouseButton[0];

        public static MouseButton[] GetDownButtons()
        {
            if (!AcceptKeyboard) return new MouseButton[0];
            List<MouseButton> list = new List<MouseButton>();
            for (var i = 0; i < (int)MouseButton.LastButton; i++)
            {
                if (mouse.IsButtonDown((MouseButton)i)) list.Add((MouseButton)i);
            }
            return list.ToArray();
        }

        public static bool IsButtonDown(MouseButton btn)
        {
            return AcceptMouse && mouse.IsButtonDown(btn);
        }
        public static bool IsButtonUp(MouseButton btn)
        {
            return AcceptMouse && mouse.IsButtonUp(btn);
        }
        public static bool IsButtonPressed(MouseButton btn)
        {
            return AcceptMouse && mouse.IsButtonDown(btn) && mousePrev.IsButtonUp(btn);
        }
        public static bool IsButtonReleased(MouseButton btn)
        {
            return AcceptMouse && mouse.IsButtonUp(btn) && mousePrev.IsButtonDown(btn);
        }
        public static Vector2 MousePosition
        {
            get { return new Vector2(mousePos.X, mousePos.Y); }
        }
        public static Vector2 MouseDelta
        {
            get
            {
                if (!AcceptMouse) return Vector2.Zero;
                return new Vector2(mouse.X - mousePrev.X, mouse.Y - mousePrev.Y);
            }
        }
        public static int MouseWheel
        {
            get
            {
                if (!AcceptMouse) return 0;
                return mousePrev.Wheel - mouse.Wheel;
            }
        }
        #endregion
    }
}
