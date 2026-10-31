using System;
using System.Windows.Forms;

namespace StartupAudit
{
    class MainForm : Form
    {
        ListView lv;
        public MainForm()
        {
            Text = "Startup Audit";
            Width = 800;
            Height = 500;

            var menu = new MenuStrip();
            var exportBtn = new ToolStripMenuItem("导出 CSV");
            exportBtn.Click += delegate { ExportCsv(); };
            menu.Items.Add(exportBtn);
            Controls.Add(menu);

            lv = new ListView();
            lv.Dock = DockStyle.Fill;
            lv.View = View.Details;
            lv.FullRowSelect = true;
            lv.Columns.Add("名称", 200);
            lv.Columns.Add("路径", 350);
            lv.Columns.Add("来源", 100);
            lv.Columns.Add("可禁用", 80);
            Controls.Add(lv);

            LoadItems();
        }

        void LoadItems()
        {
            var s = new Scanner();
            foreach (var item in s.Scan())
            {
                var row = lv.Items.Add(item.Name);
                row.SubItems.Add(item.Path);
                row.SubItems.Add(item.Source);
                row.SubItems.Add(item.SafeToDisable ? "是" : "否");
            }
        }

        void ExportCsv()
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "CSV|*.csv";
            if (dlg.ShowDialog() != DialogResult.OK) return;
            using (var w = new System.IO.StreamWriter(dlg.FileName))
            {
                w.WriteLine("名称,路径,来源,可禁用");
                foreach (ListViewItem row in lv.Items)
                {
                    w.WriteLine(string.Format("{0},{1},{2},{3}",
                        row.SubItems[0].Text,
                        row.SubItems[1].Text,
                        row.SubItems[2].Text,
                        row.SubItems[3].Text));
                }
            }
        }
    }
}
