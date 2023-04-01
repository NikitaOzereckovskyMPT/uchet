using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Linq;

namespace BudgetApp
{
    public partial class NewTypeDialog : Window
    {
        public string NewType { get; private set; }
    }

    public partial class MainWindow : Window
    {
        public ObservableCollection<string> RecordTypes { get; set; }
        public class BudgetRecord
        {
            public string Name { get; set; }
            public string RecordType { get; set; }
            public decimal Amount { get; set; }
            public string IncomeOrExpense { get; set; }
            public DateTime Date { get; set; }
        }

        public ObservableCollection<BudgetRecord> BudgetRecords { get; set; }
        private const string DataFileName = "data.json";

        public MainWindow()
        {
            InitializeComponent();
            BudgetRecords = new ObservableCollection<BudgetRecord>();
            RecordTypes = new ObservableCollection<string>();
            budgetDataGrid.ItemsSource = BudgetRecords;
            recordTypeComboBox.ItemsSource = RecordTypes;
            LoadData();
            datePicker.SelectedDate = DateTime.Now;
            UpdateFilteredData();
        }

        private void LoadData()
        {
            if (File.Exists(DataFileName))
            {
                var jsonData = File.ReadAllText(DataFileName);
                var records = JsonConvert.DeserializeObject<List<BudgetRecord>>(jsonData);
                BudgetRecords.Clear();
                foreach (var record in records)
                {
                    BudgetRecords.Add(record);
                }
            }
        }

        private void SaveData()
        {
            var jsonData = JsonConvert.SerializeObject(BudgetRecords);
            File.WriteAllText(DataFileName, jsonData);
        }

        private void UpdateTotalAmount()
        {
            decimal totalAmount = BudgetRecords.Sum(record => record.Amount);
            totalAmountTextBlock.Text = totalAmount.ToString("C");
        }

        private void UpdateFilteredData()
        {
            var selectedDate = datePicker.SelectedDate;
            if (selectedDate.HasValue)
            {
                var filteredRecords = BudgetRecords.Where(record => record.Date.Date == selectedDate.Value.Date);
                budgetDataGrid.ItemsSource = new ObservableCollection<BudgetRecord>(filteredRecords);
            }
            else
            {
                budgetDataGrid.ItemsSource = BudgetRecords;
            }
        }

        private void OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFilteredData();
        }

        private void OnBudgetDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BudgetRecord selectedRecord = (BudgetRecord)budgetDataGrid.SelectedItem;
            if (selectedRecord != null)
            {
                nameTextBox.Text = selectedRecord.Name;
                recordTypeComboBox.Text = selectedRecord.RecordType;
                amountTextBox.Text = selectedRecord.Amount.ToString();
            }
        }

        private void OnAddNewTypeClick(object sender, RoutedEventArgs e)
        {
            string newType = nameTextBox.Text;
            if (!string.IsNullOrWhiteSpace(newType) && !recordTypeComboBox.Items.Contains(newType))
            {
                recordTypeComboBox.Items.Add(newType);
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите уникальное непустое значение для нового типа.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnAddRecordClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime selectedDate = datePicker.SelectedDate.GetValueOrDefault(DateTime.Now);
                BudgetRecord newRecord = new BudgetRecord
                {
                    Name = nameTextBox.Text,
                    RecordType = recordTypeComboBox.Text,
                    Amount = decimal.Parse(amountTextBox.Text),
                    IncomeOrExpense = "Income", // Change this value according to your data
                    Date = selectedDate
                };

                BudgetRecords.Add(newRecord);
                SaveData();
                UpdateFilteredData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnEditRecordClick(object sender, RoutedEventArgs e)
        {
            BudgetRecord selectedRecord = (BudgetRecord)budgetDataGrid.SelectedItem;
            if (selectedRecord != null)
            {
                selectedRecord.Name = nameTextBox.Text;
                selectedRecord.RecordType = recordTypeComboBox.Text;
                selectedRecord.Amount = decimal.Parse(amountTextBox.Text);
                selectedRecord.IncomeOrExpense = "Income"; // Change this value according to your data
                budgetDataGrid.Items.Refresh();
                SaveData();
                UpdateTotalAmount();
            }
        }
        private void OnDeleteRecordClick(object sender, RoutedEventArgs e)
        {
            BudgetRecord selectedRecord = (BudgetRecord)budgetDataGrid.SelectedItem;
            if (selectedRecord != null)
            {
                BudgetRecords.Remove(selectedRecord);
                budgetDataGrid.Items.Refresh();
                SaveData();
                UpdateTotalAmount();
            }
        }
    }
}