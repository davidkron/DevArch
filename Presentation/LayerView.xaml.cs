﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace Presentation
{
    /// <summary>
    /// Interaction logic for layerControl.xaml
    /// </summary>
    [ContentProperty("Children")]
    public partial class LayerView : UserControl
    {
        private readonly int _column;
        private readonly int _row;

        public LayerView(string name, Color backgroundColor, IEnumerable<LayerView> childs, int column, int row,bool visible,int columns,int rows)
        {
            InitializeComponent();
            LayerName = name;
            NameBlock.Text = name;
            _column = column;
            _row = row;

            foreach (var child in childs)
            {
                ChildHolder.Children.Add(child);
                Grid.SetColumn(child, child._column);
                Grid.SetRow(child, child._row);
            }

            for (var i = 0; i < rows; i++)
                ChildHolder.RowDefinitions.Add(new RowDefinition());

            for (var i = 0; i < columns; i++)
                ChildHolder.ColumnDefinitions.Add(new ColumnDefinition());

            DataContext = this;

            Border.Background = new SolidColorBrush(backgroundColor);
            if (!visible) Hide();
        }

        private void Hide()
        {
            NameBlock.Visibility = Visibility.Collapsed;
            Border.BorderThickness = new Thickness(0);
            Border.Margin = new Thickness(0);
        }

        public string LayerName { get; set; }
    }
}
