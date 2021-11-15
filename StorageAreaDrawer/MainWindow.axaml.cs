namespace Isitar.StorageWayCalculator.StorageAreaDrawer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Controls.Shapes;
    using Avalonia.Input;
    using Avalonia.Interactivity;
    using Avalonia.Markup.Xaml;
    using Avalonia.Media;
    using MasterData.Storage;
    using Newtonsoft.Json;
    using StorageAreaToGraphProcessor;

    public partial class MainWindow : Window
    {
        private StorageArea<StorageAreaField> storageArea;
        private TextBox txtWidth;
        private TextBox txtHeight;
        private Canvas? cnvStorageArea;

        private int storageAreaWidth = 20;
        private int storageAreaHeight = 20;
        private StorageArea<StorageAreaFieldWithDistances>? calculatedStorageArea;

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
            cnvStorageArea.PointerPressed += PaintCanvas; 
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

            this.GetObservable(ClientSizeProperty).Subscribe(val =>
            {
                if (null != calculatedStorageArea)
                {
                    DrawCalculatedStorageArea();
                }
                else
                {
                    DrawStorageArea();    
                }
            });
        }

        private void InitStorageArea()
        {
            storageArea = new StorageArea<StorageAreaField>(storageAreaWidth, storageAreaHeight);
            calculatedStorageArea = null;
            if (null != cnvStorageArea)
            {
                DrawStorageArea();
            }
        }


        private Rectangle[,] rectsDrawn;

        private void PaintCanvas(object? sender, PointerPressedEventArgs args)
        {
            if (null != calculatedStorageArea)
            {
                return;
            }
            var storageAreaFieldTypeCount = Enum.GetValues<StorageAreaFieldType>().Length;

            var pos = args.GetCurrentPoint(cnvStorageArea).Position;
            var x = (int)Math.Floor(pos.X / rectLenght);
            var y = (int)Math.Floor(pos.Y / rectLenght);
            if (args.GetCurrentPoint(cnvStorageArea).Properties.IsLeftButtonPressed)
            {
                storageArea.Fields[x, y].Type = (StorageAreaFieldType) (((int) (storageArea.Fields[x, y].Type) + 1 + storageAreaFieldTypeCount) % storageAreaFieldTypeCount);
            }
            else
            {
                storageArea.Fields[x, y].Type = (StorageAreaFieldType) (((int) (storageArea.Fields[x, y].Type) - 1 + storageAreaFieldTypeCount) % storageAreaFieldTypeCount);
            }

            var rect = rectsDrawn[x, y];
            rect.Fill = BrushFromStorageAreaType(storageArea.Fields[x, y].Type);
        }
        
        private void DrawStorageArea()
        {
            Console.WriteLine("Redrawing Storage area");
            cnvStorageArea.Children.Clear();

            var numX = storageArea.Fields.GetLength(0);
            var numY = storageArea.Fields.GetLength(1);
            var w = double.IsNaN(cnvStorageArea.Width) ? ClientSize.Width : cnvStorageArea.Width;
            var h = double.IsNaN(cnvStorageArea.Height) ? ClientSize.Height : cnvStorageArea.Height;
            rectLenght = Math.Min(w / numX, h / numY);
            rectsDrawn = new Rectangle[numX, numY];
            
            
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
                    rectsDrawn[x, y] = rect;
                    Canvas.SetTop(rect, rectLenght * y);
                    Canvas.SetLeft(rect, rectLenght * (double) x);
                }
            }
        }

        private void InitClicked(object? sender, RoutedEventArgs e)
        {
            InitStorageArea();
        }

        private IBrush BrushFromStorageAreaType(StorageAreaFieldType t, bool isSelected = false)
        {
            return t switch
            {
                StorageAreaFieldType.Free => Brushes.LightGray,
                StorageAreaFieldType.Storage => isSelected ? Brushes.DarkBlue : Brushes.LightBlue,
                StorageAreaFieldType.Wall => Brushes.DarkGray,
                _ => Brushes.Red,
            };
        }

        private void LoadClicked(object? sender, RoutedEventArgs e)
        {
            storageArea = JsonConvert.DeserializeObject<StorageArea<StorageAreaField>>(File.ReadAllText("storageArea.json")) ?? throw new Exception("No file found");
            DrawStorageArea();
        }

        private void CalculateWaysClicked(object? sender, RoutedEventArgs e)
        {
            calculatedStorageArea = new Processor().CalculateDistances(storageArea);
            DrawCalculatedStorageArea();
        }

        private (int x, int y, StorageAreaFieldWithDistances? field) FromField = (0, 0, null);
        private (int x, int y, StorageAreaFieldWithDistances? field) ToField = (0, 0, null);
        private double rectLenght;

        private void DrawCalculatedStorageArea()
        {
            Console.WriteLine("Redrawing calculated Storage area");
            cnvStorageArea.Children.Clear();

            var numX = calculatedStorageArea.Fields.GetLength(0);
            var numY = calculatedStorageArea.Fields.GetLength(1);
            var w = double.IsNaN(cnvStorageArea.Width) ? ClientSize.Width : cnvStorageArea.Width;
            var h = double.IsNaN(cnvStorageArea.Height) ? ClientSize.Height : cnvStorageArea.Height;
            var rectLenght = Math.Min(w / numX, h / numY);

            for (var x = 0; x < numX; x++)
            {
                for (var y = 0; y < numY; y++)
                {
                    var rect = new Rectangle
                    {
                        Fill = BrushFromStorageAreaType(storageArea.Fields[x, y].Type, (FromField.x == x && FromField.y == y) || (null != ToField.field && ToField.x == x && ToField.y == y)),
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
                        var clickedField = calculatedStorageArea.Fields[xCopy, yCopy];
                        if (ToField.field != null || FromField.field == null)
                        {
                            ToField.field = null;
                            FromField.field = clickedField;
                            FromField.x = xCopy;
                            FromField.y = yCopy;
                        }
                        else if (FromField.field != null)
                        {
                            ToField.field = clickedField;
                            ToField.x = xCopy;
                            ToField.y = yCopy;
                        }

                        DrawCalculatedStorageArea();
                    };
                }
            }

            if (FromField.field != null && ToField.field != null)
            {
                if (FromField.field.Distances.ContainsKey(ToField.field.StorageId))
                {
                    Func<int, int, Point> coordToPoint = (a, b) => new Point(rectLenght * a + 0.5 * rectLenght, rectLenght * b + 0.5 * rectLenght);
                    Func<int, int, (int x, int y, StorageAreaFieldWithDistances? field)> createPoint = (x, y) =>
                    {
                        if (x >= calculatedStorageArea.Fields.GetLength(0)
                            || x < 0
                            || y >= calculatedStorageArea.Fields.GetLength(1)
                            || y < 0)
                        {
                            return (x, y, null);
                        }
                        return (x, y, calculatedStorageArea.Fields[x, y]);
                    };
                    // trace back from to field 
                    var line = new Polyline
                    {
                        StrokeThickness = 5,
                        Stroke = Brushes.Red,
                    };
                    line.Points = new List<Point>();
                    line.Points.Add(coordToPoint(FromField.x, FromField.y));

                    var currentPoint = FromField;
                    while (currentPoint.field.StorageId != ToField.field.StorageId)
                    {
                        var currentDist = currentPoint.field.Distances[ToField.field.StorageId];
                        double newDist;
                        // check top:
                        var newPoint = createPoint(currentPoint.x, currentPoint.y + 1);
                        if (IsShorterDist(newPoint.x, newPoint.y, currentDist))
                        {
                            currentPoint = newPoint;
                            line.Points.Add(coordToPoint(newPoint.x, newPoint.y));
                            continue;
                        }

                        newPoint = createPoint(currentPoint.x, currentPoint.y - 1);
                        if (IsShorterDist(newPoint.x, newPoint.y, currentDist))
                        {
                            currentPoint = newPoint;
                            line.Points.Add(coordToPoint(newPoint.x, newPoint.y));
                            continue;
                        }

                        newPoint = createPoint(currentPoint.x + 1, currentPoint.y);
                        if (IsShorterDist(newPoint.x, newPoint.y, currentDist))
                        {
                            currentPoint = newPoint;
                            line.Points.Add(coordToPoint(newPoint.x, newPoint.y));
                            continue;
                        }

                        newPoint = createPoint(currentPoint.x - 1, currentPoint.y);
                        if (IsShorterDist(newPoint.x, newPoint.y, currentDist))
                        {
                            currentPoint = newPoint;
                            line.Points.Add(coordToPoint(newPoint.x, newPoint.y));
                            continue;
                        }
                    }

                    cnvStorageArea.Children.Add(line);
                }
            }
        }

        private bool IsShorterDist(int x, int y, double currentDist)
        {
            if (x >= calculatedStorageArea.Fields.GetLength(0)
                || x < 0
                || y >= calculatedStorageArea.Fields.GetLength(1)
                || y < 0)
            {
                return false;
            }

            return calculatedStorageArea.Fields[x, y].Distances.TryGetValue(ToField.field.StorageId, out var newDist) && newDist < currentDist;
        }

        private void SaveClicked(object? sender, RoutedEventArgs e)
        {
            File.WriteAllText("storageArea.json", JsonConvert.SerializeObject(storageArea));
        }
    }
}