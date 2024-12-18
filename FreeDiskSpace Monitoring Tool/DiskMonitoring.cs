using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Windows.Forms;

namespace FreeDiskSpace_Monitoring_Tool
{
    //перелік для позначення одиниць виміру даних
    public enum MemoryUnits
    {
        MB = 0,
        GB = 1
    }

    public class DiskMonitoring
    {
        private Dictionary<string, DiskState> disks;                        //колекція для зберігання даних про диски
        private readonly string saveFileName;                               //ім'я файла для зберігання встановлених користувачем критичних значень
        private Dictionary<string, long> storedCriticalValues;              //тимчасове сховище для завантажених даних
        private Action<Dictionary<string, DiskState>> _limitExceeded;        //подія, що відбувається при перевищенні заданого значення
        public event Action<Dictionary<string, DiskState>> LimitExceeded
        {
            add { if (_limitExceeded.GetInvocationList().Length == 1) { _limitExceeded += value; } }
            remove { _limitExceeded -= value; }
        }
        public MemoryUnits currentUnit;                                     //поточна обрана одиниця виміру даних

        public DiskMonitoring(string fileName)
        {
            _limitExceeded = new Action<Dictionary<string, DiskState>>(LimitExceededInitMethod);
            disks = new Dictionary<string, DiskState>();
            currentUnit = MemoryUnits.MB;
            saveFileName = fileName;

            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in drives)
            { 
                if (drive.IsReady)
                {
                    disks.Add(drive.Name, new DiskState()
                    {
                        FreeSpace = drive.AvailableFreeSpace,
                        TotalSpace = drive.TotalSize,
                    });
                }
            }

            if (File.Exists(saveFileName)) {

                using (FileStream file = new FileStream(saveFileName, FileMode.Open, FileAccess.Read))
                {
                    DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(Dictionary<string, long>));
                    storedCriticalValues = json.ReadObject(file) as Dictionary<string, long>;
                }
                foreach (var item in storedCriticalValues)
                {
                    if (disks.ContainsKey(item.Key))
                    {
                        disks[item.Key].CriticalValue = item.Value;
                    }
                }
            } else
            {
                storedCriticalValues = new Dictionary<string, long>();
            }
        }

        private void LimitExceededInitMethod (Dictionary<string, DiskState> temp) { }

        //копія даних для передачі у діалогове вікно
        public Dictionary<string, DiskState> GetDataCopy()
        {
            return new Dictionary<string, DiskState>(disks);
        }

        //завантаження даних на DataGridView
        public void LoadData(DataGridView data)
        {
            data.Rows.Clear();

            double Total, Free, Critical;

            foreach (var disk in disks)
            {
                Total = (currentUnit == MemoryUnits.MB) ? (disk.Value.TotalSpace / Math.Pow(1024, 2)) : (disk.Value.TotalSpace / Math.Pow(1024, 3));
                Free = (currentUnit == MemoryUnits.MB) ? (disk.Value.FreeSpace / Math.Pow(1024, 2)) : (disk.Value.FreeSpace / Math.Pow(1024, 3));
                Critical = (currentUnit == MemoryUnits.MB) ? (disk.Value.CriticalValue / Math.Pow(1024, 2)) : (disk.Value.CriticalValue / Math.Pow(1024, 3));

                data.Rows.Add(disk.Key,
                              string.Format("{0:0.00}", Total),
                              string.Format("{0:0.00}", Free),
                              string.Format("{0:0.00}", Critical));
            }
        }

        //збереження заданих критичних значень до файлу
        public void SaveCriticalValues()
        {
            storedCriticalValues.Clear();

            foreach(var item in disks)
            {
                storedCriticalValues.Add(item.Key, item.Value.CriticalValue);
            }
            using (FileStream file = new FileStream(saveFileName, FileMode.Create, FileAccess.Write))
            {
                DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(Dictionary<string, long>));
                json.WriteObject(file, storedCriticalValues);
            }
        }

        //оновлення даних про диски
        public void UpdateDiskStates()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            Dictionary<string, DiskState> temp = new Dictionary<string, DiskState>();

            foreach (DriveInfo drive in drives)
            {
                if (drive.IsReady)
                {
                    temp.Add(drive.Name, new DiskState()
                    {
                        FreeSpace = drive.AvailableFreeSpace,
                        TotalSpace = drive.TotalSize,
                        CriticalValue = disks.ContainsKey(drive.Name) ? disks[drive.Name].CriticalValue : 0
                    });
                }
            }
           // disks.Clear();
            disks = temp;

            temp = new Dictionary<string, DiskState>();

            foreach (var item in disks)
            {
                if (item.Value.CriticalValue > item.Value.FreeSpace)
                {
                    temp.Add(item.Key, item.Value);
                }
            }


            _limitExceeded?.Invoke(temp);

            //temp.Clear();
        }

        //оновлення порогових значень, отриманих із діалогового вікна
        public void UpdateCriticalValues(Dictionary<string, long> criticalValues)
        {
            foreach (var disk in criticalValues)
            {
                if (disks.ContainsKey(disk.Key))
                {
                    disks[disk.Key].CriticalValue = disk.Value;
                }
            }
        }
    }
}