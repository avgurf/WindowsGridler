using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace windowsGriddler
{
    public partial class Form1 : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        readonly double GoldenRatio = (1 + Math.Sqrt(5)) / 2;

        public static bool FormRunning = true;

        public enum GridMode { None, Half, Thirds, GoldenThirds, Quads, MultiQuads, GoldenRectangle, FibonachiSpiral};

        private static GridMode Mode = GridMode.None;
 
        enum KeyModifier
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
        }

        private int Yfactor = 0;
        private int Yoffset = 0;

        private int Xfactor = 0;
        private int Xoffset = 0;

        enum HotKeyCMD { ModeChange, ColorChange, Terminate, ScaleX, ScaleY, ResetX, ResetY, ResetScale, MoveXUp, MoveXDown, MoveYUp, MoveYDown}

        private Color CurrentColor = Color.Black;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
 
            if (m.Msg == 0x0312)
            {
                /* Note that the three lines below are not needed if you only want to register one hotkey.
                 * The below lines are useful in case you want to register multiple keys, which you can use a switch with the id as argument,
                 * or if you want to know which key/modifier was pressed for some particular reason. */

                // The key of the hotkey that was pressed.
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                // The modifier of the hotkey that was pressed.  
                KeyModifier modifier = (KeyModifier)((int)m.LParam & 0xFFFF);
                // The id of the hotkey that was pressed.
                var id = (HotKeyCMD)m.WParam.ToInt32();

                switch(id)
                {
                    case HotKeyCMD.Terminate:
                        this.Close();
                        Application.Exit();
                        break;

                    case HotKeyCMD.ColorChange:
                        CurrentColor = (CurrentColor == Color.Black) ? Color.White : Color.Black;
                        break;

                    case HotKeyCMD.ModeChange:
                        Mode = Mode.Next();
                        break;

                    case HotKeyCMD.ScaleX:
                        this.Xfactor = (this.Xfactor < this.Width)? ++this.Xfactor : this.Xfactor;
                        break;

                    case HotKeyCMD.ResetX:
                        this.Xfactor = 0;
                        this.Xoffset = 0;
                        break;

                    case HotKeyCMD.ScaleY:
                        this.Yfactor = (this.Yfactor < this.Height)? ++this.Yfactor : this.Yfactor;
                        break;

                    case HotKeyCMD.ResetY:
                        this.Yfactor= 0;
                        this.Yoffset = 0;
                        break;

                    case HotKeyCMD.MoveXUp:
                        this.Xoffset = (this.Xoffset < this.Width/2)?  ++this.Xoffset: this.Xoffset;
                        break;

                    case HotKeyCMD.MoveXDown:
                        this.Xoffset = (this.Xoffset > 0)?  --this.Xoffset : 0;
                        break;

                    case HotKeyCMD.MoveYDown:
                        this.Yoffset = (this.Yoffset < this.Height/2)?  ++this.Yoffset: this.Yoffset;
                        break;

                    case HotKeyCMD.MoveYUp:
                        this.Yoffset = (this.Yoffset > 0) ? --this.Yoffset : 0;
                        break;

                    default:
                        break;

                }
                this.Update();
                panel1.Invalidate();
                // do something
            }
        }

        public Form1()
        {
            InitializeComponent();
    
            // Register Ctrl + G as global hotkey. 
            RegisterHotKey(this.Handle, (int)HotKeyCMD.ModeChange,
                (int)KeyModifier.Control, Keys.G.GetHashCode());
            // Ctrl + Shift + I
            RegisterHotKey(this.Handle, (int)HotKeyCMD.ColorChange,
                (int)KeyModifier.Control | (int)KeyModifier.Shift, Keys.I.GetHashCode());
            // Ctr + Win + Alt + Esc - > terminate
            RegisterHotKey(this.Handle, (int)HotKeyCMD.Terminate,
                (int)KeyModifier.Control | (int)KeyModifier.WinKey | (int)KeyModifier.Alt, Keys.Escape.GetHashCode());
            
            // Ctrl + Shift + <-  to shrink X
            RegisterHotKey(this.Handle, (int)HotKeyCMD.ScaleX,
                 (int)KeyModifier.Control | (int)KeyModifier.Shift, Keys.Left.GetHashCode());

            RegisterHotKey(this.Handle, (int)HotKeyCMD.MoveXDown,
                (int)KeyModifier.Control , Keys.Left.GetHashCode());

            RegisterHotKey(this.Handle, (int)HotKeyCMD.MoveXUp,
                    (int)KeyModifier.Control, Keys.Right.GetHashCode());

            // Ctrl + Shift + ^  to shrink y
            RegisterHotKey(this.Handle, (int)HotKeyCMD.ScaleY,
                 (int)KeyModifier.Control | (int)KeyModifier.Shift, Keys.Up.GetHashCode());

            RegisterHotKey(this.Handle, (int)HotKeyCMD.MoveYDown,
                (int)KeyModifier.Control, Keys.Down.GetHashCode());

            RegisterHotKey(this.Handle, (int)HotKeyCMD.MoveYUp,
                    (int)KeyModifier.Control, Keys.Up.GetHashCode());
  
            // Reset X
            RegisterHotKey(this.Handle, (int)HotKeyCMD.ResetX,
                (int)KeyModifier.Control | (int)KeyModifier.WinKey | (int)KeyModifier.Alt, Keys.Left.GetHashCode());
            // Reset Y
            RegisterHotKey(this.Handle, (int)HotKeyCMD.ResetY,
                (int)KeyModifier.Control | (int)KeyModifier.WinKey | (int)KeyModifier.Alt, Keys.Up.GetHashCode());
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            new Thread(() =>
            {
                try
                {
                    while (Form1.FormRunning)
                    {
                        this.TopMost = true;
                        Thread.Sleep(1);
                    }
                }
                catch (Exception ex) { }
            }).Start();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var numOfCells = 2;
            var cellSize = this.Width / numOfCells;
            var height = this.Height - this.Yfactor-this.Yoffset;
            var width = this.Width - this.Xoffset - this.Xfactor;
            var x_init = this.Xoffset;
            var y_init = this.Yoffset;

            var p = new Pen(CurrentColor, 10f);
            switch (Mode)
            {
                case GridMode.Half:
                    // Horizontal
                    g.DrawLine(p, x_init, y_init + (height / 2),
                                    x_init + width, y_init + (height / 2));
                    // Vertical
                    g.DrawLine(p, x_init + (width / 2), y_init,
                        x_init + (width / 2), y_init + height);

                    break;
                case GridMode.GoldenThirds:
                    // Horizontal
                    g.DrawLine(p,
                        x_init              , y_init + (int)((height / GoldenRatio) / GoldenRatio),
                        x_init +  width     , y_init + (int)((height / GoldenRatio) / GoldenRatio));

                    g.DrawLine(p, x_init        , y_init + (int)((height / GoldenRatio)),
                                  x_init + width, y_init +  (int)((height / GoldenRatio)));
                    // Vertical
                    g.DrawLine(p,
                        x_init + (int)((width / GoldenRatio) / GoldenRatio), y_init,
                        x_init + (int)((width / GoldenRatio) / GoldenRatio), y_init + height);

                    g.DrawLine(p,   x_init + (int)((width / GoldenRatio)), y_init,
                                    x_init + (int)((width / GoldenRatio)), y_init + height);

                    break;
                case GridMode.Thirds:
                    for (int i = 1; i < 3; i++)
                    {
                        // Horizontal
                        g.DrawLine(p, 
                            x_init          , y_init + ((height / 3) * i),
                            x_init + width  , y_init + ((height / 3)*i));
                        // Vertical
                        g.DrawLine(p,
                            x_init + ((width / 3) * i), y_init,
                            x_init + ((width / 3) * i), y_init + height);
                    }
                    break;
                case GridMode.MultiQuads:
                case GridMode.Quads:
                case GridMode.GoldenRectangle:
                    for (int i = 1; i < 4; i++)
                    {
                        // Horizontal
                        g.DrawLine(p,   x_init          ,y_init + ((height / 4) * i),
                                        x_init + width  ,y_init + ((height / 4) * i));
                        // Vertical
                        g.DrawLine(p, x_init + ((width / 4) * i), y_init,
                                      x_init + ((width / 4) * i), y_init + height);
                    }
                    if (Mode == GridMode.MultiQuads)
                    {
                        // Draw 2 diagonals
                        // top->bottom
                        g.DrawLine(p, 
                            x_init          , y_init,
                            x_init + width  , y_init + height);
                        // bottom->top
                        g.DrawLine(p,
                            x_init           , y_init + height,
                            x_init + width   , y_init);
                    }
                    if (Mode == GridMode.GoldenRectangle)
                    {
                        var p2 = new Pen(Color.Red, 10f);
                        // Horizontal
                        g.DrawLine(p2,
                            x_init, y_init + (int)((height / GoldenRatio) / GoldenRatio),
                            x_init + width, y_init + (int)((height / GoldenRatio) / GoldenRatio));

                        g.DrawLine(p2, x_init, y_init + (int)((height / GoldenRatio)),
                                      x_init + width, y_init + (int)((height / GoldenRatio)));
                        // Vertical
                        g.DrawLine(p2,
                            x_init + (int)((width / GoldenRatio) / GoldenRatio), y_init,
                            x_init + (int)((width / GoldenRatio) / GoldenRatio), y_init + height);

                        g.DrawLine(p2, x_init + (int)((width / GoldenRatio)), y_init,
                                        x_init + (int)((width / GoldenRatio)), y_init + height);
                    }
                    break;
                case GridMode.FibonachiSpiral:

                    break;
                default:
                    break;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Unregister hotkey with id 0 before closing the form. You might want to call this more than once with different id values if you are planning to register more than one hotkey.
            UnregisterHotKey(this.Handle, 0);
        }

    }

}
