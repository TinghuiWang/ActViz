using ActViz.Dialogs;
using ActViz.Models;
using ActViz.Services;
using ActViz.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input;
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
    public sealed partial class SiteEditPage : Page
    {
        SiteEditViewModel _viewModel;
        CoreDispatcher dispatcher = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().CoreWindow.Dispatcher;
        private double sensorIconWidth;
        private double sensorIconHeight;
        private double sensorTextWidth;
        private double sensorTextHeight;

        public SiteEditPage()
        {
            this.InitializeComponent();
            sensorIconWidth = AppSettingsService.RetrieveFromSettings<double>("sensorIconWidth", 10);
            sensorIconHeight = AppSettingsService.RetrieveFromSettings<double>("sensorIconHeight", 10);
            sensorTextWidth = AppSettingsService.RetrieveFromSettings<double>("sensorTextWidth", 60);
            sensorTextHeight = AppSettingsService.RetrieveFromSettings<double>("sensorTextHeight", 20);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _viewModel = new SiteEditViewModel(e.Parameter as Site);
            MainPage mainPage = (Window.Current.Content as Frame).Content as MainPage;
            mainPage.NavigationViewLoadMenu(new List<NavigationViewItem>()
            {
                new NavigationViewItem()
                {
                    Icon = new FontIcon() { Glyph = "\xE8BB" },
                    Content = "Close",
                    Tag = "close"
                }
            }, async (sender, args) => {
                if ((string)(args.SelectedItem as NavigationViewItem).Tag == "close")
                {
                    await this.ClosePageAsync();
                }
            });
            base.OnNavigatedTo(e);
        }

        private async Task ClosePageAsync()
        {
            if (_viewModel.IsSiteChanged)
            {
                MessageDialog dlg = new MessageDialog("Site " + _viewModel.Name + " has changed. Do you want to save the change back to disk?", "Save Site");
                dlg.Commands.Add(new UICommand("Save"));
                dlg.Commands.Add(new UICommand("Cancel"));
                IUICommand selectedCmd = await dlg.ShowAsync();
                if (selectedCmd.Label == "Save")
                    await _viewModel.WriteBackToFolderAsync();
            }
            ((Window.Current.Content as Frame).Content as MainPage).BackToEmpty();
        }

        #region SensorCanvas
        // Floorplan and Sensor Display
        private List<Tuple<SensorViewModel, Rectangle, Viewbox, CompositeTransform>> canvasSensorList = 
            new List<Tuple<SensorViewModel, Rectangle, Viewbox, CompositeTransform>>();

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Attach Key down event
            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().CoreWindow.KeyDown += CoreWindow_KeyDownAsync;
            PopulateSensors();
            DrawSensors();
        }

        private async void CoreWindow_KeyDownAsync(CoreWindow sender, KeyEventArgs args)
        {
            if (IsKeyPressed(VirtualKey.Control))
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                 {
                     switch (args.VirtualKey)
                     {
                         case VirtualKey.Up:
                             if(sensorTextWidth < 200 && sensorTextHeight < 200)
                             {
                                 sensorTextWidth += 1;
                                 sensorTextHeight += 1;
                             }
                             break;
                         case VirtualKey.Down:
                             if(sensorTextHeight > 10 && sensorTextWidth > 10)
                             {
                                 sensorTextWidth -= 1;
                                 sensorTextHeight -= 1;
                             }
                             break;
                         default:
                             break;
                     }
                     await CanvasUpdateAsync();
                 });
            }
        }

        private void DrawSensors()
        {
            double totalX = floorplanImage.ActualWidth;
            double totalY = floorplanImage.ActualHeight;
            foreach (Tuple<SensorViewModel, Rectangle, Viewbox, CompositeTransform> sensorTuple in canvasSensorList)
            {
                SensorViewModel sensor = sensorTuple.Item1;
                Rectangle sensorRect = sensorTuple.Item2;
                Viewbox sensorViewBox = sensorTuple.Item3;
                TextBlock sensorText = (sensorViewBox.Child as Border).Child as TextBlock;
                // Check if we hide the sensor or not
                if(_viewModel.SensorInView.Contains(sensor))
                {
                    // Update only those who are visible
                    sensorRect.Height = sensorIconHeight;
                    sensorRect.Width = sensorIconWidth;
                    sensorViewBox.Height = sensorTextHeight;
                    sensorViewBox.Width = sensorTextWidth;
                    double startPosX = (sensorCanvas.ActualWidth - totalX) / 2 + ((Sensor)sensor).LocX * totalX;
                    double startPosY = (sensorCanvas.ActualHeight - totalY) / 2 + ((Sensor)sensor).LocY * totalY;
                    Canvas.SetTop(sensorRect, startPosY);
                    Canvas.SetLeft(sensorRect, startPosX);
                    Canvas.SetTop(sensorViewBox, startPosY + sensorIconHeight + 2);
                    Canvas.SetLeft(sensorViewBox, startPosX - sensorTextWidth / 2 + sensorIconWidth / 2);
                    // Determine the color
                    string priorityCategory = (_viewModel.SensorCategorySelected == null) ? 
                        null : _viewModel.SensorCategorySelected.Name;
                    Color color = SensorType.GetBestColorForSensor(sensor, priorityCategory);
                    if((sensorRect.Fill as SolidColorBrush).Color != color)
                    {
                        sensorRect.Fill = new SolidColorBrush(color);
                        //sensorText.Foreground = new SolidColorBrush(color);
                    }
                    sensorRect.Visibility = Visibility.Visible;
                    sensorViewBox.Visibility = Visibility.Visible;
                }
                else
                {
                    sensorRect.Visibility = Visibility.Collapsed;
                    sensorViewBox.Visibility = Visibility.Collapsed;
                }
            }
            sensorCanvas.InvalidateArrange();
            sensorCanvas.UpdateLayout();
        }

        private void PopulateSensors()
        {
            foreach (SensorViewModel sensorViewModel in _viewModel.SensorInView)
            {
                AddSensorToCanvas(sensorViewModel);
            }
        }

        private void AddSensorToCanvas(SensorViewModel sensorViewModel)
        {
            Sensor sensor = sensorViewModel;
            double totalX = floorplanImage.ActualWidth;
            double totalY = floorplanImage.ActualHeight;
            CompositeTransform compositeTransform = new CompositeTransform();
            // Draw Rectangle
            Rectangle sensorRect = new Rectangle
            {
                Width = sensor.SizeX * totalX,
                Height = sensor.SizeY * totalY,
                Fill = new SolidColorBrush(SensorType.GetBestColorForSensor(sensorViewModel)),
                Stroke = new SolidColorBrush(Colors.DimGray),
                StrokeThickness = 1,
                StrokeDashCap = PenLineCap.Round
            };
            Canvas.SetZIndex(sensorRect, 0);
            sensorRect.ManipulationMode = ManipulationModes.All;
            sensorRect.ManipulationDelta += Sensor_ManipulationDelta;
            sensorRect.ManipulationCompleted += Sensor_ManipulationCompleted;
            sensorRect.RenderTransform = compositeTransform;
            sensorRect.RightTapped += Sensor_RightTapped;
            //sensorRect.PointerMoved += Sensor_PointMoved;
            //sensorRect.PointerExited += Sensor_PointExited;
            sensorRect.PointerPressed += Sensor_PointerPressed;
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
            Color bgColor = Colors.Gray;
            bgColor.A = 0x40;
            Border sensorTextBorder = new Border
            {
                Background = new SolidColorBrush(bgColor),
                Child = sensorText,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Viewbox sensorViewbox = new Viewbox
            {
                Height = sensorRect.Height,
                Width = sensorRect.Width,
                Stretch = Stretch.Uniform,
                Child = sensorTextBorder,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Canvas.SetZIndex(sensorViewbox, 0);
            sensorViewbox.ManipulationMode = ManipulationModes.All;
            sensorViewbox.ManipulationStarted += Sensor_ManipulationStarted;
            sensorViewbox.ManipulationDelta += Sensor_ManipulationDelta;
            sensorViewbox.ManipulationCompleted += Sensor_ManipulationCompleted;
            sensorViewbox.RenderTransform = compositeTransform;
            sensorViewbox.RightTapped += Sensor_RightTapped;
            //sensorViewbox.PointerMoved += Sensor_PointMoved;
            //sensorViewbox.PointerExited += Sensor_PointExited;
            sensorText.PointerPressed += Sensor_PointerPressed;
            // Populate Sensor List
            canvasSensorList.Add(new Tuple<SensorViewModel, Rectangle, Viewbox, CompositeTransform>(
                sensorViewModel, sensorRect, sensorViewbox, compositeTransform));
            sensorCanvas.Children.Add(sensorRect);
            sensorCanvas.Children.Add(sensorViewbox);
        }

        readonly CoreCursor arrowCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        readonly CoreCursor resizeNECursor = new CoreCursor(CoreCursorType.SizeNortheastSouthwest, 2);
        readonly CoreCursor resizeNWCursor = new CoreCursor(CoreCursorType.SizeNorthwestSoutheast, 3);
        readonly CoreCursor moveCursor = new CoreCursor(CoreCursorType.SizeAll, 4);

        private void Sensor_PointExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = arrowCursor;
        }

        private bool resizeNotMove = false;
        private bool onLeftBorder = false;
        private bool onRightBorder = false;
        private bool onTopBorder = false;
        private bool onBottomBorder = false;
        private bool manipulationStarted = false;
        private readonly int resizeBorderWidth = 5;
        private readonly int sensorHeightAbsMin = 12;
        private readonly int sensorWidthAbsMin = 12;

        private void Sensor_PointMoved(object sender, PointerRoutedEventArgs e)
        {
            if (manipulationStarted)
                return;
            PointerPoint point = e.GetCurrentPoint((UIElement)sender);
            onLeftBorder = (point.Position.X < resizeBorderWidth);  
            onRightBorder = (((sender as FrameworkElement).ActualWidth - point.Position.X) < resizeBorderWidth);
            onTopBorder = (point.Position.Y < resizeBorderWidth);
            onBottomBorder = (((sender as FrameworkElement).ActualHeight - point.Position.Y) < resizeBorderWidth);
            if ((onLeftBorder && onTopBorder) || (onRightBorder && onBottomBorder))
            {
                // Top Left or Bottom Right
                resizeNotMove = true;
                if (Window.Current.CoreWindow.PointerCursor != resizeNWCursor) Window.Current.CoreWindow.PointerCursor = resizeNWCursor;
            }
            else if ((onRightBorder && onTopBorder) || (onLeftBorder && onBottomBorder))
            {
                resizeNotMove = true;
                if (Window.Current.CoreWindow.PointerCursor != resizeNECursor) Window.Current.CoreWindow.PointerCursor = resizeNECursor;
            }
            else
            {
                resizeNotMove = false;
                Window.Current.CoreWindow.PointerCursor = moveCursor;
            }
        }

        private string CheckLocation(FrameworkElement sender, Point point)
        {
            double xLeftDeltaPercentage = point.X / sender.ActualWidth;
            double xRightDeltaPercentage = 1.0 - xLeftDeltaPercentage;
            double xTopDeltaPercentage = point.Y / sender.ActualHeight;
            double xBottomDeltaPercentage = 1 - xTopDeltaPercentage;
            if ((xLeftDeltaPercentage < 0.1 || xRightDeltaPercentage < 0.1) && (xTopDeltaPercentage < 0.1 || xRightDeltaPercentage < 0.1))
                return "Pointer On Border at " + ((xLeftDeltaPercentage < 0.1) ? "Left" : "Right") + ((xTopDeltaPercentage < 0.1) ? "Top" : "Bottom");
            else
                return "Pointer Not On Border";
        }

        private void Sensor_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var tuple = FindSensorIndexInTupleList(sender);
            if(tuple != null)
            {
                _viewModel.SensorSelected = tuple.Item1;
            }
        }

        private Tuple<SensorViewModel, Rectangle, Viewbox, CompositeTransform> FindSensorIndexInTupleList(object sender)
        {
            Rectangle sensorRect = null;
            Viewbox sensorViewbox = null;
            SensorViewModel sensorViewModel = null;
            int i = 0;
            if (sender is Rectangle)
            {
                sensorRect = (Rectangle)sender;
                for (i = 0; i < canvasSensorList.Count; i++)
                {
                    if (canvasSensorList[i].Item2 == sensorRect)
                    {
                        return canvasSensorList[i];
                    }
                }
            }
            else if (sender is Viewbox)
            {
                sensorViewbox = (Viewbox)sender;
                for (i = 0; i < canvasSensorList.Count; i++)
                {
                    if (canvasSensorList[i].Item3 == sensorViewbox)
                    {
                        return canvasSensorList[i];
                    }
                }
            }
            else if (sender is SensorViewModel)
            {
                sensorViewModel = (SensorViewModel)sender;
                for (i = 0; i < canvasSensorList.Count; i++)
                {
                    if (canvasSensorList[i].Item1 == sensorViewModel)
                    {
                        return canvasSensorList[i];
                    }
                }
            }
            return null;
        }

        private void Sensor_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Rectangle sensorRect = null;
            SensorViewModel sensorViewModel = null;
            Tuple<SensorViewModel, Rectangle, Viewbox, CompositeTransform> tuple = FindSensorIndexInTupleList(sender);
            if (tuple == null) return;
            sensorViewModel = tuple.Item1;
            sensorRect = tuple.Item2;
            _viewModel.SensorCategorySelected = SensorType.GetSensorCategory(sensorViewModel.SensorTypes[0].Category);
            _viewModel.SensorSelected = sensorViewModel;
            MenuFlyout sensorConfigMenu = new MenuFlyout();
            MenuFlyoutItem sensorConfigMenu_Config = new MenuFlyoutItem
            {
                Text = "Edit",
                Tag = tuple
            };
            sensorConfigMenu_Config.Click += btnEditSensor_ClickAsync;
            MenuFlyoutItem sensorConfigMenu_Delete = new MenuFlyoutItem
            {
                Text = "Delete",
                Tag = tuple
            };
            sensorConfigMenu_Delete.Click += SensorConfigMenu_Delete_ClickAsync;
            sensorConfigMenu.Items.Add(sensorConfigMenu_Config);
            sensorConfigMenu.Items.Add(sensorConfigMenu_Delete);
            sensorConfigMenu.ShowAt(sensorRect, new Point(sensorRect.ActualWidth, sensorRect.ActualHeight));
        }

        private async void SensorConfigMenu_Delete_ClickAsync(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem configMenuItem = (MenuFlyoutItem)sender;
            Tuple<SensorViewModel, Rectangle, Viewbox, CompositeTransform> sensorTuple =
                (Tuple<SensorViewModel, Rectangle, Viewbox, CompositeTransform>)configMenuItem.Tag;
            SensorViewModel sensorViewModel = sensorTuple.Item1;
            string message = "Are you sure that you want to perminantly remove sensor " + sensorViewModel.Name + "?";
            var dialog = new Windows.UI.Popups.MessageDialog(message, "Remove Sensor");
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes") { Id = 0 });
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("Cancel") { Id = 1 });
            var result = await dialog.ShowAsync();
            if (result.Label == "Yes")
            {
                // Remove Sensor
                sensorCanvas.Children.Remove(sensorTuple.Item2);
                sensorCanvas.Children.Remove(sensorTuple.Item3);
                _viewModel.RemoveSensor(sensorViewModel);
                canvasSensorList.Remove(sensorTuple);
                sensorCanvas.InvalidateArrange();
                sensorCanvas.UpdateLayout();
            }
        }

        private void Sensor_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            manipulationStarted = true;
            FrameworkElement frameworkElement = sender as FrameworkElement;
            CompositeTransform compositeTransform = frameworkElement.RenderTransform as CompositeTransform;
            if (resizeNotMove)
            {
                if (onTopBorder) compositeTransform.CenterY = frameworkElement.ActualHeight;
                else compositeTransform.CenterY = 0;
                if (onLeftBorder) compositeTransform.CenterX = frameworkElement.ActualWidth;
                else compositeTransform.CenterX = 0;
                compositeTransform.ScaleX = 1.0;
                compositeTransform.ScaleY = 1.0;
            }
        }

        private void Sensor_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _viewModel.IsSiteChanged = true;
            Rectangle sensorRect = null;
            Sensor sensor = null;
            Viewbox sensorViewbox = null;
            CompositeTransform compositeTransform = null;
            Tuple<SensorViewModel, Rectangle, Viewbox, CompositeTransform> tuple = FindSensorIndexInTupleList(sender);
            if (tuple == null) return;
            sensor = tuple.Item1;
            sensorRect = tuple.Item2;
            sensorViewbox = tuple.Item3;
            compositeTransform = tuple.Item4;
            if (compositeTransform != null)
            {
                double totalX = floorplanImage.ActualWidth;
                double totalY = floorplanImage.ActualHeight;
                double startPosX = Canvas.GetLeft(sensorRect);
                double startPosY = Canvas.GetTop(sensorRect);
                if (resizeNotMove)
                {
                    if (onLeftBorder)
                    {
                        startPosX -= sensorRect.ActualWidth * (compositeTransform.ScaleX - 1);
                        sensor.LocX -= sensor.SizeX * (compositeTransform.ScaleX - 1);
                        Canvas.SetLeft(sensorRect, startPosX);
                        Canvas.SetLeft(sensorViewbox, startPosX - sensorTextWidth / 2);
                    }
                    if (onTopBorder)
                    {
                        startPosY -= sensorRect.ActualHeight * (compositeTransform.ScaleY - 1);
                        sensor.LocY -= sensor.SizeY * (compositeTransform.ScaleY - 1);
                        Canvas.SetTop(sensorRect, startPosY);
                        Canvas.SetTop(sensorViewbox, startPosY + sensorIconHeight + 2);
                    }
                    sensorRect.Width *= compositeTransform.ScaleX;
                    sensorViewbox.Width *= compositeTransform.ScaleX;
                    sensor.SizeX *= compositeTransform.ScaleX;
                    sensorRect.Height *= compositeTransform.ScaleY;
                    sensorViewbox.Height *= compositeTransform.ScaleY;
                    sensor.SizeY *= compositeTransform.ScaleY;
                    compositeTransform.ScaleX = 1.0;
                    compositeTransform.ScaleY = 1.0;
                }
                else
                {
                    startPosX += compositeTransform.TranslateX;
                    startPosY += compositeTransform.TranslateY;
                    sensor.LocX = (startPosX - (sensorCanvas.ActualWidth - totalX) / 2) / totalX;
                    sensor.LocY = (startPosY - (sensorCanvas.ActualHeight - totalY) / 2) / totalY;
                    Canvas.SetLeft(sensorRect, startPosX);
                    Canvas.SetTop(sensorRect, startPosY);
                    Canvas.SetLeft(sensorViewbox, startPosX - sensorTextWidth / 2 + sensorIconWidth / 2);
                    Canvas.SetTop(sensorViewbox, startPosY + sensorIconHeight + 2);
                    compositeTransform.TranslateX = 0;
                    compositeTransform.TranslateY = 0;
                }
                sensorCanvas.InvalidateArrange();
                sensorCanvas.UpdateLayout();
            }
            manipulationStarted = false;
        }

        private void Sensor_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            if (frameworkElement.RenderTransform is CompositeTransform compositeTransform)
            {
                if (resizeNotMove)
                {
                    if (onLeftBorder || onRightBorder)
                    {
                        double deltaScale = ((onLeftBorder) ? -1 : 1) * e.Delta.Translation.X / frameworkElement.ActualWidth;
                        // Check Scale result
                        double sensorWidthIfScaled = frameworkElement.ActualWidth * (compositeTransform.ScaleX + deltaScale);
                        deltaScale = (sensorWidthIfScaled < sensorWidthAbsMin) ? 0 : deltaScale;
                        compositeTransform.ScaleX += deltaScale;
                    }
                    if (onTopBorder || onBottomBorder)
                    {
                        double deltaScale = ((onTopBorder) ? -1 : 1) * e.Delta.Translation.Y / frameworkElement.ActualHeight;
                        double sensorHeightIfScaled = frameworkElement.ActualHeight * (compositeTransform.ScaleY + deltaScale);
                        deltaScale = (sensorHeightIfScaled < sensorHeightAbsMin) ? 0 : deltaScale;
                        compositeTransform.ScaleY += deltaScale;
                    }
                }
                else
                {
                    compositeTransform.TranslateX += e.Delta.Translation.X;
                    compositeTransform.TranslateY += e.Delta.Translation.Y;
                }
            }
        }
        #endregion

        private void floorplanImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawSensors();
        }

        private void sensorCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawSensors();
        }

        private async void btnEditSensor_ClickAsync(object sender, RoutedEventArgs e)
        {
            EditSensorDialog dialog = new EditSensorDialog(_viewModel.SensorSelected);
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                _viewModel.IsSiteChanged = true;
            }
        }

        private async void btnNewSensor_ClickAsync(object sender, RoutedEventArgs e)
        {
            AddSensorDialog dialog = new AddSensorDialog();
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                //_viewModel.AddSensor(dialog.SensorViewModel);
                _viewModel.IsSiteChanged = true;
            }
        }

        private async void btnCloseSite_ClickAsync(object sender, RoutedEventArgs e)
        {
            await ClosePageAsync();
        }

        private async void btnSaveSite_ClickAsync(object sender, RoutedEventArgs e)
        {
            await _viewModel.WriteBackToFolderAsync();
        }

        private void sensorListTypeSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DrawSensors();
        }

        private static bool IsKeyPressed(VirtualKey key)
        {
            var ctrlState = CoreWindow.GetForCurrentThread().GetKeyState(key);
            return (ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }

        //private async Task SetSameHeightToAllSensorAsync()
        //{
        //    // Get current selected sensor
        //    if (_viewModel.SensorSelected == null) return;
        //    var tuple = FindSensorIndexInTupleList(_viewModel.SensorSelected);
        //    string message = string.Format("Broadcast height {0:F4} ({1:F2} px) of sensor {2} to all sensors?",
        //        ((Sensor)tuple.Item1).SizeY, tuple.Item2.ActualHeight, tuple.Item1.Name);
        //    var dlg = new MessageDialog(message, "Broadcast Height to All Sensors");
        //    dlg.Commands.Add(new UICommand("Yes"));
        //    dlg.Commands.Add(new UICommand("Cancel"));
        //    IUICommand selectedCmd = await dlg.ShowAsync();
        //    if (selectedCmd.Label == "Yes")
        //    {
        //        foreach (var sensorTuple in canvasSensorList)
        //        {
        //            ((Sensor)sensorTuple.Item1).SizeY = ((Sensor)tuple.Item1).SizeY;
        //            sensorTuple.Item2.Height = tuple.Item2.Height;
        //            sensorTuple.Item3.Height = tuple.Item3.Height;
        //        }
        //        _viewModel.IsSiteChanged = true;
        //        await CanvasUpdateAsync();
        //    }
        //}

        //private async Task SetSameWidthToAllSensorAsync()
        //{
        //    // Get current selected sensor
        //    if (_viewModel.SensorSelected == null) return;
        //    var tuple = FindSensorIndexInTupleList(_viewModel.SensorSelected);
        //    string message = string.Format("Broadcast width {0:4F} ({1:2F} px) of sensor {2} to all sensors?",
        //        ((Sensor)tuple.Item1).SizeX, tuple.Item2.ActualWidth, tuple.Item1.Name);
        //    var dlg = new MessageDialog(message, "Broadcast Width to All Sensors");
        //    dlg.Commands.Add(new UICommand("Yes"));
        //    dlg.Commands.Add(new UICommand("Cancel"));
        //    IUICommand selectedCmd = await dlg.ShowAsync();
        //    if (selectedCmd.Label == "Yes")
        //    {
        //        foreach (var sensorTuple in canvasSensorList)
        //        {
        //            ((Sensor)sensorTuple.Item1).SizeX = ((Sensor)tuple.Item1).SizeX;
        //            sensorTuple.Item2.Width = tuple.Item2.Width;
        //            sensorTuple.Item3.Width = tuple.Item3.Width;
        //        }
        //        _viewModel.IsSiteChanged = true;
        //        await CanvasUpdateAsync();
        //    }
        //}

        private async Task CanvasUpdateAsync()
        {
            var dispatcher = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                DrawSensors();
            });
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().CoreWindow.KeyDown -= CoreWindow_KeyDownAsync;
        }

        private void sensorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel.SensorSelected != null)
            {
                foreach (var sensorTuple in canvasSensorList)
                {
                    if (sensorTuple.Item1 != _viewModel.SensorSelected)
                    {
                        Canvas.SetZIndex(sensorTuple.Item2, 0);
                        Canvas.SetZIndex(sensorTuple.Item3, 0);
                    }
                    else
                    {
                        Canvas.SetZIndex(sensorTuple.Item2, 50);
                        Canvas.SetZIndex(sensorTuple.Item3, 50);
                    }
                }
                DrawSensors();
            }
        }
    }
}
