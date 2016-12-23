﻿using AvocadoDownloader.BusinessLayer;
using AvocadoFramework.Engine;
using AvocadoUtilities.Context;
using DownloaderProtocol;
using StandardLibrary.Processes;
using StandardLibrary.Processes.NamedPipes;
using StandardLibrary.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows.Shell;

namespace AvocadoDownloader
{
    public partial class MainWindow : GlassPane
    {
        string serializationPath 
            => Path.Combine(AvocadoContext.AppDataPath, "downloads.bin");

        readonly GrouperList grouperList;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize data context.
            grouperList = createGrouperList();
            DataContext = grouperList;

            // Clear any files that no longer exist.
            clearNonexistentFileItems();

            // Start server and process command line.
            initServer().RunAsync();
            processCommandlineArgs(EnvUtils.GetArgs());
        }

        IEnumerable<FileItem> getFileItems()
            => grouperList.Groupers.SelectMany(g => g.FileItems);

        IEnumerable<FileItem> getUnfinishedFileItems()
            => getFileItems().Where(f => !f.FinishedDownloading);

        IEnumerable<FileItem> getNonexistentFileItems()
            => getFileItems().Where(f => !File.Exists(f.FilePath));

        GrouperList createGrouperList()
        {
            if (!File.Exists(serializationPath)) return new GrouperList();
            using (var stream = new FileStream(
                serializationPath, FileMode.Open))
            {
                try
                {
                    return (GrouperList)new BinaryFormatter()
                        .Deserialize(stream);
                }
                catch (Exception exc)
                {
                    // Show error and discard the serialized file if it is 
                    // corrupt.
                    new MessagePane(exc.Message).ShowDialog();
                    return new GrouperList();
                }
            }
        }

        void serializeData()
        {
            using (var stream = new FileStream(
                serializationPath, FileMode.Create))
            {
                new BinaryFormatter().Serialize(stream, grouperList);
            }
        }

        Task initServer()
        {
            var server = new NamedPipeServer();
            server.MessageReceived += (s, e) => processMessage(e.Message);
            return server.Start(ProtocolConfig.PipeName);
        }

        void processCommandlineArgs(IEnumerable<string> args)
        {
            // Directory path is the first argument. Quit if it was not sent.
            var directoryPath = args.FirstOrDefault();
            if (directoryPath == null) return;

            // Subsequent arguments are file paths grouped under the directory.
            // Quit if this set is empty.
            var fileItems = args
                .Skip(1)
                .Select(f => createFileItem(f))
                .ToList();
            if (!fileItems.Any()) return;

            grouperList.AddGrouper(directoryPath, fileItems);
            notifyDownloadStarted();
        }

        FileItem createFileItem(string filePath)
        {
            var fileItem = new FileItem(filePath);
            fileItem.DownloadFinished += onFileItemDownloadFinished;
            return fileItem;
        }

        void notifyDownloadStarted()
        {
            TaskbarItemInfo.ProgressState 
                = TaskbarItemProgressState.Indeterminate;
            Activate();
        }

        void onFileItemDownloadFinished(object sender, EventArgs e) =>
            Dispatcher.InvokeAsync(() =>
                TaskbarItemInfo.ProgressState = getUnfinishedFileItems().Any()
                    ? TaskbarItemProgressState.Indeterminate
                    : TaskbarItemProgressState.None);

        void processMessage(string message)
        {
            if (message.StartsWith(ProtocolConfig.MessageDelimiter))
                processProtocolMessage(message);
            // If the message did not start with an expected delimiter, process
            // it as coming from the commandline.
            else
            {
                Dispatcher.InvokeAsync(() => 
                    processCommandlineArgs(EnvUtils.SplitArgStr(message)));
            }
        }

        void processProtocolMessage(string message)
        {
            var pieces = message.Split(
                ProtocolConfig.MessageDelimiter.Yield().ToArray(),
                StringSplitOptions.RemoveEmptyEntries);
            var messageType = (MessageType)int.Parse(pieces[0]);
            var directoryPath = pieces[1];
            var filePath = pieces[2];
            var data = pieces[3];
            var fileItem = grouperList.GetGrouper(directoryPath)
                .GetFileItem(filePath);
            switch (messageType)
            {
                case MessageType.PrepareForDownload:
                    fileItem.PrepareForDownload(data).RunAsync();
                    break;

                case MessageType.DownloadFromUrl:
                    fileItem.DownloadFromUrl(data).RunAsync();
                    break;
            }
        }

        // Delete file items that have not finished downloading.
        void deleteUnfinishedFileItems()
            => getUnfinishedFileItems().ToList().ForEach(f => f.Remove(true));

        // Clear any files that no longer exist.
        void clearNonexistentFileItems()
            => getNonexistentFileItems().ToList().ForEach(f => f.Remove(false));

        void GlassPane_Closed(object sender, EventArgs e)
        {
            deleteUnfinishedFileItems();
            serializeData();
        }
    }
}
