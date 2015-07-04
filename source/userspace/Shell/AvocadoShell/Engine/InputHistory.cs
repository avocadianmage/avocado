using System.Collections.Generic;

namespace AvocadoShell.Engine
{
    sealed class InputHistory
    {
        readonly LinkedList<string> buffer = new LinkedList<string>();
        LinkedListNode<string> currentNode;

        public InputHistory()
        {
            reset();
        }

        public void Add(string command)
        {
            // Set the last buffer value as the specified command.
            buffer.Last.Value = command;

            // Reset the buffer.
            reset();
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

        public void SaveInput(string input)
        {
            currentNode.Value = input;
        }

        void reset()
        {
            // Reset the pointer of the current node to a blank string at the
            // end of the list.
            currentNode = buffer.AddLast(string.Empty);
        }
    }
}