using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Toolbox.Controls
{
    public class ExtStackLayout : StackLayout
    {
        #region Properties
        public IEnumerable ItemsSource { get { return (IEnumerable)GetValue(ItemsSourceProperty); } set { SetValue(ItemsSourceProperty, value); } }
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create("ItemsSource", typeof(IEnumerable), typeof(ExtStackLayout), default(IEnumerable));

        public object ItemsNewData { get { return (IEnumerable)GetValue(ItemsNewDataProperty); } set { SetValue(ItemsNewDataProperty, value); } }
        public static readonly BindableProperty ItemsNewDataProperty = BindableProperty.Create("ItemsNewData", typeof(object), typeof(ExtStackLayout), default(object));

        public DataTemplate ItemTemplate { get { return (DataTemplate)GetValue(ItemTemplateProperty); } set { SetValue(ItemTemplateProperty, value); } }
        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create("ItemTemplate", typeof(DataTemplate), typeof(ExtStackLayout), null);

        public DataTemplate SplitterTemplate { get { return (DataTemplate)GetValue(SplitterTemplateProperty); } set { SetValue(SplitterTemplateProperty, value); } }
        public static readonly BindableProperty SplitterTemplateProperty = BindableProperty.Create("SplitterTemplate", typeof(DataTemplate), typeof(ExtStackLayout), null);

        public event EventHandler TemplateGenerate;

        public bool IsDynamicData { get; set; } = false;

        public void OnTemplateGenerate()
        {
            if (TemplateGenerate != null)
                TemplateGenerate.Invoke(this, new EventArgs());
        }

        public event EventHandler TemplateCreated;
        public void OnTemplateCreated()
        {
            if (TemplateCreated != null)
                TemplateCreated.Invoke(this, new EventArgs());
        }
        #endregion

        #region Constructor
        public ExtStackLayout()
        { }
        #endregion
        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == ItemsSourceProperty.PropertyName)
            {
                Debug.WriteLine("[ExtStackLayout] ItemsSourceProperty");
                Render();
            }
            else if (propertyName == ItemsNewDataProperty.PropertyName)
            {
                Debug.WriteLine("[ExtStackLayout] ItemsNewDataProperty");
                UpdateRender();
            }
        }

        public async void Render()
        {
            try
            {
                if (ItemTemplate == null || ItemsSource == null)
                {
                    return;
                }

                OnTemplateGenerate();

                if (Device.RuntimePlatform == Device.Android)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        //清除
                        this.Children.Clear();
                    });
                }
                else
                {
                    //清除
                    this.Children.Clear();
                }

                int count = 0;
                if (Device.RuntimePlatform == Device.Android)
                {
                    Task.Run(async () =>
                    {
                        //建立
                        foreach (var item in ItemsSource)
                        {
                            AddViewItem(item, count);
                            count++;
                            if (count % 5 == 0)
                                await Task.Delay(100);
                        }
                    });
                }
                else
                {
                    foreach (var item in ItemsSource)
                    {
                        AddViewItem(item, count);
                        count++;
                        if (count % 5 == 0)
                            await Task.Delay(100);
                    }
                }
                OnTemplateCreated();
                if (IsDynamicData && Device.RuntimePlatform == Device.iOS && Orientation == StackOrientation.Vertical)
                {
                    await Task.Delay(500);
                    double totalHeight = this.Padding.Top + this.Padding.Bottom;
                    foreach (var c in Children)
                    {
                        totalHeight += c.Measure(9999, 9999).Request.Height + Spacing;
                    }
                    HeightRequest = totalHeight;
                }
            }
            catch (Exception ex)
            {
                OnTemplateCreated();
                Debug.WriteLine(ex);
            }
        }
        public void UpdateRender()
        {
            try
            {
                if (ItemTemplate == null || ItemsNewData == null)
                {
                    return;
                }
                if (ItemsNewData is IEnumerable itemsNewDataCollection)
                {
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        Task.Run(() =>
                        {
                            foreach (var item in itemsNewDataCollection)
                            {
                                AddViewItem(item);
                            }
                        });
                    }
                    else
                    {
                        foreach (var item in itemsNewDataCollection)
                        {
                            AddViewItem(item);
                        }
                    }
                }
                else
                {
                    AddViewItem(ItemsNewData);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
        public void AddViewItem(object viewBindingSource, int index = 0)
        {
            var viewContent = ItemTemplate.CreateContent();
            if (viewContent is ViewCell viewCell)
            {
                viewCell.View.BindingContext = viewBindingSource;
                if (viewCell.View is IndicatorTintDot tintView)
                {
                    tintView.Index = index;
                }
                AddChildren(viewCell.View, viewBindingSource);
            }
            else if (viewContent is View view)
            {
                view.BindingContext = viewBindingSource;
                if (view is IndicatorTintDot tintView)
                {
                    tintView.Index = index;
                }
                AddChildren(view, viewBindingSource);
            }
        }
        public void AddChildren(View view, object viewBindingSource)
        {
            try
            {
                if (Device.RuntimePlatform == Device.Android)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (Children != null && Children.Any() && SplitterTemplate != null)
                        {
                            var splitterView = SplitterTemplate.CreateContent();
                            if (splitterView is ViewCell vcSplitter)
                            {
                                vcSplitter.View.BindingContext = viewBindingSource;
                                Children.Add(vcSplitter.View);
                            }
                            else if (splitterView is View vSplitter)
                            {
                                vSplitter.BindingContext = viewBindingSource;
                                Children.Add(vSplitter);
                            }
                        }
                        Children.Add(view);
                    });
                }
                else
                {
                    if (Children != null && Children.Any() && SplitterTemplate != null)
                    {
                        var splitterView = SplitterTemplate.CreateContent();
                        if (splitterView is ViewCell vcSplitter)
                        {
                            vcSplitter.View.BindingContext = viewBindingSource;
                            Children.Add(vcSplitter.View);
                        }
                        else if (splitterView is View vSplitter)
                        {
                            vSplitter.BindingContext = viewBindingSource;
                            Children.Add(vSplitter);
                        }
                    }
                    Children.Add(view);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
