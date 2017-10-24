using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TMFGmail.AppExtension;
using TMFGmail.GmailExtension;
using Message = Google.Apis.Gmail.v1.Data.Message;

namespace TMFGmail
{
    public partial class FormMain : Form
    {
#region Fields
        static string[] Scopes = { GmailService.Scope.GmailModify };
        static string ApplicationName = "TMF GMail Client";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        GmailService _service = new GmailService();
        private int _files;
        private int _gateways;
        private static readonly string StartupKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private static readonly string StartupValue = "TMFGmail";
        private List<Message> _messages;
#endregion
        public FormMain()
        {
            InitializeComponent();

            try
            {
                LoadCredentialService();

                LoadDirectory();

                SetStartUp();

                log.Info("Initialize without errors");
            }
            catch (Exception e)
            {
                log.Error(e);
                throw;
            }

        }
        /// <summary>
        /// External method for checking internet access:
        /// </summary>
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);
        /// <summary>
        /// C# callable method to check internet access
        /// </summary>
        private static bool IsConnectedToInternet()
        {
            return InternetGetConnectedState(out int Description, 0);
        }
        private void LoadCredentialService()
        {
            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/gmail-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None).Result;
            }

            _service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        private void SetStartUp()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(StartupKey, true);
            key.SetValue(StartupValue, Application.ExecutablePath);
        }

        private void LoadDirectory()
        {
            var root = textBoxDirectory.Text;
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
                log.Info("Folder created for TMFRoot");
            }
        }

        private void FormMain_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon.Visible = true;
                notifyIcon.BalloonTipText = "Notify " + comboBoxUpdate.Text;
                notifyIcon.ShowBalloonTip(500);
                this.Hide();
                log.Info("Form Minimize");
            }

            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon.Visible = false;
                log.Info("Form Visible");
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(comboBoxUpdate.Text) &&
                    !string.IsNullOrEmpty(textBoxDirectory.Text) &&
                    IsConnectedToInternet())
                {
                    if (comboBoxUpdate.Text.Trim() == "Custom")
                    {
                        FormDate fd = new FormDate();
                        fd.ShowDialog();
                    }

                    var updatesQuery = comboBoxUpdate.Text.Query();
                    var rootPath = textBoxDirectory.Text;

                    Task.Run(() => GetAttachmentsUpdate(updatesQuery, rootPath));
                    
                    buttonUpdate.BackColor = Color.Red;
                }
                else
                {
                    log.Info("Set Schedule by:, Directory Path: and Internet");
                    MessageBox.Show("Set Schedule by:, Directory Path: and Internet");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                log.Debug(exception);
                throw;
            }
        }

        private async void GetAttachmentsUpdate(string updateQuery, string roothPath)
        {
            LockControl();
            try
            {
                Console.SetOut(new ConsoleWriter(richTextBoxDebug));
                Console.WriteLine("Starting to download...");

                _messages = _service.GetListMessages("me", updateQuery);
                var filenames = new List<string>();
                if (_messages != null && _messages.Count > 0)
                {
                    CountStatus();
                    foreach (var message in _messages)
                    {
                        var msg = _service.GetMessage("me", message.Id);

                        if (msg.LabelIds.Contains("UNREAD"))
                        {
                            foreach (var head in msg.Payload.Headers)
                            {
                                if (head.Name == "Subject")
                                {
                                    filenames.Add(head.Value.GetSubject());
                                    var root = roothPath;
                                    if (!Directory.Exists(root + head.Value.GetSubject()))
                                    {
                                        Directory.CreateDirectory(root + head.Value.GetSubject());
                                        _gateways++;
                                    }

                                    try
                                    {
                                        Task<bool> T_attachmentResult = Task.Factory.StartNew(() => _service.GetAttachments("me", message.Id, root + head.Value.GetSubject()));

                                        T_attachmentResult.ContinueWith((antecedent) =>
                                        {
                                            //_service.GetAttachments("me", message.Id, root + head.Value.GetSubject());
                                            _files++;
                                            progressBarStatus.Invoke((Action)delegate { progressBarStatus.Increment(1); });

                                            _service.GetModifyMessage("me", message.Id, new List<string>() { "UNREAD" });

                                            //Console.WriteLine("Message mark as read.");
                                        });

                                        //Task.WaitAny(T_attachmentResult);
                                        //if (T_attachmentResult.Result)
                                        //{
                                        //    _files++;
                                        //    progressBarStatus.Invoke((Action)delegate { progressBarStatus.Increment(1); });
                                        //}

                                        //_service.GetAttachments("me", message.Id, root + head.Value.GetSubject());
                                        //_files++;
                                        //progressBarStatus.Invoke((Action)delegate { progressBarStatus.Increment(1); });

                                        //_service.GetModifyMessage("me", message.Id, new List<string>() { "UNREAD" });
                                        
                                        //Console.WriteLine("Message mark as read.");
                                    }
                                    catch (Exception e)
                                    {
                                        log.Debug(e);
                                        Console.WriteLine(e);
                                        throw;
                                    }
                                    var oks = "File Download Successfully " + message.Id;
                                    //Console.WriteLine(oks);
                                    log.Info(oks);
                                }
                            }
                        }
                        //Console.WriteLine(message.LabelIds);
                        Debug.WriteLine(message.Id.Length);
                    }
                }
                else
                {
                    Console.WriteLine("No message found.");
                    log.Info("No message found.");
                }

                labelFile.Invoke((Action)delegate { labelFile.Text = "No. of downloaded file: " + _files; });
                labelGateway.Invoke((Action)delegate { labelGateway.Text = "No. of gateway: " + filenames.Distinct().Count(); });

                if (_files == 0 && filenames.Distinct().Count() == 0)
                {
                    labelStatus.Invoke((Action)delegate { labelStatus.Text = "Status: Files are up to date"; labelStatus.ForeColor = Color.Green; });
                    notifyIcon.BalloonTipText = "Files are up to date";
                }
                else
                {
                    labelStatus.Invoke((Action)delegate { labelStatus.Text = "Status: Completed"; labelStatus.ForeColor = Color.Green; });
                    notifyIcon.BalloonTipText = "New updates is now downloaded";
                    _files = 0;
                }

                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(500);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                labelStatus.Invoke((Action)delegate { labelStatus.Text = "Status: Error"; labelStatus.ForeColor = Color.Red; });
                log.Debug(e);
                throw; //check the bug here
            }
            UnlockControl();
        }

        private void UnlockControl()
        {
            comboBoxUpdate.Invoke((Action)delegate { comboBoxUpdate.Enabled = true;  });
            buttonUpdate.Invoke((Action)delegate { buttonUpdate.Enabled = true; buttonUpdate.BackColor = Color.Green; });
            textBoxDirectory.Invoke((Action)delegate { textBoxDirectory.Enabled = true; });
            buttonSet.Invoke((Action)delegate { buttonSet.Enabled = true; });
            buttonViewFolder.Invoke((Action)delegate { buttonViewFolder.Enabled = true; });
        }

        private void LockControl()
        {
            comboBoxUpdate.Invoke((Action)delegate { comboBoxUpdate.Enabled = false; });
            buttonUpdate.Invoke((Action)delegate { buttonUpdate.Enabled = false; });
            textBoxDirectory.Invoke((Action)delegate { textBoxDirectory.Enabled = false; });
            buttonSet.Invoke((Action)delegate { buttonSet.Enabled = false; });
            buttonViewFolder.Invoke((Action)delegate { buttonViewFolder.Enabled = false; });
            labelStatus.Invoke((Action)delegate { labelStatus.Text = ""; labelStatus.ForeColor = Color.Black; });
            labelFile.Invoke((Action)delegate { labelFile.Text = "No. of downloaded file: "; });
            labelGateway.Invoke((Action)delegate { labelGateway.Text = "No. of gateway: "; });
            progressBarStatus.Invoke((Action)delegate { progressBarStatus.Maximum = 0; });
        }

        private void CountStatus()
        {
            int statuCount = 0;
            foreach (var message in _messages)
            {
                var msg = _service.GetMessage("me", message.Id);

                if (msg.LabelIds.Contains("UNREAD"))
                {
                    statuCount++;
                }
            }
            progressBarStatus.Invoke((Action)delegate { progressBarStatus.Maximum = statuCount; });
        }

        private void buttonViewFolder_Click(object sender, EventArgs e)
        {
            Process.Start(textBoxDirectory.Text);
        }

        private void buttonSet_Click(object sender, EventArgs e)
        {
            if (textBoxDirectory.ReadOnly)
            {
                textBoxDirectory.ReadOnly = false;
                buttonSet.BackColor = Color.Green;
            }
            else
            {
                textBoxDirectory.ReadOnly = true;
                buttonSet.BackColor = Color.Red;
            }
        }

        private void comboBoxUpdate_MouseClick(object sender, MouseEventArgs e)
        {
            buttonUpdate.BackColor = Color.Green;
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            buttonUpdate.PerformClick();
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            notifyIcon.Visible = true;
            notifyIcon.BalloonTipText = "Notify " + comboBoxUpdate.Text;
            notifyIcon.ShowBalloonTip(500);
            this.Hide();
            log.Info("Form Closed");
        }
    }
}
