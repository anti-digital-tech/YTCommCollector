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
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Collections.Specialized;
using System.Windows.Controls.Primitives;
using System.Data;
using ClosedXML.Excel;

namespace YTCommCollector
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    List<Status> ListStatuses_ { get; set; } = new List<Status>();
    string apiKey_ = string.Empty;

    public MainWindow()
    {
      InitializeComponent();
      UpdateDisplayList();
      EnableDragDrop(TextBox_PathOutput);

      apiKey_ = (string)Application.Current.FindResource("ApiKey");
    }
    private void EnableDragDrop(Control control)
    {
      control.AllowDrop = true;
      control.PreviewDragOver += (s, e) =>
      {
        DragDropEffects effects = DragDropEffects.None;
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
          var path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
          if (Directory.Exists(path))
          {
            effects = DragDropEffects.Copy;
          }
        }
        e.Effects = effects;
        e.Handled = true;
      };

      control.PreviewDrop += (s, e) =>
      {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
          string[] paths = ((string[])e.Data.GetData(DataFormats.FileDrop));
          TextBox_PathOutput.Text = paths[0];
        }
      };
    }
    void UpdateDisplayList()
    {
      DataGridMain.ItemsSource = new ReadOnlyCollection<Status>(ListStatuses_);
    }

    void UpdateUI()
    {
      Button_Run.IsEnabled = 0 < DataGridMain.Items.Count && !TextBox_PathOutput.Text.StartsWith('<');
      Button_Clear.IsEnabled = 0 < DataGridMain.Items.Count;
    }

    private void TextBox_VideoId_GotFocus(object sender, RoutedEventArgs e)
    {
      if(((TextBox)sender).Text == (string)Application.Current.FindResource("MSG_TextBox_VideoId"))
      {
        ((TextBox)sender).Text = "";
        ((TextBox)sender).Foreground = SystemColors.ControlTextBrush;
      }
    }

    private void TextBox_VideoId_LostFocus(object sender, RoutedEventArgs e)
    {
      if ((((TextBox)sender).Text == ""))
      {
        ((TextBox)sender).Text = (string)Application.Current.FindResource("MSG_TextBox_VideoId");
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

    private async void Button_Add_Click(object sender, RoutedEventArgs e)
    {
      string videoId = YTCommFetcher.CorrectVideoId(TextBox_VideoId.Text);
      if (string.IsNullOrEmpty(videoId))
      {
        MessageBox.Show("ERROR: Incorrect Video ID was specified.");
        return;
      }
      foreach(Status status in ListStatuses_)
      {
        if (status.VideoId == videoId)
        {
          MessageBox.Show("ERROR: The same Video ID has been already registered.");
          return;
        }
      }
      if(0 < TextBox_VideoId.Text.Length )
      {
        Status status = new Status(videoId);
        TextBox_VideoId.Text = String.Empty;
        status.Title = await YTTitleFetcher.GetTitleAsync(apiKey_, status.VideoId);
        if (status.Title == String.Empty)
        {
          MessageBox.Show("ERROR: Incorrect Video ID was specified.");
          return;
        }
        else
        {
          ListStatuses_.Add(status);
        }
        UpdateDisplayList();
        UpdateUI();
      }
    }
    private void DataGrid_Button_Click(object sender, RoutedEventArgs e)
    {
      Status status = (Status)((Button)e.Source).DataContext;
      int index = ListStatuses_.IndexOf(status);
      if (-1 != index)
      {
        ListStatuses_.RemoveAt(index);
        UpdateDisplayList();
      }
    }
    /*
    private void Button_Remove_Click(object sender, RoutedEventArgs e)

    {
      for (int i = DataGridMain.Items.Count - 1; i >= 0; i--)
      {
        Status? status = DataGridMain.Items[i] as Status;
        if (status!=null && status.IsSelected )
        {
          ListStatuses_.Remove(status);
        }
      }
      UpdateDisplayList();
      UpdateUI();
      Button_Remove.IsEnabled = false;
    }
    */

    private void Button_Clear_Click(object sender, RoutedEventArgs e)
    {
      for (int i = DataGridMain.Items.Count - 1; i >= 0; i--)
      {
        Status? status = DataGridMain.Items[i] as Status;
        if (status!=null)
        {
          ListStatuses_.Remove(status);
        }
      }
      UpdateDisplayList();
      UpdateUI();
    }

    private void DataGridMain_SourceUpdated(object sender, DataTransferEventArgs e)
    {
      for (int i = 0; i < DataGridMain.Items.Count; i++)
      {
        Status? status = DataGridMain.Items[i] as Status;
        if( status!=null && status.IsSelected )
        {
          return;
        }
      }
    }

    private void Button_Folder_Click(object sender, RoutedEventArgs e)
    {
      using (var commonOpenFileDialog = new CommonOpenFileDialog()
      {
        Title = "Choose a folder where data of comments will be placed",
        InitialDirectory = TextBox_PathOutput.Text.StartsWith('<') ?
          Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\" : TextBox_PathOutput.Text,
        IsFolderPicker = true,
        RestoreDirectory = true,
      })
      {
        if (commonOpenFileDialog.ShowDialog() != CommonFileDialogResult.Ok)
        {
          return;
        }
        TextBox_PathOutput.Text = commonOpenFileDialog.FileName;
        UpdateUI();
      }
    }

    void WriteToExcel()
    {
      DateTime dt = DateTime.Now;
      string pathFile = System.IO.Path.Combine(TextBox_PathOutput.Text, "YTCommCollector_OUTPUT_" + dt.ToString("yyyy-MM-dd_HHmmss") + ".xlsx");
      using (var workbook = new XLWorkbook())
      {
        foreach(var status in ListStatuses_)
        {
          int row = 1;
          var worksheet = workbook.Worksheets.Add($"{status.VideoId}");
          worksheet.Cell(row, 1).Value = "Video ID :";
          worksheet.Cell(row++, 2).Value = $"'{status.VideoId}";
          worksheet.Cell(row, 1).Value = "Title :";
          worksheet.Cell(row++, 2).FormulaA1 = $"=HYPERLINK(\"{status.Url}\", \"{status.Title}\")";
          row++;

          worksheet.Cell(row, 1).Value = "seq #";
          worksheet.Cell(row, 2).Value = "child #";
          worksheet.Cell(row, 3).Value = "date time";
          worksheet.Cell(row, 4).Value = "comment";
          worksheet.Cell(row, 5).Value = "author";
          worksheet.Cell(row, 6).Value = "good count";
          worksheet.Cell(row, 7).Value = "reply count";
          row++;

          foreach(var comm in status.ListComms)
          {
            worksheet.Cell(row, 1).Value = comm.SeqNum;
            worksheet.Cell(row, 2).Value = comm.ChildNum;
            worksheet.Cell(row, 3).Value = comm.CreatedAt;
            worksheet.Cell(row, 4).Value = comm.Text;
            worksheet.Cell(row, 5).Value = comm.Author;
            worksheet.Cell(row, 6).Value = comm.CountGood;
            worksheet.Cell(row, 7).Value = comm.CountReply;
            row++;
          }
        }
        workbook.SaveAs(pathFile);
      }
    }

    private async void Button_Run_Click(object sender, RoutedEventArgs e)
    {
      DataGridMain.IsEnabled = false;
      Button_Add.IsEnabled = false;
      Button_Clear.IsEnabled = false;
      Button_Run.IsEnabled = false;

      YTCommFetcher ytCommFetcher = new YTCommFetcher(apiKey_, this, UpdateDisplayList);
      await ytCommFetcher.FetchAsync(ListStatuses_);
      UpdateDisplayList();
      WriteToExcel();
      MessageBox.Show("Completed");
      DataGridMain.IsEnabled = true;
      Button_Add.IsEnabled = true;
      Button_Clear.IsEnabled = true;
      Button_Run.IsEnabled = true;
    }
  }
}
