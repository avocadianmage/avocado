using Microsoft.Management.Infrastructure;
using Microsoft.WSMan.Management;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using UtilityLib.MiscTools;

namespace AvocadoShell.PowerShellService
{
    static class OutputFormatter
    {

        public static IEnumerable<string> FormatPSObject(PSObject psObj)
        {
            // Handle specific formatting based on the underlying object type.
            var baseObj = psObj.BaseObject;
            if (baseObj is CimInstance) return formatCimInstance(psObj);
            if (baseObj is WSManConfigElement)
            {
                return formatWSManConfigElement(baseObj as WSManConfigElement);
            }

            // Default formatting.
            return psObj.ToString().Yield();
        }

        static IEnumerable<string> formatCimInstance(PSObject psObj)
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
                yield return $"{paddedName} → {prop.Value}";
            }
        }

        static IEnumerable<string> formatWSManConfigElement(
            WSManConfigElement ele)
        {
            var leaf = ele as WSManConfigLeafElement;
            var val = leaf == null ? string.Empty : $" → {leaf.Value}";
            yield return $"{ele.Name}{val}";
        }
    }
}