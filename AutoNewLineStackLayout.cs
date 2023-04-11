using MDBS.Support;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Toolbox.Controls
{
    public class AutoNewLineStackLayout : Layout<View>
    {
        public IEnumerable ItemsSource { get { return (IEnumerable)GetValue(ItemsSourceProperty); } set { SetValue(ItemsSourceProperty, value); } }
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create("ItemsSource", typeof(IEnumerable), typeof(AutoNewLineStackLayout), default(IEnumerable));

        public DataTemplate ItemTemplate { get { return (DataTemplate)GetValue(ItemTemplateProperty); } set { SetValue(ItemTemplateProperty, value); } }
        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create("ItemTemplate", typeof(DataTemplate), typeof(AutoNewLineStackLayout), null);

        public double RowSpacing { get { return (double)GetValue(RowSpacingProperty); } set { SetValue(RowSpacingProperty, value); } }
        public static readonly BindableProperty RowSpacingProperty = BindableProperty.Create("RowSpacing", typeof(double), typeof(AutoNewLineStackLayout), DataCentre.Instance.BasicSpace * 8);
        public double ColumnSpacing { get { return (double)GetValue(ColumnSpacingProperty); } set { SetValue(ColumnSpacingProperty, value); } }
        public static readonly BindableProperty ColumnSpacingProperty = BindableProperty.Create("ColumnSpacing", typeof(double), typeof(AutoNewLineStackLayout), DataCentre.Instance.BasicSpace * 8);

        public int Row { get; set; } = 0;

        double xx { get; set; } = 0.0;
        double yy { get; set; } = 0.0;

        public AutoNewLineStackLayout()
        {
        }

        protected override async void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == ItemsSourceProperty.PropertyName)
            {
                await Render();
            }
        }

        public async Task Render()
        {
            try
            {
                if (ItemTemplate == null || ItemsSource == null)
                {
                    return;
                }
                //OnTemplateGenerate();
                Row = 0;
                Device.BeginInvokeOnMainThread(() =>
                {
                    //清除
                    this.Children.Clear();
                });

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
            }
            catch (Exception ex)
            {
                //OnTemplateCreated();
                Debug.WriteLine(ex);
            }
        }
        public void AddView(View view)
        {
            try
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Children.Add(view);
                });
            }
            catch (Exception ex)
            {

            }
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            double xChild = x;
            double yChild = y;
            var initLocX = Padding.Left;

            foreach (View child in Children)
            {
                var childrenSize = child.Measure(9999, 9999).Request;
                if (xChild + childrenSize.Width + ColumnSpacing + Padding.Right >= Width)
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
                if (yChild + childrenSize.Height + RowSpacing > HeightRequest)
                    HeightRequest = yChild + childrenSize.Height + RowSpacing;
            }
        }
    }
}
