using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dicom;
using Dicom.Generators;
using Dicom.Imaging;
using Dicom.IO;
using Dicom.Log;
using Dicom.Media;
using Dicom.Network;
using Dicom.StructuredReport;


namespace PictureViewer
{

    public partial class Form1 : Form
    {
        DCMset newSet,leftSet,rightSet;
        private int coordCount;
        private Point currentMousePos;
        private Point initialMousePos;
        private bool clicked;                   // Is the picture box currently clicked down on?
        private bool drawBtnDown;               // Is the draw button clicked? 
        private bool select1Down;
        private bool select2Down;
        private Button[] deleteButtons;
        private Point leftPoint;
        private List<Label> table_x;  // table4, table3, table5
        private List<Label> table_y;  // table4, table3, table5
        private List<Label> table_z;  // table4, table3, table5
        private List<Label> table_name;     // point names
        private Point rightPoint;
        private string cutPathDir;
        private string coordPathDir;
        private int orientation_left, orientation_right;
        
        private Tuple<int, int, int> selectPoint3d;         //  Selected Scaled Point for Localization
        private Tuple<int, int, int> unsc_selectPoint3d;    //  Unscaled Selected Point for Localization

        public Form1()
        {


            InitializeComponent();
            newSet = new DCMset();
            leftSet = new DCMset();
            rightSet = new DCMset();

            table_x = new List<Label>();
            table_y = new List<Label>();
            table_z = new List<Label>();
            table_name = new List<Label>();

            

            coordCount = 0;

            //  Each label array contains the all the associated co-ordinate's values in all the tables. On the second tab, there are 3 tables.
            //  Starting from the middle right panel -> bottom left panel -> bottom right panel.

            clicked = false;
            selectPoint3d = new Tuple<int,int,int>(0,0,0);
            unsc_selectPoint3d = new Tuple<int, int, int>(0, 0, 0);
            drawBtnDown = false;
            select1Down = false;
            select2Down = false;
            orientation_left = 1;
            orientation_right = 2;

            //  Trackbar1 Init
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            this.trackBar1.Enabled = false;

            //  Trackbar2 Init
            this.trackBar2.Scroll += new System.EventHandler(this.trackBar2_Scroll);
            this.trackBar2.Enabled = false;

            //  Trackbar3 Init
            //this.trackBar3.Scroll += new System.EventHandler(this.trackBar3_Scroll);
            //this.trackBar3.Enabled = false;

            //  Table Layout Panel 4

            tableLayoutPanel3.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
            tableLayoutPanel4.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
            tableLayoutPanel5.GrowStyle = TableLayoutPanelGrowStyle.AddRows;



            for (int i = 0; i < tableLayoutPanel4.RowCount; i++)
            {
                Label newLabel = new Label();
                newLabel.AutoSize = true;
                newLabel.BackColor = Color.Transparent;
                newLabel.Font = new Font(Font.FontFamily.Name, 12);
                switch (i)
                {
                    case 0:
                        newLabel.Text = "x";
                        break;
                    case 1:
                        newLabel.Text = "y";
                        break;
                    case 2:
                        newLabel.Text = "z";
                        break;
                    
                }
                
                
                tableLayoutPanel4.Controls.Add(newLabel, 0, i);
            }

            for (int i = 0; i < 9; i++)
            {
                Label x = new Label();
                Label y = new Label();
                Label z = new Label();
                Label name = new Label();

                table_x.Add(x);
                table_y.Add(y);
                table_z.Add(z);
                table_name.Add(name);
            }

            for (int i = 0; i < 9; i++)
            {
                labelPropSet(table_x[i], "0");
                labelPropSet(table_y[i], "0");
                labelPropSet(table_z[i], "0");
                labelPropSet(table_name[i], "");


                table_name[i].AutoSize = false;
                table_name[i].Dock = DockStyle.Fill;
                table_name[i].TextAlign = ContentAlignment.MiddleLeft;
                table_name[i].BackColor = Color.Transparent;
                table_name[i].Font = new Font(Font.FontFamily.Name, 10);
                table_name[i].Text = "";

                if (i < 1)
                {
                    tableLayoutPanel4.Controls.Add(table_x[i], 1, 0);
                    tableLayoutPanel4.Controls.Add(table_y[i], 1, 1);
                    tableLayoutPanel4.Controls.Add(table_z[i], 1, 2);
                    
                }
                else if (i < 5)
                {
                    tableLayoutPanel3.Controls.Add(table_x[i], 1, i);
                    tableLayoutPanel3.Controls.Add(table_y[i], 2, i);
                    tableLayoutPanel3.Controls.Add(table_z[i], 3, i);
                    tableLayoutPanel3.Controls.Add(table_name[i], 0, i);
                }
                else if (i < 9)
                {
                    tableLayoutPanel5.Controls.Add(table_x[i], 1, i - 4);
                    tableLayoutPanel5.Controls.Add(table_y[i], 2, i - 4);
                    tableLayoutPanel5.Controls.Add(table_z[i], 3, i - 4);
                    tableLayoutPanel5.Controls.Add(table_name[i], 0, i - 4);
                }
            }

            //  Table Layout Panel 3

            for (int i = 0; i < tableLayoutPanel3.ColumnCount; i++)
            {
                Label newLabel = new Label();
                newLabel.AutoSize = false;
                newLabel.TextAlign = ContentAlignment.MiddleCenter;
                newLabel.BackColor = Color.Transparent;
                newLabel.Font = new Font(Font.FontFamily.Name, 12);
                switch (i)
                {
                    case 0:
                        newLabel.Text = "Name";
                        break;
                    case 1:
                        newLabel.Text = "x";
                        break;
                    case 2:
                        newLabel.Text = "y";
                        break;
                    case 3:
                        newLabel.Text = "z";
                        break;
                    case 4:
                        newLabel.Text = "Delete";
                        break;
                }

                
                tableLayoutPanel3.Controls.Add(newLabel, i,0);
            }

            

            //  Table Layout Panel 5

            for (int i = 0; i < tableLayoutPanel5.ColumnCount; i++)
            {
                Label newLabel = new Label();
                newLabel.AutoSize = false;
                newLabel.TextAlign = ContentAlignment.MiddleCenter;
                newLabel.BackColor = Color.Transparent;
                newLabel.Font = new Font(Font.FontFamily.Name, 12);
                
                switch (i)
                {
                    case 0:
                        newLabel.Text = "Name";
                        break;
                    case 1:
                        newLabel.Text = "x";
                        break;
                    case 2:
                        newLabel.Text = "y";
                        break;
                    case 3:
                        newLabel.Text = "z";
                        break;
                    case 4:
                        newLabel.Text = "Delete";
                        break;
                }
                tableLayoutPanel5.Controls.Add(newLabel, i,0);
            }

            //  Delete Buttons

            deleteButtons = new Button[8];
            for (int i = 0; i < 8; i++)
            {
                deleteButtons[i] = new Button();
                deleteButtons[i].AutoSize = false;
                deleteButtons[i].Text = "X";
                deleteButtons[i].Dock = DockStyle.Fill;
                deleteButtons[i].Tag = i;
                deleteButtons[i].TextAlign = ContentAlignment.MiddleCenter;
                deleteButtons[i].Click += new System.EventHandler(this.deleteClick);

                if (i < 4)
                {
                    tableLayoutPanel3.Controls.Add(deleteButtons[i], 4, i + 1);
                }
                else
                {
                    tableLayoutPanel5.Controls.Add(deleteButtons[i], 4, i - 3);
                }
            }


            
            //  PictureBox Init
            this.pictureBox1.MouseUp += new MouseEventHandler(this.pictureBox1_MouseUp);
            this.pictureBox1.MouseDown += new MouseEventHandler(this.pictureBox1_MouseDown);
            this.pictureBox1.MouseMove += new MouseEventHandler(this.pictureBox1_MouseMove);
            this.pictureBox1.Paint += new PaintEventHandler(this.pictureBox1_Paint);


            this.pictureBox2.MouseUp += new MouseEventHandler(this.pictureBox2_MouseUp);
            this.pictureBox2.MouseDown += new MouseEventHandler(this.pictureBox2_MouseDown);
            this.pictureBox2.Paint += new PaintEventHandler(this.pictureBox2_Paint);
            this.pictureBox2.Tag = "2";

            /*
            this.pictureBox3.MouseUp += new MouseEventHandler(this.pictureBox3_MouseUp);
            this.pictureBox3.MouseDown += new MouseEventHandler(this.pictureBox3_MouseDown);
            this.pictureBox3.Paint += new PaintEventHandler(this.pictureBox3_Paint);
            this.pictureBox2.Tag = "3";
            */
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            //pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;


            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            this.clearScrBtn.Enabled = false;
            this.drawBtn.Enabled = false;
            this.prevBtn.Enabled = false;
            this.nextBtn.Enabled = false;


            List<string> lines = new List<string>();
            using (StreamReader sr = new StreamReader(@"outputpaths.txt"))
            {
                while (sr.Peek() >= 0)
                {
                    string line = sr.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        //do something with text line here...
                        lines.Add(line);
                                            }
                }
            }
            cutPathDir = lines[0];
            coordPathDir = lines[1];


            Debug.Print("The cut path is {0} and the coordpath is {1}", cutPathDir, coordPathDir);



        }

        private void replaceMin(string[] zVals)
        {
            float min = 150;
            for (int i = 0; i < zVals.Count(); i++)
            {
                float n = float.Parse(zVals[i]);
                if (n < min)
                    min = n;

            }
            for (int i = 0; i < zVals.Count(); i++)
            {
                float n = float.Parse(zVals[i]);
                n = n + (-1)*min;
                zVals[i] = n.ToString();
            }

        }

        private void labelPropSet(Label a, string b)
        {
            a.AutoSize = false;
            a.BackColor = Color.Transparent;
            a.Dock = DockStyle.Fill;
            a.TextAlign = ContentAlignment.MiddleLeft;
            a.Font = new Font(Font.FontFamily.Name, 12);
            a.Text = b;
        }

        private Point scaledPoint(Point p1, PictureBox h)
        {
            //  Utilizes the rectangles drawn on top of the CT scan
            //  Outputs the scaled down version of the rectangle such that the pixels match the CT scan
            //  Pixel matching to the CT scan allows for accurate distance measurements of the cutting surfaces

            Point unscaled_p1 = new Point();
            // image and container dimensions
            int w_i = h.Image.Width;
            int h_i = h.Image.Height;
            int w_c = h.Width;
            int h_c = h.Height;

            float imageRatio = w_i / (float)h_i; // image W:H ratio
            float containerRatio = w_c / (float)h_c; // container W:H ratio

            if (imageRatio >= containerRatio)
            {
                // horizontal image
                float scaleFactor = w_c / (float)w_i;
                float scaledHeight = h_i * scaleFactor;
                // calculate gap between top of container and top of image
                float filler = Math.Abs(h_c - scaledHeight) / 2;
                unscaled_p1.X = (int)(p1.X / scaleFactor);
                unscaled_p1.Y = (int)((p1.Y - filler) / scaleFactor);
            }
            else
            {
                // vertical image
                float scaleFactor = h_c / (float)h_i;
                float scaledWidth = w_i * scaleFactor;
                float filler = Math.Abs(w_c - scaledWidth) / 2;
                unscaled_p1.X = (int)((p1.X - filler) / scaleFactor);
                unscaled_p1.Y = (int)(p1.Y / scaleFactor);

            }
            return unscaled_p1;
        }

        private int[] fillToEdge(PictureBox h)
        {
            //  Produces the left, top, right and bottom pixel values to draw lines.
            int[] corners = new int[4]; // Left, Top, Right, Bottom
            for (int i = 0; i < 4; i++)
            {
                corners[i] = new int();
            }

            // image and container dimensions
            int w_i = h.Image.Width;
            int h_i = h.Image.Height;
            int w_c = h.Width;
            int h_c = h.Height;

            float imageRatio = w_i / (float)h_i; // image W:H ratio
            float containerRatio = w_c / (float)h_c; // container W:H ratio
            
            if (imageRatio >= containerRatio)
            {
                // horizontal image
                //Debug.Print("hori");
                float scaleFactor = w_c / (float)w_i;
                float scaledHeight = h_i * scaleFactor;
                float filler = Math.Abs(h_c - scaledHeight) / 2;
                corners[0] = 0;
                corners[1] = (int)Math.Round(filler);
                corners[2] = w_c;
                corners[3] = (int)Math.Round(h_c - filler);
                //Debug.Print ("Left: {0} Top: {1} Right: {2} Down: {3} filler {4}", corners[0],corners[1],corners[2],corners[3], filler);
            }
            else
            {
                //Debug.Print("vert");
                // vertical image
                float scaleFactor = h_c / (float)h_i;
                float scaledWidth = w_i * scaleFactor;
                float filler = Math.Abs(w_c - scaledWidth) / (float)2;
                corners[0] = (int)Math.Round(filler);
                corners[1] = 0;
                corners[2] = (int)Math.Round(w_c - filler);
                corners[3] = h_c;
                filler = 5.5f;
                //Debug.Print ("Left: {0} Top: {1} Right: {2} Down: {3} fillder {4}", corners[0],corners[1],corners[2],corners[3], filler);

            }
            return corners;
        }

        private void makeShape() {
            Rectangle lastRect = new Rectangle();
            Rectangle nextRect = new Rectangle();
            Rectangle a_lastRect = new Rectangle();
            Rectangle a_nextRect = new Rectangle();
            
            for (int i = 0; i < newSet.scaledRectSet.Count(); i++)
            {
                if (newSet.scaledRectSet[i].Item1 == true)
                {
                    //  If a rectangle exists on this slide
                    lastRect = newSet.scaledRectSet[i].Item2;
                    a_lastRect = newSet.rectSet[i].Item2;
                    int ii = i + 1;
                    int interval = 0;
                    while (ii < newSet.scaledRectSet.Count())
                    {
                        if (newSet.scaledRectSet[ii].Item1 == true)
                        {
                            nextRect = newSet.scaledRectSet[ii].Item2;
                            a_nextRect = newSet.rectSet[ii].Item2;
                            interval = ii - i;
                            //Debug.Print("Interval found: " + interval + " from " + i + " to " + ii);
                            break;  // Next Rectangle found
                        }
                        else
                        {
                            ii++;
                        }
                    }

                    if (ii >= newSet.scaledRectSet.Count())
                    {
                        break;
                    }
                    else if (interval > 1)
                    {
                        //  If there are numerous empty undrawn slots between drawn shapes, we must take the average of the slots
                        int filli;
                        float left_diff = ((float)nextRect.Left - (float)lastRect.Left) / (float)interval;
                        float right_diff = ((float)nextRect.Right - (float)lastRect.Right) / (float)interval;
                        float top_diff = ((float)nextRect.Top - (float)lastRect.Top) / (float)interval;
                        float bottom_diff = ((float)nextRect.Bottom - (float)lastRect.Bottom) / (float)interval;
                        float a_left_diff = ((float)a_nextRect.Left - (float)a_lastRect.Left) / (float)interval;
                        float a_right_diff = ((float)a_nextRect.Right - (float)a_lastRect.Right) / (float)interval;
                        float a_top_diff = ((float)a_nextRect.Top - (float)a_lastRect.Top) / (float)interval;
                        float a_bottom_diff = ((float)a_nextRect.Bottom - (float)a_lastRect.Bottom) / (float)interval;
                        //Debug.Print("Diffs found: " + left_diff + " " + right_diff + " " + bottom_diff + " " + top_diff + " from " + nextRect.Left + " to " + lastRect.Left + " with interval = " + interval );

                        int multInc = 1;
                        float ltx, lty, rbx, rby = 0;
                        for (filli = i + 1; filli < ii; filli++)
                        {
                            Point LT = new Point();
                            Point RB = new Point();
                            Point a_LT = new Point();
                            Point a_RB = new Point();

                            ltx = left_diff * (float)multInc + lastRect.Left;
                            lty = top_diff * (float)multInc + lastRect.Top;
                            rbx = right_diff * (float)multInc + lastRect.Right;
                            rby = bottom_diff * (float)multInc + lastRect.Bottom;

                            LT.X = (int)(ltx);
                            LT.Y = (int)(lty);
                            RB.X = (int)(rbx);
                            RB.Y = (int)(rby);

                            ltx = a_left_diff * (float)multInc + a_lastRect.Left;
                            lty = a_top_diff * (float)multInc + a_lastRect.Top;
                            rbx = a_right_diff * (float)multInc + a_lastRect.Right;
                            rby = a_bottom_diff * (float)multInc + a_lastRect.Bottom;

                            a_LT.X = (int)ltx;
                            a_LT.Y = (int)lty;
                            a_RB.X = (int)rbx;
                            a_RB.Y = (int)rby;
                            
                            Rectangle intervalRect = Rectangle.FromLTRB(
                                        LT.X,
                                        LT.Y,
                                        RB.X,
                                        RB.Y);

                            Rectangle a_intervalRect = Rectangle.FromLTRB(
                                        a_LT.X,
                                        a_LT.Y,
                                        a_RB.X,
                                        a_RB.Y);

                            this.newSet.scaledRectSet[filli] = new Tuple<bool, Rectangle>(true, intervalRect);
                            this.newSet.rectSet[filli] = new Tuple<bool, Rectangle>(true, a_intervalRect);
                            multInc++;
                        }
                        i = i + interval-1; // Skip the empty frames that we just filled in. For loop will increment once at end of loop to the nextRect filled in.
                    }

                }
                else
                {
                }
            }
            Debug.Print("The cut path is {0} and the coordpath is {1}", cutPathDir, coordPathDir);
        }

        private void pathOutput () {
            //  Utilizes the object newSet
            //  Outputs a left to right, right to left cutting path from the rectangles drawn on the CT scan
            //bool started = false;   // First rectangle found yet? If not, do not try to produce rectangles.
            Rectangle lastRect;
            Rectangle nextRect;

            string path = cutPathDir;
            bool inUse = true;
            FileStream file;
            //int iter = 0;
            file = File.Open(path, FileMode.Create);
            file.Close();
            for ( int i = 0; i < newSet.scaledRectSet.Count(); i++ )
            {
                for (float iter = 0; iter < 4; iter++)
                {

                    if (newSet.scaledRectSet[i].Item1 == true)
                    {
                        //  If a rectangle exists on this slide
                        lastRect = newSet.scaledRectSet[i].Item2;
                        int ii = i + 1;

                        while (ii < newSet.scaledRectSet.Count())
                        {
                            if (newSet.scaledRectSet[ii].Item1 == true)
                            {
                                nextRect = newSet.scaledRectSet[ii].Item2;
                                break;
                            }
                            else
                            {
                                ii++;
                            }
                        }

                        /*
                        while (inUse == true)
                        {
                            try {
                                
                                inUse = false;
                            }
                            catch (IOException)
                            {

                                Debug.Print("crap...");
                            }
                        }*/

                        float y = newSet.scaledRectSet[i].Item2.Top;
                        //started = true;
                        float increment = iter*0.25f;
                        //Debug.Print("inc is {0}", increment);
                        while (y <= newSet.scaledRectSet[i].Item2.Bottom)
                        {

                            string output;
                            if (y % 2 == 0)
                            {
                                output = String.Format("{0:0.00} {1:0.00} {2} to {3:0.00} {4:0.00} {5}", (newSet.scaledRectSet[i].Item2.Left * newSet.pixelSpacing), (y * newSet.pixelSpacing), (float.Parse(newSet.zVals[i]) + increment), (newSet.scaledRectSet[i].Item2.Right * newSet.pixelSpacing), (y * newSet.pixelSpacing), (float.Parse(newSet.zVals[i])+ increment));
                            }
                            else
                            {
                                output = String.Format("{0:0.00} {1:0.00} {2} to {3:0.00} {4:0.00} {5}", (newSet.scaledRectSet[i].Item2.Right * newSet.pixelSpacing), (y * newSet.pixelSpacing), (float.Parse(newSet.zVals[i])+ increment), (newSet.scaledRectSet[i].Item2.Left * newSet.pixelSpacing), (y * newSet.pixelSpacing), (float.Parse(newSet.zVals[i])+ increment));

                            }
                            //Debug.Print(newSet.pixelSpacing.ToString());
                            //Debug.Print("{0:0.00} {1:0.00} {2:0.00} to {3:0.00} {4:0.00} {5:0.00}", (newSet.scaledRectSet[i].Item2.Left * newSet.pixelSpacing), (y * newSet.pixelSpacing), (float.Parse(newSet.zVals[i]) + increment ), (newSet.scaledRectSet[i].Item2.Right * newSet.pixelSpacing), (y * newSet.pixelSpacing), (float.Parse(newSet.zVals[i]) + increment));
                            
                            if (!File.Exists(path))
                            {
                                File.WriteAllText(path, output);
                                File.AppendAllText(path, Environment.NewLine);
                            }
                            else
                            {
                                File.AppendAllText(path, output);
                                File.AppendAllText(path, Environment.NewLine);
                            }
                            y++;
                        }
                    }
                }
            }
        }

        private void tableReset()
        {
            
            for (int i = 1; i < 9; i++)
            {
                if (i < 5)
                {
                    tableLayoutPanel3.Controls.Remove(tableLayoutPanel3.GetControlFromPosition(0, i));
                    tableLayoutPanel3.Controls.Remove(tableLayoutPanel3.GetControlFromPosition(1, i));
                    tableLayoutPanel3.Controls.Remove(tableLayoutPanel3.GetControlFromPosition(2, i));
                    tableLayoutPanel3.Controls.Remove(tableLayoutPanel3.GetControlFromPosition(3, i));
                    tableLayoutPanel3.Controls.Add(table_name[i], 0, i);
                    tableLayoutPanel3.Controls.Add(table_x[i], 1, i);
                    tableLayoutPanel3.Controls.Add(table_y[i], 2, i);
                    tableLayoutPanel3.Controls.Add(table_z[i], 3, i);
                }
                else if (i < 9)
                {
                    tableLayoutPanel5.Controls.Remove(tableLayoutPanel5.GetControlFromPosition(0, i - 4));
                    tableLayoutPanel5.Controls.Remove(tableLayoutPanel5.GetControlFromPosition(1, i - 4));
                    tableLayoutPanel5.Controls.Remove(tableLayoutPanel5.GetControlFromPosition(2, i - 4));
                    tableLayoutPanel5.Controls.Remove(tableLayoutPanel5.GetControlFromPosition(3, i - 4));
                    tableLayoutPanel5.Controls.Add(table_name[i], 0, i - 4);
                    tableLayoutPanel5.Controls.Add(table_x[i], 1, i - 4);
                    tableLayoutPanel5.Controls.Add(table_y[i], 2, i - 4);
                    tableLayoutPanel5.Controls.Add(table_z[i], 3, i - 4);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void deleteClick(object sender, EventArgs e)
        {
            
            Button theButton = (Button)sender;
            int i = (int)theButton.Tag;
            
            if ((table_x[i].Text != "0" || table_y[i].Text != "0" || table_z[i].Text != "0") && coordCount > 0) {
                table_name.RemoveAt(i);
                table_x.RemoveAt(i);
                table_y.RemoveAt(i);
                table_z.RemoveAt(i);

                Label name = new Label();
                Label x = new Label();
                Label y = new Label();
                Label z = new Label();

                table_name.Add(name);
                table_x.Add(x);
                table_y.Add(y);
                table_z.Add(z);

                
                table_name[8].AutoSize = false;
                table_name[8].Dock = DockStyle.Fill;
                table_name[8].TextAlign = ContentAlignment.MiddleLeft;
                table_name[8].BackColor = Color.Transparent;
                table_name[8].Font = new Font(Font.FontFamily.Name, 10);
                table_name[8].Text = "";
                labelPropSet(table_x[8], "0");
                labelPropSet(table_y[8], "0");
                labelPropSet(table_z[8], "0");

                coordCount--;

                this.tableReset();
            }

        }

        private void deleteRect_click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null && newSet.rectSet[this.trackBar1.Value].Item1 == true)
            {
                newSet.rectSet[this.trackBar1.Value] = new Tuple<bool, Rectangle>(false, Rectangle.FromLTRB(0, 0, 0, 0));
                this.pictureBox1.Image = pictureBox1.Image;
            }
        }
       
        private void showButton_Click(object sender, EventArgs e)
        {
            //  When the import DICOM button is clicked, the user is presented with a file selection dialogue

            if ( openFileDialog1.ShowDialog() == DialogResult.OK )
            {
                // The Maximum property sets the value of the track bar when 
                // the slider is all the way to the right.
                trackBar1.Maximum = openFileDialog1.FileNames.Count()-1;

                // The TickFrequency property establishes how many positions 
                // are between each tick-mark.
                trackBar1.TickFrequency = 1;

                // The LargeChange property sets how many positions to move 
                // if the bar is clicked on either side of the slider.
                trackBar1.LargeChange = trackBar1.Maximum/8;

                // The SmallChange property sets how many positions to move 
                // if the keyboard arrows are used to move the slider.
                trackBar1.SmallChange = 1;
                
                newSet.add(openFileDialog1.FileNames);
                DicomImage image = new DicomImage(newSet.names[0]);
                Image result = image.RenderImage(0);

                //  Find the changing z value
                bool found = false;
                int column = -1;
                while (found == false)
                {
                    column++;
                    float lastValue = -13.3f;       // arbitrary value
                    for (int i = 0; i < openFileDialog1.FileNames.Count(); i++)                    
                    {
                        DicomFile df = DicomFile.Open(newSet.names[i]);
                        if (lastValue != float.Parse(df.Dataset.Get<string>(Dicom.DicomTag.ImagePositionPatient, column)))
                        {
                            lastValue = float.Parse(df.Dataset.Get<string>(Dicom.DicomTag.ImagePositionPatient, column));
                            if (i > 10)
                            {
                                found = true;
                                break;
                            }
                        }
                        else {
                            //Debug.Print("breaking on i = {0}", i);
                            break;
                        }
                    }
                 }
                
                //  Fill in Z value data for each slide
                for (int i = 0; i < openFileDialog1.FileNames.Count(); i++)
                {
                    DicomFile df = DicomFile.Open(newSet.names[i]);    
                    newSet.zVals[i] = df.Dataset.Get<string>(Dicom.DicomTag.ImagePositionPatient,column);
                    //Debug.Print("no columns: " + df.Dataset.Get<string>(Dicom.DicomTag.Columns));
                    //Debug.Print("Z position of " + i + " is " + newSet.zVals[i]);
                }
                replaceMin(newSet.zVals);

                DicomFile dfa = DicomFile.Open(newSet.names[0]);
                string dep = dfa.Dataset.Get<string>(Dicom.DicomTag.Rows);
                string hei = dfa.Dataset.Get<string>(Dicom.DicomTag.Columns);
                string wid = dfa.Dataset.Get<string>(Dicom.DicomTag.PixelSpacing);
                newSet.pixelSpacing = float.Parse(wid);
                //Debug.WriteLine("The pixel spacing is" + wid);
               

                pictureBox1.Image = result;

                this.clearScrBtn.Enabled = true;
                this.drawBtn.Enabled = true;
                this.prevBtn.Enabled = true;
                this.nextBtn.Enabled = true;
                this.trackBar1.Enabled = true;

                this.pictureBox1.Image = pictureBox1.Image;
                
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void bindingSource1_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel3_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {

        }

        private void tableLayoutPanel4_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle r = e.CellBounds;

            using (Pen pen = new Pen(Color.SlateGray, 0 /*1px width despite of page scale, dpi, page units*/ ))
            {
                pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;
                // define border style
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

                // decrease border rectangle height/width by pen's width for last row/column cell
                if (e.Row == (tableLayoutPanel4.RowCount - 1))
                {
                    r.Height -= 1;
                }

                if (e.Column == (tableLayoutPanel4.ColumnCount - 1))
                {
                    r.Width -= 1;
                }

                // use graphics mehtods to draw cell's border
                e.Graphics.DrawRectangle(pen, r);
            }
        }

        private void tableLayoutPanel_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle r = e.CellBounds;

            using (Pen pen = new Pen(Color.SlateGray, 0 /*1px width despite of page scale, dpi, page units*/ ))
            {
                pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;
                // define border style
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

                // decrease border rectangle height/width by pen's width for last row/column cell
                if (e.Row == (tableLayoutPanel5.RowCount - 1))
                {
                    r.Height -= 1;
                }

                if (e.Column == (tableLayoutPanel5.ColumnCount - 1))
                {
                    r.Width -= 1;
                }

                // use graphics mehtods to draw cell's border
                e.Graphics.DrawRectangle(pen, r);
            }
        }

        private void tableLayoutPanel5_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle r = e.CellBounds;

            using (Pen pen = new Pen(Color.SlateGray, 0 /*1px width despite of page scale, dpi, page units*/ ))
            {
                pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;
                // define border style
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

                // decrease border rectangle height/width by pen's width for last row/column cell
                if (e.Row == (tableLayoutPanel5.RowCount - 1))
                {
                    r.Height -= 1;
                }

                if (e.Column == (tableLayoutPanel5.ColumnCount - 1))
                {
                    r.Width -= 1;
                }

                // use graphics mehtods to draw cell's border
                e.Graphics.DrawRectangle(pen, r);
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                DicomImage image = new DicomImage(newSet.names[trackBar1.Value]);
                Image result = image.RenderImage(0);
                //Debug.WriteLine("The shown file is {0}", newSet.names[trackBar1.Value]);
                pictureBox1.Image = result;
                initialMousePos.X = 0;
                initialMousePos.Y = 0;
                currentMousePos.X = 0;
                currentMousePos.Y = 0;
                pictureBox1.Image = pictureBox1.Image;
            }
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                DicomImage image = new DicomImage(leftSet.names[trackBar2.Value]);
                Image result = image.RenderImage(0);
                //Debug.WriteLine("The shown file is {0}", newSet.names[trackBar1.Value]);
                pictureBox2.Image = result;
                initialMousePos.X = 0;
                initialMousePos.Y = 0;
                currentMousePos.X = 0;
                currentMousePos.Y = 0;
                pictureBox2.Image = pictureBox2.Image;
            }
        }
        /*
        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            if (pictureBox3.Image != null)
            {
                DicomImage image = new DicomImage(rightSet.names[trackBar3.Value]);
                Image result = image.RenderImage(0);
                //Debug.WriteLine("The shown file is {0}", newSet.names[trackBar1.Value]);
                pictureBox3.Image = result;
                initialMousePos.X = 0;
                initialMousePos.Y = 0;
                currentMousePos.X = 0;
                currentMousePos.Y = 0;
                pictureBox3.Image = pictureBox3.Image;
            }
        }
        */
        private void TrackBar1_ValueChanged(object sender, System.EventArgs e)
        {
            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (pictureBox1.Image != null && clicked == true && drawBtnDown == true)
            {
                Debug.WriteLine("mousemove");
                // Save the current position of the mouse
                currentMousePos = e.Location;

                // Force the picture box to be repainted
                pictureBox1.Image = pictureBox1.Image;
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (pictureBox1.Image != null && clicked == false && drawBtnDown == true)
            {
                clicked = true;
                this.initialMousePos = e.Location;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            //  Finished making the box.
            if (pictureBox1.Image != null && clicked == true && drawBtnDown == true)
            {
                // Save the final position of the mouse
                Point finalMousePos = e.Location;

                //  Image scaling the pixels to the CT scan pixels (which have associated distance data)
                Point p1 = new Point();
                p1.X = this.initialMousePos.X;
                p1.Y = this.initialMousePos.Y;
                Point p2 = new Point();
                p2.X = finalMousePos.X;
                p2.Y = finalMousePos.Y;

                Point unscaled_LT = scaledPoint(p1,pictureBox1);
                Point unscaled_RB = scaledPoint(p2,pictureBox1);

                //Debug.Print("The corners of the point are: {0}  {1} & {2}  {3}", unscaled_LT.X, unscaled_LT.Y, unscaled_RB.X, unscaled_RB.Y);

                Rectangle scaledRect = Rectangle.FromLTRB(
                                         unscaled_LT.X,
                                         unscaled_LT.Y,
                                         unscaled_RB.X,
                                         unscaled_RB.Y);

                //Debug.Print("The corners of the point are: {0}  {1} & {2}  {3}", unscaled_LT.X, unscaled_LT.Y, unscaled_RB.X, unscaled_RB.Y);

                //Debug.Print("The sides are left {0} top  {1} right {2}  down {3}", scaledRect.Left, scaledRect.Top, scaledRect.Right, scaledRect.Bottom);

                Rectangle drawnRect = Rectangle.FromLTRB(
                                                         this.initialMousePos.X,
                                                         this.initialMousePos.Y,
                                                         finalMousePos.X,
                                                         finalMousePos.Y);

                //  Debug statements that output the scaled pixels for the drawn rectangles, used to verify the proper scaling
                //Debug.WriteLine("The original points were: " + p1.X + ", " + p1.Y + " and " + p2.X + ", " + p2.Y);
                //Debug.WriteLine("The scaled points are: " + unscaled_LT.X + ", " + unscaled_LT.Y + " and " + unscaled_RB.X + ", " + unscaled_RB.Y);


                this.newSet.rectSet[trackBar1.Value] = new Tuple<bool, Rectangle>(true, drawnRect);
                this.newSet.scaledRectSet[trackBar1.Value] = new Tuple<bool, Rectangle>(true, scaledRect);

                clicked = false;
            }
        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            if (pictureBox2.Image != null && clicked == false && select1Down == true)
            {
                clicked = true;
            }
        }

        private void pictureBoxTab2_MouseUp(object sender, MouseEventArgs e)
        {
            // Reserved for generic mouseup event for the pictureboxes.
        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            //  Finished making the box.
            if (pictureBox2.Image != null && clicked == true && select1Down == true)
            {
                // Save the final position of the mouse
                Point finalMousePos = e.Location;
                
                this.leftPoint = scaledPoint(finalMousePos, pictureBox2);
                //Debug.Print("X {0} Y{1}", finalMousePos.X, finalMousePos.Y);
                int x, y, z;
                
                switch (orientation_left)
                {
                    case 1:
                        //  A-P case
                        x = this.leftPoint.X;
                        y = this.leftPoint.Y;
                        z = this.selectPoint3d.Item3;
                        this.unsc_selectPoint3d = new Tuple<int, int, int>(finalMousePos.X, finalMousePos.Y, this.unsc_selectPoint3d.Item3);

                        this.selectPoint3d = new Tuple<int, int, int>(x,y,(int)Math.Round(float.Parse(this.leftSet.zVals[trackBar2.Value])));
                        table_x[0].Text = ((float)this.selectPoint3d.Item1 * this.leftSet.pixelSpacing).ToString();
                        table_y[0].Text = ((float)this.selectPoint3d.Item2 * this.leftSet.pixelSpacing).ToString();
                        table_z[0].Text = this.leftSet.zVals[trackBar2.Value];


                        //Debug.Print("X {0} Y{1} Z{2}", unsc_selectPoint3d.Item1, unsc_selectPoint3d.Item2, unsc_selectPoint3d.Item3);
                        break;
                    case 2:
                        //  T-B case
                        x = this.leftPoint.X;
                        y = this.selectPoint3d.Item2;
                        z = this.leftPoint.Y;

                        this.unsc_selectPoint3d = new Tuple<int, int, int>(finalMousePos.X, this.unsc_selectPoint3d.Item2, finalMousePos.Y);
                                               
                        this.selectPoint3d = new Tuple<int, int, int>(x, int.Parse(this.leftSet.zVals[trackBar2.Value]), z);
                        table_x[0].Text = ((float)this.selectPoint3d.Item1 * this.leftSet.pixelSpacing).ToString();
                        table_y[0].Text = this.leftSet.zVals[trackBar2.Value];
                        table_z[0].Text = ((float)this.selectPoint3d.Item3 * this.leftSet.pixelSpacing).ToString();

                        //Debug.Print("X {0} Y{1} Z{2}", unsc_selectPoint3d.Item1, unsc_selectPoint3d.Item2, unsc_selectPoint3d.Item3);
                        break;
                    case 3:
                        //  L-R case
                        x = this.selectPoint3d.Item1;
                        y = this.leftPoint.Y;
                        z = this.pictureBox2.Image.Width - this.leftPoint.X;

                        this.unsc_selectPoint3d = new Tuple<int, int, int>(this.unsc_selectPoint3d.Item1, finalMousePos.Y, finalMousePos.X);

                        this.selectPoint3d = new Tuple<int, int, int>(int.Parse(this.leftSet.zVals[trackBar2.Value]), y, z);
                        table_x[0].Text = this.leftSet.zVals[trackBar2.Value];
                        table_y[0].Text = ((float)this.selectPoint3d.Item2 * this.leftSet.pixelSpacing).ToString();
                        table_z[0].Text = ((float)this.selectPoint3d.Item3 * this.leftSet.pixelSpacing).ToString();

                        //Debug.Print("X {0} Y{1} Z{2}", unsc_selectPoint3d.Item1, unsc_selectPoint3d.Item2, unsc_selectPoint3d.Item3);
                        break;
                }
                clicked = false;
                select1Down = false;
                trackBar2.Enabled = true;
                import1.Enabled = true;
                //import2.Enabled = true;
                //trackBar3.Enabled = true;
                select1.Enabled = true;
                //select2.Enabled = true;

                select1.Text = "Select Coordinate";

                pictureBox2.Image = pictureBox2.Image;
                //pictureBox3.Image = pictureBox3.Image;
            }
        }
        /*
        private void pictureBox3_MouseDown(object sender, MouseEventArgs e)
        {
            if (pictureBox3.Image != null && clicked == false && select2Down == true)
            {
                clicked = true;
            }
        }

        private void pictureBox3_MouseUp(object sender, MouseEventArgs e)
        {
            //  Finished making the box.
            if (pictureBox3.Image != null && clicked == true && select2Down == true)
            {
                // Save the final position of the mouse
                Point finalMousePos = e.Location;

                this.rightPoint = scaledPoint(finalMousePos, pictureBox3);
                Debug.Print("X {0} Y{1}", finalMousePos.X, finalMousePos.Y);
                Debug.Print("X {0} Y{1}", rightPoint.X, rightPoint.Y);
                int x, y, z;
                Debug.Print("picturebox3 mouseup");
                switch (orientation_right)
                {
                    case 1:
                        //  A-P case
                        x = this.rightPoint.X;
                        y = this.rightPoint.Y;
                        z = int.Parse(this.rightSet.zVals[trackBar3.Value]);

                        this.unsc_selectPoint3d = new Tuple<int, int, int>(finalMousePos.X, finalMousePos.Y, this.unsc_selectPoint3d.Item3);
                        
                        this.selectPoint3d = new Tuple<int, int, int>(x, y, z);
                        table_x[0].Text = ((float)this.selectPoint3d.Item1 * this.rightSet.pixelSpacing).ToString();
                        table_y[0].Text = ((float)this.selectPoint3d.Item2 * this.rightSet.pixelSpacing).ToString();
                        table_z[0].Text = this.rightSet.zVals[trackBar3.Value];
                        
                        Debug.Print("X {0} Y{1} Z{2}", x, y, z);
                        break;
                    case 2:
                        //  T-B case
                        x = this.rightPoint.X;
                        y = (int)Math.Round(float.Parse(this.rightSet.zVals[trackBar3.Value]));
                        z = this.rightPoint.Y;

                        this.unsc_selectPoint3d = new Tuple<int, int, int>(finalMousePos.X, this.unsc_selectPoint3d.Item2, finalMousePos.Y);

                        table_x[0].Text = ((float)this.selectPoint3d.Item1 * this.rightSet.pixelSpacing).ToString();
                        table_y[0].Text = this.rightSet.zVals[trackBar3.Value];
                        table_z[0].Text = ((float)this.selectPoint3d.Item3 * this.rightSet.pixelSpacing).ToString();


                        this.selectPoint3d = new Tuple<int, int, int>(x, y, z);
                        Debug.Print("X {0} Y{1} Z{2}", x, y, z);
                        break;
                    case 3:
                        //  L-R case
                        x = int.Parse(this.rightSet.zVals[trackBar3.Value]);
                        y = this.rightPoint.Y;
                        z = this.pictureBox3.Image.Width-this.rightPoint.X;

                        this.unsc_selectPoint3d = new Tuple<int, int, int>(this.unsc_selectPoint3d.Item1, finalMousePos.Y, finalMousePos.X);

                        this.selectPoint3d = new Tuple<int, int, int>(x, y, z);
                        table_x[0].Text = this.rightSet.zVals[trackBar3.Value];
                        table_y[0].Text = ((float)this.selectPoint3d.Item2 * this.rightSet.pixelSpacing).ToString();
                        table_z[0].Text = ((float)this.selectPoint3d.Item3 * this.rightSet.pixelSpacing).ToString();

                        Debug.Print("X {0} Y{1} Z{2}", x, y, z);
                        break;
                }

                table_x[0].Text = ((float)this.selectPoint3d.Item1 * this.rightSet.pixelSpacing).ToString();
                table_y[0].Text = ((float)this.selectPoint3d.Item2 * this.rightSet.pixelSpacing).ToString();
                table_z[0].Text = ((float)this.selectPoint3d.Item3 * this.rightSet.pixelSpacing).ToString();


                clicked = false;
                select2Down = false;

                trackBar2.Enabled = true;
                import1.Enabled = true;
                import2.Enabled = true;
                trackBar3.Enabled = true;
                select1.Enabled = true;
                select2.Enabled = true;

                select2.Text = "Select Coordinate";

                pictureBox2.Image = pictureBox2.Image;
                pictureBox3.Image = pictureBox3.Image;
            }
        }
        */
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {

            if (pictureBox1.Image != null)
            {
                /*
                DicomImage image = new DicomImage(newSet.names[trackBar1.Value]);
                Image result = image.RenderImage(0);
                //Debug.WriteLine("The shown file is {0}", newSet.names[trackBar1.Value]);
                pictureBox1.Image = result;*/

                using (Font myFont = new Font("Arial", 14))
                {

                    string output = String.Format("Slide {0} of {1}, Z measurement {2} mm", this.trackBar1.Value, (newSet.numDcms - 1), this.newSet.zVals[this.trackBar1.Value]);
                    int leftFill, topFill;

                    int w_i = pictureBox1.Image.Width;
                    int h_i = pictureBox1.Image.Height;
                    int w_c = pictureBox1.Width;
                    int h_c = pictureBox1.Height;

                    float imageRatio = w_i / (float)h_i; // image W:H ratio
                    float containerRatio = w_c / (float)h_c; // container W:H ratio

                    if (imageRatio >= containerRatio)
                    {   // Horizontal Image
                        float scaleFactor = w_c / (float)w_i;
                        topFill = (int)((h_c - h_i * scaleFactor) / 2) + 20;
                        leftFill = 30;
                    }
                    else
                    {
                        float scaleFactor = h_c / (float)h_i;
                        leftFill = (int)((w_c - w_i * scaleFactor) / 2) + 20;
                        topFill = 30;
                    }

                    e.Graphics.DrawString(output, myFont, Brushes.AntiqueWhite, new Point(leftFill, topFill));
                }
                for (int ii = 0; ii < newSet.rectSet.Count(); ii++)
                {
                    Color myColor = Color.LightGreen;
                    if (ii == trackBar1.Value)
                    {
                        myColor = Color.LightGreen;
                    }
                    else if (ii < trackBar1.Value)
                    {
                        myColor = Color.Red;
                    }
                    else if (ii > trackBar1.Value)
                    {
                        myColor = Color.Blue;
                    }

                    if (newSet.rectSet[ii].Item1 == true)
                    {
                        using (Pen p = new Pen(myColor, 1.5F))
                        {
                            // Create a rectangle with the initial cursor location as the upper-left
                            // point, and the current cursor location as the bottom-right point
                            Rectangle currentRect = this.newSet.rectSet[ii].Item2;

                            // Draw the rectangle
                            e.Graphics.DrawRectangle(p, currentRect);
                        }
                    }

                    else
                    {
                        using (Pen p = new Pen(Color.LightGreen, 1.5F))
                        {
                            // Create a rectangle with the initial cursor location as the upper-left
                            // point, and the current cursor location as the bottom-right point
                            Rectangle currentRect = Rectangle.FromLTRB(
                                                                       this.initialMousePos.X,
                                                                       this.initialMousePos.Y,
                                                                       currentMousePos.X,
                                                                       currentMousePos.Y);

                            // Draw the rectangle
                            e.Graphics.DrawRectangle(p, currentRect);
                        }
                    }
                }
            }
        }
        /*
        private void pictureBox2_OnPaint(object sender, PaintEventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                DicomImage image = new DicomImage(leftSet.names[trackBar3.Value]);
                Image result = image.RenderImage(0);
                pictureBox2.Image = result;
            }
        }

        private void pictureBox3_OnPaint(object sender, PaintEventArgs e)
        {
            if (pictureBox3.Image != null)
            {
                DicomImage image = new DicomImage(rightSet.names[trackBar3.Value]);
                Image result = image.RenderImage(0);
                pictureBox3.Image = result;
            }
        }*/

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                //Debug.Print("painting");
                using (Font myFont = new Font("Arial", 10))
                {
                    if (unsc_selectPoint3d.Item1 != 0 || unsc_selectPoint3d.Item2 != 0 || unsc_selectPoint3d.Item3 != 0)
                    {
                        Pen graphPen = new Pen(Color.Red, 1.5f);
                        int[] myCorners = fillToEdge(pictureBox2);
                        
                        switch (orientation_left)
                        {
                            case 1:
                                // A-P
                                if (unsc_selectPoint3d.Item1 != 0)
                                {
                                    Point startPoint = new Point(unsc_selectPoint3d.Item1, myCorners[1]);
                                    Point endPoint = new Point(unsc_selectPoint3d.Item1, myCorners[3]);
                                    e.Graphics.DrawLine(graphPen, startPoint, endPoint);
                                }
                                if (unsc_selectPoint3d.Item2 != 0)
                                {
                                    Point startPoint = new Point(myCorners[0], unsc_selectPoint3d.Item2);
                                    Point endPoint = new Point(myCorners[2], unsc_selectPoint3d.Item2);
                                    e.Graphics.DrawLine(graphPen, startPoint, endPoint);
                                }
                                break;
                            case 2:
                                // T-B
                                if (unsc_selectPoint3d.Item1 != 0)
                                {
                                    Point startPoint = new Point(unsc_selectPoint3d.Item1, myCorners[1]);
                                    Point endPoint = new Point(unsc_selectPoint3d.Item1, myCorners[3]);
                                    e.Graphics.DrawLine(graphPen, startPoint, endPoint);
                                }
                                if (unsc_selectPoint3d.Item3 != 0)
                                {
                                    Point startPoint = new Point(myCorners[0], unsc_selectPoint3d.Item3);
                                    Point endPoint = new Point(myCorners[2], unsc_selectPoint3d.Item3);
                                    e.Graphics.DrawLine(graphPen, startPoint, endPoint);
                                }
                                
                                break;
                            case 3:
                                // L-R
                                if (unsc_selectPoint3d.Item2 != 0)
                                {
                                    Point startPoint = new Point(myCorners[0], unsc_selectPoint3d.Item2);
                                    Point endPoint = new Point(myCorners[2], unsc_selectPoint3d.Item2);
                                    e.Graphics.DrawLine(graphPen, startPoint, endPoint);
                                }
                                if (unsc_selectPoint3d.Item3 != 0)
                                {
                                    Point startPoint = new Point(unsc_selectPoint3d.Item3, myCorners[1]);
                                    Point endPoint = new Point(unsc_selectPoint3d.Item3, myCorners[3]);
                                    e.Graphics.DrawLine(graphPen, startPoint, endPoint);
                                }
                                break;

                        }
                        //Debug.Print("Status4");

                    }
                    //Debug.Print("Status5");
                    string output = String.Format("Slide {0} of {1}, Z measurement {2} mm", this.trackBar2.Value, (leftSet.numDcms - 1), this.leftSet.zVals[this.trackBar2.Value]);
                    int leftFill, topFill;

                    int w_i = pictureBox2.Image.Width;
                    int h_i = pictureBox2.Image.Height;
                    int w_c = pictureBox2.Width;
                    int h_c = pictureBox2.Height;

                    float imageRatio = w_i / (float)h_i; // image W:H ratio
                    float containerRatio = w_c / (float)h_c; // container W:H ratio

                    if (imageRatio >= containerRatio)
                    {   // Horizontal Image
                        float scaleFactor = w_c / (float)w_i;
                        topFill = (int)((h_c - h_i * scaleFactor) / 2) + 20;
                        leftFill = 30;
                    }
                    else
                    {
                        float scaleFactor = h_c / (float)h_i;
                        leftFill = (int)((w_c - w_i * scaleFactor) / 2) + 20;
                        topFill = 30;
                    }

                    e.Graphics.DrawString(output, myFont, Brushes.LightGreen, new Point(leftFill, topFill));
                }
            }
        }
        /*
        private void pictureBox3_Paint(object sender, PaintEventArgs e)
        {
            if (pictureBox3.Image != null)
            {
                

                Debug.Print("Status1");
                using (Font myFont = new Font("Arial", 10))
                {
                    Debug.Print("Status2");
                    if (unsc_selectPoint3d.Item1 != 0 || unsc_selectPoint3d.Item2 != 0 || unsc_selectPoint3d.Item3 != 0)
                    {
                        Debug.Print("Status3");
                        Pen graphPen = new Pen(Color.Red, 1.5f);
                        int[] myCorners = fillToEdge(pictureBox3);
                        Debug.Print("corenr 1", myCorners[0]);


                        switch (orientation_right)
                        {
                            case 1:
                                // A-P
                                Debug.Print("Status3a");
                                if (unsc_selectPoint3d.Item1 != 0)
                                {
                                    Point startPoint = new Point(unsc_selectPoint3d.Item1, myCorners[1]);
                                    Point endPoint = new Point(unsc_selectPoint3d.Item1, myCorners[3]);
                                    e.Graphics.DrawLine(graphPen, startPoint, endPoint);
                                }
                                if (unsc_selectPoint3d.Item2 != 0)
                                {
                                    Point startPoint = new Point(myCorners[0], unsc_selectPoint3d.Item2);
                                    Point endPoint = new Point(myCorners[2], unsc_selectPoint3d.Item2);
                                    e.Graphics.DrawLine(graphPen, startPoint, endPoint);
                                }
                                break;
                            case 2:
                                // T-B
                                Debug.Print("Status3b");
                                if (unsc_selectPoint3d.Item1 != 0)
                                {
                                    Debug.Print("Drawline");
                                    Point startPoint = new Point(unsc_selectPoint3d.Item1, myCorners[1]);
                                    Point endPoint = new Point(unsc_selectPoint3d.Item1, myCorners[3]);
                                    e.Graphics.DrawLine(graphPen, startPoint, endPoint);
                                }
                                if (unsc_selectPoint3d.Item3 != 0)
                                {
                                    Debug.Print("Drawline");
                                    Point startPoint = new Point(myCorners[0], unsc_selectPoint3d.Item3);
                                    Point endPoint = new Point(myCorners[2], unsc_selectPoint3d.Item3);
                                    e.Graphics.DrawLine(graphPen, startPoint, endPoint);
                                }

                                break;
                            case 3:
                                // L-R
                                Debug.Print("Status3c");
                                if (unsc_selectPoint3d.Item2 != 0)
                                {
                                    Debug.Print("Drawline");
                                    Point startPoint = new Point(myCorners[0], unsc_selectPoint3d.Item2);
                                    Point endPoint = new Point(myCorners[2], unsc_selectPoint3d.Item2);
                                    e.Graphics.DrawLine(graphPen, startPoint, endPoint);
                                }
                                if (unsc_selectPoint3d.Item3 != 0)
                                {
                                    Debug.Print("Drawline");
                                    Point startPoint = new Point(unsc_selectPoint3d.Item3, myCorners[1]);
                                    Point endPoint = new Point(unsc_selectPoint3d.Item3, myCorners[3]);
                                    e.Graphics.DrawLine(graphPen, startPoint, endPoint);
                                }
                                break;

                        }
                        Debug.Print("Status4");

                    }
                    Debug.Print("Status5");
                    string output = String.Format("Slide {0} of {1}, Z measurement {2} mm", this.trackBar3.Value, (rightSet.numDcms - 1), this.rightSet.zVals[this.trackBar3.Value]);

                        
                    int leftFill, topFill;

                    int w_i = pictureBox3.Image.Width;
                    int h_i = pictureBox3.Image.Height;
                    int w_c = pictureBox3.Width;
                    int h_c = pictureBox3.Height;

                    float imageRatio = w_i / (float)h_i; // image W:H ratio
                    float containerRatio = w_c / (float)h_c; // container W:H ratio

                    if (imageRatio >= containerRatio)
                    {   // Horizontal Image
                        float scaleFactor = w_c / (float)w_i;
                        topFill = (int)((h_c - h_i * scaleFactor) / 2) + 20;
                        leftFill = 30;
                    }
                    else
                    {
                        float scaleFactor = h_c / (float)h_i;
                        leftFill = (int)((w_c - w_i * scaleFactor) / 2) + 20;
                        topFill = 30;
                    }

                    e.Graphics.DrawString(output, myFont, Brushes.LightGreen, new Point(leftFill, topFill));
                }
            }
        }
        */
        private void drawBtn_Click(object sender, EventArgs e)
        {
            if (drawBtnDown == false)
            {
                trackBar1.Enabled = false;
                clearScrBtn.Enabled = false;
                debugBtn.Enabled = false;
                showButton.Enabled = false;
                prevBtn.Enabled = false;
                nextBtn.Enabled = false;
                drawBtn.Text = "Stop Drawing";
                drawBtnDown = true;
            }
            else
            {
                drawBtn.Text = "Draw";
                drawBtnDown = false;
                trackBar1.Enabled = true;
                clearScrBtn.Enabled = true;
                showButton.Enabled = true;
                debugBtn.Enabled = true;
                prevBtn.Enabled = true;
                nextBtn.Enabled = true;
            }
        }

        private void debugBtn_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                makeShape();
                pathOutput();
            }
                

        }

        private void nextBtn_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("The trackbar val is " + trackBar1.Value, " and the trackbar max is " + trackBar1.Maximum);
            if (pictureBox1.Image != null && trackBar1.Value < (trackBar1.Maximum-1) && drawBtnDown == false)
            {
                DicomImage image = new DicomImage(newSet.names[trackBar1.Value+1]);
                Image result = image.RenderImage(0);
                pictureBox1.Image = result;
                initialMousePos.X = 0;
                initialMousePos.Y = 0;
                currentMousePos.X = 0;
                currentMousePos.Y = 0;
                trackBar1.Value = trackBar1.Value + 1;
                pictureBox1.Image = pictureBox1.Image;
            }
        }

        private void prevBtn_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("The trackbar val is " + trackBar1.Value, " and the trackbar max is " + trackBar1.Maximum);
            if (pictureBox1.Image != null && trackBar1.Value > 0 && drawBtnDown == false)
            {
                DicomImage image = new DicomImage(newSet.names[trackBar1.Value - 1]);
                Image result = image.RenderImage(0);
                pictureBox1.Image = result;
                initialMousePos.X = 0;
                initialMousePos.Y = 0;
                currentMousePos.X = 0;
                currentMousePos.Y = 0;
                trackBar1.Value = trackBar1.Value - 1;
                pictureBox1.Image = pictureBox1.Image;
            }
        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void importLeft_Click(object sender, EventArgs e)
        {
            // Import DICOM Left Pane Localization Button
            //  When the import DICOM button is clicked, the user is presented with a file selection dialogue

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // The Maximum property sets the value of the track bar when 
                // the slider is all the way to the right.
                trackBar2.Maximum = openFileDialog1.FileNames.Count() - 1;

                // The TickFrequency property establishes how many positions 
                // are between each tick-mark.
                trackBar2.TickFrequency = 1;

                // The LargeChange property sets how many positions to move 
                // if the bar is clicked on either side of the slider.
                trackBar2.LargeChange = trackBar2.Maximum / 8;

                // The SmallChange property sets how many positions to move 
                // if the keyboard arrows are used to move the slider.

                trackBar2.SmallChange = 1;

                leftSet.add(openFileDialog1.FileNames);
                DicomImage image = new DicomImage(leftSet.names[0]);
                Image result = image.RenderImage(0);


                //  Find the changing z value
                bool found = false;
                int column = -1;
                while (found == false)
                {
                    column++;
                    float lastValue = -13.3f;       // arbitrary value
                    for (int i = 0; i < openFileDialog1.FileNames.Count(); i++)
                    {
                        DicomFile df = DicomFile.Open(leftSet.names[i]);
                        if (lastValue != float.Parse(df.Dataset.Get<string>(Dicom.DicomTag.ImagePositionPatient, column)))
                        {
                            lastValue = float.Parse(df.Dataset.Get<string>(Dicom.DicomTag.ImagePositionPatient, column));
                            if (i > 10)
                            {
                                found = true;
                                break;
                            }
                        }
                        else
                        {
                            //Debug.Print("breaking on i = {0}", i);
                            break;
                        }
                    }
                }


                //  Fill in Z value data for each slide
                for (int i = 0; i < openFileDialog1.FileNames.Count(); i++)
                {
                    DicomFile df = DicomFile.Open(leftSet.names[i]);
                    leftSet.zVals[i] = df.Dataset.Get<string>(Dicom.DicomTag.ImagePositionPatient, column);

                }
                replaceMin(leftSet.zVals);

                DicomFile dfa = DicomFile.Open(leftSet.names[0]);
                string dep = dfa.Dataset.Get<string>(Dicom.DicomTag.Rows);
                string hei = dfa.Dataset.Get<string>(Dicom.DicomTag.Columns);
                string wid = dfa.Dataset.Get<string>(Dicom.DicomTag.PixelSpacing);
                leftSet.pixelSpacing = float.Parse(wid);

                pictureBox2.Image = result;
                this.trackBar2.Enabled = true;
            }

        }

        private void flowLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }
        /*
        private void importRight_Click(object sender, EventArgs e)
        {
            // Import DICOM Right Pane Localization Button
            //  When the import DICOM button is clicked, the user is presented with a file selection dialogue

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // The Maximum property sets the value of the track bar when 
                // the slider is all the way to the right.
                trackBar3.Maximum = openFileDialog1.FileNames.Count() - 1;

                // The TickFrequency property establishes how many positions 
                // are between each tick-mark.
                trackBar3.TickFrequency = 1;

                // The LargeChange property sets how many positions to move 
                // if the bar is clicked on either side of the slider.
                trackBar3.LargeChange = trackBar3.Maximum / 8;

                // The SmallChange property sets how many positions to move 
                // if the keyboard arrows are used to move the slider.
                trackBar3.SmallChange = 1;

                rightSet.add(openFileDialog1.FileNames);
                DicomImage image = new DicomImage(rightSet.names[0]);
                Image result = image.RenderImage(0);

                //  Find the changing z value
                bool found = false;
                int column = -1;
                while (found == false)
                {
                    column++;
                    float lastValue = -13.3f;       // arbitrary value
                    for (int i = 0; i < openFileDialog1.FileNames.Count(); i++)
                    {
                        DicomFile df = DicomFile.Open(rightSet.names[i]);
                        if (lastValue != float.Parse(df.Dataset.Get<string>(Dicom.DicomTag.ImagePositionPatient, column)))
                        {
                            lastValue = float.Parse(df.Dataset.Get<string>(Dicom.DicomTag.ImagePositionPatient, column));
                            if (i > 10)
                            {
                                found = true;
                                break;
                            }
                        }
                        else
                        {
                            Debug.Print("breaking on i = {0}", i);
                            break;
                        }
                    }
                }


                //  Fill in Z value data for each slide
                for (int i = 0; i < openFileDialog1.FileNames.Count(); i++)
                {
                    DicomFile df = DicomFile.Open(rightSet.names[i]);
                    rightSet.zVals[i] = df.Dataset.Get<string>(Dicom.DicomTag.ImagePositionPatient, column);
                }
                replaceMin(rightSet.zVals);

                DicomFile dfa = DicomFile.Open(rightSet.names[0]);
                string dep = dfa.Dataset.Get<string>(Dicom.DicomTag.Rows);
                string hei = dfa.Dataset.Get<string>(Dicom.DicomTag.Columns);
                string wid = dfa.Dataset.Get<string>(Dicom.DicomTag.PixelSpacing);
                rightSet.pixelSpacing = float.Parse(wid);
                //Debug.WriteLine("The pixel spacing is" + wid);


                pictureBox3.Image = result;
                this.trackBar3.Enabled = true;

            }
        }*/

        private void select1_Click(object sender, EventArgs e)
        {
            if ( select1Down == false)
            {
                trackBar2.Enabled = false;
                //trackBar3.Enabled = false;
                import1.Enabled = false;
                //import2.Enabled = false;
                //select1.Enabled = false;
                select1.Text = "Cancel Selection";
                //select2.Enabled = false;
                select1Down = true;
            }
            else
            {
                trackBar2.Enabled = true;
                //trackBar3.Enabled = true;
                import1.Enabled = true;
                //import2.Enabled = true;
                
                
                select1.Text = "Select Coordinate";
                //select2.Enabled = true;
                select1Down = false;
            }
        }
        /*
        private void select2_Click(object sender, EventArgs e)
        {
            if (select2Down == false)
            {
                trackBar2.Enabled = true;
                trackBar3.Enabled = true;
                import1.Enabled = true;
                import2.Enabled = true;               
                
                select2.Text = "Cancel Selection";
                select2.Enabled = true;
                select2Down = true;
            }
            else
            {
                trackBar2.Enabled = true;
                trackBar3.Enabled = true;
                import1.Enabled = true;
                import2.Enabled = true;

                select2.Text = "Select Coordinate";
                select1.Enabled = true;
                select2Down = false;
            }
        }*/
        /*
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked == true && radioButton4.Enabled == true) {
                orientation_right = 1;
                pictureBox3.Image = pictureBox3.Image;
                Debug.Print("right {0}", orientation_right);
            }
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton5.Checked == true && radioButton5.Enabled == true)
            {
                orientation_right = 2;
                pictureBox3.Image = pictureBox3.Image;
                Debug.Print("right {0}", orientation_right);
            }
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton6.Checked == true && radioButton6.Enabled == true)
            {
                orientation_right = 3;
                pictureBox3.Image = pictureBox3.Image;
                Debug.Print("right {0}", orientation_right);
            }
        }*/

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            // L-R
            if (radioButton1.Checked == true && radioButton1.Enabled == true)
            {
                orientation_left = 3;
                pictureBox2.Image = pictureBox2.Image;
                //Debug.Print("left {0}", orientation_left);
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            // T-B
            if (radioButton2.Checked == true && radioButton2.Enabled == true)
            {
                orientation_left = 2;
                pictureBox2.Image = pictureBox2.Image;
                //Debug.Print("left {0}", orientation_left);
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            // A-P
            if (radioButton3.Checked == true && radioButton3.Enabled == true)
            {
                orientation_left = 1;
                pictureBox2.Image = pictureBox2.Image;
                //Debug.Print("left {0}", orientation_left);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Debug.Print("Left {0} Right {1}", orientation_left, orientation_right);
        }

        private void flowLayoutPanel11_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel4_Paint(object sender, PaintEventArgs e)
        {

        }
 
        private void exportPoint_Click(object sender, EventArgs e)
        {
            string path = coordPathDir;
            bool inUse = true;
            FileStream file;
            //int iter = 0;
            file = File.Open(path, FileMode.Create);
            file.Close();
            //  Finalizing a point.
            int count = 0;  //  Localized coordinate count
            for (int i = 1; i < 9; i++)
            {
                if (table_x[i].Text != "0" && table_y[i].Text != "0" && table_z[i].Text != "0")
                {
                    count++;
                }
            }

            string firstCount = String.Format("{0}", count);
            if (!File.Exists(path))
            {
                File.WriteAllText(path, firstCount);
                File.AppendAllText(path, Environment.NewLine);
            }
            else
            {
                File.AppendAllText(path, firstCount);
                File.AppendAllText(path, Environment.NewLine);
            }

            Debug.Print("yo yo yo1");
            for (int i = 1; i < 9; i++)
            {
                Debug.Print("yo yo yo2");
                if (table_x[i].Text != "0" && table_y[i].Text != "0" && table_z[i].Text != "0")
                {
                    string output = String.Format("{0} {1:0.00} {2:0.00} {3:0.00}", table_name[i].Text, table_x[i].Text, table_y[i].Text, table_z[i].Text);
                    
                    if (!File.Exists(path))
                    {
                        File.WriteAllText(path, output);
                        File.AppendAllText(path, Environment.NewLine);
                    }
                    else
                    {
                        File.AppendAllText(path, output);
                        File.AppendAllText(path, Environment.NewLine);
                    }
                }

            }
        }

        private void finalizePoint_Click(object sender, EventArgs e)
        {
            //  Finalizing a point.
            if ((this.selectPoint3d.Item1 != 0 || this.selectPoint3d.Item2 != 0 || this.selectPoint3d.Item3 != 0) && coordCount < 8)
            {
                //Debug.Print("coordcount is {0}", coordCount);
                coordCount++;
                table_name[coordCount].Text = textBox1.Text;
                table_x[coordCount].Text = table_x[0].Text;
                table_y[coordCount].Text = table_y[0].Text;
                table_z[coordCount].Text = table_z[0].Text;
                table_name[coordCount].Text = textBox1.Text;
                this.tableReset();
            }
        }

        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void flowLayoutPanel11_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel5_Paint(object sender, PaintEventArgs e)
        {

        }



        
    }
}
