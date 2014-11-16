using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dragablz
{
    public class Trapezoid : ContentControl
    {
        private PathGeometry _pathGeometry;

        static Trapezoid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Trapezoid), new FrameworkPropertyMetadata(typeof(Trapezoid)));
        }

        public static readonly DependencyProperty PenBrushProperty = DependencyProperty.Register(
            "PenBrush", typeof (Brush), typeof (Trapezoid), new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Transparent), FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Brush PenBrush
        {
            get { return (Brush) GetValue(PenBrushProperty); }
            set { SetValue(PenBrushProperty, value); }
        }

        public static readonly DependencyProperty PenThicknessProperty = DependencyProperty.Register(
            "PenThickness", typeof (double), typeof (Trapezoid), new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double PenThickness
        {
            get { return (double) GetValue(PenThicknessProperty); }
            set { SetValue(PenThicknessProperty, value); }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var contentDesiredSize = base.MeasureOverride(constraint);

            if (contentDesiredSize.Width == 0 || double.IsInfinity(contentDesiredSize.Width)
                || contentDesiredSize.Height == 0 || double.IsInfinity(contentDesiredSize.Height))

                return contentDesiredSize;

            _pathGeometry = CreateGeometry(contentDesiredSize);
            Clip = _pathGeometry;

            return _pathGeometry.GetRenderBounds(CreatePen()).Size;
        }

        private Pen CreatePen()
        {
            return new Pen(PenBrush, PenThickness)
            {
                EndLineCap = PenLineCap.Flat,
                MiterLimit = 1
            };
        }

        private static PathGeometry CreateGeometry(Size contentDesiredSize)
        {
            //TODO Make better :)  do some funky beziers or summit
            const double cheapRadiusBig = 6.0;
            const double cheapRadiusSmall = cheapRadiusBig/2;
            
            const int angle = 20;
            const double radians = angle * (Math.PI / 180);

            var startPoint = new Point(0, contentDesiredSize.Height + cheapRadiusSmall + cheapRadiusSmall);

            //clockwise starting at bottom left
            var bottomLeftSegment = new ArcSegment(new Point(startPoint.X + cheapRadiusSmall, startPoint.Y - cheapRadiusSmall),
                new Size(cheapRadiusSmall, cheapRadiusSmall), 315, false, SweepDirection.Counterclockwise, true);
            var triangleX = Math.Tan(radians) * (contentDesiredSize.Height);
            var leftSegment = new LineSegment(new Point(bottomLeftSegment.Point.X + triangleX, bottomLeftSegment.Point.Y - contentDesiredSize.Height), true);
            var topLeftSegment = new ArcSegment(new Point(leftSegment.Point.X + cheapRadiusBig, leftSegment.Point.Y - cheapRadiusSmall), new Size(cheapRadiusBig, cheapRadiusBig), 120, false, SweepDirection.Clockwise, true);
            var topSegment = new LineSegment(new Point(contentDesiredSize.Width + cheapRadiusBig + cheapRadiusBig, 0), true);
            var topRightSegment = new ArcSegment(new Point(contentDesiredSize.Width + cheapRadiusBig + cheapRadiusBig + cheapRadiusBig, cheapRadiusSmall), new Size(cheapRadiusBig, cheapRadiusBig), 40, false, SweepDirection.Clockwise, true);

            triangleX = Math.Tan(radians) * (contentDesiredSize.Height);
            //triangleX = Math.Tan(radians)*(contentDesiredSize.Height - topRightSegment.Point.Y);
            var rightSegment =
                new LineSegment(new Point(topRightSegment.Point.X + triangleX,
                    topRightSegment.Point.Y + contentDesiredSize.Height), true);

            var bottomRightSegment = new ArcSegment(new Point(rightSegment.Point.X + cheapRadiusSmall, rightSegment.Point.Y + cheapRadiusSmall),
                new Size(cheapRadiusSmall, cheapRadiusSmall), 25, false, SweepDirection.Counterclockwise, true);
            var bottomSegment = new LineSegment(new Point(0, bottomRightSegment.Point.Y), true);

            var pathSegmentCollection = new PathSegmentCollection
            {
                bottomLeftSegment, leftSegment, topLeftSegment, topSegment, topRightSegment, rightSegment, bottomRightSegment, bottomSegment
            };
            var pathFigure = new PathFigure(startPoint, pathSegmentCollection, true)
            {
                IsFilled = true
            };
            var pathFigureCollection = new PathFigureCollection
            {
                pathFigure
            };

            var geometryGroup = new PathGeometry(pathFigureCollection);            
            geometryGroup.Freeze();            

            return geometryGroup;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);            

            drawingContext.DrawGeometry(Background, CreatePen(), _pathGeometry);
        }
    }
}
