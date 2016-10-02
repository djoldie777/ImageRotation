using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace image_rotation
{
    public partial class Form2 : Form
    {
        private double angle;
        private int cache;

        public Form2(double angle)
        {
            InitializeComponent();
            this.angle = angle;
            cache = 0;
            textBox1.Text = angle.ToString();
        }

        public double Angle
        {
            get { return angle; }
        }

        public int Cache
        {
            get { return cache; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double newAngle;

            if (Double.TryParse(textBox1.Text, out newAngle))
            {
                if (newAngle > 360)
                    angle = 360;
                else if (newAngle < -360)
                    angle = -360;
                else
                    angle = newAngle;

                textBox1.Text = angle.ToString();
            }

            cache = trackBar1.Value;

            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;

            if (!char.IsDigit(ch) && (ch != '-') && (ch != 8))
                e.Handled = true;
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
