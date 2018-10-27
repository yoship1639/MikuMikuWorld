using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Controls
{
    public class Control
    {
        public static Font DefaultFontS = new Font("Yu Gothic UI Light", 11.0f, FontStyle.Bold);
        public static Font DefaultFont = new Font("Yu Gothic UI", 14.0f);
        public static Font DefaultFontB = new Font("Yu Gothic UI", 14.0f, FontStyle.Bold);

        public Control Parent { get; set; }

        public string Name { get; set; }
        public Vector2 LocalLocation { get; set; }
        public Vector2 WorldLocation
        {
            get { if (Parent == null) return LocalLocation; return Parent.WorldLocation + LocalLocation; }
            set
            {
                if (Parent == null) LocalLocation = value;
                else LocalLocation = value - Parent.WorldLocation;
            }
        }
        public Vector2 Size { get; set; }
        public bool Focus { get; set; }
        public ContentAlignment Alignment { get; set; } = ContentAlignment.TopLeft;
        public bool IsMouseOn { get; private set; }

        public static Control FocusedControl { get; set; }

        private double clickTime = 0.0;
        public virtual void Update(Graphics g, double deltaTime)
        {
            IsMouseOn = IsIn(Input.MousePosition);

            if ((MMW.TotalElapsedTime - clickTime) < 0.4 && IsMouseOn && Input.IsButtonPressed(OpenTK.Input.MouseButton.Left))
            {
                clickTime = 0.0;
                DoubleClicked(this, EventArgs.Empty);
            }
            if (IsMouseOn && Input.IsButtonPressed(OpenTK.Input.MouseButton.Left))
            {
                clickTime = MMW.TotalElapsedTime;
                Clicked(this, EventArgs.Empty);
                MouseDown(this, EventArgs.Empty);
            }

            if (IsMouseOn && Input.IsButtonPressed(OpenTK.Input.MouseButton.Right))
            {
                RightClicked(this, EventArgs.Empty);
            }

            if (IsMouseOn && Input.IsButtonReleased(OpenTK.Input.MouseButton.Left))
            {
                MouseUp(this, EventArgs.Empty);
            }
        }
        public virtual void Draw(Graphics g, double deltaTime) { }

        public virtual void Load() { }
        public virtual void Unload() { }

        protected Vector2 GetLocation(float width, float height, ContentAlignment alignment)
        {
            var size = new Vector2(MMW.ClientSize.Width, MMW.ClientSize.Height);
            if (Parent != null) size = Parent.Size;
            var v = new Vector2();
            if (alignment == ContentAlignment.TopLeft || alignment == ContentAlignment.MiddleLeft || alignment == ContentAlignment.BottomLeft)
            {
                if (alignment == ContentAlignment.MiddleLeft) v.Y = (size.Y - height) / 2.0f;
                if (alignment == ContentAlignment.BottomLeft) v.Y = size.Y - height;
            }
            else if (alignment == ContentAlignment.TopCenter || alignment == ContentAlignment.MiddleCenter || alignment == ContentAlignment.BottomCenter)
            {
                v.X = (size.X - width) / 2.0f;
                if (alignment == ContentAlignment.MiddleCenter) v.Y = (size.Y - height) / 2.0f;
                if (alignment == ContentAlignment.BottomCenter) v.Y = size.Y - height;
            }
            else if (alignment == ContentAlignment.TopRight || alignment == ContentAlignment.MiddleRight || alignment == ContentAlignment.BottomRight)
            {
                v.X = size.X - width;
                if (alignment == ContentAlignment.MiddleRight) v.Y = (size.Y - height) / 2.0f;
                if (alignment == ContentAlignment.BottomRight) v.Y = size.Y - height;
            }
            return v;
        }

        public virtual bool IsIn(Vector2 pos)
        {
            var x = WorldLocation.X;
            var y = WorldLocation.Y;
            var w = Size.X;
            var h = Size.Y;

            return pos.X >= x && pos.X < x + w && pos.Y >= y && pos.Y < y + h;
        }

        public event EventHandler MouseDown = delegate { };
        public event EventHandler MouseUp = delegate { };
        public event EventHandler Clicked = delegate { };
        public event EventHandler RightClicked = delegate { };
        public event EventHandler DoubleClicked = delegate { };
    }
}
