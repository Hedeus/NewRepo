﻿using NetCoreWeather.Infrastructure.Commands.Base;
using NetCoreWeather.ViewModels.Base;
using System;
using System.Windows;
using System.Windows.Input;
using NetCoreWeather.Services;
using System.Data;
using NetCoreWeather.Models.Weather;
using System.Windows.Markup;
using Spire.Doc;
using Spire.Doc.Documents;

namespace NetCoreWeather.ViewModels
{
    [MarkupExtensionReturnType(typeof(MainWindowViewModel))]
    internal class MainWindowViewModel : ViewModel
    {
        /*------------------------------------------------------------------------------------*/

        #region Заголовок окна
        private string _Title = "Weather";

        /// <summary>Заголовок окна</summary>

        public string Title
        {
            get => _Title;            
            set => Set(ref _Title, value);
        }

        #endregion

        #region Status : String - Статус программы

        /// <summary>Статус программы</summary>
        private string _Status = "Готов!";

        /// <summary>Статус программы</summary>
        public string Status
        {
            get => _Status;
            set => Set(ref _Status, value);
        }

        #endregion

        #region Текст помилки

        private string _ErrorText = "";

        public string ErrorText
        {
            get => _ErrorText;
            set => Set(ref _ErrorText, value);
        }

        #endregion

        #region Weather

        private string _TextWeather = "Не відомо";

        public string TextWeather
        {
            get => _TextWeather;
            set => Set(ref _TextWeather, value);
        }

        /// <summary>
        /// Чи проходить процесс редагування
        /// </summary>
        private bool _IsEditing = false;
        public bool IsEditing
        {
            get => _IsEditing;
            set => Set(ref _IsEditing, value);
        }

        /// <summary>
        /// Чи проходить процесс додавання рядка
        /// </summary>
        private bool _IsAdding = false;
        public bool IsAdding
        {
            get => _IsAdding;
            set => Set(ref _IsAdding, value);
        }


        /// <summary>
        /// Активна вкладка
        /// </summary>
        private int _ActiveTab = 0;
        public int ActiveTab
        {
            get => _ActiveTab;
            set => Set(ref _ActiveTab, value);
        }

        /// <summary>
        /// Результат запита SELECT
        /// </summary>
        private System.Data.DataTable _Table;
        public System.Data.DataTable Table
        {
            get => _Table;
            set => Set(ref _Table, value);
        }

        /// <summary>
        /// Номер Обраного рядка в таблиці
        /// </summury>
        private int _SelectedRowIndex;
        public int SelectedRowIndex
        {
            get => _SelectedRowIndex;
            set => Set(ref _SelectedRowIndex, value);
        }

        /// <summary>
        /// Обраний день в таблиці
        /// </summary>
        private DayWeather _SelectedDay;
        public DayWeather SelectedDay
        {
            get => _SelectedDay;
            set => Set(ref _SelectedDay, value);
        }

        /// <summary>
        /// Параметри пошуку
        /// </summary>
        private DesiredDay _desiredDay;
        public DesiredDay desiredDay
        {
            get => _desiredDay;
            set => Set(ref _desiredDay, value);
        }

        #endregion

        #region AWG

        private int _Month = 1;
        public int Month
        {
            get => _Month;
            set => Set(ref _Month, value);
        }

        private string _AVGTemp = "Не визначено!";
        public string AVGTemp
        {
            get => _AVGTemp;
            set => Set(ref _AVGTemp, value);
        }
        #endregion

        /*------------------------------------------------------------------------------------*/

        #region Методы

        #region Пошук у базі       

        internal void WeatherSearch()
        {
            int year = 2021;
            string SearchStr = "SELECT day(t.date) as `День`," +
                             "month(t.date) as `Місяць`," +
                             "temperature as `Температура`," +
                             "pressure as `Тиск`," +
                             "precipitation as `Опади`," +
                             "precipitation | 0 as `PercInt`" +
                             "FROM weather2021 t ";
            string StartDateStr = "";
            string EndDateStr = "";
            string StartTempStr = "";
            string EndTempStr = "";
            string StartPresStr = "";
            string EndPresStr = "";
            bool twoDates = false;
            bool IsWhere = false;

            //WorkWithDataBase.OpenConnection("server=localhost;uid=root;pwd=1h9e8d7;database=weather;");

            #region Перевірка не необхідність пошуку, або вивід всіх даних з бази

            if (desiredDay.IsDate)
            {
                if (desiredDay.EndMonth > 12)
                    desiredDay.EndMonth = 12;
                if (desiredDay.StartMonth > 12)
                    desiredDay.StartMonth = 12;

                if (desiredDay.StartDay < 0)
                    desiredDay.StartDay = 0;
                if (desiredDay.EndDay < 0)
                    desiredDay.EndDay = 0;
            }

            if ((desiredDay.IsDate && ((desiredDay.StartMonth != 0) ||
                (desiredDay.StartDay != 0) ||
                (desiredDay.EndMonth != 0) ||
                (desiredDay.EndDay != 0))) ||
                desiredDay.IsTemperature ||
                desiredDay.IsPress ||
                desiredDay.IsPrecipitation())
            {
                SearchStr += "WHERE ";
                IsWhere = true;
            }

            //if (!IsWhere)
            else
            {
                SearchStr += ";";
                Table = DataBaseService.ExecuteQuery(SearchStr);
                return;
            }


            #endregion

            #region Додавання пошуку за датою

            if (desiredDay.IsDate)
            {
                if (desiredDay.StartDay > 0)
                {
                    if (desiredDay.StartMonth == 0)
                        desiredDay.StartMonth = 1;
                    if (desiredDay.EndMonth == 0)
                        desiredDay.EndMonth = 12;
                }
                if ((desiredDay.EndDay > 0) && (desiredDay.EndMonth == 0))
                    desiredDay.EndMonth = 12;

                if (desiredDay.StartMonth > 0)
                {
                    if (desiredDay.StartMonth < desiredDay.EndMonth)
                    {
                        StartDateStr += desiredDay.StartMonth.ToString();
                        EndDateStr += desiredDay.EndMonth.ToString();
                        twoDates = true;
                    }
                    else
                    {
                        StartDateStr += desiredDay.StartMonth.ToString();
                        desiredDay.EndMonth = desiredDay.StartMonth;
                        EndDateStr += desiredDay.EndMonth.ToString();
                    }
                }
                else
                {
                    if (desiredDay.EndMonth >= 0)
                    {
                        desiredDay.StartMonth = 1;
                        StartDateStr += desiredDay.StartMonth.ToString();
                        if (desiredDay.EndMonth == 0)
                            desiredDay.EndMonth = 12;
                        EndDateStr += desiredDay.EndMonth.ToString();
                        twoDates = true;
                    }
                }

                if (desiredDay.StartDay > DateTime.DaysInMonth(year, desiredDay.StartMonth))
                    desiredDay.StartDay = DateTime.DaysInMonth(year, desiredDay.StartMonth);
                if (desiredDay.EndDay > DateTime.DaysInMonth(year, desiredDay.EndMonth))
                    desiredDay.EndDay = DateTime.DaysInMonth(year, desiredDay.EndMonth);

                if (desiredDay.StartDay > 0)
                {
                    if (desiredDay.StartDay < desiredDay.EndDay)
                    {
                        StartDateStr += "," + desiredDay.StartDay.ToString();
                        EndDateStr += "," + desiredDay.EndDay.ToString();
                        twoDates = true;
                    }
                    else
                    {
                        StartDateStr += "," + desiredDay.StartDay.ToString();
                        EndDateStr += "," + desiredDay.StartDay.ToString();
                    }
                }
                else
                {
                    if (desiredDay.EndMonth >= 0)
                    {
                        StartDateStr += ",1";
                        if (desiredDay.EndDay == 0)
                            desiredDay.EndDay = DateTime.DaysInMonth(year, desiredDay.EndMonth);
                        EndDateStr += "," + desiredDay.EndDay.ToString();
                        twoDates = true;
                    }
                }

                StartDateStr += "," + year.ToString();
                EndDateStr += "," + year.ToString();

                if (!twoDates)
                {
                    SearchStr += "`date`=STR_TO_DATE( '" + StartDateStr + "', '%m,%d,%Y' ) ";
                }
                else
                    SearchStr += "`date` >= STR_TO_DATE( '" + StartDateStr + "', '%m,%d,%Y' ) && " +
                                 "`date` <= STR_TO_DATE( '" + EndDateStr + "', '%m,%d,%Y' ) ";
            }

            #endregion

            #region Пошук за температурою

            if (desiredDay.IsTemperature)
            {
                if (desiredDay.IsDate)
                    SearchStr += "&& ";
                SearchStr += "temperature >= " + desiredDay.StartTemperature.ToString() + " &&" +
                             " temperature <= " + desiredDay.EndTemperature.ToString() + " ";
            }
            #endregion

            #region Пошук за тиском

            if (desiredDay.IsPress)
            {
                if (desiredDay.IsTemperature || (desiredDay.IsDate && !desiredDay.IsTemperature))
                    SearchStr += "&& ";
                SearchStr += "pressure >= " + desiredDay.StartPressure.ToString() + " &&" +
                            " pressure <= " + desiredDay.EndPressure.ToString() + " ";
            }

            #endregion

            #region Пошук за погодними умовами

            if (desiredDay.PresipitationToInt() > 0)
            {
                if (!(!desiredDay.IsDate && !desiredDay.IsTemperature && !desiredDay.IsPress))
                    SearchStr += "&& ";
                SearchStr += "precipitation & " + desiredDay.PresipitationToInt();
            }

            #endregion


            //if (desiredDay.IsDate)
            //{
            //    SearchStr += "month(t.date) >= " + desiredDay.StartMonth.ToString() +
            //        " && month(t.date) <= " + desiredDay.EndMonth.ToString() +
            //        " && day(t.date) >= " + desiredDay.StartDay.ToString() +
            //        " && day(t.date) <= " + desiredDay.EndDay.ToString();
            //}
            SearchStr += ";";

            Table = DataBaseService.ExecuteQuery(SearchStr);

            //WorkWithDataBase.CloseConnection();
        }

        #endregion


        #region Перехід до Редагування рядка

        public void RowEditButtonClick()
        {
            //if (Table == null) return;
            ActiveTab = 2;
            IsEditing = true;
            IsAdding = false;
            DayWeather selday = new DayWeather();
            DataRow row = Table.NewRow();
            row = Table.Rows[SelectedRowIndex];
            selday.Month = Convert.ToInt32(row["Місяць"]);
            selday.Day = Convert.ToInt32(row["День"]);
            selday.Temperature = Convert.ToSByte(row["Температура"]);
            selday.Pressure = Convert.ToUInt16(row["Тиск"]);
            selday.PreciInt = Convert.ToInt32(row["PercInt"]);
            selday.PrecipitationToBool();
            SelectedDay = selday;
            Status = "Редагування";
            ErrorText = "";
        }

        #endregion

        #region Внесення змін у рядок

        internal void SaveChanges()
        {
            SelectedDay.PresipitationToInt();
            bool IsError = false;
            string command = "";
            int year = 2021;
            if (!IsAdding)
            {
                command = "UPDATE weather2021 " +
                    "SET temperature = '" + SelectedDay.Temperature.ToString() + "', " +
                    "pressure = '" + SelectedDay.Pressure.ToString() + "', " +
                    "precipitation = '" + SelectedDay.PreciInt.ToString() + "' " +
                    "WHERE date = ";
                string dateStr = SelectedDay.Month.ToString() + "," +
                    SelectedDay.Day.ToString() + "," + year.ToString();
                command += "STR_TO_DATE( '" + dateStr + "', '%m,%d,%Y' );";
            }
            else
            {
                if ((SelectedDay.Month > 0) && (SelectedDay.Month <= 12))
                {
                    if ((SelectedDay.Day > 0) && (SelectedDay.Day <= DateTime.DaysInMonth(year, SelectedDay.Month)))
                    {
                        string dateStr = SelectedDay.Month.ToString() + "," +
                            SelectedDay.Day.ToString() + "," + year.ToString();
                        command = "SELECT * FROM weather2021 WHERE date = STR_TO_DATE('" + dateStr + "', '%m,%d,%Y'); ";
                        var table = new System.Data.DataTable();
                        table = DataBaseService.ExecuteQuery(command);


                        if (!(table == null))
                        {
                            IsError = true;
                            ErrorText = "Запис з в казаною датою вже існує!";
                        }
                        SelectedDay.PresipitationToInt();
                        command = "INSERT INTO `weather`.`weather2021` (`date`, `temperature`, `precipitation`, `pressure`)" +
                                  "VALUES (STR_TO_DATE('" + dateStr + "', '%m,%d,%Y'), '" +
                                  SelectedDay.Temperature.ToString() + "', '" +
                                  SelectedDay.PreciInt.ToString() + "', '" +
                                  SelectedDay.Pressure.ToString() + "');";
                    }
                    else
                    {
                        IsError = true;
                        ErrorText = "Номер дня має бути " +
                            " від 1 до " + DateTime.DaysInMonth(year, SelectedDay.Month).ToString() + "!";
                    }
                }
                else
                {
                    IsError = true;
                    ErrorText = "Номер місяця має бути від 1 до 12!";
                }
            }
            if (IsError)
                return;
            DataBaseService.ExecuteQueryWithoutResponse(command);
            ActiveTab = 0;
            IsEditing = false;
            IsAdding = false;
            Status = "Очікування!";
            ErrorText = "";
        }

        #endregion

        #region Додавання рядка

        internal void RowAddButtonClick()
        {
            ActiveTab = 2;
            IsEditing = true;
            IsAdding = true;
            DayWeather selday = new DayWeather();

            ErrorText = "";
            selday.Month = 1;
            selday.Day = 1;
            selday.Temperature = 0;
            selday.Pressure = 0;
            selday.PreciInt = 0;
            selday.PrecipitationToBool();
            SelectedDay = selday;

            Status = "Додавання запису!";

            return;
        }

        #endregion

        #region RowRemoveButtonClick - Видалення обраного рядка
        internal bool RowRemoveButtonClick()
        {
            if (Table == null) return false;
            //DialogResultConverter result;
            bool result = MessageBox.Show("Ви впевнені, що хочете видалити рядок: ?",
                                        "Видалення рядка", MessageBoxButton.YesNo,
                                         MessageBoxImage.Question) == MessageBoxResult.Yes;
            if (result)
            {
                DayWeather selday = new DayWeather();
                DataRow row = Table.NewRow();
                row = Table.Rows[SelectedRowIndex];
                selday.Month = Convert.ToInt32(row["Місяць"]);
                selday.Day = Convert.ToInt32(row["День"]);
                Status = "Видалення";
                ErrorText = "";
                int year = 2021;
                string dateStr = selday.Month.ToString() + "," +
                            selday.Day.ToString() + "," + year.ToString();
                string command = "DELETE FROM weather2021 WHERE date = STR_TO_DATE('" + dateStr + "', '%m,%d,%Y'); ";
                DataBaseService.ExecuteQueryWithoutResponse(command);

                Status = "Рядок " + dateStr + " видалено!";
                return true;
            }
            else
                Status = "Виділення рядка відмінено.";
            return false;
        }
        #endregion

        #region DocumentSaving - запис таблиці у файл MS Word
        internal void DocumentSaving()
        {
            string textToFile = "";
            DataRow row = Table.NewRow();
            var day = new DayWeather();
            for (var i = 0; i < Table.Rows.Count; i++)
            {
                row = Table.Rows[i];
                textToFile += Convert.ToString(row["День"])
                    + "." + Convert.ToString(row["Місяць"])
                    + "  t(C): " + Convert.ToString(row["Температура"])
                    + "  тиск: " + Convert.ToString(row["Тиск"])
                    + "  опади: ";
                day.PreciInt = Convert.ToInt32(row["PercInt"]);
                day.PrecipitationToBool();
                if (day.Precipitation.WithoutPrecipitation)
                    textToFile += "без опадів ";
                if (day.Precipitation.Rain)
                    textToFile += "дощ ";
                if (day.Precipitation.Snow)
                    textToFile += "сніг ";
                if (day.Precipitation.Hail)
                    textToFile += "град";
                textToFile += "\n";
            }

            string filename = "Weather.doc";
            Document doc = new Document();
            Section section = doc.AddSection();
            Paragraph para = section.AddParagraph();
            para.AppendText(textToFile);
            doc.SaveToFile(filename, FileFormat.Docx);
        }
        #endregion

        #region AVGtemperature - пошук і збереження середнього значення температури за місяць
        internal void AVGtemperature()
        {
            if (Month < 1 || Month > 12) return;
            int month = Month;
            string SearchStr = "SELECT AVG(temperature) as `average` FROM weather.weather2021 "
                             + $"WHERE month(date) = {month};";
            DataTable table = new DataTable();
            table = DataBaseService.ExecuteQuery(SearchStr);
            AVGTemp = Convert.ToString(table.Rows[0]["average"]);
            if (AVGTemp == "")
                AVGTemp = "Нема даних";

            string textToFile = $"Середня температура за {month} місяць: " + AVGTemp;
            string filename = "Weather.doc";
            Document doc = new Document();
            doc.LoadFromFile(filename);

            Section section = doc.LastSection;
            Paragraph para = section.AddParagraph();
            para.AppendText(textToFile);
            doc.SaveToFile(filename, FileFormat.Docx);
        } 
        #endregion

        #endregion

        /*------------------------------------------------------------------------------------*/

        #region Команды

        #region SaveDocCommand
        public ICommand SaveDocCommand { get; }

        private bool CanSaveDocCommandExecute(object p) => Table != null;

        private void OnSaveDocCommandExecuted(object p)
        {
            DocumentSaving();
        }
        #endregion

        #region AVGtemperatureCommand
        public ICommand AVGtemperatureCommand { get; }

        private bool CanAVGtemperatureCommandExecute(object p) => true;

        private void OnAVGtemperatureCommandExecuted(object p)
        {
            AVGtemperature();
        }
        #endregion

        #region  CloseApplicationCommand
        public ICommand CloseApplicationCommand { get; }

        private bool CanCloseApplicationCommandExecute(object p) => true;

        private void OnCloseApplicationCommandExecuted(object p)
        {
            System.Windows.Application.Current.Shutdown();
        }
        #endregion

        #region Пошук

        public ICommand SearchCommand { get; }

        private bool CanSearchCommandExecute(object p) => true;

        private void OnSearchCommandExecuted(object p)
        {
            WeatherSearch();
        }

        #endregion

        #region Редагування
        public ICommand EditCommand { get; }

        private bool CanEditCommandExecute(object p) => Table != null;
        //private bool CanEditCommandExecute(object p) => SelectedRowIndex >= 0;

        private void OnEditCommandExecuted(object p)
        {
            RowEditButtonClick();
        }

        #endregion

        #region Видалення
        public ICommand RemoveCommand { get; }

        private bool CanRemoveCommandExecute(object p) => Table != null;

        private void OnRemoveCommandExecuted(object p)
        {
            if (RowRemoveButtonClick())
            {
                WeatherSearch();
            }
        }

        #endregion

        #region Створення нового запису
        public ICommand AddCommand { get; }

        private bool CanAddCommandExecute(object p) => true;

        private void OnAddCommandExecuted(object p)
        {
            RowAddButtonClick();
        }

        #endregion

        #region Збереження змін у рядку
        public ICommand ApplyCommand { get; }

        private bool CanApplyCommandExecute(object p) => true;

        private void OnApplyCommandExecuted(object p)
        {
            SaveChanges();
        }

        #endregion

        #endregion

        /*------------------------------------------------------------------------------------*/

        public MainWindowViewModel()
        {
            #region Команды

            CloseApplicationCommand = new LambdaCommand(OnCloseApplicationCommandExecuted, CanCloseApplicationCommandExecute);

            SearchCommand = new LambdaCommand(OnSearchCommandExecuted, CanSearchCommandExecute);
            AddCommand = new LambdaCommand(OnAddCommandExecuted, CanAddCommandExecute);
            EditCommand = new LambdaCommand(OnEditCommandExecuted, CanEditCommandExecute);
            RemoveCommand = new LambdaCommand(OnRemoveCommandExecuted, CanRemoveCommandExecute);
            ApplyCommand = new LambdaCommand(OnApplyCommandExecuted, CanApplyCommandExecute);
            SaveDocCommand = new LambdaCommand(OnSaveDocCommandExecuted, CanSaveDocCommandExecute);
            AVGtemperatureCommand = new LambdaCommand(OnAVGtemperatureCommandExecuted, CanAVGtemperatureCommandExecute);

            #endregion

            var student_index = 1;

            desiredDay = new DesiredDay();
            //desiredDay.StartDay = 20;
            //desiredDay.StartMonth = 3;
            //desiredDay.EndDay = 29;
            //desiredDay.EndMonth = 8;
            desiredDay.StartDay = 0;
            desiredDay.StartMonth = 0;
            desiredDay.EndDay = 0;
            desiredDay.EndMonth = 0;
            desiredDay.IsTemperature = true;
            desiredDay.StartTemperature = -100;
            desiredDay.EndTemperature = -1;
            desiredDay.Precipitation.Snow = true;
            //Table = new DataTable();

            SelectedDay = new DayWeather();

            DataBaseService.OpenConnection("server=localhost;uid=root;pwd=1h9e8d7;database=weather;");


            //WorkWithDataBase.CloseConnection();

        }
    }
}
