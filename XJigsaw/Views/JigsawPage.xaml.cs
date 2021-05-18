using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MediaManager;
using Plugin.ImageEdit;
using Plugin.ImageEdit.Abstractions;
using Plugin.SimpleAudioPlayer;
using Plugin.Toast;
using XJigsaw.Helper;
using XJigsaw.Models;
using XJigsaw.ViewModels;
using Stormlion.ImageCropper;
using Xamarin.Forms;
using XJigsaw.Resources;
using Xamarin.Essentials;
using Rg.Plugins.Popup.Extensions;

namespace XJigsaw.Views
{
    public partial class JigsawPage : ContentPage
    {
        // Readonly settings
        public static readonly double SMALL_IMAGE_PERCENT = 0.33;

        // Number of tiles horizontally and vertically,
        static int NUM = 3;
        public static Jigsaw CurrentJigsaw;
        public static JigsawPage Instance = null;

        public static View JigsawParent { get; set; }
        public static Frame ToolBarFrame { get; set; }
        public static Frame StatusBarFrame { get; set; }
        public static View StackLayoutParent { get; set; }
        public static double ContainerWidth { get; set; }
        public static double ContainerHeight { get; set; }
        readonly ISimpleAudioPlayer tapPlayer = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
        readonly ISimpleAudioPlayer successPlayer = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
        readonly AdMobService adHandler = null;
        readonly Image absolute_background = new Image
        {
            Source = ImageSource.FromFile("startup_bg.png"),
            Opacity = 0.5,
            Aspect = Aspect.AspectFill
        };

        public AbsoluteLayout JigsawLayout
        {
            get
            {
                return this.absoluteLayout;
            }
        }

        // Array of tiles
        Tile[,] tiles = new Tile[NUM, NUM];

        // Empty row and column
        int emptyRow = NUM - 1;
        int emptyCol = NUM - 1;

        double tileSizeWidth;
        double tileSizeHeight;
        double borderWidth = 3;
        bool isContainerResized = false;
        Stopwatch timer;

        int steps = 0;
        public int Steps
        {
            get
            {
                return steps;
            }
            set
            {
                if (value != steps)
                    steps = value;
                labelSteps.Text = $"{steps}";
            }
        }

        bool isProcessing;
        public bool IsProcessing
        {
            get
            {
                return isProcessing;
            }
            set
            {
                isProcessing = value;
                EnableDisableToolBarButtons();
            }
        }

        bool isGameMode = false;
        public bool IsGameMode
        {
            get
            {
                return isGameMode;
            }
            set
            {
                isGameMode = value;
                if (isGameMode)
                {
                    playButton.Source = ImageSource.FromFile("stop.png");
                    UpdateTabsEnable(false);
                    labelInfor.Text = XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_GameMode;

                }
                else
                {
                    playButton.Source = ImageSource.FromFile("play.png");
                    UpdateTabsEnable(true);
                    labelInfor.Text = "";
                }
                ShowHideControls();
                labelTimer.Text = "00:00:00";
            }
        }

        public void ShowHideControls(bool isFreshNew = false)
        {
            if (isFreshNew)
            {
                chooseImageButton.IsVisible = true;
                resetButton.IsVisible = false;
                saveButton.IsVisible = false;
                timerFrame.IsVisible = false;
                playButton.IsVisible = false;
                shuffleButton.IsVisible = false;
            }
            else
            {
                chooseImageButton.IsVisible = !IsGameMode;
                resetButton.IsVisible = !IsGameMode && CurrentJigsaw.IsTilePositionChanged;
                saveButton.IsVisible = !IsGameMode;
                timerFrame.IsVisible = IsGameMode;
                playButton.IsVisible = true;
                shuffleButton.IsVisible = true;
                App.ShellInstance.JigsawTab.Items[2].IsVisible = !IsGameMode;
            }
        }

        public void EnableDisableToolBarButtons()
        {
            chooseImageButton.IsEnabled = !IsProcessing;
            resetButton.IsEnabled = !IsProcessing;
            saveButton.IsEnabled = !IsProcessing;
            playButton.IsEnabled = !IsProcessing;
            shuffleButton.IsEnabled = !IsProcessing;
        }

        public void ShowHideToolBarButton(bool isChooseVisible = true, bool isResetVisible = true, bool isSaveButtonVisible = true,
                                          bool isShuffleVisible = true, bool isPlayVisible = true)
        {
            chooseImageButton.IsVisible = isChooseVisible;
            resetButton.IsVisible = isResetVisible;
            saveButton.IsVisible = isSaveButtonVisible;
            shuffleButton.IsVisible = isShuffleVisible;
            playButton.IsVisible = isPlayVisible;
        }

        public JigsawPage()
        {
            InitializeComponent();
            JigsawParent = (View)this.absoluteLayout.Parent;
            ToolBarFrame = this.toolBarFrame;
            StatusBarFrame = this.statusBarFrame;
            StackLayoutParent = (View)this.stackLayout.Parent;
            tapPlayer.Load("tap.wav");
            successPlayer.Load("success.mp3");
            Instance = this;

            adHandler = new AdMobService(this);
        }

        public void SaveCurrentJigsaw(bool isAdPlayCompleted, bool isFailedLoading = false, string failedMsg = "")
        {
            try
            {
                if (isAdPlayCompleted)
                {
                    Task.Run(() => SaveCurrentJigsawAfterAd()).Wait();
                    CrossToastPopUp.Current.ShowToastMessage(XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_SaveJigsawSucceed);
                }
                else
                {
                    if (!isFailedLoading)
                        CrossToastPopUp.Current.ShowToastMessage(XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_SaveJigsawNotes);
                    else
                    {
                        //Task.Run(() => SaveCurrentJigsawAfterAd()).Wait();                        
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (isFailedLoading)
                    CrossToastPopUp.Current.ShowToastMessage(failedMsg);
                saveButton.IsVisible = true;
                IsProcessing = false;
            }
        }

        public void NotifyPlayAdStatus(string status)
        {
            labelInfor.Text = status;
        }

        async void SaveCurrentJigsawAfterAd()
        {
            JigsawPage.CurrentJigsaw.LocationsInString = Tile.LocationsToString();
            JigsawPage.CurrentJigsaw.Steps = Steps;
            JigsawPage.CurrentJigsaw.IsTilePositionInitial = true;
            JigsawPage.CurrentJigsaw.IsDeleted = false;

            await App.Database.SaveJigsawAsync(JigsawPage.CurrentJigsaw);
            if (HistoryLocalViewModel.JigsawIDs.Contains(JigsawPage.CurrentJigsaw.ID))
            {
                HistoryLocalPage.UpdatedJigsawID = JigsawPage.CurrentJigsaw.ID;
            }
            App.ShellInstance.JigsawSettings.CurrentJigsawId = JigsawPage.CurrentJigsaw.ID;
            await App.Database.SaveSettingAsync(App.ShellInstance.JigsawSettings);
            UpdateStatusBar(CurrentJigsaw);
        }

        void UpdateStatusBar(Jigsaw jigsaw)
        {
            var delay = Task.Run(async delegate
            {
                await Task.Delay(TimeSpan.FromSeconds(0.1));
                Device.BeginInvokeOnMainThread(() =>
                {
                    labelImageRatio.Text = jigsaw.ImageRatio;
                    labelImageResolution.Text = Utility.GetImageResolutionLabel(this);
                    labelLevel.Text = Utility.GetLevelLabel(this);
                    labelOpacity.Text = $"{string.Format("{0:N2}", jigsaw.Opacity)}";
                    labelID.Text = jigsaw.ID.ToString();
                    labelID.IsVisible = jigsaw.ID != 0;
                    UpdateIsSuccess(jigsaw);
                });
            });
        }

        void UpdateIsSuccess(Jigsaw jigsaw)
        {
            if (Tile.IsSuccess())
            {
                labelSuccess.Text = XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_Success;
                labelSuccess.TextColor = Color.Green;
                if (IsGameMode)
                {
                    IsGameMode = false;
                    timer?.Stop();
                    TimeSpan ts = timer.Elapsed;

                    // Store best score into level records
                    Level level = BestScoreViewModel.GetLevelById(jigsaw.Level);
                    string newRecord = "";
                    if (level.BestScore == 0 || ts.Ticks < level.BestScore)
                    {
                        if (level.BestScore != 0)
                        {
                            newRecord = $"{XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_BestScore_NewRecord}{Utility.FormatTimeSpan(TimeSpan.FromTicks(level.BestScore))}";
                        }
                        level.BestScore = ts.Ticks;
                        level.ScoreDateTime = DateTime.Now.ToString(Utility.DATETIME_FORMAT);
                        App.Database.UpdateLevelAysnc(level);
                    }

                    string newLevel = "";
                    string totalTime = $"{XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_TimeUsed}: {Utility.FormatTimeSpan(ts)}!";
                    if (AboutViewModel.User.Level < 32 && jigsaw.Level > AboutViewModel.User.Level)
                    {
                        AboutViewModel.User.Level += 1;
                        App.Database.UpdateUser(AboutViewModel.User);
                        string levelName = "";
                        if (AppResources.Culture.Name.Contains("zh"))
                            levelName = BestScoreViewModel.GetLevelById(AboutViewModel.User.Level).Name;
                        else
                            levelName = BestScoreViewModel.GetLevelById(AboutViewModel.User.Level).NameOther;
                        newLevel = $"{XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_CongratuateNextLevel}: {levelName}!";
                    }
                    DisplayAlert(XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_Infor,
                        $"{XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_Congratulation}! {totalTime} {newLevel} {newRecord}", XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_Confirm);
                }
                if (jigsaw.IsTilePositionInitial == false && App.ShellInstance.JigsawSettings.IsPlaySound)
                    successPlayer.Play();
            }
            else
            {
                labelSuccess.Text = XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_Failed;
                labelSuccess.TextColor = Color.Red;
            }
            if (Tile.Dictionary.Count == 0)
                labelSuccess.Text = "";
        }

        void UpdateTabsEnable(bool enabled)
        {
            App.ShellInstance.HistoryTab.IsEnabled = enabled;
            App.ShellInstance.AboutTab.IsEnabled = enabled;
        }

        async Task InitGridAsync()
        {
            NUM = CurrentJigsaw.Level;
            emptyRow = NUM - 1;
            emptyCol = NUM - 1;
            tiles = new Tile[NUM, NUM];
            Steps = 0;
            IsProcessing = true;
            progressBar.IsVisible = true;
            progressBar.Progress = 0;
            progressBar.ProgressColor = Color.Orange;
            double opacity = CurrentJigsaw.Opacity;
            UpdateTabsEnable(false);

            // Calculate tile size and position based on ContentView size.
            tileSizeWidth = (absoluteLayout.WidthRequest - 2 * borderWidth) / NUM;
            tileSizeHeight = (absoluteLayout.HeightRequest - 2 * borderWidth) / NUM;

            double imagePieceWidth = 0;
            double imagePieceHeight = 0;
            if (stackLayout.Orientation == StackOrientation.Vertical)
            {
                imagePieceWidth = CurrentJigsaw.ImageWidth / NUM;
                imagePieceHeight = CurrentJigsaw.ImageHeight / NUM;
            }

            // Clear Tiles
            absoluteLayout.Children.Clear();
            Tile.Dictionary.Clear();
            Tile.Locations.Clear();
            absoluteLayout.IsEnabled = false;

            // Add background image
            SetAbsoluteLayoutBackground();

            // Applied data from database
            if (CurrentJigsaw.IsLoaded == true && CurrentJigsaw.IsApplied == false ||
                CurrentJigsaw.IsSelected == true && CurrentJigsaw.IsApplied == false) // This means this method is called during app startup
            {
                // Load locations from database, and save them into a dictionary
                Tile.StringToLocations(JigsawPage.CurrentJigsaw.LocationsInString);
                // Load saved steps from database
                Steps = JigsawPage.CurrentJigsaw.Steps;
            }

            // Loop through the rows and columns.
            for (int row = 0; row < NUM; row++)
            {
                for (int col = 0; col < NUM; col++)
                {
                    double progress = (double)(row * NUM + col + 1) / (double)(NUM * NUM);
                    await progressBar.ProgressTo(progress, 1, Easing.Linear);
                    labelInfor.Text = $"{XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_SplitImage}: {string.Format("{0:N0}%", progress * 100)}";

                    // For the last one we need to reset its image
                    if (row == NUM - 1 && col == NUM - 1)
                    {
                        Tile emptyTile = new Tile(row, col, new Image { Source = "replace.png" }, true, tileSizeWidth, tileSizeHeight, NUM);
                        if (CurrentJigsaw.IsLoaded == true && CurrentJigsaw.IsApplied == false ||
                            CurrentJigsaw.IsSelected == true && CurrentJigsaw.IsApplied == false)
                        {
                            if (Tile.Locations.Count > 0)
                            {
                                // Get saved locations
                                string[] savedLocationArray = (Tile.Locations[emptyTile.OriginalLocation()]).Split('-');
                                emptyTile.Row = int.Parse(savedLocationArray[0]);
                                emptyTile.Col = int.Parse(savedLocationArray[1]);
                                emptyRow = emptyTile.Row;
                                emptyCol = emptyTile.Col;
                            }
                        }
                        tiles[emptyTile.Row, emptyTile.Col] = emptyTile;
                        absoluteLayout.Children.Add(emptyTile.TileView);
                        break;
                    }

                    // Create the tile
                    Image tileImage = await SplitImage(CurrentJigsaw.Bytes, imagePieceWidth, imagePieceHeight, row, col, CurrentJigsaw.ImageWidth, CurrentJigsaw.ImageHeight);
                    Tile tile = new Tile(row, col, tileImage, false, tileSizeWidth, tileSizeHeight, NUM, opacity);

                    // This means this method is called during app startup, get saved location for each tile
                    if (CurrentJigsaw.IsLoaded == true && CurrentJigsaw.IsApplied == false ||
                        CurrentJigsaw.IsSelected == true && CurrentJigsaw.IsApplied == false)
                    {
                        if (Tile.Locations.Count > 0)
                        {
                            // Get saved locations
                            string[] savedLocationArray = (Tile.Locations[tile.OriginalLocation()]).Split('-');
                            tile.Row = int.Parse(savedLocationArray[0]);
                            tile.Col = int.Parse(savedLocationArray[1]);
                        }
                    }

                    // Add tap recognition.
                    TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                    tapGestureRecognizer.Tapped += OnTileTapped;
                    tile.TileView.GestureRecognizers.Add(tapGestureRecognizer);

                    // Add the tile to the array and the AbsoluteLayout.
                    tiles[tile.Row, tile.Col] = tile;
                    absoluteLayout.Children.Add(tile.TileView);
                }
            }

            await FillGrid((View)absoluteLayout.Parent);

            absoluteLayout.IsEnabled = true;
            progressBar.IsVisible = false;
            CurrentJigsaw.IsApplied = true;
            labelInfor.Text = "";
        }

        void SetAbsoluteLayoutBackground()
        {
            absolute_background.IsVisible = true;
            absoluteLayout.Children.Add(absolute_background);
            AbsoluteLayout.SetLayoutBounds(absolute_background, new Rectangle(0, 0, absoluteLayout.WidthRequest, absoluteLayout.HeightRequest));
        }

        async Task<Image> SplitImage(byte[] bytes, double splitSizeX, double splitSizeY, int row, int col, double imageWidth, double imageHeight)
        {
            if (bytes == null || bytes.Length < 1) return new Image();
            int x = (int)splitSizeX * col;
            int y = (int)splitSizeY * row;

            int pieceSizeWidth = (int)splitSizeX;
            int pieceSizeHeight = (int)splitSizeY;
            if (col == NUM - 1) pieceSizeWidth = (int)(imageWidth - (NUM - 1) * splitSizeX);
            if (row == NUM - 1) pieceSizeHeight = (int)(imageHeight - (NUM - 1) * splitSizeY);

            IEditableImage imageEdit = await CrossImageEdit.Current.CreateImageAsync(bytes);
            IEditableImage imagePiece = imageEdit.Crop(x, y, pieceSizeWidth, pieceSizeHeight);
            string fileName = $"piece-{row}-{col}.jpg";
            var fileFullName = Path.Combine(Utility.IMAGE_TEMP_FOLDER, fileName);
            File.WriteAllBytes(fileFullName, imagePiece.ToJpeg(100));
            Image image = new Image { Source = ImageSource.FromFile(fileFullName) };
            return image;
        }

        async void OnContainerSizeChanged(object sender, EventArgs args)
        {
            /* We need to fix a bug in Android emulator issue */
            /* For some unknow reason the SizeChanged event was triggered multiple times in a Android emulatore device */
            /* So if we dectect the current jigsaw has been built and filled then we just return */
            if (!Utility.IsiOS() && DeviceInfo.DeviceType == DeviceType.Virtual)
                if (CurrentJigsaw.IsApplied) return;

            View container = (View)sender;

            /* Get some testing data */
            /*
            Console.WriteLine($"MainPage Width: {Application.Current.MainPage.Width}");
            Console.WriteLine($"MainPage Height: {Application.Current.MainPage.Height}");
            Console.WriteLine($"ContentView Width: {container.Width}");
            Console.WriteLine($"ContentView Height: {container.Height}");
            Console.WriteLine($"Toobar Width: {ToolBarFrame.Width}");
            Console.WriteLine($"Toobar Height: {ToolBarFrame.Height}");
            Console.WriteLine($"StatusBar Width: {StatusBarFrame.Width}");
            Console.WriteLine($"StatusBar Height: {StatusBarFrame.Height}");
            */

            double width = container.Width;
            double height = container.Height;
            ContainerWidth = width;
            ContainerHeight = height;
            isContainerResized = false;

            // Orient StackLayout based on portrait/landscape mode.
            stackLayout.Orientation = StackOrientation.Vertical;
            InitialImageRatio(width, height);
            absoluteLayoutResize(container);

            // update sattus bar
            UpdateStatusBar(JigsawPage.CurrentJigsaw);

            await InitGridAsync();

            isContainerResized = true;
        }

        void InitialImageRatio(double width, double height)
        {
            if ((App.ShellInstance.JigsawSettings.ImageRatio == "自动" || App.ShellInstance.JigsawSettings.ImageRatio == "Auto") && App.ShellInstance.JigsawSettings.AspectRatio_X == 0)
            {
                App.ShellInstance.JigsawSettings.AspectRatio_X = (int)width;
                App.ShellInstance.JigsawSettings.AspectRatio_Y = (int)(height - ToolBarFrame.Height - StatusBarFrame.Height);
                App.Database.SaveSettingAsync(App.ShellInstance.JigsawSettings).Wait();
            }

            if ((JigsawPage.CurrentJigsaw.ImageRatio == "自动" || JigsawPage.CurrentJigsaw.ImageRatio == "Auto") && JigsawPage.CurrentJigsaw.AspectRatio_X == 0)
            {
                JigsawPage.CurrentJigsaw.AspectRatio_X = App.ShellInstance.JigsawSettings.AspectRatio_X;
                JigsawPage.CurrentJigsaw.AspectRatio_Y = App.ShellInstance.JigsawSettings.AspectRatio_Y;
            }
        }

        void absoluteLayoutResize(View container)
        {
            // container is the ConventView, it includes the
            // ToolBar, Progress Bar, AbsoluteLayout, Status Bar
            // They are all in a StackLayout with Vertical: FillAndExpand configurations

            double width = container.Width;
            double height = container.Height;
            if (width < 0 || height < 0) return;

            // Orient StackLayout based on portrait/landscape mode.
            stackLayout.Orientation = StackOrientation.Vertical;

            if (stackLayout.Orientation == StackOrientation.Vertical)
            {
                double newWidth = container.Width;
                double newHeight = newWidth * CurrentJigsaw.AspectRatio_Y / CurrentJigsaw.AspectRatio_X;
                if ((newHeight + toolBarFrame.Height + statusBarFrame.Height) > container.Height)
                {
                    newHeight = container.Height - toolBarFrame.Height - statusBarFrame.Height;
                    newWidth = newHeight * CurrentJigsaw.AspectRatio_X / CurrentJigsaw.AspectRatio_Y;
                    double marginLeftRight = (container.Width - newWidth) / 2;
                    stackLayout.Margin = new Thickness(marginLeftRight, 0, marginLeftRight, 0);
                }
                else
                {
                    stackLayout.Margin = new Thickness(0);
                }
                absoluteLayout.WidthRequest = newWidth;
                absoluteLayout.HeightRequest = newHeight;
            }
        }

        async Task FillGrid(View container)
        {
            double width = container.Width;
            double height = container.Height;

            if (width <= 0 || height <= 0)
                return;

            IsProcessing = true;
            App.ShellInstance.HistoryTab.IsEnabled = false;

            Color borderColor = Color.White;

            // Draw the borders
            BoxView left = new BoxView
            {
                Color = borderColor,
                WidthRequest = borderWidth,
                HeightRequest = absoluteLayout.Height
            };
            BoxView top = new BoxView
            {
                Color = borderColor,
                WidthRequest = absoluteLayout.Width,
                HeightRequest = borderWidth
            };
            BoxView right = new BoxView
            {
                Color = borderColor,
                WidthRequest = borderWidth,
                HeightRequest = absoluteLayout.Height
            };
            BoxView bottom = new BoxView
            {
                Color = borderColor,
                WidthRequest = absoluteLayout.Width,
                HeightRequest = borderWidth,
            };

            if (CurrentJigsaw.Level > 16)
            {
                left.WidthRequest -= 0.25;
                top.HeightRequest -= 0.25;
            }

            absoluteLayout.Children.Add(left);
            absoluteLayout.Children.Add(top);
            absoluteLayout.Children.Add(right);
            absoluteLayout.Children.Add(bottom);

            AbsoluteLayout.SetLayoutBounds(left, new Rectangle(0, borderWidth, left.Width, left.Height));
            AbsoluteLayout.SetLayoutBounds(top, new Rectangle(0, 0, top.Width, top.Height));
            AbsoluteLayout.SetLayoutBounds(right, new Rectangle(absoluteLayout.Width - borderWidth, borderWidth, right.Width, right.Height));
            AbsoluteLayout.SetLayoutBounds(bottom, new Rectangle(0, absoluteLayout.Height - borderWidth, bottom.Width, bottom.Height));

            // Start to draw each tile
            // Calculate tile size and position based on ContentView size.
            tileSizeWidth = (absoluteLayout.Width - 2 * borderWidth) / NUM;
            tileSizeHeight = (absoluteLayout.Height - 2 * borderWidth) / NUM;

            progressBar.IsVisible = true;
            progressBar.ProgressColor = Color.LightBlue;
            progressBar.Progress = 0;
            int i = 0;
            foreach (View fileView in absoluteLayout.Children)
            {
                if (fileView is BoxView) continue;
                if (fileView is Image) continue;

                Tile tile = Tile.Dictionary[fileView];
                tile.TileView.IsVisible = true;
                if (tile.IsEmptyTile)
                    tile.TileView.IsVisible = CurrentJigsaw.Bytes != null;

                // Set tile bounds.
                AbsoluteLayout.SetLayoutBounds(fileView, new Rectangle(tile.Col * tileSizeWidth + borderWidth,
                                                                       tile.Row * tileSizeHeight + borderWidth,
                                                                       tileSizeWidth,
                                                                       tileSizeHeight));

                double progress = (double)(i++) / (double)(absoluteLayout.Children.Count);
                await progressBar.ProgressTo(progress, 1, Easing.Linear);
                labelInfor.Text = $"{XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_BuildImage}: {string.Format("{0:N0}%", progress * 100)}";

            }
            progressBar.IsVisible = false;
            labelInfor.Text = "";
            absolute_background.IsVisible = CurrentJigsaw.Bytes == null;
            UpdateTabsEnable(true);
            UpdateIsSuccess(CurrentJigsaw);
            IsProcessing = false;

            ShowHideControls(CurrentJigsaw.IsFreshNew);

            this.ShowPrivacyPolicy(App.ShellInstance.JigsawSettings.IsReadPrivacy);
        }

        async void OnTileTapped(object sender, EventArgs args)
        {
            if (CurrentJigsaw.Bytes == null || CurrentJigsaw.Bytes.Length == 0)
                return;

            if (IsProcessing)
                return;

            View tileView = (View)sender;
            Tile tappedTile = Tile.Dictionary[tileView];

            // play tap sound
            if (App.ShellInstance.JigsawSettings.IsPlaySound)
                if (tappedTile.Row == emptyRow || tappedTile.Col == emptyCol)
                    tapPlayer.Play();

            // begin move the tiles if tap on related tiles
            if (tappedTile.Row == emptyRow || tappedTile.Col == emptyCol)
            {
                IsProcessing = true;

                JigsawPage.CurrentJigsaw.IsTilePositionChanged = true;
                JigsawPage.CurrentJigsaw.IsTilePositionInitial = false;

                await ShiftIntoEmpty(tappedTile.Row, tappedTile.Col, 200);

                ShowHideControls();

                UpdateStatusBar(CurrentJigsaw);

                IsProcessing = false;
            }
        }

        async Task ShiftIntoEmpty(int tappedRow, int tappedCol, uint length = 100)
        {
            // Shift columns.
            if (tappedRow == emptyRow && tappedCol != emptyCol)
            {
                int inc = Math.Sign(tappedCol - emptyCol);
                int begCol = emptyCol + inc;
                int endCol = tappedCol + inc;

                for (int col = begCol; col != endCol; col += inc)
                {
                    await AnimateTile(emptyRow, col, emptyRow, emptyCol, length);
                }
            }
            // Shift rows.
            else if (tappedCol == emptyCol && tappedRow != emptyRow)
            {
                int inc = Math.Sign(tappedRow - emptyRow);
                int begRow = emptyRow + inc;
                int endRow = tappedRow + inc;

                for (int row = begRow; row != endRow; row += inc)
                {
                    await AnimateTile(row, emptyCol, emptyRow, emptyCol, length);
                }
            }
        }

        async Task AnimateTile(int row, int col, int newRow, int newCol, uint length)
        {
            // The tile to be animated.
            Tile tile = tiles[row, col];
            View tileView = tile.TileView;

            // The destination rectangle.
            Rectangle rect = new Rectangle(emptyCol * tileSizeWidth + borderWidth,
                                           emptyRow * tileSizeHeight + borderWidth,
                                           tileSizeWidth,
                                           tileSizeHeight);

            // Set emplty tile layout bounds to a new Rectangle.
            Tile emptyTile = tiles[emptyRow, emptyCol];
            View emptyTileView = emptyTile.TileView;
            emptyTileView.IsVisible = false;
            Rectangle emptyRect = new Rectangle(col * tileSizeWidth + borderWidth,
                                           row * tileSizeHeight + borderWidth,
                                           tileSizeWidth,
                                           tileSizeHeight);


            // Animate it!
            if (!IsGameMode)
                await tileView.LayoutTo(rect, length);

            // Set layout bounds to same Rectangle.
            AbsoluteLayout.SetLayoutBounds(tileView, rect);

            // Set the empty tile bounds
            AbsoluteLayout.SetLayoutBounds(emptyTileView, emptyRect);
            emptyTileView.IsVisible = true;

            // Set several variables and properties for new layout.
            tiles[newRow, newCol] = tile;
            tile.Row = newRow;
            tile.Col = newCol;
            emptyTile.Row = row;
            emptyTile.Col = col;
            tiles[row, col] = emptyTile;
            emptyRow = row;
            emptyCol = col;
            Steps += 1;
        }

        async void OnRandomizeButtonClicked(object sender, EventArgs args)
        {
            bool isNewGame = false;
            if (isGameMode)
            {
                var result = await DisplayAlert(XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_Confirm,
                    XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_ConfirmNewGame,
                    XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_Yes,
                    XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_No);
                if (result)
                {
                    isNewGame = true;
                }
            }
            if (isGameMode == false || (isGameMode == true && isNewGame == true))
            {
                JigsawPage.CurrentJigsaw.IsTilePositionChanged = true;
                ShowHideControls();
                RandomizeTiles(100);
            }
        }

        async void RandomizeTiles(int times)
        {
            IsProcessing = true;
            Random rand = new Random();

            // Simulate some fast crazy taps.
            for (int i = 0; i < times; i++)
            {
                await ShiftIntoEmpty(rand.Next(NUM), emptyCol, 1);
                await ShiftIntoEmpty(emptyRow, rand.Next(NUM), 1);
            }
            CurrentJigsaw.IsTilePositionChanged = true;
            Steps = 0;
            if (isGameMode)
            {
                timer?.Reset();
                timer?.Start();
            }
            UpdateStatusBar(CurrentJigsaw);
            IsProcessing = false;
        }

        async void OnReverseButtonClicked(object sender, EventArgs args)
        {
            var result = await this.DisplayAlert(XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_Confirm,
                XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_ConfirmToInit,
                XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_Yes,
                XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_No);

            if (result)
            {
                UpdateTabsEnable(false);
                // Clear container
                absoluteLayout.Children.Clear();
                Steps = 0;
                Tile.Locations.Clear();
                emptyRow = JigsawPage.CurrentJigsaw.Level - 1;
                emptyCol = JigsawPage.CurrentJigsaw.Level - 1;
                progressBar.IsVisible = true;
                progressBar.ProgressColor = Color.Orange;
                progressBar.Progress = 0;
                int i = 0;
                CurrentJigsaw.IsTilePositionInitial = true;
                JigsawPage.CurrentJigsaw.IsDeleted = false;

                // Add background image
                SetAbsoluteLayoutBackground();

                // Reset tils locations to original
                // And add new tile to the container
                foreach (KeyValuePair<View, Tile> entry in Tile.Dictionary)
                {
                    entry.Value.Row = entry.Value.OriginalRow;
                    entry.Value.Col = entry.Value.OriginalCol;
                    entry.Value.TileView.IsVisible = false;
                    tiles[entry.Value.Row, entry.Value.Col] = entry.Value;
                    absoluteLayout.Children.Add(entry.Value.TileView);

                    double progress = (double)(i++) / (double)(Tile.Dictionary.Count);
                    await progressBar.ProgressTo(progress, 1, Easing.Linear);
                    labelInfor.Text = $"{XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_ResetJigsaw}: {string.Format("{0:N0}%", progress * 100)}";
                }
                progressBar.IsVisible = false;

                await FillGrid((View)absoluteLayout.Parent);

                CurrentJigsaw.IsTilePositionChanged = false;
                ShowHideControls();

                UpdateStatusBar(CurrentJigsaw);
            }
        }

        void OnSaveButtonClicked(object sender, EventArgs args)
        {
            saveButton.IsVisible = false;
            IsProcessing = true;
            if ((Utility.IsiOS() && AdMobService.IsiOSPlayAd) ||
                (!Utility.IsiOS() && AdMobService.IsAndroidPlayAd))
                adHandler.PlayAds();
            else
                SaveCurrentJigsaw(true);
        }

        async void OnPlaybuttonClicked(object sender, EventArgs args)
        {
            if (!IsGameMode)
            {
                IsGameMode = true;
                await Task.Delay(TimeSpan.FromSeconds(0.1));
                await Device.InvokeOnMainThreadAsync(() =>
                {
                    RandomizeTiles(100);
                });
                timer = new Stopwatch();
                timer.Start();
                Device.StartTimer(TimeSpan.FromSeconds(0.1), () =>
                {
                    TimeSpan ts = timer.Elapsed;
                    string formatTS = Utility.FormatTimeSpan(ts);
                    Device.InvokeOnMainThreadAsync(() =>
                    {
                        labelTimer.Text = formatTS;
                    });
                    return IsGameMode;
                });
            }
            else
            {
                timer.Stop();
                var result = await DisplayAlert(XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_GamePause,
                    XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_ConfirmQuitGame,
                    XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_Yes,
                    XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_No);
                if (result)
                {
                    IsGameMode = false;
                }
                else
                {
                    timer.Start();
                }
            }
        }

        void OnSelectImageClicked(object sender, EventArgs args)
        {
            new ImageCropper()
            {
                PageTitle = XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_ChooseAnImage,
                AspectRatioX = App.ShellInstance.JigsawSettings.AspectRatio_X,
                AspectRatioY = App.ShellInstance.JigsawSettings.AspectRatio_Y,
                CropShape = ImageCropper.CropShapeType.Rectangle,
                SelectSourceTitle = XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_ChooseImage,
                TakePhotoTitle = XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_TakePhote,
                PhotoLibraryTitle = XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_ImageLibrary,
                CancelButtonTitle = XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_Canel,
                CropButtonTitle = XJigsaw.Resources.AppResources.XJigsaw_Jigsaw_Snapshot,
                Success = (imageFile) =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        SourceImagePage.SourceImage.Source = ImageSource.FromFile(imageFile);
                        Size imageSize = Utility.GetImageSize(imageFile);
                        JigsawPage.CurrentJigsaw = new Jigsaw();
                        JigsawPage.CurrentJigsaw.IsTilePositionInitial = true;
                        JigsawPage.CurrentJigsaw.IsDeleted = false;
                        JigsawPage.CurrentJigsaw.Bytes = File.ReadAllBytes(imageFile);
                        IEditableImage imageEdit = await CrossImageEdit.Current.CreateImageAsync(JigsawPage.CurrentJigsaw.Bytes);
                        IEditableImage imageEditSmall = imageEdit.Resize(Convert.ToInt32(imageSize.Width * SMALL_IMAGE_PERCENT), Convert.ToInt32(imageSize.Height * SMALL_IMAGE_PERCENT));
                        JigsawPage.CurrentJigsaw.BytesSmall = imageEditSmall.ToJpeg(100);
                        JigsawPage.CurrentJigsaw.AspectRatio_X = App.ShellInstance.JigsawSettings.AspectRatio_X;
                        JigsawPage.CurrentJigsaw.AspectRatio_Y = App.ShellInstance.JigsawSettings.AspectRatio_Y;
                        JigsawPage.CurrentJigsaw.ImageRatio = App.ShellInstance.JigsawSettings.GetCorrectImageRatio(AppResources.Culture.Name);
                        JigsawPage.CurrentJigsaw.ImageWidth = (long)imageSize.Width;
                        JigsawPage.CurrentJigsaw.ImageHeight = (long)imageSize.Height;
                        JigsawPage.CurrentJigsaw.Level = App.ShellInstance.JigsawSettings.Level;
                        JigsawPage.CurrentJigsaw.Opacity = App.ShellInstance.JigsawSettings.Opacity;
                        UpdateStatusBar(JigsawPage.CurrentJigsaw);
                        absoluteLayoutResize((View)stackLayout.Parent);
                        await InitGridAsync();
                    });
                },
                Faiure = () =>
                {
                    //Console.WriteLine("Failed to get an image.");
                }

            }.Show(this);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (isContainerResized == true && CurrentJigsaw != null &&
                CurrentJigsaw.IsSelected == true && CurrentJigsaw.IsApplied == false && CurrentJigsaw.IsSelectedItemNotProcessed == true)
            {
                CurrentJigsaw.IsSelectedItemNotProcessed = false; // in order to prevent the OnAppearing logic exec multiple times

                absoluteLayoutResize((View)stackLayout.Parent);

                // update sattus bar
                UpdateStatusBar(JigsawPage.CurrentJigsaw);

                await InitGridAsync();
            }
            else if (CurrentJigsaw != null && CurrentJigsaw.IsDeleted)
            {
                // Inorder to udpate the jigsaw ID to empty
                UpdateStatusBar(JigsawPage.CurrentJigsaw);
            }

            ShowHideControls(CurrentJigsaw.IsFreshNew);

        }

        public async void ShowPrivacyPolicy(bool isReadPrivacy)
        {
            if (!Utility.IsiOS() && Utility.CHECK_POLICY_READ && !isReadPrivacy)
            {
                PolicyPage policy = new PolicyPage();
                await this.Navigation.PushPopupAsync(policy);
            }
        }
    }
}
