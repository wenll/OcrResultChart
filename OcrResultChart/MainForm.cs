﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using MySql.Data.MySqlClient;
using System.Linq;

namespace OcrResultChart
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var tbBatch = GetTable("SELECT DISTINCT(BatchID) FROM billdetailbasicinfo");
            cbBatch.DataSource = tbBatch;
            cbBatch.ValueMember = "BatchID";
            cbBatch.DisplayMember = "BatchID";
            comboBox1.SelectedIndex = 1;
        }

        private void btnShow_Click(object sender, EventArgs e)
        {
            int rate = int.Parse(comboBox1.Text.ToString());
            var list = GetList(cbBatch.Text);
            var ratioList = new List<int>();
            double result = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != 0 && i != 0)
                {
                    ratioList.Add((int)((list[i] / (double)i) * rate));
                }
            }

            result = ratioList.Average() / rate;
            chart1.Series.Clear();

            Series series = new Series("Point");
            series.ChartType = SeriesChartType.Point;
            for (int i = 0; i < list.Count; i++)
            {
                series.Points.AddXY(i, list[i]);
            }
            series.IsValueShownAsLabel = true;
            chart1.Series.Add(series);

            //Series line = new Series("Average");
            //line.ChartType = SeriesChartType.Line;
            //line.Color = Color.Red;
            //line.Points.AddXY(0.0, 0.0);
            //line.Points.AddXY(list.Count, list.Count * result);
            //chart1.Series.Add(line);

            //Series line_std = new Series("1:1");
            //line_std.ChartType = SeriesChartType.Line;
            //line_std.Color = Color.Green;
            //line_std.Points.AddXY(0, 0);
            //line_std.Points.AddXY(list.Count, list.Count);
            //chart1.Series.Add(line_std);

            chart2.Series.Clear();
            Series line_count = new Series("Count");
            line_count.ChartType = SeriesChartType.Column;
            line_count.IsValueShownAsLabel = true;
            var ratioGroup = ratioList.GroupBy(i => i);
            var cnt = ratioGroup.Max(g => g.Count());
            var maxValue = ratioGroup.ToList().Find(g => g.Count() == cnt).Key;
            foreach (var s in ratioGroup)
            {
                line_count.Points.AddXY(s.Key, s.Count());
            }
            chart2.Series.Add(line_count);

            Series line_modify = new Series("Perfact_line");
            line_modify.ChartType = SeriesChartType.Line;
            line_modify.Color = Color.Blue;
            line_modify.Points.AddXY(0, 0);
            line_modify.Points.AddXY(list.Count, list.Count * maxValue / rate);
            chart1.Series.Add(line_modify);

            Series line_up = new Series("Up_line");
            line_up.ChartType = SeriesChartType.Line;
            line_up.Color = Color.Orange;
            line_up.Points.AddXY(0, 5);
            line_up.Points.AddXY(list.Count, list.Count * maxValue / rate + 5);
            chart1.Series.Add(line_up);

            Series line_down = new Series("Down_line");
            line_down.ChartType = SeriesChartType.Line;
            line_down.Color = Color.Orange;
            line_down.Points.AddXY(0, -5);
            line_down.Points.AddXY(list.Count, list.Count * maxValue / rate - 5);
            chart1.Series.Add(line_down);

            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = list.Count;
            chart1.ChartAreas[0].AxisX.Interval = 5;
            chart1.ChartAreas[0].AxisY.Interval = 5;

        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (cbBatch.SelectedIndex < cbBatch.Items.Count - 1)
            {
                cbBatch.SelectedIndex += 1;
                btnShow_Click(null, null);
            }
        }

        private List<int> GetList(string batchid)
        {
            List<int> list = new List<int>();
            var dtBill = GetTable("select ID,SeriID,isattachment from billdetailbasicinfo where isback = 0 and batchid = " + batchid);
            var dtSeri = GetTable("select ID from serialnumberlistdetail where isattach = 0 and batchid = " + batchid);
            var seriList = new List<int>();
            foreach (DataRow row in dtSeri.Rows)
            {
                seriList.Add(int.Parse(row["ID"].ToString()));
            }
            for (int i = 0; i < dtBill.Rows.Count; i++)
            {
                DataRow dr = dtBill.Rows[i];
                if (dr["isattachment"].ToString().Equals("1"))
                {
                    if (cbAttach.Checked)
                    {
                        list.Add(0);
                    }
                }
                else
                {
                    string seriID = dr["SeriID"].ToString();
                    if (string.IsNullOrEmpty(seriID) || seriID.Equals("-1"))
                    {
                        if (cbUnocr.Checked)
                        {
                            list.Add(0);
                        }
                    }
                    else
                    {
                        list.Add(seriList.FindIndex(s => s == int.Parse(seriID)));
                    }
                }
            }
            return list;
        }

        private DataTable GetTable(string sql)
        {
            using (MySqlConnection connection = new MySqlConnection("server=localhost;user=root;database=lzbankdatabase;port=3306;password=root;"))
            {
                DataSet ds = new DataSet();
                connection.Open();
                try
                {
                    MySqlDataAdapter da = new MySqlDataAdapter();
                    MySqlCommand cmd = new MySqlCommand(sql, connection);
                    cmd.CommandTimeout = 600;
                    da.SelectCommand = cmd;
                    da.Fill(ds, "ds");
                }
                catch
                { }
                connection.Clone();
                return ds.Tables[0];
            }
        }

    }
}
