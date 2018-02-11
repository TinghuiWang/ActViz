using ActViz.Dialogs;
using ActViz.Helpers;
using ActViz.Models;
using ActViz.Services;
using ActViz.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ActViz.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DatasetEventsPage : Page
    {
        DatasetEventsViewModel _viewModel;

        // Display Settings
        private double sensorIconWidth;
        private double sensorIconHeight;
        private double sensorTextWidth;
        private double sensorTextHeight;
        // Data Structure for sensor display
        private List<Tuple<SensorViewModel, Rectangle, Viewbox>> canvasSensorList =
            new List<Tuple<SensorViewModel, Rectangle, Viewbox>>();
        // Flyout menu for Tag resident/activities
        MenuFlyout annotateFlyout = new MenuFlyout();
        private List<Line> residentPathLineList = new List<Line>();

        // Suspending Handler
        private SuspendingEventHandler _appSuspendHandler;

        public DatasetEventsPage()
        {
            this.InitializeComponent();
            sensorIconWidth = AppSettingsService.RetrieveFromSettings<double>("sensorIconWidth", 10);
            sensorIconHeight = AppSettingsService.RetrieveFromSettings<double>("sensorIconHeight", 10);
            sensorTextWidth = AppSettingsService.RetrieveFromSettings<double>("sensorTextWidth", 60);
            sensorTextHeight = AppSettingsService.RetrieveFromSettings<double>("sensorTextHeight", 20);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _viewModel = new DatasetEventsViewModel(e.Parameter as Dataset);
            _appSuspendHandler = new SuspendingEventHandler(App_Suspending);
            Application.Current.Suspending += _appSuspendHandler;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Application.Current.Suspending -= _appSuspendHandler;
            base.OnNavigatedFrom(e);
        }

        private void App_Suspending(object sender, SuspendingEventArgs e)
        {
            AppSettingsService.AddToSettings<string>("WorkingDataset", _viewModel.Dataset.Name);
            ApplicationDataCompositeValue datasetLastSavedStates =
                AppSettingsService.RetrieveFromSettings<ApplicationDataCompositeValue>(
                    "DatasetLastSavedStates", new ApplicationDataCompositeValue()
                    );
            datasetLastSavedStates[_viewModel.Dataset.Name] = JsonConvert.SerializeObject(new DatasetViewSavedState
            {
                Day = _viewModel.CurrentDate,
                EventInView = dataListView.SelectedIndex,
                Filter = _viewModel.EventViewFilter
            });
            AppSettingsService.AddToSettings<ApplicationDataCompositeValue>("DatasetLastSavedStates", datasetLastSavedStates);
        }

        private async void Page_LoadedAsync(object sender, RoutedEventArgs e)
        {
            PageBusy("Loading events from dataset " + _viewModel.Dataset.Name + " ...");
            await _viewModel.LoadDataAsync();
            PopulateSensors();
            DrawSensors();
            populateAnnotateFlyout();
            // Check Application Settings for last saved states. If the state exists and is valid, load the state
            ApplicationDataCompositeValue datasetLastSavedStates =
                AppSettingsService.RetrieveFromSettings<ApplicationDataCompositeValue>(
                    "DatasetLastSavedStates", new ApplicationDataCompositeValue()
                    );
            if(datasetLastSavedStates.TryGetValue(_viewModel.Dataset.Name, out object jsonDatasetSavedState))
            {
                DatasetViewSavedState datasetState = JsonConvert.DeserializeObject<DatasetViewSavedState>((string)jsonDatasetSavedState);
                if(datasetState.Day <= _viewModel.LastEventDate && datasetState.Day >= _viewModel.FirstEventDate)
                {
                    await _viewModel.LoadEventsAsync(datasetState.Day);
                }
                _viewModel.EventViewFilter = datasetState.Filter;
                _viewModel.RefreshEventsInView();
                if(datasetState.EventInView >= 0 && datasetState.EventInView < _viewModel.EventsInView.Count)
                {
                    _viewModel.SelectedSensorEvent = (SensorEventViewModel) dataListView.Items[datasetState.EventInView];
                }
                dataListView.ScrollIntoView(_viewModel.SelectedSensorEvent);
            }
            PageReady();
        }

        private void sensorCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawSensors();
        }

        private void floorplanImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawSensors();
        }

        #region Flyout
        private void populateAnnotateFlyout()
        {
            MenuFlyoutSubItem ActivityMenu = new MenuFlyoutSubItem();
            ActivityMenu.Text = "Tag Activity";
            foreach (ActivityViewModel activity in _viewModel.Activities)
            {
                MenuFlyoutItem curMenuItem = new MenuFlyoutItem
                {
                    Text = activity.Name,
                    Foreground = new SolidColorBrush(activity.Color),
                    Tag = activity.Name
                };
                curMenuItem.Click += TagActivity_Click;
                ActivityMenu.Items.Add(curMenuItem);
            }
            annotateFlyout.Items.Add(ActivityMenu);
            MenuFlyoutSubItem ResidentMenu = new MenuFlyoutSubItem();
            ResidentMenu.Text = "Tag Resident";
            foreach (ResidentViewModel resident in _viewModel.Residents)
            {
                MenuFlyoutItem curMenuItem = new MenuFlyoutItem
                {
                    Text = resident.Name,
                    Tag = resident.Name,
                    Foreground = new SolidColorBrush(resident.Color)
                };
                curMenuItem.Click += TagResident_Click;
                ResidentMenu.Items.Add(curMenuItem);
            }
            annotateFlyout.Items.Add(ResidentMenu);
            annotateFlyout.Items.Add(new MenuFlyoutSeparator());
            MenuFlyoutItem menuUntagActivity = new MenuFlyoutItem();
            menuUntagActivity.Text = "Untag Activity";
            menuUntagActivity.Click += TagActivity_Click;
            menuUntagActivity.Tag = "";
            annotateFlyout.Items.Add(menuUntagActivity);
            MenuFlyoutItem menuUntagResident = new MenuFlyoutItem();
            menuUntagResident.Text = "Untag Resident";
            menuUntagResident.Click += TagResident_Click;
            menuUntagResident.Tag = "";
            annotateFlyout.Items.Add(menuUntagResident);
        }

        internal async Task ClosePageAsync()
        {
            if (_viewModel.IsEventsModified)
            {
                MessageDialog dlg = new MessageDialog("Dataset " + _viewModel.Dataset.Name + " has changed. Do you want to save the change back to disk?", "Save Events");
                dlg.Commands.Add(new UICommand("Save"));
                dlg.Commands.Add(new UICommand("Cancel"));
                IUICommand selectedCmd = await dlg.ShowAsync();
                if (selectedCmd.Label == "Save")
                {
                    PageBusy("Save events to dataset " + _viewModel.Dataset.Name);
                    await _viewModel.SaveEventsAsync();
                    PageReady();
                }
            }
        }

        private void TagResident_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem flyoutMenuItem = (MenuFlyoutItem)sender;
            List<SensorEventViewModel> selectedEvents = new List<SensorEventViewModel>();
            foreach (SensorEventViewModel sensorEvent in dataListView.SelectedItems)
                selectedEvents.Add(sensorEvent);
            _viewModel.TagResident((string)flyoutMenuItem.Tag, selectedEvents);
        }

        private void TagActivity_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem flyoutMenuItem = (MenuFlyoutItem)sender;
            List<SensorEventViewModel> selectedEvents = new List<SensorEventViewModel>();
            foreach (SensorEventViewModel sensorEvent in dataListView.SelectedItems)
                selectedEvents.Add(sensorEvent);
            _viewModel.TagActivity((string)flyoutMenuItem.Tag, selectedEvents);
        }
        #endregion

        #region CanvasDrawing
        private void DrawSensors()
        {
            double totalX = floorplanImage.ActualWidth;
            double totalY = floorplanImage.ActualHeight;
            foreach (Tuple<SensorViewModel, Rectangle, Viewbox> sensorTuple in canvasSensorList)
            {
                SensorViewModel sensor = sensorTuple.Item1;
                Rectangle sensorRect = sensorTuple.Item2;
                Viewbox sensorViewBox = sensorTuple.Item3;
                // Check if we hide the sensor or not
                // Update only those who are visible
                double startPosX = (sensorCanvas.ActualWidth - totalX) / 2 + ((Sensor)sensor).LocX * totalX;
                double startPosY = (sensorCanvas.ActualHeight - totalY) / 2 + ((Sensor)sensor).LocY * totalY;
                Canvas.SetTop(sensorRect, startPosY);
                Canvas.SetLeft(sensorRect, startPosX);
                Canvas.SetTop(sensorViewBox, startPosY + sensorIconHeight + 2);
                Canvas.SetLeft(sensorViewBox, startPosX - sensorTextWidth / 2 + sensorIconWidth / 2);
            }
            DrawResidentsPath();
            sensorCanvas.InvalidateArrange();
            sensorCanvas.UpdateLayout();
        }

        private void PopulateSensors()
        {
            foreach (SensorViewModel sensorViewModel in _viewModel.ActiveSensors)
            {
                AddSensorToCanvas(sensorViewModel);
            }
        }

        private void AddSensorToCanvas(SensorViewModel sensorViewModel)
        {
            Sensor sensor = sensorViewModel;
            double totalX = floorplanImage.ActualWidth;
            double totalY = floorplanImage.ActualHeight;
            // Draw Rectangle
            Rectangle sensorRect = new Rectangle
            {
                //Width = sensor.SizeX * totalX,
                //Height = sensor.SizeY * totalY,
                Width = sensorIconWidth,
                Height = sensorIconHeight,
                Fill = new SolidColorBrush(SensorType.GetBestColorForSensor(sensorViewModel)),
                Stroke = new SolidColorBrush(Colors.DimGray),
                StrokeThickness = 1,
                StrokeDashCap = PenLineCap.Round,
                Opacity = 0.1,
            };
            Canvas.SetZIndex(sensorRect, 0);
            // Draw Text
            TextBlock sensorText = new TextBlock
            {
                //Foreground = new SolidColorBrush(SensorType.GetBestColorForSensor(sensorViewModel)),
                Foreground = new SolidColorBrush(Colors.Black),
                Text = sensor.Name,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
            };
            Color bgColor = Colors.LightGray;
            bgColor.A = 0xA0;
            Border sensorTextBorder = new Border
            {
                Background = new SolidColorBrush(bgColor),
                Child = sensorText,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Viewbox sensorViewbox = new Viewbox
            {
                //Height = sensorRect.Height,
                //Width = sensorRect.Width,
                Height = sensorTextHeight,
                Width = sensorTextWidth,
                Stretch = Stretch.Uniform,
                Child = sensorTextBorder,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Canvas.SetZIndex(sensorViewbox, 0);
            // Populate Sensor List
            canvasSensorList.Add(new Tuple<SensorViewModel, Rectangle, Viewbox>(
                sensorViewModel, sensorRect, sensorViewbox));
            sensorCanvas.Children.Add(sensorRect);
            sensorCanvas.Children.Add(sensorViewbox);
        }
        #endregion


        private async void btnPrevWeek_ClickAsync(object sender, RoutedEventArgs e)
        {
            DateTimeOffset date = _viewModel.CurrentDate.AddDays(-7);
            if (date >= _viewModel.FirstEventDate)
            {
                PageBusy("Load Date " + date.ToString("G") + " ...");
                await Task.Factory.StartNew(async () => { await _viewModel.LoadEventsAsync(date); });
                PageReady();
            }
        }

        private async void btnPrevDay_ClickAsync(object sender, RoutedEventArgs e)
        {
            DateTimeOffset date = _viewModel.CurrentDate.AddDays(-1);
            if (date >= _viewModel.FirstEventDate)
            {
                PageBusy("Load Date " + date.ToString("G") + " ...");
                await Task.Factory.StartNew(async () => { await _viewModel.LoadEventsAsync(date); });
                PageReady();
            }
        }

        private void DateSlider_ValueChangeCompleted(object sender, object args)
        {
        }

        private async void btnNextDay_ClickAsync(object sender, RoutedEventArgs e)
        {
            DateTimeOffset date = _viewModel.CurrentDate.AddDays(1);
            if (date <= _viewModel.LastEventDate)
            {
                PageBusy("Load Date " + date.ToString("G") + " ...");
                await Task.Factory.StartNew(async () => { await _viewModel.LoadEventsAsync(date); });
                PageReady();
            }
        }

        private async void btnNextWeek_ClickAsync(object sender, RoutedEventArgs e)
        {
            DateTimeOffset date = _viewModel.CurrentDate.AddDays(7);
            if (date <= _viewModel.LastEventDate)
            {
                PageBusy("Load Date " + date.ToString("G") + " ...");
                await Task.Factory.StartNew(async () => { await _viewModel.LoadEventsAsync(date); });
                PageReady();
            }
        }

        private async void EventDatePicker_DateChangedAsync(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (args.NewDate != null)
            {
                DateTimeOffset date = args.NewDate.Value.Date;
                PageBusy("Load Date " + date.ToString("G") + " ...");
                await Task.Factory.StartNew(async () => { await _viewModel.LoadEventsAsync(date); });
                PageReady();
            }
        }

        private void dataListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int maxTime = 3600 * 2; // 2 hours
            int maxSensor = 10;      // Maximum 6 sensor drawing
            int timeElapse = 0;
            int maxTimeElapse = 0;
            int sensorDrawn = 0;
            _viewModel.UpdateSensorFireStatus();
            // Clear Drawing
            foreach (Tuple<SensorViewModel, Rectangle, Viewbox> canvasSensorEntry in canvasSensorList)
            {
                canvasSensorEntry.Item2.Opacity = 0.1;
            }
            // Update Each Sensor Block
            List<SensorPastInfo> sensorPastInfoList = _viewModel.GetPastSensorInfo();
            maxTimeElapse = Convert.ToInt32(((maxSensor <= sensorPastInfoList.Count) ? sensorPastInfoList[maxSensor - 1] : sensorPastInfoList[sensorPastInfoList.Count - 1]).LastFireElapse.TotalSeconds) + 1;
            if (_viewModel.EventsInView.Count > 0)
            {
                // Find the total timeElapse
                foreach (SensorPastInfo sensorPastInfo in sensorPastInfoList)
                {
                    // If the sensor is not fired in the day yet, exit.
                    if (!sensorPastInfo.IsValid) break;
                    // Already drawn enough sensor in the app.
                    if (sensorDrawn > maxSensor) break;
                    // Find time elapse of current entry
                    timeElapse = Convert.ToInt32(sensorPastInfo.LastFireElapse.TotalSeconds);
                    if (timeElapse > maxTime) break;
                    // Find Tuple in List
                    Tuple<SensorViewModel, Rectangle, Viewbox> canvasSensorEntry = 
                        canvasSensorList.Find(x => x.Item1.Name == sensorPastInfo.Name);
                    if (canvasSensorEntry != null)
                    {
                        Color sensorTypeColor = SensorType.GetColorFromSensorType(canvasSensorEntry.Item1.Types[0]);
                        canvasSensorEntry.Item2.Fill = new SolidColorBrush(sensorTypeColor);
                        canvasSensorEntry.Item2.Opacity = (1 - ((double)timeElapse) / maxTimeElapse) * 0.6 + 0.5;
                    }
                    // Add Sensor Count
                    sensorDrawn++;
                }
            }
            DrawResidentsPath();
            sensorCanvas.InvalidateArrange();
            sensorCanvas.UpdateLayout();
        }

        private void DrawResidentsPath()
        {
            ClearResidentsPath();
            DrawResidentsPath(_viewModel.GetPastStepsOfResidents());
        }

        private void DrawResidentsPath(Dictionary<ResidentViewModel, List<SensorEventViewModel>> pastStepsOfResidents)
        {
            double totalX = floorplanImage.ActualWidth;
            double totalY = floorplanImage.ActualHeight;
            foreach (KeyValuePair<ResidentViewModel, List<SensorEventViewModel>> entry in pastStepsOfResidents)
            {
                if (entry.Value.Count <= 1) continue;
                for (int i = 0; i < entry.Value.Count - 1; i++)
                {
                    Sensor startSensor = entry.Value[i + 1].Sensor;
                    Sensor stopSensor = entry.Value[i].Sensor;
                    if (startSensor.LocX == stopSensor.LocX && startSensor.LocY == stopSensor.LocY) continue;
                    Line line = new Line();
                    if (startSensor.LocX > stopSensor.LocX)
                    {
                        line.X1 = (sensorCanvas.ActualWidth - totalX) / 2 + (startSensor.LocX) * totalX;
                        line.X2 = (sensorCanvas.ActualWidth - totalX) / 2 + (stopSensor.LocX) * totalX + sensorIconWidth;
                    }
                    else
                    {
                        line.X1 = (sensorCanvas.ActualWidth - totalX) / 2 + (startSensor.LocX) * totalX + sensorIconWidth;
                        line.X2 = (sensorCanvas.ActualWidth - totalX) / 2 + (stopSensor.LocX) * totalX;
                    }
                    if (startSensor.LocY > stopSensor.LocY)
                    {
                        line.Y1 = (sensorCanvas.ActualHeight - totalY) / 2 + (startSensor.LocY) * totalY;
                        line.Y2 = (sensorCanvas.ActualHeight - totalY) / 2 + (stopSensor.LocY) * totalY + sensorIconHeight;
                    }
                    else
                    {
                        line.Y1 = (sensorCanvas.ActualHeight - totalY) / 2 + (startSensor.LocY) * totalY + sensorIconHeight;
                        line.Y2 = (sensorCanvas.ActualHeight - totalY) / 2 + (stopSensor.LocY) * totalY;
                    }
                    double opacity = (i < 20 && i >= 0) ? 1 - 0.05 * i : 0;
                    line.Stroke = new SolidColorBrush(entry.Key.Color);
                    line.StrokeThickness = 2;
                    line.Opacity = opacity;
                    line.StrokeEndLineCap = PenLineCap.Triangle;
                    sensorCanvas.Children.Add(line);
                    residentPathLineList.Add(line);
                }
            }
        }

        private void ClearResidentsPath()
        {
            foreach (Line line in residentPathLineList)
            {
                sensorCanvas.Children.Remove(line);
            }
            residentPathLineList.Clear();
        }

        private void dataListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            annotateFlyout.ShowAt(sender as ListView, e.GetPosition(sender as ListView));
        }

        private async void menuFilter_ClickAsync(object sender, RoutedEventArgs e)
        {
            DatasetEventFilterDialog dialog = new DatasetEventFilterDialog(_viewModel.EventViewFilter, _viewModel.Residents, _viewModel.Activities);
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
                _viewModel.RefreshEventsInView();
        }

        private void menuConfig_ClickAsync(object sender, RoutedEventArgs e)
        {

        }

        private void PageBusy(string message)
        {
            ((Window.Current.Content as Frame).Content as MainPage).PageBusy(message);
        }

        private void PageReady()
        {
            ((Window.Current.Content as Frame).Content as MainPage).PageReady();
        }

        private async void BtnSaveDataset_ClickAsync(object sender, RoutedEventArgs e)
        {
            PageBusy("Save events to dataset " + _viewModel.Dataset.Name);
            await _viewModel.SaveEventsAsync();
            _viewModel.IsEventsModified = false;
            PageReady();
        }

        private async void MenuExport_ClickAsync(object sender, RoutedEventArgs e)
        {
            // Check and see if there are unsaved changes
            if(_viewModel.IsEventsModified)
            {
                await ClosePageAsync();
            }
            DatasetExportDialog dialog = new DatasetExportDialog(_viewModel);
            var result = await dialog.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                StorageFolder folder = dialog.ExportFolder;
                DatasetExportViewModel datasetExportConfig = dialog.DatasetExportConfiguration;
                if(datasetExportConfig.ExportInCSV)
                {
                    // Dataset exported in csv events, where metadata about the dataset is stored in json file.
                    StorageFolder targetDatasetFolder;
                    try
                    {
                        targetDatasetFolder = await folder.GetFolderAsync(datasetExportConfig.DatasetName);
                        string message = "A folder named " + datasetExportConfig.DatasetName + " already existed at " + folder.Path + ". Do you want to overwrite?";
                        var dlg = new MessageDialog(message, "Overwrite Folder?");
                        UICommand yesCmd = new UICommand("Yes");
                        UICommand cancelCmd = new UICommand("Cancel");
                        dlg.Commands.Add(yesCmd);
                        dlg.Commands.Add(cancelCmd);
                        dlg.DefaultCommandIndex = 1;
                        dlg.CancelCommandIndex = 1;
                        var cmd = await dlg.ShowAsync();
                        if (cmd == cancelCmd) return;
                    }
                    catch (Exception)
                    { }
                    targetDatasetFolder = await folder.CreateFolderAsync(datasetExportConfig.DatasetName, CreationCollisionOption.ReplaceExisting);
                    PageBusy(string.Format("Exporting dataset {0} to folder {1}", datasetExportConfig.DatasetName, targetDatasetFolder.Path));
                    if(datasetExportConfig.ExportDateSelectionEnabled)
                    {
                        // Copy all files
                        foreach (var datasetFile in await _viewModel.Dataset.Folder.GetFilesAsync())
                        {
                            if (datasetFile.FileType == ".csv")
                            {
                                List<string> eventStringList = new List<string>();
                                // Open file, read each line, parse the time. If the time is inbetween the start date and stop date, write the line back to file.
                                using (var inputStream = await datasetFile.OpenReadAsync())
                                using (var classicStream = inputStream.AsStreamForRead())
                                using (var streamReader = new StreamReader(classicStream))
                                {
                                    int lineNo = 0;
                                    while (streamReader.Peek() >= 0)
                                    {
                                        string curEventString = streamReader.ReadLine();
                                        if (string.IsNullOrWhiteSpace(curEventString)) continue;
                                        try
                                        {
                                            string [] tokenList = curEventString.Split(new char[] { ',' });
                                            // Get the Date of the String and add to dictionary
                                            DateTimeOffset curEventTimeTag = DateTimeOffset.Parse(tokenList[0]);
                                            if(curEventTimeTag.Date >= datasetExportConfig.ExportStartDate && curEventTimeTag.Date <= datasetExportConfig.ExportStopDate)
                                            {
                                                eventStringList.Add(curEventString);
                                            }
                                            if (curEventTimeTag.Date > datasetExportConfig.ExportStopDate) break;
                                            lineNo++;
                                        }
                                        catch (Exception except)
                                        {
                                            Logger.Instance.Error(this.GetType().Name, string.Format("Failed at line {0} with error message {1}", lineNo, except.Message));
                                        }
                                    }
                                }
                                StorageFile newDatasetFile = await targetDatasetFolder.CreateFileAsync(datasetFile.Name, CreationCollisionOption.ReplaceExisting);
                                using (var outputStream = await newDatasetFile.OpenAsync(FileAccessMode.ReadWrite))
                                using (var classicStream = outputStream.AsStreamForWrite())
                                using (var streamWriter = new StreamWriter(classicStream))
                                {
                                    foreach (string eventString in eventStringList)
                                        streamWriter.WriteLine(eventString);
                                }
                            }
                            else if (datasetFile.FileType == ".json")
                            {
                                await datasetFile.CopyAsync(targetDatasetFolder, datasetFile.Name, NameCollisionOption.ReplaceExisting);
                            }
                        }
                    }
                    else
                    {
                        // Copy all files
                        foreach (var datasetFile in await _viewModel.Dataset.Folder.GetFilesAsync())
                        {
                            await datasetFile.CopyAsync(targetDatasetFolder, datasetFile.Name, NameCollisionOption.ReplaceExisting);
                        }
                    }
                    // Change name of the copied metadata file
                    if(datasetExportConfig.DatasetRenameEnabled)
                    {
                        Dataset dataset = await Dataset.LoadMetadataFromFolderAsync(targetDatasetFolder);
                        dataset.Name = datasetExportConfig.DatasetName;
                        await dataset.WriteMetadataToFolderAsync();
                    }
                    PageReady();
                    return;
                }
                if(datasetExportConfig.ExportInTxt)
                {
                    // For backward compatibility, one can export the dataset in old-style txt format.
                    // Note that txt formatted dataset cannot be loaded by this program again.
                    return;
                }
            }
        }
    }
}
