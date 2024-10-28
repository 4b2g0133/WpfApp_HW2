using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp_HW2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // 字典儲存飲料名稱和價格
        Dictionary<string, int> drinks = new Dictionary<string, int>();

        // 字典儲存客戶訂購的飲料及數量
        Dictionary<string, int> orders = new Dictionary<string, int>();

        // 儲存取餐方式 (內用或外帶)
        string takeout = "";

        public MainWindow()
        {
            InitializeComponent();

            // 初始化並新增飲料至飲料菜單
            AddNewDrink(drinks);

            // 顯示飲料菜單
            DisplayDrinkMenu(drinks);
        }

        // 開啟檔案並新增飲料到字典
        private void AddNewDrink(Dictionary<string, int> drinks)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV檔案|*.csv|文字檔案|*.txt|所有檔案|*.*";
            openFileDialog.Title = "選擇飲料菜單檔案";
            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName;
                ReadDrinksFromFile(fileName, drinks);
            }
        }

        // 從檔案讀取飲料名稱和價格，並將其加入字典
        private void ReadDrinksFromFile(string fileName, Dictionary<string, int> drinks)
        {
            try
            {
                // 讀取指定檔案中的所有行，並將結果儲存到 lines 陣列中
                string[] lines = File.ReadAllLines(fileName);

                // 逐行處理檔案中的每一行
                foreach (var line in lines)
                {
                    // 檢查是否為空行，或是否缺少逗號分隔的兩個部分
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // 將每一行的內容用逗號分隔，並儲存到 tokens 陣列中
                    string[] tokens = line.Split(',');

                    // 確認 tokens 陣列至少有兩個元素（名稱和價格）
                    if (tokens.Length < 2)
                    {
                        MessageBox.Show($"行的格式無效: {line}");
                        continue; // 跳過這行，繼續處理下一行
                    }

                    // 將第一個元素作為飲料名稱
                    string drinkName = tokens[0];

                    // 嘗試將第二個元素轉換為整數，作為價格
                    if (int.TryParse(tokens[1], out int price))
                    {
                        drinks.Add(drinkName, price);
                    }
                    else
                    {
                        MessageBox.Show($"無效的價格格式: {tokens[1]}");
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                // 如果在檔案讀取過程中發生錯誤，顯示錯誤訊息
                MessageBox.Show($"讀取檔案時發生錯誤: {ex.Message}");
            }
        }

        // 在視窗中顯示飲料菜單
        private void DisplayDrinkMenu(Dictionary<string, int> drinks)
        {
            stackpanel_DrinkMenu.Children.Clear();
            stackpanel_DrinkMenu.Height = drinks.Count * 40;

            // 對每一項飲料建立 UI 元素
            foreach (var drink in drinks)
            {
                // 建立一個水平 StackPanel 來顯示飲料項目
                var sp = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(2),
                    Height = 35,
                    VerticalAlignment = VerticalAlignment.Center,
                    Background = Brushes.AliceBlue
                };

                // 飲料名稱和價格的 CheckBox
                var cb = new CheckBox
                {
                    Content = $"{drink.Key} {drink.Value}元",
                    FontFamily = new FontFamily("微軟正黑體"),
                    FontSize = 18,
                    Foreground = Brushes.Blue,
                    Margin = new Thickness(10, 0, 40, 0),
                    VerticalContentAlignment = VerticalAlignment.Center
                };

                // 數量選擇用的 Slider
                var sl = new Slider
                {
                    Width = 150,
                    Value = 0,
                    Minimum = 0,
                    Maximum = 10,
                    IsSnapToTickEnabled = true,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                // 顯示數量的 Label
                var lb = new Label
                {
                    Width = 30,
                    Content = "0",
                    FontFamily = new FontFamily("微軟正黑體"),
                    FontSize = 18,
                };

                // 將 Slider 的值綁定到 Label 的內容上
                Binding myBinding = new Binding("Value")
                {
                    Source = sl,
                    Mode = BindingMode.OneWay
                };
                lb.SetBinding(ContentProperty, myBinding);

                // 將控制項加入 StackPanel
                sp.Children.Add(cb);
                sp.Children.Add(sl);
                sp.Children.Add(lb);

                // 將 StackPanel 加入顯示飲料菜單的 StackPanel
                stackpanel_DrinkMenu.Children.Add(sp);
            }
        }

        // 更新取餐方式
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var rb = sender as RadioButton;
            if ((rb.IsChecked == true))
            {
                takeout = rb.Content.ToString();
                // MessageBox.Show($"方式: {takeout}");
            }
        }

        // 處理客戶下訂單的事件
        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            ResultTextBlock.Text = "";
            string discoutMessage = "";

            // 清空先前的訂單資料
            orders.Clear();

            // 確認所有訂單的品項
            for (int i = 0; i < stackpanel_DrinkMenu.Children.Count; i++)
            {
                var sp = stackpanel_DrinkMenu.Children[i] as StackPanel;
                var cb = sp.Children[0] as CheckBox;
                var sl = sp.Children[1] as Slider;
                var lb = sp.Children[2] as Label;

                if (cb.IsChecked == true && sl.Value > 0)
                {
                    // 取得飲料名稱和數量
                    string drinkName = cb.Content.ToString().Split(' ')[0];
                    orders.Add(drinkName, int.Parse(lb.Content.ToString()));
                }
            }

            // 計算訂單金額和折扣
            double total = 0.0;
            double sellPrice = 0.0;
            string orderMessage = "";
            DateTime now = DateTime.Now;

            // 訂單資訊
            orderMessage += $"訂購時間: {now.ToString("yyyy/MM/dd HH:mm:ss")}\n";
            orderMessage += $"取餐方式: {takeout}\n";
            int num = 1;

            // 計算每個飲料項目的價格
            foreach (var item in orders)
            {
                string drinkName = item.Key;
                int quantity = item.Value;
                int price = drinks[drinkName];
                int subTotal = price * quantity;
                total += subTotal;
                orderMessage += $"{num}. {drinkName} x {quantity}杯，共{subTotal}元\n";
                num++;
            }

            // 折扣計算
            if (total >= 500)
            {
                discoutMessage = "滿500元打8折";
                sellPrice = total * 0.8;
            }
            else if (total >= 300)
            {
                discoutMessage = "滿300元打9折";
                sellPrice = total * 0.9;
            }
            else
            {
                discoutMessage = "無折扣";
                sellPrice = total;
            }

            // 顯示訂單總金額及折扣
            orderMessage += $"總金額: {total}元\n";
            orderMessage += $"{discoutMessage}，實付金額: {sellPrice}元\n";
            ResultTextBlock.Text = orderMessage;

            // 儲存訂單
            SaveOrder(orderMessage);
        }

        // 將訂單資訊儲存到文字檔
        private void SaveOrder(string orderMessage)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "文字檔案|*.txt|所有檔案|*.*";
            saveFileDialog.Title = "儲存訂單";
            if (saveFileDialog.ShowDialog() == true)
            {
                string fileName = saveFileDialog.FileName;

                try
                {
                    using (StreamWriter sw = new StreamWriter(fileName))
                    {
                        sw.Write(orderMessage);
                    }
                    MessageBox.Show("訂單已成功儲存。");
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"儲存檔案時發生錯誤: {ex.Message}");
                }
            }
        }
    }
}