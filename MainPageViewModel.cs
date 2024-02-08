using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace FortuneForgeSL
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string? _ignoredMainNumbersText;
        public string? IgnoredMainNumbersText
        {
            get => _ignoredMainNumbersText;
            set
            {
                if (_ignoredMainNumbersText != value)
                {
                    _ignoredMainNumbersText = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _ignoredAdditionalNumbersText;

        public string? IgnoredAdditionalNumbersText
        {
            get => _ignoredAdditionalNumbersText;
            set
            {
                if (_ignoredAdditionalNumbersText != value)
                {
                    _ignoredAdditionalNumbersText = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isInitialVisible = true;

        public bool IsInitialVisible
        {
            get => _isInitialVisible;
            set
            {
                if (_isInitialVisible != value)
                {
                    _isInitialVisible = value;
                    OnPropertyChanged();
                    UpdateFrameVisibility();
                }
            }
        }

        private bool _isFirstAdjustmentVisible = false;

        public bool IsFirstAdjustmentVisible
        {
            get => _isFirstAdjustmentVisible;
            set
            {
                if (_isFirstAdjustmentVisible != value)
                {
                    _isFirstAdjustmentVisible = value;
                    OnPropertyChanged();
                    // Update related properties
                    UpdateFrameVisibility();
                }
            }
        }

        private bool _isSecondAdjustmentVisible = false;

        public bool IsSecondAdjustmentVisible
        {
            get => _isSecondAdjustmentVisible;
            set
            {
                if (_isSecondAdjustmentVisible != value)
                {
                    _isSecondAdjustmentVisible = value;
                    OnPropertyChanged();
                    UpdateFrameVisibility();
                }
            }
        }

        private bool _isInitialButtonActive;

        public bool IsInitialButtonActive
        {
            get => _isInitialButtonActive;
            set
            {
                if (_isInitialButtonActive != value)
                {
                    _isInitialButtonActive = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isFirstAdjustmentButtonActive;

        public bool IsFirstAdjustmentButtonActive
        {
            get => _isFirstAdjustmentButtonActive;
            set
            {
                if (_isFirstAdjustmentButtonActive != value)
                {
                    _isFirstAdjustmentButtonActive = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isSecondAdjustmentButtonActive;

        public bool IsSecondAdjustmentButtonActive
        {
            get => _isSecondAdjustmentButtonActive;
            set
            {
                if (_isSecondAdjustmentButtonActive != value)
                {
                    _isSecondAdjustmentButtonActive = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> InitialCombinations { get; } = new ObservableCollection<string>();

        public ObservableCollection<string> FirstAdjustmentCombinations { get; } = new ObservableCollection<string>();

        public ObservableCollection<string> SecondAdjustmentCombinations { get; } = new ObservableCollection<string>();

        public ICommand GenerateCommand { get; private set; }

        public ICommand ResetWindowSizeCommand { get; private set; }

        public ICommand ShowInitialCommand { get; private set; }

        public ICommand ShowFirstAdjustmentCommand { get; private set; }

        public ICommand ShowSecondAdjustmentCommand { get; private set; }

        private SwissLotto swissLotto = new SwissLotto();

        public MainPageViewModel()
        {
            GenerateCommand = new Command(OnGenerateClicked);
            ResetWindowSizeCommand = new Command(ResetWindowSize);
            ShowInitialCommand = new Command(() => ShowFrame("Initial"));
            ShowFirstAdjustmentCommand = new Command(() => ShowFrame("FirstAdjustment"));
            ShowSecondAdjustmentCommand = new Command(() => ShowFrame("SecondAdjustment"));
        }

        private void UpdateFrameVisibility()
        {
            IsInitialButtonActive = IsInitialVisible;
            IsFirstAdjustmentButtonActive = IsFirstAdjustmentVisible;
            IsSecondAdjustmentButtonActive = IsSecondAdjustmentVisible;
        }

        private void ResetWindowSize()
        {
            var currentWindow = Application.Current?.Windows.FirstOrDefault();
            if (currentWindow != null)
            {
                const int originalWidth = 525;
                const int originalHeight = 1010;
                currentWindow.Width = originalWidth;
                currentWindow.Height = originalHeight;
            }
        }

        private void OnGenerateClicked()
        {
            var ignoredMainNumbers = ParseInputNumbers(IgnoredMainNumbersText);
            var ignoredAdditionalNumbers = ParseInputNumbers(IgnoredAdditionalNumbersText);

            var sumRanges = new Dictionary<(int, int), (int, int)>
            {
                { (2, 2), (97, 105) },
                { (2, 3), (106, 115) },
                { (2, 4), (116, 125) },
                { (3, 2), (126, 135) },
                { (3, 3), (136, 145) },
                { (3, 4), (146, 155) },
                { (4, 2), (97, 105) },
                { (4, 3), (106, 115) },
                { (4, 4), (116, 125) },
            };
            var result = swissLotto.GenerateAndAdjustCombinations(ignoredMainNumbers, ignoredAdditionalNumbers, sumRanges);
            var initial = result.Item1;
            var firstAdjustment = result.Item2;
            var secondAdjustment = result.Item3;

            InitialCombinations.Clear();
            FirstAdjustmentCombinations.Clear();
            SecondAdjustmentCombinations.Clear();

            foreach (var combo in initial)
                InitialCombinations.Add(combo);
            foreach (var combo in firstAdjustment)
                FirstAdjustmentCombinations.Add(combo);
            foreach (var combo in secondAdjustment)
                SecondAdjustmentCombinations.Add(combo);

            IsInitialButtonActive = true;
            IsFirstAdjustmentButtonActive = false;
            IsSecondAdjustmentButtonActive = false;

            ShowFrame("Initial");
        }

        private void ShowFrame(string frame)
        {
            IsInitialVisible = false;
            IsFirstAdjustmentVisible = false;
            IsSecondAdjustmentVisible = false;

            switch (frame)
            {
                case "Initial":
                    IsInitialVisible = true;
                    break;
                case "FirstAdjustment":
                    IsFirstAdjustmentVisible = true;
                    break;
                case "SecondAdjustment":
                    IsSecondAdjustmentVisible = true;
                    break;
            }

            OnPropertyChanged(nameof(IsInitialVisible));
            OnPropertyChanged(nameof(IsFirstAdjustmentVisible));
            OnPropertyChanged(nameof(IsSecondAdjustmentVisible));
        }

        private List<int> ParseInputNumbers(string? inputText)
        {
            if (string.IsNullOrWhiteSpace(inputText))
                return new List<int>();

            return inputText.Split(',')
                            .Select(part => part.Trim())
                            .Where(part => int.TryParse(part, out _))
                            .Select(int.Parse)
                            .ToList();
        }

        public class SwissLotto
        {
            public List<(List<int>, int)> GenerateSwissLottoCombinations(List<int> ignoredMainNumbers, List<int> ignoredAdditionalNumbers, Dictionary<(int, int), (int, int)> sumRanges)
            {
                var combinations = new List<(List<int>, int)>();
                var mainNumberRange = Enumerable.Range(1, 42).Where(i => !ignoredMainNumbers.Contains(i)).ToList();
                var additionalNumberRange = Enumerable.Range(1, 6).Where(i => !ignoredAdditionalNumbers.Contains(i)).ToList();
                var random = new Random();

                for (int oddEvenRatio = 0; oddEvenRatio <= 6; oddEvenRatio++)
                {
                    for (int highLowRatio = 0; highLowRatio <= 6; highLowRatio++)
                    {
                        var key = (oddEvenRatio, highLowRatio);
                        if (!sumRanges.ContainsKey(key))
                            continue;

                        var sumRange = sumRanges[key];

                        for (int _ = 0; _ < 200000; _++)
                        {
                            var mainNumbers = mainNumberRange.OrderBy(x => random.Next()).Take(6).OrderBy(x => x).ToList();
                            var additionalNumber = additionalNumberRange[random.Next(additionalNumberRange.Count)];

                            if (mainNumbers.Where((t, i) => i < 5 && mainNumbers[i + 1] == t + 1).Any())
                                continue;

                            if (!(sumRange.Item1 <= mainNumbers.Sum() && mainNumbers.Sum() <= sumRange.Item2))
                                continue;

                            var oddCount = mainNumbers.Count(num => num % 2 != 0);
                            var evenCount = 6 - oddCount;
                            if (oddCount != oddEvenRatio || evenCount != (6 - oddEvenRatio))
                                continue;

                            var highCount = mainNumbers.Count(num => num > 21);
                            var lowCount = 6 - highCount;
                            if (highCount != highLowRatio || lowCount != (6 - highLowRatio))
                                continue;

                            combinations.Add((mainNumbers, additionalNumber));
                            break;
                        }
                    }
                }

                return combinations;
            }

            public List<string> AdjustNumbersFromTextInput(List<string> input_data, List<List<int>>? excludeCombinations = null)
            {
                if (excludeCombinations == null)
                    excludeCombinations = new List<List<int>>();

                var adjustedCombinations = new List<string>();
                var random = new Random();

                foreach (var line in input_data)
                {
                    var parts = line.Split(", Additional Number: ");
                    var mainNumbersStr = parts[0].Replace("Main Numbers: [", "").Replace("]", "");
                    var mainNumbersParts = mainNumbersStr.Split(", ");
                    var mainNumbers = mainNumbersParts.Select(part => int.Parse(part.Trim())).ToList();
                    var additionalNumber = int.Parse(parts[1].Trim());

                    var uniqueAdjustedFound = false;
                    while (!uniqueAdjustedFound)
                    {
                        var remainSameIndex = random.Next(mainNumbers.Count);

                        var adjustedMainNumbers = new List<int>(mainNumbers);
                        for (int i = 0; i < mainNumbers.Count; i++)
                        {
                            if (i == remainSameIndex)
                                continue;

                            var num = mainNumbers[i];
                            var adjustment = random.Next(2) * 2 - 1;
                            var adjustedNum = num + adjustment;

                            if (adjustedNum < 1 || adjustedNum > 42 || adjustedMainNumbers.Contains(adjustedNum))
                            {
                                adjustedNum = num - adjustment;
                            }

                            if (adjustedNum < 1 || adjustedNum > 42 || adjustedMainNumbers.Contains(adjustedNum))
                            {
                                adjustedNum = num;
                            }

                            adjustedMainNumbers[i] = adjustedNum;
                        }

                        var sortedAdjustedNumbers = adjustedMainNumbers.OrderBy(x => x).ToList();
                        if (!excludeCombinations.Any(ec => ec.SequenceEqual(sortedAdjustedNumbers)))
                        {
                            adjustedCombinations.Add($"Main Numbers: [{string.Join(", ", sortedAdjustedNumbers)}], Additional Number: {additionalNumber}");
                            excludeCombinations.Add(sortedAdjustedNumbers);
                            uniqueAdjustedFound = true;
                        }
                    }
                }

                return adjustedCombinations;
            }

            public (List<string>, List<string>, List<string>) GenerateAndAdjustCombinations(List<int> ignoredMainNumbers, List<int> ignoredAdditionalNumbers, Dictionary<(int, int), (int, int)> sumRanges)
            {
                var initialCombinations = GenerateSwissLottoCombinations(ignoredMainNumbers, ignoredAdditionalNumbers, sumRanges);

                var formattedCombinations = initialCombinations.Select(combo => $"Main Numbers: [{string.Join(", ", combo.Item1)}], Additional Number: {combo.Item2}").ToList();

                var firstAdjustment = AdjustNumbersFromTextInput(formattedCombinations);

                // Convert the formatted first adjustment back to lists of integers for exclusion
                var firstAdjustmentMainNumbers = firstAdjustment
                    .Select(combo => combo.Split(", Additional Number: ")[0].Replace("Main Numbers: [", "").Replace("]", "").Trim())
                    .Select(mainNumbers => mainNumbers.Split(", ").Select(int.Parse).ToList())
                    .ToList();

                var secondAdjustment = AdjustNumbersFromTextInput(formattedCombinations, firstAdjustmentMainNumbers);

                return (formattedCombinations, firstAdjustment, secondAdjustment);
            }
        }
    }
}
