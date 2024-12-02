using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using UnityEngine.Device;

namespace Utils {
    public static class SystemUtils {
        public static string GetSystemInfo() {
            var sb = new StringBuilder();

            sb.AppendLine("Operating System: " + SystemInfo.operatingSystem);
            sb.AppendLine("Processor Type: " + SystemInfo.processorType);
            sb.AppendLine("Processor Count: " + SystemInfo.processorCount);
            sb.AppendLine("System Memory Size: " + SystemInfo.systemMemorySize + " MB");
            sb.AppendLine("Graphics Device Name: " + SystemInfo.graphicsDeviceName);
            sb.AppendLine("Graphics Device Type: " + SystemInfo.graphicsDeviceType);
            sb.AppendLine("Graphics Memory Size: " + SystemInfo.graphicsMemorySize + " MB");
            sb.AppendLine("Device Unique Identifier: " + SystemInfo.deviceUniqueIdentifier);

            return sb.ToString();
        }

        public static string GetSystemInfoAsListItems(string systemInfo) {
            var items = systemInfo.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var listItems = new List<string>();

            foreach (var item in items) {
                if (!string.IsNullOrWhiteSpace(item)) {
                    listItems.Add($@"
                {{
                    ""object"": ""block"",
                    ""type"": ""bulleted_list_item"",
                    ""bulleted_list_item"": {{
                        ""text"": [
                            {{
                                ""type"": ""text"",
                                ""text"": {{
                                    ""content"": ""{item}""
                                }}
                            }}
                        ]
                    }}
                }}");
                }
            }

            return string.Join(",", listItems);
        }

        public static string GetMacAddress() {
            var macAddress = string.Empty;
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces()) {
                if (nic.OperationalStatus == OperationalStatus.Up) {
                    macAddress = nic.GetPhysicalAddress().ToString();
                    if (!string.IsNullOrEmpty(macAddress)) {
                        return macAddress;
                    }
                }
            }

            return "Unknown";
        }
    }
}