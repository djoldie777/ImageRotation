using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace image_rotation
{
    public partial class Form1 : Form
    {
        private Bitmap bm1, bm2;
        private double angle;
        private Form2 form2;
        private LinkedList<Tuple<Bitmap, double>> cache;
        private bool isRotated, isRotating, isMouseDown, isSelected;
        Point startPoint;
        Rectangle selectedArea;
        Pen pen;

        public Form1()
        {
            InitializeComponent();
            bm1 = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = bm1;
            bm2 = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            pictureBox2.Image = bm2;
            angle = 30;
            form2 = new Form2(angle);
            cache = new LinkedList<Tuple<Bitmap, double>>();
            isRotated = false;
            isRotating = false;
            startPoint = new Point();
            isMouseDown = false;
            selectedArea = new Rectangle();
            pen = new Pen(Brushes.Green);
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            isSelected = false;
            rotateToolStripMenuItem.Enabled = false;
            clearToolStripMenuItem.Enabled = false;
            resetToolStripMenuItem.Enabled = false;
            cancelToolStripMenuItem.Enabled = false;
        }

        private void openFunc()
        {
            openFileDialog1.InitialDirectory = System.IO.Path.Combine(Application.StartupPath, "Images");

            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            bm1 = Bitmap.FromFile(openFileDialog1.FileName) as Bitmap;
            bm2 = Bitmap.FromFile(openFileDialog1.FileName) as Bitmap;
            pictureBox1.Image = bm1;
            pictureBox2.Image = bm2;

            pictureBox2.Location = new Point(pictureBox1.Location.X + pictureBox1.Width + 15, pictureBox2.Location.Y);
            this.Size = new Size(pictureBox2.Location.X + pictureBox2.Width + 28, pictureBox2.Location.Y + pictureBox2.Height + 43);
            pictureBox1.Location = new Point(pictureBox1.Location.X, (Size.Height - pictureBox1.Height) / 2 - 8);
            this.CenterToScreen();

            angle = form2.Angle;
            cache.Clear();
            isRotated = false;
            isRotating = false;
            isSelected = false;
            selectedArea = new Rectangle();
            rotateToolStripMenuItem.Enabled = true;
            clearToolStripMenuItem.Enabled = true;
            cancelToolStripMenuItem.Enabled = false;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFunc();
        }

        Point rotate(Point p, Point center, double angle)
        {
            double newX = (p.X - center.X) * Math.Cos(angle) - (p.Y - center.Y) * Math.Sin(angle) + center.X;
            double newY = (p.Y - center.Y) * Math.Cos(angle) + (p.X - center.X) * Math.Sin(angle) + center.Y;

            return new Point((int)newX, (int)newY);
        }

        void getNewRectangleSize(int x, int y, int width, int height, double angle, ref int newWidth, ref int newHeight, ref int dx, ref int dy)
        {
            Point centerPoint = new Point(x + width / 2, y + height / 2);

            Point p1 = rotate(new Point(x, y), centerPoint, angle);
            Point p2 = rotate(new Point(x + width - 1, y), centerPoint, angle);
            Point p3 = rotate(new Point(x, y + height - 1), centerPoint, angle);
            Point p4 = rotate(new Point(x + width - 1, y + height - 1), centerPoint, angle);

            int width1 = Math.Abs(p1.X - p4.X);
            int width2 = Math.Abs(p2.X - p3.X);
            int height1 = Math.Abs(p1.Y - p4.Y);
            int height2 = Math.Abs(p2.Y - p3.Y);

            if (width1 > bm1.Width || width2 > bm1.Width)
                newWidth = width1 > width2 ? width1 : width2;
            else
                newWidth = bm1.Width;

            if (height1 > bm1.Height || height2 > bm1.Height)
                newHeight = height1 > height2 ? height1 : height2;
            else
                newHeight = bm1.Height;

            int minX = Math.Min(p1.X, Math.Min(p2.X, Math.Min(p3.X, p4.X)));
            int maxX = Math.Max(p1.X, Math.Max(p2.X, Math.Max(p3.X, p4.X)));
            int minY = Math.Min(p1.Y, Math.Min(p2.Y, Math.Min(p3.Y, p4.Y)));
            int maxY = Math.Max(p1.Y, Math.Max(p2.Y, Math.Max(p3.Y, p4.Y)));

            if (minX < 0)
                dx = minX;

            if (maxX > newWidth - 1)
                dx = maxX - newWidth + 1;

            if (minY < 0)
                dy = minY;

            if (maxY > newHeight - 1)
                dy = maxY - newHeight + 1;
        }

        private void rotateFunc()
        {
            int areaX, areaY, areaWidth, areaHeight;
            int dx = 0, dy = 0, newWidth = pictureBox1.Width, newHeight = pictureBox1.Height;

            if (isSelected)
            {
                areaX = selectedArea.X;
                areaY = selectedArea.Y;
                areaWidth = selectedArea.Width;
                areaHeight = selectedArea.Height;
            }
            else
            {
                areaX = 0;
                areaY = 0;
                areaWidth = bm1.Width;
                areaHeight = bm1.Height;
            }

            if (cache.Count < form2.Cache)
            {
                if (isRotated)
                    cache.AddFirst(new Tuple<Bitmap, double>(bm2.Clone() as Bitmap, angle));
                else
                    cache.AddFirst(new Tuple<Bitmap, double>(bm2.Clone() as Bitmap, 0));
            }
            else if (cache.Count != 0)
            {
                cache.RemoveLast();

                if (isRotated)
                    cache.AddFirst(new Tuple<Bitmap, double>(bm2.Clone() as Bitmap, angle));
                else
                    cache.AddFirst(new Tuple<Bitmap, double>(bm2.Clone() as Bitmap, 0));
            }

            if (isRotated)
                angle = (angle + form2.Angle) % 360;
            else
                angle = form2.Angle;

            double radianAngle = angle * Math.PI / 180;

            getNewRectangleSize(areaX, areaY, areaWidth, areaHeight, radianAngle, ref newWidth, ref newHeight, ref dx, ref dy);

            bm2 = new Bitmap(newWidth, newHeight);

            BitmapData initialData = bm1.LockBits(new Rectangle(0, 0, bm1.Width, bm1.Height), ImageLockMode.ReadWrite,
                                    PixelFormat.Format32bppRgb);
            BitmapData rotatedData = bm2.LockBits(new Rectangle(0, 0, bm2.Width, bm2.Height), ImageLockMode.ReadWrite,
                                    PixelFormat.Format32bppArgb);
            IntPtr initialPointer = initialData.Scan0;
            IntPtr rotatedPointer = rotatedData.Scan0;
            int initialBytes = Math.Abs(initialData.Stride) * bm1.Height;
            int rotatedBytes = Math.Abs(rotatedData.Stride) * bm2.Height;
            byte[] initialValues = new byte[initialBytes];
            byte[] rotatedValues = new byte[rotatedBytes];

            System.Runtime.InteropServices.Marshal.Copy(initialPointer, initialValues, 0, initialBytes);
            System.Runtime.InteropServices.Marshal.Copy(rotatedPointer, rotatedValues, 0, rotatedBytes);

            Point centerPoint = new Point(areaX + areaWidth / 2, areaY + areaHeight / 2);
            double radius = (Math.Sqrt(bm1.Width * bm1.Width + bm1.Height * bm1.Height)) / 2;

            for (int i = 0; i < rotatedValues.Length; i += 4)
            {
                int x = (i / 4) % bm2.Width + dx;
                int y = (i / 4) / bm2.Width + dy;

                double distanceToCenter = Math.Sqrt((x - centerPoint.X) * (x - centerPoint.X) + (y - centerPoint.Y) * (y - centerPoint.Y));

                if (distanceToCenter > radius)
                    continue;

                Point initialPoint = rotate(new Point(x, y), centerPoint, -radianAngle);

                if (initialPoint.X >= areaX && initialPoint.X < areaX + areaWidth &&
                    initialPoint.Y >= areaY && initialPoint.Y < areaY + areaHeight)
                {
                    int index = (bm1.Width * initialPoint.Y + initialPoint.X) * 4;

                    rotatedValues[i] = initialValues[index];
                    rotatedValues[i + 1] = initialValues[index + 1];
                    rotatedValues[i + 2] = initialValues[index + 2];
                    rotatedValues[i + 3] = 255;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(initialValues, 0, initialPointer, initialBytes);
            System.Runtime.InteropServices.Marshal.Copy(rotatedValues, 0, rotatedPointer, rotatedBytes);

            bm1.UnlockBits(initialData);
            bm2.UnlockBits(rotatedData);

            pictureBox2.Image = bm2;

            this.Size = new Size(pictureBox2.Location.X + pictureBox2.Width + 29, pictureBox2.Location.Y + pictureBox2.Height + 43);
            pictureBox1.Location = new Point(pictureBox1.Location.X, (Size.Height - pictureBox1.Height) / 2 - 8);
            this.CenterToScreen();

            isRotated = true;
            isRotating = true;
            resetToolStripMenuItem.Enabled = true;

            if (form2.Cache > 0)
                cancelToolStripMenuItem.Enabled = true;
        }

        private void rotateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rotateFunc();
        }

        private void clearFunc()
        {
            Graphics g = Graphics.FromImage(bm1);
            g.Clear(Color.White);
            g.Dispose();
            pictureBox1.Invalidate();

            bm2 = bm1.Clone() as Bitmap;
            pictureBox2.Image = bm2;

            this.Size = new Size(pictureBox2.Location.X + pictureBox2.Width + 29, pictureBox2.Location.Y + pictureBox2.Height + 43);
            pictureBox1.Location = new Point(pictureBox1.Location.X, (Size.Height - pictureBox1.Height) / 2 - 8);
            this.CenterToScreen();

            rotateToolStripMenuItem.Enabled = false;
            clearToolStripMenuItem.Enabled = false;
            resetToolStripMenuItem.Enabled = false;
            cancelToolStripMenuItem.Enabled = false;
        }

        private void clearToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            clearFunc();
        }

        private void resetFunc()
        {
            pictureBox1.Refresh();
            bm2 = bm1.Clone() as Bitmap;
            pictureBox2.Image = bm2;

            this.Size = new Size(pictureBox2.Location.X + pictureBox2.Width + 29, pictureBox2.Location.Y + pictureBox2.Height + 43);
            pictureBox1.Location = new Point(pictureBox1.Location.X, (Size.Height - pictureBox1.Height) / 2 - 8);
            this.CenterToScreen();

            angle = form2.Angle;
            cache.Clear();
            isRotated = false;
            isRotating = false;
            isSelected = false;
            resetToolStripMenuItem.Enabled = false;
            cancelToolStripMenuItem.Enabled = false;
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetFunc();
        }

        private void cancelFunc()
        {
            Tuple<Bitmap, double> previous = cache.First.Value;

            angle = previous.Item2;
            bm2 = previous.Item1.Clone() as Bitmap;
            pictureBox2.Image = bm2;

            this.Size = new Size(pictureBox2.Location.X + pictureBox2.Width + 29, pictureBox2.Location.Y + pictureBox2.Height + 43);
            pictureBox1.Location = new Point(pictureBox1.Location.X, (Size.Height - pictureBox1.Height) / 2 - 8);
            this.CenterToScreen();

            cache.RemoveFirst();

            if (cache.Count == 0)
                cancelToolStripMenuItem.Enabled = false;
        }

        private void cancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cancelFunc();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            form2.Show();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !isRotating)
            {
                pictureBox1.Refresh();
                startPoint = e.Location;
                isMouseDown = true;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            int x = e.X, y = e.Y;

            if (isMouseDown)
            {
                if (e.X < 0)
                    x = 0;
                else if (e.X > pictureBox1.Width - 1)
                    x = pictureBox1.Width - 1;

                if (e.Y < 0)
                    y = 0;
                else if (e.Y > pictureBox1.Height - 1)
                    y = pictureBox1.Height - 1;

                selectedArea = new Rectangle(Math.Min(startPoint.X, x), Math.Min(startPoint.Y, y), 
                                             Math.Abs(x - startPoint.X), Math.Abs(y - startPoint.Y));
                pictureBox1.Refresh();
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (isMouseDown)
                e.Graphics.DrawRectangle(pen, selectedArea);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;

            isSelected = selectedArea.Width > 0 && selectedArea.Height > 0;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && rotateToolStripMenuItem.Enabled)
                rotateFunc();
            else if (e.KeyCode == Keys.I)
                form2.Show();
            else if (e.KeyCode == Keys.O)
                openFunc();
            else if (e.KeyCode == Keys.C && clearToolStripMenuItem.Enabled)
                clearFunc();
            else if (e.KeyCode == Keys.R && resetToolStripMenuItem.Enabled)
                resetFunc();
            else if (e.KeyCode == Keys.Back && cancelToolStripMenuItem.Enabled)
                cancelFunc();
        }
    }
}
