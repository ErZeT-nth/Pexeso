using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using System;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using System.Collections.Generic;
using System.IO;

namespace Pexeso
{
    public partial class MainWindow : Window
    {
        private int _boardSize;

        private int[,] _gameBoard;
        private Button[,] _buttons;
        private List<string> _images;
        private Button _firstClickedButton;
        private Button _secondClickedButton;
        private int _matchesFound;
        private int _score;
        private TextBlock _scoreTextBlock;

        public MainWindow()
        {
            InitializeComponent();
            InitializeImages();
            MainScreen();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void MainScreen()
        {
            Grid mainGrid = new Grid
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                Spacing = 10
            };

            Button startButton = new Button
            {
                Content = "Start",
                Width = 150,
                Height = 40,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            startButton.Click += (sender, e) => ChangeCardsScreen();

            buttonPanel.Children.Add(startButton);
            mainGrid.Children.Add(buttonPanel);

            this.Content = mainGrid;
        }

        private void ChangeCardsScreen()
        {
            Grid mainGrid = new Grid
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                Spacing = 10
            };

            for (int size = 2; size <= 8; size += 2)
            {
                Button sizeButton = new Button
                {
                    Content = $"{size}x{size}",
                    Width = 150,
                    Height = 40,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };
                int newSize = size;
                sizeButton.Click += (sender, e) => InitializeGame(newSize);
                buttonPanel.Children.Add(sizeButton);
            }

            Button backButton = new Button
            {
                Content = "Back",
                Width = 150,
                Height = 50,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            backButton.Click += (sender, e) => MainScreen();

            buttonPanel.Children.Add(backButton);
            mainGrid.Children.Add(buttonPanel);

            this.Content = mainGrid;
        }
        private void InitializeImages()
        {
            _images = new List<string>();
            for (int i = 1; i <= 32; i++)
            {
                string imagePath = $"Assets/image{i}.png";
                _images.Add(imagePath);
            }
        }

        private void DisposeImageContent(Button button)
        {
            if (button.Content is Image image && image.Source is Bitmap bitmap)
            {
                button.Content = null;
                bitmap.Dispose();
            }
        }


        private void InitializeGame(int boardSize)
        {
            _boardSize = boardSize;

            InitializeGameBoard();

            _score = 0;
            _matchesFound = 0;

            _scoreTextBlock = new TextBlock
            {
                Text = $"Score: {_score}",
                FontSize = 20,
                Margin = new Avalonia.Thickness(10)
            };

            UniformGrid buttonGrid = new UniformGrid
            {
                Rows = _boardSize,
                Columns = _boardSize
            };
            CreateButtonGrid(buttonGrid);

            Button backButton = new Button
            {
                Content = "Back",
                Width = 100,
                Height = 40,
                Margin = new Avalonia.Thickness(0, 10, 0, 0),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            backButton.Click += (sender, e) => MainScreen();

            Grid mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            mainGrid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
            mainGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            mainGrid.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            mainGrid.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            mainGrid.Children.Add(_scoreTextBlock);

            Grid.SetRow(buttonGrid, 1);
            mainGrid.Children.Add(buttonGrid);
            Grid.SetRow(backButton, 2);
            mainGrid.Children.Add(backButton);

            this.Content = mainGrid;
        }

        private void InitializeGameBoard()
        {
            _gameBoard = new int[_boardSize, _boardSize];
            _buttons = new Button[_boardSize, _boardSize];
            var random = new Random();

            List<int> imagePairs = new List<int>();
            for (int i = 0; i < _boardSize * _boardSize / 2; i++)
            {
                int randomImageIndex = random.Next(1, _images.Count + 1);
                imagePairs.Add(randomImageIndex);
                imagePairs.Add(randomImageIndex);
            }

            int n = imagePairs.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                int value = imagePairs[k];
                imagePairs[k] = imagePairs[n];
                imagePairs[n] = value;
            }

            int index = 0;
            for (int row = 0; row < _boardSize; row++)
            {
                for (int col = 0; col < _boardSize; col++)
                {
                    _gameBoard[row, col] = imagePairs[index];
                    index++;
                }
            }
        }


        private void CreateButtonGrid(UniformGrid parentGrid)
        {
            var availableWidth = this.Width - 50;
            var availableHeight = this.Height - 90;

            var buttonSize = Math.Min(availableWidth / _boardSize, availableHeight / _boardSize) - 10;

            for (int row = 0; row < _boardSize; row++)
            {
                for (int col = 0; col < _boardSize; col++)
                {
                    var button = new Button
                    {
                        Width = buttonSize,
                        Height = buttonSize
                    };

                    button.Classes.Add("gridButton");
                    button.Margin = new Avalonia.Thickness(1, 1, 1, 1);
                    button.Click += Button_Click;
                    _buttons[row, col] = button;
                    parentGrid.Children.Add(button);
                }
            }
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_firstClickedButton != null && _secondClickedButton != null)
            {
                return;
            }

            Button clickedButton = sender as Button;

            int row = -1;
            int col = -1;

            for (int i = 0; i < _boardSize; i++)
            {
                for (int j = 0; j < _boardSize; j++)
                {
                    if (_buttons[i, j] == clickedButton)
                    {
                        row = i;
                        col = j;
                        break;
                    }
                }
            }

            if (row >= 0 && col >= 0 && clickedButton.Content == null)
            {
                try
                {
                    using (var stream = new FileStream(_images[(_gameBoard[row, col] - 1) % _images.Count], FileMode.Open))
                    {
                        clickedButton.Content = new Image
                        {
                            Source = new Bitmap(stream),
                            Width = clickedButton.Width,
                            Height = clickedButton.Height,
                            Stretch = Avalonia.Media.Stretch.UniformToFill
                        };
                    }
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Error loading image at index {_gameBoard[row, col] - 1}: {ex.Message}");
                }


                if (_firstClickedButton == null)
                {
                    _firstClickedButton = clickedButton;
                }
                else
                {
                    _secondClickedButton = clickedButton;

                    int firstRow = -1;
                    int firstCol = -1;
                    int secondRow = row;
                    int secondCol = col;

                    for (int i = 0; i < _boardSize; i++)
                    {
                        for (int j = 0; j < _boardSize; j++)
                        {
                            if (_buttons[i, j] == _firstClickedButton)
                            {
                                firstRow = i;
                                firstCol = j;
                                break;
                            }
                        }
                    }

                if (_gameBoard[firstRow, firstCol] == _gameBoard[secondRow, secondCol])
                {
                    _matchesFound++;
                    _score++;
                    _scoreTextBlock.Text = $"Score: {_score}";

                    if (_matchesFound == (_boardSize * _boardSize) / 2)
                    {
                        ShowVictoryScreen();
                    }
                    else
                    {
                        ResetClickedButtons();
                    }
                }
                else
                {
                    await ResetClickedButtonsAsync();
                }
            }
        }
    }

    private void ShowVictoryScreen()
    {
        ResetClickedButtons();
        TextBlock victoryTextBlock = new TextBlock
        {
            Text = "You have won the game!",
            FontSize = 24,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        Button retryButton = new Button
        {
            Content = "Retry",
            Width = 120,
            Height = 50,
            Margin = new Avalonia.Thickness(0, 10, 0, 0),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Padding = new Avalonia.Thickness(10)
        };
        retryButton.Click += RetryButton_Click;

        Button backButton = new Button
        {
            Content = "Change Game",
            Width = 160,
            Height = 50,
            Margin = new Avalonia.Thickness(0, 10, 0, 0),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Padding = new Avalonia.Thickness(10)
        };
        backButton.Click += (sender, e) => ChangeCardsScreen();

        StackPanel victoryPanel = new StackPanel
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        victoryPanel.Children.Add(victoryTextBlock);
        victoryPanel.Children.Add(backButton);
        victoryPanel.Children.Add(retryButton);

        this.Content = victoryPanel;
    }

    private void ResetClickedButtons()
    {
        _firstClickedButton.IsEnabled = false;
        _secondClickedButton.IsEnabled = false;
        _firstClickedButton.Opacity = 0.1;
        _secondClickedButton.Opacity = 0.1;

        DisposeImageContent(_firstClickedButton);
        DisposeImageContent(_secondClickedButton);

        _firstClickedButton = null;
        _secondClickedButton = null;
    }

    private async Task ResetClickedButtonsAsync()
    {
        await Task.Delay(1000);

        DisposeImageContent(_firstClickedButton);
        DisposeImageContent(_secondClickedButton);

        _firstClickedButton = null;
        _secondClickedButton = null;
    }

    private void RetryButton_Click(object sender, RoutedEventArgs e)
    {
        InitializeGame(_boardSize);
    }
}
}
