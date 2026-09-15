using Craft.Files;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace Craft.UiBuilders.Components;

public partial class FileUpload
{
    [Inject] public required IFileUploadService FileUploadService { get; set; }
    [Inject] public required ISnackbar Snackbar { get; set; }

    [Parameter] public EventCallback<List<IBrowserFile>> OnUploadFiles { get; set; }
    [Parameter] public EventCallback<FileUploadResult> OnUploadCompleted { get; set; }
    [Parameter] public bool MultipleFiles { get; set; }
    [Parameter] public string AcceptedFileExtensions { get; set; } = ".*";
    [Parameter] public long? MaxFileSizeInBytes { get; set; }
    [Parameter] public string UploadType { get; set; } = "General";
    [Parameter] public string Height { get; set; } = "250px";
    [Parameter] public string DropZoneText { get; set; } = "Drag and drop a file here, or click to select.";
    [Parameter] public string PickText { get; set; } = "Pick";
    [Parameter] public string UploadText { get; set; } = "Upload";
    [Parameter] public string ClearText { get; set; } = "Clear";

    private const string DefaultDragClass = "relative rounded-lg border-2 border-dashed pa-4 mt-4 mud-width-full mud-height-full";
    private string _dragClass = DefaultDragClass;
    private readonly List<IBrowserFile> _browserFiles = [];
    private MudFileUpload<IBrowserFile>? _file;
    private MudFileUpload<IReadOnlyList<IBrowserFile>>? _files;
    private bool _isUploading;
    private int _uploadProgress;

    private void OnInputFileChanged(InputFileChangeEventArgs e)
    {
        ClearDragClass();

        try
        {
            var selectedFiles = MultipleFiles
                ? e.GetMultipleFiles()
                : [e.File];

            if (selectedFiles.Count == 0)
                return;

            if (IsInvalidExtension(selectedFiles))
                return;

            if (IsInvalidSize(selectedFiles))
                return;

            _browserFiles.Clear();
            _browserFiles.AddRange(selectedFiles);

            StateHasChanged();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Unable to select the file: {ex.Message}", Severity.Warning);
        }
    }

    private bool IsInvalidExtension(IReadOnlyList<IBrowserFile> files)
    {
        var validExtensions =
            AcceptedFileExtensions
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => x.ToLowerInvariant())
                .ToHashSet();

        if (validExtensions.Contains(".*"))
            return false;

        var invalid = files.Any(file =>
        {
            var extension = Path.GetExtension(file.Name).ToLowerInvariant();

            return !validExtensions.Contains(extension);
        });

        if (invalid)
            Snackbar.Add("One or more selected files have an invalid extension.", Severity.Error);

        return invalid;
    }

    private bool IsInvalidSize(IReadOnlyList<IBrowserFile> files)
    {
        if (MaxFileSizeInBytes is null)
            return false;

        var invalid = files.Any(file => file.Size > MaxFileSizeInBytes.Value);

        if (invalid)
            Snackbar.Add("One or more selected files exceed the maximum allowed size.", Severity.Error);

        return invalid;
    }

    private async Task UploadFilesAsync()
    {
        if (_browserFiles.Count == 0)
            return;

        _isUploading = true;
        _uploadProgress = 0;

        try
        {
            if (OnUploadFiles.HasDelegate)
                await OnUploadFiles.InvokeAsync(_browserFiles);

            foreach (var browserFile in _browserFiles)
            {
                var result = await FileUploadService.UploadBrowserFileAsync(
                        browserFile,
                        UploadType,
                        progress => { _uploadProgress = progress; InvokeAsync(StateHasChanged); });

                if (!result.IsSuccess)
                {
                    Snackbar.Add(result.ErrorMessage ?? "File upload failed.", Severity.Error);
                    continue;
                }

                await OnUploadCompleted.InvokeAsync(result);
            }
        }
        finally
        {
            _isUploading = false;
            _uploadProgress = 0;
        }
    }

    private async Task ClearAsync()
    {
        if (MultipleFiles)
        {
            if (_files is not null)
                await _files.ClearAsync();
        }
        else
        {
            if (_file is not null)
                await _file.ClearAsync();
        }

        _browserFiles.Clear();

        ClearDragClass();
        StateHasChanged();
    }

    private Task OpenFilePickerAsync()
    {
        return MultipleFiles
            ? _files?.OpenFilePickerAsync() ?? Task.CompletedTask
            : _file?.OpenFilePickerAsync() ?? Task.CompletedTask;
    }

    private void Closed(MudChip<string> chip)
    {
        var file = _browserFiles.FirstOrDefault(x => x.Name == chip.Text);

        if (file is not null)
            _browserFiles.Remove(file);

        StateHasChanged();
    }

    private void SetDragClass()
    {
        _dragClass = $"{DefaultDragClass} mud-border-info";
    }

    private void ClearDragClass()
    {
        _dragClass = DefaultDragClass;
    }
}
