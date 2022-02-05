using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LolAutoLogin
{
    public partial class MainForm : Form
    {
        static String Delimiter = ":|.-,";
        static String PathPrefix = "::PATH::";

        String DataPath = Directory.GetCurrentDirectory() + "\\data";
        Point newLoc = new Point(15, 125);
        Dictionary<String, Tuple<String, String>> UsersDb = new Dictionary<String, Tuple<String, String>>();
        List<Button> AccountButtons = new List<Button>();
        List<ToolStripItem> MenuItems = new List<ToolStripItem>();

        public MainForm()
        {
            InitializeComponent();
            loadFromDb();
        }

        void loadFromDb()
        {
            String lolPath = "";
            if (File.Exists(DataPath))
            {
                Boolean isFirstLine = true;
                foreach (String userInfoLine in File.ReadAllLines(DataPath))
                {
                    if (isFirstLine)
                    {
                        String decryptedPath = DataEncryption.Decrypt(userInfoLine);
                        lolPath = decryptedPath.Substring(PathPrefix.Length);
                        isFirstLine = false;
                    }
                    else if(userInfoLine != "")
                    {
                        String decrypted = DataEncryption.Decrypt(userInfoLine);
                        String[] userInfos = decrypted.Split(new string[] { Delimiter }, StringSplitOptions.None);

                        String nickname = userInfos[0];
                        String username = userInfos[1];
                        String password = userInfos[2];
                        if (!UsersDb.ContainsKey(nickname))
                        {
                            Tuple<String, String> tuple = new Tuple<String, String>(username, password);
                            UsersDb.Add(nickname, tuple);
                            addAccountToUi(nickname);
                        }
                    }
                }
            }
            checkAndSetLolPath(lolPath);
        }

        private void addAccountButton_Click(object sender, EventArgs e)
        {
            if(!UsersDb.ContainsKey(nicknameInput.Text) &&
                nicknameInput.Text != "" &&
                userNameInput.Text != "" &&
                passwordInput.Text != ""
                )
            {
                addAccountToDb(nicknameInput.Text, userNameInput.Text, passwordInput.Text);
                addAccountToUi(nicknameInput.Text);
            }

            nicknameInput.Text = "";
            userNameInput.Text = "";
            passwordInput.Text = "";
        }

        void addAccountToDb(String nickname, String username, String password)
        {
            String encrypted = DataEncryption.Encrypt(nickname + Delimiter + username + Delimiter + password);
            StreamWriter stream = File.AppendText(DataPath);
            stream.WriteLine(encrypted);
            stream.Close();
            Tuple<String, String> tuple = new Tuple<String, String>(username, password);
            UsersDb.Add(nickname, tuple);
        }

        void addAccountToUi(String nickname)
        {
            Button newButton = new Button();

            newButton.Text = nickname;
            newButton.Size = new Size(311, 25);
            newButton.Location = newLoc;
            newButton.Click += new EventHandler(loginAccount_Click);
            newLoc.Offset(0, newButton.Height + 5);
            this.Controls.Add(newButton);
            AccountButtons.Add(newButton);

            ToolStripItem item = new ToolStripMenuItem();
            item.Text = nickname;
            item.Click += new EventHandler(loginAccountTray_Click);
            contextMenu.Items.Insert(contextMenu.Items.Count - 1, item);
            MenuItems.Add(item);
        }

        private void loginAccount_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            String nickname = clickedButton.Text;
            String username = UsersDb[nickname].Item1;
            String password = UsersDb[nickname].Item2;
            LoLLogin.Login(username, password);
        }

        private void loginAccountTray_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem clickedMenuItem = sender as ToolStripMenuItem;
            String nickname = clickedMenuItem.Text;
            String username = UsersDb[nickname].Item1;
            String password = UsersDb[nickname].Item2;
            LoLLogin.Login(username, password);
        }

        private void passwordInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                addAccountButton_Click(this, new EventArgs());
                nicknameInput.Select();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void LolAutoLoginTray_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            LolAutoLoginTray.Visible = false;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                LolAutoLoginTray.Visible = true;
            }
        }

        private void lolPathInput_TextChanged(object sender, EventArgs e)
        {
            if(lolPathInput.Text != LoLLogin.getLolPath())
            {
                if (checkAndSetLolPath(lolPathInput.Text))
                    writePathToDb(lolPathInput.Text);
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            bool shouldKeepSearching = true;
            do
            {
                DialogResult res = searchLolPath.ShowDialog();
                if (res == DialogResult.OK)
                {
                    String folderPath = Path.GetDirectoryName(searchLolPath.FileName);
                    shouldKeepSearching = !checkAndSetLolPath(folderPath);
                    if (!shouldKeepSearching)
                        writePathToDb(folderPath);
                }
                else
                {
                    shouldKeepSearching = false;
                }
            } while (shouldKeepSearching);
        }

        bool checkAndSetLolPath(String lolPath)
        {
            bool foundExe = false;
            if (Directory.Exists(lolPath) && File.Exists(lolPath + "\\" + LoLLogin.getLolExeName()))
            {
                LoLLogin.setLolPath(lolPath);
                lolPathInput.Text = lolPath;
                foundExe = true;
                setUserInputsEnabled(true);
                setPathInputsEnabled(false);
            }
            else
            {
                setUserInputsEnabled(false);
                setPathInputsEnabled(true);
            }

            return foundExe;
        }

        void writePathToDb(String lolPath)
        {
            deletePathFromDb();
            String oldData = "";
            if (File.Exists(DataPath))
            {
                oldData = File.ReadAllText(DataPath);
            }
            File.WriteAllLines(DataPath, new string[] { DataEncryption.Encrypt(PathPrefix + lolPath), oldData });
        }

        void deletePathFromDb()
        {
            if (File.Exists(DataPath))
            {
                String[] dataLines = File.ReadAllLines(DataPath);
                File.Delete(DataPath);
                foreach (String userInfoLine in dataLines)
                {
                    if(userInfoLine != "")
                    {
                        String decrypted = DataEncryption.Decrypt(userInfoLine);
                        if(!decrypted.StartsWith(PathPrefix))
                        {
                            StreamWriter stream = File.AppendText(DataPath);
                            stream.WriteLine(userInfoLine);
                            stream.Close();
                        }
                    }
                }
            }
        }

        void setUserInputsEnabled(Boolean isEnabled)
        {
            addAccountButton.Enabled = isEnabled;
            nicknameInput.Enabled = isEnabled;
            userNameInput.Enabled = isEnabled;
            passwordInput.Enabled = isEnabled;

            foreach (Button accountButton in AccountButtons)
                accountButton.Enabled = isEnabled;

            foreach (ToolStripItem menuItem in MenuItems)
                menuItem.Enabled = isEnabled;
        }

        void setPathInputsEnabled(Boolean isEnabled)
        {
            lolPathInput.Enabled = isEnabled;
            browseButton.Enabled = isEnabled;
        }

    }
}
