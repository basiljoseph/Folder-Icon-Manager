using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


namespace Folder_Icon_Manager
{
    public partial class Form1 : Form
    {
        private string iconFile = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void reset()
        {
            button_change_icon.Enabled = false;
            toolStripStatusLabel1.Text = "Select Folder Icon";
            statusStrip1.Refresh();
        }

        private void show_icon()
        {
            Image image = Image.FromFile(iconFile);
            pictureBox_icon.Image = image;
            label_image_size.Text = ": " + image.Width.ToString() + " x " + image.Height.ToString();

            Icon icon = new Icon(iconFile);
            label_icon_size.Text = ": " + icon.Size.Width.ToString() + " x " + icon.Size.Height.ToString();

            FileInfo file = new FileInfo(iconFile);
            long length = file.Length;
            if (length < 1024)
                label_file_size.Text = ": " + length + " bytes";
            else if (length < 1024 * 1024)
                label_file_size.Text = ": " + (length / 1024.0).ToString("0.00") + " KB";
            else
                label_file_size.Text = ": " + (length / (1024.0 * 1024.0)).ToString("0.00") + " MB";
        }

        private void button_open_folder_Click(object sender, EventArgs e)
        {
            try
            {
                /* Reset everything */
                reset();

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    iconFile = ofd.FileName;
                    show_icon();
                    button_change_icon.Enabled = true;
                    toolStripStatusLabel1.Text = "Loaded " + Path.GetFileNameWithoutExtension(iconFile); ;
                }
            }
            catch (Exception exp)
            {
                toolStripStatusLabel1.Text = "Occured" + exp.GetType().ToString();
                MessageBox.Show(exp.Message);
            }
            finally
            {
                statusStrip1.Refresh();
            }
        }

        private void set_file_permissions(string path, FileAttributes attributes)
        {
            if (File.Exists(path))
            {
                File.SetAttributes(path, attributes);
            }
        }

        private void set_directory_permissions(string path, FileAttributes attributes)
        {
            /* First chech whether we have write permission in the folder */
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            dirInfo.Attributes = attributes;
        }

        private void create_desktop_ini(string path, string iconName)
        {
            string desktopIniPath = path + @"\desktop.ini";
            set_file_permissions(desktopIniPath, FileAttributes.Normal);

            string iniContent = "[.ShellClassInfo]\r\nIconResource=";
            iniContent += iconName;
            iniContent += ",0\r\n";
            File.WriteAllText(desktopIniPath, iniContent);

            set_file_permissions(desktopIniPath, FileAttributes.System | FileAttributes.Hidden);
        }

        private void button_change_icon_Click(object sender, EventArgs e)
        {
            try
            {
                string path = Path.GetDirectoryName(iconFile);
                string iconName = Path.GetFileName(iconFile);

                set_directory_permissions(path, FileAttributes.Normal);
                create_desktop_ini(path, iconName);
                set_directory_permissions(path, FileAttributes.ReadOnly);
                toolStripStatusLabel1.Text = "Icon Set for " + path;
            }
            catch (Exception exp)
            {
                toolStripStatusLabel1.Text = "Occured" + exp.GetType().ToString();
                MessageBox.Show(exp.Message);
            }
            finally
            {
                statusStrip1.Refresh();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form aboutForm = new AboutBox1();
            aboutForm.Show(this);
        }
    }
}
