using System;
using System.Globalization;
using System.Windows.Forms;

namespace TMFGmail
{
    public partial class FormDate : Form
    {
        public FormDate()
        {
            InitializeComponent();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            SearchDate.DateFrom = dateTimePickerFrom.Value.ToString("yyyy/M/d", CultureInfo.InvariantCulture);
            SearchDate.DateTo = dateTimePickerTo.Value.ToString("yyyy/M/d", CultureInfo.InvariantCulture);
            if (SearchDate.DateFrom == DateTime.Now.ToString("yyyy/M/d", CultureInfo.InvariantCulture))
            {
                SearchDate.DateFrom = dateTimePickerFrom.Value.AddDays(-1).ToString("yyyy/M/d", CultureInfo.InvariantCulture);
            }
            if (SearchDate.DateFrom == SearchDate.DateTo)
            {
                SearchDate.DateTo = dateTimePickerTo.Value.AddDays(-1).ToString("yyyy/M/d", CultureInfo.InvariantCulture);
            }
            //MessageBox.Show(SearchDate.DateTo);
            this.Close();
        }
    }

    public static class SearchDate
    {
        public static string DateFrom;
        public static string DateTo;
    }
}
