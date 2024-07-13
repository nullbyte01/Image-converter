using ImageConverter.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.FFmpegWasm;
using SpawnDev.BlazorJS.JSObjects;
using System.Text.Json;
using System.Text;
using File = SpawnDev.BlazorJS.JSObjects.File;

namespace ImageConverter.Pages
{
    public partial class Home
    {
        #region Properties

        [Inject]
        BlazorJSRuntime JS { get; set; }
        [Inject]
        FFmpegFactory FFmpegFactory { get; set; }

        private string Message = "";
        private bool IsValidFile = false;
        private string HoverClass = string.Empty;
        string outputFileName = string.Empty;
        public List<string> convertFrom { get; set; }
        public List<string> convertTo { get; set; }
        public bool canViewConvertScreen { get; set; } = false;
        public string FileName { get; set; }
        bool beenInit = false;
        ElementReference fileInputRef;
        HTMLInputElement? fileInput;
        File? inputFileObj;
        FFmpeg? ffmpeg = null;
        public string selectedExtension { get; set; }
        protected override void OnAfterRender(bool firstRender)
        {
            if (!beenInit)
            {
                beenInit = true;
                fileInput = new HTMLInputElement(JS.ToJSRef(fileInputRef));
                fileInput.OnChange += InputFileUploaded;
            }
        }
        #endregion

        private async Task LoadFFMPEG()
        {
            StateHasChanged();
            await FFmpegFactory.Init();
            ffmpeg = new FFmpeg();
            ffmpeg.OnLog += FFmpeg_OnLog;
            ffmpeg.OnProgress += FFmpeg_OnProgress;
            var loadConfig = FFmpegFactory.CreateLoadCoreConfig();
            await ffmpeg.Load(loadConfig);
            StateHasChanged();
        }

        void FFmpeg_OnLog(FFmpegLogEvent ev)
        {
            JS.Log("FFmpeg_OnLog", ev.Message);
            StateHasChanged();
        }

        void FFmpeg_OnProgress(FFmpegProgressEvent ev)
        {
            var progress = ev.Progress;
            var time = ev.Time;
            JS.Log("FFmpeg_OnProgress", ev.Time, ev.Progress);
            StateHasChanged();
        }

        private async void InputFileUploaded(Event e)
        {
            canViewConvertScreen = false;
            using var files = fileInput!.Files;
            inputFileObj = files?.FirstOrDefault();
            if (inputFileObj != null)
            {
                await LoadFFMPEG();

                string extension = Path.GetExtension(inputFileObj.Name).Replace(".", string.Empty);
                IsValidFile = ValidFileHelper.IsValid(extension);
                if (IsValidFile)
                {
                    canViewConvertScreen = true;

                    FileName = inputFileObj.Name;

                    convertFrom = new List<string>();
                    convertFrom.AddRange(ValidFileHelper.fileTypesSupported.Where(file => file == extension.ToUpper()).ToList());

                    convertTo = new List<string>();
                    convertTo.AddRange(ValidFileHelper.fileTypesSupported);
                    convertTo.Remove(extension.ToUpper());

                    StateHasChanged();
                }
                else
                {
                    Message = "Invalid File Format selected";
                    StateHasChanged();
                }
            }
            else
            {
                Message = "Invalid File";
            }
        }

        void OnDragEnter(DragEventArgs e)
        {
            HoverClass = "hover";
        }

        void OnDragLeave(DragEventArgs e)
        {
            HoverClass = string.Empty;
        }

        async Task OnConvertClick()
        {
            if (inputFileObj != null)
            {
                var inputDir = "/input";
                await ffmpeg.CreateDir(inputDir);
                await ffmpeg.MountWorkerFS(inputDir, new FSMountWorkerFSOptions { Files = new[] { inputFileObj } });
                var inputFile = $"{inputDir}/{FileName}";

                var outputFile = $"{Path.GetFileNameWithoutExtension(FileName)}.{selectedExtension.ToLower()}";

                StateHasChanged();

                await ffmpeg.Exec(new string[] { "-i", inputFile, outputFile });
                StateHasChanged();
                await ffmpeg.Unmount(inputDir);
                await ffmpeg.DeleteDir(inputDir);
                using var data = await ffmpeg.ReadFileUint8Array(outputFile);
                using var blob = new Blob(new Uint8Array[] { data }, new BlobOptions { Type = $"image/{selectedExtension.ToLower()}" });
                await blob.StartDownload(outputFile);

                StateHasChanged();
                Message = "Downloading will be starting soon";
            }
            else
            {
                Message = "Invalid File";
            }
        }
    }
}
