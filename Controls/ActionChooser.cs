using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CheckOutBot.Core;
using static CheckOutBot.Core.MonitoringTask;

namespace CheckOutBot.Controls
{
    public partial class ActionChooser : Form
    {
        public ActionChooser()
        {
            InitializeComponent();

            string[] names = Enum.GetNames(typeof(FinalAction));

            CLbx_Actions.Items.AddRange(names);
        }


        public IEnumerable<FinalAction> GetFinalActions()
        {
            List<FinalAction> result = new List<FinalAction>();

            foreach (string checkedItem in CLbx_Actions.CheckedItems)
            {
                result.Add((FinalAction) Enum.Parse(typeof(FinalAction), checkedItem));
            }

            return result;
        }

        private void Btn_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
