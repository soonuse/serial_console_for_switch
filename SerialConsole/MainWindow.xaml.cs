using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SerialConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static SerialPort com = null;
        public static UsbJoystickReport report = new UsbJoystickReport();
        public static object serialLock = new object();
        public static Dictionary<string, object> settings;
        public MainWindow()
        {
            InitializeComponent();
            report.ResetReport();
            ListPorts();
            ListBaudRates();
            SetTextBoxStyle(true, Brushes.LightGray);
            SetButtonStyle(Visibility.Hidden);
            settings = GetSettings();
            SetTextBoxFromSettings();
        }
        public Dictionary<string, object> GetDefaultSettings()
        {
            var defaultSettings = new Dictionary<string, object>();
            var buttons = new Dictionary<string, string>
            {
                ["A"] = Key.K.ToString(),
                ["B"] = Key.J.ToString(),
                ["X"] = Key.I.ToString(),
                ["Y"] = Key.U.ToString(),
                ["L"] = Key.L.ToString(),
                ["R"] = Key.OemSemicolon.ToString(),
                ["ZL"] = Key.O.ToString(),
                ["ZR"] = Key.P.ToString(),
                ["LCLICK"] = Key.OemPeriod.ToString(),
                ["RCLICK"] = Key.OemQuestion.ToString(),
                ["MINUS"] = Key.OemMinus.ToString(),
                ["PLUS"] = Key.OemPlus.ToString(),
                ["HOME"] = Key.B.ToString(),
                ["CAPTURE"] = Key.V.ToString(),
            };
            var hat = new Dictionary<string, string>
            {
                ["LEFT"] = Key.F.ToString(),
                ["TOP"] = Key.T.ToString(),
                ["RIGHT"] = Key.H.ToString(),
                ["BOTTOM"] = Key.G.ToString(),
            };
            var stick = new Dictionary<string, string>
            {
                ["LX_MIN"] = Key.A.ToString(),
                ["LX_MAX"] = Key.D.ToString(),
                ["LY_MIN"] = Key.W.ToString(),
                ["LY_MAX"] = Key.S.ToString(),
                ["RX_MIN"] = Key.Left.ToString(),
                ["RX_MAX"] = Key.Right.ToString(),
                ["RY_MIN"] = Key.Up.ToString(),
                ["RY_MAX"] = Key.Down.ToString(),
            };
            defaultSettings["BUTTON"] = buttons;
            defaultSettings["HAT"] = hat;
            defaultSettings["STICK"] = stick;
            return defaultSettings;
        }
        public Dictionary<string, object> GetSettings()
        {
            if (File.Exists("./settings.json"))
            {
                string content = File.ReadAllText("./settings.json");
                var serializer = new JavaScriptSerializer();
                settings = serializer.Deserialize<Dictionary<string, object>>(content);
            }
            else
            {
                settings = GetDefaultSettings();
                var serializer = new JavaScriptSerializer();
                File.WriteAllText("./settings.json", serializer.Serialize(settings));
            }
            return settings;
        }
        private void SetButtonStyle(Visibility visibility)
        {
            System.Windows.Controls.Button[] buttons =
            {
                resetButton, okButton, cancelButton,
            };
            foreach (var b in buttons)
            {
                b.Visibility = visibility;
            }
        }
        private void SetTextBoxStyle(bool isReadOnly, SolidColorBrush backgroundColor)
        {
            TextBox[] textBoxes =
            {
                lTextBox, zlTextBox, rTextBox, zrTextBox,
                minusTextBox, plusTextBox, captureTextBox, homeMaxTextBox,
                lxMinTextBox, lxMaxTextBox, lyMinTextBox, lyMaxTextBox,
                rxMinTextBox, rxMaxTextBox, ryMinTextBox, ryMaxTextBox,
                lClickTextBox, rClickTextBox,
                leftTextBox, topTextBox, rightTextBox, bottomTextBox,
                aTextBox, bTextBox, xTextBox, yTextBox,
            };
            foreach (var t in textBoxes)
            {
                t.IsReadOnly = isReadOnly;
                t.Background = backgroundColor;
            }
        }
        private void ListPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            portComboBox.ItemsSource = ports;
        }
        private void ListBaudRates()
        {
            int[] baudRates = { 9600, 115200, 3000000 };
            baudRateComboBox.ItemsSource = baudRates;
        }
        private void OpenSerial()
        {
            if (portComboBox.SelectedItem == null)
            {
                MessageBox.Show("No COM port selected...");
                return;
            }
            if (com == null)
            {
                com = new SerialPort(portComboBox.SelectedItem.ToString());
            }
            if (baudRateComboBox.SelectedItem != null)
            {
                SetBaudRate();
            }
            else
            {
                com.BaudRate = 115200;
                baudRateComboBox.SelectedItem = 115200;
            }
            try
            {
                com.Open();
            }
            catch (System.UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message);
                com = null;
                return;
            }
            openSerialButton.Background = Brushes.LightGreen;
            openSerialButton.Content = "Running";
        }
        private void SetBaudRate()
        {
            if (com != null)
            {
                com.BaudRate = int.Parse(baudRateComboBox.SelectedItem.ToString());
            }
        }
        private void CloseSerial()
        {
            com.Close();
            com = null;
            openSerialButton.Background = Brushes.LightGray;
            openSerialButton.Content = "Open";
        }
        private void openSerialButton_Click(object sender, RoutedEventArgs e)
        {
            if (com != null)
            {
                CloseSerial();
            }
            else
            {
                OpenSerial();
            }
        }

        private void baudRateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (com == null)
            {
                return;
            }
            if (com.IsOpen)
            {
                com.Close();
                SetBaudRate();
                try
                {
                    com.Open();
                }
                catch (System.UnauthorizedAccessException ex)
                {
                    MessageBox.Show(ex.Message);
                    com = null;
                }
            }
            else
            {
                SetBaudRate();
            }
            // Avoid being changed by the mouse wheel or up/down keys
            openSerialButton.Focus();
        }

        private void SendReport()
        {
            lock (serialLock)
            {
                byte[] toSend = report.GetSerialBytes();
                com.Write(toSend, 0, toSend.Length);
            }
        }

        private void SetReportButtons()
        {
            dynamic buttons = settings["BUTTON"];
            foreach (var kv in buttons)
            {
                Button button = (Button)Enum.Parse(typeof(Button), kv.Key);
                Key key = (Key)Enum.Parse(typeof(Key), kv.Value);
                if (Keyboard.IsKeyDown(key))
                {
                    report.buttons |= (ushort)button;
                }
            }
        }
        private void SetReportHat()
        {
            dynamic hatButtons = settings["HAT"];
            Key leftKey = (Key)Enum.Parse(typeof(Key), hatButtons["LEFT"]);
            Key topKey = (Key)Enum.Parse(typeof(Key), hatButtons["TOP"]);
            Key rightKey = (Key)Enum.Parse(typeof(Key), hatButtons["RIGHT"]);
            Key bottomKey = (Key)Enum.Parse(typeof(Key), hatButtons["BOTTOM"]);
            if (Keyboard.IsKeyDown(topKey))
            {
                if (Keyboard.IsKeyDown(leftKey))
                {
                    report.hat = (byte)Hat.TOP_LEFT;
                }
                else if (Keyboard.IsKeyDown(rightKey))
                {
                    report.hat = (byte)Hat.TOP_RIGHT;
                }
                else
                {
                    report.hat = (byte)Hat.TOP;
                }
            }
            else if (Keyboard.IsKeyDown(rightKey))
            {
                if (Keyboard.IsKeyDown(bottomKey))
                {
                    report.hat = (byte)Hat.BOTTOM_RIGHT;
                }
                else
                {
                    report.hat = (byte)Hat.RIGHT;
                }
            }
            else if (Keyboard.IsKeyDown(bottomKey))
            {
                if (Keyboard.IsKeyDown(leftKey))
                {
                    report.hat = (byte)Hat.BOTTOM_LEFT;
                }
                else
                {
                    report.hat = (byte)Hat.BOTTOM;
                }
            }
            else if (Keyboard.IsKeyDown(leftKey))
            {
                report.hat = (byte)Hat.LEFT;
            }
        }
        private void SetReportStick()
        {
            dynamic stick = settings["STICK"];
            Key lxMinKey = (Key)Enum.Parse(typeof(Key), stick["LX_MIN"]);
            Key lxMaxKey = (Key)Enum.Parse(typeof(Key), stick["LX_MAX"]);
            Key lyMinKey = (Key)Enum.Parse(typeof(Key), stick["LY_MIN"]);
            Key lyMaxKey = (Key)Enum.Parse(typeof(Key), stick["LY_MAX"]);
            Key rxMinKey = (Key)Enum.Parse(typeof(Key), stick["RX_MIN"]);
            Key rxMaxKey = (Key)Enum.Parse(typeof(Key), stick["RX_MAX"]);
            Key ryMinKey = (Key)Enum.Parse(typeof(Key), stick["RY_MIN"]);
            Key ryMaxKey = (Key)Enum.Parse(typeof(Key), stick["RY_MAX"]);
            if (Keyboard.IsKeyDown(lxMinKey))
            {
                report.lx = (byte)Stick.MIN;
            }
            else if (Keyboard.IsKeyDown(lxMaxKey))
            {
                report.lx = (byte)Stick.MAX;
            }
            if (Keyboard.IsKeyDown(lyMinKey))
            {
                report.ly = (byte)Stick.MIN;
            }
            else if (Keyboard.IsKeyDown(lyMaxKey))
            {
                report.ly = (byte)Stick.MAX;
            }
            if (Keyboard.IsKeyDown(ryMinKey))
            {
                report.ry = (byte)Stick.MIN;
            }
            else if (Keyboard.IsKeyDown(ryMaxKey))
            {
                report.ry = (byte)Stick.MAX;
            }
            if (Keyboard.IsKeyDown(rxMinKey))
            {
                report.rx = (byte)Stick.MIN;
            }
            else if (Keyboard.IsKeyDown(rxMaxKey))
            {
                report.rx = (byte)Stick.MAX;
            }
        }
        private void SetReport()
        {
            report.ResetReport();
            SetReportButtons();
            SetReportHat();
            SetReportStick();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (com == null || !com.IsOpen)
            {
                return;
            }
            SetReport();
            SendReport();
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (com == null || !com.IsOpen)
            {
                return;
            }
            SetReport();
            SendReport();
        }

        private void SetTextBoxFromSettings()
        {
            dynamic buttons = settings["BUTTON"];
            dynamic stick = settings["STICK"];
            dynamic hat = settings["HAT"];
            lTextBox.Text = buttons["L"];
            rTextBox.Text = buttons["R"];
            zlTextBox.Text = buttons["ZL"];
            zrTextBox.Text = buttons["ZR"];
            minusTextBox.Text = buttons["MINUS"];
            plusTextBox.Text = buttons["PLUS"];
            lxMinTextBox.Text = stick["LX_MIN"];
            lxMaxTextBox.Text = stick["LX_MAX"];
            lyMinTextBox.Text = stick["LY_MIN"];
            lyMaxTextBox.Text = stick["LY_MAX"];
            leftTextBox.Text = hat["LEFT"];
            topTextBox.Text = hat["TOP"];
            rightTextBox.Text = hat["RIGHT"];
            bottomTextBox.Text = hat["BOTTOM"];
            aTextBox.Text = buttons["A"];
            bTextBox.Text = buttons["B"];
            xTextBox.Text = buttons["X"];
            yTextBox.Text = buttons["Y"];
            rxMinTextBox.Text = stick["RX_MIN"];
            rxMaxTextBox.Text = stick["RX_MAX"];
            ryMinTextBox.Text = stick["RY_MIN"];
            ryMaxTextBox.Text = stick["RY_MAX"];
            lClickTextBox.Text = buttons["LCLICK"];
            rClickTextBox.Text = buttons["RCLICK"];
            homeMaxTextBox.Text = buttons["HOME"];
            captureTextBox.Text = buttons["CAPTURE"];
        }
        private void ButtonSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SetButtonStyle(Visibility.Visible);
            SetTextBoxStyle(false, Brushes.White);
        }
        public Dictionary<string, object> GetSettingsFromTextBox()
        {
            var textBoxDict = new Dictionary<string, object>();
            var buttons = new Dictionary<string, string>
            {
                ["A"] = aTextBox.Text,
                ["B"] = bTextBox.Text,
                ["X"] = xTextBox.Text,
                ["Y"] = yTextBox.Text,
                ["L"] = lTextBox.Text,
                ["R"] = rTextBox.Text,
                ["ZL"] = zlTextBox.Text,
                ["ZR"] = zrTextBox.Text,
                ["LCLICK"] = lClickTextBox.Text,
                ["RCLICK"] = rClickTextBox.Text,
                ["MINUS"] = minusTextBox.Text,
                ["PLUS"] = plusTextBox.Text,
                ["HOME"] = homeMaxTextBox.Text,
                ["CAPTURE"] = captureTextBox.Text,
            };
            var hat = new Dictionary<string, string>
            {
                ["LEFT"] = leftTextBox.Text,
                ["TOP"] = topTextBox.Text,
                ["RIGHT"] = rightTextBox.Text,
                ["BOTTOM"] = bottomTextBox.Text,
            };
            var stick = new Dictionary<string, string>
            {
                ["LX_MIN"] = lxMinTextBox.Text,
                ["LX_MAX"] = lxMaxTextBox.Text,
                ["LY_MIN"] = lyMinTextBox.Text,
                ["LY_MAX"] = lyMaxTextBox.Text,
                ["RX_MIN"] = rxMinTextBox.Text,
                ["RX_MAX"] = rxMaxTextBox.Text,
                ["RY_MIN"] = ryMinTextBox.Text,
                ["RY_MAX"] = ryMaxTextBox.Text,
            };
            textBoxDict["BUTTON"] = buttons;
            textBoxDict["HAT"] = hat;
            textBoxDict["STICK"] = stick;
            return textBoxDict;
        }
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            settings = GetSettingsFromTextBox();
            var serializer = new JavaScriptSerializer();
            File.WriteAllText("./settings.json", serializer.Serialize(settings));
            SetTextBoxFromSettings();
            SetButtonStyle(Visibility.Hidden);
            SetTextBoxStyle(true, Brushes.LightGray);
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            SetTextBoxFromSettings();
            SetButtonStyle(Visibility.Hidden);
            SetTextBoxStyle(true, Brushes.LightGray);
        }

        private void textBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.IsReadOnly)
            {
                return;
            }
            Key key;
            if (e.ImeProcessedKey == Key.None)
            {
                key = e.Key;
            }
            else
            {
                key = e.ImeProcessedKey;
            }
            textBox.Text = key.ToString();
            textBox.SelectionStart = textBox.Text.Length;
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Reset to default key map?", "Reset", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.Cancel)
            {
                return;
            }
            settings = GetDefaultSettings();
            var serializer = new JavaScriptSerializer();
            File.WriteAllText("./settings.json", serializer.Serialize(settings));
            SetTextBoxFromSettings();
            SetButtonStyle(Visibility.Hidden);
            SetTextBoxStyle(true, Brushes.LightGray);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(@"This is a free software and
you can download it from:

https://github.com/soonuse/", "About");
        }

        private void baudRateComboBox_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }
    }
}
