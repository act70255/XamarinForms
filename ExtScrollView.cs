using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using System.Linq;

namespace Toolbox.Controls
{
    public class ExtScrollView : ScrollView
    {
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create("ItemsSource", typeof(IEnumerable), typeof(ExtScrollView), default(IEnumerable));
        public IEnumerable ItemsSource { get { return (IEnumerable)GetValue(ItemsSourceProperty); } set { SetValue(ItemsSourceProperty, value); } }

        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create("ItemTemplate", typeof(DataTemplate), typeof(ExtScrollView), null);
        public DataTemplate ItemTemplate { get { return (DataTemplate)GetValue(ItemTemplateProperty); } set { SetValue(ItemTemplateProperty, value); } }

        public event EventHandler<ItemTappedEventArgs> ItemSelected;

        public static readonly BindableProperty SelectedCommandProperty = BindableProperty.Create("SelectedCommand", typeof(ICommand), typeof(ExtScrollView), null);
        public ICommand SelectedCommand { get { return (ICommand)GetValue(SelectedCommandProperty); } set { SetValue(SelectedCommandProperty, value); } }

        public static readonly BindableProperty SelectedCommandParameterProperty = BindableProperty.Create("SelectedCommandParameter", typeof(object), typeof(ExtScrollView), null);
        public object SelectedCommandParameter { get { return GetValue(SelectedCommandParameterProperty); } set { SetValue(SelectedCommandParameterProperty, value); } }

        public static readonly BindableProperty SpacingProperty = BindableProperty.Create("Spacing", typeof(double), typeof(ExtScrollView), 6d);
        public double Spacing { get { return (double)GetValue(SpacingProperty); } set { SetValue(SpacingProperty, value); } }

        public static readonly BindableProperty AutoCenterProperty = BindableProperty.Create("AutoCenter", typeof(bool), typeof(ExtScrollView), false);
        public bool AutoCenter { get { return (bool)GetValue(AutoCenterProperty); } set { SetValue(AutoCenterProperty, value); } }

        public static readonly BindableProperty PositionProperty = BindableProperty.Create("Position", typeof(int), typeof(ExtScrollView), 0, BindingMode.TwoWay);
        public int Position { get { return (int)GetValue(PositionProperty); } set { SetValue(PositionProperty, value); } }

        public static readonly BindableProperty ChoosableColorProperty = BindableProperty.Create("ChooseableColor", typeof(Color), typeof(ExtScrollView), Color.Gray);
        public Color ChoosableColor { get { return (Color)GetValue(ChoosableColorProperty); } set { SetValue(ChoosableColorProperty, value); } }

        public static readonly BindableProperty ChosenColorProperty = BindableProperty.Create("ChosenColor", typeof(Color), typeof(ExtScrollView), Color.White);
        public Color ChosenColor { get { return (Color)GetValue(ChosenColorProperty); } set { SetValue(ChosenColorProperty, value); } }

        public ExtScrollView()
        {
            ItemSelected += async (sender, e) =>
            {
                var index = 0;
                foreach (var item in ItemsSource)
                {
                    if (item.Equals(e.Item))
                    {
                        break;
                    }
                    ++index;
                }
                Position = index;
                await UpdateChildren();
            };
        }

        protected override async void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == ItemsSourceProperty.PropertyName)
            {
                //Debug.WriteLine($"{ItemsSourceProperty.PropertyName}: {ItemsSource}");
                //Debug.WriteLine($"{ItemTemplateProperty.PropertyName}: {ItemTemplate}");
                await Render();
            }
            else if (propertyName == PositionProperty.PropertyName)
            {
                Debug.WriteLine($"{GetType().Name}.OnPropertyChanged: {Position}");
                var index = 0;
                if (ItemsSource != null)
                {
                    foreach (var item in ItemsSource)
                    {
                        if (index == Position)
                        {
                            if (SelectedCommand != null && SelectedCommand.CanExecute(item))
                            {
                                SelectedCommand.Execute(item);
                            }
                            break;
                        }
                        ++index;
                    }
                }
                await UpdateChildren();
            }
        }

        public async Task Render()
        {
            if (ItemTemplate == null || ItemsSource == null)
            {
                return;
            }

            // TODO: 清除動作
            var layout = new StackLayout();
            layout.Orientation = Orientation == ScrollOrientation.Vertical ? StackOrientation.Vertical : StackOrientation.Horizontal;
            layout.Spacing = Spacing;

            Content = layout;
            await Task.Run(() =>
            {
                foreach (var item in ItemsSource)
                {
                    // 準備command相關
                    var commandParameter = SelectedCommandParameter ?? item;
                    var command = new Command((obj) =>
                    {
                        var args = new ItemTappedEventArgs(ItemsSource, item);
                        ItemSelected?.Invoke(this, args);
                        if (SelectedCommand != null && SelectedCommand.CanExecute(commandParameter))
                        {
                            SelectedCommand.Execute(commandParameter);
                        }
                    });

                    var viewCell = ItemTemplate.CreateContent() as ViewCell;
                    viewCell.View.BindingContext = item;
                    viewCell.View.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = command,
                        CommandParameter = commandParameter,
                        NumberOfTapsRequired = 1
                    });

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        layout.Children.Add(viewCell.View);
                    });
                }
            });


            await UpdateChildren();
        }

        public async Task UpdateChildren()
        {
            try
            {
                var layout = Content as StackLayout;
                var index = 0;

                if (AutoCenter && layout.Children.Count > Position)
                {
                    await ScrollToAsync(layout.Children[Position], ScrollToPosition.Center, true);
                }

                //foreach (var item in layout.Children)
                //{
                //    if (item is Label label)
                //    {
                //        label.TextColor = (Position != index) ? ChoosableColor : ChosenColor;
                //        ++index;
                //    }
                //}

            }
            catch(Exception ex)
            {
                return;
            }
        }
    }
}