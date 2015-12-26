using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Leap;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace JARBAS
{

    public partial class Form1 : Form
    {

        public static string i;
        public static int value=0;
        SampleListener listener = new SampleListener();
        Controller controller = new Controller();
        
        public Form1()
        {
            controller.AddListener(listener);
            controller.SetPolicy(Controller.PolicyFlag.POLICY_BACKGROUND_FRAMES);
            InitializeComponent();
            
        }

        public void button1_Click(object sender, EventArgs e)
        {
            button1.Text = i;            
        }
    }

    class SampleListener : Listener
    {
        //click constants
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);


        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        //end of click constants

        private const int X_LIMIT = 130;
        private const int Y_LOW_LIMIT = 100;
        private const int Y_TOP_LIMIT = 300;
        private const int Y_LIMIT = Y_TOP_LIMIT-Y_LOW_LIMIT;
        private const float ACC_X = 0.6f;
        private const float ACC_Y = 0.7f;
        private float lastX = 0;
        private float lastY = 0;
        private bool click;
        private Object thisLock = new Object();

        private void SafeWriteLine(String line)
        {
            lock (thisLock)
            {
                Console.WriteLine(line);
            }
        }

        public override void OnConnect(Controller controller)
        {
            SafeWriteLine("Connected");
        }


        public override void OnFrame(Controller controller)
        {
            Frame actualFrame = controller.Frame();
            getCoordenates(actualFrame);
            
        }

        private void getCoordenates(Frame f) {
            HandList allH = f.Hands;
            Hand left=null, right=null;
            Point screenPointer = new Point();
            //get right hand
            if (allH.Rightmost.IsValid)
            { 
                if (allH.Rightmost.IsRight)
                    right = allH.Rightmost;
                else
                    left = allH.Rightmost;
            }
            //get left hand
            if (allH.Leftmost.IsValid)
            {
                if (allH.Leftmost.IsLeft)
                    left = allH.Leftmost;
                else
                    right = allH.Leftmost;
            }

            if (right != null)
            {
                FingerList fin = right.Fingers.FingerType(Finger.FingerType.TYPE_INDEX);
                Finger fing = fin[0];

                float posX = fing.TipPosition.x;
                //accuracy test
                if (Math.Abs(posX - lastX) <= ACC_X)
                    posX = lastX;
                lastX = posX;
                if (posX >= -X_LIMIT && posX <= X_LIMIT)
                    screenPointer.X = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width * posX / (2 * X_LIMIT) + System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 2);
                else {
                    if (posX <= -X_LIMIT)
                        screenPointer.X = 0;
                    if (posX >= X_LIMIT)
                        screenPointer.X = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
                }

                float posY = fing.TipPosition.y;
                //accuracy test
                if (Math.Abs(posY - lastY) <= ACC_Y)
                    posY = lastY;
                lastY = posY;
                if (posY >= Y_LOW_LIMIT && posY <= Y_TOP_LIMIT)
                    screenPointer.Y = (int)(-System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height * posY / (Y_LIMIT) + System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height * Y_TOP_LIMIT / Y_LIMIT);
                else
                {
                    if (posY <= Y_LOW_LIMIT)
                        screenPointer.Y = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
                    if (posY >= Y_TOP_LIMIT)
                        screenPointer.Y = 0;

                }

                if (!right.Fingers.FingerType(Finger.FingerType.TYPE_THUMB)[0].IsExtended)
                {
                    if (!click)
                    { 
                        int X = Cursor.Position.X;
                        int Y = Cursor.Position.Y;

                        if (right.PalmNormal.y > 0f)
                          mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, (uint)X, (uint)Y, 0, 0);
                        else
                          mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)X, (uint)Y, 0, 0);

                        click = true;
                    }
                }
                else
                {
                    
                    /*int X = Cursor.Position.X;
                    int Y = Cursor.Position.Y;
                    mouse_event(MOUSEEVENTF_LEFTUP, (uint)X, (uint)Y, 0, 0);
                    */click = false;
                }
                Form1.i = getDistance(right.Fingers.FingerType(Finger.FingerType.TYPE_THUMB)[0].TipPosition, fing.TipPosition).ToString();
                Cursor.Position = screenPointer;
            }
            if (left != null)
            {
                int v = 0;
                foreach (Process proc in Process.GetProcessesByName("osk"))
                {
                    v++;
                }
                if (v == 0)
                    Process.Start("osk.exe");
            }
            else
            {
                int v = 0;
                foreach (Process proc in Process.GetProcessesByName("osk"))
                {
                    v++;
                }
                if (v != 0)
                {
                    try
                    {
                        foreach (Process proc in Process.GetProcessesByName("osk"))
                            proc.Kill();
                    }
                    catch { }
                }
            }
        }

        private double getDistance(Vector a, Vector b)
        {
            double distance;

            distance = Math.Sqrt((a.x - b.x)* (a.x - b.x) + (a.y - b.y)*(a.y - b.y) + (a.z - b.z)* (a.z - b.z));

            return distance;
        }
    }

}

