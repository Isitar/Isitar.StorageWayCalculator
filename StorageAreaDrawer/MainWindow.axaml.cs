namespace StorageAreaDrawer
{
    using System;
    using System.IO;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Controls.Shapes;
    using Avalonia.Interactivity;
    using Avalonia.Markup.Xaml;
    using Avalonia.Media;
    using Isitar.StorageWayCalculator.MasterData.Storage;
    using Newtonsoft.Json;

    public partial class MainWindow : Window
    {
        private StorageArea storageArea;
        private TextBox txtWidth;
        private TextBox txtHeight;
        private Canvas cnvStorageArea;

        private int storageAreaWidth = 20;
        private int storageAreaHeight = 20;

        public MainWindow()
        {
            InitStorageArea();

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            cnvStorageArea = this.FindControl<Canvas>("cnvStorageArea");
            txtWidth = this.FindControl<TextBox>("txtWidth");
            txtHeight = this.FindControl<TextBox>("txtHeight");

            txtWidth.GetObservable(TextBox.TextProperty).Subscribe(e =>
            {
                if (int.TryParse(e, out var w))
                {
                    storageAreaWidth = w;
                }
            });


            txtHeight.GetObservable(TextBox.TextProperty).Subscribe(e =>
            {
                if (int.TryParse(e, out var h))
                {
                    storageAreaHeight = h;
                }
            });

            this.GetObservable(ClientSizeProperty).Subscribe(val => { DrawStorageArea(); });
        }

        private void InitStorageArea()
        {
            storageArea = new StorageArea(storageAreaWidth, storageAreaHeight);
            if (null != cnvStorageArea)
            {
                DrawStorageArea();
            }
        }
        
        

        private void DrawStorageArea()
        {
            Console.WriteLine("Redrawing Storage area");
            cnvStorageArea.Children.Clear();

            var numX = storageArea.Fields.GetLength(0);
            var numY = storageArea.Fields.GetLength(1);
            var w = double.IsNaN(cnvStorageArea.Width) ? ClientSize.Width : cnvStorageArea.Width;
            var h = double.IsNaN(cnvStorageArea.Height) ? ClientSize.Height : cnvStorageArea.Height;
            var rectLenght = Math.Min(w / numX, h / numY);

            for (var x = 0; x < numX; x++)
            {
                for (var y = 0; y < numY; y++)
                {
                    var rect = new Rectangle
                    {
                        Fill = BrushFromStorageAreaType(storageArea.Fields[x, y].Type),
                        Width = rectLenght,
                        Height = rectLenght,
                        Stroke = Brushes.Black,
                        StrokeThickness = rectLenght / 100,
                    };
                    cnvStorageArea.Children.Add(rect);
                    Canvas.SetTop(rect, rectLenght * y);
                    Canvas.SetLeft(rect, rectLenght * (double) x);

                    var xCopy = x;
                    var yCopy = y;
                    rect.PointerPressed += (sender, args) =>
                    {
                        var storageAreaFieldTypeCount = Enum.GetValues<StorageAreaFieldType>().Length;
                    
                        if (args.GetCurrentPoint(rect).Properties.IsLeftButtonPressed)
                        {
                            storageArea.Fields[xCopy, yCopy].Type = (StorageAreaFieldType) (((int) (storageArea.Fields[xCopy, yCopy].Type) + 1 + storageAreaFieldTypeCount) % storageAreaFieldTypeCount);
                        }
                        else
                        {
                            storageArea.Fields[xCopy, yCopy].Type = (StorageAreaFieldType) (((int) (storageArea.Fields[xCopy, yCopy].Type) - 1 + storageAreaFieldTypeCount) % storageAreaFieldTypeCount);
                        }

                        rect.Fill = BrushFromStorageAreaType(storageArea.Fields[xCopy, yCopy].Type);
                    };
                }
            }
        }

        private void InitClicked(object? sender, RoutedEventArgs e)
        {
            InitStorageArea();
        }

        private IBrush BrushFromStorageAreaType(StorageAreaFieldType t)
        {
            return t switch
            {
                StorageAreaFieldType.Free => Brushes.LightGray,
                StorageAreaFieldType.Storage => Brushes.LightBlue,
                StorageAreaFieldType.Wall => Brushes.DarkGray,
                _ => Brushes.Red,
            };
        }
        
        private void LoadClicked(object? sender, RoutedEventArgs e)
        {
            storageArea = JsonConvert.DeserializeObject<StorageArea>(File.ReadAllText("storageArea.json")) ?? throw new Exception("No file found");
            DrawStorageArea();
        }

        private void SaveClicked(object? sender, RoutedEventArgs e)
        {
            File.WriteAllText("storageArea.json", JsonConvert.SerializeObject(storageArea));
        }
    }
}