using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

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

        public ObservableCollection<string> GeneratedCombinations { get; private set; } = new ObservableCollection<string>();
        public ICommand GenerateCommand { get; private set; }

        private SwissLotto swissLotto = new SwissLotto();

        public MainPageViewModel()
        {
            GenerateCommand = new Command(OnGenerateClicked);
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

            var finalCombinations = swissLotto.GenerateAndAdjustCombinations(ignoredMainNumbers, ignoredAdditionalNumbers, sumRanges);

            GeneratedCombinations.Clear();
            foreach (var combo in finalCombinations)
            {
                GeneratedCombinations.Add(combo);
            }
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
                    if (parts.Length != 2) continue;

                    var mainNumbersStr = parts[0].Replace("Main Numbers: [", "").Replace("]", "");
                    var mainNumbersParts = mainNumbersStr.Split(", ");
                    var mainNumbers = new List<int>();
                    foreach (var numStr in mainNumbersParts)
                    {
                        if (int.TryParse(numStr, out int num))
                        {
                            mainNumbers.Add(num);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (mainNumbers.Count != 6)
                    {
                        continue;
                    }

                    if (!int.TryParse(parts[1], out int additionalNumber))
                    {
                        continue;
                    }

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
                            adjustedCombinations.Add($"Main Numbers: {string.Join(", ", sortedAdjustedNumbers)}, Additional Number: {additionalNumber}");
                            excludeCombinations.Add(sortedAdjustedNumbers);
                            uniqueAdjustedFound = true;
                        }
                    }
                }

                return adjustedCombinations;
            }

            public List<string> GenerateAndAdjustCombinations(List<int> ignoredMainNumbers, List<int> ignoredAdditionalNumbers, Dictionary<(int, int), (int, int)> sumRanges)
            {
                var initialCombinations = GenerateSwissLottoCombinations(ignoredMainNumbers, ignoredAdditionalNumbers, sumRanges);

                var formattedCombinations = initialCombinations.Select(combo => $"Main Numbers: {string.Join(", ", combo.Item1)}, Additional Number: {combo.Item2}").ToList();

                var firstAdjustment = AdjustNumbersFromTextInput(formattedCombinations);

                var firstAdjustmentMainNumbers = firstAdjustment.Select(combo => combo.Split(", Additional Number: ")[0].Replace("Main Numbers: ", "").Trim()).Select(mainNumbers => mainNumbers.Trim('[', ']').Split(", ").Select(int.Parse).ToList()).ToList();

                var secondAdjustment = AdjustNumbersFromTextInput(formattedCombinations, firstAdjustmentMainNumbers);

                var allCombinations = formattedCombinations.Concat(firstAdjustment).Concat(secondAdjustment).ToList();

                return allCombinations;
            }
        }
    }
}
