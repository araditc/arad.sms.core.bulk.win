using System;
using System.Collections.Generic;
using System.Linq;

namespace Arad.Sms.Core.Bulk.Win;

public partial class Search : System.Windows.Forms.Form
{
    private readonly List<BulkMessage> _bulkMessages;

    public Search(List<BulkMessage> bulkMessages)
    {
        _bulkMessages = bulkMessages;
        InitializeComponent();

        dataGridView1.AutoGenerateColumns = false;
        dataGridView1.DataSource = _bulkMessages;
        txtCount.Text = _bulkMessages.Count.ToString();
    }
        
    private void btnExit_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void btnSearch_Click(object sender, EventArgs e)
    {
        List<BulkMessage> search = _bulkMessages;
        if (!string.IsNullOrWhiteSpace(txtSearch.Text))
        {
            search = _bulkMessages.Where(s => s.DestinationAddress.Contains(txtSearch.Text)).ToList();
        }
        dataGridView1.DataSource = search;
        txtCount.Text = search.Count.ToString();
    }
}