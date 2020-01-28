using System;

namespace FreeDiskSpace_Monitoring_Tool
{
    public class DiskState
    {
        //загальний об'єм пам'яті на диску
        public long TotalSpace { get; set; }

        //об'єм вільної пам'яті на диску
        public long FreeSpace { get; set; }

        //встановлений користувачем ліміт вільної пам'яті
        public long CriticalValue { get; set; }

        public DiskState() {}

        //конструктор копіювання
        public DiskState(DiskState obj)
        {
            TotalSpace = obj.TotalSpace;
            FreeSpace = obj.FreeSpace;
            CriticalValue = obj.CriticalValue;
        }
    }
}