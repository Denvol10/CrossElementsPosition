using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CrossElementsPosition.Infrastructure;

namespace CrossElementsPosition.ViewModels
{
    internal class MainWindowViewModel : Base.ViewModel
    {
        private RevitModelForfard _revitModel;

        internal RevitModelForfard RevitModel
        {
            get => _revitModel;
            set => _revitModel = value;
        }

        #region Заголовок
        private string _title = "Положение поперечных элементов";

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }
        #endregion

        #region Блоки пролетного строения
        private string _blockElementIds;
        public string BlockElementIds
        {
            get => _blockElementIds;
            set => Set(ref _blockElementIds, value);
        }
        #endregion

        #region Элементы разметки
        private string _markupElementIds;
        public string MarkupElementIds
        {
            get => _markupElementIds;
            set => Set(ref _markupElementIds, value);
        }
        #endregion

        #region Команды

        #region Получение блоков пролетного строения
        public ICommand GetBlockElementsCommand { get; }

        private void OnGetBlockElementsCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetBlockElementsBySelection();
            BlockElementIds = RevitModel.BlockElementIds;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetBlockElementsCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Получение элементов разметки
        public ICommand GetMarkupElementsCommand { get; }

        private void OnGetMarkupElementsCommandExecute(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetMarkupElementsBySelection();
            MarkupElementIds = RevitModel.MarkupElementIds;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetMarkupElementsCommandExecuted(object parameter)
        {
            return true;
        }
        #endregion

        #region Закрыть окно
        public ICommand CloseWindowCommand { get; }

        private void OnCloseWindowCommandExecuted(object parameter)
        {
            SaveSettings();
            RevitCommand.mainView.Close();
        }

        private bool CanCloseWindowCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #endregion

        private void SaveSettings()
        {
            Properties.Settings.Default.BlockElementIds = BlockElementIds;
            Properties.Settings.Default.MarkupElementIds = MarkupElementIds;
            Properties.Settings.Default.Save();
        }


        #region Конструктор класса MainWindowViewModel
        public MainWindowViewModel(RevitModelForfard revitModel)
        {
            RevitModel = revitModel;

            #region Инициализация свойств из Settings

            #region Инициализация блоков
            if (!(Properties.Settings.Default.BlockElementIds is null))
            {
                string blockElemIdsInSettings = Properties.Settings.Default.BlockElementIds;
                if (RevitModel.IsElementsExistInModel(blockElemIdsInSettings) && !string.IsNullOrEmpty(blockElemIdsInSettings))
                {
                    BlockElementIds = blockElemIdsInSettings;
                    RevitModel.GetBlocksBySettings(blockElemIdsInSettings);
                }
            }
            #endregion

            #region Инициализация элементов разметки
            if (!(Properties.Settings.Default.MarkupElementIds is null))
            {
                string markupElemIdsInSettings = Properties.Settings.Default.MarkupElementIds;
                if(RevitModel.IsElementsExistInModel(markupElemIdsInSettings) && !string.IsNullOrEmpty(markupElemIdsInSettings))
                {
                    MarkupElementIds = markupElemIdsInSettings;
                    RevitModel.GetMarkupElementsBySettings(markupElemIdsInSettings);
                }
            }
            #endregion

            #endregion

            #region Команды
            GetBlockElementsCommand = new LambdaCommand(OnGetBlockElementsCommandExecuted, CanGetBlockElementsCommandExecute);
            GetMarkupElementsCommand = new LambdaCommand(OnGetMarkupElementsCommandExecute, CanGetMarkupElementsCommandExecuted);
            CloseWindowCommand = new LambdaCommand(OnCloseWindowCommandExecuted, CanCloseWindowCommandExecute);
            #endregion
        }

        public MainWindowViewModel() { }
        #endregion
    }
}
