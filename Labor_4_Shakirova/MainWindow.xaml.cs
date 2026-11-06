using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Labor_4_Shakirova
{
    public partial class MainWindow : Window
    {
        // Используем инвариантную культуру для корректной работы с точкой
        private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

        private string currentInput = "0";
        private string fullExpression = "";
        private bool isNewNumber = true;
        private bool isResultDisplayed = false;

        public MainWindow()
        {
            InitializeComponent();
            themeSelector.SelectedIndex = 1; // Темная тема по умолчанию
            ApplyTheme("Dark");
            this.Focus();
        }

        // --- УПРАВЛЕНИЕ ТЕМАМИ ---
        private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string theme = themeSelector.SelectedIndex == 0 ? "Light" : "Dark";
            ApplyTheme(theme);
        }

        private void ApplyTheme(string themeName)
        {
            try
            {
                var uri = new Uri($"Themes/{themeName}Theme.xaml", UriKind.Relative);
                ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;

                Application.Current.Resources.Clear();
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);

                this.Style = (Style)FindResource("WindowStyle");
                display.Style = (Style)FindResource("DisplayStyle");
                expressionDisplay.Style = (Style)FindResource("ExpressionDisplayStyle");
                themeSelector.Style = (Style)FindResource("ComboBoxStyle");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки темы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- ПОДДЕРЖКА КЛАВИАТУРЫ ---
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                string num = e.Key.ToString().Replace("NumPad", "").Replace("D", "");
                SimulateNumberClick(num);
                e.Handled = true;
            }
            else if (e.Key == Key.Add || e.Key == Key.OemPlus) { AddOperator("+"); e.Handled = true; }
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus) { AddOperator("-"); e.Handled = true; }
            else if (e.Key == Key.Multiply || e.Key == Key.OemQuestion) { AddOperator("*"); e.Handled = true; }
            else if (e.Key == Key.Divide || e.Key == Key.Oem2) { AddOperator("/"); e.Handled = true; }
            else if (e.Key == Key.Decimal || e.Key == Key.OemPeriod || e.Key == Key.OemComma) { Decimal_Click(null, null); e.Handled = true; }
            else if (e.Key == Key.Return || e.Key == Key.Enter) { Equals_Click(null, null); e.Handled = true; }
            else if (e.Key == Key.Escape) { Clear_Click(null, null); e.Handled = true; }
            else if (e.Key == Key.Back) { Backspace(); e.Handled = true; }
            else if (e.Key == Key.D8 || e.Key == Key.OemOpenBrackets) { OpenParenthesis_Click(null, null); e.Handled = true; }
            else if (e.Key == Key.D9 || e.Key == Key.OemCloseBrackets) { CloseParenthesis_Click(null, null); e.Handled = true; }
        }

        private void Backspace()
        {
            if (!isNewNumber && !isResultDisplayed && currentInput.Length > 1)
            {
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
                UpdateDisplays();
            }
            else if (!isNewNumber && !isResultDisplayed)
            {
                currentInput = "0";
                isNewNumber = true;
                UpdateDisplays();
            }
        }

        // --- ВВОД ЧИСЕЛ ---
        private void Number_Click(object sender, RoutedEventArgs e)
        {
            SimulateNumberClick((sender as Button).Content.ToString());
        }

        private void SimulateNumberClick(string number)
        {
            if (isResultDisplayed || isNewNumber)
            {
                currentInput = number;
                isNewNumber = false;
                isResultDisplayed = false;
            }
            else
            {
                currentInput = currentInput == "0" ? number : currentInput + number;
            }
            UpdateDisplays();
        }

        private void Decimal_Click(object sender, RoutedEventArgs e)
        {
            if (isResultDisplayed || isNewNumber)
            {
                currentInput = "0.";
                isNewNumber = false;
                isResultDisplayed = false;
            }
            else if (!currentInput.Contains("."))
            {
                currentInput += ".";
            }
            UpdateDisplays();
        }

        private void Negate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double val = double.Parse(currentInput, _culture);
                currentInput = (-val).ToString(_culture);
                UpdateDisplays();
            }
            catch { ShowError("Ошибка преобразования числа"); }
        }

        // --- ОПЕРАТОРЫ ---
        private void Add_Click(object s, RoutedEventArgs e) => AddOperator("+");
        private void Subtract_Click(object s, RoutedEventArgs e) => AddOperator("-");
        private void Multiply_Click(object s, RoutedEventArgs e) => AddOperator("*");
        private void Divide_Click(object s, RoutedEventArgs e) => AddOperator("/");

        private void AddOperator(string op)
        {
            if (!string.IsNullOrEmpty(fullExpression) && !isNewNumber)
                fullExpression += currentInput + " ";
            else if (string.IsNullOrEmpty(fullExpression))
                fullExpression = currentInput + " ";

            fullExpression += op + " ";
            currentInput = "0";
            isNewNumber = true;
            isResultDisplayed = false;
            UpdateDisplays();
        }

        // --- НАУЧНЫЕ ФУНКЦИИ ---

        private void CalcFunc(Func<double, double> func, string name)
        {
            try
            {
                double val = double.Parse(currentInput, _culture);
                double res = func(val);

                // Проверка на бесконечность или NaN после вычисления функции
                if (double.IsInfinity(res) || double.IsNaN(res))
                {
                    throw new Exception("Результат выходит за пределы допустимых значений!");
                }

                currentInput = res.ToString(_culture);
                UpdateDisplays();
                isNewNumber = true;
                isResultDisplayed = true;
            }
            catch (Exception ex) { ShowError($"Ошибка в {name}: {ex.Message}"); }
        }

        private void Sin_Click(object s, RoutedEventArgs e) => CalcFunc(v => Math.Sin(v * Math.PI / 180), "sin");
        private void Cos_Click(object s, RoutedEventArgs e) => CalcFunc(v => Math.Cos(v * Math.PI / 180), "cos");

        // ИСПРАВЛЕНИЕ ДЛЯ ТАНГЕНСА 90 ГРАДУСОВ
        private void Tan_Click(object s, RoutedEventArgs e)
        {
            try
            {
                double v = double.Parse(currentInput, _culture);

                // Тангенс не существует для углов 90 + 180*k градусов
                // Проверяем остаток от деления на 180. Если он равен 90 (с учетом погрешности), то ошибка
                double remainder = v % 180;
                if (Math.Abs(Math.Abs(remainder) - 90) < 0.0000001)
                {
                    throw new Exception("Тангенс 90° (и кратных ему углов) не существует!");
                }

                double res = Math.Tan(v * Math.PI / 180);

                if (double.IsInfinity(res) || double.IsNaN(res))
                {
                    throw new Exception("Результат выходит за пределы допустимых значений!");
                }

                currentInput = res.ToString(_culture);
                UpdateDisplays();
                isNewNumber = true;
                isResultDisplayed = true;
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void Asin_Click(object s, RoutedEventArgs e)
        {
            try
            {
                double v = double.Parse(currentInput, _culture);
                if (v < -1 || v > 1) throw new Exception("Значение должно быть от -1 до 1");
                SetResult(Math.Asin(v) * 180 / Math.PI);
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void Acos_Click(object s, RoutedEventArgs e)
        {
            try
            {
                double v = double.Parse(currentInput, _culture);
                if (v < -1 || v > 1) throw new Exception("Значение должно быть от -1 до 1");
                SetResult(Math.Acos(v) * 180 / Math.PI);
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void Atan_Click(object s, RoutedEventArgs e)
        {
            try
            {
                double v = double.Parse(currentInput, _culture);
                SetResult(Math.Atan(v) * 180 / Math.PI);
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void Square_Click(object s, RoutedEventArgs e) => CalcFunc(v => Math.Pow(v, 2), "x²");

        private void Sqrt_Click(object s, RoutedEventArgs e)
        {
            try
            {
                double v = double.Parse(currentInput, _culture);
                if (v < 0) throw new Exception("Нельзя извлечь корень из отрицательного числа!");
                SetResult(Math.Sqrt(v));
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void Log_Click(object s, RoutedEventArgs e)
        {
            try
            {
                double v = double.Parse(currentInput, _culture);
                if (v <= 0) throw new Exception("Логарифм только для положительных чисел!");
                SetResult(Math.Log10(v));
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void Ln_Click(object s, RoutedEventArgs e)
        {
            try
            {
                double v = double.Parse(currentInput, _culture);
                if (v <= 0) throw new Exception("Натуральный логарифм только для положительных чисел!");
                SetResult(Math.Log(v));
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void Reciprocal_Click(object s, RoutedEventArgs e)
        {
            try
            {
                double v = double.Parse(currentInput, _culture);
                if (v == 0) throw new DivideByZeroException();
                SetResult(1 / v);
            }
            catch (DivideByZeroException) { ShowError("Ошибка: Деление на ноль!"); }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void PowerOf10_Click(object s, RoutedEventArgs e) => CalcFunc(v => Math.Pow(10, v), "10^x");
        private void Exp_Click(object s, RoutedEventArgs e) => CalcFunc(v => Math.Exp(v), "e^x");

        private void Pi_Click(object s, RoutedEventArgs e) { currentInput = Math.PI.ToString(_culture); SetResultDisplay(); }
        private void E_Click(object s, RoutedEventArgs e) { currentInput = Math.E.ToString(_culture); SetResultDisplay(); }

        private void Factorial_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double v = double.Parse(currentInput, _culture);
                if (v < 0 || v != Math.Floor(v) || v > 170)
                    throw new Exception("Факториал: целое неотрицательное число ≤ 170");

                long res = 1;
                for (long i = 2; i <= (long)v; i++) res *= i;
                currentInput = res.ToString(_culture);
                UpdateDisplays();
                isNewNumber = true;
                isResultDisplayed = true;
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void Power_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(fullExpression) && !isNewNumber)
                    fullExpression += currentInput + " ";
                else if (string.IsNullOrEmpty(fullExpression))
                    fullExpression = currentInput + " ";

                fullExpression += "^ ";
                currentInput = "0";
                isNewNumber = true;
                UpdateDisplays();
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        // --- СКОБКИ ---
        private void OpenParenthesis_Click(object s, RoutedEventArgs e)
        {
            if (isResultDisplayed || isNewNumber)
            {
                fullExpression = "(";
                currentInput = "0";
                isNewNumber = false;
                isResultDisplayed = false;
            }
            else
            {
                fullExpression += (string.IsNullOrEmpty(fullExpression) ? "" : currentInput) + " * (";
                currentInput = "0";
                isNewNumber = false;
            }
            UpdateDisplays();
        }

        private void CloseParenthesis_Click(object s, RoutedEventArgs e)
        {
            fullExpression += currentInput + ")";
            currentInput = "0";
            isNewNumber = true;
            UpdateDisplays();
        }

        // --- ОЧИСТКА ---
        private void Clear_Click(object s, RoutedEventArgs e)
        {
            currentInput = "0";
            fullExpression = "";
            isNewNumber = true;
            isResultDisplayed = false;
            UpdateDisplays();
        }

        private void ClearEntry_Click(object s, RoutedEventArgs e)
        {
            currentInput = "0";
            isNewNumber = true;
            UpdateDisplays();
        }

        // --- ВЫЧИСЛЕНИЕ (ИСПРАВЛЕНО ДЛЯ РУССКИХ СООБЩЕНИЙ И 0^0) ---
        private void Equals_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Собираем полное выражение
                string expr = fullExpression + currentInput;

                // 2. Замена символов умножения и деления
                expr = expr.Replace("×", "*").Replace("÷", "/");

                // 3. ОБРАБОТКА СТЕПЕНИ ВРУЧНУЮ
                if (expr.Contains("^"))
                {
                    string[] parts = expr.Split(new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length == 2)
                    {
                        double baseNum = double.Parse(parts[0].Trim(), _culture);
                        double expNum = double.Parse(parts[1].Trim(), _culture);

                        // ПРОВЕРКА НА 0 В СТЕПЕНИ 0
                        if (baseNum == 0 && expNum == 0)
                        {
                            throw new Exception("0 в степени 0 — математическая неопределенность!");
                        }

                        double result = Math.Pow(baseNum, expNum);

                        // ПРОВЕРКА НА БЕСКОНЕЧНОСТЬ ИЛИ ОШИБКУ
                        if (double.IsInfinity(result))
                        {
                            throw new OverflowException("Результат слишком велик (Бесконечность)!");
                        }
                        if (double.IsNaN(result))
                        {
                            throw new Exception("Математически неопределенный результат!");
                        }

                        result = Math.Round(result, 10);

                        currentInput = result.ToString(_culture);
                        fullExpression = "";
                        isNewNumber = true;
                        isResultDisplayed = true;
                        UpdateDisplays();
                        return;
                    }
                    else
                    {
                        throw new Exception("Некорректное использование степени.");
                    }
                }

                // 4. Если степени нет, используем стандартный вычислитель
                if (expr.EndsWith("*") || expr.EndsWith("/") || expr.EndsWith("+") || expr.EndsWith("-"))
                {
                    ShowError("Выражение завершено некорректно.");
                    return;
                }

                var table = new System.Data.DataTable();
                object resultObj = table.Compute(expr, "");

                double finalResult = Convert.ToDouble(resultObj, _culture);

                // ПРОВЕРКА РЕЗУЛЬТАТА DataTable НА "INFINITY"
                if (double.IsInfinity(finalResult))
                {
                    throw new DivideByZeroException();
                }
                if (double.IsNaN(finalResult))
                {
                    throw new Exception("Недопустимая математическая операция!");
                }

                finalResult = Math.Round(finalResult, 10);

                currentInput = finalResult.ToString(_culture);
                fullExpression = "";
                isNewNumber = true;
                isResultDisplayed = true;
                UpdateDisplays();
            }
            catch (DivideByZeroException)
            {
                ShowError("Ошибка: Деление на ноль!");
            }
            catch (OverflowException ex)
            {
                ShowError(ex.Message);
            }
            catch (System.Data.SyntaxErrorException)
            {
                ShowError("Ошибка синтаксиса. Проверьте скобки или операторы.");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка вычисления: {ex.Message}");
            }
        }

        // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ---
        private void SetResult(double val)
        {
            // Проверка перед установкой результата
            if (double.IsInfinity(val) || double.IsNaN(val))
            {
                ShowError("Результат выходит за пределы допустимых значений!");
                return;
            }
            currentInput = val.ToString(_culture);
            UpdateDisplays();
            isNewNumber = true;
            isResultDisplayed = true;
        }

        private void SetResultDisplay()
        {
            UpdateDisplays();
            isNewNumber = true;
            isResultDisplayed = true;
        }

        private void UpdateDisplays()
        {
            display.Text = currentInput;
            expressionDisplay.Text = fullExpression;
        }

        private void ShowError(string msg)
        {
            MessageBox.Show(msg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            Clear_Click(null, null);
        }
    }
}