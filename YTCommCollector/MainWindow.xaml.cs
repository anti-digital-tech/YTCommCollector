using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace YTCommCollector
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    List<Status> ListStatuses { get; set; } = new List<Status>();

    public MainWindow()
    {
      InitializeComponent();
      Engine engine = new Engine("{ApiKey}");
      //DataGridMain.ItemsSource = ListStatuses;
      UpdateDisplayList();
    }

    void UpdateDisplayList()
    {
      this.DataGridMain.ItemsSource = new ReadOnlyCollection<Status>(ListStatuses); 
    }

    private void TextBox_VideoId_GotFocus(object sender, RoutedEventArgs e)
    {
      if(((TextBox)sender).Text == "<Input Video ID>")
      {
        ((TextBox)sender).Text = "";
        ((TextBox)sender).Foreground = SystemColors.ControlTextBrush;
      }
    }

    private void TextBox_VideoId_LostFocus(object sender, RoutedEventArgs e)
    {
      if ((((TextBox)sender).Text == ""))
      {
        ((TextBox)sender).Text = "<Input Video ID>";
        const string Value = "#D0D0D0";
        ((TextBox)sender).Foreground = new BrushConverter().ConvertFrom(Value) as SolidColorBrush;
      }
    }

    private void TextBox_VideoId_TextChanged(object sender, TextChangedEventArgs e)
    {
      Button_Add.IsEnabled = (0 < ((TextBox)sender).Text.Length);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      TextBox_VideoId.GotFocus += new RoutedEventHandler(TextBox_VideoId_GotFocus);
      TextBox_VideoId.LostFocus += new RoutedEventHandler(TextBox_VideoId_LostFocus);
      TextBox_VideoId.TextChanged += new TextChangedEventHandler(TextBox_VideoId_TextChanged);
    }

    private void Button_Quit_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void Button_Add_Click(object sender, RoutedEventArgs e)
    {
      if(0 < TextBox_VideoId.Text.Length )
      {
        ListStatuses.Add(new Status(TextBox_VideoId.Text));
        TextBox_VideoId.Text = String.Empty;
        UpdateDisplayList();
      }
    }

    private void Button_Remove_Click(object sender, RoutedEventArgs e)
    {
      for (int i = DataGridMain.Items.Count - 1; i >= 0; i--)
      {
        Status? status = DataGridMain.Items[i] as Status;
        if (status!=null && status.IsSelected )
        {
          ListStatuses.Remove(status);
        }
      }
      UpdateDisplayList();
    }

    private void Button_Clear_Click(object sender, RoutedEventArgs e)
    {
      for (int i = DataGridMain.Items.Count - 1; i >= 0; i--)
      {
        Status? status = DataGridMain.Items[i] as Status;
        if (status!=null)
        {
          ListStatuses.Remove(status);
        }
      }
      UpdateDisplayList();
    }
  }
}
