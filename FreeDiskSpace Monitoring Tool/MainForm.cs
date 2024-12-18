using System;
using System.Drawing;
using System.Windows.Forms;

namespace FreeDiskSpace_Monitoring_Tool
{
    public partial class MainForm : Form
    {
        private NotifyIcon notification;            //об'єкт для відображення іконки у треї
        private Icon mainIcon;                      //об'єкт для зберігання іконки додатку
        private ContextMenuStrip notifyMenu;        //меню для іконки у треї


        public event EventHandler ExitMenuItem_Clicked;
        public event EventHandler Notify_DoubleClicked;
        public event EventHandler MainForm_Resized;
        public event EventHandler MainForm_Closing;
        public event EventHandler MeasureUnits_Changed;
        public event EventHandler SetCriticalsButton_Clicked;
        public event EventHandler DiskDataGrid_DoubleClicked;
        public event EventHandler HelpMenuItem_Clicked;


        public MainForm()
        {
            InitializeComponent();

            //створення та конфігурація об'єкту для відображення іконки у треї
            notification = new NotifyIcon();
            notification.Visible = false;
            notification.Text = "FreeDiskSpace Monitoring Tool";
            notification.DoubleClick += Notify_DoubleClick;

            //меню для іконки у треї
            notifyMenu = new ContextMenuStrip();
            ToolStripMenuItem showMenuItem = new ToolStripMenuItem("Отобразить окно");
            ToolStripMenuItem changeMenuItem = new ToolStripMenuItem("Задать критические значения");
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Выход");
            showMenuItem.Click += Notify_DoubleClick;
            changeMenuItem.Click += BSetCritical_Click;
            exitMenuItem.Click += ExitMenuItem_Click;
            notifyMenu.Items.Add(showMenuItem);
            notifyMenu.Items.Add(changeMenuItem);
            notifyMenu.Items.Add(exitMenuItem);
            notification.ContextMenuStrip = notifyMenu;

            //підписка на події головної форми
            Load += MainForm_Load;
            Resize += MainForm_Resize;
            FormClosing += MainForm_FormClosing;

            //налаштуваня ComboBox для відображення даних у різних одиницях (Мб / Гб)
            cbMeasureUnits.Items.AddRange(new string[] { "MB", "GB" });
            cbMeasureUnits.SelectedIndex = 0;
            cbMeasureUnits.SelectedValueChanged += CbMeasureUnits_SelectedValueChanged;

            //підписка на подію натискання кнопки, для встановлення ліміту
            bSetCritical.Click += BSetCritical_Click;

            //налаштування таблиці для відображення даних
            diskDataGridView.ColumnCount = 4;
            diskDataGridView.Columns[0].Name = "Tagged as";
            diskDataGridView.Columns[1].Name = "Total amount of memory";
            diskDataGridView.Columns[2].Name = "The amount of free memory";
            diskDataGridView.Columns[3].Name = "Free memory limit";
            foreach(DataGridViewColumn column in diskDataGridView.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            diskDataGridView.DoubleClick += DiskDataGridView_DoubleClick;
        }


        public DataGridView GetDataGrid()
        {
            return diskDataGridView;
        }

        //public void HideAlertWindow()
        //{
        //    AlertWindow.Hide();
        //}

        public void ChangeNotificationVisibility(bool state)
        {
            notification.Visible = state;
        }

        //обробник події завантаження головного вікна
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                mainIcon = new Icon("mainIcon.ico");
            }
            catch (System.IO.FileNotFoundException)
            {
                mainIcon = System.Drawing.SystemIcons.Application;
            }
            finally
            {
                this.Icon = mainIcon;
                notification.Icon = mainIcon;
            }
        }

        //скидання виділення для DataGridView
        private void DiskDataGridView_DoubleClick(object sender, EventArgs e)
        {
            DiskDataGrid_DoubleClicked?.Invoke(sender, e);
        }

        //обробники подій натискання на пункти меню у треї
        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            ExitMenuItem_Clicked?.Invoke(sender, e);
        }

        private void Notify_DoubleClick(object sender, EventArgs e)
        {
            Notify_DoubleClicked?.Invoke(sender, e);
        }

        //обробник події зміни розміру/стану вікна для визначення моменту появи іконки у треї
        private void MainForm_Resize(object sender, EventArgs e)
        {
            MainForm_Resized?.Invoke(sender, e);
        }

        //обробник події закриття вікна
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainForm_Closing?.Invoke(sender, e);
        }

        //обробник зміни обраного значення у ComboBox, що відповідає за одиниці виміру пам'яті
        private void CbMeasureUnits_SelectedValueChanged(object sender, EventArgs e)
        {
            MeasureUnits_Changed?.Invoke(sender, e);
        }

        //обробник події натискання на кнопку встановлення ліміту вільного об'єму пам'яті,
        //викликає відповідне діалогове вікно
        private void BSetCritical_Click(object sender, EventArgs e)
        {
            SetCriticalsButton_Clicked?.Invoke(sender, e);
        }
    }
}