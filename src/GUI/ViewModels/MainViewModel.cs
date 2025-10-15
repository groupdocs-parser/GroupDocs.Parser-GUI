using GroupDocs.Parser.Data;
using GroupDocs.Parser.GUI.Utils;
using GroupDocs.Parser.Options;
using GroupDocs.Parser.Templates;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Size = GroupDocs.Parser.Data.Size;
using Point = GroupDocs.Parser.Data.Point;

namespace GroupDocs.Parser.GUI.ViewModels
{
    class MainViewModel : ViewModelBase, ISelectedFieldHost
    {
        private const int MaxLogItemCount = 1000;
        private const int PreviewDpi = 144;
        private readonly static int[] ocrDpis = new int[] { 72, 144, 216, 288, 360, 432, 504 };

        private int fieldCounter;

        private bool windowEnabled = true;
        private readonly ObservableCollection<LogItemViewModel> log = new ObservableCollection<LogItemViewModel>();
        private LogItemViewModel selectedLogItem;
        private double percentagePosition;
        private bool isOcrUsed;
        private int ocrDpi = 288;
        private readonly BoolWrapper genVisibility = new BoolWrapper(false);

        private readonly Settings settings;
        private readonly string version;

        private double scale = 1.0;

        private string filePath = string.Empty;
        private ObservableCollection<PageViewModel> pages = new ObservableCollection<PageViewModel>();
        private readonly ObservableCollection<IFieldViewModel> fields = new ObservableCollection<IFieldViewModel>();

        private IFieldViewModel selectedField;

        public RelayCommand SetLicenseCommand { get; private set; }
        public RelayCommand OpenFileCommand { get; private set; }
        public RelayCommand ZoomInCommand { get; private set; }
        public RelayCommand ZoomOutCommand { get; private set; }
        public RelayCommand AddTextFieldCommand { get; private set; }
        public RelayCommand AddTableFieldCommand { get; private set; }
        public RelayCommand AddBarcodeFieldCommand { get; private set; }
        public RelayCommand ParseFieldsCommand { get; private set; }
        public RelayCommand ParseDocumentCommand { get; private set; }
        public RelayCommand SaveTemplatesCommand { get; private set; }
        public RelayCommand LoadTemplatesCommand { get; private set; }
        public RelayCommand SaveResultsCommand { get; private set; }
        public RelayCommand ExportTemplatesCommand { get; private set; }
        public RelayCommand IncreaseDpiCommand { get; private set; }
        public RelayCommand DecreaseDpiCommand { get; private set; }
        public RelayCommand GenerateTemplateCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }

        public MainViewModel(Settings settings)
        {
            this.settings = settings;

            version = new Options.LoadOptions().GetType().Assembly.GetName().Version.ToString(3);

            SetLicenseCommand = new RelayCommand(OnSetLicense);
            OpenFileCommand = new RelayCommand(OnOpenFile);
            ZoomInCommand = new RelayCommand(OnZoomIn);
            ZoomOutCommand = new RelayCommand(OnZoomOut);
            AddTextFieldCommand = new RelayCommand(OnAddTextField);
            AddTableFieldCommand = new RelayCommand(OnAddTableField);
            AddBarcodeFieldCommand = new RelayCommand(OnAddBarcodeField);
            ParseFieldsCommand = new RelayCommand(OnParseFieldsAsync);
            ParseDocumentCommand = new RelayCommand(OnParseDocumentAsync);
            SaveTemplatesCommand = new RelayCommand(OnSaveTemplates);
            LoadTemplatesCommand = new RelayCommand(OnLoadTemplates);
            SaveResultsCommand = new RelayCommand(OnSaveResults);
            ExportTemplatesCommand = new RelayCommand(OnExportTemplates);
            IncreaseDpiCommand = new RelayCommand(OnIncreaseDpi);
            DecreaseDpiCommand = new RelayCommand(OnDecreaseDpi);
            GenerateTemplateCommand = new RelayCommand(OnGenerateTemplateAsync);
            DeleteCommand = new RelayCommand(OnDelete);

            Init();
        }

        public bool WindowEnabled
        {
            get => windowEnabled;
            set => UpdateProperty(ref windowEnabled, value);
        }

        public string Title => "GroupDocs.Parser " + version;

        public Settings Settings => settings;

        public bool IsOcrUsed
        {
            get => isOcrUsed;
            set => UpdateProperty(ref isOcrUsed, value);
        }

        public int OcrDpi
        {
            get => ocrDpi;
            set => UpdateProperty(ref ocrDpi, value);
        }

        public bool IsGeneratedFieldsVisible
        {
            get => genVisibility.Value;
            set
            {
                if (genVisibility.Value != value)
                {
                    genVisibility.SetValue(value);
                    NotifyPropertyChanged(nameof(IsGeneratedFieldsVisible));
                }
            }
        }

        public double Scale
        {
            get => scale;
            set
            {
                if (UpdateProperty(ref scale, value))
                {
                    NotifyPropertyChanged(nameof(ScaleText));
                    foreach (var page in pages)
                    {
                        page.Scale = scale;
                    }
                }
            }
        }

        public string ScaleText => Math.Round(scale * 100) + "%";

        public string FilePath
        {
            get => filePath;
            set => UpdateProperty(ref filePath, value);
        }

        public ObservableCollection<PageViewModel> Pages
        {
            get => pages;
            set => UpdateProperty(ref pages, value);
        }

        public ObservableCollection<IFieldViewModel> Fields => fields;

        public IFieldViewModel SelectedField
        {
            get => selectedField;
            set
            {
                if (selectedField != value)
                {
                    if (selectedField != null)
                    {
                        selectedField.IsSelected = false;
                    }
                    selectedField = value;
                    if (selectedField != null)
                    {
                        selectedField.IsSelected = true;
                    }
                    NotifyPropertyChanged(nameof(SelectedField));
                }
            }
        }

        public ObservableCollection<LogItemViewModel> Log => log;

        public LogItemViewModel SelectedLogItem
        {
            get => selectedLogItem;
            set => UpdateProperty(ref selectedLogItem, value);
        }

        public void SetPercentagePosition(double percentagePosition)
        {
            this.percentagePosition = percentagePosition;
        }

        public void MouseWheelCustom(int delta)
        {
            if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl) ||
                System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightCtrl))
            {
                if (delta < 0)
                {
                    OnZoomOut();
                }
                else
                {
                    OnZoomIn();
                }
            }
        }

        private async void Init()
        {
            var result = await SetLicense();
            if (!result)
            {
                AddLogEntry("License is not set");
            }
        }

        private async Task<bool> SetLicense()
        {
            WindowEnabled = false;
            var result = true;
            try
            {
                var licensePath = Settings.LicensePath;
                result = !string.IsNullOrWhiteSpace(licensePath);
                if (result)
                {
                    AddLogEntry("Setting a license: " + licensePath);
                    result = File.Exists(licensePath);
                    if (result)
                    {
                        await Task.Factory.StartNew(() =>
                        {
                            var license = new License();
                            license.SetLicense(licensePath);
                        });

                        AddLogEntry("The license is set");
                    }
                    else
                    {
                        AddLogEntry("The license file doesn't exist.");
                    }
                }
            }
            catch(Exception ex)
            {
                result = false;
                OnError(ex, "Setting License Error");
            }
            finally
            {
                WindowEnabled = true;
            }
            return result;
        }

        private async void OnSetLicense()
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog(App.Current.MainWindow) == true)
            {
                var path = openFileDialog.FileName;
                Settings.LicensePath = path;

                await SetLicense();
            }
        }

        private void OnOpenFile()
        {
            try
            {
                var dialog = new OpenFileDialog();
                dialog.Title = "Select a document";
                if (dialog.ShowDialog() == true)
                {
                    FilePath = dialog.FileName;
                    AddLogEntry("Opened a file: " + FilePath);
                    DetectOCR();
                    GeneratePreview();
                }
            }
            catch (Exception ex)
            {
                OnError(ex, "Opening File Error");
            }
        }

        private void OnZoomIn()
        {
            double newValue = Scale + 0.1;
            if (newValue > 3.0)
            {
                newValue = 3.0;
            }
            Scale = newValue;
        }

        private void OnZoomOut()
        {
            double newValue = Scale - 0.1;
            if (newValue < 0.1)
            {
                newValue = 0.1;
            }
            Scale = newValue;
        }

        private void OnAddTextField()
        {
            if (pages == null || pages.Count == 0)
            {
                return;
            }

            int pageIndex = GetCurrentPageIndex();
            if (pageIndex < 0)
            {
                return;
            }

            if (pageIndex >= pages.Count)
            {
                pageIndex = pages.Count - 1;
            }

            var page = pages[pageIndex];
            double position = percentagePosition * pages.Count % 1.0 * page.OriginalHeight;

            var fieldName = GetFieldName("Text");
            var field = new FieldViewModel(this, genVisibility, 10, position, 80, 40, Scale, fieldName, page.PageIndex, false);
            AddField(page, field);
        }

        private void OnAddTableField()
        {
            if (pages == null || pages.Count == 0)
            {
                return;
            }

            int pageIndex = GetCurrentPageIndex();
            if (pageIndex < 0)
            {
                return;
            }

            if (pageIndex >= pages.Count)
            {
                pageIndex = pages.Count - 1;
            }

            var page = pages[pageIndex];
            double position = percentagePosition * pages.Count % 1.0 * page.OriginalHeight;

            var fieldName = GetFieldName("Table");
            var field = new TableViewModel(this, 10, position, 80, 40, Scale, fieldName, page.PageIndex);
            AddField(page, field);
        }

        private void OnAddBarcodeField()
        {
            if (pages == null || pages.Count == 0)
            {
                return;
            }

            int pageIndex = GetCurrentPageIndex();
            if (pageIndex < 0)
            {
                return;
            }

            if (pageIndex >= pages.Count)
            {
                pageIndex = pages.Count - 1;
            }

            var page = pages[pageIndex];
            double position = percentagePosition * pages.Count % 1.0 * page.OriginalHeight;

            var fieldName = GetFieldName("Barcode");
            var field = new BarcodeViewModel(this, 10, position, 80, 40, Scale, fieldName, page.PageIndex);
            AddField(page, field);
        }

        private int GetCurrentPageIndex()
        {
            int pageIndex = (int)(percentagePosition * pages.Count);
            return pageIndex;
        }

        private string GetFieldName(string prefix)
        {
            while (true)
            {
                fieldCounter++;
                int fieldNumber = fieldCounter;
                var fieldName = prefix + fieldNumber.ToString(CultureInfo.InvariantCulture);
                if (fields.All(f => f.Name != fieldName))
                {
                    return fieldName;
                }
            }
        }

        private void AddField(PageViewModel page, IFieldViewModel field)
        {
            page.Objects.Add((IPageElement)field);
            fields.Add(field);
        }

        private async void OnParseFieldsAsync()
        {
            WindowEnabled = false;
            try
            {
                AddLogEntry("Started parsing by template.");
                Task task = Task.Factory.StartNew(() =>
                {
                    if (string.IsNullOrEmpty(FilePath))
                    {
                        return;
                    }

                    Template template = GetTemplate();
                    using (Parser parser = new Parser(FilePath))
                    {
                        int pageIndex = GetCurrentPageIndex();
                        var options = new ParseByTemplateOptions(
                            pageIndex,
                            IsOcrUsed,
                            new OcrOptions(new PagePreviewOptions(OcrDpi)));
                        DocumentData data = parser.ParseByTemplate(template, options);

                        ClearParsedText();
                        for (int i = 0; i < data.Count; i++)
                        {
                            var fieldData = data[i];
                            SetParsedText(fieldData);
                            AddLogEntry(fieldData.Name + ": " + fieldData.Text);
                        }
                    }
                });
                await task;
                AddLogEntry("Parsing by template is completed.");
            }
            catch (Exception ex)
            {
                OnError(ex, "Parsing Fields Error");
            }

            finally
            {
                WindowEnabled = true;
            }
        }

        private Template GetTemplate()
        {
            var templateItems = new List<TemplateItem>();
            foreach (var page in Pages)
            {
                foreach (var pageElement in page.Objects)
                {
                    switch (pageElement.ElementType)
                    {
                        case PageElementType.TextField:
                            var templateField = new TemplateField(
                                new TemplateFixedPosition(
                                    new Rectangle(
                                        new Point(
                                            pageElement.OriginalX,
                                            pageElement.OriginalY),
                                        new Size(
                                            pageElement.OriginalWidth,
                                            pageElement.OriginalHeight))),
                                pageElement.Name,
                                page.OriginalWidth,
                                false);
                            var fvm = pageElement as FieldViewModel;
                            templateField.IsHidden = fvm.IsHidden;
                            templateField.Value = fvm.Text;
                            templateItems.Add(templateField);
                            break;
                        case PageElementType.TableField:
                            var table = (TableViewModel)pageElement;
                            double left = pageElement.OriginalX;
                            double right = left + pageElement.OriginalWidth;
                            var verticalSeparators = new double[] { left }
                                .Concat(table.Separators.Select(svm => svm.OriginalPosition + left))
                                .Append(right)
                                .ToArray();
                            double top = pageElement.OriginalY;
                            double bottom = top + pageElement.OriginalHeight;
                            var horizontalSeparators = new double[] { top, bottom, };
                            var templateTable = new TemplateTable(
                                new TemplateTableLayout(
                                    verticalSeparators,
                                    horizontalSeparators),
                                pageElement.Name,
                                page.OriginalWidth,
                                false);
                            templateItems.Add(templateTable);
                            break;
                        case PageElementType.BarcodeField:
                            var templateBarcode = new TemplateBarcode(
                                new Rectangle(
                                    new Point(
                                        pageElement.OriginalX,
                                        pageElement.OriginalY),
                                    new Size(
                                        pageElement.OriginalWidth,
                                        pageElement.OriginalHeight)),
                                pageElement.Name,
                                page.OriginalWidth,
                                false);
                            templateItems.Add(templateBarcode);
                            break;
                    }
                }
            }
            Template template = new Template(templateItems);
            return template;
        }

        private void ClearParsedText()
        {
            Action action = () =>
            {
                foreach (var field in Fields)
                {
                    if (!field.IsHidden)
                    {
                        field.Text = string.Empty;
                    }
                }
            };
            CallOnUIThreadIfNeeded(action);
        }

        private void SetParsedText(FieldData fieldData)
        {
            Action action = () =>
            {
                foreach (var field in Fields)
                {
                    if (field.Name == fieldData.Name &&
                        (field.PageIndex < 0 || field.PageIndex == fieldData.PageIndex))
                    {
                        var pageArea = fieldData.PageArea;
                        if (pageArea is PageTextArea)
                        {
                            var pageTextArea = (PageTextArea)pageArea;
                            field.Text = pageTextArea.Text;
                        }
                        else if (pageArea is PageTableArea)
                        {
                            var pageTableArea = (PageTableArea)pageArea;
                            field.Text = string.Join('\t', pageTableArea.Cells.Select(c => c.Text));
                        }
                        else if (pageArea is PageBarcodeArea)
                        {
                            var pageBarcodeArea = (PageBarcodeArea)pageArea;
                            field.Text = pageBarcodeArea.Value;
                        }
                        break;
                    }
                }
            };
            CallOnUIThreadIfNeeded(action);
        }

        private static void CallOnUIThreadIfNeeded(Action action)
        {
            var dispatcher = System.Windows.Application.Current.Dispatcher;
            if (dispatcher.CheckAccess())
            {
                action.Invoke();
            }
            else
            {
                dispatcher.Invoke(action);
            }
        }

        private async void OnParseDocumentAsync()
        {
            WindowEnabled = false;
            try
            {
                AddLogEntry("Started parsing the document.");
                Task task = Task.Factory.StartNew(() =>
                {
                    if (string.IsNullOrEmpty(FilePath))
                    {
                        return;
                    }

                    var dialog = new SaveFileDialog();
                    dialog.FileName = "Document";
                    dialog.DefaultExt = ".txt";
                    dialog.Filter = "Text documents (.txt)|*.txt";

                    var result = dialog.ShowDialog();
                    if (result == true)
                    {
                        string filePath = dialog.FileName;

                        using (Parser parser = new Parser(FilePath))
                        {
                            var options = new TextOptions(false, IsOcrUsed);
                            var reader = parser.GetText(options);
                            using (var writer = File.CreateText(filePath))
                            {
                                while (true)
                                {
                                    string line = reader.ReadLine();
                                    if (line == null)
                                    {
                                        break;
                                    }

                                    writer.WriteLine(line);
                                }
                            }
                        }
                    }
                });
                await task;
                AddLogEntry("Parsing the document is completed.");
            }
            catch (Exception ex)
            {
                OnError(ex, "On Parsing Document");
            }
            finally
            {
                WindowEnabled = true;
            }
        }

        private void OnSaveTemplates()
        {
            Template template = GetTemplate();

            var dialog = new SaveFileDialog();
            dialog.FileName = "Templates";
            dialog.DefaultExt = ".xml";
            dialog.Filter = "Templates (.xml)|*.xml";

            var result = dialog.ShowDialog();
            if (result == true)
            {
                AddLogEntry("Saved a file: " + dialog.FileName);
                template.Save(dialog.FileName);
            }
        }

        private void OnLoadTemplates()
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Select template file";
            if (dialog.ShowDialog() == true)
            {
                AddLogEntry("Opened a file: " + dialog.FileName);
                ClearTemplate();
                Template template = Template.Load(dialog.FileName);
                int pageIndex = GetCurrentPageIndex();
                ApplayTemplate(template, pageIndex);
            }
        }

        private void OnSaveResults()
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = "Results";
            dialog.DefaultExt = ".xml";
            dialog.Filter = "Results (.xml)|*.xml";

            var result = dialog.ShowDialog();
            if (result == true)
            {
                AddLogEntry("Saved a file: " + dialog.FileName);
                SaveParsingResults(dialog.FileName);
            }
        }

        private void OnExportTemplates()
        {
            Template template = GetTemplate();

            var dialog = new SaveFileDialog();
            dialog.FileName = String.IsNullOrEmpty(FilePath) ? "Templates" : Path.GetFileNameWithoutExtension(FilePath) + ".template";
            dialog.DefaultExt = ".xml";
            dialog.Filter = "Templates (.xml)|*.xml";

            var result = dialog.ShowDialog();
            if (result == true)
            {
                AddLogEntry("Saved a file: " + dialog.FileName);
                template.Save(dialog.FileName);
            }
        }

        private void OnIncreaseDpi()
        {
            int index = Array.IndexOf(ocrDpis, OcrDpi);
            int newIndex = index + 1;
            if (newIndex < ocrDpis.Length)
            {
                OcrDpi = ocrDpis[newIndex];
            }
        }

        private void OnDecreaseDpi()
        {
            int index = Array.IndexOf(ocrDpis, OcrDpi);
            if (index > 0)
            {
                int newIndex = index - 1;
                OcrDpi = ocrDpis[newIndex];
            }
        }

        private async void OnGenerateTemplateAsync()
        {
            WindowEnabled = false;
            try
            {
                AddLogEntry("Started generating template.");
                Task task = Task.Factory.StartNew(() =>
                {
                    if (string.IsNullOrEmpty(FilePath))
                    {
                        return;
                    }

                    CallOnUIThreadIfNeeded(() => ClearTemplate());
                    using (Parser parser = new Parser(FilePath))
                    {
                        int pageIndex = GetCurrentPageIndex();
                        AddLogEntry("Generating template for page: " + pageIndex);

                        var options = new AdjustmentFieldsOptions()
                        {
                            PageIndex = pageIndex,
                            OcrOptions = new OcrOptions(new PagePreviewOptions(OcrDpi)),
                        };
                        var adjustmentFields = parser.GenerateAdjustmentFields(options);
                        Template template = new Template(adjustmentFields);
                        AddLogEntry("Generated template for page: " + pageIndex);

                        CallOnUIThreadIfNeeded(() => ApplayTemplate(template, pageIndex));
                    }
                });
                await task;
                AddLogEntry("Generating template is completed.");
            }
            catch (Exception ex)
            {
                OnError(ex, "Generating Template Async");
            }
            finally
            {
                WindowEnabled = true;
            }
        }

        private void OnDelete()
        {
            var selected = SelectedField;
            if (selected != null)
            {
                Remove(selected);
            }
        }

        private void SaveParsingResults(string fileName)
        {
            XElement fieldsElement = new XElement("fields");
            foreach (var field in fields)
            {
                XElement fieldElement = new XElement("field", field.Text);
                XAttribute nameAttr = new XAttribute("name", field.Name);
                fieldElement.Add(nameAttr);
                fieldsElement.Add(fieldElement);
            }
            XDocument xdoc = new XDocument();
            xdoc.Add(fieldsElement);
            xdoc.Save(fileName);
        }

        private void ApplayTemplate(Template template, int pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= pages.Count)
            {
                return;
            }

            var page = pages[pageIndex];
            foreach (TemplateItem item in template)
            {
                double factor = page.OriginalWidth / item.PageWidth.Value;
                TemplateItem templateItem = item.Scale(factor);
                if (templateItem is TemplateField)
                {
                    var templateField = (TemplateField)templateItem;
                    var position = templateField.Position as TemplateFixedPosition;
                    var field = new FieldViewModel(
                        this,
                        genVisibility,
                        position.Rectangle.Left,
                        position.Rectangle.Top,
                        position.Rectangle.Size.Width,
                        position.Rectangle.Size.Height,
                        Scale,
                        templateField.Name,
                        pageIndex,
                        templateField.IsHidden);
                    if (templateField.IsHidden)
                    {
                        field.Text = templateField.Value;
                    }

                    AddField(page, field);
                }
                else if (templateItem is TemplateTable)
                {
                    var templateTable = (TemplateTable)templateItem;
                    var layout = templateTable.Layout;
                    var table = new TableViewModel(
                        this,
                        layout.Rectangle.Left,
                        layout.Rectangle.Top,
                        layout.Rectangle.Size.Width,
                        layout.Rectangle.Size.Height,
                        Scale,
                        templateTable.Name,
                        pageIndex);
                    foreach (var position in layout.VerticalSeparators.Skip(1).SkipLast(1))
                    {
                        var separator = new SeparatorViewModel(table, position - layout.Rectangle.Left, Scale);
                        table.Separators.Add(separator);
                    }

                    AddField(page, table);
                }
                else if (templateItem is TemplateBarcode)
                {
                    var templateBarcode = (TemplateBarcode)templateItem;
                    var barcode = new BarcodeViewModel(
                        this,
                        templateBarcode.Rectangle.Left,
                        templateBarcode.Rectangle.Top,
                        templateBarcode.Rectangle.Size.Width,
                        templateBarcode.Rectangle.Size.Height,
                        Scale,
                        templateBarcode.Name,
                        pageIndex);

                    AddField(page, barcode);
                }
            }
        }

        private void ClearTemplate()
        {
            Fields.Clear();
            SelectedField = null;
            foreach (var page in pages)
            {
                for (int i = page.Objects.Count - 1; i >= 0; i--)
                {
                    var obj = page.Objects[i];
                    if (obj is IFieldViewModel)
                    {
                        page.Objects.RemoveAt(i);
                    }
                }
            }
        }

        private void DetectOCR()
        {
            AddLogEntry("Detecting OCR.");
            IsOcrUsed = false;
            using (Parser parser = new Parser(FilePath))
            {
                var images = parser.GetImages(0);
                if (images != null)
                {
                    var firstImage = images.FirstOrDefault();
                    if (images.Count() == 1 && firstImage != null)
                    {
                        using (var img = System.Drawing.Image.FromStream(firstImage.GetImageStream()))
                        {
                            if (img.Height > 500)
                            {
                                IsOcrUsed = true;
                            }
                        }
                    }
                }
            }
        }

        private void GeneratePreview()
        {
            AddLogEntry("Started generating preview.");

            if (string.IsNullOrEmpty(FilePath))
            {
                return;
            }

            using (Parser parser = new Parser(FilePath))
            {
                var info = parser.GetDocumentInfo();
                PagePreviewOptions options = new PagePreviewOptions(PagePreviewFormat.Png, PreviewDpi);
                Clear();
                for (int pageIndex = 0; pageIndex < info.PageCount; pageIndex++)
                {
                    var stream = parser.GetPagePreview(pageIndex, options);
                    if (stream != null)
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = stream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        AddLogEntry($"Page {pageIndex}: Width={bitmap.Width:F2}, Height={bitmap.Height:F2}");

                        var page = new PageViewModel(pageIndex, bitmap, Scale);
                        Pages.Add(page);
                    }
                }
            }

            AddLogEntry("Generating preview is completed.");
        }

        private void Clear()
        {
            Pages.Clear();
            Fields.Clear();
            SelectedField = null;
        }

        public void Remove(IFieldViewModel field)
        {
            if (SelectedField == field)
            {
                SelectedField = null;
            }
            foreach (var page in pages)
            {
                page.Objects.Remove((IPageElement)field);
            }
            fields.Remove(field);
        }


        #region Working with logging
        public void AddLogEntry(DateTime time, string message)
        {
            var item = new LogItemViewModel(time, message);
            AddLogEntryPrivate(item);
            SelectedLogItem = item;
        }

        public void AddLogEntry(string message)
        {
            AddLogEntry(DateTime.Now, message);
        }

        private void AddLogEntryPrivate(LogItemViewModel item)
        {
            Action action = () =>
            {
                while (Log.Count >= MaxLogItemCount)
                {
                    Log.RemoveAt(0);
                }
                // Clear is a quick fix due to automatic resize issue
                //Log.Clear();

                Log.Add(item);
            };
            CallOnUIThreadIfNeeded(action);
        }

        private void OnError(Exception ex, string message)
        {
            var line = ex.InnerException != null ? $"{ex.Message}({ex.InnerException.Message})" : ex.Message;
            line = $"{message} : {line}";
            AddLogEntry(line);
            MessageBox.Show(line, "Error", MessageBoxButton.OK);
        }
        #endregion
    }
}
