using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FreeDiskSpace_Monitoring_Tool
{
    public partial class SetCriticalForm : Form
    {
        private Dictionary<string, DiskState> disks;     //локальна копія даних про диски
        private string currentDiskName;                 //назва поточного обраного диску
        private DiskState currentDiskState;             //стан поточного обраного диску
        public SetCriticalForm(Dictionary<string, DiskState> disks)
        {
            InitializeComponent();
            this.disks = disks;
            //this.disks = new Dictionary<string, DiskState>();
            //foreach (var item in disks)
            //{
            //    this.disks.Add(item.Key, new DiskState(item.Value));
            //}

            cbDiskName.Items.AddRange(disks.Keys.ToArray());
            cbUnits.Items.AddRange(new string[] { "MB", "GB" });
            cbUnits.SelectedIndex = 0;

            numValue.Minimum = 0;
            numPercent.Minimum = 0;
            numPercent.Maximum = 100;
            numPercent.Increment = 1;

            numValue.ValueChanged += NumValue_ValueChanged;
            numPercent.ValueChanged += NumPercent_ValueChanged;

            cbUnits.SelectedIndexChanged += CbUnits_SelectedIndexChanged;
            cbDiskName.SelectedIndexChanged += CbDiskName_SelectedIndexChanged;
            cbDiskName.SelectedIndex = 0;
        }

        //зміна критичного значення у відсотках
        private void NumPercent_ValueChanged(object sender, EventArgs e)
        {
            decimal criticalPercent = decimal.Round(numPercent.Value);

            currentDiskState.CriticalValue = currentDiskState.TotalSpace * (long)criticalPercent / 100;
            UpdateCritical();
        }

        //зміна критичного значення числом
        private void NumValue_ValueChanged(object sender, EventArgs e)
        {
            long newCriticalValue = (long)numValue.Value;
            currentDiskState.CriticalValue = (cbUnits.SelectedIndex == 0) ?
                                    (long)(newCriticalValue * Math.Pow(1024, 2)) :
                                    (long)(newCriticalValue * Math.Pow(1024, 3));
            UpdateCritical();
        }

        //зміна одиниць виміру
        private void CbUnits_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCritical();
        }

        //вибір поточного диску
        private void CbDiskName_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDiskName = cbDiskName.Items[cbDiskName.SelectedIndex].ToString();
            currentDiskState = disks[currentDiskName];
            UpdateCritical();
        }

        //оновлення даних форми
        private void UpdateCritical()
        {
            long currentCritical = (cbUnits.SelectedIndex == 0) ?
                                    (long)Math.Floor(currentDiskState.CriticalValue / Math.Pow(1024, 2)) :
                                    (long)Math.Floor(currentDiskState.CriticalValue / Math.Pow(1024, 3));
            long currentTotal = (cbUnits.SelectedIndex == 0) ?
                                    (long)Math.Floor(currentDiskState.TotalSpace / Math.Pow(1024, 2)) :
                                    (long)Math.Floor(currentDiskState.TotalSpace / Math.Pow(1024, 3));

            numValue.ValueChanged -= NumValue_ValueChanged;
            numPercent.ValueChanged -= NumPercent_ValueChanged;

            numValue.Maximum = currentTotal;

            numValue.Value = currentCritical;
            numPercent.Value = (decimal)currentDiskState.CriticalValue * 100 / currentDiskState.TotalSpace;

            numValue.ValueChanged += NumValue_ValueChanged;
            numPercent.ValueChanged += NumPercent_ValueChanged;
        }

        //
        public Dictionary<string, long> GetCriticalValues()
        {
            return new Dictionary<string, long>(disks.ToDictionary(a => a.Key, b => b.Value.CriticalValue));
        }
    }
}