using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace FreeDiskSpace_Monitoring_Tool
{
    public partial class DiskInfoForm : Form
    {
        private DriveInfo selectedDiskInfo;
        public DiskInfoForm(string diskName)
        {
            InitializeComponent();
            selectedDiskInfo = new DriveInfo(diskName);
            LblDriveName.Text += selectedDiskInfo.Name;
            LblDriveFormat.Text += selectedDiskInfo.DriveFormat;
            LblFreeSpace.Text += string.Format("{0:N} байт,  {1:0.00} Гб", selectedDiskInfo.AvailableFreeSpace, selectedDiskInfo.AvailableFreeSpace / Math.Pow(1024, 3));
            LblTotalSpace.Text += string.Format("{0:N} байт,  {1:0.00} Гб", selectedDiskInfo.TotalSize, selectedDiskInfo.TotalSize / Math.Pow(1024, 3));
            LblOccupiedSpace.Text += string.Format("{0:N} байт,  {1:0.00} Гб", selectedDiskInfo.TotalSize - selectedDiskInfo.AvailableFreeSpace, (selectedDiskInfo.TotalSize - selectedDiskInfo.AvailableFreeSpace)/Math.Pow(1024, 3));
            switch (selectedDiskInfo.DriveType)
            {
                case DriveType.Fixed:
                    LblDriveType.Text += "Жесткий диск";
                    break;

                case DriveType.Removable:
                    LblDriveType.Text += "Съемное устройство";
                    break;

                case DriveType.CDRom:
                    LblDriveType.Text += "Оптический диск (DVD/CD)";
                    break;

                case DriveType.Network:
                    LblDriveType.Text += "Сетевой диск";
                    break;

                case DriveType.Unknown:
                    LblDriveType.Text += "Неизвестен";
                    break;
            }

            Paint += DiskInfoForm_Paint;
        }

        private void DiskInfoForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush freeSpaceBrush = Brushes.Yellow;
            Brush occupiedSpaceBrush = Brushes.Blue;
            Rectangle diagramArea = new Rectangle(150, 250, 150, 150);

            g.FillRectangle(freeSpaceBrush, new Rectangle(25, 164, 25, 25));
            g.FillRectangle(occupiedSpaceBrush, new Rectangle(25, 203, 25, 25));

            float freeSizeProportion = selectedDiskInfo.AvailableFreeSpace * 360 / selectedDiskInfo.TotalSize;

            g.FillPie(occupiedSpaceBrush, diagramArea, 0, 360 - freeSizeProportion);
            g.FillPie(freeSpaceBrush, diagramArea, 360 - freeSizeProportion, freeSizeProportion);
            g.DrawEllipse(Pens.Black, diagramArea);
            
        }
    }
}
