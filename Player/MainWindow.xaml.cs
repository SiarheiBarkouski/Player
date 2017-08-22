using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TimeSpan ts1;
        TimeSpan ts2;
        Timer timer;
        bool fullScreen;
        Point position;

        public MainWindow()
        {
            InitializeComponent();
            meMain.LoadedBehavior = MediaState.Manual;
            meMain.Stretch = Stretch.UniformToFill;

            fullScreen = false;
            timer = new Timer();
            timer.Interval = 200;
            meMain.MediaOpened += meMain_MediaOpened;
            meMain.MediaFailed += (os, rea) => { System.Windows.MessageBox.Show($"{rea.ErrorException.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); };
            timer.Tick += (os, ea) =>
            {
                if ((meMain.NaturalDuration.HasTimeSpan) && (meMain.NaturalDuration.TimeSpan != TimeSpan.Zero))
                {
                    ts1 = meMain.Position;
                    ts2 = meMain.NaturalDuration.TimeSpan - ts1;
                    tbStart.Text = String.Format($"{ts1.Hours:00}:{ts1.Minutes:00}:{ts1.Seconds:00}");
                    tbEnd.Text = String.Format($"{ts2.Hours:00}:{ts2.Minutes:00}:{ts2.Seconds:00}");
                    slDuration.Value = meMain.Position.TotalMilliseconds;
                }
            };

            
        }

        private void miOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    meMain.Source = new Uri(ofd.FileName, UriKind.Absolute);
                    meMain.LoadedBehavior = MediaState.Play;
                    timer.Start();
                }
                slDuration.Value = 0;
                tbStart.Text = "00:00:00";
                tbEnd.Text = "00:00:00";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            meMain.LoadedBehavior = MediaState.Play;
            if ((meMain.NaturalDuration.HasTimeSpan) && (meMain.NaturalDuration.TimeSpan != TimeSpan.Zero))
                timer.Start();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            meMain.Position = TimeSpan.Zero;
            timer.Stop();
            slDuration.Value = 0;
        }

        private void meMain_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (meMain.NaturalDuration.HasTimeSpan)
                slDuration.Maximum = meMain.NaturalDuration.TimeSpan.TotalMilliseconds;
        }

        private void slVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            meMain.Volume = slVolume.Value;
        }

        private void Expander_Drop(object sender, System.Windows.DragEventArgs e)
        {
            Regex pattern = new Regex(@"[.](avi|flc|mp4|mpeg|mp3|wmv|wm)");
            string[] FileList = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop, false);
            foreach (string File in FileList)
            {
                if (pattern.IsMatch(File))
                    lbExpander.Items.Add(File);
                else
                {
                    System.Windows.MessageBox.Show($"{File} не может быть добавлен, т.к. данный офрмат не поддерживается.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void meMain_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!fullScreen)
            {
                TimeSpan curr = meMain.Position;
                mainGrid.Children.Remove(meMain);                
                this.Content = meMain;
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
                meMain.Position = curr;
                fullScreen = true;
            }
            else
            {
                TimeSpan curr = meMain.Position;
                this.Content = mainGrid;
                mainGrid.Children.Add(meMain);                
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.WindowState = WindowState.Normal;
                meMain.Position = curr;
                fullScreen = false;
            }
        }

        private void lbExpander_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            slDuration.Value = 0;
            meMain.Source = new Uri(lbExpander.SelectedItem.ToString());
            meMain.LoadedBehavior = MediaState.Play;
            timer.Start();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://www.google.by/search?q=%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%82%D1%8B+%D0%B2%D0%B8%D0%B4%D0%B5%D0%BE%D1%84%D0%B0%D0%B9%D0%BB%D0%BE%D0%B2+windows+media+player&oq=%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%82%D1%8B+%D0%B2%D0%B8%D0%B4%D0%B5%D0%BE%D1%84%D0%B0%D0%B9%D0%BB%D0%BE%D0%B2+windows+media+player&aqs=chrome..69i57.1363j0j8&sourceid=chrome&ie=UTF-8");
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            meMain.LoadedBehavior = MediaState.Pause;
            timer.Stop();
        }

        private void slDuration_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pts = e.GetPosition(slDuration);
            double total = this.slDuration.Maximum;
            double res = ((pts.X * 100) / ((double)this.slDuration.ActualWidth)) / 100;
            meMain.Position = TimeSpan.FromMilliseconds(total * res);
        }

        private void meMain_MediaEnded(object sender, RoutedEventArgs e)
        {
            foreach (var item in lbExpander.Items)
                if (meMain.Source.Equals(item.ToString()))
                {
                    if (lbExpander.Items.IndexOf(item) < lbExpander.Items.Count - 1)
                    {
                        int next = lbExpander.Items.IndexOf(item) + 1;
                        meMain.Source = new Uri((lbExpander.Items.GetItemAt(next)).ToString());
                        lbExpander.SelectedItem = lbExpander.Items.GetItemAt(next);
                        meMain.LoadedBehavior = MediaState.Play;
                        timer.Start();
                        return;
                    }
                }
        }

        private void slDuration_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Slider dur = sender as Slider;
            Point x = e.GetPosition(dur);
            if (position.X == 0)
            {
                position.X = x.X;
                position.Y = x.Y;                
            }
            double newValue = x.X / dur.ActualWidth * dur.Maximum;
            TimeSpan curr = TimeSpan.FromMilliseconds(newValue);
            lbDuration.Content = String.Format($"{curr.Hours:00}:{curr.Minutes:00}:{curr.Seconds:00}");
            tt.HorizontalOffset = x.X - position.X;
            tt.VerticalOffset = x.Y - position.Y;
        }

        private void tt_Loaded(object sender, RoutedEventArgs e)
        {
            position.X = 0;
            position.Y = 0;
        }
    }
}

