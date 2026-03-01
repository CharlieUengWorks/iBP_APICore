using LibreHardwareMonitor.Hardware;
using System.Text.Json;

namespace WebApplication1
{
    using System.Runtime.InteropServices;
    using System.Security.Principal;

    public class MonitorData
    {
        public bool IsRunningAsAdmin { get; set; }
        public CoreValue CPU { get; set; }
        public List<CoreValue> GPU { get; set; }
    }

    public class CoreValue
    {
        public CoreValue() {
            name = "na";
            CoreLoad = -99;
            CoreTemperature = -99;
        }
        public string name { get; set; }
        public float CoreLoad { get; set; }
        public float CoreTemperature { get; set; }
    }

    public class UpdateVisitor : IVisitor
    {
        public bool IsAdministrator()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return false;

            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }
        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
        }

        public void VisitSensor(ISensor sensor) { }

        public void VisitParameter(IParameter parameter) { }

        public MonitorData GetMonitorData()
        {
            MonitorData monitorData = new MonitorData();
            CoreValue cpu = new CoreValue();
            var gpuList = new List<CoreValue>();
            Computer computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
            };

            computer.Open();

            foreach (IHardware hardware in computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuIntel || hardware.HardwareType == HardwareType.GpuAmd)
                {
                    CoreValue gpu = new CoreValue();
                    FetchGPUValues(hardware, gpu);
                    gpuList.Add(gpu);
                }
                if (hardware.HardwareType == HardwareType.Cpu)
                    FetchCPUValues(hardware, cpu);
            }
            computer.Close();

            monitorData.CPU = cpu;
            monitorData.GPU = gpuList;

            monitorData.IsRunningAsAdmin = IsAdministrator();

            //Console.WriteLine("result:{0},{1},{2}", gpu.name, gpu.CoreTemperature, gpu.CoreLoad);
            string jsonString = JsonSerializer.Serialize(monitorData);
            Console.WriteLine(jsonString);

            return monitorData;
        }

        void FetchGPUValues(IHardware hardware,CoreValue holder)
        {
            holder.name = hardware.Name;
            foreach (ISensor sensor in hardware.Sensors)
            {
                if (sensor.Value == null) continue;
                //Console.WriteLine("\tSensor: {0}, type: {1}, value: {2}", sensor.Name, sensor.SensorType, sensor.Value);

                if (sensor.Name.Equals("GPU Core") && sensor.SensorType == SensorType.Temperature)
                    holder.CoreTemperature = (float)sensor.Value;
                if (sensor.Name.Equals("GPU Core") && sensor.SensorType == SensorType.Load)
                    holder.CoreLoad = (float)sensor.Value;
            }
        }

        void FetchCPUValues(IHardware hardware, CoreValue holder)
        {
            holder.name = hardware.Name;
            foreach (ISensor sensor in hardware.Sensors)
            {
                //Console.WriteLine("\tSensor: {0}, type: {1}, value: {2}", sensor.Name, sensor.SensorType, sensor.Value);
                if (sensor.Value == null) continue;
                
                if (sensor.Name.Equals("Core Average") && sensor.SensorType == SensorType.Temperature)
                    holder.CoreTemperature = (float)sensor.Value;
                if (sensor.Name.Equals("CPU Total") && sensor.SensorType == SensorType.Load)
                    holder.CoreLoad = (float)sensor.Value;
                
            }
        }
    }
}