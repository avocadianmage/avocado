using AvocadoShell.Engine;
using Microsoft.Management.Infrastructure;
using Microsoft.WSMan.Management;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace AvocadoShell.PowerShellService
{
    sealed class OutputFormatter
    {
        readonly IShellUI shellUI;

        public OutputFormatter(IShellUI shellUI)
        {
            this.shellUI = shellUI;
        }

        public void OutputError(ErrorRecord error)
            => shellUI.WriteErrorLine(error.ToString());

        public void OutputPSObject(PSObject psObj)
        {
            // Handle specific formatting based on the underlying object type.
            var baseObj = psObj.BaseObject;
            if (baseObj is CimInstance) outputCimInstance(psObj);
            else if (baseObj is WSManConfigElement)
            {
                outputWSManConfigElement(baseObj as WSManConfigElement);
            }

            // Default formatting.
            else shellUI.WriteOutputLine(psObj.ToString());
        }

        void outputCimInstance(PSObject psObj)
        {
            var propDict = new Dictionary<string, string>();
            var nameColWidth = 0;

            var typedObj = psObj.BaseObject as CimInstance;
            foreach (var prop in typedObj.CimInstanceProperties)
            {
                // Skip the key property (ex: 'InstanceId').
                if (prop.Flags.HasFlag(CimFlags.Key)) continue;

                // Skip properties with empty values.
                var name = prop.Name;
                var val = psObj.Properties[name].Value?.ToString();
                if (string.IsNullOrWhiteSpace(val)) continue;

                // The property is valid for display if we got this far.
                propDict.Add(name, val);
                nameColWidth = Math.Max(nameColWidth, name.Length);
            }

            // Format and output the properties.
            foreach (var prop in propDict)
            {
                var paddedName = prop.Key.PadRight(nameColWidth);
                shellUI.WriteOutputLine($"{paddedName} → {prop.Value}");
            }
        }

        void outputWSManConfigElement(WSManConfigElement ele)
        {
            var leaf = ele as WSManConfigLeafElement;
            var val = leaf == null ? string.Empty : $" → {leaf.Value}";
            shellUI.WriteOutputLine($"{ele.Name}{val}");
        }
    }
}