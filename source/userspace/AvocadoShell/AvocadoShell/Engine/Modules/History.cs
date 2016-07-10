using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UtilityLib.MiscTools;

namespace AvocadoShell.Engine.Modules
{
    sealed class History
    {
        readonly int maxHistoryCount;
        readonly LinkedList<string> buffer;
        LinkedListNode<string> currentNode;

        public History(int maxHistoryCount)
        {
            this.maxHistoryCount = maxHistoryCount;
            buffer = getHistoryFromDisk();
            reset();
        }

        string getFilePath()
        {
            var roaming = Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData);
            return Path.Combine(roaming,
                @"Microsoft\Windows\PowerShell\PSReadline\",
                "ConsoleHost_history.txt");
        }

        LinkedList<string> getHistoryFromDisk()
        {
            var filePath = getFilePath();
            return File.Exists(filePath)
                ? new LinkedList<string>(File.ReadAllLines(filePath))
                : new LinkedList<string>();
        }

        public void Add(string input)
        {
            if (input == null) return;

            try
            {
                // Check if the input should be added.
                if (!shouldAdd(input)) return;

                // Save the input.
                Task.Run(() => writeInputToDisk(input));

                // Set the last buffer value as the specified input.
                buffer.Last.Value = input;
            }
            finally { reset(); }
        }

        void writeInputToDisk(string input)
        {
            var filePath = getFilePath();
            var linesToWrite = input.Yield();
            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                linesToWrite = lines
                    .Skip(lines.Length - maxHistoryCount + 1)
                    .Concat(linesToWrite);
            }
            else Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllLines(filePath, linesToWrite);
        }

        bool shouldAdd(string input)
        {
            // Don't save blank input.
            if (isEmptyOrWhitespace(input)) return false;

            // If the input was the same as the previous one entered, don't save
            // it again.
            var prevCmd = isEmptyOrWhitespace(lastInput)
                ? buffer.Last?.Previous?.Value
                : lastInput;
            if (prevCmd == input) return false;

            return true;
        }

        public string Cycle(bool forward)
        {
            // Get the node to move to.
            var destinationNode = forward
                ? currentNode.Next
                : currentNode.Previous;

            // Return null if the destination node does not exist.
            if (destinationNode == null) return null;

            // Update the pointer to the current node and return its new 
            // value.
            currentNode = destinationNode;
            return currentNode.Value;
        }

        public void CacheInput(string input) => currentNode.Value = input;

        void reset()
        {
            // If not already there add a blank string to the end of the buffer.
            if (!isEmptyOrWhitespace(lastInput)) buffer.AddLast(string.Empty);

            // Reset the pointer of the current node to that blank string.
            currentNode = buffer.Last;
        }

        string lastInput => buffer.Last?.Value;

        bool isEmptyOrWhitespace(string str) => str?.Trim() == string.Empty;
    }
}