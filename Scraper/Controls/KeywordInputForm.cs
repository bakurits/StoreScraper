using System.Windows.Forms;

namespace Scraper.Controls
{
    public partial class KeywordInputForm : Form
    {
        public string ResultText { get; private set; }

        public KeywordInputForm()
        {
            InitializeComponent();
        }

        private void KeywordInputForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ResultText = Tbx_Input.Text;
        }
    }
}
