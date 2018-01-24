﻿using Testownik.Model;
using Testownik.Model.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System;
using Testownik.Elements;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using Windows.System;

//Szablon elementu Pusta strona jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x415

namespace Testownik
{
    /// <summary>
    /// Pusta strona, która może być używana samodzielnie lub do której można nawigować wewnątrz ramki.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public string appName = AppIdentity.AppName;

        public List<AnswerBlock> answers;
        public List<AnswerBlock> Answers {
            get => answers;
            set {
                answers = value;
                RaisePropertyChanged(nameof(Answers));
            }
        }

        public TestController testController;
        public TestController TestController {
            get => testController;
            set {
                testController = value;
                RaisePropertyChanged(nameof(TestController));
            }
        }

        public KeyValuePair<string, IQuestion> textQuestion;
        public KeyValuePair<string, IQuestion> TextQuestion {
            get => textQuestion;
            set {
                textQuestion = value;
                RaisePropertyChanged(nameof(TextQuestion));
            }
        }

        public int reoccurrencesOfQuestion;
        public int ReoccurrencesOfQuestion {
            get => reoccurrencesOfQuestion;
            set {
                reoccurrencesOfQuestion = value;
                RaisePropertyChanged(nameof(ReoccurrencesOfQuestion));
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            SettingsHelper.SetSettings();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is TestController)
            {
                TestController = (TestController)e.Parameter;
                NextQuestion();
            }

            if (Frame.CanGoBack)
            {
                // Show UI in title bar if opted-in and in-app backstack is not empty.
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Visible;
            }
            else
            {
                // Remove the UI from the title bar if in-app back stack is empty.
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Collapsed;
            }

            base.OnNavigatedTo(e);
        }
        
        private void ShowAcceptAnswerButton()
        {
            ButtonAcceptAnswer.Visibility = Visibility.Visible;
            ButtonNextQuestion.Visibility = Visibility.Collapsed;
        }

        private void ShowNextQuestionButton()
        {
            ButtonAcceptAnswer.Visibility = Visibility.Collapsed;
            ButtonNextQuestion.Visibility = Visibility.Visible;
        }

        private void ButtonAcceptAnswer_Click(object sender, RoutedEventArgs e)
        {
            var selectedAnswers = AnswersGridView.SelectedItems.Cast<AnswerBlock>().Select(i => i.Answer.Key);
            TestController.CheckAnswer(TextQuestion.Key, selectedAnswers);
            
            var gridViewItems = AnswersGridView
                .Items
                .Cast<AnswerBlock>()
                .Where(i => textQuestion.Value.CorrectAnswerKeys.Contains(i.Answer.Key));
            foreach (var item in gridViewItems)
                item.MarkAsCorrect();

            ShowNextQuestionButton();
        }

        private void ButtonNextQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (TestController.IsTestCompleted())
            {
                var contentDialog = new ContentDialog
                {
                    Content = "Test zakończony!",
                    PrimaryButtonText = "Spoko",
                    SecondaryButtonText = "Anuluj"
                };
                var result = contentDialog.ShowAsync();
            }
            else
            {
                NextQuestion();
            }
        }

        private void NextQuestion()
        {
            if (TestController == null)
                return;

            TextQuestion = TestController.RandQuestion();
            Answers = TextQuestion.Value.Answers
                .Select(i => new AnswerBlock { Answer = i })
                .OrderBy(a => Guid.NewGuid())
                .ToList();
            
            ReoccurrencesOfQuestion = TestController.ReoccurrencesOfQuestion(TextQuestion.Key);
            ShowAcceptAnswerButton();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SettingsDialog();
            await dialog.ShowAsync();
        }

        private void AnswersGridView_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key >= VirtualKey.Number1 && e.Key <= VirtualKey.Number9)
            {
                var index = e.Key - VirtualKey.Number1;

                var actualSelected = AnswersGridView.SelectedItems;
                if (index < 0 || index >= AnswersGridView.Items.Count)
                    return;
                var itemToChangeSelection = AnswersGridView.Items[index];
                if (actualSelected.Contains(itemToChangeSelection))
                    actualSelected.Remove(itemToChangeSelection);
                else
                    actualSelected.Add(itemToChangeSelection);
            }
            else if (e.Key == VirtualKey.X)
            {
                if (ButtonAcceptAnswer.Visibility == Visibility.Visible)
                    ButtonAcceptAnswer_Click(ButtonAcceptAnswer, null);
                else if (ButtonNextQuestion.Visibility == Visibility.Visible)
                    ButtonNextQuestion_Click(ButtonNextQuestion, null);
            }
        }
    }
}
