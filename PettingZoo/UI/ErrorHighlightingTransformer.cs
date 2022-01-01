using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace PettingZoo.UI
{
    public class ErrorHighlightingTransformer : DocumentColorizingTransformer
    {
        public Brush BackgroundBrush { get; set; }
        public TextPosition? ErrorPosition { get; set; }


        public ErrorHighlightingTransformer()
        {
            BackgroundBrush = new SolidColorBrush(Color.FromRgb(255, 230, 230));
        }


        protected override void ColorizeLine(DocumentLine line)
        {
            if (ErrorPosition == null)
                return;

            if (line.LineNumber != Math.Max(ErrorPosition.Value.Row, 1))
                return;

            var lineStartOffset = line.Offset;

            ChangeLinePart(lineStartOffset, lineStartOffset + line.Length,
                element =>
                {
                    element.BackgroundBrush = BackgroundBrush;
                });
        }
    }
}
