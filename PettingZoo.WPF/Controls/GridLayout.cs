﻿using System.Windows;
using System.Windows.Controls;

namespace PettingZoo.WPF.Controls
{
    // Source: http://daniel-albuschat.blogspot.nl/2011/07/gridlayout-for-wpf-escape-margin-hell.html

    // The GridLayout is a special Panel that can be used exactly like the Grid Panel, except that it
    // defines a new property ChildMargin. ChildMargin's left, top, right and bottom margins will be applied
    // to all children in a way that the children will have a vertical space of ChildMargin.Top+ChildMargin.Bottom
    // and a horizontal space of ChildMargin.Left+ChildMargin.Right between them.
    // However, there is no margin for the borders of the internal widget, so that the GridLayout itself can be
    // aligned to another element without a margin.
    // It's best to have a look at TestWindow, which effectively tests all possible alignments of children.
    public class GridLayout : Grid
    {
        public static readonly DependencyProperty ChildMarginProperty = DependencyProperty.Register(
            "ChildMargin",
            typeof (Thickness),
            typeof (GridLayout),
            new FrameworkPropertyMetadata(new Thickness(5))
            {
                AffectsArrange = true,
                AffectsMeasure = true
            });

        // The child margin defines a margin that will be automatically applied to all children of this Grid.
        // However, the children at the edges will have the respective margins remove. E.g. the leftmost children will have
        // a Margin.Left of 0 and the children in the first row will have a Margin.Top of 0.
        // The margins that are not set to 0 are set to half the ChildMargin's value, since it's neighbour will also apply it,
        // effectively doubling it.

        public Thickness ChildMargin
        {
            get => (Thickness) GetValue(ChildMarginProperty);
            set
            {
                SetValue(ChildMarginProperty, value);
                UpdateChildMargins();
            }
        }

        // UpdateChildMargin first finds out what's the rightmost column and bottom row and then applies
        // the correct margins to all children.

        public void UpdateChildMargins()
        {
            var maxColumn = 0;
            var maxRow = 0;
            foreach (UIElement element in InternalChildren)
            {
                var row = GetRow(element);
                var column = GetColumn(element);
                
                if (row > maxRow)
                    maxRow = row;
                if (column > maxColumn)
                    maxColumn = column;
            }
            foreach (UIElement element in InternalChildren)
            {
                if (element is not FrameworkElement fe)
                    continue;

                var row = GetRow(fe);
                var column = GetColumn(fe);
                var factorLeft = 0.5;
                var factorTop = 0.5;
                var factorRight = 0.5;
                var factorBottom = 0.5;
                // Top row - no top margin
                if (row == 0)
                    factorTop = 0;
                // Bottom row - no bottom margin
                if (row == maxRow)
                    factorBottom = 0;
                // Leftmost column = no left margin
                if (column == 0)
                    factorLeft = 0;
                // Rightmost column - no right margin
                if (column == maxColumn)
                    factorRight = 0;
                fe.Margin = new Thickness(ChildMargin.Left*factorLeft,
                    ChildMargin.Top*factorTop,
                    ChildMargin.Right*factorRight,
                    ChildMargin.Bottom*factorBottom);
            }
        }

        // We change all children's margins in MeasureOverride, since this is called right before
        // the layouting takes place. I was first skeptical to do this here, because I thought changing
        // the margin will trigger a LayoutUpdate, which in turn would lead to an endless recursion,
        // but apparantly WPF takes care of this.

        protected override Size MeasureOverride(Size availableSize)
        {
            UpdateChildMargins();
            return base.MeasureOverride(availableSize);
        }
    }
}