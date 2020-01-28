using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

using System.Text;


namespace FreeDiskSpace_Monitoring_Tool
{
    public class MainPresenter
    {
        public BindingSource _binding;

        private Timer _messageTimer, _updateTimer;
        private MainForm _mainForm;

        private DiskMonitoring _diskData;           //об'єкт, що збирає дані про вільне місце на дисках
        private DataGridView _dataGrid;

        private Form AlertWindow;                   //вікно для відображення повідомлення
        private Label AlertMessage;                 //керуючий елемент, що зберігає текст повідомлення

       

        public MainPresenter(MainForm form, DiskMonitoring monitor)
        {

            _mainForm = form;
            _diskData = monitor;

            

            _dataGrid = _mainForm.GetDataGrid();

            //_binding = new BindingSource();
            //_binding.DataSource = _diskData.disks;
            //_dataGrid.DataSource = _binding;

            _messageTimer = new Timer();
            _messageTimer.Interval = 60000;
            _messageTimer.Tick += _messageTimer_Tick;

            //створення та налаштування таймеру для оновлення даних та виведення повідомлення
            _updateTimer = new Timer();
            _updateTimer.Interval = 5000;
            _updateTimer.Tick += _updateTimer_Tick;
            _updateTimer.Start();

            //створення та налаштування вікна та текстового поля (Label) для відображення повідомлення
            AlertWindow = new Form();
            AlertWindow.Size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height / 4);
            AlertWindow.FormBorderStyle = FormBorderStyle.None;

            AlertWindow.FormClosing += AlertWindow_Closing;

            AlertMessage = new Label();
            AlertMessage.Location = new Point(0, 0);
            AlertMessage.Size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height / 4);
            AlertMessage.Font = new Font(FontFamily.GenericMonospace, 16);
            AlertMessage.TextAlign = ContentAlignment.MiddleCenter;

            AlertMessage.Click += AlertWindow_Clicked;

            AlertWindow.Controls.Add(AlertMessage);

            //підписка на подію про перевищення встановленого ліміту
            _diskData.LimitExceeded += _diskData_LimitExceeded;

            //підписки на події головного вікна
            _mainForm.ExitMenuItem_Clicked += _mainForm_ExitMenuItem_Clicked;
            _mainForm.Notify_DoubleClicked += _mainForm_Notify_DoubleClicked;
            _mainForm.MeasureUnits_Changed += _mainForm_MeasureUnits_Changed;
            _mainForm.SetCriticalsButton_Clicked += _mainForm_SetCriticalsButton_Clicked;
            _mainForm.MainForm_Closing += _mainForm_MainForm_Closing;
            _mainForm.MainForm_Resized += _mainForm_MainForm_Resized;
            _mainForm.DiskDataGrid_DoubleClicked += _mainForm_DiskDataGrid_DoubleClicked;

            _diskData.LoadData(_dataGrid);
        }

        private void _mainForm_DiskDataGrid_DoubleClicked(object sender, EventArgs e)
        {
            var diskInfoDialog = new DiskInfoForm(_dataGrid.SelectedRows[0].Cells[0].Value.ToString());
            diskInfoDialog.ShowDialog();
        }

        //обробка згортання головного вікна
        private void _mainForm_MainForm_Resized(object sender, EventArgs e)
        {
            if (_mainForm.WindowState == FormWindowState.Minimized)
            {
                _mainForm.Hide();
                _mainForm.ChangeNotificationVisibility(true);
            }
        }

        private void _mainForm_MainForm_Closing(object sender, EventArgs e)
        {
            _diskData.SaveCriticalValues();
            _updateTimer.Stop();
            _messageTimer.Stop();
            AlertWindow.Hide();
        }

        //виклик діалогового вікна
        private void _mainForm_SetCriticalsButton_Clicked(object sender, EventArgs e)
        {
            var dialog = new SetCriticalForm(_diskData.GetDataCopy());
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _diskData.LoadData(_dataGrid);
                
                _diskData.UpdateCriticalValues(dialog.GetCriticalValues());
            }
        }

        private void _mainForm_MeasureUnits_Changed(object sender, EventArgs e)
        {
            _diskData.currentUnit = (MemoryUnits)((ComboBox)sender).SelectedIndex;
            _diskData.LoadData(_dataGrid);
        }

        //подвійне натискання на значок у треї
        private void _mainForm_Notify_DoubleClicked(object sender, EventArgs e)
        {
            _mainForm.Show();
            _mainForm.ChangeNotificationVisibility(false);
            _mainForm.WindowState = FormWindowState.Normal;
        }

        //натискання пункту "Вихід" у контекстному меню у треї
        private void _mainForm_ExitMenuItem_Clicked(object sender, EventArgs e)
        {
            _diskData.SaveCriticalValues();
            _updateTimer.Stop();
            _mainForm.Close();
        }

        private void _updateTimer_Tick(object sender, EventArgs e)
        {
            _diskData.UpdateDiskStates();
            _diskData.LoadData(_dataGrid);
        }

        private void AlertWindow_Clicked(object sender, EventArgs e)
        {
            _messageTimer.Start();
            AlertWindow.Hide();
        }

        private void AlertWindow_Closing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            _messageTimer.Start();
            AlertWindow.Hide();
        }

        private void _diskData_LimitExceeded(Dictionary<string, DiskState> obj)
        {
            if (obj.Count > 0)
            {
                 _diskData.LimitExceeded -= _diskData_LimitExceeded;

                ShowAlertMessage(obj);
            }
            else
            {
                _diskData.LimitExceeded += _diskData_LimitExceeded;
                if (_messageTimer.Enabled)
                {
                    _messageTimer.Stop();
                }
            }
            
            //obj.Clear();
        }

        public void ShowAlertMessage(Dictionary<string, DiskState> obj)
        {
            StringBuilder message = new StringBuilder();
            message.Append("Увага!\n");
            foreach (var item in obj)
            {
                message.Append(string.Format("Об'єм вільного простору на диску {0} менше заданого ліміту на {1:0.00} ГБ\n",
                                            item.Key, (item.Value.CriticalValue - item.Value.FreeSpace) / Math.Pow(1024, 3)));
            }
            message.Append("Натисніть ЛКМ на даному вікні щоб закрити це повідомлення");
            AlertMessage.Text = message.ToString();
            AlertWindow.Show();
            AlertWindow.BringToFront();
            message.Clear();
        }

        private void _messageTimer_Tick(object sender, EventArgs e)
        {
            _diskData.LimitExceeded += _diskData_LimitExceeded;
        }
    }
}
