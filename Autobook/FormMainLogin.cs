using System;
using System.Windows.Forms;

namespace Autobook
{
    public partial class FormMainLogin : Form
    {
        public FormMainLogin()
        {
            InitializeComponent();
        }

        /*
         ***	done in FormMain, for logging purpose
            private void buttonCancel_Click (object sender, EventArgs e)
            {
              Application.Exit ();
            }
        */

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                Globals.Exchange.Login(textBoxLogin.Text, textBoxPassword.Text);
                if (Globals.Exchange.IsLoggedIn)
                {
                    DialogResult = DialogResult.OK;
                    AppSettings.BetfairUser = textBoxLogin.Text;
                    AppSettings.BetfairPassword = textBoxPassword.Text;
                    AppSettings.BetfairNgKey = textBoxAppKey.Text;
                    AppSettings.SaveSettings();
                }
                else
                {
                    MessageBox.Show(this, "Betfair login failed", "Betfair Login",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Betfair Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void FormMainLogin_Load(object sender, EventArgs e)
        {
#if NOBET
            labelNoBet.Visible = true;
#endif
            textBoxAppKey.Text = AppSettings.BetfairNgKey;
            textBoxLogin.Text = AppSettings.BetfairUser;
            textBoxPassword.Text = AppSettings.BetfairPassword;
        }
    }
}