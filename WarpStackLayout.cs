using MDBS.Support;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Toolbox.Controls
{
    public class WarpStackLayout : Layout<View>
    {
        public IEnumerable ItemsSource { get { return (IEnumerable)GetValue(ItemsSourceProperty); } set { SetValue(ItemsSourceProperty, value); } }
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create("ItemsSource", typeof(IEnumerable), typeof(WarpStackLayout), default(IEnumerable), propertyChanged: (bindable, oldvalue, newvalue) => {((WarpStackLayout)bindable).InvalidateLayout();});
        public DataTemplate ItemTemplate { get { return (DataTemplate)GetValue(ItemTemplateProperty); } set { SetValue(ItemTemplateProperty, value); } }
        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create("ItemTemplate", typeof(DataTemplate), typeof(WarpStackLayout), null, propertyChanged: (bindable, oldvalue, newvalue) => { ((WarpStackLayout)bindable).InvalidateLayout(); });
        public int ColumnCount { get { return (int)GetValue(ColumnCountProperty); } set { SetValue(ColumnCountProperty, value); } }
        public static readonly BindableProperty ColumnCountProperty = BindableProperty.Create("ColumnCount", typeof(int), typeof(WarpStackLayout), 1);
        public double RowSpacing { get { return (double)GetValue(RowSpacingProperty); } set { SetValue(RowSpacingProperty, value); } }
        public static readonly BindableProperty RowSpacingProperty = BindableProperty.Create("RowSpacing", typeof(double), typeof(WarpStackLayout), DataCentre.Instance.BasicSpace * 8);
        public double ColumnSpacing { get { return (double)GetValue(ColumnSpacingProperty); } set { SetValue(ColumnSpacingProperty, value); } }
        public static readonly BindableProperty ColumnSpacingProperty = BindableProperty.Create("ColumnSpacing", typeof(double), typeof(WarpStackLayout), DataCentre.Instance.BasicSpace * 8);

        public bool IsSquare { get { return (bool)GetValue(IsSquareProperty); } set { SetValue(IsSquareProperty, value); } }
        public static readonly BindableProperty IsSquareProperty = BindableProperty.Create("IsSquare", typeof(bool), typeof(WarpStackLayout), false);
        
        public WarpStackLayout()
        {
            HorizontalOptions = LayoutOptions.Fill;
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == ItemsSourceProperty.PropertyName)
            {
                Debug.WriteLine($"ItemsSourcePropertyChange");
                Render();
            }
        }

        public void Render()
        {
            try
            {
                if (ItemTemplate == null || ItemsSource == null)
                {
                    return;
                }
                if (Device.RuntimePlatform == Device.Android)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        //清除
                        Children.Clear();
                        HeightRequest = 1;
                    });
                }
                else
                {
                    //清除
                    Children.Clear();
                    HeightRequest = 1;
                }
                //建立
                foreach (var item in ItemsSource)
                {
                    var viewContent = ItemTemplate.CreateContent();
                    if (viewContent is ViewCell viewCell)
                    {
                        viewCell.View.BindingContext = item;
                        AddView(viewCell.View);
                    }
                    else if (viewContent is View view)
                    {
                        view.BindingContext = item;
                        AddView(view);
                    }
                }
                this.UpdateChildrenLayout();
                this.InvalidateLayout();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public void AddView(View view)
        {
            try
            {
                if (Device.RuntimePlatform == Device.Android)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Children.Add(view);
                    });
                }
                else
                {
                    Children.Add(view);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            try
            {
                double xChild = x;
                double yChild = y;
                var initLocX = Padding.Left;
                var childWidthSums = Width - Padding.HorizontalThickness - ColumnSpacing * (ColumnCount - 1) -1;
                var childWidth = childWidthSums / ColumnCount;
                foreach (View child in Children)
                {
                    child.WidthRequest = childWidth;
                    if (IsSquare)
                    {
                        child.HeightRequest = childWidth;
                    }
                    var childrenSize = child.Measure(9999, 9999).Request;
                    if (xChild + childrenSize.Width + Padding.Right > Width)
                    {
                        //換行
                        xChild = initLocX;
                        yChild += childrenSize.Height + RowSpacing;
                        LayoutChildIntoBoundingRegion(child, new Rectangle(new Point(xChild, yChild), childrenSize));
                        xChild += childrenSize.Width + ColumnSpacing;
                    }
                    else
                    {
                        //不換行
                        LayoutChildIntoBoundingRegion(child, new Rectangle(new Point(xChild, yChild), childrenSize));
                        xChild += childrenSize.Width + ColumnSpacing;
                    }
                    //處理元件範圍-高度
                    if (yChild + childrenSize.Height + RowSpacing > HeightRequest)
                    {
                        HeightRequest = yChild + childrenSize.Height + RowSpacing;
                    }
                    Debug.WriteLine($"[HeightRequest]{HeightRequest} [WidthRequest]{WidthRequest}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
