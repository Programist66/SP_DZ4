using Microsoft.Win32;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SP_DZ4
{
    public class MainVM : BindableBase
    {
        private CountdownEvent @event;

        private List<CopiedFile>? copiedFiles;
        private string sourceDirectory = "";
        private string destinationDirectory = "";

        private long totalBytesCopied = 0;
        private long totalBytesToCopy = 0;
        private readonly object lockObj = new();

        private const int FileCount = 4;

        private bool copyCanStart = true;

        public string SourceDirectory 
        {
            get => sourceDirectory;
            set 
            {
                SetProperty(ref sourceDirectory, value);
            }
        }

        public string DestinationDirectory
        {
            get => destinationDirectory;
            set 
            {
                SetProperty(ref destinationDirectory, value);
            }
        }

        public bool CopyCanStart 
        {
            get 
            {
                return copyCanStart;
            }
            set 
            {
                SetProperty(ref copyCanStart, value);
            }
        }


        public double TotalProgress 
        {
            get 
            {
                if (totalBytesToCopy == 0)
                {
                    return 0;
                }
                return (double) totalBytesCopied / totalBytesToCopy;
            }
        }

        public MainVM() 
        {
            StartCopyCommand = new DelegateCommand(StartCopy);
            ChoiseSourceFolderCommand = new DelegateCommand(ChoiseSourceFolder);
            ChoiseDestinationFolderCommand = new DelegateCommand(ChoiseDestinationFolder);
            copiedFiles = [];
            //copiedFiles.Add(new CopiedFile("", 0));
        }

        #region "File Names"
        public string FileName1 
        {
            get 
            {
                if (copiedFiles is null || copiedFiles.Count < 1)
                {
                    return "";
                }
                return copiedFiles[0].FileName;
            }
            set 
            {
                if (copiedFiles is null || copiedFiles.Count < 1)
                {
                    throw new IndexOutOfRangeException();
                }
                RaisePropertyChanged(nameof(FileName1));
                copiedFiles[0].FileName = value;
            }
        }
        public string FileName2
        {
            get
            {
                if (copiedFiles is null || copiedFiles.Count < 2)
                {
                    return "";
                }
                return copiedFiles[1].FileName;
            }
            set
            {
                if (copiedFiles is null || copiedFiles.Count < 2)
                {
                    throw new IndexOutOfRangeException();
                }
                RaisePropertyChanged(nameof(FileName2));
                copiedFiles[1].FileName = value;
            }
        }
        public string FileName3
        {
            get
            {
                if (copiedFiles is null || copiedFiles.Count < 3)
                {
                    return "";
                }
                return copiedFiles[2].FileName;
            }
            set
            {
                if (copiedFiles is null || copiedFiles.Count < 3)
                {
                    throw new IndexOutOfRangeException();
                }
                RaisePropertyChanged(nameof(FileName3));
                copiedFiles[2].FileName = value;
            }
        }
        public string FileName4
        {
            get
            {
                if (copiedFiles is null || copiedFiles.Count < 4)
                {
                    return "";
                }
                return copiedFiles[3].FileName;
            }
            set
            {
                if (copiedFiles is null || copiedFiles.Count < 4)
                {
                    throw new IndexOutOfRangeException();
                }
                RaisePropertyChanged(nameof(FileName4));
                copiedFiles[3].FileName = value;
            }
        }
        #endregion

        #region "Progresses"

        public double Progress1 
        {
            get
            {
                if (copiedFiles is null || copiedFiles.Count < 1)
                {
                    return 0;
                }
                return copiedFiles[0].Progress;
            }
            set 
            {
                if (copiedFiles is null || copiedFiles.Count < 1)
                {
                    throw new IndexOutOfRangeException();
                }
                RaisePropertyChanged(nameof(Progress1));
                copiedFiles[0].Progress = value;
            }
        }
        public double Progress2
        {
            get
            {
                if (copiedFiles is null || copiedFiles.Count < 2)
                {
                    return 0;
                }
                return copiedFiles[1].Progress;
            }
            set
            {
                if (copiedFiles is null || copiedFiles.Count < 2)
                {
                    throw new IndexOutOfRangeException();
                }
                RaisePropertyChanged(nameof(Progress2));
                copiedFiles[1].Progress = value;
            }
        }
        public double Progress3
        {
            get
            {
                if (copiedFiles is null || copiedFiles.Count < 3)
                {
                    return 0;
                }
                return copiedFiles[2].Progress;
            }
            set
            {
                if (copiedFiles is null || copiedFiles.Count < 3)
                {
                    throw new IndexOutOfRangeException();
                }
                RaisePropertyChanged(nameof(Progress3));
                copiedFiles[2].Progress = value;
            }
        }
        public double Progress4
        {
            get
            {
                if (copiedFiles is null || copiedFiles.Count < 4)
                {
                    return 0;
                }
                return copiedFiles[3].Progress;
            }
            set
            {
                if (copiedFiles is null || copiedFiles.Count < 4)
                {
                    throw new IndexOutOfRangeException();
                }
                RaisePropertyChanged(nameof(Progress4));
                copiedFiles[3].Progress = value;
            }
        }

        #endregion

        #region "Commands and Functions"

        public ICommand ChoiseSourceFolderCommand { get; }
        private void ChoiseSourceFolder() 
        {
            OpenFolderDialog dialog = new();
            if (dialog.ShowDialog() == true)
            {
                SourceDirectory = dialog.FolderName;
            }
        }

        public ICommand ChoiseDestinationFolderCommand { get; }
        private void ChoiseDestinationFolder()
        {
            OpenFolderDialog dialog = new();
            if (dialog.ShowDialog() == true)
            {
                DestinationDirectory = dialog.FolderName;
            }
        }

        public ICommand StartCopyCommand { get; }
        private void StartCopy() 
        {
            if (SourceDirectory == "" || DestinationDirectory == "" || copiedFiles is null || !CopyCanStart)
            {
                return;
            }
            totalBytesCopied = 0;
            totalBytesToCopy = 0;
            CopyCanStart = false;
            @event = new(FileCount);
            Thread copyThread = new(StartCopyThread);
            copyThread.Start();
        }

        private void StartCopyThread() 
        {            
            List<string> filesToCopy = Directory.GetFiles(SourceDirectory).ToList();
            for (int i = 0; i < filesToCopy.Count; i++)
            {
                copiedFiles!.Add(new CopiedFile(filesToCopy[i], 0));
                totalBytesToCopy += new FileInfo(filesToCopy[i]).Length;
            }
            RaisePropertyChanged(nameof(FileName1));
            RaisePropertyChanged(nameof(FileName2));
            RaisePropertyChanged(nameof(FileName3));
            RaisePropertyChanged(nameof(FileName4));
            RaisePropertyChanged(nameof(TotalProgress));
            for (int i = 0; i < FileCount; i++)
            {
                int index = i;
                Thread copyThread = new(() => CopyFile(filesToCopy[index], Path.Combine(DestinationDirectory, Path.GetFileName(filesToCopy[index])), index));
                copyThread.Start();
            }
            @event.Wait();
            CopyCanStart = true;
        }

        private void CopyFile(string sourceFilePath, string destinationFilePath, int index)
        {
            using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
            using (FileStream destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = new byte[4096];
                int bytesRead;
                long fileSize = sourceStream.Length;
                long bytesCopied = 0;

                while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    Thread.Sleep(100);
                    destinationStream.Write(buffer, 0, bytesRead);
                    bytesCopied += bytesRead;

                    
                    lock (lockObj)
                    {
                        totalBytesCopied += bytesRead;
                        RaisePropertyChanged(nameof(TotalProgress));
                    }                    
                    double fileProgress = (double)bytesCopied / fileSize;
                    switch (index)
                    {
                        case 0:
                            Progress1 = fileProgress;
                            break;
                        case 1:
                            Progress2 = fileProgress;
                            break;
                        case 2:
                            Progress3 = fileProgress;
                            break;
                        case 3:
                            Progress4 = fileProgress;
                            break;
                    }                    
                }
            }
            @event.Signal();
        }
    #endregion
    }
}
