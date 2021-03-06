﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FireWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int width = 400;
        const int height = 400;
        const int light = 0xFF;
        const int black = 0xFF << 24;
        const int yellow = black | light << 16 | light << 8;
        const int gher = 3;
        const int wind = 0;
        const int flame = 15;
        readonly Int32Rect rect = new Int32Rect(0, 0, width, height);
        readonly MyRandom random = new MyRandom(85533);
        readonly WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
        readonly int ptr;
        readonly Thread t;
        readonly List<int> cursorX = new List<int>();
        readonly List<int> cursorY = new List<int>();
        int framesShown = 0;

        public MainWindow()
        {
            InitializeComponent();

            ptr = (int)bitmap.BackBuffer;
            bitmap.Lock();
            unsafe
            {
                int index = width * (height - 1) * 4 + ptr;
                int last = width * height * 4 + ptr;
                for (; index < last; index += 4)
                    *((int*)index) = yellow;
            }
            bitmap.AddDirtyRect(rect);
            bitmap.Unlock();
            imageControl.Source = bitmap;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;
            timer.Start();

            t = new Thread(() =>
            {
                while (true)
                {
                    int i, j, color;
                    int last = (width * (height - 1) - gher + 1) * 4 + ptr;

                    unsafe
                    {
                        for (i = wind * 4 + ptr; i < last; i += 4)
                        {
                            color = *((int*)(i + width * 4));

                            if (random.Next(100) < flame)
                                color = NextColor(color);

                            *((int*)(i + (random.Next(gher) - wind) * 4)) = color;
                        }

                        for (int k = 0; k < cursorX.Count; k++)
                            for (i = cursorX[k] - 10; i < cursorX[k] + 10; i++)
                                if (i >= 0 && i < width)
                                    for (j = cursorY[k] - 10; j < cursorY[k] + 10; j++)
                                        if (j >= 0 && j < height && Math.Pow(i - cursorX[k], 2) + Math.Pow(j - cursorY[k], 2) <= 100)
                                            *((int*)((j * width + i) * 4 + ptr)) = yellow;
                        if (cursorX.Count > 1)
                        {
                            cursorX.RemoveRange(0, cursorX.Count - 1);
                            cursorY.RemoveRange(0, cursorY.Count - 1);
                        }
                    }
                    framesShown++;

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        bitmap.Lock();
                        bitmap.AddDirtyRect(rect);
                        bitmap.Unlock();
                    }));
                }
            });
            t.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Title = "Fire - FPS: " + framesShown;
            framesShown = 0;
        }

        int NextColor(int color)
        {

            if (color == black)
                return color;

            int b = color & 0xFF;
            if (b != 0)
                return color;

            int g = (color >> 8) & 0xFF;
            int r = (color >> 16) & 0xFF;

            if (r == light && g > 0)
            {
                if (g >= flame)
                    g = g - flame;
                else
                    g = 0;
                return black | (r << 16) | (g << 8);
            }
            if (r > 0 && g == 0)
            {
                if (r >= flame)
                    r = r - flame;
                else
                    r = 0;
                return black | (r << 16);
            }

            return color;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ClearValue(SizeToContentProperty);
            imageControl.ClearValue(WidthProperty);
            imageControl.ClearValue(HeightProperty);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            t.Abort();
        }

        private void imageControl_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(imageControl);
            cursorX.Add((int)(p.X * width / imageControl.ActualWidth));
            cursorY.Add((int)(p.Y * height / imageControl.ActualHeight));
        }
    }
}
