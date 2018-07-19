using System;
using System.Collections.Generic;
using System.Windows.Forms;
using StoreScraper.Core;

namespace StoreScraper.Controls
{
    public partial class ActionChooser : Form
    {
        public ActionChooser()
        {
            InitializeComponent();

            string[] names = Enum.GetNames(typeof(MonitoringTask.FinalAction));

            CLbx_Actions.Items.AddRange(names);
        }


        public IEnumerable<MonitoringTask.FinalAction> GetFinalActions()
        {
            List<MonitoringTask.FinalAction> result = new List<MonitoringTask.FinalAction>();

            foreach (string checkedItem in CLbx_Actions.CheckedItems)
            {
                result.Add((MonitoringTask.FinalAction) Enum.Parse(typeof(MonitoringTask.FinalAction), checkedItem));
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
