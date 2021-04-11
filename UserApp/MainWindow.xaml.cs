using System;
using System.Collections.Generic;
using System.Windows;
using System.Speech.Synthesis;
using System.Windows.Automation;
using System.Windows.Threading;

namespace UserApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly SpeechSynthesizer speech;
        readonly DispatcherTimer timer;
        private string fromCursorOld;
        private string fromFocusOld;
        private System.Windows.Media.Brush color;
        private AutomationElement appElement;
        private List<string> exceptions;
        private bool IsNotepadOpened;

        public MainWindow()
        {
            InitializeComponent();
            exceptions = new List<string>() { "Свернуть", "Развернуть", "Закрыть", "Start", "Текстовый редактор", "", "Безымянный – Блокнот" };
            speech = new SpeechSynthesizer();
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 1);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            string fromCursor = "";
            if (ElementFromCursor() != null)
            {
                fromCursor = ElementFromCursor().Current.Name;
            }
            var fromFocus = AutomationElement.FocusedElement.Current.Name;
            if (fromCursorOld != fromCursor && IsNotepadOpened && !exceptions.Contains(fromCursor))
            {
                TextBox.Items.Add(fromCursor);
                speech.Speak(fromCursor);
                fromCursorOld = fromCursor;
            }
            if (fromFocusOld != fromFocus && IsNotepadOpened && !exceptions.Contains(fromFocus))
            {
                TextBox.Items.Add(fromFocus);
                speech.Speak(fromFocus);
                fromFocusOld = fromFocus;
            }
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            color = ButtonStart.Background;
            ButtonStart.Background = System.Windows.Media.Brushes.Green;
            Automate();
        }

        private AutomationElement ElementFromCursor()
        {
            Rect rectangle;
            try
            {
                rectangle = appElement.Current.BoundingRectangle;
            }
            catch (ElementNotAvailableException)
            {
                IsNotepadOpened = false;
                return null; 
            }
            var x = System.Windows.Forms.Cursor.Position.X;
            var y = System.Windows.Forms.Cursor.Position.Y;
            if (x > rectangle.X && x < rectangle.Right && y > rectangle.Top && y < rectangle.Bottom)
            {
                System.Windows.Point point = new System.Windows.Point(x, y);
                return AutomationElement.FromPoint(point);
            }
            return null;
        }

        private void Automate()
        {
            AutomationElement rootElement = AutomationElement.RootElement;
            if (rootElement != null)
            {
                System.Windows.Automation.Condition condition = new PropertyCondition(AutomationElement.NameProperty, "Безымянный – Блокнот");
                appElement = rootElement.FindFirst(TreeScope.Children, condition);
                if (appElement != null)
                {
                    IsNotepadOpened = true;
                    timer.Start();
                }
                else
                {
                    MessageBox.Show("Запустите Блокнот", "error");
                    ButtonStart.Background = color;
                }
            }
        }
    }
}