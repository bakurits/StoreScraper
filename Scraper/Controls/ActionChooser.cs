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

            string[] names = Enum.GetNames(typeof(SearchMonitoringTask.FinalAction));

            CLbx_Actions.Items.AddRange(names);
        }


        public IEnumerable<SearchMonitoringTask.FinalAction> GetFinalActions()
        {
            List<SearchMonitoringTask.FinalAction> result = new List<SearchMonitoringTask.FinalAction>();

            foreach (string checkedItem in CLbx_Actions.CheckedItems)
            {
                result.Add((SearchMonitoringTask.FinalAction) Enum.Parse(typeof(SearchMonitoringTask.FinalAction), checkedItem));
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
