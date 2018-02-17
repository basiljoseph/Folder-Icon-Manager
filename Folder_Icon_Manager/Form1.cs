using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace Folder_Icon_Manager
{
    public partial class Form1 : Form
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern Int32 SHParseDisplayName(
            [MarshalAs(UnmanagedType.LPWStr)] String pszName,
            IntPtr pbc,
            out IntPtr ppidl,
            UInt32 sfgaoIn,
            out UInt32 psfgaoOut);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        internal static extern void SHChangeNotify(
            UInt32 wEventId,
            UInt32 uFlags,
            IntPtr dwItem1,
            IntPtr dwItem2);

        private string iconFile = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void ResetComponents()
        {
            button_change_icon.Enabled = false;
            toolStripStatusLabel1.Text = "Select Folder Icon";
            statusStrip1.Refresh();
        }

        private void ShowPictureIcon()
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
                ResetComponents();

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    iconFile = ofd.FileName;
                    ShowPictureIcon();
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

        private void SetFilePermissions(string path, FileAttributes attributes)
        {
            if (File.Exists(path))
            {
                File.SetAttributes(path, attributes);
            }
        }

        private void SetDirectoryPermissions(string path, FileAttributes attributes)
        {
            /* First chech whether we have write permission in the folder */
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            dirInfo.Attributes = attributes;
        }

        private void create_desktop_ini(string path, string iconName)
        {
            string desktopIniPath = path + @"\desktop.ini";
            SetFilePermissions(desktopIniPath, FileAttributes.Normal);

            string iniContent = "[.ShellClassInfo]\r\nIconResource=";
            iniContent += iconName;
            iniContent += ",0\r\n";
            File.WriteAllText(desktopIniPath, iniContent);

            SetFilePermissions(desktopIniPath, FileAttributes.System | FileAttributes.Hidden);
        }

        private void button_change_icon_Click(object sender, EventArgs e)
        {
            try
            {
                string path = Path.GetDirectoryName(iconFile);
                string iconName = Path.GetFileName(iconFile);

                SetDirectoryPermissions(path, FileAttributes.Normal);
                create_desktop_ini(path, iconName);
                SetDirectoryPermissions(path, FileAttributes.ReadOnly);
                toolStripStatusLabel1.Text = "Icon Set for " + path;
                RefreshThumbnail(path);
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

        private void RefreshThumbnail(string path)
        {
            uint iAttribute;
            IntPtr pidl;
            SHParseDisplayName(path, IntPtr.Zero, out pidl, 0, out iAttribute);
            SHChangeNotify(
                (uint)0x00002000,
                (uint)0x1000,
                pidl,
                IntPtr.Zero);
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
