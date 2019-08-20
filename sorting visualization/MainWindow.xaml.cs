using System;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Controls;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
namespace sorting_visualization
{
    public partial class MainWindow : Window
    {
        private int d = 100;//минимальная задержка 
        private double r = 130;//длина палочки
        private double a = 140, b = 230;//отступы от краёв формы для построения палочек
        private int[] numbers;//массив значений
        private Line[] lines;//массив линий 
        private Thread SortingThread;//экземпляр класса Thread для отдельного потока
        public MainWindow()
        {
            InitializeComponent();
            Closed += EndProgram;
            InitializeRandom();
        }
        private int[] ArrayGeneration()
        {
            //Генерирование массива значений от 5 до 175
            Random random = new Random();
            int[] numbers = new int[(int)InputN.Value];
            for (int i = 0; i < (int)InputN.Value; i++)
                numbers[i] = random.Next(5, 175);
            return numbers;
        }
        private void InitializeLines(int[] array)
        {
            //построение палочек на форму
            DrawingCanvas.Children.Clear();
            lines = new Line[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                //создание палочки с нужными характеристиками и добавление на форму
                Line line = new Line();
                line.X1 = a + (double)(i + 1) * 30;
                line.Y1 = b;
                line.X2 = -Math.Cos(((double)array[i] / 180) * Math.PI) * r + line.X1;
                line.Y2 = -Math.Sin(((double)array[i] / 180) * Math.PI) * r + line.Y1;
                line.StrokeThickness = 3;
                SolidColorBrush brush = null;
                switch (SelectColor.SelectedIndex)
                {
                    case 0:
                        {
                            brush = new SolidColorBrush(Color.FromArgb(250, (byte)(array[i] + 70), 0, 0));
                            break;
                        }
                    case 1:
                        {
                            brush = new SolidColorBrush(Color.FromArgb(250, 0, (byte)(array[i] + 70), 0));
                            break;
                        }
                    case 2:
                        {
                            brush = new SolidColorBrush(Color.FromArgb(250, 0, 0, (byte)(array[i]+70)));
                            break;
                        }
                    default:
                        break;
                }
                line.Stroke = brush;
                lines[i] = line;
                DrawingCanvas.Children.Add(line);
            }
        }
        private void EndProgram(object sender, EventArgs e)
        {
            //обработка закрытия программы 
            if (SortingThread != null)
                SortingThread.Abort();
            Close();
        }
        private void RunSorting()
        {
            //вызов сортировки и отлючение кнопок 
            RunButton.IsEnabled = false;
            RandomButton.IsEnabled = false;
            InputN.IsEnabled = false;
            InputDelay.IsEnabled = false;
            OpenBtn.IsEnabled = false;
            SaveBtn.IsEnabled = false;
            SelectBox.IsEnabled = false;
            SelectColor.IsEnabled = false;
            int selectSort = SelectBox.SelectedIndex;
            int selectColor = SelectColor.SelectedIndex;
            SortingThread = new Thread(delegate ()
            {
                Sorting(selectSort, selectColor);
            });
            SortingThread.Start();
        }
        private void Sorting(int SelectSort, int selectColor)
        {
            //запуск сортировки, по окончанию включение кнопок
            //подсчет времени работы сортировки
            //выбор сортировки
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            switch (SelectSort)
            {
                case 0:
                    {
                        numbers = BulbSort(numbers, selectColor);
                        break;
                    }
                case 1:
                    {
                        numbers = InsertionSort(numbers, selectColor);
                        break;
                    }
                case 2:
                    {
                        numbers = ShakerSort(numbers, selectColor);
                        break;
                    }
                default:
                    InitializeRandom();
                    return;
            }
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                RunButton.IsEnabled = false;
                RandomButton.IsEnabled = true;
                InputN.IsEnabled = true;
                InputDelay.IsEnabled = true;
                OpenBtn.IsEnabled = true;
                SaveBtn.IsEnabled = true;
                SelectBox.IsEnabled = true;
                SelectColor.IsEnabled = true;
                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                MessageBox.Show(elapsedTime,"Время работы");
            });
        }
        private int[] BulbSort(int[] input, int SelectColor)
        {
            //сортировка пузырьком
            for (int i = 0; i < input.Length; i++)
            {
                for (int j = i + 1; j < input.Length; j++)
                {
                    if (input[j] < input[i])
                    {
                        int temp = input[j];
                        input[j] = input[i];
                        input[i] = temp;
                        SwapLines(i, j); //изменение положения палочек
                        PrintArray(numbers); //печать массива
                    }
                    Thread.Sleep(d); //задержка
                }
                SetColor(i, numbers[i], SelectColor);//установка цвета
            }
            return input;
        }
        private int[] InsertionSort(int[] input, int SelectColor)
        {
            //сортировка вставками
            int counter = 0;
            for (int i = 1; i < input.Length; i++)
            {
                for (int j = i; j > 0 && input[j - 1] > input[j]; j--)
                {
                    counter++;
                    int tmp = input[j - 1];
                    input[j - 1] = input[j];
                    input[j] = tmp;
                    Thread.Sleep(d); //задержка
                    PrintArray(numbers); //печать массива
                    SwapLines(j-1,j);
                }
            }
            return input;
        }
        private int[] ShakerSort(int[] input, int SelectColor)
        {
            //сортировка смешиванием
            int left = 0,
                right = input.Length - 1,
                count = 0;
            while (left < right)
            {
                for (int i = left; i < right; i++)
                {
                    count++;
                    if (input[i] > input[i + 1])
                    {
                        int tmp = input[i + 1];
                        input[i + 1] = input[i];
                        input[i] = tmp;
                        Thread.Sleep(d/2); //задержка
                        SwapLines(i+1, i); //изменение положения палочек
                    }
                }
                PrintArray(numbers); //печать массива
                SetLine(right);
                SetColor(right,numbers[right], SelectColor);//установка цвета
                right--;
                SetColor(right, numbers[right], SelectColor);//установка цвета
                for (int i = right; i > left; i--)
                {
                    count++;
                    if (input[i - 1] > input[i])
                    {
                        int tmp = input[i - 1];
                        input[i - 1] = input[i];
                        input[i] = tmp;
                        Thread.Sleep(d/2); //задержка
                        SwapLines(i - 1, i); //изменение положения палочек
                    }
                }
                PrintArray(numbers); //печать массива
                SetLine(left);
                SetColor(left, numbers[left], SelectColor);//установка цвета
                left++;
                SetColor(left, numbers[left], SelectColor);//установка цвета
            }
            return input;
        }
        private void SetColor(int index, int gr, int selectColor)
        {
            //изменение цвета палочки, которые на своём месте
            gr += 70;
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                SolidColorBrush brush = null;
                switch (selectColor)
                {
                    case 0: {
                            brush = new SolidColorBrush(Color.FromArgb(250, (byte)gr, 0 , 0));
                            break;
                        }
                    case 1:
                        {
                            brush = new SolidColorBrush(Color.FromArgb(250, 0, (byte)gr, 0));
                            break;
                        }
                    case 2:
                        {
                            brush = new SolidColorBrush(Color.FromArgb(250, 0, 0, (byte)gr));
                            break;
                        }
                    default:
                        break;
                }
                lines[index].Stroke = brush;
            });
        }
        private void SetLine(int index)
        {
            //изменение положения палочки
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                for (int i = 0; i <= index; i++)
                {
                    double Onex1 = lines[i].X1;
                    double Oney1 = lines[i].Y1;
                    lines[i].X2 = -Math.Cos(((double)numbers[i] / 180) * Math.PI) * r + lines[i].X1;
                    lines[i].Y2 = -Math.Sin(((double)numbers[i] / 180) * Math.PI) * r + lines[i].Y1;
                }
            });
        }
        private void SwapLines(int index1, int index2)
        {
            //изменение положения двух палочек
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                double Onex1 = lines[index1].X1;
                double Oney1 = lines[index1].Y1;
                double Twox1 = lines[index2].X1;
                double Twoy1 = lines[index2].Y1;
                lines[index2].X1 = Onex1;
                lines[index2].Y1 = Oney1;
                lines[index2].X2 = -Math.Cos(((double)numbers[index2] / 180) * Math.PI) * r + lines[index2].X1;
                lines[index2].Y2 = -Math.Sin(((double)numbers[index2] / 180) * Math.PI) * r + lines[index2].Y1;
                lines[index1].X1 = Twox1;
                lines[index1].Y1 = Twoy1;
                lines[index1].X2 = -Math.Cos(((double)numbers[index1] / 180) * Math.PI) * r + lines[index1].X1;
                lines[index1].Y2 = -Math.Sin(((double)numbers[index1] / 180) * Math.PI) * r + lines[index1].Y1;
            });
            Line temp = lines[index1];
            lines[index1] = lines[index2];
            lines[index2] = temp;
        }
        private void InitializeRandom()
        {
            //инициализация палочек и значений
            d = (int)InputDelay.Value;
            numbers = ArrayGeneration();
            InitializeLines(numbers);
            OutputTextBox.Children.Clear();
            foreach (int i in numbers)
            {
                Label label = new Label();
                label.Background = Brushes.Wheat;
                label.FontSize = 30;
                label.Content = i;
                label.BorderThickness = new Thickness(4);
                OutputTextBox.Children.Add(label);
            }
            RunButton.IsEnabled = true;
        }
        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeRandom();
        }
        private void InputN_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Quantity.Content = ((int)InputN.Value).ToString();
        }
        private void InputDelay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Delay.Content = ((int)InputDelay.Value).ToString();
        }
        private void PrintArray(int[] array)
        {
            //печать массива значений
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                OutputTextBox.Children.Clear();
                foreach (int i in array)
                {
                    Label label = new Label();
                    label.Background = Brushes.Wheat;
                    label.FontSize = 30;
                    label.Content = i;
                    label.BorderThickness = new Thickness(4);
                    OutputTextBox.Children.Add(label);
                }
            });
        }
        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            //Считывание с файла 
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Text documents (.txt)|*.txt";
                openFileDialog.ShowDialog();
                string temp = File.ReadAllText(openFileDialog.FileName);
                string[] Lines = temp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (Lines.Length > InputN.Maximum || Lines.Length < InputN.Minimum)
                    throw new Exception("Ошибка");
                numbers = new int[Lines.Length];
                for (int i = 0; i < Lines.Length; i++)
                    numbers[i] = Int32.Parse(Lines[i]);
                InitializeLines(numbers);
                OutputTextBox.Children.Clear();
                foreach (int i in numbers)
                {
                    Label label = new Label();
                    label.Background = Brushes.Wheat;
                    label.FontSize = 30;
                    label.Content = i;
                    label.BorderThickness = new Thickness(4);
                    OutputTextBox.Children.Add(label);
                }
                RunButton.IsEnabled = true;
            }
            catch (Exception exce)
            {
                MessageBox.Show(exce.Message);
            }
        }
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //сохранение результата в файл
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text documents (.txt)|*.txt";
                saveFileDialog.ShowDialog();
                string SaveResult="";
                for (int i = 0; i < numbers.Length; i++)
                    SaveResult += numbers[i].ToString() + " ";
                File.WriteAllText(saveFileDialog.FileName, SaveResult);
                MessageBox.Show("Файл сохранён", "Система");
            }
            catch (Exception exce)
            {
                MessageBox.Show(exce.Message);
            }
        }
        private void RunSortingButton_Click(object sender, RoutedEventArgs e)
        {
            RunSorting();
        }
    }
}