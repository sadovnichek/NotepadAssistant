using System;
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
        private string fromCursorOldText;
        private string fromFocusOldText;
        private System.Windows.Media.Brush StartButtonColor;

        public MainWindow()
        {
            InitializeComponent();
            speech = new SpeechSynthesizer();
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 1);
        }

        private void AddElement(AutomationElement element, ref string oldText)
        {
            if (oldText != element.Current.Name && element.Current.ControlType == ControlType.MenuItem)
            {
                TextBox.Items.Add(element.Current.Name);
                speech.Speak(element.Current.Name);
                oldText = element.Current.Name;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            var fromCursor = ElementFromCursor();
            var fromFocus = AutomationElement.FocusedElement;
            if (fromCursor.Current.Name == fromFocus.Current.Name)
                AddElement(fromFocus, ref fromFocusOldText);
            else
            {
                AddElement(fromCursor, ref fromCursorOldText);
                AddElement(fromFocus, ref fromFocusOldText);
            }
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            StartButtonColor = ButtonStart.Background;
            ButtonStart.Background = System.Windows.Media.Brushes.Green;
            Automate();
        }

        private AutomationElement ElementFromCursor()
        {
            var x = System.Windows.Forms.Cursor.Position.X;
            var y = System.Windows.Forms.Cursor.Position.Y;
            return AutomationElement.FromPoint(new System.Windows.Point(x, y));
        }

        private void Automate()
        {
            AutomationElement rootElement = AutomationElement.RootElement;
            if (rootElement != null)
            {
                System.Windows.Automation.Condition findNotepad = new PropertyCondition(AutomationElement.NameProperty, "Безымянный – Блокнот");
                var appElement = rootElement.FindFirst(TreeScope.Children, findNotepad);
                if (appElement != null)
                {
                    timer.Start();
                }
                else
                {
                    MessageBox.Show("Запустите Блокнот", "Error");
                    ButtonStart.Background = StartButtonColor;
                }
            }
        }
    }
}